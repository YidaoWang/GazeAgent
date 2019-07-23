using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.UDP
{
    public class GazeMediaData : IMediaData
    {
        public MediaCondition MediaCondition => MediaCondition.N;
        public Vector2 GazePoint { get; set; }

        public GazeMediaData(Vector2 gazePoint)
        {
            GazePoint = gazePoint;
        }

        public GazeMediaData(byte[] data)
        {
            if (data[0] != (byte)MediaCondition) return;

            var floatArray = new float[(data.Length - 1) / 4];
            Buffer.BlockCopy(data, 1, floatArray, 0, data.Length - 1);

            GazePoint = new Vector2(floatArray[0], floatArray[1]);
        }

        public byte[] ToBytes()
        {
            var floatArray = new float[2];

            floatArray[0] = GazePoint.x;
            floatArray[1] = GazePoint.y;

            var byteArray = new byte[floatArray.Length * 4 + 1];
            byteArray[0] = (byte)MediaCondition;
            Buffer.BlockCopy(floatArray, 0, byteArray, 1, byteArray.Length - 1);

            return byteArray;
        }
    }
}
