using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MynjenDook
{
    [AddComponentMenu("MynjenDook/MdUserParams")]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MdWater))]
    public class MdUserParams : MonoBehaviour
    {
        [Serializable]
        public struct Param
        {
            public string m_Name;
            public float m_Value;
            public bool m_Shader;
            public Param(string name, float value, bool shader)
            {
                m_Name = name;
                m_Value = value;
                m_Shader = shader;
            }
        }
        public List<Param> Params = new List<Param>();

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

        private void InitWaterParams()
        {
            if (Params.Count > 0) return;
            Params.Add(new Param("WaterColorR", 0.12f, false));
            Params.Add(new Param("WaterColorG", 0.22f, false));
            Params.Add(new Param("WaterColorB", 0.29f, false));
            Params.Add(new Param("DeltaUV_X", 0.01f, false));
            Params.Add(new Param("DeltaUV_Y", 0.03f, false));
            Params.Add(new Param("DeltaUV_Z", -0.005f, false));
            Params.Add(new Param("DeltaUV_W", 0.015f, false));
            Params.Add(new Param("gw_fNormalUVScale0", 15.0f, true)); // 整数就可以了
            Params.Add(new Param("gw_fNormalUVScale1", 15.0f, true)); // 整数就可以了
            Params.Add(new Param("gw_fNormalRatio", 0.5f, true));
            Params.Add(new Param("gw_fNormalNoise", 0.05f, true));
            Params.Add(new Param("gw_fNoNoiseScreen", 0.03f, true));
            Params.Add(new Param("gw_fRefractRadius", 5000f, true));
            Params.Add(new Param("gw_fRefractMinAlpha", 0.82f, true));
            Params.Add(new Param("gw_fFresnelBias", 0.60f, true));
            Params.Add(new Param("gw_fFresnelScale", 1.35f, true)); // 1.8
            Params.Add(new Param("gw_fFresnelPower", 1.218f, true)); // 1.31
            Params.Add(new Param("gw_fCausticsUVScale", 30.0f, true));
            Params.Add(new Param("gw_fCausticsDepth", 270.0f, true));
            Params.Add(new Param("CausticsSpeed", 0.6f, false));
            Params.Add(new Param("gw_fWaveVertexSpacing", 12.0f, true)); // 27
            Params.Add(new Param("gw_fWaveRatio", 0.28f, true)); // 5
            Params.Add(new Param("WaveHeightDiv", 55.0f, false));
            Params.Add(new Param("ClipHeight", 80.0f, false));
            Params.Add(new Param("SunColorR", 1.2f, false));
            Params.Add(new Param("SunColorG", 0.4f, false));
            Params.Add(new Param("SunColorB", 0.1f, false));
            Params.Add(new Param("gw_fSunFactor", 1.5f, true));
            Params.Add(new Param("gw_fSunPower", 220.0f, true));
            Params.Add(new Param("SunFaceTo", 90.0f, false));
            Params.Add(new Param("SunHeight", 8.3f, false));
            Params.Add(new Param("gw_fNoiseUVScale", 128f, true));
            Params.Add(new Param("gw_fNoiseWaveHeightDiv", 60f, true));
            Params.Add(new Param("gw_fSunNormalSpacing", 0.007f, true));
            Params.Add(new Param("gw_fSunNormalRatio", 0.86f, true));
        }


    }
}
