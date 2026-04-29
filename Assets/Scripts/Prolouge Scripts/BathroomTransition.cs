using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BathroomTransition : MonoBehaviour
{
    [Header("Settings")]
    public string sceneToLoad = "BathroomScene";
    public string requiredTag = "Player";
    public float waitBeforeLoad = 1.0f; // Matches your fade duration

    [Header("Condition")]
    public MingleTracker mingleTracker;

    [Header("UI References")]
    [Tooltip("Drag the UIFader that handles your full-screen black fade here")]
    public UIFader screenFader;

    public void Interact()
    {
        if (mingleTracker != null && mingleTracker.HasFinishedMingle())
        {
            StartCoroutine(FadeAndLoad());
        }
        else
        {
            Debug.Log("I can't leave yet, I haven't talked to everyone.");
            // Optional: You could trigger a quick 'I'm not done yet' inner monologue here
        }
    }

    IEnumerator FadeAndLoad()
    {
        Debug.Log("Starting fade to " + sceneToLoad);

        // 1. Trigger the screen fade
        if (screenFader != null)
        {
            screenFader.FadeOut();
        }

        // 2. Wait for the fade duration so the player doesn't see the scene 'pop'
        yield return new WaitForSeconds(waitBeforeLoad);

        // 3. Change the scene
        SceneManager.LoadScene(sceneToLoad);
    }
}