using System;
using Game.Scripts.UI;
using Game.Scripts.Util;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts.Network
{
    public class GameNetworkPlayer : NetworkBehaviour
    {
        public static GameNetworkPlayer LocalPlayer;
        public static int PlayerCount;

        public const float StartHealth = 100;

        [Tooltip("All car prefabs available for Player's spawn; ordered as CarSelection")]
        public GameObject[] cars;

        [Tooltip("Prefab for the HoveringDetails")]
        public GameObject hoveringPrefab;

        [Tooltip("Reference to the child object of a vehicle camera, which is automatically enabled when playing")]
        public VehicleCamera vehicleCamera;

        [Header("Synced vars")] [SyncVar] public string playerName;
        [SyncVar] public int score;
        [SyncVar(hook = nameof(OnSyncHealth))] public float health;

        private NetworkIdentity _identity;
        private Camera _spectatorCamera;
        private HoveringDetails _hoveringDetails;
        private bool _inCarSelection;

        [SyncVar] private GameObject _car;
        private int _carIndex;
        private VehiclePhysics _vehiclePhysics;
        private bool _quitting;

        public GameObject Car => _car;

        public void Start()
        {
            PlayerCount++;
            _identity = GetComponent<NetworkIdentity>();
            // If this is the local player, then the Start means he has just has just connected, so he will pick a car
            if (isLocalPlayer)
            {
                CmdInitialize(MainMenu.PlayerName);
                LocalPlayer = this;
                _spectatorCamera = GameObject.Find("SpectatorCamera").GetComponent<Camera>()
                                   ?? throw new NullReferenceException($"Missing SpectatorCamera");
                ChangeCarByLocalPlayer();
            }
            // If the car was already spawned by other player before connecting, add the relevant additions
            else if (!ReferenceEquals(_car, null))
            {
                ConfigureSpawnedCar(_car);
            }

            // If the car wasn't spawned yet, then wait for the RPC callback
        }

        private void OnDestroy()
        {
            if (!_quitting && !isLocalPlayer)
            {
                DisplayPositiveEvent($"{playerName}: Disconnected", true);
            }

            PlayerCount--;
        }

        private void OnApplicationQuit()
        {
            _quitting = true;
        }

        [Command]
        public void CmdInitialize(string playerName)
        {
            this.playerName = playerName;
            // RPC doesn't have access to the playerName yet
            RpcDisplayPositiveEvent($"{playerName}: Connected", true);
        }

        /// <summary>
        /// Assumes isLocalPlayer == true
        /// </summary>
        public void ChangeCarByLocalPlayer()
        {
            // Inform the server of the car change, so that re-spawning on new round start stops
            if (!ReferenceEquals(_car, null))
            {
                CmdDestroyCar();
            }

            _spectatorCamera.gameObject.SetActive(false);
            vehicleCamera.gameObject.SetActive(false);
            ArenaUi.Instance.DisableUi();
            // Ensure that the car selection scene is never loaded twice
            if (!_inCarSelection)
            {
                SceneManager.LoadScene("CarSelection", LoadSceneMode.Additive);
            }

            _inCarSelection = true;
        }

        public void RespawnCarByNewRoundByServer()
        {
            if (ReferenceEquals(_car, null))
            {
                // Still under the CarSelection screen
                return;
            }

            DestroyCarByServer();
            SelectedCar(_carIndex, true);
        }

        public void SelectedCar(int carIndex, bool byNewRound = false)
        {
            if (!byNewRound)
            {
                _inCarSelection = false;
            }

            // Display the spectator camera during loading
            if (isLocalPlayer)
            {
                _spectatorCamera.gameObject.SetActive(true);
            }

            if (isServer)
            {
                _carIndex = carIndex;
                var car = Instantiate(cars[carIndex], transform);
                // Spawning without parent is necessary, otherwise stacked cars collide on client at the re-spawn moment
                car.ExecuteWithoutParent(o => NetworkServer.Spawn(o, _identity.connectionToClient));
                _car = car;
                RpcOnSpawnedCar(car, byNewRound);
            }
            else
            {
                CmdSelectedCar(carIndex, byNewRound);
            }
        }

        [Command]
        public void CmdSelectedCar(int carIndex, bool byNewRound)
        {
            // Server-sided protection against spawning two cars
            if (_car)
            {
                TargetForceChangeCar();
                Debug.LogWarning($"Player {playerName} attempted to spawn two cars");
                return;
            }

            _carIndex = carIndex;
            var car = Instantiate(cars[carIndex], transform);
            // Spawning without parent is necessary, otherwise the parent position is ignored
            car.ExecuteWithoutParent(o => NetworkServer.Spawn(o, _identity.connectionToClient));
            _car = car;
            // Also sync the player name before creating the HoveringDetails
            health = StartHealth;
            RpcOnSpawnedCar(car, byNewRound);
        }

        [TargetRpc]
        public void TargetForceChangeCar()
        {
            Debug.Log("Server forced a car change");
            ChangeCarByLocalPlayer();
        }

        [ClientRpc]
        public void RpcOnSpawnedCar(GameObject car, bool byNewRound)
        {
            // The _car will be synced in the following cycle, so we have to use param car instead
            health = StartHealth;
            if (isLocalPlayer)
            {
                ArenaUi.Instance.EnableUi(this);
                _spectatorCamera.gameObject.SetActive(false);
                vehicleCamera.gameObject.SetActive(true);
                vehicleCamera.playerCar = car.transform;
                var vehiclePhysics = car.GetComponent<VehiclePhysics>();
                //var vehicleSync = car.GetComponent<VehicleSync>();
                //vehicleSync.l = true;
                vehiclePhysics.canControl = true;
                vehiclePhysics.StartEngine();
                // Always rotate HoveringDetails towards the current player
                HoveringDetails.VehicleCamera = vehicleCamera.transform;
            }

            ConfigureSpawnedCar(car);

            if (!byNewRound)
            {
                DisplayPositiveEvent($"Spawned car {car.name}", false);
            }
        }

        [Command]
        public void CmdDestroyCar()
        {
            // TODO: RPC to explode instead of destroy
            DestroyCarByServer();
        }

        private void DestroyCarByServer()
        {
            Destroy(_car);
            _car = null;
        }

        private void ConfigureSpawnedCar(GameObject car)
        {
            car.transform.parent = transform;
            _hoveringDetails = Instantiate(hoveringPrefab, car.transform).GetComponent<HoveringDetails>();
            _hoveringDetails.Player = this;
            _hoveringDetails.DisplayScore(score);
            _hoveringDetails.DisplayHealth(health);
            //var healthAndScores = car.GetComponent<HealthAndScores>();
            //healthAndScores.Player = this;
            //_vehiclePhysics = car.GetComponent<VehiclePhysics>();
            //_vehiclePhysics.Player = this;
        }

        public void SetScore(int setScore)
        {
            score = setScore;
            RpcDisplayScore(setScore);
        }

        public void SetHealth(float setHealth)
        {
            if (ReferenceEquals(_car, null))
            {
                return;
            }

            RpcDisplayHealth(setHealth);
        }

        private void OnSyncHealth(float oldHealth, float newHealth)
        {
            //  Hook gets invoked before Start configures _vehiclePhysics
            if (!ReferenceEquals(_vehiclePhysics, null))
            {
                _vehiclePhysics.health = newHealth;
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

        [ClientRpc]
        public void RpcDisplayPlayerHitEvent(string player2, float health, int score)
        {
            ArenaUi.Instance.DisplayPlayerHitEvent(playerName, player2, health, score);
        }

        [ClientRpc]
        public void RpcDisplayObjectHitEvent(string by, float hp)
        {
            ArenaUi.Instance.DisplayObjectHitEvent(playerName, by, hp);
        }

        [ClientRpc]
        public void RpcDisplayPositiveEvent(string positiveMessage, bool announcement)
        {
            DisplayPositiveEvent(positiveMessage, announcement);
        }

        public void DisplayPositiveEvent(string positiveMessage, bool announcement)
        {
            ArenaUi.Instance.DisplayPositiveEvent(!announcement ? $"{playerName}: {positiveMessage}" : positiveMessage);
        }

        [Command]
        public void CmdRestartServer(string remoteAdminPassword)
        {
            Debug.Log($"Attempt of {playerName} to restart server using password {remoteAdminPassword}");
            if (MainMenu.RemoteAdminPassword != null && MainMenu.RemoteAdminPassword.Equals(remoteAdminPassword))
            {
                GameObject.Find("NetworkManager").GetComponent<NetworkManager>().StopHost();
            }
        }
    }
}
