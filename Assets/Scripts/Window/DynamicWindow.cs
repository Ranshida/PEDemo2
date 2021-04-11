using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 动态弹窗、消息提示面板
/// </summary>
public class DynamicWindow : WindowRoot
{
    [SerializeField]static public int shit = 0;
    public static DynamicWindow Instance { get; private set; }
    private GameObject dialoguePrefab;

    private Transform tips;
    private Transform infos;
    private Transform buildingInfoParent;

    private GameObject buildingInfoPrefab;        //预制体

    private Text m_EmpName;
    private bool m_ShowName;

    private void Awake()
    {
        Instance = this;
        tips = transform.Find("Tips");
        infos = transform.Find("Infos");
        buildingInfoParent = transform.Find("BuildingInfos");
        m_EmpName = infos.Find("EmpName").GetComponent<Text>();
        dialoguePrefab = ResourcesLoader.LoadPrefab("Prefabs/UI/Dialogue");
        buildingInfoPrefab = ResourcesLoader.LoadPrefab("Prefabs/UI/Building/BuildingInfo");
    }

    protected override void UpdateSpecific()
    {
        if (!m_ShowName)
            m_EmpName.transform.position = new Vector3(-1000, 0, 0);
        m_ShowName = false;
    }


    //设置对话
    public void SetDialogue(Transform followedTrans, string conversition, float timer = 3, Vector3 UIOffset = default, Vector3 worldOffset = default)
    {
        GameObject sign = Instantiate(dialoguePrefab, tips); //设置了锚点，就不需要初始化位置了
        UIDialogue pooledDialogue = sign.GetComponentInChildren<UIDialogue>();
        pooledDialogue.Init(conversition, timer);
        pooledDialogue.Anchor(followedTrans, UIOffset, worldOffset);
    }

    public void ShowName(string name, Transform trans, Vector3 worldOffset = default)
    {
        if (m_ShowName)
        {
            return;
        }
        m_ShowName = true;
        m_EmpName.transform.position = Function.World2ScreenPoint(trans.position + worldOffset);
        m_EmpName.text = name;
    }
}
