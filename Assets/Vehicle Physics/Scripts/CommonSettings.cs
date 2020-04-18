using UnityEngine;
using System.Collections;

///<summary>
///Contains everything that is common for cars!
///</summary>
[System.Serializable]
public class CommonSettings : ScriptableObject {
	
	public static CommonSettings instance;
	public static CommonSettings Instance{	get{if(instance == null) instance = Resources.Load("ScriptableObjects/CommonSettings") as CommonSettings; return instance;}}


	[Range(.5f, 20f)]public float maxAngularVelocity = 6;

	// Behavior Types

	public string verticalInput = "Vertical";
	public string horizontalInput = "Horizontal";
	public KeyCode handbrakeKB = KeyCode.Space;
	public KeyCode startEngineKB = KeyCode.I;
	public KeyCode lowBeamHeadlightsKB = KeyCode.L;
	public KeyCode highBeamHeadlightsKB = KeyCode.K;
	public KeyCode rightIndicatorKB = KeyCode.E;
	public KeyCode leftIndicatorKB = KeyCode.Q;
	public KeyCode hazardIndicatorKB = KeyCode.Z;
	public KeyCode shiftGearUp = KeyCode.LeftShift;
	public KeyCode shiftGearDown = KeyCode.LeftControl;
	public KeyCode NGear = KeyCode.N;
	public KeyCode boostKB = KeyCode.F;
	public KeyCode slowMotionKB = KeyCode.G;
	public KeyCode changeCameraKB = KeyCode.C;
	public KeyCode enterExitVehicleKB = KeyCode.E;

	public bool useAutomaticGear = true;
	public GameObject contactParticles;


	// Used for using the lights more efficent and realistic
	public bool useLightsAsVertexLights = true;
	public bool useLightProjectorForLightingEffect = false;

	public GameObject chassisJoint;
	public GameObject exhaustGas;
	public SkidmarksManager skidmarksManager;
	public GameObject projector;
	public LayerMask projectorIgnoreLayer;

	public GameObject headLights;
	public GameObject brakeLights;
	public GameObject reverseLights;
	public GameObject indicatorLights;

	// Sound FX
	public AudioClip[] gearShiftingClips;
	public AudioClip[] crashClips;
	public AudioClip reversingClip;
	public AudioClip windClip;
	public AudioClip brakeClip;
	public AudioClip indicatorClip;
	public AudioClip NOSClip;
	public AudioClip turboClip;
	public AudioClip[] blowoutClip;
	public AudioClip[] exhaustFlameClips;
	public bool useSharedAudioSources = true;

	[Range(0f, 1f)]public float maxGearShiftingSoundVolume = .25f;
	[Range(0f, 1f)]public float maxCrashSoundVolume = 1f;
	[Range(0f, 1f)]public float maxWindSoundVolume = .1f;
	[Range(0f, 1f)]public float maxBrakeSoundVolume = .1f;
}
