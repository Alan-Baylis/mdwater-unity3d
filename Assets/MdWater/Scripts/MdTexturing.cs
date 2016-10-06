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
            InitHeightMaps();
            InitCausticsMaps();
            InitNormapMaps();
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

        void InitHeightMaps()
        {

        }
        void InitCausticsMaps()
        {

        }
        void InitNormapMaps()
        {

        }
    }
}
