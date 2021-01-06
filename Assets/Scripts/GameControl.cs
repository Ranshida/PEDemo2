using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameControl : MonoBehaviour
{
    public static GameControl Instance;
    [HideInInspector] public int Salary, Income, BuildingPay, MobilizeExtraMent = 0, ExtraDice = 0, TimeMultiply = 1, WorkEndEmpCount = 0;
    [HideInInspector] public float EfficiencyExtraNormal = 0, EfficiencyExtraScience = 0, ExtrafailRate = 0, TotalSalaryMultiply = 1.0f,
        HRBuildingMentalityExtra = 1.0f, BuildingSkillSuccessExtra = 0, TotalBuildingPayMultiply = 1.0f, HireSuccessExtra = 0, 
        BaseDepExtraSuccessRate = 0;
    public int SelectMode = 1; //1员工招聘时部门选择 2员工移动时部门选择 3部门的高管办公室选择 4发动动员技能时员工选择 
    //5发动建筑技能时员工选择 6CEO技能员工/部门选择 7选择两个员工发动动员技能 8普通办公室的上级(高管办公室)选择  9头脑风暴的员工选择
    //10选择两个员工的CEO技能
    public int Money = 1000, CEOSkillNum = 0, DoubleMobilizeCost = 0, MeetingBlockTime = 0, MobTime = 192;
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
            Text_Morale.text = "士气:" + morale;
        }
    }

    #region 杂项变量
    [HideInInspector] public bool ProduceBuffBonus = false;//“精进”和“团结”状态的持续时间提高至4m战略效果
    [HideInInspector] public bool GymBuffBonus = false;//“健身房”大成功时获得“强化”或“铁人”状态的概率上升为80%战略效果
    [HideInInspector] public bool WorkToggle = false;//下周开始是否加班
    [HideInInspector] public bool NeutralOccupieBonus = false;//商战中立占领Buff加成
    [HideInInspector] public bool TargetOccupieBonus = false;//商战反占领Buff加成
    [HideInInspector] public bool CEOExtraVote = false;//CEO投票是否有额外加成
    [HideInInspector] public bool ResearchExtraMentality = false;//科研额外心力恢复
    [HideInInspector] public bool CEOVacation = false;
    [HideInInspector] public float ResearchExtraSuccessRate = 0;//研发业务额外成功率
    [HideInInspector] public int ResearchExtraTimeBoost = 0;//科研业务时间增益
    bool WorkOverTime = false;//是否已经处于加班状态
    #endregion

    [HideInInspector] public EmpInfo CurrentEmpInfo, CurrentEmpInfo2;//2主要用于需要两个员工的动员技能
    public DepControl CurrentDep, SelectedDep;
    [HideInInspector] public OfficeControl CurrentOffice;
    public EmpEntity EmpEntityPrefab;
    public PerkInfo PerkInfoPrefab;
    public SkillInfo SkillInfoPrefab;
    public DepControl DepPrefab, HRDepPrefab;
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
    public GameObject DepSelectPanel, StandbyButton, MessagePrefab, CEOSkillPanel, VacationPanel, GameOverPanel, OfficeModeSelectPanel,
        OfficeModeBuildingOptionButton, OfficeModeTalkOptionButton, DepModeSelectPanel, DepSkillConfirmPanel, SkillTreeSelectPanel,
        TrainingPanel;
    public Text Text_Time, Text_TechResource, Text_MarketResource, Text_MarketResource2, Text_ProductResource, Text_Money, Text_Stamina, Text_Mentality, 
        Text_Morale, Text_DepMode1, Text_DepMode2, Text_DepSkillDescribe;
    public Toggle WorkOvertimeToggle;
    public SkillControl SC;
    [HideInInspector] public UnityEvent DailyEvent, WeeklyEvent, MonthlyEvent, HourEvent, YearEvent;

    public Button[] CEOSkillButton = new Button[5];
    public Text[] Text_CEOSkillCD = new Text[5];
    public List<DepControl> CurrentDeps = new List<DepControl>();
    public List<OfficeControl> CurrentOffices = new List<OfficeControl>();
    public List<Employee> CurrentEmployees = new List<Employee>();
    public int[] FinishedTask = new int[10];//0程序迭代 1技术研发 2可行性调研 3公关谈判 4营销文案 5资源拓展 6原型图 7产品研究 8用户访谈 9已删除
    public int[] CEOSkillCD = new int[5];

    int Year = 1, Month = 1, Week = 1, Day = 1, Hour = 1, morale = 50;
    float Timer, MoneyCalcTimer;
    bool TimePause = false; //现在仅用来判断是否处于下班状态，用于其他功能时需检查WorkEndCheck()和WeekStart



    private void Awake()
    {
        Instance = this;
        Morale = 50;
    }

    private void Start()
    {
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

        //1秒算一次金钱
        MoneyCalcTimer += Time.deltaTime;
        if(MoneyCalcTimer > 1)
        {
            MoneyCalcTimer = 0;
            CalcTotalSalary();
            CalcBuildingPay();
        }
        Text_Money.text = "金钱:" + Money +"\n" 
                        + "    " + (Income - Salary - BuildingPay) + "/月";
        if (CurrentEmployees.Count > 0)
        {
            Text_Stamina.text = "体力:" + CurrentEmployees[0].Stamina + "/" + CurrentEmployees[0].StaminaLimit;
            Text_Mentality.text = "心力:" + CurrentEmployees[0].Mentality + "/" + CurrentEmployees[0].MentalityLimit;
        }
    }

    void HourPass()
    {
        Hour += 1;
        //以下为加班判定
        //体力额外扣除
        CalcBuildingPay();
        CalcTotalSalary();
        if(WorkOverTime == true)
        {
            foreach(Employee emp in CurrentEmployees)
            {
                emp.Stamina -= 1;
            }
        }
        //正常工作时间内正常走时间
        if (WorkOverTime == false || (WorkOverTime == true && Hour <= 8))
        {
            HourEvent.Invoke();
            if (Hour > 8 && WorkOverTime == false)
            {
                Hour = 1;
                WeekPass();
                //StartWorkEnd();
            }

            //开会封锁时间
            if (MeetingBlockTime > 0)
                MeetingBlockTime -= 1;
            if (MobTime > 0)
            {
                MobTime -= 1;
                if (MobTime == 0)
                {
                    SC.gameObject.SetActive(true);
                    SC.SkillSetButton.interactable = false;
                    AskPause(SC);
                }
            }
        }
        //加班时间只进行工作和事件计算
        else if (WorkOverTime == true && Hour > 8)
        {
            foreach(DepControl dep in CurrentDeps)
            {
                dep.Produce();
            }
            foreach(OfficeControl office in CurrentOffices)
            {
                office.TimePass();
            }
            foreach(Employee emp in CurrentEmployees)
            {
                emp.EventTimePass();
                emp.InfoDetail.Entity.TimePass();
            }
            if(Hour > 12)
            {
                Hour = 1;
                WeekPass();
            }
        }

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
        //每周开始时的加班判定
        if (WorkToggle == false)
            WorkOverTime = false;
        else
            WorkOverTime = true;


        if (Week > 4)
        {
            Month += 1;
            Week = 1;
            Money = Money + Income - Salary - (int)(BuildingPay * TotalBuildingPayMultiply);
            MonthlyEvent.Invoke();
            for (int i = 0; i < CurrentDeps.Count; i++)
            {
                CurrentDeps[i].FailCheck();
                if(ResearchExtraMentality == true && CurrentDeps[i].building.Type == BuildingType.研发部门)
                {
                    foreach(Employee emp in CurrentDeps[i].CurrentEmps)
                    {
                        emp.Mentality += 20;
                    }
                }
            }
        }
        if (Month > 12)
        {
            YearEvent.Invoke();
            Year += 1;
            Month = 1;
            for (int i = 0; i < CurrentEmployees.Count; i++)
            {
                CurrentEmployees[i].Age += 1;
                foreach(PerkInfo p in CurrentEmployees[i].InfoDetail.PerksInfo)
                {
                    if(p.CurrentPerk.Num == 58)
                    {
                        PerkInfo pi = p;
                        CurrentEmployees[i].InfoDetail.PerksInfo.Remove(pi);
                        Destroy(pi.gameObject);
                        break;
                    }
                }
            }
        }
        Text_Time.text = "年" + Year + " 月" + Month + " 周" + Week + " 时" + Hour;
    }

    void CalcTotalSalary()
    {
        Salary = 0;
        foreach(Employee e in CurrentEmployees)
        {
            int value = e.InfoDetail.CalcSalary();
            if (e.CurrentDep != null)
                value = (int)(value * e.CurrentDep.SalaryMultiply);
            value = (int)(value * TotalSalaryMultiply);
            Salary += value;
        }
    }
    void CalcBuildingPay()
    {
        int value = 0;
        foreach(DepControl dep in CurrentDeps)
        {
            value += dep.CalcCost(2);
        }
        foreach(OfficeControl office in CurrentOffices)
        {
            value += office.building.Pay;
        }
        BuildingPay = value;
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
        //else

        //start
        TimePause = false;
        for (int i = 0; i < CurrentEmployees.Count; i++)
        {
            CurrentEmployees[i].InfoDetail.Entity.WorkStart();
        }
    }

    public DepControl CreateDep(Building b)
    {
        DepControl newDep;
        newDep = Instantiate(DepPrefab, this.transform);
        newDep.EmpPanel.parent = EmpPanelContent;
        if (newDep.SRateDetailPanel != null)
            newDep.SRateDetailPanel.parent = SRateDetailContent;
        if (newDep.LabPanel != null)
            newDep.LabPanel.parent = EmpPanelContent;
        newDep.transform.parent = DepContent;
        newDep.building = b;

        //部门命名
        string newDepName = b.Type.ToString();
        newDep.ProducePointLimit = int.Parse(b.Time_A);
        if (b.Type == BuildingType.技术部门)
        {
            newDep.type = EmpType.Tech;
            newDep.EmpLimit = 4;
            newDep.building.effectValue = 1;
            newDep.ModeChangeButton.gameObject.SetActive(false);
            newDep.ActiveButton.gameObject.SetActive(false);
            PC.SetName(newDep);
            PC.TaskNum = 1;
            PC.CreateTask();
        }
        else if (b.Type == BuildingType.市场部门)
        {
            newDep.type = EmpType.Market;
            newDep.EmpLimit = 4;
            newDep.building.effectValue = 2;
            newDep.ModeChangeButton.gameObject.SetActive(false);
            newDep.ActiveButton.gameObject.SetActive(false);
        }
        else if (b.Type == BuildingType.产品部门)
        {
            newDep.type = EmpType.Product;
            newDep.EmpLimit = 4;
            newDep.building.effectValue = 3;
            newDep.ModeChangeButton.gameObject.SetActive(false);
            newDep.ActiveButton.gameObject.SetActive(false);
        }
        else if (b.Type == BuildingType.公关营销部)
        {            
            newDep.EmpLimit = 2;
            newDep.building.effectValue = 2;
            newDep.Mode2EffectValue = 10;
            newDep.ActiveMode = 1;
            newDep.ActiveButton.gameObject.SetActive(false);
        }
        else if (b.Type == BuildingType.研发部门)
        {
            newDep.EmpLimit = 2;
            newDep.building.effectValue = 1;
            newDep.Mode2EffectValue = 3;
            newDep.ModeChangeButton.gameObject.SetActive(false);
            newDep.ActiveButton.gameObject.SetActive(false);
        }
        else if (b.Type == BuildingType.智库小组)
        {
            newDep.EmpLimit = 2;
            newDep.building.effectValue = 11;
            newDep.Mode2EffectValue = 9;
            newDep.ActiveMode = 3;
        }
        else if (b.Type == BuildingType.人力资源部)
        {
            newDep.type = EmpType.HR;
            newDep.EmpLimit = 2;
            newDep.building.effectValue = 8;
            newDep.Mode2EffectValue = 12;
            newDep.ActiveButton.gameObject.SetActive(false);
        }
        else if (b.Type == BuildingType.心理咨询室)
        {
            newDep.EmpLimit = 2;
            newDep.building.effectValue = 8;
            newDep.Mode2EffectValue = 14;
            newDep.ActiveMode = 2;
        }
        else if (b.Type == BuildingType.财务部)
        {
            newDep.EmpLimit = 2;
            newDep.building.effectValue = 9;
            newDep.ActiveMode = 3;
        }
        else if (b.Type == BuildingType.体能研究室)
        {
            newDep.EmpLimit = 4;
            newDep.building.effectValue = 10;
            newDep.Mode2EffectValue = 4;
            newDep.ActiveMode = 3;
        }
        else if (b.Type == BuildingType.按摩房)
        {
            newDep.EmpLimit = 1;
            newDep.building.effectValue = 6;
            newDep.Mode2EffectValue = 14;
            newDep.ActiveMode = 2;
        }
        else if (b.Type == BuildingType.健身房)
        {
            newDep.EmpLimit = 2;
            newDep.building.effectValue = 6;
            newDep.Mode2EffectValue = 5;
            newDep.ActiveMode = 2;
        }
        else if (b.Type == BuildingType.宣传中心)
        {
            newDep.EmpLimit = 1;
            newDep.building.effectValue = 15;
            newDep.Mode2EffectValue = 12;
            newDep.ActiveMode = 2;
        }
        else if (b.Type == BuildingType.科技工作坊)
        {
            newDep.EmpLimit = 1;
            newDep.building.effectValue = 1;
            newDep.Mode2EffectValue = 3;
            newDep.ActiveMode = 2;
        }
        else if (b.Type == BuildingType.绩效考评中心)
        {
            newDep.EmpLimit = 1;
            newDep.building.effectValue = 4;
            newDep.Mode2EffectValue = 11;
            newDep.ActiveMode = 2;
        }
        else if (b.Type == BuildingType.员工休息室)
        {
            newDep.EmpLimit = 1;
            newDep.building.effectValue = 5;
            newDep.Mode2EffectValue = 9;
            newDep.Mode2EffectValue = 10;
            newDep.ActiveMode = 2;
        }
        else if (b.Type == BuildingType.人文沙龙)
        {
            newDep.EmpLimit = 2;
            newDep.building.effectValue = 8;
            newDep.Mode2EffectValue = 14;
        }
        else if (b.Type == BuildingType.兴趣社团)
        {
            newDep.EmpLimit = 2;
            newDep.building.effectValue = 8;
            newDep.Mode2EffectValue = 15;
            newDep.ActiveMode = 3;
            newDep.ModeChangeButton.gameObject.SetActive(false);
        }
        else if (b.Type == BuildingType.电子科技展)
        {
            newDep.EmpLimit = 2;
            newDep.building.effectValue = 1;
            newDep.Mode2EffectValue = 14;
        }
        else if (b.Type == BuildingType.冥想室)
        {
            newDep.EmpLimit = 1;
            newDep.building.effectValue = 5;
            newDep.Mode2EffectValue = 13;
            newDep.ActiveMode = 2;
        }
        else if (b.Type == BuildingType.特别秘书处)
        {
            newDep.EmpLimit = 1;
            newDep.building.effectValue = 12;
            newDep.Mode2EffectValue = 15;
            newDep.ActiveMode = 2;
        }
        else if (b.Type == BuildingType.成人再教育所)
        {
            newDep.EmpLimit = 1;
            newDep.building.effectValue = 13;
            newDep.Mode2EffectValue = 6;
            newDep.ActiveMode = 2;
        }
        else if (b.Type == BuildingType.职业再发展中心)
        {
            newDep.EmpLimit = 1;
            newDep.building.effectValue = 12;
            newDep.Mode2EffectValue = 8;
            newDep.ActiveMode = 2;
        }
        else if (b.Type == BuildingType.中央监控室)
        {
            newDep.EmpLimit = 3;
            newDep.building.effectValue = 15;
            newDep.ModeChangeButton.gameObject.SetActive(false);
        }
        else if (b.Type == BuildingType.谍战中心)
        {
            newDep.EmpLimit = 2;
            newDep.building.effectValue = 12;
            newDep.Mode2EffectValue = 6;
            newDep.ActiveMode = 2;
        }

        newDep.Mode1EffectValue = b.effectValue;

        int num = 1;
        for(int i = 0; i < CurrentDeps.Count; i++)
        {
            if (CurrentDeps[i].building.Type == newDep.building.Type)
                num += 1;
        }
        newDep.Text_DepName.text = newDepName + num;
        newDep.GC = this;
        HourEvent.AddListener(newDep.Produce);
        WeeklyEvent.AddListener(newDep.FaithEffect);

        //创建对应按钮
        newDep.DS = Instantiate(DepSelectButtonPrefab, DepSelectContent);
        newDep.DS.Text_DepName.text = newDep.Text_DepName.text;
        newDep.DS.DC = newDep;
        newDep.DS.GC = this;

        //检测老板摸鱼的附加状态
        CurrentDeps.Add(newDep);
        if (CEOVacation == true)
            newDep.AddPerk(new Perk115(null));
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
        b.effectValue = 10;

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
            if (office.InRangeOffices.Contains(CurrentOffices[i]) && office.ControledOffices.Contains(CurrentOffices[i]) == false)
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
        //调动信息
        if (SelectMode == 1 || SelectMode == 2)
        {
            CurrentEmpInfo.emp.InfoDetail.AddHistory("调动至待命室");
            if (CurrentEmpInfo.emp.isCEO == false)
                CC.CEO.InfoDetail.AddHistory("将" + CurrentEmpInfo.emp.Name + "调动至待命室");
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
            //给部门添加临时状态
            foreach(DepControl dep in office.ControledDeps)
            {
                dep.AddPerk(new Perk108(null));
                dep.FaithRelationCheck();
            }
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
            //给部门添加临时状态
            foreach (DepControl dep in office.ControledDeps)
            {
                dep.AddPerk(new Perk108(null));
                dep.FaithRelationCheck();
            }
        }
        //确定部门领导者
        else if (SelectMode == 3)
        {
            if(CurrentDep.CommandingOffice != null)
            {
                CurrentDep.CommandingOffice.ControledDeps.Remove(CurrentDep);
                CurrentDep.CommandingOffice.CheckManage();
                CurrentDep.AddPerk(new Perk110(null));
            }
            CurrentDep.CommandingOffice = office;
            office.ControledDeps.Add(CurrentDep);
            office.CheckManage();
            //加额外状态
            if(office.CurrentManager != null)
                CurrentDep.AddPerk(new Perk108(null));
            CurrentDep.FaithRelationCheck();
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
            CC.CEOSkillConfirm();
        }

        //调动信息设定
        if (SelectMode == 1 || SelectMode == 2)
        {
            CurrentEmpInfo.emp.InfoDetail.AddHistory("调动至" + office.Text_OfficeName.text);
            if (CurrentEmpInfo.emp.isCEO == false)
                CC.CEO.InfoDetail.AddHistory("将" + CurrentEmpInfo.emp.Name + "调动至" + office.Text_OfficeName.text);
        }
    }

    //将移动或雇佣的员工放入特定部门 + 选择部门发动建筑特效 + CEO技能发动
    public void SelectDep(DepControl depControl)
    {
        if(SelectMode == 1)
        {
            HC.SetInfoPanel();
            depControl.CurrentEmps.Add(CurrentEmpInfo.emp);
            depControl.UpdateUI();
            CurrentEmpInfo.emp.CurrentDep = depControl;
            CurrentEmpInfo.emp.CurrentDep.EmpMove(false);
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
            CurrentEmpInfo.emp.CurrentDep.EmpMove(false);
            depControl.UpdateUI();
        }
        //选择部门发动建筑特效
        else if(SelectMode == 5)
        {
            SelectedDep = depControl;
            CurrentDep.BuildingActive();
        }
        //CEO技能
        else if (SelectMode == 6)
        {
            if (CEOSkillNum == 1)
            {
                new ProduceBuff(0.45f, depControl, 128);
                new EmpBuff(CC.CEO, 16, -45);
                ResetSelectMode();
            }
            else if (CEOSkillNum == 2)
            {
                //  depControl.SpTime += 16;

            }
            CC.CEOSkillConfirm();
        }
        DepSelectPanel.SetActive(false);
        //调动信息
        if (SelectMode == 1 || SelectMode == 2)
        {
            //调动信息设定
            CurrentEmpInfo.emp.InfoDetail.AddHistory("调动至" + depControl.Text_DepName.text);
            if (CurrentEmpInfo.emp.isCEO == false)
                CC.CEO.InfoDetail.AddHistory("将" + CurrentEmpInfo.emp.Name + "调动至" + depControl.Text_DepName.text);
        }
    }
    //重置各项信息
    public void ResetOldAssignment(Employee target = null)
    {
        Employee emp = target;
        if (emp == null)
            emp = CurrentEmpInfo.emp;
        if (emp.CurrentDep != null)
        {
            emp.CurrentDep.CurrentEmps.Remove(emp);
            //修改生产力显示
            emp.CurrentDep.UpdateUI();
            CurrentEmpInfo.emp.CurrentDep.EmpMove(true);
            emp.CurrentDep = null;
        }
        if (emp.CurrentOffice != null)
        {
            foreach (DepControl dep in emp.CurrentOffice.ControledDeps)
            {
                dep.AddPerk(new Perk110(null));
                dep.FaithRelationCheck();
            }
            if (emp.CurrentOffice.building.Type == BuildingType.高管办公室 || emp.CurrentOffice.building.Type == BuildingType.CEO办公室)
                emp.InfoDetail.TempDestroyStrategy();
            emp.CurrentOffice.CurrentManager = null;
            emp.CurrentOffice.SetOfficeStatus();
            emp.CurrentOffice.CheckManage();
            emp.CurrentOffice = null;
        }
    }

    public void UpdateResourceInfo()
    {
        int[] C = FinishedTask;
        //Text_TechResource.text = "程序迭代: " + C[0] + "\n" +
        //    "技术研发: " + C[1] + "\n" +
        //    "可行性调研: " + C[2] + "\n";
        Text_TechResource.text = "程序迭代: " + C[0];

        //Text_MarketResource.text = "传播: " + C[3] + "\n" +
        //    "营销文案: " + C[4] + "\n" +
        //    "资源拓展: " + C[5] + "\n";
        Text_MarketResource.text = "传播: " + C[3];
        Text_MarketResource2.text = "营销文案: " + C[4];
        //Text_ProductResource.text = "原型图: " + C[6] + "\n" +
        //   "产品研究: " + C[7] + "\n" +
        //   "用户访谈: " + C[8] + "\n";
        Text_ProductResource.text = "原型图: " + C[6];
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
        if (num == 1)
        {
            if (CC.CEO.StaminaLimit >= 45)
            {
                CEOSkillNum = 1;
                CEOSkillPanel.SetActive(false);
                ShowDepSelectPanel(CurrentDeps);
                SelectMode = 6;
            }
            else
            {
                CreateMessage("CEO体力上限不足");
            }
        }
        else if (num == 2)
        {
            if (Stamina >= 30)
            {
                CEOSkillNum = 2;
                CEOSkillPanel.SetActive(false);
                ShowDepSelectPanel(CurrentDeps);
                SelectMode = 6;
            }
        }
        else if (num == 3)
        {
            if (CC.CEO.StaminaLimit >= 45)
            {
                SelectMode = 6;
                CEOSkillNum = num;
                CEOSkillPanel.SetActive(false);
                TotalEmpContent.parent.parent.gameObject.SetActive(true);
                Text_EmpSelectTip.gameObject.SetActive(true);
                Text_EmpSelectTip.text = "选择一个员工";
            }
            else
            {
                CreateMessage("CEO体力上限不足");
            }
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
        else if (num == 4)
        {
            if (Stamina >= 20)
            {
                SelectMode = 6;
                CEOSkillNum = 4;
                CEOSkillPanel.SetActive(false);
                TotalEmpContent.parent.parent.gameObject.SetActive(true);
            }
        }
        else
        {
            CEOSkillNum = num;
            SelectMode = 6;
            if (CEOSkillNum == 11)
                SelectMode = 10;
            CEOSkillPanel.SetActive(false);
            TotalEmpContent.parent.parent.gameObject.SetActive(true);
            Text_EmpSelectTip.gameObject.SetActive(true);
            Text_EmpSelectTip.text = "选择一个员工";
            if (num == 5 || num == 19)
            {
                foreach (Employee e in CurrentEmployees)
                {
                    if (e.InfoDetail.Entity.OutCompany == true)
                        e.InfoB.gameObject.SetActive(false);
                }
            }
        }
    }

    public void TrainEmp(int type)
    {
        if (CC.CEO.StaminaLimit >= 45)
        {
            new EmpBuff(CurrentEmpInfo.emp, type);
            new EmpBuff(CC.CEO, 16, -45);
            CurrentEmpInfo.emp.InfoDetail.AddHistory("接受了CEO的培养");
            CC.CEO.InfoDetail.AddHistory("培养了" + CurrentEmpInfo.emp.Name);
        }
        else
        {
            CreateMessage("CEO体力上限不足");
        }
    }

    public void SetEmpVacationTime(int type)
    {
        CurrentEmpInfo.emp.VacationTime = type;
        CurrentEmpInfo.emp.InfoDetail.Entity.SetBusy();
        CurrentEmpInfo.emp.InfoDetail.AddHistory("被安排放假" + type + "工时");
        CC.CEO.InfoDetail.AddHistory("安排" + CurrentEmpInfo.emp.Name + "放假" + type + "工时");
        if (CurrentEmpInfo.emp.isCEO == true)
        {
            ResetSelectMode();
            CC.SkillButton.interactable = false;
            CEOVacation = true;
            foreach(DepControl dep in CurrentDeps)
            {
                dep.AddPerk(new Perk115(null));
            }
        }
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
    }

    public void ResetSelectMode()
    {
        SelectMode = 0;
        CEOSkillNum = 0;
        Text_EmpSelectTip.gameObject.SetActive(false);
        SC.SelectConfirmButton.SetActive(false);
        CC.SelectPanel.SetActive(false);
        CC.ResultPanel.SetActive(false);
        for(int i = 0; i < CurrentEmployees.Count; i++)
        {
            CurrentEmployees[i].InfoB.gameObject.SetActive(true);
            CurrentEmployees[i].InfoB.MoveButton.GetComponentInChildren<Text>().text = "移动";
            if (CurrentEmployees[i].isCEO == false)
                CurrentEmployees[i].InfoB.FireButton.gameObject.SetActive(true);

        }
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
            CurrentOffice.Progress = 0;
            CurrentOffice.UpdateUI();
            if (num == 1)
            {
                CurrentOffice.Text_OfficeMode.text = "办公室模式:决策";
                CurrentOffice.MaxProgress = 96;
                CurrentOffice.building.effectValue = 10;
            }
            else if (num == 2)
            {
                CurrentOffice.Text_OfficeMode.text = "办公室模式:人力";
                CurrentOffice.MaxProgress = 24;
                CurrentOffice.building.effectValue = 8;
            }
            else if (num == 3)
            {
                CurrentOffice.Text_OfficeMode.text = "办公室模式:管理";
                CurrentOffice.MaxProgress = 32;
                CurrentOffice.building.effectValue = 7;
            }
            else if (num == 4)
            {
                CurrentOffice.Text_OfficeMode.text = "办公室模式:招聘";
                CurrentOffice.MaxProgress = 24;
                CurrentOffice.building.effectValue = 8;
            }
            else if (num == 5)
            {
                CurrentOffice.Text_OfficeMode.text = "办公室模式:部门研究";
                CurrentOffice.MaxProgress = 48;
                CurrentOffice.building.effectValue = 7;
            }
        }
    }

    public void SetDepMode(int num)
    {
        if (CurrentDep != null)
            CurrentDep.ChangeBuildingMode(num);
    }

    //激活部门效果
    public void ActiveDepSkill()
    {
        if (CurrentDep != null)
            CurrentDep.ConfirmActive();
    }

    //选两个员工时取消选择
    public void CancelEmpSelect()
    {
        if(SelectMode == 4)
        {
            TotalEmpContent.parent.parent.gameObject.SetActive(false);
            ResetSelectMode();
        }
        else if (SelectMode == 6)
            ResetSelectMode();
        else if (SelectMode == 7 || SelectMode == 10)
        {
            if(CurrentEmpInfo2 == null)
            {
                if (SelectMode == 7)
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
    //开关996加班效果
    public void ToggleWorkHour(bool value)
    {
        WorkToggle = value;
    }
    //手动分配技能树
    public void SetSkillTree(int num)
    {
        if (CurrentEmpInfo != null)
            CurrentEmpInfo.emp.InfoDetail.ST.ChangeSkillTree(num);
        SkillTreeSelectPanel.SetActive(false);
        TotalEmpContent.parent.parent.gameObject.SetActive(false);
        ResetSelectMode();
    }
    //游戏结束后续钱继续
    public void GameReset()
    {
        Money += 10000;
        GameOverPanel.gameObject.SetActive(false);
    }

    //一些提示框信息
    public void ShowCompayCost()
    {
        infoPanel.Text_Name.text = "";
        infoPanel.Text_ExtraInfo.text = "";
        string content = "维护费用";
        foreach(DepControl dep in CurrentDeps)
        {
            content += "\n" + dep.Text_DepName.text + "维护费:" + dep.CalcCost(2);
            if (dep.CurrentEmps.Count > 0)
                content += "\n" + dep.Text_DepName.text + "工资:" + dep.CalcCost(1);
        }
        foreach(OfficeControl office in CurrentOffices)
        {
            content += "\n" + office.Text_OfficeName.text + "维护费:" + office.building.Pay;
            if(office.CurrentManager != null)
            content += "\n" + office.Text_OfficeName.text + "工资:" + (int)(office.CurrentManager.InfoDetail.CalcSalary() * TotalSalaryMultiply);
        }
        int otherCost = 0;
        foreach(Employee emp in CurrentEmployees)
        {
            if(emp.CurrentDep == null && emp.CurrentOffice == null)
            {
                otherCost += (int)(emp.InfoDetail.CalcSalary() * TotalSalaryMultiply);
            }
        }
        if (otherCost > 0)
            content += "\n待命员工工资:" + otherCost;
        infoPanel.Text_Description.text = content;
        infoPanel.ShowPanel();
        infoPanel.transform.position = Input.mousePosition;
    }
    public void ShowStatusDetail(int type)
    {
        if (type == 1)
        {
            infoPanel.Text_Name.text = "CEO本人的体力，可以通过提升CEO的强壮来提高上限";
            infoPanel.Text_ExtraInfo.text = "";
            string content = "初始体力上限:100";
            content += "\nCEO强壮" + CC.CEO.Strength + "点:+" + (CC.CEO.Strength * 5);
            foreach (EmpBuff buff in CC.CEO.CurrentBuffs)
            {
                if (buff.Type == 16)
                    content += "\n技能影响:" + buff.Value + "(" + buff.Time + "时)";
            }
            infoPanel.Text_Description.text = content;
        }
        else if (type == 2)
        {
            infoPanel.Text_Name.text = "CEO本人的心力，可以通过提升CEO的坚韧来提高上限";
            infoPanel.Text_ExtraInfo.text = "";
            string content = "初始心力上限:100";
            content += "\nCEO坚韧" + CC.CEO.Tenacity + "点:+" + (CC.CEO.Tenacity * 5);
            infoPanel.Text_Description.text = content;
        }
        else if (type == 3)
        {
            infoPanel.Text_Name.text = "士气会影响CEO技能的成功率和部分事件的判定";
            infoPanel.Text_Description.text = "士气越高，越容易出现正面的结果，可点击员工发生事件后头顶冒出的气泡查看规则";
            infoPanel.Text_ExtraInfo.text = "可以从商战中消耗“传播”获取";
        }

        infoPanel.ShowPanel();
        infoPanel.transform.position = Input.mousePosition;
    }

    //暂停检测相关
    List<MonoBehaviour> pauseMono = new List<MonoBehaviour>();
    public void AskPause(MonoBehaviour mono)
    {
        if (!pauseMono.Contains(mono))
        {
            pauseMono.Add(mono);
            ForceTimePause = true;
        }
    }

    public void RemovePause(MonoBehaviour mono)
    {
        pauseMono.Remove(mono);

        if (pauseMono.Count == 0)
        {
            ForceTimePause = false;
        }
        else
        {
            ForceTimePause = true;
        }
    }
}
