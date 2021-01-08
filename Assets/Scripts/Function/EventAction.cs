using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class EventAction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Action PointerEnter;
    public Action PointerExit;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (PointerEnter != null)
        {
            PointerEnter();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (PointerExit != null)
        {
            PointerExit();
        }
    }
}
