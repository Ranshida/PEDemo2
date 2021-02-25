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
    public int Money = 1000, DoubleMobilizeCost = 0, MeetingBlockTime = 0, MobTime = 192;
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
    public QuestControl QC;
    public EmpEntity EmpEntityPrefab;
    public PerkInfo PerkInfoPrefab;
    public DepControl DepPrefab, HRDepPrefab;
    public OfficeControl OfficePrefab;
    public DepSelect DepSelectButtonPrefab, OfficeSelectButtonPrefab;
    public RelationInfo RelationInfoPrefab;
    public Text HistoryTextPrefab, Text_EmpSelectTip;
    public HireControl HC;
    public BuildingManage BM;
    public StrategyControl StrC;
    public FOEControl foeControl;
    public EventControl EC;
    public CEOControl CC;
    public WindowBaseControl TotalEmpPanel;
    public Transform DepContent, DepSelectContent, StandbyContent, MessageContent;
    public InfoPanel infoPanel;
    public GameObject DepSelectPanel, StandbyButton, MessagePrefab, GameOverPanel, OfficeModeSelectPanel,
        OfficeModeBuildingOptionButton, OfficeModeTalkOptionButton, DepModeSelectPanel, DepSkillConfirmPanel;
    public Text Text_Time, Text_TechResource, Text_MarketResource, Text_MarketResource2, Text_ProductResource, Text_Money, Text_Stamina, Text_Mentality, 
        Text_Morale, Text_DepMode1, Text_DepMode2, Text_DepSkillDescribe, Text_WarTime, Text_MobTime;
    public Toggle WorkOvertimeToggle;
    public SkillControl SC;
    [HideInInspector] public UnityEvent DailyEvent, WeeklyEvent, MonthlyEvent, HourEvent, YearEvent;

    public Button[] TimeButtons = new Button[5];
    public List<DepControl> CurrentDeps = new List<DepControl>();
    public List<OfficeControl> CurrentOffices = new List<OfficeControl>();
    public List<Employee> CurrentEmployees = new List<Employee>();
    public List<CompanyItem> Items = new List<CompanyItem>();
    public int[] FinishedTask = new int[10];//0程序迭代 1技术研发 2可行性调研 3传播 4营销文案 5资源拓展 6原型图 7产品研究 8用户访谈 9已删除

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
        UpdateResourceInfo();
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
        {
            Money += 1000;
            Stamina += 100;
        }
        if (Money < 0)
            GameOverPanel.GetComponent<WindowBaseControl>().SetWndState(true);

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
            Text_Stamina.text = "CEO体力:" + CurrentEmployees[0].Stamina + "/" + CurrentEmployees[0].StaminaLimit;
            Text_Mentality.text = "CEO心力:" + CurrentEmployees[0].Mentality + "/" + CurrentEmployees[0].MentalityLimit;
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
                    //SC.SetWndState(true);
                    //SC.FinishSign.SetActive(false);
                    //SC.SkillSetButton.interactable = false;
                    //AskPause(SC);
                    MobTime = 192;
                }
                Text_MobTime.text = "距离下次头脑风暴还剩" + (MobTime / 8) + "周";
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
        DailyEvent.Invoke();
        Hour = 1;
        Week += 1;
        WeeklyEvent.Invoke();
        //Day += 1;
        //每周开始时的加班判定
        Stamina += 10;
        if (WorkToggle == false)
            WorkOverTime = false;
        else
            WorkOverTime = true;

        //部门的每周体力buff判定
        for (int i = 0; i < CurrentDeps.Count; i++)
        {
            if(CurrentDeps[i].StaminaExtra != 0)
            {
                foreach(Employee e in CurrentDeps[i].CurrentEmps)
                {
                    e.Stamina += CurrentDeps[i].StaminaExtra;
                }
            }
        }

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

    //计算支出
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

    //旧下班判定，现在没有太大用处
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
        UIManager.Instance.OnAddNewWindow(newDep.EmpPanel.GetComponent<WindowBaseControl>());
        if (newDep.SRateDetailPanel != null)
            UIManager.Instance.OnAddNewWindow(newDep.SRateDetailPanel.GetComponent<WindowBaseControl>());
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
            UIManager.Instance.OnAddNewWindow(newOffice.SRateDetailPanel.GetComponent<WindowBaseControl>());
        return newOffice;
    }

    //招募的部门选择
    public void ShowDepSelectPanel()
    {
        DepSelectPanel.GetComponent<WindowBaseControl>().SetWndState(true);
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
        DepSelectPanel.GetComponent<WindowBaseControl>().SetWndState(true);
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
        DepSelectPanel.GetComponent<WindowBaseControl>().SetWndState(true);
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
        DepSelectPanel.GetComponent<WindowBaseControl>().SetWndState(true);
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
        DepSelectPanel.GetComponent<WindowBaseControl>().SetWndState(true);
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
        DepSelectPanel.GetComponent<WindowBaseControl>().SetWndState(true);
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
        DepSelectPanel.GetComponent<WindowBaseControl>().SetWndState(false);
        if (SelectMode == 1)
        {
            HC.SetInfoPanel();
            CurrentEmpInfo.emp.InfoA.transform.parent = StandbyContent;
            QC.Finish(8);
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
        DepSelectPanel.GetComponent<WindowBaseControl>().SetWndState(false);
        if (office.CurrentManager != null && SelectMode < 3)
        {
            //这部分可能已经不需要了，因为筛选DepSelect按钮时已经剔除了有高管的办公室
            office.CurrentManager.InfoDetail.TempDestroyStrategy();
            office.CurrentManager.CurrentOffice = null;
            //office.CurrentManager.InfoDetail.Entity.FindWorkPos();
        }
        if (SelectMode == 1)
        {
            QC.Finish(8);
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
        DepSelectPanel.GetComponent<WindowBaseControl>().SetWndState(false);
        if (SelectMode == 1)
        {
            QC.Finish(8);
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
            if (CC.CEOSkillNum == 1)
            {
                depControl.AddPerk(new Perk117(null));
                QC.Finish(5);
                new EmpBuff(CC.CEO, 16, -45);
                ResetSelectMode();
                QC.Init("技能释放成功\n\n" + depControl.Text_DepName.text + "成功率上升45%");
            }
        }
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

    //更新资源面板
    public void UpdateResourceInfo()
    {
        int[] C = FinishedTask;
        Text_TechResource.text = "程序迭代: " + C[0];
        Text_MarketResource.text = "传播: " + C[3];
        Text_MarketResource2.text = "营销文案: " + C[4];
        Text_ProductResource.text = "原型图: " + C[6];
    }

    //创建左侧信息栏内容
    public void CreateMessage(string content)
    {
        Text T = Instantiate(MessagePrefab, MessageContent).GetComponentInChildren<Text>();
        T.text = content;
    }

    //设置时间流逝速度，由右上角面板的速度按钮调用
    public void SetTimeMultiply(int value)
    {
        TimeMultiply = value;
    }
    //调用同上，用于设置速度按钮颜色
    public void SetButtonColor(int num)
    {
        for (int i = 0; i < 5; i++)
        {
            if (num == i)
                TimeButtons[i].GetComponent<Image>().color = Color.gray;
            else
                TimeButtons[i].GetComponent<Image>().color = Color.white;
        }
    }

    //重置选择模式以及部分相关变量
    public void ResetSelectMode()
    {
        SelectMode = 0;
        CC.CEOSkillNum = 0;
        Text_EmpSelectTip.gameObject.SetActive(false);
        SC.SelectConfirmButton.SetActive(false);
        CC.SelectPanel.GetComponent<WindowBaseControl>().SetWndState(false);
        CC.ResultPanel.GetComponent<WindowBaseControl>().SetWndState(false); ;
        for(int i = 0; i < CurrentEmployees.Count; i++)
        {
            CurrentEmployees[i].InfoB.gameObject.SetActive(true);
            CurrentEmployees[i].InfoB.MoveButton.GetComponentInChildren<Text>().text = "移动";
            if (CurrentEmployees[i].isCEO == false)
                CurrentEmployees[i].InfoB.FireButton.gameObject.SetActive(true);

        }
    }

    //设置办公室模式
    public void SetOfficeMode(int num)
    {
        if(CurrentOffice != null)
        {
            CurrentOffice.OfficeMode = num;
            OfficeModeSelectPanel.GetComponent<WindowBaseControl>().SetWndState(false);
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
                QC.Finish(4);
            }
            else if (num == 4)
            {
                CurrentOffice.Text_OfficeMode.text = "办公室模式:招聘";
                CurrentOffice.MaxProgress = 24;
                CurrentOffice.building.effectValue = 8;
            }
            else if (num == 5)
            {
                CurrentOffice.Text_OfficeMode.text = "办公室模式:组织研究";
                CurrentOffice.MaxProgress = 48;
                CurrentOffice.building.effectValue = 7;
            }
        }
    }
    //设置部门模式
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
            TotalEmpPanel.SetWndState(false);
            ResetSelectMode();
        }
        else if (SelectMode == 6)
            ResetSelectMode();
        else if (SelectMode == 7 || SelectMode == 10)
        {
            if(CurrentEmpInfo2 == null)
            {
                if (SelectMode == 7)
                    TotalEmpPanel.SetWndState(false);
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
    //游戏结束后续钱继续
    public void GameReset()
    {
        Money += 10000;
        GameOverPanel.GetComponent<WindowBaseControl>().SetWndState(false);
    }

    //一些提示框信息
    public void ShowCompayCost()
    {
        infoPanel.Text_Name.text = "商战收入:+" + Income;
        infoPanel.Text_ExtraInfo.text = "";
        string content = "维护费用";
        foreach(DepControl dep in CurrentDeps)
        {
            content += "\n" + dep.Text_DepName.text + "维护费:-" + dep.CalcCost(2);
            if (dep.CurrentEmps.Count > 0)
                content += "\n" + dep.Text_DepName.text + "工资:-" + dep.CalcCost(1);
        }
        foreach(OfficeControl office in CurrentOffices)
        {
            content += "\n" + office.Text_OfficeName.text + "维护费:-" + office.building.Pay;
            if(office.CurrentManager != null)
            content += "\n" + office.Text_OfficeName.text + "工资:-" + (int)(office.CurrentManager.InfoDetail.CalcSalary() * TotalSalaryMultiply);
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
            infoPanel.Text_Name.text = "CEO本人的体力，每提高1点强壮增加5点体力上限";
            infoPanel.Text_ExtraInfo.text = "CEO每周自动回复10点体力\n放假时每工时回复2点体力";
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
            infoPanel.Text_Name.text = "CEO本人的心力，每提高1点坚韧增加5点心力上限";
            infoPanel.Text_ExtraInfo.text = "";
            string content = "初始心力上限:100";
            content += "\nCEO坚韧" + CC.CEO.Tenacity + "点:+" + (CC.CEO.Tenacity * 5);
            infoPanel.Text_Description.text = content;
        }
        else if (type == 3)
        {
            infoPanel.Text_Name.text = "士气会影响CEO技能的成功率和部分事件的判定";
            infoPanel.Text_Description.text = "可点击员工发生事件后头顶冒出的气泡查看规则";
            infoPanel.Text_ExtraInfo.text = "士气越高，越容易出现正面的结果";
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
