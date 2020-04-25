using Game.Scripts.Network;
using Game.Scripts.Util;
using Mirror;
using UnityEngine;

public class HealthAndScores : NetworkBehaviour
{
    public const float EnvironmentDamageModifier = 1 / 2f;
    public const float ImpactDamageModifier = 1 / 1000f;

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
        var hp = velocity * EnvironmentDamageModifier;
        if (hp < 1)
        {
            // Do at least 1 damage
            hp = 1;
        }

        var otherRigidbody = other.gameObject.GetComponent<Rigidbody>();
        if (!otherRigidbody)
        {
            Player.SetHealth(Player.health - hp);
            Player.RpcDisplayObjectHitEvent("environment", hp);
            return;
        }

        var otherVelocity = otherRigidbody.velocity.magnitude;
        var otherHealthAndScores = other.transform.FindComponentIncludingParents<HealthAndScores>();
        if (!otherHealthAndScores)
        {
            Player.SetHealth(Player.health - hp);
            Player.RpcDisplayObjectHitEvent("object", hp);
            return;
        }

        var otherPlayer = otherHealthAndScores.Player;
        var impactMagnitude = Mathf.Abs(otherVelocity * otherRigidbody.mass - velocity * _rigidbody.mass);
        hp = impactMagnitude * ImpactDamageModifier;
        if (hp < 1)
        {
            // Do at least 1 damage
            hp = 1;
        }

        // Collision with {otherPlayer.playerName}'s {other.gameObject.name}
        // The one who was slower gets damaged
        if (velocity > otherVelocity)
        {
            Player.RpcDisplayPlayerHitEvent(otherPlayer.playerName, hp);
            otherPlayer.SetHealth(otherPlayer.health - hp);
            Player.SetScore(Player.score + Mathf.RoundToInt(impactMagnitude));
        }
        else
        {
            Debug.Log(
                $"Reverse collision by {Player.playerName} (@{velocity} vs. {otherVelocity}) with {otherPlayer.playerName}'s {other.gameObject.name}");
            //otherPlayer.RpcDisplayPlayerHitEvent("(only damage) " + Player.playerName, hp);
            //Player.SetHealth(Player.health - hp);
            // TODO: should we give score to the other one?
        }
    }
}
