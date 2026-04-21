using System.Collections;
using TMPro;
using UnityEngine;

public class InteractPromptUI : MonoBehaviour
{
    public static InteractPromptUI Instance { get; private set; }

    public TextMeshProUGUI promptText;

    public float fadeDuration = 0.12f;

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;
    private bool isVisible;

    private void Awake()
    {
        Instance = this;
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        SetImmediate(0f);
    }

    public void Show(string message = "Press  <b>[E]</b>  to Interact")
    {
        if (promptText != null)
            promptText.text = message;
        if (!isVisible)
            Fade(1f);
    }

    public void Hide()
    {
        if (isVisible)
            Fade(0f);
    }

    private void Fade(float target)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(DoFade(target));
        isVisible = target > 0f;
    }

    private IEnumerator DoFade(float target)
    {
        float start = canvasGroup.alpha;
        float elapsed = 0f;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = target;
        fadeCoroutine = null;
    }

    private void SetImmediate(float alpha)
    {
        canvasGroup.alpha = alpha;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}
