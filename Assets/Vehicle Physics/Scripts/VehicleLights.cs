using UnityEngine;
using System.Collections;

///<summary>
///Class handles everything related to light
///Even controls their intensity, indicator time, etc....
///</summary>
public class VehicleLights : MonoBehaviour {

	private CommonSettings CommonSettingsInstance;
	private CommonSettings CommonSettings {
		get {
			if (CommonSettingsInstance == null) {
				CommonSettingsInstance = CommonSettings.Instance;
			}
			return CommonSettingsInstance;
		}
	}


	private VehiclePhysics carController;
	private Light _light;
	private Projector projector;

	[Tooltip("Type of the light")]
	public LightType lightType;
	public enum LightType{HeadLight, BrakeLight, ReverseLight, Indicator};
	///<summary>
	///How fast the light will be bright enough
	///</summary>
	[Tooltip("How fast the light will be bright enough")]
	public float inertia = 1f;

	// For Indicators.
	private VehiclePhysics.IndicatorsOn indicatorsOn;
	private AudioSource indicatorSound;
	public AudioClip indicatorClip{get{return CommonSettings.indicatorClip;}}

	void Start () {
		
		carController = GetComponentInParent<VehiclePhysics>();
		_light = GetComponent<Light>();
		_light.enabled = true;

		if(lightType == LightType.Indicator){
			
			if(!carController.transform.Find("All Audio Sources/Indicator Sound AudioSource"))
				indicatorSound = CreateAudioSource.NewAudioSource(carController.gameObject, "Indicator Sound AudioSource", 1f, 3f, 1, indicatorClip, false, false, false);
			else
				indicatorSound = carController.transform.Find("All Audio Sources/Indicator Sound AudioSource").GetComponent<AudioSource>();
			
		}

	}

	void Update () {

		switch(lightType){

		case LightType.HeadLight:
			if(!carController.lowBeamHeadLightsOn && !carController.highBeamHeadLightsOn)
				Lighting(0f);
			if(carController.lowBeamHeadLightsOn && !carController.highBeamHeadLightsOn){
				Lighting(.6f, 50f, 90f);
				transform.localEulerAngles = new Vector3(10f, 0f, 0f);
			}else if(carController.highBeamHeadLightsOn){
				Lighting(1f, 200f, 45f);
				transform.localEulerAngles = new Vector3(0f, 0f, 0f);
			}
			break;

		case LightType.BrakeLight:
			Lighting((!carController.lowBeamHeadLightsOn ? (carController._brakeInput >= .1f ? 1f : 0f)  : (carController._brakeInput >= .1f ? 1f : .3f)));
			break;

		case LightType.ReverseLight:
			Lighting(carController.direction == -1 ? 1f : 0f);
			break;

		case LightType.Indicator:
			indicatorsOn = carController.indicatorsOn;
			Indicators();
			break;

		}
		
	}

	void Lighting(float input){

		_light.intensity = Mathf.Lerp(_light.intensity, input, Time.deltaTime * inertia * 20f);

	}

	void Lighting(float input, float range, float spotAngle){

		_light.intensity = Mathf.Lerp(_light.intensity, input, Time.deltaTime * inertia * 20f);
		_light.range = range;
		_light.spotAngle = spotAngle;

	}

	void Indicators(){

		switch(indicatorsOn){

		case VehiclePhysics.IndicatorsOn.Left:

			if(transform.localPosition.x > 0f){
				Lighting (0);
				break;
			}

			if(carController.indicatorTimer >= .5f){
				Lighting (0);
				if(indicatorSound.isPlaying)
					indicatorSound.Stop();
			}else{
				Lighting (1);
				if(!indicatorSound.isPlaying && carController.indicatorTimer <= .05f)
					indicatorSound.Play();
			}
			if(carController.indicatorTimer >= 1f)
				carController.indicatorTimer = 0f;
			break;

		case VehiclePhysics.IndicatorsOn.Right:

			if(transform.localPosition.x < 0f){
				Lighting (0);
				break;
			}

			if(carController.indicatorTimer >= .5f){
				Lighting (0);
			if(indicatorSound.isPlaying)
				indicatorSound.Stop();
			}else{
				Lighting (1);
				if(!indicatorSound.isPlaying && carController.indicatorTimer <= .05f)
					indicatorSound.Play();
			}
			if(carController.indicatorTimer >= 1f)
				carController.indicatorTimer = 0f;
			break;

		case VehiclePhysics.IndicatorsOn.All:
			
			if(carController.indicatorTimer >= .5f){
				Lighting (0);
				if(indicatorSound.isPlaying)
					indicatorSound.Stop();
			}else{
				Lighting (1);
				if(!indicatorSound.isPlaying && carController.indicatorTimer <= .05f)
					indicatorSound.Play();
			}
			if(carController.indicatorTimer >= 1f)
				carController.indicatorTimer = 0f;
			break;

		case VehiclePhysics.IndicatorsOn.Off:
			
			Lighting (0);
			carController.indicatorTimer = 0f;
			break;
			
		}

	}		
}
