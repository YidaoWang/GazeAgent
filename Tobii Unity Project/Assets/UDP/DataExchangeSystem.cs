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
    public class DataExchangeSystem : MonoBehaviour
    {
        [SerializeField, TooltipAttribute("Set remote condition.")]
        public bool RemoteFlg;

        public SynchronizationContext MainContext { get; private set; }

        public UDPSystem UdpSystem { get; set; }

        public delegate void OnReceiveEventHandler(IMediaData data);
        public event OnReceiveEventHandler OnReceive;

        public GazeMediaData LatestGazeData { get; set; }
        //public AgentMediaData LatestAgentData { get; set; }
        //public VideoMediaData LatestVideoData { get; set; }

        void Start()
        {
            RemoteFlg = false;
            MainContext = SynchronizationContext.Current;
        }

        public void SetUDP(string localadress, string remoteadress)
        {
            RemoteFlg = true;
            if (UdpSystem != null)
            {
                UdpSystem.Finish();
            }

            var localipport = localadress.Split(':');
            var remoteipport = remoteadress.Split(':');

            var localip = localipport[0];
            var localport = int.Parse(localipport[1]);
            var remoteip = remoteipport[0];
            var remoteport = int.Parse(remoteipport[1]);

            UdpSystem = new UDPSystem((x) => Receive(x));
            UdpSystem.Set(localip, localport, remoteip, remoteport);
            UdpSystem.Receive();
        }

        public void FinishUDP()
        {
            RemoteFlg = false;
            if (UdpSystem != null)
            {
                UdpSystem.Finish();
            }
        }

        public void Receive(byte[] data)
        {
            if (data == null)
            {
                return;
            }
            MainContext.Post(_ =>
            {
                ReceiveOnMainContext(data);
            }, null);
        }

        private void ReceiveOnMainContext(byte[] data)
        {
            switch ((MediaCondition)data[0])
            {
                case MediaCondition.A:
                    //LatestAgentData = new AgentMediaData(data);
                    OnReceive?.Invoke(new AgentMediaData(data));
                    break;
                case MediaCondition.F:
                    //LatestVideoData = new VideoMediaData(data);
                    OnReceive?.Invoke(new VideoMediaData(data));
                    break;
                default:
                    LatestGazeData = new GazeMediaData(data);
                    OnReceive?.Invoke(LatestGazeData);
                    break;
            }
        }

        public void Post(IMediaData data)
        {
            if (!RemoteFlg)
            {
                ReceiveOnMainContext(data.ToBytes());
            }
            else
            {
                UdpSystem.Send_NonAsync2(data.ToBytes());
            }
        }
    }
}
