using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RetriveID : MonoBehaviour
{
    public static Dictionary<uint, GameObject> Netids;
    public uint id;

    public void Update()
    {
        if (Netids == null)
            Netids = new Dictionary<uint, GameObject>();
        id = GetComponent<NetworkIdentity>().netId;
        if (!Netids.ContainsKey(id))
            Netids[id] = gameObject;
    }

    private void OnDisable()
    {
        Netids.Clear();
    }
}
