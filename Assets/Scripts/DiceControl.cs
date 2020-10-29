using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceControl : MonoBehaviour
{
    public Text Text_Value;
    public Toggle toggle;
    public SkillControl SC;

    public int value;

    private void Start()
    {
        toggle = this.GetComponent<Toggle>();
    }

    public void RandomValue()
    {
        value = Random.Range(1, 7);
        Text_Value.text = value.ToString();
    }

    public void SetValue(int num)
    {
        value = num;
        Text_Value.text = value.ToString();
    }

    public void ToggleDice(bool check)
    {
        if (SC.SCSelectMode != 1)
        {
            if (check == false)
            {
                if (SC.SelectedDices.Contains(this))
                {
                    SC.SelectedDices.Remove(this);
                    SC.TotalValue -= value;
                }
            }
            else
            {
                SC.TotalValue += value;
                SC.SelectedDices.Add(this);
            }
            SC.SkillCheck();
        }
        else
        {
            if (SC.CurrentSkill.skill.Name == "精打细算")
            {
                if (value >= 3)
                {
                    SC.TargetDice = this;
                    SC.ConfirmPanel.SetActive(true);
                }
            }
            else
            {
                SC.TargetDice = this;
                SC.ConfirmPanel.SetActive(true);
            }
            if (check == true)
                toggle.isOn = false;
        }
    }
}
