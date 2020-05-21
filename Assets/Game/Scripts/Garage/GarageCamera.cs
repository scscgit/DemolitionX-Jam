using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GarageCamera : MonoBehaviour
{
    public Transform target;
    public float speed = 200;
    float maxMovement = 1;

    float rotY;
    float rotX;

    private void OnDrag()
    {
        if (Input.GetMouseButton(0))
        {
            Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
            transform.rotation = localRotation;
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            var pos = transform.GetChild(0).localPosition;
            pos.z += Input.GetAxis("Mouse ScrollWheel") * 10;

            transform.GetChild(0).transform.localPosition = pos;
        }
    }

    public void OnDrag(PointerEventData pointerData)
    {
        // Receiving drag input from UI.
        rotX += pointerData.delta.x * speed;
        rotY -= pointerData.delta.y * speed; // / 1000f;
    }
}
