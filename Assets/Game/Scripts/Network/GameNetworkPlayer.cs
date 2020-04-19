using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts.Network
{
    public class GameNetworkPlayer : NetworkBehaviour
    {
        public Camera spectatorCamera;
        public Camera vehicleCamera;

        public void Start()
        {
            ChangeCar();
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
            spectatorCamera.enabled = false;
            vehicleCamera.enabled = false;
            SceneManager.LoadScene("CarSelection", LoadSceneMode.Additive);
        }

        public void SelectedCar(int carIndex)
        {
            // TODO
            Debug.LogError("TODO: spawn player with selected car: " + carIndex);

            vehicleCamera.enabled = true;
        }
    }
}
