using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // Added for List support

/* 
 * SUMMARY:
 * This script manages the Case File interface. It handles pausing the game,
 * unlocking the cursor for UI interaction, and populating the clue list.
 * It now tracks a list of collected Clue_Data for NPCs to reference.
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

    private bool is_open = false;
    private bool can_be_showcased = false;
    private bool is_showcasing = false;
    private string showcased_clue = "";

    // NEW: This list stores the actual data objects you've found
    private List<Clue_Data> collected_clues_list = new List<Clue_Data>();

    private void Start()
    {
        case_file_panel.SetActive(false);

        if (case_file_background != null)
        {
            case_file_background.SetActive(false);
        }

        display_image.enabled = false;
        showcase_button.onClick.AddListener(() => Showcase());
        showcase_button.gameObject.SetActive(false);
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

        if (display_image.enabled) {
            showcase_button.gameObject.SetActive(can_be_showcased);
        }
    }

    public void Open_Case_File(bool part_of_dialogue=false)
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
        // NEW: Check if we already have this clue to prevent duplicates in the list
        if (!collected_clues_list.Contains(new_clue))
        {
            collected_clues_list.Add(new_clue);
        }

        GameObject new_button = Instantiate(clue_button_prefab, clue_list_container);
        new_button.GetComponentInChildren<TextMeshProUGUI>().text = new_clue.clue_title;
        new_button.GetComponent<Button>().onClick.AddListener(() => Show_Clue_Details(new_clue));
    }

    // NEW: This function allows NPCs to see what you've found
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

    private void Showcase() {
        Close_Case_File();
        is_showcasing = true;
        showcased_clue = display_title.text;
    }

    public bool Is_Showcasing() {
        return is_showcasing;
    }

    public string Get_Showcased_Clue() {
        is_showcasing = false;
        return showcased_clue;
    }
}