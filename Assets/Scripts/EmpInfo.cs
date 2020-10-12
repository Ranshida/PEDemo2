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
    public Text Text_Name, Text_Mentality, Text_Stamina, Text_Skill1, Text_Skill2, Text_Skill3,  Text_Ability, Text_Age;
    public Text Text_DepName, Text_Observation, Text_Tenacity, Text_Strength, Text_Manage, Text_HR, Text_Finance, Text_Decision, 
        Text_Forecast, Text_Strategy, Text_Convince, Text_Charm, Text_Gossip, Text_SName1, Text_SName2, Text_SName3,
         Text_Motiv_Study, Text_Motiv_Recover, Text_Motive_Ambition, Text_Motiv_Social;
    public Text[] Text_Stars = new Text[5], Text_Exps = new Text[5];
    public Scrollbar[] Scrollbar_Character = new Scrollbar[5];
    public EmpInfo DetailInfo;
    public Transform PerkContent, SkillContent, StrategyContent, RelationContent, HistoryContent;
    public StrategyInfo CurrentStrategy;

    public List<PerkInfo> PerksInfo = new List<PerkInfo>();
    public List<SkillInfo> SkillsInfo = new List<SkillInfo>();
    public List<StrategyInfo> StrategiesInfo = new List<StrategyInfo>();

    public int InfoType;

    int RechargeStrategyNum = -1, RechargeProgress = 0;

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
                Text_Age.text = emp.Age + "岁";
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
                for(int i = 0; i < 5; i++)
                {
                    Text_Stars[i].text = "热情 " + ((float)emp.Stars[i] / 5f) + "/" + emp.StarLimit[i];
                }
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
                for (int i = 0; i < 15; i++)
                {
                    int Exp = 0, SkillLevel = 0;
                    #region 设定等级
                    if (i == 0)
                        SkillLevel = emp.Skill1;
                    else if (i == 1)
                        SkillLevel = emp.Skill2;
                    else if (i == 2)
                        SkillLevel = emp.Skill3;
                    else if (i == 3)
                        SkillLevel = emp.Observation;
                    else if (i == 4)
                        SkillLevel = emp.Tenacity;
                    else if (i == 5)
                        SkillLevel = emp.Strength;
                    else if (i == 6)
                        SkillLevel = emp.Manage;
                    else if (i == 7)
                        SkillLevel = emp.HR;
                    else if (i == 8)
                        SkillLevel = emp.Finance;
                    else if (i == 9)
                        SkillLevel = emp.Decision;
                    else if (i == 10)
                        SkillLevel = emp.Forecast;
                    else if (i == 11)
                        SkillLevel = emp.Strategy;
                    else if (i == 12)
                        SkillLevel = emp.Convince;
                    else if (i == 13)
                        SkillLevel = emp.Charm;
                    else if (i == 14)
                        SkillLevel = emp.Gossip;
                    #endregion
                    if (SkillLevel < 10)
                        Exp = 50 + 50 * (SkillLevel - 0);
                    else if (SkillLevel < 15)
                        Exp = 500 + 100 * (SkillLevel - 10);                    
                    else if (SkillLevel < 20)
                        Exp = 1000 + 200 * (SkillLevel - 15);
                    else
                        Exp = 2000 + 300 * (SkillLevel - 20);
                    Text_Exps[i].text = "Exp " + emp.SkillExp[i] + "/" + Exp;
                }
            }
        }
    }

    //新建一个Employee并初始化数值
    public void CreateEmp(EmpType type, int[] Hst, int AgeRange)
    {       
        emp = new Employee();
        emp.InfoA = this;
        emp.InitStatus(type, Hst, AgeRange);
        HireButton.interactable = true;
        SetSkillName();
        InitSkillAndStrategy();
    }

    //初始化头脑风暴技能和战略
    public void InitSkillAndStrategy()
    {
        for (int i = 0; i < 4; i++)
        {
            int Snum = Random.Range(20, SkillData.Skills.Count);
            Skill NewSkill = SkillData.Skills[Snum].Clone();
            NewSkill.TargetEmp = this.emp;
            AddSkill(NewSkill);
        }
        AddThreeRandomStrategy();
    }

    public void CopyStatus(EmpInfo ei)
    {
        ei.GC = GC;
        ei.emp = emp;
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
        //不能删CEO
        if (emp.isCEO == true)
            return;

        //会议未通过不能开除
        if (GC.EC.ManagerVoteCheck(emp, true, true) == false)
            return;

        //删除员工实体
        emp.InfoDetail.Entity.RemoveEntity();

        //重新计算工资
        ClearSkillPreset();
        GC.HourEvent.RemoveListener(emp.TimePass);
        GC.Salary -= CalcSalary();
        GC.CurrentEmployees.Remove(emp);
        GC.WorkEndCheck();
        if(emp.CurrentDep != null)
        {
            emp.CurrentDep.CurrentEmps.Remove(emp);
            emp.CurrentDep.UpdateUI();
        }
        else if(emp.CurrentOffice != null)
        {
            TempDestroyStrategy();
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
        else if(GC.SelectMode == 6)
        {
            //调查员工
            if(GC.CEOSkillNum == 18)
            {
                GC.CC.EmpSelectWarning.SetActive(false);
                GC.CEOSkillNum = 15;
                int Posb = Random.Range(1, 7);
                Posb += (int)(GC.CurrentEmpInfo.emp.Gossip * 0.2f);
                if (Posb >= 3)
                {
                    if (emp.isSpy == true)
                    {
                        emp.InfoDetail.AddPerk(new Perk29(emp), true);
                        GC.CreateMessage("成功发现了内鬼" + emp.Name);
                    }
                    else
                        GC.CreateMessage("发现" + emp.Name + "不是内鬼");
                }
                else
                    GC.CreateMessage("调查失败，无法了解" + emp.Name + "的成分");
                return;
            }
            if (GC.CEOSkillNum == 4)
            {
                emp.Mentality += 10;
            }
            else if (GC.CEOSkillNum == 5)
            {
                GC.CurrentEmpInfo = this;
                GC.EmpTrainingPanel.SetActive(true);
            }
            else if (GC.CEOSkillNum > 5 && emp.isCEO == false)
            {
                GC.CC.SetPanelContent(emp);
            }
            if (GC.CEOSkillNum < 6)
                GC.CEOSkillConfirm();
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
        for(int i = 0; i < emp.Relations.Count; i++)
        {
            emp.Relations[i].UpdateUI();
        }
    }

    public void SetSkillName()
    {
        Text_Name.text = emp.Name;
        //if (emp.Type == EmpType.Tech)
        //{
        //    Text_SName1.text = "程序迭代";
        //    Text_SName2.text = "技术研发";
        //    Text_SName3.text = "可行性调研";
        //}
        //else if (emp.Type == EmpType.Market)
        //{
        //    Text_SName1.text = "公关谈判";
        //    Text_SName2.text = "营销文案";
        //    Text_SName3.text = "资源拓展";
        //}
        //else if (emp.Type == EmpType.Product)
        //{
        //    Text_SName1.text = "原型图";
        //    Text_SName2.text = "产品研究";
        //    Text_SName3.text = "用户访谈";
        //}
        //else if (emp.Type == EmpType.Operate)
        //{
        //    Text_SName1.text = "技术维护";
        //    Text_SName2.text = "增长运营";
        //    Text_SName3.text = "产品运营";
        //}
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
                Scrollbar_Character[i].value = (emp.Character[i] + 100) / 200;
            else
                Scrollbar_Character[4].value = emp.Character[4] / 100;
        }
    }

    public void ClearSkillPreset()
    {
        //清除技能预设
        if(emp.CurrentDep != null)
        {
            for(int i = 0; i < SkillsInfo.Count; i++)
            {
                for(int j = 0; j < 6; j++)
                {
                    if (emp.CurrentDep.DSkillSetA[j] == SkillsInfo[i].skill)
                        emp.CurrentDep.DSkillSetA[j] = null;

                    if (emp.CurrentDep.DSkillSetB[j] == SkillsInfo[i].skill)
                        emp.CurrentDep.DSkillSetB[j] = null;

                    if (emp.CurrentDep.DSkillSetB[j] == SkillsInfo[i].skill)
                        emp.CurrentDep.DSkillSetB[j] = null;
                }
            }
        }
    }

    public void UpdateMotivationPanel()
    {
        Text_Motiv_Study.text = emp.CheckMotivationContent(0);
        Text_Motiv_Recover.text = emp.CheckMotivationContent(1);
        Text_Motive_Ambition.text = emp.CheckMotivationContent(2);
        Text_Motiv_Social.text = emp.CheckMotivationContent(3);
    }

    //以下三个函数为战略充能相关
    public void CreateStrategy()
    {
        if (RechargeStrategyNum < 0)
            RechargeStrategyNum = Random.Range(0, StrategiesInfo.Count);
        StrategyInfo NewStr = Instantiate(GC.StrC.ChargePrefab, GC.StrC.StrategyContent);
        CurrentStrategy = NewStr;
        NewStr.SC = GC.StrC;
        GC.StrC.StrInfos.Add(NewStr);
        NewStr.Str = StrategiesInfo[RechargeStrategyNum].Str;
        NewStr.UpdateUI();
        NewStr.Text_Progress.text = RechargeProgress + "/300";
    }
    public void ReChargeStrategy()
    {
        RechargeProgress += emp.Manage;
        if (RechargeProgress >= 300)
        {
            RechargeProgress = 0;
            RechargeStrategyNum = -1;
            CurrentStrategy.Text_Progress.text = "充能完毕";
            CurrentStrategy.RechargeComplete = true;
            CreateStrategy();
            emp.GainExp(250, 7);
        }
        else
            CurrentStrategy.Text_Progress.text = RechargeProgress + "/300";
    }
    public void TempDestroyStrategy()
    {
        if(CurrentStrategy != null)
        {
            GC.StrC.StrInfos.Remove(CurrentStrategy);
            Destroy(CurrentStrategy.gameObject);
            CurrentStrategy = null;
        }
    }
}
