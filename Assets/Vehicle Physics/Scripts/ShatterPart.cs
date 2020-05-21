using UnityEngine;

//[RequireComponent(typeof(Renderer))]
//[DisallowMultipleComponent]
[AddComponentMenu("Shatter Part", 2)]

//Class for parts that shatter
public class ShatterPart : MonoBehaviour
{
    public GameObject parentBody;

    //[System.NonSerialized]
    //public Renderer rend;
    [System.NonSerialized] public bool shattered;

    public float breakForce = 5f;

//
    //[Tooltip("Transform used for maintaining seams when deformed after shattering")]
    public Transform seamKeeper;

    //[System.NonSerialized]
    //public Material initialMat;
    //public Material brokenMaterial;
    public ParticleSystem shatterParticles;
    public AudioSource shatterSnd;
    public AudioClip shatterClip;

    void Update()
    {
        // parentBody can be destroyed if the car re-spawns
        if (shattered && parentBody)
        {
            //Destroy(shatterParticles.gameObject, 2.02f);
            if (parentBody.GetComponent<VehiclePhysics>().hasPivotIssues)
            {
                transform.parent.transform.parent = null;
                Destroy(transform.parent.gameObject, 2f);
            }
            else
            {
                transform.parent = null;
                Destroy(gameObject, 2f);
            }

            //Destroy(GetComponent<ShatterPart>(), 2.5f);
        }
    }

    public void Shatter()
    {
        if (!shattered)
        {
            shattered = true;

            if (shatterParticles)
            {
                shatterParticles.Play();
            }

            GetComponent<Renderer>().enabled = false;


            shatterSnd = CreateAudioSource.NewAudioSource(gameObject, "Shatter Sound AudioSource", 5, 20, 1,
                shatterClip, false, true, true);

            if (!shatterSnd.isPlaying)
                shatterSnd.Play();
        }
    }
}
