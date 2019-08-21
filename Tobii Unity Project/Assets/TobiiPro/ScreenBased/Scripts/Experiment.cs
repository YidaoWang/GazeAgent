using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum ExperimentType
{
    P = 0,
    A = 1,
    AC = 2,
    F = 3,
    FC = 4,
    N = 5,
    C = 6
}

public class Experiment
{
    public ExperimentType ExperimentType { get; set; }
    public int Number { get; set; }
    public string Respondent { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? ResponseTime { get; set; }
    public string ImageFile { get; set; }
    public string GazeDataFile { get; set; }
    public bool CorrectAnswer { get; set; }
    public bool Answer { get; set; }
    public bool IsPractice { get; set; }
    public TimeSpan TimeSpan { get; set; }
    public int ObjectNumber
    {
        get
        {
            if(ImageFile != null)
            {
                return int.Parse(ImageFile.Substring(8, 2));
            }
            else
            {
                return 0;
            }
        }
    }

    public Experiment()
    {

    }

    public Experiment(ExperimentType experimentType, int number, string imagePath, bool correctAnswer, bool isPractice = false)
    {
        ExperimentType = experimentType;
        Number = number;
        ImageFile = imagePath;
        CorrectAnswer = correctAnswer;
        IsPractice = isPractice;
    }

    public void Start()
    {
        StartTime = DateTime.Now;
    }

    public void Finish(string respondent, bool answer, string gazeDataFile)
    {
        ResponseTime = DateTime.Now;
        Respondent = respondent;
        Answer = answer;
        GazeDataFile = gazeDataFile;
    }
}

