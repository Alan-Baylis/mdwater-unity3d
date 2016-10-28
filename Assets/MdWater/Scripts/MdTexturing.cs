using UnityEngine;
using System.Collections;

namespace MynjenDook
{
    [AddComponentMenu("MynjenDook/MdTexturing")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MdWater))]
    public class MdTexturing : MonoBehaviour
    {
        public bool m_bCaustics = true;
        public float m_fCurrentCaustics = 0;
        public float m_fCausticsSpeed = 1;
        public Texture2D[] m_CausticsMaps;
        public Texture2D[] m_NormalMaps;
        public Texture2D m_spTexNormal0;            // "Data\water\textures\wave1.dds"
        public Texture2D m_spTexHeight;
        public float m_fCurrentNormal = 0;
        public bool m_bBackwardNormal = false;
        public bool m_bGrey = false;
        public bool m_bWireframe = false;
        

        void Awake()
        {

        }

        void Start()
        {
        }

        void OnDestroy()
        {

        }

        void Update()
        {
        }

        public void Initialize()
        {
            InitHeightMaps();
            InitCausticsMaps();
            InitNormapMaps();
        }

        public Texture2D GetCurrentCausticsTexture()
        {
            m_fCurrentCaustics = m_fCurrentCaustics + m_fCausticsSpeed;
            int index = (int)m_fCurrentCaustics;
            if (index >= (int)MdPredefinition.Macro.watercaustics)
            {
                m_fCurrentCaustics -= (float)MdPredefinition.Macro.watercaustics;
                index -= (int)MdPredefinition.Macro.watercaustics;
            }
            return m_CausticsMaps[index];
        }

        public Texture2D GetCurrentNormalTexture()
        {
            m_fCurrentNormal = m_fCurrentNormal + (m_bBackwardNormal ? -1.0f : 1.0f);
            int index = (int)m_fCurrentNormal;
            if (index >= (int)MdPredefinition.Macro.waternormals)
            {
                m_fCurrentNormal -= 2.0f;
                index -= 2;
                m_bBackwardNormal = true;
            }
            else if (index < 0)
            {
                m_fCurrentNormal += 2.0f;
                index += 2;
                m_bBackwardNormal = false;
            }
            return m_NormalMaps[index];
        }

        private void InitHeightMaps()
        {

        }
        private void InitCausticsMaps()
        {

        }
        private void InitNormapMaps()
        {

        }
    }
}
