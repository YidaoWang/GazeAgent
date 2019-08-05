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
        public SynchronizationContext MainContext { get; private set; }

        public UDPSystem UdpSystem { get; set; }

        public delegate void OnReceiveEventHandler(IMediaData data);
        public event OnReceiveEventHandler OnReceive;

        public GazeMediaData LatestGazeData { get; set; }

        void Start()
        {
            MainContext = SynchronizationContext.Current;
            if (ExperimentSettings.RemoteFlg)
            {
                SetUDP();
            }
        }

        public void SetUDP()
        {
            if (UdpSystem != null)
            {
                UdpSystem.Finish();
            }
            UdpSystem = new UDPSystem((x) => Receive(x));
            UdpSystem.Set(
                ExperimentSettings.LocalAdress,
                ExperimentSettings.DataPort,
                ExperimentSettings.RemoteAdress,
                ExperimentSettings.DataPort,
                ExperimentSettings.DataSendHostPort
                );
            UdpSystem.Receive();
        }

        public void FinishUDP()
        {
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
                    var videoData = new VideoMediaData(data);
                    OnReceive?.Invoke(videoData);
                    break;
                case MediaCondition.N:
                    LatestGazeData = new GazeMediaData(data);
                    OnReceive?.Invoke(LatestGazeData);
                    break;
                default:

                    break;
            }
        }

        public void Post(IMediaData data)
        {
            if (!ExperimentSettings.RemoteFlg)
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
