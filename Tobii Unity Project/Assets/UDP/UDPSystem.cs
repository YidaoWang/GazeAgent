using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using UnityEngine;

namespace Assets.UDP
{
    public class UDPSystem
    {
        private class IPandPort
        {
            IPandPort(string ipAddr, int port)
            {
                this.ipAddr = ipAddr;
                this.port = port;
            }
            public string ipAddr;
            public int port;
            public UdpClient udpClient;
        }
        readonly int RETRY_SEND_TIME = 10; // ms

        static byte sendTaskCount = 0;
        static List<IPandPort> recList = new List<IPandPort>();

        bool finishFlag = false;
        bool onlyFlag = false;

        int sendHostPort = 6001;
        int sendHostPortRange = 0;

        Action<byte[]> callBack;


        public string localIP, remoteIP;
        public int localPort = 5000, remotePort = 5000;

        UdpClient udpClientSend;
        UdpClient tmpReceiver; //受信終了用TMP

        public UDPSystem(Action<byte[]> callback)
        {
            callBack = callback;

        }
        public UDPSystem(string local_ip, int localport, string remote_ip, int remoteport, Action<byte[]> callback, bool onlyflag = false) //オーバーロード 2
        {
            /* rec,send IP == null -> AnyIP */

            localIP = local_ip;
            remoteIP = remote_ip;
            localPort = localport;
            remotePort = remoteport;
            callBack = callback;
            onlyFlag = onlyflag;
        }

        public void Set(string local_ip, int localport, string remote_ip, int remoteport, Action<byte[]> callback = null)
        {
            localIP = local_ip;
            remoteIP = remote_ip;
            localPort = localport;
            remotePort = remoteport;
            if (callback != null) callBack = callback;
        }
        public void SetSendHostPort(int port, int portRange = 0) //送信用 自己ポート設定
        {
            sendHostPort = port;
            sendHostPortRange = portRange;
        }

        int GetSendHostPort()
        {
            if (sendHostPortRange == 0) return sendHostPort;
            return UnityEngine.Random.Range(sendHostPort, sendHostPort + 1);
        }
        public void Finish() //エラー時チェック項目 : Close()が2度目ではないか
        {
            if (udpClientSend != null) udpClientSend.Close();
            if (tmpReceiver != null) tmpReceiver.Close();
            else finishFlag = true;
        }
        public void Receive() // ポートの監視を始めます。
        {
            string targetIP = localIP; //受信
            int port = localPort;

            //if (recList.Contains(new IPandPort())) ;

            UdpClient udpClientReceive;

            if (targetIP == null) udpClientReceive = new UdpClient(new IPEndPoint(IPAddress.Any, port));
            else if (targetIP == "") udpClientReceive = new UdpClient(new IPEndPoint(IPAddress.Parse(ScanIPAddr.IP[0]), port));
            else udpClientReceive = new UdpClient(new IPEndPoint(IPAddress.Parse(targetIP), port));

            //udpClientReceive.Connect(IPAddress.Parse(remoteIP), remotePort);
            udpClientReceive.BeginReceive(UDPReceive, udpClientReceive);

            if (targetIP == null) Debug.Log("受信を開始しました。 Any " + IPAddress.Any + " " + port);
            else if (targetIP == "") Debug.Log("受信を開始しました。 Me " + ScanIPAddr.IP[0] + " " + port);
            else Debug.Log("受信を開始しました。" + IPAddress.Parse(targetIP) + " " + port);

            tmpReceiver = udpClientReceive;
        }

        void UDPReceive(IAsyncResult res)
        {// CallBack ポートに着信があると呼ばれます。
            if (finishFlag)
            {
                FinishUDP(res.AsyncState as UdpClient);
                return;
            }

            UdpClient getUdp = (UdpClient)res.AsyncState;
            IPEndPoint ipEnd = null;
            byte[] getByte;

            try
            { //受信成功時アクション
                getByte = getUdp.EndReceive(res, ref ipEnd);
                callBack?.Invoke(getByte);
            }
            catch (SocketException ex)
            {
                Debug.Log("Error" + ex);
                return;
            }
            catch (ObjectDisposedException) // Finish : Socket Closed
            {
                Debug.Log("Socket Already Closed.");
                return;
            }

            if (finishFlag || onlyFlag)
            {
                FinishUDP(getUdp);
                return;
            }


            Debug.Log("Retry");
            getUdp.BeginReceive(UDPReceive, getUdp); // Retry

        }
        private void FinishUDP(UdpClient udp)
        {
            udp.Close();
        }

        public void Send_NonAsync(byte[] sendByte) //同期送信を行います。(未検証＆使用不要)
        {
            if (udpClientSend == null) udpClientSend = new UdpClient(new IPEndPoint(IPAddress.Parse(ScanIPAddr.IP[0]), GetSendHostPort()));
            udpClientSend.EnableBroadcast = true;

            try
            {
                udpClientSend.Send(sendByte, sendByte.Length, remoteIP, remotePort);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        public void Send_NonAsync2(byte[] sendByte) //同期送信を始めます。(2 検証済)
        {
            string targetIP = remoteIP;
            int port = remotePort;

            if (udpClientSend == null)
            {
                udpClientSend = new UdpClient(new IPEndPoint(IPAddress.Parse(ScanIPAddr.IP[0]), GetSendHostPort()));
            }
            udpClientSend.EnableBroadcast = true;
            Socket uSocket = udpClientSend.Client;
            uSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

            if (targetIP == null)
            {
                udpClientSend.Send(sendByte, sendByte.Length, new IPEndPoint(IPAddress.Broadcast, remotePort));
                //Debug.Log("送信処理しました。" + ScanIPAddr.IP[0] + " > BroadCast " + IPAddress.Broadcast + ":" + remotePort);
            }
            else
            {
                udpClientSend.Send(sendByte, sendByte.Length, new IPEndPoint(IPAddress.Parse(targetIP), remotePort));
                //Debug.Log("送信処理しました。" + ScanIPAddr.IP[0] + " > " + IPAddress.Parse(targetIP) + ":" + remotePort);
            }
        }
        public void Send(byte[] sendByte, byte retryCount = 0) //非同期送信をUdpClientで開始します。(通常) <retry>
        {
            if (sendByte == null) return;
            string targetIP = remoteIP;
            int port = remotePort;

            //if (sendTaskCount > 0)//送信中タスクの確認。 送信中有の場合、定数時間後リトライ
            //{

            //    Debug.Log("SendTask is There.[" + retryCount);
            //    retryCount++;

            //    if (retryCount > 10)
            //    {
            //        Debug.LogError("Retry OverFlow.");
            //        return;
            //    }

            //    Timer timer = new Timer(RETRY_SEND_TIME);
            //    timer.Elapsed += delegate (object obj, ElapsedEventArgs e) { Send(sendByte, retryCount); timer.Stop(); };
            //    timer.Start();
            //    return;
            //}
            //sendTaskCount++; //送信中タスクを増加

            if (udpClientSend == null)
                udpClientSend = new UdpClient(new IPEndPoint(IPAddress.Parse(ScanIPAddr.IP[0]), GetSendHostPort()));

            if (targetIP == null)
            {
                udpClientSend.BeginSend(sendByte, sendByte.Length, new IPEndPoint(IPAddress.Broadcast, remotePort), UDPSender, udpClientSend);
                //Debug.Log("送信処理しました。" + ScanIPAddr.IP[0] + " > BroadCast " + IPAddress.Broadcast + ":" + remotePort);
            }
            else
            {
                udpClientSend.BeginSend(sendByte, sendByte.Length, remoteIP, remotePort, UDPSender, udpClientSend);
                //Debug.Log("送信処理しました。" + ScanIPAddr.IP[0] + " > " + IPAddress.Parse(targetIP) + ":" + remotePort + "[" + sendByte[0] + "][" + sendByte[1] + "]...");
            }
        }

        void UDPSender(IAsyncResult res)
        {
            UdpClient udp = (UdpClient)res.AsyncState;
            try
            {
                udp.EndSend(res);
                Debug.Log("Send");
            }
            catch (SocketException ex)
            {
                Debug.Log("Error" + ex);
                return;
            }
            catch (ObjectDisposedException) // Finish : Socket Closed
            {
                Debug.Log("Socket Already Closed.");
                return;
            }

            sendTaskCount--;
            udp.Close();

        }


    }

    public class ScanIPAddr
    {
        public static string[] IP { get { return Get(); } }
        public static byte[][] ByteIP { get { return GetByte(); } }

        public static string[] Get()
        {
            IPAddress[] addr_arr = Dns.GetHostAddresses(Dns.GetHostName());
            List<string> list = new List<string>();
            foreach (IPAddress address in addr_arr)
            {
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    list.Add(address.ToString());
                }
            }
            if (list.Count == 0) return null;
            return list.ToArray();
        }
        public static byte[][] GetByte()
        {
            IPAddress[] addr_arr = Dns.GetHostAddresses(Dns.GetHostName());
            List<byte[]> list = new List<byte[]>();
            foreach (IPAddress address in addr_arr)
            {
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    list.Add(address.GetAddressBytes());
                }
            }
            if (list.Count == 0) return null;
            return list.ToArray();
        }
    }
}
