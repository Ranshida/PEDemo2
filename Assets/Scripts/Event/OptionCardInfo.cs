﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionCardInfo : MonoBehaviour
{
    private int SelectType = 0;//确认选择（按钮）类型
    public bool Selected = false;//(在抉择事件面板)是否被选中

    public Outline outline;
    public OptionCardLibrary OCL;
    public OptionCard OC;
    public Employee Emp;
    public EventGroup EG;
    public ChoiceEvent CurrentEvent;
    public Perk ProvidePerk;//抉择卡效果中可能提供的随机特质的存储
    public Text Text_Name, Text_Description, Text_Correction, Text_Emp;

    public void SetBaseInfo(OptionCard oc)
    {
        OC = oc;
        Text_Name.text = oc.Name;
        Text_Description.text = oc.Description;
        if (oc.Correction > 0)
        {
            Text_Correction.text = "+" + oc.Correction;
            Text_Correction.transform.parent.gameObject.SetActive(true);
        }
        else if (oc.Correction < 0)
        {
            Text_Correction.text = oc.Correction.ToString();
            Text_Correction.transform.parent.gameObject.SetActive(true);
        }
        if (oc.DebuffCard == true)
            this.gameObject.GetComponent<Image>().color = new Color(0.7f, 0.85f, 1);
    }

    public void SetInfo(OptionCard oc, Employee emp)
    {
        SetBaseInfo(oc);
        Emp = emp;
        Text_Emp.text = "持有人:" + emp.Name;
        Text_Emp.gameObject.SetActive(true);
        emp.InfoDetail.OptionCards.Add(this);
        SelectType = 1;
    }

    public void SetInfo(OptionCard oc, Employee emp, ChoiceEvent Cevent)
    {
        SetBaseInfo(oc);
        Emp = emp;
        CurrentEvent = Cevent;
        SelectType = 2;
    }

    public void SelectOptionCard()
    {
        //抉择卡库界面信息展示
        if(SelectType == 1 && Emp != null)
        {
            OCL.ShowEmpOptions(Emp);
        }
        //抉择事件选择面板进行选择
        else if (SelectType == 2)
        {
            //正面事件组只能用正面卡，负面事件组只能用负面卡
            if (CurrentEvent.EGI != null && CurrentEvent.EGI.TargetEventGroup.DebuffEvent != OC.DebuffCard)
            {
                GameControl.Instance.CreateMessage("抉择卡类型和事件组类型不符");
                return;
            }
            if (Selected == true)
            {
                Selected = false;
                outline.enabled = false;
                CurrentEvent.TotalCorrection -= OC.Correction;
            }
            else
            {
                Selected = true;
                outline.enabled = true;
                CurrentEvent.TotalCorrection += OC.Correction;
            }
            CurrentEvent.CheckCorrectionUI();
        }
    }

    //随机一个负面特质
    public void RandomPerk()
    {
        int num = Random.Range(0, PerkData.DebuffPerkList.Count);
        ProvidePerk = PerkData.DebuffPerkList[num].Clone();
        Text_Emp.text = "获得特质:" + ProvidePerk.Name + "\n特质效果:" + ProvidePerk.Description;
        Text_Emp.gameObject.SetActive(true);
    }

    //为目标提供负面特质
    public void TargetAddPerk(Employee emp)
    {
        if (ProvidePerk == null || emp == null)
            return;

        emp.InfoDetail.AddPerk(ProvidePerk);
    }
}
