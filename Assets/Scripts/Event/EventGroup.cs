using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventGroup : Event
{
    public int ExtraStage = 0;//事件组生成后待机(不产生效果)的回合数
    public int StageCount = 6;//事件组总阶段数
    public int BSBossLevel = 2;//头脑风暴boss等级
    public int MoneyRequest = 0;//资源消耗选项-金钱消耗
    public List<int> ItemTypeRequest = new List<int>();//物品类型需求
    public List<int> ItemValueRequest = new List<int>();//物品数量需求

    //特别小组修正相关
    public float ST_BaseRate = 0.3f;//基础成功率
    public float ST_EmpRate = 0.1f;//每增加一名员工提供的额外成功率

    public float ST_SkillRate = 0.1f;//技能需求额外成功率
    public int ST_SkillCount = 3;//技能需求阈值
    public int ST_SkillType = 1;//1决策 2管理 3坚韧

    public float ST_ProfessionRate = 0.2f;//岗位优势额外成功率
    public int ST_ProfessionCount = 3;//岗位优势需求人数
    public ProfessionType ST_ProfessionType = ProfessionType.工程学;//岗位优势需求类型


    public EventGroup() : base()
    {
        isEventGroup = true;
    }

    public void UseResource(EventGroupInfo egi)
    {
        //先检测金钱
        if (MoneyRequest > 0)
        {
            if (egi.EC.GC.Money >= MoneyRequest)
                egi.EC.GC.Money -= MoneyRequest;
            else
                return;
        }
        //再检测是否有足够物品
        if (ItemTypeRequest.Count > 0)
        {
            for(int i = 0; i < ItemTypeRequest.Count; i++)
            {
                int count = 0;
                foreach(CompanyItem CItem in egi.EC.GC.Items)
                {
                    if (CItem.item.Num == ItemTypeRequest[i])
                        count += 1;
                }
                if (count < ItemValueRequest[i])
                    return;
            }
        }
        //都满足则生效
        egi.ResolveStage(1);
        egi.UseResourceButton.interactable = false;
    }

    public override void StartEvent(Employee emp, int ExtraCorrection = 0, Employee target = null, EventGroupInfo egi = null)
    {
        //如果判定成功就不继续
        if (FindResult(emp, ExtraCorrection, target) == 1)
        {
            egi.StageMarker[egi.Stage - 1].color = Color.red;
            QuestControl.Instance.Init("判定失败," + ResultDescription(emp, target, egi.Stage));
            return;
        }

        egi.StageMarker[egi.Stage - 1].color = Color.green;
        if (egi.Stage == 1)
            EffectA(emp, ExtraCorrection, target);
        else if (egi.Stage == 2)
            EffectB(emp, ExtraCorrection, target);
        else if (egi.Stage == 3)
            EffectC(emp, ExtraCorrection, target);
        else if (egi.Stage == 4)
            EffectD(emp, ExtraCorrection, target);
        else if (egi.Stage == 5)
            EffectE(emp, ExtraCorrection, target);
        else if (egi.Stage == 6)
            EffectF(emp, ExtraCorrection, target);

        QuestControl.Instance.Init("判定成功");
    }

    protected virtual void EffectA(Employee emp, int ExtraCorrection = 0, Employee target = null)
    {

    }
    protected virtual void EffectB(Employee emp, int ExtraCorrection = 0, Employee target = null)
    {

    }
    protected virtual void EffectC(Employee emp, int ExtraCorrection = 0, Employee target = null)
    {

    }
    protected virtual void EffectD(Employee emp, int ExtraCorrection = 0, Employee target = null)
    {

    }
    protected virtual void EffectE(Employee emp, int ExtraCorrection = 0, Employee target = null)
    {

    }
    protected virtual void EffectF(Employee emp, int ExtraCorrection = 0, Employee target = null)
    {

    }
}

public class EventGroup1 : EventGroup
{
    public EventGroup1() : base()
    {
        EventName = "冷酷管理";
        StageCount = 6;
        ExtraStage = 2;
        SubEventNames[0] = "晕船风波";
        SubEventNames[1] = "异常电波";
        SubEventNames[2] = "虐待传闻";
        SubEventNames[3] = "要求解释";
        SubEventNames[4] = "仙人掌中毒";
        SubEventNames[5] = "蹭网风波";
        SingleResult = false;

        ST_BaseRate = 0.3f;//基础成功率
        ST_EmpRate = 0.05f;//每增加一名员工提供的额外成功率

        ST_SkillRate = 0.1f;//技能需求额外成功率
        ST_SkillCount = 5;//技能需求阈值
        ST_SkillType = 1;//1决策 2管理 3坚韧

        ST_ProfessionRate = 0.2f;//岗位优势额外成功率
        ST_ProfessionCount = 3;//岗位优势需求人数
        ST_ProfessionType = ProfessionType.工程学;//岗位优势需求类型

        MoneyRequest = 50;
    }
    public override string EventDescription(Employee Emp, Employee targetEmp, int index)
    {
        string content = "";
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = TargetName + "因为长期的颠簸晕船了，联想到公司内网中的传闻，公司中的员工纷纷将矛头指向CEO频繁更换办公地点";
        else if (index == 2)
            content = TargetName + "在公司论坛中跟帖称自己总是在工作中收到异常的信号，电脑上总是出现奇怪的英文字符，尤其是在自己睡着之后最为明显（实际上是上班睡觉压键盘上了）";
        else if (index == 3)
            content = TargetName + "被其他员工指控冷暴力同事";
        else if (index == 4)
            content = TargetName + "要求CEO对最近公司中频繁出现的控诉做出解释";
        else if (index == 5)
            content = TargetName + "员工在论坛中控诉公司中午餐只有仙人掌和仙人掌泥制品是免费的，认为公司缺乏人文关怀";
        else if (index == 6)
            content = TargetName + "员工在论坛中发帖称CEO频繁移动公司所在位置是因为上一个地方的网改密码了";

        return content;
    }

    public override string ResultDescription(Employee Emp, Employee targetEmp, int index)
    {
        string content = "";
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = "员工所在事业部所有员工获得“反感”×2";
        else if (index == 2)
            content = "员工所在事业部所有员工获得“反感”×2";
        else if (index == 3)
            content = TargetName + "与所有员工好感度下降30点";
        else if (index == 4)
            content = "事业部获得状态“怀疑”×5，每个“怀疑”导致信念下降10点，持续6回合";
        else if (index == 5)
            content = "事业部获得状态“怀疑”×5，每个“怀疑”导致信念下降10点，持续6回合";
        else if (index == 6)
            content = "事业部获得状态“怀疑”×5，每个“怀疑”导致信念下降10点，持续6回合";

        return content;
    }
}
