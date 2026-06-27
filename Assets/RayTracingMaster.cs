using UnityEngine;

public class RayTracingMaster : MonoBehaviour
{
    [Header("Shader Settings")]
    public ComputeShader rayTracingShader;

    [Header("Scene View")]
    public bool renderInSceneView = true;

    [Header("Scene Objects")]
    public bool addTestSpheres = true;
    public bool addGroundPlane = true;

    private RenderTexture target;
    private Camera mainCamera;
    private ComputeBuffer sphereBuffer;
    private ComputeBuffer planeBuffer;
    private Sphere[] spheres;
    private Plane[] planes;
    private int kernelIndex = -1;

    public struct Sphere
    {
        public Vector3 position;
        public float radius;
        public Vector3 albedo;
        public float pad0;
        public Vector3 specular;
        public float pad1;
        public Vector3 emission;
        public float pad2;
    }

    public struct Plane
    {
        public Vector3 normal;
        public float distance;
        public Vector3 albedo;
        public float pad0;
        public Vector3 specular;
        public float pad1;
        public Vector3 emission;
        public float pad2;
    }

    void OnEnable()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("RayTracingMaster: No Camera found! Attach this script to a Camera GameObject.");
            enabled = false;
            return;
        }

        if (rayTracingShader != null)
        {
            kernelIndex = rayTracingShader.FindKernel("MainRayGen");
            if (kernelIndex < 0)
            {
                Debug.LogError("RayTracingMaster: Kernel 'MainRayGen' not found in compute shader!");
            }
        }

        InitScene();
    }

    void OnDisable()
    {
        if (sphereBuffer != null) { sphereBuffer.Release(); sphereBuffer = null; }
        if (planeBuffer != null) { planeBuffer.Release(); planeBuffer = null; }
        if (target != null) { target.Release(); target = null; }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (rayTracingShader == null || kernelIndex < 0)
        {
            Graphics.Blit(source, destination);
            return;
        }

        SetShaderParameters();
        RenderToTexture(destination);
    }

    void InitScene()
    {
        // Spheres - positioned closer to camera and above ground
        if (addTestSpheres)
        {
            spheres = new Sphere[3];

            // Large red diffuse sphere - center, sitting on ground
            spheres[0] = new Sphere()
            {
                position = new Vector3(0, 1.0f, 3.0f),
                radius = 1.0f,
                albedo = new Vector3(0.8f, 0.2f, 0.2f),
                specular = new Vector3(0.0f, 0.0f, 0.0f),
                emission = Vector3.zero
            };

            // Small chrome reflective sphere - left, sitting on ground
            spheres[1] = new Sphere()
            {
                position = new Vector3(-1.8f, 0.6f, 2.5f),
                radius = 0.6f,
                albedo = new Vector3(0.9f, 0.9f, 0.9f),
                specular = new Vector3(0.9f, 0.9f, 0.9f),
                emission = Vector3.zero
            };

            // Emissive light sphere - upper right, floating
            spheres[2] = new Sphere()
            {
                position = new Vector3(1.5f, 2.0f, 2.0f),
                radius = 0.4f,
                albedo = Vector3.zero,
                specular = Vector3.zero,
                emission = new Vector3(1.0f, 0.9f, 0.7f) * 3.0f
            };

            sphereBuffer = new ComputeBuffer(spheres.Length, 64);
            sphereBuffer.SetData(spheres);
        }
        else
        {
            spheres = new Sphere[0];
            sphereBuffer = new ComputeBuffer(1, 64);
        }

        // Ground plane at Y = -1 (below all spheres)
        if (addGroundPlane)
        {
            planes = new Plane[1];
            planes[0] = new Plane()
            {
                normal = new Vector3(0, 1, 0),
                distance = -1.0f,  // Plane at Y = -1
                albedo = new Vector3(0.3f, 0.3f, 0.35f),
                specular = new Vector3(0.05f, 0.05f, 0.05f),
                emission = Vector3.zero
            };

            planeBuffer = new ComputeBuffer(planes.Length, 64);
            planeBuffer.SetData(planes);
        }
        else
        {
            planes = new Plane[0];
            planeBuffer = new ComputeBuffer(1, 64);
        }

        Debug.Log($"RayTracingMaster: {spheres.Length} spheres, {planes.Length} planes.");
    }

    void SetShaderParameters()
    {
        if (mainCamera == null) return;

        rayTracingShader.SetMatrix("_CameraToWorld", mainCamera.cameraToWorldMatrix);
        rayTracingShader.SetMatrix("_CameraInverseProjection", mainCamera.projectionMatrix.inverse);

        if (sphereBuffer != null)
        {
            rayTracingShader.SetBuffer(kernelIndex, "_Spheres", sphereBuffer);
            rayTracingShader.SetInt("_SphereCount", spheres.Length);
        }
        else
        {
            rayTracingShader.SetInt("_SphereCount", 0);
        }

        if (planeBuffer != null)
        {
            rayTracingShader.SetBuffer(kernelIndex, "_Planes", planeBuffer);
            rayTracingShader.SetInt("_PlaneCount", planes.Length);
        }
        else
        {
            rayTracingShader.SetInt("_PlaneCount", 0);
        }
    }

    void RenderToTexture(RenderTexture destination)
    {
        int width = destination != null ? destination.width : Screen.width;
        int height = destination != null ? destination.height : Screen.height;

        if (target == null || target.width != width || target.height != height)
        {
            if (target != null) target.Release();
            target = new RenderTexture(width, height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create();
        }

        rayTracingShader.SetTexture(kernelIndex, "Result", target);

        int threadGroupsX = Mathf.CeilToInt(width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(height / 8.0f);

        rayTracingShader.Dispatch(kernelIndex, threadGroupsX, threadGroupsY, 1);

        Graphics.Blit(target, destination);
    }
}
