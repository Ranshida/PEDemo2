using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 动态弹窗、消息提示面板
/// </summary>
public class DynamicWindow : MonoBehaviour
{
    public static DynamicWindow Instance { get; private set; }
    private GameObject dialoguePrefab;

    private Transform tips;
    private Transform infos;

    private Text empName;

    private bool showName;

    private void Awake()
    {
        Instance = this;
        tips = transform.Find("Tips");
        infos = transform.Find("Infos");
        empName = infos.Find("EmpName").GetComponent<Text>();
        dialoguePrefab = ResourcesLoader.LoadPrefab("Prefabs/UI/Dialogue");
    }

    private void Update()
    {

        if (!showName)
            empName.transform.position = new Vector3(-1000, 0, 0);
        showName = false;
    }

    //设置对话
    public void SetDialogue(Transform followedTrans, string conversition, float timer = 3, Vector3 UIOffset = default, Vector3 worldOffset = default)
    {
        GameObject sign = Instantiate(dialoguePrefab, tips); //设置了锚点，就不需要初始化位置了
        UIDialogue pooledDialogue = sign.GetComponentInChildren<UIDialogue>();
        pooledDialogue.Init(conversition, timer);
        pooledDialogue.Anchor(followedTrans, UIOffset, worldOffset);
    }

    public void SetEmpName(string name, Transform trans, Vector3 worldOffset = default)
    {
        if (showName)
        {
            return;
        }
        showName = true;
        empName.transform.position = Function.World2ScreenPoint(trans.position + worldOffset);
        empName.text = name;
    }
}
