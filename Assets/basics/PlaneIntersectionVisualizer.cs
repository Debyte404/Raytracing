using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class PlaneIntersectionVisualizer : MonoBehaviour
{
    [Header("Ray")]
    public bool useCameraAsRayOrigin = true;
    public Vector3 rayOrigin = new Vector3(0f, 2f, -5f);
    public Vector3 localRayDirection = Vector3.forward;
    [Min(0.01f)] public float rayLength = 12f;

    [Header("Plane")]
    public Vector3 planePoint = new Vector3(0f, 0f, 0f);
    public Vector3 planeNormal = Vector3.up;
    [Min(0.1f)] public float planeSize = 5f;

    [Header("Drawing")]
    [Min(0.001f)] public float hitPointRadius = 0.1f;
    [Min(0.01f)] public float normalLength = 1f;
    public Color planeColor = new Color(1f, 1f, 1f, 0.35f);
    public Color rayHitColor = Color.green;
    public Color rayMissColor = Color.red;
    public Color normalColor = Color.yellow;

    void OnDrawGizmos()
    {
        Vector3 normal = planeNormal.normalized;
        Vector3 origin = useCameraAsRayOrigin ? transform.position : rayOrigin;
        Vector3 direction = transform.TransformDirection(localRayDirection.normalized);

        DrawPlane(normal);

        bool didHit = HitPlane(origin, direction, planePoint, normal, out float distance);
        float visibleDistance = didHit ? Mathf.Min(distance, rayLength) : rayLength;

        Gizmos.color = didHit ? rayHitColor : rayMissColor;
        Gizmos.DrawLine(origin, origin + direction * visibleDistance);

        Gizmos.color = normalColor;
        Gizmos.DrawLine(planePoint, planePoint + normal * normalLength);

#if UNITY_EDITOR
        Handles.color = normalColor;
        Handles.ArrowHandleCap(0, planePoint + normal * normalLength, Quaternion.LookRotation(normal), 0.25f, EventType.Repaint);
#endif

        if (!didHit)
        {
            return;
        }

        Gizmos.color = rayHitColor;
        Gizmos.DrawSphere(origin + direction * distance, hitPointRadius);
    }

    void DrawPlane(Vector3 normal)
    {
        Vector3 tangent = Vector3.Cross(normal, Vector3.forward);
        if (tangent.sqrMagnitude < 0.001f)
        {
            tangent = Vector3.Cross(normal, Vector3.right);
        }

        tangent.Normalize();
        Vector3 bitangent = Vector3.Cross(normal, tangent).normalized;
        float halfSize = planeSize * 0.5f;

        Vector3 a = planePoint + (-tangent - bitangent) * halfSize;
        Vector3 b = planePoint + (tangent - bitangent) * halfSize;
        Vector3 c = planePoint + (tangent + bitangent) * halfSize;
        Vector3 d = planePoint + (-tangent + bitangent) * halfSize;

        Gizmos.color = planeColor;
        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(b, c);
        Gizmos.DrawLine(c, d);
        Gizmos.DrawLine(d, a);
        Gizmos.DrawLine(planePoint - tangent * halfSize, planePoint + tangent * halfSize);
        Gizmos.DrawLine(planePoint - bitangent * halfSize, planePoint + bitangent * halfSize);
    }

    bool HitPlane(Vector3 origin, Vector3 direction, Vector3 point, Vector3 normal, out float distance)
    {
        float denom = Vector3.Dot(direction, normal);
        if (Mathf.Abs(denom) < 0.0001f)
        {
            distance = 0f;
            return false;
        }

        distance = Vector3.Dot(point - origin, normal) / denom;
        return distance > 0f;
    }
}
