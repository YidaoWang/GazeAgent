using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{

    public override void OnStartLocalPlayer()
    {

        base.OnStartLocalPlayer();
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
    void CmdDisplayAgent(NetworkInstanceId id)
    {
        print("CmdDsp");
        print("NetId:" + id.Value);
        //switch (id.Value)
        //{
        //    case 1:
        //        RpcDisplayAgent(new NetworkInstanceId(2), agent);
        //        break;
        //    case 2:
        //        RpcDisplayAgent(new NetworkInstanceId(1), agent);
        //        break;
        //}
    }

    [ClientRpc]
    void RpcDisplayAgent(NetworkInstanceId id)
    {
    }
}
