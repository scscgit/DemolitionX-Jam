using Game.Scripts.Network;
using Game.Scripts.Util;
using Mirror;
using UnityEngine;

public class HealthAndScores : NetworkBehaviour
{
    public const float EnvironmentDamageModifier = 1 / 20f;
    public const float ImpactDamageModifier = 1 / 10000f;

    public GameNetworkPlayer Player { get; set; }

    private Rigidbody _rigidbody;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    [ServerCallback]
    private void OnCollisionEnter(Collision other)
    {
        var velocity = _rigidbody.velocity.magnitude;
        var otherRigidbody = other.gameObject.GetComponent<Rigidbody>();
        if (!otherRigidbody)
        {
            Player.SetHealth(Player.health - velocity * EnvironmentDamageModifier);
            Debug.Log($"{Player.playerName} randomly collided for damage of {velocity * EnvironmentDamageModifier}");
        }
        else
        {
            var otherVelocity = otherRigidbody.velocity.magnitude;
            var otherPlayer = other.transform.FindComponentIncludingParents<HealthAndScores>().Player;
            Debug.Log(
                $"Collision by {Player.playerName} @ {velocity} with {otherPlayer.playerName}'s {other.gameObject.name}");
            var impactMagnitude = Mathf.Abs(otherVelocity * otherRigidbody.mass - velocity * _rigidbody.mass);
            // The one who was slower gets damaged
            if (velocity > otherVelocity)
            {
                otherPlayer.SetHealth(otherPlayer.health - impactMagnitude * ImpactDamageModifier);
                Player.SetScore(Player.score + Mathf.RoundToInt(impactMagnitude));
            }
            else
            {
                Player.SetHealth(Player.health - impactMagnitude * ImpactDamageModifier);
                // TODO: should we give score to the other one?
            }
        }
    }
}
