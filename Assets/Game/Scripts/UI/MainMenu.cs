using System;
using System.Linq;
using Game.Scripts.Network;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = System.Random;

namespace Game.Scripts.UI
{
    public class MainMenu : MonoBehaviour
    {
        public static string PlayerName;

        [Header("Settings pages")] public GameObject menuPage;
        public GameObject settingsPage;
        [Header("Text callbacks")] public InputField playerInput;

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
