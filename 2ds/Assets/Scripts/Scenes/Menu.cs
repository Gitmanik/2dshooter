using Gitmanik.BaseCode;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField] private TMP_InputField nickInput;
    [SerializeField] private TMP_InputField ipInput;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private TMP_Text compileDate;

    void OnValueChanged(string _)
    {
        if (InputValid())
        {
            joinButton.interactable = true;
            DataManager.Name = nickInput.text;
            DataManager.RecentIP = ipInput.text;
        }
        else
        {
            joinButton.interactable = false;
        }
    }

    private bool InputValid() => !string.IsNullOrWhiteSpace(DataManager.Name) && !string.IsNullOrWhiteSpace(DataManager.RecentIP);

    private void Awake()
    {
        if (GameManager.Instance == null)
        {
            SceneManager.LoadScene("Preloader");
            DestroyImmediate(this);
        }

    }

    void Start()
    {
        nickInput.text = DataManager.Name;
        ipInput.text = DataManager.RecentIP;
        joinButton.interactable = InputValid();
        nickInput.onValueChanged.AddListener(OnValueChanged);
        ipInput.onValueChanged.AddListener(OnValueChanged);
        compileDate.text = $"{BuildInfo.Instance.BuildDate} {GameManager.Instance.GameVersion}";

        if (ParrelSync.ClonesManager.IsClone())
        {
            NetworkManager.singleton.StartServer();
        }
    }

    public void OnConnectClick()
    {
        if (!InputValid())
            return;

        NetworkManager.singleton.networkAddress = DataManager.RecentIP;
        NetworkManager.singleton.StartClient();
        hostButton.interactable = false;
    }

    public void OnHostClick()
    {
        NetworkManager.singleton.StartHost();
    }
}
