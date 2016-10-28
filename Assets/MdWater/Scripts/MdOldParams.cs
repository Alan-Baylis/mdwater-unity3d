using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MynjenDook
{
    [AddComponentMenu("MynjenDook/MdOldParams")]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MdWater))]
    public class MdOldParams : MonoBehaviour
    {
        public enum pParameters
        {
            p_fStrength = 0,
            p_fFalloff,
            p_fScale,
            p_bSmooth,
            p_fReflRefrStrength,
            p_iOctaves,
            p_fLODbias,
            p_fAnimspeed,
            p_fTimemulti,
            p_bPaused,
            p_bAsPoints,
            p_fElevation,
            p_bDisplayTargets,
            p_fSunPosTheta,
            p_fSunPosAlpha,
            p_fSunShininess,
            p_fSunStrength,
            p_fWaterColourR,
            p_fWaterColourG,
            p_fWaterColourB,
            p_bDisplace,
            p_bDrawDuckie,
            p_bDrawIsland,
            p_bDiffuseRefl,
            numParameters
        };


        [Serializable]
        public struct Param
        {
            public string m_Name;
            public string m_Desc;
            public int m_iValue;
            public float m_fValue;
            public bool m_bValue;
            public Param(string name, string desc, int value)
            {
                m_Name = name;
                m_Desc = desc;
                m_iValue = value;
                m_fValue = 0f;
                m_bValue = false;
            }
            public Param(string name, string desc, float value)
            {
                m_Name = name;
                m_Desc = desc;
                m_iValue = 0;
                m_fValue = value;
                m_bValue = false;
            }
            public Param(string name, string desc, bool value)
            {
                m_Name = name;
                m_Desc = desc;
                m_iValue = 0;
                m_fValue = 0;
                m_bValue = value;
            }
        }
        public Param[] Params;

        void Awake()
        {
            InitWaterParams();
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
            InitWaterParams();
        }

        public void loadpreset(int n)
        {
            switch (n)
            {
                case 1:
                    SetFloat(pParameters.p_fScale, 0.38f);
                    SetFloat(pParameters.p_fStrength, 0.9f);
                    SetFloat(pParameters.p_fFalloff, 0.607f);
                    SetFloat(pParameters.p_fWaterColourR, 0.07f);
                    SetFloat(pParameters.p_fWaterColourG, 0.11f);
                    SetFloat(pParameters.p_fWaterColourB, 0.11f);
                    SetFloat(pParameters.p_fLODbias, 0.0f);
                    SetBool (pParameters.p_bDiffuseRefl, false);
                    break;
                case 2:
                    SetFloat(pParameters.p_fScale, 0.38f);
                    SetFloat(pParameters.p_fStrength, 4.0f);
                    SetFloat(pParameters.p_fFalloff, 0.47f);
                    SetFloat(pParameters.p_fWaterColourR, 0.13f);
                    SetFloat(pParameters.p_fWaterColourG, 0.19f);
                    SetFloat(pParameters.p_fWaterColourB, 0.22f);
                    SetFloat(pParameters.p_fLODbias, 4.5f);
                    SetBool (pParameters.p_bDiffuseRefl, false);
                    break;
                case 3:
                    SetFloat(pParameters.p_fScale, 0.38f);
                    SetFloat(pParameters.p_fStrength, 7.0f);
                    SetFloat(pParameters.p_fFalloff, 0.53f);
                    SetFloat(pParameters.p_fWaterColourR, 0.12f);
                    SetFloat(pParameters.p_fWaterColourG, 0.22f);
                    SetFloat(pParameters.p_fWaterColourB, 0.29f);
                    SetFloat(pParameters.p_fLODbias, 10.0f);
                    SetBool (pParameters.p_bDiffuseRefl, true);
                    SetFloat(pParameters.p_fSunPosAlpha, 2.91f);
                    SetFloat(pParameters.p_fSunPosTheta, 0.36f);
                    SetFloat(pParameters.p_fSunShininess, 1263.0f);
                    SetFloat(pParameters.p_fSunStrength, 208.0f);
                    break;
                case 4:
                    SetFloat(pParameters.p_fScale, 0.38f);
                    SetFloat(pParameters.p_fStrength, 4.0f);
                    SetFloat(pParameters.p_fFalloff, 0.47f);
                    SetFloat(pParameters.p_fWaterColourR, 0.13f);
                    SetFloat(pParameters.p_fWaterColourG, 0.19f);
                    SetFloat(pParameters.p_fWaterColourB, 0.22f);
                    SetFloat(pParameters.p_fLODbias, 4.5f);
                    SetBool (pParameters.p_bDiffuseRefl, true);
                    break;
                case 5:
                    SetFloat(pParameters.p_fScale, 0.197f);
                    SetFloat(pParameters.p_fStrength, 12.9f);
                    SetFloat(pParameters.p_fFalloff, 0.467f);
                    SetFloat(pParameters.p_fWaterColourR, 0.12f);
                    SetFloat(pParameters.p_fWaterColourG, 0.20f);
                    SetFloat(pParameters.p_fWaterColourB, 0.24f);
                    SetFloat(pParameters.p_fLODbias, 0.0f);
                    SetFloat(pParameters.p_fSunPosAlpha, 2.91f);
                    SetFloat(pParameters.p_fSunPosTheta, 0.88f);
                    SetFloat(pParameters.p_fSunShininess, 1263.0f);
                    SetFloat(pParameters.p_fSunStrength, 5270.0f);
                    SetBool (pParameters.p_bDiffuseRefl, true);
                    break;
                case 6:
                    SetFloat(pParameters.p_fScale, 0.38f);
                    SetFloat(pParameters.p_fStrength, 11.3f);
                    SetFloat(pParameters.p_fFalloff, 0.56f);
                    SetFloat(pParameters.p_fWaterColourR, 0.17f);
                    SetFloat(pParameters.p_fWaterColourG, 0.27f);
                    SetFloat(pParameters.p_fWaterColourB, 0.26f);
                    SetFloat(pParameters.p_fLODbias, 0.0f);
                    SetFloat(pParameters.p_fSunPosAlpha, 2.91f);
                    SetFloat(pParameters.p_fSunPosTheta, 0.256f);
                    SetFloat(pParameters.p_fSunShininess, 700.0f);
                    SetFloat(pParameters.p_fSunStrength, 200.85f);
                    SetBool (pParameters.p_bDiffuseRefl, false);
                    break;
            }
        }

        private void InitWaterParams()
        {
            if (Params != null && Params.Length > 0) return;

            Params = new Param[(int)pParameters.numParameters];

            Params[(int)pParameters.p_fStrength] = new Param(pParameters.p_fStrength.ToString(), "Noise strength", 0.9f);
            Params[(int)pParameters.p_bDisplace] = new Param(pParameters.p_bDisplace.ToString(), "Toggle displacement", true);
            Params[(int)pParameters.p_bSmooth] = new Param(pParameters.p_bSmooth.ToString(), "Smooth heightmap", true);
            Params[(int)pParameters.p_fReflRefrStrength] = new Param(pParameters.p_fReflRefrStrength.ToString(), "Reflection/Refraction strength", 0.1f);
            Params[(int)pParameters.p_iOctaves] = new Param(pParameters.p_iOctaves.ToString(), "Octaves", (int)8);
            Params[(int)pParameters.p_fScale] = new Param(pParameters.p_fScale.ToString(), "Noise scale", 0.38f);
            Params[(int)pParameters.p_fFalloff] = new Param(pParameters.p_fFalloff.ToString(), "Noise falloff", 0.607f);
            Params[(int)pParameters.p_fAnimspeed] = new Param(pParameters.p_fAnimspeed.ToString(), "Animation speed", 1.4f);
            Params[(int)pParameters.p_fTimemulti] = new Param(pParameters.p_fTimemulti.ToString(), "Animation multi", 1.27f);
            Params[(int)pParameters.p_bPaused] = new Param(pParameters.p_bPaused.ToString(), "Pause animation", false);
            Params[(int)pParameters.p_bDisplayTargets] = new Param(pParameters.p_bDisplayTargets.ToString(), "Display render targets(D)", false);
            Params[(int)pParameters.p_fSunPosAlpha] = new Param(pParameters.p_fSunPosAlpha.ToString(), "Sun location horizontal", 1.38f);
            Params[(int)pParameters.p_fSunPosTheta] = new Param(pParameters.p_fSunPosTheta.ToString(), "Sun location vertical", 1.09f);
            Params[(int)pParameters.p_fSunShininess] = new Param(pParameters.p_fSunShininess.ToString(), "Sun shininess", 84.0f);
            Params[(int)pParameters.p_fSunStrength] = new Param(pParameters.p_fSunStrength.ToString(), "Sun strength", 12.0f);
            Params[(int)pParameters.p_bAsPoints] = new Param(pParameters.p_bAsPoints.ToString(), "Render as points", false);
            Params[(int)pParameters.p_fLODbias] = new Param(pParameters.p_fLODbias.ToString(), "Mipmap LOD Bias", 0.0f);
            Params[(int)pParameters.p_fElevation] = new Param(pParameters.p_fElevation.ToString(), "Projector elevation", 7.0f);
            Params[(int)pParameters.p_fWaterColourR] = new Param(pParameters.p_fWaterColourR.ToString(), "water color Red", 0.17f);
            Params[(int)pParameters.p_fWaterColourG] = new Param(pParameters.p_fWaterColourG.ToString(), "water color Green", 0.27f);
            Params[(int)pParameters.p_fWaterColourB] = new Param(pParameters.p_fWaterColourB.ToString(), "water color Blue", 0.26f);
            Params[(int)pParameters.p_bDrawDuckie] = new Param(pParameters.p_bDrawDuckie.ToString(), "Render Duckie", true);
            Params[(int)pParameters.p_bDrawIsland] = new Param(pParameters.p_bDrawIsland.ToString(), "Render Island", false);
            Params[(int)pParameters.p_bDiffuseRefl] = new Param(pParameters.p_bDiffuseRefl.ToString(), "Diffuse sky reflection", false);
        }

        public int GetInt(pParameters p)
        {
            return Params[(int)p].m_iValue;
        }
        public float GetFloat(pParameters p)
        {
            return Params[(int)p].m_fValue;
        }
        public bool GetBool(pParameters p)
        {
            return Params[(int)p].m_bValue;
        }

        public void SetInt(pParameters p, int value)
        {
            Params[(int)p].m_iValue = value;
        }
        public void SetFloat(pParameters p, float value)
        {
            Params[(int)p].m_fValue = value;
        }
        public void SetBool(pParameters p, bool value)
        {
            Params[(int)p].m_bValue = value;
        }

        [ContextMenu("Test")]
        void Test()
        {
            InitWaterParams();
        }
    }
}
