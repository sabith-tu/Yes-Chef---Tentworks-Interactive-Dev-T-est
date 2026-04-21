using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FridgeSelectionUI : MonoBehaviour
{
    public Transform buttonContainer;

    public GameObject buttonTemplate;

    public TextMeshProUGUI titleText;

    public float fadeDuration = 0.12f;

    private CanvasGroup canvasGroup;
    private Action<IngredientData> onSelected;
    private Coroutine fadeCoroutine;

    private readonly List<(Button btn, IngredientData ingredient)> buttons =
        new List<(Button, IngredientData)>();

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        SetVisibility(0f, false);
    }

    public void Initialise(IngredientData[] ingredients)
    {
        if (buttonTemplate == null)
        {
            Debug.LogError("[FridgeSelectionUI] buttonTemplate not assigned.", this);
            return;
        }

        foreach (var entry in buttons)
            if (entry.btn != null)
                Destroy(entry.btn.gameObject);
        buttons.Clear();

        if (titleText != null)
            titleText.text = "Choose Ingredient";

        foreach (IngredientData ingredient in ingredients)
        {
            if (ingredient == null)
                continue;

            GameObject btnGO = Instantiate(buttonTemplate, buttonContainer);
            btnGO.SetActive(true);

            TextMeshProUGUI label = btnGO.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                string hint =
                    ingredient.requiredPrepType != null
                        ? $"\n<size=70%><color=#bbbbbb>needs {ingredient.requiredPrepType.name}</color></size>"
                        : "\n<size=70%><color=#bbbbbb>serve immediately</color></size>";
                label.text = $"<b>{ingredient.ingredientName}</b>{hint}";
            }

            Button btn = btnGO.GetComponent<Button>();
            buttons.Add((btn, ingredient));
        }
    }

    public void Show(Action<IngredientData> callback)
    {
        onSelected = callback;

        foreach (var (btn, ingredient) in buttons)
        {
            if (btn == null)
                continue;
            btn.onClick.RemoveAllListeners();
            IngredientData captured = ingredient;
            btn.onClick.AddListener(() => OnButtonClicked(captured));
        }

        Fade(1f);
    }

    public void Hide()
    {
        Fade(0f);

        foreach (var (btn, _) in buttons)
            btn?.onClick.RemoveAllListeners();

        onSelected = null;
    }

    private void OnButtonClicked(IngredientData ingredient)
    {
        onSelected?.Invoke(ingredient);
    }

    private void Fade(float targetAlpha)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(DoFade(targetAlpha));
    }

    private IEnumerator DoFade(float target)
    {
        float start = canvasGroup.alpha;
        float elapsed = 0f;

        if (target > 0f)
            SetInteractable(true);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = target;

        SetInteractable(target > 0f);
        fadeCoroutine = null;
    }

    private void SetInteractable(bool on)
    {
        canvasGroup.interactable = on;
        canvasGroup.blocksRaycasts = on;
    }

    private void SetVisibility(float alpha, bool interactable)
    {
        canvasGroup.alpha = alpha;
        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = interactable;
    }
}
