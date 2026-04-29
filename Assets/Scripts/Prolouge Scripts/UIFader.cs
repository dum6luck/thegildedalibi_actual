using UnityEngine;
using System.Collections;

public class UIFader : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    public float fadeSpeed = 5f;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void FadeIn()
    {
        StopAllCoroutines();
        StartCoroutine(DoFade(1f));
    }

    public void FadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(DoFade(0f));
    }

    IEnumerator DoFade(float targetAlpha)
    {
        while (!Mathf.Approximately(canvasGroup.alpha, targetAlpha))
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            yield return null;
        }
    }
}