using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillControl : MonoBehaviour
{
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
    public bool Sp1Active = false, AdvanceMobilize = false;
    //头脑风暴点数和无事件技能时间,2下一个基础技能获得点数倍率,3每用一个骰子头脑风暴点数+1
    //4非基础技能消耗倍率, 需要额外添加的点数1骰子数量
    public int SelectNum = 0, CurrentPoint = 0, EventLimit = 0, Sp2Multiply = 0, Sp3Multiply = 0, Sp4Multiply = 0, Sp5Multiply = 0;


    public SkillInfo CurrentSkill, SkillInfoPrefab, SkillInfoPrefab2;
    public GameObject ConfirmPanel, EventPanel, SkillSelectPanel;
    public GameControl GC;
    public DiceControl DicePrefab;
    public Transform DiceContent, SkillContent, SkillSelectContent;
    public Text Text_TotalValue, Text_CurrentSkillName, Text_EventDescription, Text_Point;
    public Button RollButton;
    public DepControl TargetDep = null;

    public List<SkillInfo> CurrentSkills = new List<SkillInfo>(), TotalSkills = new List<SkillInfo>();
    public List<DiceControl> Dices = new List<DiceControl>();
    public List<DiceControl> SelectedDices = new List<DiceControl>();
    public SkillInfo[] CSkillSetA = new SkillInfo[6], CSkillSetB = new SkillInfo[6], CSkillSetC = new SkillInfo[6];

    int DiceNum, totalValue, RequirePoint;

    public void SetDice(DepControl dep)
    {
        CurrentPoint = 0;
        if (dep.SpTime > 0)
            CurrentPoint += 5;
        if (dep.EfficiencyLevel == 0)
            RequirePoint = 20;
        else if (dep.EfficiencyLevel == 1)
            RequirePoint = 40;
        else if (dep.EfficiencyLevel == 2)
            RequirePoint = 80;
        else if (dep.EfficiencyLevel == 3)
            RequirePoint = 160;
        else if (dep.EfficiencyLevel == 4)
            RequirePoint = 320;
        else if (dep.EfficiencyLevel == 5)
            RequirePoint = 640;
        if (GC.DoubleMobilizeCost > 0)
            RequirePoint *= 2;
        Text_Point.text = "当前点数:" + CurrentPoint + "\n下一级所需点数:" + RequirePoint;

        TargetDep = dep;
        ShowSkill();

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
        }
        TotalValue = 0;
        RollButton.interactable = true;
    }

    public void StartRoll(int num)
    {
        if (TargetDep.CommandingOffice != null)
        {
            DiceNum = (TargetDep.CommandingOffice.ManageValue - TargetDep.CommandingOffice.ControledDeps.Count + GC.ManageExtra) / 2;
            GC.UpdateResourceInfo();
        }
        CreateDice(DiceNum);
        RollButton.interactable = false;

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
            }
        }
        if (AdvanceMobilize == true)
            Sp2Multiply += 1;
    }

    public void ShowSetPanel(bool Start)
    {

    }

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

    public void ClearPanel()
    {
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
        if (CurrentPoint >= RequirePoint)
        {
            if (TargetDep.EfficiencyLevel < 5)
                TargetDep.EfficiencyLevel += 1;
            TargetDep.Efficiency += 0.2f;
            TargetDep.LevelDownTime = 96;
            TargetDep.Text_LevelDownTime.text = "降级时间:" + TargetDep.LevelDownTime + "时";
            if (GC.DoubleMobilizeCost > 0)
                GC.DoubleMobilizeCost -= 1;
        }
        Dices.Clear();
        SelectedDices.Clear();
        CurrentSkills.Clear();
        TotalSkills.Clear();
        CurrentPoint = 0;
        EventLimit = 0;
        TargetDep = null;
        Sp1Active = false;
        Sp2Multiply = 0;
        Sp3Multiply = 0;
        Sp4Multiply = 0;
        Sp5Multiply = 0;
        this.gameObject.SetActive(false);
    }

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
        int TempPoint = CurrentPoint;

        //先设置骰子状态
        for (int i = 0; i < SelectedDices.Count; i++)
        {
            Toggle t = SelectedDices[i].GetComponent<Toggle>();
            if (t.interactable == true)
            {
                t.interactable = false;
                if (Sp3Multiply > 0)
                    CurrentPoint += Sp3Multiply;
            }
        }

        CurrentSkill.skill.StartEffect();
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
        GC.SelectMode = 1;
        GC.TotalEmpContent.parent.parent.gameObject.SetActive(false);
        SkillCheck();

        if (Sp1Active == true)
            CreateDice(1);
        if (Sp2Multiply > 0 && CurrentSkill.skill.Type == SkillType.Basic)
        {
            CurrentPoint += TempPoint * Sp2Multiply + TempPoint;
            Sp2Multiply = 0;
        }
        if (EventLimit > 0)
            EventLimit -= 1;
        else
            RandomEvent();

        Text_Point.text = "当前点数:" + CurrentPoint + "\n下一级所需点数:" + RequirePoint; 
    }  

    public void RandomEvent()
    {
        int type = Random.Range(1, 8);
        string Description = "";
        if(type == 1)
        {
            Description = "全体恢复10点心力";
            for(int i = 0; i < TargetDep.CurrentEmps.Count; i++)
            {
                TargetDep.CurrentEmps[i].Mentality += 10;
            }
        }
        else if (type == 2)
        {
            Description = "全体员工降低20点心力";
            for (int i = 0; i < TargetDep.CurrentEmps.Count; i++)
            {
                TargetDep.CurrentEmps[i].Mentality -= 20;
            }
        }
        else if (type == 3)
        {
            Description = "随机一个非基础技能不可用";
            List<SkillInfo> S = new List<SkillInfo>();
            for(int i = 0; i < CurrentSkills.Count; i++)
            {
                if (CurrentSkills[i].Active == true && CurrentSkills[i].skill.Type != SkillType.Basic)
                    S.Add(CurrentSkills[i]);
            }
            if (S.Count > 0)
            {
                int num = Random.Range(0, S.Count);
                S[num].Active = false;
                S[num].gameObject.SetActive(false);
            }
        }
        else if (type == 4)
        {
            Description = "一名员工退场（技能不可用）";
            Employee E = TargetDep.CurrentEmps[Random.Range(0, TargetDep.CurrentEmps.Count)];
            for(int i = 0; i < CurrentSkills.Count; i++)
            {
                if (CurrentSkills[i].skill.TargetEmp == E)
                    CurrentSkills[i].gameObject.SetActive(false);
            }
        }
        else if (type == 5)
        {
            Description = "下一次使用非基础技能的消耗翻倍";
            if (Sp4Multiply == 0)
            {
                for (int i = 0; i < CurrentSkills.Count; i++)
                {
                    if (CurrentSkills[i].skill.Type != SkillType.Basic)
                        CurrentSkills[i].skill.StaminaExtra = CurrentSkills[i].skill.StaminaCost;
                }
            }
            else
            {
                for (int i = 0; i < CurrentSkills.Count; i++)
                {
                    if (CurrentSkills[i].skill.Type != SkillType.Basic)
                        CurrentSkills[i].skill.StaminaExtra *= 2;
                }
            }
            Sp4Multiply += 1;
        }
        else if (type == 6)
        {
            Description = "所有基础技能消耗翻倍";
            for (int i = 0; i < CurrentSkills.Count; i++)
            {
                if (CurrentSkills[i].skill.Type == SkillType.Basic)
                {
                    if (CurrentSkills[i].skill.StaminaExtra == 0)
                        CurrentSkills[i].skill.StaminaExtra = CurrentSkills[i].skill.StaminaCost * -1;
                    else
                        CurrentSkills[i].skill.StaminaExtra *= 2;
                }
            }
        }
        else if (type == 7)
        {
            Description = "所有技能必须额外使用一枚点数为1的骰子才可以使用";
            Sp5Multiply += 1;
        }
        Text_EventDescription.text = Description;
        EventPanel.SetActive(true);
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
}
