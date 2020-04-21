using System.Linq;
using Game.Scripts.Network;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Timer : NetworkBehaviour
{
    public Text timerText;
    public Animator a;
    public Text resultsText;
    public float mins = 3;
    public float sec;
    public float breakSec = 10;
    public float time;
    private bool paused;

    // Start is called before the first frame update
    void Awake()
    {
        OnReset();
    }

    public void OnReset()
    {
        resultsText.enabled = false;
        paused = false;
        time = (sec + (mins * 60));
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            if (time > 0)
            {
                time -= Time.deltaTime;
                RpcSetTime(time);
            }
        }

        if (paused)
        {
            if (time < 0)
            {
                OnReset();
            }

            return;
        }

        string m = Mathf.Floor((time / 60) % 60).ToString("00");
        string s = Mathf.Floor(time % 60).ToString("00");
        timerText.text = m + ":" + s;

        if (time < 0)
        {
            ResultScreen();
        }

        if (time < 11)
        {
            a.SetBool("start", true);
        }
    }

    [ClientRpc]
    public void RpcSetTime(float time)
    {
        this.time = time;
    }

    public void ResultScreen()
    {
        var players = FindObjectsOfType<GameNetworkPlayer>();

        paused = true;
        time = breakSec;
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
        }

        resultsText.enabled = true;
    }
}
