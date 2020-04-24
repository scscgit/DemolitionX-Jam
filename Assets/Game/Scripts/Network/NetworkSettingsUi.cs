using System.IO;
using Mirror;
using Mirror.Websocket;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Scripts.Network
{
    /// <summary>
    /// see Mirror.NetworkManagerHUD
    /// </summary>
    public class NetworkSettingsUi : MonoBehaviour
    {
        public const string RealServer = "demolitionx.scscdev.eu";

        public InputField addressInput;
        public Toggle securedToggle;
        public Text disconnectButtonText;

        public UnityEvent onConnected;
        public UnityEvent onDisconnected;

        private NetworkManager _manager;
        private WebsocketTransport _transport;

        private void Awake()
        {
            _manager = FindObjectOfType<NetworkManager>();
            _transport = _manager.GetComponent<WebsocketTransport>();

            // Load the previously set values
            securedToggle.isOn = _transport.Secure;
            addressInput.text = RealServer.Equals(_manager.networkAddress) ? "" : _manager.networkAddress;
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

            try
            {
                _manager.StartServer();
            }
            catch (FileNotFoundException)
            {
                securedToggle.isOn = false;
            }

            WhenConnected();
        }

        public void StartServerAndClient()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                Debug.LogError("WebGL cannot be server");
            }

            try
            {
                _manager.StartHost();
            }
            catch (FileNotFoundException)
            {
                securedToggle.isOn = false;
            }

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
                // Possibly after a hot-swap, which pretty much "breaks" the NetworkManager instance
                Destroy(manager.gameObject);
                SceneManager.LoadScene("Game/Scenes/MainMenu");
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

        public void SetSecured(bool secured)
        {
            _manager.GetComponent<WebsocketTransport>().Secure = secured;
        }
    }
}
