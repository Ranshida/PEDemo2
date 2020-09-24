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
    public int SelectMode = 1; //1员工招聘时部门选择 2员工移动时部门选择 3部门的高管办公室选择 4发动动员技能时员工选择 5发动建筑技能时员工选择 6CEO技能员工/部门选择
    public int Money = 1000, CEOSkillNum = 0, DoubleMobilizeCost = 0, MeetingBlockTime = 0;
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

    [HideInInspector] public EmpInfo CurrentEmpInfo;
    [HideInInspector] public DepControl CurrentDep;
    [HideInInspector] public OfficeControl CurrentOffice;
    public EmpEntity EmpEntityPrefab;
    public PerkInfo PerkInfoPrefab;
    public SkillInfo SkillInfoPrefab;
    public DepControl DepPrefab, LabPrefab, HRDepPrefab;
    public OfficeControl OfficePrefab;
    public DepSelect DepSelectButtonPrefab, OfficeSelectButtonPrefab;
    public RelationInfo RelationInfoPrefab;
    public Text HistoryTextPrefab;
    public HireControl HC;
    public BuildingManage BM;
    public ProduceControl PC, PC2;
    public ProductControl PrC;
    public StrategyControl StrC;
    public FOEControl foeControl;
    public EventControl EC;
    public Transform HireContent, EmpPanelContent, DepContent, DepSelectContent, TotalEmpContent, StandbyContent, EmpDetailContent, MessageContent;
    public InfoPanel infoPanel;
    public GameObject DepSelectPanel, StandbyButton, MessagePrefab, CEOSkillPanel, EmpTrainingPanel, GameOverPanel;
    public Text Text_Time, Text_TechResource, Text_MarketResource, Text_ProductResource, Text_Money, Text_Stamina, Text_Mentality, Text_Morale;
    public SkillControl SC;
    [HideInInspector] public UnityEvent DailyEvent, WeeklyEvent, MonthlyEvent, HourEvent;

    public Button[] CEOSkillButton = new Button[5];
    public Text[] Text_CEOSkillCD = new Text[5];
    public List<DepControl> CurrentDeps = new List<DepControl>();
    public List<OfficeControl> CurrentOffices = new List<OfficeControl>();
    public List<Employee> CurrentEmployees = new List<Employee>();
    public int[] FinishedTask = new int[10];

    Animator Anim;

    int Year = 1, Month = 1, Week = 1, Day = 1, Hour = 1, morale = 100, SpecialEventCount = 0;
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
        HC.AddHireTypes(new HireType(1));
        SpecialEventCount = Random.Range(1, 6);//随机一段时间发生影响产品的事件
        HourEvent.AddListener(GCTimePass);      
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
        //if (CurrentEmployees.Count > 0)
        //{
        //    TimePause = true;
        //    WorkEndEmpCount = CurrentEmployees.Count;
        //    for (int i = 0; i < CurrentEmployees.Count; i++)
        //    {
        //        CurrentEmployees[i].InfoDetail.Entity.WorkEnd();
        //    }
        //}
        //else
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
        TimePause = false;
        //for(int i = 0; i < CurrentEmployees.Count; i++)
        //{
        //    CurrentEmployees[i].InfoDetail.Entity.WorkStart();
        //}
    }

    public DepControl CreateDep(int type)
    {
        DepControl newDep;
        if (type < 4)
            newDep = Instantiate(DepPrefab, this.transform);
        else if(type == 4)
            newDep = Instantiate(LabPrefab, this.transform);
        else
            newDep = Instantiate(HRDepPrefab, this.transform);

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
        else if (type == 5)
        {
            newDep.type = EmpType.HR;
            newDep.EmpLimit = 2;
            newDepName = "人力资源部B";
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
    //办公室领导选择
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

    //将移动或雇佣的员工放入特定办公室 + 确定部门领导者 + CEO技能发动
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
            else
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
            }
            CurrentDep.CommandingOffice = office;
            CurrentDep.Text_Office.text = "由 " + office.Text_OfficeName.text + " 管理";
            office.ControledDeps.Add(CurrentDep);
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

    void ResetOldAssignment()
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
            CEOSkillNum = 3;
            CEOSkillPanel.SetActive(false);
            List<OfficeControl> TempOffices = new List<OfficeControl>();
            for(int i = 0; i < CurrentOffices.Count; i++)
            {
                BuildingType T = CurrentOffices[i].building.Type;
                if (T == BuildingType.目标修正小组 || T == BuildingType.档案管理室 || T == BuildingType.效能研究室 || T == BuildingType.财务部 
                    || T == BuildingType.战略咨询部B || T == BuildingType.精确标准委员会)
                {
                    TempOffices.Add(CurrentOffices[i]);
                }
            }
            ShowDepSelectPanel(TempOffices);
            SelectMode = 6;
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
}
