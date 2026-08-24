using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

/*
 * EndingImageScene
 * -----------------
 * Simple ending screen: starts on black, fades in a single image, then
 * fades in a "CLICK TO CONTINUE" button. Clicking the button fades back
 * to black and loads the title screen.
 *
 * Use the SAME script for both the guilty and not-guilty endings - just
 * drag a different image into "Display Image" on each scene's instance.
 *
 * SETUP:
 * 1. Create an empty GameObject (e.g. "EndingScene") in your ending scene.
 * 2. Add this script to it.
 * 3. Drag your ending image into "Display Image".
 * 4. Set "Title Scene Name" to whatever scene your main menu / title
 *    screen lives in.
 * 5. Press Play. No manual Canvas/UI setup required.
 */
public class EndingImageScene : MonoBehaviour
{
    [Header("Content")]
    [Tooltip("The image to fade in on screen.")]
    public Sprite displayImage;
    [Tooltip("Text shown on the continue button.")]
    public string buttonLabel = "CLICK TO CONTINUE";

    [Header("Scene Navigation")]
    [Tooltip("Scene loaded after the button is clicked (your title/main menu scene).")]
    public string titleSceneName = "";

    [Header("Theme")]
    public Color colorBackground = Color.black;
    public Color colorButtonBG = new Color32(0x20, 0x20, 0x20, 0xE0);
    public Color colorButtonBGHover = new Color32(0x35, 0x35, 0x35, 0xE0);
    public Color colorButtonText = Color.white;

    [Header("Timing")]
    [Tooltip("How long the image takes to fade in.")]
    public float imageFadeInDuration = 1.5f;
    [Tooltip("Pause after the image finishes fading in, before the button appears.")]
    public float delayBeforeButton = 0.8f;
    [Tooltip("How long the button takes to fade in.")]
    public float buttonFadeInDuration = 0.6f;
    [Tooltip("How long the whole-screen fade to black takes before the next scene loads.")]
    public float fadeOutDuration = 1f;

    private CanvasGroup imageGroup;
    private CanvasGroup buttonGroup;
    private CanvasGroup fadeOverlayGroup;
    private Button continueButton;
    private bool clicked;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        BuildUI();
        StartCoroutine(PlaySequence());
    }

    // ==================== UI CONSTRUCTION ====================

    private void BuildUI()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        GameObject canvasGO = new GameObject("EndingCanvas", typeof(RectTransform));
        canvasGO.transform.SetParent(transform, false);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);
        canvasGO.AddComponent<GraphicRaycaster>();
        StretchFull(canvasGO.GetComponent<RectTransform>());

        GameObject bg = CreatePanel("Background", canvasGO.transform, colorBackground);
        StretchFull(bg.GetComponent<RectTransform>());

        // Image, stretched to fill the entire screen. Uses an AspectRatioFitter in
        // "Envelope Parent" mode so it covers the full screen without stretching/
        // distorting - it scales up and crops overflow instead, like a CSS
        // background-size: cover.
        GameObject imageGO = new GameObject("DisplayImage", typeof(RectTransform));
        imageGO.transform.SetParent(canvasGO.transform, false);
        RectTransform imageRT = imageGO.GetComponent<RectTransform>();
        StretchFull(imageRT);
        Image img = imageGO.AddComponent<Image>();
        img.raycastTarget = false;
        if (displayImage != null)
        {
            img.sprite = displayImage;
            img.color = Color.white;
            AspectRatioFitter fitter = imageGO.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = displayImage.rect.width / displayImage.rect.height;
        }
        else
        {
            img.color = new Color32(0x33, 0x33, 0x33, 0xFF);
        }
        imageGroup = imageGO.AddComponent<CanvasGroup>();
        imageGroup.alpha = 0f;

        // Continue button, bottom right corner, starts invisible
        GameObject buttonGO = new GameObject("ContinueButton", typeof(RectTransform));
        buttonGO.transform.SetParent(canvasGO.transform, false);
        RectTransform buttonRT = buttonGO.GetComponent<RectTransform>();
        buttonRT.anchorMin = new Vector2(1f, 0f);
        buttonRT.anchorMax = new Vector2(1f, 0f);
        buttonRT.pivot = new Vector2(1f, 0f);
        buttonRT.sizeDelta = new Vector2(340, 80);
        buttonRT.anchoredPosition = new Vector2(-60, 60);
        buttonGroup = buttonGO.AddComponent<CanvasGroup>();
        buttonGroup.alpha = 0f;
        buttonGroup.interactable = false;
        buttonGroup.blocksRaycasts = false;

        Image buttonBG = buttonGO.AddComponent<Image>();
        buttonBG.color = colorButtonBG;

        continueButton = buttonGO.AddComponent<Button>();
        ColorBlock cb = continueButton.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1f, 1f, 1f, 0.9f);
        cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        continueButton.colors = cb;

        var buttonText = CreateText("ButtonText", buttonGO.transform, buttonLabel.ToUpper(), 26, colorButtonText, TextAlignmentOptions.Center, FontStyles.Bold);
        StretchFull(buttonText.GetComponent<RectTransform>());
        buttonText.characterSpacing = 2f;
        buttonText.raycastTarget = false;

        continueButton.onClick.AddListener(OnContinueClicked);

        // Full-screen fade overlay for the transition out, sits above everything
        GameObject fadeGO = CreatePanel("FadeOverlay", canvasGO.transform, colorBackground);
        StretchFull(fadeGO.GetComponent<RectTransform>());
        fadeOverlayGroup = fadeGO.AddComponent<CanvasGroup>();
        fadeOverlayGroup.alpha = 1f;
        fadeOverlayGroup.blocksRaycasts = true;
        fadeOverlayGroup.interactable = false;
    }

    // ==================== SEQUENCE ====================

    private IEnumerator PlaySequence()
    {
        // Reveal the scene (in case the overlay was covering it)
        yield return StartCoroutine(FadeCanvasGroup(fadeOverlayGroup, 1f, 0f, 0.3f));
        fadeOverlayGroup.blocksRaycasts = false;

        // Fade in the image
        yield return StartCoroutine(FadeCanvasGroup(imageGroup, 0f, 1f, imageFadeInDuration));

        yield return new WaitForSeconds(delayBeforeButton);

        // Fade in the button and make it clickable
        yield return StartCoroutine(FadeCanvasGroup(buttonGroup, 0f, 1f, buttonFadeInDuration));
        buttonGroup.interactable = true;
        buttonGroup.blocksRaycasts = true;

        // Wait for the click
        yield return new WaitUntil(() => clicked);

        buttonGroup.interactable = false;
        buttonGroup.blocksRaycasts = false;

        // Fade to black, then load the title screen
        fadeOverlayGroup.blocksRaycasts = true;
        yield return StartCoroutine(FadeCanvasGroup(fadeOverlayGroup, 0f, 1f, fadeOutDuration));

        if (!string.IsNullOrEmpty(titleSceneName))
        {
            SceneManager.LoadScene(titleSceneName);
        }
        else
        {
            Debug.LogWarning("EndingImageScene: no Title Scene Name set - staying on this scene.");
        }
    }

    private void OnContinueClicked()
    {
        clicked = true;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        cg.alpha = from;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        cg.alpha = to;
    }

    // ==================== UI BUILDING HELPERS ====================

    private void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        if (color.a <= 0f) img.raycastTarget = false;
        return go;
    }

    private TextMeshProUGUI CreateText(string name, Transform parent, string text, float fontSize, Color color,
        TextAlignmentOptions align = TextAlignmentOptions.MidlineLeft, FontStyles style = FontStyles.Normal)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = align;
        tmp.fontStyle = style;
        tmp.enableWordWrapping = true;
        return tmp;
    }
}