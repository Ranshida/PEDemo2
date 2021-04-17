using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DepInfo : MonoBehaviour
{
    public Text Text_Name, Text_Require, Text_Function, Text_Size, Text_PurchaseCost, Text_MaintainCost;
    public GameObject SingleEffectPrefab, DoubleEffectPrefab;
    public HireControl HC;
    public Transform EmpInfoContent;

    public List<GameObject> Effects = new List<GameObject>();

    BuildingType CurrentType;
    int cost;

    public void SetInfo()
    {
        foreach(GameObject g in Effects)
        {
            Destroy(g);
        }
        Effects.Clear();

        List<BuildingType> buildingList = new List<BuildingType>();     //临时存储所有可用的抽卡列表
        foreach (BuildingType type in BuildingManage.Instance.SelectList)
        {
            buildingList.Add(type);
        }
        CurrentType = buildingList[Random.Range(0, buildingList.Count)];

        Building building = null;
        if (BuildingManage.Instance.m_AllBuildingPrefab.TryGetValue(CurrentType, out GameObject go))
            building = go.GetComponent<Building>();

        Text_Name.text = building.Name;
        Text_Size.text = "尺寸:" + building.Size;
        Text_Require.text = "岗位需求:" + building.Require_A;
        Text_PurchaseCost.text = "手续费:" + building.PurchaseCost;
        Text_MaintainCost.text = "维护费:" + building.MaintainCost + "/回合";
        cost = building.PurchaseCost;

        if (CurrentType == BuildingType.机械自动化中心)
        {
            InitMarker("事业部效率+1");
            InitMarker("事业部效率+1");
            InitDoubleMarker("事业部效率+2");
            Text_Function.text = "增加事业部效率";
        }
        else if (CurrentType == BuildingType.心理咨询室)
        {
            InitMarker("充能周期:4");
            InitMarker("充能周期:2");
            Text_Function.text = "充能后指定1名员工心力+20";
        }
        else if (CurrentType == BuildingType.智库小组)
        {
            InitMarker("生产周期:4");
            InitMarker("生产周期:2");
            Text_Function.text = "生产“管理咨询报告”物品";
        }
        else if (CurrentType == BuildingType.前端小组)
        {
            InitDoubleMarker("生产商战牌-迭代");
            Text_Function.text = "生产商战卡牌";
        }
    }

    public void ConfirmBuilding()
    {
        if (HC.GC.Money >= cost)
            HC.GC.Money -= cost;
        else
        {
            QuestControl.Instance.Init("金钱不足");
            return;
        }
        HC.BuildingPurchase(CurrentType);
        HC.StorePanel.SetWndState(false);
    }

    //生成单图标记
    void InitMarker(string describe)
    {
        EmpEffect effect = Instantiate(SingleEffectPrefab, EmpInfoContent).GetComponent<EmpEffect>();
        effect.text.text = describe;
        Effects.Add(effect.gameObject);
    }
    //生成双图标记
    void InitDoubleMarker(string describe)
    {
        EmpEffect effect = Instantiate(DoubleEffectPrefab, EmpInfoContent).GetComponent<EmpEffect>();
        effect.text.text = describe;
        Effects.Add(effect.gameObject);
    }
}
