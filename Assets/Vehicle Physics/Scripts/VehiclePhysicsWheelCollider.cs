using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(WheelCollider))]

///<summary>
///A class that helps to stabilize the wheel
///Ads skidmarks, changes the values of wheel friction curve for realism!
///</summary>
public class VehiclePhysicsWheelCollider : MonoBehaviour {

	private CommonSettings CommonSettingsInstance;
	private CommonSettings CommonSettings {
		get {
			if (CommonSettingsInstance == null) {
				CommonSettingsInstance = CommonSettings.Instance;
			}
			return CommonSettingsInstance;
		}
	}

	private GroundMaterials RCCGroundMaterialsInstance;
	private GroundMaterials RCCGroundMaterials {
		get {
			if (RCCGroundMaterialsInstance == null) {
				RCCGroundMaterialsInstance = GroundMaterials.Instance;
			}
			return RCCGroundMaterialsInstance;
		}
	}


	private WheelCollider _wheelCollider;
	public WheelCollider wheelCollider{
		get{
			if(_wheelCollider == null)
				_wheelCollider = GetComponent<WheelCollider>();
			return _wheelCollider;
		}set{
			_wheelCollider = value;
		}
	}
	
	private VehiclePhysics carController;
	private Rigidbody rigid;

	private List <VehiclePhysicsWheelCollider> allWheelColliders = new List<VehiclePhysicsWheelCollider>() ;		
	public Transform wheelModel;		

	private float wheelRotation = 0f;	

	///<summary>
	///Camber for the wheel
	///</summary>
	public float camber = 0f;		

	internal float wheelRPMToSpeed = 0f;		

	private GroundMaterials physicsMaterials{get{return RCCGroundMaterials;}}		
	private GroundMaterials.GroundMaterialFrictions[] physicsFrictions{get{return RCCGroundMaterials.frictions;}}

	private SkidmarksManager skidmarks;		
	private float startSlipValue = .25f;		
	private int lastSkidmark = -1;

	private float wheelSlipAmountForward = 0f;		
	private float wheelSlipAmountSideways = 0f;		
	internal float totalSlip = 0f;		

	private float orgForwardStiffness = 1f;		
	private float orgSidewaysStiffness = 1f;		

	public WheelFrictionCurve forwardFrictionCurve;
	public WheelFrictionCurve sidewaysFrictionCurve;

	private AudioSource audioSource;	
	private AudioClip audioClip;		

	internal List<ParticleSystem> allWheelParticles = new List<ParticleSystem>();
	internal ParticleSystem.EmissionModule emission;

	internal float tractionHelpedSidewaysStiffness = 1f;

	private float minForwardStiffness = .75f;
	private float maxForwardStiffness  = 1f;

	private float minSidewaysStiffness = .75f;
	private float maxSidewaysStiffness = 1f;
	
	void Awake (){
		
		carController = GetComponentInParent<VehiclePhysics>();
		rigid = carController.GetComponent<Rigidbody> ();
		
		if (FindObjectOfType (typeof(SkidmarksManager))) 
			skidmarks = FindObjectOfType (typeof(SkidmarksManager)) as SkidmarksManager;
			

		wheelCollider.mass = rigid.mass / 15f;

		forwardFrictionCurve = wheelCollider.forwardFriction;
		sidewaysFrictionCurve = wheelCollider.sidewaysFriction;

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

		orgForwardStiffness = forwardFrictionCurve.stiffness;
		orgSidewaysStiffness = sidewaysFrictionCurve.stiffness;

		wheelCollider.forwardFriction = forwardFrictionCurve;
		wheelCollider.sidewaysFriction = sidewaysFrictionCurve;

		if(CommonSettings.useSharedAudioSources){
			if(!carController.transform.Find("All Audio Sources/Skid Sound AudioSource"))
				audioSource = CreateAudioSource.NewAudioSource(carController.gameObject, "Skid Sound AudioSource", 5, 50, 0, audioClip, true, true, false);
			else
				audioSource = carController.transform.Find("All Audio Sources/Skid Sound AudioSource").GetComponent<AudioSource>();
		}else{
			audioSource = CreateAudioSource.NewAudioSource(carController.gameObject, "Skid Sound AudioSource", 5, 50, 0, audioClip, true, true, false);
			audioSource.transform.position = transform.position;
		}

		
		for (int i = 0; i < RCCGroundMaterials.frictions.Length; i++) {

			GameObject ps = (GameObject)Instantiate (RCCGroundMaterials.frictions [i].groundParticles, transform.position, transform.rotation) as GameObject;
			emission = ps.GetComponent<ParticleSystem> ().emission;
			emission.enabled = false;
			ps.transform.SetParent (transform, false);
			ps.transform.localPosition = Vector3.zero;
			ps.transform.localRotation = Quaternion.identity;
			allWheelParticles.Add (ps.GetComponent<ParticleSystem> ());

		}
			
	}

	void Start(){

		allWheelColliders = carController.allWheelColliders.ToList();
		allWheelColliders.Remove(this);

	}

	private WheelFrictionCurve SetFrictionCurves(WheelFrictionCurve curve, float extremumSlip, float extremumValue, float asymptoteSlip, float asymptoteValue){

		WheelFrictionCurve newCurve = curve;

		newCurve.extremumSlip = extremumSlip;
		newCurve.extremumValue = extremumValue;
		newCurve.asymptoteSlip = asymptoteSlip;
		newCurve.asymptoteValue = asymptoteValue;

		return newCurve;

	}

	void Update(){

		if (!carController.enabled)
			return;

		if(!carController.sleepingRigid){
			
			WheelAlign();
			WheelCamber();

		}
	}
	
	void  FixedUpdate (){

		if (!carController.enabled)
			return;

		wheelRPMToSpeed = (((wheelCollider.rpm * wheelCollider.radius) / 2.9f)) * rigid.transform.lossyScale.y;

		switch(carController._wheelTypeChoise){

		case VehiclePhysics.WheelType.FWD:
			if(this == carController.FrontLeftWheelCollider || this == carController.FrontRightWheelCollider)
				ApplyMotorTorque(carController.engineTorque);
			break;
		case VehiclePhysics.WheelType.RWD:
			if(this == carController.RearLeftWheelCollider || this == carController.RearRightWheelCollider)
				ApplyMotorTorque(carController.engineTorque);
			break;
		case VehiclePhysics.WheelType.AWD:
			ApplyMotorTorque(carController.engineTorque / 2f);
			break;
		case VehiclePhysics.WheelType.BIASED:
			if(this == carController.FrontLeftWheelCollider || this == carController.FrontRightWheelCollider)
				ApplyMotorTorque((carController.engineTorque * (100 - carController.biasedWheelTorque)) / 100f);
			if(this == carController.RearLeftWheelCollider || this == carController.RearRightWheelCollider)
				ApplyMotorTorque((carController.engineTorque * carController.biasedWheelTorque) / 100f);
			break;

		}

		if(carController.ExtraRearWheelsCollider.Length > 0 && carController.applyEngineTorqueToExtraRearWheelColliders){

			for(int i = 0; i < carController.ExtraRearWheelsCollider.Length; i++){

				if(this == carController.ExtraRearWheelsCollider[i])
					ApplyMotorTorque(carController.engineTorque);

			}

		}


		if (this == carController.FrontLeftWheelCollider || this == carController.FrontRightWheelCollider) {
			ApplySteering ();
		}


		// Apply Handbrake if this wheel is one of the rear wheels.
		if(carController.handbrakeInput > .1f){

			if(this == carController.RearLeftWheelCollider || this == carController.RearRightWheelCollider)
				ApplyBrakeTorque((carController.brakeTorque * 1.5f) * carController.handbrakeInput);

		}else{

			// Apply Braking to all wheels.
			if(this == carController.FrontLeftWheelCollider || this == carController.FrontRightWheelCollider)
				ApplyBrakeTorque(carController.brakeTorque * (Mathf.Clamp(carController._brakeInput, 0, 1)));
			else
				ApplyBrakeTorque(carController.brakeTorque * (Mathf.Clamp(carController._brakeInput, 0, 1) / 2f));

		}

		// ESP System. All wheels have individual brakes. In case of loosing control of the car, corresponding wheel will brake for gaining the control again.
		if (carController.ESP) {

			if(carController.underSteering){
				
				if(this == carController.RearLeftWheelCollider)
					ApplyBrakeTorque((carController.brakeTorque * carController.ESPStrength) * Mathf.Clamp(-carController.frontSlip, 0f, Mathf.Infinity));
				
				if(this == carController.RearRightWheelCollider)
					ApplyBrakeTorque((carController.brakeTorque * carController.ESPStrength) * Mathf.Clamp(carController.frontSlip, 0f, Mathf.Infinity));
				
			}

			if(carController.overSteering){

				if(this == carController.FrontLeftWheelCollider)
					ApplyBrakeTorque((carController.brakeTorque * carController.ESPStrength) * Mathf.Clamp(-carController.rearSlip, 0f, Mathf.Infinity));

				if(this == carController.FrontRightWheelCollider)
					ApplyBrakeTorque((carController.brakeTorque * carController.ESPStrength) * Mathf.Clamp(carController.rearSlip, 0f, Mathf.Infinity));

			}

		}

		SkidMarks();
		Frictions();
		Audio();

	}

	public void WheelAlign (){

		if(!wheelModel){
			Debug.LogError(transform.name + " wheel of the " + carController.transform.name + " is missing wheel model. This wheel is disabled");
			enabled = false;
			return;
		}

		RaycastHit hit;
		WheelHit CorrespondingGroundHit;

		Vector3 ColliderCenterPoint = wheelCollider.transform.TransformPoint(wheelCollider.center);
		wheelCollider.GetGroundHit(out CorrespondingGroundHit);

		if(Physics.Raycast(ColliderCenterPoint, -wheelCollider.transform.up, out hit, (wheelCollider.suspensionDistance + wheelCollider.radius) * transform.localScale.y) && !hit.transform.IsChildOf(carController.transform) && !hit.collider.isTrigger){
			wheelModel.transform.position = hit.point + (wheelCollider.transform.up * wheelCollider.radius) * transform.localScale.y;
			float extension = (-wheelCollider.transform.InverseTransformPoint(CorrespondingGroundHit.point).y - wheelCollider.radius) / wheelCollider.suspensionDistance;
			Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point + wheelCollider.transform.up * (CorrespondingGroundHit.force / rigid.mass), extension <= 0.0 ? Color.magenta : Color.white);
			Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point - wheelCollider.transform.forward * CorrespondingGroundHit.forwardSlip * 2f, Color.green);
			Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point - wheelCollider.transform.right * CorrespondingGroundHit.sidewaysSlip * 2f, Color.red);
		}else{
			wheelModel.transform.position = ColliderCenterPoint - (wheelCollider.transform.up * wheelCollider.suspensionDistance) * transform.localScale.y;
		}

		wheelRotation += wheelCollider.rpm * 6 * Time.deltaTime;

		wheelModel.transform.rotation = wheelCollider.transform.rotation * Quaternion.Euler(wheelRotation, wheelCollider.steerAngle, wheelCollider.transform.rotation.z);

	}

	public void WheelCamber (){

		Vector3 wheelLocalEuler;

		if(wheelCollider.transform.localPosition.x < 0)
			wheelLocalEuler = new Vector3(wheelCollider.transform.localEulerAngles.x, wheelCollider.transform.localEulerAngles.y, (-camber));
		else
			wheelLocalEuler = new Vector3(wheelCollider.transform.localEulerAngles.x, wheelCollider.transform.localEulerAngles.y, (camber));

		Quaternion wheelCamber = Quaternion.Euler(wheelLocalEuler);
		wheelCollider.transform.localRotation = wheelCamber;

	}

	void SkidMarks(){

		WheelHit GroundHit;
		wheelCollider.GetGroundHit(out GroundHit);

		wheelSlipAmountSideways = Mathf.Abs(GroundHit.sidewaysSlip);
		wheelSlipAmountForward = Mathf.Abs(GroundHit.forwardSlip);
		totalSlip = wheelSlipAmountSideways + (wheelSlipAmountForward / 2f);

		if(skidmarks){

			if (wheelSlipAmountSideways > startSlipValue || wheelSlipAmountForward > startSlipValue * 2f){

				Vector3 skidPoint = GroundHit.point + 2f * (rigid.velocity) * Time.deltaTime;

				if(rigid.velocity.magnitude > 1f){
					lastSkidmark = skidmarks.AddSkidMark(skidPoint, GroundHit.normal, (wheelSlipAmountSideways / 2f) + (wheelSlipAmountForward / 2f), lastSkidmark);
				}else{
					lastSkidmark = -1;
				}

			}else{
				
				lastSkidmark = -1;

			}

		}

	}

	void Frictions(){

		WheelHit GroundHit;
		wheelCollider.GetGroundHit(out GroundHit);

		bool contacted = false;

		for (int i = 0; i < physicsFrictions.Length; i++) {

			if(GroundHit.point != Vector3.zero && GroundHit.collider.sharedMaterial == physicsFrictions[i].groundMaterial){

				contacted = true;

				forwardFrictionCurve.stiffness = physicsFrictions[i].forwardStiffness;
				sidewaysFrictionCurve.stiffness = (physicsFrictions[i].sidewaysStiffness * tractionHelpedSidewaysStiffness);

				if(carController.behaviorType == VehiclePhysics.BehaviorType.SportsMuscle){
					SportsMuscle();
				}

				wheelCollider.forwardFriction = forwardFrictionCurve;
				wheelCollider.sidewaysFriction = sidewaysFrictionCurve;

				wheelCollider.wheelDampingRate = physicsFrictions[i].damp;

				emission = allWheelParticles[i].emission;
				audioClip = physicsFrictions[i].groundSound;				
					
				if (wheelSlipAmountSideways > physicsFrictions [i].slip || wheelSlipAmountForward > physicsFrictions [i].slip) {
					emission.enabled = true;
				} else {
					emission.enabled = false;
				}

				

			}

		}

		
		if(!contacted && physicsMaterials.useTerrainSplatMapForGroundFrictions){

			for (int k = 0; k < physicsMaterials.terrainSplatMapIndex.Length; k++) {

				if(GroundHit.point != Vector3.zero && GroundHit.collider.sharedMaterial == physicsMaterials.terrainPhysicMaterial){

					if(TerrainSurface.GetTextureMix(transform.position) != null && TerrainSurface.GetTextureMix(transform.position)[k] > .5f){

						contacted = true;

						forwardFrictionCurve.stiffness = physicsFrictions[physicsMaterials.terrainSplatMapIndex[k]].forwardStiffness;
						sidewaysFrictionCurve.stiffness = (physicsFrictions[physicsMaterials.terrainSplatMapIndex[k]].sidewaysStiffness * tractionHelpedSidewaysStiffness);

						if(carController.behaviorType == VehiclePhysics.BehaviorType.SportsMuscle){
							SportsMuscle();
						}

						wheelCollider.forwardFriction = forwardFrictionCurve;
						wheelCollider.sidewaysFriction = sidewaysFrictionCurve;

						wheelCollider.wheelDampingRate = physicsFrictions[physicsMaterials.terrainSplatMapIndex[k]].damp;
						
						emission = allWheelParticles[physicsMaterials.terrainSplatMapIndex[k]].emission;

						audioClip = physicsFrictions[physicsMaterials.terrainSplatMapIndex[k]].groundSound;

						
						if (wheelSlipAmountSideways > physicsFrictions [physicsMaterials.terrainSplatMapIndex [k]].slip || wheelSlipAmountForward > physicsFrictions [physicsMaterials.terrainSplatMapIndex [k]].slip) {
							emission.enabled = true;
						} else {
							emission.enabled = false;
						}
							 
					}

				}
				
			}

		}

		
		if(!contacted){

			forwardFrictionCurve.stiffness = orgForwardStiffness;
			sidewaysFrictionCurve.stiffness = orgSidewaysStiffness * tractionHelpedSidewaysStiffness;

			if(carController.behaviorType == VehiclePhysics.BehaviorType.SportsMuscle){
				SportsMuscle();
			}

			wheelCollider.forwardFriction = forwardFrictionCurve;
			wheelCollider.sidewaysFriction = sidewaysFrictionCurve;

			wheelCollider.wheelDampingRate = physicsFrictions[0].damp;
			
			emission = allWheelParticles[0].emission;

			audioClip = physicsFrictions[0].groundSound;

				
			if (wheelSlipAmountSideways > physicsFrictions [0].slip || wheelSlipAmountForward > physicsFrictions [0].slip) {
				emission.enabled = true;
			} else {
				emission.enabled = false;
			}

		}


		for (int i = 0; i < allWheelParticles.Count; i++) {

			if (wheelSlipAmountSideways > startSlipValue || wheelSlipAmountForward > startSlipValue) {
				
			} else {
				emission = allWheelParticles [i].emission;
				emission.enabled = false;
			}
			
		}
	}

	void SportsMuscle(){
		
		Vector3 relativeVelocity = transform.InverseTransformDirection(rigid.velocity);
		float sqrVel = ((relativeVelocity.x * relativeVelocity.x)) / 100f;

		if(wheelCollider == carController.FrontLeftWheelCollider.wheelCollider || wheelCollider == carController.FrontRightWheelCollider.wheelCollider){
			forwardFrictionCurve.extremumValue = Mathf.Clamp(1f - sqrVel, .1f, maxForwardStiffness);
			forwardFrictionCurve.asymptoteValue = Mathf.Clamp(.75f - (sqrVel / 2f), .1f, minForwardStiffness);
		}else{
			forwardFrictionCurve.extremumValue = Mathf.Clamp(1f - sqrVel, .75f, maxForwardStiffness);
			forwardFrictionCurve.asymptoteValue = Mathf.Clamp(.75f - (sqrVel / 2f), .75f,  minForwardStiffness);
		}

		if(wheelCollider == carController.FrontLeftWheelCollider.wheelCollider || wheelCollider == carController.FrontRightWheelCollider.wheelCollider){
			sidewaysFrictionCurve.extremumValue = Mathf.Clamp(1f - sqrVel / 1f, .5f, maxSidewaysStiffness);
			sidewaysFrictionCurve.asymptoteValue = Mathf.Clamp(.75f - (sqrVel / 2f), .5f, minSidewaysStiffness);
		}else{
			sidewaysFrictionCurve.extremumValue = Mathf.Clamp(1f - sqrVel, .45f, maxSidewaysStiffness);
			sidewaysFrictionCurve.asymptoteValue = Mathf.Clamp(.75f - (sqrVel / 2f), .45f, minSidewaysStiffness);
		}

	}

	void Audio(){

		if(CommonSettings.useSharedAudioSources && isSkidding())
			return;

		if(totalSlip > startSlipValue){

			if(audioSource.clip != audioClip)
				audioSource.clip = audioClip;

			if(!audioSource.isPlaying)
				audioSource.Play();

			if(rigid.velocity.magnitude > 1f){
				audioSource.volume = Mathf.Lerp(audioSource.volume, Mathf.Lerp(0f, 1f, totalSlip - startSlipValue), Time.deltaTime * 5f);
				audioSource.pitch = Mathf.Lerp(1f, .8f, audioSource.volume);
			}else{
				audioSource.volume = Mathf.Lerp(audioSource.volume, 0f, Time.deltaTime * 5f);
			}
			
		}else{
			
			audioSource.volume = Mathf.Lerp(audioSource.volume, 0f, Time.deltaTime * 5f);

			if(audioSource.volume <= .05f && audioSource.isPlaying)
				audioSource.Stop();
			
		}

	}

	bool isSkidding(){

		for (int i = 0; i < allWheelColliders.Count; i++) {

			if(allWheelColliders[i].totalSlip > totalSlip)
				return true;

		}

		return false;

	}


	///<summary>
	///Applies the motor torque for the wheels
	///</summary>
	///<param name="torque">Torque that engine supplies</param>
	void ApplyMotorTorque(float torque){

		if(carController.TCS){

			WheelHit hit;
			wheelCollider.GetGroundHit(out hit);

			if(Mathf.Abs(wheelCollider.rpm) >= 100){
				if(hit.forwardSlip > .25f){
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

		carController.ApplyEngineSound(wheelCollider.motorTorque);

	}

	public void ApplySteering(){

		if(carController.applyCounterSteering && carController.currentGear != 0)
			wheelCollider.steerAngle = Mathf.Clamp((carController.steerAngle * (carController._steerInput + carController.DriftAngle)), -carController.steerAngle, carController.steerAngle);
		else
			wheelCollider.steerAngle = Mathf.Clamp((carController.steerAngle * carController._steerInput), -carController.steerAngle, carController.steerAngle);

	}

	///<summary>
	///Applies the brake torque for the wheels
	///</summary>
	///<param name="brake">Brake torque that is required</param>
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

}