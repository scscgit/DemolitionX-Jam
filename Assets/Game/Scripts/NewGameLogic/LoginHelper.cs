 using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;

/*
	Authenticators: https://mirror-networking.com/docs/Components/Authenticators/
	Documentation: https://mirror-networking.com/docs/Guides/Authentication.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkAuthenticator.html
*/

public class LoginHelper : NetworkAuthenticator
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

    public void OnAuthRequestMessage(NetworkConnection conn, AuthRequestMessage msg)
    {
        AccountLogin.Login.OnServerReceiveLoginMessage(conn, msg);
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

    public void OnAuthResponseMessage(NetworkConnection conn, AuthResponseMessage msg)
    {
        AccountLogin.Login.OnClientReceiveLoginResponseMessage(conn, msg);
    }

    public override void OnServerAuthenticate(NetworkConnection conn)
    {
    }

    public override void OnClientAuthenticate(NetworkConnection conn)
    {
    }

    #endregion
}

#region Messages

public class AuthRequestMessage : MessageBase
{
    public string email;
    public string username;
    public string password;
    public string comfirmPassword;
    public bool newLogin;
}

public class AuthResponseMessage : MessageBase
{
    public string errors;
    public long PlayerID;
    public bool success;
}

#endregion
