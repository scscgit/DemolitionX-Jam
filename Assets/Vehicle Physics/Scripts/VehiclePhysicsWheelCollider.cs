using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Based on Unity's WheelCollider. Modifies few curves, settings in order to get stable and realistic physics.
/// </summary>
[RequireComponent (typeof(WheelCollider))]
public class VehiclePhysicsWheelCollider : MonoBehaviour {

	[SerializeField] private bool canDetach = true;
	[Header("Damage")]

    [Tooltip("Point around which the suspension pivots when damaged")]
    public Vector3 damagePivot;
	[System.NonSerialized] public float damage;
	public float detachForce = 5f;
	public bool detached = false;

	private float _lastLocalZ;

	private CommonSettings commonSettingsInstance;
	private CommonSettings commonSettings {
		get {
			if (commonSettingsInstance == null) {
				commonSettingsInstance = CommonSettings.Instance;
			}
			return commonSettingsInstance;
		}
	}


	// Getting an Instance of Ground Materials.
	#region GroundMaterials Instance

	private GroundMaterials groundMaterialsInstance;
	private GroundMaterials groundMaterials {
		get {
			if (groundMaterialsInstance == null) {
				groundMaterialsInstance = GroundMaterials.Instance;
			}
			return groundMaterialsInstance;
		}
	}

	#endregion

	private WheelCollider _wheelCollider;
	public WheelCollider wheelCollider{
		get{
			if(_wheelCollider == null)
				_wheelCollider = GetComponent<WheelCollider>();
			return _wheelCollider;
		}
	}
	
	private VehiclePhysics carController;
	private Rigidbody rigid;

	private List <VehiclePhysicsWheelCollider> allWheelColliders = new List<VehiclePhysicsWheelCollider>() ;		// All other wheelcolliders attached to this vehicle.
	public Transform wheelModel;		// Wheel model.

	private float wheelRotation = 0f;		// Wheel model rotation based on WheelCollider rpm. 
	public float camber = 0f;					// Camber angle.

	internal float wheelRPMToSpeed = 0f;		// Wheel RPM to Speed.

	private GroundMaterials physicsMaterials{get{return groundMaterials;}}		// Instance of Configurable Ground Materials.
	private GroundMaterials.GroundMaterialFrictions[] physicsFrictions{get{return groundMaterials.frictions;}}

	private SkidmarksManager skidmarks;		// Main Skidmarks Manager class.
	private float startSlipValue = .25f;		// Draw skidmarks when forward or sideways slip is bigger than this value.
	private int lastSkidmark = -1;

	private float wheelSlipAmountForward = 0f;		// Forward slip.
	private float wheelSlipAmountSideways = 0f;	// Sideways slip.
	internal float totalSlip = 0f;								// Total amount of forward and sideways slips.

	//WheelFriction Curves and Stiffness.
	public WheelFrictionCurve forwardFrictionCurve;
	public WheelFrictionCurve sidewaysFrictionCurve;

	private AudioSource audioSource;		// Audiosource for tire skid SFX.
	private AudioClip audioClip;					// Audioclip for tire skid SFX.

	// List for all particle systems.
	internal List<ParticleSystem> allWheelParticles = new List<ParticleSystem>();
	internal ParticleSystem.EmissionModule emission;

	internal float tractionHelpedSidewaysStiffness = 1f;

	private float minForwardStiffness = .75f;
	private float maxForwardStiffness  = 1f;

	private float minSidewaysStiffness = .75f;
	private float maxSidewaysStiffness = 1f;

	TerrainData mTerrainData;
	int alphamapWidth;
	int alphamapHeight;

	float[,,] mSplatmapData;
	float mNumTextures;

	public float compression;
	private GameObject detachedWheel;
	
	void Start (){
		
		carController = GetComponentInParent<VehiclePhysics>();
		rigid = carController.GetComponent<Rigidbody> ();

		// Getting all WheelColliders attached to this vehicle (Except this).
		allWheelColliders = carController.GetComponentsInChildren<VehiclePhysicsWheelCollider>().ToList();
//		allWheelColliders.Remove(this);
		GetTerrainData ();
		damagePivot = transform.position;
		

		// Getting friction curves.
		forwardFrictionCurve = wheelCollider.forwardFriction;
		sidewaysFrictionCurve = wheelCollider.sidewaysFriction;

		// Proper settings for selected behavior type.
		switch(carController.behaviorType){

		case VehiclePhysics.BehaviorType.Jeeps:
			forwardFrictionCurve = SetFrictionCurves(forwardFrictionCurve, .2f, 2f, 2f, 2f);
			sidewaysFrictionCurve = SetFrictionCurves(sidewaysFrictionCurve, .25f, 2f, 2f, 2f);
			wheelCollider.forceAppPointDistance = Mathf.Clamp(wheelCollider.forceAppPointDistance, .35f, 1f);
			break;

		case VehiclePhysics.BehaviorType.SportsMuscle:
			forwardFrictionCurve = SetFrictionCurves(forwardFrictionCurve, .25f, 1f, .8f, .5f);
			sidewaysFrictionCurve = SetFrictionCurves(sidewaysFrictionCurve, .4f, 1f, .5f, .75f);
			wheelCollider.forceAppPointDistance = Mathf.Clamp(wheelCollider.forceAppPointDistance, .1f, 1f);
			if(carController._wheelTypeChoise == VehiclePhysics.WheelType.FWD){
				Debug.LogError("Current behavior mode is ''SportsMuscle'', but your vehicle named " + carController.name + " was FWD. You have to use RWD, AWD, or BIASED to rear wheels. Setting it to *RWD* now. ");
				carController._wheelTypeChoise = VehiclePhysics.WheelType.RWD;
			}
			break;

		case VehiclePhysics.BehaviorType.Rovers:
			forwardFrictionCurve = SetFrictionCurves(forwardFrictionCurve, .2f, 2f, 2f, 2f);
			sidewaysFrictionCurve = SetFrictionCurves(sidewaysFrictionCurve, .25f, 2f, 2f, 2f);
			wheelCollider.forceAppPointDistance = Mathf.Clamp(wheelCollider.forceAppPointDistance, .75f, 2f);
			break;

		case VehiclePhysics.BehaviorType.SuperCars:
			forwardFrictionCurve = SetFrictionCurves(forwardFrictionCurve, .2f, 1f, .8f, .75f);
			sidewaysFrictionCurve = SetFrictionCurves(sidewaysFrictionCurve, .3f, 1f, .25f, .75f);
			wheelCollider.forceAppPointDistance = Mathf.Clamp(wheelCollider.forceAppPointDistance, .25f, 1f);
			break;

		case VehiclePhysics.BehaviorType.CivilianCars:
			forwardFrictionCurve = SetFrictionCurves(forwardFrictionCurve, .2f, 1f, .8f, .75f);
			sidewaysFrictionCurve = SetFrictionCurves(sidewaysFrictionCurve, .25f, 1f, .5f, .75f);
			wheelCollider.forceAppPointDistance = Mathf.Clamp(wheelCollider.forceAppPointDistance, .1f, 1f);
			break;

		}

		if (canDetach)
		{
			detachedWheel = new GameObject("Pivot_Detached_Wheel_" + wheelModel.transform.name);
			detachedWheel.transform.parent = rigid.transform;
			//detachedWheel.layer = LayerMask.NameToLayer("Detachable Part");
			detachedWheel.AddComponent<MeshFilter>();
			detachedWheel.GetComponent<MeshFilter>().sharedMesh = wheelModel.GetComponent<MeshFilter>().sharedMesh;
			MeshRenderer detachRend = detachedWheel.AddComponent<MeshRenderer>();
			detachRend.sharedMaterial = wheelModel.GetComponent<MeshRenderer>().sharedMaterial;
			detachedWheel.AddComponent<MeshCollider>();
			detachedWheel.GetComponent<MeshCollider>().convex = true;
			detachedWheel.AddComponent<Rigidbody>();
			//detachedWheel.GetComponent<Rigidbody>().isKinematic = true;
			detachedWheel.GetComponent<Rigidbody>().mass = wheelCollider.mass;

			detachedWheel.SetActive(false);
		}

		// Assigning new frictons if one of the behavior preset selected above.
		wheelCollider.forwardFriction = forwardFrictionCurve;
		wheelCollider.sidewaysFriction = sidewaysFrictionCurve;

		// Creating audiosource for skid SFX.
		if(commonSettings.useSharedAudioSources){
			
			if(!carController.transform.Find("All Audio Sources/Skid Sound AudioSource"))
				audioSource = CreateAudioSource.NewAudioSource(carController.gameObject, "Skid Sound AudioSource", 5f, 50f, 0f, audioClip, true, true, false);
			else
				audioSource = carController.transform.Find("All Audio Sources/Skid Sound AudioSource").GetComponent<AudioSource>();
			
		}else{
			
			audioSource = CreateAudioSource.NewAudioSource(carController.gameObject, "Skid Sound AudioSource", 5f, 50f, 0f, audioClip, true, true, false);
			audioSource.transform.position = transform.position;

		}

		// Creating all ground particles, and adding them to list.
		if (!commonSettings.dontUseAnyParticleEffects) {

			for (int i = 0; i < groundMaterials.frictions.Length; i++) {

				GameObject ps = (GameObject)Instantiate (groundMaterials.frictions [i].groundParticles, transform.position, transform.rotation) as GameObject;
				emission = ps.GetComponent<ParticleSystem> ().emission;
				emission.enabled = false;
				ps.transform.SetParent (transform, false);
				ps.transform.localPosition = Vector3.zero;
				ps.transform.localRotation = Quaternion.identity;
				allWheelParticles.Add (ps.GetComponent<ParticleSystem> ());

			}

		}

		GameObject newPivot = new GameObject ("Pivot_" + wheelModel.transform.name);
        //carController.gameObject.AddComponent<Mirror.NetworkTransformChild>().target = newPivot.transform;
		//carController.gameObject.GetComponent<Mirror.NetworkTransformChild>().clientAuthority = true;
		newPivot.transform.position = GetBounds.GetBoundsCenter (wheelModel.transform);
		newPivot.transform.rotation = transform.rotation;
		newPivot.transform.SetParent (wheelModel.transform.parent, true);
		wheelModel.SetParent (newPivot.transform, true);
		wheelModel = newPivot.transform;
	}

	void Update(){

		if (!carController.enabled)
			return;

		if (carController.isSleeping)
			return;

		// Only runs when vehicle is active. Raycasts are used for WheelAlign().
		if(!detached) WheelAlign ();

	}
	
	void FixedUpdate (){

		if (!carController.enabled)
			return;

		if (carController.isSleeping)
			return;

		wheelRPMToSpeed = (((wheelCollider.rpm * wheelCollider.radius) / 2.9f)) * rigid.transform.lossyScale.y;

		#region Motor Torque, TCS.

		//Applying WheelCollider Motor Torques Depends On Wheel Type Choice.
		switch (carController._wheelTypeChoise) {

		case VehiclePhysics.WheelType.FWD:
			if (this == carController.FrontLeftWheelCollider || this == carController.FrontRightWheelCollider)
				ApplyMotorTorque (carController.engineTorque);
			break;
		case VehiclePhysics.WheelType.RWD:
			if (this == carController.RearLeftWheelCollider || this == carController.RearRightWheelCollider)
				ApplyMotorTorque (carController.engineTorque);
			break;
		case VehiclePhysics.WheelType.AWD:
			ApplyMotorTorque (carController.engineTorque);
			break;
		case VehiclePhysics.WheelType.BIASED:
			if (this == carController.FrontLeftWheelCollider || this == carController.FrontRightWheelCollider)
				ApplyMotorTorque (((carController.engineTorque * (100 - carController.biasedWheelTorque)) / 100f));
			if (this == carController.RearLeftWheelCollider || this == carController.RearRightWheelCollider)
				ApplyMotorTorque (((carController.engineTorque * carController.biasedWheelTorque) / 100f));
			break;

		}

		if (carController.ExtraRearWheelsCollider.Length > 0 && carController.applyEngineTorqueToExtraRearWheelColliders) {

			for (int i = 0; i < carController.ExtraRearWheelsCollider.Length; i++) {

				if (this == carController.ExtraRearWheelsCollider [i])
					ApplyMotorTorque (carController.engineTorque);

			}

		}

		#endregion

		#region Steering.

		// Apply Steering if this wheel is one of the front wheels.
		if (this == carController.FrontLeftWheelCollider || this == carController.FrontRightWheelCollider)
			ApplySteering ();

		#endregion

		#region Braking, ABS.

		// Apply Handbrake if this wheel is one of the rear wheels.
		if (carController.handbrakeInput > .5f) {

			if (this == carController.RearLeftWheelCollider || this == carController.RearRightWheelCollider)
				ApplyBrakeTorque ((carController.brakeTorque * 1f) * carController.handbrakeInput);

		} else {

			// Apply Braking to all wheels.
			if (this == carController.FrontLeftWheelCollider || this == carController.FrontRightWheelCollider)
				ApplyBrakeTorque (carController.brakeTorque * (Mathf.Clamp (carController._brakeInput, 0f, 1f)));
			else
				ApplyBrakeTorque (carController.brakeTorque * (Mathf.Clamp (carController._brakeInput, 0f, 1f) / 2f));

		}

		#endregion

		#region ESP.

		// ESP System. All wheels have individual brakes. In case of loosing control of the vehicle, corresponding wheel will brake for gaining the control again.
		if (carController.ESP) {

			if (carController.handbrakeInput < .5f) {

				if (carController.underSteering) {

					if (this == carController.FrontLeftWheelCollider)
						ApplyBrakeTorque ((carController.brakeTorque * carController.ESPStrength) * Mathf.Clamp (-carController.rearSlip, 0f, Mathf.Infinity));

					if (this == carController.FrontRightWheelCollider)
						ApplyBrakeTorque ((carController.brakeTorque * carController.ESPStrength) * Mathf.Clamp (carController.rearSlip, 0f, Mathf.Infinity));

				}

				if (carController.overSteering) {

					if (this == carController.RearLeftWheelCollider)
						ApplyBrakeTorque ((carController.brakeTorque * carController.ESPStrength) * Mathf.Clamp (-carController.frontSlip, 0f, Mathf.Infinity));

					if (this == carController.RearRightWheelCollider)
						ApplyBrakeTorque ((carController.brakeTorque * carController.ESPStrength) * Mathf.Clamp (carController.frontSlip, 0f, Mathf.Infinity));

				}

			}

		}

		#endregion

		if (!carController.isSleeping){
			
			Frictions ();
			SkidMarks ();
			Audio ();

		}

	}

	// Aligning wheel model position and rotation.
	public void WheelAlign (){

		// Return if no wheel model selected.
		if(!wheelModel){
			
			Debug.LogError(transform.name + " wheel of the " + carController.transform.name + " is missing wheel model. This wheel is disabled");
			enabled = false;
			return;

		}

		WheelHit GroundHit;
		bool grounded = wheelCollider.GetGroundHit(out GroundHit );

		float newCompression = compression;

		if (grounded)
			newCompression = 1f - ((Vector3.Dot(transform.position - GroundHit.point, transform.up) - (wheelCollider.radius * transform.lossyScale.y)) / wheelCollider.suspensionDistance);
		else
			newCompression = wheelCollider.suspensionDistance;

		compression = Mathf.Lerp (compression, newCompression, Time.deltaTime * 50f);

		// Set the position of the wheel model.
		wheelModel.position = transform.position;
		wheelModel.position += (transform.up * (compression - 1.0f) * wheelCollider.suspensionDistance);

		// X axis rotation of the wheel.
		wheelRotation +=  WheelRpm() * 6f * Time.deltaTime;
		wheelModel.rotation = transform.rotation * Quaternion.Euler(wheelRotation, wheelCollider.steerAngle, transform.rotation.z);

		// Z axis rotation of the wheel for camber.
		if(transform.localPosition.x > 0f)
			transform.localRotation = Quaternion.identity * Quaternion.AngleAxis((camber), Vector3.forward);
		else
			transform.localRotation = Quaternion.identity * Quaternion.AngleAxis((-camber), Vector3.forward);

		// Gizmos for wheel forces and slips.
		float extension = (-wheelCollider.transform.InverseTransformPoint(GroundHit.point).y - (wheelCollider.radius * transform.lossyScale.y)) / wheelCollider.suspensionDistance;
		Debug.DrawLine(GroundHit.point, GroundHit.point + transform.up * (GroundHit.force / rigid.mass), extension <= 0.0 ? Color.magenta : Color.white);
		Debug.DrawLine(GroundHit.point, GroundHit.point - transform.forward * GroundHit.forwardSlip * 2f, Color.green);
		Debug.DrawLine(GroundHit.point, GroundHit.point - transform.right * GroundHit.sidewaysSlip * 2f, Color.red);

	}

	private float WheelRpm()
	{
		if (carController.canControl)
		{
			return wheelCollider.rpm;
		}
		// Fake RPM
		//var carVelocity = transform.InverseTransformDirection(rigid.velocity).z; // Mirror doesn't sync velocity :(
		var currentLocalZ = transform.InverseTransformDirection(carController.transform.position).z;
		var carVelocity = (currentLocalZ - _lastLocalZ) / Time.deltaTime;
		_lastLocalZ = currentLocalZ;
		return carVelocity >= 0 ? carVelocity * 500f / 16f : carVelocity > -1 ? carVelocity / 300 : -300;
	}

	// Creating skidmarks.
	void SkidMarks(){

		// First, we are getting groundhit data.
		WheelHit GroundHit;
		wheelCollider.GetGroundHit(out GroundHit);

		// Forward, sideways, and total slips.
		wheelSlipAmountForward = Mathf.Abs(GroundHit.forwardSlip);
		wheelSlipAmountSideways = Mathf.Abs(GroundHit.sidewaysSlip);

		totalSlip = Mathf.Lerp(totalSlip, (wheelSlipAmountSideways + wheelSlipAmountForward), Time.fixedDeltaTime * 3f) / 1f;

		// If scene has skidmarks manager...
		if(skidmarks){

			// If slips are bigger than target value...
			if (wheelSlipAmountSideways + wheelSlipAmountForward > startSlipValue){

				Vector3 skidPoint = GroundHit.point + 2f * (rigid.velocity) * Time.deltaTime;

				if(rigid.velocity.magnitude > 1f)
					lastSkidmark = skidmarks.AddSkidMark(skidPoint, GroundHit.normal, totalSlip, lastSkidmark);
				else
					lastSkidmark = -1;

			}else{
				
				lastSkidmark = -1;

			}

		}

	}

	// Setting ground frictions to wheel frictions.
	void Frictions(){

		int groundIndex = GetGroundMaterialIndex ();

		WheelHit hit;
		wheelCollider.GetGroundHit (out hit);

		float hbInput = carController.handbrakeInput;

		if ((this == carController.RearLeftWheelCollider || this == carController.RearRightWheelCollider) && hbInput > .75f)
			hbInput = .75f;
		else
			hbInput = 1;

		// Setting wheel stiffness to ground physic material stiffness.
		forwardFrictionCurve.stiffness = physicsFrictions[groundIndex].forwardStiffness;
		sidewaysFrictionCurve.stiffness = (physicsFrictions[groundIndex].sidewaysStiffness * hbInput * tractionHelpedSidewaysStiffness);

		// If drift mode is selected, apply specific frictions.
		if(carController.behaviorType == VehiclePhysics.BehaviorType.SportsMuscle)
			Drift();

		// Setting new friction curves to wheels.
		wheelCollider.forwardFriction = forwardFrictionCurve;
		wheelCollider.sidewaysFriction = sidewaysFrictionCurve;

		// Also damp too.
		wheelCollider.wheelDampingRate = physicsFrictions[groundIndex].damp;

		// Set audioclip to ground physic material sound.
		audioClip = physicsFrictions[groundIndex].groundSound;

		// If wheel slip is bigger than ground physic material slip, enable particles. Otherwise, disable particles.
		if (!commonSettings.dontUseAnyParticleEffects) {

			for (int i = 0; i < allWheelParticles.Count; i++) {

				if (totalSlip > physicsFrictions [groundIndex].slip) {

					if (i != groundIndex) {

						ParticleSystem.EmissionModule em;

						em = allWheelParticles [i].emission;
						em.enabled = false;

					} else {

						ParticleSystem.EmissionModule em;

						em = allWheelParticles [i].emission;
						em.enabled = true;

					}

				} else {

					ParticleSystem.EmissionModule em;

					em = allWheelParticles [i].emission;
					em.enabled = false;

				}

			}

		}

	}

	void Drift(){
		
		WheelHit hit;
		wheelCollider.GetGroundHit (out hit);

		Vector3 relativeVelocity = transform.InverseTransformDirection(rigid.velocity);

		float sqrVel = (relativeVelocity.x * relativeVelocity.x) / 10f;
		sqrVel += (Mathf.Abs(hit.forwardSlip * hit.forwardSlip) * .5f);

		// Forward
		if(wheelCollider == carController.FrontLeftWheelCollider.wheelCollider || wheelCollider == carController.FrontRightWheelCollider.wheelCollider){
			forwardFrictionCurve.extremumValue = Mathf.Clamp(1f - sqrVel, .1f, maxForwardStiffness);
			forwardFrictionCurve.asymptoteValue = Mathf.Clamp(.75f - (sqrVel / 2f), .1f, minForwardStiffness);
		}else{
			forwardFrictionCurve.extremumValue = Mathf.Clamp(1f - sqrVel, 1f, maxForwardStiffness);
			forwardFrictionCurve.asymptoteValue = Mathf.Clamp(.75f - (sqrVel / 2f), 1.2f, minForwardStiffness);
		}

		// Sideways
		if(wheelCollider == carController.FrontLeftWheelCollider.wheelCollider || wheelCollider == carController.FrontRightWheelCollider.wheelCollider){
			sidewaysFrictionCurve.extremumValue = Mathf.Clamp(1f - sqrVel / 1f, .5f, maxSidewaysStiffness);
			sidewaysFrictionCurve.asymptoteValue = Mathf.Clamp(.75f - (sqrVel / 2f), .5f, minSidewaysStiffness);
		}else{
			sidewaysFrictionCurve.extremumValue = Mathf.Clamp(1f - sqrVel, .5f, maxSidewaysStiffness);
			sidewaysFrictionCurve.asymptoteValue = Mathf.Clamp(.75f - (sqrVel / 2f), .5f, minSidewaysStiffness);
		}

	}

	void Audio(){

		if(commonSettings.useSharedAudioSources && isSkidding())
			return;
		
		if(totalSlip > startSlipValue){

			if(audioSource.clip != audioClip)
				audioSource.clip = audioClip;

			if(!audioSource.isPlaying)
				audioSource.Play();

			if(rigid.velocity.magnitude > 1f){
				
				audioSource.volume = Mathf.Lerp(0f, 1f, totalSlip - 0);
				audioSource.pitch = Mathf.Lerp(1f, .8f, audioSource.volume);

			}else{
				
				audioSource.volume = 0f;

			}
			
		}else{
			
			audioSource.volume = 0f;

			if(audioSource.volume <= .05f && audioSource.isPlaying)
				audioSource.Stop();
			
		}

	}

	bool isSkidding(){

		for (int i = 0; i < allWheelColliders.Count; i++) {

			if(allWheelColliders[i].totalSlip > startSlipValue)
				return true;

		}

		return false;

	}

	void ApplyMotorTorque(float torque){

		if(carController.TCS){

			WheelHit hit;
			wheelCollider.GetGroundHit(out hit);

			if(Mathf.Abs(wheelCollider.rpm) >= 100){
				
				if(hit.forwardSlip > startSlipValue){
					
					carController.TCSAct = true;
					torque -= Mathf.Clamp(torque * (hit.forwardSlip) * carController.TCSStrength, 0f, carController.engineTorque);

				}else{
					
					carController.TCSAct = false;
					torque += Mathf.Clamp(torque * (hit.forwardSlip) * carController.TCSStrength, -carController.engineTorque, 0f);

				}

			}else{
				
				carController.TCSAct = false;

			}

		}

		if(OverTorque())
			torque = 0;

		wheelCollider.motorTorque = ((torque * (1 - carController.clutchInput) * carController._boostInput) * carController._gasInput) * (carController.engineTorqueCurve[carController.currentGear].Evaluate(wheelRPMToSpeed * carController.direction) * carController.direction);

	}

	public void ApplySteering(){

		wheelCollider.steerAngle = Mathf.Clamp(carController.steerAngle * carController._steerInput, -carController.steerAngle, carController.steerAngle);

	}

	void ApplyBrakeTorque(float brake){

		if(carController.ABS && carController.handbrakeInput <= .1f){

			WheelHit hit;
			wheelCollider.GetGroundHit(out hit);

			if((Mathf.Abs(hit.forwardSlip) * Mathf.Clamp01(brake)) >= carController.ABSThreshold){
				carController.ABSAct = true;
				brake = 0;
			}else{
				carController.ABSAct = false;
			}

		}

		wheelCollider.brakeTorque = brake;

	}

	bool OverTorque(){

		if(carController.speed > carController.maxspeed || !carController.engineRunning)
			return true;

		return false;

	}

	void GetTerrainData(){

		if (!Terrain.activeTerrain)
			return;
		
		mTerrainData = Terrain.activeTerrain.terrainData;
		alphamapWidth = mTerrainData.alphamapWidth;
		alphamapHeight = mTerrainData.alphamapHeight;

		mSplatmapData = mTerrainData.GetAlphamaps(0, 0, alphamapWidth, alphamapHeight);
		mNumTextures = mSplatmapData.Length / (alphamapWidth * alphamapHeight);

	}

	private Vector3 ConvertToSplatMapCoordinate(Vector3 playerPos){
		
		Vector3 vecRet = new Vector3();
		Terrain ter = Terrain.activeTerrain;
		Vector3 terPosition = ter.transform.position;
		vecRet.x = ((playerPos.x - terPosition.x) / ter.terrainData.size.x) * ter.terrainData.alphamapWidth;
		vecRet.z = ((playerPos.z - terPosition.z) / ter.terrainData.size.z) * ter.terrainData.alphamapHeight;
		return vecRet;

	}

	int GetGroundMaterialIndex(){

		// Contacted any physic material in Configurable Ground Materials yet?
		bool contacted = false;

		// First, we are getting groundhit data.
		WheelHit GroundHit;
		wheelCollider.GetGroundHit(out GroundHit);

		if (GroundHit.point == Vector3.zero)
			return 0;

		int ret = 0;
		
		for (int i = 0; i < physicsFrictions.Length; i++) {

			if (GroundHit.collider.sharedMaterial == physicsFrictions [i].groundMaterial) {
				
				contacted = true;
				ret = i;

			}

		}

		// If ground pyhsic material is not one of the ground material in Configurable Ground Materials, check if we are on terrain collider...
		if(!contacted && physicsMaterials.useTerrainSplatMapForGroundFrictions && GroundHit.collider.sharedMaterial == groundMaterials.terrainPhysicMaterial){

			Vector3 playerPos = transform.position;
			Vector3 TerrainCord = ConvertToSplatMapCoordinate(playerPos);
			float comp = 0f;

			for (int i = 0; i < mNumTextures; i++){
				
				if (comp < mSplatmapData[(int)TerrainCord.z, (int)TerrainCord.x, i])
					ret = i;
				
			}

		}

		return ret;

	}

	// Setting a new friction to WheelCollider.
	public WheelFrictionCurve SetFrictionCurves(WheelFrictionCurve curve, float extremumSlip, float extremumValue, float asymptoteSlip, float asymptoteValue){

		WheelFrictionCurve newCurve = curve;

		newCurve.extremumSlip = extremumSlip;
		newCurve.extremumValue = extremumValue;
		newCurve.asymptoteSlip = asymptoteSlip;
		newCurve.asymptoteValue = asymptoteValue;

		return newCurve;

	}


	public void Detach()
	{
		
		detached = true;
		
		detachedWheel.transform.parent = null;
		detachedWheel.SetActive(true);
		detachedWheel.transform.position = wheelModel.position;
		detachedWheel.transform.rotation = wheelModel.rotation;
		//detachedCol.sharedMaterial = popped ? detachedRimMaterial : detachedTireMaterial;


		rigid.mass -= detachedWheel.GetComponent<Rigidbody>().mass;
		//detachedWheel.GetComponent<Rigidbody>().isKinematic = false;
		detachedWheel.GetComponent<Rigidbody>().velocity = rigid.GetPointVelocity(transform.position);
		detachedWheel.GetComponent<Rigidbody>().angularVelocity = rigid.angularVelocity;

		wheelModel.gameObject.SetActive(false);
		gameObject.SetActive(false);

		/*if (sphereColTr)
		{
			sphereColTr.gameObject.SetActive(false);		}*/
		

	}

}
