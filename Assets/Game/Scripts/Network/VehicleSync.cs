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

	// Main Vehicle, Rigidbody, and WheelColliders. 
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

	#endregion

	private float updateTime = 0;

	VehicleInput currentVehicleInputs = new VehicleInput ();
	VehicleTransform currentVehicleTransform = new VehicleTransform ();
	VehicleLights currentVehicleLights = new VehicleLights ();

	bool CB_running = false;

	void Start(){

		// Getting wheelcolliders, rigidbody, and Network Identity of the vehicle. 
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

			if((m_Transform[0].position - transform.position).sqrMagnitude < 225f){
				
				transform.position = Vector3.Lerp (transform.position, projectedPosition, Time.deltaTime * 5f);
				transform.rotation = Quaternion.Lerp (transform.rotation, m_Transform[0].rotation, Time.deltaTime * 5f);

			}else{
				
				transform.position = m_Transform[0].position;
				transform.rotation = m_Transform[0].rotation;

			}

		}

		updateTime = Time.time;
	}

	[Command]
	public void CmdVehicleInputs(VehicleInput _input){

		m_Inputs.Insert (0, _input);
		 
	}

	[Command]
	public void CmdVehicleTransform(VehicleTransform _input){

		m_Transform.Insert(0, _input);

	}
}
