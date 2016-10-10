using UnityEngine;
using System.Collections;

namespace MynjenDook
{
    public static class MMMeshCreator
    {
        static public Mesh CreateMesh(float profile)
        {
            float width = 2;
            float height = 2;

            Mesh mesh = new Mesh();
            mesh.name = "WaterMesh";
            mesh.vertices = new Vector3[] {
                new Vector3(-width, 0, -height),
                new Vector3( width, 0, -height),
                new Vector3( width, 0,  height),
                new Vector3(-width, 0,  height)
            };
            mesh.uv = new Vector2[] {
                new Vector2 (0, 0),
                new Vector2 (0, 1),
                new Vector2 (1, 1),
                new Vector2 (1, 0)
            };
            mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
            mesh.RecalculateNormals();

            return mesh;
        }
    }
}
