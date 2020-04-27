using System;
using System.Collections.Generic;
using Game.Scripts.Network;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class ArenaUi : MonoBehaviour
    {
        public GameObject[] enableOnStart;
        public GameObject[] toggleOnEscape;
        public GameObject canvas;
        public GameObject events;
        public GameObject eventRedPrefab;
        public GameObject eventGreenPrefab;

        private GameNetworkPlayer _activePlayer;
        private readonly LinkedList<GameObject> _childrenEvents = new LinkedList<GameObject>();

        void Start()
        {
            for (var childIndex = 0; childIndex < events.transform.childCount; childIndex++)
            {
                // Destroy the development-only examples of children
                Destroy(events.transform.GetChild(childIndex).gameObject);
            }

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

        public void DisplayPlayerHitEvent(string player1, string player2, float hp)
        {
            var add = Instantiate(eventRedPrefab, events.transform);
            add.transform.SetSiblingIndex(0);
            add.GetComponentInChildren<Text>().text =
                $"[{DateTime.Now:mm:ss}] {player1} -> {player2} for {hp:0.0} HP";
            AddEvent(add);
        }

        public void DisplayObjectHitEvent(string player, string target, float hp)
        {
            var add = Instantiate(eventRedPrefab, events.transform);
            add.transform.SetSiblingIndex(0);
            add.GetComponentInChildren<Text>().text =
                $"[{DateTime.Now:mm:ss}] {player} lost {hp:0.0} HP by {target} collision";
            AddEvent(add);
        }

        public void DisplayPositiveEvent(string positiveMessage)
        {
            var add = Instantiate(eventGreenPrefab, events.transform);
            add.transform.SetSiblingIndex(0);
            add.GetComponentInChildren<Text>().text =
                $"[{DateTime.Now:mm:ss}] {positiveMessage}";
            AddEvent(add);
        }

        private void AddEvent(GameObject eventObject)
        {
            _childrenEvents.AddLast(eventObject);

            var count = _childrenEvents.Count;
            if (count > 11)
            {
                // Remove the oldest events when there are too many
                //Destroy(events.transform.GetChild(events.transform.childCount - 1).gameObject); // Not the correct one
                var last = _childrenEvents.First.Value;
                _childrenEvents.RemoveFirst();
                // Don't make a duplicate request if already asynchronously destroyed
                if (last)
                {
                    Destroy(last);
                }
            }

            Destroy(eventObject, 6f);
        }
    }
}
