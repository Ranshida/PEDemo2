using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventGroupInfo : MonoBehaviour
{
    public int Stage = 1;
    public int STEmpIndex = 0;
    public int STTime = 0;
    public int FinishStage = 0; //已完成的阶段，完成抉择事件会增加，完成特殊指令会增加
    public float STSuccessRate = 0;//特别小组成功率
    public bool SpecialTeamUsed = false;//是否已经添加了特别小组修正
    private int PrepareTurnLeft = 0;

    public Text Text_GroupName1, Text_GroupName2, Text_GroupInfo1, Text_GroupInfo2, Text_Description, Text_SpecialTeamEffect,
        Text_BSEffect, Text_ResourceEffect, Text_FailResult, Text_PrepareTurn, Text_STRateDetail, Text_STStatus, Text_STRate;
    public EventControl EC;
    public Button SpecialTeamButton, BSButton, UseResourceButton, STOperateButton;
    public GameObject SpecialTeamPanel;
    public Transform DetailPanel, StagePointer;
    public EventGroup TargetEventGroup;
    public Employee Target;

    public Employee[] STMembers = new Employee[3];
    public Button[] STRemoveButtons = new Button[3];
    public Text[] STMemberInfos = new Text[3];
    public Image[] STImages = new Image[3];
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
            STExtraTime();
            EC.ChoiceEventCheck(true);
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
        STOperateButton.interactable = true;
        if (STTime > 0)
        {
            STTime -= 1;
            if (STTime == 0)
            {
                for(int i = 0; i < 3; i++)
                {
                    STRemoveButtons[i].interactable = true;
                }
                STOperateButton.GetComponentInChildren<Text>().text = "派遣并处理事件";
            }
        }
    }

    //检查是否有目标并刷新事件效果
    public void UpdateUI()
    {
        if (Target == null)
        {
            List<Employee> PosbEmps = new List<Employee>();
            foreach(Employee e in EC.GC.CurrentEmployees)
            {
                if (e.inSpecialTeam == false)
                    PosbEmps.Add(e);
            }
            Target = PosbEmps[Random.Range(0, PosbEmps.Count)];
        }
        Text_Description.text = TargetEventGroup.EventDescription(Target, null, Stage);
        Text_FailResult.text = "事件失败效果:" + TargetEventGroup.ResultDescription(Target, null, Stage);
        int TurnCount = 1 + PrepareTurnLeft;
        Text_GroupInfo1.text = "距离下一事件:" + TurnCount + "回合\n下一事件:" + TargetEventGroup.SubEventNames[Stage - 1];
        Text_GroupInfo2.text = Text_GroupInfo1.text;
    }

    public void ShowDetailPanel()
    {
        foreach(EventGroupInfo info in EC.CurrentEventGroups)
        {
            info.DetailPanel.GetComponent<WindowBaseControl>().SetWndState(false);
        }
        UpdateUI();
        DetailPanel.GetComponent<WindowBaseControl>().SetWndState(true);
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

    //事件组结束后额外的特别小组禁用时间
    public void STExtraTime()
    {
        if (STTime > 0)
        {
            foreach(Employee emp in STMembers)
            {
                if (emp != null)
                    emp.SpecialTeamTime = STTime;
            }
        }
    }

    //开始派遣/处理事件
    public void STOperate()
    {
        CheckSTStatus();
        if (STTime == 0)
        {
            for(int i = 0; i < 3; i++)
            {
                STRemoveButtons[i].interactable = false;
            }
            STOperateButton.GetComponentInChildren<Text>().text = "处理事件";
        }
        STOperateButton.interactable = false;
        if (Random.Range(0.0f, 1.0f) < STSuccessRate)
        {
            if (SpecialTeamUsed == false)
            {
                SpecialTeamUsed = true;
                BSButton.interactable = true;
                UseResourceButton.interactable = true;
            }
            ResolveStage(1);
            QuestControl.Instance.Init("特别小组处理成功");
        }
        else
            QuestControl.Instance.Init("特别小组处理失败");
    }

    //开始选择特别小组成员
    public void SelectSTMember(int index)
    {
        //0-2
        STEmpIndex = index;
        EC.GC.SelectMode = 3;
        EC.GC.Text_EmpSelectTip.text = "从人才储备中选择一个员工";
        foreach(Employee emp in EC.GC.CurrentEmployees)
        {
            if (emp.inSpecialTeam == true || emp.CurrentDep != null || emp.CurrentDivision != null)
                emp.InfoB.gameObject.SetActive(false);
        }
        EC.GC.TotalEmpPanel.SetWndState(true);
        EC.CurrentEventGroup = this;
    }

    //移除特别小组成员
    public void RemoveSTMember(int index)
    {
        STMembers[index].inSpecialTeam = false;
        STMembers[index] = null;
        STImages[index].color = Color.white;
        STRemoveButtons[index].gameObject.SetActive(false);
        CheckSTStatus();
    }

    //设置特别小组成员
    public void SetSTMember(Employee emp)
    {
        emp.inSpecialTeam = true;
        STMembers[STEmpIndex] = emp;
        STImages[STEmpIndex].color = Color.gray;
        STRemoveButtons[STEmpIndex].gameObject.SetActive(true);
        CheckSTStatus();
        STMemberInfos[STEmpIndex].text = "";
        foreach (int num in emp.Professions)
        {
            if (num == (int)TargetEventGroup.ST_ProfessionType)
            {
                STMemberInfos[STEmpIndex].text = TargetEventGroup.ST_ProfessionType + "岗位优势\n";
            }
        }
        EC.GC.TotalEmpPanel.SetWndState(false);
    }

    //检测特别小组状态
    public void CheckSTStatus()
    {
        int MemberCount = 0, ProfessionCount = 0, SkillCount = 0;
        foreach(Employee emp in STMembers)
        {
            if (emp != null)
            {
                MemberCount += 1;
                foreach(int num in emp.Professions)
                {
                    if (num == (int)TargetEventGroup.ST_ProfessionType)
                    {
                        ProfessionCount += 1;                       
                        break;
                    }
                }
                if (TargetEventGroup.ST_SkillType == 1)
                    SkillCount += emp.Decision;
                else if(TargetEventGroup.ST_SkillType == 2)
                    SkillCount += emp.Manage;
                else if (TargetEventGroup.ST_SkillType == 3)
                    SkillCount += emp.Tenacity;
            }
        }
        Text_STRateDetail.text = "<color=black>初始几率 +" + (TargetEventGroup.ST_BaseRate * 100) + "%\n</color>";
        STSuccessRate = TargetEventGroup.ST_BaseRate;

        if (ProfessionCount >= TargetEventGroup.ST_ProfessionCount)
        {
            Text_STRateDetail.text += "<color=black>" + TargetEventGroup.ST_ProfessionCount + "人具有"
                + TargetEventGroup.ST_ProfessionType + "岗位优势 +" + (TargetEventGroup.ST_ProfessionRate * 100)
                + "%(" + ProfessionCount + "/" + TargetEventGroup.ST_ProfessionCount + ")\n</color>";
            STSuccessRate += TargetEventGroup.ST_ProfessionRate;
        }
        else
        {
            Text_STRateDetail.text += "<color=gray>" + TargetEventGroup.ST_ProfessionCount + "人具有"
    + TargetEventGroup.ST_ProfessionType + "岗位优势 +" + (TargetEventGroup.ST_ProfessionRate * 100)
    + "%(" + ProfessionCount + "/" + TargetEventGroup.ST_ProfessionCount + ")\n</color>";
        }

        Text_STRateDetail.text += "<color=black>每增加1人 +" + (TargetEventGroup.ST_EmpRate * 100) + "%(" + 
            (MemberCount * TargetEventGroup.ST_EmpRate * 100) + "%)\n</color>";
        STSuccessRate += MemberCount * TargetEventGroup.ST_EmpRate;

        string SkillContent = "";
        if (TargetEventGroup.ST_SkillType == 1)
            SkillContent = "决策能力总和>";
        else if (TargetEventGroup.ST_SkillType == 2)
            SkillContent = "管理能力总和>";
        else if (TargetEventGroup.ST_SkillType == 3)
            SkillContent = "坚韧能力总和>";
        if (SkillCount > TargetEventGroup.ST_SkillCount)
        {
            Text_STRateDetail.text += "<color=black>" + SkillContent + TargetEventGroup.ST_SkillCount + " +" 
                + (TargetEventGroup.ST_SkillRate * 100) + "%(" + SkillCount + ")\n</color>";
            STSuccessRate += TargetEventGroup.ST_SkillRate;
        }
        else
        {
            Text_STRateDetail.text += "<color=gray>" + SkillContent + TargetEventGroup.ST_SkillCount + " +"
                + (TargetEventGroup.ST_SkillRate * 100) + "%(" + SkillCount + ")\n</color>";
        }

        Text_STStatus.text = "被派遣调研的员工直到第" + (EC.GC.Turn + 3) + "回合不能调整岗位\n";
        if (STTime == 0)
            Text_STStatus.text += "当前状态:未派遣";
        else
            Text_STStatus.text += "当前状态:已派遣-剩余" + STTime + "回合";

        Text_STRate.text = "成功率 " + (STSuccessRate * 100) + "%";
    }
}
