using System.Collections;
using UnityEngine;

public static class StationFeedback
{
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
    private static readonly Color HighlightColour = new Color(0.45f, 0.38f, 0.08f);
    private static readonly MaterialPropertyBlock _mpb = new MaterialPropertyBlock();

    public static void HighlightStation(MonoBehaviour station, bool on)
    {
        foreach (Renderer r in station.GetComponentsInChildren<Renderer>())
        {
            r.GetPropertyBlock(_mpb);
            _mpb.SetColor(EmissionColorID, on ? HighlightColour : Color.black);
            r.SetPropertyBlock(_mpb);

            foreach (Material mat in r.sharedMaterials)
            {
                if (mat == null)
                    continue;
                if (on)
                    mat.EnableKeyword("_EMISSION");
                else
                    mat.DisableKeyword("_EMISSION");
            }
        }
    }

    public static void PulseStation(MonoBehaviour station)
    {
        station.StartCoroutine(DoPulse(station.transform));
    }

    public static void PulseTransform(MonoBehaviour owner, Transform target)
    {
        owner.StartCoroutine(DoPulse(target));
    }

    private static IEnumerator DoPulse(Transform t)
    {
        if (t == null)
            yield break;

        Vector3 original = t.localScale;
        Vector3 big = original * 1.14f;
        const float half = 0.08f;

        for (float e = 0f; e < half; e += Time.deltaTime)
        {
            if (t == null)
                yield break;
            t.localScale = Vector3.Lerp(original, big, e / half);
            yield return null;
        }
        for (float e = 0f; e < half; e += Time.deltaTime)
        {
            if (t == null)
                yield break;
            t.localScale = Vector3.Lerp(big, original, e / half);
            yield return null;
        }

        if (t != null)
            t.localScale = original;
    }
}
