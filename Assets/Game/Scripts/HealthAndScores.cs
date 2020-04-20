using UnityEngine;
using UnityEngine.UI;

public class HealthAndScores : MonoBehaviour
{
    public float health = 100f;
    public int scores = 0;
    private Rigidbody r;
    public Slider s;
    public Text scoreText;

    private void Start() {
        r = GetComponent<Rigidbody>();
        s.value = 1f;
    }


    private void OnCollisionEnter(Collision other) 
    {
        float v = r.velocity.magnitude;
        if(!other.gameObject.GetComponent<Rigidbody>())
        {
            health -= v/20f;
        }
  
        
        if(other.gameObject.GetComponent<Rigidbody>())
        {
            float impactMag = Mathf.Abs(other.gameObject.GetComponent<Rigidbody>().velocity.magnitude * other.gameObject.GetComponent<Rigidbody>().mass - v * r.mass);                      
            if(v > other.gameObject.GetComponent<Rigidbody>().velocity.magnitude)
            {
                if(other.gameObject.GetComponent<HealthAndScores>()) 
                {
                    other.gameObject.GetComponent<HealthAndScores>().health -= impactMag/10000f;
                    scores += Mathf.RoundToInt(impactMag);
                }
                else health -= 1;                
            }
            else
            {
                health -= impactMag/10000f;
            }
        } 

        
        
        s.value = health/100f;   
        scoreText.text = scores.ToString();    
    }
}
