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
        IngameHUDManager.Instance.Disable();
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
            CustomNetworkManager.instance.SpawnNotification($"{players[conn].info.Nickname} has connected!", Color.blue - new Color(0, 0, 0, 0.2f), 2.5f);
    }

    internal void OnClientDisconnect(NetworkConnection conn)
    {
        CustomNetworkManager.instance.SpawnNotification($"{players[conn].info.Nickname} has disconnected!", Color.blue - new Color(0, 0, 0, 0.2f), 2.5f);
    }

    private void RespawnPlayer(Player player, Vector3 newPos)
    {
        player.health = 100;
        player.TargetTeleport(newPos);
        player.i.ResetInventory();
        player.Respawn();
        CustomNetworkManager.instance.SpawnNotification($"{player.info.Nickname} respawned!", new Color(105f / 255f, 181f / 255f, 120f / 255f, 0.4f), 1f);
    }

    public void PlayerDied(Player player, GameObject from)
    {
        Player playerKiller = from.GetComponent<Player>();
        string killer = from.name;
        if (playerKiller != null)
            killer = playerKiller.info.Nickname;

        CustomNetworkManager.instance.SpawnNotification($"{killer} > {player.info.Nickname}", new Color(0, 0, 0, 0.8f), 5f);
        LeanTween.delayedCall(2.5f, () => RespawnPlayer(player, NetworkManager.singleton.GetStartPosition().position));
    }
}
