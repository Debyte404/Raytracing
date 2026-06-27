using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

    struct MaterialPropertyData
    {
        public Vector3 color;
        public float emissionStrength;
        public Vector3 emissionColor;
        public float roughness;
    }
    struct SphereData
    {
        public Vector3 position;
        public float radius;
        public MaterialPropertyData material;
    }

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        RefreshKernel();
        ReleaseSphereBuffer();
        ReleaseTargetTexture();

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        SceneView.RepaintAll();
#endif
    }

    void OnValidate()
    {
        RefreshKernel();
    }

    void UpdateSphereBuffer()
    {
        if (Spheres == null || Spheres.Length == 0)
        {
            ReleaseSphereBuffer();
            testingshader.SetInt("_SphereCount", 0);
            return;
        }

        int validSphereCount = 0;
        for (int i = 0; i < Spheres.Length; i++)
        {
            if (Spheres[i] != null)
            {
                validSphereCount++;
            }
        }

        if (validSphereCount == 0)
        {
            ReleaseSphereBuffer();
            testingshader.SetInt("_SphereCount", 0);
            return;
        }

        SphereData[] sphereData = new SphereData[validSphereCount];

        int sphereDataIndex = 0;
        for (int i = 0; i < Spheres.Length; i++)
        {
            if (Spheres[i] == null)
            {
                continue;
            }

            sphereData[sphereDataIndex] = new SphereData
            {
                position = Spheres[i].Position,
                radius = Spheres[i].Radius,
                material = new MaterialPropertyData
                {
                    color = new Vector3(
                        Spheres[i].materialProperty.color.r,
                        Spheres[i].materialProperty.color.g,
                        Spheres[i].materialProperty.color.b
                    ),
                    emissionStrength = Spheres[i].materialProperty.emissionStrength,
                    emissionColor = new Vector3(
                        Spheres[i].materialProperty.emissionColor.r,
                        Spheres[i].materialProperty.emissionColor.g,
                        Spheres[i].materialProperty.emissionColor.b
                    ),
                    roughness = Spheres[i].materialProperty.roughness
                }
            };

            sphereDataIndex++;
        }

        if (sphereBuffer == null || !sphereBuffer.IsValid() || sphereBuffer.count != sphereData.Length)
        {
            ReleaseSphereBuffer();
            sphereBuffer = new ComputeBuffer(sphereData.Length, sizeof(float) * 12);
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

        Camera renderCamera = Camera.current != null ? Camera.current : cam;
        if (renderCamera == null)
        {
            Graphics.Blit(src, dst);
            return;
        }

        testingshader.SetVector("_CameraPosition", renderCamera.transform.position);
        testingshader.SetMatrix("_CameraToWorld", renderCamera.cameraToWorldMatrix);
        testingshader.SetMatrix("_CameraInverseProjection", renderCamera.projectionMatrix.inverse);

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
        if (renderTexture == null || !renderTexture.IsCreated() || renderTexture.width != width || renderTexture.height != height)
        {
            if (renderTexture != null)
            {
                ReleaseTargetTexture();
            }

            renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
            {
                enableRandomWrite = true
            };
            renderTexture.Create();
        }
    }

#if UNITY_EDITOR
    void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode)
        {
            ReleaseSphereBuffer();
            ReleaseTargetTexture();
            SceneView.RepaintAll();
        }
    }
#endif

    void ReleaseSphereBuffer()
    {
        if (sphereBuffer != null)
        {
            sphereBuffer.Release();
            sphereBuffer = null;
        }
    }

    void ReleaseTargetTexture()
    {
        if (renderTexture == null)
        {
            return;
        }

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

    void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
        ReleaseSphereBuffer();
        ReleaseTargetTexture();

#if UNITY_EDITOR
        SceneView.RepaintAll();
#endif
    }

    void OnDestroy()
    {
        ReleaseSphereBuffer();
        ReleaseTargetTexture();
    }
}
