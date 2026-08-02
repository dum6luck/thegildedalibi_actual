using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/* 
 * SUMMARY:
 * This script manages the VN-style dialogue box. It features a "skip" 
 * mechanic: the first click completes the text instantly, and the 
 * second click closes the window once the arrow is active.
 */

public class Dialogue_Manager : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup dialogue_canvas_group;
    public TextMeshProUGUI name_text;
    public TextMeshProUGUI dialogue_text;
    public GameObject floating_arrow;
    public Case_File_UI case_canvas;
    public GameObject cutscene_background;
    public GameObject character_a;
    public GameObject character_b;
    public AudioSource audioSource;

    [Header("Typewriter Settings")]
    public float type_speed = 0.05f;

    [Header("Arrow Bounce Settings")]
    public float bounce_speed = 5f;
    public float bounce_amplitude = 10f;

    private List<Character_Data> left_npcs = null;
    private List<Character_Data> right_npcs = null;
    private List<Character_Data> speaking_npcs = null;

    private List<string> lines = new List<string>();
    private int line_index = 0;

    private Cutscene_Data cutscene = null;

    private bool is_typing = false;
    private bool can_close = false;
    private bool can_open_case_file = false;
    private string current_full_text; // Stores the full sentence for skipping
    private Vector3 arrow_start_pos;
    private Coroutine typing_coroutine;

    private void Start()
    {
        if (dialogue_canvas_group != null)
        {
            dialogue_canvas_group.alpha = 0;
            dialogue_canvas_group.interactable = false;
            dialogue_canvas_group.blocksRaycasts = false;
        }

        if (floating_arrow != null)
        {
            arrow_start_pos = floating_arrow.transform.localPosition;
            floating_arrow.SetActive(false);
        }

        if (cutscene_background != null)
        {
            cutscene_background.SetActive(false);
        }

        if (character_a != null)
        {
            character_a.SetActive(false);
        }

        if (character_b != null)
        {
            character_b.SetActive(false);
        }
    }

    // Inside your Dialogue_Manager script
    public static Dialogue_Manager Instance; // Add this line

    private void Awake()
    {
        // Initialize the Singleton
        if (Instance == null) Instance = this;
    }
    private void Update()
    {
        // Handle the arrow bounce
        if (floating_arrow != null && floating_arrow.activeSelf)
        {
            float new_y = arrow_start_pos.y + Mathf.Sin(Time.unscaledTime * bounce_speed) * bounce_amplitude;
            floating_arrow.transform.localPosition = new Vector3(arrow_start_pos.x, new_y, arrow_start_pos.z);
        }

        // Click Logic
        if (Input.GetMouseButtonDown(0))
        {
            if (is_typing)
            {
                // First click: Fast forward to the end
                Finish_Typing_Early();
            }
            else if (can_close)
            {
                line_index++;
                if (line_index < lines.Count) {
                    Show_Dialogue(speaking_npcs[line_index % speaking_npcs.Count].name, lines[line_index], can_open_case_file);
                }
                else {
                    if (can_open_case_file)
                    {
                        case_canvas.Open_Case_File(true);
                        can_open_case_file = false;
                    }
                    // Second click (when arrow is up): Close the panel
                    Hide_Dialogue();
                }
            }
        }
    }

    public void Show_Dialogue(string name, string text, bool case_buttons=false)
    {
        name_text.text = name;
        dialogue_text.text = "";
        current_full_text = text; // Cache the full sentence
        can_close = false;
        can_open_case_file = case_buttons;
        is_typing = true;

        int img_num = cutscene == null ? 0 : cutscene.images.Count;
        int char_a_num = cutscene == null ? 0 : cutscene.left_characters.Count;
        int char_b_num = cutscene == null ? 0 : cutscene.right_characters.Count;

        int char_a_emotions_num = cutscene == null ? 0 : cutscene.left_chr_emotions.Count;
        int char_b_emotions_num = cutscene == null ? 0 : cutscene.right_chr_emotions.Count;

        Sprite _image;
        Character_Data _character;
        string _emotion;
        
        if (img_num > 0)
        {
            _image = cutscene.images[line_index % img_num];
            if (_image != null)
            {
                cutscene_background.SetActive(true);
                cutscene_background.GetComponent<Image>().sprite = _image;
            }
            else
            {
                cutscene_background.SetActive(false);
            }
        }

        if (char_a_num > 0)
        {
            _character = cutscene.left_characters[line_index % char_a_num];
            if (_character != null)
            {
                _emotion = char_a_emotions_num == 0 ? null : cutscene.left_chr_emotions[line_index % char_a_emotions_num];
                character_a.SetActive(true);
                character_a.GetComponent<Image>().sprite = _character.Get_Dialogue_Sprite(_emotion);
            }
            else
            {
                character_a.SetActive(false);
            }
        }

        if (char_b_num > 0)
        {
            _character = cutscene.right_characters[line_index % char_b_num];
            if (_character != null)
            {
                _emotion = char_b_emotions_num == 0 ? null : cutscene.right_chr_emotions[line_index % char_b_emotions_num];
                character_b.SetActive(true);
                character_b.GetComponent<Image>().sprite = _character.Get_Dialogue_Sprite(_emotion);
            }
            else
            {
                character_b.SetActive(false);
            }
        }

        if (dialogue_canvas_group != null)
        {
            dialogue_canvas_group.alpha = 1;
            dialogue_canvas_group.interactable = true;
            dialogue_canvas_group.blocksRaycasts = true;
        }

        if (typing_coroutine != null) StopCoroutine(typing_coroutine);
        typing_coroutine = StartCoroutine(Typewriter_Effect(text));
    }

    public void Show_Dialogue(Character_Data character, string text, bool case_buttons=false)
    {
        speaking_npcs = new List<Character_Data>();
        speaking_npcs.Add(character);
        Show_Dialogue(character.name, text, case_buttons);
    }

    public void Show_Dialogue(Character_Data character, List<string> text_list, bool case_buttons=false)
    {
        List<Character_Data> _characters = new List<Character_Data>();
        _characters.Add(character);
        Show_Dialogue(_characters, text_list, case_buttons);
    }

    public void Show_Dialogue(
            List<Character_Data> characters, List<string> text_list, bool case_buttons=false)
    {
        line_index = 0;
        lines = text_list;
        speaking_npcs = characters;
        Show_Dialogue(characters[0].name, lines[0], case_buttons);
    }

    public void Show_Cutscene(Cutscene_Data _cutscene)
    {
        // TO DO: Implement characters (in dialogue form) appear in the cutscene
        cutscene = _cutscene;
        Show_Dialogue(_cutscene.characters, _cutscene.dialogue_lines);
    }

    IEnumerator Typewriter_Effect(string text)
    {
        floating_arrow.SetActive(false);

        int tick = 0;

        foreach (char c in text.ToCharArray())
        {
            dialogue_text.text += c;

            // Prevents loud echoing for debug purposes
            if (tick % 5 == 0) audioSource.Stop();

            PlaySound();
            tick++;
            yield return new WaitForSecondsRealtime(type_speed);
        }

        Complete_Dialogue();
    }

    private void Finish_Typing_Early()
    {
        // Stop the typing coroutine and immediately show the full cached text
        if (typing_coroutine != null) StopCoroutine(typing_coroutine);

        dialogue_text.text = current_full_text;
        Complete_Dialogue();
    }

    private void Complete_Dialogue()
    {
        is_typing = false;
        can_close = true;
        if (floating_arrow != null) floating_arrow.SetActive(true);
    }

    public void Hide_Dialogue()
    {
        can_close = false;
        speaking_npcs = null;
        if (floating_arrow != null) floating_arrow.SetActive(false);

        if (dialogue_canvas_group != null)
        {
            dialogue_canvas_group.alpha = 0;
            dialogue_canvas_group.interactable = false;
            dialogue_canvas_group.blocksRaycasts = false;
        }

        cutscene = null;
        cutscene_background.SetActive(false);
        character_a.SetActive(false);
        character_b.SetActive(false);
    }

    public bool Is_Dialogue_Ongoing(bool with_case_ui=false)
    {
        if (dialogue_canvas_group.alpha == 1)
        {
            return true;
        }

        if (case_canvas != null && with_case_ui)
        {
            return case_canvas.Is_Open();
        }

        return false;
    }

    public void OnSliderValueChanged(float value)
    {
        type_speed = 0.2f - value;
    }

    private void PlaySound()
    {
        if (speaking_npcs == null) {
            return;
        }

        int npcs_size = speaking_npcs.Count;

        //Play the sound only if it exists
        if (npcs_size == 0 || audioSource == null)
            return;

        //Stop the audioSource so that the new sentence does not overlap with the old one
        //audioSource.Stop();

        Character_Data npc = speaking_npcs[line_index % npcs_size];

        //Play sentence sound
        audioSource.PlayOneShot(
            npc.voiceSamples[UnityEngine.Random.Range(0, npc.voiceSamples.Count)]);
    }
}