using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/* 
 * SUMMARY:
 * Manages VN-style dialogue boxes and multi-character cutscenes.
 * Displays left/right character portraits, handles speaker dimming,
 * typewriter text, and UI flow.
 */

public class Dialogue_Manager : MonoBehaviour
{
    public static Dialogue_Manager Instance;

    [Header("UI References")]
    public CanvasGroup dialogue_canvas_group;
    public TextMeshProUGUI name_text;
    public TextMeshProUGUI dialogue_text;
    public GameObject floating_arrow;
    public Case_File_UI case_canvas;
    public GameObject cutscene_background;
    public AudioSource audioSource;
    public List<AudioClip> default_voice_samples;

    [Header("Cutscene Portrait Displays")]
    public Image left_character_image;
    public Image right_character_image;

    [Header("Portrait Focus Colors")]
    public Color active_color = Color.white;
    public Color inactive_color = new Color(0.4f, 0.4f, 0.4f, 1f); // Dims non-speaking character

    [Header("Typewriter Settings")]
    public float type_speed = 0.05f;

    [Header("Arrow Bounce Settings")]
    public float bounce_speed = 5f;
    public float bounce_amplitude = 10f;

    // Cutscene Frame Tracking
    private List<CutsceneFrame> current_cutscene_frames = new List<CutsceneFrame>();
    private bool is_cutscene_mode = false;

    // Standard Dialogue Tracking
    private List<Character_Data> npcs = null;
    private List<string> lines = new List<string>();
    private int line_index = 0;

    private Cutscene_Data cutscene = null;

    private bool is_typing = false;
    private bool can_close = false;
    private bool can_open_case_file = false;
    private string current_full_text;
    private Vector3 arrow_start_pos;
    private Coroutine typing_coroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

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
    }

    private void Update()
    {
        // Handle floating arrow bounce
        if (floating_arrow != null && floating_arrow.activeSelf)
        {
            float new_y = arrow_start_pos.y + Mathf.Sin(Time.unscaledTime * bounce_speed) * bounce_amplitude;
            floating_arrow.transform.localPosition = new Vector3(arrow_start_pos.x, new_y, arrow_start_pos.z);
        }

        // Click logic for progressing dialogue
        if (Input.GetMouseButtonDown(0))
        {
            if (is_typing)
            {
                Finish_Typing_Early();
            }
            else if (can_close)
            {
                line_index++;

                if (is_cutscene_mode)
                {
                    if (line_index < current_cutscene_frames.Count)
                    {
                        Display_Cutscene_Frame(line_index);
                    }
                    else
                    {
                        Hide_Dialogue();
                    }
                }
                else
                {
                    if (line_index < lines.Count)
                    {
                        Show_Dialogue(npcs[line_index].name, lines[line_index], can_open_case_file);
                    }
                    else
                    {
                        if (can_open_case_file && case_canvas != null)
                        {
                            case_canvas.Open_Case_File(true);
                            can_open_case_file = false;
                        }
                        Hide_Dialogue();
                    }
                }
            }
        }
    }

    #region Standard Dialogue Methods

    public void Show_Dialogue(string name, string text, bool case_buttons = false)
    {
        name_text.text = name;
        dialogue_text.text = "";
        current_full_text = text;
        can_close = false;
        can_open_case_file = case_buttons;
        is_typing = true;

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
        npcs = new List<Character_Data>();
        npcs.Add(character);
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
        npcs = characters;
        Show_Dialogue(characters[0].name, lines[0], case_buttons);
    }

    #endregion

    #region Cutscene Methods

    public void Show_Cutscene(Cutscene_Data cutscene)
    {
        if (cutscene == null || cutscene.frames == null || cutscene.frames.Count == 0) return;

        is_cutscene_mode = true;
        current_cutscene_frames = cutscene.frames;
        line_index = 0;

        if (cutscene_background != null)
        {
            cutscene_background.SetActive(true);
        }

        Display_Cutscene_Frame(line_index);
    }

    private void Display_Cutscene_Frame(int index)
    {
        CutsceneFrame frame = current_cutscene_frames[index];

        // Set left and right character sprites
        Update_Portrait(left_character_image, frame.leftCharacterSprite);
        Update_Portrait(right_character_image, frame.rightCharacterSprite);

        // Highlight active speaker and dim the inactive one
        if (frame.activeSpeaker == CutsceneFrame.ActiveSpeaker.Left)
        {
            if (left_character_image != null) left_character_image.color = active_color;
            if (right_character_image != null) right_character_image.color = inactive_color;
        }
        else if (frame.activeSpeaker == CutsceneFrame.ActiveSpeaker.Right)
        {
            if (left_character_image != null) left_character_image.color = inactive_color;
            if (right_character_image != null) right_character_image.color = active_color;
        }
        else
        {
            if (left_character_image != null) left_character_image.color = active_color;
            if (right_character_image != null) right_character_image.color = active_color;
        }

        string speakerName = "";
        if (frame.speaker != null) {
            speakerName = frame.speaker.name;
        }

        // Trigger typewriter effect with speaker name and line
        Show_Dialogue(speakerName, frame.dialogueLine);
        is_cutscene_mode = true;
    }

    private void Update_Portrait(Image portraitImage, Sprite sprite)
    {
        if (portraitImage == null) return;

        if (sprite != null)
        {
            portraitImage.gameObject.SetActive(true);
            portraitImage.sprite = sprite;
        }
        else
        {
            portraitImage.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Helpers & Flow Control

    IEnumerator Typewriter_Effect(string text)
    {
        if (floating_arrow != null) floating_arrow.SetActive(false);

        int tick = 0;

        foreach (char c in text.ToCharArray())
        {
            dialogue_text.text += c;

            // Prevents loud echoing for debug purposes
            if ((tick % 20) - 1 == 0) audioSource.Stop();

            PlaySound();
            tick++;
            yield return new WaitForSecondsRealtime(type_speed);
        }

        Complete_Dialogue();
    }

    private void Finish_Typing_Early()
    {
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
        is_cutscene_mode = false;
        npcs = null;

        if (floating_arrow != null) floating_arrow.SetActive(false);

        if (dialogue_canvas_group != null)
        {
            dialogue_canvas_group.alpha = 0;
            dialogue_canvas_group.interactable = false;
            dialogue_canvas_group.blocksRaycasts = false;
        }

        if (cutscene_background != null)
        {
            cutscene_background.SetActive(false);
        }

        if (left_character_image != null) left_character_image.gameObject.SetActive(false);
        if (right_character_image != null) right_character_image.gameObject.SetActive(false);
    }

    public bool Is_Dialogue_Ongoing(bool with_case_ui = false)
    {
        if (dialogue_canvas_group != null && dialogue_canvas_group.alpha == 1)
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
        //Play sounds only if the source exists
        if (audioSource == null) return;

        if (npcs != null)
        {
            int npcs_size = npcs.Count;

            if (npcs_size > 0)
            {
                //Stop the audioSource so that the new sentence does not overlap with the old one
                //audioSource.Stop();

                Character_Data npc = npcs[line_index % npcs_size];

                //Play sentence sound
                if (npc.voiceSamples != null)
                {
                    audioSource.PlayOneShot(
                        npc.voiceSamples[UnityEngine.Random.Range(0, npc.voiceSamples.Count)]);
                }

                return;
            }
        }

        if (is_cutscene_mode && current_cutscene_frames != null) {
            Character_Data npc = current_cutscene_frames[line_index].speaker;

            if (npc != null && npc.voiceSamples != null)
            {
                audioSource.PlayOneShot(
                    npc.voiceSamples[UnityEngine.Random.Range(0, npc.voiceSamples.Count)]);

                return;
            }
        }

        if (default_voice_samples != null)
        {
            audioSource.PlayOneShot(
                default_voice_samples[UnityEngine.Random.Range(0, default_voice_samples.Count)]);
        }
    }

    #endregion
}