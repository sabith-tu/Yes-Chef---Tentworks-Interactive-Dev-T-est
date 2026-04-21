using UnityEngine;

public class RefrigeratorStation : MonoBehaviour, IInteractable
{
    public IngredientData[] availableIngredients;

    public FridgeSelectionUI selectionUI;

    public bool AutoInteract => true;

    public void OnInteract(PlayerInventory player)
    {
        if (player.HasItem)
            return;

        if (selectionUI == null)
        {
            Debug.LogError("selectionUI not assigned.", this);
            return;
        }

        selectionUI.Show(chosen => OnIngredientChosen(player, chosen));
    }

    public void OnEnterTrigger(PlayerInventory player)
    {
        StationFeedback.HighlightStation(this, true);
    }

    public void OnExitTrigger(PlayerInventory player)
    {
        StationFeedback.HighlightStation(this, false);
        selectionUI?.Hide();
    }

    private void Awake()
    {
        if (selectionUI != null && availableIngredients != null)
            selectionUI.Initialise(availableIngredients);
    }

    private void OnIngredientChosen(PlayerInventory player, IngredientData chosen)
    {
        if (player == null || player.HasItem)
        {
            selectionUI?.Hide();
            return;
        }

        if (chosen == null || chosen.rawPrefab == null)
        {
            Debug.LogError($"{chosen?.ingredientName} has no rawPrefab.", this);
            selectionUI?.Hide();
            return;
        }

        GameObject visual = Instantiate(chosen.rawPrefab);
        player.PickUpItem(chosen, false, visual);

        selectionUI?.Hide();
    }
}
