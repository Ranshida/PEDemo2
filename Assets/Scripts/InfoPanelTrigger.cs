using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InfoPanelTrigger : MonoBehaviour
{
    public InfoPanel info;
    public float ShowTime = 0.25f;
    public string ContentA = "", ContentB = "", ContentC = "";

    private Button button;

    float Timer = 0;
    bool ShowPanel;
    bool ExitEventSet = false;//用于检测是否已经对已有的按钮设置过离开事件

    private void Start()
    {
        SetEvent();
    }
    private void OnEnter(BaseEventData pointData)
    {
        PointerEnter();
    }
    private void OnExit(BaseEventData pointData)
    {
        PointerExit();
    }

    void SetEvent()
    {
        button = this.GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(PointerExit);
        
        info = GameControl.Instance.infoPanel;
        EventTrigger tr = this.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();

        // 鼠标进入
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback = new EventTrigger.TriggerEvent();
        entry.callback.AddListener(OnEnter);
        tr.triggers.Add(entry);

        EventTrigger.Entry entry2 = new EventTrigger.Entry();

        // 鼠标离开
        entry2.eventID = EventTriggerType.PointerExit;
        entry2.callback = new EventTrigger.TriggerEvent();
        entry2.callback.AddListener(OnExit);
        tr.triggers.Add(entry2);

        info = GameControl.Instance.infoPanel;
    }

    private void Update()
    {
        if(info == null)
            info = GameControl.Instance.infoPanel;
        if (ShowPanel == true)
        {
            if (Timer < ShowTime)
                Timer += Time.deltaTime;
            else
            {
                info.Text_Name.text = ContentA;
                info.Text_Description.text = ContentB;
                info.Text_ExtraInfo.text = ContentC;

                if (info.Visible == false)
                    info.ShowPanel();
                info.transform.position = Input.mousePosition;
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
}
