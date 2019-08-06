﻿using Assets.UDP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class ExperimentSettings
{
    public static bool RemoteFlg { get; set; }
    public static bool ServerFlg { get; set; }
    public static string LocalAdress { get; set; }
    public static string RemoteAdress { get; set; }

    public static int CommandPort { get { return 5000; } }
    public static int CommandSendHostPort { get { return 6000; } }
    public static int DataPort { get { return 5001; } }
    public static int DataSendHostPort { get { return 6001; } }

    public static void SetCommandUDP(UDPSystem udp, Action<byte[]> callback)
    {
        if (udp != null)
        {
            udp.Finish();
        }
        udp = new UDPSystem(callback);
        udp.Set(LocalAdress,
            CommandPort,
            RemoteAdress,
            CommandPort,
            CommandSendHostPort);
        udp.Receive();
    }


    public static void SetDataUDP(UDPSystem udp, Action<byte[]> callback)
    {
        if (udp != null)
        {
            udp.Finish();
        }
        udp = new UDPSystem(callback);
        udp.Set(
            LocalAdress,
            DataPort,
            RemoteAdress,
            DataPort,
            DataSendHostPort);
        udp.Receive();
    }


}

