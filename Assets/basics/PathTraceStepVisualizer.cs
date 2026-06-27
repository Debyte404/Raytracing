using UnityEngine;

[ExecuteAlways]
public class PathTraceStepVisualizer : MonoBehaviour
{
    [Header("Ray")]
    public bool useCameraAsRayOrigin = true;
    public Vector3 rayOrigin = new Vector3(0f, 2f, -5f);
    public Vector3 localRayDirection = Vector3.forward;
    [Min(1)] public int maxBounces = 4;
    [Min(0.01f)] public float missDistance = 4f;
    public int seed = 19;

    [Header("Sphere")]
    public Vector3 sphereCenter = new Vector3(0f, 1f, 3f);
    [Min(0.01f)] public float sphereRadius = 1f;

    [Header("Plane")]
    public Vector3 planePoint = Vector3.zero;
    public Vector3 planeNormal = Vector3.up;

    [Header("Drawing")]
    [Min(0.001f)] public float hitPointRadius = 0.08f;
    public Color sphereColor = new Color(1f, 1f, 1f, 0.45f);
    public Color planeColor = new Color(1f, 1f, 1f, 0.25f);
    public Color pathColor = Color.black;
    public Color hitColor = Color.green;
    public Color missColor = Color.red;

    void OnDrawGizmos()
    {
        Vector3 origin = useCameraAsRayOrigin ? transform.position : rayOrigin;
        Vector3 direction = transform.TransformDirection(localRayDirection.normalized);
        Vector3 normal = planeNormal.normalized;
        uint randomState = (uint)Mathf.Max(1, seed);

        Gizmos.color = sphereColor;
        Gizmos.DrawWireSphere(sphereCenter, sphereRadius);

        DrawPlane(normal);

        for (int bounce = 0; bounce < maxBounces; bounce++)
        {
            bool sphereHit = HitSphere(origin, direction, sphereCenter, sphereRadius, out float sphereDistance, out Vector3 sphereNormal);
            bool planeHit = HitPlane(origin, direction, planePoint, normal, out float planeDistance);

            float closestDistance = float.MaxValue;
            Vector3 hitNormal = Vector3.zero;

            if (sphereHit && sphereDistance < closestDistance)
            {
                closestDistance = sphereDistance;
                hitNormal = sphereNormal;
            }

            if (planeHit && planeDistance < closestDistance)
            {
                closestDistance = planeDistance;
                hitNormal = normal;
            }

            if (closestDistance == float.MaxValue)
            {
                Gizmos.color = missColor;
                Gizmos.DrawLine(origin, origin + direction * missDistance);
                return;
            }

            Vector3 hitPoint = origin + direction * closestDistance;
            Gizmos.color = pathColor;
            Gizmos.DrawLine(origin, hitPoint);

            Gizmos.color = hitColor;
            Gizmos.DrawSphere(hitPoint, hitPointRadius);
            Gizmos.DrawLine(hitPoint, hitPoint + hitNormal * 0.6f);

            origin = hitPoint + hitNormal * 0.02f;
            direction = RandomHemisphereDirection(hitNormal, ref randomState);
        }
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
        float halfSize = 3f;

        Gizmos.color = planeColor;
        Gizmos.DrawLine(planePoint - tangent * halfSize - bitangent * halfSize, planePoint + tangent * halfSize - bitangent * halfSize);
        Gizmos.DrawLine(planePoint + tangent * halfSize - bitangent * halfSize, planePoint + tangent * halfSize + bitangent * halfSize);
        Gizmos.DrawLine(planePoint + tangent * halfSize + bitangent * halfSize, planePoint - tangent * halfSize + bitangent * halfSize);
        Gizmos.DrawLine(planePoint - tangent * halfSize + bitangent * halfSize, planePoint - tangent * halfSize - bitangent * halfSize);
    }

    bool HitSphere(Vector3 origin, Vector3 direction, Vector3 center, float radius, out float distance, out Vector3 normal)
    {
        Vector3 offset = origin - center;
        float a = Vector3.Dot(direction, direction);
        float b = 2f * Vector3.Dot(offset, direction);
        float c = Vector3.Dot(offset, offset) - radius * radius;
        float discriminant = b * b - 4f * a * c;

        if (discriminant < 0f)
        {
            distance = 0f;
            normal = Vector3.zero;
            return false;
        }

        distance = (-b - Mathf.Sqrt(discriminant)) / (2f * a);
        if (distance <= 0f)
        {
            normal = Vector3.zero;
            return false;
        }

        Vector3 hitPoint = origin + direction * distance;
        normal = (hitPoint - center).normalized;
        return true;
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

    Vector3 RandomHemisphereDirection(Vector3 normal, ref uint state)
    {
        Vector3 direction = RandomUnitVector(ref state);
        return Vector3.Dot(direction, normal) < 0f ? -direction : direction;
    }

    Vector3 RandomUnitVector(ref uint state)
    {
        Vector3 point = new Vector3(RandomValue(ref state), RandomValue(ref state), RandomValue(ref state)) * 2f - Vector3.one;
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
