using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PrologueIntroController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The Black Image that covers the screen")]
    public Image fadeImage;
    public float fadeDuration = 2.5f;

    [Header("Systems to Trigger")]
    [Tooltip("The UI Panel that contains your dialogue text boxes and sprites")]
    public GameObject dialoguePanel;

    [Tooltip("Drag the object with the Talking_Manager script here")]
    public Talking_Manager talkingManager;

    void Awake()
    {
        // Force the screen to be black the instant the scene loads
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color c = fadeImage.color;
            c.a = 1f;
            fadeImage.color = c;
        }

        // Hide the dialogue panel so it doesn't pop up over the black screen
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    void Start()
    {
        if (fadeImage != null)
        {
            StartCoroutine(FadeInFromBlack());
        }
        else
        {
            Debug.LogError("PrologueIntroController: No Fade Image assigned!");
        }
    }

    IEnumerator FadeInFromBlack()
    {
        // Wait a small moment to let the scene settle
        yield return new WaitForSeconds(0.5f);

        float elapsed = 0;
        Color tempColor = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            // Smoothly transition Alpha from 1 (Black) to 0 (Clear)
            tempColor.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            fadeImage.color = tempColor;
            yield return null;
        }

        // Ensure it's fully clear and disable it
        tempColor.a = 0f;
        fadeImage.color = tempColor;
        fadeImage.gameObject.SetActive(false);

        Debug.Log("Fade complete. Initializing Dialogue...");

        // Brief pause for "dramatic effect" before the text starts
        yield return new WaitForSeconds(0.3f);

        StartDialogue();
    }

    void StartDialogue()
    {
        // 1. Reveal the Dialogue UI Panel
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        // 2. Tell the Talking_Manager to start the first line
        if (talkingManager != null)
        {
            talkingManager.StartDialogueSequence();
        }
        else
        {
            Debug.LogError("PrologueIntroController: Talking Manager is not assigned!");
        }
    }
}