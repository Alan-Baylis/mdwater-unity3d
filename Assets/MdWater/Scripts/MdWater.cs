using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace MynjenDook
{
    [AddComponentMenu("MynjenDook/MdWater")]
    [DisallowMultipleComponent]
    public class MdWater : MonoBehaviour
    {
        public MdPredefinition predefinition = null;                        // components
        public MdOldParams oldparams = null;
        public MdUserParams userparams = null;
        public MdTexturing texturing = null;
        public MdSurface surface = null;
        public MdReflection reflect = null;
        public MdRefraction refract = null;
        [HideInInspector]
        public Camera m_camera;                                             // camera, init时赋值一次

        private int m_LastReflectFrameCount = 0;                            // 记录上一次做反射、折射的camera和帧号，避免冗余做反射贴图
        private int m_LastRefractFrameCount = 0;                            //
        private Camera m_LastReflectCamera;                                 //
        private Camera m_LastRefractCamera;                                 //

        public enum WaterMode
        {
            Simple = 0,
            Reflective = 1,
            Refractive = 2,
        };
        public WaterMode m_WaterMode = WaterMode.Refractive;                // 普通、仅反射、反射+折射
        private WaterMode m_HardwareWaterSupport = WaterMode.Refractive;    // 硬件支持最大的mode

        private int m_maxProfile = 0;                                       // 设备支持最大profile
        private int m_profile = 0;                                          // 当前profile

        private GameObject[] ProfileNodes;                                  // 不同profile的水体容器结点
        public Material material = null;                                    // 水材质

        public List<GameObject> ReflectionIgnoreList = null;                // 反射过滤列表
        private Queue<bool> ReflectionIgnoreSavedActive = null;
        public List<GameObject> RefractionIgnoreList = null;                // 折射过滤列表
        private Queue<bool> RefractionIgnoreSavedActive = null;

        public Transform Pivot = null;                                      // 水体中心
        [HideInInspector]
        private Vector4 m_kTexOffset = new Vector4(0.0f, 0.0f, 0.2f, 0.3f); // uv动画
        public bool m_Refract = true;                                       // 折射
        public bool m_FogEnable = true;                                     // 雾
        public float m_FogDepth = 1;                                        // 雾深度
        [HideInInspector]
        public float m_ClipAdjustHeight = 0;                                // camera裁剪高度微调
        public bool m_FollowCamera = true;                                  // 自动跟随camera

        struct displacement
        {
            public float waterx;
            public float watery;
            public int noisex;
            public int noisey;
        };
        private displacement m_displacement = new displacement();           // noise跳动



        ////////////////////////////////////////////////////////////////
        // 测试
        private int m_framecount = 0;
        public int FrameCount { get { return m_framecount; } }
        private float m_lasttime = 0;
        public float LastTime { get { return m_lasttime; } }
        public Renderer TestNoiseView;
        public Renderer TestReflectView;
        public Renderer TestRefractView;


        void Awake()
        {
            Initialize();
        }

        void Start()
        {
        }

        void OnDestroy()
        {

        }

        void OnEnable()
        {
        }
        void OnDisable()
        {
        }

        public void Initialize()
        {
            // Actual water rendering mode depends on both the current setting AND
            // the hardware support. There's no point in rendering refraction textures
            // if they won't be visible in the end.
            m_HardwareWaterSupport = FindHardwareWaterSupport();

            predefinition = GetComponent<MdPredefinition>();
            oldparams = GetComponent<MdOldParams>();
            userparams = GetComponent<MdUserParams>();
            texturing = GetComponent<MdTexturing>();
            surface = GetComponent<MdSurface>();
            reflect = GetComponent<MdReflection>();
            refract = GetComponent<MdRefraction>();
            //reflection组件搬到submesh上
            SetupCamera(Camera.main);

            predefinition.Initialize();
            oldparams.Initialize();
            userparams.Initialize();
            userparams.Water = this;
            texturing.Initialize();
            m_maxProfile = CheckHardware();
            surface.Initialize(Vector3.zero, Vector3.up, m_maxProfile);
            reflect.Initialize();
            reflect.Water = this;
            refract.Initialize();
            refract.Water = this;

            BuildWaterMeshes();

            ReflectionIgnoreSavedActive = new Queue<bool>();
            RefractionIgnoreSavedActive = new Queue<bool>();
        }

        void Update()
        {
            PreRendering();

            // update xzh water params
            userparams.UpdateWaterParams();

            // kuangsihao: 更新uv
            float fDeltaUV_X = userparams.GetFloat(MdUserParams.UserParams.DeltaUV_X);
            float fDeltaUV_Y = userparams.GetFloat(MdUserParams.UserParams.DeltaUV_Y);
            float fDeltaUV_Z = userparams.GetFloat(MdUserParams.UserParams.DeltaUV_Z);
            float fDeltaUV_W = userparams.GetFloat(MdUserParams.UserParams.DeltaUV_W);
            fDeltaUV_X = (float)MathUtil.Clamp(fDeltaUV_X, -1, 1);
            fDeltaUV_Y = (float)MathUtil.Clamp(fDeltaUV_Y, -1, 1);
            fDeltaUV_Z = (float)MathUtil.Clamp(fDeltaUV_Z, -1, 1);
            fDeltaUV_W = (float)MathUtil.Clamp(fDeltaUV_W, -1, 1);
            m_kTexOffset += new Vector4(fDeltaUV_X, fDeltaUV_Y, fDeltaUV_Z, fDeltaUV_W) * Time.deltaTime;

            // 根据太阳光faceto和height角，更新shader里的SunLightDirection
            float fSunFaceTo = userparams.GetFloat(MdUserParams.UserParams.SunFaceTo);
            float ffaceto = (float)(fSunFaceTo * (2 * Math.PI) / 360f);
            float sx = (float)(-Math.Sin(ffaceto));
            float sy = (float)Math.Cos(ffaceto);
            float fSunHeight = userparams.GetFloat(MdUserParams.UserParams.SunHeight);
            float ffaceheight = (float)(fSunHeight * (2 * Math.PI) / 360f);
            float sz = (float)(-Math.Sin(ffaceheight));
            Vector4 kSunDir = new Vector4(sx, sy, sz, 0);
            material.SetVector("gw_SunLightDir", kSunDir);

            // 折射
            float fRefract = m_Refract ? 1.0f : 0.0f;
            material.SetFloat("gw_bRefract", fRefract);

            // 雾
            m_FogEnable = RenderSettings.fog;
            float fbFogEnable = m_FogEnable ? 1.0f : 0.0f;
            material.SetFloat("gw_fFogEnable", fbFogEnable);
            Color kFogInfo = new Color(RenderSettings.fogColor.r, RenderSettings.fogColor.g, RenderSettings.fogColor.b, m_FogDepth);
            material.SetColor("gw_fFog", kFogInfo);

            // 根据camera的仰角，给clipheight增加一个值以修正看到高浪头的白色
            float fAdjust = CalcClipHeightAdjust();
            //printf("%f\r\n", fAdjust);
            m_ClipAdjustHeight = userparams.GetFloat(MdUserParams.UserParams.ClipHeight);
            m_ClipAdjustHeight += fAdjust;
            float fWaveHeightDiv = userparams.GetFloat(MdUserParams.UserParams.WaveHeightDiv);
            // 水高度参数
            material.SetFloat("gw_fNoiseWaveHeightDiv", fWaveHeightDiv);

            // proj grid常数
            float f_np_size = predefinition.np_size;
            float f_waterlv2 = predefinition.waterlv2;
            material.SetFloat("gw_np_size", f_np_size);
            material.SetFloat("gw_waterlv2", f_waterlv2);

            // 根据camera的lookat位置，更新水结点的位置：
            if (m_FollowCamera)
            {
                Debug.Assert(Pivot != null);
                Vector3 posLookAt = Pivot.transform.position;
                CalcNoiseDisplacement(posLookAt.x, posLookAt.z, ref m_displacement.waterx, ref m_displacement.watery, ref m_displacement.noisex, ref m_displacement.noisey);
                float fNoiseDisplaceX = m_displacement.noisex;
                float fNoiseDisplaceY = m_displacement.noisey;
                material.SetFloat("gw_fNoiseDisplacementX", fNoiseDisplaceX);
                material.SetFloat("gw_fNoiseDisplacementY", fNoiseDisplaceY);
                Vector3 kWaterPos = new Vector3(m_displacement.waterx, 0, m_displacement.watery);
                this.transform.position = kWaterPos;
            }

            // test
            //printf("camerax: %.1f  cameray: %.1f  waterx: %.1f  watery: %.1f  noisex: %d  noisey: %d\r\n", m_pkCamera->GetTranslate().x, m_pkCamera->GetTranslate().y, waterx, watery, noisex, noisey);

            // 更新kernel
            /*
            NiNode* pkSubNode = GetSubNode(GetProfile());
            EE_ASSERT(pkSubNode);
            for (unsigned int i = 0; i < pkSubNode->GetArrayCount(); i++)
            {
                NiMesh* pkWaterMesh = NiDynamicCast(NiMesh, pkSubNode->GetAt(i));
                if (pkWaterMesh == NULL)
                    continue;

                if (pkWaterMesh->GetModifierCount() == 1)
                {
                    EE_ASSERT(pkWaterMesh->GetModifierCount() == 1);
                    XzhWaterModifier* pkXzhModifier = NiDynamicCast(XzhWaterModifier, pkWaterMesh->GetModifierAt(0));
                    EE_ASSERT(pkXzhModifier);
                    pkXzhModifier->SetWaveHeightDiv(m_fWaveHeightDiv * 1.5f);
                    if (GetFollowCamera())
                    {
                        pkXzhModifier->SetNoiseDisplacement(m_displacement.noisex, m_displacement.noisey);
                    }
                }
            }*/

            // 摄像机:
            //camera_mouse->update();
            //g_camera->m_pkCamera->Update(fAccumTime);
            //g_camera->updateFromGamebryo();

            //Debug.LogErrorFormat("md update, frame: {0}  time: {1}", m_framecount, Time.time - m_lasttime);
            surface.MdUpdate();

            m_framecount++;
            m_lasttime = Time.time;
        }

        private void PreRendering()
        {
            // keyword
            if (m_profile == 0)
                Shader.EnableKeyword("_WPROFILE_LOW");
            else
                Shader.DisableKeyword("_WPROFILE_LOW");
            if (texturing.m_bWireframe)
                material.EnableKeyword("WIREFRAME");
            // water mode
            WaterMode mode = SetupWaterModeKeyword();
            reflect.enabled = mode >= WaterMode.Reflective;
            refract.enabled = mode >= WaterMode.Refractive;


            // update shader constants
            Vector4 cam_loc = m_camera.transform.position;
            cam_loc.w = 1;
            material.SetVector("gw_EyePos", cam_loc);
            material.SetVector("gw_TexOffsets", m_kTexOffset);
            material.SetFloat("gw_fFrameTime", Time.time);

            float waterR = userparams.GetFloat(MdUserParams.UserParams.WaterColorR);
            float waterG = userparams.GetFloat(MdUserParams.UserParams.WaterColorG);
            float waterB = userparams.GetFloat(MdUserParams.UserParams.WaterColorB);
            Color kWaterColor = new Color(waterR, waterG, waterB);
            material.SetColor("gw_WaterColor", kWaterColor);

            float sunR = userparams.GetFloat(MdUserParams.UserParams.SunColorR);
            float sunG = userparams.GetFloat(MdUserParams.UserParams.SunColorG);
            float sunB = userparams.GetFloat(MdUserParams.UserParams.SunColorB);
            Color kSunColor = new Color(sunR, sunG, sunB);
            material.SetColor("gw_SunColor", kSunColor);

            // caustics map和normal map每帧换
            Texture2D pkCausticsTexture = texturing.GetCurrentCausticsTexture();
            Texture2D pkNormalTexture = texturing.GetCurrentNormalTexture();
            float fCaustics = texturing.m_bCaustics ? 1.0f : 0.0f;
            material.SetFloat("gw_fCaustics", fCaustics);

            // grey
            float fGrey = texturing.m_bGrey ? 1.0f : 0.0f;
            material.SetFloat("gw_fGrey", fGrey);

            // texture map: reflect refract noise在其他地方
            material.SetTexture("_NormalTex1", pkNormalTexture);
            material.SetTexture("_NormalTex0", texturing.m_spTexNormal0);
            material.SetTexture("_HeightTex", texturing.m_spTexHeight);
            material.SetTexture("_CausticsTex", pkCausticsTexture);
        }

        public void SetupCamera(Camera c)
        {
            Debug.Assert(c != null);
            m_camera = c;
        }

        private void BuildWaterMeshes()
        {
            MMMeshCreator.Water = this;

            ProfileNodes = new GameObject[m_maxProfile + 1];
            for (int i = 0; i < 3; i++)
            {
                ProfileNodes[i] = transform.FindChild("profile" + i).gameObject;
            }

            for (int profile = 0; profile <= m_maxProfile; profile++)
            {
                SetProfile(profile);
                MMMeshCreator.CreateLodMesh(profile, ref ProfileNodes[profile]);
            }
            SetProfile(m_maxProfile);


            /*
             // grey when player dead
	        g_Application->AddMeshGreyExtraDataForNode(m_spWaterNode);

	        //HRESULT hr = InitD3D(hWnd);
	        //XZH_CHECK_ERROR(SUCCEEDED(hr));

	        //hr = InitGeometry();
	        //XZH_CHECK_ERROR(SUCCEEDED(hr));

	        m_kTexOffset = NiPoint4(0.0f, 0.0f, 0.2f, 0.3f);

	        m_fDeltaUV_X = 0.01f;
	        m_fDeltaUV_Y = 0.03f;
	        m_fDeltaUV_Z = -0.005f;
	        m_fDeltaUV_W = 0.015f;
    
             */
        }

        private int CheckHardware()
        {
            return 2; // todo.ksh: 
        }

        private void SetProfile(int profile)
        {
	        if (profile > m_maxProfile)
		        return;
            m_profile = profile;
            surface.m_noiseMaker.m_profile = profile;

            predefinition.UpdatePredefinitions(profile);
	        for (int i = 0; i< 3; i++)
	        {
                ProfileNodes[i].SetActive(i == profile);
            }

            //GetWater()->Update(0); // todo:kuangsihao
            //m_spXzhWaterParam->SetValue("gw_fNormalUVScale1", profile == 0 ? 35.0f : 10.0f);
        }

        private WaterMode FindHardwareWaterSupport()
        {
            if (!SystemInfo.supportsRenderTextures)
                return WaterMode.Simple;

            string mode = material.GetTag("WATERMODE", false);
            if (mode == "Refractive")
                return WaterMode.Refractive;
            if (mode == "Reflective")
                return WaterMode.Reflective;

            return WaterMode.Simple;
        }
        private WaterMode GetWaterMode()
        {
            if (m_HardwareWaterSupport < m_WaterMode)
                return m_HardwareWaterSupport;
            else
                return m_WaterMode;
        }
        private WaterMode SetupWaterModeKeyword()
        {
            WaterMode mode = GetWaterMode();
            switch (mode)
            {
                case WaterMode.Simple:
                    Shader.EnableKeyword("WATER_SIMPLE");
                    Shader.DisableKeyword("WATER_REFLECTIVE");
                    Shader.DisableKeyword("WATER_REFRACTIVE");
                    break;
                case WaterMode.Reflective:
                    Shader.DisableKeyword("WATER_SIMPLE");
                    Shader.EnableKeyword("WATER_REFLECTIVE");
                    Shader.DisableKeyword("WATER_REFRACTIVE");
                    break;
                case WaterMode.Refractive:
                    Shader.DisableKeyword("WATER_SIMPLE");
                    Shader.DisableKeyword("WATER_REFLECTIVE");
                    Shader.EnableKeyword("WATER_REFRACTIVE");
                    break;
            }
            return mode;
        }

        public void BeginReflect(bool bBegin)
        {
            if (ReflectionIgnoreList == null)
                return;

            if (bBegin)
            {
                ReflectionIgnoreSavedActive.Clear();
                foreach (GameObject o in ReflectionIgnoreList)
                {
                    ReflectionIgnoreSavedActive.Enqueue(o.activeSelf);
                    o.SetActive(false);
                }
            }
            else
            {
                foreach (GameObject o in ReflectionIgnoreList)
                {
                    bool oldActiva = ReflectionIgnoreSavedActive.Dequeue();
                    o.SetActive(oldActiva);
                }
            }
        }

        public void BeginRefract(bool bBegin)
        {
            if (RefractionIgnoreList == null)
                return;

            if (bBegin)
            {
                RefractionIgnoreSavedActive.Clear();
                foreach (GameObject o in RefractionIgnoreList)
                {
                    RefractionIgnoreSavedActive.Enqueue(o.activeSelf);
                    o.SetActive(false);
                }
            }
            else
            {
                foreach (GameObject o in RefractionIgnoreList)
                {
                    bool oldActiva = RefractionIgnoreSavedActive.Dequeue();
                    o.SetActive(oldActiva);
                }
            }
        }

        public void _OnWillRenderObject(Camera camera) // 函数名加下划线避免重名而被unity错误调到
        {
            int curFrameCount = Time.frameCount;
            if (m_LastReflectCamera != camera || m_LastReflectFrameCount != curFrameCount) // 避免冗余做反射贴图
            {
                reflect.OnWillRenderObject();
                m_LastReflectCamera = camera;
                m_LastReflectFrameCount = curFrameCount;
            }
            if (m_LastRefractCamera != camera || m_LastRefractFrameCount != curFrameCount) // 避免冗余做折射贴图
            {
                refract.OnWillRenderObject();
                m_LastRefractCamera = camera;
                m_LastRefractFrameCount = curFrameCount;
            }
        }

        private void CalcNoiseDisplacement(float centerx, float centery, ref float waterx, ref float watery, ref int noisex, ref int noisey)
        {
            // 先计算水结点的位置
            waterx = 0;
            watery = 0;

            while (centerx >= predefinition.waterl0)
            {
                centerx -= predefinition.waterl0;
                waterx += predefinition.waterl0;
            }
            while (centery >= predefinition.waterl0)
            {
                centery -= predefinition.waterl0;
                watery += predefinition.waterl0;
            }

            while (centerx < 0.0f)
            {
                centerx += predefinition.waterl0;
                waterx -= predefinition.waterl0;
            }
            while (centery < 0.0f)
            {
                centery += predefinition.waterl0;
                watery -= predefinition.waterl0;
            }

            // 再计算noise偏移
            noisex = 0;
            noisey = 0;
            float fVertDis = predefinition.waterl2 / (predefinition.waterlv2 - 1); // 水移动单位为ring2的顶点间距离

            while (centerx >= fVertDis)
            {
                centerx -= fVertDis;
                waterx += fVertDis;
                noisex++;
            }
            while (centery >= fVertDis)
            {
                centery -= fVertDis;
                watery += fVertDis;
                noisey++;
            }
        }

        private float CalcClipHeightAdjust()
        {
            Vector3 dir = m_camera.transform.eulerAngles;

            float pitch = (float)(Math.Asin(dir.z / Math.Sqrt(dir.x * dir.x + dir.y * dir.y)) * 180f / Math.PI);

            float f1 = 12000; // clipheight
            float f2 = 500;
            float pitch1 = 5; // 角度
            float pitch2 = 2;

            if (pitch > 0) // 向上看
            {
                return f1;
            }
            else
            {
                // 向下看
                pitch = -pitch;

                if (pitch > pitch1)
                    return 0;
                else if (pitch > pitch2)
                    return Mathf.Lerp((pitch - pitch1) / (pitch2 - pitch1), 0, f2);
                else if (pitch > 0)
                    return Mathf.Lerp(pitch / pitch2, f1, f2);
                else
                    return 0;
            }
        }


        [ContextMenu("Test")]
        void Test()
        {
            //SetProfile(0);
            EditorUtility.SetSelectedWireframeHidden(GetComponent<Renderer>(), true);
        }
    }
}

