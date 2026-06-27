using UnityEngine;
using static shadermanager;

[ExecuteAlways]
[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
public class shadermanager : MonoBehaviour
{
    [Header("Shader Settings")]
    public ComputeShader testingshader;
    public Material blendMaterial;

    [Header("Scene View")]
    public bool useShaderInSceneView = true;

    [Header("Blend")]
    [Range(0f, 1f)]
    public float blendStrength = 1f;

    private RenderTexture renderTexture;
    private int kernelIndex = -1;
    private Camera cam;

    public RaytracingSphere[] Spheres;
    private ComputeBuffer sphereBuffer;

    struct SphereData
    {
        public Vector3 position;
        public float radius;
    }

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        RefreshKernel();
    }

    void OnValidate()
    {
        RefreshKernel();
    }

    void UpdateSphereBuffer()
    {
        if (Spheres == null)
        {
            testingshader.SetInt("_SphereCount", 0);
            return;
        }

        SphereData[] sphereData = new SphereData[Spheres.Length];

        for (int i = 0; i < Spheres.Length; i++)
        {
            if (Spheres[i] == null)
                continue;

            sphereData[i] = new SphereData
            {
                position = Spheres[i].Position,
                radius = Spheres[i].Radius,
            };
        }

        if (sphereBuffer == null || sphereBuffer.count != sphereData.Length)
        {
            if (sphereBuffer != null)
            {
                sphereBuffer.Release();
            }

            sphereBuffer = new ComputeBuffer(sphereData.Length, sizeof(float) * 4);
        }

        sphereBuffer.SetData(sphereData);
        testingshader.SetBuffer(kernelIndex, "_Spheres", sphereBuffer);
        testingshader.SetInt("_SphereCount", sphereData.Length);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (IsSceneViewCamera() && !useShaderInSceneView)
        {
            Graphics.Blit(src, dst);
            return;
        }

        RefreshKernel();

        if (testingshader == null || blendMaterial == null || kernelIndex < 0)
        {
            Graphics.Blit(src, dst);
            return;
        }

        EnsureTargetTexture(src.width, src.height);
        //data transfer for camera
        if (cam == null)
        {
            cam = GetComponent<Camera>();
        }

        UpdateSphereBuffer();

        testingshader.SetVector("_CameraPosition", cam.transform.position);
        testingshader.SetMatrix("_CameraToWorld", cam.cameraToWorldMatrix);
        testingshader.SetMatrix("_CameraInverseProjection", cam.projectionMatrix.inverse);

        //Camera rayCamera = Camera.main != null ? Camera.main : cam;

        //float renderAspect = src.width / (float)src.height;
        //Matrix4x4 projection = Matrix4x4.Perspective(
        //    rayCamera.fieldOfView,
        //    renderAspect,
        //    rayCamera.nearClipPlane,
        //    rayCamera.farClipPlane
        //);

        //testingshader.SetVector("_CameraPosition", rayCamera.transform.position);
        //testingshader.SetMatrix("_CameraToWorld", rayCamera.cameraToWorldMatrix);
        //testingshader.SetMatrix("_CameraInverseProjection", projection.inverse);

        testingshader.SetTexture(kernelIndex, "Result", renderTexture);

        int threadGroupsX = Mathf.CeilToInt(src.width / 8f);
        int threadGroupsY = Mathf.CeilToInt(src.height / 8f);
        testingshader.Dispatch(kernelIndex, threadGroupsX, threadGroupsY, 1);

        blendMaterial.SetTexture("_EffectTex", renderTexture);
        blendMaterial.SetFloat("_Blend", blendStrength);
        Graphics.Blit(src, dst, blendMaterial);
    }

    void RefreshKernel()
    {
        kernelIndex = -1;
        if (testingshader != null)
        {
            kernelIndex = testingshader.FindKernel("HelloWorld");
        }
    }

    bool IsSceneViewCamera()
    {
        return Camera.current != null && Camera.current.cameraType == CameraType.SceneView;
    }

    void EnsureTargetTexture(int width, int height)
    {
        if (renderTexture == null || renderTexture.width != width || renderTexture.height != height)
        {
            if (renderTexture != null)
            {
                renderTexture.Release();
                if (Application.isPlaying)
                {
                    Destroy(renderTexture);
                }
                else
                {
                    DestroyImmediate(renderTexture);
                }
            }

            renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
            {
                enableRandomWrite = true
            };
            renderTexture.Create();
        }
    }

    void OnDestroy()
    {
        if (sphereBuffer != null)
        {
            sphereBuffer.Release();
            sphereBuffer = null;
        }

        if (renderTexture != null)
        {
            renderTexture.Release();
            if (Application.isPlaying)
            {
                Destroy(renderTexture);
            }
            else
            {
                DestroyImmediate(renderTexture);
            }
            renderTexture = null;
        }
    }
}
