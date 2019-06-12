using DlibFaceLandmarkDetectorExample;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    private Agent agent;

    public void UpdateAgent(List<Vector2> landmarkPoints, Vector2 gazePoint)
    {
        CmdDisplayAgent(netId, landmarkPoints, gazePoint);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        agent = GameObject.Find("Agent").GetComponent<Agent>();
        agent.OnUpdateAgent += UpdateAgent;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    [Command]
    void CmdDisplayAgent(NetworkInstanceId id, List<Vector2> landmarkPoints, Vector2 gazePoint)
    {
        RpcDisplayAgent(id, landmarkPoints, gazePoint);
    }

    [ClientRpc]
    void RpcDisplayAgent(NetworkInstanceId id, List<Vector2> landmarkPoints, Vector2 gazePoint)
    {
        if(id.Value != netId.Value)
        {
            if(agent != null)
            {
                agent.SetAgent(landmarkPoints, gazePoint);
            }
        }
    }
}
