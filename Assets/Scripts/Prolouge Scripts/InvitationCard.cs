using UnityEngine;
using System.Collections;

public class InvitationCard : MonoBehaviour
{
    [Header("Movement Settings")]
    public Vector3 slideTargetPos = new Vector3(0, 0, 5);
    public float slideSpeed = 2f;

    [Header("Rotation Settings")]
    [Tooltip("The rotation when the card first appears.")]
    public Vector3 startRotationEuler = new Vector3(90, 0, 0);

    [Tooltip("The rotation after the player clicks the card.")]
    public Vector3 endRotationEuler = new Vector3(44.876f, -6.626f, 0.913f);

    public float flipDuration = 1.0f;

    private bool isIntroFinished = false;
    private bool isFlipped = false;

    void Start()
    {
        // 1. Set the initial rotation immediately
        transform.rotation = Quaternion.Euler(startRotationEuler);

        // 2. Start the card below the screen
        transform.position = new Vector3(slideTargetPos.x, slideTargetPos.y - 10f, slideTargetPos.z);

        StartCoroutine(SlideIn());
    }

    IEnumerator SlideIn()
    {
        while (Vector3.Distance(transform.position, slideTargetPos) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, slideTargetPos, Time.deltaTime * slideSpeed);
            yield return null;
        }
        transform.position = slideTargetPos;
        isIntroFinished = true;
    }

    void OnMouseDown()
    {
        if (isIntroFinished && !isFlipped)
        {
            StartCoroutine(FlipToSpecificAngle());
        }
    }

    IEnumerator FlipToSpecificAngle()
    {
        isFlipped = true;

        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(endRotationEuler);

        float elapsed = 0;
        while (elapsed < flipDuration)
        {
            elapsed += Time.deltaTime;
            // Smoothly transition between the two rotations
            transform.rotation = Quaternion.Slerp(startRot, endRot, elapsed / flipDuration);
            yield return null;
        }
        transform.rotation = endRot;

        yield return new WaitForSeconds(2f);

        if (CutsceneController.Instance != null)
        {
            CutsceneController.Instance.StartFadeOut();
        }
    }
}