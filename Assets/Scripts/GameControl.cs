using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameControl : MonoBehaviour
{
    public static GameControl Instance;
    [HideInInspector] public int Salary, Income, BuildingPay, MobilizeExtraMent = 0, ManageExtra, TimeMultiply = 1, WorkEndEmpCount = 0;
    [HideInInspector] public float EfficiencyExtraNormal = 0, EfficiencyExtraScience = 0, ResearchSuccessRateExtra = 0, ExtrafailRate = 0, HireEfficiencyExtra = 1.0f,
        HRBuildingMentalityExtra = 1.0f, BuildingSkillSuccessExtra = 0, BuildingMaintenanceCostRate = 1.0f;
    public int SelectMode = 1; //1员工招聘时部门选择 2员工移动时部门选择 3部门的高管办公室选择 4发动动员技能时员工选择 
    //5发动建筑技能时员工选择 6CEO技能员工/部门选择 7选择两个员工发动动员技能 8普通办公室的上级(高管办公室)选择
    public int Money = 1000, CEOSkillNum = 0, DoubleMobilizeCost = 0, MeetingBlockTime = 0;
    public bool ForceTimePause = false;
    public int Stamina
    {
        get
        {
            if (CurrentEmployees.Count > 0)
                return CurrentEmployees[0].Stamina;
            else
                return 100;
        }
        set
        {
            CurrentEmployees[0].Stamina = value;
            Text_Stamina.text = "体力:" + CurrentEmployees[0].Stamina;
        }
    }
    public int Mentality
    {
        get
        {
            if (CurrentEmployees.Count > 0)
                return CurrentEmployees[0].Mentality;
            else
                return 100;
        }
        set
        {
            CurrentEmployees[0].Mentality = value;
            Text_Mentality.text = "心力:" + CurrentEmployees[0].Mentality;
        }
    }
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

    [HideInInspector] public EmpInfo CurrentEmpInfo, CurrentEmpInfo2;//2主要用于需要两个员工的动员技能
    [HideInInspector] public DepControl CurrentDep, SelectedDep;
    [HideInInspector] public OfficeControl CurrentOffice;
    public EmpEntity EmpEntityPrefab;
    public PerkInfo PerkInfoPrefab;
    public SkillInfo SkillInfoPrefab;
    public DepControl DepPrefab, LabPrefab, HRDepPrefab;
    public OfficeControl OfficePrefab;
    public DepSelect DepSelectButtonPrefab, OfficeSelectButtonPrefab;
    public RelationInfo RelationInfoPrefab;
    public Text HistoryTextPrefab, Text_EmpSelectTip;
    public HireControl HC;
    public BuildingManage BM;
    public ProduceControl PC, PC2;
    public ProductControl PrC;
    public StrategyControl StrC;
    public FOEControl foeControl;
    public EventControl EC;
    public CEOControl CC;
    public Transform HireContent, EmpPanelContent, DepContent, DepSelectContent, TotalEmpContent, StandbyContent, EmpDetailContent, MessageContent, SRateDetailContent;
    public InfoPanel infoPanel;
    public GameObject DepSelectPanel, StandbyButton, MessagePrefab, CEOSkillPanel, EmpTrainingPanel, GameOverPanel, OfficeModeSelectPanel, OfficeModeHireOptionButton;
    public Text Text_Time, Text_TechResource, Text_MarketResource, Text_ProductResource, Text_Money, Text_Stamina, Text_Mentality, Text_Morale;
    public SkillControl SC;
    [HideInInspector] public UnityEvent DailyEvent, WeeklyEvent, MonthlyEvent, HourEvent, YearEvent;

    public Button[] CEOSkillButton = new Button[5];
    public Text[] Text_CEOSkillCD = new Text[5];
    public List<DepControl> CurrentDeps = new List<DepControl>();
    public List<OfficeControl> CurrentOffices = new List<OfficeControl>();
    public List<Employee> CurrentEmployees = new List<Employee>();
    public int[] FinishedTask = new int[10];//0程序迭代 1技术研发 2可行性调研 3公关谈判 4营销文案 5资源拓展 6原型图 7产品研究 8用户访谈 9已删除

    Animator Anim;

    int Year = 1, Month = 1, Week = 1, Day = 1, Hour = 1, morale = 50, SpecialEventCount = 0;
    float Timer;
    bool TimePause = false; //现在仅用来判断是否处于下班状态，用于其他功能时需检查WorkEndCheck()和WeekStart

    int[] CEOSkillCD = new int[5];

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Anim = this.gameObject.GetComponent<Animator>();
        SpecialEventCount = Random.Range(1, 6);//随机一段时间发生影响产品的事件
        HourEvent.AddListener(GCTimePass);
    }

    private void Update()
    {       
        if (TimePause == false && ForceTimePause == false)
            Timer += Time.deltaTime * TimeMultiply;
        if(Timer >= 10)
        {
            Timer = 0;
            HourPass();
        }
        if (Input.GetKeyDown(KeyCode.I))
            Money += 1000;
        if (Money < 0)
            GameOverPanel.SetActive(true);
        Text_Money.text = "金钱:" + Money +"\n" 
                        + "    " + (Income - Salary - (int)(BuildingPay * BuildingMaintenanceCostRate)) + "/月";
    }

    void HourPass()
    {
        Hour += 1;
        HourEvent.Invoke();
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
        PrC.UserChange();
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
            Money = Money + Income - Salary - (int)(BuildingPay * BuildingMaintenanceCostRate);
            MonthlyEvent.Invoke();
            for (int i = 0; i < CurrentDeps.Count; i++)
            {
                CurrentDeps[i].FailCheck();
            }


        }
        if (Month > 12)
        {
            YearEvent.Invoke();
            Year += 1;
            Month = 1;
            SpecialEventCount -= 1;
            if(SpecialEventCount == 0)
            {
                SpecialEventCount = Random.Range(1, 6);
                PrC.StartSpecialEvent();
            }
            for (int i = 0; i < CurrentEmployees.Count; i++)
                CurrentEmployees[i].Age += 1;
        }
        Text_Time.text = "年" + Year + " 月" + Month + " 周" + Week + " 时" + Hour;
    }

    void StartWorkEnd()
    {
        Anim.SetTrigger("FadeIn");
        if (CurrentEmployees.Count > 0)
        {
            TimePause = true;
            WorkEndEmpCount = CurrentEmployees.Count;
            for (int i = 0; i < CurrentEmployees.Count; i++)
            {
                CurrentEmployees[i].InfoDetail.Entity.WorkEnd();
            }
        }
        //else

        //start
        TimePause = false;
        for (int i = 0; i < CurrentEmployees.Count; i++)
        {
            CurrentEmployees[i].InfoDetail.Entity.WorkStart();
        }
    }

    public void WorkEndCheck()
    {
        //WorkEndEmpCount -= 1;
        //if(WorkEndEmpCount <= 0 && TimePause == true)
        //{
        //    Anim.SetTrigger("FadeIn");
        //}
        Anim.SetTrigger("FadeIn");
    }

    public void WorkStart()
    {
        //TimePause = false;
        //for (int i = 0; i < CurrentEmployees.Count; i++)
        //{
        //    CurrentEmployees[i].InfoDetail.Entity.WorkStart();
        //}
    }

    public DepControl CreateDep(int type, Building b)
    {
        DepControl newDep;
        if (type != 10)
            newDep = Instantiate(DepPrefab, this.transform);
        //else if(type == 4)
        //    newDep = Instantiate(LabPrefab, this.transform);
        else
            newDep = Instantiate(HRDepPrefab, this.transform);

        newDep.EmpPanel.parent = EmpPanelContent;
        if (newDep.SRateDetailPanel != null)
            newDep.SRateDetailPanel.parent = SRateDetailContent;
        if (newDep.LabPanel != null)
            newDep.LabPanel.parent = EmpPanelContent;
        newDep.transform.parent = DepContent;
        newDep.building = b;

        //部门命名
        string newDepName = "";
        if (type == 1)
        {
            newDep.EmpLimit = 4;
            newDepName = "技术部";
            newDep.building.effectValue = 1;
            newDep.ProducePointLimit = 16;
        }
        else if (type == 2)
        {
            newDep.EmpLimit = 4;
            newDepName = "市场部";
            newDep.building.effectValue = 2;
            newDep.ProducePointLimit = 16;
        }
        else if (type == 3)
        {
            newDep.EmpLimit = 4;
            newDepName = "产品部";
            newDep.building.effectValue = 3;
            newDep.ProducePointLimit = 16;
        }
        else if (type == 4)
        {            
            newDep.EmpLimit = 2;
            newDepName = "公关营销部";
            newDep.building.effectValue = 2;
            newDep.ProducePointLimit = 16;
        }
        else if (type == 5)
        {
            newDep.type = EmpType.Science;
            newDep.EmpLimit = 2;
            newDepName = "研发部";
            newDep.building.effectValue = 1;
            newDep.ProducePointLimit = 16;
        }
        else if (type == 6)
        {
            newDep.EmpLimit = 2;
            newDepName = "智库小组";
            newDep.building.effectValue = 11;
            newDep.ProducePointLimit = 48;
        }
        else if (type == 7)
        {
            newDep.EmpLimit = 2;
            newDepName = "人力资源部";
            newDep.building.effectValue = 8;
            newDep.ProducePointLimit = 48;
        }
        else if (type == 8)
        {
            newDep.EmpLimit = 2;
            newDepName = "心理咨询室";
            newDep.building.effectValue = 8;
            newDep.ProducePointLimit = 24;
        }
        else if (type == 9)
        {
            newDep.EmpLimit = 2;
            newDepName = "财务部";
            newDep.building.effectValue = 9;
            newDep.ProducePointLimit = 24;
        }
        else if (type == 10)
        {
            newDep.EmpLimit = 4;
            newDepName = "体能研究室";
            newDep.building.effectValue = 10;
            newDep.ProducePointLimit = 48;
        }
        else if (type == 11)
        {
            newDep.EmpLimit = 1;
            newDepName = "按摩房";
            newDep.building.effectValue = 6;
            newDep.ProducePointLimit = 24;
        }
        else if (type == 12)
        {
            newDep.EmpLimit = 2;
            newDepName = "健身房";
            newDep.building.effectValue = 6;
            newDep.ProducePointLimit = 24;
        }
        else if (type == 13)
        {
            newDep.EmpLimit = 1;
            newDepName = "宣传中心";
            newDep.building.effectValue = 2;
            newDep.ProducePointLimit = 24;
        }
        else if (type == 14)
        {
            newDep.EmpLimit = 1;
            newDepName = "科技工作坊";
            newDep.building.effectValue = 1;
            newDep.ProducePointLimit = 24;
        }
        else if (type == 15)
        {
            newDep.EmpLimit = 1;
            newDepName = "绩效考评中心";
            newDep.building.effectValue = 4;
            newDep.ProducePointLimit = 24;
        }
        else if (type == 16)
        {
            newDep.EmpLimit = 1;
            newDepName = "员工休息室";
            newDep.building.effectValue = 5;
            newDep.ProducePointLimit = 24;
        }
        else if (type == 17)
        {
            newDep.EmpLimit = 2;
            newDepName = "人文沙龙";
            newDep.building.effectValue = 8;
            newDep.ProducePointLimit = 48;
        }
        else if (type == 18)
        {
            newDep.EmpLimit = 2;
            newDepName = "兴趣社团";
            newDep.building.effectValue = 8;
            newDep.ProducePointLimit = 32;
        }
        else if (type == 19)
        {
            newDep.EmpLimit = 2;
            newDepName = "电子科技展";
            newDep.building.effectValue = 1;
            newDep.ProducePointLimit = 48;
        }
        else if (type == 20)
        {
            newDep.EmpLimit = 1;
            newDepName = "冥想室";
            newDep.building.effectValue = 5;
            newDep.ProducePointLimit = 48;
        }
        else if (type == 21)
        {
            newDep.EmpLimit = 1;
            newDepName = "特别秘书处";
            newDep.building.effectValue = 12;
            newDep.ProducePointLimit = 48;
        }
        else if (type == 22)
        {
            newDep.EmpLimit = 1;
            newDepName = "成人再教育所";
            newDep.building.effectValue = 13;
            newDep.ProducePointLimit = 96;
        }
        else if (type == 23)
        {
            newDep.EmpLimit = 1;
            newDepName = "职业再发展中心";
            newDep.building.effectValue = 12;
            newDep.ProducePointLimit = 48;
        }
        else if (type == 24)
        {
            newDep.EmpLimit = 3;
            newDepName = "中央监控室";
            newDep.building.effectValue = 15;
            newDep.ProducePointLimit = 48;
        }
        else if (type == 25)
        {
            newDep.EmpLimit = 2;
            newDepName = "谍战中心";
            newDep.building.effectValue = 12;
            newDep.ProducePointLimit = 96;
        }

        int num = 1;
        for(int i = 0; i < CurrentDeps.Count; i++)
        {
            if (CurrentDeps[i].type == newDep.type)
                num += 1;
        }
        newDep.Text_DepName.text = newDepName + num;
        newDep.GC = this;
        HourEvent.AddListener(newDep.Produce);

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

        if (newOffice.SRateDetailPanel != null)
            newOffice.SRateDetailPanel.parent = SRateDetailContent;
        return newOffice;
    }

    //招募的部门选择
    public void ShowDepSelectPanel(EmpType type)
    {
        DepSelectPanel.SetActive(true);
        StandbyButton.SetActive(true);
        for(int i = 0; i < CurrentDeps.Count; i++)
        {
            if (CurrentDeps[i].CheckEmpNum() == false)
                CurrentDeps[i].DS.gameObject.SetActive(false);
            else
                CurrentDeps[i].DS.gameObject.SetActive(true);
        }
        for (int i = 0; i < CurrentOffices.Count; i++)
        {
            if (CurrentOffices[i].CurrentManager != null)
                CurrentOffices[i].DS.gameObject.SetActive(false);
            else
                CurrentOffices[i].DS.gameObject.SetActive(true);
        }

    }
    //移动的部门选择
    public void ShowDepSelectPanel(Employee emp)
    {
        DepSelectPanel.SetActive(true);
        StandbyButton.SetActive(true);
        for (int i = 0; i < CurrentDeps.Count; i++)
        {
            if (CurrentDeps[i] == emp.CurrentDep || CurrentDeps[i].CheckEmpNum() == false)
                CurrentDeps[i].DS.gameObject.SetActive(false);
            else
                CurrentDeps[i].DS.gameObject.SetActive(true);
        }
        for (int i = 0; i < CurrentOffices.Count; i++)
        {
            //（旧）用来隐藏CEO办公室
            //if (CurrentOffices[i].DS.gameObject.tag == "HideSelect")
            //    CurrentOffices[i].DS.gameObject.SetActive(false);
            if (CurrentOffices[i].CurrentManager != null)
                CurrentOffices[i].DS.gameObject.SetActive(false);
            else
                CurrentOffices[i].DS.gameObject.SetActive(true);
        }
    }
    //部门领导选择
    public void ShowDepSelectPanel(DepControl dep)
    {
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
    //办公室领导选择
    public void ShowDepSelectPanel(OfficeControl office)
    {
        DepSelectPanel.SetActive(true);
        StandbyButton.SetActive(false);
        for (int i = 0; i < CurrentDeps.Count; i++)
        {
            CurrentDeps[i].DS.gameObject.SetActive(false);
        }
        for (int i = 0; i < CurrentOffices.Count; i++)
        {
            if (office.InRangeOffices.Contains(CurrentOffices[i]))
                CurrentOffices[i].DS.gameObject.SetActive(true);
            else
                CurrentOffices[i].DS.gameObject.SetActive(false);
        }
    }
    //显示所有相关部门
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
    //显示所有相关办公室
    public void ShowDepSelectPanel(List<OfficeControl> offices)
    {
        DepSelectPanel.SetActive(true);
        StandbyButton.SetActive(false);
        for (int i = 0; i < CurrentDeps.Count; i++)
        {
            CurrentDeps[i].DS.gameObject.SetActive(false);
        }
        for (int i = 0; i < CurrentOffices.Count; i++)
        {
            if (offices.Contains(CurrentOffices[i]) == true)
                CurrentOffices[i].DS.gameObject.SetActive(true);
            else
                CurrentOffices[i].DS.gameObject.SetActive(false);
        }
    }


    //员工管理 SelectMode 1雇佣 2移动 3指定高管（办公室）5指定部门发动建筑特效
    //InfoB常驻Total面板

    //将移动或雇佣的员工放入待命室
    public void SelectDep()
    {
        DepSelectPanel.SetActive(false);
        if (SelectMode == 1)
        {
            //还需要重新计算工资
            Salary += CurrentEmpInfo.CalcSalary();
            HC.SetInfoPanel();
            CurrentEmpInfo.emp.InfoA.transform.parent = StandbyContent;
        }
        else if (SelectMode == 2)
        {
            if(CurrentEmpInfo.emp.CurrentOffice != null && (CurrentEmpInfo.emp.CurrentOffice.building.Type == BuildingType.CEO办公室 || CurrentEmpInfo.emp.CurrentOffice.building.Type == BuildingType.高管办公室))
            {
                //离任高管投票检测
                if (EC.ManagerVoteCheck(CurrentEmpInfo.emp, true) == false)
                    return;
            }
            if (CurrentEmpInfo == CurrentEmpInfo.emp.InfoB)
                CurrentEmpInfo = CurrentEmpInfo.emp.InfoA;
            CurrentEmpInfo.transform.parent = StandbyContent;
            ResetOldAssignment();
            //CurrentEmpInfo.DetailInfo.Entity.FindWorkPos();
        }
    }

    //将移动或雇佣的员工放入特定办公室 + 确定部门(办公室)领导者 + CEO技能发动
    public void SelectDep(OfficeControl office)
    {
        DepSelectPanel.SetActive(false);
        if (office.CurrentManager != null && SelectMode < 3)
        {
            //这部分可能已经不需要了，因为筛选DepSelect按钮时已经剔除了有高管的办公室
            office.CurrentManager.InfoDetail.TempDestroyStrategy();
            office.CurrentManager.CurrentOffice = null;
            //office.CurrentManager.InfoDetail.Entity.FindWorkPos();
        }
        if (SelectMode == 1)
        {
            //还需要重新计算工资
            Salary += CurrentEmpInfo.CalcSalary();
            HC.SetInfoPanel();
            CurrentEmpInfo.emp.InfoA.transform.parent = StandbyContent;
            if (office.building.Type == BuildingType.CEO办公室 || office.building.Type == BuildingType.高管办公室)
            {
                //就任高管投票检测
                if (EC.ManagerVoteCheck(CurrentEmpInfo.emp) == false)
                {
                    return;
                }
            }
            office.CurrentManager = CurrentEmpInfo.emp;
            CurrentEmpInfo.emp.CurrentOffice = office;
            office.SetOfficeStatus();
        }
        else if (SelectMode == 2)
        {
            if (office.building.Type == BuildingType.CEO办公室 || office.building.Type == BuildingType.高管办公室)
            {
                //就任高管投票检测
                if (EC.ManagerVoteCheck(CurrentEmpInfo.emp) == false)
                    return;
            }
            else if (CurrentEmpInfo.emp.CurrentOffice != null && (CurrentEmpInfo.emp.CurrentOffice.building.Type == BuildingType.CEO办公室 || CurrentEmpInfo.emp.CurrentOffice.building.Type == BuildingType.高管办公室))
            {
                //离任高管投票检测
                if (EC.ManagerVoteCheck(CurrentEmpInfo.emp, true) == false)
                    return;
            }
            if (CurrentEmpInfo == CurrentEmpInfo.emp.InfoB)
                CurrentEmpInfo = CurrentEmpInfo.emp.InfoA;
            CurrentEmpInfo.transform.parent = StandbyContent;
            ResetOldAssignment();
            CurrentEmpInfo.emp.CurrentOffice = office;
            office.CurrentManager = CurrentEmpInfo.emp;
            office.SetOfficeStatus();
            //CurrentEmpInfo.DetailInfo.Entity.FindWorkPos();
        }
        //确定部门领导者
        else if (SelectMode == 3)
        {
            if(CurrentDep.CommandingOffice != null)
            {
                CurrentDep.CommandingOffice.ControledDeps.Remove(CurrentDep);
                CurrentDep.CommandingOffice.CheckManage();
                //清除前高管技能预设
                if (CurrentDep.CommandingOffice.CurrentManager != null)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        if (CurrentDep.DSkillSetA[j] != null && CurrentDep.DSkillSetA[j].TargetEmp == CurrentDep.CommandingOffice.CurrentManager)
                            CurrentDep.DSkillSetA[j] = null;

                        if (CurrentDep.DSkillSetB[j] != null && CurrentDep.DSkillSetB[j].TargetEmp == CurrentDep.CommandingOffice.CurrentManager)
                            CurrentDep.DSkillSetB[j] = null;

                        if (CurrentDep.DSkillSetC[j] != null && CurrentDep.DSkillSetC[j].TargetEmp == CurrentDep.CommandingOffice.CurrentManager)
                            CurrentDep.DSkillSetC[j] = null;
                    }
                }
            }
            CurrentDep.CommandingOffice = office;
            CurrentDep.Text_Office.text = "由 " + office.Text_OfficeName.text + " 管理";
            office.ControledDeps.Add(CurrentDep);
            office.CheckManage();
        }
        //确定办公室的部门领导者
        else if (SelectMode == 8)
        {
            if (CurrentOffice.CommandingOffice != null)
            {
                CurrentOffice.CommandingOffice.ControledOffices.Remove(CurrentOffice);
                CurrentOffice.CommandingOffice.CheckManage();
            }
            CurrentOffice.CommandingOffice = office;
            //CurrentDep.Text_Office.text = "由 " + office.Text_OfficeName.text + " 管理";
            office.ControledOffices.Add(CurrentOffice);
            office.CheckManage();
        }
        //CEO技能
        else if (SelectMode == 6)
        {
            if (CEOSkillNum == 3)
            {
                office.Progress = 100;
                office.ActiveButton.interactable = true;
                office.Text_Progress.text = "激活进度:" + office.Progress + "%";
            }
            CEOSkillConfirm();
        }
    }

    //将移动或雇佣的员工放入特定部门 + 选择部门发动建筑特效 + CEO技能发动
    public void SelectDep(DepControl depControl)
    {
        if(SelectMode == 1)
        {
            //还需要重新计算工资
            Salary += CurrentEmpInfo.CalcSalary();
            depControl.CurrentEmps.Add(CurrentEmpInfo.emp);
            depControl.UpdateUI();
            CurrentEmpInfo.emp.CurrentDep = depControl;
            HC.SetInfoPanel();
            CurrentEmpInfo.emp.InfoA.transform.parent = depControl.EmpContent;
        }
        else if(SelectMode == 2)
        {
            if (CurrentEmpInfo.emp.CurrentOffice != null && (CurrentEmpInfo.emp.CurrentOffice.building.Type == BuildingType.CEO办公室 || CurrentEmpInfo.emp.CurrentOffice.building.Type == BuildingType.高管办公室))
            {
                //离任高管投票检测
                if (EC.ManagerVoteCheck(CurrentEmpInfo.emp, true) == false)
                    return;
            }
            if (CurrentEmpInfo == CurrentEmpInfo.emp.InfoB)
                CurrentEmpInfo = CurrentEmpInfo.emp.InfoA;
            CurrentEmpInfo.transform.parent = depControl.EmpContent;

            ResetOldAssignment();
            //修改新部门生产力显示
            CurrentEmpInfo.emp.CurrentDep = depControl;
            depControl.CurrentEmps.Add(CurrentEmpInfo.emp);
            depControl.UpdateUI();
            //CurrentEmpInfo.DetailInfo.Entity.FindWorkPos();
        }
        //选择部门发动建筑特效
        else if(SelectMode == 5)
        {
            CurrentDep = depControl;
            CurrentOffice.BuildingActive();
        }
        //CEO技能
        else if (SelectMode == 6)
        {
            if (CEOSkillNum == 1)
                new ProduceBuff(0.2f, depControl, 16);
            else if (CEOSkillNum == 2)
                depControl.SpTime += 16;
            CEOSkillConfirm();
        }
        DepSelectPanel.SetActive(false);
    }

    public void ResetOldAssignment()
    {
        if (CurrentEmpInfo.emp.CurrentDep != null)
        {
            CurrentEmpInfo.ClearSkillPreset();
            CurrentEmpInfo.emp.CurrentDep.CurrentEmps.Remove(CurrentEmpInfo.emp);
            //修改生产力显示
            CurrentEmpInfo.emp.CurrentDep.UpdateUI();
            CurrentEmpInfo.emp.CurrentDep = null;
        }
        if (CurrentEmpInfo.emp.CurrentOffice != null)
        {
            CurrentEmpInfo.ClearSkillPreset();
            if (CurrentEmpInfo.emp.CurrentOffice.building.Type == BuildingType.高管办公室 || CurrentEmpInfo.emp.CurrentOffice.building.Type == BuildingType.CEO办公室)
                CurrentEmpInfo.emp.InfoDetail.TempDestroyStrategy();
            CurrentEmpInfo.emp.CurrentOffice.CurrentManager = null;
            CurrentEmpInfo.emp.CurrentOffice.SetOfficeStatus();
            CurrentEmpInfo.emp.CurrentOffice.CheckManage();
            CurrentEmpInfo.emp.CurrentOffice = null;
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

    public void CreateMessage(string content)
    {
        Text T = Instantiate(MessagePrefab, MessageContent).GetComponentInChildren<Text>();
        T.text = content;
    }

    public void SetTimeMultiply(int value)
    {
        TimeMultiply = value;
    }

    public void SetCEOSkill(int num)
    {
        if(num == 1 && Stamina >= 20)
        {
            CEOSkillNum = 1;
            CEOSkillPanel.SetActive(false);
            ShowDepSelectPanel(CurrentDeps);
            SelectMode = 6;
        }
        else if (num == 2 && Stamina >= 30)
        {
            CEOSkillNum = 2;
            CEOSkillPanel.SetActive(false);
            ShowDepSelectPanel(CurrentDeps);
            SelectMode = 6;
        }
        else if (num == 3 && Stamina >= 50)
        {
            //CEOSkillNum = 3;
            //CEOSkillPanel.SetActive(false);
            //List<OfficeControl> TempOffices = new List<OfficeControl>();
            //for(int i = 0; i < CurrentOffices.Count; i++)
            //{
            //    BuildingType T = CurrentOffices[i].building.Type;
            //    if (T == BuildingType.目标修正小组 || T == BuildingType.档案管理室 || T == BuildingType.效能研究室 || T == BuildingType.财务部 
            //        || T == BuildingType.战略咨询部B || T == BuildingType.精确标准委员会)
            //    {
            //        TempOffices.Add(CurrentOffices[i]);
            //    }
            //}
            //ShowDepSelectPanel(TempOffices);
            //SelectMode = 6;
        }
        else if(num == 4 && Stamina >= 20)
        {
            SelectMode = 6;
            CEOSkillNum = 4;
            CEOSkillPanel.SetActive(false);
            TotalEmpContent.parent.parent.gameObject.SetActive(true);
        }
        else if (num == 5 && Stamina >= 50)
        {
            SelectMode = 6;
            CEOSkillNum = 5;
            CEOSkillPanel.SetActive(false);
            TotalEmpContent.parent.parent.gameObject.SetActive(true);            
        }
        else
        {
            SelectMode = 6;
            CEOSkillNum = num;
            CEOSkillPanel.SetActive(false);
            TotalEmpContent.parent.parent.gameObject.SetActive(true);
        }
    }
    public void CEOSkillConfirm()
    {       
        if (CEOSkillNum == 1)
        {
            Stamina -= 20;
            CEOSkillCD[0] = 16;
        }
        else if (CEOSkillNum == 2)
        {
            Stamina -= 30;
            CEOSkillCD[1] = 24;
        }
        else if (CEOSkillNum == 3)
        {
            Stamina -= 50;
            CEOSkillCD[2] = 32;
        }
        else if (CEOSkillNum == 4)
        {
            Stamina -= 20;
            CEOSkillCD[3] = 8;
        }
        else if (CEOSkillNum == 5)
        {
            Stamina -= 50;
            CEOSkillCD[4] = 32;
        }
        CEOSkillButton[CEOSkillNum - 1].interactable = false;
        Text_CEOSkillCD[CEOSkillNum - 1].gameObject.SetActive(true);
        Text_CEOSkillCD[CEOSkillNum - 1].text = "CD:" + CEOSkillCD[CEOSkillNum - 1] + "时";
        CEOSkillNum = 0;
        SelectMode = 0;
    }

    public void TrainEmp(int type)
    {
        CurrentEmpInfo.emp.GainExp(300, type);
    }
    void GCTimePass()
    {
        //CEO技能CD
        for (int i = 0; i < CEOSkillCD.Length; i++)
        {
            if (CEOSkillCD[i] > 0)
            {
                CEOSkillCD[i] -= 1;
                if (CEOSkillCD[i] == 0)
                {
                    CEOSkillButton[i].interactable = true;
                    Text_CEOSkillCD[i].gameObject.SetActive(false);
                }
            }
            Text_CEOSkillCD[i].text = "CD:" + CEOSkillCD[i] + "时";
        }
        //开会封锁时间
        if (MeetingBlockTime > 0)
            MeetingBlockTime -= 1;
    }

    public void ResetSelectMode()
    {
        SelectMode = 0;
        CEOSkillNum = 0;
        Text_EmpSelectTip.gameObject.SetActive(false);
    }

    public void SellTask(int num)
    {
        if (FinishedTask[num] > 0)
        {
            FinishedTask[num] -= 1;
            Money += 100;
            UpdateResourceInfo();
        }
    }

    public void SetOfficeMode(int num)
    {
        if(CurrentOffice != null)
        {
            CurrentOffice.OfficeMode = num;
            OfficeModeSelectPanel.SetActive(false);
            CurrentOffice.Progress = 100;
            CurrentOffice.SetOfficeUI();
            if (num == 1)
                CurrentOffice.Text_OfficeMode.text = "办公室模式:决策";
            else if (num == 2)
                CurrentOffice.Text_OfficeMode.text = "办公室模式:人力";
            else if (num == 3)
                CurrentOffice.Text_OfficeMode.text = "办公室模式:管理";
            else if (num == 4)
                CurrentOffice.Text_OfficeMode.text = "办公室模式:招聘";
            else if (num == 5)
                CurrentOffice.Text_OfficeMode.text = "办公室模式:部门研究";
        }
    }

    public void CancelEmpSelect()
    {
        if(SelectMode == 4)
        {
            TotalEmpContent.parent.parent.gameObject.SetActive(false);
            ResetSelectMode();
        }
        else if (SelectMode == 7)
        {
            if(CurrentEmpInfo2 == null)
            {
                TotalEmpContent.parent.parent.gameObject.SetActive(false);
                ResetSelectMode();
            }
            else
            {
                CurrentEmpInfo2 = null;
                Text_EmpSelectTip.text = "选择第一个员工";
            }
        }
    }
}
