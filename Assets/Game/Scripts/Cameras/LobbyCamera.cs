using UnityEngine;
using System.Collections;

public class LobbyCamera : MonoBehaviour
{
    public Transform target;
    public float speed = 200;
    float maxMovement = 1;

    float rotY;
    float rotX;
    private void Update() 
    {
		if(Input.GetMouseButton(0))
        {
            rotY += Input.GetAxis("Mouse X") * speed * Time.deltaTime;
            rotX += -Input.GetAxis("Mouse Y") * speed * Time.deltaTime;

            rotX = Mathf.Clamp(rotX, -70, 90);

            Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
            transform.rotation = localRotation;     
        }

        if(Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            var pos = transform.GetChild(0).localPosition;
            pos.z += Input.GetAxis("Mouse ScrollWheel") * 10;
            
            transform.GetChild(0).transform.localPosition = pos;
        }
    }
}