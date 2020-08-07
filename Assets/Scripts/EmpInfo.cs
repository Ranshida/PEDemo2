using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmpInfo : MonoBehaviour
{
    [HideInInspector] public EmpEntity Entity;

    public Employee emp;
    public GameControl GC;
    public Button HireButton;
    public Text Text_Name, Text_Mentality, Text_Stamina, Text_Type, Text_Skill1, Text_Skill2, Text_Skill3,  Text_Ability;
    public Text Text_DepName, Text_Observation, Text_Tenacity, Text_Strength, Text_Manage, Text_HR, Text_Finance, Text_Decision, 
        Text_Forecast, Text_Strategy, Text_Convince, Text_Charm, Text_Gossip, Text_SName1, Text_SName2, Text_SName3;
    public Scrollbar[] Scrollbar_Character = new Scrollbar[5];
    public EmpInfo DetailInfo;
    public Transform PerkContent, SkillContent, StrategyContent, RelationContent, HistoryContent;

    public List<PerkInfo> PerksInfo = new List<PerkInfo>();
    public List<SkillInfo> SkillsInfo = new List<SkillInfo>();
    public List<StrategyInfo> StrategiesInfo = new List<StrategyInfo>();

    public int InfoType;

    void Update()
    {
        if (emp != null)
        {
            //小面板
            if (InfoType == 1)
            {
                Text_Mentality.text = "心力:" + emp.Mentality;
                Text_Stamina.text = "体力:" + emp.Stamina;
                Text_Skill1.text = "职业技能: " + (emp.Skill1 + emp.SkillExtra1) + " / " + (emp.Skill2 + emp.SkillExtra2) + 
                    " / " + (emp.Skill3 + emp.SkillExtra3);
                Text_Manage.text = "管理能力:" + emp.Manage;
                if (emp.CurrentDep != null)
                    Text_DepName.text = emp.CurrentDep.Text_DepName.text;
                else if (emp.CurrentOffice != null)
                    Text_DepName.text = emp.CurrentOffice.Text_OfficeName.text;
                else
                    Text_DepName.text = "无";
            }
            //雇佣和详细面板通用部分
            else
            {
                Text_Skill1.text = (emp.Skill1 + emp.SkillExtra1).ToString();
                Text_Skill2.text = (emp.Skill2 + emp.SkillExtra2).ToString();
                Text_Skill3.text = (emp.Skill3 + emp.SkillExtra3).ToString();
                Text_Observation.text = emp.Observation.ToString();
                Text_Tenacity.text = emp.Tenacity.ToString();
                Text_Strength.text = emp.Strength.ToString();
                Text_Manage.text = emp.Manage.ToString();
                Text_HR.text = emp.HR.ToString(); 
                Text_Finance.text = emp.Finance.ToString();
                Text_Decision.text = emp.Decision.ToString();
                Text_Forecast.text = emp.Forecast.ToString();
                Text_Strategy.text = emp.Strategy.ToString();
                Text_Convince.text = emp.Convince.ToString();
                Text_Charm.text = emp.Charm.ToString();
                Text_Gossip.text = emp.Gossip.ToString();
                UpdateCharacterUI();
            }
            //详细面板
            if(InfoType == 2)
            {
                Text_Mentality.text = "心力:" + emp.Mentality;
                Text_Stamina.text = "体力:" + emp.Stamina;
                if (emp.CurrentDep != null)
                    Text_DepName.text = "所属部门:" + emp.CurrentDep.Text_DepName.text;
                else if (emp.CurrentOffice != null)
                    Text_DepName.text = "所属部门:" + emp.CurrentOffice.Text_OfficeName.text;
                else
                    Text_DepName.text = "所属部门:无";
            }
        }
    }

    public void CreateEmp(EmpType type)
    {       
        emp = new Employee();
        emp.InfoA = this;
        emp.InitStatus(type);
        Text_Name.text = emp.Name;
        HireButton.interactable = true;
        SetSkillName();
        //AddPerk(new Perk5(emp));
        //AddPerk(new Perk1(emp));
        int Snum = Random.Range(1, 21);

        #region OldSkillAdd
        if (Snum == 1)
            AddSkill(new Skill1(emp));
        else if(Snum == 2)
            AddSkill(new Skill2(emp));
        else if (Snum == 3)
            AddSkill(new Skill3(emp));
        else if (Snum == 4)
            AddSkill(new Skill4(emp));
        else if (Snum == 5)
            AddSkill(new Skill5(emp));
        else if (Snum == 6)
            AddSkill(new Skill6(emp));
        else if (Snum == 7)
            AddSkill(new Skill7(emp));
        else if (Snum == 8)
            AddSkill(new Skill8(emp));
        else if (Snum == 9)
            AddSkill(new Skill9(emp));
        else if (Snum == 10)
            AddSkill(new Skill10(emp));
        else if (Snum == 11)
            AddSkill(new Skill11(emp));
        else if (Snum == 12)
            AddSkill(new Skill12(emp));
        else if (Snum == 12)
            AddSkill(new Skill12(emp));
        else if (Snum == 14)
            AddSkill(new Skill14(emp));
        else if (Snum == 15)
            AddSkill(new Skill15(emp));
        else if (Snum == 16)
            AddSkill(new Skill16(emp));
        else if (Snum == 17)
            AddSkill(new Skill17(emp));
        else if (Snum == 18)
            AddSkill(new Skill18(emp));
        else if (Snum == 19)
            AddSkill(new Skill19(emp));
        else if (Snum == 20)
            AddSkill(new Skill20(emp));
        #endregion
        AddThreeRandomStrategy();
 
    }

    public void CopyStatus(EmpInfo ei)
    {
        ei.GC = GC;
        ei.emp = emp;
        if (emp.Type == EmpType.Tech)
            ei.Text_Type.text = "技术";
        else if (emp.Type == EmpType.Market)
            ei.Text_Type.text = "市场";
        else
            ei.Text_Type.text = "运营";
        ei.Text_Name.text = emp.Name;
    }

    public void StartHire()
    {
        GC.CurrentEmpInfo = this;
        GC.SelectMode = 1;
        GC.ShowDepSelectPanel(emp.Type);
    }
    public void StartMove()
    {
        GC.CurrentEmpInfo = this;
        GC.SelectMode = 2;
        GC.ShowDepSelectPanel(emp);
    }

    public void Fire()
    {
        //重新计算工资
        GC.Salary -= CalcSalary();
        GC.CurrentEmployees.Remove(emp);
        GC.WorkEndCheck();
        if(emp.CurrentDep != null)
        {
            emp.CurrentDep.CurrentEmps.Remove(emp);
            emp.CurrentDep.ShowProducePower();
        }
        else if(emp.CurrentOffice != null)
        {
            emp.CurrentOffice.CurrentManager = null;
        }
        if (emp.InfoA == this)
            Destroy(emp.InfoB.gameObject);
        else
            Destroy(emp.InfoA.gameObject);
        Destroy(this.gameObject);
    }

    //需要改名字
    public void ShowDetailPanel()
    {
        if (GC.SelectMode == 4)
        {
            GC.CurrentEmpInfo = DetailInfo;
            GC.SC.ConfirmPanel.SetActive(true);
        }
        else if(GC.SelectMode == 5)
        {
            GC.CurrentEmpInfo = DetailInfo;
            GC.CurrentOffice.BuildingActive();
        }
        else
        {
            DetailInfo.ShowPanel();
        }
    }

    public void ShowPanel()
    {
        foreach (Transform child in GC.EmpDetailContent)
        {
            child.gameObject.SetActive(false);
        }
        gameObject.SetActive(true);
        AdjustSize();
    }

    public void SetSkillName()
    {
        if (emp.Type == EmpType.Tech)
        {
            Text_SName1.text = "程序迭代";
            Text_SName2.text = "技术研发";
            Text_SName3.text = "可行性调研";
        }
        else if (emp.Type == EmpType.Market)
        {
            Text_SName1.text = "公关谈判";
            Text_SName2.text = "营销文案";
            Text_SName3.text = "资源拓展";
        }
        else if (emp.Type == EmpType.Product)
        {
            Text_SName1.text = "原型图";
            Text_SName2.text = "产品研究";
            Text_SName3.text = "用户访谈";
        }
        else if (emp.Type == EmpType.Operate)
        {
            Text_SName1.text = "技术维护";
            Text_SName2.text = "增长运营";
            Text_SName3.text = "产品运营";
        }
    }

    public void AddHistory(string Content)
    {
        Text t = Instantiate(GC.HistoryTextPrefab, HistoryContent);
        t.text = "[" + GC.Text_Time.text + "] " + Content;
    }

    public void AdjustSize()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(HistoryContent.gameObject.GetComponent<RectTransform>());
    }

    public void AddPerk(Perk perk, bool AddEffect = false)
    {
        PerkInfo newPerk = Instantiate(GC.PerkInfoPrefab, PerkContent);
        newPerk.CurrentPerk = perk;
        newPerk.CurrentPerk.Info = newPerk;
        newPerk.Text_Name.text = perk.Name;
        newPerk.empInfo = this;
        newPerk.info = GC.infoPanel;
        PerksInfo.Add(newPerk);

        if (AddEffect == true)
            newPerk.CurrentPerk.AddEffect();
    }

    public void AddSkill(Skill skill)
    {
        SkillInfo newSkill = Instantiate(GC.SkillInfoPrefab, SkillContent);
        newSkill.skill = skill;
        newSkill.Text_Name.text = skill.Name;
        newSkill.empInfo = this;
        newSkill.info = GC.infoPanel;
        SkillsInfo.Add(newSkill);
    }

    public void AddStrategy(int num)
    {
        StrategyInfo newStrategy = Instantiate(GC.StrC.InfoPrefabA, StrategyContent);
        newStrategy.Str = StrategyData.Strategies[num];
        newStrategy.info = GC.infoPanel;
        newStrategy.Text_Name.text = newStrategy.Str.Name;
        StrategiesInfo.Add(newStrategy);
    }
    void AddThreeRandomStrategy()
    {
        int numA, numB, numC;
        numA = Random.Range(0, StrategyData.Strategies.Count);
        numB = Random.Range(0, StrategyData.Strategies.Count);
        numC = Random.Range(0, StrategyData.Strategies.Count);
        while(numB == numA)
        {
            numB = Random.Range(0, StrategyData.Strategies.Count);
        }
        while(numC == numB || numC == numA)
        {
            numC = Random.Range(0, StrategyData.Strategies.Count);
        }
        AddStrategy(numA);
        AddStrategy(numB);
        AddStrategy(numC);
    }

    public int CalcSalary()
    {
        int salary = emp.Manage + emp.Skill1 + emp.Skill2 + emp.Skill3 + emp.SalaryExtra;
        return salary;
    }

    void UpdateCharacterUI()
    {
        for (int i = 0; i < 5; i++)
        {
            if (i < 4)
                Scrollbar_Character[i].value = (emp.Character[i] + 3) / 6;
            else
                Scrollbar_Character[4].value = emp.Character[4] / 3;
        }
    }
}
