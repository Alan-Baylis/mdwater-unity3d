using UnityEngine;
using System.Collections;

namespace MynjenDook
{
    [AddComponentMenu("MynjenDook/MdTexturing")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MdWater))]
    public class MdTexturing : MonoBehaviour
    {
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
