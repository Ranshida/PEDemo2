using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ui事件监听插件
/// </summary>
public class Listener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public Action<PointerEventData> onClickDown;
    public Action<PointerEventData> onClickUp;
    public Action<PointerEventData> onDrag;
    public Action onPointerEnter;
    public Action onPointerExit;
    public Action onDisable;
    public Action onPointing;

    bool m_Pointing = false;

    private void Update()
    {
        if (!m_Pointing)
        {
            if (UISvc.PointingSelf(transform))
            {
                m_Pointing = true;
                onPointerEnter?.Invoke();
            }
        }
        else
        {
            if (UISvc.PointingSelf(transform))
            {
                onPointing?.Invoke();
            }
            else
            {
                m_Pointing = false;
                onPointerExit?.Invoke();
            }
        }
    }

    private void OnDisable()
    {
        if (m_Pointing)
        {
            onDisable?.Invoke();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (onClickDown != null)
        {
            onClickDown(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (onClickUp != null)
        {
            onClickUp(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (onDrag != null)
        {
            onDrag(eventData);
        }
    }
}
