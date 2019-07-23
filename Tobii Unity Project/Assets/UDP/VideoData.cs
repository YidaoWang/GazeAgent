using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.UDP
{
    public class VideoMediaData : IMediaData
    {
        private byte[] data;

        public MediaCondition MediaCondition => MediaCondition.F;

        public VideoMediaData(byte[] data)
        {
            this.data = data;
        }

        public Color32[] Colors { get; set; }
        public Vector2[] FaceLandmark { get; set; }

        public byte[] ToBytes()
        {
            throw new NotImplementedException();
        }
    }
}
