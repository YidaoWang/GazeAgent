using Assets.TobiiPro.ScreenBased.Scripts;
using Assets.UDP;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneSystem : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var local = GameObject.Find("MyIP").GetComponent<Dropdown>();
        var ips = ScanIPAddr.IP;
        for(int i = 0; i < ips.Length; i++)
        {
            ips[i] += ":5000";
        }
        local.ClearOptions();
        local.AddOptions(ips.OfType<string>().ToList());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickStart()
    {
        var experimentOrder = GameObject.Find("ExperimentOrder").GetComponent<InputField>()?.text;
        var repeatNumber = GameObject.Find("RepeatNumber").GetComponent<InputField>()?.text;

        if(string.IsNullOrEmpty(experimentOrder) || string.IsNullOrEmpty(repeatNumber))
        {

        }
        else
        {
            ExperimentSettings.ExperimentOrder = new int[6];
            for (int i = 0; i < 6; i++)
            {
                ExperimentSettings.ExperimentOrder[i] = experimentOrder[i] - '0';
            }
            ExperimentSettings.RepeatNumber = int.Parse(repeatNumber);
        } 

        SceneManager.LoadScene("MainScene");
    }

    public void OnClickFinish()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
    UnityEngine.Application.Quit();
#endif
    }

    public void Connect()
    {
        var local = GameObject.Find("MyIP").GetComponent<Dropdown>();
        var remote = GameObject.Find("RemoteIP").GetComponent<InputField>();

        ExperimentSettings.RemoteFlg = true;
        ExperimentSettings.LocalAdress = local.options[local.value].text;
        ExperimentSettings.RemoteAdress = remote.text;
    }

}
