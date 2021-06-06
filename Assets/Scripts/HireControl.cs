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
    public Text Text_HireButtonText, Text_Hire, Text_Dep;
    public EmpInfo EmpInfoPrefab, EmpDetailPrefab;
    public WindowBaseControl StorePanel;

    public EmpInfo[] HireInfos = new EmpInfo[5];
    public List<DepInfo> DepInfos = new List<DepInfo>();
    public List<CWCardInfo> CardInfos = new List<CWCardInfo>();
    List<HireType> HireTypes = new List<HireType>();

    int CurrentHireNum;//用于计算单次招聘已选择的人数

    private void Start()
    {
        InitCEO();
        AddHireTypes(new HireType());
        Refresh();
        //初始的员工
        for (int i = 0; i < 5; i++)
        {
            if (i < 2)
            {           
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
        GC.CurrentEmpInfo = null;
        AddHireTypes(new HireType());
        Refresh();
    }

    public void OpenStorePanel()
    {
        //有未处理事件时不能继续
        if (GC.EC.UnfinishedEvents.Count > 0)
            return;
        StorePanel.SetWndState(true);
    }

    //添加从人力资源部获得的招聘
    public void AddHireTypes(HireType ht)
    {
        HireTypes.Add(ht);
        //HireRefreshButton.interactable = true;
        Text_HireButtonText.color = Color.red;
    }

    //刷新招聘
    public void Refresh()
    {
        if (HireTypes.Count > 0)
        {
            CurrentHireNum = 0;
            if(ExtraHireOption > 2)
            {
                HireTypes[0].HireNum += 2;
                ExtraHireOption -= 2;
            }
            else
            {
                HireTypes[0].HireNum += ExtraHireOption;
                ExtraHireOption = 0;
            }

            for (int i = 0; i < 5; i++)
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

                if(i > HireTypes[0].HireNum - 1)
                {
                    HireInfos[i].gameObject.SetActive(false);
                }
            }
            HireTypes.RemoveAt(0);
            if (HireTypes.Count < 1)
                HireRefreshButton.interactable = false;
        }
        //随机一下是出现建筑还是卡牌
        for (int i = 0; i < 3; i++)
        {
            if (Random.Range(0.0f, 1.0f) < 0.5f)
            {
                DepInfos[i].SetInfo();
                DepInfos[i].gameObject.SetActive(true);
                CardInfos[i].gameObject.SetActive(false);
            }
            else
            {
                CardInfos[i].CurrentCard = CWCard.CWCardData[Random.Range(0, CWCard.CWCardData.Count)].Clone();
                CardInfos[i].UpdateUI();
                CardInfos[i].gameObject.SetActive(true);
                DepInfos[i].gameObject.SetActive(false);
            }
        }
        Text_Hire.color = Color.red;
        Text_Dep.color = Color.red;
    }

    //(Hire)招聘后信息转移和创建信息面板
    public void SetInfoPanel()
    {
        GC.CurrentEmpInfo.HireButton.interactable = false;

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
        }
        GC.CurrentEmpInfo.PerksInfo.Clear();

        //复制战略
        for (int i = 0; i < GC.CurrentEmpInfo.StrategiesInfo.Count; i++)
        {
            GC.CurrentEmpInfo.StrategiesInfo[i].transform.parent = ED.StrategyContent;
            ED.StrategiesInfo.Add(GC.CurrentEmpInfo.StrategiesInfo[i]);
        }
        GC.CurrentEmpInfo.StrategiesInfo.Clear();
        //创建招聘历史
        if (GC.CC.CEO != null)
            GC.CC.CEO.InfoDetail.AddHistory("招聘了" + ED.emp.Name);
        //HideOptions();
        Text_Hire.color = Color.black;
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
        Text_Dep.color = Color.black;
    }
}
