using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillInfo : MonoBehaviour
{
    public int InfoType;
    public bool Active = true;

    public Skill skill;
    public SkillControl SC;
    public EmpInfo empInfo;
    public InfoPanel info;

    public Text Text_Name, Text_EmpName, Text_StaminaCost, Text_DiceCost, Text_Description;
    public Button button;


    float Timer = 0;
    bool ShowPanel;

    private void Update()
    {
        if (ShowPanel == true)
        {
            if (Timer < 0.25f)
                Timer += Time.deltaTime;
            else
            {
                if (skill != null)
                {
                    info.Text_Name.text = skill.Name;
                    info.Text_Description.text = skill.Description;
                    if (InfoType == 0)
                        info.Text_ExtraInfo.text = "体力消耗:" + skill.StaminaCost + " 点数需求:" + skill.DiceCost;
                    else
                        info.Text_ExtraInfo.text = "体力消耗:" + (skill.StaminaCost - skill.StaminaExtra) + " 点数需求:" + (skill.DiceCost - skill.DiceExtra);
                }
                if (info.Visible == false)
                    info.ShowPanel();
                info.transform.position = Input.mousePosition;
            }
        }
        if(InfoType == 1)
        {
            if (skill != null)
            {
                Text_StaminaCost.text = (skill.StaminaCost - skill.StaminaExtra).ToString();
                Text_DiceCost.text = (skill.DiceCost - skill.DiceExtra).ToString();
            }
            else
            {
                Text_StaminaCost.text = "-";
                Text_DiceCost.text = "-";
            }
        }
    }

    public void PointerEnter()
    {
        ShowPanel = true;
    }

    public void PointerExit()
    {
        ShowPanel = false;
        info.ClosePanel();
        Timer = 0;
    }

    public void UpdateUI()
    {
        if (skill != null && empInfo != null)
        {
            Text_Name.text = skill.Name;
            Text_EmpName.text = empInfo.emp.Name;
            if (Text_Description != null)
                Text_Description.text = skill.Description;
        }
        else
        {
            Text_Name.text = "---";
            Text_EmpName.text = "---";
            Text_Description.text = "---";
        }
    }

    public void SetSCNum(int num)
    {
        SC.SelectNum = num;
        int SetNum1 = num / 6;
        SkillInfo[] s;
        if (SetNum1 == 0)
            s = SC.CSkillSetA;
        else if (SetNum1 == 1)
            s = SC.CSkillSetB;
        else
            s = SC.CSkillSetC;

        for(int i = 0; i < SC.TotalSkills.Count; i++)
        {
            SC.TotalSkills[i].gameObject.SetActive(true);
            for (int j = 0; j < 6; j++)
            {
                if(s[j].skill != null && s[j].skill == SC.TotalSkills[i].skill)
                {
                    SC.TotalSkills[i].gameObject.SetActive(false);
                    break;
                }
            }

        }
        SC.SkillSelectPanel.SetActive(true);
    }

    public void SkillSet()
    {
        int num = SC.SelectNum;
        int SetNum1 = num / 6;
        int SetNum2 = num % 6;
        if(SetNum1 == 0)
        {
            SC.CSkillSetA[SetNum2].skill = skill;
            SC.CSkillSetA[SetNum2].empInfo = empInfo;
            SC.CSkillSetA[SetNum2].UpdateUI();
            SC.TargetDep.DSkillSetA[SetNum2] = skill;
        }
        else if (SetNum1 == 1)
        {
            SC.CSkillSetB[SetNum2].skill = skill;
            SC.CSkillSetB[SetNum2].empInfo = empInfo;
            SC.CSkillSetB[SetNum2].UpdateUI();
            SC.TargetDep.DSkillSetB[SetNum2] = skill;
        }
        else if (SetNum1 == 2)
        {
            SC.CSkillSetC[SetNum2].skill = skill;
            SC.CSkillSetC[SetNum2].empInfo = empInfo;
            SC.CSkillSetC[SetNum2].UpdateUI();
            SC.TargetDep.DSkillSetC[SetNum2] = skill;
        }
        SC.SkillSelectPanel.SetActive(false);
    }

    public void SelectSkill()
    {
        SC.CurrentSkill = this;
        SC.Text_CurrentSkillName.text = "确定发动 " + skill.Name + " 能力？";
        if(skill.EffectMode == 1)
            SC.ConfirmPanel.SetActive(true);
        else
        {
            empInfo.GC.SelectMode = 4;
            empInfo.GC.TotalEmpContent.parent.parent.gameObject.SetActive(true);
        }

    }
}
