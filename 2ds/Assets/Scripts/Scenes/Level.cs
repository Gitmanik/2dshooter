using Gitmanik.Notification;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level : MonoBehaviourPunCallbacks
{
    public static Level Instance;
    [SerializeField] private Transform[] startPositions;
    public Transform preGameMask;

    #region MonoBehaviour
    private void Awake()
    {
        if (!GameManager.CheckScene())
            DestroyImmediate(this);
    }

    void Start()
    {
        Instance = this;
        Destroy(preGameMask.gameObject);
        GameManager.Instance.SetBlackMask(true);

        Transform startPos = GetStartPosition();
        PhotonNetwork.Instantiate("PlayerPrefab", startPos.position, startPos.rotation, 0, new object[] { Random.Range(0, SkinManager.Instance.AllSkins.Length) });
    }

    private void OnDestroy()
    {
        IngameHUDManager.Instance?.DisableAll();
    }
    #endregion

    public Transform GetStartPosition() => startPositions[Random.Range(0, startPositions.Length)];

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        NotificationManager.Instance.LocalSpawn($"{newPlayer.NickName} has connected!", Color.blue - new Color(0, 0, 0, 0.2f), 2.5f);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player newPlayer)
    {
        NotificationManager.Instance.LocalSpawn($"{newPlayer.NickName} has disconnected!", Color.blue - new Color(0, 0, 0, 0.2f), 2.5f);
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Menu");
    }
}
