using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NetworkManager : MonoBehaviour, ILobbyCallbacks
{
    public List<RoomInfo> roomInfos = new List<RoomInfo>();

    public static UnityEvent OnRoom = new UnityEvent();

    public static NetworkManager Instance;

    private void Start()
    {
        Instance = this;
        PhotonNetwork.AddCallbackTarget(this);
    }

    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        roomInfos = roomList;
        OnRoom?.Invoke();
    }

    public void OnJoinedLobby()
    {
    }

    public void OnLeftLobby()
    {
    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
    }
}
