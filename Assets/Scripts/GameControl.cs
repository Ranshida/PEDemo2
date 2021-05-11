using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameControl : MonoBehaviour
{
    public static GameControl Instance;
    [HideInInspector] public int Salary, Income, BuildingPay, MobilizeExtraMent = 0, ExtraDice = 0, TimeMultiply = 1, WorkEndEmpCount = 0;
    [HideInInspector] public float EfficiencyExtraScience = 0, ExtrafailRate = 0, TotalSalaryMultiply = 1.0f,
        HRBuildingMentalityExtra = 1.0f, BuildingSkillSuccessExtra = 0, TotalBuildingPayMultiply = 1.0f, HireSuccessExtra = 0, 
        BaseDepExtraSuccessRate = 0;
    public int SelectMode = 1; //1员工招聘时部门选择 2员工移动时部门选择 3事件组特别小组成员选择 4发动动员技能时员工选择 
    //5发动建筑技能时员工选择 6CEO技能员工/部门选择 7选择两个员工发动动员技能 8普通办公室的上级(高管办公室)选择  9头脑风暴的员工选择
    //10选择两个员工的CEO技能 11部门/员工的物品使用
    public int AreaSelectMode = 1;//1部门目标区域的选择（暂时删除）  2物品目标区域的选择
    public int Money = 1000, DoubleMobilizeCost = 0, MeetingBlockTime = 0, MonthMeetingTime = 3;
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
    public int Turn = 1;//当前回合数
    public int ItemLimit = 6;//物品数量限制
    public int StandbyEmpLimit = 5;//人才库数量限制

    #region 杂项变量
    [HideInInspector] public bool ProduceBuffBonus = false;//“精进”和“团结”状态的持续时间提高至4m战略效果
    [HideInInspector] public bool GymBuffBonus = false;//“健身房”大成功时获得“强化”或“铁人”状态的概率上升为80%战略效果
    [HideInInspector] public bool WorkToggle = false;//下周开始是否加班
    [HideInInspector] public bool NeutralOccupieBonus = false;//商战中立占领Buff加成
    [HideInInspector] public bool TargetOccupieBonus = false;//商战反占领Buff加成
    [HideInInspector] public bool CEOExtraVote = false;//CEO投票是否有额外加成
    [HideInInspector] public bool ResearchExtraMentality = false;//科研额外心力恢复
    [HideInInspector] public bool CEOVacation = false;
    bool WorkOverTime = false;//是否已经处于加班状态
    #endregion

    [HideInInspector] public EmpInfo CurrentEmpInfo, CurrentEmpInfo2;//2主要用于需要两个员工的动员技能
    public DepControl CurrentDep, SelectedDep;
    public QuestControl QC;
    public EmpEntity EmpEntityPrefab;
    public PerkInfo PerkInfoPrefab;
    public DepControl DepPrefab, HRDepPrefab;
    public DepSelect DepSelectButtonPrefab;
    public RelationInfo RelationInfoPrefab;
    public Text HistoryTextPrefab, Text_EmpSelectTip;
    public CompanyItem ItemPrefab, CurrentItem;
    public HireControl HC;
    public BuildingManage BM;
    public StrategyControl StrC;
    public FOEControl foeControl;
    public EventControl EC;
    public OptionCardLibrary OCL;
    public CEOControl CC;
    public Areas AC;
    public WindowBaseControl TotalEmpPanel;
    public Transform DepContent, DepSelectContent, StandbyContent, MessageContent, ItemContent;
    public InfoPanel infoPanel;
    public GameObject DepSelectPanel, StandbyButton, MessagePrefab, GameOverPanel;
    public Text Text_Time, Text_TechResource, Text_MarketResource, Text_MarketResource2, Text_ProductResource, Text_Money, 
        Text_Stamina, Text_Mentality, Text_Morale, Text_WarTime, Text_MonthMeetingTime, Text_NextTurn;
    public Toggle WorkOvertimeToggle;
    public BrainStormControl BSC;
    [HideInInspector] public UnityEvent DailyEvent, WeeklyEvent, MonthlyEvent, HourEvent, YearEvent, TurnEvent;

    public List<DepControl> CurrentDeps = new List<DepControl>();
    public List<Employee> CurrentEmployees = new List<Employee>();
    public List<CompanyItem> Items = new List<CompanyItem>();
    public List<DivisionControl> CurrentDivisions = new List<DivisionControl>();
    public int[] FinishedTask = new int[10];//0程序迭代 1技术研发 2可行性调研 3传播 4营销文案 5资源拓展 6原型图 7产品研究 8用户访谈 9已删除

    int Year = 1, Month = 1, Week = 1, Day = 1, Hour = 1, morale = 50;
    int HireTime = 3;//临时的每3回合刷新一次招聘
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
        OCL.AddStaticOptions(new OptionCard1());
        OCL.AddStaticOptions(new OptionCard2());
        OCL.AddStaticOptions(new OptionCard3());
        //CreateItem(3);
        //CreateItem(4);
        //CreateItem(5);
        //CreateItem(6);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            HC.AddHireTypes(new HireType());
        //if (TimePause == false && ForceTimePause == false)
        //    Timer += Time.deltaTime * TimeMultiply;
        //if(Timer >= 10)
        //{
        //    Timer = 0;
        //    HourPass();
        //}
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

    public void NextTurn()
    {
        if (ForceTimePause == true)
            return;
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

        Turn += 1;
        MonthMeetingTime -= 1;

        //临时招聘
        HireTime -= 1;
        if (HireTime == 0)
        {
            HireTime = 3;
            HC.AddHireTypes(new HireType());
            HC.Refresh();
            HC.Text_HireButtonText.transform.parent.GetComponent<Button>().interactable = true;
        }
        else
        {
            HC.Text_HireButtonText.transform.parent.GetComponent<Button>().interactable = false;
            HC.Text_HireButtonText.color = Color.black;
            HC.StorePanel.SetWndState(false);//玩家直接进下一回合的话强制关闭
        }

        Text_Time.text = "第" + Turn + "回合";
        Text_MonthMeetingTime.text = "距离下次月会还剩" + MonthMeetingTime + "回合";

        //大概在这里确定所有的事件

        CheckButtonName();
        TurnEvent.Invoke();
        EC.EventGroupIndex = 0;
        EC.StartEventQueue = true;
        EC.StartSpecialEvent();
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
        else
            Text_NextTurn.text = "下一回合";
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
        }
        //加班时间只进行工作和事件计算
        else if (WorkOverTime == true && Hour > 8)
        {
            foreach(DepControl dep in CurrentDeps)
            {
                dep.Produce();
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
        //部门的每周体力buff判定
        foreach(DepControl dep in CurrentDeps)
        {
            if (dep.StaminaExtra != 0)
            {
                foreach (Employee e in dep.CurrentEmps)
                {
                    e.Stamina += (int)(dep.StaminaExtra * dep.StaminaCostRate);
                }
            }
        }

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

        if (Week > 4)
        {
            Month += 1;
            Week = 1;
            Money = Money + Income - Salary - (int)(BuildingPay * TotalBuildingPayMultiply);
            MonthlyEvent.Invoke();
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
        newDep.transform.parent = DepContent;
        newDep.building = b;
        UIManager.Instance.OnAddNewWindow(newDep.EmpPanel.GetComponent<WindowBaseControl>());

        //部门命名
        string newDepName = b.Type.ToString();

            

        //根据Excel设置
        //设置员工上限
        newDep.SetDepStatus(int.Parse(b.Jobs));

        int num = 1;
        for(int i = 0; i < CurrentDeps.Count; i++)
        {
            if (CurrentDeps[i].building.Type == newDep.building.Type)
                num += 1;
        }
        newDep.Text_DepName.text = newDepName + num;
        newDep.Text_DepName2.text = newDepName + num;
        newDep.Text_DepName3.text = newDepName + num + "<" + b.Require_A + ">";
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
            newDep.AddPerk(new Perk115());
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

        //人才库员工数量检查（最好单独记一下待命员工数量？）
        int StandbyCount = 0;
        foreach(Employee e in CurrentEmployees)
        {
            if (e.CurrentDep == null && e.CurrentDivision == null)
                StandbyCount += 1;
        }
        if (StandbyCount >= StandbyEmpLimit || emp.isCEO == true)
            StandbyButton.SetActive(false);
        else
            StandbyButton.SetActive(true);

        for (int i = 0; i < CurrentDeps.Count; i++)
        {
            if (CurrentDeps[i] == emp.CurrentDep || CurrentDeps[i].CheckEmpNum() == false)
                CurrentDeps[i].DS.gameObject.SetActive(false);
            else
            {
                CurrentDeps[i].DS.gameObject.SetActive(true);
                if (CurrentDeps[i].CheckSkillType(emp) == false)
                    CurrentDeps[i].DS.Text_DepName.color = Color.red;
                else
                    CurrentDeps[i].DS.Text_DepName.color = Color.black;
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
        }
    }

    //将移动或雇佣的员工放入特定部门 + 选择部门发动建筑特效 + CEO技能发动 + 确认领导部门(3) + 物品选择(11)
    public void SelectDep(DepControl depControl)
    {
        DepSelectPanel.GetComponent<WindowBaseControl>().SetWndState(false);
        if (SelectMode == 1)
        {
            QC.Finish(8);
            HC.SetInfoPanel();
            CurrentEmpInfo.emp.InfoA.transform.parent = StandbyContent;//不管后面有无投票，投票结果如何都先把infoA放好
            CurrentEmpInfo.emp.InfoA.transform.parent = depControl.EmpContent;
        }
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
                depControl.AddPerk(new Perk117());
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
            CurrentEmpInfo.emp.CurrentDep = depControl;
            depControl.CurrentEmps.Add(CurrentEmpInfo.emp);
            CurrentEmpInfo.emp.CurrentDep.EmpMove(false);
            foreach(PerkInfo perk in CurrentEmpInfo.emp.InfoDetail.PerksInfo)
            {
                if (perk.CurrentPerk.DepPerk == true)
                    perk.CurrentPerk.ActiveSpecialEffect();
            }
            //调动信息历史添加
            CurrentEmpInfo.emp.InfoDetail.AddHistory("调动至" + depControl.Text_DepName.text);
            if (CurrentEmpInfo.emp.isCEO == false)
                CC.CEO.InfoDetail.AddHistory("将" + CurrentEmpInfo.emp.Name + "调动至" + depControl.Text_DepName.text);
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
            foreach (PerkInfo perk in emp.InfoDetail.PerksInfo)
            {
                if (perk.CurrentPerk.DepPerk == true)
                    perk.CurrentPerk.DeActiveSpecialEffect();
            }
            emp.CurrentDep.CurrentEmps.Remove(emp);
            //修改生产力显示
            emp.CurrentDep.UpdateUI();
            CurrentEmpInfo.emp.CurrentDep.EmpMove(true);
            emp.CurrentDep = null;
        }
        else if (CurrentEmpInfo.emp.CurrentDivision != null)
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
            CurrentEmployees[i].InfoB.MoveButton.GetComponentInChildren<Text>().text = "移动";
            if (CurrentEmployees[i].isCEO == false)
                CurrentEmployees[i].InfoB.FireButton.gameObject.SetActive(true);

        }
    }

    //选两个员工时取消选择
    public void CancelEmpSelect()
    {
        if(SelectMode == 4 || SelectMode == 11)
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
        //不能持有超过6个物品
        if (Items.Count >= ItemLimit)
            return;
        else
        {
            CompanyItem newItem = Instantiate(ItemPrefab, ItemContent);
            newItem.GC = this;
            newItem.SetType(num);
            Items.Add(newItem);
        }
    }

    //增加不满或认同,bool = true 为不满，反之是认同
    public void AddEventProgress(int value, bool dissatisfied)
    {

    }

    public void CheckEventProgress()
    {

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

}
