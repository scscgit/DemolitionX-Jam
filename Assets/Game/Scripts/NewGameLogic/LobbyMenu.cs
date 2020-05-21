using UnityEngine;
using UnityEngine.SceneManagement;

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
        CoreManager.Core.Logout();
        CoreManager.Core.LoadScene(name).completed += StartDataBase;
    }

    private void StartDataBase(AsyncOperation obj)
    {
        //if(NetworkServer.active)
        //PlayerDatabase.StartDataBase();
    }
}
