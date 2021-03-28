using Gitmanik.Notification;
using Mirror;
using UnityEngine;

namespace Gitmanik.Networked
{
    public class NetworkedNotification
    {
        public static void Setup()
        {
            NetworkManager.singleton.authenticator.OnClientAuthenticated.AddListener((_) => NetworkClient.RegisterHandler<NotificationMessage>(OnClientReceiveNotification));
        }

        public static void Spawn(string text, Color color, float aliveTime)
        {
            NotificationMessage m = new NotificationMessage() { text = text, color = color, aliveTime = aliveTime };
            NetworkServer.SendToAll(m);
        }

        private static void OnClientReceiveNotification(NetworkConnection conn, NotificationMessage msg)
        {
            NotificationManager.Instance.Spawn(msg.text, msg.color, msg.aliveTime);
        }
    }

}