using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField] private TMP_InputField nickInput;
    [SerializeField] private TMP_InputField ipInput;
    [SerializeField] private Button joinButton;
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

    private bool InputValid() => !string.IsNullOrWhiteSpace(nickInput.text) && !string.IsNullOrWhiteSpace(ipInput.text);

    void Start()
    {
        nickInput.text = DataManager.Name;
        ipInput.text = DataManager.RecentIP;
        joinButton.interactable = InputValid();
        nickInput.onValueChanged.AddListener(OnValueChanged);
        ipInput.onValueChanged.AddListener(OnValueChanged);
        compileDate.text = GameManager.Instance.CompileText;
    }

    public void OnConnectClick()
    {
        if (string.IsNullOrWhiteSpace(DataManager.Name))
        {
            Debug.LogError("Wrong Nickname");
            return;
        }

        if (string.IsNullOrWhiteSpace(DataManager.RecentIP))
        {
            Debug.LogError("Wrong IP");
            return;
        }

        NetworkManager.singleton.networkAddress = DataManager.RecentIP;
        NetworkManager.singleton.StartClient();
    }

    public void OnHostClick()
    {
        NetworkManager.singleton.StartHost();
    }
}
