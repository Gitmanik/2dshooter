using Gitmanik.Notification;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public static Level Instance;

    public Dictionary<NetworkConnection, Player> players = new Dictionary<NetworkConnection, Player>();

    public Transform preGameMask;

    #region MonoBehaviour
    void Start()
    {
        Instance = this;
        preGameMask.gameObject.SetActive(false);
        GameManager.Instance.SetBlackMask(true);

        NetworkManager.singleton.authenticator.OnServerAuthenticated.AddListener(OnPlayerConnected);
        CustomNetworkManager.instance.onClientDisconnected.AddListener(OnClientDisconnect);
    }
    private void OnDestroy()
    {
        NetworkManager.singleton.authenticator.OnServerAuthenticated.RemoveListener(OnPlayerConnected);
    }
    #endregion

    public void OnPlayerConnected(NetworkConnection conn)
    {
        Transform startPos = NetworkManager.singleton.GetStartPosition();

        GameObject player = Instantiate(NetworkManager.singleton.playerPrefab, startPos.position, startPos.rotation);
        players[conn] = player.GetComponent<Player>();
        players[conn].Setup((AuthRequestMessage)conn.authenticationData);

        NetworkServer.AddPlayerForConnection(conn, player);

        if (!conn.identity.isLocalPlayer)
            NotificationManager.Spawn($"{players[conn].info.Nickname} has connected!", Color.blue - new Color(0, 0, 0, 0.2f), 5f);
    }
    internal void OnClientDisconnect(NetworkConnection conn)
    {
        NotificationManager.Spawn($"{players[conn].info.Nickname} has disconnected!", Color.magenta - new Color(0, 0, 0, 0.2f), 5f);
    }

    private void RespawnPlayer(Player player)
    {
        player.health = 100;
        player.TargetTeleport(NetworkManager.singleton.GetStartPosition().position);
        player.i.ResetInventory();
        player.Respawn();
    }

    public void PlayerDied(Player player, GameObject from)
    {
        LeanTween.delayedCall(2.5f, () => RespawnPlayer(player));
    }
}
