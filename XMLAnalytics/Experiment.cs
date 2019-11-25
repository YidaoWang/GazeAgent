using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum ExperimentType
{
    A = 0,
    F = 1,
    N = 2,
    C = 3
}

public class Experiment
{
    public ExperimentType ExperimentType { get; set; }
    public int Number { get; set; }
    public string Ip1 { get; set; }
    public string Ip2 { get; set; }
    public DateTime? StartTime { get; set; }
    public TimeSpan TimeSpan1 { get; set; }
    public TimeSpan TimeSpan2 { get; set; }
    public string ImageFile { get; set; }
    public string GazeDataFile1 { get; set; }
    public string GazeDataFile2 { get; set; }
    public int Respondant { get; set; }
    public bool CorrectAnswer { get; set; }
    public bool Answer { get; set; }
    public bool IsPractice { get; set; }
    public TimeSpan RespondantTimeSpan
    {
        get
        {
            return Respondant == 1 ? TimeSpan1 : TimeSpan2;
        }
    }
    public int ObjectNumber
    {
        get
        {
            if (ImageFile != null)
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
}

