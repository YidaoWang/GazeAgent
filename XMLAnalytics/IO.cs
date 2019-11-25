using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLAnalytics
{
    public static class IO
    {
        public static void WriteToFile(string str, string path)
        {
            using (StreamWriter file = new StreamWriter(path, true, Encoding.UTF8))
            {
                file.WriteLine(str);
            }
        }
    }
}
