using System.Linq;
using Game.Scripts.Network;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.Serialization;

public class Timer : NetworkBehaviour
{
    public Text timerText;
    [FormerlySerializedAs("a")] public Animator animator;
    public Text resultsText;
    public Environment environment;

    public float mins = 3;
    public float sec;
    public float breakSec = 10;
    public float time;
    [Range(0.01f, 5f)] public float timeSyncInterval = 0.5f;
    private float _lastSyncedTime;
    private bool paused;

    void Awake()
    {
        resultsText.enabled = false;
        time = sec + mins * 60;
        _lastSyncedTime = time;
    }

    void Update()
    {
        if (isServer)
        {
            // Countdown is above zero (paused or not paused)
            if (time - Time.deltaTime > 0)
            {
                time -= Time.deltaTime;
                if (Mathf.Abs(time - _lastSyncedTime) > timeSyncInterval)
                {
                    RpcSetTime(time, paused);
                    _lastSyncedTime = time;
                }
            }
            // Countdown reached zero, toggle paused state
            else
            {
                if (paused)
                {
                    //time = sec + (mins * 60);
                    //RestartGame();
                    RpcSetTime(sec + (mins * 60), false);
                    _lastSyncedTime = sec + (mins * 60);
                }
                else
                {
                    //time = breakSec;
                    //ResultScreen();
                    RpcSetTime(breakSec, true);
                    _lastSyncedTime = breakSec;
                }
            }
        }

        string m = Mathf.Floor((time / 60) % 60).ToString("00");
        string s = Mathf.Floor(time % 60).ToString("00");
        timerText.text = m + ":" + s;

        if (!paused && time < 11)
        {
            animator.SetBool("start", true);
        }
    }

    [ClientRpc]
    public void RpcSetTime(float time, bool doPause)
    {
        if (doPause)
        {
            if (!paused)
            {
                ResultScreen();
            }

            this.time = time;
        }
        else
        {
            if (paused)
            {
                RestartGame();
            }

            paused = false;
            this.time = time;
        }
    }

    public void ResultScreen()
    {
        animator.SetBool("start", false);
        paused = true;

        // Display the best player based on locally available synced data, don't involve server
        var players = FindObjectsOfType<GameNetworkPlayer>();
        if (players.Length == 0)
        {
            resultsText.text = "No players, what a shame";
        }
        else
        {
            var best = players.First(p1 => p1.score == players.Max(p2 => p2.score));
            resultsText.text = $@"Best player:
{best.playerName}

with score:
{best.score}";
            best.DisplayPositiveEvent($"{best.playerName} won with score {best.score}", true);
        }

        resultsText.enabled = true;
    }

    private void RestartGame()
    {
        resultsText.enabled = false;
        paused = false;
        if (isServer)
        {
            environment.RespawnAll();
            var players = FindObjectsOfType<GameNetworkPlayer>();
            var onlyOnce = true;
            foreach (var player in players)
            {
                player.SetScore(0);
                player.SetHealth(100);
                player.RespawnCarByNewRoundByServer();
                if (onlyOnce)
                {
                    // Report the round start via server (this could be done locally as well)
                    player.RpcDisplayPositiveEvent("New round started", true);
                    onlyOnce = false;
                }
            }
        }
    }
}
