using UnityEngine;
using System.Collections;

namespace MynjenDook
{
    [AddComponentMenu("MynjenDook/MdWater")]
    [DisallowMultipleComponent]
    public class MdWater : MonoBehaviour
    {
        public MdPredefinition predefinition = null;
        public MdOldParams oldparams = null;
        public MdUserParams userparams = null;
        public MdTexturing texturing = null;
        public MdSurface surface = null;

        int m_maxProfile = 0;
        public GameObject[] SubNodes;


        // Test:
        private int m_framecount = 0;
        public int FrameCount { get { return m_framecount; } }
        private float m_lasttime = 0;
        public float LastTime { get { return m_lasttime; } }
        public Renderer texviewRenderer;

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
            surface.Initialize(Vector3.zero, Vector3.up, (int)MdPredefinition.Macro.gridsize_x, (int)MdPredefinition.Macro.gridsize_y, m_maxProfile);


            BuildWaterMeshes();
        }

        private void BuildWaterMeshes()
        {
            SubNodes = new GameObject[m_maxProfile + 1];
            for (int i = 0; i < 3; i++)
            {
                SubNodes[i] = transform.FindChild("sub" + i).gameObject;
            }

            for (int i = 0; i <= m_maxProfile; i++)
            {
                SetProfile(i);
                CreateLodMesh(i);
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

        private void CreateLodMesh(int profile)
        {
            MeshFilter meshFilter = SubNodes[profile].GetComponent<MeshFilter>();
            meshFilter.mesh.Clear();
            Mesh newMesh = MMMeshCreator.CreateMesh(profile);
            meshFilter.mesh = newMesh;

            MdReflection reflection = SubNodes[profile].GetComponent<MdReflection>();
            reflection.Initialize();
        }

        private void SetProfile(int profile)
        {
	        if (profile > m_maxProfile)
		        return;
            surface.m_noiseMaker.m_profile = profile;

            predefinition.UpdatePredefinitions(profile);
	        for (int i = 0; i< 3; i++)
	        {
                SubNodes[i].SetActive(i == profile);
            }

            //GetWater()->Update(0); // todo:kuangsihao
            //m_spXzhWaterParam->SetValue("gw_fNormalUVScale1", profile == 0 ? 35.0f : 10.0f);
        }
        private int GetProfile()
        {
            return surface.m_noiseMaker.m_profile;
        }


        [ContextMenu("Test")]
        void Test()
        {
        }
    }
}

