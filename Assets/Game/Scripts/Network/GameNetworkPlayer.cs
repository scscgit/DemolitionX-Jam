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
            // If this is the local player, then the Start means he has just has just connected, so he will pick a car
            // If the car was already spawned by other player before connecting, add the relevant additions
            // If the car wasn't spawned yet, then wait for the RPC callback
            if (isLocalPlayer)
            {
                _spectatorCamera = GameObject.Find("SpectatorCamera").GetComponent<Camera>()
                                   ?? throw new NullReferenceException($"Missing SpectatorCamera");
                ChangeCar();
            }
            else if (!ReferenceEquals(_car, null))
            {
                ConfigureSpawnedCar(_car);
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
                RpcOnSpawnedCar(car);
            }
            else
            {
                CmdSelectedCar(carIndex, gameObject, MainMenu.PlayerName);
            }
        }

        [Command]
        public void CmdSelectedCar(int carIndex, GameObject player, string setPlayerName)
        {
            var car = Instantiate(cars[carIndex], transform);
            NetworkServer.Spawn(car, player);
            _car = car;
            // Also sync the player name before creating the HoveringDetails
            playerName = setPlayerName;
            health = StartHealth;
            RpcOnSpawnedCar(car);
        }

        [ClientRpc]
        public void RpcOnSpawnedCar(GameObject car)
        {
            // The _car will be synced in the following cycle, so we have to use param car instead
            health = StartHealth;
            if (isLocalPlayer)
            {
                _arenaUi.EnableUi(this);
                vehicleCamera.gameObject.SetActive(true);
                vehicleCamera.playerCar = car.transform;
                car.transform.parent = transform;
                car.GetComponent<VehiclePhysics>().canControl = true;
                car.GetComponent<VehiclePhysics>().StartEngine();
                // playerName SyncVar isn't set until the next frame, but if this is a local callback, we know the name
                playerName = MainMenu.PlayerName;
                // Always rotate HoveringDetails towards the current player
                HoveringDetails.VehicleCamera = vehicleCamera.transform;
            }

            ConfigureSpawnedCar(car);
        }

        private void ConfigureSpawnedCar(GameObject car)
        {
            car.transform.parent = transform;
            _hoveringDetails = Instantiate(hoveringPrefab, car.transform).GetComponent<HoveringDetails>();
            _hoveringDetails.Player = this;
            _hoveringDetails.DisplayScore(score);
            _hoveringDetails.DisplayHealth(health);
            var healthAndScores = car.GetComponent<HealthAndScores>();
            healthAndScores.Player = this;
        }

        public void SetScore(int setScore)
        {
            score = setScore;
            RpcDisplayScore(setScore);
        }

        public void SetHealth(float setHealth)
        {
            health = setHealth;
            RpcDisplayHealth(setHealth);
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
