using Assets.TobiiPro.ScreenBased.Scripts;
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

    int PracticeNumber = 16;

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

        remoteIP.text = "136.187.82.148";
        repeatNumber.text = "80";
        experimentOrder.text = "1";

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
            ExperimentOrder = new int[experimentOrder.Length];
            for (int i = 0; i < experimentOrder.Length; i++)
            {
                ExperimentOrder[i] = experimentOrder[i] - '0';
            }
            RepeatNumber = int.Parse(repeatNumber);
            return true;
        }
    }

    void SetExperimentList()
    {
        var pexperimentList = new List<Experiment>();
        var experimentList = new List<Experiment>();
        System.Random r = new System.Random();

        // 各問題の練習問題カウンタ
        var pcounters = new int[] { 0, 0, 0, 0 };
        // 各問題のカウンタ
        var counters = new int[] { 0, 0, 0, 0 };
        // 各問題の上限
        var pmax = PracticeNumber / 4;
        var max = RepeatNumber / 4;

        foreach (ExperimentType et in ExperimentOrder)
        {
            for (var i = 0; i < pmax; i++)
            {
                pexperimentList.Add(new Experiment(et, i * 4, "/images/35/T/" + i + ".png", true, true));
                pexperimentList.Add(new Experiment(et, i * 4 + 1, "/images/35/F/" + i + ".png", false, true));
                pexperimentList.Add(new Experiment(et, i * 4 + 2, "/images/21/T/" + i + ".png", true, true));
                pexperimentList.Add(new Experiment(et, i * 4 + 3, "/images/21/F/" + i + ".png", false, true));
            }
            for (var i = pmax; i < pmax + max; i++)
            {
                experimentList.Add(new Experiment(et, i * 4, "/images/35/T/" + i + ".png", true));
                experimentList.Add(new Experiment(et, i * 4 + 1, "/images/35/F/" + i + ".png", false));
                experimentList.Add(new Experiment(et, i * 4 + 2, "/images/21/T/" + i + ".png", true));
                experimentList.Add(new Experiment(et, i * 4 + 3, "/images/21/F/" + i + ".png", false));
            }

            // シャッフル
            pexperimentList = pexperimentList.OrderBy(i => Guid.NewGuid()).ToList();
            experimentList = experimentList.OrderBy(i => Guid.NewGuid()).ToList();
            // 結合
            pexperimentList.AddRange(experimentList);
        }
        foreach (var t in pexperimentList)
        {
            print(t.ImageFile);
        }
        ExperimentSystem.PracticeNumber = PracticeNumber;
        ExperimentSystem.ExperimentList = pexperimentList;
    }
}
