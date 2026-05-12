using UnityEngine;
using TMPro;
using System.Collections;

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

    [Header("Typewriter Settings")]
    public float type_speed = 0.05f;

    [Header("Arrow Bounce Settings")]
    public float bounce_speed = 5f;
    public float bounce_amplitude = 10f;

    private bool is_typing = false;
    private bool can_close = false;
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
                // Second click (when arrow is up): Close the panel
                Hide_Dialogue();
            }
        }
    }

    public void Show_Dialogue(string name, string text)
    {
        name_text.text = name;
        dialogue_text.text = "";
        current_full_text = text; // Cache the full sentence
        can_close = false;
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

    IEnumerator Typewriter_Effect(string text)
    {
        floating_arrow.SetActive(false);

        foreach (char c in text.ToCharArray())
        {
            dialogue_text.text += c;
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
        if (floating_arrow != null) floating_arrow.SetActive(false);

        if (dialogue_canvas_group != null)
        {
            dialogue_canvas_group.alpha = 0;
            dialogue_canvas_group.interactable = false;
            dialogue_canvas_group.blocksRaycasts = false;
        }
    }
}