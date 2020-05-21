using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameManager : MonoBehaviour
{
    public bool ShowPing;
    public GameObject PingUI;

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkServer.active)
        {
            NetworkServer.Spawn(Instantiate(PingUI));
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
