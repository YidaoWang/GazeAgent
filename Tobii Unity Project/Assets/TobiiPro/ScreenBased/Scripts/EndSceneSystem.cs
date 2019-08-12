using System;
using System.Collections;
using System.Collections.Generic;
using Tobii.Research;
using Tobii.Research.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndSceneSystem : MonoBehaviour
{
    public static double CorrectRate;
    public static double MRespondedTime;

    // Start is called before the first frame update
    void Start()
    {
        var cr = GameObject.Find("CorrectRate").GetComponent<Text>();
        cr.text = "正答率:  " + CorrectRate.ToString("F1") + " ％";
        var mrt = GameObject.Find("RespondTime").GetComponent<Text>();
        mrt.text = "平均回答時間:  " + MRespondedTime.ToString("F2") + " 秒";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Back()
    {
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        SceneManager.LoadScene("StartScene");
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
}
