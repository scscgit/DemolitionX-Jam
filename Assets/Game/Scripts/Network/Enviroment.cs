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
            var tmp = Instantiate(arena, new Vector3(-27.1f,-4.8f,-27.4f), new Quaternion(0f, 0f, 0f, 0f));
            NetworkServer.Spawn(arena);
        }
    }
}
