using Assets.UDP;
using DlibFaceLandmarkDetectorExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.TobiiPro.ScreenBased.Scripts
{
    public class RenderingManager
    {
        public bool RemoteFlg { get; set; }

        string remoteHost { get; set; }
        int remotePort { get; set; }

        string localHost { get; set; }
        int localPort { get; set; }

        private Texture2D texture;

        public UDPSystem UdpSystem { get; set; }

        public Agent agent;

        public RenderingManager(bool remoteFlg, Texture2D texture, Agent agent)
        {
            RemoteFlg = remoteFlg;
            this.texture = texture;
            this.agent = agent;
            localHost = "192.168.3.6";
            localPort = 5001;
            remoteHost = "192.168.3.6";
            remotePort = 5002;

            UdpSystem = new UDPSystem((x) => Receive(x));
            UdpSystem.Set(localHost, localPort, remoteHost, remotePort);
            UdpSystem.Receive();
        }

        public void Receive(byte[] data)
        {
            if(data == null || data[0] != (byte)ConditionSettings.MediaCondition)
            {
                return;
            }
            switch (ConditionSettings.MediaCondition)
            {
                case MediaCondition.A:
                   
                    var agentData = new AgentData(data);
                    agent.DrawAgent(texture, agentData.FaceLandmark, agentData.GazePoint);
                    break;
                case MediaCondition.F:
                    break;
                default:
                    break;
            }
        }

        public void Commit(IMediaData data)
        {
            if (!RemoteFlg)
            {
                Receive(data.ToBytes());
            }
            else
            {
                UdpSystem.Send(data.ToBytes());
            }
        }



    }
}
