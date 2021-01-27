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
    // Start is called before the first frame update
    public void Init(WindowRoot window)
    {
        m_ThisWindow = window;
        m_RectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        m_Offset = RectPosition2D - eventData.position;
        Debug.Log("StartDrag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 pos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(m_RectTransform, eventData.position + m_Offset, eventData.enterEventCamera, out pos);
        m_RectTransform.position = pos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("EndDrag");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UIManager.Instance.SetTop(m_ThisWindow);
    }
}
