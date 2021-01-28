using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 拖拽窗口
/// </summary>
public class Wnd_Dynamic : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    private WindowRoot m_ThisWindow;
    private RectTransform m_RectTransform;
    private Vector2 RectPosition2D { get { return new Vector2(m_RectTransform.position.x, m_RectTransform.position.y); } }
    private Vector2 m_Offset;
    private bool m_Draging;

    public void Init(WindowRoot window)
    {
        m_ThisWindow = window;
        m_RectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (m_Draging)
        {
            if (Input.mousePosition.x < 0 || Input.mousePosition.y < 0 || Input.mousePosition.x > Screen.width || Input.mousePosition.y > Screen.height)
            {
                return;
            }
            m_RectTransform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y) + m_Offset;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        m_Draging = true;
        m_Offset = RectPosition2D - eventData.position;
        Debug.Log("StartDrag");
    }


    //public void OnDrag(PointerEventData eventData)
    //{
    //    if (eventData.position.x < 0 || eventData.position.y < 0 || eventData.position.x > Screen.width || eventData.position.y > Screen.height) 
    //    {
    //        return;
    //    }
    //    Vector3 pos;
    //    RectTransformUtility.ScreenPointToWorldPointInRectangle(m_RectTransform, eventData.position + m_Offset, eventData.enterEventCamera, out pos);
    //    m_RectTransform.position = pos;
    //}

    public void OnEndDrag(PointerEventData eventData)
    {
        m_Draging = false;
        Debug.Log("EndDrag");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UIManager.Instance.SetTop(m_ThisWindow);
    }

    public void OnDrag(PointerEventData eventData)
    {
    }
}
