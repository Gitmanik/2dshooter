﻿using Mirror;
using UnityEngine;

public struct AuthRequestMessage : NetworkMessage
{
    public string nick;
    public int version;
}

public struct AuthResponseMessage : NetworkMessage
{
    public bool success;
    public string message;
}

public struct NotificationMessage : NetworkMessage
{
    public string text;
    public float aliveTime;
    public Color color;
}

public struct PlayersPingMessage : NetworkMessage
{
    public int[] ids;
    public int[] pings;
}

public struct PlayerPingMessage : NetworkMessage
{
    public int ping;
}