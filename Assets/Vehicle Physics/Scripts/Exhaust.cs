using UnityEngine;
using System.Collections;


///<summary>
///Class that handles the exaust of the vehicle!
///</summary>
public class Exhaust : MonoBehaviour
{
    private CommonSettings CommonSettingsInstance;

    private CommonSettings CommonSettings
    {
        get
        {
            if (CommonSettingsInstance == null)
            {
                CommonSettingsInstance = CommonSettings.Instance;
            }

            return CommonSettingsInstance;
        }
    }


    private VehiclePhysics carController;
    private ParticleSystem particle;
    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.MinMaxCurve emissionRate;
    public ParticleSystem flame;
    private ParticleSystem.EmissionModule subEmission;
    private ParticleSystem.MinMaxCurve subEmissionRate;
    private Light flameLight;

    public float flameTime = 0f;
    private AudioSource flameSource;

    public Color flameColor = Color.red;
    public Color boostFlameColor = Color.blue;

    public bool previewFlames = false;

    void Start()
    {
        carController = GetComponentInParent<VehiclePhysics>();
        particle = GetComponent<ParticleSystem>();
        emission = particle.emission;

        if (flame)
        {
            subEmission = flame.emission;
            flameLight = flame.GetComponentInChildren<Light>();
            flameSource = CreateAudioSource.NewAudioSource(gameObject, "Exhaust Flame AudioSource", 10f, 25f, 1f,
                CommonSettings.exhaustFlameClips[0], false, false, false);
            flameLight.renderMode = CommonSettings.useLightsAsVertexLights
                ? LightRenderMode.ForceVertex
                : LightRenderMode.ForcePixel;
        }
    }

    void Update()
    {
        if (!carController || !particle)
            return;

        if (carController.engineRunning)
        {
            if (carController.speed < 150)
            {
                if (!emission.enabled)
                    emission.enabled = true;
                if (carController._gasInput > .05f)
                {
                    emissionRate.constantMax = 50f;
                    emission.rate = emissionRate;
                    particle.startSpeed = 5f;
                    particle.startSize = 5f;
                    //particle.startLifetime = .25f;
                }
                else
                {
                    emissionRate.constantMax = 5;
                    emission.rate = emissionRate;
                    particle.startSpeed = .5f;
                    particle.startSize = 2.5f;
                    //particle.startLifetime = 1f;
                }
            }
            else
            {
                if (emission.enabled)
                    emission.enabled = false;
            }

            if (carController._gasInput >= .25f)
                flameTime = 0f;

            if (((carController.useExhaustFlame && carController.engineRPM >= 5000 && carController.engineRPM <= 5500
                  && carController._gasInput <= .25f && flameTime <= .5f) || carController._boostInput >= 1.5f)
                || previewFlames)
            {
                flameTime += Time.deltaTime;
                subEmission.enabled = true;

                if (flameLight)
                    flameLight.intensity = flameSource.pitch * 3f * Random.Range(.25f, 1f);

                if (carController._boostInput >= 1.5f && flame)
                {
                    flame.startColor = boostFlameColor;
                    flameLight.color = flame.startColor;
                }
                else
                {
                    flame.startColor = flameColor;
                    flameLight.color = flame.startColor;
                }

                if (!flameSource.isPlaying)
                {
                    flameSource.clip =
                        CommonSettings.exhaustFlameClips[Random.Range(0, CommonSettings.exhaustFlameClips.Length)];
                    flameSource.Play();
                }
            }
            else
            {
                subEmission.enabled = false;

                if (flameLight)
                    flameLight.intensity = 0f;
                if (flameSource.isPlaying)
                    flameSource.Stop();
            }
        }
        else
        {
            if (emission.enabled)
                emission.enabled = false;

            subEmission.enabled = false;

            if (flameLight)
                flameLight.intensity = 0f;
            if (flameSource.isPlaying)
                flameSource.Stop();
        }
    }
}
