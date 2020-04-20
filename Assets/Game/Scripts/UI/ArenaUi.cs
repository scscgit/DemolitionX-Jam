using Game.Scripts.Network;
using UnityEngine;

namespace Game.Scripts.UI
{
    public class ArenaUi : MonoBehaviour
    {
        public GameObject[] enableOnStart;
        public GameObject[] toggleOnEscape;
        public GameObject canvas;

        public GameNetworkPlayer ActivePlayer { get; set; }

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
            }
        }

        public void ChangeCar()
        {
            //gameObject.SetActive(false);
            ActivePlayer.ChangeCar();
        }

        public void EnableUi(GameNetworkPlayer player)
        {
            ActivePlayer = player;
            canvas.SetActive(true);
        }

        public void DisableUi()
        {
            canvas.SetActive(false);
        }
    }
}
