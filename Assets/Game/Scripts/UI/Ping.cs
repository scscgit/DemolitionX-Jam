using Game.Scripts.Network;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class Ping : NetworkBehaviour
{
    public Text pingText;

    private void Start()
    {
        transform.SetParent(GameObject.Find("Managers").transform);
    }

    void Update()
    {
        var playerCount = GameNetworkPlayer.PlayerCount - 1;
        pingText.text =
            $"Ping: {(int) (NetworkTime.rtt * 1000 / 2)}ms ({playerCount} player{(playerCount == 1 ? "" : "s")})";
    }
}
