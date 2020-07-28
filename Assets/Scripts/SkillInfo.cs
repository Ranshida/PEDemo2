using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillInfo : MonoBehaviour
{
    public int InfoType;

    public Skill skill;
    public SkillControl SC;
    public EmpInfo empInfo;
    public InfoPanel info;

    public Text Text_Name, Text_EmpName, Text_StaminaCost, Text_DiceCost;
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
            Text_StaminaCost.text = (skill.StaminaCost - skill.StaminaExtra).ToString();
            Text_DiceCost.text = (skill.DiceCost - skill.DiceExtra).ToString();
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
