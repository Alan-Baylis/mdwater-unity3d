using UnityEngine;
using System.Collections;

namespace MynjenDook
{
    // This is in fact just the Water script from Pro Standard Assets,
    // just with refraction stuff removed.

    [AddComponentMenu("MynjenDook/MdReflection")]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    //[RequireComponent(typeof(MdWater))] // 搬到submesh里，不需要water组件（父亲结点才绑water）
    public class MdReflection : MonoBehaviour
    {
        public bool m_DisablePixelLights = true;
        public int m_TextureSize = 256;
        public float m_ClipPlaneOffset = 0.07f;

        public LayerMask m_ReflectLayers = -1;

        //private Hashtable m_ReflectionCameras = new Hashtable(); // Camera -> Camera table
        private Camera m_ReflectCamera = null;
        private static string m_strReflectCameraName = "mmwater_reflect_camera";

        private RenderTexture m_ReflectionTexture = null;
        private int m_OldReflectionTextureSize = 0;

        private static bool s_InsideRendering = false;

        void Awake()
        {
            MakeSureCamera();
        }

        void Start()
        {
        }

        void Update()
        {
        }

        public void Initialize()
        {
            MakeSureCamera(); // 因为camera中途不会被删除，初始化一次即可。 但rendertexture可能被删掉，每次画都要检查
        }


        // This is called when it's known that the object will be rendered by some
        // camera. We render reflections and do other updates here.
        // Because the script executes in edit mode, reflections for the scene view
        // camera will just work!
        public void OnWillRenderObject()
        {
            var rend = GetComponent<Renderer>();
            if (!enabled || !rend || !rend.sharedMaterial || !rend.enabled)
                return;

            Camera cam = Camera.current;
            if (!cam || cam.name == m_strReflectCameraName)
                return;

            // Safeguard from recursive reflections.        
            if (s_InsideRendering)
                return;
            s_InsideRendering = true;

            CheckMirrorObjects();

            // find out the reflection plane: position and normal in world space
            Vector3 pos = transform.position;
            Vector3 normal = transform.up;

            // Optionally disable pixel lights for reflection
            int oldPixelLightCount = QualitySettings.pixelLightCount;
            if (m_DisablePixelLights)
                QualitySettings.pixelLightCount = 0;

            UpdateCameraModes(cam, m_ReflectCamera);

            // Render reflection
            // Reflect camera around reflection plane
            float d = -Vector3.Dot(normal, pos) - m_ClipPlaneOffset;
            Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

            Matrix4x4 reflection = Matrix4x4.zero;
            CalculateReflectionMatrix(ref reflection, reflectionPlane);
            Vector3 oldpos = cam.transform.position;
            Vector3 newpos = reflection.MultiplyPoint(oldpos);
            m_ReflectCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;

            // Setup oblique projection matrix so that near plane is our reflection
            // plane. This way we clip everything below/above it for free.
            Vector4 clipPlane = CameraSpacePlane(m_ReflectCamera, pos, normal, 1.0f);
            //Matrix4x4 projection = cam.projectionMatrix;
            Matrix4x4 projection = cam.CalculateObliqueMatrix(clipPlane);
            m_ReflectCamera.projectionMatrix = projection;

            m_ReflectCamera.cullingMask = ~(1 << 4) & m_ReflectLayers.value; // never render water layer
            m_ReflectCamera.targetTexture = m_ReflectionTexture;
            //GL.SetRevertBackfacing (true);
            GL.invertCulling = true;
            m_ReflectCamera.transform.position = newpos;
            Vector3 euler = cam.transform.eulerAngles;
            m_ReflectCamera.transform.eulerAngles = new Vector3(0, euler.y, euler.z);
            m_ReflectCamera.Render();
            m_ReflectCamera.transform.position = oldpos;
            //GL.SetRevertBackfacing (false);
            GL.invertCulling = false;
            Material[] materials = rend.sharedMaterials;
            foreach (Material mat in materials)
            {
                if (mat.HasProperty("_ReflectionTex")) mat.SetTexture("_ReflectionTex", m_ReflectionTexture);
            }

            // Restore pixel light count
            if (m_DisablePixelLights)
                QualitySettings.pixelLightCount = oldPixelLightCount;

            s_InsideRendering = false;
        }


        // Cleanup all the objects we possibly have created
        void OnDisable()
        {
            if (m_ReflectionTexture)
            {
                DestroyImmediate(m_ReflectionTexture); // todo.ksh: DestroyImmediate应该区分EDITOR吧？
                m_ReflectionTexture = null;
            }
            //foreach( DictionaryEntry kvp in m_ReflectionCameras )
            //	DestroyImmediate( ((Camera)kvp.Value).gameObject );
            //m_ReflectionCameras.Clear();
        }


        private void UpdateCameraModes(Camera src, Camera dest)
        {
            if (dest == null)
                return;
            // set camera to clear the same way as current camera
            dest.clearFlags = src.clearFlags;
            dest.backgroundColor = src.backgroundColor;
            if (src.clearFlags == CameraClearFlags.Skybox)
            {
                Skybox sky = src.GetComponent(typeof(Skybox)) as Skybox;
                Skybox mysky = dest.GetComponent(typeof(Skybox)) as Skybox;
                if (!sky || !sky.material)
                {
                    mysky.enabled = false;
                }
                else
                {
                    mysky.enabled = true;
                    mysky.material = sky.material;
                }
            }
            // update other values to match current camera.
            // even if we are supplying custom camera&projection matrices,
            // some of values are used elsewhere (e.g. skybox uses far plane)
            dest.farClipPlane = src.farClipPlane;
            dest.nearClipPlane = src.nearClipPlane;
            dest.orthographic = src.orthographic;
            dest.fieldOfView = src.fieldOfView;
            dest.aspect = src.aspect;
            dest.orthographicSize = src.orthographicSize;
        }

        // On-demand create any objects we need
        private void CheckMirrorObjects()
        {
            // Reflection render texture
            if (!m_ReflectionTexture || m_OldReflectionTextureSize != m_TextureSize)
            {
                if (m_ReflectionTexture)
                    DestroyImmediate(m_ReflectionTexture);
                m_ReflectionTexture = new RenderTexture(m_TextureSize, m_TextureSize, 16);
                m_ReflectionTexture.name = "__MirrorReflection" + GetInstanceID();
                m_ReflectionTexture.isPowerOfTwo = true;
                m_ReflectionTexture.hideFlags = HideFlags.DontSave;
                m_OldReflectionTextureSize = m_TextureSize;
            }
        }
        private void MakeSureCamera()
        {
            if (m_ReflectCamera != null)
                return;

            GameObject go = new GameObject("reflect_camera", typeof(Camera), typeof(Skybox), typeof(FlareLayer)); // todo.ksh flare是需要的吗？
            Camera cam = go.GetComponent<Camera>();
            cam.name = m_strReflectCameraName;
            cam.enabled = false;
            cam.transform.position = transform.position;
            cam.transform.rotation = transform.rotation;
            go.hideFlags = HideFlags.HideAndDontSave;
            m_ReflectCamera = cam;
        }

        // Extended sign: returns -1, 0 or 1 based on sign of a
        private static float sgn(float a)
        {
            if (a > 0.0f) return 1.0f;
            if (a < 0.0f) return -1.0f;
            return 0.0f;
        }

        // Given position/normal of the plane, calculates plane in camera space.
        private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
        {
            Vector3 offsetPos = pos + normal * m_ClipPlaneOffset;
            Matrix4x4 m = cam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(offsetPos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }

        // Calculates reflection matrix around the given plane
        private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
        {
            reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
            reflectionMat.m01 = (-2F * plane[0] * plane[1]);
            reflectionMat.m02 = (-2F * plane[0] * plane[2]);
            reflectionMat.m03 = (-2F * plane[3] * plane[0]);

            reflectionMat.m10 = (-2F * plane[1] * plane[0]);
            reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
            reflectionMat.m12 = (-2F * plane[1] * plane[2]);
            reflectionMat.m13 = (-2F * plane[3] * plane[1]);

            reflectionMat.m20 = (-2F * plane[2] * plane[0]);
            reflectionMat.m21 = (-2F * plane[2] * plane[1]);
            reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
            reflectionMat.m23 = (-2F * plane[3] * plane[2]);

            reflectionMat.m30 = 0F;
            reflectionMat.m31 = 0F;
            reflectionMat.m32 = 0F;
            reflectionMat.m33 = 1F;
        }

        [ContextMenu("Test")]
        void Test()
        {

        }
    }
}
