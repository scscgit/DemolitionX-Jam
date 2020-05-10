using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Mirror;

public class LobbyMenu : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Play(string name)
    {
        SceneManager.LoadSceneAsync(name, LoadSceneMode.Single).completed += CoreNetwork.Core.AddPlayer;
    }

    public void Disconnect(string name)
    {
        if (NetworkServer.active)
            CoreNetwork.Core.NetManager.StopHost();
        else
            CoreNetwork.Core.NetManager.StopClient();
        DestroyImmediate(CoreNetwork.Core.gameObject);
        CoreManager.Core.Logout();
        CoreManager.Core.LoadScene(name);
        DestroyImmediate(CoreManager.Core.gameObject);
    }
}
