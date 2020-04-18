using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts.UI
{
    public class MainMenu : MonoBehaviour
    {
        private void Awake()
        {
            // Android: don't sleep
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                SelectCar();
            }
        }

        public void SelectCar()
        {
            try
            {
                SceneManager.LoadScene("Game/Scenes/CarSelection");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                //SceneManager.LoadScene(1);
            }
        }

        public void ExitGame()
        {
            Application.Quit();
        }
    }
}
