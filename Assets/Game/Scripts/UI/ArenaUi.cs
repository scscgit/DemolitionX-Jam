using System;
using System.Collections.Generic;
using Game.Scripts.Network;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class ArenaUi : MonoBehaviour
    {
        public static ArenaUi Instance;

        private enum EventType
        {
            PlayerHit,
            ObjectHit,
            Positive,
        }

        private static readonly TimeSpan GroupEventsWithinInterval = TimeSpan.FromSeconds(1.5f);

        public GameObject[] enableOnStart;
        public GameObject[] toggleOnEscape;
        public GameObject canvas;
        public GameObject events;
        public GameObject eventRedPrefab;
        public GameObject eventGreenPrefab;

        private GameNetworkPlayer _activePlayer;
        private readonly LinkedList<GameObject> _childrenEvents = new LinkedList<GameObject>();
        private Tuple<EventType?, float> _lastEventTypeAndTime = new Tuple<EventType?, float>(null, 0);

        private Tuple<string, string, float, int> _lastPlayerHitEvent =
            new Tuple<string, string, float, int>(null, null, 0, 0);

        private Tuple<string, string, float> _lastObjectHitEvent =
            new Tuple<string, string, float>(null, null, 0);

        private void Awake()
        {
            Instance = this;
        }

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
            _activePlayer.ChangeCarByLocalPlayer();
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

        public void DisplayPlayerHitEvent(string player1, string player2, float hp, int score)
        {
            var eventTime = Time.time;
            // Group with the last event
            if (_lastEventTypeAndTime.Item1 == EventType.PlayerHit
                && Time.time - _lastEventTypeAndTime.Item2 < GroupEventsWithinInterval.Milliseconds / 1000f
                && _lastPlayerHitEvent.Item1.Equals(player1)
                && _lastPlayerHitEvent.Item2.Equals(player2))
            {
                Destroy(_childrenEvents.Last.Value);
                _childrenEvents.RemoveLast();
                eventTime = _lastEventTypeAndTime.Item2;
                hp += _lastPlayerHitEvent.Item3;
                score += _lastPlayerHitEvent.Item4;
            }

            _lastPlayerHitEvent = new Tuple<string, string, float, int>(player1, player2, hp, score);
            _lastEventTypeAndTime = new Tuple<EventType?, float>(EventType.PlayerHit, eventTime);

            var add = Instantiate(eventRedPrefab, events.transform);
            add.transform.SetSiblingIndex(0);
            add.GetComponentInChildren<Text>().text =
                $"[{DateTime.Now:mm:ss}] {player1} -> {player2} for {hp:0.0} HP, {score} score";
            AddEvent(add);
        }

        public void DisplayObjectHitEvent(string player, string by, float hp)
        {
            var eventTime = Time.time;
            // Group with the last event
            if (_lastEventTypeAndTime.Item1 == EventType.ObjectHit
                && Time.time - _lastEventTypeAndTime.Item2 < GroupEventsWithinInterval.Milliseconds / 1000f
                && _lastObjectHitEvent.Item1.Equals(player)
                && _lastObjectHitEvent.Item2.Equals(by))
            {
                Destroy(_childrenEvents.Last.Value);
                _childrenEvents.RemoveLast();
                eventTime = _lastEventTypeAndTime.Item2;
                hp += _lastObjectHitEvent.Item3;
            }

            _lastObjectHitEvent = new Tuple<string, string, float>(player, by, hp);
            _lastEventTypeAndTime = new Tuple<EventType?, float>(EventType.ObjectHit, eventTime);

            var add = Instantiate(eventRedPrefab, events.transform);
            add.transform.SetSiblingIndex(0);
            add.GetComponentInChildren<Text>().text =
                $"[{DateTime.Now:mm:ss}] {player} lost {hp:0.0} HP by {by}";
            AddEvent(add);
        }

        public void DisplayPositiveEvent(string positiveMessage)
        {
            _lastEventTypeAndTime = new Tuple<EventType?, float>(EventType.Positive, Time.time);

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
