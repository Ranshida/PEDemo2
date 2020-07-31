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
            Text_TotalValue.text = totalValue.ToString();
        }
    }

    public List<SkillInfo> CurrentSkills = new List<SkillInfo>();
    public List<DiceControl> Dices = new List<DiceControl>();
    public List<DiceControl> SelectedDices = new List<DiceControl>();
    public SkillInfo CurrentSkill, SkillInfoPrefab;
    public GameObject ConfirmPanel;
    public GameControl GC;
    public DiceControl DicePrefab;
    public Transform DiceContent, SkillContent;
    public Text Text_TotalValue, Text_CurrentSkillName;
    public Button RollButton;
    public DepControl TargetDep = null;

    public bool SpecialSkill1Active = false;

    int DiceNum, totalValue;

    public void SetDice(DepControl dep)
    {
        TargetDep = dep;
        if(dep.CommandingOffice != null)
        {
            DiceNum = dep.CommandingOffice.ManageValue - dep.CommandingOffice.ControledDeps.Count + GC.ManageExtra;
        }
        for(int i = 0; i < dep.CurrentEmps.Count; i++)
        {
            for(int j = 0; j < dep.CurrentEmps[i].InfoDetail.SkillsInfo.Count; j++)
            {
                SkillInfo newSkill = Instantiate(SkillInfoPrefab, SkillContent);
                newSkill.skill = dep.CurrentEmps[i].InfoDetail.SkillsInfo[j].skill;
                newSkill.Text_Name.text = newSkill.skill.Name;
                newSkill.Text_EmpName.text = dep.CurrentEmps[i].Name;
                newSkill.SC = this;
                newSkill.empInfo = dep.CurrentEmps[i].InfoDetail;
                newSkill.button.interactable = false;
                newSkill.info = GC.infoPanel;
                CurrentSkills.Add(newSkill);
            }
        }
        TotalValue = 0;
        RollButton.interactable = true;
    }

    public void StartRoll()
    {
        CreateDice(DiceNum);
        RollButton.interactable = false;
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
        Dices.Clear();
        SelectedDices.Clear();
        CurrentSkills.Clear();
        TargetDep = null;
        SpecialSkill1Active = false;
        this.gameObject.SetActive(false);
    }

    public void SkillCheck()
    {
        for(int i = 0; i < CurrentSkills.Count; i++)
        {
            if ((TotalValue == CurrentSkills[i].skill.DiceCost - CurrentSkills[i].skill.DiceExtra ||
                CurrentSkills[i].skill.DiceCost == CurrentSkills[i].skill.DiceExtra) &&
                CurrentSkills[i].empInfo.emp.Stamina >= (CurrentSkills[i].skill.StaminaCost - CurrentSkills[i].skill.StaminaExtra)
                && CurrentSkills[i].skill.ConditionCheck() == true)
            {
                CurrentSkills[i].button.interactable = true;
            }
            else
                CurrentSkills[i].button.interactable = false;
        }
    }

    public void SkillConfirm()
    {
        CurrentSkill.skill.StartEffect();
        CurrentSkill.skill.DiceExtra = 0;
        CurrentSkill.skill.StaminaExtra = 0;
        for(int i = 0; i < SelectedDices.Count; i++)
        {
            SelectedDices[i].GetComponent<Toggle>().interactable = false;
        }
        TotalValue = 0;
        GC.SelectMode = 1;
        GC.TotalEmpContent.parent.parent.gameObject.SetActive(false);
        SkillCheck();
        if (SpecialSkill1Active == true)
            CreateDice(1);
    }
}
