using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.Events;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public List<RoomInfo> roomInfos = new List<RoomInfo>();
    public static UnityEvent OnRoom = new UnityEvent();
    public static NetworkManager Instance;

    private void Awake()
    {
        Instance = this;
    }
    
    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = GameManager.Instance.GameVersion.ToString();
        PhotonPeer.RegisterType(typeof(GunHolder), 0, GunHolder.Serialize, GunHolder.Deserialize);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        roomInfos = roomList;
        OnRoom?.Invoke();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
}