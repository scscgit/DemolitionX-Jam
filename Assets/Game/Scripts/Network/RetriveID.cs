using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RetriveID : MonoBehaviour
{
    public static Dictionary<uint, GameObject> Netids;

    public void OnEnable()
    {
        if (Netids == null)
            Netids = new Dictionary<uint, GameObject>();
        var id = GetComponent<NetworkIdentity>().netId;
        if (!Netids.ContainsKey(id))
            Netids[id] = gameObject;
    }

    private void OnDisable()
    {
        Netids.Clear();
    }
}
