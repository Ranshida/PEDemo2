using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISvc : MonoBehaviour
{
    public static UISvc Instance = null;

    public Transform Canvas { get; set; }
    public static bool IsPointingUI { get { return EventSystem.current.IsPointerOverGameObject(); } }
    private static Transform uiTrans_Cursor;


    private void Awake()
    {
        Instance = this;
        Canvas = GameObject.Find("MainCanvas").transform;
    }

    void Update()
    {
        uiTrans_Cursor = null;
        if (IsPointingUI)
        {
            if (!Canvas)
            {
                Debug.LogError("Canvas没有赋值");
                return;
            }

            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = Input.mousePosition;
            GraphicRaycaster gr = Canvas.GetComponent<GraphicRaycaster>();
            List<RaycastResult> results = new List<RaycastResult>();
            gr.Raycast(pointerEventData, results);
            if (results.Count != 0)
            {
                uiTrans_Cursor = results[0].gameObject.transform;
            }
        }
    }

    /// <summary>
    /// 判断鼠标指针是否正在指向UI物体
    /// </summary>
    /// <param name="transform">UI物体</param>
    /// <returns></returns>
    public static bool PointingSelf(Transform transform)
    {
        List<Transform> transList = Function.ReturnAllChildList(transform);
        if (uiTrans_Cursor != null)
        {
            foreach (Transform tr in transList)
            {
                if (uiTrans_Cursor == tr)
                {
                    return true;
                }
            }
        }
        return false;
    }

}
