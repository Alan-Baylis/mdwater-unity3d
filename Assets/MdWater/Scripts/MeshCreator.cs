using UnityEngine;
using System.Collections;

namespace MynjenDook
{
    public static class MMMeshCreator
    {
        static public Mesh CreateMesh(float profile, int verts) // 边长128个点
        {
            float width = 2;
            float height = 2;

            Mesh mesh = new Mesh();
            mesh.name = "WaterMesh";
            Vector3[] vertices = new Vector3[verts * verts];
            Vector2[] uv = new Vector2[verts * verts];
            int triangleCount = (verts - 1) * (verts - 1) * 2;
            int[] triangles = new int[triangleCount * 3];

            float MinX = -width;
            float MaxX = width;
            float MinY = -height;
            float MaxY = height;

            int VertIndex = 0;
            for (int y = 0; y < verts; ++y) 
            {
                float V = (float)y / (verts - 1f);
                float Y = Mathf.Lerp(MinY, MaxY, V);
                for (int x = 0; x < verts; ++x)
                {
                    float U = (float)x / (verts - 1f);
                    float X = Mathf.Lerp(MinX, MaxX, U);
                    vertices[y * verts + x].Set(X, 0, Y);
                    uv[y * verts + x].Set(U, V);

                    //////////////////////////////////////////////////////////////////////////
                    // 比如正在处理的是第129这个点：
                    //
                    //  128  129
                    //
                    //  0    1
                    if (x > 0 && y > 0)
                    {
                        int CurVertIndex = y * verts + x;
                        triangles[VertIndex++] = CurVertIndex - verts - 1;
                        triangles[VertIndex++] = CurVertIndex - 1;
                        triangles[VertIndex++] = CurVertIndex;

                        triangles[VertIndex++] = CurVertIndex - verts - 1;
                        triangles[VertIndex++] = CurVertIndex;
                        triangles[VertIndex++] = CurVertIndex - verts;
                    }
                    
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            return mesh;
        }
    }
}
