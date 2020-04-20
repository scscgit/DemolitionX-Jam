using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Enviroment : NetworkBehaviour
{
    public GameObject arena;
    // Start is called before the first frame update
    void Start()
    {
        if (isServer)
        {
            var tmp = Instantiate(arena, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f));
            NetworkServer.Spawn(arena);
        }
    }
}
