using Game.Scripts.Util;
using UnityEngine;
using Game.Scripts.Network;

public class HealthAndScores : MonoBehaviour
{
    public GameNetworkPlayer gameNetworkPlayer;

    public float health;
    public int score;

    private void Start() {
        health = GetComponent<VehiclePhysics>().health;
        score = GetComponent<VehiclePhysics>().scoreGained;

        gameNetworkPlayer = transform.root.GetComponent<GameNetworkPlayer>();
    }

    public void SetHealth(float h)
    {
        health = h;
        gameNetworkPlayer.SetHealth(health);
    }

    public void SetScores(int scoreGained)
    {
        score += scoreGained;
        gameNetworkPlayer.SetScore(score);
    }
}
