using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

        int m_maxProfile = 0;                                               // 当前设备支持最大profile
        private GameObject[] ProfileNodes;                                  // 不同profile的水体容器结点
        public Material material = null;                                    // 水材质

        public List<GameObject> ReflectionIgnoreList = null;                // 反射过滤列表
        private Queue<bool> ReflectionIgnoreSavedActive = null;


        ////////////////////////////////////////////////////////////////
        // 测试
        private int m_framecount = 0;
        public int FrameCount { get { return m_framecount; } }
        private float m_lasttime = 0;
        public float LastTime { get { return m_lasttime; } }
        public Renderer TestNoiseView;

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

        void Update()
        {
            //Debug.LogErrorFormat("md update, frame: {0}  time: {1}", m_framecount, Time.time - m_lasttime);
            surface.MdUpdate();

            m_framecount++;
            m_lasttime = Time.time;
        }

        public void Initialize()
        {
            predefinition = GetComponent<MdPredefinition>();
            oldparams = GetComponent<MdOldParams>();
            userparams = GetComponent<MdUserParams>();
            texturing = GetComponent<MdTexturing>();
            surface = GetComponent<MdSurface>();
            //reflection组件搬到submesh上

            predefinition.Initialize();
            oldparams.Initialize();
            userparams.Initialize();
            texturing.Initialize();
            m_maxProfile = CheckHardware();
            surface.Initialize(Vector3.zero, Vector3.up, m_maxProfile);

            BuildWaterMeshes();

            ReflectionIgnoreSavedActive = new Queue<bool>();
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
            surface.m_noiseMaker.m_profile = profile;

            predefinition.UpdatePredefinitions(profile);
	        for (int i = 0; i< 3; i++)
	        {
                ProfileNodes[i].SetActive(i == profile);
            }

            //GetWater()->Update(0); // todo:kuangsihao
            //m_spXzhWaterParam->SetValue("gw_fNormalUVScale1", profile == 0 ? 35.0f : 10.0f);
        }
        private int GetProfile()
        {
            return surface.m_noiseMaker.m_profile;
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


        [ContextMenu("Test")]
        void Test()
        {
        }
    }
}

