using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 矛盾事件弹窗面板
/// </summary>
public class JudgeEventWindow : WindowRoot
{
    private EmpManager _Manage;
    Event m_ThisEvent;
    Transform acceptBtn;
    Transform cantAccept;

    protected override void OnActive()
    {
        _Manage = EmpManager.Instance;

        acceptBtn = transform.Find("Btn_Accept");
        cantAccept = transform.Find("Txt_CantAccept");
    }
    public void AddEvent(Event currentEvent, bool canAccept)
    {
        m_ThisEvent = currentEvent;
        transform.Find("Txt_Title").GetComponent<Text>().text = currentEvent.EventName;
        transform.Find("Txt_Description").GetComponent<Text>().text = currentEvent.Self.Name + "发生了事件" + currentEvent.EventName;

        if (canAccept)
        {  //接受
            acceptBtn.gameObject.SetActive(true);
            cantAccept.gameObject.SetActive(false);
        }
        else
        {
            //无法接受
            acceptBtn.gameObject.SetActive(false);
            cantAccept.gameObject.SetActive(true);
        }
    }

    protected override void OnButton(string btnName)
    {
        switch (btnName)
        {
            case "Btn_Accept":
                _Manage.AcceptJudgeEvent(m_ThisEvent);
                break;
            case "Btn_Refuse":
                _Manage.RefuseJudgeEvent(m_ThisEvent);
                break;
            default:
                break;
        }
    }
}
