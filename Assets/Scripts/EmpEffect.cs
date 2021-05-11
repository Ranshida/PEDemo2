using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EmpEffect : MonoBehaviour
{
    public int EmpIndex;

    public Image Marker1, Marker2;
    public Text Text_Effect, Text_Debuff;
    public InfoPanelTrigger iptEffect, iptDebuff;
    public DepControl dep;
    public GameObject EmpTip, ReplaceButton, ReplaceButton2;

    //根据文本分割确定显示内容
    public void InitEffect(string content)
    {
        string[] str = content.Split('|');
        if (str.Length == 2)
        {
            Text_Effect.text = str[0];
            iptEffect.enabled = true;
            iptEffect.ContentB = str[1];
        }
        else
        {
            Text_Effect.text = content;
            iptEffect.enabled = false;
        }
    }
    public void InitDebuff(string content)
    {
        string[] str = content.Split('|');
        if (str.Length == 2)
        {
            Text_Debuff.text = str[0];
            iptDebuff.enabled = true;
            iptDebuff.ContentB = str[1];
        }
        else
        {
            Text_Debuff.text = content;
            iptDebuff.enabled = false;
        }
    }

    //替换员工(如果有的话)
    public void ReplaceEmp(int num)
    {

    }

    //显示员工详细信息(如果有的话)
    public void SelectEmp(int num)
    {
        if (num == 1)
        {
            if (dep.CurrentEmps.Count > EmpIndex && dep.CurrentEmps[EmpIndex] != null)
            {
                dep.CurrentEmps[EmpIndex].InfoDetail.ShowPanel();
                EmpTip.SetActive(false);
            }
        }
        else
        {
            if (dep.CurrentEmps.Count > EmpIndex + 1 && dep.CurrentEmps[EmpIndex + 1] != null)
            {
                dep.CurrentEmps[EmpIndex + 1].InfoDetail.ShowPanel();
                EmpTip.SetActive(false);
            }
        }
    }

    //显示/隐藏按钮提示
    public void ShowTip(int num)
    {
        if (num == 1)
        {
            if (dep.CurrentEmps.Count > EmpIndex && dep.CurrentEmps[EmpIndex] != null)
            {
                EmpTip.transform.position = Marker1.transform.position;
                EmpTip.SetActive(true);
            }
        }
        else
        {
            if (dep.CurrentEmps.Count > EmpIndex + 1 && dep.CurrentEmps[EmpIndex + 1] != null)
            {
                EmpTip.transform.position = Marker2.transform.position;
                EmpTip.SetActive(true);
            }
        }
    }
    public void HideTip()
    {
        EmpTip.SetActive(false);
    }

    //商店部门购买面板中隐藏各种选项
    public void HideOptions()
    {
        ReplaceButton.SetActive(false);
        if (ReplaceButton2 != null)
            ReplaceButton2.SetActive(false);
        Marker1.gameObject.GetComponent<Button>().enabled = false;
        Marker1.gameObject.GetComponent<EventTrigger>().enabled = false;
        if (Marker2 != null)
        {
            Marker2.gameObject.GetComponent<Button>().enabled = false;
            Marker2.gameObject.GetComponent<EventTrigger>().enabled = false;
        }
    }
}
