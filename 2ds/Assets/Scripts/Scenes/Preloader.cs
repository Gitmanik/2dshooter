using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Preloader : MonoBehaviourPunCallbacks
{
    [SerializeField] private Slider loadingBar;
    [SerializeField] private TMP_Text loadingText;

    private float target;
    private bool v;

    void Start()
    {
        loadingBar.value = 0;
        Progress(0, "Loading");
        DataManager.Load();
        NetworkManager.Instance.Connect();
        Progress(.33f, "Loaded Player data, connecting to Photon");
    }

    private void Progress(float v, string t)
    {
        Debug.Log($"{v * 100f:000}%: {t}");
        target = v;
        loadingText.text = t;
    }

    private void Update()
    {
        loadingBar.value = Mathf.Lerp(loadingBar.value, target, Time.deltaTime * 4);

        if (!v && loadingBar.value >= .99f)
        {
            v = true;
            SceneManager.LoadScene("Menu");
        }
    }

    public override void OnJoinedLobby()
    {
        Progress(1f, "Joined Lobby");
    }

    public override void OnConnectedToMaster()
    {
        Progress(.7f, $"Connected to the {PhotonNetwork.CloudRegion} server");
        PhotonNetwork.AutomaticallySyncScene = true;
    }
}
