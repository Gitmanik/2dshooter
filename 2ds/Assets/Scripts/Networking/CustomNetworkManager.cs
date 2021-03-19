using Gitmanik.Notification;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

class CustomNetworkManager : NetworkManager
{
    public static CustomNetworkManager instance;
    public UnityEvent<NetworkConnection> onClientDisconnected;

    private void Update()
    {
        if (!NetworkServer.active)
            return;

        if (Level.Instance == null)
            return;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<PlayerPingMessage>(OnServerReceivePing);
        instance = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        NetworkClient.RegisterHandler<NotificationMessage>(OnClientReceiveNotification);
        instance = this;
    }

    private void OnServerReceivePing(NetworkConnection arg1, PlayerPingMessage arg2)
    {
        Player.allPlayers.Find(x => x.connectionToClient == arg1).ping = arg2.ping;
    }

    public void SpawnNotification(string text, Color color, float aliveTime)
    {
        NotificationMessage m = new NotificationMessage() { text = text, color = color, aliveTime = aliveTime };
        NetworkServer.SendToAll(m);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        onClientDisconnected?.Invoke(conn);
        base.OnServerDisconnect(conn);
    }

    private void OnClientReceiveNotification(NetworkConnection conn, NotificationMessage msg)
    {
        NotificationManager.Spawn(msg.text, msg.color, msg.aliveTime);
    }
}