using UnityEngine;
using System;
using System.Collections;

namespace MynjenDook
{
    public class NoiseMaker
    {
        public enum Macro
        {
            MAXNOISE = 6000,
        }

        private MdWater m_water = null;
        public MdWater Water {
            get { return m_water; }
            set { m_water = value; }
        }


        private int sizeX, sizeY; // framebuffer size
        float f_sizeX, f_sizeY;
        int[][] noise;    // 3
        int[][] o_noise;  // 3
        int[][] p_noise;  // 3
        int[][] tempdata; // 3
        int octaves;

        int[] multitable; //MdPredefinition.Macro.max_octaves
        uint last_time;
        float[] f_multitable;//MdPredefinition.Macro.max_octaves
        double time;

        public int m_profile;
        public int m_maxprofile;

        double lp_itime;


        struct SOFTWARESURFACEVERTEX
        {
            public float x, y, z;
            public float nx, ny, nz;
            public float tu, tv;
        };
        SOFTWARESURFACEVERTEX[] vertices;

        Texture2D[][][] packed_noise_texture; // profile, t=2, level=8，其中level没用（对应原来d3d最大8层），t现在只用第0个


        public NoiseMaker(MdWater Water, int sX, int sY, int maxprofile)
        {
            this.Water = Water;

            noise = new int[3][];
            o_noise = new int[3][];
            p_noise = new int[3][];
            tempdata = new int[3][];

            // 因为把原来一些宏改成全局变量（方便代码控制），所以把原来这几个静态数组改成new的方式
            for (int i = 0; i < 3; i++)
            {
                noise[i] = new int[Water.predefinition.a_n_size_sq[i] * (int)MdPredefinition.Macro.noise_frames];
                o_noise[i] = new int[Water.predefinition.a_n_size_sq[i] * (int)MdPredefinition.Macro.max_octaves];
                p_noise[i] = new int[Water.predefinition.a_np_size_sq[i] * ((int)MdPredefinition.Macro.max_octaves >> ((int)MdPredefinition.Macro.n_packsize - 1))];
                tempdata[i] = new int[Water.predefinition.a_np_size_sq[i]];
            }

            m_maxprofile = maxprofile;
            m_profile = m_maxprofile;
            sizeX = sX;
            sizeY = sY;
            time = 0.0;

            multitable = new int[(int)MdPredefinition.Macro.max_octaves];
            f_multitable = new float[(int)MdPredefinition.Macro.max_octaves];

            last_time = (uint)Time.time * 1000;
            octaves = 0;    // don't want to have the noise accessed before it's calculated

            f_sizeX = (float)sizeX;
            f_sizeY = (float)sizeY;

            // reset normals
            vertices = new SOFTWARESURFACEVERTEX[sizeX * sizeY];
            for (int v = 0; v < sizeY; v++)
            {
                for (int u = 0; u < sizeX; u++)
                {
                    vertices[v * sizeX + u].nx = 0.0f;
                    vertices[v * sizeX + u].ny = 1.0f;
                    vertices[v * sizeX + u].nz = 0.0f;
                    vertices[v * sizeX + u].tu = (float)u / (sizeX - 1);
                    vertices[v * sizeX + u].tv = (float)v / (sizeY - 1);
                }
            }
            init_noise();

            //load_effects();
            init_textures();
        }

        public void render_geometry()
        {
            calc_noise();
            upload_noise();
        }

        void init_noise()
        {
            for (int profile = 0; profile < 3; profile++)
            {
                // 因为初始化时3套参数都要准备
                int N_SIZE = Water.predefinition.a_n_size[profile];
                int N_SIZE_M1 = Water.predefinition.a_n_size_m1[profile];
                int N_SIZE_SQ = Water.predefinition.a_n_size_sq[profile];

                // create noise (uniform)
                float[] tempnoise = new float[N_SIZE_SQ * (int)MdPredefinition.Macro.noise_frames];
                for (int i = 0; i < (N_SIZE_SQ * (int)MdPredefinition.Macro.noise_frames); i++)
                {
                    //this->noise[i] = rand()&0x0000FFFF;

                    //float temp = (float)rand() / RAND_MAX;
                    float temp = UnityEngine.Random.Range(0f, 1f);
                    tempnoise[i] = 4 * (temp - 0.5f);
                }

                for (int frame = 0; frame < (int)MdPredefinition.Macro.noise_frames; frame++)
                {
                    for (int v = 0; v < N_SIZE; v++)
                    {
                        for (int u = 0; u < N_SIZE; u++)
                        {
                            /*float temp = 0.25f * (tempnoise[frame*n_size_sq + v*n_size + u] +
                            tempnoise[frame*n_size_sq + v*n_size + ((u+1)&n_size_m1)] + 
                            tempnoise[frame*n_size_sq + ((v+1)&n_size_m1)*n_size + u] +
                            tempnoise[frame*n_size_sq + ((v+1)&n_size_m1)*n_size + ((u+1)&n_size_m1)]);*/
                            int v0 = ((v - 1) & N_SIZE_M1) * N_SIZE,
                                v1 = v * N_SIZE,
                                v2 = ((v + 1) & N_SIZE_M1) * N_SIZE,
                                u0 = ((u - 1) & N_SIZE_M1),
                                u1 = u,
                                u2 = ((u + 1) & N_SIZE_M1),
                                f = frame * N_SIZE_SQ;
                            float temp = (1.0f / 14.0f) * (tempnoise[f + v0 + u0] + tempnoise[f + v0 + u1] + tempnoise[f + v0 + u2] +
                                tempnoise[f + v1 + u0] + 6.0f * tempnoise[f + v1 + u1] + tempnoise[f + v1 + u2] +
                                tempnoise[f + v2 + u0] + tempnoise[f + v2 + u1] + tempnoise[f + v2 + u2]);

                            this.noise[profile][frame * N_SIZE_SQ + v * N_SIZE + u] = (int)((int)MdPredefinition.Macro.noise_magnitude * temp);
                        }
                    }
                }
            }
        }

        void calc_noise()
        {
            octaves = Math.Min(Water.oldparams.GetInt(MdOldParams.pParameters.p_iOctaves), (int)MdPredefinition.Macro.max_octaves);

            // calculate the strength of each octave
            float sum = 0.0f;
            for (int i = 0; i < octaves; i++)
            {
                f_multitable[i] = (float)Math.Pow(Water.oldparams.GetFloat(MdOldParams.pParameters.p_fFalloff), 1.0f * i);
                sum += f_multitable[i];
            }

            {
                for (int i = 0; i < octaves; i++)
                {
                    f_multitable[i] /= sum;
                }
            }

            {
                for (int i = 0; i < octaves; i++)
                {
                    multitable[i] = (int)((int)MdPredefinition.Macro.scale_magnitude * f_multitable[i]);
                }
            }


            uint this_time = (uint)Time.time * 1000;;
            double itime = this_time - last_time;
            //static double lp_itime = 0.0;
            last_time = this_time;
            itime *= 0.001 * Water.oldparams.GetFloat(MdOldParams.pParameters.p_fAnimspeed);
            lp_itime = /*0.99*/0.01 * lp_itime + /*0.01*/0.99 * itime; // 缩短某个很卡的帧对以后帧的影响时间
            if (!Water.oldparams.GetBool(MdOldParams.pParameters.p_bPaused))
                time += lp_itime;


            double r_timemulti = 1.0;

            for (int o = 0; o < octaves; o++)
            {
                uint[] image = new uint[3];
                int[] amount = new int[3];
                double dImage, fraction = MathUtil.modf(time * r_timemulti, out dImage);
                int iImage = (int)dImage;
                amount[0] = (int)((int)MdPredefinition.Macro.scale_magnitude * f_multitable[o] * (Math.Pow(Math.Sin((fraction + 2) * Math.PI / 3), 2) / 1.5));
                amount[1] = (int)((int)MdPredefinition.Macro.scale_magnitude * f_multitable[o] * (Math.Pow(Math.Sin((fraction + 1) * Math.PI / 3), 2) / 1.5));
                amount[2] = (int)((int)MdPredefinition.Macro.scale_magnitude * f_multitable[o] * (Math.Pow(Math.Sin((fraction) * Math.PI / 3), 2) / 1.5));
                image[0] = (uint)(iImage) & (int)MdPredefinition.Macro.noise_frames_m1;
                image[1] = (uint)(iImage + 1) & (int)MdPredefinition.Macro.noise_frames_m1;
                image[2] = (uint)(iImage + 2) & (int)MdPredefinition.Macro.noise_frames_m1;
                {
                    for (int i = 0; i < Water.predefinition.n_size_sq; i++)
                    {
                        o_noise[m_profile][i + Water.predefinition.n_size_sq * o] = (
                            ((amount[0] * noise[m_profile][i + Water.predefinition.n_size_sq * image[0]]) >> (int)MdPredefinition.Macro.scale_decimalbits) +
                            ((amount[1] * noise[m_profile][i + Water.predefinition.n_size_sq * image[1]]) >> (int)MdPredefinition.Macro.scale_decimalbits) +
                            ((amount[2] * noise[m_profile][i + Water.predefinition.n_size_sq * image[2]]) >> (int)MdPredefinition.Macro.scale_decimalbits)
                            );
                    }
                }

                r_timemulti *= Water.oldparams.GetFloat(MdOldParams.pParameters.p_fTimemulti);
            }

            if (MdPredefinition.Macro.packednoise != 0)
            {
                int octavepack = 0;
                for (int o = 0; o < octaves; o += (int)MdPredefinition.Macro.n_packsize)
                {
                    for (int v = 0; v < Water.predefinition.np_size; v++)
                        for (int u = 0; u < Water.predefinition.np_size; u++)
                        {
                            p_noise[m_profile][v * Water.predefinition.np_size + u + octavepack * Water.predefinition.np_size_sq] = o_noise[m_profile][(o + 3) * Water.predefinition.n_size_sq + (v & Water.predefinition.n_size_m1) * Water.predefinition.n_size + (u & Water.predefinition.n_size_m1)];
                            p_noise[m_profile][v * Water.predefinition.np_size + u + octavepack * Water.predefinition.np_size_sq] += mapsample(u, v, 3, o);
                            p_noise[m_profile][v * Water.predefinition.np_size + u + octavepack * Water.predefinition.np_size_sq] += mapsample(u, v, 2, o + 1);
                            p_noise[m_profile][v * Water.predefinition.np_size + u + octavepack * Water.predefinition.np_size_sq] += mapsample(u, v, 1, o + 2);
                        }
                    octavepack++;

                    /*for(int v=0; v<20; v++)
                    for(int u=0; u<20; u++)
                        p_noise[v*np_size+u] = 1000;*/
                    // debug box

                }
            }
        }

        void upload_noise()
        {
            // 低配版不需要noise纹理
            if (m_profile == 0)
                return;

            if ((int)MdPredefinition.Macro.water_cpu_normal == 1)
            {
                for (int i = 0; i < Water.predefinition.np_size_sq; i++)
                {
                    p_noise[m_profile][i] += p_noise[m_profile][i + Water.predefinition.np_size_sq]; // 将来对2张noise的合成算法有改动的话改这里
                }
            }
            else
            {
                float[] fdata = new float[Water.predefinition.np_size_sq];
                Color[] colourData = new Color[Water.predefinition.np_size_sq];
                ushort[] data = new ushort[Water.predefinition.np_size_sq];

                for (int t = 0; t < 2; t++)
                {
                    int offset = Water.predefinition.np_size_sq * t;
                    // upload the first level

                    float MAX = 0;
                    for (int i = 0; i < Water.predefinition.np_size_sq; i++)
                    {
                        //data[i] = 32768+p_noise[m_profile][i+offset];
                        float value = (float)(p_noise[m_profile][i + offset]);
                        value = value / (float)Macro.MAXNOISE;                          // 经调试value最大5000+，所以除以6000，匹配[0, 1]颜色空间
                        fdata[i] = value;
                        //fdata[i] = 3.14f; // 
                        colourData[i] = new Color(value, value, value, 1);

                        if (t == 0)
                        {
                            if (MAX < value)
                                MAX = value;
                        }
                    }
                    Texture2D tex = packed_noise_texture[m_profile][t][0];
                    tex.SetPixels(colourData);
                    tex.Apply();

                    // test: 绑纹理到plane、写tga文件
                    if (Water.texviewRenderer != null && t == 0)
                    {
                        Water.texviewRenderer.sharedMaterial.mainTexture = tex;
                        Debug.LogFormat("[upload noise] frame: {0}, time: {1}", Water.FrameCount, Time.time - Water.LastTime);

                        string fileName = string.Format(@"C:\Users\kuangsihao1\Desktop\mdwater\noise0_{0}.tga", Water.FrameCount);
                        tex.Save2Tga(fileName);
                    }

                    continue; // 只lock第一个level

                    // 最大level：8 其他level不做了 todo.ksh
                    //int c = 8; // packed_noise_texture[m_profile][t]->GetLevels();
                }
            }
        }

        int mapsample(int u, int v, int upsamplepower, int octave)
        {
            int magnitude = 1 << upsamplepower;
            int pu = u >> upsamplepower;
            int pv = v >> upsamplepower;
            int fu = u & (magnitude - 1);
            int fv = v & (magnitude - 1);
            int fu_m = magnitude - fu;
            int fv_m = magnitude - fv;

            int o = fu_m * fv_m * o_noise[m_profile][octave * Water.predefinition.n_size_sq + ((pv) & Water.predefinition.n_size_m1) * Water.predefinition.n_size + ((pu) & Water.predefinition.n_size_m1)] +
                    fu * fv_m * o_noise[m_profile][octave * Water.predefinition.n_size_sq + ((pv) & Water.predefinition.n_size_m1) * Water.predefinition.n_size + ((pu + 1) & Water.predefinition.n_size_m1)] +
                    fu_m * fv * o_noise[m_profile][octave * Water.predefinition.n_size_sq + ((pv + 1) & Water.predefinition.n_size_m1) * Water.predefinition.n_size + ((pu) & Water.predefinition.n_size_m1)] +
                    fu * fv * o_noise[m_profile][octave * Water.predefinition.n_size_sq + ((pv + 1) & Water.predefinition.n_size_m1) * Water.predefinition.n_size + ((pu + 1) & Water.predefinition.n_size_m1)];

            return o >> (upsamplepower + upsamplepower);
        }

        void init_textures()
        {
            /*
            // kuangsihao: 改成用gb的创建函数，然后传给heightClick和normalClick
            // the noise textures. currently two of them (= 8 levels)
            //device->CreateTexture(np_size,np_size,0,D3DUSAGE_DYNAMIC, D3DFMT_L16, D3DPOOL_DEFAULT, &(this->packed_noise_texture[0]),NULL);	
            //device->CreateTexture(np_size,np_size,0,D3DUSAGE_DYNAMIC, D3DFMT_L16, D3DPOOL_DEFAULT, &(this->packed_noise_texture[1]),NULL);
            NiTexture::FormatPrefs kDynamicTexFormat;
            kDynamicTexFormat.m_eAlphaFmt = NiTexture::FormatPrefs::NONE;
            kDynamicTexFormat.m_eMipMapped = NiTexture::FormatPrefs::MIP_DEFAULT;
            kDynamicTexFormat.m_ePixelLayout = NiTexture::FormatPrefs::SINGLE_COLOR_32; // 单通道浮点数

            // 最低配不需要noise纹理
            for (int i = 1; i <= m_maxprofile; i++)
            {
                int NP_SIZE = a_np_size[i];
                packed_noise_texture[i][0] = NiDynamicTexture::Create(NP_SIZE, NP_SIZE, 1, kDynamicTexFormat);
                packed_noise_texture[i][1] = NiDynamicTexture::Create(NP_SIZE, NP_SIZE, 1, kDynamicTexFormat);
            }

            return; // kuangsihao
            */
            packed_noise_texture = new Texture2D[3][][];
            for (int profile = 0; profile < 3; profile++)
            {
                int NP_SIZE = Water.predefinition.a_np_size[profile];
                packed_noise_texture[profile] = new Texture2D[2][];
                for (int i = 0; i < 2; i++)
                {
                    packed_noise_texture[profile][i] = new Texture2D[8];
                    for (int j = 0; j < 8; j++)
                    {
                        packed_noise_texture[profile][i][j] = new Texture2D(NP_SIZE, NP_SIZE);
                    }
                }
            }
        }


        [ContextMenu("Test")]
        void Test()
        {
            // 把生成的noise存成文件查看
            for (int i = 0; i < 2; ++i)
            {
                string fileName = string.Format(@"C:\Users\kuangsihao1\Desktop\noise{0}.tga", i);
                Texture2D tex = packed_noise_texture[m_profile][i][0];
                tex.Save2Tga(fileName);
            }
            Debug.Log("noise texture saved!");
        }
    }
}
