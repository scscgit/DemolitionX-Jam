using Game.Scripts.Network;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class HoveringDetails : MonoBehaviour
    {
        public static Transform VehicleCamera;

        public Text text;
        public Slider slider;

        public GameNetworkPlayer Player
        {
            set
            {
                _playerName = value.playerName;
                _player = value;
            }
        }

        private string _playerName;
        private GameNetworkPlayer _player;

        private void Update()
        {
            if (!VehicleCamera) // || !_playerName
            {
                return;
            }

            var cameraPosition = VehicleCamera.transform.position;
            transform.rotation = Quaternion.LookRotation(
                (transform.position - cameraPosition).normalized
            );
        }

        public void DisplayScore(int score)
        {
            text.text = $"{_playerName}: {score}";
        }

        public void DisplayHealth(float healthPercentage)
        {
            slider.value = healthPercentage / 100;
        }
    }
}
