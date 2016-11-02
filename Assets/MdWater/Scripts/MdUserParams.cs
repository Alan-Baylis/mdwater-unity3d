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
        public MdWater Water = null;

        public enum UserParams
        {
            WaterColorR = 0,
            WaterColorG,
            WaterColorB,
            DeltaUV_X,
            DeltaUV_Y,
            DeltaUV_Z,
            DeltaUV_W,
            gw_fNormalUVScale0,
            gw_fNormalUVScale1,
            gw_fNormalRatio,
            gw_fNormalNoise,
            gw_fNoNoiseScreen,
            gw_fRefractRadius,
            gw_fRefractMinAlpha,
            gw_fFresnelBias,
            gw_fFresnelScale,
            gw_fFresnelPower,
            gw_fCausticsUVScale,
            gw_fCausticsDepth,
            CausticsSpeed,
            gw_fWaveVertexSpacing,
            gw_fWaveRatio,
            WaveHeightDiv,
            ClipHeight,
            SunColorR,
            SunColorG,
            SunColorB,
            gw_fSunFactor,
            gw_fSunPower,
            SunFaceTo,
            SunHeight,
            gw_fNoiseUVScale,
            gw_fNoiseWaveHeightDiv,
            gw_fSunNormalSpacing,
            gw_fSunNormalRatio,

            numParameters
        };

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

        private void InitWaterParams()
        {
            if (Params != null && Params.Length > 0) return;

            Params = new Param[(int)UserParams.numParameters];

            Params[(int)UserParams.WaterColorR]            = new Param(UserParams.WaterColorR.ToString()           , 0.12f,   false);
            Params[(int)UserParams.WaterColorG]            = new Param(UserParams.WaterColorG.ToString()           , 0.22f,   false);
            Params[(int)UserParams.WaterColorB]            = new Param(UserParams.WaterColorB.ToString()           , 0.29f,   false);
            Params[(int)UserParams.DeltaUV_X]              = new Param(UserParams.DeltaUV_X.ToString()             , 0.01f,   false);
            Params[(int)UserParams.DeltaUV_Y]              = new Param(UserParams.DeltaUV_Y.ToString()             , 0.03f,   false);
            Params[(int)UserParams.DeltaUV_Z]              = new Param(UserParams.DeltaUV_Z.ToString()             , -0.005f, false);
            Params[(int)UserParams.DeltaUV_W]              = new Param(UserParams.DeltaUV_W.ToString()             , 0.015f,  false);
            Params[(int)UserParams.gw_fNormalUVScale0]     = new Param(UserParams.gw_fNormalUVScale0.ToString()    , 15.0f,   true); // 整数就可以了
            Params[(int)UserParams.gw_fNormalUVScale1]     = new Param(UserParams.gw_fNormalUVScale1.ToString()    , 15.0f,   true); // 整数就可以了
            Params[(int)UserParams.gw_fNormalRatio]        = new Param(UserParams.gw_fNormalRatio.ToString()       , 0.5f,    true);
            Params[(int)UserParams.gw_fNormalNoise]        = new Param(UserParams.gw_fNormalNoise.ToString()       , 0.1f,    true);
            Params[(int)UserParams.gw_fNoNoiseScreen]      = new Param(UserParams.gw_fNoNoiseScreen.ToString()     , 0.03f,   true);
            Params[(int)UserParams.gw_fRefractRadius]      = new Param(UserParams.gw_fRefractRadius.ToString()     , 5000f,   true);
            Params[(int)UserParams.gw_fRefractMinAlpha]    = new Param(UserParams.gw_fRefractMinAlpha.ToString()   , 0.82f,   true);
            Params[(int)UserParams.gw_fFresnelBias]        = new Param(UserParams.gw_fFresnelBias.ToString()       , 0.60f,   true);
            Params[(int)UserParams.gw_fFresnelScale]       = new Param(UserParams.gw_fFresnelScale.ToString()      , 1.35f,   true); // 1.8
            Params[(int)UserParams.gw_fFresnelPower]       = new Param(UserParams.gw_fFresnelPower.ToString()      , 1.218f,  true); // 1.31
            Params[(int)UserParams.gw_fCausticsUVScale]    = new Param(UserParams.gw_fCausticsUVScale.ToString()   , 30.0f,   true);
            Params[(int)UserParams.gw_fCausticsDepth]      = new Param(UserParams.gw_fCausticsDepth.ToString()     , 270.0f,  true);
            Params[(int)UserParams.CausticsSpeed]          = new Param(UserParams.CausticsSpeed.ToString()         , 0.6f,    false);
            Params[(int)UserParams.gw_fWaveVertexSpacing]  = new Param(UserParams.gw_fWaveVertexSpacing.ToString() , 0.2f,    true); // 27
            Params[(int)UserParams.gw_fWaveRatio]          = new Param(UserParams.gw_fWaveRatio.ToString()         , 0.28f,   true); // 5
            Params[(int)UserParams.WaveHeightDiv]          = new Param(UserParams.WaveHeightDiv.ToString()         , 2.0f,    false);
            Params[(int)UserParams.ClipHeight]             = new Param(UserParams.ClipHeight.ToString()            , 80.0f,   false);
            Params[(int)UserParams.SunColorR]              = new Param(UserParams.SunColorR.ToString()             , 1.2f,    false);
            Params[(int)UserParams.SunColorG]              = new Param(UserParams.SunColorG.ToString()             , 0.4f,    false);
            Params[(int)UserParams.SunColorB]              = new Param(UserParams.SunColorB.ToString()             , 0.1f,    false);
            Params[(int)UserParams.gw_fSunFactor]          = new Param(UserParams.gw_fSunFactor.ToString()         , 1.5f,    true);
            Params[(int)UserParams.gw_fSunPower]           = new Param(UserParams.gw_fSunPower.ToString()          , 220.0f,  true);
            Params[(int)UserParams.SunFaceTo]              = new Param(UserParams.SunFaceTo.ToString()             , 90.0f,   false);
            Params[(int)UserParams.SunHeight]              = new Param(UserParams.SunHeight.ToString()             , 8.3f,    false);
            Params[(int)UserParams.gw_fNoiseUVScale]       = new Param(UserParams.gw_fNoiseUVScale.ToString()      , 128f,    true);
            Params[(int)UserParams.gw_fNoiseWaveHeightDiv] = new Param(UserParams.gw_fNoiseWaveHeightDiv.ToString(), 2.0f,    true);
            Params[(int)UserParams.gw_fSunNormalSpacing]   = new Param(UserParams.gw_fSunNormalSpacing.ToString()  , 0.7f,    true);
            Params[(int)UserParams.gw_fSunNormalRatio]     = new Param(UserParams.gw_fSunNormalRatio.ToString()    , 0.86f,   true);
        }

        public void UpdateWaterParams()
        {
            foreach (Param p in Params)
            {
                string name = p.m_Name;
                if (name.StartsWith("gw_f"))
                {
                    Water.material.SetFloat(name, p.m_Value);
                    //Debug.LogWarningFormat("UserParams update shder : {0} = {1}", name, p.m_Value);
                }
            }
        }

        public float GetFloat(UserParams p)
        {
            return Params[(int)p].m_Value;
        }


        [ContextMenu("Test")]
        void Test()
        {
            InitWaterParams();
        }
    }
}
