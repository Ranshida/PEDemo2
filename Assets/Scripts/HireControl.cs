using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HireControl : MonoBehaviour
{
    public int MaxHireNum = 1;//单次招聘最多选择的人数
    public int ExtraHireOption = 0;//刷新时额外提供的人选数量，单次最多3，多余的下次继承

    public Transform TotalEmpContent;
    public GameControl GC;
    public Button HireRefreshButton;
    public Text Text_HireButtonText, Text_Hire, Text_Dep, Text_Item;
    public EmpInfo EmpInfoPrefab, EmpDetailPrefab;
    public WindowBaseControl StorePanel;

    public EmpInfo[] HireInfos = new EmpInfo[5];
    public List<DepInfo> DepInfos = new List<DepInfo>(); //3个,0号为货运平台的， 1/2号为走私船的
    public List<CItemPurchaseInfo> ItemInfos = new List<CItemPurchaseInfo>();//3个 0/1为走私船的，2为货运平台的
    public List<CWCardInfo> CardInfos = new List<CWCardInfo>();

    int CurrentHireNum;//用于计算单次招聘已选择的人数

    private void Start()
    {
        InitCEO();

        Refresh();
        //初始的员工
        for (int i = 0; i < 5; i++)
        {
            if (i < 2)
            {
                if (i == 0)
                    HireInfos[i].emp.RandomOccupation(6);
                else if (i == 1)
                    HireInfos[i].emp.RandomOccupation(0);
                GC.CurrentEmpInfo = HireInfos[i];
                SetInfoPanel();
                GC.CurrentEmpInfo.emp.InfoA.transform.parent = GC.StandbyContent;
                if (i == 0)
                {
                    GC.CurrentEmpInfo.emp.AddEmotion(EColor.Red);
                    GC.CurrentEmpInfo.emp.AddEmotion(EColor.Red);
                }
            }
            HireInfos[i].gameObject.SetActive(false);
        }
        HireInfos[2].CurrentNode.RemoveEmpInfo();

        GC.CurrentEmpInfo = null;

        //因为目前跟招聘有关，所以航线地图初始化在这里进行
        GC.CrC.InitDefaultMap();
    }

    public void OpenStorePanel()
    {
        //有未处理事件时不能继续
        if (GC.EC.UnfinishedEvents.Count > 0)
            return;
        StorePanel.SetWndState(true);
    }


    public void Refresh()
    {
        //先全检查一遍并重置已有归属的城市，如果在下面的for中依次检查，有可能靠后的info原本属于靠前的info刚随机到的城市
        for(int i = 0; i < 3; i++)
        {
            if (HireInfos[i].CurrentNode != null)
                HireInfos[i].CurrentNode.RemoveEmpInfo();
        }

        for (int i = 0; i < 3; i++)
        {
            HireInfos[i].gameObject.SetActive(true);
            foreach (Transform child in HireInfos[i].PerkContent)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform child in HireInfos[i].SkillContent)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform child in HireInfos[i].StrategyContent)
            {
                Destroy(child.gameObject);
            }
            HireInfos[i].PerksInfo.Clear();
            HireInfos[i].StrategiesInfo.Clear();
            HireInfos[i].CreateEmp();
            RandomNode(HireInfos[i]);
        }
        ////随机一下是出现建筑还是卡牌
        //for (int i = 0; i < 3; i++)
        //{
        //    if (Random.Range(0.0f, 1.0f) < 0.5f)
        //    {
        //        DepInfos[i].SetInfo();
        //        DepInfos[i].gameObject.SetActive(true);
        //        CardInfos[i].gameObject.SetActive(false);
        //    }
        //    else
        //    {
        //        CardInfos[i].CurrentCard = CWCard.CWCardData[Random.Range(0, CWCard.CWCardData.Count)].Clone();
        //        CardInfos[i].UpdateUI();
        //        CardInfos[i].gameObject.SetActive(true);
        //        DepInfos[i].gameObject.SetActive(false);
        //    }
        //}
    }

    //(Hire)招聘后信息转移和创建信息面板
    public void SetInfoPanel()
    {
        GC.CurrentEmpInfo.CurrentNode.RemoveEmpInfo();
        GC.CurrentEmpInfo.gameObject.SetActive(false);
        GC.HC.NodeCheck();

        EmpInfo ED = UIManager.Instance.NewWindow(EmpDetailPrefab.gameObject).GetComponent<EmpInfo>();
        GC.CurrentEmpInfo.CopyStatus(ED);

        EmpInfo EI1 = Instantiate(EmpInfoPrefab, TotalEmpContent);
        GC.CurrentEmpInfo.CopyStatus(EI1);

        EmpInfo EI2 = Instantiate(EmpInfoPrefab, TotalEmpContent);
        GC.CurrentEmpInfo.CopyStatus(EI2);

        EI1.DetailInfo = ED;
        EI2.DetailInfo = ED;
        ED.DetailInfo = ED;
        ED.emp.InfoA = EI1;
        ED.emp.InfoB = EI2;
        ED.emp.InfoDetail = ED;
        ED.emp.InitRelation();
        GC.TurnEvent.AddListener(ED.emp.TimePass);
        ED.Text_Name.text = ED.emp.Name;

        //创建员工实体
        ED.Entity = EmpManager.Instance.CreateEmp(BuildingManage.Instance.ExitPos.position);
        ED.Entity.SetInfo(ED);

        //注意应放在初始化人际关系后再添加至链表
        GC.CurrentEmployees.Add(GC.CurrentEmpInfo.emp);
        //复制特质
        for (int i = 0; i < GC.CurrentEmpInfo.PerksInfo.Count; i++)
        {
            int Num = GC.CurrentEmpInfo.PerksInfo[i].CurrentPerk.Num;
            if (Num >= 1 && Num <= 51)
                GC.CurrentEmpInfo.PerksInfo[i].transform.parent = ED.PerkContent2;
            else
                GC.CurrentEmpInfo.PerksInfo[i].transform.parent = ED.PerkContent;
            ED.PerksInfo.Add(GC.CurrentEmpInfo.PerksInfo[i]);

            //部分影响公司的负面特质的临时效果添加（刷新时不直接添加效果）
            if (GC.CurrentEmpInfo.PerksInfo[i].CurrentPerk.CompanyDebuffPerk == true)
                GC.CurrentEmpInfo.PerksInfo[i].CurrentPerk.AddEffect(); 
        }
        GC.CurrentEmpInfo.PerksInfo.Clear();

        //复制战略
        for (int i = 0; i < GC.CurrentEmpInfo.StrategiesInfo.Count; i++)
        {
            GC.CurrentEmpInfo.StrategiesInfo[i].transform.parent = ED.StrategyContent;
            ED.StrategiesInfo.Add(GC.CurrentEmpInfo.StrategiesInfo[i]);
        }
        GC.CurrentEmpInfo.StrategiesInfo.Clear();

        ED.SetAmbitionInfo();

        //创建招聘历史
        if (GC.CC.CEO != null)
            GC.CC.CEO.InfoDetail.AddHistory("招聘了" + ED.emp.Name);
        //HideOptions();
    }

    //初始化CEO
    public void InitCEO()
    {
        Employee emp = new Employee();
        emp.InitCEOStatus();

        EmpInfo ED = UIManager.Instance.NewWindow(EmpDetailPrefab.gameObject).GetComponent<EmpInfo>();
        ED.GC = GC;
        ED.emp = emp;
        ED.Text_Name.text = ED.emp.Name;
        GC.CC.CEO = emp;

        EmpInfo EI1 = Instantiate(EmpInfoPrefab, TotalEmpContent);
        ED.CopyStatus(EI1);
        EI1.FireButton.gameObject.SetActive(false);
        EI1.transform.parent = GC.StandbyContent;

        EmpInfo EI2 = Instantiate(EmpInfoPrefab, TotalEmpContent);
        ED.CopyStatus(EI2);
        EI2.FireButton.gameObject.SetActive(false);

        emp.InfoDetail = ED;
        emp.InfoA = EI1;
        emp.InfoB = EI2;
        EI1.DetailInfo = ED;
        EI2.DetailInfo = ED;
        emp.InitRelation();
        //创建特质和技能
        ED.InitStrategyAndPerk();
        ED.SetAmbitionInfo();
        //创建员工实体
        ED.Entity = EmpManager.Instance.CreateEmp(BuildingManage.Instance.ExitPos.position);
        ED.Entity.SetInfo(ED);

        GC.CurrentEmployees.Add(emp);
        //将CEO添加到核心团队
        GC.BSC.CEOInfo.EmpJoin(emp);
        GC.AC.AreaLists[0].DC.SetManager(false, emp);

        //初始的抉择卡创建
        GC.OCL.AddOptionCard(new OptionCard1(), emp);
        GC.OCL.AddOptionCard(new OptionCard1(), emp);
        GC.OCL.AddOptionCard(new OptionCard2(), emp);
        GC.OCL.AddOptionCard(new OptionCard3(), emp);
        GC.OCL.AddOptionCard(new OptionCard4(), emp);
    }

    //招聘后隐藏其它选项
    public void HideOptions()
    {
        CurrentHireNum += 1;
        if (CurrentHireNum >= MaxHireNum)
        {
            for (int i = 0; i < 5; i++)
            {
                HireInfos[i].gameObject.SetActive(false);
            }
        }
    }

    public void BuildingPurchase(BuildingType type)
    {
        //foreach(DepInfo info in DepInfos)
        //{
        //    info.gameObject.SetActive(false);
        //}
        BuildingManage.Instance.EnterBuildMode();
        BuildingManage.Instance.StartBuildNew(type);
    }

    //检测当前节点是否能雇佣员工和购买物品与建筑
    public void NodeCheck()
    {
        bool EmpCheck = false, BuildingCheck = false, ItemCheck = false;
        foreach(EmpInfo info in HireInfos)
        {
            if (info.gameObject.activeSelf == false)
                continue;
            if (info.CurrentNode == null || GC.CrC.CurrentNode.CityNum != info.CurrentNode.CityNum)
            {
                info.HireButton.interactable = false;
            }
            else if (GC.CrC.CurrentNode.CityNum == info.CurrentNode.CityNum)
            {
                info.HireButton.interactable = true;
                EmpCheck = true;
            }
        }
        foreach (DepInfo info in DepInfos)
        {
            if (info.gameObject.activeSelf == false)
                continue;
            if (info.CurrentNode == null)
            {
                info.PurchaseButton.interactable = false;
                continue;
            }
            if (info.CurrentNode != GC.CrC.CurrentNode)
                info.PurchaseButton.interactable = false;
            else
            {
                info.PurchaseButton.interactable = true;
                BuildingCheck = true;
            }
        }

        foreach (CItemPurchaseInfo info in ItemInfos)
        {
            if (info.gameObject.activeSelf == false)
                continue;
            if (info.CurrentNode == null)
            {
                info.PurchaseButton.interactable = false;
                continue;
            }
            if (info.CurrentNode != GC.CrC.CurrentNode)
                info.PurchaseButton.interactable = false;
            else
            {
                info.PurchaseButton.interactable = true;
                ItemCheck = true;
            }
        }

        if (EmpCheck == true)
            Text_Hire.color = Color.red;
        else
            Text_Hire.color = Color.black;

        if (BuildingCheck == true)
            Text_Dep.color = Color.red;
        else
            Text_Dep.color = Color.black;

        if (ItemCheck == true)
            Text_Item.color = Color.red;
        else
            Text_Item.color = Color.black;

        //物品栏

        if (EmpCheck == true || BuildingCheck == true || ItemCheck == true)
            Text_HireButtonText.color = Color.red;
        else
            Text_HireButtonText.color = Color.black;

    }

    private void RandomNode(EmpInfo info)
    {
        List<CourseNode> nodes = new List<CourseNode>();
        bool CityGAdded = false;
        foreach(CourseNode node in GC.CrC.CityNodes)
        {
            //G城只能有1个员工
            if (node.HaveEmp == false)
            {
                if (node.CityNum == 7 && CityGAdded == true)
                    continue;
                else
                    CityGAdded = true;
                nodes.Add(node);
            }
            else if (node.CityNum == 7)
                CityGAdded = true;
        }
        int num = Random.Range(0, nodes.Count);
        nodes[num].SetEmpInfo();
        info.CurrentNode = nodes[num];
        info.Text_City.text = nodes[num].Text_City.text;
    }
}
