using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestWindow : WindowRoot
{
    private QuestControl _Manage;
    Button detailBtn;
    string m_Title;
    string m_Info;
    string m_Condition;

    protected override void OnActive()
    {
        _Manage = QuestControl.Instance;
    }

    /// <summary>
    /// 任务引导面板
    /// </summary>
    /// <param name="title"></param>
    /// <param name="info"></param>
    /// <param name="condition"></param>
    public void ShowQuestPanel(string title, string info, string condition)
    {
        m_Title = title;
        m_Info = info;
        m_Condition = condition;

        transform.Find("Txt_Title").GetComponent<Text>().text = m_Title;
        transform.Find("Txt_Condition").GetComponent<Text>().text = m_Condition;
        detailBtn = transform.Find("Btn_Detail").GetComponent<Button>();

        _Manage.Init(m_Title + "|" + m_Info, () => { detailBtn.interactable = true; });
        detailBtn.interactable = false;

        detailBtn.onClick.RemoveAllListeners();
        detailBtn.onClick.AddListener(() =>
        {
            _Manage.Init(m_Title + "|" + m_Info, () => { detailBtn.interactable = true; });
            detailBtn.interactable = false;
        });
    }

    protected override void OnButton(string btnName)
    {
        switch (btnName)
        {
            case "Btn_Detail":
                _Manage.Init(m_Title + "|" + m_Info, () => { detailBtn.interactable = true; });
                detailBtn.interactable = false;
                break;
            default:
                break;
        }
    }
}
