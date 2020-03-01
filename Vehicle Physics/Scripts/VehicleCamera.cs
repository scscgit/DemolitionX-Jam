using UnityEngine;
using System.Collections;

///<summary>
///Basic Camera system for the vehicle
///</summary>
public class VehicleCamera : MonoBehaviour{

	public Transform playerCar;
	private Rigidbody playerRigid;
	private Camera cam;

	public float distance = 6.0f;
	
	public float height = 2.0f;
	
	public float heightOffset = .75f;
	public float heightDamping = 2.0f;
	public float rotationDamping = 3.0f;
	public bool useSmoothRotation = true;
	
	public float minimumFOV = 50f;
	public float maximumFOV = 70f;
	
	public float maximumTilt = 15f;
	private float tiltAngle = 0f;

	[Header("Hood Camera")]
    public bool hoodCamera;
    public GameObject hoodCameraObj;
	
	void Start(){
		
		if (!playerCar){
			if(GameObject.FindObjectOfType<VehiclePhysics>())
				playerCar = GameObject.FindObjectOfType<VehiclePhysics>().transform;
			else
				return;
		}

		playerRigid = playerCar.GetComponent<Rigidbody>();
		cam = GetComponent<Camera>();		
	}
	
	void Update(){
		
		if (!playerCar)
			return;
		
		if(playerRigid != playerCar.GetComponent<Rigidbody>())
			playerRigid = playerCar.GetComponent<Rigidbody>();
		
		tiltAngle = Mathf.Lerp (tiltAngle, (Mathf.Clamp (-playerCar.InverseTransformDirection(playerRigid.velocity).x, -35, 35)), Time.deltaTime * 2f);

		if(!cam)
			cam = GetComponent<Camera>();

		cam.fieldOfView = Mathf.Lerp (minimumFOV, maximumFOV, (playerRigid.velocity.magnitude * 3f) / 150f);
		
	}

	private void LateUpdate() {
		if(Input.GetKeyDown(KeyCode.V)) hoodCamera = !hoodCamera;

            if(hoodCamera)
            {
                //transform.parent = target;
                //transform.position = hoodCameraObj.transform.position;
                FirstUpdate();
            }
            else 
            {
                //transform.parent = null;  
                TPSUpdate();
            }     
	}

	void FirstUpdate()
    {
        //Vector3 desiredPosition = Vector3.Lerp(transform.position, hoodCameraObj.transform.position, Time.deltaTime * 50f);   
        transform.position = hoodCameraObj.transform.position;    
        transform.rotation = Quaternion.Lerp(transform.rotation, hoodCameraObj.transform.rotation, Time.deltaTime * 10f);     
    }
	
	void TPSUpdate (){
		
		if (!playerCar || !playerRigid)
			return;
		
		float speed = (playerRigid.transform.InverseTransformDirection(playerRigid.velocity).z) * 3f;
		
		float wantedRotationAngle = playerCar.eulerAngles.y;
		float wantedHeight = playerCar.position.y + height;
		float currentRotationAngle = transform.eulerAngles.y;
		float currentHeight = transform.position.y;

		if(useSmoothRotation)
			rotationDamping = Mathf.Lerp(0f, 3f, (playerRigid.velocity.magnitude * 3f) / 40f);
		
		if(speed < -10)
			wantedRotationAngle = playerCar.eulerAngles.y + 180;
		
		currentRotationAngle = Mathf.LerpAngle (currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
		
		currentHeight = Mathf.Lerp (currentHeight, wantedHeight + Mathf.Lerp(-1f, 0f, (playerRigid.velocity.magnitude * 3f) / 20f), heightDamping * Time.deltaTime);
		
		Quaternion currentRotation = Quaternion.Euler (0, currentRotationAngle, 0);
		
		transform.position = playerCar.position;
		transform.position -= currentRotation * Vector3.forward * distance;

		transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);
		
		transform.LookAt (new Vector3(playerCar.position.x, playerCar.position.y + heightOffset, playerCar.position.z));
		transform.eulerAngles = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y, Mathf.Clamp(tiltAngle, -10f, 10f));
		
	}

}