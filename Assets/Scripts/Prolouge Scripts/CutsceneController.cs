using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
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
    public UnityEvent onCutsceneEnd;

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

        if (backgroundImage != null)
        {
            backgroundImage.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        if (cutsceneData != null && cutsceneData.frames.Count > 0)
        {
            ApplyCutsceneBackground();
            DisplayFrame(currentFrameIndex);
        }
    }

    void Update()
    {
        if (cutsceneData != null && (Input.GetKeyDown(advanceKey) || Input.GetMouseButtonDown(0)))
        {
            NextFrame();
        }
    }

    public void PlayCutscene(Cutscene_Data newData)
    {
        if (newData == null)
        {
            Debug.LogWarning("Tried to play a cutscene with null Cutscene_Data!");
            return;
        }

        cutsceneData = newData;
        currentFrameIndex = 0;

        ApplyCutsceneBackground();
        DisplayFrame(currentFrameIndex);
    }

    public void ApplyCutsceneBackground()
    {
        if (backgroundImage == null || cutsceneData == null) return;

        if (cutsceneData.backgroundSprite != null)
        {
            backgroundImage.gameObject.SetActive(true);
            backgroundImage.sprite = cutsceneData.backgroundSprite;
            backgroundImage.color = Color.white;
        }
        else
        {
            backgroundImage.sprite = null;
            backgroundImage.gameObject.SetActive(false);
        }
    }

    public void NextFrame()
    {
        currentFrameIndex++;

        if (cutsceneData != null && currentFrameIndex < cutsceneData.frames.Count)
        {
            DisplayFrame(currentFrameIndex);
        }
        else
        {
            onCutsceneEnd?.Invoke();
            StartFadeOut();
        }
    }

    public void DisplayFrame(int index)
    {
        if (cutsceneData == null || index >= cutsceneData.frames.Count) return;

        CutsceneFrame frame = cutsceneData.frames[index];

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

        UpdateSprite(leftCharacterImage, frame.leftCharacterSprite, leftScale, leftCol);
        UpdateSprite(rightCharacterImage, frame.rightCharacterSprite, rightScale, rightCol);

        if (speakerNameText != null)
        {
            speakerNameText.gameObject.SetActive(true);
            if (speakerNameText.transform.parent != null)
                speakerNameText.transform.parent.gameObject.SetActive(true);

            speakerNameText.text = (frame.speaker != null) ? frame.speaker.name : "";
        }

        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(true);
            if (dialogueText.transform.parent != null)
                dialogueText.transform.parent.gameObject.SetActive(true);

            dialogueText.text = frame.isItalic ? $"<i>{frame.dialogueLine}</i>" : frame.dialogueLine;
        }

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

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}