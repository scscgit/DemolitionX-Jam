using UnityEngine;
using System.Collections;
using Mirror;
using Mirror.Websocket;
public class CoreNetwork : MonoBehaviour
{
    public static CoreNetwork Core;
    public NetworkManager NetManager;
    public WebsocketTransport telepathyTransport;
    public GameObject ManagerCore;
    void Awake()
    {
        DontDestroyOnLoad(this);
        Core = this;
    }

    // Use this for initialization
    void Start()
    {
        NetManager.StartClient();
    }

    // Update is called once per frame
    void Update()
    {
        if (!CoreManager.Core && NetworkServer.active)
        {
            NetworkServer.Spawn(Instantiate(ManagerCore));
        }
    }
    public void AddPlayer(AsyncOperation obj)
    {
        SpawnPlayer(CoreManager.Core.CurrentCar);
    }

    public void SpawnPlayer(GameObject player)
    {
        var car = Instantiate(player);
        NetworkServer.AddPlayerForConnection(NetworkServer.localConnection, car);
        var vp = car.GetComponent<VehiclePhysics>();
        vp.canControl = true;
        vp.StartEngine();
        Camera.main.GetComponent<VehicleCamera>().playerCar = car.transform;
    }

    public void DespawnPlayer()
    {
        NetworkServer.DestroyPlayerForConnection(NetworkServer.localConnection);
    }
}
