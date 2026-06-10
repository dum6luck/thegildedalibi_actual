using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/* * SUMMARY:
 * This script manages the Case File interface. It handles pausing the game,
 * unlocking the cursor for UI interaction, and populating the clue list.
 * It now stores clues inside InspectionData so they save across scene loads!
 * Added: Audio support for brand new clue discovery events.
 */

public class Case_File_UI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject case_file_panel;
    public Transform clue_list_container;
    public GameObject case_file_background;

    [Header("Display Elements")]
    public TextMeshProUGUI display_title;
    public TextMeshProUGUI display_description;
    public Image display_image;
    public Button showcase_button;

    [Header("Prefabs")]
    public GameObject clue_button_prefab;

    [Header("Audio SFX Settings")]
    public AudioSource uiAudioSource;      // Drag your AudioSource component here in the Inspector
    public AudioClip clueDiscoverySound;   // Drop your .mp3 or .wav discovery sound here

    private bool is_open = false;
    private bool can_be_showcased = false;
    private bool is_showcasing = false;
    private string showcased_clue = "";

    private List<Clue_Data> collected_clues_list = new List<Clue_Data>();

    private void Start()
    {
        case_file_panel.SetActive(false);

        if (case_file_background != null)
        {
            case_file_background.SetActive(false);
        }

        display_image.enabled = false;
        if (showcase_button != null)
        {
            showcase_button.onClick.AddListener(() => Showcase());
            showcase_button.gameObject.SetActive(false);
        }

        RestoreSavedCluesFromVault();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (is_open)
            {
                Close_Case_File();
            }
            else
            {
                Open_Case_File(can_be_showcased);
            }
        }

        if (showcase_button != null && display_image.enabled)
        {
            showcase_button.gameObject.SetActive(can_be_showcased);
        }
    }

    public void Open_Case_File(bool part_of_dialogue = false)
    {
        is_open = true;
        can_be_showcased = part_of_dialogue;
        case_file_panel.SetActive(true);

        if (case_file_background != null)
        {
            case_file_background.SetActive(true);
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Close_Case_File()
    {
        is_open = false;
        can_be_showcased = false;
        case_file_panel.SetActive(false);

        if (case_file_background != null)
        {
            case_file_background.SetActive(false);
        }

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Add_Clue_To_Log(Clue_Data new_clue)
    {
        if (new_clue == null) return;

        // --- DYNAMIC AUDIO JINGLE TRIGGER ---
        // Checks if this is a brand new clue entry before it gets appended to your collection databases
        if (!InspectionData.SavedClues.Contains(new_clue))
        {
            if (uiAudioSource != null && clueDiscoverySound != null)
            {
                uiAudioSource.PlayOneShot(clueDiscoverySound);
                Debug.Log($"[Audio Logic] Playing discovery jingle for new clue: {new_clue.clue_title}");
            }
        }

        if (!InspectionData.SavedClues.Contains(new_clue))
        {
            InspectionData.SavedClues.Add(new_clue);
        }

        if (!collected_clues_list.Contains(new_clue))
        {
            collected_clues_list.Add(new_clue);
        }

        CreateClueButtonUI(new_clue);
    }

    private void RestoreSavedCluesFromVault()
    {
        foreach (Transform child in clue_list_container)
        {
            Destroy(child.gameObject);
        }

        collected_clues_list.Clear();

        Debug.Log($"[Notebook System] Restoring {InspectionData.SavedClues.Count} clues from cross-scene storage...");
        foreach (Clue_Data saved_clue in InspectionData.SavedClues)
        {
            if (saved_clue != null)
            {
                collected_clues_list.Add(saved_clue);
                CreateClueButtonUI(saved_clue);
            }
        }
    }

    private void CreateClueButtonUI(Clue_Data clue)
    {
        GameObject new_button = Instantiate(clue_button_prefab, clue_list_container);
        new_button.GetComponentInChildren<TextMeshProUGUI>().text = clue.clue_title;
        new_button.GetComponent<Button>().onClick.AddListener(() => Show_Clue_Details(clue));
    }

    public List<Clue_Data> Get_Collected_Clues()
    {
        return collected_clues_list;
    }

    public void Show_Clue_Details(Clue_Data clue)
    {
        display_title.text = clue.clue_title;
        display_description.text = clue.clue_description;
        display_image.sprite = clue.clue_icon;
        display_image.enabled = true;
    }

    public bool Is_Open()
    {
        return is_open;
    }

    private void Showcase()
    {
        Close_Case_File();
        is_showcasing = true;
        showcased_clue = display_title.text;
    }

    public bool Is_Showcasing()
    {
        return is_showcasing;
    }

    public string Get_Showcased_Clue()
    {
        is_showcasing = false;
        return showcased_clue;
    }

    // --- HELPER FUNCTION: Matches parameter against local clue_title values ---
    public bool Has_Collected_Clue(string clueID)
    {
        if (collected_clues_list == null) return false;
        return collected_clues_list.Exists(clue => clue != null && clue.clue_title == clueID);
    }
}