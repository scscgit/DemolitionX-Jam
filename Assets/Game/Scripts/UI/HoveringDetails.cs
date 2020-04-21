using Game.Scripts.Network;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class HoveringDetails : MonoBehaviour
    {
        public Text text;
        public Slider slider;

        public GameNetworkPlayer Player
        {
            set
            {
                _playerName = value.playerName;
                _vehicleCamera = value.vehicleCamera.transform;
                SetScore(0);
            }
        }

        private string _playerName;
        private Transform _vehicleCamera;

        private void Update()
        {
            if (!_vehicleCamera) // || !_playerName
            {
                Debug.Log("HoveringDetails doesn't have any camera assigned");
                return;
            }

            var cameraPosition = _vehicleCamera.transform.position;
            transform.rotation = Quaternion.LookRotation(
                (transform.position - cameraPosition).normalized
            );
        }

        private void SetScore(int score)
        {
            text.text = $"{_playerName}: {score}";
        }

        private void SetHealth(float healthPercentage)
        {
            slider.value = healthPercentage;
        }
    }
}
