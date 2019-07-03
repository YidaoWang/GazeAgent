using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.UDP
{
    public class AgentData
    {
        public Vector2 GazePoint { get; set; }
        public Vector2[] FaceLandmark { get; set; }

        public AgentData(Vector2[] faceLandmark, Vector2 gazePoint)
        {
            FaceLandmark = faceLandmark;
            gazePoint = GazePoint;
        }

        public AgentData(byte[] agentData)
        {
            var floatArray = new float[agentData.Length / 4];
            Buffer.BlockCopy(agentData, 0, floatArray, 0, agentData.Length);

            GazePoint = new Vector2(floatArray[0], floatArray[1]);
            FaceLandmark = new Vector2[floatArray.Length / 2 - 1];
            for(var i = 0; i < FaceLandmark.Length; i++)
            {
                FaceLandmark[i] = new Vector2(floatArray[i * 2 + 2], floatArray[i * 2 + 3]);
            }
        }

        public byte[] ToBytes()
        {
            var floatArray = new float[(FaceLandmark.Length + 1) * 2];

            floatArray[0] = GazePoint.x;
            floatArray[1] = GazePoint.y;

            for (var i = 0; i < FaceLandmark.Length; i++)
            {
                floatArray[i * 2 + 2] = FaceLandmark[i].x;
                floatArray[i * 2 + 3] = FaceLandmark[i].y;
            }

            var byteArray = new byte[floatArray.Length * 4];
            Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);

            return byteArray;
        }
    }
}
