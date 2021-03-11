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

    private BuildingInfo buildingInfo;     //单一建筑的信息，在鼠标指向时显示
    private List<BuildingInfo> allBuildingInfos = new List<BuildingInfo>();   //所有建筑的信息列表，在分配水晶时全部显示
    private GameObject buildingInfoPrefab;        //预制体

    private bool m_ShowAllBuildingInfo = false;   //正在显示所有建筑信息
    private Text m_EmpName;
    private bool m_ShowName;

    private void Awake()
    {
        Instance = this;
        tips = transform.Find("Tips");
        infos = transform.Find("Infos");
        buildingInfoParent = transform.Find("BuildingInfos");
        buildingInfo = transform.Find("BuildingInfo").GetComponent<BuildingInfo>();
        m_EmpName = infos.Find("EmpName").GetComponent<Text>();
        dialoguePrefab = ResourcesLoader.LoadPrefab("Prefabs/UI/Dialogue");
        buildingInfoPrefab = ResourcesLoader.LoadPrefab("Prefabs/UI/Building/BuildingInfo");
    }

    protected override void UpdateSpecific()
    {
        if (!m_ShowName)
            m_EmpName.transform.position = new Vector3(-1000, 0, 0);
        m_ShowName = false;


        if (m_ShowAllBuildingInfo)
        {  //显示所有建筑信息
            foreach (var info in allBuildingInfos)
            {
                info.OnUpdate();
            }
        }
        //显示单一建筑信息
        buildingInfo.OnUpdate();
        buildingInfo.thisBuilding = null;
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

    public void ShowBuildingInfo(Building building)
    {
        if (m_ShowAllBuildingInfo)
        {
            return;
        }
        buildingInfo.Init(building);
    }
    public void ShowAllBuildingInfo(List<Building> buildings)
    {
        m_ShowAllBuildingInfo = true;
        foreach (Building building in buildings)
        {
            BuildingInfo info = Instantiate(buildingInfoPrefab, buildingInfoParent).GetComponent<BuildingInfo>();
            info.Init(building);
            allBuildingInfos.Add(info);
        }
    }
    public void HideAllBuildingInfo()
    {
        m_ShowAllBuildingInfo = false;
        foreach (BuildingInfo info in allBuildingInfos)
        {
            Destroy(info.gameObject);
        }
        allBuildingInfos.Clear();
    }
}
