using Game.Scripts.Network;
using UnityEngine;

namespace Game.Scripts.UI
{
    public class ArenaUi : MonoBehaviour
    {
        public GameObject[] enableOnStart;
        public GameObject[] toggleOnEscape;
        public GameObject canvas;

        private GameNetworkPlayer _activePlayer;

        void Start()
        {
            for (var i = 0; i < enableOnStart.Length; i++)
            {
                enableOnStart[i].SetActive(true);
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                for (var i = 0; i < toggleOnEscape.Length; i++)
                {
                    toggleOnEscape[i].SetActive(!toggleOnEscape[i].activeSelf);
                }

                if (canvas.activeSelf)
                {
                    ChangeCar();
                }
            }
        }

        public void ChangeCar()
        {
            _activePlayer.ChangeCar();
        }

        public void EnableUi(GameNetworkPlayer player)
        {
            _activePlayer = player;
            canvas.SetActive(true);
        }

        public void DisableUi()
        {
            canvas.SetActive(false);
        }
    }
}
