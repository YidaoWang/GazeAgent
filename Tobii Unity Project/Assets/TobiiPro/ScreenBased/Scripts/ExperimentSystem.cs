
using Assets.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentSystem : MonoBehaviour
{
    public static List<Experiment> ExperimentList { get; set; }
    public int CurrentIndex { get; private set; }
    public Experiment CurrentExperiment
    {
        get
        {
            return ExperimentList[CurrentIndex];
        }
    }

    void Start()
    {      
        StartExperiment();
    }

    public void StartExperiment()
    {
        var img = GameObject.Find("Canvas/Task").GetComponent<Image>();

        var texture = FileManager.LoadPNG(CurrentExperiment.ImageFile);
        img.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

        var cs = GameObject.Find("ConditionSettings").GetComponent<ConditionSettings>();
        switch (CurrentExperiment.ExperimentType)
        {
            case ExperimentType.P:
                cs.COnClick();
                break;
            case ExperimentType.A:
                
                cs.AOnClick();
                break;
            case ExperimentType.AC:
                cs.ACOnClick();
                break;
            case ExperimentType.F:
                cs.FOnClick();
                break;
            case ExperimentType.FC:
                cs.FCOnClick();
                break;
            case ExperimentType.N:
                cs.NOnClick();
                break;
            case ExperimentType.C:
                cs.COnClick();
                break;
        }
        CurrentExperiment.Start();
    }

    public void OnClickT()
    {
        Next(true);
    }

    public void OnClickF()
    {
        Next(false);
    }

    public void Next(bool answer)
    {
        CurrentExperiment.Finish(ExperimentSettings.LocalAdress, answer, null);
        CurrentIndex++;
        if(CurrentIndex >= ExperimentList.Count)
        {
            // Finish Experiment.
        }
        else
        {
            StartExperiment();
        }
    } 

}

