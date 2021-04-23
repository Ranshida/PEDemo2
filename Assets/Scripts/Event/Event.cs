using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//事件状态,存在Employee上
public enum EventCondition
{
    烦恼, 顺利, 悔恨, 困惑, 成就感, 深刻交谈, 寻求安慰, 认可交谈, 分享日常, 分享乐事, 无
}
//基类
public abstract class Event
{
    protected int FriendRequire = -3; //-3无需求 -2仇人 -1陌路 0陌生人 1朋友 2挚友 此处是指目标相对自身的关系(如目标是我的师傅, 下同)
    protected int MasterRequire = -1; //-1无需求 0无师徒关系 1徒弟 2师傅
    protected int LoveRequire = -1;   //-1无需求 0无恋爱关系 1倾慕 2追求 3情侣 4伴侣
    protected int LevelRequire = -1;  //-1无需求 0无等级关系 1同事 2上级 3下属
    protected int FailLimitValue = 10;//事件失败判定的阈值
    protected bool SingleResult = false;
    protected string EventName;//事件名称

    protected string[] SelfDescriptions;//个人历史描述
    protected string[] TargetDescriptions;//目标历史描述
    protected string[] EventDescriptions;//事件描述
    protected string[] OptionDescriptions;//选项描述

    protected EventCondition RequiredCondition; //事件状态需求


    public virtual bool ConditionCheck(Employee CheckEmp, Employee TargetEmp = null)
    {
        //事件状态检测
        if (EventConditionCheck(CheckEmp) == false)
            return false;

        //目标关系检测
        if (RelationCheck(CheckEmp, TargetEmp) == false)
            return false;

        return true;
    }

    //事件状态检测
    protected virtual bool EventConditionCheck(Employee emp)
    {
        if (RequiredCondition == EventCondition.无)
            return true;
        else
        {
            foreach (EventCondition c in emp.EventConditions)
            {
                if (c == RequiredCondition)
                    return true;
            }
        }
        return false;
    }

    //关系检测
    protected virtual bool RelationCheck(Employee emp, Employee target)
    {
        //无任何需求时
        if (target == null)
        {
            if (FriendRequire == -3 && MasterRequire == 0 && LoveRequire == 0 && LevelRequire == 0)
                return true;
            else
                return false;
        }
        Relation r = emp.FindRelation(target);
        if (FriendRequire != -3 && r.FriendValue != FriendRequire)
            return false;
        else if (MasterRequire != -1 && r.MasterValue != MasterRequire)
            return false;
        else if (LoveRequire != -1 && r.LoveValue != LoveRequire)
            return false;
        else if (LevelRequire != -1)
        {
            //不能有上下级关系时
            if (LevelRequire == 0)
            {
                if (emp.CurrentDep != null)
                {
                    //作为员工时目标不能是自己的同事或上级
                    if (emp.CurrentDep.CurrentEmps.Contains(target) || emp.CurrentDep.CurrentDivision.Manager == target)
                        return false;
                }
                else if (emp.CurrentDivision != null)
                {
                    //作为上级是目标不能是自己的下属
                    if (target.CurrentDep != null && emp.CurrentDivision.CurrentDeps.Contains(target.CurrentDep))
                        return false;
                }
                else
                {
                    //待命时,目标同样不能待命(算作自己的同事)
                    if (target.CurrentDep == null && target.CurrentDivision == null)
                        return false;
                }
            }
            //要求目标是自己的同事
            else if (LevelRequire == 1)
            {
                //所属不同就不算同事
                if (emp.CurrentDep != null)
                {
                    if (emp.CurrentDep != target.CurrentDep)
                        return false;
                }
                //待命时目标不待命就不算同事
                else if (emp.CurrentDivision == null)
                {
                    if (target.CurrentDivision != null || target.CurrentDep != null)
                        return false;
                }
                //作为高管时不会有同事
                else
                    return false;
            }
            //要求目标是自己的上级时
            else if (LevelRequire == 2)
            {
                //无部门或自己是高管时不会有上级
                if (emp.CurrentDep == null || emp.CurrentDivision != null)
                    return false;
                //自己的上级不是目标时
                if (emp.CurrentDep != null && emp.CurrentDep.CurrentDivision.Manager != target)
                    return false;
            }
            //要求目标是自己的下属时
            else if (LevelRequire == 3)
            {
                if (emp.CurrentDivision == null)
                    return false;
                else if (target.CurrentDep == null)
                    return false;
                else if (target.CurrentDep.CurrentDivision != emp.CurrentDivision)
                    return false;
            }
        }

        return true;
    }

    //计算结果
    protected virtual int FindResult(Employee emp, int ExtraCorrection = 0, Employee target = null)
    {
        //0失败 1成功
        int result = 0;

        //单结果事件直接成功
        if (SingleResult == true)
            return 1;
        return result;
    }

    //成功效果
    protected virtual void SuccessResult(Employee emp, Employee target = null)
    {

    }
    //失败效果
    protected virtual void FailResult(Employee emp, Employee target = null)
    {

    }

    //随机一个事件文案并添加到对应位置
    protected virtual void RandomDescription(Employee emp, Employee target = null)
    {

    }

    /// <summary>
    /// 事件效果结算
    /// </summary>
    /// <param name="emp">员工自身</param>
    /// <param name="ExtraCorrection">额外点数修正</param>
    /// <param name="target">目标员工</param>
    /// <param name="Stage">事件组执行阶段</param>
    public virtual void StartEvent(Employee emp, int ExtraCorrection = 0, Employee target = null, int Stage = 0)
    {
        if (FindResult(emp, ExtraCorrection, target) == 1)
            SuccessResult(emp, target);
        else
            FailResult(emp, target);
    }
}

public static class EventData
{
    //公司日常事件序列(无状态条件事件)
    public static List<Event> CompanyRoutineEventA = new List<Event>()
    {

    };

    //公司日常事件序列(状态条件事件)
    public static List<Event> CompanyRoutineEventB = new List<Event>()
    {

    };

    //公司一般事件序列
    public static List<Event> CompanyNormalEvent = new List<Event>()
    {

    };

    //个人港口事件序列
    public static List<Event> EmpPortEvent = new List<Event>()
    {

    };

    //个人事件序列
    public static List<Event> EmpPersonalEvent = new List<Event>()
    {

    };

    //特殊事件组
    public static List<EventGroup> SpecialEventGroups = new List<EventGroup>()
    {

    };
}