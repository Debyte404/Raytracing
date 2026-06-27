using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class SphereIntersectionVisualizer : MonoBehaviour
{
    [Header("Ray")]
    public bool useCameraAsRayOrigin = true;
    public Vector3 rayOrigin = new Vector3(0f, 1f, -5f);
    public Vector3 localRayDirection = Vector3.forward;
    [Min(0.01f)] public float rayLength = 12f;

    [Header("Sphere")]
    public Vector3 sphereCenter = new Vector3(0f, 1f, 3f);
    [Min(0.01f)] public float sphereRadius = 1f;

    [Header("Drawing")]
    [Min(0.001f)] public float hitPointRadius = 0.1f;
    [Min(0.01f)] public float normalLength = 1f;
    public Color sphereColor = new Color(1f, 1f, 1f, 0.65f);
    public Color rayHitColor = Color.green;
    public Color rayMissColor = Color.red;
    public Color normalColor = Color.cyan;

    void OnDrawGizmos()
    {
        Vector3 origin = useCameraAsRayOrigin ? transform.position : rayOrigin;
        Vector3 direction = transform.TransformDirection(localRayDirection.normalized);

        bool didHit = HitSphere(origin, direction, sphereCenter, sphereRadius, out float distance);
        float visibleDistance = didHit ? Mathf.Min(distance, rayLength) : rayLength;

        Gizmos.color = sphereColor;
        Gizmos.DrawWireSphere(sphereCenter, sphereRadius);

        Gizmos.color = didHit ? rayHitColor : rayMissColor;
        Gizmos.DrawLine(origin, origin + direction * visibleDistance);

        if (!didHit)
        {
            return;
        }

        Vector3 hitPoint = origin + direction * distance;
        Vector3 normal = (hitPoint - sphereCenter).normalized;

        Gizmos.color = rayHitColor;
        Gizmos.DrawSphere(hitPoint, hitPointRadius);

        Gizmos.color = normalColor;
        Gizmos.DrawLine(hitPoint, hitPoint + normal * normalLength);

#if UNITY_EDITOR
        Handles.color = normalColor;
        Handles.ArrowHandleCap(0, hitPoint + normal * normalLength, Quaternion.LookRotation(normal), 0.25f, EventType.Repaint);
#endif
    }

    bool HitSphere(Vector3 origin, Vector3 direction, Vector3 center, float radius, out float distance)
    {
        Vector3 offset = origin - center;
        float a = Vector3.Dot(direction, direction);
        float b = 2f * Vector3.Dot(offset, direction);
        float c = Vector3.Dot(offset, offset) - radius * radius;
        float discriminant = b * b - 4f * a * c;

        if (discriminant < 0f)
        {
            distance = 0f;
            return false;
        }

        distance = (-b - Mathf.Sqrt(discriminant)) / (2f * a);
        return distance > 0f;
    }
}
