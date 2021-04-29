using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventGroupInfo : MonoBehaviour
{
    public int Stage = 1;
    public int FinishStage = 0; //已完成的阶段，完成抉择事件会增加，完成特殊指令会增加
    public bool SpecialTeamUsed = false;//是否已经添加了特别小组修正
    private int PrepareTurnLeft = 0;

    public Text Text_GroupName1, Text_GroupName2, Text_GroupInfo1, Text_GroupInfo2, Text_Description, Text_SpecialTeamEffect,
        Text_BSEffect, Text_ResourceEffect, Text_FailResult, Text_PrepareTurn;
    public EventControl EC;
    public Button SpecialTeamButton, BSButton, UseResourceButton;
    public Transform DetailPanel, StagePointer;
    public EventGroup TargetEventGroup;
    public Employee Target;

    public Image[] Lines = new Image[6];
    public Image[] StageMarker = new Image[6];

    public void SetEvent(EventGroup e)
    {
        TargetEventGroup = e;
        PrepareTurnLeft = e.ExtraStage;
        Text_GroupName1.text = TargetEventGroup.EventName;
        Text_GroupName2.text = TargetEventGroup.EventName;
        Text_PrepareTurn.text = "准备\n" + e.ExtraStage + "回合";
        if (e.StageCount < 6)
        {
            for(int i = e.StageCount; i < 6; i++)
            {
                Lines[i].gameObject.SetActive(false);
                StageMarker[i].gameObject.SetActive(false);
            }
        }
        UpdateUI();
    }

    //开始一个子事件
    public void StartGroupEvent()
    {
        //先判断是否处于准备阶段或下一阶段已经完成,是的话直接继续
        if (PrepareTurnLeft > 0 || Stage <= FinishStage)
        {
            NextStage();
            EC.ChoiceEventCheck(true);
            print("aa");
        }
        //否则创建相应抉择事件
        else
        {
            if (Target == null)
                UpdateUI();
            EC.StartChoiceEvent(TargetEventGroup, Target, this);
        }
    }

    //通过特殊手段提前完成特定数量的阶段
    public void ResolveStage(int num)
    {
        for(int i = Stage - 1; i < FinishStage + num; i++)
        {
            if (i < 6)
                StageMarker[i].color = Color.green;
            else
                break;
        }
        FinishStage += num;
    }

    //进入下一个事件阶段(事件流程中最后结算的部分)
    public void NextStage()
    {
        if (PrepareTurnLeft > 0)
            PrepareTurnLeft -= 1;
        else
        {
            Lines[Stage - 1].sprite = null;
            Stage += 1;
        }
        //更新UI，是否执行完毕在EventControl统一判断
        if (Stage <= TargetEventGroup.StageCount)
        {
            StagePointer.position = StageMarker[Stage - 1].transform.position;
            UpdateUI();
        }
        if (SpecialTeamUsed == true)
        {
            BSButton.interactable = true;
            UseResourceButton.interactable = true;
        }
    }

    //检查是否有目标并刷新事件效果
    public void UpdateUI()
    {
        if (Target == null)
            Target = EC.GC.CurrentEmployees[Random.Range(0, EC.GC.CurrentEmployees.Count)];
        Text_Description.text = TargetEventGroup.EventDescription(Target, null, Stage);
        Text_FailResult.text = "事件失败效果:" + TargetEventGroup.ResultDescription(Target, null, Stage);
        int TurnCount = 1 + PrepareTurnLeft;
        Text_GroupInfo1.text = "距离下一事件:" + TurnCount + "回合\n下一事件:" + TargetEventGroup.SubEventNames[Stage - 1];
        Text_GroupInfo2.text = Text_GroupInfo1.text;
    }

    public void StartBS()
    {
        //有未处理事件时不能继续
        if (EC.UnfinishedEvents.Count > 0)
            return;
        BSButton.interactable = false;
        EC.GC.BSC.StartEventBossFight(this);
        DetailPanel.GetComponent<WindowBaseControl>().SetWndState(false);
    }
}
