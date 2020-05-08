using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

///<summary>
/// Main Customization Class For Vehicle.
///</summary>
public class Custamization : MonoBehaviour {

	public static void SetCustomizationMode(VehiclePhysics vehicle, bool state){

		if (!vehicle) {
			
			Debug.LogError ("Get a vehicle you fool!!!");
			return;

		}

		if (state) {

			vehicle.canControl = (false);

		} else {

			SetSmokeParticle (vehicle, false);
			SetExhaustFlame (vehicle, false);
			vehicle.canControl = (true);
		}

	}

	/// <summary>
	///	 Updates Vehicle while vehicle is inactive.
	/// </summary>
	public static void OverrideVehicle (VehiclePhysics vehicle) {

		if (!CheckVehicle (vehicle))
			return;

		vehicle.isSleeping = false;

	}

	/// <summary>
	///	 Enable / Disable Smoke Particles. You can use it for previewing current wheel smokes.
	/// </summary>
	public static void SetSmokeParticle (VehiclePhysics vehicle, bool state) {

		if (!CheckVehicle (vehicle))
			return;

		vehicle.PreviewSmokeParticle (state);

	}

	/// <summary>
	/// Set Smoke Color.
	/// </summary>
	public static void SetSmokeColor (VehiclePhysics vehicle, int indexOfGroundMaterial, Color color) {

		if (!CheckVehicle (vehicle))
			return;

		VehiclePhysicsWheelCollider[] wheels = vehicle.GetComponentsInChildren<VehiclePhysicsWheelCollider> ();

		foreach(VehiclePhysicsWheelCollider wheel in wheels){

			for (int i = 0; i < wheel.allWheelParticles.Count; i++) {

				ParticleSystem ps = wheel.allWheelParticles[i];
				ParticleSystem.MainModule psmain = ps.main;
				color.a = psmain.startColor.color.a;
				psmain.startColor = color;

			}

		}

	}

	/// <summary>
	/// Set Headlights Color.
	/// </summary>
	public static void SetHeadlightsColor (VehiclePhysics vehicle, Color color) {

		if (!CheckVehicle (vehicle))
			return;

		VehicleLights[] lights = vehicle.GetComponentsInChildren<VehicleLights> ();
		vehicle.lowBeamHeadLightsOn = true;

		foreach(VehicleLights l in lights){

			if(l.lightType == VehicleLights.LightType.HeadLight)
				l.GetComponent<Light>().color = color;

		}

	}

	/// <summary>
	/// Enable / Disable Exhaust Flame Particles.
	/// </summary>
	public static void SetExhaustFlame (VehiclePhysics vehicle, bool state) {

		if (!CheckVehicle (vehicle))
			return;

		Exhaust[] exhausts = vehicle.GetComponentsInChildren<Exhaust> ();

		foreach (Exhaust exhaust in exhausts)
			exhaust.previewFlames = state;

	}

	/// <summary>
	/// Set Front Wheel Cambers.
	/// </summary>
	public static void SetFrontCambers(VehiclePhysics vehicle, float camberAngle){

		if (!CheckVehicle (vehicle))
			return;

		VehiclePhysicsWheelCollider[] wc = vehicle.GetComponentsInChildren<VehiclePhysicsWheelCollider> ();

		foreach (VehiclePhysicsWheelCollider w in wc) {
			
			if (w == vehicle.FrontLeftWheelCollider || w == vehicle.FrontRightWheelCollider)
				w.camber = camberAngle;
			
		}

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Set Rear Wheel Cambers.
	/// </summary>
	public static void SetRearCambers(VehiclePhysics vehicle, float camberAngle){

		if (!CheckVehicle (vehicle))
			return;

		VehiclePhysicsWheelCollider[] wc = vehicle.GetComponentsInChildren<VehiclePhysicsWheelCollider> ();

		foreach (VehiclePhysicsWheelCollider w in wc) {
			
			if (w != vehicle.FrontLeftWheelCollider && w != vehicle.FrontRightWheelCollider)
				w.camber = camberAngle;
			
		}

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Change Wheel Models.
	/// </summary>
	public static void ChangeWheels(VehiclePhysics vehicle, GameObject wheel){

		if (!CheckVehicle (vehicle))
			return;

		for (int i = 0; i < vehicle.allWheelColliders.Length; i++) {

			if (vehicle.allWheelColliders [i].wheelModel.GetComponent<MeshRenderer> ()) 
				vehicle.allWheelColliders [i].wheelModel.GetComponent<MeshRenderer> ().enabled = false;

			foreach (Transform t in vehicle.allWheelColliders [i].wheelModel.GetComponentInChildren<Transform> ())
				t.gameObject.SetActive (false);

			GameObject newWheel = (GameObject)Instantiate (wheel, vehicle.allWheelColliders[i].wheelModel.position, vehicle.allWheelColliders[i].wheelModel.rotation, vehicle.allWheelColliders[i].wheelModel);

			if (vehicle.allWheelColliders [i].wheelModel.localPosition.x > 0f)
				newWheel.transform.localScale = new Vector3 (newWheel.transform.localScale.x * -1f, newWheel.transform.localScale.y, newWheel.transform.localScale.z);

		}

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Set Front Suspension targetPositions. It changes targetPosition of the front WheelColliders.
	/// </summary>
	public static void SetFrontSuspensionsTargetPos(VehiclePhysics vehicle, float targetPosition){

		if (!CheckVehicle (vehicle))
			return;

		targetPosition = Mathf.Clamp01(targetPosition);

		JointSpring spring1 = vehicle.FrontLeftWheelCollider.wheelCollider.suspensionSpring;
		spring1.targetPosition = 1f - targetPosition;

		vehicle.FrontLeftWheelCollider.wheelCollider.suspensionSpring = spring1;

		JointSpring spring2 = vehicle.FrontRightWheelCollider.wheelCollider.suspensionSpring;
		spring2.targetPosition = 1f - targetPosition;

		vehicle.FrontRightWheelCollider.wheelCollider.suspensionSpring = spring2;

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Set Rear Suspension targetPositions. It changes targetPosition of the rear WheelColliders.
	/// </summary>
	public static void SetRearSuspensionsTargetPos(VehiclePhysics vehicle, float targetPosition){

		if (!CheckVehicle (vehicle))
			return;

		targetPosition = Mathf.Clamp01(targetPosition);

		JointSpring spring1 = vehicle.RearLeftWheelCollider.wheelCollider.suspensionSpring;
		spring1.targetPosition = 1f - targetPosition;

		vehicle.RearLeftWheelCollider.wheelCollider.suspensionSpring = spring1;

		JointSpring spring2 = vehicle.RearRightWheelCollider.wheelCollider.suspensionSpring;
		spring2.targetPosition = 1f - targetPosition;

		vehicle.RearRightWheelCollider.wheelCollider.suspensionSpring = spring2;

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Set All Suspension targetPositions. It changes targetPosition of the all WheelColliders.
	/// </summary>
	public static void SetAllSuspensionsTargetPos(VehiclePhysics vehicle, float targetPosition){

		if (!CheckVehicle (vehicle))
			return;

		targetPosition = Mathf.Clamp01(targetPosition);

		JointSpring spring1 = vehicle.RearLeftWheelCollider.wheelCollider.suspensionSpring;
		spring1.targetPosition = 1f - targetPosition;

		vehicle.RearLeftWheelCollider.wheelCollider.suspensionSpring = spring1;

		JointSpring spring2 = vehicle.RearRightWheelCollider.wheelCollider.suspensionSpring;
		spring2.targetPosition = 1f - targetPosition;

		vehicle.RearRightWheelCollider.wheelCollider.suspensionSpring = spring2;

		JointSpring spring3 = vehicle.FrontLeftWheelCollider.wheelCollider.suspensionSpring;
		spring3.targetPosition = 1f - targetPosition;

		vehicle.FrontLeftWheelCollider.wheelCollider.suspensionSpring = spring3;

		JointSpring spring4 = vehicle.FrontRightWheelCollider.wheelCollider.suspensionSpring;
		spring4.targetPosition = 1f - targetPosition;

		vehicle.FrontRightWheelCollider.wheelCollider.suspensionSpring = spring4;

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Set Front Suspension Distances.
	/// </summary>
	public static void SetFrontSuspensionsDistances(VehiclePhysics vehicle, float distance){

		if (!CheckVehicle (vehicle))
			return;

		if (distance <= 0)
			distance = .05f;

		vehicle.FrontLeftWheelCollider.wheelCollider.suspensionDistance = distance;
		vehicle.FrontRightWheelCollider.wheelCollider.suspensionDistance = distance;

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Set Rear Suspension Distances.
	/// </summary>
	public static void SetRearSuspensionsDistances(VehiclePhysics vehicle, float distance){

		if (!CheckVehicle (vehicle))
			return;

		if (distance <= 0)
			distance = .05f;

		vehicle.RearLeftWheelCollider.wheelCollider.suspensionDistance = distance;
		vehicle.RearRightWheelCollider.wheelCollider.suspensionDistance = distance;

		if (vehicle.ExtraRearWheelsCollider != null && vehicle.ExtraRearWheelsCollider.Length > 0) {
			
			foreach (VehiclePhysicsWheelCollider wc in vehicle.ExtraRearWheelsCollider)
				wc.wheelCollider.suspensionDistance = distance;
			
		}

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Set Drivetrain Mode.
	/// </summary>
	public static void SetDrivetrainMode(VehiclePhysics vehicle, VehiclePhysics.WheelType mode){

		if (!CheckVehicle (vehicle))
			return;

		vehicle._wheelTypeChoise = mode;

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Set Gear Shifting Threshold. Automatic gear will shift up at earlier rpm on lower values. Automatic gear will shift up at later rpm on higher values. 
	/// </summary>
	public static void SetGearShiftingThreshold(VehiclePhysics vehicle, float targetValue){

		if (!CheckVehicle (vehicle))
			return;

		vehicle.gearShiftingThreshold = targetValue;

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Set Clutch Threshold. Automatic gear will shift up at earlier rpm on lower values. Automatic gear will shift up at later rpm on higher values. 
	/// </summary>
	public static void SetClutchThreshold(VehiclePhysics vehicle, float targetValue){

		if (!CheckVehicle (vehicle))
			return;

		vehicle.clutchInertia = targetValue;

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Enable / Disable Counter Steering while vehicle is drifting. Useful for avoid spinning.
	/// </summary>
	public static void SetCounterSteering(VehiclePhysics vehicle, bool state){

		if (!CheckVehicle (vehicle))
			return;

		vehicle.applyCounterSteering = state;

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Enable / Disable NOS.
	/// </summary>
	public static void SetNOS(VehiclePhysics vehicle, bool state){

		if (!CheckVehicle (vehicle))
			return;

		vehicle.useNOS = state;

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Enable / Disable Turbo.
	/// </summary>
	public static void SetTurbo(VehiclePhysics vehicle, bool state){

		if (!CheckVehicle (vehicle))
			return;

		vehicle.useTurbo = state;

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Enable / Disable Exhaust Flames.
	/// </summary>
	public static void SetUseExhaustFlame(VehiclePhysics vehicle, bool state){

		if (!CheckVehicle (vehicle))
			return;

		vehicle.useExhaustFlame = state;

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Enable / Disable Rev Limiter.
	/// </summary>
	public static void SetRevLimiter(VehiclePhysics vehicle, bool state){

		if (!CheckVehicle (vehicle))
			return;

		vehicle.useRevLimiter = state;

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Enable / Disable Clutch Margin.
	/// </summary>
	public static void SetClutchMargin(VehiclePhysics vehicle, bool state){

		if (!CheckVehicle (vehicle))
			return;

		vehicle.useClutchMarginAtFirstGear = state;

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Set Front Suspension Spring Force.
	/// </summary>
	public static void SetFrontSuspensionsSpringForce(VehiclePhysics vehicle, float targetValue){

		if (!CheckVehicle (vehicle))
			return;

		JointSpring spring = vehicle.FrontLeftWheelCollider.GetComponent<WheelCollider> ().suspensionSpring;
		spring.spring = targetValue;
		vehicle.FrontLeftWheelCollider.GetComponent<WheelCollider> ().suspensionSpring = spring;
		vehicle.FrontRightWheelCollider.GetComponent<WheelCollider> ().suspensionSpring = spring;

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Set Rear Suspension Spring Force.
	/// </summary>
	public static void SetRearSuspensionsSpringForce(VehiclePhysics vehicle, float targetValue){

		if (!CheckVehicle (vehicle))
			return;

		JointSpring spring = vehicle.RearLeftWheelCollider.GetComponent<WheelCollider> ().suspensionSpring;
		spring.spring = targetValue;
		vehicle.RearLeftWheelCollider.GetComponent<WheelCollider> ().suspensionSpring = spring;
		vehicle.RearRightWheelCollider.GetComponent<WheelCollider> ().suspensionSpring = spring;

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Set Front Suspension Spring Damper.
	/// </summary>
	public static void SetFrontSuspensionsSpringDamper(VehiclePhysics vehicle, float targetValue){

		if (!CheckVehicle (vehicle))
			return;

		JointSpring spring = vehicle.FrontLeftWheelCollider.GetComponent<WheelCollider> ().suspensionSpring;
		spring.damper = targetValue;
		vehicle.FrontLeftWheelCollider.GetComponent<WheelCollider> ().suspensionSpring = spring;
		vehicle.FrontRightWheelCollider.GetComponent<WheelCollider> ().suspensionSpring = spring;

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Set Rear Suspension Spring Damper.
	/// </summary>
	public static void SetRearSuspensionsSpringDamper(VehiclePhysics vehicle, float targetValue){

		if (!CheckVehicle (vehicle))
			return;

		JointSpring spring = vehicle.RearLeftWheelCollider.GetComponent<WheelCollider> ().suspensionSpring;
		spring.damper = targetValue;
		vehicle.RearLeftWheelCollider.GetComponent<WheelCollider> ().suspensionSpring = spring;
		vehicle.RearRightWheelCollider.GetComponent<WheelCollider> ().suspensionSpring = spring;

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Set Maximum Speed of the vehicle.
	/// </summary>
	public static void SetMaximumSpeed(VehiclePhysics vehicle, float targetValue){

		if (!CheckVehicle (vehicle))
			return;

		vehicle.maxspeed = Mathf.Clamp(targetValue, 10f, 300f);

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Set Maximum Engine Torque of the vehicle.
	/// </summary>
	public static void SetMaximumTorque(VehiclePhysics vehicle, float targetValue){

		if (!CheckVehicle (vehicle))
			return;

		vehicle.engineTorque = Mathf.Clamp(targetValue, 500f, 50000f);

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Set Maximum Brake of the vehicle.
	/// </summary>
	public static void SetMaximumBrake(VehiclePhysics vehicle, float targetValue){

		if (!CheckVehicle (vehicle))
			return;

		vehicle.brakeTorque = Mathf.Clamp(targetValue, 0f, 50000f);

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Enable / Disable ESP.
	/// </summary>
	public static void SetESP(VehiclePhysics vehicle, bool state){
		
		if (!CheckVehicle (vehicle))
			return;

		vehicle.ESP = state;

	}

	/// <summary>
	/// Enable / Disable ABS.
	/// </summary>
	public static void SetABS(VehiclePhysics vehicle, bool state){

		if (!CheckVehicle (vehicle))
			return;

		vehicle.ABS = state;

	}

	/// <summary>
	/// Enable / Disable TCS.
	/// </summary>
	public static void SetTCS(VehiclePhysics vehicle, bool state){

		if (!CheckVehicle (vehicle))
			return;

		vehicle.TCS = state;

	}

	/// <summary>
	/// Enable / Disable Steering Helper.
	/// </summary>
	public static void SetSH(VehiclePhysics vehicle, bool state){

		if (!CheckVehicle (vehicle))
			return;

		vehicle.steeringHelper = state;

	}

	/// <summary>
	/// Set Steering Helper strength.
	/// </summary>
	public static void SetSHStrength(VehiclePhysics vehicle, float value){

		if (!CheckVehicle (vehicle))
			return;

		vehicle.steeringHelper = true;
		vehicle.steerHelperLinearVelStrength = value;
		vehicle.steerHelperAngularVelStrength = value;

	}

	public static void SetCarBodyColor(VehiclePhysics vehicle, Color color){
		
		if(!CheckVehicle(vehicle))
		return;

		vehicle.carProperties.carMaterial.color = color;

	}

	/// <summary>
	/// Save all stats with PlayerPrefs.
	/// </summary>
	public static void SaveStats(VehiclePhysics vehicle){

		if (!CheckVehicle (vehicle))
			return;

		PlayerPrefs.SetFloat(vehicle.transform.name + "_FrontCamber", vehicle.FrontLeftWheelCollider.camber);
		PlayerPrefs.SetFloat(vehicle.transform.name + "_RearCamber", vehicle.RearLeftWheelCollider.camber);

		PlayerPrefs.SetFloat(vehicle.transform.name + "_FrontSuspensionsDistance", vehicle.FrontLeftWheelCollider.wheelCollider.suspensionDistance);
		PlayerPrefs.SetFloat(vehicle.transform.name + "_RearSuspensionsDistance", vehicle.RearLeftWheelCollider.wheelCollider.suspensionDistance);

		PlayerPrefs.SetFloat(vehicle.transform.name + "_FrontSuspensionsSpring", vehicle.FrontLeftWheelCollider.wheelCollider.suspensionSpring.spring);
		PlayerPrefs.SetFloat(vehicle.transform.name + "_RearSuspensionsSpring", vehicle.RearLeftWheelCollider.wheelCollider.suspensionSpring.spring);

		PlayerPrefs.SetFloat(vehicle.transform.name + "_FrontSuspensionsDamper", vehicle.FrontLeftWheelCollider.wheelCollider.suspensionSpring.damper);
		PlayerPrefs.SetFloat(vehicle.transform.name + "_RearSuspensionsDamper", vehicle.RearLeftWheelCollider.wheelCollider.suspensionSpring.damper);

		PlayerPrefs.SetFloat(vehicle.transform.name + "_MaximumSpeed", vehicle.maxspeed);
		PlayerPrefs.SetFloat(vehicle.transform.name + "_MaximumBrake", vehicle.brakeTorque);
		PlayerPrefs.SetFloat(vehicle.transform.name + "_MaximumTorque", vehicle.engineTorque);

		PlayerPrefs.SetString(vehicle.transform.name + "_DrivetrainMode", vehicle._wheelTypeChoise.ToString());

		PlayerPrefs.SetFloat(vehicle.transform.name + "_GearShiftingThreshold", vehicle.gearShiftingThreshold);
		PlayerPrefs.SetFloat(vehicle.transform.name + "_ClutchingThreshold", vehicle.clutchInertia);

		SaveData.SetBool(vehicle.transform.name + "_CounterSteering", vehicle.applyCounterSteering);

		foreach(VehicleLights _light in vehicle.GetComponentsInChildren<VehicleLights>()){
			
			if (_light.lightType == VehicleLights.LightType.HeadLight) {
				
				SaveData.SetColor(vehicle.transform.name + "_HeadlightsColor", _light.GetComponentInChildren<Light>().color);
				break;

			}

		}

		ParticleSystem ps = vehicle.RearLeftWheelCollider.allWheelParticles[0];
		ParticleSystem.MainModule psmain = ps.main;

		SaveData.SetColor(vehicle.transform.name + "_WheelsSmokeColor", psmain.startColor.color);
		SaveData.SetColor(vehicle.transform.name + "_BodyColor", vehicle.carProperties.carMaterial.color);

		SaveData.SetBool(vehicle.transform.name + "_ABS", vehicle.ABS);
		SaveData.SetBool(vehicle.transform.name + "_ESP", vehicle.ESP);
		SaveData.SetBool(vehicle.transform.name + "_TCS", vehicle.TCS);
		SaveData.SetBool(vehicle.transform.name + "_SH", vehicle.steeringHelper);

		SaveData.SetBool(vehicle.transform.name + "NOS", vehicle.useNOS);
		SaveData.SetBool(vehicle.transform.name + "Turbo", vehicle.useTurbo);
		SaveData.SetBool(vehicle.transform.name + "ExhaustFlame", vehicle.useExhaustFlame);
		SaveData.SetBool(vehicle.transform.name + "RevLimiter", vehicle.useRevLimiter);
		SaveData.SetBool(vehicle.transform.name + "ClutchMargin", vehicle.useClutchMarginAtFirstGear);

	}

	/// <summary>
	/// Load all stats with PlayerPrefs.
	/// </summary>
	public static void LoadStats(VehiclePhysics vehicle){

		if (!CheckVehicle (vehicle))
			return;

		SetFrontCambers (vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_FrontCamber", vehicle.FrontLeftWheelCollider.camber));
		SetRearCambers (vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_RearCamber", vehicle.RearLeftWheelCollider.camber));

		SetFrontSuspensionsDistances (vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_FrontSuspensionsDistance", vehicle.FrontLeftWheelCollider.wheelCollider.suspensionDistance));
		SetRearSuspensionsDistances (vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_RearSuspensionsDistance", vehicle.RearLeftWheelCollider.wheelCollider.suspensionDistance));

		SetFrontSuspensionsSpringForce (vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_FrontSuspensionsSpring", vehicle.FrontLeftWheelCollider.wheelCollider.suspensionSpring.spring));
		SetRearSuspensionsSpringForce (vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_RearSuspensionsSpring", vehicle.RearLeftWheelCollider.wheelCollider.suspensionSpring.spring));

		SetFrontSuspensionsSpringDamper (vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_FrontSuspensionsDamper", vehicle.FrontLeftWheelCollider.wheelCollider.suspensionSpring.damper));
		SetRearSuspensionsSpringDamper (vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_RearSuspensionsDamper", vehicle.RearLeftWheelCollider.wheelCollider.suspensionSpring.damper));

		SetMaximumSpeed (vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_MaximumSpeed", vehicle.maxspeed));
		SetMaximumBrake (vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_MaximumBrake", vehicle.brakeTorque));
		SetMaximumTorque (vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_MaximumTorque", vehicle.engineTorque));

		string drvtrn = PlayerPrefs.GetString(vehicle.transform.name + "_DrivetrainMode", vehicle._wheelTypeChoise.ToString());

		switch (drvtrn) {

		case "FWD":
			vehicle._wheelTypeChoise = VehiclePhysics.WheelType.FWD;
			break;

		case "RWD":
			vehicle._wheelTypeChoise = VehiclePhysics.WheelType.RWD;
			break;

		case "AWD":
			vehicle._wheelTypeChoise = VehiclePhysics.WheelType.AWD;
			break;

		}

		SetGearShiftingThreshold (vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_GearShiftingThreshold", vehicle.gearShiftingThreshold));
		SetClutchThreshold(vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_ClutchingThreshold", vehicle.clutchInertia));

		SetCounterSteering (vehicle, SaveData.GetBool(vehicle.transform.name + "_CounterSteering", vehicle.applyCounterSteering));

		SetABS (vehicle, SaveData.GetBool(vehicle.transform.name + "_ABS", vehicle.ABS));
		SetESP (vehicle, SaveData.GetBool(vehicle.transform.name + "_ESP", vehicle.ESP));
		SetTCS (vehicle, SaveData.GetBool(vehicle.transform.name + "_TCS", vehicle.TCS));
		SetSH (vehicle, SaveData.GetBool(vehicle.transform.name + "_SH", vehicle.steeringHelper));

		SetNOS (vehicle, SaveData.GetBool(vehicle.transform.name + "NOS", vehicle.useNOS));
		SetTurbo (vehicle, SaveData.GetBool(vehicle.transform.name + "Turbo", vehicle.useTurbo));
		SetUseExhaustFlame (vehicle, SaveData.GetBool(vehicle.transform.name + "ExhaustFlame", vehicle.useExhaustFlame));
		SetRevLimiter (vehicle, SaveData.GetBool(vehicle.transform.name + "RevLimiter", vehicle.useRevLimiter));
		SetClutchMargin (vehicle, SaveData.GetBool(vehicle.transform.name + "ClutchMargin", vehicle.useClutchMarginAtFirstGear));

		if(PlayerPrefs.HasKey(vehicle.transform.name + "_WheelsSmokeColor"))
			SetSmokeColor (vehicle, 0, SaveData.GetColor(vehicle.transform.name + "_WheelsSmokeColor"));

		if(PlayerPrefs.HasKey(vehicle.transform.name + "_HeadlightsColor"))
			SetHeadlightsColor (vehicle, SaveData.GetColor(vehicle.transform.name + "_HeadlightsColor"));
		
		if(PlayerPrefs.HasKey(vehicle.transform.name + "_BodyColor"))
			SetCarBodyColor (vehicle, SaveData.GetColor(vehicle.transform.name + "_BodyColor"));

		OverrideVehicle (vehicle);

	}

	/// <summary>
	/// Resets all stats and saves default values with PlayerPrefs.
	/// </summary>
	public static void ResetStats(VehiclePhysics vehicle, VehiclePhysics defaultCar){

		if (!CheckVehicle (vehicle))
			return;

		if (!CheckVehicle (defaultCar))
			return;

		SetFrontCambers (vehicle, defaultCar.FrontLeftWheelCollider.camber);
		SetRearCambers (vehicle, defaultCar.RearLeftWheelCollider.camber);

		SetFrontSuspensionsDistances (vehicle, defaultCar.FrontLeftWheelCollider.wheelCollider.suspensionDistance);
		SetRearSuspensionsDistances (vehicle, defaultCar.RearLeftWheelCollider.wheelCollider.suspensionDistance);

		SetFrontSuspensionsSpringForce (vehicle, defaultCar.FrontLeftWheelCollider.wheelCollider.suspensionSpring.spring);
		SetRearSuspensionsSpringForce (vehicle, defaultCar.RearLeftWheelCollider.wheelCollider.suspensionSpring.spring);

		SetFrontSuspensionsSpringDamper (vehicle, defaultCar.FrontLeftWheelCollider.wheelCollider.suspensionSpring.damper);
		SetRearSuspensionsSpringDamper (vehicle, defaultCar.RearLeftWheelCollider.wheelCollider.suspensionSpring.damper);

		SetMaximumSpeed (vehicle, defaultCar.maxspeed);
		SetMaximumBrake (vehicle, defaultCar.brakeTorque);
		SetMaximumTorque (vehicle, defaultCar.engineTorque);

		string drvtrn = defaultCar._wheelTypeChoise.ToString();

		switch (drvtrn) {

		case "FWD":
			vehicle._wheelTypeChoise = VehiclePhysics.WheelType.FWD;
			break;

		case "RWD":
			vehicle._wheelTypeChoise = VehiclePhysics.WheelType.RWD;
			break;

		case "AWD":
			vehicle._wheelTypeChoise = VehiclePhysics.WheelType.AWD;
			break;

		}

		SetGearShiftingThreshold (vehicle, defaultCar.gearShiftingThreshold);
		SetClutchThreshold(vehicle, defaultCar.clutchInertia);

		SetCounterSteering (vehicle, defaultCar.applyCounterSteering);

		SetABS (vehicle, defaultCar.ABS);
		SetESP (vehicle, defaultCar.ESP);
		SetTCS (vehicle, defaultCar.TCS);
		SetSH (vehicle, defaultCar.steeringHelper);

		SetNOS (vehicle, defaultCar.useNOS);
		SetTurbo (vehicle, defaultCar.useTurbo);
		SetUseExhaustFlame (vehicle, defaultCar.useExhaustFlame);
		SetRevLimiter (vehicle, defaultCar.useRevLimiter);
		SetClutchMargin (vehicle, defaultCar.useClutchMarginAtFirstGear);

		SetSmokeColor (vehicle, 0, Color.white);
		SetHeadlightsColor (vehicle, Color.white);

		SaveStats (vehicle);

		OverrideVehicle (vehicle);

	}

	public static bool CheckVehicle(VehiclePhysics vehicle){

		if (!vehicle) {

			Debug.LogError ("Vehicle is missing!");
			return false;

		}

		return true;

	}

}
