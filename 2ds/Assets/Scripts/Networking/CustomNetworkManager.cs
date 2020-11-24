using Gitmanik.Notification;
using Mirror;
using UnityEngine;
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
        NetworkClient.RegisterHandler<NotificationMessage>(OnNotification);
        instance = this;
    }

    public void SpawnNotification(string text, Color color, float aliveTime)
    {
        NotificationMessage m = new NotificationMessage() { text = text, color = color, aliveTime = aliveTime };
        NetworkServer.SendToAll(m);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
        onClientDisconnected?.Invoke(conn);
    }

    private void OnNotification(NetworkConnection conn, NotificationMessage msg)
    {
        NotificationManager.Spawn(msg.text, msg.color, msg.aliveTime);
    }
}