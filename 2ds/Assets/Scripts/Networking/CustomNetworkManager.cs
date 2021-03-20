using Mirror;
using UnityEngine.Events;

class CustomNetworkManager : NetworkManager
{
    public static CustomNetworkManager Instance;
    public UnityEvent<NetworkConnection> onClientDisconnected;

    public override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        onClientDisconnected?.Invoke(conn);
        base.OnServerDisconnect(conn);
    }

}