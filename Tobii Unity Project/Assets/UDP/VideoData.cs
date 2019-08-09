﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.UDP
{
    public class VideoMediaData : IMediaData
    {
        public MediaCondition MediaCondition => MediaCondition.F;

        const int JPG_QUALITY = 40;

        public VideoMediaData(Color32[] colors, int width, int height)
        {
            Texture = new Texture2D(width, height);
            Texture.SetPixels32(colors);
            Texture.Apply(false);
        }

        public VideoMediaData(byte[] data)
        {
            var jpg = new ArraySegment<byte>(data, 1, data.Length - 1);
            Texture = new Texture2D(1, 1);
            Texture.LoadImage(jpg.ToArray());
        }

        public Color32[] GetPixels32(byte alpha)
        {
            var pixels = Texture.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].a = alpha;
            }
            return pixels;
        }

        private Texture2D Texture { get; set; }

        public byte[] ToBytes()
        {
            var data = new List<byte> { (byte)MediaCondition };
            data.AddRange(Texture.EncodeToJPG(JPG_QUALITY));
            return data.ToArray();
        }

        public static void PrintColors(Color32[] colors, int length)
        {
            string str = "";
            for (int i = 0; i < length; i++)
            {
                str += string.Format("({0},{1},{2},{3})", colors[i].r, colors[i].g, colors[i].b, colors[i].a);
            }
            Debug.Log(str);
        }

        public static void PrintBytes(byte[] bytes, int length)
        {
            string str = "";
            for (int i = 0; i < length; i++)
            {
                str += bytes[i];
            }
            Debug.Log(str);
        }

        public void Dispose()
        {
            MonoBehaviour.Destroy(Texture);
        }
    }
}
