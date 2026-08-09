using UnityEngine;

public class QuitGameOnEsc : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private KeyCode quitKey = KeyCode.Escape;

    private void Update()
    {
        if (Input.GetKeyDown(quitKey))
        {
            QuitGame();
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");

#if UNITY_EDITOR
        // Stops play mode in the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // Closes the application in a built executable
            Application.Quit();
#endif
    }
}