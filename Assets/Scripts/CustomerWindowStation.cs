using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class CustomerWindowStation : MonoBehaviour, IInteractable
{
    public IngredientData[] allPossibleIngredients;

    public TextMeshProUGUI timerText;
    public TextMeshProUGUI ingredientsListText;
    public GameObject floatingScorePrefab;
    public Transform floatingScoreSpawnPoint;

    public Color colourNormal = Color.white;
    public Color colourUrgent = new Color(1f, 0.6f, 0f);
    public Color colourCritical = Color.red;
    public float urgentThreshold = 20f;
    public float criticalThreshold = 35f;

    private readonly List<IngredientData> currentOrder = new List<IngredientData>();
    private int orderBaseScore;
    private float orderActiveTime;
    private bool hasActiveOrder;

    private void Start()
    {
        GameManager.Instance.OnGameEnded += OnGameEnded;
        GameManager.Instance.OnGameStarted += OnGameStarted;
        StartCoroutine(SpawnOrderDelay(0f));
    }

    private void OnDestroy()
    {
        if (GameManager.Instance == null)
            return;
        GameManager.Instance.OnGameEnded -= OnGameEnded;
        GameManager.Instance.OnGameStarted -= OnGameStarted;
    }

    private void Update()
    {
        if (!hasActiveOrder)
            return;

        orderActiveTime += Time.deltaTime;

        if (timerText != null)
        {
            timerText.text = orderActiveTime.ToString("F1") + "s";
            timerText.color = GetUrgencyColour(orderActiveTime);
        }
    }

    public bool AutoInteract => false;

    public void OnEnterTrigger(PlayerInventory player) =>
        StationFeedback.HighlightStation(this, true);

    public void OnExitTrigger(PlayerInventory player) =>
        StationFeedback.HighlightStation(this, false);

    public void OnInteract(PlayerInventory player)
    {
        if (!hasActiveOrder || !player.HasItem)
            return;

        IngredientData held = player.currentItem;
        bool prepOk = held.requiredPrepType == null || player.isItemPrepared;

        if (!currentOrder.Contains(held) || !prepOk)
            return;

        currentOrder.Remove(held);
        Destroy(player.RemoveItem());

        UpdateIngredientsUI();

        if (currentOrder.Count == 0)
            CompleteOrder();
    }

    private void CompleteOrder()
    {
        hasActiveOrder = false;
        ClearTimerUI();

        int score = CalculateScore();
        GameManager.Instance.AddScore(score);
        SpawnFloatingScore(score);

        if (CameraShake.Instance != null)
        {
            if (score < 0)
                CameraShake.Instance.ShakeHeavy();
            else
                CameraShake.Instance.ShakeLight();
        }

        StartCoroutine(SpawnOrderDelay(5f));
    }

    private int CalculateScore()
    {
        return orderBaseScore - Mathf.FloorToInt(orderActiveTime);
    }

    private IEnumerator SpawnOrderDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
            yield break;
        GenerateOrder();
    }

    private void GenerateOrder()
    {
        if (allPossibleIngredients == null || allPossibleIngredients.Length == 0)
        {
            Debug.LogError($"{name}: no ingredients assigned.", this);
            return;
        }

        currentOrder.Clear();
        orderBaseScore = 0;
        orderActiveTime = 0f;

        int count = Random.value > 0.5f ? 2 : 3;
        for (int i = 0; i < count; i++)
        {
            IngredientData chosen = allPossibleIngredients[
                Random.Range(0, allPossibleIngredients.Length)
            ];
            currentOrder.Add(chosen);
            orderBaseScore += chosen.scoreValue;
        }

        hasActiveOrder = true;
        UpdateIngredientsUI();
    }

    private void UpdateIngredientsUI()
    {
        if (ingredientsListText == null)
            return;

        StringBuilder sb = new StringBuilder("<b>Order:</b>\n");
        foreach (IngredientData item in currentOrder)
        {
            string prepNote =
                item.requiredPrepType != null
                    ? $" <size=80%><color=#aaaaaa>({item.requiredPrepType.name})</color></size>"
                    : "";
            sb.AppendLine($"• {item.ingredientName}{prepNote}");
        }
        ingredientsListText.text = sb.ToString();
    }

    private void SpawnFloatingScore(int score)
    {
        if (floatingScorePrefab == null || floatingScoreSpawnPoint == null)
            return;
        GameObject obj = Instantiate(
            floatingScorePrefab,
            floatingScoreSpawnPoint.position,
            Quaternion.identity
        );
        obj.GetComponent<FloatingScoreUI>()?.Setup(score);
    }

    private Color GetUrgencyColour(float elapsed)
    {
        if (elapsed >= criticalThreshold)
            return colourCritical;
        if (elapsed >= urgentThreshold)
            return colourUrgent;
        return colourNormal;
    }

    private void ClearTimerUI()
    {
        if (timerText != null)
            timerText.text = "";
        if (ingredientsListText != null)
            ingredientsListText.text = "";
    }

    private void OnGameEnded()
    {
        hasActiveOrder = false;
        StopAllCoroutines();
        ClearTimerUI();
    }

    private void OnGameStarted()
    {
        StopAllCoroutines();
        hasActiveOrder = false;
        orderActiveTime = 0f;
        currentOrder.Clear();
        ClearTimerUI();
        StartCoroutine(SpawnOrderDelay(0f));
    }
}
