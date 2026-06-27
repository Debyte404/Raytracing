using UnityEngine;

[System.Serializable]
public struct MaterialProperty
{
    public Color color;
    public Color emissionColor;
    [Min(0f)] public float emissionStrength;
    [Range(0f, 1f)] public float roughness;
}

[ExecuteAlways]
public class RaytracingSphere : MonoBehaviour
{
    public MaterialProperty materialProperty = new MaterialProperty
    {
        color = Color.white,
        emissionColor = Color.black,
        emissionStrength = 0f,
        roughness = 1f
    };

    public Vector3 Position => transform.position;
    public float Radius => 0.5f * Mathf.Max(
        transform.lossyScale.x,
        transform.lossyScale.y,
        transform.lossyScale.z
    );

}
