using Assets.TobiiPro.ScreenBased.Scripts;
using Assets.UDP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneSystem : MonoBehaviour
{
    private UDPSystem UdpSystem;

    // Start is called before the first frame update
    void Start()
    {
        var local = GameObject.Find("MyIP").GetComponent<Dropdown>();
        var ips = ScanIPAddr.IP;

        local.ClearOptions();
        local.AddOptions(ips.OfType<string>().ToList());

        var repeatNumber = GameObject.Find("RepeatNumber").GetComponent<InputField>();
        var experimentOrder = GameObject.Find("ExperimentOrder").GetComponent<InputField>();

        repeatNumber.text = "5";
        experimentOrder.text = "123456";
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
        if(LoadConnection() && LoadExperiment())
        {
            SetExperimentList();

            SetUDP(data =>
            {

            });
        }
     
    }

    public void StartAsClient()
    {
        LoadConnection();
        SetUDP(data =>
        {

        });
    }

    public void OnClickFinish()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
    UnityEngine.Application.Quit();
#endif
    }

    bool LoadConnection()
    {
        var local = GameObject.Find("MyIP").GetComponent<Dropdown>();
        var remote = GameObject.Find("RemoteIP").GetComponent<InputField>();

        if(remote.text != null)
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
            ExperimentSettings.ExperimentOrder = new int[6];
            for (int i = 0; i < 6; i++)
            {
                ExperimentSettings.ExperimentOrder[i] = experimentOrder[i] - '0';
            }
            ExperimentSettings.RepeatNumber = int.Parse(repeatNumber);
            return true;
        }
    }


    void SetUDP(Action<byte[]> callback)
    {
        if (UdpSystem != null)
        {
            UdpSystem.Finish();
        }

        var localip = ExperimentSettings.LocalAdress;
        var localport = ExperimentSettings.CommandPort;
        var remoteip = ExperimentSettings.RemoteAdress;
        var remoteport = ExperimentSettings.CommandPort;

        UdpSystem = new UDPSystem(callback);
        UdpSystem.Set(localip, localport, remoteip, remoteport);
        UdpSystem.Receive();
    }

    void SetExperimentList()
    {
        var experimentList = new List<Experiment>();
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
                experimentList.Add(new Experiment(et, imgPath, ca));
            }
        }

        ExperimentSystem.ExperimentList = experimentList;
    }
}
