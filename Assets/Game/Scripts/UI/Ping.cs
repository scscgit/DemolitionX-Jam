using System;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class Ping : NetworkBehaviour
{
    public Text pingText;
    [Range(0, 10)] public float intervalByServer = 0.5f;
    public float ping;
    private float _lastSync;

    private void Start()
    {
        transform.parent = GameObject.Find("Managers").transform;
    }

    void Update()
    {
        if (isServer && Time.time - _lastSync > intervalByServer)
        {
            _lastSync = Time.time;
            RpcReceivePacket(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }
    }

    [ClientRpc]
    public void RpcReceivePacket(long unixTimeMillis)
    {
        ping = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - unixTimeMillis;
        pingText.text = $"Ping : {ping}ms";
    }
}
