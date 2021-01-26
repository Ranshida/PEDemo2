using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 事件细节
/// </summary>
public class EventDetailWindow : WindowRoot
{
    protected override void InitSpecific()
    {
        base.InitSpecific();
    }
    public void AddEvent(Event thisEvent)
    {
        if (thisEvent.result == 1 || thisEvent.result == 2)
        {  //失败  
            transform.Find("Txt_Success").gameObject.SetActive(false);
            transform.Find("Txt_Failure").gameObject.SetActive(true);
        }
        else
        {  //成功
            transform.Find("Txt_Success").gameObject.SetActive(true);
            transform.Find("Txt_Failure").gameObject.SetActive(false);
        }

        transform.Find("Txt_EventName").GetComponent<Text>().text = thisEvent.EventName;
        if (thisEvent.HaveTarget)
            transform.Find("Txt_Entity").GetComponent<Text>().text = thisEvent.SelfEntity.EmpName + "与" + thisEvent.TargetEntity.EmpName;
        else
            transform.Find("Txt_Entity").GetComponent<Text>().text = thisEvent.SelfEntity.EmpName;
    }

    protected override void OnButton(string btnName)
    {
        switch (btnName)
        {
            case "Btn_Close":
                SetWndState(false);
                break;
            default:
                break;
        }
    }
}
