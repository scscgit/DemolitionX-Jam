using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class LoginMenu : MonoBehaviour
{
    public GameObject info;
    public NetworkManager NetManager => CoreNetwork.Core.NetManager;

    public InputField Address;

    private AsyncOperation operation;

    // Use this for initialization
    void Start()
    {
        NetManager.networkAddress = "demolitionx.scscdev.eu";
    }

    // Update is called once per frame
    void Update()
    {
        if (!string.IsNullOrEmpty(Address.text))
        {
            NetManager.networkAddress = Address.text;
        }
        else
            NetManager.networkAddress = "demolitionx.scscdev.eu";

        //if (NetworkClient.isConnected && operation == null)
        //    operation = CoreManager.Core.LoadScene("lobby");
    }

    public void Reset()
    {
        Address.text = string.Empty;
        NetManager.networkAddress = "demolitionx.scscdev.eu";
    }

    public void StartHost()
    {
        NetManager.StartHost();
    }

    public void StartServer()
    {
        NetManager.StartServer();
    }

    public void Secured()
    {
        CoreNetwork.Core.telepathyTransport.Secure = !CoreNetwork.Core.telepathyTransport.Secure;
    }
}
