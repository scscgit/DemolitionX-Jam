using System;
using Game.Scripts.Network;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts.UI
{
    public class MainMenu : MonoBehaviour
    {
        public GameObject menuPage;
        public GameObject settingsPage;

        private NetworkManager _networkManager;

        private void Awake()
        {
            _networkManager = FindObjectOfType<NetworkManager>();
            // Assign the default until changed
            _networkManager.networkAddress = NetworkSettingsUi.RealServer;

            // Android: don't sleep
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (menuPage.activeInHierarchy)
                {
                    ConnectClient();
                }
                else
                {
                    settingsPage.SetActive(false);
                    menuPage.SetActive(true);
                }
            }
        }

        public void ConnectClient()
        {
            _networkManager.StartClient();
        }

        [Obsolete]
        public void CarSelectionOffline()
        {
            try
            {
                SceneManager.LoadScene("Game/Scenes/CarSelection");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                //SceneManager.LoadScene(1);
            }
        }

        public void ExitGame()
        {
            Application.Quit();
        }
    }
}
