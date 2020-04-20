using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Timer : NetworkBehaviour
{
    private Text t;
    public float mins;
    public float sec;
    public float time;
    Animator a;
    
    // Start is called before the first frame update
    void Awake()
    {
        t = GetComponent<Text>();  
        a = GetComponent<Animator>();
        if (isServer)
            OnReset();
    }

    public void OnReset()
    {
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
                string m = Mathf.Floor((time / 60) % 60).ToString();
                string s = Mathf.Floor(time % 60).ToString();
                RpcShowText(m + ":" + s,time);
            }

            if (time < 0)
                ResetGame();
        }
    }

    [ClientRpc]
    public void RpcShowText(string text, float time)
    {
        t.text = text;
        if (time < 11)
            a.SetBool("start", true);
    }

    public void ResetGame()
    {
        
    }
}
