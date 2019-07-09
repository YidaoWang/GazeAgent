using Assets.UDP;
using DlibFaceLandmarkDetectorExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.TobiiPro.ScreenBased.Scripts
{
    public class RenderingManager
    {
        public bool RemoteFlg { get; set; }

        private Texture2D texture;

        public SynchronizationContext MainContext { get; private set; }

        public UDPSystem UdpSystem { get; set; }

        public Agent agent;

        public RenderingManager(Texture2D texture, Agent agent)
        {
            RemoteFlg = false;
            this.texture = texture;
            this.agent = agent;
            MainContext = SynchronizationContext.Current;
        }

        public void SetUDP(string localadress, string remoteadress, bool remoteFlg = true)
        {
            RemoteFlg = remoteFlg;

            var localipport = localadress.Split(':');
            var remoteipport = remoteadress.Split(':');

            var localip = localipport[0];
            var localport = int.Parse(localipport[1]);
            var remoteip = remoteipport[0];
            var remoteport = int.Parse(remoteipport[1]);

            Debug.Log("local:" + localip + ":" + localport);
            Debug.Log("remote:" + remoteip + ":" + remoteport);

            UdpSystem = new UDPSystem((x) => Receive(x));
            UdpSystem.Set(localip, localport, remoteip, remoteport);
            UdpSystem.Receive();
        }

        public void Receive(byte[] data)
        {
            if (data == null || data[0] != (byte)ConditionSettings.MediaCondition)
            {
                return;
            }
            switch (ConditionSettings.MediaCondition)
            {
                case MediaCondition.A:
                    var agentData = new AgentData(data);
                    Debug.Log("landmark:" + agentData.FaceLandmark.Length);
                    Debug.Log("gazepoint:" + agentData.GazePoint);
                    MainContext.Post(_ =>
                    {
                        agent.DrawAgent(texture, agentData.FaceLandmark, agentData.GazePoint);
                    }, null);
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
                UdpSystem.Send_NonAsync2(data.ToBytes());
            }
        }



    }
}
