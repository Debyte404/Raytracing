using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class RayDirectionVisualizer : MonoBehaviour
{
    [Header("Projection Plane")]
    [Min(1)] public int columns = 14;
    [Min(1)] public int rows = 8;
    [Min(0.01f)] public float projectionPlaneDistance = 3f;

    [Header("Rays")]
    [Min(0.01f)] public float rayLength = 7f;
    [Min(0.001f)] public float pointRadius = 0.08f;
    [Min(0.001f)] public float arrowSize = 0.28f;
    public bool drawProjectionFrame = true;
    public bool drawRays = true;

    [Header("Colors")]
    public Color projectionPointColor = Color.white;
    public Color rayColor = Color.black;
    public Color frameColor = new Color(1f, 1f, 1f, 0.35f);

    void OnDrawGizmos()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null || !cam.enabled)
        {
            return;
        }

        float planeHeight = 2f * projectionPlaneDistance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float planeWidth = planeHeight * cam.aspect;

        Vector3 origin = transform.position;
        Vector3 center = origin + transform.forward * projectionPlaneDistance;
        Vector3 right = transform.right;
        Vector3 up = transform.up;

        if (drawProjectionFrame)
        {
            DrawProjectionFrame(center, right, up, planeWidth, planeHeight);
        }

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                float u = ((x + 0.5f) / columns - 0.5f) * planeWidth;
                float v = ((y + 0.5f) / rows - 0.5f) * planeHeight;
                Vector3 pointOnProjectionPlane = center + right * u + up * v;
                Vector3 rayDirection = (pointOnProjectionPlane - origin).normalized;

                Gizmos.color = projectionPointColor;
                Gizmos.DrawSphere(pointOnProjectionPlane, pointRadius);

                if (drawRays)
                {
                    DrawRay(origin, rayDirection);
                }
            }
        }
    }

    void DrawProjectionFrame(Vector3 center, Vector3 right, Vector3 up, float width, float height)
    {
        Vector3 halfRight = right * width * 0.5f;
        Vector3 halfUp = up * height * 0.5f;

        Vector3 topLeft = center - halfRight + halfUp;
        Vector3 topRight = center + halfRight + halfUp;
        Vector3 bottomRight = center + halfRight - halfUp;
        Vector3 bottomLeft = center - halfRight - halfUp;

        Gizmos.color = frameColor;
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }

    void DrawRay(Vector3 origin, Vector3 direction)
    {
        Vector3 end = origin + direction * rayLength;

        Gizmos.color = rayColor;
        Gizmos.DrawLine(origin, end);

#if UNITY_EDITOR
        Handles.color = rayColor;
        Handles.ArrowHandleCap(0, end, Quaternion.LookRotation(direction), arrowSize, EventType.Repaint);
#endif
    }
}
