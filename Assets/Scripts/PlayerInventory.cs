using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public Transform holdPoint;

    public IngredientData currentItem { get; private set; }
    public bool isItemPrepared { get; private set; }
    public bool HasItem => currentItem != null;

    private GameObject currentVisualPrefab;
    private PlayerPickupBob pickupBob;

    private void Awake()
    {
        pickupBob = GetComponent<PlayerPickupBob>();
    }

    public void PickUpItem(IngredientData item, bool prepared, GameObject visualInstance)
    {
        if (HasItem)
            return;
        if (item == null || visualInstance == null)
        {
            Debug.LogError("PickUpItem called with null item or visual.");
            return;
        }

        currentItem = item;
        isItemPrepared = prepared;
        currentVisualPrefab = visualInstance;

        if (holdPoint != null)
        {
            currentVisualPrefab.transform.SetParent(holdPoint);
            currentVisualPrefab.transform.localPosition = Vector3.zero;
            currentVisualPrefab.transform.localRotation = Quaternion.identity;
        }

        pickupBob?.TriggerPickupBounce();
    }

    public GameObject RemoveItem()
    {
        if (!HasItem)
            return null;

        GameObject itemToReturn = currentVisualPrefab;

        if (itemToReturn != null)
            itemToReturn.transform.SetParent(null);

        currentItem = null;
        isItemPrepared = false;
        currentVisualPrefab = null;

        return itemToReturn;
    }
}
