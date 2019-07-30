using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum ExperimentType
{
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
    public string Respondent { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime ResponseTime { get; set; }
    public string ImageFile { get; set; }
    public string GazeDataFile { get; set; }
    public bool CorrectAnswer { get; set; }
    public bool Answer { get; set; }

    public Experiment(ExperimentType experimentType, string imagePath, bool correctAnswer)
    {
        ExperimentType = ExperimentType;
        ImageFile = imagePath;
        CorrectAnswer = correctAnswer;
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

