using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillInfo : MonoBehaviour
{
    public int InfoType, LockTime = 0;
    public bool Active = true;//目前用于禁用技能检测
    public GameObject ForbidSign, DeadSign;

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
            Text_Description.text = "点击此处选择技能";
        }
    }

    //不显示已进入预设的技能？
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
        SkillInfo[] s;
        if (SetNum1 == 0)
            s = SC.CSkillSetA;
        else if (SetNum1 == 1)
            s = SC.CSkillSetB;
        else
            s = SC.CSkillSetC;

        //确认人数不超过5人
        List<Employee> emps = new List<Employee>();
        foreach(SkillInfo info in s)
        {
            if (info.skill != null && emps.Contains(info.skill.TargetEmp) == false && info != s[SetNum2])
                emps.Add(info.skill.TargetEmp);
        }
        if (emps.Contains(skill.TargetEmp) == false)
            emps.Add(skill.TargetEmp);
        if(emps.Count > 5)
        {
            GameControl.Instance.CreateMessage("最多编入5人");
            return;
        }

        s[SetNum2].skill = skill;
        s[SetNum2].empInfo = empInfo;
        s[SetNum2].UpdateUI();
        SC.SkillSelectPanel.SetActive(false);
    }

    public void SkillRemove()
    {
        int num = SC.SelectNum;
        int SetNum1 = num / 6;
        int SetNum2 = num % 6;
        SkillInfo[] s;
        if (SetNum1 == 0)
            s = SC.CSkillSetA;
        else if (SetNum1 == 1)
            s = SC.CSkillSetB;
        else
            s = SC.CSkillSetC;

        s[SetNum2].skill = null;
        s[SetNum2].empInfo = null;
        s[SetNum2].UpdateUI();
        SC.SkillSelectPanel.SetActive(false);
    }

    public void SelectSkill()
    {
        if (empInfo.emp.Mentality <= 0)
        {
            GameControl.Instance.CreateMessage("该员工已退场");
            return;
        }
        if (SC.SCSelectMode != 2)
        {
            SC.CurrentSkill = this;
            SC.Text_CurrentSkillName.text = "确定发动 " + skill.Name + " 能力？";
            //直接发动技能
            if (skill.EffectMode == 1)
                SC.ConfirmPanel.SetActive(true);
            //选择一个员工后发动技能
            else if (skill.EffectMode == 2)
            {
                empInfo.GC.CurrentEmpInfo = null;
                empInfo.GC.SelectMode = 4;
                empInfo.GC.TotalEmpPanel.SetWndState(true);
                empInfo.GC.Text_EmpSelectTip.gameObject.SetActive(true);
                empInfo.GC.Text_EmpSelectTip.text = "选择一个员工";
            }
            //选择骰子后发动技能
            else if (skill.EffectMode == 3)
            {
                SC.SCSelectMode = 1;
                SC.Text_Tip.gameObject.SetActive(true);
                SC.Text_Tip.text = "选择一个骰子";
                for(int i = 0; i < SC.SelectedDices.Count; i++)
                {
                    SC.SelectedDices[i].toggle.interactable = false;
                }
            }
            //选择技能后发动技能(6为选择技能后再选择员工）
            else if (skill.EffectMode == 4 || skill.EffectMode == 6)
            {
                SC.SCSelectMode = 2;
                SC.Text_Tip.gameObject.SetActive(true);
                SC.Text_Tip.text = "选择一个技能";
                for (int i = 0; i < SC.CurrentSkills.Count; i++)
                {
                    SC.CurrentSkills[i].button.interactable = true;
                }
            }
            //选择两个员工后发动技能
            else if (skill.EffectMode == 5)
            {
                empInfo.GC.CurrentEmpInfo = null;
                empInfo.GC.CurrentEmpInfo2 = null;
                empInfo.GC.SelectMode = 7;
                empInfo.GC.TotalEmpPanel.SetWndState(true);
                empInfo.GC.Text_EmpSelectTip.gameObject.SetActive(true);
                empInfo.GC.Text_EmpSelectTip.text = "选择第一个员工";
            }
            
        }
        else
        {
            if (skill.EffectMode != 6)
            {
                SC.TargetSkill = this;
                SC.Text_CurrentSkillName.text = "确定发动 " + SC.TargetSkill.skill.Name + " 能力？";
                SC.ConfirmPanel.SetActive(true);
            }
            //(选择一个技能后)选择一个员工
            else if (skill.EffectMode == 6)
            {
                SC.TargetSkill = this;
                empInfo.GC.CurrentEmpInfo = null;
                empInfo.GC.SelectMode = 4;
                empInfo.GC.TotalEmpPanel.SetWndState(true);
                empInfo.GC.Text_EmpSelectTip.gameObject.SetActive(true);
                empInfo.GC.Text_EmpSelectTip.text = "选择一个员工";
            }
        }

    }

    public void SkillLock(bool UnLock)
    {
        if(UnLock == false)
        {
            Active = false;
            button.interactable = false;
            ForbidSign.SetActive(true);
        }
        else
        {
            Active = true;
            ForbidSign.SetActive(true);
        }
    }
}
