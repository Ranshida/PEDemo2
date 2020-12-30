using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventInfo : MonoBehaviour
{
    public Text Text_MainDescribe, Text_EmpName1, Text_EmpName2, Text_Option1, Text_Option2, Text_Option3, Text_Option4, Text_EventName, Text_TimeLeft;
    public Event CurrentEvent;
    public EventControl EC;
    public GameObject SelectPanel;
    public Button SolveButton;

    public List<Button> Buttons = new List<Button>();

    bool EventFinish = false;

    public void OpenEmpDetail(int num)
    {
        if (num == 1)
            CurrentEvent.Self.InfoDetail.ShowPanel();
        else
            CurrentEvent.Target.InfoDetail.ShowPanel();
    }

    public void Select(int num)
    {
        Text_MainDescribe.text = CurrentEvent.ConfirmEventSelect(num);
        for(int i = 0; i < Buttons.Count; i++)
        {
            Buttons[i].gameObject.SetActive(false);
        }
        EventFinish = true;
        SolveButton.interactable = false;
    }
    //确定文字描述（可能后期解决不同选项的事件时需要进行一些改动）
    public void SetContent()
    {
        Text_EmpName1.text = CurrentEvent.Self.Name;
        if (CurrentEvent.Target != null)
            Text_EmpName2.text = CurrentEvent.Target.Name;
        else
            Text_EmpName2.transform.parent.gameObject.SetActive(false);
        Text_MainDescribe.text = CurrentEvent.SetSolvableEventText(0);
        Text_Option1.text = CurrentEvent.SetSolvableEventText(1);
        Text_Option2.text = CurrentEvent.SetSolvableEventText(2);
        Text_Option3.text = CurrentEvent.SetSolvableEventText(3);
        Text_Option4.text = CurrentEvent.SetSolvableEventText(4);
        EC.GC.AskPause(this);
    }

    public void CloseOptionPanel()
    {
        if (EventFinish == true)
            RemoveEvent();
        else
        {
            SelectPanel.SetActive(false);
        }
        EC.GC.RemovePause(this);
    }

    //后期可能需要从员工身上移除事件
    public void RemoveEvent()
    {
        EC.GC.RemovePause(this);
        EC.GC.HourEvent.RemoveListener(TimePass);
        EC.EventInfos.Remove(this);
        Destroy(SelectPanel);
        Destroy(this.gameObject);
    }

    public void TimePass()
    {
        CurrentEvent.TimeLeft -= 1;
        Text_TimeLeft.text = "剩余时间:" + CurrentEvent.TimeLeft + "/时";
        if (CurrentEvent.TimeLeft <= 0)
            RemoveEvent();
    }
}
