using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

/*
 * OutlookStyleEmailUI
 * -------------------
 * Builds a full Outlook-style email client UI entirely at runtime:
 * a folder sidebar, a scrollable email list, and a reading pane.
 *
 * SETUP:
 * 1. Create an empty GameObject in your scene (e.g. "EmailClient").
 * 2. Add this script to it.
 * 3. Press Play. That's it - no manual Canvas/UI setup required.
 *
 * CUSTOMIZE:
 * - Edit the `emails` list in the Inspector, or leave it empty to use
 *   the built-in sample emails.
 * - Call AddEmail() at runtime (e.g. from your clue/dialogue system)
 *   to push a new email into the list.
 * - Tweak the Theme Colors section in the Inspector to restyle it.
 * - Assign a clip to `loginSound` in the Inspector to hear a chime
 *   when the "signing in" screen plays.
 *
 * Requires TextMeshPro (TMP Essential Resources imported - you already
 * have this since your other scripts use TextMeshProUGUI).
 */

[System.Serializable]
public class EmailData
{
    public string senderName = "Sender";
    public string subject = "Subject";
    public string preview = "Email preview text...";

    [TextArea(3, 10)]
    public string body = "Full email body text.";

    public string timestamp = "12:00 PM";
    public bool isUnread = true;
}

public class OutlookStyleEmailUI : MonoBehaviour
{
    [Header("Emails (leave empty to use built-in samples)")]
    public List<EmailData> emails = new List<EmailData>();

    [Header("Sidebar Folders (display only)")]
    public List<string> folderNames = new List<string>
    {
        "Inbox", "Drafts", "Sent Items", "Deleted Items", "Junk Email", "Archive"
    };

    [Header("Scene Navigation")]
    [Tooltip("Scene to load when the player clicks the button to leave the email screen.")]
    public string mainGameSceneName = "Main_Game";

    [Header("Login Sequence")]
    [Tooltip("Name shown on the fake Windows-style sign-in screen.")]
    public string userDisplayName = "Detective";
    [Tooltip("Sound played when the sign-in screen appears.")]
    public AudioClip loginSound;
    [Range(0f, 1f)] public float loginSoundVolume = 0.8f;
    [Tooltip("How long the sign-in screen stays up before fading out.")]
    public float loginScreenDuration = 2.2f;
    [Tooltip("How long the fade-out to the mail client takes.")]
    public float loginFadeDuration = 0.6f;
    [Tooltip("Degrees per second the loading spinner rotates.")]
    public float spinnerSpeed = 220f;

    [Header("Theme Colors")]
    public Color colorTopBar = Color.white;
    public Color colorSidebar = new Color32(0xF3, 0xF2, 0xF1, 0xFF);
    public Color colorEmailListBg = Color.white;
    public Color colorSelectedItem = new Color32(0xD9, 0xE9, 0xF9, 0xFF);
    public Color colorHoverItem = new Color32(0xF3, 0xF2, 0xF1, 0xFF);
    public Color colorDivider = new Color32(0xE1, 0xE1, 0xE1, 0xFF);
    public Color colorTextPrimary = new Color32(0x20, 0x20, 0x20, 0xFF);
    public Color colorTextSecondary = new Color32(0x60, 0x60, 0x60, 0xFF);
    public Color colorAccent = new Color32(0x00, 0x78, 0xD4, 0xFF);

    private Transform emailListContent;
    private TextMeshProUGUI readingSubjectText;
    private TextMeshProUGUI readingSenderText;
    private TextMeshProUGUI readingTimestampText;
    private TextMeshProUGUI readingBodyText;
    private readonly List<Image> emailItemBackgrounds = new List<Image>();
    private readonly List<TextMeshProUGUI> emailItemSenderTexts = new List<TextMeshProUGUI>();
    private readonly List<TextMeshProUGUI> emailItemSubjectTexts = new List<TextMeshProUGUI>();
    private int selectedIndex = -1;

    // --- Login sequence state ---
    private CanvasGroup loginOverlayCanvasGroup;
    private RectTransform spinnerRoot;
    private AudioSource audioSource;
    private bool loginSpinnerActive;

    private void Start()
    {
        UnlockCursor();

        if (emails.Count == 0)
        {
            PopulateSampleEmails();
        }

        BuildUI();

        if (emails.Count > 0)
        {
            SelectEmail(0);
        }

        StartCoroutine(PlayLoginSequence());
    }

    private void Update()
    {
        if (loginSpinnerActive && spinnerRoot != null)
        {
            spinnerRoot.Rotate(0f, 0f, -spinnerSpeed * Time.deltaTime);
        }
    }

    // Makes sure the OS cursor is visible and free to move (not locked to
    // the game view), so the player can actually click around the UI.
    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void PopulateSampleEmails()
    {
        emails.Add(new EmailData
        {
            senderName = "Detective Bureau",
            subject = "Case File #4471 - New Evidence",
            preview = "Forensics just sent over the toxicology report from the scene...",
            body = "Detective,\n\nForensics just sent over the toxicology report from the museum scene. The results are... unusual. Call me when you get this.\n\n- Harlow",
            timestamp = "8:41 AM",
            isUnread = true
        });
        emails.Add(new EmailData
        {
            senderName = "Museum Archives",
            subject = "Re: Access Request Approved",
            preview = "Your request to view the restricted east wing has been approved...",
            body = "Your request to view the restricted east wing has been approved. Please bring your credentials to the front desk.\n\nRegards,\nArchives Dept.",
            timestamp = "Yesterday",
            isUnread = false
        });
        emails.Add(new EmailData
        {
            senderName = "Unknown Sender",
            subject = "You shouldn't be looking into this",
            preview = "Some things are better left buried. Consider this a warning...",
            body = "Some things are better left buried.\n\nConsider this a warning.",
            timestamp = "Yesterday",
            isUnread = true
        });
        emails.Add(new EmailData
        {
            senderName = "IT Support",
            subject = "Password Expiration Notice",
            preview = "Your department password will expire in 3 days...",
            body = "Your department password will expire in 3 days. Please update it through the employee portal.",
            timestamp = "Mon",
            isUnread = false
        });
        emails.Add(new EmailData
        {
            senderName = "Captain Reyes",
            subject = "Weekly Briefing Notes",
            preview = "Attached are the notes from this week's briefing...",
            body = "Attached are the notes from this week's briefing. Let me know if you have questions before Friday.",
            timestamp = "Mon",
            isUnread = false
        });
    }

    // Adds a new email at runtime and appends it to the list UI.
    public void AddEmail(EmailData newEmail)
    {
        emails.Add(newEmail);
        CreateEmailListItem(newEmail, emails.Count - 1);
    }

    private void BuildUI()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        GameObject canvasGO = new GameObject("EmailClientCanvas", typeof(RectTransform));
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

        GameObject rootLayout = CreatePanel("RootLayout", canvasGO.transform, Color.white);
        RectTransform rootRT = rootLayout.GetComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = Vector2.zero;
        rootRT.offsetMax = Vector2.zero;
        VerticalLayoutGroup rootVLG = rootLayout.AddComponent<VerticalLayoutGroup>();
        rootVLG.childForceExpandWidth = true;
        rootVLG.childForceExpandHeight = false;
        rootVLG.childControlWidth = true;
        rootVLG.childControlHeight = true;
        rootVLG.spacing = 0;

        // --- Top bar ---
        GameObject topBar = CreatePanel("TopBar", rootLayout.transform, colorTopBar);
        AddLayoutElement(topBar, preferredHeight: 50, flexibleHeight: 0);
        HorizontalLayoutGroup topBarHLG = topBar.AddComponent<HorizontalLayoutGroup>();
        topBarHLG.childAlignment = TextAnchor.MiddleLeft;
        topBarHLG.padding = new RectOffset(16, 16, 8, 8);
        topBarHLG.spacing = 20;
        topBarHLG.childControlWidth = true;
        topBarHLG.childControlHeight = true;
        topBarHLG.childForceExpandWidth = false;
        topBarHLG.childForceExpandHeight = true;
        CreateText("AppTitleText", topBar.transform, "Mail", 24, colorTextPrimary, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);

        GameObject topDivider = CreatePanel("TopDivider", rootLayout.transform, colorDivider);
        AddLayoutElement(topDivider, preferredHeight: 1, flexibleHeight: 0);

        // --- Body row ---
        GameObject bodyRow = CreatePanel("BodyRow", rootLayout.transform, Color.white);
        AddLayoutElement(bodyRow, flexibleHeight: 1);
        HorizontalLayoutGroup bodyHLG = bodyRow.AddComponent<HorizontalLayoutGroup>();
        bodyHLG.childForceExpandWidth = false;
        bodyHLG.childForceExpandHeight = true;
        bodyHLG.childControlWidth = true;
        bodyHLG.childControlHeight = true;
        bodyHLG.spacing = 0;

        // --- Sidebar ---
        GameObject sidebar = CreatePanel("Sidebar", bodyRow.transform, colorSidebar);
        AddLayoutElement(sidebar, preferredWidth: 240, flexibleWidth: 0);
        VerticalLayoutGroup sideVLG = sidebar.AddComponent<VerticalLayoutGroup>();
        sideVLG.padding = new RectOffset(14, 14, 18, 18);
        sideVLG.spacing = 6;
        sideVLG.childForceExpandWidth = true;
        sideVLG.childForceExpandHeight = false;
        sideVLG.childControlWidth = true;
        sideVLG.childControlHeight = true;

        var foldersHeader = CreateText("FoldersHeader", sidebar.transform, "FOLDERS", 14, colorTextSecondary, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        AddLayoutElement(foldersHeader.gameObject, preferredHeight: 28, flexibleHeight: 0);

        foreach (string folder in folderNames)
        {
            GameObject folderItem = CreatePanel(folder + "_Item", sidebar.transform,
                folder == "Inbox" ? colorSelectedItem : new Color(0, 0, 0, 0));
            AddLayoutElement(folderItem, preferredHeight: 40, flexibleHeight: 0);

            var txt = CreateText(folder + "_Text", folderItem.transform, folder, 17, colorTextPrimary, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            RectTransform txtRT = txt.GetComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = new Vector2(10, 0);
            txtRT.offsetMax = Vector2.zero;
            txt.alignment = TextAlignmentOptions.MidlineLeft;
        }

        GameObject sideDivider = CreatePanel("SideDivider", bodyRow.transform, colorDivider);
        AddLayoutElement(sideDivider, preferredWidth: 1, flexibleWidth: 0);

        // --- Email list panel ---
        GameObject emailListPanel = CreatePanel("EmailListPanel", bodyRow.transform, colorEmailListBg);
        AddLayoutElement(emailListPanel, preferredWidth: 440, flexibleWidth: 0);
        VerticalLayoutGroup listVLG = emailListPanel.AddComponent<VerticalLayoutGroup>();
        listVLG.childForceExpandWidth = true;
        listVLG.childForceExpandHeight = false;
        listVLG.childControlWidth = true;
        listVLG.childControlHeight = true;
        listVLG.spacing = 0;

        GameObject listHeader = CreatePanel("ListHeader", emailListPanel.transform, colorEmailListBg);
        AddLayoutElement(listHeader, preferredHeight: 44, flexibleHeight: 0);
        HorizontalLayoutGroup listHeaderHLG = listHeader.AddComponent<HorizontalLayoutGroup>();
        listHeaderHLG.padding = new RectOffset(18, 18, 6, 6);
        listHeaderHLG.spacing = 20;
        listHeaderHLG.childControlWidth = true;
        listHeaderHLG.childControlHeight = true;
        listHeaderHLG.childForceExpandWidth = false;
        listHeaderHLG.childForceExpandHeight = true;
        CreateText("AllTab", listHeader.transform, "All", 16, colorTextPrimary, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        CreateText("UnreadTab", listHeader.transform, "Unread", 16, colorTextSecondary, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);

        GameObject listHeaderDivider = CreatePanel("ListHeaderDivider", emailListPanel.transform, colorDivider);
        AddLayoutElement(listHeaderDivider, preferredHeight: 1, flexibleHeight: 0);

        GameObject scrollGO = CreatePanel("EmailScrollView", emailListPanel.transform, colorEmailListBg);
        AddLayoutElement(scrollGO, flexibleHeight: 1);
        ScrollRect scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 20;

        GameObject viewport = CreatePanel("Viewport", scrollGO.transform, colorEmailListBg);
        RectTransform viewportRT = viewport.GetComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.offsetMin = Vector2.zero;
        viewportRT.offsetMax = Vector2.zero;
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        scrollRect.viewport = viewportRT;

        GameObject content = CreatePanel("Content", viewport.transform, colorEmailListBg);
        RectTransform contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1f);
        contentRT.sizeDelta = new Vector2(0, contentRT.sizeDelta.y);
        contentRT.anchoredPosition = Vector2.zero;
        VerticalLayoutGroup contentVLG = content.AddComponent<VerticalLayoutGroup>();
        contentVLG.childForceExpandWidth = true;
        contentVLG.childForceExpandHeight = false;
        contentVLG.childControlWidth = true;
        contentVLG.childControlHeight = true;
        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = contentRT;

        emailListContent = content.transform;

        for (int i = 0; i < emails.Count; i++)
        {
            CreateEmailListItem(emails[i], i);
        }

        GameObject readingDivider = CreatePanel("ReadingDivider", bodyRow.transform, colorDivider);
        AddLayoutElement(readingDivider, preferredWidth: 1, flexibleWidth: 0);

        // --- Reading pane ---
        GameObject readingPane = CreatePanel("ReadingPane", bodyRow.transform, colorEmailListBg);
        AddLayoutElement(readingPane, flexibleWidth: 1);
        VerticalLayoutGroup readingVLG = readingPane.AddComponent<VerticalLayoutGroup>();
        readingVLG.padding = new RectOffset(24, 24, 20, 20);
        readingVLG.spacing = 12;
        readingVLG.childForceExpandWidth = true;
        readingVLG.childForceExpandHeight = false;
        readingVLG.childControlWidth = true;
        readingVLG.childControlHeight = true;

        readingSubjectText = CreateText("SubjectText", readingPane.transform, "", 28, colorTextPrimary, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        AddLayoutElement(readingSubjectText.gameObject, preferredHeight: 40, flexibleHeight: 0);

        GameObject senderRow = CreatePanel("SenderRow", readingPane.transform, new Color(0, 0, 0, 0));
        AddLayoutElement(senderRow, preferredHeight: 28, flexibleHeight: 0);
        HorizontalLayoutGroup senderHLG = senderRow.AddComponent<HorizontalLayoutGroup>();
        senderHLG.spacing = 10;
        senderHLG.childControlWidth = true;
        senderHLG.childControlHeight = true;
        senderHLG.childForceExpandWidth = false;
        senderHLG.childForceExpandHeight = true;
        readingSenderText = CreateText("SenderText", senderRow.transform, "", 17, colorTextSecondary, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        AddLayoutElement(readingSenderText.gameObject, flexibleWidth: 1);
        readingTimestampText = CreateText("TimestampText", senderRow.transform, "", 14, colorTextSecondary, TextAlignmentOptions.MidlineRight, FontStyles.Bold);

        GameObject readingHeaderDivider = CreatePanel("ReadingHeaderDivider", readingPane.transform, colorDivider);
        AddLayoutElement(readingHeaderDivider, preferredHeight: 1, flexibleHeight: 0);

        GameObject bodyScrollGO = CreatePanel("BodyScrollView", readingPane.transform, colorEmailListBg);
        AddLayoutElement(bodyScrollGO, flexibleHeight: 1);
        ScrollRect bodyScrollRect = bodyScrollGO.AddComponent<ScrollRect>();
        bodyScrollRect.horizontal = false;
        bodyScrollRect.vertical = true;

        GameObject bodyViewport = CreatePanel("BodyViewport", bodyScrollGO.transform, colorEmailListBg);
        RectTransform bodyViewportRT = bodyViewport.GetComponent<RectTransform>();
        bodyViewportRT.anchorMin = Vector2.zero;
        bodyViewportRT.anchorMax = Vector2.one;
        bodyViewportRT.offsetMin = Vector2.zero;
        bodyViewportRT.offsetMax = Vector2.zero;
        bodyViewport.AddComponent<Mask>().showMaskGraphic = false;
        bodyScrollRect.viewport = bodyViewportRT;

        GameObject bodyContent = CreatePanel("BodyContent", bodyViewport.transform, colorEmailListBg);
        RectTransform bodyContentRT = bodyContent.GetComponent<RectTransform>();
        bodyContentRT.anchorMin = new Vector2(0, 1);
        bodyContentRT.anchorMax = new Vector2(1, 1);
        bodyContentRT.pivot = new Vector2(0.5f, 1f);
        bodyContentRT.sizeDelta = new Vector2(0, bodyContentRT.sizeDelta.y);
        bodyContentRT.anchoredPosition = Vector2.zero;
        VerticalLayoutGroup bodyContentVLG = bodyContent.AddComponent<VerticalLayoutGroup>();
        bodyContentVLG.childControlWidth = true;
        bodyContentVLG.childControlHeight = true;
        bodyContentVLG.childForceExpandWidth = true;
        ContentSizeFitter bodyFitter = bodyContent.AddComponent<ContentSizeFitter>();
        bodyFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        bodyScrollRect.content = bodyContentRT;

        readingBodyText = CreateText("BodyText", bodyContent.transform, "", 18, colorTextPrimary, TextAlignmentOptions.TopLeft, FontStyles.Bold);

        // --- Bottom bar with return-to-game button ---
        GameObject bottomDivider = CreatePanel("BottomDivider", rootLayout.transform, colorDivider);
        AddLayoutElement(bottomDivider, preferredHeight: 1, flexibleHeight: 0);

        GameObject bottomBar = CreatePanel("BottomBar", rootLayout.transform, colorTopBar);
        AddLayoutElement(bottomBar, preferredHeight: 64, flexibleHeight: 0);
        HorizontalLayoutGroup bottomHLG = bottomBar.AddComponent<HorizontalLayoutGroup>();
        bottomHLG.childAlignment = TextAnchor.MiddleRight;
        bottomHLG.padding = new RectOffset(24, 24, 12, 12);
        bottomHLG.childControlWidth = true;
        bottomHLG.childControlHeight = true;
        bottomHLG.childForceExpandWidth = false;
        bottomHLG.childForceExpandHeight = true;

        CreateQuitButton(bottomBar.transform);

        // --- Login / "signing in" overlay (built last so it renders on top) ---
        BuildLoginOverlay(canvasGO.transform);
    }

    private void CreateQuitButton(Transform parent)
    {
        GameObject btnGO = CreatePanel("QuitButton", parent, colorAccent);
        AddLayoutElement(btnGO, preferredWidth: 220, preferredHeight: 40, flexibleWidth: 0, flexibleHeight: 0);

        Button btn = btnGO.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = colorAccent;
        cb.highlightedColor = new Color(colorAccent.r * 0.85f, colorAccent.g * 0.85f, colorAccent.b * 0.85f, 1f);
        cb.pressedColor = new Color(colorAccent.r * 0.65f, colorAccent.g * 0.65f, colorAccent.b * 0.65f, 1f);
        btn.colors = cb;
        btn.onClick.AddListener(QuitToMainGame);

        var txt = CreateText("QuitButtonText", btnGO.transform, "Log Off", 16, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
        RectTransform txtRT = txt.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;
    }

    // Called by the bottom bar button. Public so you can also hook it up to a UnityEvent elsewhere.
    public void QuitToMainGame()
    {
        if (!string.IsNullOrEmpty(mainGameSceneName))
        {
            SceneManager.LoadScene(mainGameSceneName);
        }
        else
        {
            Debug.LogWarning("OutlookStyleEmailUI: mainGameSceneName is empty - can't return to the main game scene.");
        }
    }

    private void CreateEmailListItem(EmailData data, int index)
    {
        GameObject item = CreatePanel($"EmailItem_{index}", emailListContent, Color.white);
        AddLayoutElement(item, preferredHeight: 92, flexibleHeight: 0);
        Image bg = item.GetComponent<Image>();
        emailItemBackgrounds.Add(bg);

        Button btn = item.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = colorHoverItem;
        cb.pressedColor = colorSelectedItem;
        btn.colors = cb;
        int capturedIndex = index;
        btn.onClick.AddListener(() => SelectEmail(capturedIndex));

        VerticalLayoutGroup itemVLG = item.AddComponent<VerticalLayoutGroup>();
        itemVLG.padding = new RectOffset(20, 20, 12, 12);
        itemVLG.spacing = 4;
        itemVLG.childForceExpandWidth = true;
        itemVLG.childControlWidth = true;
        itemVLG.childControlHeight = true;

        GameObject topRow = CreatePanel("TopRow", item.transform, new Color(0, 0, 0, 0));
        AddLayoutElement(topRow, preferredHeight: 24, flexibleHeight: 0);
        HorizontalLayoutGroup topRowHLG = topRow.AddComponent<HorizontalLayoutGroup>();
        topRowHLG.childControlWidth = true;
        topRowHLG.childControlHeight = true;
        topRowHLG.childForceExpandWidth = false;
        topRowHLG.childForceExpandHeight = true;

        var senderTxt = CreateText("SenderText", topRow.transform, data.senderName, 17, colorTextPrimary, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        AddLayoutElement(senderTxt.gameObject, flexibleWidth: 1);
        CreateText("TimeText", topRow.transform, data.timestamp, 13, colorTextSecondary, TextAlignmentOptions.MidlineRight, FontStyles.Bold);

        var subjectTxt = CreateText("SubjectText", item.transform, data.subject, 16, colorTextPrimary, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        AddLayoutElement(subjectTxt.gameObject, preferredHeight: 20, flexibleHeight: 0);

        var previewTxt = CreateText("PreviewText", item.transform, data.preview, 14, colorTextSecondary, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        AddLayoutElement(previewTxt.gameObject, preferredHeight: 20, flexibleHeight: 0);
        previewTxt.enableWordWrapping = false;
        previewTxt.overflowMode = TextOverflowModes.Ellipsis;

        emailItemSenderTexts.Add(senderTxt);
        emailItemSubjectTexts.Add(subjectTxt);
    }

    // Shows the given email in the reading pane and marks it as read.
    public void SelectEmail(int index)
    {
        if (index < 0 || index >= emails.Count) return;

        selectedIndex = index;

        for (int i = 0; i < emailItemBackgrounds.Count; i++)
        {
            emailItemBackgrounds[i].color = (i == index) ? colorSelectedItem : Color.white;
        }

        EmailData data = emails[index];
        data.isUnread = false;

        readingSubjectText.text = data.subject;
        readingSenderText.text = data.senderName;
        readingTimestampText.text = data.timestamp;
        readingBodyText.text = data.body;
    }

    // --- Login / sign-in overlay ---

    // Builds a full-screen "Signing in..." panel (avatar circle, welcome text,
    // and a rotating dot spinner) that sits above the mail UI on a separate
    // top-level child of the canvas so it can fade out independently.
    private void BuildLoginOverlay(Transform canvasParent)
    {
        GameObject overlay = new GameObject("LoginOverlay", typeof(RectTransform));
        overlay.transform.SetParent(canvasParent, false);
        RectTransform overlayRT = overlay.GetComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.offsetMin = Vector2.zero;
        overlayRT.offsetMax = Vector2.zero;

        Image bg = overlay.AddComponent<Image>();
        bg.color = colorAccent;

        loginOverlayCanvasGroup = overlay.AddComponent<CanvasGroup>();
        loginOverlayCanvasGroup.alpha = 1f;
        loginOverlayCanvasGroup.blocksRaycasts = true;
        loginOverlayCanvasGroup.interactable = false;

        GameObject center = new GameObject("Center", typeof(RectTransform));
        center.transform.SetParent(overlay.transform, false);
        RectTransform centerRT = center.GetComponent<RectTransform>();
        centerRT.anchorMin = new Vector2(0.5f, 0.5f);
        centerRT.anchorMax = new Vector2(0.5f, 0.5f);
        centerRT.pivot = new Vector2(0.5f, 0.5f);
        centerRT.sizeDelta = new Vector2(420, 260);
        centerRT.anchoredPosition = Vector2.zero;
        VerticalLayoutGroup centerVLG = center.AddComponent<VerticalLayoutGroup>();
        centerVLG.childAlignment = TextAnchor.MiddleCenter;
        centerVLG.spacing = 16;
        centerVLG.childForceExpandWidth = false;
        centerVLG.childForceExpandHeight = false;
        centerVLG.childControlWidth = false;
        centerVLG.childControlHeight = false;

        // Avatar circle with initials
        GameObject avatarGO = new GameObject("Avatar", typeof(RectTransform));
        avatarGO.transform.SetParent(center.transform, false);
        RectTransform avatarRT = avatarGO.GetComponent<RectTransform>();
        avatarRT.sizeDelta = new Vector2(96, 96);
        Image avatarImg = avatarGO.AddComponent<Image>();
        avatarImg.sprite = CreateCircleSprite(128, Color.white);
        avatarImg.color = new Color(1f, 1f, 1f, 0.95f);

        var initials = CreateText("AvatarInitials", avatarGO.transform, GetInitials(userDisplayName), 32, colorAccent, TextAlignmentOptions.Center, FontStyles.Bold);
        RectTransform initRT = initials.GetComponent<RectTransform>();
        initRT.anchorMin = Vector2.zero;
        initRT.anchorMax = Vector2.one;
        initRT.offsetMin = Vector2.zero;
        initRT.offsetMax = Vector2.zero;

        var welcomeText = CreateText("WelcomeText", center.transform, "Welcome, " + userDisplayName, 24, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
        welcomeText.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 34);

        var signingInText = CreateText("SigningInText", center.transform, "Signing in", 15, new Color(1f, 1f, 1f, 0.85f), TextAlignmentOptions.Center, FontStyles.Normal);
        signingInText.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 22);

        // Rotating dot spinner
        GameObject spinnerGO = new GameObject("Spinner", typeof(RectTransform));
        spinnerGO.transform.SetParent(center.transform, false);
        spinnerRoot = spinnerGO.GetComponent<RectTransform>();
        spinnerRoot.sizeDelta = new Vector2(46, 46);

        int dotCount = 8;
        float radius = 18f;
        Sprite dotSprite = CreateCircleSprite(20, Color.white);
        for (int i = 0; i < dotCount; i++)
        {
            GameObject dotGO = new GameObject("Dot" + i, typeof(RectTransform));
            dotGO.transform.SetParent(spinnerRoot, false);
            RectTransform dotRT = dotGO.GetComponent<RectTransform>();
            float angle = (i / (float)dotCount) * Mathf.PI * 2f;
            dotRT.anchoredPosition = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            dotRT.sizeDelta = new Vector2(7, 7);
            Image dotImg = dotGO.AddComponent<Image>();
            dotImg.sprite = dotSprite;
            float alpha = 0.2f + 0.8f * (i / (float)(dotCount - 1)); // trailing fade, brightest at the "head"
            dotImg.color = new Color(1f, 1f, 1f, alpha);
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    // Plays the login chime, holds the sign-in screen for a moment while the
    // spinner turns, then fades it out to reveal the mail client underneath.
    private IEnumerator PlayLoginSequence()
    {
        loginSpinnerActive = true;

        if (loginOverlayCanvasGroup != null)
        {
            loginOverlayCanvasGroup.alpha = 1f;
            loginOverlayCanvasGroup.blocksRaycasts = true;
        }

        if (audioSource != null && loginSound != null)
        {
            audioSource.PlayOneShot(loginSound, loginSoundVolume);
        }

        float elapsed = 0f;
        while (elapsed < loginScreenDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        float fadeElapsed = 0f;
        while (fadeElapsed < loginFadeDuration)
        {
            fadeElapsed += Time.deltaTime;
            if (loginOverlayCanvasGroup != null)
            {
                loginOverlayCanvasGroup.alpha = 1f - Mathf.Clamp01(fadeElapsed / loginFadeDuration);
            }
            yield return null;
        }

        loginSpinnerActive = false;

        if (loginOverlayCanvasGroup != null)
        {
            loginOverlayCanvasGroup.alpha = 0f;
            loginOverlayCanvasGroup.blocksRaycasts = false;
            loginOverlayCanvasGroup.gameObject.SetActive(false);
        }
    }

    private string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        string[] parts = name.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "?";
        if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
        return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
    }

    // Procedurally draws a solid circle into a texture and wraps it as a
    // Sprite, so avatar/spinner dots don't need any imported art assets.
    private Sprite CreateCircleSprite(int diameter, Color color)
    {
        Texture2D tex = new Texture2D(diameter, diameter, TextureFormat.ARGB32, false);
        tex.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2(diameter / 2f, diameter / 2f);
        float radius = diameter / 2f;

        for (int y = 0; y < diameter; y++)
        {
            for (int x = 0; x < diameter; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                if (dist <= radius)
                {
                    tex.SetPixel(x, y, color);
                }
                else
                {
                    tex.SetPixel(x, y, new Color(color.r, color.g, color.b, 0f));
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, diameter, diameter), new Vector2(0.5f, 0.5f));
    }

    // --- UI building helpers ---

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