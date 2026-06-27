using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class HemisphereBounceVisualizer : MonoBehaviour
{
    [Header("Surface")]
    public Vector3 surfacePoint = new Vector3(0f, 1f, 0f);
    public Vector3 surfaceNormal = Vector3.up;

    [Header("Samples")]
    [Min(1)] public int sampleCount = 40;
    [Min(0.01f)] public float sampleLength = 1.6f;
    public int seed = 7;

    [Header("Drawing")]
    [Min(0.001f)] public float pointRadius = 0.08f;
    [Min(0.01f)] public float normalLength = 1.2f;
    public Color pointColor = Color.white;
    public Color normalColor = Color.cyan;
    public Color sampleColor = new Color(0f, 0f, 0f, 0.72f);

    void OnDrawGizmos()
    {
        Vector3 normal = surfaceNormal.normalized;

        Gizmos.color = pointColor;
        Gizmos.DrawSphere(surfacePoint, pointRadius);

        Gizmos.color = normalColor;
        Gizmos.DrawLine(surfacePoint, surfacePoint + normal * normalLength);

#if UNITY_EDITOR
        Handles.color = normalColor;
        Handles.ArrowHandleCap(0, surfacePoint + normal * normalLength, Quaternion.LookRotation(normal), 0.25f, EventType.Repaint);
#endif

        uint state = (uint)Mathf.Max(1, seed);
        Gizmos.color = sampleColor;

        for (int i = 0; i < sampleCount; i++)
        {
            Vector3 randomDirection = RandomUnitVector(ref state);
            if (Vector3.Dot(randomDirection, normal) < 0f)
            {
                randomDirection = -randomDirection;
            }

            Gizmos.DrawLine(surfacePoint, surfacePoint + randomDirection * sampleLength);
        }
    }

    Vector3 RandomUnitVector(ref uint state)
    {
        Vector3 point;
        int guard = 0;

        do
        {
            point = new Vector3(RandomValue(ref state), RandomValue(ref state), RandomValue(ref state)) * 2f - Vector3.one;
            guard++;
        }
        while (point.sqrMagnitude > 1f && guard < 32);

        return point.sqrMagnitude > 0.0001f ? point.normalized : Vector3.up;
    }

    float RandomValue(ref uint state)
    {
        state = state * 747796405u + 2891336453u;
        uint word = ((state >> (int)((state >> 28) + 4u)) ^ state) * 277803737u;
        word = (word >> 22) ^ word;
        return word / 4294967295f;
    }
}
