using Gitmanik.BaseCode;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviourPunCallbacks
{
    public static Menu Instance;
    public List<RoomEntry> entries = new List<RoomEntry>();

    [SerializeField] private TMP_InputField nickInput;
    [SerializeField] private TMP_InputField ipInput;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private TMP_Text compileDate;

    [SerializeField] private GameObject RoomEntryPrefab;
    [SerializeField] private Transform RoomEntryTransform;

    void OnValueChanged(string _)
    {
        DataManager.Name = nickInput.text;
        DataManager.RecentIP = ipInput.text;
        PhotonNetwork.NickName = nickInput.text;

        if (InputValid())
        {
            joinButton.interactable = true;
        }
        else
        {
            joinButton.interactable = false;
        }
    }

    private bool InputValid() => !string.IsNullOrWhiteSpace(DataManager.Name) && !string.IsNullOrWhiteSpace(DataManager.RecentIP);

    private void Awake()
    {
        if (!GameManager.CheckScene())
            DestroyImmediate(this);
    }

    void Start()
    {
        Instance = this;
        nickInput.text = DataManager.Name;
        ipInput.text = DataManager.RecentIP;
        joinButton.interactable = InputValid();
        nickInput.onValueChanged.AddListener(OnValueChanged);
        ipInput.onValueChanged.AddListener(OnValueChanged);
        compileDate.text = $"{PhotonNetwork.CloudRegion} [{BuildInfo.Instance.BuildDate} {GameManager.Instance.GameVersion}]";
        NetworkManager.OnRoom.AddListener(ReloadRooms);
        ReloadRooms();
    }

    private void OnDestroy()
    {
        NetworkManager.OnRoom.RemoveListener(ReloadRooms);
    }

    public void OnConnectClick()
    {
        if (!InputValid())
            return;

        PhotonNetwork.NickName = nickInput.text;
        PhotonNetwork.JoinOrCreateRoom(ipInput.text, new RoomOptions() { IsVisible = true, MaxPlayers = 4 }, TypedLobby.Default);
        joinButton.interactable = false;
    }

    public void ReloadRooms()
    {
        foreach (RoomEntry e in entries)
        {
            print(e.info.Name);
        }
        foreach (RoomInfo r in NetworkManager.Instance.roomInfos)
        {
            RoomEntry e = entries.Find(x => x.info.Name == r.Name);
            if (e != null)
            {
                if (r.RemovedFromList)
                {
                    if (e != null)
                    {
                        entries.Remove(e);
                        Destroy(e.gameObject);
                    }
                }
                else
                {
                    e.UpdateInfo(r);
                }
            }
            else
            {
                RoomEntry ee = Instantiate(RoomEntryPrefab, RoomEntryTransform).GetComponent<RoomEntry>();
                ee.Setup(r);
                entries.Add(ee);
            }
        }
    }

    public void RoomSelected(RoomInfo i)
    {
        PhotonNetwork.NickName = nickInput.text;
        PhotonNetwork.JoinRoom(i.Name);
        joinButton.interactable = false;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log(message);
        joinButton.interactable = true;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log(message);
        joinButton.interactable = true;
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Created room");
        PhotonNetwork.LoadLevel("Game");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room");
    }

    public void OnExitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
