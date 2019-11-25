using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace XMLAnalytics
{
    class Program
    {
        static string strCurDir = Environment.CurrentDirectory;
        static string strDataDir = strCurDir + "/Data";
        static string strOutDir = strCurDir + "/Out";

        static void Main(string[] args)
        {
            Aggerage(new int[] { 1, 2, 3, 4 });
            Aggerage(new int[] { 6, 7, 8, 11 });
            Aggerage(new int[] { 1, 2, 3, 4, 6, 7, 8, 11 });
            Csv(new int[] { 1, 2, 3, 4, 6, 7, 8, 11 });
        }

        static void Csv(int[] array)
        {
            var xmlList = new List<Experiment>();
            foreach (var i in array)
            {
                var folder1 = string.Format(strDataDir + "/{0}-{1}", i, 1);
                var folder2 = string.Format(strDataDir + "/{0}-{1}", i, 2);
                var xml1 = Directory.GetFiles(folder1, "experiment_*")[0];
                var xml2 = Directory.GetFiles(folder2, "experiment_*")[0];
                xmlList.AddRange(XmlAnalytics.GetExperimentList(xml1, xml2));
            }
            var csv = XmlAnalytics.ExperimentListToCSV(xmlList);
            IO.WriteToFile(csv, strOutDir + "/out.csv");
        }

        static void Aggerage(int[] array)
        {
            var xmlList = new List<Experiment>();
            foreach (var i in array)
            {
                var folder1 = string.Format(strDataDir + "/{0}-{1}", i, 1);
                var folder2 = string.Format(strDataDir + "/{0}-{1}", i, 2);
                var xml1 = Directory.GetFiles(folder1, "experiment_*")[0];
                var xml2 = Directory.GetFiles(folder2, "experiment_*")[0];
                xmlList.AddRange(XmlAnalytics.GetExperimentList(xml1, xml2));
            }
            
            var csv = XmlAnalytics.ExperimentListToAggerageCSV(xmlList);
            IO.WriteToFile(csv, strOutDir + "/aggerage.csv");
        }
    }
}
