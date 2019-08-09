﻿using Assets.TobiiPro.ScreenBased.Scripts;
using Assets.UDP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using Tobii.Research;
using Tobii.Research.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneSystem : MonoBehaviour
{
    private int[] ExperimentOrder;
    private int RepeatNumber;

    public SynchronizationContext MainContext { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        MainContext = SynchronizationContext.Current;

        var local = GameObject.Find("MyIP").GetComponent<Dropdown>();
        var ips = ScanIPAddr.IP;

        local.ClearOptions();
        local.AddOptions(ips.OfType<string>().ToList());

        var repeatNumber = GameObject.Find("RepeatNumber").GetComponent<InputField>();
        var experimentOrder = GameObject.Find("ExperimentOrder").GetComponent<InputField>();
        var remoteIP = GameObject.Find("RemoteIP").GetComponent<InputField>();

        remoteIP.text = "136.187.82.0";
        repeatNumber.text = "5";
        experimentOrder.text = "123456";

        Application.quitting += OnQuit;
    }

    private void OnQuit()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void StartOffLine()
    {
        if (LoadExperiment())
        {
            SetExperimentList();
            SceneManager.LoadScene("MainScene");
        }
    }

    public void StartAsServer()
    {
        if (LoadConnection() && LoadExperiment())
        {
            SetExperimentList();
            ExperimentSettings.ServerFlg = true;
            SceneManager.LoadScene("MainScene");
        }
    }

    public void StartAsClient()
    {
        if (LoadConnection())
        {
            Debug.Log(ExperimentSettings.RemoteFlg);
            SceneManager.LoadScene("MainScene");
        }
    }

    public void OnClickFinish()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
    UnityEngine.Application.Quit();
#endif
    }

    private void OnApplicationQuit()
    {
        EyeTracker.Instance.SubscribeToUserPositionGuide = false;
        EyeTrackingOperations.Terminate();
    }

    bool LoadConnection()
    {
        var local = GameObject.Find("MyIP").GetComponent<Dropdown>();
        var remote = GameObject.Find("RemoteIP").GetComponent<InputField>();

        if (remote.text != null)
        {
            ExperimentSettings.RemoteFlg = true;
            ExperimentSettings.LocalAdress = local.options[local.value].text;
            ExperimentSettings.RemoteAdress = remote.text;
            return true;
        }
        else
        {
            return false;
        }
    }

    bool LoadExperiment()
    {
        var experimentOrder = GameObject.Find("ExperimentOrder").GetComponent<InputField>()?.text;
        var repeatNumber = GameObject.Find("RepeatNumber").GetComponent<InputField>()?.text;
        if (string.IsNullOrEmpty(experimentOrder) || string.IsNullOrEmpty(repeatNumber))
        {
            return false;
        }
        else
        {
            ExperimentOrder = new int[6];
            for (int i = 0; i < 6; i++)
            {
                ExperimentOrder[i] = experimentOrder[i] - '0';
            }
            RepeatNumber = int.Parse(repeatNumber);
            return true;
        }
    }

    void SetExperimentList()
    {
        var experimentList = new List<Experiment>();
        var imgs = new int[4];
        System.Random r = new System.Random();

        foreach (ExperimentType et in ExperimentOrder)
        {
            print(et);
            for (var i = 0; i < RepeatNumber; i++)
            {
                var rand = r.Next(4);
                var imgPath = "/images";
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
                experimentList.Add(new Experiment(et, i, imgPath, ca));
            }
        }

        ExperimentSystem.ExperimentList = experimentList;
    }
}
