using DlibFaceLandmarkDetectorExample;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    private CommunicationMedia media;

    public void UpdateAgent(Rect rect, List<Vector2> landmarkPoints, Vector2 gazePoint)
    {
        //CmdDisplayAgent(netId, rect, landmarkPoints, gazePoint);
    }

    public override void OnStartLocalPlayer()
    {
        print("OnStartLocalPlayer");
        base.OnStartLocalPlayer();
        media = GameObject.Find("CommunicationMedia").GetComponent<CommunicationMedia>();
        media.OnUpdateAgent += UpdateAgent;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    //[Command]
    //void CmdDisplayAgent(NetworkInstanceId id, Rect rect, List<Vector2> landmarkPoints, Vector2 gazePoint)
    //{
    //    print("cmd" + id.Value);
    //    RpcDisplayAgent(id, rect, landmarkPoints, gazePoint);
    //}

    //[ClientRpc]
    //void RpcDisplayAgent(NetworkInstanceId id, Rect rect, List<Vector2> landmarkPoints, Vector2 gazePoint)
    //{
    //    print("rpc");
    //    print("netid" + netId.Value);
    //    print("id" + id.Value);
    //    if (id.Value == netId.Value)
    //    {
    //        if(media != null)
    //        {
    //            media.SetAgent(rect, landmarkPoints, gazePoint);
    //        }
    //    }
    //}
}
