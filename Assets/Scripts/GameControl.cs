using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameControl : MonoBehaviour
{
    public static GameControl Instance;
    [HideInInspector] public int Salary, Income = 0, BuildingPay, MobilizeExtraMent = 0, ExtraDice = 0;
    [HideInInspector] public float EfficiencyExtraScience = 0, ExtrafailRate = 0, TotalSalaryMultiply = 1.0f,
        HRBuildingMentalityExtra = 1.0f, BuildingSkillSuccessExtra = 0, TotalBuildingPayMultiply = 1.0f, HireSuccessExtra = 0, 
        BaseDepExtraSuccessRate = 0;
    public int SelectMode = 1; //1员工招聘时部门选择 2员工移动时部门选择 3事件组特别小组成员选择 4发动动员技能时员工选择 
    //5发动建筑技能时员工选择 6CEO技能员工/部门选择 7选择两个员工发动动员技能 8普通办公室的上级(高管办公室)选择  
    //10选择两个员工的CEO技能 11部门/员工的物品使用 12航线事件和去除负面特质相关 13在员工面板选择加入的部门（或替换已有员工）
    public int AreaSelectMode = 1;//1部门目标区域的选择（暂时删除）  2物品目标区域的选择
    public int Money = 1000, DoubleMobilizeCost = 0, MonthMeetingTime = 3, WarTime = 3, AdjustTurn = 0;
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
            //if (morale > 100)
            //    morale = 100;
            //else if (morale < 0)
            //    morale = 0;
            Text_Morale.text = "士气:" + morale;
        }
    }
    public int Turn = 1;//当前回合数
    public int ItemLimit = 6;//物品数量限制
    public int EmpLimit = 10;//人才库数量限制
    public int TotalWorkStatus = 0, ExtraWorkStatus = 0;//公司工作状态
    public int TotalEfficiency = 0, ExtraEfficiency = 0;//公司效率

    #region 杂项变量
    [HideInInspector] public int ApproveLimit = 5, Year = 1, Month = 1;//认同上限;当前年月
    [HideInInspector] public int DissatisfiedLimit = 5;//不满上限
    [HideInInspector] public int ExtraCost = 0;//额外维护费效果
    [HideInInspector] public bool ProduceBuffBonus = false;//“精进”和“团结”状态的持续时间提高至4m战略效果
    [HideInInspector] public bool GymBuffBonus = false;//“健身房”大成功时获得“强化”或“铁人”状态的概率上升为80%战略效果
    [HideInInspector] public bool WorkToggle = false;//下周开始是否加班
    [HideInInspector] public bool NeutralOccupieBonus = false;//商战中立占领Buff加成
    [HideInInspector] public bool TargetOccupieBonus = false;//商战反占领Buff加成
    [HideInInspector] public bool CEOExtraVote = false;//CEO投票是否有额外加成
    [HideInInspector] public bool ResearchExtraMentality = false;//科研额外心力恢复
    [HideInInspector] public bool CEOVacation = false;
    [HideInInspector] public bool ShowEmpEmotion = false;
    [HideInInspector] public bool FirstExhausted = false;//是否在一回合内产生过心力爆炸的员工
    private int Approve = 0;
    private int Dissatisfied = 0;
    #endregion

    [HideInInspector] public EmpInfo CurrentEmpInfo, CurrentEmpInfo2;//2主要用于需要两个员工的动员技能
    public DepControl CurrentDep, SelectedDep;
    public QuestControl QC;
    public EmpEntity EmpEntityPrefab;
    public PerkInfo PerkInfoPrefab;
    public AmbitionSelect AmbitionPanelPrefab;
    public DepControl DepPrefab, PCDepPrefab, PsycholCDep, EngineDep;
    public DepSelect DepSelectButtonPrefab;
    public RelationInfo RelationInfoPrefab;
    public Text HistoryTextPrefab, Text_EmpSelectTip, Text_CompanyEfficiency, Text_CompanyWorkStatus;
    public CompanyItem ItemPrefab, CurrentItem;
    public HireControl HC;
    public BuildingManage BM;
    public StrategyControl StrC;
    public FOEControl foeControl;
    public EventControl EC;
    public OptionCardLibrary OCL;
    public CWCardLibrary CWCL;
    public CEOControl CC;
    public CourseControl CrC;
    public CompanyPerkControl CPC;
    public Areas AC;
    public WindowBaseControl TotalEmpPanel;
    public Transform DepContent, DepSelectContent, StandbyContent, MessageContent, ItemContent;
    public InfoPanel infoPanel;
    public GameObject DepSelectPanel, StandbyButton, MessagePrefab, GameOverPanel, AdjustTurnWarning;
    public Text Text_Time, Text_TechResource, Text_MarketResource, Text_MarketResource2, Text_ProductResource, Text_Money, 
        Text_Morale, Text_WarTime, Text_MonthMeetingTime, Text_NextTurn, Text_EventProgress;
    public Toggle WorkOvertimeToggle;
    public BrainStormControl BSC;
    [HideInInspector] public UnityEvent DailyEvent, WeeklyEvent, MonthlyEvent, HourEvent, YearEvent, TurnEvent;

    public List<DepControl> CurrentDeps = new List<DepControl>();
    public List<Employee> CurrentEmployees = new List<Employee>();
    public List<CompanyItem> Items = new List<CompanyItem>();
    public List<DivisionControl> CurrentDivisions = new List<DivisionControl>();
    public int[] FinishedTask = new int[10];//0程序迭代 1技术研发 2可行性调研 3传播 4营销文案 5资源拓展 6原型图 7产品研究 8用户访谈 9已删除

    int morale = 50;
    float Timer, MoneyCalcTimer;
    bool TimePause = false; //现在仅用来判断是否处于下班状态，用于其他功能时需检查WorkEndCheck()和WeekStart

    private void Awake()
    {
        Instance = this;
        Morale = 50;
        CrC.InitNodeRef();//因为招聘刷新初始员工需要节点先设置好所以放在Awake内
    }

    private void Start()
    {
        UpdateResourceInfo();
        CWCL.AddCWCard(new CWCard1());
        CurrentDivisions[0].CWCards[0].StartReplace();
        CWCL.ReplaceCard(CWCL.CurrentCards[0]);
        CurrentDivisions[0].PreCards[0] = CurrentDivisions[0].CWCards[0].CurrentCard;
        //OCL.AddStaticOptions(new OptionCard11());
        //OCL.AddStaticOptions(new OptionCard13());
        //OCL.AddStaticOptions(new OptionCard9());
        //OCL.AddStaticOptions(new OptionCard8());
        //OCL.AddStaticOptions(new OptionCard9());
        //CreateItem(3);
        //CreateItem(4);
        //CreateItem(5);
        //CreateItem(6);
        Money = AdjustData.DefaultMoney;
        UpdateUI();
    }

    public void UpdateUI()
    {
        Text_Time.text = "<size=27>第" + Turn + "回合</size>" + "\n<size=20>第" + Year + "年" + Month + "月</size>";
        Text_MonthMeetingTime.text = "距离下次月会还剩" + MonthMeetingTime + "回合";
        Text_WarTime.text = "距离下次商战还剩" + WarTime + "回合";
    }

    private void Update()
    {
        //旧的计时
        //if (TimePause == false && ForceTimePause == false)
        //    Timer += Time.deltaTime * TimeMultiply;
        //if(Timer >= 10)
        //{
        //    Timer = 0;
        //    HourPass();
        //}

        Text_EventProgress.text = "不满:" + Dissatisfied + "/" + DissatisfiedLimit + "                            认同:" + Approve + "/" + ApproveLimit;

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
        }
        //显示金钱和员工数量限制
        Text_Money.text = "金钱:" + Money + "  " + (CalcCost() * -1 + Income + AdjustData.DefaultIncome) + "/月\n\n员工数量:" 
            + CurrentEmployees.Count + "/" + EmpLimit;
    }

    public void NextTurn()
    {
        if (ForceTimePause == true)
            return;
        if (CWCL.newCard != null)
        {
            CreateMessage("有商战卡牌没有选择");
            return;
        }
        if (Items.Count > ItemLimit)
        {
            CreateMessage("物品超出上限");
            return;
        }
        if (CWCL.CardDivCheck() == false)
        {
            CreateMessage("有商战卡牌没有放入插槽");
            return;
        }
        if (CrC.PowerLevel == 0)
        {
            CreateMessage("动力舱没有员工");
            return;
        }
        if (CurrentEmployees.Count > EmpLimit)
        {
            CreateMessage("人才储备库员工超出上限");
            return;
        }
        TurnPass();
    }

    public void TurnPass()
    {
        if (EC.UnfinishedEvents.Count > 0)
            return;

        //月会时间检测
        if (MonthMeetingTime == 0)
        {
            MonthMeeting.Instance.StartMeeting();
            return;
        }

        //商战时间检测
        if (WarTime == 0)
        {
            foeControl.PrepareCWar();
            return;
        }

        //航线检测
        if (CrC.ShipMoved == false)
        {
            CrC.SetCourse();
            return;
        }
        //扣维护费
        int value = CalcCost();
        if (value > 0)
            Money -= value;
        Money += Income + 50;
        Turn += 1;
        MonthMeetingTime -= 1;
        WarTime -= 1;
       
        if (AdjustTurn == 0)
        {
            AdjustTurn = 2;
            AdjustTurnWarning.SetActive(true);
        }
        //原调整回合计算（现改为月会结束后直接进入调整回合）
        //else
        //{
        //    AdjustTurn -= 1;
        //    AdjustTurnWarning.SetActive(false);
        //    //进入调整回合后恢复心理咨询室的员工的心力
        //    if (AdjustTurn == 0)
        //        PsycholCDep.RestoreMentality();
        //}
        TimeChange();
        UpdateUI();

        //大概在这里确定所有的事件

        CheckButtonName();
        TurnEvent.Invoke();
        FirstExhausted = false;
        EC.EventGroupIndex = 0;
        EC.StartEventQueue = true;
        EC.StartSpecialEvent();
        //CEO体力回复
        CC.CEO.Stamina += 10;
    }

    public void CheckButtonName()
    {
        if(EC.UnfinishedEvents.Count > 0 || ForceTimePause == true)
        {
            Text_NextTurn.text = "处理事件";
            return;
        }

        if (MonthMeetingTime == 0)
        {
            Text_NextTurn.text = "开始月会";
            return;
        }

        else if (WarTime == 0)
        {
            Text_NextTurn.text = "开始商战";
            return;
        }

        else if (CrC.ShipMoved == false)
        {
            Text_NextTurn.text = "开始航行";
            return;
        }
        else
            Text_NextTurn.text = "下一回合";
    }

    //(回合制下的)时间变化
    void TimeChange()
    {
        Month += 1;
        if (Month > 12)
        {
            Month = 1;
            Year += 1;
        }
    }

    //计算支出
    public int CalcCost()
    {
        int value = 0;
        foreach (DivisionControl div in CurrentDivisions)
        {
            value += (div.CalcCost(1) + div.CalcCost(2) + div.ExtraCost);
        }
        foreach(Employee emp in CurrentEmployees)
        {
            if (emp.CurrentDep == null && emp.CurrentDivision == null)
                value += emp.InfoDetail.CalcSalary();
        }
        value += PsycholCDep.CalcCost(1);
        value += EngineDep.CalcCost(1);

        value += ExtraCost;
        return value;
    }

    //计算公司的整体效率和工作状态
    public void CalcCompanyEW()
    {
        TotalEfficiency = 0;
        TotalWorkStatus = 0;
        foreach (DivisionControl div in CurrentDivisions)
        {
            if (div.CurrentDeps.Count == 0)
                continue;
            TotalEfficiency += (div.ExtraEfficiency + div.Efficiency);
            TotalWorkStatus += (div.ExtraWorkStatus + div.WorkStatus);
        }
        Text_CompanyEfficiency.text = "公司整体效率:" + (TotalEfficiency + ExtraEfficiency);
        Text_CompanyWorkStatus.text = "公司整体工作状态:" + (TotalWorkStatus + ExtraWorkStatus);
    }

    public DepControl CreateDep(Building b)
    {
        DepControl newDep;
        //心理咨询室需要额外设定
        if (b.Type == BuildingType.心理咨询室)
        {
            newDep = Instantiate(PCDepPrefab, this.transform);
            PsycholCDep = newDep;
            TurnEvent.AddListener(newDep.CloseRemoveButton);
            UIManager.Instance.OnAddNewWindow(newDep.DivPanel.GetComponent<WindowBaseControl>());
        }
        else
            newDep = Instantiate(DepPrefab, this.transform);

        if (b.Type == BuildingType.动力舱)
            EngineDep = newDep;
        newDep.transform.parent = DepContent;
        newDep.building = b;
        UIManager.Instance.OnAddNewWindow(newDep.EmpPanel.GetComponent<WindowBaseControl>());

        //部门命名
        string newDepName = b.Type.ToString();
           
        //根据Excel设置
        //设置员工上限
        newDep.SetDepStatus();

        int num = 1;
        for(int i = 0; i < CurrentDeps.Count; i++)
        {
            if (CurrentDeps[i].building.Type == newDep.building.Type)
                num += 1;
        }
        newDep.Text_DepName.text = newDepName + num;
        newDep.Text_DepName2.text = newDepName + num;
        if (b.Type != BuildingType.商战建筑)
            newDep.Text_DepName3.text = newDepName + num + "<" + b.Require_A + ">";
        else
            newDep.Text_DepName3.text = newDepName + num;
        newDep.GC = this;
        HourEvent.AddListener(newDep.Produce);

        //创建对应按钮
        newDep.DS = Instantiate(DepSelectButtonPrefab, DepSelectContent);
        newDep.DS.Text_DepName.text = newDep.Text_DepName.text;
        newDep.DS.DC = newDep;
        newDep.DS.GC = this;

        //检测老板摸鱼的附加状态
        CurrentDeps.Add(newDep);
        return newDep;
    }

    //员工招聘和移动时部门选择(ShowDivision表示是否显示事业部选项，只有在头脑风暴面板和事业部面板才能选择事业部)
    public void ShowDepSelectPanel(Employee emp, bool ShowDivision = false)
    {
        //有事件没处理时不能选择
        if(EC.UnfinishedEvents.Count > 0)
        {
            ResetSelectMode();
            return;
        }

        DepSelectPanel.GetComponent<WindowBaseControl>().SetWndState(true);

        if (AdjustTurn == 0)
            StandbyButton.SetActive(true);
        else
        {
            //非调整回合如果是招募则可以待命，否则不能待命
            if (CurrentEmployees.Contains(emp) == false)
                StandbyButton.SetActive(true);
            else
                StandbyButton.SetActive(false);
        }

        for (int i = 0; i < CurrentDeps.Count; i++)
        {
            //如果员工已经在该部门或部门人数达到上限则不显示
            if (CurrentDeps[i] == emp.CurrentDep || CurrentDeps[i].CheckEmpNum() == false)
                CurrentDeps[i].DS.gameObject.SetActive(false);
            //非调整回合时不显示非独立建筑
            else if (AdjustTurn != 0 && CurrentDeps[i].building.IndependentBuilding == false)
                CurrentDeps[i].DS.gameObject.SetActive(false);
            else
            {
                if (CurrentDeps[i].CheckSkillType(emp) == 0 && CurrentDeps[i].building.Type != BuildingType.动力舱)
                {
                    CurrentDeps[i].DS.Text_DepName.color = Color.red;
                    //心理咨询室只能放符合条件的员工
                    if (CurrentDeps[i].building.Type == BuildingType.心理咨询室)
                    {
                        CurrentDeps[i].DS.gameObject.SetActive(false);
                        continue;
                    }
                }
                else
                    CurrentDeps[i].DS.Text_DepName.color = Color.black;
                CurrentDeps[i].DS.gameObject.SetActive(true);
            }
        }
        foreach(DivisionControl div in CurrentDivisions)
        {
            if (ShowDivision == false)
                div.DS.gameObject.SetActive(false);
            else if (div.Manager != null || div.Locked == true || div.CurrentDeps.Count == 0)
                div.DS.gameObject.SetActive(false);
            else
                div.DS.gameObject.SetActive(true);
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
        foreach(DivisionControl div in CurrentDivisions)
        {
            div.DS.gameObject.SetActive(false);
        }
    }

    //显示所有已激活事业部
    public void ShowDivSelectPanel()
    {
        DepSelectPanel.GetComponent<WindowBaseControl>().SetWndState(true);
        StandbyButton.SetActive(false);
        foreach (DepControl dep in CurrentDeps)
        {
            dep.DS.gameObject.SetActive(false);
        }
        foreach (DivisionControl div in CurrentDivisions)
        {
             if (div.Manager == null || div.Locked == true || div.CurrentDeps.Count == 0)
                div.DS.gameObject.SetActive(false);
            else
                div.DS.gameObject.SetActive(true);
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
            if(CurrentEmpInfo.emp.CurrentDivision != null)
            {
                //离任高管投票检测
                if (EC.ManagerVoteCheck(CurrentEmpInfo.emp, true) == false)
                    return;
                CurrentEmpInfo.emp.CurrentDivision.SetManager(true);
            }
            CurrentEmpInfo.emp.InfoA.transform.parent = StandbyContent;
            ResetOldAssignment();
            //CurrentEmpInfo.DetailInfo.Entity.FindWorkPos();
        }
        //调动信息
        if (SelectMode == 1 || SelectMode == 2)
        {
            CurrentEmpInfo.emp.InfoDetail.AddHistory("调动至待命室");
            if (CurrentEmpInfo.emp.isCEO == false)
                CC.CEO.InfoDetail.AddHistory("将" + CurrentEmpInfo.emp.Name + "调动至待命室");
            EmpManager.Instance.CheckRelation(CurrentEmpInfo.emp);
        }
    }

    //将移动或雇佣的员工放入特定部门 + 选择部门发动建筑特效 + CEO技能发动 + 确认领导部门(3) + 物品选择(11)
    public void SelectDep(DepControl depControl)
    {
        DepSelectPanel.GetComponent<WindowBaseControl>().SetWndState(false);
        //雇佣直接加入
        if (SelectMode == 1)
        {
            QC.Finish(8);
            HC.SetInfoPanel();
            CurrentEmpInfo.emp.InfoA.transform.parent = StandbyContent;//不管后面有无投票，投票结果如何都先把infoA放好
            CurrentEmpInfo.emp.InfoA.transform.parent = depControl.EmpContent;
        }
        //移动
        else if (SelectMode == 2)
        {
            //离任高管投票检测
            if (CurrentEmpInfo.emp.CurrentDivision != null)
            {
                if (EC.ManagerVoteCheck(CurrentEmpInfo.emp, true) == false)
                    return;
            }
            CurrentEmpInfo.emp.InfoA.transform.parent = depControl.EmpContent;
            ResetOldAssignment();
        }
        //选择部门发动建筑特效
        else if (SelectMode == 5)
        {
            SelectedDep = depControl;
            CurrentDep.BuildingActive();
        }
        //CEO技能
        else if (SelectMode == 6)
        {
            if (CC.CEOSkillNum == 1)
            {
                //depControl.AddPerk(new Perk117());
                QC.Finish(5);
                new EmpBuff(CC.CEO, 16, -45);
                ResetSelectMode();
                QC.Init("技能释放成功\n\n" + depControl.Text_DepName.text + "成功率上升45%");
            }
        }
        //物品使用
        else if (SelectMode == 11)
        {
            CurrentItem.item.TargetDep = depControl;
            CurrentItem.UseItem();
            ResetSelectMode();
        }
        //调动信息设置
        if (SelectMode == 1 || SelectMode == 2)
        {
            CurrentEmpInfo.emp.ProfessionUse = 1;
            CurrentEmpInfo.emp.CurrentDep = depControl;
            depControl.CurrentEmps.Add(CurrentEmpInfo.emp);
            CurrentEmpInfo.emp.CurrentDep.EmpEffectCheck();

            //独立建筑没有事业部所以需要额外检测是否有所属事业部
            if (CurrentEmpInfo.emp.CurrentDep.CurrentDivision != null)
            {
                foreach (PerkInfo perk in CurrentEmpInfo.emp.InfoDetail.PerksInfo)
                {
                    if (perk.CurrentPerk.DepPerk == true)
                        perk.CurrentPerk.ActiveSpecialEffect();
                }
                CurrentEmpInfo.emp.CurrentDep.CurrentDivision.SpecialPerkCheck();
            }
            //调动信息历史添加
            CurrentEmpInfo.emp.InfoDetail.AddHistory("调动至" + depControl.Text_DepName.text);
            if (CurrentEmpInfo.emp.isCEO == false)
                CC.CEO.InfoDetail.AddHistory("将" + CurrentEmpInfo.emp.Name + "调动至" + depControl.Text_DepName.text);
            EmpManager.Instance.CheckRelation(CurrentEmpInfo.emp);
            ResetSelectMode();
        }
    }

    //为事业部指定高管 + 为事业部使用物品
    public void SelectDivManager(DivisionControl div)
    {
        DepSelectPanel.GetComponent<WindowBaseControl>().SetWndState(false);
        //高管指定
        if (SelectMode == 2)
        {
            //确认是否通过投票
            if (EC.ManagerVoteCheck(CurrentEmpInfo.emp) == false)
                return;
            CurrentEmpInfo.emp.InfoA.transform.parent = StandbyContent;
            //如果是移动的话重置之前的部门
            ResetOldAssignment();

            div.SetManager(false, CurrentEmpInfo.emp);
            //调动信息历史添加
            CurrentEmpInfo.emp.InfoDetail.AddHistory("调动至" + div.DivName);
            if (CurrentEmpInfo.emp.isCEO == false)
                CC.CEO.InfoDetail.AddHistory("将" + CurrentEmpInfo.emp.Name + "调动至" + div.DivName);
            ResetSelectMode();
        }
        //物品使用
        else if (SelectMode == 11)
        {
            CurrentItem.item.TargetDiv = div;
            CurrentItem.UseItem();
            ResetSelectMode();
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
            //如果是心理咨询室的被回复员工，则特殊处理
            if (emp.CurrentDep.building.Type == BuildingType.心理咨询室)
            {
                //作为被回复的员工时将自身从槽位中移除
                if (emp.CurrentDep.AffectedEmps.Contains(emp) == true)
                {
                    emp.CurrentDep.RemovePCTarget(emp.CurrentDep.AffectedEmps.IndexOf(emp));
                    return;
                }
            }
            foreach (PerkInfo perk in emp.InfoDetail.PerksInfo)
            {
                if (perk.CurrentPerk.DepPerk == true)
                    perk.CurrentPerk.DeActiveSpecialEffect();
            }
            emp.CurrentDep.CurrentEmps.Remove(emp);

            if (emp.CurrentDep.CurrentDivision != null)
                emp.CurrentDep.CurrentDivision.SpecialPerkCheck();

            //修改生产力显示
            emp.CurrentDep.UpdateUI();
            emp.CurrentDep.EmpEffectCheck();
            emp.CurrentDep = null;
            emp.SpecialPerkEffect();
        }
        else if (emp.CurrentDivision != null)
            CurrentEmpInfo.emp.CurrentDivision.SetManager(true);
    }

    //更新资源面板
    public void UpdateResourceInfo()
    {
        int[] C = FinishedTask;
        Text_TechResource.text = "迭代: " + C[0] + "\n升级架构:" + C[1] + "\n大数据:" + C[2];
        Text_ProductResource.text = "访谈: " + C[3] + "\n原型图B:" + C[4] + "\n原型图C:" + C[5];
        Text_MarketResource.text = "营销: " + C[6] + "\n很多传单:" + C[7] + "\n地推:" + C[8];
        Text_MarketResource2.text = "曝光: " + C[9];

    }

    //创建左侧信息栏内容
    public void CreateMessage(string content)
    {
        Text T = Instantiate(MessagePrefab, MessageContent).GetComponentInChildren<Text>();
        T.text = content;
    }

    //重置选择模式以及部分相关变量
    public void ResetSelectMode()
    {
        SelectMode = 0;
        CC.CEOSkillNum = 0;
        Text_EmpSelectTip.gameObject.SetActive(false);
        CC.SelectPanel.GetComponent<WindowBaseControl>().SetWndState(false);
        CC.ResultPanel.GetComponent<WindowBaseControl>().SetWndState(false); ;
        for(int i = 0; i < CurrentEmployees.Count; i++)
        {
            CurrentEmployees[i].InfoB.gameObject.SetActive(true);
            CurrentEmployees[i].InfoB.gameObject.GetComponent<Button>().interactable = true;
            CurrentEmployees[i].InfoB.Text_CoreMemberCD.gameObject.SetActive(false);
            CurrentEmployees[i].InfoB.MoveButton.gameObject.SetActive(true);
            if (CurrentEmployees[i].isCEO == false)
                CurrentEmployees[i].InfoB.FireButton.gameObject.SetActive(true);
        }
        CurrentEmpInfo = null;
        CurrentDep = null;
    }

    //选两个员工时取消选择
    public void CancelEmpSelect()
    {
        if(SelectMode == 4 || SelectMode == 11 || SelectMode == 13)
        {
            TotalEmpPanel.SetWndState(false);
            ResetSelectMode();
        }
        else if (SelectMode == 6 || SelectMode == 12 )
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

    public void ToggleEmpEmotionGraph(bool value)
    {
        ShowEmpEmotion = value;
        foreach (Employee emp in CurrentEmployees)
        {
            emp.InfoDetail.Entity.EmotionImage.gameObject.SetActive(ShowEmpEmotion);
        }
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
        int otherCost = 0;
        foreach(Employee emp in CurrentEmployees)
        {
            if(emp.CurrentDep == null)
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

    //生成物品
    public void CreateItem(int num)
    {
        CompanyItem newItem = Instantiate(ItemPrefab, ItemContent);
        newItem.GC = this;
        newItem.SetType(num);
        Items.Add(newItem);
        ItemCountCheck();
    }
    //物品数量监测
    public void ItemCountCheck()
    {
        if (Items.Count > ItemLimit)
        {
            for(int i = 0; i < Items.Count; i++)
            {
                if (i < ItemLimit)
                    Items[i].gameObject.GetComponent<Image>().color = Color.white;
                else
                    Items[i].gameObject.GetComponent<Image>().color = Color.red;
            }
        }
    }

    //增加不满或认同,bool = true 为不满，反之是认同
    public void AddEventProgress(int value, bool dissatisfied)
    {
        if (dissatisfied == true)
        {
            Dissatisfied += value;
            CPC.DebuffEffect(136);
        }
        else
            Approve += value;
    }

    //检测不满和认同状态，满了就产生事件组
    public void CheckEventProgress()
    {
        if (Dissatisfied >= DissatisfiedLimit)
        {
            Dissatisfied -= DissatisfiedLimit;
            EC.CreateEventGroup(EventData.SpecialEventGroups2[Random.Range(0, EventData.SpecialEventGroups2.Count)]);
        }
        if (Approve >= ApproveLimit)
        {
            Approve -= ApproveLimit;
            EC.CreateEventGroup(EventData.SpecialEventGroups[Random.Range(0, EventData.SpecialEventGroups.Count)]);
        }
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
        CheckButtonName();
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
        CheckButtonName();
    }

    //生成志向选择面板（选择岗位优势）
    public void CreateAmbitionSelectPanel(Employee emp)
    {
        AmbitionSelect As = Instantiate(AmbitionPanelPrefab, this.transform);
        As.SetEmp(emp);
    }
}
