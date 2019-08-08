
using Assets.IO;
using Assets.TobiiPro.ScreenBased.Scripts;
using Assets.UDP;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using System.Xml;
using Tobii.Research.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExperimentSystem : MonoBehaviour
{
    public static List<Experiment> ExperimentList { get; set; }
    public int CurrentIndex { get; private set; }

    UDPSystem UdpSystem;
    GameObject Wall;
    ScreenBasedSaveData SaveData;
    CenterCircle CenterCircle;
    DataExchangeSystem DataExchangeSystem;
    private ConditionSettings ConditionSettings;
    string _folder = "Data";
    private XmlWriterSettings _fileSettings;
    private System.Timers.Timer Timer;

    public Experiment CurrentExperiment
    {
        get
        {
            if (CurrentIndex >= ExperimentList.Count) return null;
            return ExperimentList[CurrentIndex];
        }
    }
    public Experiment NextExperiment
    {
        get
        {
            if (CurrentIndex + 1 >= ExperimentList.Count) return null;
            return ExperimentList[CurrentIndex + 1];
        }
    }

    public SynchronizationContext MainContext { get; private set; }


    void Start()
    {
        MainContext = SynchronizationContext.Current;
        Wall = GameObject.Find("Wall");
        SaveData = GameObject.Find("[SaveData]").GetComponent<ScreenBasedSaveData>();
        CenterCircle = GameObject.Find("Circle").GetComponent<CenterCircle>();
        DataExchangeSystem = GameObject.Find("DataExchangeSystem").GetComponent<DataExchangeSystem>();
        ConditionSettings = GameObject.Find("ConditionSettings").GetComponent<ConditionSettings>();
        ConditionSettings.FCOnClick();
    }

    public void Connect()
    {
        if (!CenterCircle.GazingCenter) return;
        CenterCircle.gameObject.SetActive(false);

        if (ExperimentSettings.RemoteFlg)
        {
            if (ExperimentSettings.ServerFlg)
            {
                ConnectAsServer();
            }
            else
            {
                ConnectAsClient();
            }
        }
        else
        {
            StartFromFirstExperiment();
        }
    }

    void ConnectAsServer()
    {
        UdpSystem = ExperimentSettings.GetCommandUDP(data =>
        {
            Debug.Log("COMMAND RECEIVED AT " + nameof(ConnectAsServer));
            if (data[0] != (byte)CommandType.Text) return;
            var res = new TextCommand(data);
            if (res.Text == ExperimentSettings.RemoteAdress + "SETTING RECEIVED")
            {
                Timer?.Stop();
                UdpSystem.Finish();
                StartFromFirstExperiment();
            }
        });
        UdpSystem.Receive();

        var setting = new SettingCommand(ExperimentList);
        Timer = new System.Timers.Timer(1000);
        Timer.Elapsed += (sender, e) =>
        {
            UdpSystem.Send_NonAsync2(setting.ToBytes());
        };
        Timer.Start();
    }

    void ConnectAsClient()
    {
        UdpSystem = ExperimentSettings.GetCommandUDP(data =>
        {
            Debug.Log("COMMAND RECEIVED AT " + nameof(ConnectAsClient));
            if (data[0] != (byte)CommandType.Setting) return;
            var setting = new SettingCommand(data);
            ExperimentList = setting.ExperimentList;
            var res = new TextCommand(ExperimentSettings.LocalAdress + "SETTING RECEIVED");
            UdpSystem.Send_NonAsync2(res.ToBytes());
            UdpSystem.Finish();
            StartFromFirstExperiment();
        });
        UdpSystem.Receive();
    }

    public void StartFromFirstExperiment()
    {
        ConditionSettings.NOnClick();
        if (ExperimentSettings.RemoteFlg)
        {
            DataExchangeSystem.RemoteFlg = true;
            UdpSystem = ExperimentSettings.GetCommandUDP(OnReceiveCommand);
            UdpSystem.Receive();
            if (ExperimentSettings.ServerFlg)
            {
                Thread.Sleep(3000);
                var nextStartTime = DateTime.Now + TimeSpan.FromMilliseconds(2000);
                CurrentExperiment.StartTime = nextStartTime;
                var nextCmd = new NextCommand(-1, false, string.Empty, nextStartTime);
                UdpSystem.Send_NonAsync2(nextCmd.ToBytes());
                ScheduleStartExperiment();
            }
        }
        else
        {
            var nextStartTime = DateTime.Now + TimeSpan.FromMilliseconds(2000);
            CurrentExperiment.StartTime = nextStartTime;
            ScheduleStartExperiment();
        }
    }

    void OnReceiveCommand(byte[] data)
    {
        Debug.Log("COMMAND RECEIVED AT " + nameof(Start));
        switch ((CommandType)data[0])
        {
            case CommandType.Next:
                var next = new NextCommand(data);
                if (next.LastExperimentNumber == -1)
                {
                    CurrentExperiment.StartTime = next.NextStartTime;
                    ScheduleStartExperiment();
                }
                else
                {
                    // 相手が先に押した場合
                    var nextExp = ExperimentList[next.LastExperimentNumber + 1];
                    if (nextExp.StartTime == null || nextExp.StartTime > next.NextStartTime)
                    {
                        MainContext.Post(_ =>
                        {
                            DisableInputs();
                        }, null);
                        NextExperiment.StartTime = next.NextStartTime;
                        Next(ExperimentSettings.RemoteAdress, next.Answer, GetGazeDataFile());
                    }
                    // 自分が先に押した場合
                    else
                    {
                        // do nothing.
                    }
                }
                break;
        }
    }

    public void ScheduleStartExperiment()
    {
        Debug.Log(CurrentExperiment.StartTime);
        if (CurrentExperiment.StartTime == null)
        {
            return;
        }
        TimeSpan interval = CurrentExperiment.StartTime.Value - DateTime.Now;
        if (interval > TimeSpan.Zero)
        {
            var timer = new System.Timers.Timer(interval.TotalMilliseconds);
            timer.Elapsed += (s, e) =>
            {
                timer.Stop();
                MainContext.Post(_ =>
                {
                    StartExperiment();
                }, null);
            };
            timer.Start();
        }
    }

    void StartExperiment()
    {
        Debug.Log("EXPEROMENT STARTED");
        var img = GameObject.Find("Canvas/Task").GetComponent<Image>();

        var texture = FileManager.LoadPNG(Application.dataPath + "/StreamingAssets/" + CurrentExperiment.ImageFile);
        img.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        
        switch (CurrentExperiment.ExperimentType)
        {
            case ExperimentType.P:
                ConditionSettings.COnClick();
                break;
            case ExperimentType.A:
                ConditionSettings.AOnClick();
                break;
            case ExperimentType.AC:
                ConditionSettings.ACOnClick();
                break;
            case ExperimentType.F:
                ConditionSettings.FOnClick();
                break;
            case ExperimentType.FC:
                ConditionSettings.FCOnClick();
                break;
            case ExperimentType.N:
                ConditionSettings.NOnClick();
                break;
            case ExperimentType.C:
                ConditionSettings.COnClick();
                break;
        }
        CurrentExperiment.Start();
        EnableInputs();
    }

    public void OnClickT()
    {
        OnClick(true);
    }

    public void OnClickF()
    {
        OnClick(false);
    }


    public void Back()
    {
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        SceneManager.LoadScene("StartScene");
    }

    public void OnClickFinish()
    {
        UdpSystem.Finish();
        DataExchangeSystem.UdpSystem.Finish();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
    UnityEngine.Application.Quit();
#endif
    }

    void OnClick(bool answer)
    {
        DisableInputs();
        var nextStartTime = DateTime.Now + TimeSpan.FromMilliseconds(1000);
        if (NextExperiment != null)
        {
            NextExperiment.StartTime = nextStartTime;
        }
        NoticeRemote(answer, nextStartTime);
        Next(ExperimentSettings.LocalAdress, answer, GetGazeDataFile());
    }

    void NoticeRemote(bool answer, DateTime nextStartTime)
    {
        if (!ExperimentSettings.RemoteFlg) return;
        var nextCmd = new NextCommand(CurrentIndex, answer, ExperimentSettings.LocalAdress, nextStartTime);
        UdpSystem.Send_NonAsync2(nextCmd.ToBytes());
    }

    void Next(string respondent, bool answer, string dazedataFile)
    {
        CurrentExperiment.Finish(respondent, answer, dazedataFile);
        CurrentIndex++;
        if (CurrentIndex >= ExperimentList.Count)
        {
            FinishExperiment();
        }
        else
        {
            ScheduleStartExperiment();
        }
    }

    string GetGazeDataFile()
    {
        return SaveData.LastFileName;
    }

    void DisableInputs()
    {
        Wall.SetActive(true);
        SaveData.SaveData = false;
    }

    void EnableInputs()
    {
        Wall.SetActive(false);
        SaveData.SaveData = true;
    }

    void FinishExperiment()
    {
        UdpSystem?.Finish();
        DataExchangeSystem?.UdpSystem?.Finish();
        EyeTracker.Instance.StopAllCoroutines();

        var fileSettings = new XmlWriterSettings();
        fileSettings.Indent = true;
        var fileName = string.Format("experiment_{0}.xml", System.DateTime.Now.ToString("yyyyMMddTHHmmss"));
        var file = XmlWriter.Create(System.IO.Path.Combine(_folder, fileName), fileSettings);
        file.WriteStartDocument();
        file.WriteStartElement("Experiments");
        foreach (var e in ExperimentList)
        {
            var span = e.ResponseTime - e.StartTime;
            file.WriteStartElement(string.Format("exp{0}", e.Number));
            file.WriteAttributeString("Type", e.ExperimentType.ToString());
            file.WriteAttributeString("ImageFile", e.ImageFile);
            file.WriteAttributeString("CorrectAnswer", e.CorrectAnswer.ToString());
            file.WriteAttributeString("StartTime", e.StartTime.Value.ToString("yyyyMMddTHHmmss"));
            file.WriteAttributeString("ResponseTime", e.ResponseTime.Value.ToString("yyyyMMddTHHmmss"));
            file.WriteAttributeString("TimeSpan", span.Value.TotalMilliseconds.ToString());
            file.WriteAttributeString("Respondent", e.Respondent);
            file.WriteAttributeString("Answer", e.Answer.ToString());
            file.WriteAttributeString("GazeDataFile", e.GazeDataFile);
            file.WriteEndElement();
        }
        file.WriteEndElement();
        file.WriteEndDocument();
        file.Flush();
        file.Close();

        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        SceneManager.LoadScene("EndScene");
    }
}

