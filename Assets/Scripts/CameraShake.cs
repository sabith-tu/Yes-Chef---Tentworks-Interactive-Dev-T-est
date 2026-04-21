using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private Vector3 restLocalPos;
    private bool isShaking;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        restLocalPos = transform.localPosition;
    }

    public void Shake(float duration, float magnitude)
    {
        if (!isShaking)
            StartCoroutine(DoShake(duration, magnitude));
    }

    public void ShakeLight() => Shake(0.18f, 0.07f);

    public void ShakeHeavy() => Shake(0.40f, 0.20f);

    private IEnumerator DoShake(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float dampen = 1f - (elapsed / duration);
            float x = Random.Range(-1f, 1f) * magnitude * dampen;
            float z = Random.Range(-1f, 1f) * magnitude * dampen;

            transform.localPosition = restLocalPos + new Vector3(x, 0f, z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = restLocalPos;
        isShaking = false;
    }

    public void RefreshRestPosition() => restLocalPos = transform.localPosition;
}
