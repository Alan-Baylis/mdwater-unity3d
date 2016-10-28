//#define SIMPLE_POLY

using UnityEngine;
using System.Collections;

#pragma warning disable 0429 // disable warning CS0429 : Unreachable expression code detected

namespace MynjenDook
{
    public static class MMMeshCreator
    {
        static public MdWater Water = null;

        enum TCORNER
        {
            BOTTOMLEFT = 0,
            BOTTOMRIGHT = 1,
            UPLEFT = 2,
            UPRIGHT = 3
        };

        static public Mesh CreateTestMesh(int profile)
        {
            float width = 2;
            float height = 2;

            int verts = MdPredefinition.Instance.a_np_size[profile];
            verts = 2;

            Mesh mesh = new Mesh();
            mesh.name = "WaterMesh";
            Vector3[] vertices = new Vector3[verts * verts];
            Vector2[] uv = new Vector2[verts * verts];
            int triangleCount = (verts - 1) * (verts - 1) * 2;
            int[] triangles = new int[triangleCount * 3];

            float MinX = -width / 2f;
            float MaxX = width / 2f;
            float MinY = -height / 2f;
            float MaxY = height / 2f;

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

        static public void CreateLodMesh(int profile, ref GameObject ProfileNode)
        {
#if SIMPLE_POLY
            Mesh m = CreateTestMesh(profile);
            GameObject o = AfterCreateMesh(profile, m, 0, 0, 0);
            o.transform.parent = ProfileNode.transform;
            return;
#endif

            MdPredefinition def = MdPredefinition.Instance;

            // uv示意图（ring0和ring1）
            // 
            //                10 10
            //             8 8
            //          6 6
            //       4 4
            //    2 2
            // 0 0
            // 所以四个ring0的uv从(4,4)到(6,6)，ring1 ring2的uv每一个都不同

            //NiNode* pkParent = m_spSubNode[profile];
            uint uiTotalVerts = 0;
            uint uiTotalTris = 0;

            // 先处理4个ring0
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 2; i++)
                {
                    float fStartX = (i - 1) * def.waterl0;
                    float fStartY = (j - 1) * def.waterl0;
                    Vector3 kMin = new Vector3(fStartX, 0, fStartY);
                    Vector3 kMax = new Vector3(fStartX + def.waterl0, 0, fStartY + def.waterl0);
                    Vector2 kMinUV = new Vector3(i + 4, j + 4);
                    Vector2 kMaxUV = new Vector3(i + 5, j + 5);
                    Mesh pkMesh = _CreateRectangleMesh(0, i, j, (uint)def.waterlv0, kMin, kMax, kMinUV, kMaxUV);

                    uiTotalVerts += (uint)pkMesh.vertexCount;
                    uiTotalTris += (uint)pkMesh.triangles.Length / 3;
                    GameObject goMesh = AfterCreateMesh(profile, pkMesh, 0, i, j);
                    goMesh.transform.parent = ProfileNode.transform;
                }
            }

            // 最低配水块边长比较大，所以需要ring0
            if (profile != 0)
            {
                // 再处理8个ring1
                for (int j = 0; j < 3; j++)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (i == 1 && j == 1) // 中间的不用
                            continue;

                        float fRing1Radius = def.waterl1 / 2f;
                        float fStartX = (i - 1) * def.waterl1;
                        float fStartY = (j - 1) * def.waterl1;
                        Vector3 kMin = new Vector3(fStartX - fRing1Radius, 0, fStartY - fRing1Radius);
                        Vector3 kMax = new Vector3(fStartX + fRing1Radius, 0, fStartY + fRing1Radius);
                        Vector2 kMinUV = new Vector2(i * 2 + 2, j * 2 + 2);
                        Vector2 kMaxUV = new Vector2(i * 2 + 4, j * 2 + 4);
                        Mesh pkMesh = _CreateRectangleMesh(1, i, j, (uint)def.waterlv1, kMin, kMax, kMinUV, kMaxUV);
                        uiTotalVerts += (uint)pkMesh.vertexCount;
                        uiTotalTris += (uint)pkMesh.triangles.Length / 3;
                        GameObject goMesh = AfterCreateMesh(profile, pkMesh, 1, i, j);
                        goMesh.transform.parent = ProfileNode.transform;
                    }
                }

		        // 再处理16个ring2
		        for (int j = 0; j< 5; j++)
		        {
			        for (int i = 0; i< 5; i++)
			        {
				        if (i >= 1 && i <= 3 && j >= 1 && j <= 3) // 中间的不用
					        continue;

				        float fRing2Radius = def.waterl2 / 2f;
                        float fStartX = (i - 2) * def.waterl2;
                        float fStartY = (j - 2) * def.waterl2;
                        Vector3 kMin = new Vector3(fStartX - fRing2Radius, 0, fStartY - fRing2Radius);
                        Vector3 kMax = new Vector3(fStartX + fRing2Radius, 0, fStartY + fRing2Radius);
                        Vector2 kMinUV = new Vector2(i* 2, j* 2);
                        Vector2 kMaxUV = new Vector2(i* 2 + 2, j* 2 + 2);

                        Mesh pkMesh = _CreateRectangleMesh(2, i, j, (uint)def.waterlv2, kMin, kMax, kMinUV, kMaxUV);
                        uiTotalVerts += (uint)pkMesh.vertexCount;
                        uiTotalTris += (uint)pkMesh.triangles.Length / 3;

                        GameObject goMesh = AfterCreateMesh(profile, pkMesh, 2, i, j);
                        goMesh.transform.parent = ProfileNode.transform;
                    }
                }
	        }
            Debug.LogFormat("顶点数：{0}, 面数：{1}", uiTotalVerts, uiTotalTris);

            // update
            //pkParent->UpdateProperties();
            //pkParent->UpdateEffects();
            //pkParent->Update(0);
        }


        static private Mesh _CreateRectangleMesh(int ring, int iRect, int jRect, uint uiNumVertsSide, Vector3 kMin, Vector3 kMax, Vector2 kMinUV, Vector2 kMaxUV)
        {
            // nimesh
            Mesh pkMesh = new Mesh();
            string szName = string.Format("WaterLODMeshRing{0}({1}_{2})", ring, iRect, jRect);
            pkMesh.name = szName;
            //pkMesh->SetSubmeshCount(1);
            //pkMesh->SetPrimitiveType(NiPrimitiveType::PRIMITIVE_TRIANGLES);

            // 份数
            uint uiNumVertsX = uiNumVertsSide - 1;
            uint uiNumVertsY = uiNumVertsSide - 1;

            // ring0、ring1要缩一圈
            uint uiNumSides = 0;
            uint uiMinX = 0;
            uint uiMaxX = uiNumVertsX - 1;
            uint uiMinY = 0;
            uint uiMaxY = uiNumVertsY - 1;
            switch (ring)
            {
                case 0:
                    uiNumSides = 2;
                    switch (jRect * 2 + iRect)
                    {
                        case 0: uiMinX++; uiMinY++; break;
                        case 1: uiMaxX--; uiMinY++; break;
                        case 2: uiMinX++; uiMaxY--; break;
                        case 3: uiMaxX--; uiMaxY--; break;
                        default: Debug.Assert(false); break;
                    }
                    break;

                case 1:
                    if (iRect == 0)
                    {
                        uiMinX++;
                        uiNumSides++;
                    }
                    else if (iRect == 2)
                    {
                        uiMaxX--;
                        uiNumSides++;
                    }
                    if (jRect == 0)
                    {
                        uiMinY++;
                        uiNumSides++;
                    }
                    else if (jRect == 2)
                    {
                        uiMaxY--;
                        uiNumSides++;
                    }
                    break;
                case 2:
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
            Debug.Assert(uiNumSides <= 2);

            // 计算顶点数和面数
            Debug.Assert((uiNumVertsSide - 1) % 4 == 0);
            uint uiTotalVerts = uiNumVertsSide * uiNumVertsSide;
            uint uiTrisR = (uiNumVertsSide - 1) * (uiNumVertsSide - 1) * 2;
            uint uiNumRectOnSide = (uiNumVertsSide - 1) / 2;
            uint uiTotalTris = uiTrisR - uiNumSides * uiNumRectOnSide; // 有一条tcorner边就会比普通的rect少uiNumRectOnSide个三角形
            Debug.Assert(uiTotalVerts <= 65535);

            /*
            // Create the index stream
            NiDataStreamElementLock kIndexLock = pkMesh->AddStreamGetLock(
                NiCommonSemantics::INDEX(), 0,
                NiDataStreamElement::F_UINT16_1,
                3 * uiTotalTris,
                NiDataStream::ACCESS_GPU_READ |
                NiDataStream::ACCESS_CPU_WRITE_STATIC,
                NiDataStream::USAGE_VERTEX_INDEX);
            Debug.Assert(kIndexLock.count() == (3 * uiTotalTris));
            NiTStridedRandomAccessIterator<ushort> kIndicesIter = kIndexLock.begin<ushort>();

            // Create the position stream
            NiDataStreamElementLock kPositionLock = pkMesh->AddStreamGetLock(
                NiCommonSemantics::POSITION(), 0,
                NiDataStreamElement::F_FLOAT32_3,
                uiTotalVerts,
                NiDataStream::ACCESS_GPU_READ |
                NiDataStream::ACCESS_CPU_WRITE_VOLATILE,
                NiDataStream::USAGE_VERTEX);
            Debug.Assert(kPositionLock.count() == uiTotalVerts);
            NiTStridedRandomAccessIterator<NiPoint3> kPointsIter = kPositionLock.begin<NiPoint3>();

            // Create the texture uvs stream
            NiDataStreamElementLock kTexCoordLock = pkMesh->AddStreamGetLock(
                NiCommonSemantics::TEXCOORD(), 0,
                NiDataStreamElement::F_FLOAT32_2,
                uiTotalVerts,
                NiDataStream::ACCESS_GPU_READ |
                NiDataStream::ACCESS_CPU_WRITE_VOLATILE, // 贴图uv需要每帧根据noise displacement进行修改
                NiDataStream::USAGE_VERTEX);
            Debug.Assert(kTexCoordLock.count() == uiTotalVerts);
            NiTStridedRandomAccessIterator<NiPoint2> kTexCoordsIter = kTexCoordLock.begin<NiPoint2>();

            // Create the normal stream
            NiDataStreamElementLock kNormalLock = pkMesh->AddStreamGetLock(
                NiCommonSemantics::NORMAL(), 0,
                NiDataStreamElement::F_FLOAT32_4,
                uiTotalVerts,
                NiDataStream::ACCESS_GPU_READ |
                NiDataStream::ACCESS_CPU_WRITE_VOLATILE, // low低配版才用: cpu每帧计算4高度写入normal
                NiDataStream::USAGE_VERTEX);
            Debug.Assert(kNormalLock.count() == uiTotalVerts);
            NiTStridedRandomAccessIterator<NiColorA> kNormalsIter = kNormalLock.begin<NiColorA>();

            // Create the color stream: for preprocess vert location
            NiDataStreamElementLock kColorLock = pkMesh->AddStreamGetLock(
                NiCommonSemantics::COLOR(), 0,
                NiDataStreamElement::F_FLOAT32_4,
                uiTotalVerts,
                NiDataStream::ACCESS_CPU_READ |
                NiDataStream::ACCESS_CPU_WRITE_STATIC,
                NiDataStream::USAGE_VERTEX);
            Debug.Assert(kColorLock.count() == uiTotalVerts);
            NiTStridedRandomAccessIterator<NiColorA> kColorsIter = kColorLock.begin<NiColorA>(); // 只写一次，顶点的noise初始uv

            */
            Vector3[] vertices = new Vector3[uiTotalVerts];
            Vector2[] uv = new Vector2[uiTotalVerts];
            //normal在ring0时是需要的，到时候再改
            Color[] colors = new Color[uiTotalVerts];
            int[] triangles = new int[uiTotalTris * 3];
            


            // Get the center and radius
            Vector3 kTemp = kMax - kMin;

            // Get the increment values
            float fIncrementX = kTemp.x / (float)uiNumVertsX;
            float fIncrementY = kTemp.z / (float)uiNumVertsY;

            // Get the increment values for the texture coordinates
            Vector2 kTempUV = kMaxUV - kMinUV;
            float fIncrementU = kTempUV.x / (float)uiNumVertsX; // uv保持一致，所以ring0是1, ring1是3, ring2是9
            float fIncrementV = kTempUV.y / (float)uiNumVertsY;

            // Set the positions
            Vector3 kPosition = kMin;
            Vector2 kUvCoord = kMinUV;

            // 顶点和索引的计数
            uint uiNumVerts = 0;
            uint uiIndex = 0;

            for (uint uiY = 0; uiY <= uiNumVertsY; uiY++)
            {
                // Set the x position back to default
                kPosition.x = kMin.x;
                kUvCoord.x = kMinUV.x;

                for (uint uiX = 0; uiX <= uiNumVertsX; uiX++)
                {
                    // Set the vertex position
                    vertices[uiNumVerts] = kPosition;
                    uv[uiNumVerts] = kUvCoord;
                    //kNormalsIter[uiNumVerts] = NiColorA(0, 0, 0, 0); // normal数据只在低配版shader有用 todo.ksh

                    // 计算顶点的noise UV
                    int vx = 0, vy = 0;
                    float posX = kPosition.x;
                    float posY = kPosition.z;
                    CalcVertNoiseUV(ref posX, ref posY, ref vx, ref vy);
                    colors[uiNumVerts] = new Color((float)vx, (float)vy, kUvCoord.x, kUvCoord.y); // color stream: noise uv 和 纹理uv

                    // Set the index buffer
                    if (uiY >= uiMinY && uiY <= uiMaxY && uiX >= uiMinX && uiX <= uiMaxX)
                    {
                        // 初始化index buffer
                        // 1  2
                        // 0  3
                        // 每个小矩形分成2个逆时针的三角形
                        uint uiIndex0 = (uiY * (uiNumVertsX + 1)) + uiX;
                        uint uiIndex1 = ((uiY + 1) * (uiNumVertsX + 1)) + uiX;
                        uint uiIndex2 = ((uiY + 1) * (uiNumVertsX + 1)) + (uiX + 1);
                        uint uiIndex3 = (uiY * (uiNumVertsX + 1)) + (uiX + 1);

                        triangles[uiIndex++] = (ushort)uiIndex0;
                        triangles[uiIndex++] = (MdPredefinition.Macro.waterccw != 0) ? (ushort)uiIndex2 : (ushort)uiIndex1;
                        triangles[uiIndex++] = (MdPredefinition.Macro.waterccw != 0) ? (ushort)uiIndex1 : (ushort)uiIndex2;

                        triangles[uiIndex++] = (ushort)uiIndex0;
                        triangles[uiIndex++] = (MdPredefinition.Macro.waterccw != 0) ? (ushort)uiIndex3 : (ushort)uiIndex2;
                        triangles[uiIndex++] = (MdPredefinition.Macro.waterccw != 0) ? (ushort)uiIndex2 : (ushort)uiIndex3;
                    }

                    // Increment to the next vertex
                    uiNumVerts++;

                    // Increment the position
                    kPosition.x += fIncrementX;
                    kUvCoord.x += fIncrementU;
                }

                // Increment the position
                kPosition.z += fIncrementY;
                kUvCoord.y += fIncrementV;
            }

            // t-corner: 没有新顶点，只需填充indexbuffer
            _CreateTCornerRectangles(ring, iRect, jRect, uiNumVertsSide, ref triangles, ref uiIndex);

            // Unlock the data
            /*
            kTexCoordLock.Unlock();
            kPositionLock.Unlock();
            kIndexLock.Unlock();
            kNormalLock.Unlock();
            kColorLock.Unlock(); */

            pkMesh.vertices = vertices;
            pkMesh.uv = uv;
            pkMesh.colors = colors;
            pkMesh.triangles = triangles;

            pkMesh.RecalculateBounds();
            pkMesh.RecalculateNormals();

            return pkMesh;
        }

        static private void _CreateTCornerRectangles(int ring, int iRect, int jRect, uint uiNumVertsSide, ref int[] kIndicesIter, ref uint uiIndex)
        {
            // 接下来准备tcorner的顶点索引
            uint[] indexDown0 = new uint[uiNumVertsSide];
            uint[] indexDown1 = new uint[uiNumVertsSide];
            uint[] indexUp0 = new uint[uiNumVertsSide];
            uint[] indexUp1 = new uint[uiNumVertsSide];
            uint[] indexLeft0 = new uint[uiNumVertsSide];
            uint[] indexLeft1 = new uint[uiNumVertsSide];
            uint[] indexRight0 = new uint[uiNumVertsSide];
            uint[] indexRight1 = new uint[uiNumVertsSide];

            for (uint ui = 0; ui < uiNumVertsSide; ui++)
            {
                // 0是外层 1是内层
                indexDown0[ui] = (0 * uiNumVertsSide) + ui;
                indexDown1[ui] = (1 * uiNumVertsSide) + ui;
                indexUp0[ui] = (uiNumVertsSide - 1) * uiNumVertsSide + ui;
                indexUp1[ui] = (uiNumVertsSide - 2) * uiNumVertsSide + ui;
                indexLeft0[ui] = uiNumVertsSide * ui;
                indexLeft1[ui] = uiNumVertsSide * ui + 1;
                indexRight0[ui] = uiNumVertsSide * ui + (uiNumVertsSide - 1);
                indexRight1[ui] = uiNumVertsSide * ui + (uiNumVertsSide - 2);
            }

            // 填充IndexIter构造三角形
            // 4条横边
            uint uiNumRectOnSide = (uiNumVertsSide - 1) / 2;
            //NiTPrimitiveArray<NiUInt32> uiIndices(8);
            uint[] uiIndices = new uint[8];

            switch (ring)
            {
                case 0:
                case 1:
                    if (jRect == 0)
                    {
                        for (uint ui = 1; ui < uiNumRectOnSide - 1; ui++)
                        {
                            uiIndices[0] = indexDown0[ui * 2 + 0];
                            uiIndices[1] = indexDown0[ui * 2 + 1];
                            uiIndices[2] = indexDown0[ui * 2 + 2];
                            uiIndices[3] = indexDown1[ui * 2 + 0];
                            uiIndices[4] = indexDown1[ui * 2 + 1];
                            uiIndices[5] = indexDown1[ui * 2 + 2];
                            _FillIndexRectSide(ref kIndicesIter, ref uiIndex, ref uiIndices);
                        }
                    }
                    if (jRect == (ring == 0 ? 1 : 2))
                    {
                        for (uint ui = 1; ui < uiNumRectOnSide - 1; ui++)
                        {
                            uiIndices[2] = indexUp0[ui * 2 + 0];
                            uiIndices[1] = indexUp0[ui * 2 + 1];
                            uiIndices[0] = indexUp0[ui * 2 + 2];
                            uiIndices[5] = indexUp1[ui * 2 + 0];
                            uiIndices[4] = indexUp1[ui * 2 + 1];
                            uiIndices[3] = indexUp1[ui * 2 + 2];
                            _FillIndexRectSide(ref kIndicesIter, ref uiIndex, ref uiIndices);
                        }
                    }
                    if (iRect == 0)
                    {
                        for (uint ui = 1; ui < uiNumRectOnSide - 1; ui++)
                        {
                            uiIndices[2] = indexLeft0[ui * 2 + 0];
                            uiIndices[1] = indexLeft0[ui * 2 + 1];
                            uiIndices[0] = indexLeft0[ui * 2 + 2];
                            uiIndices[5] = indexLeft1[ui * 2 + 0];
                            uiIndices[4] = indexLeft1[ui * 2 + 1];
                            uiIndices[3] = indexLeft1[ui * 2 + 2];
                            _FillIndexRectSide(ref kIndicesIter, ref uiIndex, ref uiIndices);
                        }
                    }
                    if (iRect == (ring == 0 ? 1 : 2))
                    {
                        for (uint ui = 1; ui < uiNumRectOnSide - 1; ui++)
                        {
                            uiIndices[0] = indexRight0[ui * 2 + 0];
                            uiIndices[1] = indexRight0[ui * 2 + 1];
                            uiIndices[2] = indexRight0[ui * 2 + 2];
                            uiIndices[3] = indexRight1[ui * 2 + 0];
                            uiIndices[4] = indexRight1[ui * 2 + 1];
                            uiIndices[5] = indexRight1[ui * 2 + 2];
                            _FillIndexRectSide(ref kIndicesIter, ref uiIndex, ref uiIndices);
                        }
                    }
                    break;
                case 2:
                    break;
            }

            // t-corner
            switch (ring)
            {
                case 0:
                    switch (jRect * 2 + iRect)
                    {
                        case 0:
                            _FillIndexRectCorner(TCORNER.BOTTOMLEFT, ref kIndicesIter, ref uiIndex, uiNumVertsSide,
                                ref indexDown0, ref indexDown1, ref indexUp0, ref indexUp1, ref indexLeft0, ref indexLeft1, ref indexRight0, ref indexRight1);
                            break;
                        case 1:
                            _FillIndexRectCorner(TCORNER.BOTTOMRIGHT, ref kIndicesIter, ref uiIndex, uiNumVertsSide,
                                ref indexDown0, ref indexDown1, ref indexUp0, ref indexUp1, ref indexLeft0, ref indexLeft1, ref indexRight0, ref indexRight1);
                            break;
                        case 2:
                            _FillIndexRectCorner(TCORNER.UPLEFT, ref kIndicesIter, ref uiIndex, uiNumVertsSide,
                                ref indexDown0, ref indexDown1, ref indexUp0, ref indexUp1, ref indexLeft0, ref indexLeft1, ref indexRight0, ref indexRight1);
                            break;
                        case 3:
                            _FillIndexRectCorner(TCORNER.UPRIGHT, ref kIndicesIter, ref uiIndex, uiNumVertsSide,
                                ref indexDown0, ref indexDown1, ref indexUp0, ref indexUp1, ref indexLeft0, ref indexLeft1, ref indexRight0, ref indexRight1);
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }
                    break;

                case 1:
                    switch (jRect * 3 + iRect)
                    {
                        case 1:
                            {
                                uiIndices[0] = indexDown0[0];
                                uiIndices[1] = indexDown0[1];
                                uiIndices[2] = indexDown0[2];
                                uiIndices[3] = indexDown1[0];
                                uiIndices[4] = indexDown1[1];
                                uiIndices[5] = indexDown1[2];
                                _FillIndexRectSide(ref kIndicesIter, ref uiIndex, ref uiIndices);
                                uiIndices[0] = indexDown0[uiNumVertsSide - 3];
                                uiIndices[1] = indexDown0[uiNumVertsSide - 2];
                                uiIndices[2] = indexDown0[uiNumVertsSide - 1];
                                uiIndices[3] = indexDown1[uiNumVertsSide - 3];
                                uiIndices[4] = indexDown1[uiNumVertsSide - 2];
                                uiIndices[5] = indexDown1[uiNumVertsSide - 1];
                                _FillIndexRectSide(ref kIndicesIter, ref uiIndex, ref uiIndices);
                            }
                            break;
                        case 3:
                            {
                                uiIndices[2] = indexLeft0[0];
                                uiIndices[1] = indexLeft0[1];
                                uiIndices[0] = indexLeft0[2];
                                uiIndices[5] = indexLeft1[0];
                                uiIndices[4] = indexLeft1[1];
                                uiIndices[3] = indexLeft1[2];
                                _FillIndexRectSide(ref kIndicesIter, ref uiIndex, ref uiIndices);
                                uiIndices[2] = indexLeft0[uiNumVertsSide - 3];
                                uiIndices[1] = indexLeft0[uiNumVertsSide - 2];
                                uiIndices[0] = indexLeft0[uiNumVertsSide - 1];
                                uiIndices[5] = indexLeft1[uiNumVertsSide - 3];
                                uiIndices[4] = indexLeft1[uiNumVertsSide - 2];
                                uiIndices[3] = indexLeft1[uiNumVertsSide - 1];
                                _FillIndexRectSide(ref kIndicesIter, ref uiIndex, ref uiIndices);
                            }
                            break;
                        case 7:
                            {
                                uiIndices[2] = indexUp0[0];
                                uiIndices[1] = indexUp0[1];
                                uiIndices[0] = indexUp0[2];
                                uiIndices[5] = indexUp1[0];
                                uiIndices[4] = indexUp1[1];
                                uiIndices[3] = indexUp1[2];
                                _FillIndexRectSide(ref kIndicesIter, ref uiIndex, ref uiIndices);
                                uiIndices[2] = indexUp0[uiNumVertsSide - 3];
                                uiIndices[1] = indexUp0[uiNumVertsSide - 2];
                                uiIndices[0] = indexUp0[uiNumVertsSide - 1];
                                uiIndices[5] = indexUp1[uiNumVertsSide - 3];
                                uiIndices[4] = indexUp1[uiNumVertsSide - 2];
                                uiIndices[3] = indexUp1[uiNumVertsSide - 1];
                                _FillIndexRectSide(ref kIndicesIter, ref uiIndex, ref uiIndices);
                            }
                            break;
                        case 5:
                            {
                                uiIndices[0] = indexRight0[0];
                                uiIndices[1] = indexRight0[1];
                                uiIndices[2] = indexRight0[2];
                                uiIndices[3] = indexRight1[0];
                                uiIndices[4] = indexRight1[1];
                                uiIndices[5] = indexRight1[2];
                                _FillIndexRectSide(ref kIndicesIter, ref uiIndex, ref uiIndices);
                                uiIndices[0] = indexRight0[uiNumVertsSide - 3];
                                uiIndices[1] = indexRight0[uiNumVertsSide - 2];
                                uiIndices[2] = indexRight0[uiNumVertsSide - 1];
                                uiIndices[3] = indexRight1[uiNumVertsSide - 3];
                                uiIndices[4] = indexRight1[uiNumVertsSide - 2];
                                uiIndices[5] = indexRight1[uiNumVertsSide - 1];
                                _FillIndexRectSide(ref kIndicesIter, ref uiIndex, ref uiIndices);
                            }
                            break;

                        // 4个角落
                        case 0:
                            _FillIndexRectCorner(TCORNER.BOTTOMLEFT, ref kIndicesIter, ref uiIndex, uiNumVertsSide,
                                ref indexDown0, ref indexDown1, ref indexUp0, ref indexUp1, ref indexLeft0, ref indexLeft1, ref indexRight0, ref indexRight1);
                            break;
                        case 2:
                            _FillIndexRectCorner(TCORNER.BOTTOMRIGHT, ref kIndicesIter, ref uiIndex, uiNumVertsSide,
                                ref indexDown0, ref indexDown1, ref indexUp0, ref indexUp1, ref indexLeft0, ref indexLeft1, ref indexRight0, ref indexRight1);
                            break;
                        case 6:
                            _FillIndexRectCorner(TCORNER.UPLEFT, ref kIndicesIter, ref uiIndex, uiNumVertsSide,
                                ref indexDown0, ref indexDown1, ref indexUp0, ref indexUp1, ref indexLeft0, ref indexLeft1, ref indexRight0, ref indexRight1);
                            break;
                        case 8:
                            _FillIndexRectCorner(TCORNER.UPRIGHT, ref kIndicesIter, ref uiIndex, uiNumVertsSide,
                                ref indexDown0, ref indexDown1, ref indexUp0, ref indexUp1, ref indexLeft0, ref indexLeft1, ref indexRight0, ref indexRight1);
                            break;

                        case 4:
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }
                    break;
                case 2:
                    break;
            }
        }

        // index示意图（其中1、3没用）
        //
        //  0  3  6    2  1  0    2  1  0 
        //  1  4  7    5  4  3    5  4  3
        //  2  5                     7  6
        //
        //  0  3                     5  2
        //  1  4                     4  1
        //  2  5                     3  0
        //
        //  6  7                     5  2
        //  3  4  5    3  4  5    7  4  1
        //  0  1  2    0  1  2    6  3  0
        static private void _FillIndexRectSide(ref int[] kIndicesIter, ref uint uiIndex, ref uint[] uiIndices)
        {
            kIndicesIter[uiIndex++] = (ushort)uiIndices[0];
            kIndicesIter[uiIndex++] = (MdPredefinition.Macro.waterccw != 0) ? (ushort)uiIndices[4] : (ushort)uiIndices[3];
            kIndicesIter[uiIndex++] = (MdPredefinition.Macro.waterccw != 0) ? (ushort)uiIndices[3] : (ushort)uiIndices[4];

            kIndicesIter[uiIndex++] = (ushort)uiIndices[0];
            kIndicesIter[uiIndex++] = (MdPredefinition.Macro.waterccw != 0) ? (ushort)uiIndices[2] : (ushort)uiIndices[4];
            kIndicesIter[uiIndex++] = (MdPredefinition.Macro.waterccw != 0) ? (ushort)uiIndices[4] : (ushort)uiIndices[2];

            kIndicesIter[uiIndex++] = (ushort)uiIndices[2];
            kIndicesIter[uiIndex++] = (MdPredefinition.Macro.waterccw != 0) ? (ushort)uiIndices[5] : (ushort)uiIndices[4];
            kIndicesIter[uiIndex++] = (MdPredefinition.Macro.waterccw != 0) ? (ushort)uiIndices[4] : (ushort)uiIndices[5];
        }
        static private void _FillIndexRectCorner(TCORNER corner, ref int[] kIndicesIter, ref uint uiIndex, uint uiNumVertsSide,
            ref uint[] indexDown0, ref uint[] indexDown1, ref uint[] indexUp0, ref uint[] indexUp1,
            ref uint[] indexLeft0, ref uint[] indexLeft1, ref uint[] indexRight0, ref uint[] indexRight1)
        {
            uint[] uiIndices = new uint[8];
            switch (corner)
            {
                case TCORNER.BOTTOMLEFT:
                    uiIndices[0] = indexDown0[uiNumVertsSide - 3];
                    uiIndices[1] = indexDown0[uiNumVertsSide - 2];
                    uiIndices[2] = indexDown0[uiNumVertsSide - 1];
                    uiIndices[3] = indexDown1[uiNumVertsSide - 3];
                    uiIndices[4] = indexDown1[uiNumVertsSide - 2];
                    uiIndices[5] = indexDown1[uiNumVertsSide - 1];
                    _FillIndexRectSide(ref kIndicesIter, ref uiIndex, ref uiIndices);
                    uiIndices[2] = indexLeft0[uiNumVertsSide - 3];
                    uiIndices[1] = indexLeft0[uiNumVertsSide - 2];
                    uiIndices[0] = indexLeft0[uiNumVertsSide - 1];
                    uiIndices[5] = indexLeft1[uiNumVertsSide - 3];
                    uiIndices[4] = indexLeft1[uiNumVertsSide - 2];
                    uiIndices[3] = indexLeft1[uiNumVertsSide - 1];
                    _FillIndexRectSide(ref kIndicesIter, ref uiIndex, ref uiIndices);
                    uiIndices[0] = indexDown0[0];
                    uiIndices[1] = indexDown0[1];
                    uiIndices[2] = indexDown0[2];
                    uiIndices[3] = indexDown1[0];
                    uiIndices[4] = indexDown1[1];
                    uiIndices[5] = indexDown1[2];
                    uiIndices[6] = indexLeft0[2];
                    uiIndices[7] = indexLeft1[2];
                    _FillIndexCorner(ref kIndicesIter, ref uiIndex, ref uiIndices);
                    break;
                case TCORNER.BOTTOMRIGHT:
                    uiIndices[0] = indexDown0[0];
                    uiIndices[1] = indexDown0[1];
                    uiIndices[2] = indexDown0[2];
                    uiIndices[3] = indexDown1[0];
                    uiIndices[4] = indexDown1[1];
                    uiIndices[5] = indexDown1[2];
                    _FillIndexRectSide(ref kIndicesIter, ref uiIndex, ref uiIndices);
                    uiIndices[0] = indexRight0[uiNumVertsSide - 3];
                    uiIndices[1] = indexRight0[uiNumVertsSide - 2];
                    uiIndices[2] = indexRight0[uiNumVertsSide - 1];
                    uiIndices[3] = indexRight1[uiNumVertsSide - 3];
                    uiIndices[4] = indexRight1[uiNumVertsSide - 2];
                    uiIndices[5] = indexRight1[uiNumVertsSide - 1];
                    _FillIndexRectSide(ref kIndicesIter, ref uiIndex, ref uiIndices);
                    uiIndices[0] = indexRight0[0];
                    uiIndices[1] = indexRight0[1];
                    uiIndices[2] = indexRight0[2];
                    uiIndices[3] = indexRight1[0];
                    uiIndices[4] = indexRight1[1];
                    uiIndices[5] = indexRight1[2];
                    uiIndices[6] = indexDown0[uiNumVertsSide - 3];
                    uiIndices[7] = indexDown1[uiNumVertsSide - 3];
                    _FillIndexCorner(ref kIndicesIter, ref uiIndex, ref uiIndices);
                    break;
                case TCORNER.UPLEFT:
                    uiIndices[2] = indexLeft0[0];
                    uiIndices[1] = indexLeft0[1];
                    uiIndices[0] = indexLeft0[2];
                    uiIndices[5] = indexLeft1[0];
                    uiIndices[4] = indexLeft1[1];
                    uiIndices[3] = indexLeft1[2];
                    _FillIndexRectSide(ref kIndicesIter, ref uiIndex, ref uiIndices);
                    uiIndices[2] = indexUp0[uiNumVertsSide - 3];
                    uiIndices[1] = indexUp0[uiNumVertsSide - 2];
                    uiIndices[0] = indexUp0[uiNumVertsSide - 1];
                    uiIndices[5] = indexUp1[uiNumVertsSide - 3];
                    uiIndices[4] = indexUp1[uiNumVertsSide - 2];
                    uiIndices[3] = indexUp1[uiNumVertsSide - 1];
                    _FillIndexRectSide(ref kIndicesIter, ref uiIndex, ref uiIndices);
                    uiIndices[0] = indexLeft0[uiNumVertsSide - 1];
                    uiIndices[1] = indexLeft0[uiNumVertsSide - 2];
                    uiIndices[2] = indexLeft0[uiNumVertsSide - 3];
                    uiIndices[3] = indexLeft1[uiNumVertsSide - 1];
                    uiIndices[4] = indexLeft1[uiNumVertsSide - 2];
                    uiIndices[5] = indexLeft1[uiNumVertsSide - 3];
                    uiIndices[6] = indexUp0[2];
                    uiIndices[7] = indexUp1[2];
                    _FillIndexCorner(ref kIndicesIter, ref uiIndex, ref uiIndices);
                    break;
                case TCORNER.UPRIGHT:
                    uiIndices[2] = indexUp0[0];
                    uiIndices[1] = indexUp0[1];
                    uiIndices[0] = indexUp0[2];
                    uiIndices[5] = indexUp1[0];
                    uiIndices[4] = indexUp1[1];
                    uiIndices[3] = indexUp1[2];
                    _FillIndexRectSide(ref kIndicesIter, ref uiIndex, ref uiIndices);
                    uiIndices[0] = indexRight0[0];
                    uiIndices[1] = indexRight0[1];
                    uiIndices[2] = indexRight0[2];
                    uiIndices[3] = indexRight1[0];
                    uiIndices[4] = indexRight1[1];
                    uiIndices[5] = indexRight1[2];
                    _FillIndexRectSide(ref kIndicesIter, ref uiIndex, ref uiIndices);
                    uiIndices[0] = indexUp0[uiNumVertsSide - 1];
                    uiIndices[1] = indexUp0[uiNumVertsSide - 2];
                    uiIndices[2] = indexUp0[uiNumVertsSide - 3];
                    uiIndices[3] = indexUp1[uiNumVertsSide - 1];
                    uiIndices[4] = indexUp1[uiNumVertsSide - 2];
                    uiIndices[5] = indexUp1[uiNumVertsSide - 3];
                    uiIndices[6] = indexRight0[uiNumVertsSide - 3];
                    uiIndices[7] = indexRight1[uiNumVertsSide - 3];
                    _FillIndexCorner(ref kIndicesIter, ref uiIndex, ref uiIndices);
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }
        static private void _FillIndexCorner(ref int[] kIndicesIter, ref uint uiIndex, ref uint[] uiIndices)
        {
            kIndicesIter[uiIndex++] = (ushort)uiIndices[0];
            kIndicesIter[uiIndex++] = (MdPredefinition.Macro.waterccw != 0) ? (ushort)uiIndices[2] : (ushort)uiIndices[4];
            kIndicesIter[uiIndex++] = (MdPredefinition.Macro.waterccw != 0) ? (ushort)uiIndices[4] : (ushort)uiIndices[2];

            kIndicesIter[uiIndex++] = (ushort)uiIndices[0];
            kIndicesIter[uiIndex++] = (MdPredefinition.Macro.waterccw != 0) ? (ushort)uiIndices[4] : (ushort)uiIndices[6];
            kIndicesIter[uiIndex++] = (MdPredefinition.Macro.waterccw != 0) ? (ushort)uiIndices[6] : (ushort)uiIndices[4];

            kIndicesIter[uiIndex++] = (ushort)uiIndices[4];
            kIndicesIter[uiIndex++] = (MdPredefinition.Macro.waterccw != 0) ? (ushort)uiIndices[7] : (ushort)uiIndices[6];
            kIndicesIter[uiIndex++] = (MdPredefinition.Macro.waterccw != 0) ? (ushort)uiIndices[6] : (ushort)uiIndices[7];

            kIndicesIter[uiIndex++] = (ushort)uiIndices[2];
            kIndicesIter[uiIndex++] = (MdPredefinition.Macro.waterccw != 0) ? (ushort)uiIndices[5] : (ushort)uiIndices[4];
            kIndicesIter[uiIndex++] = (MdPredefinition.Macro.waterccw != 0) ? (ushort)uiIndices[4] : (ushort)uiIndices[5];
        }

        static private GameObject AfterCreateMesh(int profile, Mesh pkMesh, int ring, int i, int j)
        {
            //// Set the bound
            // sm3.0不需要modifier
            if (profile == 0) // 
            {
            }
            //// vertex color property
            //// material property
            // texturing property
            // update


            string name = string.Format("mesh_{0}_{1}_{2}_{3}", profile, ring, i, j);
            GameObject SubMesh = new GameObject(name);
            SubMesh.layer = LayerMask.NameToLayer("Water");

            MeshFilter mf = SubMesh.AddComponent<MeshFilter>();
            mf.mesh = pkMesh;

            MeshRenderer mr = SubMesh.AddComponent<MeshRenderer>();
            mr.material = Water.material;

            MdRenderable renderable = SubMesh.AddComponent<MdRenderable>();
            renderable.Initialize();
            renderable.Water = Water;

            BoxCollider bc = SubMesh.AddComponent<BoxCollider>();

            return SubMesh;
        }

        static private void CalcVertNoiseUV(ref float x, ref float y, ref int vx, ref int vy) // x、y是顶点位置，vx、vy代表noise的uv
        {
            MdPredefinition def = MdPredefinition.Instance;

            float fEpsilon = 0.001f;

            x += fEpsilon;
            y += fEpsilon;

            while (x >= def.waterl0)
            {
                x -= def.waterl0;
            }
            while (y >= def.waterl0)
            {
                y -= def.waterl0;
            }
            while (x < 0)
            {
                x += def.waterl0;
            }
            while (y < 0)
            {
                y += def.waterl0;
            }
            Debug.Assert(x >= 0 && x < def.waterl0);
            Debug.Assert(y >= 0 && y < def.waterl0);

            vx = 0;
            vy = 0;
            while (x >= def.vspacing0)
            {
                x -= def.vspacing0;
                vx++;
            }
            while (y >= def.vspacing0)
            {
                y -= def.vspacing0;
                vy++;
            }

            x -= fEpsilon;
            y -= fEpsilon;
            x = (float)MathUtil.Clamp(x, 0.0f, def.vspacing0);
            y = (float)MathUtil.Clamp(y, 0.0f, def.vspacing0);

            Debug.Assert(vx >= 0 && vx < def.np_size);
            Debug.Assert(vy >= 0 && vy < def.np_size);
        }
    }
}
