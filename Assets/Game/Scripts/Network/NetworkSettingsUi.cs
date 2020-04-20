using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.Scripts.Network
{
    /// <summary>
    /// see Mirror.NetworkManagerHUD
    /// </summary>
    public class NetworkSettingsUi : MonoBehaviour
    {
        public const string RealServer = "demolitionx.scscdev.eu";

        public Text disconnectButtonText;

        public UnityEvent onConnected;
        public UnityEvent onDisconnected;

        private NetworkManager _manager;

        private void Awake()
        {
            _manager = FindObjectOfType<NetworkManager>();
        }

        public void OnChangeAddress(string serverAddress)
        {
            if ("".Equals(serverAddress))
            {
                _manager.networkAddress = RealServer;
            }
            else
            {
                _manager.networkAddress = serverAddress;
            }
        }

        public void StartServer()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                Debug.LogError("WebGL cannot be server");
            }

            _manager.StartServer();
            WhenConnected();
        }

        public void StartServerAndClient()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                Debug.LogError("WebGL cannot be server");
            }

            _manager.StartHost();
            WhenConnected();
        }

        public void Disconnect()
        {
            DisconnectStatic(_manager);
            WhenDisconnected();
        }

        public static void DisconnectStatic(NetworkManager manager)
        {
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                manager.StopHost();
            }
            else if (NetworkClient.isConnected)
            {
                // stop client if client-only
                manager.StopClient();
            }
            else if (NetworkServer.active)
            {
                // stop server if server-only
                manager.StopServer();
            }
            else if (NetworkClient.active)
            {
                // TODO: verify - client connecting?
                manager.StopClient();
            }
            else
            {
                Debug.LogError("Disconnect didn't match any conditions");
            }
        }

        public bool SetDisconnectButtonName()
        {
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                disconnectButtonText.text = "Stop Host";
                return true;
            }

            if (NetworkClient.isConnected)
            {
                disconnectButtonText.text = "Stop Client";
                return true;
            }

            if (NetworkServer.active)
            {
                disconnectButtonText.text = "Stop Server";
                return true;
            }

            // We get no callback on timeout, so this would be displayed permanently; also it's too long to display IP
            // if (NetworkClient.active)
            // {
            //     $"Cancel client attempts {_manager.networkAddress}";
            // }

            return false;
        }

        public void WhenConnected()
        {
            if (NetworkClient.isConnected && !NetworkServer.active)
            {
                Debug.LogWarning("Assertion failed: connected but not connected?");
            }

            if (SetDisconnectButtonName())
            {
                onConnected.Invoke();
            }
            else
            {
                // Client attempts won't cause connecting state
                WhenDisconnected();
            }
        }

        public void WhenDisconnected()
        {
            onDisconnected.Invoke();
        }
    }
}
