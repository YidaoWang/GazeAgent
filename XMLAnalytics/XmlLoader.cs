using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XMLAnalytics
{
    public static class XmlAnalytics
    {
        static readonly string IP1 = "136.187.82.176";
        static readonly string IP2 = "136.187.81.41";

        public static List<Experiment> GetExperimentList(string file1, string file2)
        {
            var experimentList = new List<Experiment>();

            var xml1 = XDocument.Load(file1);
            var xml2 = XDocument.Load(file2);

            var elements1 = xml1.Element("Experiments").Elements().ToList();
            var elements2 = xml2.Element("Experiments").Elements().ToList();
            var number = 0;

            for (var i = 0; i < elements1.Count; i++)
            {
                number++;
                var e1 = elements1[i];
                var e2 = elements2[i];

                var type = e1.Attribute("Type").Value;
                var image = e1.Attribute("ImageFile").Value;
                var correctAnswer = e1.Attribute("CorrectAnswer").Value;
                var startTime = e1.Attribute("StartTime").Value;
                var timeSpan1 = e1.Attribute("TimeSpan").Value;
                var timeSpan2 = e2.Attribute("TimeSpan").Value;
                var respondant = e1.Attribute("Respondent").Value == IP1 ? 1 : 2;
                var answer = e1.Attribute("Answer").Value;
                var gazeDataFile1 = e1.Attribute("GazeDataFile").Value;
                var gazeDataFile2 = e2.Attribute("GazeDataFile").Value;

                var experiment = new Experiment()
                {
                    ExperimentType = (ExperimentType)Enum.Parse(typeof(ExperimentType), type),
                    Number = number,
                    Respondant = respondant,
                    StartTime = DateTime.ParseExact(startTime, "yyyyMMddTHHmmss", null),
                    ImageFile = image,
                    GazeDataFile1 = gazeDataFile1,
                    GazeDataFile2 = gazeDataFile2,
                    CorrectAnswer = correctAnswer == "True",
                    Answer = answer == "True",
                    IsPractice = number <= 16,
                    TimeSpan1 = TimeSpan.FromMilliseconds(double.Parse(timeSpan1)),
                    TimeSpan2 = TimeSpan.FromMilliseconds(double.Parse(timeSpan2))
                };
                experimentList.Add(experiment);
            }
            return experimentList;
        }

        public static string ExperimentListToCSV(List<Experiment> experimentList)
        {
            var csv = string.Empty;
            var csvheader = "メディア,実験番号,ターゲット数,ターゲット有無,正解判定,回答者回答時間\n";
            csv += csvheader;
            for (int i = 0; i < experimentList.Count; i++)
            {
                var exp = experimentList[i];
                if (exp.IsPractice) continue;
                var newLine = string.Format("{0},{1},{2},{3},{4},{5}\n",
                    exp.ExperimentType,
                    exp.Number,
                    exp.ObjectNumber,
                    exp.CorrectAnswer,
                    exp.CorrectAnswer == exp.Answer,
                    exp.RespondantTimeSpan.TotalMilliseconds);
                csv += newLine;
            }
            return csv;
        }

        public static string ExperimentListToAggerageCSV(List<Experiment> experimentList)
        {
            var csv = string.Empty;
            var csvheader = "メディア,ターゲット有無,誤答率,回答時間平均,標準偏差,標準誤差\n";
            csv += csvheader;

            for (int b = 0; b < 2; b++)
            {
                var correctAnswer = (b == 0);
                for (int j = 0; j < 4; j++)
                {
                    double sum = 0;
                    double squaredSum = 0;
                    int correctSum = 0;
                    int count = 0;
                    for (int i = 0; i < experimentList.Count; i++)
                    {
                        var exp = experimentList[i];
                        if (!exp.IsPractice && (int)exp.ExperimentType == j && exp.CorrectAnswer == correctAnswer)
                        {
                            count++;
                            if (exp.Answer == exp.CorrectAnswer)
                                correctSum++;
                            sum += exp.RespondantTimeSpan.TotalMilliseconds;
                            squaredSum += exp.RespondantTimeSpan.TotalMilliseconds * exp.RespondantTimeSpan.TotalMilliseconds;
                        }
                    }
                    var average = sum / count;
                    var variance = squaredSum / count - average * average;
                    var sigma = Math.Sqrt(variance);
                    var SE = sigma / Math.Sqrt(count);
                    var newLine = string.Format("{0},{1},{2},{3},{4},{5}\n",
                        (ExperimentType)j, correctAnswer, 1 - (double)correctSum / count, average, sigma, SE);
                    csv += newLine;
                }
            }
            return csv;
        }

    }
}
