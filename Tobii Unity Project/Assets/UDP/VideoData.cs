using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.UDP
{
    public class VideoData : IMediaData
    {
        public const byte MEDIATYPE = 1;
        public Color32[] Colors { get; set; }
        public Vector2[] FaceLandmark { get; set; }

        public byte[] ToBytes()
        {
            throw new NotImplementedException();
        }
    }
}
