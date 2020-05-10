using UnityEngine;
using System.Collections;

public class GameMenu : MonoBehaviour
{
    
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ReturnToLobby(string name)
    {
        CoreNetwork.Core.DespawnPlayer();
        CoreManager.Core.LoadScene(name);
    }
}
