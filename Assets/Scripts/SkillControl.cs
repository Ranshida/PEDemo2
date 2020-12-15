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
    public int SelectNum = 0, CurrentPoint = 0, Sp2Multiply = 0, Sp3Multiply = 0, Sp4Multiply = 0, Sp5Multiply = 0, 
        BossHp = 0, Sp1Multiply = 0;

    public float ExtraSuccessRate = 0.0f, ExtraMajorSuccessRate = 0, ExtraMajorFailureRate = 0;//各部门的额外成功率加成
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
    public GameObject ConfirmPanel, EventPanel, SkillSelectPanel, BossPanel, VictoryPanel, SelectConfirmButton, PresetPanel;
    public GameControl GC;
    public DiceControl DicePrefab, TargetDice;
    public Transform DiceContent, SkillContent, SkillSelectContent, EmpContent;
    public Text Text_TotalValue, Text_CurrentSkillName, Text_EventDescription, Text_Point, Text_Tip, Text_BossHp, Text_BossLevel, 
        Text_BossAction, Text_TurnLeft, Text_DotValue, Text_ExtraDamage;
    public Button RollButton, EndButton, SkillSetButton;
    public DepControl TargetDep = null;
    public EmpInfo EmpInfoPrefab;

    public List<EmpInfo> SelectedEmps = new List<EmpInfo>();
    public List<SkillInfo> CurrentSkills = new List<SkillInfo>(), TotalSkills = new List<SkillInfo>();
    public List<DiceControl> Dices = new List<DiceControl>();
    public List<DiceControl> SelectedDices = new List<DiceControl>();
    public SkillInfo[] CSkillSetA = new SkillInfo[6], CSkillSetB = new SkillInfo[6], CSkillSetC = new SkillInfo[6];

    int DiceNum, totalValue, RequirePoint, BossLevel, NextBossSkill, SetNum;
    bool FightStart = false;

    Employee TargetEmployee; //Boss目标员工

    List<Employee> InvolvedEmps = new List<Employee>(); //参与头脑风暴的员工

    private void Update()
    {
        Text_DotValue.text = "洞察:" + DotValue;
        Text_ExtraDamage.text = "想象力:" + ExtraDamage;
    }

    //初始化面板
    public void SetStatus()
    {
        if (BossLevel == 1)
            BossHp = 25;
        else if (BossLevel == 2)
            BossHp = 50;
        else if (BossLevel == 3)
            BossHp = 100;
        else if (BossLevel == 4)
            BossHp = 150;
        else if (BossLevel >= 5)
        {
            BossHp = 250;
            BossLevel = 5;
        }
        TurnLeft = 6;
        //初始化各项Text
        Text_BossHp.text = "Boss剩余血量:" + BossHp;
        Text_BossLevel.text = "等级:" + BossLevel;
        Text_TurnLeft.text = "剩余回合:" + TurnLeft;

        TotalValue = 0;
    }

    public void SetPreSetNum(int num)
    {
        SetNum = num;
    }
    //投掷
    public void StartRoll()
    {
        //确定并生成预设技能
        SkillInfo[] s;
        if (SetNum == 1)
            s = CSkillSetA;
        else if (SetNum == 2)
            s = CSkillSetB;
        else
            s = CSkillSetC;

        Employee CurrentManager = null;
        foreach(SkillInfo i in s)
        {
            if(i.skill != null && i.skill.TargetEmp.CurrentOffice != null)
            {
                if(i.skill.TargetEmp.CurrentOffice.building.Type == BuildingType.CEO办公室 || i.skill.TargetEmp.CurrentOffice.building.Type == BuildingType.高管办公室)
                {
                    if (CurrentManager == null)
                        CurrentManager = i.skill.TargetEmp;
                    else if (i.skill.TargetEmp.Manage > CurrentManager.Manage)
                        CurrentManager = i.skill.TargetEmp; 
                }
            }
        }
        if (CurrentManager == null)
        {
            GC.CreateMessage("必须有一个高管上场");
            return;
        }

        //中途换技能时清空已有技能
        if (CurrentSkills.Count > 0)
        {
            for (int i = 0; i < CurrentSkills.Count; i++)
            {
                CurrentSkills[i].skill.DiceExtra = 0;
                CurrentSkills[i].skill.StaminaExtra = 0;
                Destroy(CurrentSkills[i].gameObject);
            }
            CurrentSkills.Clear();
        }
        PresetPanel.gameObject.SetActive(false);
        //清空之前参与头脑风暴的员工列表
        InvolvedEmps.Clear();
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
        {
            GC.CreateMessage("没有技能");
            return;
        }
        //初始时创建第一批骰子
        if (FightStart == false)
        {
            BossLevel = 1;
            SetStatus();
            DiceNum = CurrentManager.Manage / 2 + GC.ExtraDice;
            if (DiceNum > 6)
                DiceNum = 6;
            CreateDice(DiceNum);
            FightStart = true;
            RollButton.interactable = false;
        }
        BossPanel.SetActive(true);
        RandomBossSkill();
        EndButton.interactable = false;
        SkillSetButton.interactable = false;

        if (AdvanceMobilize == true)
            Sp2Multiply = 1;
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
        //直接退出的Debuff
        if(FightStart == false)
        {
            GC.Morale -= 50;
            GC.CreateMessage("直接放弃头脑风暴,士气-50");
            ExtraMajorSuccessRate = 0;
            ExtraSuccessRate = 0;
        }

        FightStart = false;
        GC.ForceTimePause = false;
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
        //重置信心
        foreach(EmpInfo e in SelectedEmps)
        {
            if (e.emp != null)
                e.emp.Confidence = 0;
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
        PresetPanel.SetActive(false);
        GC.MobTime = 192;
        GC.ForceTimePause = false;
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

        //重置部分属性
        SCSelectMode = 0;
        GC.CurrentEmpInfo2 = null;
        GC.CurrentEmpInfo = null;
        GC.ResetSelectMode();
        GC.TotalEmpContent.parent.parent.gameObject.SetActive(false);
        Text_Tip.gameObject.SetActive(false);

        Text_Point.text = "当前点数:" + CurrentPoint + "\n下一级所需点数:" + RequirePoint; 
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
                int tempvalue = value;
                if (InvolvedEmps[i].Confidence > tempvalue)
                    InvolvedEmps[i].Confidence -= tempvalue;
                else
                {
                    tempvalue -= InvolvedEmps[i].Confidence;
                    InvolvedEmps[i].Confidence = 0;
                    InvolvedEmps[i].Mentality -= tempvalue;
                    if (InvolvedEmps[i].Mentality == 0)
                        InvolvedEmps[i].Exhausted();
                }
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

            if (TargetEmployee.Confidence > value)
                TargetEmployee.Confidence -= value;
            else
            {
                value -= TargetEmployee.Confidence;
                TargetEmployee.Confidence = 0;
                TargetEmployee.Mentality -= value;
                if (TargetEmployee.Mentality == 0)
                    TargetEmployee.Exhausted();
            }
        }

        //下一次技能的消耗翻倍
        else if (NextBossSkill == 8)
        {
            for (int i = 0; i < CurrentSkills.Count; i++)
            {
                CurrentSkills[i].skill.StaminaExtra = CurrentSkills[i].skill.StaminaCost * -1;
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
            int maxValue = (TargetDep.CommandingOffice.ManageValue - TargetDep.CommandingOffice.ControledDeps.Count + GC.ExtraDice) / 2;
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
        if (AdvanceMobilize == true)//晨会制度，基本技翻倍效果
            Sp2Multiply = 1;
        for (int i = 0; i < CurrentSkills.Count; i++)
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
        {
            EndButton.interactable = true;
            SkillSetButton.interactable = true;
        }
        else
        { 
            EndButton.interactable = false;
            SkillSetButton.interactable = false;
        }
        for (int i = 0; i < Dices.Count; i++)
        {
            Destroy(Dices[i].gameObject);
        }
        Dices.Clear();
        SelectedDices.Clear();
        CreateDice(DiceNum + ExtraDiceNum);
        ExtraDiceNum = 0;
        RandomBossSkill();
    }

    //Boss战胜利
    public void Victory()
    {
        //重置骰子
        for (int i = 0; i < Dices.Count; i++)
        {
            Destroy(Dices[i].gameObject);
        }
        Dices.Clear();
        SelectedDices.Clear();
        CreateDice(DiceNum);
        ExtraDiceNum = 0;

        if (BossLevel == 1)
            ExtraSuccessRate = 0.1f;
        else if (BossLevel == 2)
            ExtraSuccessRate = 0.2f;
        else if (BossLevel == 3)
        {
            ExtraSuccessRate = 0.2f;
            ExtraMajorSuccessRate = 0.2f;
        }
        else if (BossLevel == 4)
        {
            ExtraSuccessRate = 0.2f;
            ExtraMajorSuccessRate = 0.4f;
        }
        else if (BossLevel == 5)
        {
            ExtraSuccessRate = 0.3f;
            ExtraMajorSuccessRate = 0.5f;
        }

        BossLevel += 1;
        SetStatus();
        SkillSetButton.interactable = true;
        EndButton.interactable = true;
        VictoryPanel.SetActive(false);
        BossPanel.SetActive(true);
    }

    //以下为全公司头脑风暴版本相关
    public void ShowEmpSelectPanel()
    {
        GC.TotalEmpContent.parent.parent.gameObject.SetActive(true);
        GC.ResetSelectMode();
        foreach(EmpInfo info in SelectedEmps)
        {
            //移除不存在的部分
            if (info.emp.InfoB == null)
            {
                SelectedEmps.Remove(info);
                Destroy(info.gameObject);
                print("Check");
                continue;
            }
            info.emp.InfoB.gameObject.SetActive(false);
            info.MoveButton.gameObject.SetActive(true);
        }
        GC.SelectMode = 9;
        SelectConfirmButton.SetActive(true);
    }
    public void InitEmpInfo(Employee emp)
    {
        if (SelectedEmps.Count >= 8)
        {
            GC.CreateMessage("最多选择8人");
            return;
        }
        EmpInfo info = Instantiate(EmpInfoPrefab, EmpContent);
        info.emp = emp;
        emp.InfoB.gameObject.SetActive(false);
        info.Text_Name.text = emp.Name;
        SelectedEmps.Add(info);
    }
    public void RemoveEmpInfo(EmpInfo info)
    {
        //重置技能
        foreach (SkillInfo s in CSkillSetA)
        {
            if (s.empInfo == info)
            {
                s.skill = null;
                s.empInfo = null;
                s.UpdateUI();
            }
        }
        foreach (SkillInfo s in CSkillSetB)
        {
            if (s.empInfo == info)
            {
                s.skill = null;
                s.empInfo = null;
                s.UpdateUI();
            }
        }
        foreach (SkillInfo s in CSkillSetC)
        {
            if (s.empInfo == info)
            {
                s.skill = null;
                s.empInfo = null;
                s.UpdateUI();
            }
        }
        info.emp.InfoB.gameObject.SetActive(true);
        SelectedEmps.Remove(info);
        Destroy(info.gameObject);
    }
    public void ConfirmEmpSelect()
    {
        bool HaveManager = false;
        foreach(EmpInfo info in SelectedEmps)
        {
            if(info.emp.CurrentOffice != null)
            {
                if(info.emp.CurrentOffice.building.Type == BuildingType.CEO办公室 || info.emp.CurrentOffice.building.Type == BuildingType.高管办公室)
                {
                    HaveManager = true;
                    break;
                }
            }
        }
        if (HaveManager == false)
        {
            GC.CreateMessage("需要至少一名高管");
            return;
        }
        GC.ResetSelectMode();
        GC.TotalEmpContent.parent.parent.gameObject.SetActive(false);
        SelectConfirmButton.SetActive(false);
        foreach (EmpInfo info in SelectedEmps)
        {
            //确认技能，关闭按钮
            info.MoveButton.gameObject.SetActive(false);
            for (int j = 0; j < info.emp.InfoDetail.SkillsInfo.Count; j++)
            {
                SkillInfo newSkill = Instantiate(SkillInfoPrefab, SkillSelectContent);
                newSkill.skill = info.emp.InfoDetail.SkillsInfo[j].skill;
                newSkill.SC = this;
                newSkill.empInfo = info.emp.InfoDetail;
                newSkill.UpdateUI();
                newSkill.info = GC.infoPanel;
                TotalSkills.Add(newSkill);
            }
            //重置信心
            info.emp.InfoDetail.emp.Confidence = 0;
        }
        PresetPanel.SetActive(true);
    }

}
