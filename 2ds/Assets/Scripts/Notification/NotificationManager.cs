﻿using UnityEngine;

namespace Gitmanik.Notification
{
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager Instance;
        [SerializeField] private GameObject notificationObject;

        private void Awake()
        {
            Instance = this;
        }

        public static void Spawn(string text, Color color, float aliveTime)
        {
            Instantiate(Instance.notificationObject, Instance.transform).GetComponent<NotificationObject>().Setup(text, color, aliveTime);
        }
    }
}