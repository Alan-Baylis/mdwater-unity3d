using UnityEngine;
using System.Collections;

namespace MynjenDook
{
    // This is in fact just the Water script from Pro Standard Assets,
    // just with refraction stuff removed.

    [AddComponentMenu("MynjenDook/MdRefraction")]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MdWater))]
    public class MdRefraction : MonoBehaviour
    {
        [HideInInspector]
        public MdWater Water = null;

        public bool m_DisablePixelLights = true;
        public int m_TextureSize = 1024;
        public float m_ClipPlaneOffset = 0.01f; // 0.07f

        public LayerMask m_RefractLayers = -1;

        //private Hashtable m_RefractionCameras = new Hashtable(); // Camera -> Camera table
        private Camera m_RefractCamera = null;
        private static string m_strRefractCameraName = "mmwater_refract_camera";

        private RenderTexture m_RefractionTexture = null;
        private int m_OldRefractionTextureSize = 0;

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
        // camera. We render refractions and do other updates here.
        // Because the script executes in edit mode, refractions for the scene view
        // camera will just work!
        public void OnWillRenderObject()
        {
            //Debug.LogWarningFormat("mesh will render : {0}", Time.frameCount);

            if (!enabled)
                return;

            Camera cam = Camera.current;
            if (!cam || cam.name == m_strRefractCameraName)
                return;

            // Safeguard from recursive refractions.        
            if (s_InsideRendering)
                return;
            s_InsideRendering = true;

            CheckMirrorObjects();

            // find out the refraction plane: position and normal in world space
            Vector3 pos = transform.position;
            Vector3 normal = transform.up;

            // Optionally disable pixel lights for refraction
            int oldPixelLightCount = QualitySettings.pixelLightCount;
            if (m_DisablePixelLights)
                QualitySettings.pixelLightCount = 0;

            UpdateCameraModes(cam, m_RefractCamera);

            m_RefractCamera.worldToCameraMatrix = cam.worldToCameraMatrix;

            // Setup oblique projection matrix so that near plane is our reflection
            // plane. This way we clip everything below/above it for free.
            Vector4 clipPlane = CameraSpacePlane(m_RefractCamera, pos, normal, -1.0f);
            m_RefractCamera.projectionMatrix = cam.CalculateObliqueMatrix(clipPlane);

            int layerWater = LayerMask.NameToLayer("Water");
            int layerUI    = LayerMask.NameToLayer("UI");
            m_RefractCamera.cullingMask = ~(1 << layerWater) & m_RefractLayers.value; // never render water layer, 4
            m_RefractCamera.cullingMask = ~(1 << layerUI) & m_RefractLayers.value;    // never render UI    layer, 5

            Water.BeginRefract(true);
            m_RefractCamera.targetTexture = m_RefractionTexture;
            m_RefractCamera.transform.position = cam.transform.position;
            m_RefractCamera.transform.rotation = cam.transform.rotation;
            m_RefractCamera.Render();
            Water.BeginRefract(false);
            // shader map
            Water.material.SetTexture("_RefractionTex", m_RefractionTexture);
            // test: 查看
            if (cam == Camera.main)
            {
                if (Water.TestRefractView != null)
                    Water.TestRefractView.sharedMaterial.mainTexture = m_RefractionTexture;
            }

            // Restore pixel light count
            if (m_DisablePixelLights)
                QualitySettings.pixelLightCount = oldPixelLightCount;

            s_InsideRendering = false;
        }


        // Cleanup all the objects we possibly have created
        void OnDisable()
        {
            if (m_RefractionTexture)
            {
                DestroyImmediate(m_RefractionTexture); // todo.ksh: DestroyImmediate应该区分EDITOR吧？
                m_RefractionTexture = null;
            }
            //foreach( DictionaryEntry kvp in m_RefractionCameras )
            //	DestroyImmediate( ((Camera)kvp.Value).gameObject );
            //m_RefractionCameras.Clear();
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
            // Refraction render texture
            if (!m_RefractionTexture || m_OldRefractionTextureSize != m_TextureSize)
            {
                if (m_RefractionTexture)
                    DestroyImmediate(m_RefractionTexture);
                m_RefractionTexture = new RenderTexture(m_TextureSize, m_TextureSize, 16);
                m_RefractionTexture.name = "__WaterRefraction" + GetInstanceID();
                m_RefractionTexture.isPowerOfTwo = true;
                m_RefractionTexture.hideFlags = HideFlags.DontSave;
                m_OldRefractionTextureSize = m_TextureSize;
            }
        }
        private void MakeSureCamera()
        {
            if (m_RefractCamera != null)
                return;

            GameObject go = new GameObject("refract_camera", typeof(Camera), typeof(Skybox), typeof(FlareLayer)); // todo.ksh flare是需要的吗？
            Camera cam = go.GetComponent<Camera>();
            cam.name = m_strRefractCameraName;
            cam.enabled = false;
            cam.transform.position = transform.position;
            cam.transform.rotation = transform.rotation;
            go.hideFlags = HideFlags.HideAndDontSave;
            m_RefractCamera = cam;
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

        [ContextMenu("Test")]
        void Test()
        {

        }
    }
}
