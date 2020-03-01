using UnityEngine;
using UnityEditor;
using System.Collections;

public class ObjectCreation : Editor {

	public static void CreateHeadLight(){

			GameObject lightsMain;

			if(!Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().transform.Find(Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().chassis.name+"/Lights")){
				lightsMain = new GameObject("Lights");
				lightsMain.transform.SetParent(Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().chassis.transform, false);
			}else{
				lightsMain = Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().transform.Find(Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().chassis.name+"/Lights").gameObject;
			}

			GameObject headLight = GameObject.Instantiate (CommonSettings.Instance.headLights, lightsMain.transform.position, lightsMain.transform.rotation) as GameObject;
			headLight.name = CommonSettings.Instance.headLights.name;
			headLight.transform.SetParent(lightsMain.transform);
			headLight.transform.localRotation = Quaternion.identity;
			headLight.transform.localPosition = new Vector3(0f, 0f, 2f);
			Selection.activeGameObject = headLight;

	}

	public static void CreateBrakeLight(){

			GameObject lightsMain;

			if(!Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().transform.Find(Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().chassis.name+"/Lights")){
				lightsMain = new GameObject("Lights");
				lightsMain.transform.SetParent(Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().chassis.transform, false);
			}else{
				lightsMain = Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().transform.Find(Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().chassis.name+"/Lights").gameObject;
			}

			GameObject brakeLight = GameObject.Instantiate (CommonSettings.Instance.brakeLights, lightsMain.transform.position, lightsMain.transform.rotation) as GameObject;
			brakeLight.name = CommonSettings.Instance.brakeLights.name;
			brakeLight.transform.SetParent(lightsMain.transform);
			brakeLight.transform.localRotation = Quaternion.identity;
			brakeLight.transform.localPosition = new Vector3(0f, 0f, -2f);
			Selection.activeGameObject = brakeLight;
	}


	public static void CreateReverseLight(){

			GameObject lightsMain;

			if(!Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().transform.Find(Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().chassis.name+"/Lights")){
				lightsMain = new GameObject("Lights");
				lightsMain.transform.SetParent(Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().chassis.transform, false);
			}else{
				lightsMain = Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().transform.Find(Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().chassis.name+"/Lights").gameObject;
			}

			GameObject reverseLight = GameObject.Instantiate (CommonSettings.Instance.reverseLights, lightsMain.transform.position, lightsMain.transform.rotation) as GameObject;
			reverseLight.name = CommonSettings.Instance.reverseLights.name;
			reverseLight.transform.SetParent(lightsMain.transform);
			reverseLight.transform.localRotation = Quaternion.identity;
			reverseLight.transform.localPosition = new Vector3(0f, 0f, -2f);
			Selection.activeGameObject = reverseLight;
	}

	public static void CreateIndicatorLight(){

			GameObject lightsMain;

			if(!Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().transform.Find(Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().chassis.name+"/Lights")){
				lightsMain = new GameObject("Lights");
				lightsMain.transform.SetParent(Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().chassis.transform, false);
			}else{
				lightsMain = Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().transform.Find(Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().chassis.name+"/Lights").gameObject;
			}

			GameObject indicatorLight = GameObject.Instantiate (CommonSettings.Instance.indicatorLights, lightsMain.transform.position, lightsMain.transform.rotation) as GameObject;
			indicatorLight.name = CommonSettings.Instance.indicatorLights.name;
			indicatorLight.transform.SetParent(lightsMain.transform);
			indicatorLight.transform.localRotation = Quaternion.identity;
			indicatorLight.transform.localPosition = new Vector3(0f, 0f, -2f);
			Selection.activeGameObject = indicatorLight;

	}

	public static void CreateExhaust(){

			GameObject exhaustsMain;

			if(!Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().transform.Find(Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().chassis.name+"/Exhausts")){
				exhaustsMain = new GameObject("Exhausts");
				exhaustsMain.transform.SetParent(Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().chassis.transform, false);
			}else{
				exhaustsMain = Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().transform.Find(Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().chassis.name+"/Exhausts").gameObject;
			}

			GameObject exhaust = (GameObject)Instantiate(CommonSettings.Instance.exhaustGas, Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().transform.position, Selection.activeGameObject.GetComponentInParent<VehiclePhysics>().transform.rotation * Quaternion.Euler(0f, 180f, 0f));
			exhaust.name = CommonSettings.Instance.exhaustGas.name;
			exhaust.transform.SetParent(exhaustsMain.transform);
			exhaust.transform.localPosition = new Vector3(1f, 0f, -2f);
			Selection.activeGameObject = exhaust;

	}

}
