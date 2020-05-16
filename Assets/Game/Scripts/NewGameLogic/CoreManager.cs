using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CoreManager : NetworkBehaviour
{
    public static CoreManager Core;
    public GameObject CurrentCar;
    public long PlayerID;
    public List<long> FreeIDList = new List<long>();

    // Start is called before the first frame update
    void Awake()
    {
        if (Core)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            DontDestroyOnLoad(this);
            Core = this;
        }
    }

    public override void OnStartServer()
    {
        PlayerDatabase.StartDataBase();
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
            if (FreeIDList.Count < 100)
            {
                var tmpid = long.MaxValue - Random.Range(int.MinValue, int.MaxValue);
                if (!PlayerDatabase.PlayerIDExist(tmpid))
                {
                    FreeIDList.Add(tmpid);
                }
            }
    }

    public override void OnStartClient()
    {
        if (PlayerPrefs.HasKey("UPlayerID"))
        {
            this.PlayerID = long.Parse(PlayerPrefs.GetString("UPlayerID"));
            CmdLoadPlayer(PlayerID);
            LoadScene("Lobby");
        }
    }

    [Command]
    public void CmdLoadPlayer(long PlayerID)
    {
        PlayerDatabase.LoadPlayerData(PlayerID);
    }


    public void Logout()
    {
        NetworkClient.Send(new LogoutRequestMessage() { playerID = PlayerID });
    }

    public AsyncOperation LoadScene(string name)
    {
        return SceneManager.LoadSceneAsync(name, LoadSceneMode.Single);
    }

    private void OnDisable()
    {
        PlayerDatabase.Dispose();
    }
}
