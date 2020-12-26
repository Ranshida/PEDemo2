using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventBulle : MonoBehaviour
{
    Event m_TempEvent;
    float m_Timer;

    public void Init(Event thisEvent)
    {
        m_TempEvent = thisEvent;

        TextMesh result = transform.Find("Txt_Result").GetComponent<TextMesh>();
        if (thisEvent.result == 1 || thisEvent.result == 2) 
        {
            result.text = "失败";
            result.color = Color.red;
        }
        else
        {
            result.text = "成功";
            result.color = Color.green;
        }

        m_Timer = 0;
    }

    private void Update()
    {
        m_Timer += Time.deltaTime;
        if (m_Timer >= 10) 
        {
            Destroy(this.gameObject);
        }
    }

    public void OnRightClick()
    {
        Destroy(this.gameObject);
    }

    public void OnLeftClick()
    {
        EmpManager.Instance.ShowEventDetail(m_TempEvent);
        Destroy(this.gameObject);
    }
}
