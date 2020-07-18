using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceControl : MonoBehaviour
{
    public Text Text_Value;
    public SkillControl SC;

    int value;

    public void RandomValue()
    {
        value = Random.Range(1, 7);
        Text_Value.text = value.ToString();
    }

    public void ToggleDice(bool check)
    {
        if(check == false)
        {
            SC.TotalValue -= value;
            if (SC.SelectedDices.Contains(this))
                SC.SelectedDices.Remove(this);
        }
        else
        {
            SC.TotalValue += value;
            SC.SelectedDices.Add(this);
        }
        SC.SkillCheck();
    }
}
