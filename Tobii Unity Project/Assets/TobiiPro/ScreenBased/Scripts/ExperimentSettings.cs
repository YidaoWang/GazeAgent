using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class ExperimentSettings
{
    public static int[] ExperimentOrder { get; set; }
    public static int RepeatNumber { get; set; }

    public static bool RemoteFlg { get; set; }
    public static string LocalAdress { get; set; }
    public static string RemoteAdress { get; set; }

    public static int CommandPort { get { return 5000; } }
    public static int DataPort { get { return 5001; } }
}

