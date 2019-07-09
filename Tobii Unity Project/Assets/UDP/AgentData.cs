using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.UDP
{
    public class AgentData : IMediaData
    {
        public const byte MEDIATYPE = 0;
        public Vector2 GazePoint { get; set; }
        public Vector2[] FaceLandmark { get; set; }

        public AgentData(Vector2[] faceLandmark, Vector2 gazePoint)
        {
            FaceLandmark = faceLandmark;
            GazePoint = gazePoint;
        }

        public AgentData(byte[] agentData)
        {
            
            if (agentData[0] != MEDIATYPE) return;

            var floatArray = new float[(agentData.Length - 1) / 4];
            Buffer.BlockCopy(agentData, 1, floatArray, 0, agentData.Length - 1);

            GazePoint = new Vector2(floatArray[0], floatArray[1]);
            
            FaceLandmark = new Vector2[floatArray.Length / 2 - 1];
            for (var i = 0; i < FaceLandmark.Length; i++)
            {
                FaceLandmark[i] = new Vector2(floatArray[i * 2 + 2], floatArray[i * 2 + 3]);
            }
        }

        public byte[] ToBytes()
        {
            var landmarkLength = FaceLandmark != null ? FaceLandmark.Length : 0;

            var floatArray = new float[(landmarkLength + 1) * 2];


            floatArray[0] = GazePoint.x;
            floatArray[1] = GazePoint.y;
            

            for (var i = 0; i < landmarkLength; i++)
            {
                floatArray[i * 2 + 2] = FaceLandmark[i].x;
                floatArray[i * 2 + 3] = FaceLandmark[i].y;
            }

            var byteArray = new byte[floatArray.Length * 4 + 1];
            byteArray[0] = MEDIATYPE;
            Buffer.BlockCopy(floatArray, 0, byteArray, 1, byteArray.Length - 1);

            return byteArray;
        }
    }
}
