using System;
using System.Linq;
using Game.Scripts.Network;
using Mirror;
using Mirror.Websocket;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace Game.Scripts.UI
{
    public class MainMenu : MonoBehaviour
    {
        public static string PlayerName;
        public static bool ShouldReconnect;
        public static string RemoteAdminPassword;

        [Header("Settings pages")] public GameObject menuPage;
        public GameObject settingsPage;
        [Header("Text callbacks")] public InputField playerInput;
        private bool _headlessServerStarted;

        private NetworkManager _networkManager;

        private void Awake()
        {
            _networkManager = FindObjectOfType<NetworkManager>();
            // Assign the default until changed, but keep any persisted value between connections
            if (_networkManager.networkAddress.Equals(""))
            {
                _networkManager.networkAddress = NetworkSettingsUi.RealServer;
            }

            // Android: don't sleep
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        private void OnEnable()
        {
            // Initialization & Hot-swap
            InitializePlayerName();
            if (ShouldReconnect)
            {
                ConnectClient();
            }
        }

        private void HeadlessServerStart()
        {
            Debug.Log("Running server in a headless mode. Following command line argument formats are supported:\n* Change the listening port:\n    port 7778\n* Enable remote administration using password:\n    admin MySecretPassword");
            var args = System.Environment.GetCommandLineArgs();
            for (int argsIndex = 2; argsIndex < args.Length - 1; argsIndex += 2)
            {
                switch (args[argsIndex])
                {
                    case "port":
                    case "-port":
                    case "--port":
                        Debug.Log(
                            $"Switching {(int.TryParse(args[argsIndex + 1], out var port) ? $"to port {(_networkManager.GetComponent<WebsocketTransport>().port = port).ToString()}" : $"port failed, cannot recognize {args[argsIndex + 1]}")}");
                        break;
                    case "admin":
                    case "-admin":
                    case "--admin":
                        Debug.Log(
                            $"Setting remote administration password to: ${RemoteAdminPassword = args[argsIndex + 1]}");
                        break;
                }
            }

            _networkManager.StartHost();
        }

        private void InitializePlayerName()
        {
            // Player's name initialization
            if (PlayerName == null)
            {
                var randomPlayerNames = new[]
                {
                    "Amazincar",
                    "Auldkill",
                    "BadSilly",
                    "CarLumbar",
                    "CornyCounty",
                    "DamageDace",
                    "DeathSilly",
                    "DestroyDespot",
                    "Destruction",
                    "DestructionFree",
                    "DestructionKill",
                    "EarSilly",
                    "KillCar",
                    "Killersh",
                    "KillSpice",
                    "ProdigyKill",
                    "BoboFunk",
                    "Cipring",
                    "Cucondov",
                    "Cyteamar",
                    "Deomanwo",
                    "Depiles",
                    "Derscgre",
                    "DiaryMj",
                    "Dragoney",
                    "Ebroniah",
                    "Finalco",
                    "Gossipmere",
                    "Hippogo",
                    "HolyStand",
                    "Interestod",
                    "Iverlion",
                    "JimQuey",
                    "LastingMura",
                    "LuckySaiyan",
                    "LummoGurly",
                    "Pinsten",
                    "Proptexo",
                    "Prosstar",
                    "Reematte",
                    "Riconintr",
                    "Ronectis",
                    "Rozbo",
                    "ShmoeWicked",
                    "Soltran",
                    "Yourchno",
                    "Adygenco",
                    "Appelered",
                    "Beautyra",
                    "BlinkSlim",
                    "Enjoyerca",
                    "Exoticitec",
                    "Facellon",
                    "Flamespier",
                    "Gielineba",
                    "Glimmercoun",
                    "Humandson",
                    "Interela",
                    "KhadZeether",
                    "KingAni",
                    "Malleonia",
                    "Mamade",
                    "Mooneyes",
                    "Nycci",
                    "Pflowapri",
                    "QueyChiquita",
                    "SellProud",
                    "Serentma",
                    "Sliprope",
                    "Stroonsentu",
                    "SumoKnight",
                    "TaryBago",
                    "Tenaxinet",
                    "TheborgStronger",
                    "Thrillwood",
                    "WetPunk",
                };
                if (randomPlayerNames.Any(test => test.Length > playerInput.characterLimit))
                {
                    throw new Exception("playerInput.characterLimit exceeded by one of the default names");
                }

                ChangePlayerName(randomPlayerNames[new Random().Next(randomPlayerNames.Length)]);
            }
            else
            {
                ChangePlayerName(PlayerName);
            }
        }

        private void Update()
        {
            // Called when running using server build, or after invoking command GameNetworkPlayer.CmdRestartServer
            // This has to be called from Update instead of Start or OnEnable in order to properly initialize
            if (NetworkManager.isHeadless && !_headlessServerStarted)
            {
                _headlessServerStarted = true;
                PlayerName = "Server";
                HeadlessServerStart();
                return;
            }

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
            ShouldReconnect = true;
            _networkManager.StartClient();
        }

        /// <summary>
        /// Player name change, compatible with whichever the side it was called from from (text-box or code).
        /// </summary>
        public void ChangePlayerName(string playerName)
        {
            PlayerName = playerName;
            playerInput.text = playerName;
        }

        public void ExitGame()
        {
            Application.Quit();
        }
    }
}
