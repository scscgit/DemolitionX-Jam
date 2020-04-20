using System;
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
        public Camera vehicleCamera;

        private Camera _spectatorCamera;

        [SyncVar]
        private GameObject _car;
        [SyncVar]
        public bool fetchCar = false;

        public void Start()
        {
            if (isLocalPlayer)
            {
                _spectatorCamera = GameObject.Find("SpectatorCamera").GetComponent<Camera>()
                                   ?? throw new NullReferenceException($"Missing SpectatorCamera");
                ChangeCar();
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
            _spectatorCamera.gameObject.SetActive(false);
            vehicleCamera.gameObject.SetActive(false);
            SceneManager.LoadScene("CarSelection", LoadSceneMode.Additive);
        }

        [Command]
        public void CmdSelectedCar(int carIndex)
        {
            var car = Instantiate(cars[carIndex], transform);
            NetworkServer.Spawn(car);
            _car = car;
            fetchCar = true;
        }

        public void Update()
        {
            if (fetchCar)
            {
                if (isLocalPlayer)
                {
                    vehicleCamera.gameObject.SetActive(true);
                    _car.transform.parent = transform;
                    _car.GetComponent<VehiclePhysics>().canControl = true;
                }
                fetchCar = false;
            }
        }
    }
}
