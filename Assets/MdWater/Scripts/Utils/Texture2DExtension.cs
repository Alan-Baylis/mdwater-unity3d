using UnityEngine;
using System;
using System.Collections;
using System.IO;

namespace MynjenDook
{
    public static class Texture2DExtension
    {
        static public void Save2Tga(this Texture2D tex, string fileName)
        {
            // 写一个16bit的tga文件
            FileStream fsw = new FileStream(fileName, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fsw);
            // tga头文件
            Byte bHeader = 0;
            UInt16 wHeader = 0;
            bw.Write(bHeader);
            bw.Write(bHeader);
            bHeader = 2;                    // 这里要改成2才能explorer.exe正确预览
            bw.Write(bHeader);
            bw.Write(wHeader);
            bw.Write(wHeader);
            bHeader = 0;
            bw.Write(bHeader);
            bw.Write(wHeader);
            bw.Write(wHeader);
            wHeader = (UInt16)tex.width;    // 图片宽
            bw.Write(wHeader);
            wHeader = (UInt16)tex.height;   // 图片高
            bw.Write(wHeader);
            bHeader = 32;                   // 深度(每图素4个字节)
            bw.Write(bHeader);
            bHeader = 32;                   // Flip vertically
            bw.Write(bHeader);

            Color[] colors = tex.GetPixels();
            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    Color color = colors[y * tex.width + x];
                    byte a = (byte)(color.a * 255f);
                    byte b = (byte)(color.b * 255f);
                    byte g = (byte)(color.g * 255f);
                    byte r = (byte)(color.r * 255f);
                    uint cc = ((uint)b) + (((uint)g) << 8) + (((uint)r) << 16) + (((uint)a) << 24);
                    bw.Write(cc);
                }
            }
            bw.Close();
            fsw.Close();
        }
    }
}
