using Mirror;
using UnityEngine;

public class CustomAuthenticator : NetworkAuthenticator
{
    #region Server

    /// <summary>
    /// Called on server from StartServer to initialize the Authenticator
    /// <para>Server message handlers should be registered in this method.</para>
    /// </summary>
    public override void OnStartServer()
    {
        // register a handler for the authentication request we expect from client
        NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
    }

    /// <summary>
    /// Called on server from OnServerAuthenticateInternal when a client needs to authenticate
    /// </summary>
    /// <param name="conn">Connection to client.</param>
    public override void OnServerAuthenticate(NetworkConnection conn) { }

    public void OnAuthRequestMessage(NetworkConnection conn, AuthRequestMessage authData)
    {
        if (authData.version == GameManager.Instance.GameVersion)
        {
            conn.Send(new AuthResponseMessage() { message = "Welcome!", success = true });
            conn.authenticationData = authData;
            OnServerAuthenticated.Invoke(conn);
        }
        else
        {
            conn.Send(new AuthResponseMessage() { message = "Game Version does not match!", success = false });
            conn.Disconnect();
        }
    }

    #endregion

    #region Client

    /// <summary>
    /// Called on client from StartClient to initialize the Authenticator
    /// <para>Client message handlers should be registered in this method.</para>
    /// </summary>
    public override void OnStartClient()
    {
        // register a handler for the authentication response we expect from server
        NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
    }

    /// <summary>
    /// Called on client from OnClientAuthenticateInternal when a client needs to authenticate
    /// </summary>
    /// <param name="conn">Connection of the client.</param>
    public override void OnClientAuthenticate(NetworkConnection conn)
    {
        print("Authenticating..");
        NetworkClient.Send(new AuthRequestMessage { nick = DataManager.Name, version = GameManager.Instance.GameVersion });
    }

    public void OnAuthResponseMessage(NetworkConnection conn, AuthResponseMessage arg2)
    {

        if (arg2.success)
        {
            Debug.Log($"Authentication succesful! {arg2.message}");
            base.OnClientAuthenticated.Invoke(conn);
        }
        else
        {
            //TODO: wysraj sie 
            Debug.LogError($"Authentication failed! {arg2.message}");
            NetworkClient.Disconnect();
        }
    }

    #endregion
}
