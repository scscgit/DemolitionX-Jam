using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(Rigidbody))]
///<summary>
///Main script that handles most of the functionalities of the vehicle!
///</summary>
public class VehiclePhysics : MonoBehaviour {

	[Header("Behaviour Preset")]
	/// <summary>
    /// How must the vehicle behave?
    /// </summary>
    [Tooltip("How must the vehicle behave?")]	
	public BehaviorType behaviorType = BehaviorType.SportsMuscle;
	public enum BehaviorType{CivilianCars, SuperCars, Jeeps, SportsMuscle, Rovers, Custom}
	[Header("Drive Choise")]
	/// <summary>
    /// What kind of vehicle is it?
    /// </summary>
    [Tooltip("What kind of vehicle is it?")]
	public WheelType _wheelTypeChoise = WheelType.RWD;
	public enum WheelType{FWD, RWD, AWD, BIASED}
	/// <summary>
    /// How much torque bias?
    /// </summary>
    [Tooltip("How much torque bias?")]
	[Range(0f, 100f)]public float biasedWheelTorque = 100f;

	private CommonSettings CommonSettingsInstance;
	private CommonSettings CommonSettings {
		get {
			if (CommonSettingsInstance == null) {
				CommonSettingsInstance = CommonSettings.Instance;
			}
			return CommonSettingsInstance;
		}
	}


	private Rigidbody rigid;		
	internal bool sleepingRigid = false;

	[Header("Wheel Transforms")]	
	/// <summary>
    /// Front Left Wheel Transform
    /// </summary>
    [Tooltip("Front Left Wheel Transform")]
	public Transform FrontLeftWheelTransform;
	/// <summary>
    /// Front Right Wheel Transform
    /// </summary>
    [Tooltip("Front Right Wheel Transform")]
	public Transform FrontRightWheelTransform;
	/// <summary>
    /// Front Rear Wheel Transform
    /// </summary>
    [Tooltip("Rear Left Wheel Transform")]
	public Transform RearLeftWheelTransform;
	/// <summary>
    /// Rear Right Wheel Transform 
    /// </summary>
    [Tooltip("Rear Right Wheel Transform")]
	public Transform RearRightWheelTransform;
	/// <summary>
    /// Any extra wheels other than four
    /// </summary>
     [Tooltip("Any extra wheels other than four")]
	public Transform[] ExtraRearWheelsTransform;

	[Header("Wheel Colliders")]	
	/// <summary>
    /// Front Left Wheel Collider
    /// </summary>
    [Tooltip("Front Left Wheel Collider")]
	public VehiclePhysicsWheelCollider FrontLeftWheelCollider;
	/// <summary>
    /// Front Left Wheel Collider
    /// </summary>
    [Tooltip("Front Left Wheel Collider")]
	public VehiclePhysicsWheelCollider FrontRightWheelCollider;
	/// <summary>
    /// Front Left Wheel Collider
    /// </summary>
    [Tooltip("Front Left Wheel Collider")]
	public VehiclePhysicsWheelCollider RearLeftWheelCollider;
	/// <summary>
    /// Front Left Wheel Collider
    /// </summary>
    [Tooltip("Front Left Wheel Collider")]
	public VehiclePhysicsWheelCollider RearRightWheelCollider;
	internal VehiclePhysicsWheelCollider[] allWheelColliders;
	/// <summary>
    /// Any extra wheels other than four
    /// </summary>
    [Tooltip("Any extra wheels other than four")]	
	public VehiclePhysicsWheelCollider[] ExtraRearWheelsCollider;

	[Header("Preferences")]	
	/// <summary>
    /// Applies Motor Torque for extra wheel colliders
    /// </summary>
    [Tooltip("Applies Motor Torque for extra wheel colliders")]	
	public bool applyEngineTorqueToExtraRearWheelColliders = true;
	/// <summary>
    /// Can The Car be Controlled?
    /// </summary>
    [Tooltip("Can The Car be Controlled?")]		
	public bool canControl = true;
	/// <summary>
    /// Is engine running?
    /// </summary>
    [Tooltip("Is engine running?")]				
	public bool engineRunning = true;
	/// <summary>
    /// Is automatic gear?
    /// </summary>
    [Tooltip("Is automatic gear?")]			
	public bool automaticGear = true;	
	/// <summary>
    /// Is semi automatic gear?
    /// </summary>
    [Tooltip("Is semi automatic gear?")]	
	public bool semiAutomaticGear = false;
	/// <summary>
    /// Is semi automatic gear?
    /// </summary>
    [Tooltip("Is semi automatic gear?")]			
	private bool canGoReverseNow = false;
	/// <summary>
    /// Is semi automatic gear?
    /// </summary>
    [Tooltip("Is semi automatic gear?")]	
	public bool useRevLimiter = true;
	/// <summary>
    /// Must use clutch margin? (for smoother first gear change.)
    /// </summary>
    [Tooltip("Must use clutch margin? (for smoother first gear change.)")]	
	public bool useClutchMarginAtFirstGear = true;
	
	/// <summary>
    /// Use exhaust flame?
    /// </summary>
    [Tooltip("Use exhaust flame?")]	
	public bool useExhaustFlame = true;

	[Header("Steering")]
	/// <summary>
    /// Steering wheel transform!
    /// </summary>
    [Tooltip("Steering Wheel Transform")]	
	public Transform SteeringWheel;		
	private Quaternion orgSteeringWheelRot;
	/// <summary>
    /// Axis of rotation of steering Wheel!
    /// </summary>
    [Tooltip("Axis of rotation of steering Wheel!")]	
	public SteeringWheelRotateAround steeringWheelRotateAround;
	public enum SteeringWheelRotateAround { XAxis, YAxis, ZAxis }
	/// <summary>
    /// How much the steering wheel must rotate? (with respect to the steer angle of wheel!)
    /// </summary>
    [Tooltip("How much the steering wheel must rotate? (with respect to the steer angle of wheel!)")]
	public float steeringWheelAngleMultiplier = 3f;
	/// <summary>
    /// Maximum Steering angle
    /// </summary>
    [Tooltip("Maximum Steering angle")]	
	public float steerAngle = 40f;		
	/// <summary>
    /// Minimum Steering angle
    /// </summary>
    [Tooltip("Minimum Steering angle")]	
	public float highspeedsteerAngle = 15f;	
	/// <summary>
    /// AT what speed minimum steer angle must be achieved?
    /// </summary>
    [Tooltip("AT what speed minimum steer angle must be achieved?")]		
	public float highspeedsteerAngleAtspeed = 100f;	

	[Header("Center of Mass")]
	/// <summary>
    /// Center of mass of the vehicle
    /// </summary>
    [Tooltip("Center of mass of the vehicle")]	
	public Transform COM;
	[Header("Engine Properties")]	
	/// <summary>
    /// Max Engine Torque
    /// </summary>
    [Tooltip("Max Engine Torque")]	
	public float engineTorque = 2000f;		
	/// <summary>
    /// Max Brake Torque
    /// </summary>
    [Tooltip("Max Brake Torque")]	
	public float brakeTorque = 2000f;		
	/// <summary>
    /// Max Engine RPM
    /// </summary>
    [Tooltip("Max Engine RPM")]	
	public float maxEngineRPM = 7000f;		
	/// <summary>
    /// Min Engine RPM
    /// </summary>
    [Tooltip("Min Engine RPM")]	
	public float minEngineRPM = 1000f;	
	/// <summary>
    /// Engine's inertia! (how fast can it reach max RPM?)
    /// </summary>
    [Tooltip("Engine's inertia! (how fast can it reach max RPM?)")]		
	[Range(.75f, 2f)]public float engineInertia = 1f;

	[Header("AntiRoll Bars")]	
	/// <summary>
    /// Horizontal Anti Roll Force at front wheels
    /// </summary>
    [Tooltip("Horizontal Anti Roll Force at front wheels")]		
	public float antiRollFrontHorizontal = 5000f;
	/// <summary>
    /// Horizontal Anti Roll Force at rear wheels
    /// </summary>
    [Tooltip("Horizontal Anti Roll Force at rear wheels")]				
	public float antiRollRearHorizontal = 5000f;
	/// <summary>
    /// Vertical Anti Roll Force 
    /// </summary>
    [Tooltip("Horizontal Anti Roll Force")]			
	public float antiRollVertical = 0f;		

	[Header("Speed")]
	/// <summary>
    /// Max speed vehicle can attain
    /// </summary>
    [Tooltip("Max speed vehicle can attain")]	
	public float maxspeed = 220f;	
	[HideInInspector] public float speed;		
	[HideInInspector] public float orgMaxSpeed;	
		
		
	private float orgSteerAngle = 0f;		
	internal float fuelInput = 1f;

	[Header("Transmission")]
	/// <summary>
    /// Total number of gears
    /// </summary>
    [Tooltip("Total number of gears")]	
	public int totalGears = 6;
	[HideInInspector]public int currentGear = 0;
	/// <summary>
    /// Time in seconds taken to change gear
    /// </summary>
    [Tooltip(" Time in seconds taken to change gear")]				
	[Range(0f, .5f)]public float gearShiftingDelay = .35f;
	/// <summary>
    /// Gear shifting treshold
    /// </summary>
    [Tooltip("Gear shifting treshold")]	
	[Range(.5f, .95f)]public float gearShiftingThreshold = .85f;
	/// <summary>
    /// Clutch's inertia, used to change the first gear
    /// </summary>
    [Tooltip("Clutch's inertia, used to change the first gear")]	
	[Range(.1f, .9f)]public float clutchInertia = .25f;
	private float orgGearShiftingThreshold;		
	[HideInInspector]public bool changingGear = false;		
	[HideInInspector]public bool NGear = false;		
	[HideInInspector]public int direction = 1;		
	[HideInInspector]public float launched = 0f;

	[Header("Audio")]	
	public AudioClip engineStartClip;
	private AudioSource engineStartSound;
	internal AudioSource engineSoundOn;
	public AudioClip engineClipOn;
	private AudioSource engineSoundOff;
	public AudioClip engineClipOff;
	private AudioSource engineSoundIdle;
	public AudioClip engineClipIdle;
	private AudioSource gearShiftingSound;

	private AudioClip[] gearShiftingClips{get{return CommonSettings.gearShiftingClips;}}
	private AudioSource crashSound;
	private AudioClip[] crashClips{get{return CommonSettings.crashClips;}}
	private AudioSource reversingSound;
	private AudioClip reversingClip{get{return CommonSettings.reversingClip;}}
	private AudioSource windSound;
	private AudioClip windClip{get{return CommonSettings.windClip;}}
	private AudioSource brakeSound;
	private AudioClip brakeClip{get{return CommonSettings.brakeClip;}}
	private AudioSource NOSSound;
	private AudioClip NOSClip{get{return CommonSettings.NOSClip;}}
	private AudioSource turboSound;
	private AudioClip turboClip{get{return CommonSettings.turboClip;}}
	private AudioSource blowSound;
	private AudioClip blowClip{get{return CommonSettings.turboClip;}}

	[Range(.25f, 1f)]public float minEngineSoundPitch = .75f;
	[Range(1.25f, 2f)]public float maxEngineSoundPitch = 1.75f;
	[Range(0f, 1f)]public float minEngineSoundVolume = .05f;
	[Range(0f, 1f)]public float maxEngineSoundVolume = .85f;

	private GameObject allContactParticles;
	
	[HideInInspector]public float gasInput = 0f;
	[HideInInspector]public float brakeInput = 0f;
	[HideInInspector]public float steerInput = 0f;
	[HideInInspector]public float clutchInput = 0f;
	[HideInInspector]public float handbrakeInput = 0f;
	[HideInInspector]public float boostInput = 1f;
	[HideInInspector]public bool cutGas = false;
	[HideInInspector]public float idleInput = 0f;

	internal float _gasInput{get{

			if(fuelInput <= .25f)
				return 0f;

			if(!automaticGear || semiAutomaticGear){
				if(!changingGear && !cutGas)
					return Mathf.Clamp01(gasInput);
				else
					return 0f;
			}else{
				if(!changingGear && !cutGas)
					return (direction == 1 ? Mathf.Clamp01(gasInput) : Mathf.Clamp01(brakeInput));
				else
					return 0f;
			}
				
		}set{gasInput = value;}}

	internal float _brakeInput{get{

			if(!automaticGear || semiAutomaticGear){
				return Mathf.Clamp01(brakeInput);
			}else{
				if(!cutGas)
					return (direction == 1 ? Mathf.Clamp01(brakeInput) : Mathf.Clamp01(gasInput));
				else
					return 0f;
			}
				
		}set{brakeInput = value;}}

	internal float _boostInput{get{
			
			if(useNOS && NoS > 5 && _gasInput >= .5f){
				return boostInput;
			}else{
				return 1f;
			}

		}set{boostInput = value;}}

	internal float _steerInput;


	internal float engineRPM = 0f;		
	internal float rawEngineRPM = 0f;

	[Header("Chassis")]	
	///<summary>
	/// The chassis of the vehicle
	///</summary>
	[Tooltip("The chassis of the vehicle")]
	public GameObject chassis;			
	
	[HideInInspector]public bool lowBeamHeadLightsOn = false;		
	[HideInInspector]public bool highBeamHeadLightsOn = false;		

	[HideInInspector]public IndicatorsOn indicatorsOn;		
	public enum IndicatorsOn{Off, Right, Left, All}
	[HideInInspector]public float indicatorTimer = 0f;		

	public GameObject contactSparkle{get{return CommonSettings.contactParticles;}}		
	[HideInInspector]public int maximumContactSparkle = 5;		
	private List<ParticleSystem> contactSparkeList = new List<ParticleSystem>();	

	private float oldRotation;
	[HideInInspector]public Transform velocityDirection;
	[HideInInspector]public Transform steeringDirection;
	[HideInInspector]public float velocityAngle;
	private float angle;
	private float angularVelo;

	[Header("Driving Assistances")]
	///<summary>
	/// Must we use ABS?
	///</summary>
	[Tooltip("Must we use ABS?")]
	public bool ABS = true;
	///<summary>
	/// ABS Treshold
	///</summary>
	[Tooltip("ABS Treshold")]
	[Range(.05f, .5f)]public float ABSThreshold = .35f;
	[Space(10)]
	///<summary>
	/// Must we use TCS?
	///</summary>
	[Tooltip("Must we use TCS?")]
	public bool TCS = true;
	///<summary>
	/// TCS Treshold
	///</summary>
	[Tooltip("TCS Treshold")]
	[Range(.05f, .5f)]public float TCSThreshold = .25f;
	///<summary>
	/// TCS Strength
	///</summary>
	[Tooltip("TCS Strength")]
	[Range(0f, 1f)]public float TCSStrength = 1f;
	[Space(10)]
	///<summary>
	/// Must we use ESP?
	///</summary>
	[Tooltip("Must we use ESP?")]
	public bool ESP = true;
	///<summary>
	/// ESP Treshold
	///</summary>
	[Tooltip("ESP Treshold")]
	[Range(0f, .5f)]public float ESPThreshold = .25f;
	///<summary>
	/// ESP Strength
	///</summary>
	[Tooltip("ESP Strength")]
	[Range(.1f, 1f)]public float ESPStrength = .25f;
	[Space(10)]
	///<summary>
	/// Must we use Steering Helper?
	///</summary>
	[Tooltip("Must we use Steering Helper?")]
	public bool steeringHelper = true;
	///<summary>
	/// Linear verticla strength
	///</summary>
	[Tooltip("linear vertical strength")]
	[Range(0f, 1f)] public float steerHelperLinearVelStrength = .1f;
	///<summary>
	/// Linear Angular strength
	///</summary>
	[Tooltip("linear angular strength")]
	[Range(0f, 1f)] public float steerHelperAngularVelStrength = .1f;
	[Space(10)]	
	///<summary>
	/// Must we use Traction Helper?
	///</summary>
	[Tooltip("Must we use Traction Helper?")]
	public bool tractionHelper = true;
	///<summary>
	/// Traction Helper strength
	///</summary>
	[Tooltip("Traction Helper strength")]
	[Range(0f, 1f)] public float tractionHelperStrength = .1f;
	[Space(10)]
	///<summary>
	/// Must we use Counter Steering?
	///</summary>
	[Tooltip("Must we use Counter Steering ?")]
	public bool applyCounterSteering = true;	
	
	[HideInInspector]public bool ABSAct = false;
	[HideInInspector]public bool TCSAct = false;
	[HideInInspector]public bool ESPAct = false;

	[HideInInspector]public bool overSteering = false;
	[HideInInspector]public bool underSteering = false;

	internal float DriftAngle = 0f;
	internal bool DriftingNow = false;

	[HideInInspector]public float frontSlip = 0f;
	[HideInInspector]public float rearSlip = 0f;


	[HideInInspector]public float turboBoost = 0f;
	[HideInInspector]public float NoS = 100f;
	private float NoSConsumption = 25f;
	private float NoSRegenerateTime = 10f;

	[Header("Turbo and NOS (for recing game!)")]
	///<summary>
	/// Must we use NOS?
	///</summary>
	[Tooltip("Must we use NOS?")]
	public bool useNOS = false;
	///<summary>
	/// Must we use Turbo?
	///</summary>
	[Tooltip("Must we use Turbo?")]
	public bool useTurbo = false;

	[Header("Needles")]
	///<summary>
	/// Speed Needle's transform
	///</summary>
	[Tooltip("Speed Needle's transform")]
	public Transform speedNeedle;
	///<summary>
	/// Speed Needle's angle at 0 speed
	///</summary>
	[Tooltip("Speed Needle's angle at 0 speed")]
	public float speedStartAngle;
	///<summary>
	/// Speed Needle's angle at max speed
	///</summary>
	[Tooltip("Speed Needle's angle at max speed")]
	public float speedEndAngle;
	[Space(10)]
	///<summary>
	/// RPM Needle's transform
	///</summary>
	[Tooltip("RPM Needle's transform")]
	public Transform rpmNeedle;
	///<summary>
	/// RPM Needle's angle at 0 RPM
	///</summary>
	[Tooltip("RPM Needle's angle at 0 RPM")]	
	public float rpmStartAngle;
	///<summary>
	/// RPM Needle's angle at max RPM
	///</summary>
	[Tooltip("RPM Needle's angle at max RPM")]
	public float rpmEndAngle;

	[Header("Generated Curves!")]
	[Space(4f)]
	public AnimationCurve[] engineTorqueCurve;		
	public float[] targetSpeedForGear;		
	public float[] maxSpeedForGear;	

	void Awake (){

		rigid = GetComponent<Rigidbody>();
		rigid.maxAngularVelocity = CommonSettings.maxAngularVelocity;

		allWheelColliders = GetComponentsInChildren<VehiclePhysicsWheelCollider>();

		FrontLeftWheelCollider.wheelModel = FrontLeftWheelTransform;
		FrontRightWheelCollider.wheelModel = FrontRightWheelTransform;
		RearLeftWheelCollider.wheelModel = RearLeftWheelTransform;
		RearRightWheelCollider.wheelModel = RearRightWheelTransform;

		for (int i = 0; i < ExtraRearWheelsCollider.Length; i++) {
			ExtraRearWheelsCollider[i].wheelModel = ExtraRearWheelsTransform[i];
		}

		orgSteerAngle = steerAngle;

		allContactParticles = new GameObject("All Contact Particles");
		allContactParticles.transform.SetParent(transform, false);

		SoundsInitialize();

		if (chassis) {
			if (!chassis.GetComponent<Chassis> ())
				chassis.AddComponent<Chassis> ();
		}

		switch(behaviorType){

		case BehaviorType.Jeeps:
			steeringHelper = true;
			tractionHelper = true;
			ABS = false;
			ESP = false;
			TCS = false;
			steerHelperLinearVelStrength = Mathf.Clamp(steerHelperLinearVelStrength, .5f, 1f);
			steerHelperAngularVelStrength = Mathf.Clamp(steerHelperAngularVelStrength, 1f, 1f);
			tractionHelperStrength = Mathf.Clamp(tractionHelperStrength, .25f, 1f);
			antiRollFrontHorizontal = Mathf.Clamp(antiRollFrontHorizontal, 10000f, Mathf.Infinity);
			antiRollRearHorizontal = Mathf.Clamp(antiRollRearHorizontal, 10000f, Mathf.Infinity);
			gearShiftingDelay = Mathf.Clamp(gearShiftingDelay, 0f, .1f);
			break;

		case BehaviorType.SportsMuscle:
			steeringHelper = false;
			tractionHelper = true;
			ABS = false;
			ESP = false;
			TCS = false;
			highspeedsteerAngle = Mathf.Clamp(highspeedsteerAngle, 40f, 50f);
			highspeedsteerAngleAtspeed = Mathf.Clamp(highspeedsteerAngleAtspeed, 100f, maxspeed);
			tractionHelperStrength = Mathf.Clamp(tractionHelperStrength, .5f, 1f);
			engineTorque = Mathf.Clamp(engineTorque, 2000f, Mathf.Infinity);
			antiRollFrontHorizontal = Mathf.Clamp(antiRollFrontHorizontal, 2500f, Mathf.Infinity);
			antiRollRearHorizontal = Mathf.Clamp(antiRollRearHorizontal, 2500f, Mathf.Infinity);
			gearShiftingDelay = Mathf.Clamp(gearShiftingDelay, 0f, .15f);
			break;

		case BehaviorType.Rovers:
			steeringHelper = true;
			tractionHelper = true;
			ABS = false;
			ESP = false;
			TCS = false;
			steerHelperLinearVelStrength = Mathf.Clamp(steerHelperLinearVelStrength, .5f, 1f);
			steerHelperAngularVelStrength = Mathf.Clamp(steerHelperAngularVelStrength, 1f, 1f);
			highspeedsteerAngle = Mathf.Clamp(highspeedsteerAngle, 30f, 50f);
			highspeedsteerAngleAtspeed = Mathf.Clamp(highspeedsteerAngleAtspeed, 100f, maxspeed);
			antiRollFrontHorizontal = Mathf.Clamp(antiRollFrontHorizontal, 20000f, Mathf.Infinity);
			antiRollRearHorizontal = Mathf.Clamp(antiRollRearHorizontal, 20000f, Mathf.Infinity);
			gearShiftingDelay = Mathf.Clamp(gearShiftingDelay, 0f, .1f);
			break;

		case BehaviorType.SuperCars:
			steeringHelper = true;
			tractionHelper = true;
			steerHelperLinearVelStrength = Mathf.Clamp(steerHelperLinearVelStrength, .25f, 1f);
			steerHelperAngularVelStrength = Mathf.Clamp(steerHelperAngularVelStrength, .25f, 1f);
			tractionHelperStrength = Mathf.Clamp(tractionHelperStrength, .25f, 1f);
			antiRollFrontHorizontal = Mathf.Clamp(antiRollFrontHorizontal, 10000f, Mathf.Infinity);
			antiRollRearHorizontal = Mathf.Clamp(antiRollRearHorizontal, 10000f, Mathf.Infinity);
			break;

		case BehaviorType.CivilianCars:
			antiRollFrontHorizontal = Mathf.Clamp(antiRollFrontHorizontal, 1000f, Mathf.Infinity);
			antiRollRearHorizontal = Mathf.Clamp(antiRollRearHorizontal, 1000f, Mathf.Infinity);
			break;

		}

	}

	void OnEnable(){

		currentGear = 0;
		changingGear = false;

	}

	/// <summary>
	/// Creates wheel collider in one single click on the inspector!
	/// </summary>
	public void CreateWheelColliders (){

		List <Transform> allWheelModels = new List<Transform>();
		allWheelModels.Add(FrontLeftWheelTransform); allWheelModels.Add(FrontRightWheelTransform); allWheelModels.Add(RearLeftWheelTransform); allWheelModels.Add(RearRightWheelTransform);

		if (ExtraRearWheelsTransform.Length > 0 && ExtraRearWheelsTransform [0]) {
			foreach (Transform t in ExtraRearWheelsTransform)
				allWheelModels.Add (t);
		}

		if(allWheelModels != null && allWheelModels[0] == null){
			return;
		}

		Quaternion currentRotation = transform.rotation;

		transform.rotation = Quaternion.identity;

		GameObject WheelColliders = new GameObject("Wheel Colliders");
		WheelColliders.transform.SetParent(transform, false);
		WheelColliders.transform.localRotation = Quaternion.identity;
		WheelColliders.transform.localPosition = Vector3.zero;
		WheelColliders.transform.localScale = Vector3.one;

		foreach(Transform wheel in allWheelModels){
			
			GameObject wheelcollider = new GameObject(wheel.transform.name); 
			
			wheelcollider.transform.position = wheel.transform.position;
			wheelcollider.transform.rotation = transform.rotation;
			wheelcollider.transform.name = wheel.transform.name;
			wheelcollider.transform.SetParent(WheelColliders.transform);
			wheelcollider.transform.localScale = Vector3.one;
			wheelcollider.AddComponent<WheelCollider>();

			Bounds biggestBound = new Bounds();
			Renderer[] renderers = wheel.GetComponentsInChildren<Renderer>();

			foreach (Renderer render in renderers) {
				if (render != GetComponent<Renderer>()){
					if(render.bounds.size.z > biggestBound.size.z)
						biggestBound = render.bounds;
				}
			}

			wheelcollider.GetComponent<WheelCollider>().radius = (biggestBound.extents.y) / transform.localScale.y;
			wheelcollider.AddComponent<VehiclePhysicsWheelCollider>();
			JointSpring spring = wheelcollider.GetComponent<WheelCollider>().suspensionSpring;

			spring.spring = 40000f;
			spring.damper = 1500f;
			spring.targetPosition = .5f;

			wheelcollider.GetComponent<WheelCollider>().suspensionSpring = spring;
			wheelcollider.GetComponent<WheelCollider>().suspensionDistance = .2f;
			wheelcollider.GetComponent<WheelCollider>().forceAppPointDistance = .1f;
			wheelcollider.GetComponent<WheelCollider>().mass = 40f;
			wheelcollider.GetComponent<WheelCollider>().wheelDampingRate = 1f;

			WheelFrictionCurve sidewaysFriction;
			WheelFrictionCurve forwardFriction;
			
			sidewaysFriction = wheelcollider.GetComponent<WheelCollider>().sidewaysFriction;
			forwardFriction = wheelcollider.GetComponent<WheelCollider>().forwardFriction;

			forwardFriction.extremumSlip = .3f;
			forwardFriction.extremumValue = 1;
			forwardFriction.asymptoteSlip = .8f;
			forwardFriction.asymptoteValue = .6f;
			forwardFriction.stiffness = 1.5f;

			sidewaysFriction.extremumSlip = .3f;
			sidewaysFriction.extremumValue = 1;
			sidewaysFriction.asymptoteSlip = .5f;
			sidewaysFriction.asymptoteValue = .8f;
			sidewaysFriction.stiffness = 1.5f;

			wheelcollider.GetComponent<WheelCollider>().sidewaysFriction = sidewaysFriction;
			wheelcollider.GetComponent<WheelCollider>().forwardFriction = forwardFriction;

		}
		
		VehiclePhysicsWheelCollider[] allWheelColliders = new VehiclePhysicsWheelCollider[allWheelModels.Count];
		allWheelColliders = GetComponentsInChildren<VehiclePhysicsWheelCollider>();
		
		FrontLeftWheelCollider = allWheelColliders[0];
		FrontRightWheelCollider = allWheelColliders[1];
		RearLeftWheelCollider = allWheelColliders[2];
		RearRightWheelCollider = allWheelColliders[3];

		ExtraRearWheelsCollider = new VehiclePhysicsWheelCollider[ExtraRearWheelsTransform.Length];

		for (int i = 0; i < ExtraRearWheelsTransform.Length; i++) {
			ExtraRearWheelsCollider [i] = allWheelColliders [i + 4];
		}

		transform.rotation = currentRotation;
		
	}

	/// <summary>
	/// Initializes audio sources with audio clips, and set's their properties!
	/// </summary>
	void SoundsInitialize (){

		engineSoundOn = CreateAudioSource.NewAudioSource(gameObject, "Engine Sound On AudioSource", 5, 50, 0, engineClipOn, true, true, false);
		engineSoundOff = CreateAudioSource.NewAudioSource(gameObject, "Engine Sound Off AudioSource", 5, 25, 0, engineClipOff, true, true, false);
		engineSoundIdle = CreateAudioSource.NewAudioSource(gameObject, "Engine Sound Idle AudioSource", 5, 25, 0, engineClipIdle, true, true, false);

		reversingSound = CreateAudioSource.NewAudioSource(gameObject, "Reverse Sound AudioSource", 1, 10, 0, reversingClip, true, false, false);
		windSound = CreateAudioSource.NewAudioSource(gameObject, "Wind Sound AudioSource", 1, 10, 0, windClip, true, true, false);
		brakeSound = CreateAudioSource.NewAudioSource(gameObject, "Brake Sound AudioSource", 1, 10, 0, brakeClip, true, true, false);

		if(useNOS)
			NOSSound = CreateAudioSource.NewAudioSource(gameObject, "NOS Sound AudioSource", 5, 10, 1f, NOSClip, true, false, false);
		if(useNOS || useTurbo)
			blowSound = CreateAudioSource.NewAudioSource(gameObject, "NOS Blow", 1, 10, 1, null, false, false, false);
		if(useTurbo){
			turboSound = CreateAudioSource.NewAudioSource(gameObject, "Turbo Sound AudioSource", .1f, .5f, 0, turboClip, true, true, false);
			CreateAudioSource.NewHighPassFilter(turboSound, 10000f, 10);
		}
		
	}

	/// <Summary>
	/// Starts engine, or stops engine if a;ready started
	/// </summary>		
	public void KillOrStartEngine (){
		
		if(engineRunning){
			engineRunning = false;
			fuelInput = 0f;
		}else{
			StartCoroutine("StartEngine");
		}
		
	}

	/// <Summary>
	/// Starts engine
	/// </summary>	
	public void StartEngine (){

		StartCoroutine ("StartEngineDelayed");

	}

	public IEnumerator StartEngineDelayed (){

		engineRunning = false;
		engineStartSound = CreateAudioSource.NewAudioSource(gameObject, "Engine Start AudioSource", 5, 10, 1, engineStartClip, false, true, true);
		if(engineStartSound.isPlaying)
			engineStartSound.Play();
		yield return new WaitForSeconds(1f);
		engineRunning = true;
		fuelInput = 1f;
		yield return new WaitForSeconds(1f);

	}

	/// <Summary>
	/// Stops engine
	/// </summary>	
	public void KillEngine (){

		engineRunning = false;
		fuelInput = 0f;

	}

	/// <summary>
	/// Rotates steering wheel based on wheel rotation
	/// </summary>
	void SteeringWheelRotation(){

		if (SteeringWheel) {

			if (orgSteeringWheelRot.eulerAngles == Vector3.zero)
				orgSteeringWheelRot = SteeringWheel.transform.localRotation;
			
			switch (steeringWheelRotateAround) {

			case SteeringWheelRotateAround.XAxis:
				SteeringWheel.transform.localRotation = orgSteeringWheelRot * Quaternion.AngleAxis(((FrontLeftWheelCollider.wheelCollider.steerAngle) * steeringWheelAngleMultiplier), Vector3.right);
				break;

			case SteeringWheelRotateAround.YAxis:
				SteeringWheel.transform.localRotation = orgSteeringWheelRot * Quaternion.AngleAxis(((FrontLeftWheelCollider.wheelCollider.steerAngle) * steeringWheelAngleMultiplier), Vector3.up);
				break;

			case SteeringWheelRotateAround.ZAxis:
				SteeringWheel.transform.localRotation = orgSteeringWheelRot * Quaternion.AngleAxis(((FrontLeftWheelCollider.wheelCollider.steerAngle) * steeringWheelAngleMultiplier), Vector3.forward);
				break;

			}

		}

	}
	
	void Update (){
		
		if(canControl){
				Inputs();
		}else {
			_gasInput = 0f;
			brakeInput = 0f;
			boostInput = 1f;
			handbrakeInput = 1f;
		}
			
		Sounds();
		SteeringWheelRotation ();

		indicatorTimer += Time.deltaTime;		
		_steerInput = steerInput;

		if (_gasInput >= .1f)
			launched += _gasInput * Time.deltaTime;
		else
			launched -= Time.deltaTime;
		
		launched = Mathf.Clamp (launched, 0f, 1f);

		Needles();
		
	}

	/// <summary>
	/// Gets inputs from the player!
	/// </summary>
	void Inputs(){
			
			gasInput = Input.GetAxis(CommonSettings.verticalInput);
			brakeInput = Mathf.Clamp01(-Input.GetAxis(CommonSettings.verticalInput));
			handbrakeInput = Input.GetKey(CommonSettings.handbrakeKB) ? 1f : 0f;
			steerInput = Input.GetAxis(CommonSettings.horizontalInput);
			boostInput = Input.GetKey(CommonSettings.boostKB) ? 2.5f : 1f;

			if(Input.GetKeyDown(CommonSettings.lowBeamHeadlightsKB)){
				lowBeamHeadLightsOn = !lowBeamHeadLightsOn;
			}

			if(Input.GetKeyDown(CommonSettings.highBeamHeadlightsKB)){
				highBeamHeadLightsOn = true;
			}else if(Input.GetKeyUp(CommonSettings.highBeamHeadlightsKB)){
				highBeamHeadLightsOn = false;
			}

			// if(Input.GetKeyDown(CommonSettings.startEngineKB))
			// 	KillOrStartEngine();

			if(Input.GetKeyDown(CommonSettings.rightIndicatorKB)){
				if(indicatorsOn != IndicatorsOn.Right)
					indicatorsOn = IndicatorsOn.Right;
				else
					indicatorsOn = IndicatorsOn.Off;
			}

			if(Input.GetKeyDown(CommonSettings.leftIndicatorKB)){
				if(indicatorsOn != IndicatorsOn.Left)
					indicatorsOn = IndicatorsOn.Left;
				else
					indicatorsOn = IndicatorsOn.Off;
			}

			if(Input.GetKeyDown(CommonSettings.hazardIndicatorKB)){
				if(indicatorsOn != IndicatorsOn.All){
					indicatorsOn = IndicatorsOn.Off;
					indicatorsOn = IndicatorsOn.All;
				}else{
					indicatorsOn = IndicatorsOn.Off;
				}
			}

			if (Input.GetKeyDown (CommonSettings.NGear))
				NGear = true;

			if (Input.GetKeyUp (CommonSettings.NGear))
				NGear = false;

			if(!automaticGear){

				if(currentGear < totalGears - 1 && !changingGear){
					if(Input.GetKeyDown(CommonSettings.shiftGearUp)){
						if(direction != -1)
							StartCoroutine("ChangingGear", currentGear + 1);
						else
							StartCoroutine("ChangingGear", 0);
					}
				}

				if(currentGear >= 0){
					if(Input.GetKeyDown(CommonSettings.shiftGearDown)){
						StartCoroutine("ChangingGear", currentGear - 1);	
					}
				}

			}

	}

	/// <summary>
	/// Rotates needles based on speed and RPM
	/// </summary>
	void Needles(){
		if(speedNeedle){
			var rot = speedNeedle.localEulerAngles;
			rot.z = speedStartAngle + ((speed/maxspeed) * (speedEndAngle - speedStartAngle));
			speedNeedle.localEulerAngles = rot;
		}

		if(rpmNeedle){
			var rot = rpmNeedle.localEulerAngles;
			rot.z = rpmStartAngle + (engineRPM/maxEngineRPM * (rpmEndAngle - rpmStartAngle));
			rpmNeedle.localEulerAngles = rot;			
		}
	}
	
	void FixedUpdate (){

		//TorqueCurve();
		Engine();

		if (canControl) {
			GearBox ();
			Clutch ();
		}

		AntiRollBars();
		DriftVariables();
		RevLimiter();
		Turbo();
		NOS();

		if(steeringHelper)
			SteerHelper();
		
		if(tractionHelper)
			TractionHelper();

		if(ESP)
			ESPCheck(rigid.angularVelocity.y, FrontLeftWheelCollider.wheelCollider.steerAngle);

		if(behaviorType == BehaviorType.SportsMuscle){
			
			if (RearLeftWheelCollider.wheelCollider.isGrounded) {
				//rigid.angularVelocity = new Vector3(rigid.angularVelocity.x, rigid.angularVelocity.y + (direction * steerInput / 30f) + ((((steerInput * _gasInput)) * Mathf.Lerp(0f, 1f, 1f / Mathf.Clamp(speed - 30f, 0f, Mathf.Infinity))) / 30f), rigid.angularVelocity.z);
				rigid.AddRelativeTorque (Vector3.up * (((steerInput * _gasInput) * 1f)), ForceMode.Acceleration); 
			}

//			if(RearLeftWheelCollider.isGrounded)
//				rigid.AddRelativeTorque (Vector3.up * (((steerInput * _gasInput) * 10000f)), ForceMode.Force); 
			 
		}
			
		rigid.centerOfMass = transform.InverseTransformPoint(COM.transform.position);

	}

	/// <summary>
	/// Calculates RPM, speed, steer angle, and determines can we go reverse or not.
	/// </summary>	
	void Engine (){
		
		speed = rigid.velocity.magnitude * 3.6f;

		steerAngle = Mathf.Lerp(orgSteerAngle, highspeedsteerAngle, (speed / highspeedsteerAngleAtspeed));

		if(rigid.velocity.magnitude < .01f && Mathf.Abs(steerInput) < .01f && Mathf.Abs(_gasInput) < .01f && Mathf.Abs(rigid.angularVelocity.magnitude) < .01f)
			sleepingRigid = true;
		else
			sleepingRigid = false;

		float wheelRPM = _wheelTypeChoise == WheelType.FWD ? (FrontLeftWheelCollider.wheelRPMToSpeed + FrontRightWheelCollider.wheelRPMToSpeed) : (RearLeftWheelCollider.wheelRPMToSpeed + RearRightWheelCollider.wheelRPMToSpeed);
		
		rawEngineRPM = Mathf.Clamp(Mathf.MoveTowards(rawEngineRPM, (maxEngineRPM * 1.1f) * 
			(Mathf.Clamp01(Mathf.Lerp(0f, 1f, (1f - clutchInput) * 
				(((wheelRPM * direction) / 2f) / maxSpeedForGear[currentGear])) + 
				(((_gasInput) * clutchInput) + idleInput)))
		                                             , engineInertia * 100f), 0f, maxEngineRPM * 1.1f);
		
		rawEngineRPM *= fuelInput;

		engineRPM = Mathf.Lerp(engineRPM, rawEngineRPM, Mathf.Lerp(Time.fixedDeltaTime * 5f, Time.fixedDeltaTime * 50f, rawEngineRPM / maxEngineRPM));
		
		if(_brakeInput < .1f && speed < 5)
			canGoReverseNow = true;
		else if(_brakeInput > 0 && transform.InverseTransformDirection(rigid.velocity).z > 1f)
			canGoReverseNow = false;
		
		
	}

	/// <summary>
	/// Handles wind and brake sounds
	/// </summary>
	void Sounds(){

		windSound.volume = Mathf.Lerp (0f, CommonSettings.maxWindSoundVolume, speed / 300f);
		windSound.pitch = UnityEngine.Random.Range(.9f, 1f);
		
		if(direction == 1)
			brakeSound.volume = Mathf.Lerp (0f, CommonSettings.maxBrakeSoundVolume, Mathf.Clamp01((FrontLeftWheelCollider.wheelCollider.brakeTorque + FrontRightWheelCollider.wheelCollider.brakeTorque) / (brakeTorque * 2f)) * Mathf.Lerp(0f, 1f, FrontLeftWheelCollider.wheelCollider.rpm / 50f));
		else
			brakeSound.volume = 0f;

	}

	/// <summary>
    /// Checks for ESP, and determines over steering and understeering
    /// </summary>
	/// <param name="velocity">Angular velocity of the vehicle along y axis</param>
	/// <param name="steering">Steering angle of the wheel collider</param>
	void ESPCheck(float velocity, float steering){

		WheelHit frontHit1;
		FrontLeftWheelCollider.wheelCollider.GetGroundHit(out frontHit1);

		WheelHit frontHit2;
		FrontRightWheelCollider.wheelCollider.GetGroundHit(out frontHit2);

		frontSlip = frontHit1.sidewaysSlip + frontHit2.sidewaysSlip;

		WheelHit rearHit1;
		RearLeftWheelCollider.wheelCollider.GetGroundHit(out rearHit1);

		WheelHit rearHit2;
		RearRightWheelCollider.wheelCollider.GetGroundHit(out rearHit2);

		rearSlip = rearHit1.sidewaysSlip + rearHit2.sidewaysSlip;

		if(Mathf.Abs(frontSlip) < ESPThreshold || Math.Abs(rearSlip) < ESPThreshold)
			return;

		if(Mathf.Abs(frontSlip) >= ESPThreshold)
			overSteering = true;
		else
			overSteering = false;

		if(Mathf.Abs(rearSlip) >= ESPThreshold)
			underSteering = true;
		else
			underSteering = false;

		if(underSteering || overSteering)
			ESPAct = true;
		else
			ESPAct = false;			
	}

	/// <summary>
    /// Handles engine sound
    /// </summary>
	/// <param name="input">Torque output of the engine</param>
	public void ApplyEngineSound(float input){

		if(!engineRunning){

			engineSoundOn.pitch = Mathf.Lerp ( engineSoundOn.pitch, 0, Time.fixedDeltaTime * 50f);
			engineSoundOff.pitch = Mathf.Lerp ( engineSoundOff.pitch, 0, Time.fixedDeltaTime * 50f);
			engineSoundIdle.pitch = Mathf.Lerp ( engineSoundOff.pitch, 0, Time.fixedDeltaTime * 50f);

			if(engineSoundOn.pitch <= .1f && engineSoundOff.pitch <= .1f && engineSoundIdle.pitch <= .1f){
				engineSoundOn.Stop();
				engineSoundOff.Stop();
				engineSoundIdle.Stop();
				return;
			}

		}else{
				
			if(!engineSoundOn.isPlaying)
				engineSoundOn.Play();
			if(!engineSoundOff.isPlaying)
				engineSoundOff.Play();
			if(!engineSoundIdle.isPlaying)
				engineSoundIdle.Play();

		}

		if(engineSoundOn){

			engineSoundOn.volume = Mathf.Clamp(_gasInput, minEngineSoundVolume, maxEngineSoundVolume);
			engineSoundOn.pitch = Mathf.Lerp ( engineSoundOn.pitch, Mathf.Lerp (minEngineSoundPitch, maxEngineSoundPitch, engineRPM / 7000f), Time.fixedDeltaTime * 50f);
					
		}
		
		if(engineSoundOff){

			engineSoundOff.volume = Mathf.Clamp((1 - _gasInput) - engineSoundIdle.volume, minEngineSoundVolume, maxEngineSoundVolume);
			engineSoundOff.pitch = Mathf.Lerp ( engineSoundOff.pitch, Mathf.Lerp (minEngineSoundPitch, maxEngineSoundPitch, (engineRPM) / (7000f)), Time.fixedDeltaTime * 50f);

		}

		if(engineSoundIdle){

			engineSoundIdle.volume = Mathf.Lerp(maxEngineSoundVolume, 0f, engineRPM / (maxEngineRPM / 2f));
			engineSoundIdle.pitch = Mathf.Lerp ( engineSoundIdle.pitch, Mathf.Lerp (minEngineSoundPitch, maxEngineSoundPitch, (engineRPM) / (7000f)), Time.fixedDeltaTime * 50f);

		}

	}

	/// <summary>
    /// Applies Anti Roll forces for the wheel colliders
    /// </summary>	
	void AntiRollBars (){

		WheelHit FrontWheelHit;
		
		float travelFL = 1.0f;
		float travelFR = 1.0f;
		
		bool groundedFL= FrontLeftWheelCollider.wheelCollider.GetGroundHit(out FrontWheelHit);
		
		if (groundedFL)
			travelFL = (-FrontLeftWheelCollider.transform.InverseTransformPoint(FrontWheelHit.point).y - FrontLeftWheelCollider.wheelCollider.radius) / FrontLeftWheelCollider.wheelCollider.suspensionDistance;
		
		bool groundedFR= FrontRightWheelCollider.wheelCollider.GetGroundHit(out FrontWheelHit);
		
		if (groundedFR)
			travelFR = (-FrontRightWheelCollider.transform.InverseTransformPoint(FrontWheelHit.point).y - FrontRightWheelCollider.wheelCollider.radius) / FrontRightWheelCollider.wheelCollider.suspensionDistance;
		
		float antiRollForceFrontHorizontal= (travelFL - travelFR) * antiRollFrontHorizontal;
		
		if (groundedFL)
			rigid.AddForceAtPosition(FrontLeftWheelCollider.transform.up * -antiRollForceFrontHorizontal, FrontLeftWheelCollider.transform.position); 
		if (groundedFR)
			rigid.AddForceAtPosition(FrontRightWheelCollider.transform.up * antiRollForceFrontHorizontal, FrontRightWheelCollider.transform.position); 
		
		WheelHit RearWheelHit;

		float travelRL = 1.0f;
		float travelRR = 1.0f;
		
		bool groundedRL= RearLeftWheelCollider.wheelCollider.GetGroundHit(out RearWheelHit);
		
		if (groundedRL)
			travelRL = (-RearLeftWheelCollider.transform.InverseTransformPoint(RearWheelHit.point).y - RearLeftWheelCollider.wheelCollider.radius) / RearLeftWheelCollider.wheelCollider.suspensionDistance;
		
		bool groundedRR= RearRightWheelCollider.wheelCollider.GetGroundHit(out RearWheelHit);
		
		if (groundedRR)
			travelRR = (-RearRightWheelCollider.transform.InverseTransformPoint(RearWheelHit.point).y - RearRightWheelCollider.wheelCollider.radius) / RearRightWheelCollider.wheelCollider.suspensionDistance;
		
		float antiRollForceRearHorizontal= (travelRL - travelRR) * antiRollRearHorizontal;
		
		if (groundedRL)
			rigid.AddForceAtPosition(RearLeftWheelCollider.transform.up * -antiRollForceRearHorizontal, RearLeftWheelCollider.transform.position); 
		if (groundedRR)
			rigid.AddForceAtPosition(RearRightWheelCollider.transform.up * antiRollForceRearHorizontal, RearRightWheelCollider.transform.position);
		
		float antiRollForceFrontVertical= (travelFL - travelRL) * antiRollVertical;

		if (groundedFL)
			rigid.AddForceAtPosition(FrontLeftWheelCollider.transform.up * -antiRollForceFrontVertical, FrontLeftWheelCollider.transform.position); 
		if (groundedRL)
			rigid.AddForceAtPosition(RearLeftWheelCollider.transform.up * antiRollForceFrontVertical, RearLeftWheelCollider.transform.position); 

		float antiRollForceRearVertical= (travelFR - travelRR) * antiRollVertical;

		if (groundedFR)
			rigid.AddForceAtPosition(FrontRightWheelCollider.transform.up * -antiRollForceRearVertical, FrontRightWheelCollider.transform.position); 
		if (groundedRR)
			rigid.AddForceAtPosition(RearRightWheelCollider.transform.up * antiRollForceRearVertical, RearRightWheelCollider.transform.position); 

	}

	/// <summary>
    /// Helps to steer the car better, by adjusting the steering angle towards velocity direction!
    /// </summary>
	void SteerHelper(){

		if (!steeringDirection || !velocityDirection) {

			if (!steeringDirection) {

				GameObject steeringDirectionGO = new GameObject ("Steering Direction");
				steeringDirectionGO.transform.SetParent (transform, false);
				steeringDirection = steeringDirectionGO.transform;
				steeringDirectionGO.transform.localPosition = new Vector3 (1f, 2f, 0f);
				steeringDirectionGO.transform.localScale = new Vector3 (.1f, .1f, 3f);

			}

			if (!velocityDirection) {

				GameObject velocityDirectionGO = new GameObject ("Velocity Direction");
				velocityDirectionGO.transform.SetParent (transform, false);
				velocityDirection = velocityDirectionGO.transform;
				velocityDirectionGO.transform.localPosition = new Vector3 (-1f, 2f, 0f);
				velocityDirectionGO.transform.localScale = new Vector3 (.1f, .1f, 3f);

			}

			return;

		}

		for (int i = 0; i < allWheelColliders.Length; i++){

			WheelHit hit;
			allWheelColliders[i].wheelCollider.GetGroundHit(out hit);
			if (hit.normal == Vector3.zero)
				return;

		}

		Vector3 v = rigid.angularVelocity;
		velocityAngle = (v.y * Mathf.Clamp(transform.InverseTransformDirection(rigid.velocity).z, -1f, 1f)) * Mathf.Rad2Deg;
		velocityDirection.localRotation = Quaternion.Lerp(velocityDirection.localRotation, Quaternion.AngleAxis(Mathf.Clamp(velocityAngle / 3f, -45f, 45f), Vector3.up), Time.fixedDeltaTime * 20f);
		steeringDirection.localRotation = Quaternion.Euler (0f, FrontLeftWheelCollider.wheelCollider.steerAngle, 0f);

		int normalizer = 1;

		if (steeringDirection.localRotation.y > velocityDirection.localRotation.y)
			normalizer = 1;
		else
			normalizer = -1;

		float angle2 = Quaternion.Angle (velocityDirection.localRotation, steeringDirection.localRotation) * (normalizer);

		rigid.AddRelativeTorque (Vector3.up * ((angle2 * (Mathf.Clamp(transform.InverseTransformDirection(rigid.velocity).z, -10f, 10f) / 500f)) * steerHelperAngularVelStrength), ForceMode.VelocityChange);

		if (Mathf.Abs(oldRotation - transform.eulerAngles.y) < 10f){
			
			float turnadjust = (transform.eulerAngles.y - oldRotation) * (steerHelperLinearVelStrength / 2f);
			Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
			rigid.velocity = (velRotation * rigid.velocity);

		}

		oldRotation = transform.eulerAngles.y;

	}

	/// <summary>
    /// Helps to gain stability over different surfaces
    /// </summary>
	void TractionHelper(){

		Vector3 velocity =rigid.velocity;
		velocity -= transform.up * Vector3.Dot(velocity, transform.up);
		velocity.Normalize();

		angle = -Mathf.Asin(Vector3.Dot(Vector3.Cross(transform.forward, velocity), transform.up));

		angularVelo = rigid.angularVelocity.y;

		if (angle * FrontLeftWheelCollider.wheelCollider.steerAngle < 0) {
			FrontLeftWheelCollider.tractionHelpedSidewaysStiffness = (1f - Mathf.Clamp01 (tractionHelperStrength * Mathf.Abs (angularVelo)));
		} else {
			FrontLeftWheelCollider.tractionHelpedSidewaysStiffness = 1f;
		}

		if (angle * FrontRightWheelCollider.wheelCollider.steerAngle < 0) {
			FrontRightWheelCollider.tractionHelpedSidewaysStiffness = (1f - Mathf.Clamp01 (tractionHelperStrength * Mathf.Abs (angularVelo)));
		} else {
			FrontRightWheelCollider.tractionHelpedSidewaysStiffness = 1f;
		}

	}

	/// <summary>
    /// Determines the amount of clutch pressed
    /// </summary>
	void Clutch(){

		if(engineRunning)
			idleInput = Mathf.Lerp(1f, 0f, engineRPM / minEngineRPM);
		else
			idleInput = 0f;

		if (currentGear == 0) {

			if (useClutchMarginAtFirstGear) {
				
				if (launched >= .25f)
					clutchInput = Mathf.Lerp (clutchInput, (Mathf.Lerp (1f, (Mathf.Lerp (clutchInertia, 0f, ((RearLeftWheelCollider.wheelRPMToSpeed + RearRightWheelCollider.wheelRPMToSpeed) / 2f) / targetSpeedForGear [0])), Mathf.Abs (_gasInput))), Time.fixedDeltaTime * 5f);
				else
					clutchInput = Mathf.Lerp (clutchInput, 1f, Time.fixedDeltaTime * 5f);
				
			} else {
				
				clutchInput = Mathf.Lerp (clutchInput, (Mathf.Lerp (1f, (Mathf.Lerp (clutchInertia, 0f, ((RearLeftWheelCollider.wheelRPMToSpeed + RearRightWheelCollider.wheelRPMToSpeed) / 2f) / targetSpeedForGear [0])), Mathf.Abs (_gasInput))), Time.fixedDeltaTime * 5f);

			}
			
		} else {
			
			if (changingGear)
				clutchInput = Mathf.Lerp (clutchInput, 1, Time.fixedDeltaTime * 5f);
			else
				clutchInput = Mathf.Lerp (clutchInput, 0, Time.fixedDeltaTime * 5f);

		} 

		if(cutGas || handbrakeInput >= .1f)
			clutchInput = 1f;

		if (NGear)
			clutchInput = 1f;

		clutchInput = Mathf.Clamp01(clutchInput);

	}


	/// <summary>
	/// Determines the correct gear and changes accordingly
	/// </summary>
	void GearBox (){

		if(brakeInput > .5f  && transform.InverseTransformDirection(rigid.velocity).z < 1f && canGoReverseNow && automaticGear && !semiAutomaticGear && !changingGear && direction != -1)
			StartCoroutine("ChangingGear", -1);
		else if(brakeInput < .1f && transform.InverseTransformDirection(rigid.velocity).z > -1f && direction == -1 && !changingGear && automaticGear && !semiAutomaticGear)
			StartCoroutine("ChangingGear", 0);



		if(automaticGear){

			if(currentGear < totalGears - 1 && !changingGear){
				if(speed >= (targetSpeedForGear[currentGear]) && FrontLeftWheelCollider.wheelCollider.rpm > 0){
					if(!semiAutomaticGear)
						StartCoroutine("ChangingGear", currentGear + 1);
					else if(semiAutomaticGear && direction != -1)
						StartCoroutine("ChangingGear", currentGear + 1);
				}
			}
			
			if(currentGear > 0){

				if(!changingGear){

					if(speed < (targetSpeedForGear[currentGear - 1] * .8f) && direction != -1){
						StartCoroutine("ChangingGear", currentGear - 1);
					}

				}

			}
			
		}

		if(direction == -1){
			
			if(!reversingSound.isPlaying)
				reversingSound.Play();
			reversingSound.volume = Mathf.Lerp(0f, 1f, speed / 60f);
			reversingSound.pitch = reversingSound.volume;

		}else{
			
			if(reversingSound.isPlaying)
				reversingSound.Stop();
			reversingSound.volume = 0f;
			reversingSound.pitch = 0f;

		}
		
	}
	
	internal IEnumerator ChangingGear(int gear){

		changingGear = true;

		if(gearShiftingClips.Length > 0){
			gearShiftingSound = CreateAudioSource.NewAudioSource(gameObject, "Gear Shifting AudioSource", 0f, .5f, CommonSettings.maxGearShiftingSoundVolume, gearShiftingClips[UnityEngine.Random.Range(0, gearShiftingClips.Length)], false, true, true);
			if(!gearShiftingSound.isPlaying)
				gearShiftingSound.Play();
		}
		
		yield return new WaitForSeconds(gearShiftingDelay);

		if(gear == -1){
			currentGear = 0;
			direction = -1;
		}else{
			currentGear = gear;
			direction = 1;
		}

		changingGear = false;

	}

	/// <summary>
	/// LImits the RPM of engine
	/// </summary>
	void RevLimiter(){

		if((useRevLimiter && engineRPM >= maxEngineRPM * 1.05f))
			cutGas = true;
		else if(engineRPM < maxEngineRPM)
			cutGas = false;
		
	}

	/// <summary>
	/// A function to add NOS
	/// </summary>
	void NOS(){

		if(!useNOS)
			return;

		if(!NOSSound)
			NOSSound = CreateAudioSource.NewAudioSource(gameObject, "NOS Sound AudioSource", 5, 10, 1f, NOSClip, true, false, false);

		if(!blowSound)
			blowSound = CreateAudioSource.NewAudioSource(gameObject, "NOS Blow", 1, 10, 1, null, false, false, false);

		if(boostInput > 1.5f && _gasInput >= .8f && NoS > 5){
			NoS -= NoSConsumption * Time.fixedDeltaTime;
			NoSRegenerateTime = 0f;
			if(!NOSSound.isPlaying)
				NOSSound.Play();
		}else{
			if(NoS < 100 && NoSRegenerateTime > 3)
				NoS += (NoSConsumption / 1.5f) * Time.fixedDeltaTime;
			NoSRegenerateTime += Time.fixedDeltaTime;
			if(NOSSound.isPlaying){
				NOSSound.Stop();
				blowSound.clip = CommonSettings.blowoutClip[UnityEngine.Random.Range(0, CommonSettings.blowoutClip.Length)];
				blowSound.Play();
			}
		}

	}


	/// <summary>
	/// A function to add turbe
	/// </summary>
	void Turbo(){

		if(!useTurbo)
			return;

		if (!turboSound) {
			turboSound = CreateAudioSource.NewAudioSource (gameObject, "Turbo Sound AudioSource", .1f, .5f, 0, turboClip, true, true, false);
			CreateAudioSource.NewHighPassFilter (turboSound, 10000f, 10);
		}

		turboBoost = Mathf.Lerp(turboBoost, Mathf.Clamp(Mathf.Pow(_gasInput, 10) * 30f + Mathf.Pow(engineRPM / maxEngineRPM, 10) * 30f, 0f, 30f), Time.fixedDeltaTime * 10f);

		if(turboBoost >= 25f){
			if(turboBoost < (turboSound.volume * 30f)){
				if(!blowSound.isPlaying){
					blowSound.clip = CommonSettings.blowoutClip[UnityEngine.Random.Range(0, CommonSettings.blowoutClip.Length)];
					blowSound.Play();
				}
			}
		}

		turboSound.volume = Mathf.Lerp(turboSound.volume, turboBoost / 30f, Time.fixedDeltaTime * 5f);
		turboSound.pitch = Mathf.Lerp(Mathf.Clamp(turboSound.pitch, 2f, 3f), (turboBoost / 30f) * 2f, Time.fixedDeltaTime * 5f);


	}

	/// <summary>
	/// Determines the drift angle
	/// </summary>
	void DriftVariables(){
		
		WheelHit hit;
		RearRightWheelCollider.wheelCollider.GetGroundHit(out hit);
		
		if(speed > 1f && DriftingNow)
			DriftAngle = hit.sidewaysSlip * .75f;
		else
			DriftAngle = 0f;
		
		if(Mathf.Abs(hit.sidewaysSlip) > .25f)
			DriftingNow = true;
		else
			DriftingNow = false;
		
	}
	
	
	void OnCollisionEnter (Collision collision){
		
		if (collision.contacts.Length < 1 || collision.relativeVelocity.magnitude < 5f)
			return;

			if(crashClips.Length > 0){
				if (collision.contacts[0].thisCollider.gameObject.transform != transform.parent){
					crashSound = CreateAudioSource.NewAudioSource(gameObject, "Crash Sound AudioSource", 5, 20, CommonSettings.maxCrashSoundVolume, crashClips[UnityEngine.Random.Range(0, crashClips.Length)], false, true, true);
				if(!crashSound.isPlaying)
					crashSound.Play();
				}
			}	
	}


	void OnDrawGizmos(){

		#if Unity_Editor
		if(Application.isPlaying){

			WheelHit hit;

			for(int i = 0; i < allWheelColliders.Length; i++){

				allWheelColliders[i].wheelCollider.GetGroundHit(out hit);

				Matrix4x4 temp = Gizmos.matrix;
				Gizmos.matrix = Matrix4x4.TRS(allWheelColliders[i].transform.position, Quaternion.AngleAxis(-90, Vector3.right), Vector3.one);
				Gizmos.color = new Color((hit.force / rigid.mass) / 5f, (-hit.force / rigid.mass) / 5f, 0f);
				Gizmos.DrawFrustum(Vector3.zero, 2f, hit.force / rigid.mass, .1f, 1f);
				Gizmos.matrix = temp;

			}

		}
		#endif
	}

	/// <summary>
    /// Makes Torque curves based on spped, rpm, and torque!
	/// Generates target speed for gears and max speed for gears!
    /// </summary>		
	public void TorqueCurve (){

		if(maxSpeedForGear == null)
			maxSpeedForGear = new float[totalGears];

		if(targetSpeedForGear == null)
			targetSpeedForGear = new float[totalGears - 1];

		if(maxSpeedForGear != null && maxSpeedForGear.Length != totalGears)
			maxSpeedForGear = new float[totalGears];

		if(targetSpeedForGear != null && targetSpeedForGear.Length != totalGears - 1)
			targetSpeedForGear = new float[totalGears - 1];

		for (int j = 0; j < totalGears; j++) 
			maxSpeedForGear [j] = Mathf.Lerp (0f, maxspeed * 1.1f, (float)(j + 1) / (float)(totalGears));

		
				
		for (int k = 0; k < totalGears - 1; k++) 
			targetSpeedForGear [k] = Mathf.Lerp (0, maxspeed * Mathf.Lerp(0f, 1f, gearShiftingThreshold), ((float)(k + 1) / (float)(totalGears)));

		
		if (orgMaxSpeed != maxspeed || orgGearShiftingThreshold != gearShiftingThreshold) {

			if (totalGears < 1) {
				totalGears = 1;
				return;
			}

			engineTorqueCurve = new AnimationCurve[totalGears];

			currentGear = 0;

			for (int i = 0; i < engineTorqueCurve.Length; i++) {
				engineTorqueCurve [i] = new AnimationCurve (new Keyframe (0, 1));
			}

			for (int i = 0; i < totalGears; i++) {

				if (i != 0) {
					engineTorqueCurve [i].MoveKey (0, new Keyframe (0, Mathf.Lerp (1f, .05f, (float)(i + 1) / (float)totalGears)));
					engineTorqueCurve [i].AddKey (Mathf.Lerp (0, maxspeed * .75f, ((float)(i) / (float)(totalGears))), Mathf.Lerp (1f, .5f, ((float)(i) / (float)(totalGears))));
					engineTorqueCurve [i].AddKey (Mathf.Lerp (0, maxspeed * 1.25f, ((float)(i + 1) / (float)(totalGears))), .05f);
					engineTorqueCurve [i].AddKey (Mathf.Lerp (0, maxspeed, ((float)(i + 1) / (float)(totalGears))) * 2f, -3f);
					engineTorqueCurve [i].postWrapMode = WrapMode.Clamp;
				} else {
					engineTorqueCurve [i].MoveKey (0, new Keyframe (0, 2f));
					engineTorqueCurve [i].AddKey (maxSpeedForGear [i] / 5f, 2.5f);
					engineTorqueCurve [i].AddKey (maxSpeedForGear [i], 0f);
					engineTorqueCurve [i].postWrapMode = WrapMode.Clamp;
				}

			orgMaxSpeed = maxspeed;
			orgGearShiftingThreshold = gearShiftingThreshold;

			}

		}

	}
} 
