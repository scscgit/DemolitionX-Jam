#pragma warning disable 0414 // private field assigned but not used.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mirror;
 
/// <summary>
/// Streaming player input, or receiving data from server.
/// </summary>
[RequireComponent(typeof(NetworkIdentity))]
public class VehicleSync : NetworkBehaviour {
	
	// Network Identity of the vehicle. All networked gameobjects must have this component.
	private NetworkIdentity networkID;

	// Main RCC, Rigidbody, and WheelColliders. 
	private VehiclePhysics carController;
	private VehiclePhysicsWheelCollider[] wheelColliders;
	private Rigidbody rigid;

	// Syncing transform, inputs, configurations, and lights of the vehicle. Not using any SyncVar. All synced data is organized via structs and lists.
	#region STRUCTS

	#region Transform

	public struct VehicleTransform{

		public Vector3 position;
		public Quaternion rotation;

		public Vector3 rigidLinearVelocity;
		public Vector3 rigidAngularVelocity;

	}

	public class SyncListVehicleTransform : SyncList<VehicleTransform>{}

	public SyncListVehicleTransform m_Transform = new SyncListVehicleTransform();

	#endregion Transform

	#region Inputs

	public struct VehicleInput{

		public float gasInput;
		public float brakeInput;
		public float steerInput;
		public float handbrakeInput;
		public float boostInput;
		public float clutchInput;
		public float idleInput;
		public int gear;
		public int direction;
		public bool changingGear;
		public float fuelInput;
		public bool engineRunning;
		public bool canGoReverseNow;

	}

	public class SyncListVehicleInput : SyncList<VehicleInput>{}

	public SyncListVehicleInput m_Inputs = new SyncListVehicleInput();

	#endregion Inputs

	#region Configurations

	public struct VehicleConfigurations{

		public int gear;
		public int direction;
		public bool changingGear;
		public bool semiAutomaticGear;
		public float fuelInput;
		public bool engineRunning;
		public float[] cambers;
		public bool applyEngineTorqueToExtraRearWheelColliders;
		public VehiclePhysics.WheelType wheelTypeChoise;
		public float biasedWheelTorque;
		public bool canGoReverseNow;
		public float engineTorque;
		public float brakeTorque;
		public float minEngineRPM;
		public float maxEngineRPM;
		public float engineInertia;
		public bool useRevLimiter;
		public bool useExhaustFlame;
		public bool useClutchMarginAtFirstGear;
		public float highspeedsteerAngle;
		public float highspeedsteerAngleAtspeed;
		public float antiRollFrontHorizontal;
		public float antiRollRearHorizontal;
		public float antiRollVertical;
		public float maxspeed;
		public float engineHeat;
		public float engineHeatMultiplier;
		public int totalGears;
		public float gearShiftingDelay;
		public float gearShiftingThreshold;
		public float clutchInertia;
		public bool NGear;
		public float launched;
		public bool ABS;
		public bool TCS;
		public bool ESP;
		public bool steeringHelper;
		public bool tractionHelper;
		public bool applyCounterSteering;
		public bool useNOS;
		public bool useTurbo;

	}

	public class SyncListVehicleConfigurations : SyncList<VehicleConfigurations>{}

	public SyncListVehicleConfigurations m_Configurations = new SyncListVehicleConfigurations();

	#endregion Configurations


	#endregion

	private float updateTime = 0;

	VehicleInput currentVehicleInputs = new VehicleInput ();
	VehicleTransform currentVehicleTransform = new VehicleTransform ();
	VehicleLights currentVehicleLights = new VehicleLights ();

	bool CB_running = false;

	void Start(){

		// Getting RCC, wheelcolliders, rigidbody, and Network Identity of the vehicle. 
		carController = GetComponent<VehiclePhysics>();
		wheelColliders = GetComponentsInChildren<VehiclePhysicsWheelCollider> ();
		rigid = GetComponent<Rigidbody>();
		networkID = GetComponent<NetworkIdentity> ();

		// Setting name of the gameobject with Network ID.
		gameObject.name = gameObject.name + networkID.netId;

		currentVehicleTransform = new VehicleTransform ();

	}

	void FixedUpdate(){

		if (isLocalPlayer) {

			// If we are owner of this vehicle, stream all inputs from vehicle.
			if(!CB_running)
				StartCoroutine(VehicleIsMein());

		} else {

			// If we are not owner of this vehicle, receive all inputs from server.
			VehicleIsClient ();

		}

	}

	IEnumerator VehicleIsMein (){

		CB_running = true;

		currentVehicleInputs.gasInput = carController.gasInput;
		currentVehicleInputs.brakeInput = carController.brakeInput;
		currentVehicleInputs.steerInput = carController.steerInput;
		currentVehicleInputs.handbrakeInput = carController.handbrakeInput;
		currentVehicleInputs.boostInput = carController.boostInput;
		currentVehicleInputs.clutchInput = carController.clutchInput;
		currentVehicleInputs.idleInput = carController.idleInput;
		currentVehicleInputs.gear = carController.currentGear;
		currentVehicleInputs.direction = carController.direction;
		currentVehicleInputs.changingGear = carController.changingGear;
		currentVehicleInputs.fuelInput = carController.fuelInput;
		currentVehicleInputs.engineRunning = carController.engineRunning;
		currentVehicleInputs.canGoReverseNow = carController.canGoReverseNow;

		CmdVehicleInputs (currentVehicleInputs);

		yield return new WaitForSeconds (.02f);

		currentVehicleTransform.position = transform.position;
		currentVehicleTransform.rotation = transform.rotation;
		currentVehicleTransform.rigidLinearVelocity = rigid.velocity;
		currentVehicleTransform.rigidAngularVelocity = rigid.angularVelocity;

		CmdVehicleTransform (currentVehicleTransform);

		yield return new WaitForSeconds (.02f);

		// DıSABLED
		//		VehicleConfigurations currentVehicleConfigurations = new VehicleConfigurations ();
		//
		//		currentVehicleConfigurations.gear = carController.currentGear;
		//		currentVehicleConfigurations.direction = carController.direction;
		//		currentVehicleConfigurations.changingGear = carController.changingGear;
		//		currentVehicleConfigurations.semiAutomaticGear = carController.semiAutomaticGear;
		//
		//		currentVehicleConfigurations.fuelInput = carController.fuelInput;
		//		currentVehicleConfigurations.engineRunning = carController.engineRunning;
		//		currentVehicleConfigurations.cambers = new float[wheelColliders.Length];
		//
		//		for (int i = 0; i < wheelColliders.Length; i++) {
		//
		//			currentVehicleConfigurations.cambers[i] = wheelColliders[i].camber;
		//
		//		}
		//
		//		currentVehicleConfigurations.applyEngineTorqueToExtraRearWheelColliders = carController.applyEngineTorqueToExtraRearWheelColliders;
		//		currentVehicleConfigurations.wheelTypeChoise = carController._wheelTypeChoise;
		//		currentVehicleConfigurations.biasedWheelTorque = carController.biasedWheelTorque;
		//		currentVehicleConfigurations.canGoReverseNow = carController.canGoReverseNow;
		//		currentVehicleConfigurations.engineTorque = carController.engineTorque;
		//		currentVehicleConfigurations.brakeTorque = carController.brakeTorque;
		//		currentVehicleConfigurations.minEngineRPM = carController.minEngineRPM;
		//		currentVehicleConfigurations.maxEngineRPM = carController.maxEngineRPM;
		//		currentVehicleConfigurations.engineInertia = carController.engineInertia;
		//		currentVehicleConfigurations.useRevLimiter = carController.useRevLimiter;
		//		currentVehicleConfigurations.useExhaustFlame = carController.useExhaustFlame;
		//		currentVehicleConfigurations.useClutchMarginAtFirstGear = carController.useClutchMarginAtFirstGear;
		//		currentVehicleConfigurations.highspeedsteerAngle = carController.highspeedsteerAngle;
		//		currentVehicleConfigurations.highspeedsteerAngleAtspeed = carController.highspeedsteerAngleAtspeed;
		//		currentVehicleConfigurations.antiRollFrontHorizontal = carController.antiRollFrontHorizontal;
		//		currentVehicleConfigurations.antiRollRearHorizontal = carController.antiRollRearHorizontal;
		//		currentVehicleConfigurations.antiRollVertical = carController.antiRollVertical;
		//		currentVehicleConfigurations.maxspeed = carController.maxspeed;
		//		currentVehicleConfigurations.engineHeat = carController.engineHeat;
		//		currentVehicleConfigurations.engineHeatMultiplier = carController.engineHeatRate;
		//		currentVehicleConfigurations.totalGears = carController.totalGears;
		//		currentVehicleConfigurations.gearShiftingDelay = carController.gearShiftingDelay;
		//		currentVehicleConfigurations.gearShiftingThreshold = carController.gearShiftingThreshold;
		//		currentVehicleConfigurations.clutchInertia = carController.clutchInertia;
		//		currentVehicleConfigurations.NGear = carController.NGear;
		//		currentVehicleConfigurations.launched = carController.launched;
		//		currentVehicleConfigurations.ABS = carController.ABS;
		//		currentVehicleConfigurations.TCS = carController.TCS;
		//		currentVehicleConfigurations.ESP = carController.ESP;
		//		currentVehicleConfigurations.steeringHelper = carController.steeringHelper;
		//		currentVehicleConfigurations.tractionHelper = carController.tractionHelper;
		//		currentVehicleConfigurations.applyCounterSteering = carController.applyCounterSteering;
		//		currentVehicleConfigurations.useNOS = carController.useNOS;
		//		currentVehicleConfigurations.useTurbo = carController.useTurbo;
		//
		//		CmdVehicleConfigurations (currentVehicleConfigurations);

		CB_running = false;

	}

	void VehicleIsClient(){

		if (m_Inputs != null && m_Inputs.Count >= 1) {

			carController.gasInput = m_Inputs [0].gasInput;
			carController.brakeInput = m_Inputs [0].brakeInput;
			carController.steerInput = m_Inputs [0].steerInput;
			carController.handbrakeInput = m_Inputs [0].handbrakeInput;
			carController.boostInput = m_Inputs [0].boostInput;
			carController.clutchInput = m_Inputs [0].clutchInput;
			carController.idleInput = m_Inputs [0].idleInput;
			carController.currentGear = m_Inputs [0].gear;
			carController.direction = m_Inputs [0].direction;
			carController.changingGear = m_Inputs [0].changingGear;
			carController.fuelInput = m_Inputs [0].fuelInput;
			carController.engineRunning = m_Inputs [0].engineRunning;
			carController.canGoReverseNow = m_Inputs [0].canGoReverseNow;

		}

		if (m_Transform != null && m_Transform.Count >= 1) {

			Vector3 projectedPosition = m_Transform[0].position + m_Transform[0].rigidLinearVelocity * (Time.time - updateTime);

			if(Vector3.Distance(transform.position, m_Transform[0].position) < 15f){
				
				transform.position = Vector3.Lerp (transform.position, projectedPosition, Time.deltaTime * 5f);
				transform.rotation = Quaternion.Lerp (transform.rotation, m_Transform[0].rotation, Time.deltaTime * 5f);

			}else{
				
				transform.position = m_Transform[0].position;
				transform.rotation = m_Transform[0].rotation;

			}

		}

		updateTime = Time.time;

		// DıSABLED
//		if (m_Configurations != null && m_Configurations.Count >= 1) {
//
//			carController.currentGear = m_Configurations[m_Configurations.Count - 1].gear;
//			carController.direction = m_Configurations[m_Configurations.Count - 1].direction;
//			carController.changingGear = m_Configurations[m_Configurations.Count - 1].changingGear;
//			carController.semiAutomaticGear = m_Configurations[m_Configurations.Count - 1].semiAutomaticGear;
//	
//			carController.fuelInput = m_Configurations[m_Configurations.Count - 1].fuelInput;
//			carController.engineRunning = m_Configurations[m_Configurations.Count - 1].engineRunning;
//	
//			for (int i = 0; i < wheelColliders.Length; i++) {
//	
//				wheelColliders [i].camber = m_Configurations[m_Configurations.Count - 1].cambers [i];
//	
//			}
//	
//			carController.applyEngineTorqueToExtraRearWheelColliders = m_Configurations[m_Configurations.Count - 1].applyEngineTorqueToExtraRearWheelColliders;
//			carController._wheelTypeChoise = m_Configurations[m_Configurations.Count - 1].wheelTypeChoise;
//			carController.biasedWheelTorque = m_Configurations[m_Configurations.Count - 1].biasedWheelTorque;
//			carController.canGoReverseNow = m_Configurations[m_Configurations.Count - 1].canGoReverseNow;
//			carController.engineTorque = m_Configurations[m_Configurations.Count - 1].engineTorque;
//			carController.brakeTorque = m_Configurations[m_Configurations.Count - 1].brakeTorque;
//			carController.minEngineRPM = m_Configurations[m_Configurations.Count - 1].minEngineRPM;
//			carController.maxEngineRPM = m_Configurations[m_Configurations.Count - 1].maxEngineRPM;
//			carController.engineInertia = m_Configurations[m_Configurations.Count - 1].engineInertia;
//			carController.useRevLimiter = m_Configurations[m_Configurations.Count - 1].useRevLimiter;
//			carController.useExhaustFlame = m_Configurations[m_Configurations.Count - 1].useExhaustFlame;
//			carController.useClutchMarginAtFirstGear = m_Configurations[m_Configurations.Count - 1].useClutchMarginAtFirstGear;
//			carController.highspeedsteerAngle = m_Configurations[m_Configurations.Count - 1].highspeedsteerAngle;
//			carController.highspeedsteerAngleAtspeed = m_Configurations[m_Configurations.Count - 1].highspeedsteerAngleAtspeed;
//			carController.antiRollFrontHorizontal = m_Configurations[m_Configurations.Count - 1].antiRollFrontHorizontal;
//			carController.antiRollRearHorizontal = m_Configurations[m_Configurations.Count - 1].antiRollRearHorizontal;
//			carController.antiRollVertical = m_Configurations[m_Configurations.Count - 1].antiRollVertical;
//			carController.maxspeed = m_Configurations[m_Configurations.Count - 1].maxspeed;
//			carController.engineHeat = m_Configurations[m_Configurations.Count - 1].engineHeat;
//			carController.engineHeatRate = m_Configurations[m_Configurations.Count - 1].engineHeatMultiplier;
//			carController.totalGears = m_Configurations[m_Configurations.Count - 1].totalGears;
//			carController.gearShiftingDelay = m_Configurations[m_Configurations.Count - 1].gearShiftingDelay;
//			carController.gearShiftingThreshold = m_Configurations[m_Configurations.Count - 1].gearShiftingThreshold;
//			carController.clutchInertia = m_Configurations[m_Configurations.Count - 1].clutchInertia;
//			carController.NGear = m_Configurations[m_Configurations.Count - 1].NGear;
//			carController.launched = m_Configurations[m_Configurations.Count - 1].launched;
//			carController.ABS = m_Configurations[m_Configurations.Count - 1].ABS;
//			carController.TCS = m_Configurations[m_Configurations.Count - 1].TCS;
//			carController.ESP = m_Configurations[m_Configurations.Count - 1].ESP;
//			carController.steeringHelper = m_Configurations[m_Configurations.Count - 1].steeringHelper;
//			carController.tractionHelper = m_Configurations[m_Configurations.Count - 1].tractionHelper;
//			carController.applyCounterSteering = m_Configurations[m_Configurations.Count - 1].applyCounterSteering;
//			carController.useNOS = m_Configurations[m_Configurations.Count - 1].useNOS;
//			carController.useTurbo = m_Configurations[m_Configurations.Count - 1].useTurbo;
//
//		}

	}

	[Command]
	public void CmdVehicleInputs(VehicleInput _input){

		m_Inputs.Insert (0, _input);
		 
	}

	[Command]
	public void CmdVehicleTransform(VehicleTransform _input){

		m_Transform.Insert(0, _input);

	}

	[Command]
	public void CmdVehicleConfigurations(VehicleConfigurations _input){

		m_Configurations.Insert (0, _input);

	}

}
