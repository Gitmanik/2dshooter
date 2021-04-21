using Gitmanik.BaseCode;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_InputField nickInput;
    [SerializeField] private TMP_InputField ipInput;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private TMP_Text compileDate;

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
        nickInput.text = DataManager.Name;
        ipInput.text = DataManager.RecentIP;
        joinButton.interactable = InputValid();
        nickInput.onValueChanged.AddListener(OnValueChanged);
        ipInput.onValueChanged.AddListener(OnValueChanged);
        compileDate.text = $"{PhotonNetwork.CloudRegion}\n{BuildInfo.Instance.BuildDate} {GameManager.Instance.GameVersion}";
    }

    public void OnConnectClick()
    {
        if (!InputValid())
            return;

        PhotonNetwork.NickName = nickInput.text;
        PhotonNetwork.JoinOrCreateRoom(ipInput.text, new Photon.Realtime.RoomOptions() { IsVisible = true, MaxPlayers = 4 }, Photon.Realtime.TypedLobby.Default);
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
