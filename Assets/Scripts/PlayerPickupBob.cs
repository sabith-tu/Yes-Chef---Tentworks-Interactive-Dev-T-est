using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerInventory))]
public class PlayerPickupBob : MonoBehaviour
{
    public Transform holdPoint;

    public float bounceHeight = 0.22f;
    public float bounceDuration = 0.18f;

    public float floatAmplitude = 0.04f;
    public float floatFrequency = 1.6f;

    private PlayerInventory inventory;
    private Vector3 holdOrigin;
    private bool isBouncing;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        if (holdPoint != null)
            holdOrigin = holdPoint.localPosition;
    }

    private void Update()
    {
        if (holdPoint == null || isBouncing)
            return;

        if (inventory.HasItem)
        {
            float offset = Mathf.Sin(Time.time * floatFrequency * Mathf.PI * 2f) * floatAmplitude;
            holdPoint.localPosition = holdOrigin + new Vector3(0f, offset, 0f);
        }
        else
        {
            holdPoint.localPosition = Vector3.Lerp(
                holdPoint.localPosition,
                holdOrigin,
                Time.deltaTime * 12f
            );
        }
    }

    public void TriggerPickupBounce()
    {
        if (!isBouncing)
            StartCoroutine(DoBounce());
    }

    private IEnumerator DoBounce()
    {
        isBouncing = true;
        float half = bounceDuration * 0.5f;

        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            if (holdPoint == null)
                break;
            float y = Mathf.Lerp(0f, bounceHeight, t / half);
            holdPoint.localPosition = holdOrigin + new Vector3(0f, y, 0f);
            yield return null;
        }

        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            if (holdPoint == null)
                break;
            float y = Mathf.Lerp(bounceHeight, 0f, t / half);
            holdPoint.localPosition = holdOrigin + new Vector3(0f, y, 0f);
            yield return null;
        }

        if (holdPoint != null)
            holdPoint.localPosition = holdOrigin;

        isBouncing = false;
    }
}
