using System.Collections.Generic;
using Game.Scripts.Network;
using UnityEngine;
using Mirror;
using Game.Scripts.Util;

public class Explosivebarrel : NetworkBehaviour
{
    public const string HitName = "barrel";
    public const float Damage = 20;

    public float radius = 5;
    public float force = 2000;

    private bool _exploded;

    [ServerCallback]
    private void OnCollisionEnter(Collision collision)
    {
        if (!_exploded && !collision.gameObject.CompareTag(gameObject.tag) && collision.rigidbody)
        {
            var hits = Physics.SphereCastAll(transform.position, radius, transform.forward, radius);
            if (hits.Length > 0)
            {
                //var hitPlayers = new HashSet<uint>();
                foreach (var hit in hits)
                {
                    var hitPlayer = hit.collider.transform.FindComponentIncludingParents<GameNetworkPlayer>();
                    if (hitPlayer)
                    {
                        //if (!hitPlayers.Contains(hitPlayer.netId))
                        //{
                            //hitPlayers.Add(hitPlayer.netId);
                            //continue;
                        //}
                       // else
                       // {
                            hitPlayer.RpcDisplayHealth(hitPlayer.health - Damage);
                            hitPlayer.RpcDisplayObjectHitEvent(HitName, Damage);
                            // Hit the vehicle parent rigidbody, which has an identity
                            if(hitPlayer.Car)
                            {
                                RpcAddForce(hitPlayer.Car);
                            }
                            else
                            {
                                Debug.LogError("Where the fuck is car?");
                            }
                       // }
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
        if(!go)
        {
            Debug.Log("Where's the argument?");
        }
        
        var rigidbody = go.GetComponent<Rigidbody>();

        if(rigidbody)
        {
            rigidbody.AddExplosionForce(rigidbody.mass * force, transform.position, radius);
        }
    }
}
