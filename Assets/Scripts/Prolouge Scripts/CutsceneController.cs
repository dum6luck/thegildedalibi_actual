using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Required for changing scenes
using System.Collections;

public class CutsceneController : MonoBehaviour
{
    public static CutsceneController Instance;

    [Header("UI References")]
    public Image fadeImage;
    public float fadeDuration = 2.0f;

    [Header("Scene Settings")]
    public string nextSceneName = "PrologueScene"; // Name of your 2nd scene
    public float waitTimeInBlack = 1.0f; // How long to stay in black before loading

    void Awake()
    {
        Instance = this;

        // Ensure the fade image is invisible at the start
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0;
            fadeImage.color = c;
        }
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
        float elapsed = 0;
        Color tempColor = fadeImage.color;

        // 1. Gradually Fade to Black
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            tempColor.a = Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = tempColor;
            yield return null;
        }

        // 2. Stay in black for a moment (Improves pacing)
        yield return new WaitForSeconds(waitTimeInBlack);

        // 3. Load the Second Scene
        Debug.Log("Loading Scene: " + nextSceneName);
        SceneManager.LoadScene(nextSceneName);
    }
}