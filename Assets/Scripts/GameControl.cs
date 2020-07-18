using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameControl : MonoBehaviour
{
    public int SelectMode = 1;


    [HideInInspector] public EmpInfo CurrentEmpInfo;
    [HideInInspector] public DepControl CurrentDep;
    public EmpInfo EmpInfoPrefab, EmpDetailPrefab;
    public PerkInfo PerkInfoPrefab;
    public SkillInfo SkillInfoPrefab;
    public DepControl DepPrefab;
    public OfficeControl OfficePrefab;
    public DepSelect DepSelectButtonPrefab, OfficeSelectButtonPrefab;
    public ProduceControl PC;
    public Transform HireContent, EmpPanelContent, DepContent, DepSelectContent, TotalEmpContent, StandbyContent, EmpDetailContent;
    public InfoPanel infoPanel;
    public GameObject DepSelectPanel, StandbyButton;
    public Text Text_Time;
    public SkillControl SC;
    [HideInInspector] public UnityEvent DailyEvent, WeeklyEvent, MonthlyEvent;

    public EmpInfo[] HireInfos = new EmpInfo[5];
    public List<DepControl> CurrentDeps = new List<DepControl>();
    public List<OfficeControl> CurrentOffices = new List<OfficeControl>();
    public List<Employee> CurrentEmployees = new List<Employee>();
    public List<Task> FinishedTask = new List<Task>();

    int Year = 1, Month = 1, Week = 1, Day = 1, Hour = 1;
    float Timer;

    private void Update()
    {
        Timer += Time.deltaTime * 10;
        if(Timer >= 2)
        {
            Timer = 0;
            HourPass();
        }
    }

    void HourPass()
    {
        Hour += 1;
        for(int i = 0; i < CurrentDeps.Count; i++)
        {
            CurrentDeps[i].Produce();
        }
        if (Hour > 8)
        {
            for(int i = 0; i < CurrentDeps.Count; i++)
            {
                CurrentDeps[i].OneDayPass();
            }
            DailyEvent.Invoke();
            Day += 1;
            Hour = 1;
        }
        if (Day > 5)
        {
            Week += 1;
            Day = 1;
            WeeklyEvent.Invoke();
        }
        if(Week > 4)
        {
            Month += 1;
            Week = 1;
            MonthlyEvent.Invoke();
        }
        if(Month > 12)
        {
            Year += 1;
            Month = 1;
        }
        Text_Time.text = "年" + Year + " 月" + Month + " 周" + Week + " 日" + Day + " 时" + Hour;
    }

    public DepControl CreateDep(int type)
    {
        DepControl newDep = Instantiate(DepPrefab, this.transform);
        newDep.EmpPanel.parent = EmpPanelContent;
        newDep.transform.parent = DepContent;

        //部门命名
        string newDepName;
        if (type == 1)
        {
            newDep.type = EmpType.Tech;
            newDepName = "技术部门";
        }
        else if (type == 2)
        {
            newDep.type = EmpType.Market;
            newDepName = "市场部门";
        }
        else
        {
            newDep.type = EmpType.Product;
            newDepName = "产品部门";
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

    public OfficeControl CreateOffice()
    {
        OfficeControl newOffice = Instantiate(OfficePrefab, this.transform);
        newOffice.transform.parent = DepContent;
        CurrentOffices.Add(newOffice);
        newOffice.Text_OfficeName.text = "高管办公室" + CurrentOffices.Count;

        //创建对应按钮
        newOffice.DS = Instantiate(OfficeSelectButtonPrefab, DepSelectContent);
        newOffice.DS.Text_DepName.text = newOffice.Text_OfficeName.text;
        newOffice.DS.OC = newOffice;
        newOffice.DS.GC = this;

        return newOffice;
    }

    public void ShowDepSelectPanel(EmpType type)
    {
        DepSelectPanel.SetActive(true);
        StandbyButton.SetActive(true);
        for(int i = 0; i < CurrentDeps.Count; i++)
        {
            if (CurrentDeps[i].type != type)
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
        DepSelectPanel.SetActive(true);
        StandbyButton.SetActive(true);
        for (int i = 0; i < CurrentDeps.Count; i++)
        {
            if (CurrentDeps[i].type != emp.Type || CurrentDeps[i] == emp.CurrentDep)
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


    //员工管理 SelectMode 1雇佣 2移动 3指定高管（办公室）
    //InfoB常驻Total面板

    //将移动或雇佣的员工放入待命室
    public void SelectDep()
    {
        if (SelectMode == 1)
        {
            //还需要重新计算工资
            SetInfoPanel();
            CurrentEmpInfo.emp.InfoA.transform.parent = StandbyContent;
        }
        else if (SelectMode == 2)
        {
            if (CurrentEmpInfo == CurrentEmpInfo.emp.InfoB)
                CurrentEmpInfo = CurrentEmpInfo.emp.InfoA;
            CurrentEmpInfo.transform.parent = StandbyContent;
            ResetOldAssignment();
        }
        DepSelectPanel.SetActive(false);
    }

    //将移动或雇佣的员工放入特定办公室
    public void SelectDep(OfficeControl office)
    {
        if (SelectMode == 1)
        {
            //还需要重新计算工资
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
        }
        else if(SelectMode == 3)
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

    //将移动或雇佣的员工放入特定部门
    public void SelectDep(DepControl depControl)
    {
        if(SelectMode == 1)
        {
            //还需要重新计算工资
            depControl.CurrentEmps.Add(CurrentEmpInfo.emp);
            depControl.ShowProducePower();
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
            depControl.ShowProducePower();
        }
        DepSelectPanel.SetActive(false);
    }

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
        ED.SetSkillName();

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

    }

    void ResetOldAssignment()
    {
        if (CurrentEmpInfo.emp.CurrentDep != null)
        {
            CurrentEmpInfo.emp.CurrentDep.CurrentEmps.Remove(CurrentEmpInfo.emp);
            //修改生产力显示
            CurrentEmpInfo.emp.CurrentDep.ShowProducePower();
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
        EmpType EType;
        if (type == 1)
            EType = EmpType.Tech;
        else if (type == 2)
            EType = EmpType.Market;
        else if (type == 3)
            EType = EmpType.Product;
        else
            EType = EmpType.Operate;

        for(int i = 0; i < 5; i++)
        {
            foreach (Transform child in HireInfos[i].PerkContent)
            {
                Destroy(child.gameObject);
            }
            HireInfos[i].PerksInfo.Clear();
            HireInfos[i].SkillsInfo.Clear();
            HireInfos[i].CreateEmp(EType);
        }
    }
}
