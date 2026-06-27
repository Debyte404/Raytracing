using UnityEngine;

[ExecuteAlways]
public class RaytracingSphere : MonoBehaviour
{
    public Vector3 Position => transform.position;
    public float Radius => 0.5f * Mathf.Max(
        transform.lossyScale.x,
        transform.lossyScale.y,
        transform.lossyScale.z
    );

}
