using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.UDP
{
    public class AgentMediaData : IMediaData
    {
        public Vector2[] FaceLandmark { get; set; }

        public MediaCondition MediaCondition => MediaCondition.A;

        public AgentMediaData(Vector2[] faceLandmark)
        {
            FaceLandmark = faceLandmark;
        }

        public AgentMediaData(byte[] agentData)
        {      
            if (agentData[0] != (byte)MediaCondition) return;

            var floatArray = new float[(agentData.Length - 1) / 4];
            Buffer.BlockCopy(agentData, 1, floatArray, 0, agentData.Length - 1);      
            
            FaceLandmark = new Vector2[floatArray.Length / 2];
            for (var i = 0; i < FaceLandmark.Length; i++)
            {
                FaceLandmark[i] = new Vector2(floatArray[i * 2], floatArray[i * 2 + 1]);
            }
        }

        public byte[] ToBytes()
        {
            var landmarkLength = FaceLandmark != null ? FaceLandmark.Length : 0;

            var floatArray = new float[landmarkLength * 2];

            for (var i = 0; i < landmarkLength; i++)
            {
                floatArray[i * 2] = FaceLandmark[i].x;
                floatArray[i * 2 + 1] = FaceLandmark[i].y;
            }

            var byteArray = new byte[floatArray.Length * 4 + 1];
            byteArray[0] = (byte)MediaCondition;
            Buffer.BlockCopy(floatArray, 0, byteArray, 1, byteArray.Length - 1);

            return byteArray;
        }

        public void Dispose()
        {
            FaceLandmark = null;
        }
    }
}
