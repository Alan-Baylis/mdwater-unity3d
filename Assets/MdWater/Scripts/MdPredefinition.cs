using UnityEngine;
using System.Collections;

namespace MynjenDook
{
    [AddComponentMenu("MynjenDook/MdPredefinition")]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MdWater))]
    public class MdPredefinition : MonoBehaviour
    {
        static public MdPredefinition Instance = null;
        public MdPredefinition()
        {
            Instance = this;
        }

        public enum Macro
        {
            // 下面3个用作bool值（来自SoftwareNoiseMaker）
            SSE = 0, // SSE is broken atm
            quadpipe = 0,
            packednoise = 1,


            n_packsize = 4,
            n_dec_bits = 12,
            n_dec_magn = 4096,
            n_dec_magn_m1 = 4095,

            max_octaves = 32,

            noise_frames = 256,
            noise_frames_m1 = (noise_frames -1),

            noise_decimalbits = 15,
            noise_magnitude = (1 <<(noise_decimalbits-1)),

            scale_decimalbits = 15,
            scale_magnitude = (1 <<(scale_decimalbits-1)),

            nmapsize_x = 512,
            nmapsize_y = 1024,

            // 水（XzhWater）的所有宏参数在此文件定义
            OLD_XZH_WATER_LEN = 20000,							    // 没用了
            xzhwater = 1,									        // water for XZH
            xzhrefractculler = 1,									// 是否使用我的culler
            xzhreflectculler = 1,									// 
            xzhmaxrefractdis = 20000,							    // 折射最大距离
            xzhmaxreflectdis = 20000,							    // 反射最大距离
            xzhwaterprofile = 0,									// 是否在控制台打印水的profile信息
            waterccw = (0),								            // CullMode, 与shader要配合                     false
            water_cpu_normal = 0,
            watercaustics = 32,									    // 刻蚀图数量(0-31)
            waternormals = 60,									    // 法线图数量(1-60)

            // 原来在surface.cpp里
            gridsize_x = 256,
            gridsize_y = 256,
        }

        //////////////////////////////////////////////////////////////////////////
        // 供使用的全局变量
        public int n_bits;
        public int n_size;
        public int n_size_m1;
        public int n_size_sq;
        public int n_size_sq_m1;

        public int np_bits;
        public int np_size;
        public int np_size_m1;
        public int np_size_sq;
        public int np_size_sq_m1;

        // 最新做法是： ring0边长100米有4个，ring1、ring2边长是200米。这样水总边长是1000米(中配、高配)
        public float waterl0;                                              // ring0边长低配400米，中配高配100米
        public float waterl1;                                              // 一个ring1的水块边长必须与ring0相等（共 8个）
        public float waterl2;                                              // 一个ring2的水块边长必须与ring0相等（共16个）
        public float waterr0;                                              // ring0包括tcorner的半径
        public float waterr1;                                              // 全部ring1的半径300米
        public float waterr2;                                              // 全部ring2的半径500米
        public float waterr;                                               // 整个水的半径是500米

        public int waterlv0;                                               // ring0：内层水块ring0（4个）边长顶点数
        public int waterlv1;                                               // ring1：次密集水块（共 8个）边长顶点数
        public int waterlv2;                                               // ring2：最远处水块（共16个）边长顶点数
        public int waterlv0sq;
        public int waterlv1sq;
        public int waterlv2sq;

        public float vspacing0;                                            // 顶点间距离
        public float vspacing1;
        public float vspacing2;


        //////////////////////////////////////////////////////////////////////////
        // 数组，个数都为3，分别为低配，中配，高配
        public int[] a_n_bits = new int[3];
        public int[] a_n_size = new int[3];
        public int[] a_n_size_m1 = new int[3];
        public int[] a_n_size_sq = new int[3];
        public int[] a_n_size_sq_m1 = new int[3];

        public int[] a_np_bits = new int[3];
        public int[] a_np_size = new int[3];
        public int[] a_np_size_m1 = new int[3];
        public int[] a_np_size_sq = new int[3];
        public int[] a_np_size_sq_m1 = new int[3];

        public float[] a_waterl0 = new float[3];

        public int[] a_waterlv0 = new int[3];
        public int[] a_waterlv1 = new int[3];
        public int[] a_waterlv2 = new int[3];
        public int[] a_waterlv0sq = new int[3];
        public int[] a_waterlv1sq = new int[3];
        public int[] a_waterlv2sq = new int[3];
        public float[] a_vspacing0 = new float[3];
        public float[] a_vspacing1 = new float[3];
        public float[] a_vspacing2 = new float[3];


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
            SetupPredefinitions();
        }

        private void SetupPredefinitions()
        {
            int[] BITS = { 2, 4, 5 }; // 5最好
            float[] WATERL = { 400f, 100f, 100f }; // 低配由于只有ring0，所以ring0的边长要非常大 注意单位是米

            for (int i = 0; i < 3; i++)
            {
                a_n_bits[i] = BITS[i];
                a_n_size[i] = (1 << (a_n_bits[i] - 1));
                a_n_size_m1[i] = (a_n_size[i] - 1);
                a_n_size_sq[i] = (a_n_size[i] * a_n_size[i]);
                a_n_size_sq_m1[i] = (a_n_size_sq[i] - 1);

                a_np_bits[i] = (a_n_bits[i] + (int)Macro.n_packsize - 1);
                a_np_size[i] = (1 << (a_np_bits[i] - 1));
                a_np_size_m1[i] = (a_np_size[i] - 1);
                a_np_size_sq[i] = (a_np_size[i] * a_np_size[i]);
                a_np_size_sq_m1[i] = (a_np_size_sq[i] - 1);

                a_waterl0[i] = WATERL[i];

                a_waterlv0[i] = (a_np_size[i] + 1);
                a_waterlv1[i] = (a_np_size[i] + 1);
                a_waterlv2[i] = (a_np_size[i] / 2 + 1);
                a_waterlv0sq[i] = (a_waterlv0[i] * a_waterlv0[i]);
                a_waterlv1sq[i] = (a_waterlv1[i] * a_waterlv1[i]);
                a_waterlv2sq[i] = (a_waterlv2[i] * a_waterlv2[i]);
            }
        }

        public void UpdatePredefinitions(int profile)
        {
            n_bits = a_n_bits[profile];
            n_size = a_n_size[profile];
            n_size_m1 = a_n_size_m1[profile];
            n_size_sq = a_n_size_sq[profile];
            n_size_sq_m1 = a_n_size_sq_m1[profile];

            np_bits = a_np_bits[profile];
            np_size = a_np_size[profile];
            np_size_m1 = a_np_size_m1[profile];
            np_size_sq = a_np_size_sq[profile];
            np_size_sq_m1 = a_np_size_sq_m1[profile];

            waterl0 = a_waterl0[profile];
            waterl1 = waterl0 * 2;
            waterl2 = waterl0 * 2;
            waterr0 = waterl0 / 2;              // ring0包括tcorner的半径
            waterr1 = waterr0 + waterl1;        // 全部ring1的半径300米
            waterr2 = waterr1 + waterl2;        // 全部ring2的半径500米
            waterr = waterr2;                   // 整个水的半径是500米

            waterlv0 = a_waterlv0[profile];
            waterlv1 = a_waterlv1[profile];
            waterlv2 = a_waterlv2[profile];
            waterlv0sq = a_waterlv0sq[profile];
            waterlv1sq = a_waterlv1sq[profile];
            waterlv2sq = a_waterlv2sq[profile];

            vspacing0 = waterl0 / (waterlv0 - 1);
            vspacing1 = waterl1 / (waterlv1 - 1);
            vspacing2 = waterl2 / (waterlv2 - 1);
        }

        private int pos2i(int x, int y)
        {
            return np_size * (y) + (x);
        }
    }
}
