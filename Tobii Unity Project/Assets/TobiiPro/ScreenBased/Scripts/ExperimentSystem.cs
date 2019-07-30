
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
        foreach (var et in ExperimentSettings.ExperimentOrder)
        {
            for (var i = 0; i < ExperimentSettings.RepeatNumber; i++)
            {
                ExperimentList.Add(new Experiment((ExperimentType)et, Application.dataPath + "/images/1.png", true));
            }
        }
        CurrentIndex = 0;
        StartExperiment();
    }

    public void StartExperiment()
    {
        var img = GameObject.Find("Canvas/Task").GetComponent<Image>();

        Debug.Log(CurrentExperiment.ImageFile);
        var texture = FileManager.LoadPNG(CurrentExperiment.ImageFile);
        img.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

        CurrentExperiment.Start();
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

