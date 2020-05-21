using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Drag : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public GarageCamera cam;

    private bool isPressing = false;

    public void OnDrag(PointerEventData data)
    {
        isPressing = true;

        cam.OnDrag(data);
    }

    public void OnEndDrag(PointerEventData data)
    {
        isPressing = false;
    }
}
