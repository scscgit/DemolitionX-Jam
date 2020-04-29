using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System.Diagnostics;

public class Ping : NetworkBehaviour
{
    public Text pingText;
    public static float ping;
    [SyncVar]
    public float lastms;
    public Stopwatch stopwatch = new Stopwatch();
    // Update is called once per frame
    public void Start()
    {
        stopwatch.Start();
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (isServer)
        {
            lastms = stopwatch.ElapsedMilliseconds;
            RpcRecivePacket();
        }
        pingText.text = "Ping : " + ping + "ms";
    }

    [ClientRpc]
    public void RpcRecivePacket()
    {
        ping = stopwatch.ElapsedMilliseconds - lastms;
    }
}
