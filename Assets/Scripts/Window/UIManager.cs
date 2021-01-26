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
    public List<WindowRoot> windowList;
    public Dictionary<string, WindowRoot> WindowDict;

    public List<WindowRoot> BottomWindows;
    public List<WindowRoot> FixedWindows;
    public List<WindowRoot> DynamicWindows;
    public List<WindowRoot> TopWindows;

    private void Awake()
    {
        Instance = this;
        WindowDict = new Dictionary<string, WindowRoot>();
        windowList = new List<WindowRoot>();
        BottomWindows = new List<WindowRoot>();
        FixedWindows = new List<WindowRoot>();
        DynamicWindows = new List<WindowRoot>();
        TopWindows = new List<WindowRoot>();
        WindowRoot[] windows = mainCanvas.GetComponentsInChildren<WindowRoot>();
        foreach (WindowRoot window in windows)
        {
            windowList.Add(window);
            WindowDict.Add(window.transform.name, window);

            switch (window.Layer)
            {
                case UILayer.Bottom:
                    BottomWindows.Add(window);
                    break;
                case UILayer.Fixed:
                    FixedWindows.Add(window);
                    break;
                case UILayer.Dynamic:
                    DynamicWindows.Add(window);
                    break;
                case UILayer.Top:
                    TopWindows.Add(window);
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 生成某个新的面板
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public WindowRoot NewWindow(GameObject prefab)
    {
        //GameObject.Instantiate()
        return null;
    }

    /// <summary>
    /// 消除某个真的用不上的面板
    /// </summary>
    public void DestroyWindow()
    {
        //Destroy()
    }

    public void OnClickDown(GameObject go, Action<PointerEventData> cb)
    {
        Listener listener = Function.GetOrAddComponent<Listener>(go);
        listener.onClickDown = cb;
    }

    public void OnClickUp(GameObject go, Action<PointerEventData> cb)
    {
        Listener listener = Function.GetOrAddComponent<Listener>(go);
        listener.onClickUp = cb;
    }
    public void OnDrag(GameObject go, Action<PointerEventData> cb)
    {
        Listener listener = Function.GetOrAddComponent<Listener>(go);
        listener.onDrag = cb;
    }
    public void OnPointerEnter(GameObject go, Action cb)
    {
        Listener listener = Function.GetOrAddComponent<Listener>(go);
        listener.onPointerEnter = cb;
    }
    public void OnPointerExit(GameObject go, Action cb, bool includeOnDisable = true)
    {
        Listener listener = Function.GetOrAddComponent<Listener>(go);
        listener.onPointerExit = cb;
        if (includeOnDisable)
        {
            listener.onDisable = cb;
        }
    }
    public void OnPointing(GameObject go, Action cb)
    {
        Listener listener = Function.GetOrAddComponent<Listener>(go);
        listener.onPointing = cb;
    }
}

