
using Assets.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentSystem : MonoBehaviour
{
    public List<Experiment> ExperimentList { get; private set; }
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
        ExperimentList = new List<Experiment>();
        var imgs = new int[4];
        System.Random r = new System.Random();

        foreach (ExperimentType et in ExperimentSettings.ExperimentOrder)
        {
            print(et);
            for (var i = 0; i < ExperimentSettings.RepeatNumber; i++)
            {
                var rand = r.Next(4);
                var imgPath = Application.dataPath + "/images";
                var ca = false;
                switch (rand)
                {
                    case 0:
                        imgPath += "/35/T/" + imgs[rand] + ".png";
                        ca = true;
                        break;
                    case 1:
                        imgPath += "/35/F/" + imgs[rand] + ".png";
                        break;
                    case 2:
                        imgPath += "/21/T/" + imgs[rand] + ".png";
                        ca = true;
                        break;
                    case 3:
                        imgPath += "/21/F/" + imgs[rand] + ".png";
                        break;
                }
                imgs[rand]++;
                ExperimentList.Add(new Experiment(et, imgPath, ca));
            }
        }
        CurrentIndex = 0;
        StartExperiment();
    }

    public void StartExperiment()
    {
        var img = GameObject.Find("Canvas/Task").GetComponent<Image>();

        var texture = FileManager.LoadPNG(CurrentExperiment.ImageFile);
        img.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

        var cs = GameObject.Find("ConditionSettings").GetComponent<ConditionSettings>();
        Debug.Log(CurrentExperiment.ExperimentType);
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
        print(CurrentIndex);
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

