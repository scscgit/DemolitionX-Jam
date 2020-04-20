using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    private Text t;
    public bool start;
    public float tim;
    Animator a;
    
    // Start is called before the first frame update
    void Awake()
    {
        t = GetComponent<Text>();  
        a = GetComponent<Animator>();      
    }

    // Update is called once per frame
    void Update()
    {
        if(start)
        { 
            tim -= Time.deltaTime;
            string m = Mathf.Floor((tim % 3600)/60).ToString();
            string s = Mathf.Floor(tim % 60).ToString();
            t.text = m + ":" + s;
        }

        a.SetBool("start", start);
    }
}
