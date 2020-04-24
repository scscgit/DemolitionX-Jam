using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Explosivebarrel : NetworkBehaviour
{
    public bool exploaded = false;
    public float radius = 5;
    public float force = 250;
    public float damage = 200;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ServerCallback]
    private void OnCollisionEnter(Collision collision)
    {
        if (!exploaded && collision.gameObject.tag != gameObject.tag && collision.rigidbody)
        {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, radius, transform.forward, radius);
            if (hits != null && hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    RpcAddForce(hit.transform.gameObject);
                    if (hit.rigidbody)
                        hit.rigidbody.AddExplosionForce(hit.rigidbody.mass * force, transform.position, radius);
                }
            }
            exploaded = true;
            Destroy(gameObject, 2f);
        }
    }

    [ClientRpc]
    public void RpcAddForce(GameObject go)
    {
        Rigidbody rigidbody = go.GetComponent<Rigidbody>();
        if (!rigidbody)
            rigidbody = go.GetComponentInParent<Rigidbody>();
        if (!rigidbody)
            rigidbody = go.GetComponentInChildren<Rigidbody>();
        if (rigidbody)
            rigidbody.AddExplosionForce(rigidbody.mass * force, transform.position, radius);
        Debug.Log(rigidbody.name);
    }
}
