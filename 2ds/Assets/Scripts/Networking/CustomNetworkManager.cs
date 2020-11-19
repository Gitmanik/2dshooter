using Mirror;
using UnityEngine.Events;

class CustomNetworkManager : NetworkManager
{
    public static CustomNetworkManager instance;
    public UnityEvent<NetworkConnection> onClientDisconnected;


    public override void OnStartServer()
    {
        base.OnStartServer();
        instance = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        instance = this;
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
        onClientDisconnected?.Invoke(conn);
    }
}