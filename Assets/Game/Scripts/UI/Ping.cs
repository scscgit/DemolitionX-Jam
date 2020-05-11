using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class Ping : NetworkBehaviour
{
    public Text pingText;

    private void Start()
    {
        transform.parent = GameObject.Find("Managers").transform;
    }

    void Update()
    {
        pingText.text = $"Ping : {(int) (NetworkTime.rtt * 1000 / 2)}ms";
    }
}
