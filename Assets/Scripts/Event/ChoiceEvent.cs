using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceEvent : MonoBehaviour
{
    public int TotalCorrection = 0; //当前提供的总修正

    public Text Text_EventName, Text_Correction, Text_EventResult, Text_EventDescrition, Text_Condition;
    public Transform OptionContent;
    public OptionCardInfo OptionPrefab;
    public EventControl EC;
    public EventGroupInfo EGI;
    public Event CurrentEvent;
    public Employee Self;//自身和目标员工

    public List<OptionCardInfo> Options = new List<OptionCardInfo>();

    public void CheckCorrectionUI()
    {
        Text_Correction.text = "当前修正:" + TotalCorrection + "\n";
        foreach(OptionCardInfo option in Options)
        {
            if (option.Selected == true && option.OC.Correction != 0)
            {
                if (option.OC.Correction > 0)
                    Text_Correction.text += option.OC.Name + ":  +" + option.OC.Correction + "修正\n";
                else
                    Text_Correction.text += option.OC.Name + ":  " + option.OC.Correction + "修正\n";
            }
        }

        Text_Condition.text = "初始成功条件:\n1D20 > " + CurrentEvent.FailLimitValue + "\n当前成功条件:\n1D20 > " 
            + (CurrentEvent.FailLimitValue - TotalCorrection);
    }

    //完成抉择
    public void ConfirmChoice()
    {
        EC.UnfinishedEvents.Remove(this);
        foreach (OptionCardInfo option in Options)
        {
            if (option.Selected == true)
                option.OC.StartEffect(Self);
        }
        if (EGI == null)
        {
            CurrentEvent.StartEvent(Self, TotalCorrection);
            EC.ChoiceEventCheck(false);
        }
        else
        {
            CurrentEvent.StartEvent(Self, TotalCorrection, null, EGI);
            EGI.FinishStage += 1;
            EGI.NextStage();
            EC.ChoiceEventCheck(true);
        }
        Destroy(this.gameObject);
    }

    public void SetEventInfo(Event e, Employee emp, EventGroupInfo info = null)
    {
        int ContentIndex = Random.Range(1, e.DescriptionCount + 1);
        CurrentEvent = e;
        EGI = info;
        Self = emp;
        if (info != null)
        {
            Text_EventName.text = e.SubEventNames[info.Stage - 1] + "\n所属事件组:" + e.EventName;
            Text_EventResult.text = "失败效果:" + e.ResultDescription(emp, null, info.Stage);
        }
        else
        {
            Text_EventName.text = e.EventName;
            if (e.SuccessDescription != null)
                Text_EventResult.text = "成功效果:" + e.SuccessDescription;
            else
                Text_EventResult.text = "失败效果:" + e.FailDescription;
        }
        CheckSpecialCorrection();
        CreateOptions();
    }
    void CreateOptions()
    {
        for (int i = 0; i < 3; i++)
        {
            OptionCardInfo option = Instantiate(OptionPrefab, OptionContent);
            Options.Add(option);
            int num = Random.Range(0, EC.GC.OCL.CurrentOptions.Count);
            option.SetInfo(EC.GC.OCL.CurrentOptions[num].OC, Self, this);
            if (option.OC.DebuffCard == true)
            {
                //option.Text_Emp.gameObject.SetActive(true);
                //option.Text_Emp.text = 
            }
        }
    }
    //检查部门和高管提供的修正
    void CheckSpecialCorrection()
    {


        CheckCorrectionUI();
    }
}
