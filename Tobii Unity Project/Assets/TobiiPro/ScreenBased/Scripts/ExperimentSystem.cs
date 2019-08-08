
using Assets.IO;
using Assets.UDP;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using System.Xml;
using Tobii.Research.Unity;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentSystem : MonoBehaviour
{
    public static List<Experiment> ExperimentList { get; set; }
    public int CurrentIndex { get; private set; }

    UDPSystem UdpSystem;
    GameObject Wall;
    ScreenBasedSaveData SaveData;
    string _folder = "Data";
    private XmlWriterSettings _fileSettings;

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

        var cs = GameObject.Find("ConditionSettings").GetComponent<ConditionSettings>();
        cs.COnClick();

        if (ExperimentSettings.RemoteFlg)
        {
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

        var texture = FileManager.LoadPNG(Application.dataPath + CurrentExperiment.ImageFile);
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
    }
}

