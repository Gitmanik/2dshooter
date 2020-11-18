using Mirror;
using System.Collections.Generic;
using UnityEngine;

class CustomNetworkManager : NetworkManager
{
    

    public static CustomNetworkManager instance;

    public override void OnStartServer()
    {
        base.OnStartServer();
        instance = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }
}