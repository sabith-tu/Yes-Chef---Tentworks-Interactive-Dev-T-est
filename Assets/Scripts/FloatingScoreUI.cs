using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingScoreUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public float floatSpeed = 1.5f;
    public float lifetime = 2.2f;

    private CanvasGroup canvasGroup;

    public void Setup(int score)
    {
        if (scoreText == null)
        {
            Debug.LogError("coreText not assigned.", this);
            Destroy(gameObject, lifetime);
            return;
        }

        scoreText.text = score >= 0 ? $"+{score}" : score.ToString();
        scoreText.color = score >= 0 ? new Color(0.2f, 1f, 0.3f) : new Color(1f, 0.25f, 0.25f);

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float popDuration = 0.12f;
        Vector3 origin = transform.localScale;
        transform.localScale = Vector3.zero;

        for (float t = 0f; t < popDuration; t += Time.deltaTime)
        {
            float progress = Mathf.SmoothStep(0f, 1f, t / popDuration);
            transform.localScale = Vector3.LerpUnclamped(Vector3.zero, origin * 1.25f, progress);
            yield return null;
        }

        for (float t = 0f; t < 0.06f; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(origin * 1.25f, origin, t / 0.06f);
            yield return null;
        }
        transform.localScale = origin;

        float fadeStart = lifetime * 0.5f;
        float elapsed = 0f;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;

            if (elapsed > fadeStart)
            {
                float alpha = 1f - ((elapsed - fadeStart) / (lifetime - fadeStart));
                canvasGroup.alpha = Mathf.Clamp01(alpha);
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
