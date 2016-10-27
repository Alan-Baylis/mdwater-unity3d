using UnityEngine;
using System.Collections;

namespace MynjenDook
{
    [AddComponentMenu("MynjenDook/MdRenderable")]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class MdRenderable : MonoBehaviour
    {
        [HideInInspector]
        public MdWater Water = null;

        void Awake()
        {
        }
        void Start()
        {
        }
        void Update()
        {
        }
        public void Initialize()
        {
        }

        // This is called when it's known that the object will be rendered by some
        // camera. We render reflections and do other updates here.
        // Because the script executes in edit mode, reflections for the scene view
        // camera will just work!
        public void OnWillRenderObject()
        {
            Water._OnWillRenderObject(Camera.current);
        }

        // Cleanup all the objects we possibly have created
        void OnDisable()
        {
        }

        [ContextMenu("Test")]
        void Test()
        {

        }
    }
}
