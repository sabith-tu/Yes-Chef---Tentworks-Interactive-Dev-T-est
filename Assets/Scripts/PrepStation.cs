using System.Collections.Generic;
using UnityEngine;

public class PrepStation : MonoBehaviour, IInteractable
{
    public PreparationType stationType;
    public bool requiresPlayerPresence;
    public Transform[] slotLocations;

    public GameObject prepSlotUIPrefab;

    public GameObject prepCompleteVFXPrefab;

    public Transform pulseTarget;

    private class ProcessingSlot
    {
        public IngredientData item;
        public float currentTimer;
        public bool isFinished;

        public GameObject visualInstance;
        public int slotIndex;
        public PrepSlotUI uiInstance;
        public Transform slotTransform;
    }

    private readonly List<ProcessingSlot> activeSlots = new List<ProcessingSlot>();
    private bool isPlayerPresent;

    private static readonly Vector3 UIOffset = new Vector3(0f, 1.5f, 0f);

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            return;
        if (requiresPlayerPresence && !isPlayerPresent)
            return;

        foreach (ProcessingSlot slot in activeSlots)
        {
            if (slot.isFinished)
                continue;

            slot.currentTimer += Time.deltaTime;

            if (slot.uiInstance != null)
            {
                slot.uiInstance.transform.position = slot.slotTransform.position + UIOffset;
                BillboardToCamera(slot.uiInstance.transform);
                slot.uiInstance.UpdateProgress(slot.currentTimer, slot.item.requiredPrepTime);
            }

            if (slot.currentTimer >= slot.item.requiredPrepTime)
            {
                slot.isFinished = true;
                slot.uiInstance?.HideUI();
                SwapToPrepararedVisual(slot);
                OnPreparationComplete(slot);
            }
        }
    }

    public bool AutoInteract => false;

    public void OnEnterTrigger(PlayerInventory player)
    {
        isPlayerPresent = true;
        StationFeedback.HighlightStation(this, true);
    }

    public void OnExitTrigger(PlayerInventory player)
    {
        isPlayerPresent = false;
        StationFeedback.HighlightStation(this, false);
    }

    public void OnInteract(PlayerInventory player)
    {
        if (requiresPlayerPresence && !isPlayerPresent)
            return;

        if (!player.HasItem)
        {
            for (int i = 0; i < activeSlots.Count; i++)
            {
                if (!activeSlots[i].isFinished)
                    continue;

                ProcessingSlot slot = activeSlots[i];
                player.PickUpItem(slot.item, true, slot.visualInstance);

                if (slot.uiInstance != null)
                    Destroy(slot.uiInstance.gameObject);

                activeSlots.RemoveAt(i);
                return;
            }
            return;
        }

        if (player.isItemPrepared)
            return;
        if (player.currentItem.requiredPrepType != stationType)
            return;
        if (activeSlots.Count >= slotLocations.Length)
            return;

        IngredientData itemData = player.currentItem;
        int openIndex = GetFirstOpenSlotIndex();
        Transform slotTf = slotLocations[openIndex];

        GameObject visual = player.RemoveItem();
        visual.transform.position = slotTf.position;
        visual.transform.rotation = slotTf.rotation;

        PrepSlotUI uiInstance = null;
        if (prepSlotUIPrefab != null)
        {
            GameObject uiObj = Instantiate(
                prepSlotUIPrefab,
                slotTf.position + UIOffset,
                Quaternion.identity
            );
            uiInstance = uiObj.GetComponent<PrepSlotUI>();
        }

        activeSlots.Add(
            new ProcessingSlot
            {
                item = itemData,
                slotIndex = openIndex,
                slotTransform = slotTf,
                currentTimer = 0f,
                isFinished = false,
                visualInstance = visual,
                uiInstance = uiInstance,
            }
        );
    }

    private void SwapToPrepararedVisual(ProcessingSlot slot)
    {
        if (slot.item.preparedPrefab == null)
        {
            Debug.LogError($"{slot.item.ingredientName} has no preparedPrefab.", this);
            return;
        }

        Destroy(slot.visualInstance);

        slot.visualInstance = Instantiate(
            slot.item.preparedPrefab,
            slot.slotTransform.position,
            slot.slotTransform.rotation
        );
    }

    private void OnPreparationComplete(ProcessingSlot slot)
    {
        if (prepCompleteVFXPrefab != null)
            Destroy(
                Instantiate(
                    prepCompleteVFXPrefab,
                    slot.slotTransform.position + UIOffset,
                    Quaternion.identity
                ),
                3f
            );

        Transform target = pulseTarget != null ? pulseTarget : transform;
        StationFeedback.PulseTransform(this, target);
    }

    private int GetFirstOpenSlotIndex()
    {
        var used = new HashSet<int>();
        foreach (ProcessingSlot s in activeSlots)
            used.Add(s.slotIndex);
        for (int i = 0; i < slotLocations.Length; i++)
            if (!used.Contains(i))
                return i;

        Debug.LogError("No free slot found!");
        return 0;
    }

    private static void BillboardToCamera(Transform t)
    {
        Camera cam = Camera.main;
        if (cam == null)
            return;
        t.LookAt(
            t.position + cam.transform.rotation * Vector3.forward,
            cam.transform.rotation * Vector3.up
        );
    }
}
