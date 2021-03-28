using Gitmanik.Networked;
using Mirror;
using UnityEngine;

public class Level : MonoBehaviour
{
    public static Level Instance;
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

        NetworkManager.singleton.authenticator.OnServerAuthenticated.AddListener(OnPlayerConnected);
        CustomNetworkManager.Instance.onClientDisconnected.AddListener(OnClientDisconnect);
    }

    private void OnDestroy()
    {
        IngameHUDManager.Instance?.DisableAll();
        NetworkManager.singleton?.authenticator.OnServerAuthenticated.RemoveListener(OnPlayerConnected);
        CustomNetworkManager.Instance?.onClientDisconnected.RemoveListener(OnClientDisconnect);
    }
    #endregion

    public void OnPlayerConnected(NetworkConnection conn)
    {
        Transform startPos = NetworkManager.singleton.GetStartPosition();

        Player player = Instantiate(NetworkManager.singleton.playerPrefab, startPos.position, startPos.rotation).GetComponent<Player>();
        player.Setup((AuthRequestMessage)conn.authenticationData);

        NetworkServer.AddPlayerForConnection(conn, player.gameObject);

        if (!conn.identity.isLocalPlayer)
            NetworkedNotification.Spawn($"{player.Nickname} has connected!", Color.blue - new Color(0, 0, 0, 0.2f), 2.5f);
    }

    internal void OnClientDisconnect(NetworkConnection conn)
    {
        NetworkedNotification.Spawn($"{Player.allPlayers.Find(x => x.connectionToClient == conn).Nickname} has disconnected!", Color.blue - new Color(0, 0, 0, 0.2f), 2.5f);
    }

    private void RespawnPlayer(Player player)
    {
        player.TargetTeleport(NetworkManager.singleton.GetStartPosition().position);
        player.health = 100;
        player.inventory.ResetInventory();
        player.Respawn();
        NetworkedNotification.Spawn($"{player.Nickname} respawned!", new Color(105f / 255f, 181f / 255f, 120f / 255f, 0.4f), 1f);
    }

    public void PlayerDied(Player player, GameObject from)
    {
        Player playerKiller = from.GetComponent<Player>();
        string killer = from.name;
        if (playerKiller != null)
            killer = playerKiller.Nickname;

        NetworkedNotification.Spawn($"{killer} > {player.Nickname}", new Color(0, 0, 0, 0.8f), 5f);
        LeanTween.delayedCall(2.5f, () => RespawnPlayer(player));
    }
}
