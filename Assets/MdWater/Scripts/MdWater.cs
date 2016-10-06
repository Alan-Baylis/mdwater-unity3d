using UnityEngine;
using System.Collections;

namespace MynjenDook
{
    [AddComponentMenu("MynjenDook/MdWater")]
    [DisallowMultipleComponent]
    public class MdWater : MonoBehaviour
    {
        void Awake()
        {
            InitializeWaterMeshes();
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
        }

        private void InitializeWaterMeshes()
        {
            MeshFilter meshFilter = transform.GetComponent<MeshFilter>();
            meshFilter.mesh.Clear();
            Mesh newMesh = MMMeshCreator.CreateMesh(2);
            meshFilter.mesh = newMesh;
        }

        [ContextMenu("Test")]
        void Test()
        {
        }
    }
}

