﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmpInfo : MonoBehaviour
{
    public EmpEntity Entity;
    public Employee emp;
    public GameControl GC;
    public Button HireButton, MoveButton, FireButton;
    public Text Text_Name, Text_Mentality, Text_Stamina, Text_Skill1, Text_Ability, Text_Age, Text_Professions, Text_Occupation, Text_Ambition;
    public Text Text_DepName, Text_Tenacity, Text_Manage, Text_Decision, Text_RTarget;
    public EmpInfo DetailInfo;
    public EmotionInfo EmotionInfoPrefab, MainEmotion;
    public Transform PerkContent, SkillContent, StrategyContent, RelationContent, HistoryContent, EmotionContent, MarkerContent;
    public Transform PerkContent2;//特质专用面板，上面的是状态面板
    public StrategyInfo CurrentStrategy;

    public Scrollbar[] Scrollbar_Character = new Scrollbar[5];
    public List<PerkInfo> PerksInfo = new List<PerkInfo>();
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
                Text_Manage.text = "管理能力:" + emp.Manage;
                if (emp.CurrentDep != null)
                    Text_DepName.text = emp.CurrentDep.Text_DepName.text;
                else if (emp.CurrentDivision != null)
                    Text_DepName.text = emp.CurrentDivision.DivName;
                else
                    Text_DepName.text = "无";
            }

            //雇佣和详细面板通用部分
            else
            {
                Text_Age.text = emp.Age + "岁";                
                Text_Tenacity.text = emp.Tenacity.ToString();
                Text_Manage.text = emp.Manage.ToString();
                Text_Decision.text = emp.Decision.ToString();
                Text_Ambition.text = "志向:" + emp.ambition;
                Text_Occupation.text = "职业:" + emp.Occupation;

                UpdateCharacterUI();
            }
            //详细面板
            if(InfoType == 2)
            {
                Text_Mentality.text = "心力:" + emp.Mentality;

                if (emp.CurrentDep != null)
                    Text_DepName.text = "所属部门:" + emp.CurrentDep.Text_DepName.text;
                else
                    Text_DepName.text = "所属部门:无";
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
        Text_Name.text = emp.Name;
        InitStrategyAndPerk();
        CheckProfessions();
        CheckMarker();
    }

    //初始化头脑风暴技能和战略
    public void InitStrategyAndPerk()
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
        GC.ShowDepSelectPanel(emp);
    }
    public void StartMove()
    {
        if (GC.SelectMode != 9)
        {
            GC.CurrentEmpInfo = this;
            GC.SelectMode = 2;
            GC.ShowDepSelectPanel(emp);
        }
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

        GC.ResetOldAssignment(emp);
        emp.ClearRelations();//清空所有关系
        GC.TurnEvent.RemoveListener(emp.TimePass);
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
                    //GC.CurrentEmpInfo = DetailInfo;
                    //GC.SC.ConfirmPanel.SetActive(true);
                }
                else if (GC.SelectMode == 10)
                {
                    if (GC.CurrentEmpInfo2.emp != emp)
                        GC.CC.SetPanelContent(emp, GC.CurrentEmpInfo2.emp);
                }
            }
        }

        //使用物品
        else if (GC.SelectMode == 11)
        {
            GC.CurrentItem.TargetEmp = emp.InfoDetail;
            GC.CurrentItem.UseItem();
            GC.ResetSelectMode();
            GC.TotalEmpPanel.SetWndState(false);
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
        CheckMarker();
        CheckProfessions();
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

    public void AddPerk(Perk perk, bool AddEffect = true)
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
        PerkInfo newPerk;
        if (perk.Num >= 1 && perk.Num <= 51 && PerkContent2 != null)
            newPerk = Instantiate(GC.PerkInfoPrefab, PerkContent2);
        else
            newPerk = Instantiate(GC.PerkInfoPrefab, PerkContent);
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
        //初始化经历
        int num = Random.Range(0, PerkData.DefaultPerkList.Count);
        Perk perk3 = PerkData.DefaultPerkList[num].Clone();
        perk3.TargetEmp = emp;
        AddPerk(perk3, true);

        //职业特质
        if (emp.Occupation == OccupationType.超级黑客)
            AddPerk(new Perk35(emp));
        else if (emp.Occupation == OccupationType.神秘打工仔)
            AddPerk(new Perk36(emp));
        else if (emp.Occupation == OccupationType.大企业中层)
            AddPerk(new Perk37(emp));
        else if (emp.Occupation == OccupationType.海盗)
            AddPerk(new Perk38(emp));
        else if (emp.Occupation == OccupationType.大学毕业生)
            AddPerk(new Perk39(emp));
        else if (emp.Occupation == OccupationType.论坛版主)
            AddPerk(new Perk40(emp));
        else if (emp.Occupation == OccupationType.独立开发者)
            AddPerk(new Perk41(emp));
        else if (emp.Occupation == OccupationType.键盘艺术家)
            AddPerk(new Perk42(emp));
        else if (emp.Occupation == OccupationType.酒保)
            AddPerk(new Perk43(emp));
    }

    public int CalcSalary()
    {//暂时定为10
        return 10;
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
    //设置主导情绪
    public void SetMainEmotion(EColor color)
    {
        if (MainEmotion.Active == true)
        {
            if (MainEmotion.E.color == color)
                MainEmotion.TimeLeft = 2;
            else
            {
                Emotion newEmotion = new Emotion(color);
                emp.CurrentEmotions.Remove(MainEmotion.E);
                emp.CurrentEmotions.Add(newEmotion);
                MainEmotion.E = newEmotion;
                MainEmotion.TimeLeft = 2;
                MainEmotion.SetColor(newEmotion);
            }
        }
        else
        {
            Emotion newEmotion = new Emotion(color);
            emp.CurrentEmotions.Add(newEmotion);
            MainEmotion.SetColor(newEmotion);
            MainEmotion.TimeLeft = 2;
            MainEmotion.gameObject.SetActive(true);
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

    //检查并显示骰子
    public void CheckMarker()
    {
        for (int i = 0; i < SkillMarkers.Count; i++)
        {
            Destroy(SkillMarkers[i].gameObject);
        }
        SkillMarkers.Clear();
        foreach(int[] count in emp.CurrentDices)
        {
            int num = 0;
            foreach(int n in count)
            {
                if (n != -1)
                    num += 1;
                else
                    break;
            }

            BSSkillMarker m = Instantiate(GC.BSC.MarkerPrefab, MarkerContent).GetComponent<BSSkillMarker>();
            m.SetInfo(count[0], num);
            SkillMarkers.Add(m);
        }
    }

    //检查并显示已有的岗位优势
    public void CheckProfessions()
    {    //1.技术 2.市场 3.产品 4.Ob观察 5.Te坚韧 6.Str强壮 7.Ma管理 8.HR人力 9.Fi财务 10.De决策 
         //11.Fo行业 12.St谋略 13.Co说服 14.Ch魅力 15.Go八卦 16.Ad行政
         //int[] Nst = { 1, 2, 3, 8, 9, 11, 12, 13, 15, 16 };//Nst:几个专业技能对应的编号
        Text_Professions.text = "";
        foreach(int num in emp.Professions)
        {
            if (num == 1)
                Text_Professions.text += "技术/";
            else if (num == 2)
                Text_Professions.text += "市场/";
            else if (num == 3)
                Text_Professions.text += "产品/";
            else if (num == 8)
                Text_Professions.text += "人力/";
            else if (num == 9)
                Text_Professions.text += "财务/";
            else if (num == 11)
                Text_Professions.text += "行业/";
            else if (num == 12)
                Text_Professions.text += "谋略/";
            else if (num == 13)
                Text_Professions.text += "说服/";
            else if (num == 15)
                Text_Professions.text += "八卦/";
            else if (num == 16)
                Text_Professions.text += "行政/";
        }
    }

}
