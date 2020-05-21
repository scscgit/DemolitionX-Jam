using Mirror;
using Mirror.Websocket;
using UnityEngine;

public class CoreNetwork : MonoBehaviour
{
    public static CoreNetwork Core;
    public NetworkManager NetManager;
    public WebsocketTransport telepathyTransport;

    void Awake()
    {
        if (Core)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            DontDestroyOnLoad(this);
            Core = this;
        }
    }

    // Use this for initialization
    void Start()
    {
        NetManager.StartClient();
    }

    // Update is called once per frame
    void Update()
    {
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
