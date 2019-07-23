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

        public VideoMediaData(Color32[] colors)
        {
            Colors = colors;
        }

        public VideoMediaData(byte[] data)
        {
            if(data[0] != (byte)MediaCondition)
            {
                return;
            }
            int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
            int length = (data.Length - 1)/ lengthOfColor32;
            Colors = new Color32[length];

            GCHandle handle = default(GCHandle);
            try
            {
                handle = GCHandle.Alloc(Colors, GCHandleType.Pinned);
                IntPtr ptr = handle.AddrOfPinnedObject();
                Marshal.Copy(data, 1, ptr, data.Length - 1);
            }
            finally
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
        }

        public Color32[] Colors { get; set; }

        public byte[] ToBytes()
        {
            if (Colors == null || Colors.Length == 0)
                return null;

            int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
            int length = lengthOfColor32 * Colors.Length;
            byte[] bytes = new byte[length + 1];
            bytes[0] = (byte)MediaCondition;

            GCHandle handle = default(GCHandle);
            try
            {
                handle = GCHandle.Alloc(Colors, GCHandleType.Pinned);
                IntPtr ptr = handle.AddrOfPinnedObject();
                Marshal.Copy(ptr, bytes, 1, length);
            }
            finally
            {
                if (handle != default(GCHandle))
                    handle.Free();
            }

            return bytes;
        }

        public void PrintColors(int length)
        {
            PrintColors(Colors, length);
        }

        public static void PrintColors(Color32[] colors, int length)
        {
            string str = "";
            for (int i = 1000; i < 1000 + length; i++)
            {
                str += string.Format("({0},{1},{2},{3})", colors[i].r,colors[i].g, colors[i].b, colors[i].a);
            }
            Debug.Log(str);
        }

        public static void PrintBytes(byte[] bytes,int length)
        {
            string str = "";
            for (int i = 4000; i < 4000 + length; i++)
            {
                str += bytes[i];
            }
            Debug.Log(str);
        }
    }
}
