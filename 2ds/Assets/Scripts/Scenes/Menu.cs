using Gitmanik.BaseCode;
using Gitmanik.BaseCode.Tab;
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
    [SerializeField] private TMP_Text compileDate;

    [SerializeField] private Slider VolumeSlider;

    [SerializeField] private GameObject RoomEntryPrefab;
    [SerializeField] private Transform RoomEntryTransform;

    void OnValueChanged(string _)
    {
        DataManager.Name = nickInput.text;
        DataManager.RecentRoomName = ipInput.text;
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

    private bool InputValid() => !string.IsNullOrWhiteSpace(DataManager.Name) && !string.IsNullOrWhiteSpace(DataManager.RecentRoomName);

    private void Awake()
    {
        if (!GameManager.CheckScene())
            DestroyImmediate(this);
    }

    void Start()
    {
        Instance = this;
        nickInput.text = DataManager.Name;
        ipInput.text = DataManager.RecentRoomName;
        joinButton.interactable = InputValid();
        nickInput.onValueChanged.AddListener(OnValueChanged);
        ipInput.onValueChanged.AddListener(OnValueChanged);
        compileDate.text = $"{PhotonNetwork.CloudRegion} [{BuildInfo.Instance.BuildDate} {GameManager.Instance.GameVersion}]";
        VolumeSlider.value = AudioListener.volume;
        VolumeSlider.onValueChanged.RemoveAllListeners();
        VolumeSlider.onValueChanged.AddListener(OnChangedVolume);

        AddRoomTitlebar();
        NetworkManager.OnRoom.AddListener(OnRooms);
        
        OnRooms();
    }

    private void OnDestroy()
    {
        NetworkManager.OnRoom.RemoveListener(OnRooms);
    }

    public void OnConnectClick()
    {
        if (!InputValid())
            return;

        PhotonNetwork.JoinOrCreateRoom(ipInput.text, new RoomOptions() { IsVisible = true, MaxPlayers = 4 }, TypedLobby.Default);
        joinButton.interactable = false;
    }

    private void AddRoomTitlebar()
    {
        RoomEntry a = Instantiate(RoomEntryPrefab, RoomEntryTransform).GetComponent<RoomEntry>();
        a.Titlebar();
        entries.Add(a);
    }

    public void OnRooms()
    {
        foreach (RoomInfo r in NetworkManager.Instance.roomInfos)
        {
            var existing = entries.Find(x => x.GetComponent<RoomEntry>().info?.Name == r.Name);
            if (existing != null)
                Destroy(existing.gameObject);
            if (!r.RemovedFromList)
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
    
    private void OnChangedVolume(float newv)
    {
        AudioListener.volume = newv;
        DataManager.MainVolume = newv;
    }
    
    #region Photon
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
    #endregion
}
