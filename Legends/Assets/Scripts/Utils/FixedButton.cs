using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FixedButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IDragHandler
{

    public Joystick Joystick;

    [HideInInspector]
    public bool Pressed { get; private set; } = false;

    [HideInInspector]
    public bool Released { get; private set; } = false;

    public void OnDrag(PointerEventData eventData)
    {
        Joystick.OnDrag(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Pressed = true;
        Released = false;
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("FixedButton-OPU");
        Pressed = false;
        Released = true;
        Joystick.OnPointerUp(eventData);
    }

}
