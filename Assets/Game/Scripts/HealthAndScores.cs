using Game.Scripts.UI;
using UnityEngine;

public class HealthAndScores : MonoBehaviour
{
    public float health = 100f;
    public int scores = 0;
    private Rigidbody r;

    public HoveringDetails display;

    private void Start()
    {
        r = GetComponent<Rigidbody>();
    }

    public void DisplayCurrent()
    {
        display.SetHealth(health);
        display.SetScore(scores);
    }

    private void OnCollisionEnter(Collision other)
    {
        float v = r.velocity.magnitude;
        if (!other.gameObject.GetComponent<Rigidbody>())
        {
            health -= v / 20f;
            display.SetHealth(health);
        }


        if (other.gameObject.GetComponent<Rigidbody>())
        {
            float impactMag = Mathf.Abs(other.gameObject.GetComponent<Rigidbody>().velocity.magnitude *
                other.gameObject.GetComponent<Rigidbody>().mass - v * r.mass);
            if (v > other.gameObject.GetComponent<Rigidbody>().velocity.magnitude)
            {
                var otherHealthAndScores = other.gameObject.GetComponent<HealthAndScores>();
                if (otherHealthAndScores)
                {
                    otherHealthAndScores.health -= impactMag / 10000f;
                    otherHealthAndScores.display.SetHealth(otherHealthAndScores.health);
                    scores += Mathf.RoundToInt(impactMag);
                    display.SetScore(scores);
                }
                else
                {
                    health -= 1;
                    display.SetHealth(health);
                }
            }
            else
            {
                health -= impactMag / 10000f;
                display.SetHealth(health);
            }
        }
    }
}
