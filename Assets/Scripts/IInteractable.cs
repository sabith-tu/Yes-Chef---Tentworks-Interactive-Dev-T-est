public interface IInteractable
{
    void OnInteract(PlayerInventory player);

    void OnEnterTrigger(PlayerInventory player);

    void OnExitTrigger(PlayerInventory player);

    bool AutoInteract { get; }
}
