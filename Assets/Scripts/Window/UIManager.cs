using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI面板层级
/// </summary>
public enum UILayer
{
    Bottom,           //底部的，通常永久显示，固定层级和摆放位置
    Fixed,            //更上一层，可以显示或关系，但位置固定（参考BuildingWindow）
    Dynamic,          //更上一层，浮动面板，点击的那个在最上面，同时可以拖拽（类似员工信息面板）
    Top,              //最上层，消息提示，最后弹出来的在最上面，不能拖或点
}

/// <summary>
/// UI面板统一管理
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public Transform mainCanvas;
    public Transform BottomTrans;
    public Transform FixedTrans;
    public Transform DynamicTrans;
    public Transform TopTrans;


    public List<WindowRoot> windowList;
    public Dictionary<string, WindowRoot> StaticWindowDict;       //固定的UI，Awake后固定

    public List<WindowRoot> BottomWindows;         //底层UI
    public List<WindowRoot> FixedWindows;          //2级固定UI
    public List<WindowRoot> DynamicWindows;        //3级动态UI
    public List<WindowRoot> TopWindows;            //4级顶部UI

    private void Awake()
    {
        Instance = this;
        //StaticWindowDict = new Dictionary<string, WindowRoot>();
        windowList = new List<WindowRoot>();
        BottomWindows = new List<WindowRoot>();
        FixedWindows = new List<WindowRoot>();
        DynamicWindows = new List<WindowRoot>();
        TopWindows = new List<WindowRoot>();
        WindowRoot[] windows = mainCanvas.GetComponentsInChildren<WindowRoot>(true);
        foreach (WindowRoot window in windows)
        {
            //StaticWindowDict.Add(window.transform.name, window);
            OnAddNewWindow(window);
        }
    }

    /// <summary>
    /// 生成某个新的面板
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public WindowRoot NewWindow(GameObject prefab)
    {
        GameObject go = Instantiate(prefab, mainCanvas);
        WindowRoot window = go.GetComponent<WindowRoot>();
        OnAddNewWindow(window);
        return window;
    }

    public void OnAddNewWindow(WindowRoot window)
    {
        windowList.Add(window);
        switch (window.Layer)
        {
            case UILayer.Bottom:
                BottomWindows.Add(window);
                window.transform.SetParent(BottomTrans);
                break;
            case UILayer.Fixed:
                FixedWindows.Add(window);
                window.transform.SetParent(FixedTrans);
                break;
            case UILayer.Dynamic:
                DynamicWindows.Add(window);
                window.gameObject.AddComponent<Wnd_Dynamic>().Init(window);
                window.transform.SetParent(DynamicTrans);
                break;
            case UILayer.Top:
                TopWindows.Add(window);
                window.transform.SetParent(TopTrans);
                break;
            default:
                break;
        }
        window.SetDefaultPos();
    }

    /// <summary>
    /// 消除某个真的用不上的面板
    /// </summary>
    public void DestroyWindow(WindowRoot window)
    {
        //if (StaticWindowDict.ContainsKey(window.transform.name))
        //{
        //    StaticWindowDict.Remove(window.transform.name);
        //}

        windowList.Remove(window);

        switch (window.Layer)
        {
            case UILayer.Bottom:
                BottomWindows.Remove(window);
                break;
            case UILayer.Fixed:
                FixedWindows.Remove(window);
                break;
            case UILayer.Dynamic:
                DynamicWindows.Remove(window);
                break;
            case UILayer.Top:
                TopWindows.Remove(window);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 将窗口放在自身父物体的最下方（顶层）
    /// </summary>
    /// <param name="window"></param>
    public void SetTop(WindowRoot window)
    {
        window.transform.SetAsLastSibling();
    }


    /// <summary>
    /// 鼠标按下点击事件
    /// </summary>
    /// <param name="go">物体</param>
    /// <param name="cb">事件</param>
    public void OnClickDown(GameObject go, Action<PointerEventData> cb)
    {
        Listener listener = Function.GetOrAddComponent<Listener>(go);
        listener.onClickDown = cb;
    }
    //增加一个
    public void AddOnClickDown(GameObject go, Action<PointerEventData> cb)
    {
        Listener listener = Function.GetOrAddComponent<Listener>(go);
        listener.onClickDown += cb;
    }
    //移除一个
    public void RemoveOnClickDown(GameObject go, Action<PointerEventData> cb)
    {
        Listener listener = Function.GetOrAddComponent<Listener>(go);
        listener.onClickDown -= cb;
    }

    /// <summary>
    /// 鼠标松开事件
    /// </summary>
    /// <param name="go">物体</param>
    /// <param name="cb">事件</param>
    public void OnClickUp(GameObject go, Action<PointerEventData> cb)
    {
        Listener listener = Function.GetOrAddComponent<Listener>(go);
        listener.onClickUp = cb;
    }
    //增加一个
    public void AddOnClickUp(GameObject go, Action<PointerEventData> cb)
    {
        Listener listener = Function.GetOrAddComponent<Listener>(go);
        listener.onClickUp += cb;
    }
    //移除一个
    public void RemoveOnClickUp(GameObject go, Action<PointerEventData> cb)
    {
        Listener listener = Function.GetOrAddComponent<Listener>(go);
        listener.onClickUp -= cb;
    }

    /// <summary>
    /// 鼠标拖拽事件
    /// </summary>
    /// <param name="go">物体</param>
    /// <param name="cb">事件</param>
    public void OnDrag(GameObject go, Action<PointerEventData> cb)
    {
        Listener listener = Function.GetOrAddComponent<Listener>(go);
        listener.onDrag = cb;
    }
    //增加一个
    public void AddOnDrag(GameObject go, Action<PointerEventData> cb)
    {
        Listener listener = Function.GetOrAddComponent<Listener>(go);
        listener.onDrag += cb;
    } 
    //移除一个
    public void RemoveOnDrag(GameObject go, Action<PointerEventData> cb)
    {
        Listener listener = Function.GetOrAddComponent<Listener>(go);
        listener.onDrag -= cb;
    }

    /// <summary>
    /// 鼠标进入UI物体事件
    /// </summary>
    /// <param name="go">物体</param>
    /// <param name="cb">事件</param>
    public void OnPointerEnter(GameObject go, Action cb)
    {
        Listener listener = Function.GetOrAddComponent<Listener>(go);
        listener.onPointerEnter = cb;
    }
    //增加一个
    public void AddOnPointerEnter(GameObject go, Action cb)
    {
        Listener listener = Function.GetOrAddComponent<Listener>(go);
        listener.onPointerEnter += cb;
    }
    //移除一个
    public void RemoveOnPointerEnter(GameObject go, Action cb)
    {
        Listener listener = Function.GetOrAddComponent<Listener>(go);
        listener.onPointerEnter -= cb;
    }

    /// <summary>
    /// 鼠标离开UI物体事件
    /// </summary>
    /// <param name="go">物体</param>
    /// <param name="cb">事件</param>
    public void OnPointerExit(GameObject go, Action cb, bool includeOnDisable = true)
    {
        Listener listener = Function.GetOrAddComponent<Listener>(go);
        listener.onPointerExit = cb;
        if (includeOnDisable)
        {
            listener.onDisable = cb;
        }
    } 
    //增加一个
    public void AddOnPointerExit(GameObject go, Action cb, bool includeOnDisable = true)
    {
        Listener listener = Function.GetOrAddComponent<Listener>(go);
        listener.onPointerExit += cb;
        if (includeOnDisable)
        {
            listener.onDisable += cb;
        }
    }   
    //移除一个
    public void RemoveOnPointerExit(GameObject go, Action cb)
    {
        Listener listener = Function.GetOrAddComponent<Listener>(go);
        listener.onPointerExit -= cb;
        listener.onDisable -= cb;
    }

    /// <summary>
    /// 鼠标放在UI物体事件
    /// </summary>
    /// <param name="go">物体</param>
    /// <param name="cb">事件</param>
    public void OnPointing(GameObject go, Action cb)
    {
        Listener listener = Function.GetOrAddComponent<Listener>(go);
        listener.onPointing = cb;
    }
    //增加一个
    public void AddOnPointing(GameObject go, Action cb)
    {
        Listener listener = Function.GetOrAddComponent<Listener>(go);
        listener.onPointing += cb;
    }
    //移除一个
    public void RemoveOnPointing(GameObject go, Action cb)
    {
        Listener listener = Function.GetOrAddComponent<Listener>(go);
        listener.onPointing -= cb;
    }
}

