using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventGroupInfo : MonoBehaviour
{
    public int RandomEventNum = 0;//当前随机到的事件编号
    public int Stage = 1;
    public int STEmpIndex = 0;
    public int STTime = 0;
    public int FinishStage = 1; //已完成的阶段，完成抉择事件会增加，完成特殊指令会增加
    public float STSuccessRate = 0;//特别小组成功率
    public bool SpecialTeamUsed = false, BrainStormUsed = false;//是否已经添加了特别小组修正/是否进行过头脑风暴了
    private int PrepareTurnLeft = 0, STEndTurn;

    public Text Text_GroupName1, Text_GroupName2, Text_GroupInfo1, Text_GroupInfo2, Text_Description, Text_SpecialTeamEffect,
        Text_BSEffect, Text_ResourceEffect, Text_FailResult, Text_PrepareTurn, Text_STRateDetail, Text_STStatus, Text_STRate;
    public EventControl EC;
    public Button SpecialTeamButton, BSButton, UseResourceButton, STOperateButton;
    public GameObject SpecialTeamPanel, STEndPanel;
    public Transform DetailPanel, StagePointer;
    public EventGroup TargetEventGroup;
    public Employee Target;
    public DivisionControl TargetDivision;
    public DepControl TargetDep;

    public Employee[] STMembers = new Employee[3];
    public Button[] STRemoveButtons = new Button[3];
    public Text[] STMemberInfos = new Text[3];
    public Image[] STImages = new Image[3];
    public Image[] Lines = new Image[6];
    public Image[] StageMarker = new Image[6];

    public void SetEvent(EventGroup e)
    {
        RandomEventNum = Random.Range(1, e.StageCount + 1);
        TargetEventGroup = e;
        PrepareTurnLeft = e.ExtraStage;
        Text_GroupName1.text = TargetEventGroup.EventName;
        Text_GroupName2.text = TargetEventGroup.EventName;
        Text_PrepareTurn.text = "准备\n" + e.ExtraStage + "回合";
        Text_SpecialTeamEffect.text = "成功效果:事件组-" + e.ST_TurnCorrection + "回合";
        Text_BSEffect.text = "成功效果:事件组-" + e.BSTurnCorrection + "回合\n难度等级:" + (e.BSBossLevel + ((EC.GC.Turn - 1) / 10));
        Text_ResourceEffect.text = "成功效果:事件组-" + e.ResourceTurnCorretion + "回合\n需求:";
        if (e.MoneyRequest > 0)
            Text_ResourceEffect.text += "金钱 " + e.MoneyRequest + " ";
        if (e.ItemTypeRequest.Count > 0)
        {
            for (int i = 0; i < e.ItemTypeRequest.Count; i++)
            {
                Text_ResourceEffect.text += ItemData.Items[e.ItemTypeRequest[i] - 1].Name + "*" + e.ItemValueRequest[i];
            }
        }
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
            //正面事件组的话，如果下一阶段已完成则直接输出成功结果
            if (Stage <= FinishStage && TargetEventGroup.DebuffEvent == false)
                TargetEventGroup.StartEvent(Target, 1000, null, this);

            NextStage();
            EC.ChoiceEventCheck(true);
        }
        //否则创建相应抉择事件
        else
        {
            //没有目标时重新设定目标
            if (Target == null || TargetDep == null || TargetDivision == null)
            {
                UpdateUI();
                print("抉择事件开始前丢失目标");
            }
            //如果有减心力的buff则寻找目标

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
            //重置目标并随机下个事件类型
            RandomEventNum = Random.Range(1, TargetEventGroup.StageCount + 1);
            Target = null;
            TargetDep = null;
            TargetDivision = null;
        }
        //更新UI，是否执行完毕在EventControl统一判断
        if (Stage <= TargetEventGroup.StageCount)
        {
            StagePointer.position = StageMarker[Stage - 1].transform.position;
            UpdateUI();
        }

        //特别小组和其他修正相关
        //重置按钮
        if (SpecialTeamUsed == true)
        {
            BSButton.interactable = true;
            if (BrainStormUsed == true)
                UseResourceButton.interactable = true;
        }
        STOperateButton.interactable = true;

        //特别小组时间减少
        if (STTime > 0)
        {
            STTime -= 1;
            //3回合事件结束后重置
            if (STTime == 0)
            {
                for(int i = 0; i < 3; i++)
                {
                    STRemoveButtons[i].interactable = true;
                    STRemoveButtons[i].gameObject.SetActive(false);
                    if (STMembers[i] != null)
                        RemoveSTMember(i);
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
            //找目标员工
            List<Employee> PosbEmps = new List<Employee>();
            foreach(Employee e in EC.GC.CurrentEmployees)
            {
                //在心理咨询室中回复的员工无法作为事件目标
                if (e.CurrentDep != null && e.CurrentDep.building.Type == BuildingType.心理咨询室 && EC.GC.PsycholCDep.AffectedEmps.Contains(e))
                    continue;

                if (e.inSpecialTeam == false)
                    PosbEmps.Add(e);
            }
            Target = PosbEmps[Random.Range(0, PosbEmps.Count)];
        }

        if (TargetDivision == null)
        {
            //找目标事业部，目标员工没有所属事业部时，找第一个有部门的事业部
            if (Target.CurrentDep != null && Target.CurrentDep.CurrentDivision != null)
                TargetDivision = Target.CurrentDep.CurrentDivision;
            else if (Target.CurrentDivision != null)
                TargetDivision = Target.CurrentDivision;
            else
            {
                List<DivisionControl> dcList = new List<DivisionControl>();
                foreach (DivisionControl dc in GameControl.Instance.CurrentDivisions)
                {
                    if (dc.CurrentDeps.Count > 0)
                    {
                        dcList.Add(dc);
                    }
                }
                TargetDivision = dcList[Random.Range(0, dcList.Count)];
            }
        }

        if (TargetDep == null)
        {
            if (Target.CurrentDep != null)
                TargetDep = Target.CurrentDep;
            else if (Target.CurrentDivision != null)
            {
                if (Target.CurrentDivision.CurrentDeps.Count > 0)
                    TargetDep = Target.CurrentDivision.CurrentDeps[Random.Range(0, Target.CurrentDivision.CurrentDeps.Count)];
                else
                    TargetDep = EC.GC.CurrentDeps[Random.Range(0, EC.GC.CurrentDeps.Count)];
            }
            else
                TargetDep = EC.GC.CurrentDeps[Random.Range(0, EC.GC.CurrentDeps.Count)];
        }
        Text_Description.text = TargetEventGroup.EventDescription(Target, null, RandomEventNum);
        if (TargetEventGroup.DebuffEvent == true)
            Text_FailResult.text = "事件失败效果:" + TargetEventGroup.ResultDescription(Target, null, RandomEventNum, this);
        else
            Text_FailResult.text = "事件成功效果:" + TargetEventGroup.ResultDescription(Target, null, RandomEventNum, this);

        int TurnCount = 1 + PrepareTurnLeft;
        Text_GroupInfo1.text = "距离下一事件:" + TurnCount + "回合\n下一事件:" + TargetEventGroup.SubEventNames[RandomEventNum - 1];
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

    //开始头脑风暴修正
    public void StartBS()
    {
        //有未处理事件时不能继续
        if (EC.UnfinishedEvents.Count > 0)
            return;
        //不处于调整回合时无法使用
        if (EC.GC.AdjustTurn != 0)
        {
            EC.GC.CreateMessage("未处于调整回合");
            return;
        }
        BSButton.interactable = false;
        EC.GC.BSC.StartEventBossFight(this);
        DetailPanel.GetComponent<WindowBaseControl>().SetWndState(false);
    }

    //使用消耗资源修正
    public void UseResource()
    {
        //不处于调整回合时无法使用
        if (EC.GC.AdjustTurn != 0)
        {
            EC.GC.CreateMessage("未处于调整回合");
            return;
        }
        TargetEventGroup.UseResource(this);
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
        else
        {
            foreach (Employee emp in STMembers)
            {
                if (emp != null)
                {
                    emp.SpecialTeamTime = 0;
                    emp.inSpecialTeam = false;
                }
            }
        }
    }

    //开始派遣/处理事件
    public void STOperate()
    {
        //不处于调整回合时无法使用
        if (EC.GC.AdjustTurn != 0)
        {
            EC.GC.CreateMessage("未处于调整回合");
            return;
        }
        CheckSTStatus();
        if (STTime == 0)
        {
            STTime = 3;
            for(int i = 0; i < 3; i++)
            {
                STRemoveButtons[i].gameObject.SetActive(true);
                STRemoveButtons[i].interactable = false;
            }
            STOperateButton.GetComponentInChildren<Text>().text = "处理事件";
            STEndTurn = EC.GC.Turn + 3;
        }
        STOperateButton.interactable = false;
        if (Random.Range(0.0f, 1.0f) < STSuccessRate)
        {
            if (SpecialTeamUsed == false)
            {
                SpecialTeamUsed = true;
                BSButton.interactable = true;
            }
            ResolveStage(TargetEventGroup.ST_TurnCorrection);
            STEndPanel.SetActive(true);
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
        int MemberCount = 0, ProfessionCount = 0, SkillCount = 0, PerkCount = 0;
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
                foreach(PerkInfo info in emp.InfoDetail.PerksInfo)
                {
                    if (info.CurrentPerk.Num == 130)
                        PerkCount += 1;
                }
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

        if (PerkCount > 0)
        {
            Text_STRateDetail.text += "<color=black>特质“救火队员” +" + (10 * PerkCount) + "%(" + PerkCount + "人)\n</color>";
            STSuccessRate += 0.1f * PerkCount;
        }
        else
        {
            Text_STRateDetail.text += "<color=gray>特质“救火队员” +10%/人\n</color>";

        }
        if (STTime == 0)
            Text_STStatus.text = "被派遣调研的员工直到第" + (EC.GC.Turn + 3) + "回合不能调整岗位\n当前状态:未派遣";
        else
            Text_STStatus.text = "被派遣调研的员工直到第" + STEndTurn + "回合不能调整岗位\n当前状态:已派遣-剩余" + STTime + "回合";

        Text_STRate.text = "成功率 " + (STSuccessRate * 100) + "%";
    }
}
