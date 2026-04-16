using UnityEngine;
using TMPro;

/// <summary>
/// One rotary dial on the combination lock box.
/// The player looks at it and presses F to cycle the digit up.
/// The current digit is shown on a TextMeshPro label on the dial face.
/// </summary>
public class DialWheel : MonoBehaviour
{
    [Tooltip("Which digit position this dial controls (0 = leftmost).")]
    public int DialIndex = 0;

    [Tooltip("The TextMeshPro label rendered on the face of this dial.")]
    public TextMeshProUGUI DigitLabel;

    [Tooltip("Starting digit (0-9).")]
    [Range(0, 9)]
    public int StartDigit = 0;

    /// <summary>Current digit shown on this dial (0-9).</summary>
    public int CurrentDigit { get; private set; }

    private void Start()
    {
        CurrentDigit = StartDigit;
        RefreshLabel();
    }

    /// <summary>Advances the digit by one (wraps 9 → 0) and updates the label.</summary>
    public void CycleUp()
    {
        CurrentDigit = (CurrentDigit + 1) % 10;
        RefreshLabel();

        // Rotate the mesh so it feels physical
        transform.Rotate(Vector3.right, 36f, Space.Self);
    }

    private void RefreshLabel()
    {
        if (DigitLabel != null)
            DigitLabel.text = CurrentDigit.ToString();
    }
}
