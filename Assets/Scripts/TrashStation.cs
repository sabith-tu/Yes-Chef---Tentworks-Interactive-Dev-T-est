using UnityEngine;

public class TrashStation : MonoBehaviour, IInteractable
{
    public bool AutoInteract => false;

    public void OnEnterTrigger(PlayerInventory player) =>
        StationFeedback.HighlightStation(this, true);

    public void OnExitTrigger(PlayerInventory player) =>
        StationFeedback.HighlightStation(this, false);

    public void OnInteract(PlayerInventory player)
    {
        if (!player.HasItem)
            return;
        GameObject visual = player.RemoveItem();
        if (visual != null)
            Destroy(visual);
    }
}
