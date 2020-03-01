using UnityEngine;
using System.Collections;

///<summary>
///Simulates Chassis of the car as a configurable joint
///</summary>
public class Chassis : MonoBehaviour {

	private CommonSettings CommonSettingsInstance;
	private CommonSettings CommonSettings {
		get {
			if (CommonSettingsInstance == null) {
				CommonSettingsInstance = CommonSettings.Instance;
			}
			return CommonSettingsInstance;
		}
	}

	private Rigidbody mainRigid;

	private float chassisVerticalLean = 4.0f;		// Chassis Vertical Lean Sensitivity.
	private float chassisHorizontalLean = 4.0f;		// Chassis Horizontal Lean Sensitivity.

	private float horizontalLean = 0f;
	private float verticalLean = 0f;

	void Start () {

		mainRigid = GetComponentInParent<VehiclePhysics> ().GetComponent<Rigidbody> ();

		ChassisJoint ();

	}

	void OnEnable(){
		
		StartCoroutine ("ReEnable");

	}

	IEnumerator ReEnable(){

		if(!GetComponent<ConfigurableJoint>())
			yield return null;

		GameObject _joint = GetComponentInParent<ConfigurableJoint>().gameObject;

		_joint.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
		yield return new WaitForFixedUpdate();
		_joint.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;

	}

	///<summary>
	///Creates a joint for us :)
	///</summary>
	void ChassisJoint(){

		GameObject colliders = new GameObject("Colliders");
		colliders.transform.SetParent(GetComponentInParent<VehiclePhysics> ().transform, false);

		GameObject chassisJoint;

		Transform[] childTransforms = GetComponentInParent<VehiclePhysics> ().chassis.GetComponentsInChildren<Transform>();

		foreach(Transform t in childTransforms){

			if(t.gameObject.activeSelf && t.GetComponent<Collider>()){

				if (t.childCount >= 1) {
					Transform[] childObjects = t.GetComponentsInChildren<Transform> ();
					foreach (Transform c in childObjects) {
						if (c != t) {
							c.SetParent (transform);
						}
					}
				}

				GameObject newGO = (GameObject)Instantiate(t.gameObject, t.transform.position, t.transform.rotation);
				newGO.transform.SetParent(colliders.transform, true);
				newGO.transform.localScale = t.lossyScale;

				Component[] components = newGO.GetComponents(typeof(Component));

				foreach(Component comp  in components){
					if(!(comp is Transform) && !(comp is Collider)){
						Destroy(comp);
					}
				}

			}

		}

		chassisJoint = (GameObject)Instantiate((CommonSettings.chassisJoint), Vector3.zero, Quaternion.identity);
		chassisJoint.transform.SetParent(mainRigid.transform, false);
		chassisJoint.GetComponent<ConfigurableJoint> ().connectedBody = mainRigid;
		chassisJoint.GetComponent<ConfigurableJoint> ().autoConfigureConnectedAnchor = false;

		transform.SetParent(chassisJoint.transform, false);

		Collider[] collidersInChassis = GetComponentsInChildren<Collider>();

		foreach(Collider c in collidersInChassis)
			Destroy(c);

		GetComponentInParent<Rigidbody> ().centerOfMass = new Vector3 (mainRigid.centerOfMass.x, mainRigid.centerOfMass.y + 1f, mainRigid.centerOfMass.z);

	}
}
