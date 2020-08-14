using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameControl : MonoBehaviour
{
    [HideInInspector] public int Salary, Income, MobilizeExtraMent = 0, ManageExtra, TimeMultiply = 1, WorkEndEmpCount = 0;
    [HideInInspector] public float EfficiencyExtraNormal = 0, EfficiencyExtraScience = 0, ResearchSuccessRateExtra = 0, ExtrafailRate = 0;
    public int SelectMode = 1;
    public int Money = 1000, Stamina = 100, Mentality = 100;
    public int Morale
    {
        get { return morale; }
        set
        {
            morale = value;
            if (morale > 100)
                morale = 100;
            else if (morale < 0)
                morale = 0;
            Text_Morale.text = "士气:" + Morale;
        }
    }

    [HideInInspector] public EmpInfo CurrentEmpInfo;
    [HideInInspector] public DepControl CurrentDep;
    [HideInInspector] public OfficeControl CurrentOffice;
    public EmpInfo EmpInfoPrefab, EmpDetailPrefab;
    public EmpEntity EmpEntityPrefab;
    public PerkInfo PerkInfoPrefab;
    public SkillInfo SkillInfoPrefab;
    public DepControl DepPrefab, LabPrefab;
    public OfficeControl OfficePrefab;
    public DepSelect DepSelectButtonPrefab, OfficeSelectButtonPrefab;
    public RelationInfo RelationInfoPrefab;
    public Text HistoryTextPrefab;
    public BuildingManage BM;
    public ProduceControl PC;
    public ProductControl PrC;
    public StrategyControl StrC;
    public Transform HireContent, EmpPanelContent, DepContent, DepSelectContent, TotalEmpContent, StandbyContent, EmpDetailContent;
    public InfoPanel infoPanel;
    public GameObject DepSelectPanel, StandbyButton;
    public Text Text_Time, Text_TechResource, Text_MarketResource, Text_ProductResource, Text_Money, Text_Stamina, Text_Mentality, Text_Morale;
    public SkillControl SC;
    [HideInInspector] public UnityEvent DailyEvent, WeeklyEvent, MonthlyEvent, HourEvent;

    public EmpInfo[] HireInfos = new EmpInfo[5];
    public List<DepControl> CurrentDeps = new List<DepControl>();
    public List<OfficeControl> CurrentOffices = new List<OfficeControl>();
    public List<Employee> CurrentEmployees = new List<Employee>();
    public int[] FinishedTask = new int[9];

    Animator Anim;

    int Year = 1, Month = 1, Week = 1, Day = 1, Hour = 1, morale = 100;
    float Timer;
    bool TimePause = false; //现在仅用来判断是否处于下班状态，用于其他功能时需检查WorkEndCheck()和WeekStart

    private void Start()
    {
        Anim = this.gameObject.GetComponent<Animator>();
    }

    private void Update()
    {
        if (TimePause == false)
            Timer += Time.deltaTime * TimeMultiply;
        if(Timer >= 10)
        {
            Timer = 0;
            HourPass();
        }
        Text_Money.text = "金钱:" + Money +"\n" 
                        + "    " + (Income - Salary) + "/月";
        Text_Stamina.text = "体力:" + Stamina;
        Text_Mentality.text = "心力:" + Mentality;
    }

    void HourPass()
    {
        Hour += 1;
        HourEvent.Invoke();
        for(int i = 0; i < CurrentDeps.Count; i++)
        {
            CurrentDeps[i].Produce();
        }
        if (Hour > 8)
        {
            //for(int i = 0; i < CurrentDeps.Count; i++)
            //{
            //    CurrentDeps[i].OneHourPass();
            //}
            Hour = 8;
            StartWorkEnd();
        }
        //if (Day > 5) //旧的5日1周时间流
        //{
        //    Week += 1;
        //    Day = 1;
        //    WeeklyEvent.Invoke();
        //}
        //if(Week > 4)
        //{
        //    Month += 1;
        //    Week = 1;
        //    PrC.UserChange();
        //    Money = Money + Income - Salary;
        //    MonthlyEvent.Invoke();
        //    for (int i = 0; i < CurrentDeps.Count; i++)
        //    {
        //        CurrentDeps[i].FailCheck();
        //    }
        //}
        //if(Month > 12)
        //{
        //    Year += 1;
        //    Month = 1;
        //}
        //Text_Time.text = "年" + Year + " 月" + Month + " 周" + Week + " 日" + Day + " 时" + Hour;
        Text_Time.text = "年" + Year + " 月" + Month + " 周" + Week + " 时" + Hour;
    }
    public void WeekPass()
    {
        DailyEvent.Invoke();
        Hour = 1;
        Week += 1;
        WeeklyEvent.Invoke();
        //Day += 1;
        Stamina += 30;
        Mentality += 30;
        if (Stamina > 100)
            Stamina = 100;
        if (Mentality > 100)
            Mentality = 100;

        if (Week > 4)
        {
            Month += 1;
            Week = 1;
            PrC.UserChange();
            Money = Money + Income - Salary;
            MonthlyEvent.Invoke();
            for (int i = 0; i < CurrentDeps.Count; i++)
            {
                CurrentDeps[i].FailCheck();
            }
        }
        if (Month > 12)
        {
            Year += 1;
            Month = 1;
        }
        Text_Time.text = "年" + Year + " 月" + Month + " 周" + Week + " 时" + Hour;
    }

    void StartWorkEnd()
    {
        if (CurrentEmployees.Count > 0)
        {
            TimePause = true;
            WorkEndEmpCount = CurrentEmployees.Count;
            for (int i = 0; i < CurrentEmployees.Count; i++)
            {
                CurrentEmployees[i].InfoDetail.Entity.WorkEnd();
            }
        }
        else
            Anim.SetTrigger("FadeIn");
    }

    public void WorkEndCheck()
    {
        WorkEndEmpCount -= 1;
        if(WorkEndEmpCount <= 0 && TimePause == true)
        {
            Anim.SetTrigger("FadeIn");
        }
    }

    public void WorkStart()
    {
        TimePause = false;
        for(int i = 0; i < CurrentEmployees.Count; i++)
        {
            CurrentEmployees[i].InfoDetail.Entity.WorkStart();
        }
    }

    public DepControl CreateDep(int type)
    {
        DepControl newDep;
        if (type < 4)
            newDep = Instantiate(DepPrefab, this.transform);
        else
            newDep = Instantiate(LabPrefab, this.transform);

        newDep.EmpPanel.parent = EmpPanelContent;
        if(newDep.LabPanel != null)
            newDep.LabPanel.parent = EmpPanelContent;
        newDep.transform.parent = DepContent;

        //部门命名
        string newDepName = "";
        if (type == 1)
        {
            newDep.type = EmpType.Tech;
            newDep.EmpLimit = 4;
            newDepName = "技术部门";
        }
        else if (type == 2)
        {
            newDep.type = EmpType.Market;
            newDep.EmpLimit = 4;
            newDepName = "市场部门";
        }
        else if (type == 3)
        {
            newDep.type = EmpType.Product;
            newDep.EmpLimit = 4;
            newDepName = "产品部门";
        }
        else if (type == 4)
        {
            newDep.type = EmpType.Science;
            newDep.EmpLimit = 4;
            newDepName = "研发部门";
        }
        int num = 1;
        for(int i = 0; i < CurrentDeps.Count; i++)
        {
            if (CurrentDeps[i].type == newDep.type)
                num += 1;
        }
        newDep.Text_DepName.text = newDepName + num;
        newDep.GC = this;

        //创建对应按钮
        newDep.DS = Instantiate(DepSelectButtonPrefab, DepSelectContent);
        newDep.DS.Text_DepName.text = newDep.Text_DepName.text;
        newDep.DS.DC = newDep;
        newDep.DS.GC = this;

        CurrentDeps.Add(newDep);
        return newDep;
    }

    public OfficeControl CreateOffice(Building b)
    {
        OfficeControl newOffice = Instantiate(OfficePrefab, this.transform);
        newOffice.transform.parent = DepContent;
        newOffice.building = b;
        newOffice.GC = this;
        newOffice.SetName();
        CurrentOffices.Add(newOffice);

        //创建对应按钮
        newOffice.DS = Instantiate(OfficeSelectButtonPrefab, DepSelectContent);
        newOffice.DS.Text_DepName.text = newOffice.Text_OfficeName.text;
        newOffice.DS.OC = newOffice;
        newOffice.DS.GC = this;

        return newOffice;
    }

    public void ShowDepSelectPanel(EmpType type)
    {
        //招募的部门选择
        DepSelectPanel.SetActive(true);
        StandbyButton.SetActive(true);
        for(int i = 0; i < CurrentDeps.Count; i++)
        {
            if(CurrentDeps[i].CheckEmpNum() == false)
                CurrentDeps[i].DS.gameObject.SetActive(false);
            else if (CurrentDeps[i].type != type && CurrentDeps[i].type != EmpType.Science)
                CurrentDeps[i].DS.gameObject.SetActive(false);
            else if(CurrentDeps[i].type == EmpType.Science && (type == EmpType.Operate || type == EmpType.Market))
                CurrentDeps[i].DS.gameObject.SetActive(false);
            else
                CurrentDeps[i].DS.gameObject.SetActive(true);
        }
        for (int i = 0; i < CurrentOffices.Count; i++)
        {
            if (CurrentOffices[i].DS.gameObject.tag == "HideSelect")
                CurrentOffices[i].DS.gameObject.SetActive(false);
            else
                CurrentOffices[i].DS.gameObject.SetActive(true);
        }

    }
    public void ShowDepSelectPanel(Employee emp)
    {
        //移动的部门选择
        DepSelectPanel.SetActive(true);
        StandbyButton.SetActive(true);
        for (int i = 0; i < CurrentDeps.Count; i++)
        {
            if (CurrentDeps[i] == emp.CurrentDep || CurrentDeps[i].CheckEmpNum() == false)
                CurrentDeps[i].DS.gameObject.SetActive(false);
            else if (CurrentDeps[i].type != emp.Type && CurrentDeps[i].type != EmpType.Science)
                CurrentDeps[i].DS.gameObject.SetActive(false);
            else if (CurrentDeps[i].type == EmpType.Science && (emp.Type == EmpType.Operate || emp.Type == EmpType.Market))
                CurrentDeps[i].DS.gameObject.SetActive(false);
            else
                CurrentDeps[i].DS.gameObject.SetActive(true);
        }
        for (int i = 0; i < CurrentOffices.Count; i++)
        {
            if (CurrentOffices[i].DS.gameObject.tag == "HideSelect")
                CurrentOffices[i].DS.gameObject.SetActive(false);
            else
                CurrentOffices[i].DS.gameObject.SetActive(true);
        }
    }
    public void ShowDepSelectPanel(DepControl dep)
    {
        //办公室领导选择
        DepSelectPanel.SetActive(true);
        StandbyButton.SetActive(false);
        for (int i = 0; i < CurrentDeps.Count; i++)
        {
            CurrentDeps[i].DS.gameObject.SetActive(false);
        }
            for (int i = 0; i < CurrentOffices.Count; i++)
        {
            if (dep.InRangeOffices.Contains(CurrentOffices[i]))
                CurrentOffices[i].DS.gameObject.SetActive(true);
            else
                CurrentOffices[i].DS.gameObject.SetActive(false);
        }
    }
    public void ShowDepSelectPanel(List<DepControl> deps)
    {
        DepSelectPanel.SetActive(true);
        StandbyButton.SetActive(false);
        for (int i = 0; i < CurrentDeps.Count; i++)
        {
            if(deps.Contains(CurrentDeps[i]))          
                CurrentDeps[i].DS.gameObject.SetActive(true);
            else
                CurrentDeps[i].DS.gameObject.SetActive(false);
        }
        for(int i = 0; i < CurrentOffices.Count; i++)
        {
            CurrentOffices[i].DS.gameObject.SetActive(false);
        }
    }


    //员工管理 SelectMode 1雇佣 2移动 3指定高管（办公室）5指定部门发动建筑特效
    //InfoB常驻Total面板

    //将移动或雇佣的员工放入待命室
    public void SelectDep()
    {
        if (SelectMode == 1)
        {
            //还需要重新计算工资
            Salary += CurrentEmpInfo.CalcSalary();
            SetInfoPanel();
            CurrentEmpInfo.emp.InfoA.transform.parent = StandbyContent;
        }
        else if (SelectMode == 2)
        {
            if (CurrentEmpInfo == CurrentEmpInfo.emp.InfoB)
                CurrentEmpInfo = CurrentEmpInfo.emp.InfoA;
            CurrentEmpInfo.transform.parent = StandbyContent;
            ResetOldAssignment();
            CurrentEmpInfo.DetailInfo.Entity.FindWorkPos();
        }
        DepSelectPanel.SetActive(false);
    }

    //将移动或雇佣的员工放入特定办公室 + 确定部门领导者
    public void SelectDep(OfficeControl office)
    {
        if(office.CurrentManager != null)
        {
            office.CurrentManager.CurrentOffice = null;
            office.CurrentManager.InfoDetail.Entity.FindWorkPos();
        }
        if (SelectMode == 1)
        {
            //还需要重新计算工资
            Salary += CurrentEmpInfo.CalcSalary();
            office.CurrentManager = CurrentEmpInfo.emp;
            CurrentEmpInfo.emp.CurrentOffice = office;
            SetInfoPanel();
            CurrentEmpInfo.emp.InfoA.transform.parent = StandbyContent;
            office.SetOfficeStatus();
        }
        else if (SelectMode == 2)
        {
            if (CurrentEmpInfo == CurrentEmpInfo.emp.InfoB)
                CurrentEmpInfo = CurrentEmpInfo.emp.InfoA;
            CurrentEmpInfo.transform.parent = StandbyContent;
            ResetOldAssignment();
            CurrentEmpInfo.emp.CurrentOffice = office;
            office.CurrentManager = CurrentEmpInfo.emp;
            office.SetOfficeStatus();
            CurrentEmpInfo.DetailInfo.Entity.FindWorkPos();
        }
        //确定部门领导者
        else if (SelectMode == 3)
        {
            if(CurrentDep.CommandingOffice != null)
            {
                CurrentDep.CommandingOffice.ControledDeps.Remove(CurrentDep);
                CurrentDep.CommandingOffice.CheckManage();
            }
            CurrentDep.CommandingOffice = office;
            CurrentDep.Text_Office.text = "由 " + office.Text_OfficeName.text + " 管理";
            office.ControledDeps.Add(CurrentDep);
            office.CheckManage();
        }
        DepSelectPanel.SetActive(false);
    }

    //将移动或雇佣的员工放入特定部门 + 选择部门发动建筑特效
    public void SelectDep(DepControl depControl)
    {
        if(SelectMode == 1)
        {
            //还需要重新计算工资
            Salary += CurrentEmpInfo.CalcSalary();
            depControl.CurrentEmps.Add(CurrentEmpInfo.emp);
            depControl.UpdateUI();
            CurrentEmpInfo.emp.CurrentDep = depControl;
            SetInfoPanel();
            CurrentEmpInfo.emp.InfoA.transform.parent = depControl.EmpContent;
        }
        else if(SelectMode == 2)
        {
            if (CurrentEmpInfo == CurrentEmpInfo.emp.InfoB)
                CurrentEmpInfo = CurrentEmpInfo.emp.InfoA;
            CurrentEmpInfo.transform.parent = depControl.EmpContent;

            ResetOldAssignment();
            //修改新部门生产力显示
            CurrentEmpInfo.emp.CurrentDep = depControl;
            depControl.CurrentEmps.Add(CurrentEmpInfo.emp);
            depControl.UpdateUI();
            CurrentEmpInfo.DetailInfo.Entity.FindWorkPos();
        }
        //选择部门发动建筑特效
        else if(SelectMode == 5)
        {
            CurrentDep = depControl;
            CurrentOffice.BuildingActive();
        }
        DepSelectPanel.SetActive(false);
    }

    //(Hire)招聘后信息转移
    void SetInfoPanel()
    {
        CurrentEmpInfo.HireButton.interactable = false;

        EmpInfo ED = Instantiate(EmpDetailPrefab, EmpDetailContent);
        CurrentEmpInfo.CopyStatus(ED);

        EmpInfo EI1 = Instantiate(EmpInfoPrefab, TotalEmpContent);
        CurrentEmpInfo.CopyStatus(EI1);

        EmpInfo EI2 = Instantiate(EmpInfoPrefab, TotalEmpContent);
        CurrentEmpInfo.CopyStatus(EI2);

        EI1.DetailInfo = ED;
        EI2.DetailInfo = ED;
        ED.emp.InfoA = EI1;
        ED.emp.InfoB = EI2;
        ED.emp.InfoDetail = ED;
        ED.emp.InitRelation();
        ED.SetSkillName();
        //创建员工实体
        ED.Entity = Instantiate(EmpEntityPrefab, BM.ExitPos.position, Quaternion.Euler(0, 0, 0), BM.EntityContent);
        ED.Entity.SetInfo(ED);

        //注意应放在初始化人际关系后再添加至链表
        CurrentEmployees.Add(CurrentEmpInfo.emp);
        //复制特质
        for (int i = 0; i < CurrentEmpInfo.PerksInfo.Count; i++)
        {
            CurrentEmpInfo.PerksInfo[i].CurrentPerk.AddEffect();
            CurrentEmpInfo.PerksInfo[i].transform.parent = ED.PerkContent;
        }
        ED.PerksInfo = CurrentEmpInfo.PerksInfo;
        //复制能力
        for(int i = 0; i < CurrentEmpInfo.SkillsInfo.Count; i++)
        {
            CurrentEmpInfo.SkillsInfo[i].transform.parent = ED.SkillContent;
        }
        ED.SkillsInfo = CurrentEmpInfo.SkillsInfo;
        //复制战略
        for (int i = 0; i < CurrentEmpInfo.StrategiesInfo.Count; i++)
        {
            CurrentEmpInfo.StrategiesInfo[i].transform.parent = ED.StrategyContent;
        }
        ED.StrategiesInfo = CurrentEmpInfo.StrategiesInfo;
    }

    void ResetOldAssignment()
    {
        if (CurrentEmpInfo.emp.CurrentDep != null)
        {
            CurrentEmpInfo.emp.CurrentDep.CurrentEmps.Remove(CurrentEmpInfo.emp);
            //修改生产力显示
            CurrentEmpInfo.emp.CurrentDep.UpdateUI();
            CurrentEmpInfo.emp.CurrentDep = null;
        }
        if (CurrentEmpInfo.emp.CurrentOffice != null)
        {
            CurrentEmpInfo.emp.CurrentOffice.CurrentManager = null;
            CurrentEmpInfo.emp.CurrentOffice.SetOfficeStatus();
            CurrentEmpInfo.emp.CurrentOffice.CheckManage();
            CurrentEmpInfo.emp.CurrentOffice = null;
        }
    }

    public void RefreshHire(int type)
    {
        if (Stamina > 30)
        {
            Stamina -= 30;
            EmpType EType;
            if (type == 1)
                EType = EmpType.Tech;
            else if (type == 2)
                EType = EmpType.Market;
            else if (type == 3)
                EType = EmpType.Product;
            else
                EType = EmpType.Operate;

            for (int i = 0; i < 5; i++)
            {
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
                HireInfos[i].SkillsInfo.Clear();
                HireInfos[i].StrategiesInfo.Clear();
                HireInfos[i].CreateEmp(EType);
            }
        }
    }

    public void UpdateResourceInfo()
    {
        int[] C = FinishedTask;
        Text_TechResource.text = "程序迭代: " + C[0] + "\n" +
            "技术研发: " + C[1] + "\n" +
            "可行性调研: " + C[2] + "\n";

        Text_MarketResource.text = "公关谈判: " + C[3] + "\n" +
            "营销文案: " + C[4] + "\n" +
            "资源拓展: " + C[5] + "\n";

        Text_ProductResource.text = "原型图: " + C[6] + "\n" +
           "产品研究: " + C[7] + "\n" +
           "用户访谈: " + C[8] + "\n";
    }

    public void SetTimeMultiply(int value)
    {
        TimeMultiply = value;
    }
}
