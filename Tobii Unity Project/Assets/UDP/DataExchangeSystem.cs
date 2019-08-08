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

        public bool RemoteFlg { get; set; }

        void Start()
        {
            MainContext = SynchronizationContext.Current;
            RemoteFlg = false;
            if (ExperimentSettings.RemoteFlg)
            {
                UdpSystem = ExperimentSettings.GetDataUDP(Receive);
                UdpSystem.Receive();
            }
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
            IMediaData mediaData = null;
            switch ((MediaCondition)data[0])
            {
                case MediaCondition.A:
                    mediaData = new AgentMediaData(data);
                    OnReceive?.Invoke(mediaData);
                    mediaData?.Dispose();
                    break;
                case MediaCondition.F:
                    mediaData = new VideoMediaData(data);
                    OnReceive?.Invoke(mediaData);
                    mediaData?.Dispose();
                    break;
                case MediaCondition.N:
                    mediaData = new GazeMediaData(data);
                    LatestGazeData?.Dispose();
                    LatestGazeData = mediaData as GazeMediaData;
                    OnReceive?.Invoke(mediaData);
                    break;
                default:
                    break;
            }
            GC.Collect();
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
