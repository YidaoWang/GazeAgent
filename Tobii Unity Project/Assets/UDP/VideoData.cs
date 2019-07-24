using System;
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

        const int jpg_width = 640;
        const int jpg_height = 480;

        public VideoMediaData(Color32[] colors)
        {
            Texture = new Texture2D(jpg_width, jpg_height);
            Texture.SetPixels32(colors);
            Texture.Apply(false);
        }

        public VideoMediaData(byte[] data)
        {
            #region レガシー
            //if(data[0] != (byte)MediaCondition)
            //{
            //    return;
            //}
            //int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
            //int length = (data.Length - 1)/ lengthOfColor32;
            //Colors = new Color32[length];

            //GCHandle handle = default(GCHandle);
            //try
            //{
            //    handle = GCHandle.Alloc(Colors, GCHandleType.Pinned);
            //    IntPtr ptr = handle.AddrOfPinnedObject();
            //    Marshal.Copy(data, 1, ptr, data.Length - 1);
            //}
            //finally
            //{
            //    if (handle.IsAllocated)
            //        handle.Free();
            //}
            #endregion
            //PrintBytes(data, 1000);

            var jpg = new byte[data.Length - 1];
            Array.Copy(data, 1, jpg, 0, jpg.Length);

            Texture = new Texture2D(jpg_width, jpg_height);
            Texture.LoadImage(jpg);
        }

        public Color32[] GetPixels32(byte alpha)
        {
            var pixels = Texture.GetPixels32();
            for(int i = 0; i < pixels.Length; i++)
            {
                pixels[i].a = alpha;
            }
            return pixels;
        }

        private Texture2D Texture { get; set; }

        public byte[] ToBytes()
        {
            #region レガシー
            //if (Colors == null || Colors.Length == 0)
            //    return null;

            //int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
            //int length = lengthOfColor32 * Colors.Length;
            //byte[] bytes = new byte[length + 1];
            //bytes[0] = (byte)MediaCondition;

            //GCHandle handle = default(GCHandle);
            //try
            //{
            //    handle = GCHandle.Alloc(Colors, GCHandleType.Pinned);
            //    IntPtr ptr = handle.AddrOfPinnedObject();
            //    Marshal.Copy(ptr, bytes, 1, length);
            //}
            //finally
            //{
            //    if (handle != default(GCHandle))
            //        handle.Free();
            //}
            #endregion

            var jpg = Texture.EncodeToJPG();
            var data = new byte[jpg.Length + 1];

            data[0] = (byte)MediaCondition;
            Array.Copy(jpg, 0, data, 1, jpg.Length);
            
            return data;
        }

        public static void PrintColors(Color32[] colors, int length)
        {
            string str = "";
            for (int i = 0; i < length; i++)
            {
                str += string.Format("({0},{1},{2},{3})", colors[i].r,colors[i].g, colors[i].b, colors[i].a);
            }
            Debug.Log(str);
        }

        public static void PrintBytes(byte[] bytes,int length)
        {
            string str = "";
            for (int i = 0; i < length; i++)
            {
                str += bytes[i];
            }
            Debug.Log(str);
        }
    }
}
