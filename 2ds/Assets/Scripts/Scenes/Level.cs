using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public Dictionary<NetworkConnection, Player> players = new Dictionary<NetworkConnection, Player>();

    public Transform preGameMask;

    public static Level Instance;

    void Start()
    {
        Instance = this;
        preGameMask.gameObject.SetActive(false);
        GameManager.Instance.SetBlackMask(true);

        NetworkManager.singleton.authenticator.OnServerAuthenticated.AddListener(SpawnPlayer);
    }

    private GameObject InternalSpawnPlayer(NetworkConnection conn)
    {
        Transform startPos = NetworkManager.singleton.GetStartPosition();

        GameObject player = Instantiate(NetworkManager.singleton.playerPrefab, startPos.position, startPos.rotation);
        players[conn] = player.GetComponent<Player>();
        players[conn].Setup((AuthRequestMessage)conn.authenticationData);
        return player;
    }

    public void SpawnPlayer(NetworkConnection conn)
    {
        GameObject player = InternalSpawnPlayer(conn);
        NetworkServer.AddPlayerForConnection(conn, player);
    }

    private void OnDestroy()
    {
        NetworkManager.singleton.authenticator.OnServerAuthenticated.RemoveListener(SpawnPlayer);
    }
}
