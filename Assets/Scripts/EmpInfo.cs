using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmpInfo : MonoBehaviour
{
    [HideInInspector] public EmpEntity Entity;

    public Employee emp;
    public GameControl GC;
    public GameObject MobDownSign, SkillPointSelector;
    public Button HireButton, MoveButton, FireButton;
    public Text Text_Name, Text_Mentality, Text_Stamina, Text_Skill1, Text_Ability, Text_Age, Text_School, Text_Major;
    public Text Text_DepName, Text_Tenacity, Text_Strength, Text_Manage, Text_Decision, Text_Emotion, Text_RTarget;
    public EmpInfo DetailInfo;
    public EmotionInfo EmotionInfoPrefab;
    public Transform PerkContent, SkillContent, StrategyContent, RelationContent, HistoryContent, EmotionContent, MarkerContent;
    public StrategyInfo CurrentStrategy;

    public GameObject[] LevelUpButtons = new GameObject[4];
    public Text[] Text_Stars = new Text[5], Text_Exps = new Text[5], Text_SkillNames = new Text[3], Text_SkillValues = new Text[3];
    public Scrollbar[] Scrollbar_Character = new Scrollbar[5];
    public List<PerkInfo> PerksInfo = new List<PerkInfo>();
    public List<SkillInfo> SkillsInfo = new List<SkillInfo>();
    public List<StrategyInfo> StrategiesInfo = new List<StrategyInfo>();
    public List<EmotionInfo> EmotionInfos = new List<EmotionInfo>();
    public List<BSSkillMarker> SkillMarkers = new List<BSSkillMarker>();

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

                if (emp.Confidence > 0)
                {
                    Text_Mentality.text += "/" + emp.Confidence;
                }

                Text_Skill1.text = "职业技能: " + emp.BaseAttributes[emp.Professions[0] - 1] + " / " + emp.BaseAttributes[emp.Professions[1] - 1] + 
                    " / " + emp.BaseAttributes[emp.Professions[2] - 1];
                Text_Manage.text = "管理能力:" + emp.Manage;
                if (emp.CurrentDep != null)
                    Text_DepName.text = emp.CurrentDep.Text_DepName.text;
                else if (emp.CurrentOffice != null)
                    Text_DepName.text = emp.CurrentOffice.Text_OfficeName.text;
                else
                    Text_DepName.text = "无";
                if(MobDownSign != null)
                {
                    if (emp.Mentality <= 0)
                        MobDownSign.SetActive(true);
                    else
                        MobDownSign.SetActive(false);
                }
            }
            //头脑风暴面板
            else if (InfoType == 3)
            {

            }
            //雇佣和详细面板通用部分
            else
            {
                Text_Age.text = emp.Age + "岁";                
                Text_Tenacity.text = emp.Tenacity.ToString();
                Text_Strength.text = emp.Strength.ToString();
                Text_Manage.text = emp.Manage.ToString();
                Text_Decision.text = emp.Decision.ToString();

                for(int i = 0; i < 3; i++)
                {
                    if (emp.Professions[i] == 1)
                    {
                        Text_SkillNames[i].text = "技术技能";
                        Text_SkillValues[i].text = emp.Tech.ToString();
                    }
                    else if (emp.Professions[i] == 2)
                    { 
                        Text_SkillNames[i].text = "市场技能";
                        Text_SkillValues[i].text = emp.Market.ToString();
                    }
                    else if (emp.Professions[i] == 3)
                    { 
                        Text_SkillNames[i].text = "产品技能";
                        Text_SkillValues[i].text = emp.Product.ToString();
                    }
                    else if (emp.Professions[i] == 8)
                    { 
                        Text_SkillNames[i].text = "人力技能";
                        Text_SkillValues[i].text = emp.HR.ToString();
                    }
                    else if (emp.Professions[i] == 9)
                    { 
                        Text_SkillNames[i].text = "财务技能";
                        Text_SkillValues[i].text = emp.Finance.ToString();
                    }
                    else if (emp.Professions[i] == 11)
                    { 
                        Text_SkillNames[i].text = "行业技能";
                        Text_SkillValues[i].text = emp.Forecast.ToString();
                    }
                    else if (emp.Professions[i] == 12)
                    { 
                        Text_SkillNames[i].text = "谋略技能";
                        Text_SkillValues[i].text = emp.Strategy.ToString();
                    }
                    else if (emp.Professions[i] == 13)
                    { 
                        Text_SkillNames[i].text = "说服技能";
                        Text_SkillValues[i].text = emp.Convince.ToString();
                    }
                    else if (emp.Professions[i] == 15)
                    { 
                        Text_SkillNames[i].text = "八卦技能";
                        Text_SkillValues[i].text = emp.Gossip.ToString();
                    }
                    else if (emp.Professions[i] == 16)
                    { 
                        Text_SkillNames[i].text = "行政技能";
                        Text_SkillValues[i].text = emp.Admin.ToString();
                    }
                }
                UpdateCharacterUI();
                for(int i = 0; i < 3; i++)
                {
                    Text_Stars[i].text = emp.StarLimit[i].ToString();
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
                for (int i = 0; i < 3; i++)
                {
                    int Exp = 0, SkillLevel = 0;
                    #region 设定等级
                    if (i == 0)
                        SkillLevel = emp.Tech;
                    else if (i == 1)
                        SkillLevel = emp.Market;
                    else if (i == 2)
                        SkillLevel = emp.Product;
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
                    Exp = SkillLevel * 50;
                    Text_Exps[i].text = "Exp " + emp.SkillExp[emp.Professions[i] - 1] + "/" + Exp;
                }
            }
        }
    }

    //新建一个Employee并初始化数值
    public void CreateEmp()
    {       
        emp = new Employee();
        emp.InfoA = this;
        emp.InitStatus();
        HireButton.interactable = true;
        SetSkillName();
        InitStrategy();

        //根据学校和专业设定特效
        if(emp.SchoolType == 1)
        {
            Text_School.text = "名牌大学";
            Text_School.GetComponent<InfoPanelTrigger>().ContentB = "第一份工作技能等级+1";
        }
        else if(emp.SchoolType == 2)
        {
            Text_School.text = "普通院校";
            Text_School.GetComponent<InfoPanelTrigger>().ContentB = "无特效";
        }
        else if (emp.SchoolType == 3)
        {
            Text_School.text = "普通院校";
            Text_School.GetComponent<InfoPanelTrigger>().ContentB = "第一份工作技能等级-1，坚韧+2";
        }

        int[] Nst = { 1, 2, 3, 8, 9, 11, 12, 13, 15, 16 };//Nst:几个专业技能对应的编号
        int major = Random.Range(0, 10);
        major = Nst[major];

        for(int i = 0; i < 3; i++)
        {
            if(major == emp.Professions[i])
            {
                Text_Major.GetComponent<InfoPanelTrigger>().ContentB = "大学所选专业与工作内容相匹配，对应技能等级+2";
                break;
            }
            if(i == 2)
                Text_Major.GetComponent<InfoPanelTrigger>().ContentB = "无额外效果";
        }
        if (major == 1)
            Text_Major.text = "技术专业";
        else if (major == 2)
            Text_Major.text = "市场专业";
        else if (major == 3)
            Text_Major.text = "产品专业";
        else if (major == 8)
            Text_Major.text = "人力专业";
        else if (major == 9)
            Text_Major.text = "财务专业";
        else if (major == 11)
            Text_Major.text = "行业专业";
        else if (major == 12)
            Text_Major.text = "谋略专业";
        else if (major == 13)
            Text_Major.text = "说服专业";
        else if (major == 15)
            Text_Major.text = "八卦专业";
        else if (major == 16)
            Text_Major.text = "行政专业";

    }

    //初始化头脑风暴技能和战略
    public void InitStrategy()
    {
        AddThreeRandomStrategy();
        AddRandomPerk();
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
        GC.ShowDepSelectPanel();
    }
    public void StartMove()
    {
        if (GC.SelectMode != 9)
        {
            GC.CurrentEmpInfo = this;
            GC.SelectMode = 2;
            GC.ShowDepSelectPanel(emp);
        }
        //头脑风暴初始
        else
            GC.SC.InitEmpInfo(emp);
    }

    public void Fire(bool NeedVote = true)
    {
        //不能删CEO
        if (emp.isCEO == true)
            return;
        //会议未通过不能开除
        if (NeedVote == true && GC.EC.ManagerVoteCheck(emp, true, true) == false)
            return;

        //如果是核心成员则离开核心团队
        foreach(EmpBSInfo info in GC.BSC.EmpSelectInfos)
        {
            if(info.emp == emp)
            {
                info.EmpLeft();
                break;
            }
        }

        foreach (PerkInfo perk in emp.InfoDetail.PerksInfo)
        {
            perk.CurrentPerk.RemoveAllListeners();
        }

        GC.CC.CEO.InfoDetail.AddHistory("解雇了" + emp.Name);

        //这个函数调用要删
        ClearSkillPreset();

        GC.ResetOldAssignment(emp);
        emp.ClearRelations();//清空所有关系
        GC.HourEvent.RemoveListener(emp.TimePass);
        GC.CurrentEmployees.Remove(emp);
        
        //删除员工实体
        emp.InfoDetail.Entity.RemoveEntity();

        emp.DestroyAllInfos();
    }

    //选择Info后的各种行为
    public void ShowDetailPanel()
    {
        //发动动员技能
        if (GC.SelectMode == 4)
        {
            GC.CurrentEmpInfo = DetailInfo;
            //GC.SC.ConfirmPanel.SetActive(true);
            GC.BSC.UseSkillOnSelectedEmp(emp);
            GC.TotalEmpPanel.SetWndState(false);
            GC.ResetSelectMode();
        }
        //发动建筑技能
        else if(GC.SelectMode == 5)
        {
            GC.CurrentEmpInfo = DetailInfo;
            GC.CurrentDep.BuildingActive();
        }
        //发动CEO技能
        else if(GC.SelectMode == 6)
        {
            ////调查员工
            //if(GC.CEOSkillNum == 18)
            //{
            //    GC.CC.EmpSelectWarning.SetActive(false);
            //    GC.CEOSkillNum = 15;
            //    int Posb = Random.Range(1, 7);
            //    Posb += (int)(GC.CurrentEmpInfo.emp.Gossip * 0.2f);
            //    if (Posb >= 3)
            //    {
            //        if (emp.isSpy == true)
            //        {
            //            emp.InfoDetail.AddPerk(new Perk29(emp), true);
            //            GC.CreateMessage("成功发现了内鬼" + emp.Name);
            //        }
            //        else
            //            GC.CreateMessage("发现" + emp.Name + "不是内鬼");
            //    }
            //    else
            //        GC.CreateMessage("调查失败，无法了解" + emp.Name + "的成分");
            //    return;
            //}
            if (GC.CC.CEOSkillNum == 4)
            {
                emp.Mentality += (int)(10 * GC.HRBuildingMentalityExtra);
                GC.TotalEmpPanel.SetWndState(false);
                emp.InfoDetail.AddHistory("收到CEO安抚,心力+" + (10 * GC.HRBuildingMentalityExtra));
                GC.CC.CEO.InfoDetail.AddHistory("安抚了" + emp.Name + ",对方心力+" + (10 * GC.HRBuildingMentalityExtra));
            }
            else if (GC.CC.CEOSkillNum == 3)
            {
                GC.CurrentEmpInfo = this;
                GC.CC.TrainingPanel.SetActive(true);
            }
            else if (GC.CC.CEOSkillNum == 5)
            {
                GC.CurrentEmpInfo = this;
                GC.CC.VacationPanel.SetActive(true);
            }
            else if (GC.CC.CEOSkillNum > 5)
            {
                if (emp.isCEO == false || GC.CC.CEOSkillNum == 18)
                    GC.CC.SetPanelContent(emp);
            }
        }
        //选两个员工的动员技能
        else if(GC.SelectMode == 7 || GC.SelectMode == 10)
        {
            if (GC.CurrentEmpInfo2 == null)
            {
                GC.CurrentEmpInfo2 = DetailInfo;
                GC.Text_EmpSelectTip.text = "选择第二个员工";
            }
            else
            {
                if (GC.SelectMode == 7)
                {
                    GC.CurrentEmpInfo = DetailInfo;
                    GC.SC.ConfirmPanel.SetActive(true);
                }
                else if (GC.SelectMode == 10)
                {
                    if (GC.CurrentEmpInfo2.emp != emp)
                        GC.CC.SetPanelContent(emp, GC.CurrentEmpInfo2.emp);
                }
            }
        }

        //显示员工详细信息
        else
        {
            DetailInfo.ShowPanel();
        }
    }

    public void ShowPanel()
    {
        foreach (Employee emp in GC.CurrentEmployees)
        {
            emp.InfoDetail.GetComponent<WindowBaseControl>().SetWndState(false);
        }
        this.GetComponent<WindowBaseControl>().SetWndState(true);
        AdjustSize();
        SkillPointCheck();
        CheckMarker();
    }

    public void UseSkillPoint(int type)
    {
        if (type == 1)
        {
            emp.Tenacity += 1;
            if (emp.Tenacity == 10)
                LevelUpButtons[0].SetActive(false);
        }
        else if (type == 2)
        {
            emp.Strength += 1;
            if (emp.Strength == 10)
                LevelUpButtons[1].SetActive(false);
        }
        else if (type == 3)
        {
            emp.Manage += 1;
            if (emp.Manage == 5)
                LevelUpButtons[2].SetActive(false);
        }
        else if (type == 4)
        {
            emp.Decision += 1;
            if (emp.Decision == 5)
                LevelUpButtons[3].SetActive(false);
        }
        emp.SkillPoint -= 1;
        SkillPointCheck();
    }

    public void SkillPointCheck()
    {
        if (emp.SkillPoint > 0)
            SkillPointSelector.SetActive(true);
        else
            SkillPointSelector.SetActive(false);
        SkillPointSelector.GetComponent<Text>().text = "剩余技能点" + emp.SkillPoint;
    }

    public void SetSkillName()
    {
        Text_Name.text = emp.Name;
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
        //同类Perk检测
        foreach (PerkInfo p in PerksInfo)
        {
            if (p.CurrentPerk.Num == perk.Num)
            {
                //可叠加的进行累加
                if (perk.canStack == true)
                {
                    p.CurrentPerk.Level += 1;
                    if (AddEffect == true)
                        p.CurrentPerk.AddEffect();
                    return;
                }
                //不可叠加的清除
                else
                {
                    p.CurrentPerk.RemoveEffect();
                    break;
                }
            }
        }
        PerkInfo newPerk = Instantiate(GC.PerkInfoPrefab, PerkContent);
        newPerk.CurrentPerk = perk;
        newPerk.CurrentPerk.BaseTime = perk.TimeLeft; 
        newPerk.CurrentPerk.Info = newPerk;
        newPerk.Text_Name.text = perk.Name;
        newPerk.empInfo = this;
        newPerk.info = GC.infoPanel;
        PerksInfo.Add(newPerk);

        if (AddEffect == true)
            newPerk.CurrentPerk.AddEffect();
    }
    public void RemovePerk(int num)
    {
        foreach(PerkInfo info in PerksInfo)
        {
            if(info.CurrentPerk.Num == num)
            {
                info.CurrentPerk.RemoveEffect();
                break;
            }
        }
    }
    public void DepAddPerk(Perk perk)
    {
        if (emp.CurrentDep != null)
            emp.CurrentDep.AddPerk(perk);
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
    void AddRandomPerk()
    {
        float Posb = Random.Range(0.0f, 1.0f);
        //20%几率没特质
        if (Posb < 0.2f)
            return;

        int numA = Random.Range(0, PerkData.PerkList.Count);      
        Perk perk1 = PerkData.PerkList[numA].Clone();
        perk1.TargetEmp = emp;
        AddPerk(perk1, true);

        //60%几率只有一个特质
        if (Posb < 0.8f)
            return;

        int numB = Random.Range(0, PerkData.PerkList.Count); ;
        while (numB == numA)
        {
            numB = Random.Range(0, PerkData.PerkList.Count);
        }
        Perk perk2 = PerkData.PerkList[numB].Clone();
        perk2.TargetEmp = emp;
        AddPerk(perk2, true);
    }

    public int CalcSalary()
    {//暂时定为25
        return 25;
        //int type = emp.InfoDetail.ST.SkillType;
        //int salary = emp.SalaryExtra + emp.Manage + emp.Skill1 + emp.Skill2 + emp.Skill3 + emp.Observation + emp.Tenacity + emp.Strength
        //     + emp.HR + emp.Finance + emp.Decision + emp.Forecast + emp.Strategy + emp.Convince + emp.Charm + emp.Gossip;
        //if (type == 1)
        //    salary = emp.Observation;
        //else if (type == 2)
        //    salary += emp.Skill1;
        //else if (type == 3)
        //    salary += emp.Skill3;
        //else if (type == 4)
        //    salary += emp.Skill2;
        //else if (type == 5)
        //    salary += emp.Finance;
        //else if (type == 6)
        //    salary += emp.HR;
        //else if (type == 7)
        //    salary += emp.Strength;
        //else if (type == 8)
        //    salary += emp.Tenacity;
        //else if (type == 9)
        //    salary += emp.Forecast;
        //else if (type == 10)
        //    salary += emp.Strategy;
        //else
        //    salary += emp.Manage;
        //salary = (int)(salary * emp.SalaryMultiple);
        //return salary;
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
        for(int i = 0; i < 6; i++)
        {
            if (GC.SC.CSkillSetA[i].skill != null && GC.SC.CSkillSetA[i].skill.TargetEmp == emp)
            {
                GC.SC.CSkillSetA[i].skill = null;
                GC.SC.CSkillSetA[i].empInfo = null;
                GC.SC.CSkillSetA[i].UpdateUI();
            }
            if (GC.SC.CSkillSetB[i].skill != null && GC.SC.CSkillSetB[i].skill.TargetEmp == emp)
            {
                GC.SC.CSkillSetB[i].skill = null;
                GC.SC.CSkillSetB[i].empInfo = null;
                GC.SC.CSkillSetB[i].UpdateUI();
            }
            if (GC.SC.CSkillSetC[i].skill != null && GC.SC.CSkillSetC[i].skill.TargetEmp == emp)
            {
                GC.SC.CSkillSetC[i].skill = null;
                GC.SC.CSkillSetC[i].empInfo = null;
                GC.SC.CSkillSetC[i].UpdateUI();
            }
        }
        foreach(EmpInfo i in GC.SC.SelectedEmps)
        {
            if(i.emp == emp)
            {
                GC.SC.SelectedEmps.Remove(i);
                Destroy(i.gameObject);
                break;
            }
        }
    }

    public void UpdateEmotionPanel()
    {
        Text_Emotion.text = "当前情绪:";
        for(int i = 0; i < emp.CurrentEmotions.Count; i++)
        {
            if (emp.CurrentEmotions[i].color == EColor.White)
                Text_Emotion.text += "  白" + emp.CurrentEmotions[i].Level;
            else if (emp.CurrentEmotions[i].color == EColor.Gray)
                Text_Emotion.text += "  灰" + emp.CurrentEmotions[i].Level;
            else if (emp.CurrentEmotions[i].color == EColor.LYellow)
                Text_Emotion.text += "  淡黄" + emp.CurrentEmotions[i].Level;
            else if (emp.CurrentEmotions[i].color == EColor.LRed)
                Text_Emotion.text += "  淡红" + emp.CurrentEmotions[i].Level;
            else if (emp.CurrentEmotions[i].color == EColor.LBlue)
                Text_Emotion.text += "  淡蓝" + emp.CurrentEmotions[i].Level;
            else if (emp.CurrentEmotions[i].color == EColor.LOrange)
                Text_Emotion.text += "  淡橙" + emp.CurrentEmotions[i].Level;
            else if (emp.CurrentEmotions[i].color == EColor.LPurple)
                Text_Emotion.text += "  淡紫" + emp.CurrentEmotions[i].Level;
            else if (emp.CurrentEmotions[i].color == EColor.LGreen)
                Text_Emotion.text += "  淡绿" + emp.CurrentEmotions[i].Level;
            else if (emp.CurrentEmotions[i].color == EColor.Yellow)
                Text_Emotion.text += "  黄" + emp.CurrentEmotions[i].Level;
            else if (emp.CurrentEmotions[i].color == EColor.Red)
                Text_Emotion.text += "  红" + emp.CurrentEmotions[i].Level;
            else if (emp.CurrentEmotions[i].color == EColor.Blue)
                Text_Emotion.text += "  蓝" + emp.CurrentEmotions[i].Level;
            else if (emp.CurrentEmotions[i].color == EColor.Orange)
                Text_Emotion.text += "  橙" + emp.CurrentEmotions[i].Level;
            else if (emp.CurrentEmotions[i].color == EColor.Purple)
                Text_Emotion.text += "  紫" + emp.CurrentEmotions[i].Level;
            else if (emp.CurrentEmotions[i].color == EColor.Green)
                Text_Emotion.text += "  绿" + emp.CurrentEmotions[i].Level;
        }
        Text_RTarget.text = "发展目标:";
        for(int i = 0; i < emp.RelationTargets.Count; i++)
        {
            Text_RTarget.text += "  " + emp.RelationTargets[i].Name;
        }
    }

    //从头脑风暴员工列表中移除
    public void MobInfoRemove()
    {
        emp.InfoDetail.ClearSkillPreset();
        GameControl.Instance.SC.RemoveEmpInfo(this);
    }

    //以下四个函数为战略充能相关
    public void DirectAddStrategy()
    {
        //只能充能和自己信仰或文化相符的战略
        Strategy S = null;
        List<Strategy> LS = new List<Strategy>();
        foreach(StrategyInfo si in StrategiesInfo)
        {
            if (si.Str.Type == StrategyType.人力 && emp.CharacterTendency[1] == 1)
                LS.Add(si.Str);
            if (si.Str.Type == StrategyType.研发 && emp.CharacterTendency[1] == -1)
                LS.Add(si.Str);
            if (si.Str.Type == StrategyType.管理 && emp.CharacterTendency[0] == 1)
                LS.Add(si.Str);
            if (si.Str.Type == StrategyType.执行 && emp.CharacterTendency[0] == -1)
                LS.Add(si.Str);
        }
        if (LS.Count > 0)
            S = LS[Random.Range(0, LS.Count)];
        else
            GC.CreateMessage(emp.Name + "没有符合自身特性的战略");
        if (S != null)
        {
            StrategyInfo NewStr = Instantiate(GC.StrC.ChargePrefab, GC.StrC.StrategyContent);
            NewStr.Owner = emp;
            CurrentStrategy = NewStr;
            NewStr.SC = GC.StrC;
            GC.StrC.StrInfos.Add(NewStr);
            NewStr.Str = S;
            CurrentStrategy.Text_Progress.text = "充能完毕";
            CurrentStrategy.RechargeComplete = true;
            NewStr.UpdateUI();
        }
    }

    public void AddEmotionInfo(Emotion E)
    {
        EmotionInfo info = Instantiate(EmotionInfoPrefab, EmotionContent);
        info.SetColor(E);
        EmotionInfos.Add(info);
        emp.CurrentEmotions.Add(E);
    }
    public void RemoveEmotionInfo(Emotion emotion)
    {
        foreach(EmotionInfo info in EmotionInfos)
        {
            if(info.E == emotion)
            {
                EmotionInfos.Remove(info);
                emp.CurrentEmotions.Remove(emotion);
                Destroy(info.gameObject);
                return;
            }
        }
    }

    public void CreateStrategy()
    {
        //if (RechargeStrategyNum < 0)
        //    RechargeStrategyNum = Random.Range(0, StrategiesInfo.Count);
        //StrategyInfo NewStr = Instantiate(GC.StrC.ChargePrefab, GC.StrC.StrategyContent);
        //CurrentStrategy = NewStr;
        //NewStr.SC = GC.StrC;
        //GC.StrC.StrInfos.Add(NewStr);
        //NewStr.Str = StrategiesInfo[RechargeStrategyNum].Str;
        //NewStr.UpdateUI();
        //NewStr.Text_Progress.text = RechargeProgress + "/300";
    }
    public void ReChargeStrategy()
    {
        //RechargeProgress += emp.Manage;
        //if (RechargeProgress >= 300)
        //{
        //    RechargeProgress = 0;
        //    RechargeStrategyNum = -1;
        //    CurrentStrategy.Text_Progress.text = "充能完毕";
        //    CurrentStrategy.RechargeComplete = true;
        //    CreateStrategy();
        //    emp.GainExp(250, 7);
        //}
        //else
        //    CurrentStrategy.Text_Progress.text = RechargeProgress + "/300";
    }
    public void TempDestroyStrategy()
    {
        //if(CurrentStrategy != null)
        //{
        //    GC.StrC.StrInfos.Remove(CurrentStrategy);
        //    Destroy(CurrentStrategy.gameObject);
        //    CurrentStrategy = null;
        //}
    }

    public void CheckMarker()
    {
        for (int i = 0; i < SkillMarkers.Count; i++)
        {
            Destroy(SkillMarkers[i].gameObject);
        }
        SkillMarkers.Clear();
        foreach (int num in emp.Professions)
        {
            //技能为0则不继续算
            if (emp.BaseAttributes[num - 1] == 0)
                continue;

            int type = 0;
            if (num == 8)
                type = 0;
            else if (num == 9)
                type = 1;
            else if (num == 16)
                type = 2;
            else if (num == 1 || num == 3 || num == 11)
                type = 3;
            else if (num == 2 || num == 13)
                type = 4;
            else if (num == 12 || num == 15)
                type = 5;

            if (emp.BaseAttributes[num - 1] > 3)
            {
                for (int i = 0; i < emp.BaseAttributes[num - 1] / 3; i++)
                {
                    BSSkillMarker m = Instantiate(GC.BSC.MarkerPrefab, MarkerContent).GetComponent<BSSkillMarker>();
                    m.SetInfo(type, 3);
                    SkillMarkers.Add(m);
                }
            }
            if (emp.BaseAttributes[num - 1] % 3 > 0)
            {
                BSSkillMarker m = Instantiate(GC.BSC.MarkerPrefab, MarkerContent).GetComponent<BSSkillMarker>();
                m.SetInfo(type, emp.BaseAttributes[num - 1] % 3);
                SkillMarkers.Add(m);
            }
        }
    }

}
