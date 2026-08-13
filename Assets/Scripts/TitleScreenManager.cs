using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Type the EXACT name of the scene you want the Play button to load.")]
    [SerializeField] private string nextSceneName = "Main_Game";

    /// <summary>
    /// Call this method from your Play Button's OnClick() event.
    /// </summary>
    public void PlayGame()
    {
        Debug.Log("Loading next scene: " + nextSceneName);
        SceneManager.LoadScene(nextSceneName);
    }

    /// <summary>
    /// Call this method from a Quit Button's OnClick() event.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting application...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}