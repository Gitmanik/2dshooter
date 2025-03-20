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
    [SerializeField] private Button exitButton;
    [SerializeField] private TMP_Text compileDate;

    [SerializeField] private Slider VolumeSlider;
    [SerializeField] private Toggle FullscreenToggle;

    [SerializeField] private GameObject RoomEntryPrefab;
    [SerializeField] private Transform RoomEntryTransform;

    [SerializeField] private ResolutionDropdown resdrop;
    [SerializeField] private Tab tab;

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
        resdrop.onChange.AddListener(OnResolutionPicked);
        VolumeSlider.value = AudioListener.volume;
        VolumeSlider.onValueChanged.RemoveAllListeners();
        VolumeSlider.onValueChanged.AddListener(OnChangedVolume);

        FullscreenToggle.isOn = DataManager.Fullscreen;
        FullscreenToggle.onValueChanged.RemoveAllListeners();
        FullscreenToggle.onValueChanged.AddListener(ToggleFullscreen);
        AddRoomTitlebar();
        NetworkManager.OnRoom.AddListener(OnRooms);
        
        OnRooms();
    }

    private void OnDestroy()
    {
        NetworkManager.OnRoom.RemoveListener(OnRooms);
    }

    #region Connect Tab
    public void OnConnectClick()
    {
        if (!InputValid())
            return;

        PhotonNetwork.JoinOrCreateRoom(ipInput.text, new RoomOptions() { IsVisible = true, MaxPlayers = 4 }, TypedLobby.Default);
        joinButton.interactable = false;
    }
    #endregion

    #region Room List Tab
    private void AddRoomTitlebar()
    {
        RoomEntry a = Instantiate(RoomEntryPrefab, RoomEntryTransform).GetComponent<RoomEntry>();
        a.Titlebar();
        entries.Add(a);
    }

    public void OnRooms()
    {
        Debug.Log("OnRooms");

        foreach (RoomInfo r in NetworkManager.Instance.roomInfos)
        {
            if (r.RemovedFromList)
            {
                Destroy(entries.Find(x => x.GetComponent<RoomEntry>().info?.Name == r.Name).gameObject);
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
    #endregion

    #region Options Tab
    private void OnChangedVolume(float newv)
    {
        AudioListener.volume = newv;
        DataManager.MainVolume = newv;
    }

    private void OnResolutionPicked(ResolutionDropdown.Resolution res)
    {
        DataManager.ResolutionWidth = res.Width;
        DataManager.ResolutionHeight = res.Height;
        Screen.SetResolution(res.Width, res.Height, DataManager.Fullscreen, 0);
    }

    public void ToggleFullscreen(bool v)
    {
        DataManager.Fullscreen = v;
        Screen.fullScreen = v;
    }

    #endregion

    #region Exit Tab

    public void ResetTab()
    {
        tab.SelectTab(0);
    }

    public void OnExitClick()
    {
        PlayerPrefs.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
    #endregion

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

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room");
    }
    #endregion
}
