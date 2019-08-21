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
        static void Main(string[] args)
        {
            string folder1 = "Data/b1_1/";
            string folder2 = "Data/b1_2/";
            var files1 = Directory.GetFiles(folder1);
            var files2 = Directory.GetFiles(folder2);
            var experimentList1 = getExperimentList(files1);
            var experimentList2 = getExperimentList(files2);
            var csv = string.Empty;
            var csvheader = "メディア,実験番号,ターゲット数,ターゲット有無,回答,回答時間1,回答時間2,回答者,問題\n";
            csv += csvheader;
            for (int i = 0; i < experimentList1.Count; i++)
            {
                var exp1 = experimentList1[i];
                var exp2 = experimentList2[i];
                if (exp1.ExperimentType != exp2.ExperimentType || exp1.Number != exp2.Number)
                {
                    var str = "Err:";
                    str += "EXP1.ExperimentType " + exp1.ExperimentType + "\n";
                    str += "EXP2.ExperimentType " + exp2.ExperimentType + "\n";
                    str += "EXP1.Number " + exp1.Number + "\n";
                    str += "EXP2.Number " + exp2.Number + "\n";
                    throw new Exception(str);
                }
                var respondant = 0;
                if (exp1.Respondent == "136.187.82.214")
                {
                    respondant = 1;
                }
                else if (exp1.Respondent == "136.187.82.148")
                {
                    respondant = 2;
                }
                else
                {
                    respondant = -1;
                }
                var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}\n",
                    exp1.ExperimentType, exp1.Number, exp1.ObjectNumber, exp1.CorrectAnswer,
                    exp1.Answer, exp1.TimeSpan.TotalMilliseconds, exp2.TimeSpan.TotalMilliseconds, respondant, exp1.ImageFile);
                csv += newLine;
            }
            string strCurDir = System.Environment.CurrentDirectory;
            using (StreamWriter file = new StreamWriter(strCurDir + "/Data/out.csv", true, Encoding.UTF8))
            {
                file.WriteLine(csv);
            }
        }


        static List<Experiment> getExperimentList(string[] files)
        {
            var experimentList = new List<Experiment>();
            foreach (var file in files)
            {
                if (!file.Contains("experiment"))
                    continue;
                var xml = XDocument.Load(file);
                var elements = xml.Element("Experiments").Elements();
                foreach (var e in elements)
                {
                    var type = e.Attribute("Type").Value;
                    var number = int.Parse(e.Name.LocalName.Substring(3));
                    var image = e.Attribute("ImageFile").Value;
                    var correctAnswer = e.Attribute("CorrectAnswer").Value;
                    var startTime = e.Attribute("StartTime").Value;
                    var responseTime = e.Attribute("ResponseTime").Value;
                    var timeSpan = e.Attribute("TimeSpan").Value;
                    var respondent = e.Attribute("Respondent").Value;
                    var answer = e.Attribute("Answer").Value;
                    var gazeDataFile = e.Attribute("GazeDataFile").Value;

                    var experiment = new Experiment()
                    {
                        ExperimentType = (ExperimentType)Enum.Parse(typeof(ExperimentType), type),
                        Number = number,
                        Respondent = respondent,
                        StartTime = DateTime.ParseExact(startTime, "yyyyMMddTHHmmss", null),
                        ResponseTime = DateTime.ParseExact(responseTime, "yyyyMMddTHHmmss", null),
                        ImageFile = image,
                        GazeDataFile = gazeDataFile,
                        CorrectAnswer = correctAnswer == "True",
                        Answer = answer == "True",
                        IsPractice = number < 10,
                        TimeSpan = TimeSpan.FromMilliseconds(double.Parse(timeSpan))
                    };
                    experimentList.Add(experiment);
                }
            }
            return experimentList;
        }
    }
}
