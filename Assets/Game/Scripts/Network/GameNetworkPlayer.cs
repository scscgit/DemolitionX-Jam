using System;
using Game.Scripts.UI;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts.Network
{
    public class GameNetworkPlayer : NetworkBehaviour
    {
        [Tooltip("All car prefabs available for Player's spawn; ordered as CarSelection")]
        public GameObject[] cars;

        [Tooltip("Reference to the child object of a vehicle camera, which is automatically enabled when playing")]
        public VehicleCamera vehicleCamera;

        public string playerName;

        private ArenaUi _arenaUi;
        private Camera _spectatorCamera;

        // TODO FIXME: SyncVar does nothing, not even with public field, so it's missing for old players before connect
        [SyncVar] private GameObject _car;

        public void Start()
        {
            playerName = MainMenu.PlayerName;
            _arenaUi = GameObject.Find("ArenaUI").GetComponent<ArenaUi>();
            if (isLocalPlayer)
            {
                _spectatorCamera = GameObject.Find("SpectatorCamera").GetComponent<Camera>()
                                   ?? throw new NullReferenceException($"Missing SpectatorCamera");
                ChangeCar();
            }
        }

        private void Update()
        {
            if (ReferenceEquals(_car, null))
            {
                var vehicle = transform.GetComponentInChildren<VehiclePhysics>();
                if (vehicle != null)
                {
                    Debug.Log("Quick-assigned missing _cars reference to a found VehiclePhysics");
                }
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
            CmdSelectedCar(carIndex, gameObject);
        }

        [Command]
        public void CmdSelectedCar(int carIndex, GameObject player)
        {
            var car = Instantiate(cars[carIndex], transform);
            NetworkServer.Spawn(car, player);
            // TODO: Placebo, doesn't work
            //_car = car;
            RpcSetup(car);
        }

        [ClientRpc]
        public void RpcSetup(GameObject car)
        {
            _car = car;
            if (isLocalPlayer)
            {
                _arenaUi.EnableUi(this);
                vehicleCamera.gameObject.SetActive(true);
                vehicleCamera.playerCar = car.transform;
                _car.transform.parent = transform;
                _car.GetComponent<VehiclePhysics>().canControl = true;
            }
        }
    }
}
