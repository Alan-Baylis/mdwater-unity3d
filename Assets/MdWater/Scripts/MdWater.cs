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
        }

        public void Initialize()
        {
            GetComponent<MdPredefinition>().Initialize();
            GetComponent<MdOldParams>().Initialize();
            GetComponent<MdUserParams>().Initialize();
            GetComponent<MdTexturing>().Initialize();
            GetComponent<MdReflection>().Initialize();
            int maxProfile = CheckHardware();
            GetComponent<MdSurface>().Initialize(Vector3.zero, Vector3.up, (int)MdPredefinition.Macro.gridsize_x, (int)MdPredefinition.Macro.gridsize_y, maxProfile);

            BuildWaterMeshes();
        }

        private void BuildWaterMeshes()
        {
            MeshFilter meshFilter = transform.GetComponent<MeshFilter>();
            meshFilter.mesh.Clear();
            Mesh newMesh = MMMeshCreator.CreateMesh(2);
            meshFilter.mesh = newMesh;
        }

        private int CheckHardware()
        {
            return 2; // todo.ksh: 
        }

        [ContextMenu("Test")]
        void Test()
        {
        }
    }
}

