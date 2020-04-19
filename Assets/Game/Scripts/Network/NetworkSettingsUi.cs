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
        public const string RealServer = "localhost";

        public Text serverAddress;
        public Text disconnectButtonText;

        public UnityEvent onConnected;
        public UnityEvent onDisconnected;

        private NetworkManager _manager;

        private void Awake()
        {
            _manager = FindObjectOfType<NetworkManager>();
        }

        public void StartClient()
        {
            if (!serverAddress.isActiveAndEnabled || "".Equals(serverAddress.text))
            {
                _manager.networkAddress = RealServer;
            }
            else
            {
                _manager.networkAddress = serverAddress.text;
            }

            Debug.Log("Connecting client to " + _manager.networkAddress);
            _manager.StartClient();
            WhenConnected();
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
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                _manager.StopHost();
            }
            else if (NetworkClient.isConnected)
            {
                // stop client if client-only
                _manager.StopClient();
            }
            else if (NetworkServer.active)
            {
                // stop server if server-only
                _manager.StopServer();
            }
            else if (NetworkClient.active)
            {
                // TODO: verify - client connecting?
                _manager.StopClient();
            }
            else
            {
                Debug.LogError("Disconnect didn't match any conditions");
            }

            WhenDisconnected();
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
