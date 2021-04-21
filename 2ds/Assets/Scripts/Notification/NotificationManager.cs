using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Gitmanik.Notification
{
    public class NotificationManager : MonoBehaviour, IOnEventCallback
    {
        public struct NotificationMessage
        {
            public string text;
            public float aliveTime;
            public Color color;
        }

        public static NotificationManager Instance;
        [SerializeField] private GameObject notificationObject;

        private void Awake()
        {
            Instance = this;
            PhotonNetwork.AddCallbackTarget(this);
        }

        public void RemoteSpawn(string text, Color color, float aliveTime)
        {
            PhotonNetwork.RaiseEvent(1, new object[] { text, color.r, color.g, color.b, color.a, aliveTime }, new RaiseEventOptions() { Receivers = ReceiverGroup.All}, SendOptions.SendReliable);
        }


        public void LocalSpawn(string text, Color color, float aliveTime)
        {
            Instantiate(Instance.notificationObject, Instance.transform).GetComponent<NotificationObject>().Setup(text, color, aliveTime);
        }

        public void OnEvent(EventData photonEvent)
        {
            
            if (photonEvent.Code == 1)
            {
                object[] data = (object[])photonEvent.CustomData;
                Instance.LocalSpawn((string) data[0], new Color((float)data[1], (float)data[2], (float)data[3], (float)data[4]), (float)data[5]);
            }
        }
    }
}