using System;
using Game.Scripts.UI;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts.Network
{
    public class GameNetworkPlayer : NetworkBehaviour
    {
        public const float StartHealth = 100;

        [Tooltip("All car prefabs available for Player's spawn; ordered as CarSelection")]
        public GameObject[] cars;

        [Tooltip("Prefab for the HoveringDetails")]
        public GameObject hoveringPrefab;

        [Tooltip("Reference to the child object of a vehicle camera, which is automatically enabled when playing")]
        public VehicleCamera vehicleCamera;

        [Header("Synced vars")] [SyncVar] public string playerName;
        [SyncVar] public int score;
        [SyncVar] public float health;

        private ArenaUi _arenaUi;
        private Camera _spectatorCamera;
        private HoveringDetails _hoveringDetails;

        [SyncVar] private GameObject _car;

        public void Start()
        {
            _arenaUi = GameObject.Find("ArenaUI").GetComponent<ArenaUi>();
            if (isLocalPlayer)
            {
                _spectatorCamera = GameObject.Find("SpectatorCamera").GetComponent<Camera>()
                                   ?? throw new NullReferenceException($"Missing SpectatorCamera");
                ChangeCar();
            }
            else if (!ReferenceEquals(_car, null))
            {
                // If the car was already spawned, add the relevant additions
                // If it wasn't, then wait for the RPC callback
                ConfigureHoveringDetails(_car);
            }
        }

        private void Update()
        {
            if (_car)
            {
                if (!_car.transform.parent || _car.transform.parent != transform)
                    _car.transform.parent = transform;
            }
        }

        public void AddPlayer()
        {
            if (NetworkClient.isConnected && !ClientScene.ready)
            {
                ClientScene.Ready(NetworkClient.connection);

                if (ClientScene.localPlayer == null)
                {
                    ClientScene.AddPlayer();
                }
            }
            else
            {
                Debug.LogError("Probably cannot add a network player client - not connected yet");
            }
        }

        public void ChangeCar()
        {
            if (!ReferenceEquals(_car, null))
            {
                Destroy(_car);
            }

            _spectatorCamera.gameObject.SetActive(false);
            vehicleCamera.gameObject.SetActive(false);
            _arenaUi.DisableUi();
            SceneManager.LoadScene("CarSelection", LoadSceneMode.Additive);
        }

        public void SelectedCar(int carIndex)
        {
            if (isServer)
            {
                var car = Instantiate(cars[carIndex], transform);
                NetworkServer.Spawn(car);
                _car = car;
                playerName = MainMenu.PlayerName;
                RpcSetup(car);
            }
            else
                CmdSelectedCar(carIndex, gameObject, MainMenu.PlayerName);
        }

        [Command]
        public void CmdSelectedCar(int carIndex, GameObject player, string setPlayerName)
        {
            var car = Instantiate(cars[carIndex], transform);
            NetworkServer.Spawn(car, player);
            _car = car;
            // Also set the player name before creating the HoveringDetails
            playerName = setPlayerName;
            health = StartHealth;
            RpcSetup(car);
        }

        [ClientRpc]
        public void RpcSetup(GameObject car)
        {
            health = StartHealth;

            // The _car will be set in the following cycle, so we have to use param car instead
            if (isLocalPlayer)
            {
                _arenaUi.EnableUi(this);
                vehicleCamera.gameObject.SetActive(true);
                vehicleCamera.playerCar = car.transform;
                car.transform.parent = transform;
                car.GetComponent<VehiclePhysics>().canControl = true;
                car.GetComponent<VehiclePhysics>().StartEngine();
                // playerName SyncVar isn't set until the next frame, but if this is a callback, we know the name
                playerName = MainMenu.PlayerName;
                // Always rotate HoveringDetails towards the current player
                HoveringDetails.VehicleCamera = vehicleCamera.transform;
            }

            ConfigureHoveringDetails(car);
        }

        private void ConfigureHoveringDetails(GameObject carObject)
        {
            _hoveringDetails = Instantiate(hoveringPrefab, carObject.transform).GetComponent<HoveringDetails>();
            _hoveringDetails.Player = this;
            _hoveringDetails.DisplayScore(score);
            _hoveringDetails.DisplayHealth(health);
            var healthAndScores = carObject.GetComponent<HealthAndScores>();
            healthAndScores.Player = this;
        }

        [Command]
        public void CmdSetScore(int setScore)
        {
            score = setScore;
            RpcDisplayScore(setScore);
            if (!ReferenceEquals(_hoveringDetails, null))
            {
                _hoveringDetails.DisplayScore(setScore);
            }
        }

        [Command]
        public void CmdSetHealth(float setHealth)
        {
            health = setHealth;
            RpcDisplayHealth(setHealth);
            if (!ReferenceEquals(_hoveringDetails, null))
            {
                _hoveringDetails.DisplayHealth(setHealth);
            }
        }

        [ClientRpc]
        public void RpcDisplayScore(int displayScore)
        {
            if (!ReferenceEquals(_hoveringDetails, null))
            {
                _hoveringDetails.DisplayScore(displayScore);
            }
        }

        [ClientRpc]
        public void RpcDisplayHealth(float displayHealth)
        {
            if (!ReferenceEquals(_hoveringDetails, null))
            {
                _hoveringDetails.DisplayHealth(displayHealth);
            }
        }
    }
}
