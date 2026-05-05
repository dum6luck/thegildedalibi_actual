using UnityEngine;
using UnityEngine.SceneManagement;

public class CaseFileSceneToggle : MonoBehaviour
{
    [Header("Scene Names")]
    public string gameplaySceneName = "MainScene";
    public string caseFileSceneName = "Case_File_Scene";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleScene();
        }
    }

    void ToggleScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == caseFileSceneName)
        {
            // Go back to gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            SceneManager.LoadScene(gameplaySceneName);
        }
        else
        {
            // Go to case file
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            SceneManager.LoadScene(caseFileSceneName);
        }
    }
}