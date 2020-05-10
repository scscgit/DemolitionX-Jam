using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class CoreManager : NetworkBehaviour
{
    public static CoreManager Core;
    public GameObject CurrentCar;
    public long PlayerID;
    public List<long> FreeIDList = new List<long>();

    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(this);
        Core = this;
        if (PlayerPrefs.HasKey("UPlayerID"))
        {
            this.PlayerID = long.Parse(PlayerPrefs.GetString("UPlayerID"));
            CmdLoadPlayer(PlayerID);
            LoadScene("Lobby");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
            if (FreeIDList.Count < 100)
            {
                var tmpid = long.MaxValue - Random.Range(int.MinValue, int.MinValue);
                if (!PlayerDatabase.PlayerExist(tmpid))
                {
                    FreeIDList.Add(tmpid);
                }
            }

    }

    public void InitLogin()
    {

    }

    [Command]
    public void CmdLoadPlayer(long PlayerID)
    {
        PlayerDatabase.LoadPlayerData(PlayerID);
    }

    [Command]
    public void CmdInitNewAccoutLogin(string email, string username, string Password)
    {
        int ErrorCount = 0;
        if (PlayerDatabase.PlayerExist(username))
        {
            RpcUsernameError();
            ErrorCount++;
        }
        if (PlayerDatabase.PlayerEmailExist(email))
        {
            RpcEmailError();
            ErrorCount++;
        }
        if (ErrorCount > 0)
            return;
        var id = FreeIDList[0];
        PlayerDatabase.AddPlayer(id, username, email, Password);
        FreeIDList.Remove(id);
        RpcEnterLobby(netId,id);
    }

    public void Logout()
    {
        CmdLogout(PlayerID);
    }

    [Command]
    public void CmdLogout(long PlayerID)
    {
        PlayerDatabase.SignPlayerOut(PlayerID);
    }

    [ClientRpc]
    public void RpcEmailError()
    {
        AccountLogin.Login.ErrorEmailExist();
    }

    [ClientRpc]
    public void RpcUsernameError()
    {
        AccountLogin.Login.ErrorUsernameExist();
    }
    [ClientRpc]
    public void RpcEnterLobby(uint id, long PlayerID)
    {
        if (netId == id)
        {
            this.PlayerID = PlayerID;
            PlayerPrefs.SetString("UPlayerID",PlayerID.ToString());
            LoadScene("Lobby");
        }
    }

    public AsyncOperation LoadScene(string name)
    {
        return SceneManager.LoadSceneAsync(name, LoadSceneMode.Single);
    }
}
