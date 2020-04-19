using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(VehiclePhysics)), CanEditMultipleObjects]
public class VehiclePhysicsEditor : Editor {
	VehiclePhysics carScript;	
	bool openAll;
	bool setup;
	
	//bool CarSettings;	
	bool SuspensionSettings;
	bool FrontSuspension;
	bool RearSuspension;	
	bool LightSettings;


	static void CreateBehavior(){

		if(!Selection.activeGameObject.GetComponentInParent<VehiclePhysics>()){

			GameObject pivot = new GameObject (Selection.activeGameObject.name);
			pivot.transform.position = Selection.activeGameObject.transform.position;
			pivot.transform.rotation = Selection.activeGameObject.transform.rotation;

			pivot.AddComponent<VehiclePhysics>();

			pivot.GetComponent<Rigidbody>().mass = 1350f;
			pivot.GetComponent<Rigidbody>().drag = .05f;
			pivot.GetComponent<Rigidbody>().angularDrag = .5f;
			pivot.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;

			Selection.activeGameObject.transform.SetParent (pivot.transform);
			Selection.activeGameObject = pivot;
		}

	}
	
	
	public override void OnInspectorGUI () {

		//serializedObject.Update();
		
		carScript = (VehiclePhysics)target;

		if(GUILayout.Button("Setup car") && !setup) {
			CreateBehavior();
			setup = true;
		}
		
		if(GUILayout.Button("Make hierarchy")){
				
				Transform[] objects = carScript.gameObject.GetComponentsInChildren<Transform>();
				bool didWeHaveThisObject = false;
				
				foreach(Transform g in objects){
					if (g.name == "Chassis") {
						didWeHaveThisObject = true;
					}
				}
				
				if(!didWeHaveThisObject){
					
					GameObject chassis = new GameObject("Chassis");
					chassis.transform.parent = carScript.transform;
					chassis.transform.localPosition = Vector3.zero;
					chassis.transform.localScale = Vector3.one;
					chassis.transform.rotation = carScript.transform.rotation;
					carScript.chassis = chassis;
					GameObject wheelModels = new GameObject("Wheel Models");
					wheelModels.transform.parent = chassis.transform;
					wheelModels.transform.localPosition = Vector3.zero;
					wheelModels.transform.localScale = Vector3.one;
					wheelModels.transform.rotation = carScript.transform.rotation;
					GameObject COM = new GameObject("COM");
					COM.transform.parent = carScript.transform;
					COM.transform.localPosition = Vector3.zero;
					COM.transform.localScale = Vector3.one;
					COM.transform.rotation = carScript.transform.rotation;
					carScript.COM = COM.transform;
					
				}
				
			}

			if(GUILayout.Button("Create Wheel Colliders")){
				
				WheelCollider[] wheelColliders = carScript.gameObject.GetComponentsInChildren<WheelCollider>();
				
				if(wheelColliders.Length < 1)
					carScript.CreateWheelColliders();				
				
			}

			if(GUILayout.Button("Make Troque Curves")){			
				carScript.TorqueCurve();
		}

		EditorGUILayout.HelpBox("The wheel colliders will get auto assigned, when you make them!!", MessageType.None);

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
		
		EditorGUILayout.BeginHorizontal();
		
		if(GUILayout.Button("Suspension Settings"))
			SuspensionSettings = EnableCategory();

		if(GUILayout.Button("Lights and Exhaust"))
			LightSettings = EnableCategory();
		
		EditorGUILayout.EndHorizontal();

		if(SuspensionSettings){

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			JointSpring frontSspring = carScript.FrontLeftWheelCollider.wheelCollider.suspensionSpring;
			JointSpring rearSpring = carScript.RearLeftWheelCollider.wheelCollider.suspensionSpring;

			GUILayout.BeginHorizontal();

			if(GUILayout.Button("Front Suspensions")){
				FrontSuspension = true;
				RearSuspension = false;
			}

			if(GUILayout.Button("Rear Suspensions")){
				FrontSuspension = false;
				RearSuspension = true;
			}

			GUILayout.EndHorizontal();

			if(FrontSuspension){
				EditorGUILayout.Space();
				carScript.FrontLeftWheelCollider.wheelCollider.suspensionDistance = carScript.FrontRightWheelCollider.wheelCollider.suspensionDistance = EditorGUILayout.FloatField("Front Suspensions Distance", carScript.FrontLeftWheelCollider.wheelCollider.suspensionDistance);
				carScript.FrontLeftWheelCollider.wheelCollider.forceAppPointDistance = carScript.FrontRightWheelCollider.wheelCollider.forceAppPointDistance = EditorGUILayout.FloatField("Front Force App Distance", carScript.FrontLeftWheelCollider.wheelCollider.forceAppPointDistance);
				if(carScript.FrontLeftWheelCollider && carScript.FrontRightWheelCollider)
					carScript.FrontLeftWheelCollider.camber = carScript.FrontRightWheelCollider.camber = EditorGUILayout.FloatField("Front Camber Angle", carScript.FrontLeftWheelCollider.camber);
				EditorGUILayout.Space();
				frontSspring.spring = EditorGUILayout.FloatField("Front Suspensions Spring", frontSspring.spring);
				frontSspring.damper = EditorGUILayout.FloatField("Front Suspensions Damping", frontSspring.damper);
				frontSspring.targetPosition = EditorGUILayout.FloatField("Front Suspensions Target Position", frontSspring.targetPosition);
				EditorGUILayout.Space();
			}

			if(RearSuspension){
				EditorGUILayout.Space();
				carScript.RearLeftWheelCollider.wheelCollider.suspensionDistance = carScript.RearRightWheelCollider.wheelCollider.suspensionDistance = EditorGUILayout.FloatField("Rear Suspensions Distance", carScript.RearLeftWheelCollider.wheelCollider.suspensionDistance);
				carScript.RearLeftWheelCollider.wheelCollider.forceAppPointDistance = carScript.RearRightWheelCollider.wheelCollider.forceAppPointDistance = EditorGUILayout.FloatField("Rear Force App Distance", carScript.RearLeftWheelCollider.wheelCollider.forceAppPointDistance);

				if(carScript.RearLeftWheelCollider && carScript.RearRightWheelCollider){
					
					carScript.RearLeftWheelCollider.camber = carScript.RearRightWheelCollider.camber = EditorGUILayout.FloatField("Rear Camber Angle", carScript.RearLeftWheelCollider.camber);

					if (carScript.ExtraRearWheelsCollider != null && carScript.ExtraRearWheelsCollider.Length > 0) {
						foreach (VehiclePhysicsWheelCollider wc in carScript.ExtraRearWheelsCollider)
							wc.camber = carScript.RearLeftWheelCollider.camber;
					}

				}

				EditorGUILayout.Space();
				rearSpring.spring = EditorGUILayout.FloatField("Rear Suspensions Spring", rearSpring.spring);
				rearSpring.damper = EditorGUILayout.FloatField("Rear Suspensions Damping", rearSpring.damper);
				rearSpring.targetPosition = EditorGUILayout.FloatField("Rear Suspensions Target Position", rearSpring.targetPosition);
				EditorGUILayout.Space();
			}

			carScript.FrontLeftWheelCollider.wheelCollider.suspensionSpring = frontSspring;
			carScript.FrontRightWheelCollider.wheelCollider.suspensionSpring = frontSspring;
			carScript.RearLeftWheelCollider.wheelCollider.suspensionSpring = rearSpring;
			carScript.RearRightWheelCollider.wheelCollider.suspensionSpring = rearSpring;

			EditorGUILayout.Space();
			
		}
		
		else if(LightSettings){
			
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("lowBeamHeadLightsOn"), new GUIContent("Head Lights On"));
			EditorGUILayout.Space();

			VehicleLights[] lights = carScript.GetComponentsInChildren<VehicleLights>();

			EditorGUILayout.LabelField("Head Lights", EditorStyles.boldLabel);
			EditorGUILayout.Space();
			EditorGUI.indentLevel ++;

			for (int i = 0; i < lights.Length; i++) {

				EditorGUILayout.BeginHorizontal();
				if(lights[i].lightType == VehicleLights.LightType.HeadLight){
					EditorGUILayout.ObjectField("Head Light", lights[i].GetComponent<Light>(), typeof(Light), true);
					if(GUILayout.Button("X", GUILayout.Width(25f)))
						DestroyImmediate(lights[i].gameObject);
				}
				EditorGUILayout.EndHorizontal();

			}

			EditorGUILayout.Space();
			EditorGUI.indentLevel --;
			EditorGUILayout.LabelField("Brake Lights", EditorStyles.boldLabel);
			EditorGUILayout.Space();
			EditorGUI.indentLevel ++;

			for (int i = 0; i < lights.Length; i++) {

				EditorGUILayout.BeginHorizontal();
				if(lights[i].lightType == VehicleLights.LightType.BrakeLight){
					EditorGUILayout.ObjectField("Brake Light", lights[i].GetComponent<Light>(), typeof(Light), true);
					if(GUILayout.Button("X", GUILayout.Width(25f)))
						DestroyImmediate(lights[i].gameObject);
				}
				EditorGUILayout.EndHorizontal();

			}

			EditorGUILayout.Space();
			EditorGUI.indentLevel --;
			EditorGUILayout.LabelField("Reverse Lights", EditorStyles.boldLabel);
			EditorGUILayout.Space();
			EditorGUI.indentLevel ++;

			for (int i = 0; i < lights.Length; i++) {

				EditorGUILayout.BeginHorizontal();
				if(lights[i].lightType == VehicleLights.LightType.ReverseLight){
					EditorGUILayout.ObjectField("Reverse Light", lights[i].GetComponent<Light>(), typeof(Light), true);
					if(GUILayout.Button("X", GUILayout.Width(25f)))
						DestroyImmediate(lights[i].gameObject);
				}
				EditorGUILayout.EndHorizontal();

			}

			EditorGUILayout.Space();
			EditorGUI.indentLevel --;
			EditorGUILayout.LabelField("Indicator Lights", EditorStyles.boldLabel);
			EditorGUILayout.Space();
			EditorGUI.indentLevel ++;

			for (int i = 0; i < lights.Length; i++) {

				EditorGUILayout.BeginHorizontal();
				if(lights[i].lightType == VehicleLights.LightType.Indicator){
					EditorGUILayout.ObjectField("Indicator Light", lights[i].GetComponent<Light>(), typeof(Light), true);
					if(GUILayout.Button("X", GUILayout.Width(25f)))
						DestroyImmediate(lights[i].gameObject);
				}
				EditorGUILayout.EndHorizontal();

			}

			EditorGUI.indentLevel --;
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("Head Light"))
				ObjectCreation.CreateHeadLight();
			if(GUILayout.Button("Brake Light"))
				ObjectCreation.CreateBrakeLight();
			if(GUILayout.Button("Reverse Light"))
				ObjectCreation.CreateReverseLight();
			if(GUILayout.Button("Indicator Light"))
				ObjectCreation.CreateIndicatorLight();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();

			EditorGUILayout.Space();
			EditorGUI.indentLevel --;
			EditorGUILayout.LabelField("Exhausts", EditorStyles.boldLabel);
			EditorGUILayout.Space();
			EditorGUI.indentLevel ++;

			Exhaust[] exhausts = carScript.GetComponentsInChildren<Exhaust>();

			for (int i = 0; i < exhausts.Length; i++) {
				EditorGUILayout.BeginHorizontal();				
				EditorGUILayout.ObjectField("Exhausts", exhausts[i].GetComponent<Exhaust>(), typeof(ParticleSystem), true);
				if(GUILayout.Button("X", GUILayout.Width(25f)))
				DestroyImmediate(exhausts[i].gameObject);				
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.BeginHorizontal();		
			
			if(GUILayout.Button("Exhaust", GUILayout.Width(125f)))
				ObjectCreation.CreateExhaust();

			EditorGUILayout.EndHorizontal();			
			
		}	

		
		DrawDefaultInspector();
		//else if (GUILayout.Button("CloseProperties")) CloseAllCatergory();
		
	}


	bool EnableCategory(){

		//CarSettings = false;
		SuspensionSettings = false;
		FrontSuspension = false;
		RearSuspension = false;		
		LightSettings = false;		

		return true;

	}

	void CloseAllCatergory(){
		SuspensionSettings = false;
		FrontSuspension = false;
		RearSuspension = false;		
		LightSettings = false;		
	}	
}
