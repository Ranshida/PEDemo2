using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI面板统一管理
/// </summary>
public class UIManager : MonoBehaviour
{
    public UIManager Instance { get; private set; }
    public Transform mainCanvas;
    public List<WindowRoot> windowList;
    public Dictionary<string, WindowRoot> WindowDict;


    private void Awake()
    {
        Instance = this;
        WindowDict = new Dictionary<string, WindowRoot>();
        windowList = new List<WindowRoot>();
        WindowRoot[] windows = mainCanvas.GetComponentsInChildren<WindowRoot>();
        foreach (WindowRoot window in windows)
        {
            windowList.Add(window);
            WindowDict.Add(window.transform.name, window);
        }
    }
}
