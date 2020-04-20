using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    private Text t;
    public bool start;
    public float mins;
    public float sec;
    public float time;
    Animator a;
    
    // Start is called before the first frame update
    void Awake()
    {
        t = GetComponent<Text>();  
        a = GetComponent<Animator>();
        OnReset();
    }

    public void OnReset()
    {
        time = (sec + (mins * 60));
    }

    // Update is called once per frame
    void Update()
    {
        if(start)
        { 
            time -= Time.deltaTime;
            string m = Mathf.Floor((time /60) % 60).ToString();
            string s = Mathf.Floor(time % 60).ToString();
            t.text = m + ":" + s;
        }
        if (time < 0)
            ResetGame();

        a.SetBool("start", start);
    }

    public void ResetGame()
    {
        
    }
}
