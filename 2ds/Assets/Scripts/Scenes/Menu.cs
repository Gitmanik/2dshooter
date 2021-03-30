using Gitmanik.BaseCode;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField] private TMP_InputField nickInput;
    [SerializeField] private TMP_InputField ipInput;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private TMP_Text compileDate;

    void OnValueChanged(string _)
    {
        DataManager.Name = nickInput.text;
        DataManager.RecentIP = ipInput.text;

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
        compileDate.text = $"{BuildInfo.Instance.BuildDate} {GameManager.Instance.GameVersion}";
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

    public void OnExitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
