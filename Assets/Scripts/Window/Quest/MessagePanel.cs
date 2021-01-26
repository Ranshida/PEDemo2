using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessagePanel : WindowRoot
{
    public Transform typeA;
    public Transform typeB;

    string m_Text;
    Action m_OnAccept;
    Action m_OnRefuse;

    protected override void InitSpecific()
    {
        base.InitSpecific();
    }

    /// <summary>
    /// 确定，确定之后接触暂停继续
    /// </summary>
    /// <param name="txt">文案</param>
    public void Init(string txt)
    {
        typeA.gameObject.SetActive(true);
        GameControl.Instance.AskPause(this);

        string[] str = txt.Split('|');
        if (str.Length == 2)
        {
            typeA.transform.Find("Txt_Title").GetComponent<Text>().text = str[0];
            typeA.transform.Find("Txt_Info").GetComponent<Text>().text = str[1];
        }
        else
        {
            typeA.transform.Find("Txt_Info").GetComponent<Text>().text = txt;
        }

        typeA.transform.Find("Btn_Accept").GetComponent<Button>().onClick.RemoveAllListeners();
        typeA.transform.Find("Btn_Accept").GetComponent<Button>().onClick.AddListener(()=>
        {
            GameControl.Instance.RemovePause(this);
            Destroy(gameObject);
        });
    }

    /// <summary>
    /// 确定，确定之后执行方法
    /// </summary>
    /// <param name="txt">文案</param>
    /// <param name="onAccept">确定后执行</param>
    public void Init(string txt, Action onAccept)
    {
        typeA.gameObject.SetActive(true);
        GameControl.Instance.AskPause(this);

        string[] str = txt.Split('|');
        if (str.Length == 2)
        {
            typeA.transform.Find("Txt_Title").GetComponent<Text>().text = str[0];
            typeA.transform.Find("Txt_Info").GetComponent<Text>().text = str[1];
        }
        else
        {
            typeA.transform.Find("Txt_Info").GetComponent<Text>().text = txt;
        }

        typeA.transform.Find("Btn_Accept").GetComponent<Button>().onClick.RemoveAllListeners();
        typeA.transform.Find("Btn_Accept").GetComponent<Button>().onClick.AddListener(() =>
        {
            onAccept();
            GameControl.Instance.RemovePause(this);
            Destroy(gameObject);
        });
    }

    /// <summary>
    /// 接受或拒绝
    /// </summary>
    /// <param name="txt">文案</param>
    /// <param name="onAccept">接受后执行</param>
    /// <param name="onRefuse">拒绝后执行</param>
    public void Init(string txt, Action onAccept, Action onRefuse)
    {
        typeB.gameObject.SetActive(true);
        GameControl.Instance.AskPause(this);

        string[] str = txt.Split('|');
        if (str.Length == 2)
        {
            typeB.transform.Find("Txt_Title").GetComponent<Text>().text = str[0];
            typeB.transform.Find("Txt_Info").GetComponent<Text>().text = str[1];
        }
        else
        {
            typeB.transform.Find("Txt_Info").GetComponent<Text>().text = txt;
        }

        typeB.transform.Find("Btn_Accept").GetComponent<Button>().onClick.RemoveAllListeners();
        typeB.transform.Find("Btn_Refuse").GetComponent<Button>().onClick.RemoveAllListeners();
        typeB.transform.Find("Btn_Accept").GetComponent<Button>().onClick.AddListener(() =>
        {
            onAccept();
            GameControl.Instance.RemovePause(this);
            Destroy(gameObject);
        });
        
        typeB.transform.Find("Btn_Refuse").GetComponent<Button>().onClick.AddListener(() =>
        {
            onRefuse();
            GameControl.Instance.RemovePause(this);
            Destroy(gameObject);
        });
    }
}
