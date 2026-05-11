using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Builds its own Canvas at runtime and subscribes to InspectableClue.OnAnyClueDiscovered.
/// Shows the clue text in a panel at the bottom of the screen.
/// Hides after DisplayDuration seconds or when the player presses any key.
/// </summary>
public class ClueUI : MonoBehaviour
{
    [Tooltip("How many seconds the clue panel stays visible.")]
    public float DisplayDuration = 5f;

    private GameObject panel;
    private TextMeshProUGUI clueLabel;
    private TextMeshProUGUI instructionLabel;

    private bool isShowing;
    private float hideAt;

    private void Awake()
    {
        BuildCanvas();
    }

    private void OnEnable()
    {
        InspectableClue.OnAnyClueDiscovered += ShowClue;
    }

    private void OnDisable()
    {
        InspectableClue.OnAnyClueDiscovered -= ShowClue;
    }

    private void Update()
    {
        if (!isShowing) return;

        if (Time.time >= hideAt || Input.anyKeyDown)
        {
            HidePanel();
        }
    }

    /// <summary>Displays the clue text panel.</summary>
    private void ShowClue(string text)
    {
        clueLabel.text = text;
        panel.SetActive(true);
        isShowing = true;
        hideAt = Time.time + DisplayDuration;
    }

    private void HidePanel()
    {
        panel.SetActive(false);
        isShowing = false;
    }

    /// <summary>Creates a Canvas, panel, and two TMP labels entirely at runtime.</summary>
    private void BuildCanvas()
    {
        // Canvas
        GameObject canvasGO = new GameObject("ClueCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Dark semi-transparent panel anchored to the bottom third of the screen
        panel = new GameObject("CluePanel");
        panel.transform.SetParent(canvasGO.transform, false);
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.05f, 0.85f);
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.15f, 0.06f);
        panelRT.anchorMax = new Vector2(0.85f, 0.28f);
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        // Clue text
        GameObject textGO = new GameObject("ClueText");
        textGO.transform.SetParent(panel.transform, false);
        clueLabel = textGO.AddComponent<TextMeshProUGUI>();
        clueLabel.fontSize = 28;
        clueLabel.fontStyle = FontStyles.Italic;
        clueLabel.color = new Color(1f, 0.9f, 0.6f);
        clueLabel.alignment = TextAlignmentOptions.Center;
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0f, 0.3f);
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(20f, 0f);
        textRT.offsetMax = new Vector2(-20f, -10f);

        // Small instruction label below the clue text
        GameObject instrGO = new GameObject("InstructionText");
        instrGO.transform.SetParent(panel.transform, false);
        instructionLabel = instrGO.AddComponent<TextMeshProUGUI>();
        instructionLabel.text = "Press any key to dismiss";
        instructionLabel.fontSize = 18;
        instructionLabel.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        instructionLabel.alignment = TextAlignmentOptions.Center;
        RectTransform instrRT = instrGO.GetComponent<RectTransform>();
        instrRT.anchorMin = Vector2.zero;
        instrRT.anchorMax = new Vector2(1f, 0.3f);
        instrRT.offsetMin = new Vector2(20f, 5f);
        instrRT.offsetMax = new Vector2(-20f, 0f);

        panel.SetActive(false);
    }
}
