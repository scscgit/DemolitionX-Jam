using UnityEngine;

namespace Game.Scripts.UI
{
    public class UI : MonoBehaviour
    {
        public GameObject[] enableOnStart;
        public GameObject[] toggleOnEscape;

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
    }
}
