using Game.Scripts.Network;
using UnityEngine;

public class HealthAndScores : MonoBehaviour
{
    public GameNetworkPlayer Player { get; set; }

    private Rigidbody _rigidbody;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision other)
    {
        float v = _rigidbody.velocity.magnitude;
        if (!other.gameObject.GetComponent<Rigidbody>())
        {
            Player.CmdSetHealth(Player.health - v / 20f);
        }


        if (other.gameObject.GetComponent<Rigidbody>())
        {
            float impactMag = Mathf.Abs(other.gameObject.GetComponent<Rigidbody>().velocity.magnitude *
                other.gameObject.GetComponent<Rigidbody>().mass - v * _rigidbody.mass);
            if (v > other.gameObject.GetComponent<Rigidbody>().velocity.magnitude)
            {
                var otherHealthAndScores = other.gameObject.GetComponent<HealthAndScores>();
                if (otherHealthAndScores)
                {
                    otherHealthAndScores.Player.CmdSetHealth(otherHealthAndScores.Player.health - impactMag / 10000f);
                    Player.CmdSetScore(Player.score + Mathf.RoundToInt(impactMag));
                }
                else
                {
                    Player.CmdSetHealth(Player.health - 1);
                }
            }
            else
            {
                Player.CmdSetHealth(Player.health - impactMag / 10000f);
            }
        }
    }
}
