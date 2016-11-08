using System;
using UnityEngine;
using System.Collections;

namespace MynjenDook
{
    [AddComponentMenu("MynjenDook/MdSurface")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MdWater))]
    public class MdSurface : MonoBehaviour // 幸好unity和dx都是左手系
    {
        private MdWater m_water = null;
        public MdWater Water {
            get {
                m_water = GetComponent<MdWater>();
                return m_water;
            }
        }

        // shader版的noise生成
        public enum NoiseType
        {
            CpuNoise = 0,
            ClassicPerlin,
            PeriodicPerlin,
            Simplex,
            SimplexNumericalGrad,
            SimplexAnalyticalGrad
        }
        public NoiseType _noiseType;
        public bool _is3D;
        public bool _isFractal;


        public NoiseMaker m_noiseMaker = null;

        enum RenderMode
        {
            RM_POINTS = 0,
            RM_WIREFRAME,
            RM_SOLID
        };

        Plane plane, upper_bound, lower_bound;
        Vector3 normal, u, v, pos;
        float min_height, max_height;
        int gridsize_x, gridsize_y;
        RenderMode rendermode;


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

        public void MdUpdate()
        {
            m_noiseMaker.render_geometry();
            UpdateNoiseShaderKeywords();
        }

        public void Initialize(Vector3 inpos, Vector3 n, int maxProfile)
        {
            int size_x = (int)MdPredefinition.Macro.gridsize_x;
            int size_y = (int)MdPredefinition.Macro.gridsize_y;

            plane = new Plane(n, inpos);
            normal = n.normalized;

            // calculate the u and v-vectors
            // take one of two vectors (the one further away from the normal) and force it into the plane
            Vector3 x;
            if (Math.Abs(Vector3.Dot(Vector3.right, normal)) < Math.Abs(Vector3.Dot(Vector3.forward, normal)))
            {
                x = Vector3.right;
            }
            else
            {
                x = Vector3.forward;
            }
            u = x - normal * Vector3.Dot(normal, x);
            u.Normalize();

            // get v (cross)
            v = Vector3.Cross(u, normal);

            pos = inpos;
            gridsize_x = size_x/*+1*/; // 注意
            gridsize_y = size_y/*+1*/;
            rendermode = RenderMode.RM_SOLID;

            set_displacement_amplitude(0.0f);

            m_noiseMaker = new NoiseMaker(Water, gridsize_x, gridsize_y, maxProfile);
        }

        private void set_displacement_amplitude(float amplitude)
        {
            upper_bound = new Plane(normal, pos + amplitude * normal);
            lower_bound = new Plane(normal, pos - amplitude * normal);
        }

        private void UpdateNoiseShaderKeywords()
        {
            Material material = Water.material;

            string[] strTypes = {
                "CPUNOISE",
                "CNOISE",
                "PNOISE",
                "SNOISE",
                "SNOISE_NGRAD",
                "SNOISE_AGRAD"
            };
            for (int i = 0; i < strTypes.Length; i++)
            {
                string s = strTypes[i];
                material.DisableKeyword(s);
            }
            material.EnableKeyword(strTypes[(int)_noiseType]);
            /*
            if (_noiseType == NoiseType.ClassicPerlin)
                material.EnableKeyword("CNOISE");
            else if (_noiseType == NoiseType.PeriodicPerlin)
                material.EnableKeyword("PNOISE");
            else if (_noiseType == NoiseType.Simplex)
                material.EnableKeyword("SNOISE");
            else if (_noiseType == NoiseType.SimplexNumericalGrad)
                material.EnableKeyword("SNOISE_NGRAD");
            else // SimplexAnalyticalGrad
                material.EnableKeyword("SNOISE_AGRAD"); 
             */

            if (_is3D)
                material.EnableKeyword("THREED");
            else
                material.DisableKeyword("THREED");

            if (_isFractal)
                material.EnableKeyword("FRACTAL");
            else
                material.DisableKeyword("FRACTAL");
        }
    }
}
