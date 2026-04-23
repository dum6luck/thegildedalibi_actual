using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CutsceneController : MonoBehaviour
{
    public static CutsceneController Instance;

    [Header("UI References")]
    public Image fadeImage;
    public float fadeDuration = 2.0f;

    void Awake()
    {
        // This allows the InvitationCard script to find this one easily
        Instance = this;
    }

    // This is the specific method the error is complaining about
    public void StartFadeOut()
    {
        if (fadeImage != null)
        {
            StartCoroutine(FadeToBlack());
        }
        else
        {
            Debug.LogError("Fade Image is not assigned on the CutsceneController!");
        }
    }

    IEnumerator FadeToBlack()
    {
        float elapsed = 0;
        Color tempColor = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            tempColor.a = Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = tempColor;
            yield return null;
        }

        Debug.Log("Sequence Complete.");
    }
}