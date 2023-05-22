using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class PanelComponentFade : MonoBehaviour
{
    public enum FadeType
    {
        FadeOut,
        FadeIn
    }

    public FadeType fadeType = FadeType.FadeOut;
    public float duration;
    public bool onSceneLoad = false;
    public UnityEvent OnFadeFinished;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
    {
        if (onSceneLoad)
        {
            fadeType = FadeType.FadeOut;

            StartCoroutine(DoFade(canvasGroup.alpha, (int)fadeType));
        }
    }

    public void Fade(int currentFadeType)
    {
        fadeType = (FadeType)currentFadeType;

        StartCoroutine(DoFade(canvasGroup.alpha, (int)fadeType));
    }

    [ContextMenu("FadeInOut")]
    public void FadeOutAndIn()
    {
        StartCoroutine(DoSpecialFade(canvasGroup.alpha, 1));
    }

    IEnumerator DoFade(float start, float end)
    {
        float counter = 0f;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, counter / duration);

            yield return null;
        }

        OnFadeFinished?.Invoke();
        OnFadeFinished?.RemoveAllListeners();
    }

    IEnumerator DoSpecialFade(float start, float end)
    {
        float counter = 0f;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, counter / duration);

            yield return null;
        }

        StartCoroutine(WaitDuringFade());
    }

    IEnumerator WaitDuringFade()
    {
        yield return new WaitForSeconds(duration / 2);

        StartCoroutine(DoFade(canvasGroup.alpha, 0));
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
