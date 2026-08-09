using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events; // Required for UnityEvents
using System.Collections;
using TMPro;

public class CutsceneController : MonoBehaviour
{
    public static CutsceneController Instance;

    [Header("Cutscene Data")]
    public Cutscene_Data cutsceneData;

    [Header("UI Image References")]
    public Image backgroundImage;
    public Image leftCharacterImage;
    public Image rightCharacterImage;
    public Image fadeImage;

    [Header("UI Text References")]
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;

    [Header("Visual Novel Focus Animation")]
    public float transitionDuration = 0.25f;
    public Vector3 activeScale = new Vector3(1.2f, 1.2f, 1f);
    public Vector3 inactiveScale = new Vector3(0.85f, 0.85f, 1f);
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    [Header("Scene Transition Settings")]
    public float fadeDuration = 2.0f;
    public string nextSceneName = "PrologueScene";
    public float waitTimeInBlack = 1.0f;
    public KeyCode advanceKey = KeyCode.Space;

    [Header("Cutscene Events")]
    public UnityEvent onCutsceneEnd; // Shows up as an Event Box in the Inspector!

    private int currentFrameIndex = 0;
    private Coroutine leftAnimCoroutine;
    private Coroutine rightAnimCoroutine;

    void Awake()
    {
        Instance = this;

        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }
    }

    void Start()
    {
        if (cutsceneData != null && cutsceneData.frames.Count > 0)
        {
            DisplayFrame(currentFrameIndex);
        }
        else
        {
            Debug.LogWarning("No Cutscene_Data assigned or frames list is empty.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(advanceKey))
        {
            NextFrame();
        }
    }

    public void NextFrame()
    {
        currentFrameIndex++;

        if (currentFrameIndex < cutsceneData.frames.Count)
        {
            DisplayFrame(currentFrameIndex);
        }
        else
        {
            // Trigger events (e.g. GemCutsceneListener.HideGem) when dialogue ends
            onCutsceneEnd?.Invoke();

            StartFadeOut();
        }
    }

    public void DisplayFrame(int index)
    {
        CutsceneFrame frame = cutsceneData.frames[index];

        // 1. Update Background
        if (backgroundImage != null)
        {
            if (cutsceneData.backgroundSprite != null)
            {
                backgroundImage.gameObject.SetActive(true);
                backgroundImage.sprite = cutsceneData.backgroundSprite;
            }
            else
            {
                backgroundImage.gameObject.SetActive(false);
            }
        }

        // 2. Determine target states based on Active Speaker
        Vector3 leftScale = inactiveScale, rightScale = inactiveScale;
        Color leftCol = inactiveColor, rightCol = inactiveColor;

        switch (frame.activeSpeaker)
        {
            case CutsceneFrame.ActiveSpeaker.Left:
                leftScale = activeScale;
                leftCol = activeColor;
                if (leftCharacterImage != null) leftCharacterImage.transform.SetAsLastSibling();

                rightScale = inactiveScale;
                rightCol = inactiveColor;
                break;

            case CutsceneFrame.ActiveSpeaker.Right:
                rightScale = activeScale;
                rightCol = activeColor;
                if (rightCharacterImage != null) rightCharacterImage.transform.SetAsLastSibling();

                leftScale = inactiveScale;
                leftCol = inactiveColor;
                break;

            default:
                leftScale = inactiveScale;
                leftCol = activeColor;
                rightScale = inactiveScale;
                rightCol = activeColor;
                break;
        }

        // 3. Update Sprites & Initial transform states
        UpdateSprite(leftCharacterImage, frame.leftCharacterSprite, leftScale, leftCol);
        UpdateSprite(rightCharacterImage, frame.rightCharacterSprite, rightScale, rightCol);

        // 4. Update Text & Italic state
        if (speakerNameText != null)
        {
            speakerNameText.text = frame.speakerName;
        }

        if (dialogueText != null)
        {
            dialogueText.text = frame.isItalic ? $"<i>{frame.dialogueLine}</i>" : frame.dialogueLine;
        }

        // 5. Trigger Focus Animations
        if (leftCharacterImage != null && leftCharacterImage.gameObject.activeSelf)
        {
            if (leftAnimCoroutine != null) StopCoroutine(leftAnimCoroutine);
            leftAnimCoroutine = StartCoroutine(AnimateCharacter(leftCharacterImage, leftScale, leftCol));
        }

        if (rightCharacterImage != null && rightCharacterImage.gameObject.activeSelf)
        {
            if (rightAnimCoroutine != null) StopCoroutine(rightAnimCoroutine);
            rightAnimCoroutine = StartCoroutine(AnimateCharacter(rightCharacterImage, rightScale, rightCol));
        }
    }

    private void UpdateSprite(Image img, Sprite sprite, Vector3 defaultScale, Color defaultColor)
    {
        if (img == null) return;

        if (sprite != null)
        {
            bool wasInactive = !img.gameObject.activeSelf;
            img.gameObject.SetActive(true);
            img.sprite = sprite;

            if (wasInactive)
            {
                img.transform.localScale = defaultScale;
                img.color = defaultColor;
            }
        }
        else
        {
            img.gameObject.SetActive(false);
        }
    }

    private IEnumerator AnimateCharacter(Image targetImage, Vector3 targetScale, Color targetColor)
    {
        Vector3 startScale = targetImage.transform.localScale;
        Color startColor = targetImage.color;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / transitionDuration);

            targetImage.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            targetImage.color = Color.Lerp(startColor, targetColor, t);

            yield return null;
        }

        targetImage.transform.localScale = targetScale;
        targetImage.color = targetColor;
    }

    public void StartFadeOut()
    {
        if (fadeImage != null)
        {
            StartCoroutine(FadeToBlackAndLoad());
        }
        else
        {
            Debug.LogError("Fade Image is not assigned on the CutsceneController!");
        }
    }

    IEnumerator FadeToBlackAndLoad()
    {
        float elapsed = 0f;
        Color tempColor = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            tempColor.a = Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = tempColor;
            yield return null;
        }

        yield return new WaitForSeconds(waitTimeInBlack);

        Debug.Log("Loading Scene: " + nextSceneName);
        SceneManager.LoadScene(nextSceneName);
    }
}