using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillControl : MonoBehaviour
{
    //选中的骰子点数
    public int TotalValue
    {
        get { return totalValue; }
        set
        {
            totalValue = value;
            Text_TotalValue.text = "骰子点数:" + totalValue.ToString();
        }
    }
    //1发动技能后加一个1点骰子, 4下一个非基础技能消耗翻倍
    public bool AdvanceMobilize = false;
    //头脑风暴点数和无事件技能时间,2下一个基础技能获得点数倍率,3每用一个骰子头脑风暴点数+1
    //4非基础技能消耗倍率, 需要额外添加的点数1骰子数量
    public int SelectNum = 0, CurrentPoint = 0, EventLimit = 0, Sp2Multiply = 0, Sp3Multiply = 0, Sp4Multiply = 0, Sp5Multiply = 0, 
        BossHp = 0, Sp1Multiply = 0;

    public int DiceUseNum = 0;//用于记录每回合使用的骰子数量
    public int DotValue = 0;//每回合持续伤害(洞察)
    public int ExtraDamage = 0;//每回合附加伤害(想象力)
    public int ExtraDiceDamage = 0;//每使用一颗骰子造成的额外伤害
    public int DiceSelectType = 0;//一些需要选择骰子的技能的类型
    public int SCSelectMode = 0;//选择模式0为无 1为选择骰子 2为选择技能
    public int SkillLockBonus = 0;//本回合每锁定一个技能额外生成两个骰子效果的层数
    public int TurnLeft = 0;//距离可以结束的回合数
    public int ExtraDiceNum = 0;//下回合额外获得的骰子数量
    public bool NoStaminaCost = false; //下一个技能无消耗buff
    public bool DoubleCost = false; //下一个技能消耗翻倍buff

    public SkillInfo CurrentSkill, SkillInfoPrefab, SkillInfoPrefab2, TargetSkill;
    public GameObject ConfirmPanel, EventPanel, SkillSelectPanel, BossPanel, VictoryPanel;
    public GameControl GC;
    public DiceControl DicePrefab, TargetDice;
    public Transform DiceContent, SkillContent, SkillSelectContent;
    public Text Text_TotalValue, Text_CurrentSkillName, Text_EventDescription, Text_Point, Text_Tip, Text_BossHp, Text_BossLevel, 
        Text_BossAction, Text_TurnLeft, Text_DotValue, Text_ExtraDamage;
    public Button RollButton, EndButton;
    public DepControl TargetDep = null;

    public List<SkillInfo> CurrentSkills = new List<SkillInfo>(), TotalSkills = new List<SkillInfo>();
    public List<DiceControl> Dices = new List<DiceControl>();
    public List<DiceControl> SelectedDices = new List<DiceControl>();
    public SkillInfo[] CSkillSetA = new SkillInfo[6], CSkillSetB = new SkillInfo[6], CSkillSetC = new SkillInfo[6];

    int DiceNum, totalValue, RequirePoint, BossLevel, NextBossSkill;

    Employee TargetEmployee; //Boss目标员工

    List<Employee> InvolvedEmps = new List<Employee>(); //参与头脑风暴的员工

    private void Update()
    {
        Text_DotValue.text = "洞察:" + DotValue;
        Text_ExtraDamage.text = "想象力:" + ExtraDamage;
    }

    //初始化面板
    public void SetDice(DepControl dep)
    {
        //头脑风暴版本1的点数需求设置
        //CurrentPoint = 0;
        //if (dep.SpTime > 0)
        //    CurrentPoint += 5;
        //if (dep.EfficiencyLevel == 0)
        //    RequirePoint = 20;
        //else if (dep.EfficiencyLevel == 1)
        //    RequirePoint = 40;
        //else if (dep.EfficiencyLevel == 2)
        //    RequirePoint = 80;
        //else if (dep.EfficiencyLevel == 3)
        //    RequirePoint = 160;
        //else if (dep.EfficiencyLevel == 4)
        //    RequirePoint = 320;
        //else if (dep.EfficiencyLevel == 5)
        //    RequirePoint = 640;
        //if (GC.DoubleMobilizeCost > 0)
        //    RequirePoint *= 2;
        //Text_Point.text = "当前点数:" + CurrentPoint + "\n下一级所需点数:" + RequirePoint;

        //头脑风暴版本2 设定Boss等级
        BossLevel = dep.EfficiencyLevel + 1;
        GC.ForceTimePause = true;
        if (dep.EmpChanged == true && BossLevel > 1)
            BossLevel -= 1;
        if (BossLevel == 1)
            BossHp = 50;
        else if (BossLevel == 2)
            BossHp = 100;
        else if (BossLevel == 3)
            BossHp = 200;
        else if (BossLevel == 4)
            BossHp = 300;
        else if (BossLevel >= 5)
        {
            BossHp = 500;
            BossLevel = 5;
        }
        TurnLeft = 6;
        //初始化各项Text
        Text_BossHp.text = "Boss剩余血量:" + BossHp;
        Text_BossLevel.text = "等级:" + BossLevel;
        Text_TurnLeft.text = "剩余回合:" + TurnLeft;

        TargetDep = dep;
        ShowSkill();

        //确认所有技能
        for(int i = 0; i < dep.CurrentEmps.Count; i++)
        {
            for(int j = 0; j < dep.CurrentEmps[i].InfoDetail.SkillsInfo.Count; j++)
            {
                SkillInfo newSkill = Instantiate(SkillInfoPrefab, SkillSelectContent);
                newSkill.skill = dep.CurrentEmps[i].InfoDetail.SkillsInfo[j].skill;
                newSkill.SC = this;
                newSkill.empInfo = dep.CurrentEmps[i].InfoDetail;
                newSkill.UpdateUI();
                newSkill.info = GC.infoPanel;
                TotalSkills.Add(newSkill);
            }
            //重置信心
            dep.CurrentEmps[i].InfoDetail.emp.Confidence = 0;
        }
        //确定管理人员技能
        if(dep.CommandingOffice != null && dep.CommandingOffice.CurrentManager != null)
        {
            for(int i = 0; i < dep.CommandingOffice.CurrentManager.InfoDetail.SkillsInfo.Count; i++)
            {
                if(dep.CommandingOffice.CurrentManager.InfoDetail.SkillsInfo[i].skill.ManageSkill == true)
                {
                    SkillInfo newSkill = Instantiate(SkillInfoPrefab, SkillSelectContent);
                    newSkill.skill = dep.CommandingOffice.CurrentManager.InfoDetail.SkillsInfo[i].skill;
                    newSkill.SC = this;
                    newSkill.empInfo = dep.CommandingOffice.CurrentManager.InfoDetail;
                    newSkill.UpdateUI();
                    newSkill.info = GC.infoPanel;
                    TotalSkills.Add(newSkill);
                }
            }
        }
        TotalValue = 0;
        RollButton.interactable = true;
    }

    //投掷
    public void StartRoll(int num)
    {
        //清空之前参与头脑风暴的员工列表
        InvolvedEmps.Clear();
        //通过几个return设置开始头脑风暴的条件，可能以后无法开始需要提示面板
        if (TargetDep.CommandingOffice != null)
        {
            if (TargetDep.CommandingOffice.CurrentManager == null)
                return;
            DiceNum = (TargetDep.CommandingOffice.ManageValue - TargetDep.CommandingOffice.ControledDeps.Count + GC.ManageExtra) / 2;
            GC.UpdateResourceInfo();
            InvolvedEmps.Add(TargetDep.CommandingOffice.CurrentManager);
        }
        else
            return;

        //确定并生成预设技能
        SkillInfo[] s;
        if (num == 1)
            s = CSkillSetA;
        else if (num == 2)
            s = CSkillSetB;
        else
            s = CSkillSetC;

        for (int j = 0; j < 6; j++)
        {
            if (s[j].skill != null)
            {
                SkillInfo newSkill = Instantiate(SkillInfoPrefab2, SkillContent);
                newSkill.skill = s[j].skill;
                newSkill.SC = this;
                newSkill.empInfo = s[j].skill.TargetEmp.InfoDetail;
                newSkill.UpdateUI();
                newSkill.button.interactable = false;
                CurrentSkills.Add(newSkill);
                if (InvolvedEmps.Contains(newSkill.empInfo.emp) == false)
                    InvolvedEmps.Add(newSkill.empInfo.emp);
            }
        }
        if (CurrentSkills.Count == 0)
            return;

        CreateDice(DiceNum);
        BossPanel.SetActive(true);
        RandomBossSkill();
        RollButton.interactable = false;
        EndButton.interactable = false;

        if (AdvanceMobilize == true)
            Sp2Multiply += 1;
        SkillCheck();
    }

    //创建骰子
    public void CreateDice(int num, int value = 0)
    {
        for (int i = 0; i < num; i++)
        {
            DiceControl newDice = Instantiate(DicePrefab, DiceContent);
            newDice.SC = this;
            Dices.Add(newDice);
            if (value > 0)
            {
                newDice.value = value;
                newDice.Text_Value.text = value.ToString();
            }
            else
                newDice.RandomValue();
        }
    }

    //清空面板，重置各项数值
    public void ClearPanel()
    {
        GC.ForceTimePause = false;
        if (TargetDep != null && TargetDep.EmpPanel != null)
            TargetDep.EmpPanel.gameObject.SetActive(false);
        for(int i = 0; i < Dices.Count; i++)
        {
            Destroy(Dices[i].gameObject);
        }
        for(int i = 0; i < CurrentSkills.Count; i++)
        {
            CurrentSkills[i].skill.DiceExtra = 0;
            CurrentSkills[i].skill.StaminaExtra = 0;
            Destroy(CurrentSkills[i].gameObject);
        }
        for (int i = 0; i < TotalSkills.Count; i++)
        {
            TotalSkills[i].skill.DiceExtra = 0;
            TotalSkills[i].skill.StaminaExtra = 0;
            Destroy(TotalSkills[i].gameObject);
        }
        for (int i = 0; i < TargetDep.CurrentEmps.Count; i++)//重置信心
        {
            TargetDep.CurrentEmps[i].Confidence = 0;
        }
        if (CurrentPoint >= RequirePoint)
        {
            //if (TargetDep.EfficiencyLevel < 5)
            //    TargetDep.EfficiencyLevel += 1;
            //TargetDep.Efficiency += 0.2f;
            //TargetDep.LevelDownTime = 96;
            //TargetDep.Text_LevelDownTime.text = "降级时间:" + TargetDep.LevelDownTime + "时";
            //if (GC.DoubleMobilizeCost > 0)
            //    GC.DoubleMobilizeCost -= 1;
        }
        Dices.Clear();
        SelectedDices.Clear();
        CurrentSkills.Clear();
        TotalSkills.Clear();
        CurrentPoint = 0;
        EventLimit = 0;
        TargetDep = null;
        Sp1Multiply = 0;
        Sp2Multiply = 0;
        Sp3Multiply = 0;
        Sp4Multiply = 0;
        Sp5Multiply = 0;

        //第二版头脑风暴相关数值重置
        DiceUseNum = 0;
        DotValue = 0;
        ExtraDamage = 0;
        ExtraDiceDamage = 0;
        ExtraDiceNum = 0;
        DiceSelectType = 0;

        RollButton.interactable = true;
        EndButton.interactable = true;
        BossPanel.SetActive(false);
        VictoryPanel.SetActive(false);
        this.gameObject.SetActive(false);
    }

    //检测当前点数（和体力）下可用的技能
    public void SkillCheck()
    {
        for(int i = 0; i < CurrentSkills.Count; i++)
        {
            if (Sp5Multiply == 0)
            {
                if ((TotalValue == CurrentSkills[i].skill.DiceCost - CurrentSkills[i].skill.DiceExtra ||
                    CurrentSkills[i].skill.DiceCost == CurrentSkills[i].skill.DiceExtra) &&
                    CurrentSkills[i].empInfo.emp.Stamina >= (CurrentSkills[i].skill.StaminaCost - CurrentSkills[i].skill.StaminaExtra)
                    && CurrentSkills[i].skill.ConditionCheck() == true && CurrentSkills[i].Active == true)
                {
                    CurrentSkills[i].button.interactable = true;
                }
                else
                    CurrentSkills[i].button.interactable = false;
            }
            else
            {
                if (ExtraDiceCheck(CurrentSkills[i]) == true &&
                    CurrentSkills[i].empInfo.emp.Stamina >= (CurrentSkills[i].skill.StaminaCost - CurrentSkills[i].skill.StaminaExtra)
                    && CurrentSkills[i].skill.ConditionCheck() == true && CurrentSkills[i].Active == true)
                {
                    CurrentSkills[i].button.interactable = true;
                }
                else
                    CurrentSkills[i].button.interactable = false;
            }
        }
    }

    //显示预设
    public void ShowSkill()
    {
        for(int i = 0; i < 6; i++)
        {
            if (TargetDep.DSkillSetA[i] != null)
            {
                CSkillSetA[i].skill = TargetDep.DSkillSetA[i];
                CSkillSetA[i].empInfo = TargetDep.DSkillSetA[i].TargetEmp.InfoDetail;
            }
            else
            {
                CSkillSetA[i].skill = null;
                CSkillSetA[i].empInfo = null;
            }
            CSkillSetA[i].UpdateUI();

            if (TargetDep.DSkillSetB[i] != null)
            {
                CSkillSetB[i].skill = TargetDep.DSkillSetB[i];
                CSkillSetB[i].empInfo = TargetDep.DSkillSetB[i].TargetEmp.InfoDetail;
            }
            else
            {
                CSkillSetB[i].skill = null;
                CSkillSetB[i].empInfo = null;
            }
            CSkillSetB[i].UpdateUI();

            if (TargetDep.DSkillSetC[i] != null)
            {
                CSkillSetC[i].skill = TargetDep.DSkillSetC[i];
                CSkillSetC[i].empInfo = TargetDep.DSkillSetC[i].TargetEmp.InfoDetail;
            }
            else
            {
                CSkillSetC[i].skill = null;
                CSkillSetC[i].empInfo = null;
            }
            CSkillSetC[i].UpdateUI();
        }
    }

    public void SkillConfirm()
    {
        int TempPoint = CurrentPoint, ExDamage = 0;

        List<DiceControl> DesDices = new List<DiceControl>();

        //先设置骰子状态
        for (int i = 0; i < SelectedDices.Count; i++)
        {
            //if (Sp3Multiply > 0)
            //    CurrentPoint += Sp3Multiply;
            ExDamage += ExtraDiceDamage;
            DesDices.Add(SelectedDices[i]);
        }
        for(int i = 0; i < DesDices.Count; i++)
        {
            Dices.Remove(DesDices[i]);
            SelectedDices.Remove(DesDices[i]);
            Destroy(DesDices[i].gameObject);
        }
        if (ExDamage > 0)
            CauseDamage(ExDamage);

        CurrentSkill.skill.StartEffect();//发动技能效果

        if(Sp4Multiply > 0)
        {
            for(int i = 0; i < CurrentSkills.Count; i++)
            {
                if (CurrentSkills[i].skill.Type != SkillType.Basic)
                    CurrentSkills[i].skill.StaminaExtra = 0;
            }
        }

        //获得本次效果作用后增加的点数
        TempPoint = CurrentPoint - TempPoint;
        TotalValue = 0;

        SkillCheck();

        if (Sp1Multiply > 0)
            CreateDice(Sp1Multiply);
        if (Sp2Multiply > 0 && CurrentSkill.skill.Type == SkillType.Basic)
        {
            //CurrentPoint += TempPoint * Sp2Multiply + TempPoint;
            //Sp2Multiply = 0;
        }
        if (EventLimit > 0)
            EventLimit -= 1;
        else
            RandomEvent();

        //重置部分属性
        SCSelectMode = 0;
        GC.CurrentEmpInfo2 = null;
        GC.CurrentEmpInfo = null;
        GC.ResetSelectMode();
        GC.TotalEmpContent.parent.parent.gameObject.SetActive(false);
        Text_Tip.gameObject.SetActive(false);

        Text_Point.text = "当前点数:" + CurrentPoint + "\n下一级所需点数:" + RequirePoint; 
    }  

    //第一版头脑风暴随机事件(已弃用)
    public void RandomEvent()
    {
        //int type = Random.Range(1, 8);
        //string Description = "";
        //if(type == 1)
        //{
        //    Description = "全体恢复10点心力";
        //    for(int i = 0; i < TargetDep.CurrentEmps.Count; i++)
        //    {
        //        TargetDep.CurrentEmps[i].Mentality += 10;
        //    }
        //}
        //else if (type == 2)
        //{
        //    Description = "全体员工降低20点心力";
        //    for (int i = 0; i < TargetDep.CurrentEmps.Count; i++)
        //    {
        //        TargetDep.CurrentEmps[i].Mentality -= 20;
        //    }
        //}
        //else if (type == 3)
        //{
        //    Description = "随机一个非基础技能不可用";
        //    List<SkillInfo> S = new List<SkillInfo>();
        //    for(int i = 0; i < CurrentSkills.Count; i++)
        //    {
        //        if (CurrentSkills[i].Active == true && CurrentSkills[i].skill.Type != SkillType.Basic)
        //            S.Add(CurrentSkills[i]);
        //    }
        //    if (S.Count > 0)
        //    {
        //        int num = Random.Range(0, S.Count);
        //        S[num].Active = false;
        //        S[num].gameObject.SetActive(false);
        //    }
        //}
        //else if (type == 4)
        //{
        //    Description = "一名员工退场（技能不可用）";
        //    Employee E = TargetDep.CurrentEmps[Random.Range(0, TargetDep.CurrentEmps.Count)];
        //    for(int i = 0; i < CurrentSkills.Count; i++)
        //    {
        //        if (CurrentSkills[i].skill.TargetEmp == E)
        //            CurrentSkills[i].gameObject.SetActive(false);
        //    }
        //}
        //else if (type == 5)
        //{
        //    Description = "下一次使用非基础技能的消耗翻倍";
        //    if (Sp4Multiply == 0)
        //    {
        //        for (int i = 0; i < CurrentSkills.Count; i++)
        //        {
        //            if (CurrentSkills[i].skill.Type != SkillType.Basic)
        //                CurrentSkills[i].skill.StaminaExtra = CurrentSkills[i].skill.StaminaCost;
        //        }
        //    }
        //    else
        //    {
        //        for (int i = 0; i < CurrentSkills.Count; i++)
        //        {
        //            if (CurrentSkills[i].skill.Type != SkillType.Basic)
        //                CurrentSkills[i].skill.StaminaExtra *= 2;
        //        }
        //    }
        //    Sp4Multiply += 1;
        //}
        //else if (type == 6)
        //{
        //    Description = "所有基础技能消耗翻倍";
        //    for (int i = 0; i < CurrentSkills.Count; i++)
        //    {
        //        if (CurrentSkills[i].skill.Type == SkillType.Basic)
        //        {
        //            if (CurrentSkills[i].skill.StaminaExtra == 0)
        //                CurrentSkills[i].skill.StaminaExtra = CurrentSkills[i].skill.StaminaCost * -1;
        //            else
        //                CurrentSkills[i].skill.StaminaExtra *= 2;
        //        }
        //    }
        //}
        //else if (type == 7)
        //{
        //    Description = "所有技能必须额外使用一枚点数为1的骰子才可以使用";
        //    Sp5Multiply += 1;
        //}
        //Text_EventDescription.text = Description;
        //EventPanel.SetActive(true);
    }

    public bool ExtraDiceCheck(SkillInfo s)
    {
        if (Sp5Multiply == 0)
            return true;
        else
        {
            int num = 0;
            for(int i = 0; i < SelectedDices.Count; i++)
            {
                if (SelectedDices[i].GetComponent<Toggle>().interactable == true && SelectedDices[i].value == 1)
                    num += 1;
            }
            if (num >= Sp5Multiply && totalValue - s.skill.DiceExtra - Sp5Multiply == s.skill.DiceCost)
                return true;
            else
                return false;
        }
    }

    public void SkillLockBonusCheck()
    {
        for(int i = 0; i < SkillLockBonus; i++)
        {
            CreateDice(2);
        }
    }

    public void StartDiceSeletMode(int Type)
    {
        DiceSelectType = Type;
    }

    public void CauseDamage(int damage)
    {
        damage += ExtraDamage;
        damage += damage * Sp2Multiply;
        Sp2Multiply = 0;
        if (damage < 0)
            damage = 0;
        BossHp -= damage;
        Text_BossHp.text = "Boss剩余血量:" + BossHp;

        if(BossHp <= 0)
        {
            VictoryPanel.SetActive(true);
            BossPanel.SetActive(false);
        }
    }

    //Boss回合
    public void BossTurn()
    {
        CauseDamage(DotValue);//先造成持续伤害
        if (BossHp <= 0)
            return;
        ActiveBossSkill();
        NextTurn();
    }
    //根据Boss等级随机一个技能
    void RandomBossSkill()
    {
        int num = 1;
        if(BossLevel == 1)
        {
            num = Random.Range(1, 5);
            if (num == 1)
                NextBossSkill = 1;
            else if (num == 2)
                NextBossSkill = 4;
            else if (num == 3)
                NextBossSkill = 8;
            else
                NextBossSkill = 9;
        }
        else if (BossLevel == 2)
        {
            num = Random.Range(1, 5);
            if (num == 1)
                NextBossSkill = 1;
            else if (num == 2)
                NextBossSkill = 5;
            else if (num == 3)
                NextBossSkill = 10;
            else
                NextBossSkill = 11;
        }
        else if (BossLevel == 3)
        {
            num = Random.Range(1, 5);
            if (num == 1)
                NextBossSkill = 2;
            else if (num == 2)
                NextBossSkill = 6;
            else if (num == 3)
                NextBossSkill = 10;
            else
                NextBossSkill = 12;
        }
        else if (BossLevel == 4)
        {
            num = Random.Range(1, 4);
            if (num == 1)
                NextBossSkill = 3;
            else if (num == 2)
                NextBossSkill = 6;
            else
                NextBossSkill = 12;
        }
        else if (BossLevel == 5)
        {
            num = Random.Range(1, 7);
            if (num == 1)
                NextBossSkill = 8;
            else if (num == 2)
                NextBossSkill = 11;
            else if (num == 3)
                NextBossSkill = 7;
            else if (num == 4)
                NextBossSkill = 3;
            else if (num == 5)
                NextBossSkill = 12;
            else if (num == 6)
                NextBossSkill = 10;
        }

        SetBossSkill();
    }
    //设定技能目标和更新UI
    void SetBossSkill()
    {
        //随机目标员工和技能
        TargetEmployee = InvolvedEmps[Random.Range(0, InvolvedEmps.Count)];
        if (NextBossSkill == 1)
            Text_BossAction.text = "Boss下一个行为:全体降低15点心力";
        else if (NextBossSkill == 2)
            Text_BossAction.text = "Boss下一个行为:全体降低25点心力";
        else if (NextBossSkill == 3)
            Text_BossAction.text = "Boss下一个行为:全体降低30点心力";
        else if (NextBossSkill == 4)
            Text_BossAction.text = "Boss下一个行为:降低" + TargetEmployee.Name + "心力35点";
        else if (NextBossSkill == 5)
            Text_BossAction.text = "Boss下一个行为:降低" + TargetEmployee.Name + "心力40点";
        else if (NextBossSkill == 6)
            Text_BossAction.text = "Boss下一个行为:降低" + TargetEmployee.Name + "心力45点";
        else if (NextBossSkill == 7)
            Text_BossAction.text = "Boss下一个行为:降低" + TargetEmployee.Name + "心力55点";
        else if (NextBossSkill == 8)
            Text_BossAction.text = "Boss下一个行为:下一次技能的消耗翻倍";
        else if (NextBossSkill == 9)
            Text_BossAction.text = "Boss下一个行为:禁用一个技能3回合";
        else if (NextBossSkill == 10)
            Text_BossAction.text = "Boss下一个行为:会议想象力-1";
        else if (NextBossSkill == 11)
            Text_BossAction.text = "Boss下一个行为:下回合少获得n个骰子";
        else if (NextBossSkill == 12)
            Text_BossAction.text = "Boss下一个行为:禁用" + TargetEmployee.Name + "技能5回合";
    }
    //施展技能效果
    void ActiveBossSkill()
    {
        //全体降低n点心力
        if (NextBossSkill <= 3)
        {
            int value = 15;//NextBossSkill == 1
            if (NextBossSkill == 2)
                value = 25;
            else if (NextBossSkill == 3)
                value = 30;

            for (int i = 0; i < InvolvedEmps.Count; i++)
            {
                InvolvedEmps[i].Mentality -= value;
            }
        }

        //一名员工降低n点心力
        else if (NextBossSkill <= 7)
        {
            int value = 35; //NextBossSkill == 4
            if (NextBossSkill == 5)
                value = 40;
            else if (NextBossSkill == 6)
                value = 45;
            else if (NextBossSkill == 7)
                value = 55;
            TargetEmployee.Mentality -= value;
        }

        //下一次技能的消耗翻倍
        else if (NextBossSkill == 8)
        {
            for (int i = 0; i < CurrentSkills.Count; i++)
            {
                CurrentSkills[i].skill.StaminaExtra = CurrentSkills[i].skill.StaminaCost * -1;

                CurrentSkills[i].skill.DiceExtra = CurrentSkills[i].skill.DiceCost * -1;
            }
            DoubleCost = true;
        }

        //禁用一个技能3回合
        else if (NextBossSkill == 9)
        {
            SkillInfo s = CurrentSkills[Random.Range(0, CurrentSkills.Count)];
            s.LockTime += 3;
            s.gameObject.SetActive(false);
            SkillLockBonusCheck();
        }

        //会议想象力-1
        else if (NextBossSkill == 10)
            ExtraDamage -= 1;

        //下回合少获得n个骰子
        else if (NextBossSkill == 11)
        {
            int maxValue = (TargetDep.CommandingOffice.ManageValue - TargetDep.CommandingOffice.ControledDeps.Count + GC.ManageExtra) / 2;
            if (maxValue <= 0)
                maxValue = 2;
            ExtraDiceNum = Random.Range(1, maxValue) * -1;
        }

        //禁用一名员工5回合
        else if (NextBossSkill == 12)
        {
           for(int i = 0; i < CurrentSkills.Count; i++)
            {
                if(CurrentSkills[i].empInfo.emp == TargetEmployee)
                {
                    CurrentSkills[i].LockTime += 5;
                    CurrentSkills[i].gameObject.SetActive(false);
                }
            }
        }
    }

    //取消技能/骰子选择
    public void CancelSelect()
    {
        for(int i = 0; i < SelectedDices.Count; i++)
        {
            SelectedDices[i].toggle.interactable = true;
        }
        SkillCheck();
        Text_Tip.gameObject.SetActive(false);
        SCSelectMode = 0;
    }

    //下一回合
    public void NextTurn()
    {
        DiceUseNum = 0;//重置回合使用骰子数
        TotalValue = 0;//重置骰子点数
        if (DotValue > 0)//减Dot层数
            DotValue -= 1;
        ExtraDiceDamage = 0;//重置额外伤害
        SkillLockBonus = 0;//重置锁技能奖励层数
        Sp1Multiply = 0;//重置额外骰子奖励
        for(int i = 0; i < CurrentSkills.Count; i++)
        {
            //重置技能锁定时间
            if (CurrentSkills[i].LockTime > 0)
            {
                CurrentSkills[i].LockTime -= 1;
                if (CurrentSkills[i].LockTime == 0)
                    CurrentSkills[i].gameObject.SetActive(true);
            }
        }

        if (TurnLeft > 0)
            TurnLeft -= 1;
        else
            TurnLeft = 6;
        Text_TurnLeft.text = "剩余回合:" + TurnLeft;

        if (TurnLeft == 0)
            EndButton.interactable = true;
        else
            EndButton.interactable = false;

        for (int i = 0; i < Dices.Count; i++)
        {
            Destroy(Dices[i].gameObject);
        }
        Dices.Clear();
        SelectedDices.Clear();
        int dicenum = (TargetDep.CommandingOffice.ManageValue - TargetDep.CommandingOffice.ControledDeps.Count + GC.ManageExtra) / 2 + ExtraDiceNum;
        CreateDice(dicenum);
        ExtraDiceNum = 0;
        RandomBossSkill();
    }

    //Boss战胜利
    public void Victory()
    {
        if (TargetDep.EmpChanged == true)
        {
            TargetDep.EmpChanged = false;
            TargetDep.Text_Efficiency.color = Color.black;
        }
        else
            TargetDep.EfficiencyLevel += 1;
        if (TargetDep.EfficiencyLevel == 1)
            TargetDep.ExtraSuccessRate = 0.1f;
        else if (TargetDep.EfficiencyLevel == 2)
            TargetDep.ExtraSuccessRate = 0.2f;
        else if (TargetDep.EfficiencyLevel == 3)
        {
            TargetDep.ExtraSuccessRate = 0.2f;
            TargetDep.ExtraMajorSuccessRate = 0.2f;
        }
        else if (TargetDep.EfficiencyLevel == 4)
        {
            TargetDep.ExtraSuccessRate = 0.2f;
            TargetDep.ExtraMajorSuccessRate = 0.4f;
        }
        else if (TargetDep.EfficiencyLevel == 5)
        {
            TargetDep.ExtraSuccessRate = 0.3f;
            TargetDep.ExtraMajorSuccessRate = 0.5f;
        }
        ClearPanel();       
    }
}
