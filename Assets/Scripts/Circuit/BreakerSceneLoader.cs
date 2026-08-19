using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BreakerSceneLoader : MonoBehaviour
{
    public static BreakerSceneLoader Instance;

    [Header("Return Target Scene")]
    [Tooltip("Exact name of your main gameplay scene in Build Settings.")]
    public string mainGameSceneName = "Main_Game";

    private void Awake()
    {
        Instance = this;
    }

    public void ReturnToMainGame(float delay = 2f)
    {
        StartCoroutine(LoadMainGameRoutine(delay));
    }

    private IEnumerator LoadMainGameRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(mainGameSceneName);
    }
}