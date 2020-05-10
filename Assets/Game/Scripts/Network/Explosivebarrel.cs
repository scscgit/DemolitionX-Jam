using System.Collections.Generic;
using Game.Scripts.Network;
using UnityEngine;
using Mirror;
using Game.Scripts.Util;

public class Explosivebarrel : NetworkBehaviour
{
    public const string HitName = "a barrel explosion";
    public const float Damage = 20;

    public float radius = 5;
    public float force = 800;

    private bool _exploded;
    private readonly HashSet<uint> _alreadyHitPlayers = new HashSet<uint>();

    [ServerCallback]
    private void OnCollisionEnter(Collision collision)
    {
        if (!_exploded && !collision.gameObject.CompareTag(gameObject.tag) && collision.rigidbody)
        {
            var hits = Physics.SphereCastAll(transform.position, radius, transform.forward, radius);
            if (hits.Length > 0)
            {
                _alreadyHitPlayers.Clear();
                foreach (var hit in hits)
                {
                    if (!hit.collider.transform.FindComponentIncludingParents<Rigidbody>())
                    {
                        // Only the static environment was hit, ignore it
                        continue;
                    }

                    var hitPlayer = hit.collider.transform.FindComponentIncludingParents<GameNetworkPlayer>();
                    if (hitPlayer)
                    {
                        if (_alreadyHitPlayers.Contains(hitPlayer.netId))
                        {
                            continue;
                        }

                        _alreadyHitPlayers.Add(hitPlayer.netId);
                        hitPlayer.SetHealth(hitPlayer.health - Damage);
                        hitPlayer.RpcDisplayObjectHitEvent(HitName, Damage);
                        // Hit the vehicle parent rigidbody, which has an identity
                        RpcAddForce(hitPlayer.Car);
                    }
                    else
                    {
                        RpcAddForce(hit.collider.gameObject);
                    }
                }
            }

            _exploded = true;
            Destroy(gameObject, 2f);
        }
    }

    [ClientRpc]
    public void RpcAddForce(GameObject go)
    {
        if (!go)
        {
            return;
        }

        var rigidbody = go.GetComponent<Rigidbody>();
        rigidbody.AddExplosionForce(rigidbody.mass * force, transform.position, radius);
    }
}
