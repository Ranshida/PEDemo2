using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//商店页面的建筑信息
public class DepInfo : MonoBehaviour
{
    public Text Text_Name, Text_Require, Text_Function, Text_Size, Text_PurchaseCost, Text_MaintainCost, Text_NodeName;
    public CourseNode CurrentNode;//当前所处节点
    public GameObject SingleEffectPrefab, DoubleEffectPrefab;
    public HireControl HC;
    public Transform EmpInfoContent;
    public InfoPanelTrigger IPT;
    public Button PurchaseButton;

    public List<GameObject> Effects = new List<GameObject>();

    BuildingType CurrentType;
    int cost;

    public void SetInfo(CourseNode node)
    {
        this.gameObject.SetActive(true);
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

        CurrentNode = node;
        if (node.Type == CourseNodeType.货运浮岛平台)
            Text_NodeName.text = "位于货运浮岛平台";
        else if (node.Type == CourseNodeType.走私船)
            Text_NodeName.text = "位于" + node.Text_Name.text;

        Text_Name.text = building.Name;
        Text_Size.text = "尺寸:" + building.Size;
        Text_Require.text = "岗位需求:" + building.Require_A;
        Text_PurchaseCost.text = "手续费:" + building.PurchaseCost;
        Text_MaintainCost.text = "维护费:" + building.MaintainCost + "/回合";
        cost = building.PurchaseCost;

        for(int i = 0; i < 3; i++)
        {
            if (building.EmpCount[i] == 1)
                InitMarker(building.Functions[i], building.Debuffs[i]);
            else if (building.EmpCount[i] == 2)
                InitDoubleMarker(building.Functions[i], building.Debuffs[i]);
            Text_Function.text = building.Description;
        }

        if (building.ExtraInfo != null)
        {
            IPT.ContentB = building.ExtraInfo;
            IPT.gameObject.SetActive(true);
        }
        else
            IPT.gameObject.SetActive(false);
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
        this.gameObject.SetActive(false);
        HC.GC.CrC.GetComponent<WindowBaseControl>().SetWndState(false);
        HC.NodeCheck();
    }

    //生成单图标记
    void InitMarker(string describe, string debuff)
    {
        EmpEffect effect = Instantiate(SingleEffectPrefab, EmpInfoContent).GetComponent<EmpEffect>();
        effect.InitEffect(describe);
        effect.InitDebuff(debuff);
        Effects.Add(effect.gameObject);
        effect.HideOptions();
    }
    //生成双图标记
    void InitDoubleMarker(string describe, string debuff)
    {
        EmpEffect effect = Instantiate(DoubleEffectPrefab, EmpInfoContent).GetComponent<EmpEffect>();
        effect.InitEffect(describe);
        effect.InitDebuff(debuff);
        Effects.Add(effect.gameObject);
        effect.HideOptions();
    }
}
