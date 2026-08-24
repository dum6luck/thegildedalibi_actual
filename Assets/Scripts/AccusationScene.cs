using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

/*
 * AccusationScene
 * ----------------
 * "Who killed Max?" suspect-select screen. Four suspects stand in a row;
 * hovering zooms their sprite in slightly. Clicking a suspect slams down
 * a "GUILTY" stamp on their portrait, then the whole screen fades to
 * black before loading the next scene (e.g. your newspaper/ending scene).
 *
 * SETUP:
 * 1. Create an empty GameObject (e.g. "AccusationScene") in your scene.
 * 2. Add this script to it.
 * 3. In the Inspector, fill in the 4 entries under "Suspects": drag a
 *    portrait Sprite into each Portrait slot, set their name and role,
 *    and tick "Is Guilty" on the ONE actual culprit.
 * 4. Set "Next Scene Name" to whatever scene should load after a click
 *    (your newspaper/ending scene). If a suspect needs to load a
 *    DIFFERENT scene than the others, fill in their own "Scene Override".
 * 5. Press Play. No manual Canvas/UI setup required - it's all built at runtime.
 *
 * READING THE RESULT IN YOUR NEXT SCENE:
 * Whichever suspect gets clicked is stashed in the static CaseFileResult
 * class below, which survives the scene load. From any script in your
 * next scene you can read:
 *     CaseFileResult.AccusedName
 *     CaseFileResult.AccusedRole
 *     CaseFileResult.WasGuilty   (true = correct accusation)
 */

[System.Serializable]
public class Suspect
{
    public string suspectName = "Suspect";
    public string role = "Role";
    [Tooltip("Drag a Sprite asset here (drag the character's image in from your Project window).")]
    public Sprite portrait;
    [Tooltip("Tick this on exactly one suspect - the real culprit.")]
    public bool isGuilty;
    [Tooltip("Optional. If set, clicking THIS suspect loads this scene instead of Next Scene Name.")]
    public string sceneOverride = "";
}

// Simple static carrier so the next scene knows who got picked.
// Survives the SceneManager.LoadScene call since it's just a static class.
public static class CaseFileResult
{
    public static string AccusedName = "";
    public static string AccusedRole = "";
    public static bool WasGuilty = false;
}

public class AccusationScene : MonoBehaviour
{
    [Header("Suspects (drag 4 portraits in, tick the guilty one)")]
    public Suspect[] suspects = new Suspect[4];

    [Header("Scene Text")]
    public string sceneTitle = "WHO KILLED MAX?";

    [Header("Scene Navigation")]
    [Tooltip("Scene loaded after a suspect is clicked, unless that suspect has its own Scene Override set.")]
    public string nextSceneName = "";

    [Header("Audio (optional)")]
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public AudioClip stampSound;

    [Header("Theme")]
    public Color colorBackground = new Color32(0xDE, 0xC7, 0x93, 0xFF);
    public Color colorText = new Color32(0x1A, 0x14, 0x0A, 0xFF);
    public Color colorTextSecondary = new Color32(0x4A, 0x3B, 0x24, 0xFF);

    [Header("Portrait Size")]
    [Tooltip("Base size of each portrait before any hover zoom is applied.")]
    public Vector2 portraitBaseSize = new Vector2(340, 550);
    [Tooltip("Height reserved for the clickable/hoverable portrait area (should be >= portraitBaseSize.y).")]
    public float portraitAreaHeight = 550f;
    [Tooltip("Height reserved for each suspect card (portrait + name + role).")]
    public float cardHeight = 640f;

    [Header("Hover Zoom")]
    [Range(1f, 1.5f)]
    public float hoverScale = 1.1f;
    public float hoverDuration = 0.2f;

    [Header("Accusation Stamp")]
    public string stampText = "GUILTY";
    public Color stampColor = new Color32(0xA6, 0x1B, 0x1B, 0xFF);
    [Tooltip("How long the stamp sits on screen before the fade-out starts.")]
    public float stampHoldDuration = 0.9f;

    [Header("Fade Out")]
    [Tooltip("How long the whole-screen fade to black takes before the next scene loads.")]
    public float fadeOutDuration = 1.3f;
    public Color fadeColor = Color.black;

    // --- runtime refs ---
    private AudioSource audioSource;
    private CanvasGroup fadeOverlayGroup;

    private class Card
    {
        public RectTransform portraitRT;
        public RectTransform portraitAreaRT;
        public CanvasGroup cardGroup;
        public Button button;
        public Coroutine scaleAnim;
    }
    private readonly List<Card> cards = new List<Card>();

    private bool selectionMade;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        EnsureFourSuspects();
        BuildUI();
    }

    private void EnsureFourSuspects()
    {
        if (suspects == null || suspects.Length != 4)
        {
            Suspect[] fixedArr = new Suspect[4];
            for (int i = 0; i < 4; i++)
            {
                fixedArr[i] = (suspects != null && i < suspects.Length && suspects[i] != null)
                    ? suspects[i]
                    : new Suspect { suspectName = "Suspect " + (i + 1) };
            }
            suspects = fixedArr;
        }
        for (int i = 0; i < 4; i++)
        {
            if (suspects[i] == null) suspects[i] = new Suspect { suspectName = "Suspect " + (i + 1) };
        }
    }

    // ==================== UI CONSTRUCTION ====================

    private void BuildUI()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        GameObject canvasGO = new GameObject("AccusationCanvas", typeof(RectTransform));
        canvasGO.transform.SetParent(transform, false);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);
        canvasGO.AddComponent<GraphicRaycaster>();

        RectTransform canvasRT = canvasGO.GetComponent<RectTransform>();
        canvasRT.anchorMin = Vector2.zero;
        canvasRT.anchorMax = Vector2.one;
        canvasRT.offsetMin = Vector2.zero;
        canvasRT.offsetMax = Vector2.zero;

        GameObject bg = CreatePanel("Background", canvasGO.transform, colorBackground);
        StretchFull(bg.GetComponent<RectTransform>());

        GameObject root = CreatePanel("Root", canvasGO.transform, new Color(0, 0, 0, 0));
        StretchFull(root.GetComponent<RectTransform>());
        VerticalLayoutGroup rootVLG = root.AddComponent<VerticalLayoutGroup>();
        rootVLG.padding = new RectOffset(60, 60, 50, 50);
        rootVLG.spacing = 20;
        rootVLG.childAlignment = TextAnchor.UpperCenter;
        rootVLG.childForceExpandWidth = true;
        rootVLG.childForceExpandHeight = false;
        rootVLG.childControlWidth = true;
        rootVLG.childControlHeight = true;

        var titleText = CreateText("TitleText", root.transform, sceneTitle, 54, colorText, TextAlignmentOptions.Center, FontStyles.Bold);
        AddLayoutElement(titleText.gameObject, preferredHeight: 70, flexibleHeight: 0);

        GameObject spacer = CreatePanel("Spacer", root.transform, new Color(0, 0, 0, 0));
        AddLayoutElement(spacer, flexibleHeight: 1);

        GameObject cardRow = CreatePanel("CardRow", root.transform, new Color(0, 0, 0, 0));
        AddLayoutElement(cardRow, preferredHeight: cardHeight, flexibleHeight: 0);
        HorizontalLayoutGroup rowHLG = cardRow.AddComponent<HorizontalLayoutGroup>();
        rowHLG.spacing = 50;
        rowHLG.childAlignment = TextAnchor.LowerCenter;
        rowHLG.childForceExpandWidth = true;
        rowHLG.childForceExpandHeight = false;
        rowHLG.childControlWidth = true;
        rowHLG.childControlHeight = true;

        for (int i = 0; i < 4; i++)
        {
            CreateSuspectCard(i, cardRow.transform);
        }

        GameObject bottomSpacer = CreatePanel("BottomSpacer", root.transform, new Color(0, 0, 0, 0));
        AddLayoutElement(bottomSpacer, flexibleHeight: 1);

        // Full-screen fade overlay for the dramatic transition, sits above everything
        GameObject fadeGO = CreatePanel("FadeOverlay", canvasGO.transform, fadeColor);
        StretchFull(fadeGO.GetComponent<RectTransform>());
        fadeOverlayGroup = fadeGO.AddComponent<CanvasGroup>();
        fadeOverlayGroup.alpha = 0f;
        fadeOverlayGroup.blocksRaycasts = false;
        fadeOverlayGroup.interactable = false;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    private void CreateSuspectCard(int index, Transform parent)
    {
        Suspect s = suspects[index];

        GameObject cardGO = new GameObject("SuspectCard_" + index, typeof(RectTransform));
        cardGO.transform.SetParent(parent, false);
        AddLayoutElement(cardGO, preferredWidth: 280, flexibleWidth: 1, preferredHeight: cardHeight);
        CanvasGroup cardGroup = cardGO.AddComponent<CanvasGroup>();

        VerticalLayoutGroup cardVLG = cardGO.AddComponent<VerticalLayoutGroup>();
        cardVLG.spacing = 6;
        cardVLG.childAlignment = TextAnchor.LowerCenter;
        cardVLG.childForceExpandWidth = true;
        cardVLG.childForceExpandHeight = false;
        cardVLG.childControlWidth = true;
        cardVLG.childControlHeight = true;

        // Portrait area - the clickable, hoverable region
        GameObject portraitArea = new GameObject("PortraitArea", typeof(RectTransform));
        portraitArea.transform.SetParent(cardGO.transform, false);
        AddLayoutElement(portraitArea, preferredHeight: portraitAreaHeight, flexibleHeight: 0);
        RectTransform areaRT = portraitArea.GetComponent<RectTransform>();

        // Invisible full-area image so the whole region is clickable/hoverable
        Image hitboxImg = portraitArea.AddComponent<Image>();
        hitboxImg.color = new Color(0, 0, 0, 0);

        // Portrait, pivoted at the bottom so zoom-in grows upward like it's stepping toward camera
        GameObject portraitGO = new GameObject("Portrait", typeof(RectTransform));
        portraitGO.transform.SetParent(portraitArea.transform, false);
        RectTransform portraitRT = portraitGO.GetComponent<RectTransform>();
        portraitRT.anchorMin = new Vector2(0.5f, 0f);
        portraitRT.anchorMax = new Vector2(0.5f, 0f);
        portraitRT.pivot = new Vector2(0.5f, 0f);
        portraitRT.sizeDelta = portraitBaseSize;
        portraitRT.anchoredPosition = Vector2.zero;
        Image portraitImg = portraitGO.AddComponent<Image>();
        portraitImg.raycastTarget = false;
        if (s.portrait != null)
        {
            portraitImg.sprite = s.portrait;
            portraitImg.preserveAspect = true;
        }
        else
        {
            portraitImg.color = new Color32(0x33, 0x2A, 0x1B, 0xFF);
            var placeholderTxt = CreateText("Placeholder", portraitGO.transform, "?", 72, colorBackground, TextAlignmentOptions.Center, FontStyles.Bold);
            StretchFull(placeholderTxt.GetComponent<RectTransform>());
        }

        // Stamp text, hidden until this suspect is accused
        GameObject stampGO = new GameObject("Stamp", typeof(RectTransform));
        stampGO.transform.SetParent(portraitArea.transform, false);
        RectTransform stampRT = stampGO.GetComponent<RectTransform>();
        stampRT.anchorMin = new Vector2(0.5f, 0.55f);
        stampRT.anchorMax = new Vector2(0.5f, 0.55f);
        stampRT.pivot = new Vector2(0.5f, 0.5f);
        stampRT.sizeDelta = new Vector2(300, 100);
        stampRT.anchoredPosition = Vector2.zero;
        stampRT.localRotation = Quaternion.Euler(0, 0, -16f);
        stampRT.localScale = Vector3.zero;
        CanvasGroup stampGroup = stampGO.AddComponent<CanvasGroup>();
        stampGroup.alpha = 0f;
        TextMeshProUGUI stampTMP = CreateText("StampText", stampGO.transform, stampText.ToUpper(), 44, stampColor, TextAlignmentOptions.Center, FontStyles.Bold);
        StretchFull(stampTMP.GetComponent<RectTransform>());
        stampTMP.characterSpacing = 4f;
        // Boxed border around the stamp text to sell the "ink stamp" look (plain Image border, no Outline component - works fine on Unity 2021)
        GameObject stampBoxGO = new GameObject("StampBox", typeof(RectTransform));
        stampBoxGO.transform.SetParent(stampGO.transform, false);
        stampBoxGO.transform.SetAsFirstSibling();
        RectTransform stampBoxRT = stampBoxGO.GetComponent<RectTransform>();
        StretchFull(stampBoxRT);
        Image stampBoxImg = stampBoxGO.AddComponent<Image>();
        stampBoxImg.color = new Color(0, 0, 0, 0);
        GameObject stampBorderGO = new GameObject("StampBorder", typeof(RectTransform));
        stampBorderGO.transform.SetParent(stampBoxGO.transform, false);
        RectTransform stampBorderRT = stampBorderGO.GetComponent<RectTransform>();
        stampBorderRT.anchorMin = Vector2.zero;
        stampBorderRT.anchorMax = Vector2.one;
        stampBorderRT.offsetMin = new Vector2(6, 6);
        stampBorderRT.offsetMax = new Vector2(-6, -6);
        Image stampBorderImg = stampBorderGO.AddComponent<Image>();
        stampBorderImg.color = new Color(0, 0, 0, 0);
        Image stampBorderOutline = AddSimpleBorder(stampBorderGO, stampColor, 5f);

        Button button = portraitArea.AddComponent<Button>();
        ColorBlock cb = button.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = Color.white;
        cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        button.colors = cb;

        // Name + role labels below the portrait, matching the sketch order
        var nameTxt = CreateText("NameText", cardGO.transform, s.suspectName.ToUpper(), 24, colorText, TextAlignmentOptions.Center, FontStyles.Bold);
        AddLayoutElement(nameTxt.gameObject, preferredHeight: 32, flexibleHeight: 0);

        var roleTxt = CreateText("RoleText", cardGO.transform, s.role.ToUpper(), 17, colorTextSecondary, TextAlignmentOptions.Center, FontStyles.Normal);
        AddLayoutElement(roleTxt.gameObject, preferredHeight: 24, flexibleHeight: 0);

        Card card = new Card
        {
            portraitRT = portraitRT,
            portraitAreaRT = areaRT,
            cardGroup = cardGroup,
            button = button
        };
        cards.Add(card);

        int capturedIndex = index;

        EventTrigger trigger = portraitArea.AddComponent<EventTrigger>();

        EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) => { OnCardHoverEnter(capturedIndex); });
        trigger.triggers.Add(enterEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => { OnCardHoverExit(capturedIndex); });
        trigger.triggers.Add(exitEntry);

        button.onClick.AddListener(() =>
        {
            if (!selectionMade) StartCoroutine(SelectSuspect(capturedIndex, stampRT, stampGroup));
        });
    }

    // Builds a thin, plain-color rectangle "frame" out of 4 Images instead of the
    // Outline component (Outline's shader-based glow doesn't render reliably
    // pre-2022, so this uses regular UI Images instead - fully 2021-safe).
    private Image AddSimpleBorder(GameObject parent, Color color, float thickness)
    {
        CreateBorderEdge(parent.transform, color, new Vector2(0, 1), new Vector2(1, 1), thickness, true);   // top
        CreateBorderEdge(parent.transform, color, new Vector2(0, 0), new Vector2(1, 0), thickness, true);   // bottom
        CreateBorderEdge(parent.transform, color, new Vector2(0, 0), new Vector2(0, 1), thickness, false);  // left
        CreateBorderEdge(parent.transform, color, new Vector2(1, 0), new Vector2(1, 1), thickness, false);  // right
        return null;
    }

    private void CreateBorderEdge(Transform parent, Color color, Vector2 anchorA, Vector2 anchorB, float thickness, bool horizontal)
    {
        GameObject edgeGO = new GameObject("BorderEdge", typeof(RectTransform));
        edgeGO.transform.SetParent(parent, false);
        RectTransform rt = edgeGO.GetComponent<RectTransform>();
        rt.anchorMin = anchorA;
        rt.anchorMax = anchorB;
        rt.pivot = new Vector2(0.5f, 0.5f);
        if (horizontal)
        {
            rt.sizeDelta = new Vector2(0, thickness);
        }
        else
        {
            rt.sizeDelta = new Vector2(thickness, 0);
        }
        rt.anchoredPosition = Vector2.zero;
        Image img = edgeGO.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
    }

    // ==================== HOVER ====================

    private void OnCardHoverEnter(int index)
    {
        if (selectionMade) return;
        Card card = cards[index];

        if (card.scaleAnim != null) StopCoroutine(card.scaleAnim);
        card.scaleAnim = StartCoroutine(AnimateScale(card.portraitRT, Vector3.one * hoverScale, hoverDuration));

        if (audioSource != null && hoverSound != null) audioSource.PlayOneShot(hoverSound, 0.5f);
    }

    private void OnCardHoverExit(int index)
    {
        if (selectionMade) return;
        Card card = cards[index];

        if (card.scaleAnim != null) StopCoroutine(card.scaleAnim);
        card.scaleAnim = StartCoroutine(AnimateScale(card.portraitRT, Vector3.one, hoverDuration));
    }

    // ==================== SELECTION ====================

    private IEnumerator SelectSuspect(int index, RectTransform stampRT, CanvasGroup stampGroup)
    {
        selectionMade = true;
        foreach (Card c in cards) c.button.interactable = false;

        Suspect s = suspects[index];

        if (audioSource != null && clickSound != null) audioSource.PlayOneShot(clickSound);

        // Dim the other three suspects so focus lands on the accused
        for (int i = 0; i < cards.Count; i++)
        {
            if (i == index) continue;
            StartCoroutine(FadeCanvasGroup(cards[i].cardGroup, cards[i].cardGroup.alpha, 0.25f, 0.4f));
        }

        // Little confirm punch on the chosen portrait
        yield return StartCoroutine(AnimateScale(cards[index].portraitRT, Vector3.one * (hoverScale * 1.06f), 0.12f));

        // Slam the stamp down
        if (audioSource != null && stampSound != null) audioSource.PlayOneShot(stampSound);
        yield return StartCoroutine(StampSlam(stampRT, stampGroup));

        yield return new WaitForSeconds(stampHoldDuration);

        // Dramatic whole-screen fade to black
        fadeOverlayGroup.blocksRaycasts = true;
        yield return StartCoroutine(FadeCanvasGroup(fadeOverlayGroup, 0f, 1f, fadeOutDuration));

        CaseFileResult.AccusedName = s.suspectName;
        CaseFileResult.AccusedRole = s.role;
        CaseFileResult.WasGuilty = s.isGuilty;

        string targetScene = !string.IsNullOrEmpty(s.sceneOverride) ? s.sceneOverride : nextSceneName;
        if (!string.IsNullOrEmpty(targetScene))
        {
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            Debug.LogWarning("AccusationScene: " + s.suspectName + " was accused, but no Next Scene Name or Scene Override is set.");
            yield return StartCoroutine(FadeCanvasGroup(fadeOverlayGroup, 1f, 0f, 0.4f));
            fadeOverlayGroup.blocksRaycasts = false;
            selectionMade = false;
            foreach (Card c in cards) c.button.interactable = true;
            foreach (Card c in cards) StartCoroutine(FadeCanvasGroup(c.cardGroup, c.cardGroup.alpha, 1f, 0.3f));
        }
    }

    // ==================== ANIMATION HELPERS ====================

    private IEnumerator AnimateScale(RectTransform rt, Vector3 targetScale, float duration)
    {
        Vector3 startScale = rt.localScale;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = EaseOutBack(Mathf.Clamp01(t / duration));
            rt.localScale = Vector3.LerpUnclamped(startScale, targetScale, p);
            yield return null;
        }
        rt.localScale = targetScale;
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

    private IEnumerator StampSlam(RectTransform rt, CanvasGroup group)
    {
        Vector3 startScale = Vector3.one * 2.4f;
        Vector3 endScale = Vector3.one;
        rt.localScale = startScale;
        group.alpha = 0f;
        float t = 0f;
        float duration = 0.16f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            rt.localScale = Vector3.LerpUnclamped(startScale, endScale, p);
            group.alpha = p;
            yield return null;
        }
        rt.localScale = endScale;
        group.alpha = 1f;

        // Quick settle shake so it reads as a physical stamp impact
        Vector2 original = rt.anchoredPosition;
        float shakeDuration = 0.12f;
        float magnitude = 5f;
        t = 0f;
        while (t < shakeDuration)
        {
            t += Time.deltaTime;
            float damper = 1f - (t / shakeDuration);
            rt.anchoredPosition = original + Random.insideUnitCircle * magnitude * damper;
            yield return null;
        }
        rt.anchoredPosition = original;
    }

    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
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

    private LayoutElement AddLayoutElement(GameObject go, float preferredWidth = -1, float preferredHeight = -1, float flexibleWidth = -1, float flexibleHeight = -1)
    {
        LayoutElement le = go.AddComponent<LayoutElement>();
        if (preferredWidth >= 0) le.preferredWidth = preferredWidth;
        if (preferredHeight >= 0) le.preferredHeight = preferredHeight;
        if (flexibleWidth >= 0) le.flexibleWidth = flexibleWidth;
        if (flexibleHeight >= 0) le.flexibleHeight = flexibleHeight;
        return le;
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