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
    protected List<int> LevelRequire = new List<int>();  //-1无需求 0无等级关系 1同事 2上级 3下属
    public int FailLimitValue = 10;//事件失败判定的阈值
    public int DescriptionCount = 1;//总共有几种描述文案
    protected bool SingleResult = false;  //是否为单结果事件(无需判定直接输出SuccessResult)
    protected bool NeedDivision = false; //自身和目标是否必须属于一个事业部
    public string EventName;//事件(组)名称
    public string SuccessDescription, FailDescription;
    protected string SelfName;//员工自身名字
    protected string TargetName;//目标员工名字(员工名字用于事件各种描述中调用)
    protected string PlaceName;//目标发生地点的部门/事业部名字

    public bool isEventGroup = false;//是否为事件组事件

    public string[] SubEventNames = new string[6]; //(事件组)各个子事件的名称

    protected EventCondition RequiredCondition; //事件状态需求


    public virtual bool ConditionCheck(Employee CheckEmp, Employee TargetEmp = null)
    {
        //事件状态检测
        if (EventConditionCheck(CheckEmp) == false)
            return false;

        //目标关系检测
        if (RelationCheck(CheckEmp, TargetEmp) == false)
            return false;

        //事业部存在检测
        if (DivisionConditionCheck(CheckEmp, TargetEmp) == false)
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

    //事业部存在检测
    protected virtual bool DivisionConditionCheck(Employee emp, Employee target)
    {
        if (NeedDivision == false)
            return true;

        if (emp.CurrentDep != null || emp.CurrentDivision != null)
            return true;
        else if (target != null)
        {
            if (target.CurrentDep != null || target.CurrentDivision != null)
                return true;
        }
        return false;
    }

    //关系检测
    protected virtual bool RelationCheck(Employee emp, Employee target)
    {
        //无任何需求时
        if (target == null)
        {
            if (FriendRequire == -3 && MasterRequire == 0 && LoveRequire == 0 && LevelRequire.Count == 0)
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
        else if (LevelRequire.Count > 0)
        {
            bool result = false;
            foreach (int num in LevelRequire)
            {
                //不能有上下级关系时
                if (num == 0)
                {
                    if (emp.CurrentDep != null)
                    {
                        //作为员工时目标不能是自己的同事或上级
                        if (emp.CurrentDep.CurrentEmps.Contains(target) == false && emp.CurrentDep.CurrentDivision.Manager != target)
                            result = true;
                    }
                    else if (emp.CurrentDivision != null)
                    {
                        //作为上级时目标不能是自己的下属
                        if (target.CurrentDep != null && emp.CurrentDivision.CurrentDeps.Contains(target.CurrentDep))
                            result = false;
                        else
                            result = true;
                    }
                    //待命时,目标同样不能待命(算作自己的同事)
                    else if (target.CurrentDep != null || target.CurrentDivision != null)
                        result = true;
                }
                //要求目标是自己的同事
                else if (num == 1)
                {
                    //所属不同就不算同事
                    if (emp.CurrentDep != null)
                    {
                        if (emp.CurrentDep == target.CurrentDep)
                            result = true;
                    }
                    //待命时目标不待命就不算同事
                    else if (emp.CurrentDivision == null)
                    {
                        if (target.CurrentDivision == null && target.CurrentDep == null)
                            result = true;
                    }
                    //作为高管时不会有同事
                    else
                        result = false;
                }
                //要求目标是自己的上级时
                else if (num == 2)
                {
                    if (emp.CurrentDep != null && emp.CurrentDep.CurrentDivision.Manager == target)
                        result = true;
                }
                //要求目标是自己的下属时
                else if (num == 3)
                {
                    if (emp.CurrentDivision != null && target.CurrentDep != null && target.CurrentDep.CurrentDivision == emp.CurrentDivision)
                        result = true;
                }
                if (result == true)
                    return true;
            }
            return false;
        }

        return true;
    }

    //计算结果
    protected virtual int FindResult(Employee emp, int ExtraCorrection = 0, Employee target = null, EventGroupInfo egi = null)
    {
        //0失败 1成功
        int result = 0;

        //单结果事件直接成功
        if (SingleResult == true)
            return 1;

        int posb = Random.Range(1, 21);
        posb = (CalcBonus(emp, target, egi) + ExtraCorrection);
        //这里posb应该加上额外判定的点数

        if (posb > FailLimitValue)
            result = 1;
        else
            result = 2;

        return result;
    }

    //计算额外加成
    protected virtual int CalcBonus(Employee emp, Employee target = null, EventGroupInfo egi = null)
    {
        int result = CalCulture(emp, target);
        return result;
    }

    //检测文化加成
    protected virtual int CalCulture(Employee emp, Employee target)
    {
        int result = 0;
        if (emp == null || target == null)
            return 0;
        return result;
    }

    //检测事业部信念加成
    public virtual int CalcDivisionFaith(Employee emp, Employee target = null, EventGroupInfo egi = null)
    {
        int result = 0;
        DivisionControl TargetDivision = null;
        //找目标事业部
        if (egi != null && egi.TargetDivision)
            TargetDivision = egi.TargetDivision;
        else if (emp.CurrentDep != null)
            TargetDivision = emp.CurrentDep.CurrentDivision;
        else if (emp.CurrentDivision != null)
            TargetDivision = emp.CurrentDivision;
        else if (target != null)
        {
            if (target.CurrentDep != null)
                TargetDivision = target.CurrentDep.CurrentDivision;
            else if (target.CurrentDivision != null)
                TargetDivision = target.CurrentDivision;
        }
        if (TargetDivision == null)
            return 0;
        else
        {
            if (TargetDivision.Faith >= 90)
                result += 4;
            else if (TargetDivision.Faith >= 30)
                result += 2;
            else if (TargetDivision.Faith >= 10)
                result += 1;
            else if (TargetDivision.Faith > -10)
                result += 0;
            else if (TargetDivision.Faith >= -30)
                result -= 1;
            else if (TargetDivision.Faith >= -90)
                result -= 2;
            else
                result -= 4;
        }

        return result;
    }

    //事业部管理加成
    public virtual int CalcDivisionManage(Employee emp, Employee target = null, EventGroupInfo egi = null)
    {
        int result = 0;
        DivisionControl TargetDivision = null;
        //找目标事业部
        if (egi != null && egi.TargetDivision)
            TargetDivision = egi.TargetDivision;
        else if (emp.CurrentDep != null)
            TargetDivision = emp.CurrentDep.CurrentDivision;
        else if (emp.CurrentDivision != null)
            TargetDivision = emp.CurrentDivision;
        else if (target != null)
        {
            if (target.CurrentDep != null)
                TargetDivision = target.CurrentDep.CurrentDivision;
            else if (target.CurrentDivision != null)
                TargetDivision = target.CurrentDivision;
        }
        if (TargetDivision == null)
            return 0;

        if (TargetDivision.Manager != null)
        {
            if (TargetDivision.Manager.Manage == 1)
                result += 1;
            else if (TargetDivision.Manager.Manage == 2)
                result += 2;
            else if (TargetDivision.Manager.Manage == 3)
                result += 3;
            else if (TargetDivision.Manager.Manage == 4)
                result += 5;
            else if (TargetDivision.Manager.Manage == 5)
                result += 7;
        }
        return result;
    }

    //检测特质加成
    protected virtual int CalcPerk(Employee emp)
    {
        int result = 0;
        foreach (PerkInfo perk in emp.InfoDetail.PerksInfo)
        {
            if (perk.CurrentPerk.Num == 16)
            {
                result += 1;
                break;
            }
        }
        return result;
    }

    protected virtual int CalcEmotion(Employee emp)
    {
        int result = 0;
        if (emp == null)
            return 0;
        else if (emp.InfoDetail.MainEmotion.Active == true)
        {
            if (emp.InfoDetail.MainEmotion.E.color == EColor.Yellow)
                result += 3;
            else if (emp.InfoDetail.MainEmotion.E.color == EColor.Green)
                result += 3;
            else if (emp.InfoDetail.MainEmotion.E.color == EColor.Purple)
                result -= 3;
            else if (emp.InfoDetail.MainEmotion.E.color == EColor.Red)
                result -= 3;
        }
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
    public virtual void StartEvent(Employee emp, int ExtraCorrection = 0, Employee target = null, EventGroupInfo egi = null)
    {
        if (FindResult(emp, ExtraCorrection, target) == 1)
            SuccessResult(emp, target);
        else
            FailResult(emp, target);
    }

    public virtual string ResultDescription(Employee Emp, Employee targetEmp, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        return content;
    }

    public virtual string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        return content;
    }

    //对方文案暂时无随机
    public virtual string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        return content;
    }

    public virtual string EventDescription(Employee Emp, Employee targetEmp, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        return content;
    }

    protected virtual void SetNames(Employee Emp, Employee targetEmp)
    {
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;
        if (Emp.CurrentDep != null)
            PlaceName = Emp.CurrentDep.Text_DepName.text;
        else if (targetEmp != null && targetEmp.CurrentDep != null)
            PlaceName = targetEmp.CurrentDep.name;
    }
}


//以下是公司一般事件
public class EventC1 : Event
{
    public EventC1() : base()
    {
        EventName = "烦恼缠身";
        DescriptionCount = 1;
        RequiredCondition = EventCondition.烦恼;
        FailDescription = " 失败，公司内产生不满 x1"; //只设置失败/成功其中之一的描述，另一个没效果的不要定义描述
    }

    protected override int CalcBonus(Employee emp, Employee target = null, EventGroupInfo egi = null)
    {
        int result = CalcEmotion(emp) + CalcDivisionFaith(emp, target, egi) + CalcPerk(emp) + CalcDivisionManage(emp, target, egi);
        return result;
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //随机文案
        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        GameControl.Instance.AddEventProgress(1, true);

        //随机文案
        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = SelfName + "在工作中处处碰壁，感到烦恼缠身。因此发来邮件，希望老板能帮助自己改善工作表现...，但是没有效果";
        }
        else
        {
            if (index == 1)
                content = SelfName + "在工作中处处碰壁，感到烦恼缠身。因此发来邮件，希望老板能帮助自己改善工作表现..." + FailDescription;

        }
        return content;
    }
}


public class EventC2 : Event
{
    public EventC2() : base()
    {
        EventName = "进展喜人";
        DescriptionCount = 1;
        RequiredCondition = EventCondition.顺利;
        SuccessDescription = " 成功，公司内产生认同 x1"; //只设置失败/成功其中之一的描述，另一个没效果的不要定义描述
    }

    protected override int CalcBonus(Employee emp, Employee target = null, EventGroupInfo egi = null)
    {
        int result = CalcEmotion(emp) + CalcDivisionFaith(emp, target, egi) + CalcPerk(emp) + CalcDivisionManage(emp, target, egi);
        return result;
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //随机文案
        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        GameControl.Instance.AddEventProgress(1, true);

        //随机文案
        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = SelfName + "已经连续多次工作没有被驳回！项目进展非常迅速！" + SuccessDescription;
        }
        else
        {
            if (index == 1)
                content = SelfName + "已经连续多次工作没有被驳回！项目进展非常迅速！但是没有效果";

        }
        return content;
    }
}
public class EventC3 : Event
{
    public EventC3() : base()
    {
        EventName = "欢欣鼓舞";
        DescriptionCount = 1;
        RequiredCondition = EventCondition.成就感;
        SuccessDescription = " 成功，公司内产生认同 x1"; //只设置失败/成功其中之一的描述，另一个没效果的不要定义描述
    }

    protected override int CalcBonus(Employee emp, Employee target = null, EventGroupInfo egi = null)
    {
        int result = CalcEmotion(emp) + CalcDivisionFaith(emp, target, egi) + CalcPerk(emp) + CalcDivisionManage(emp, target, egi);
        return result;
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //随机文案
        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        GameControl.Instance.AddEventProgress(1, true);

        //随机文案
        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = SelfName + "在统一了方法论之后使自己的效率翻倍了！成堆的已完成的工作让他感受到了每一滴汗水的价值！" + SuccessDescription;
        }
        else
        {
            if (index == 1)
                content = SelfName + "在统一了方法论之后使自己的效率翻倍了！成堆的已完成的工作让他感受到了每一滴汗水的价值！但是没有效果";

        }
        return content;
    }
}


public class EventC4 : Event
{
    public EventC4() : base()
    {
        EventName = "怀疑发展方向";
        DescriptionCount = 1;
        RequiredCondition = EventCondition.困惑;
        FailDescription = " 失败，公司内产生不满 x1"; //只设置失败/成功其中之一的描述，另一个没效果的不要定义描述
    }

    protected override int CalcBonus(Employee emp, Employee target = null, EventGroupInfo egi = null)
    {
        int result = CalcEmotion(emp) + CalcDivisionFaith(emp, target, egi) + CalcPerk(emp) + CalcDivisionManage(emp, target, egi);
        return result;
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //随机文案
        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        GameControl.Instance.AddEventProgress(1, true);

        //随机文案
        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = SelfName + "对自己的工作感到非常困惑，总是在抱怨自己不知道自己在做什么…，但是没有效果";
        }
        else
        {
            if (index == 1)
                content = SelfName + "对自己的工作感到非常困惑，总是在抱怨自己不知道自己在做什么…" + FailDescription;

        }
        return content;
    }
}

public class EventC5 : Event
{
    public EventC5() : base()
    {
        EventName = "甩锅";
        DescriptionCount = 1;
        RequiredCondition = EventCondition.悔恨;
        FailDescription = " 失败，公司内产生不满 x1"; //只设置失败/成功其中之一的描述，另一个没效果的不要定义描述
    }

    protected override int CalcBonus(Employee emp, Employee target = null, EventGroupInfo egi = null)
    {
        int result = CalcEmotion(emp) + CalcDivisionFaith(emp, target, egi) + CalcPerk(emp) + CalcDivisionManage(emp, target, egi);
        return result;
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //随机文案
        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        GameControl.Instance.AddEventProgress(1, true);

        //随机文案
        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = SelfName + "感到非常耻辱，他觉得都是同事的错才会导致他犯下这样的低级错误，但是没有效果";
        }
        else
        {
            if (index == 1)
                content = SelfName + "感到非常耻辱，他觉得都是同事的错才会导致他犯下这样的低级错误" + FailDescription;

        }
        return content;
    }
}//以下是公司一般事件

public static class EventData
{
    //公司日常事件序列(无状态条件事件)
    public static List<Event> CompanyRoutineEventA = new List<Event>()
    {
        Event1(),Event2(),Event3(),Event4(),Event5()
    };

    //公司日常事件序列(状态条件事件)
    public static List<Event> CompanyRoutineEventB = new List<Event>()
    {
        Event6(),Event7(),Event8()
    };

    //公司一般事件序列
    public static List<Event> CompanyNormalEvent = new List<Event>()
    {
        new EventC1(),new EventC2(),new EventC3(),new EventC4(),new EventC5(),
    };

    //个人港口事件序列
    public static List<Event> EmpPortEvent = new List<Event>()
    {
        Event9(),Event10(),Event11(),Event12(),Event13()
    };

    //个人事件序列(只记第一个事件)，且Event14比较特殊故不计入
    public static List<Event> EmpPersonalEvent = new List<Event>()
    {
        Event19(),Event24(),Event29(),Event34(),Event39(),Event44(),Event49(),Event54(),Event59(),Event64(),Event69(),Event74(),Event79(),Event84(),Event89(),Event94(),Event99(),Event104(),Event109(),Event114(),Event119(),Event124(),Event129(),Event134(),Event139()
    };

    //特殊事件组
    public static List<EventGroup> SpecialEventGroups = new List<EventGroup>()
    {
        new EventGroup1()
    };
}

public class Event1 : Event
{
    public Event1() : base()
    {
        EventName = "互相协助";
        LevelRequire = new List<int>() { 1, 2, 3 };
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方获得事件状态“顺利” ×2"; //成功/失败描述最前面加个空格或者逗号
        FailDescription = " 双方获得事件状态“顺利” ×2";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        emp.AddTempEventCondition(EventCondition.顺利);
        emp.AddTempEventCondition(EventCondition.顺利);
        target.AddTempEventCondition(EventCondition.顺利);
        target.AddTempEventCondition(EventCondition.顺利);

        //随机文案
        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        emp.AddTempEventCondition(EventCondition.顺利);
        target.AddTempEventCondition(EventCondition.顺利);

        //随机文案
        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);//这一行一定要加
        if (success == true)
        {
            if (index == 1)
                content = "在" + PlaceName + "，" + SelfName + "与" + TargetName + "一起梳理工作流程，获得很大进展";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + PlaceName + "，" + SelfName + "与" + TargetName + "开会，沟通彼此工作方法中的差异";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);//这一行一定要加
        //注意这里SelfName和TargetName要反过来，如果文案本身要反过来的话
        if (success == true)
            content = content = "在" + PlaceName + "，" + TargetName + "与" + SelfName + "一起梳理工作流程，获得很大进展";
        else
            content = content = "在" + PlaceName + "，" + TargetName + "与" + SelfName + "开会，沟通彼此工作方法中的差异";
        return content;
    }
}

public class Event2 : Event
{
    public Event2() : base()
    {
        EventName = "/";
        LevelRequire = new List<int>(2,3);
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方获得事件状态“成就感” ×2";
        FailDescription = " 双方获得事件状态“成就感” ×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方获得事件状态“成就感” ×2
        emp.AddTempEventCondition(EventCondition.成就感);
        emp.AddTempEventCondition(EventCondition.成就感);
        target.AddTempEventCondition(EventCondition.成就感);
        target.AddTempEventCondition(EventCondition.成就感);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方获得事件状态“成就感” ×1
        emp.AddTempEventCondition(EventCondition.成就感);
        target.AddTempEventCondition(EventCondition.成就感);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在 " + placeName + "的会议中，上级mm对下属nn当前做法表达赞赏";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在 " + placeName + "的会议中，上级mm认可了下属nn当前的做法";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在 " + placeName + "的会议中，下属nn接受上级mm的嘉奖";
        else
            content = content = "在 " + placeName + "的会议中，下属nn当前的做法获得上级mm许可";
        return content;
    }
}
public class Event3 : Event
{
    public Event3() : base()
    {
        EventName = "扯皮";
        LevelRequire = new List<int>(1);//"关系:同事"
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方获得事件状态“烦恼” ×1";
        FailDescription = " 双方获得事件状态“烦恼” ×2";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方获得事件状态“烦恼” ×1
        emp.AddTempEventCondition(EventCondition.烦恼);
        target.AddTempEventCondition(EventCondition.烦恼);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方获得事件状态“烦恼” ×2
        emp.AddTempEventCondition(EventCondition.烦恼);
        emp.AddTempEventCondition(EventCondition.烦恼);
        target.AddTempEventCondition(EventCondition.烦恼);
        target.AddTempEventCondition(EventCondition.烦恼);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "，" + SelfName + "提出同事" + TargetName + "过于纠结细节，有些浪费时间";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "，" + SelfName + "提出自己只想把事做好，同事" + TargetName + "却总设置阻碍";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "，" + TargetName + "提出同事" + SelfName + "过于强调愿景，有些不切实际";
        else
            content = content = "在" + placeName + "，" + TargetName + "提出" + SelfName + "过于自我，对团队配合一无所知";
        return content;
    }
}
public class Event4 : Event
{
    public Event4() : base()
    {
        EventName = "甩锅三连";
        LevelRequire = new List<int>(1, 2, 3); //"关系:上下级同事"
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方获得事件状态“困惑” ×1";
        FailDescription = " 双方获得事件状态“困惑” ×2";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方获得事件状态“困惑” ×1
        emp.AddTempEventCondition(EventCondition.困惑);
        target.AddTempEventCondition(EventCondition.困惑);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方获得事件状态“困惑” ×2
        emp.AddTempEventCondition(EventCondition.困惑);
        emp.AddTempEventCondition(EventCondition.困惑);
        target.AddTempEventCondition(EventCondition.困惑);
        target.AddTempEventCondition(EventCondition.困惑);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "，" + SelfName + "表示" + TargetName + "的想法过于模糊，缺乏逻辑";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "，" + SelfName + "表示无法理解" + TargetName + "不考虑自己建议的原因";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "，" + TargetName + "提出" + SelfName + "沉浸在自己的逻辑里，根本没有在听自己在说什么";
        else
            content = content = "在" + placeName + "，" + TargetName + "提出如果没有" + SelfName + "瞎指挥，自己会更明确如何工作";
        return content;
    }
}
public class Event5 : Event
{
    public Event5() : base()
    {
        EventName = "验收工作";
        LevelRequire = new List<int>(2, 3); //"关系:上下级"
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方获得事件状态“悔恨” ×1";
        FailDescription = " 双方获得事件状态“悔恨” ×2";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方获得事件状态“悔恨” ×1
        emp.AddTempEventCondition(EventCondition.悔恨);
        target.AddTempEventCondition(EventCondition.悔恨);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方获得事件状态“悔恨” ×2
        emp.AddTempEventCondition(EventCondition.悔恨);
        emp.AddTempEventCondition(EventCondition.悔恨);
        target.AddTempEventCondition(EventCondition.悔恨);
        target.AddTempEventCondition(EventCondition.悔恨);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "，上级mm在验收工作时表示未达要求，下属nn误解自己的意思";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "，上级mm在验收工作时表示未达标，下属nn捏造理由推卸责任";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "汇报工作时被骂，下属nn用沉默反抗上级mm，最后表示会做修改";
        else
            content = content = "在" + placeName + "汇报工作时被骂，下属nn表示自己完全按照上级mm的要求来做的";
        return content;
    }
}
public class Event6 : Event
{
    public Event6() : base()
    {
        EventName = "振奋";
        DescriptionCount = 1;
        SingleResult = false;
        //主导情绪为振奋
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方获得事件状态“成就感” ×2";
        FailDescription = " 双方获得事件状态“成就感” ×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方获得事件状态“成就感” ×2
        emp.AddTempEventCondition(EventCondition.成就感);
        target.AddTempEventCondition(EventCondition.成就感);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方获得事件状态“成就感” ×1
        emp.AddTempEventCondition(EventCondition.成就感);
        emp.AddTempEventCondition(EventCondition.成就感);
        target.AddTempEventCondition(EventCondition.成就感);
        target.AddTempEventCondition(EventCondition.成就感);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + SelfName + "十分振奋，感到解决方案如星辰画卷在自己眼前展开";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + SelfName + "感到振奋，隐隐感到自己辛苦的工作都是值得的";
        }
        return content;
    }

    protected override int CalcBonus(Employee emp, Employee target = null, EventGroupInfo egi = null)
    {
        return 0;
    }
}
public class Event7 : Event
{
    public Event7() : base()
    {
        EventName = "偏执";
        DescriptionCount = 1;
        SingleResult = false;
        //主导情绪：偏执
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方获得事件状态“烦恼” ×1";
        FailDescription = " 双方获得事件状态“烦恼” ×2";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方获得事件状态“烦恼” ×1
        emp.AddTempEventCondition(EventCondition.烦恼);
        target.AddTempEventCondition(EventCondition.烦恼);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方获得事件状态“烦恼” ×2
        emp.AddTempEventCondition(EventCondition.烦恼);
        emp.AddTempEventCondition(EventCondition.烦恼);
        target.AddTempEventCondition(EventCondition.烦恼);
        target.AddTempEventCondition(EventCondition.烦恼);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + SelfName + "认为同事很蠢，都在用低效的方式工作";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + SelfName + "无法理解自己明明是对的，为什么其他人不听从自己的要求";
        }
        return content;
    }
    protected override int CalcBonus(Employee emp, Employee target = null, EventGroupInfo egi = null)
    {
        return 0;
    }
}
public class Event8 : Event
{
    public Event8() : base()
    {
        EventName = "疲劳";
        DescriptionCount = 1;
        SingleResult = false;//"体力<50
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方获得事件状态“悔恨” ×1";
        FailDescription = " 双方获得事件状态“悔恨” ×2";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方获得事件状态“悔恨” ×1
        emp.AddTempEventCondition(EventCondition.悔恨);
        target.AddTempEventCondition(EventCondition.悔恨);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方获得事件状态“悔恨” ×2
        emp.AddTempEventCondition(EventCondition.悔恨);
        emp.AddTempEventCondition(EventCondition.悔恨);
        target.AddTempEventCondition(EventCondition.悔恨);
        target.AddTempEventCondition(EventCondition.悔恨);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "";
        }
        return content;
    }
    protected override int CalcBonus(Employee emp, Employee target = null, EventGroupInfo egi = null)
    {
        return 0;
    }
}
public class Event9 : Event
{
    public Event9() : base()
    {
        EventName = "分享快乐";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.顺利;
        SuccessDescription = " 获得事件状态“分享乐事” ×1";
        FailDescription = " 获得事件状态“分享乐事” ×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"获得事件状态“分享乐事” ×1
        emp.AddTempEventCondition(EventCondition.分享乐事);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"获得事件状态“分享乐事” ×1
        emp.AddTempEventCondition(EventCondition.分享乐事);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "由于工作顺利，" + SelfName + "难以抑制地想要与他人分享自己心中的乐事";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "由于工作顺利，" + SelfName + "难以抑制地想要与他人分享自己心中的乐事";
        }
        return content;
    }

    protected override int CalcBonus(Employee emp, Employee target = null, EventGroupInfo egi = null)
    {
        return 0;
    }
}
public class Event10 : Event
{
    public Event10() : base()
    {
        EventName = "分享日常";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.烦恼;
        SuccessDescription = " 获得事件状态“分享日常” ×1";
        FailDescription = " 获得事件状态“分享日常” ×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"获得事件状态“分享日常” ×1
        emp.AddTempEventCondition(EventCondition.分享日常);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"获得事件状态“分享日常” ×1
        emp.AddTempEventCondition(EventCondition.分享日常);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "由于工作上的烦恼，" + SelfName + "想和其他人吐槽一下老板和同事";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "由于工作上的烦恼，" + SelfName + "想和其他人吐槽一下老板和同事";
        }
        return content;
    }

    protected override int CalcBonus(Employee emp, Employee target = null, EventGroupInfo egi = null)
    {
        return 0;
    }
}
public class Event11 : Event
{
    public Event11() : base()
    {
        EventName = "寻求安慰";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.悔恨;
        SuccessDescription = " 获得事件状态“寻求安慰” ×1";
        FailDescription = " 获得事件状态“寻求安慰” ×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"获得事件状态“寻求安慰” ×1
        emp.AddTempEventCondition(EventCondition.寻求安慰);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"获得事件状态“寻求安慰” ×1
        emp.AddTempEventCondition(EventCondition.寻求安慰);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "由于内心的悔恨无法平静，" + SelfName + "想向其他人寻求安慰";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "由于内心的悔恨无法平静，" + SelfName + "想向其他人寻求安慰";
        }
        return content;
    }
}
public class Event12 : Event
{
    public Event12() : base()
    {
        EventName = "深刻交谈";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.成就感;
        SuccessDescription = " 获得事件状态“深刻交谈” ×1";
        FailDescription = " 获得事件状态“深刻交谈” ×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"获得事件状态“深刻交谈” ×1
        emp.AddTempEventCondition(EventCondition.深刻交谈);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"获得事件状态“深刻交谈” ×1
        emp.AddTempEventCondition(EventCondition.深刻交谈);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "由于近来工作颇有成就，满足的" + SelfName + "想找人进行更深刻的探讨";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "由于近来工作颇有成就，满足的" + SelfName + "想找人进行更深刻的探讨";
        }
        return content;
    }
    protected override int CalcBonus(Employee emp, Employee target = null, EventGroupInfo egi = null)
    {
        return 0;
    }
}
public class Event13 : Event
{
    public Event13() : base()
    {
        EventName = "认可交谈";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.困惑;
        SuccessDescription = " 获得事件状态“认可交谈” ×1";
        FailDescription = " 获得事件状态“认可交谈” ×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"获得事件状态“认可交谈” ×1
        emp.AddTempEventCondition(EventCondition.认可交谈);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"获得事件状态“认可交谈” ×1
        emp.AddTempEventCondition(EventCondition.认可交谈);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "由于工作上的困惑，迷茫的" + SelfName + "开始怀疑自己是否能够做到";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "由于工作上的困惑，迷茫的" + SelfName + "开始怀疑自己是否能够做到";
        }
        return content;
    }
    protected override int CalcBonus(Employee emp, Employee target = null, EventGroupInfo egi = null)
    {
        return 0;
    }
}
public class Event14 : Event
{
    public Event14() : base()
    {
        EventName = "认识新人A";
        DescriptionCount = 1;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度+5"; //成功/失败描述最前面加个空格或者逗号
        FailDescription = " 双方好感度-5";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        emp.AddTempRelation(target, 5);
        target.AddTempRelation(emp, 5);

        //随机文案
        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));

        new Event15().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);

        //随机文案
        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));

        new Event17().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + PlaceName + "初遇" + TargetName + "，感觉似曾相识";
        }
        else
        {
            if (index == 1)
                content = "在" + PlaceName + "初遇" + TargetName + "，感觉不太喜欢对方";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = "在" + PlaceName + "初遇" + SelfName + "，感觉似曾相识";
        else
            content = "在" + PlaceName + "初遇" + SelfName + "，感觉对方比较冷淡";
        return content;
    }
}

public class Event15 : Event
{
    public Event15() : base()
    {
        EventName = "认识新人B";
        DescriptionCount = 1;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度+5"; //成功/失败描述最前面加个空格或者逗号
        FailDescription = "";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        emp.AddTempRelation(target, 5);
        target.AddTempRelation(emp, 5);

        //随机文案
        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));

        new Event16().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //随机文案
        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = TargetName + "与自己聊了起来，意外地感到愉快";
        }
        else
        {
            if (index == 1)
                content = "与" + TargetName + "聊了两句，感觉不太投机";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = "与" + SelfName + "聊了一会儿，感觉有点投缘";
        else
            content = "与" + SelfName + "聊了一会儿，似乎没找到共同话题";
        return content;
    }
}

public class Event16 : Event
{
    public Event16() : base()
    {
        EventName = "认识新人C";
        DescriptionCount = 1;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度+5，双方获得情绪“愉悦”×1"; //成功/失败描述最前面加个空格或者逗号
        FailDescription = "";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        emp.AddTempRelation(target, 5);
        emp.AddTempEmotion(EColor.LYellow);
        target.AddTempRelation(emp, 5);
        target.AddTempEmotion(EColor.LYellow);

        //随机文案
        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //随机文案
        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "与" + TargetName + "聊起自己的经历，对方也有许多共鸣";
        }
        else
        {
            if (index == 1)
                content = "与" + TargetName + "聊了会儿以前的经历，就回去工作了";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = "与" + SelfName + "聊了起来，对方给自己留下很好的印象";
        else
            content = "与" + SelfName + "又聊了一会儿，就各自回去工作了";
        return content;
    }
}

public class Event17 : Event
{
    public Event17() : base()
    {
        EventName = "认识新人C";
        DescriptionCount = 1;
        RequiredCondition = EventCondition.无;
        SuccessDescription = ""; //成功/失败描述最前面加个空格或者逗号
        FailDescription = "双方好感度-5，双方获得情绪“厌恶”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //随机文案
        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        emp.AddTempRelation(target, -5);
        emp.AddTempEmotion(EColor.LRed);
        target.AddTempRelation(emp, -5);
        target.AddTempEmotion(EColor.LRed);
        //随机文案
        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event18().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "与" + TargetName + "聊了两句日常，" + TargetName + "机械地回应着";
        }
        else
        {
            if (index == 1)
                content = "与" + TargetName + "保持沉默，气氛中有一丝尴尬";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = "跟" + SelfName + "聊了些无聊的日常，至少认识了一个新同事";
        else
            content = "在一言不发的尴尬中，仍然未和" + SelfName + "说一句话";
        return content;
    }
}

public class Event18 : Event
{
    public Event18() : base()
    {
        EventName = "认识新人E";
        DescriptionCount = 1;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " "; //成功/失败描述最前面加个空格或者逗号
        FailDescription = " 双方好感度-5，双方获得情绪“苦涩”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //随机文案
        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        emp.AddTempRelation(target, -5);
        emp.AddTempEmotion(EColor.LBlue);
        target.AddTempRelation(emp, -5);
        target.AddTempEmotion(EColor.LBlue);

        //随机文案
        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "率先离开了办公室";
        }
        else
        {
            if (index == 1)
                content = "强行追问" + TargetName + "是干什么的，来这儿做什么";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = SelfName + "离开了办公室，自己也松了口气";
        else
            content = "不喜欢" + SelfName + "居高临下的说话方式，在回答后就离开了办公室";
        return content;
    }
}
public class Event19 : Event
{
    public Event19() : base()
    {
        EventName = "陌生日常A";
        FriendRequire = 0;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.分享日常;
        SuccessDescription = " /";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event20().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event22().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到" + TargetName + "，开了几句别的同事的玩笑";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到" + TargetName + "，开了几句别的同事的玩笑";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到" + SelfName + "，对方开了几句别的同事的玩笑";
        else
            content = content = "在" + placeName + "见到" + SelfName + "，对方在嘲笑别的同事";
        return content;
    }
}
public class Event20 : Event
{
    public Event20() : base()
    {
        EventName = "陌生日常B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event21().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "然后开始模仿同事的腔调说话，" + TargetName + "也跟着一起笑";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "然后开始模仿同事的腔调说话，" + TargetName + "却觉得有些尴尬";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "模仿同事非常传神，于是和对方一起笑了起来";
        else
            content = content = "" + SelfName + "模仿同事的表现有点过分，似乎对他人不太尊重";
        return content;
    }
}
public class Event21 : Event
{
    public Event21() : base()
    {
        EventName = "陌生日常C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5 ";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5 
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);


        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "最后吐槽了上级最近的不合理行为，引得" + TargetName + "共鸣";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "最后吐槽了上级最近的不合理行为，" + TargetName + "却面露难色";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "认为" + SelfName + "说出了自己心声，对方上级确实需要多补脑";
        else
            content = content = "认为" + SelfName + "吐槽老板的行为有些越线，因此不知所措";
        return content;
    }
}
public class Event22 : Event
{
    public Event22() : base()
    {
        EventName = "陌生日常D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5, 双方获得情绪“厌恶”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5双方获得情绪“厌恶”×1
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);
        emp.AddTempEmotion(EColor.LRed);
        target.AddTempEmotion(EColor.LRed);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event23().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "然后自言自语了起来，同时" + TargetName + "也没有什么反应。";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "问" + TargetName + "是否也觉得那个同事有问题，对方表示请闭嘴。";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "不喜欢听" + SelfName + "聊办公室日常，认为应该多花心思工作";
        else
            content = content = "对" + SelfName + "不好好工作且打扰别人的低效行为表达了厌恶";
        return content;
    }
}
public class Event23 : Event
{
    public Event23() : base()
    {
        EventName = "陌生日常E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "向" + TargetName + "解释自己只是有些烦恼，无意打扰" + TargetName + "";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "回击" + TargetName + "没有同理心，表示对方是个无聊的机器人";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "对" + SelfName + "的道歉表示接受，并希望下不为例";
        else
            content = content = "对" + SelfName + "持续不断发言感到无语，只好无奈地继续工作";
        return content;
    }
}
public class Event24 : Event
{
    public Event24() : base()
    {
        EventName = "陌生安慰A";
        FriendRequire = 0;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.寻求安慰;
        SuccessDescription = " /";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event25().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event27().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到" + TargetName + "，抱怨工作真是麻烦";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到" + TargetName + "，抱怨工作真是麻烦";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到" + SelfName + "，对方似乎遇到了一些困难";
        else
            content = content = "在" + placeName + "见到" + SelfName + "，对方在抱怨无法胜任工作";
        return content;
    }
}
public class Event25 : Event
{
    public Event25() : base()
    {
        EventName = "陌生安慰B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event26().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "主动询问自己怎么了，自己只是微笑着摆摆手";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "主动询问自己怎么了，示意对方帮不上忙";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "主动询问" + SelfName + "遇到了什么问题，对方微笑着说没事";
        else
            content = content = "主动询问" + SelfName + "遇到了什么问题，对方却示意继续工作";
        return content;
    }
}
public class Event26 : Event
{
    public Event26() : base()
    {
        EventName = "陌生安慰C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +10 ";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +10 
        emp.AddTempRelation(target, +10);
        target.AddTempRelation(emp, +10);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "表达了祝愿，自己心里竟然感到一丝温暖";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "拒绝了" + TargetName + "让自己倾诉心事的请求，并表示想一个人待着";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "表示" + SelfName + "看起来有些辛苦，祝愿对方轻松顺利";
        else
            content = content = "表示" + SelfName + "一直憋着对身体不好，希望对方开朗一点";
        return content;
    }
}
public class Event27 : Event
{
    public Event27() : base()
    {
        EventName = "陌生安慰D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5 , 双方获得情绪“厌恶”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5, 双方获得情绪“厌恶”×1
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);
        emp.AddTempEmotion(EColor.LRed);
        target.AddTempEmotion(EColor.LRed);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event28().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "转头向" + TargetName + "讲述自己最近有很多失误，自己非常懊悔";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "询问" + TargetName + "是不是也搞砸过很多事情，觉得对方和自己很像";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "向" + SelfName + "表达了大家都差不多，不用太自责";
        else
            content = content = "对" + SelfName + "的询问感到无法理解，表示对方应该多反省";
        return content;
    }
}
public class Event28 : Event
{
    public Event28() : base()
    {
        EventName = "陌生安慰E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -10";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -10
        emp.AddTempRelation(target, -10);
        target.AddTempRelation(emp, -10);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "向" + TargetName + "表示自己刚才只是想多和人说两句话，现在不想了";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "表示自己不需要反省，因为" + TargetName + "比自己更需要反省";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "对" + SelfName + "以自我为中心的发言感到无奈，只好保持沉默";
        else
            content = content = "指责" + SelfName + "只会用挑衅式发言博取其他人注意";
        return content;
    }
}
public class Event29 : Event
{
    public Event29() : base()
    {
        EventName = "陌生认可A";
        FriendRequire = 0;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.认可交谈;
        SuccessDescription = " /";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event30().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event32().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到" + TargetName + "，询问" + TargetName + "对自己的印象如何";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到" + TargetName + "，询问" + TargetName + "对自己的印象如何";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到" + SelfName + "，对方诚恳地询问自己对其看法";
        else
            content = content = "在" + placeName + "见到" + SelfName + "，对方看起来没有自信";
        return content;
    }
}
public class Event30 : Event
{
    public Event30() : base()
    {
        EventName = "陌生认可B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event31().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "的评价是，能力有些短板，但是未来可期";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "对方表示不太了解，于是认为" + TargetName + "的看法有所保留";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "谨慎评价了" + SelfName + "的工作能力，表示很期待对方的成长";
        else
            content = content = "向" + SelfName + "表示自己对其不太了解";
        return content;
    }
}
public class Event31 : Event
{
    public Event31() : base()
    {
        EventName = "陌生认可C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5 ";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5 
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "向" + TargetName + "寻求了建议，对方很耐心平和地分享了看法";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "向" + TargetName + "寻求了建议，对方表示应该主要靠自己摸索";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "向自己寻求建议，于是耐心向对方分享了看法";
        else
            content = content = "" + SelfName + "向自己寻求建议，于是回应对方应自行摸索";
        return content;
    }
}
public class Event32 : Event
{
    public Event32() : base()
    {
        EventName = "陌生认可D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5, 双方获得情绪“苦涩”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5, 双方获得情绪“苦涩”×1
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);
        emp.AddTempEmotion(EColor.LBlue);
        target.AddTempEmotion(EColor.LBlue);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event33().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "进一步向" + TargetName + "表明自己不确定是否胜任工作";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "告诫人应该有自信，否则别人会瞧不起你";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "向" + SelfName + "表达对方工作不难，应该可以胜任";
        else
            content = content = "向" + SelfName + "表达，不应该在工作中表现得没有自信";
        return content;
    }
}
public class Event33 : Event
{
    public Event33() : base()
    {
        EventName = "陌生认可E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -10, 双方获得情绪“厌恶”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -10, 双方获得情绪“厌恶”×1
        emp.AddTempRelation(target, -10);
        target.AddTempRelation(emp, -10);
        emp.AddTempEmotion(EColor.LRed);
        target.AddTempEmotion(EColor.LRed);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "向" + TargetName + "表达自己暂时做不到";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "被" + TargetName + "的批评说的无地自容，感到更加讨厌自己";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "对" + SelfName + "的投降姿态表示无话可说";
        else
            content = content = "批评" + SelfName + "，能力差就去干活儿，不要在自己眼前晃";
        return content;
    }
}
public class Event34 : Event
{
    public Event34() : base()
    {
        EventName = "陌生深刻A";
        FriendRequire = 0;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.深刻交谈;
        SuccessDescription = " /";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event35().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event37().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到" + TargetName + "，表示希望和对方探讨工作方法";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到" + TargetName + "，表示希望和对方探讨工作方法";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到" + SelfName + "，对方确实看起来有很多好点子";
        else
            content = content = "在" + placeName + "见到" + SelfName + "，对方看起来想推销一些建议";
        return content;
    }
}
public class Event35 : Event
{
    public Event35() : base()
    {
        EventName = "陌生深刻B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5, 双方获得情绪“愉悦”×1";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5, 双方获得情绪“愉悦”×1
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);
        emp.AddTempEmotion(EColor.LYellow);
        target.AddTempEmotion(EColor.LYellow);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event36().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "询问对方最近工作的心得，并获得" + TargetName + "积极反馈";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "建议" + TargetName + "可以从我的经验里学习一些东西，对方报以微笑";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "向我咨询了一些工作方法，我很愿意和对方交流";
        else
            content = content = "" + SelfName + "希望我向其学习，我只好尴尬地微笑";
        return content;
    }
}
public class Event36 : Event
{
    public Event36() : base()
    {
        EventName = "陌生深刻C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +10 ";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +10 
        emp.AddTempRelation(target, +10);
        target.AddTempRelation(emp, +10);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "和" + TargetName + "一起头脑风暴，发现彼此的工作方法非常互补";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "和" + TargetName + "聊了之后，表示每个人的经验都是不可复制的";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "认为" + SelfName + "是个思维敏锐且包容的伙伴，想要多多交流";
        else
            content = content = "认为" + SelfName + "更看重我的经验是否取得过成就，因此感到无奈";
        return content;
    }
}
public class Event37 : Event
{
    public Event37() : base()
    {
        EventName = "陌生深刻D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5, 双方获得情绪“苦涩”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5, 双方获得情绪“苦涩”×1
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);
        emp.AddTempEmotion(EColor.LBlue);
        target.AddTempEmotion(EColor.LBlue);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event38().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "向" + TargetName + "分享了一些我最近的成功经验";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "向" + TargetName + "表达了对方应该多多思考如何做好工作，别再浪费青春";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "默默听完" + SelfName + "分享成就和经验，转头去做我的事了";
        else
            content = content = "" + SelfName + "不间断地向我讲述大道理，我偶尔点几下头";
        return content;
    }
}
public class Event38 : Event
{
    public Event38() : base()
    {
        EventName = "陌生深刻E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "要求" + TargetName + "总结我最近的成就中有哪些共通的经验，对方没有想到";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "当我想继续分享知识时，" + TargetName + "大声地说让我闭嘴，然后逃跑了";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "要求我说出其成功的原因，我实在是不感兴趣";
        else
            content = content = "因无法忍受" + SelfName + "的行为，终于呵斥了对方无聊的发言";
        return content;
    }
}
public class Event39 : Event
{
    public Event39() : base()
    {
        EventName = "陌生乐事A";
        FriendRequire = 0;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.分享乐事;
        SuccessDescription = " /";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event40().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event42().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到" + TargetName + "，于是我分享了最近开心的事";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到" + TargetName + "，于是我分享了最近开心的事";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到" + SelfName + "，对方看起来很开心";
        else
            content = content = "在" + placeName + "见到" + SelfName + "，对方自顾自地说着开心的事情";
        return content;
    }
}
public class Event40 : Event
{
    public Event40() : base()
    {
        EventName = "陌生乐事B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event41().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "也和我讲了一些从前开心的事情，我觉得很有趣";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "也和我讲了一些从前开心的事情，我不是很关心";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "我也向" + SelfName + "分享了一些开心的事，气氛很好";
        else
            content = content = "我也向" + SelfName + "分享了一些开心的事，对方似乎反应平平";
        return content;
    }
}
public class Event41 : Event
{
    public Event41() : base()
    {
        EventName = "陌生乐事C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5 , 双方获得情绪“愉悦”×1";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5 ,双方获得情绪“愉悦”×1
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);
        emp.AddTempEmotion(EColor.LYellow);
        target.AddTempEmotion(EColor.LYellow);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我和" + TargetName + "哈哈大笑，好一阵儿才能停下来";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我和" + TargetName + "又开了几个玩笑，大家就各自工作了";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "和我大笑了起来，感觉肚子都笑疼了";
        else
            content = content = "" + SelfName + "和我又开了几个玩笑，大家就各自工作了";
        return content;
    }
}
public class Event42 : Event
{
    public Event42() : base()
    {
        EventName = "陌生乐事D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5, 双方获得情绪“苦涩”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5, 双方获得情绪“苦涩”×1
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);
        emp.AddTempEmotion(EColor.LBlue);
        target.AddTempEmotion(EColor.LBlue);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event43().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我又说了一会儿就工作了，" + TargetName + "似乎笑了一下";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我又尴尬地独自说了一会儿，然后问" + TargetName + "觉得怎么样";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "终于停下来不说了，好开心啊";
        else
            content = content = "" + SelfName + "停下来了，却转过来问我觉得幽默不";
        return content;
    }
}
public class Event43 : Event
{
    public Event43() : base()
    {
        EventName = "陌生乐事E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "只是看了我一眼，我只好回去工作了";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "表示能看见我的后槽牙，并且是蛀牙";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "对" + SelfName + "开心的事情没有兴趣，只看了对方一眼";
        else
            content = content = "眼前让我感到唯一开心的事，就是讽刺" + SelfName + "的丑陋笑容";
        return content;
    }
}
public class Event44 : Event
{
    public Event44() : base()
    {
        EventName = "朋友日常A";
        FriendRequire = 1;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.分享日常;
        SuccessDescription = " 双方好感度   +5";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event45().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event47().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到朋友" + TargetName + "，和对方聊起私人船只价格还在上涨";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到朋友" + TargetName + "，和对方聊起私人船只价格还在上涨";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到朋友" + SelfName + "，彼此聊了会儿生活方面的话题";
        else
            content = content = "在" + placeName + "见到朋友" + SelfName + "，对方想聊生活日常，没注意到我在忙";
        return content;
    }
}
public class Event45 : Event
{
    public Event45() : base()
    {
        EventName = "朋友日常B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5, 双方获得情绪“愉悦”×1";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5, 双方获得情绪“愉悦”×1
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);
        emp.AddTempEmotion(EColor.LYellow);
        target.AddTempEmotion(EColor.LYellow);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event46().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "对我说的话题很感兴趣，向我分享了一个相关内容的网站";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "说有事要忙，我们就中断了聊天";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "和" + SelfName + "的聊天令人轻松愉悦，我向对方推荐了相关网站";
        else
            content = content = "对" + SelfName + "聊的内容不太感兴趣，于是我就找了个借口离开了";
        return content;
    }
}
public class Event46 : Event
{
    public Event46() : base()
    {
        EventName = "朋友日常C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5 ";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5 
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我想到最近买的一本书很适合" + TargetName + "，于是送给了对方";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我想起了有个邮件没发，于是打断了" + TargetName + "的分享";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "然后" + SelfName + "向我赠送了一个小礼物，我感到很开心";
        else
            content = content = "" + SelfName + "有事要忙，于是我们终止了聊天";
        return content;
    }
}
public class Event47 : Event
{
    public Event47() : base()
    {
        EventName = "朋友日常D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5, 双方获得情绪“苦涩”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5, 双方获得情绪“苦涩”×1
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);
        emp.AddTempEmotion(EColor.LBlue);
        target.AddTempEmotion(EColor.LBlue);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event48().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "对方暗示还有事情要忙，于是我和" + TargetName + "分开了";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "对方说还有事情要忙，但是我觉得可以再聊一会儿";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "和" + SelfName + "表达了自己的工作还没做完之后，对方主动离开了";
        else
            content = content = "和" + SelfName + "表达了自己的工作还没做完之后，对方无动于衷";
        return content;
    }
}
public class Event48 : Event
{
    public Event48() : base()
    {
        EventName = "朋友日常E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -15, 双方获得情绪“苦涩”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -15,双方获得情绪“苦涩”×1
        emp.AddTempRelation(target, -15);
        target.AddTempRelation(emp, -15);
        emp.AddTempEmotion(EColor.LBlue);
        target.AddTempEmotion(EColor.LBlue);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "直接表示还有事情要做，于是我悻悻离去了";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "表示还有事情要做，我提出对方的事情既不紧急又不重要";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "直接地向" + SelfName + "表明了自己在忙，对方不情愿地离开了";
        else
            content = content = "直接地向" + SelfName + "表明了自己在忙，对方却认为我的事不重要";
        return content;
    }
}
public class Event49 : Event
{
    public Event49() : base()
    {
        EventName = "朋友安慰A";
        FriendRequire = 1;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.寻求安慰;
        SuccessDescription = " 双方好感度   +5";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event50().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event52().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到朋友" + TargetName + "，向" + TargetName + "倾诉我低落的心情";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到朋友" + TargetName + "，向" + TargetName + "倾诉我低落的心情";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到朋友" + SelfName + "，对方向我分享了最近遇到的麻烦";
        else
            content = content = "在" + placeName + "见到朋友" + SelfName + "，对方似乎把什么事情搞砸了";
        return content;
    }
}
public class Event50 : Event
{
    public Event50() : base()
    {
        EventName = "朋友安慰B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5, 双方获得情绪“愉悦”×1";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5, 双方获得情绪“愉悦”×1
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);
        emp.AddTempEmotion(EColor.LYellow);
        target.AddTempEmotion(EColor.LYellow);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event51().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "耐心倾听了我的遭遇，并给了我一个拥抱";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "给了我一些建议，我虽然觉得没什么用不过也感谢对方帮忙";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "听起来" + SelfName + "经历了很多，于是我拥抱了对方";
        else
            content = content = "听起来" + SelfName + "需要指导，于是我提出，如果是我会怎么处理";
        return content;
    }
}
public class Event51 : Event
{
    public Event51() : base()
    {
        EventName = "朋友安慰C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5 ";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5 
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我向" + TargetName + "表达了感谢，感觉有这样的朋友很幸运";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我感觉好了一些，于是就和" + TargetName + "说再见了";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "对我表达了感谢，我也很高兴看到对方开心起来";
        else
            content = content = "" + SelfName + "似乎好了一些，于是我们就各自回去干活了";
        return content;
    }
}
public class Event52 : Event
{
    public Event52() : base()
    {
        EventName = "朋友安慰D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -15, 双方获得情绪“厌恶”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -15, 双方获得情绪“厌恶”×1
        emp.AddTempRelation(target, -15);
        target.AddTempRelation(emp, -15);
        emp.AddTempEmotion(EColor.LRed);
        target.AddTempEmotion(EColor.LRed);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event53().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "表示很抱歉自己还有些事，回头有空了再聊我的事";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "提出，请让我别把对方当成垃圾桶，一直说那些倒霉事";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "希望今天" + SelfName + "不要把我当垃圾桶，于是我找了借口离开";
        else
            content = content = "我直接和" + SelfName + "提出，对方应该学会自己解决这些问题";
        return content;
    }
}
public class Event53 : Event
{
    public Event53() : base()
    {
        EventName = "朋友安慰E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -10";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -10
        emp.AddTempRelation(target, -10);
        target.AddTempRelation(emp, -10);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "面对" + TargetName + "的话，我只好沉默离开了";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "面对" + TargetName + "的话，我怒斥对方应该倾听朋友的心声，不要那么自私";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "终于离开了，希望对方以后能像个成年人一样行事";
        else
            content = content = "" + SelfName + "没有离开，大吼什么倾听朋友，没想到对方如此幼稚";
        return content;
    }
}
public class Event54 : Event
{
    public Event54() : base()
    {
        EventName = "朋友认可A";
        FriendRequire = 1;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.认可交谈;
        SuccessDescription = " /";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event55().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event57().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到朋友" + TargetName + "，分享近来工作内容，并询问对方评价";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到朋友" + TargetName + "，分享近来工作内容，并询问对方评价";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到朋友" + SelfName + "，对方请我评价其近期工作";
        else
            content = content = "在" + placeName + "见到朋友" + SelfName + "，对方请我评价其工作能力";
        return content;
    }
}
public class Event55 : Event
{
    public Event55() : base()
    {
        EventName = "朋友认可B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event56().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "认为我做的挺好的，有些具体内容需要注意";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "认为我确实有些外行，有很多方面可以优化";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "给" + SelfName + "提了些建议，并肯定了对方的工作方案";
        else
            content = content = "给" + SelfName + "提了些建议，并希望对方更快地成长";
        return content;
    }
}
public class Event56 : Event
{
    public Event56() : base()
    {
        EventName = "朋友认可C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5 ";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5 
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "表示始终相信着我，对方认为我很有才能";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "表示" + TargetName + "比较有才能，我或许永远也做不到对方的水平";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "向" + SelfName + "表达了心底对其的认可，希望对方更相信自己";
        else
            content = content = "" + SelfName + "有些妄自菲薄，还说其才能不如我，心累啊";
        return content;
    }
}
public class Event57 : Event
{
    public Event57() : base()
    {
        EventName = "朋友认可D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5, 双方获得情绪“苦涩”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5， 双方获得情绪“苦涩”×1
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);
        emp.AddTempEmotion(EColor.LBlue);
        target.AddTempEmotion(EColor.LBlue);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event58().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "认为我很强，并且不要多想，我觉得对方有些敷衍";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "认为我确实能力一般，不过大多数像我一样的人都很平凡";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "向" + SelfName + "表示对方超级强大，对方似乎不太接受";
        else
            content = content = "向" + SelfName + "表达对其能力的真实看法，我和多数人都很渺小的事实";
        return content;
    }
}
public class Event58 : Event
{
    public Event58() : base()
    {
        EventName = "朋友认可E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -10, 双方获得情绪“厌恶”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -10 ,双方获得情绪“厌恶”×1
        emp.AddTempRelation(target, -10);
        target.AddTempRelation(emp, -10);
        emp.AddTempEmotion(EColor.LRed);
        target.AddTempEmotion(EColor.LRed);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "向" + TargetName + "表明，我不想只做一个普通人，对方点点头";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "向对方提出" + TargetName + "或许只是普通人，但是我一定不要一辈子打工";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "突然说不想过普通生活，有些震惊的我只好点头附和";
        else
            content = content = "" + SelfName + "认为我是燕雀，不了解鸿鹄的志向";
        return content;
    }
}
public class Event59 : Event
{
    public Event59() : base()
    {
        EventName = "朋友深刻A";
        FriendRequire = 1;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.深刻交谈;
        SuccessDescription = " 双方好感度   +5";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event60().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event62().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到朋友" + TargetName + "，我积极地分享自己最近工作上的收获";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到朋友" + TargetName + "，我积极地分享自己最近工作上的收获";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到朋友" + SelfName + "，对方看起来状态非常好";
        else
            content = content = "在" + placeName + "见到朋友" + SelfName + "，对方看起来很成功";
        return content;
    }
}
public class Event60 : Event
{
    public Event60() : base()
    {
        EventName = "朋友深刻B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5, 双方获得情绪“愉悦”×1";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5, 双方获得情绪“愉悦”×1
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);
        emp.AddTempEmotion(EColor.LYellow);
        target.AddTempEmotion(EColor.LYellow);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event61().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "认为我状态非常好，对我分享的事情很感兴趣";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "认为我状态非常好，祝愿我可以保持，然后就离开了";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "我主动向" + SelfName + "咨询了一些经验心得，确实有不少收获";
        else
            content = content = "我向" + SelfName + "表达了祝愿，希望对方一直保持，然后去工作了";
        return content;
    }
}
public class Event61 : Event
{
    public Event61() : base()
    {
        EventName = "朋友深刻C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我向" + TargetName + "询问了近况，从中发现了一些很有趣的想法";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我继续向" + TargetName + "分享自己的经验，对方有时会打几个哈欠";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "向询问我的近况，对我的很多想法表示受到启发";
        else
            content = content = "" + SelfName + "还在兴奋地发表意见，我有点溜号了";
        return content;
    }
}
public class Event62 : Event
{
    public Event62() : base()
    {
        EventName = "朋友深刻D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -10";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -10
        emp.AddTempRelation(target, -10);
        target.AddTempRelation(emp, -10);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event63().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "想要聊点别的话题，对我的收获不是很感兴趣";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "想要聊别的话题，我却仍然滔滔不绝";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "我想要聊点别的话题，于是向" + SelfName + "建议可否换个话题";
        else
            content = content = "我想要聊点别的话题，可是" + SelfName + "仍然说个没完没了";
        return content;
    }
}
public class Event63 : Event
{
    public Event63() : base()
    {
        EventName = "朋友深刻E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -15";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -15
        emp.AddTempRelation(target, -15);
        target.AddTempRelation(emp, -15);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "当我回过头时，发现" + TargetName + "已经默默走开了";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "想要溜走，我却留住对方，告诉对方这是难得的学习机会";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "趁着" + SelfName + "不注意我就溜走了，并给对方发了条留言告知";
        else
            content = content = "本想溜走却被发现，" + SelfName + "强行留我在这里听人生经验";
        return content;
    }
}
public class Event64 : Event
{
    public Event64() : base()
    {
        EventName = "朋友乐事A";
        FriendRequire = 1;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.分享乐事;
        SuccessDescription = " 双方好感度   +5";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event65().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event67().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到朋友" + TargetName + "，开心地向对方分享最近高兴的事";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到朋友" + TargetName + "，开心地向对方分享最近高兴的事";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到朋友" + SelfName + "，对方看起来非常开心";
        else
            content = content = "在" + placeName + "见到朋友" + SelfName + "，对方高兴的样子有些夸张";
        return content;
    }
}
public class Event65 : Event
{
    public Event65() : base()
    {
        EventName = "朋友乐事B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event66().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "听到我的事情，看起来也非常高兴";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "没有理解我高兴的原因，不过也表示祝贺";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "我听到" + SelfName + "的事情之后也很高兴";
        else
            content = content = "虽然没有理解" + SelfName + "高兴的原因，不过也向对方表达了祝贺";
        return content;
    }
}
public class Event66 : Event
{
    public Event66() : base()
    {
        EventName = "朋友乐事C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5 , 双方获得情绪“愉悦”×1";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5 , 双方获得情绪“愉悦”×1
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);
        emp.AddTempEmotion(EColor.LYellow);
        target.AddTempEmotion(EColor.LYellow);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "打算送给我一辆古董自行车表示祝贺";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我和" + TargetName + "又聊了一会儿，就回去忙了";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "看起来非常高兴，于是打算送给对方一个礼物表达祝贺";
        else
            content = content = "" + SelfName + "和我又聊了会儿，就回去忙工作了";
        return content;
    }
}
public class Event67 : Event
{
    public Event67() : base()
    {
        EventName = "朋友乐事D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -10";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -10
        emp.AddTempRelation(target, -10);
        target.AddTempRelation(emp, -10);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event68().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "偶尔附和两句好棒之类的，我一会儿就意兴阑珊了";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "一直保持沉默神情呆滞，我也不想说什么了";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "我偶尔感慨一下" + SelfName + "的高兴事，但是心里却高兴不起来";
        else
            content = content = "我实在不感兴趣" + SelfName + "在说什么，所幸对方终于停下来了";
        return content;
    }
}
public class Event68 : Event
{
    public Event68() : base()
    {
        EventName = "朋友乐事E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -10";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -10
        emp.AddTempRelation(target, -10);
        target.AddTempRelation(emp, -10);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我试探地问" + TargetName + "是不是不感兴趣，" + TargetName + "只是微微点头";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我对" + TargetName + "无视我行为表示抗议，对方却只是一脸无奈";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "问我是否不感兴趣，我只得点头承认";
        else
            content = content = "" + SelfName + "认为我不关心对方说的内容，说实话我确实不关心";
        return content;
    }
}
public class Event69 : Event
{
    public Event69() : base()
    {
        EventName = "挚友日常A";
        FriendRequire = 2;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.分享日常;
        SuccessDescription = " /";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event70().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event72().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到挚友" + TargetName + "，" + TargetName + "主动问我最近怎么样";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到挚友" + TargetName + "，" + TargetName + "和我简单打了个招呼";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到挚友" + SelfName + "，对方有事想聊，于是我主动询问";
        else
            content = content = "在" + placeName + "见到挚友" + SelfName + "，对方有心事，但是我没太理会";
        return content;
    }
}
public class Event70 : Event
{
    public Event70() : base()
    {
        EventName = "挚友日常B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event71().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我只好和" + TargetName + "讲，还是老样子，对方也是笑笑";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我和" + TargetName + "讲：没事，别担心了。对方只是笑笑";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "对方回答“老样子”，我就知道" + SelfName + "的同事或上级又发飙了";
        else
            content = content = "对方似乎不想讲，" + SelfName + "看起来有点逞强，我也不好说什么";
        return content;
    }
}
public class Event71 : Event
{
    public Event71() : base()
    {
        EventName = "挚友日常C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5 ";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5 

        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "约我下班了去喝酒，对方说最近某个岛上新开了个不错的";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "默默地陪我做了一会儿，并拍拍我的肩膀";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "我主动约" + SelfName + "下班后一起玩耍，帮助对方缓解下心情";
        else
            content = content = "帮不上什么忙，我只好陪" + SelfName + "坐一会儿";
        return content;
    }
}
public class Event72 : Event
{
    public Event72() : base()
    {
        EventName = "挚友日常D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方获得情绪“苦涩”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方获得情绪“苦涩”×1
        emp.AddTempEmotion(EColor.LBlue);
        target.AddTempEmotion(EColor.LBlue);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event73().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "发了几个搞笑的meme过来，我也笑了起来";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "转身离开了，我并没有挽留对方";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "帮不上什么忙，我只好给" + SelfName + "发几个表情包开心开心";
        else
            content = content = "就让" + SelfName + "一个人静静好了，我转身离开了";
        return content;
    }
}
public class Event73 : Event
{
    public Event73() : base()
    {
        EventName = "挚友日常E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -10";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -10
        emp.AddTempRelation(target, -10);
        target.AddTempRelation(emp, -10);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我喊住了正在离开的" + TargetName + "，向对方分享自己日常里的各种事情";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我喊住正在离开的" + TargetName + "，对方却说自己有事要做";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "把我叫住了，我听对方讲完了最近发生的日常";
        else
            content = content = "" + SelfName + "把我叫住，由于有事要忙，我拒绝了对方的聊天请求";
        return content;
    }
}
public class Event74 : Event
{
    public Event74() : base()
    {
        EventName = "挚友安慰A";
        FriendRequire = 2;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.寻求安慰;
        SuccessDescription = " /";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event75().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event77().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到挚友" + TargetName + "，由于工作中的痛苦向对方请求帮助";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到挚友" + TargetName + "，由于工作中的痛苦向对方请求帮助";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到挚友" + SelfName + "，对方似乎看起来很痛苦";
        else
            content = content = "在" + placeName + "见到挚友" + SelfName + "，对方似乎遇到了一些困难";
        return content;
    }
}
public class Event75 : Event
{
    public Event75() : base()
    {
        EventName = "挚友安慰B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event76().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "向" + TargetName + "坦诚了最近我造成的许多失误，对方安慰我放轻松";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "向" + TargetName + "说了一些最近我们部门的许多失误，但是责任不在我身上";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "向我坦诚地说了犯的错误，我安慰对方先放松下来";
        else
            content = content = "我认为" + SelfName + "在推卸对方在公司里的责任，未坦陈其失误";
        return content;
    }
}
public class Event76 : Event
{
    public Event76() : base()
    {
        EventName = "挚友安慰C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5 ";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "坚定地看着我，安慰我一切都会好起来的，对方很信任我";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "给我讲了个其它朋友的故事，我没有太多感觉";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "受到了挫折，但是我坚信对方能够克服";
        else
            content = content = "试着与" + SelfName + "分享其他朋友的类似经历，对方似乎没反应";
        return content;
    }
}
public class Event77 : Event
{
    public Event77() : base()
    {
        EventName = "挚友安慰D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方获得情绪“厌恶”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方获得情绪“厌恶”×1
        emp.AddTempEmotion(EColor.LRed);
        target.AddTempEmotion(EColor.LRed);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event78().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "表示没关系的，睡一觉就好了";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "表示我的事都不算什么，对方经历过更严重的";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "希望" + SelfName + "能够把这些事忘了，不用这么难受";
        else
            content = content = "试图让" + SelfName + "不要在意自己的问题，但是对方却对我发脾气";
        return content;
    }
}
public class Event78 : Event
{
    public Event78() : base()
    {
        EventName = "挚友安慰E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -10";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -10
        emp.AddTempRelation(target, -10);
        target.AddTempRelation(emp, -10);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我向" + TargetName + "表示：算了，我自己安静一下吧。";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我无法忍受" + TargetName + "轻描淡写的评判我的痛苦，转身离去";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "希望自己安静下，我只好去忙自己的事";
        else
            content = content = "" + SelfName + "似乎对我有很大意见，我真觉得莫名其妙";
        return content;
    }
}
public class Event79 : Event
{
    public Event79() : base()
    {
        EventName = "挚友认可A";
        FriendRequire = 2;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.认可交谈;
        SuccessDescription = " /";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event80().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event82().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到挚友" + TargetName + "，询问" + TargetName + "自己是否适合当前工作";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到挚友" + TargetName + "，询问" + TargetName + "自己是否适合当前工作";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到挚友" + SelfName + "，对方怀疑其能力是否胜任工作";
        else
            content = content = "在" + placeName + "见到挚友" + SelfName + "，对方看起来缺少方向";
        return content;
    }
}
public class Event80 : Event
{
    public Event80() : base()
    {
        EventName = "挚友认可B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5, 双方获得情绪“愉悦”×1";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5, 双方获得情绪“愉悦”×1
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);
        emp.AddTempEmotion(EColor.LYellow);
        target.AddTempEmotion(EColor.LYellow);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event81().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "表示当然没问题，只要我有动力去做的话就没问题";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "表示努力去做就没问题，相信我的能力";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "相信" + SelfName + "只要愿意就能做到，对方若有所思";
        else
            content = content = "向" + SelfName + "表达了无条件的信任，对方也点点头";
        return content;
    }
}
public class Event81 : Event
{
    public Event81() : base()
    {
        EventName = "挚友认可C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5 ";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5 
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我似乎找到问题关键在于缺乏动力，对" + TargetName + "表达了感谢";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我觉得还是自己的能力不够，" + TargetName + "只是沉默地拍了拍我的肩膀";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "似乎发现自己缺少工作动力，并且对我表示感谢";
        else
            content = content = "" + SelfName + "似乎依然认为自己能力不足，我只好拍拍对方的肩膀";
        return content;
    }
}
public class Event82 : Event
{
    public Event82() : base()
    {
        EventName = "挚友认可D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription =  "获得情绪“厌恶”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"" + SelfName + "获得情绪“厌恶”×1
        emp.AddTempEmotion(EColor.LRed);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event83().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "陪我坐了一会儿，只说了一句：加油吧！";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "认为到现在我还是犹犹豫豫，这样很不好，赶紧振作起来";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "陪" + SelfName + "沉默地坐了一会儿，并让对方加油";
        else
            content = content = "向" + SelfName + "表达了不要怀疑自己，这样子什么都做不到";
        return content;
    }
}
public class Event83 : Event
{
    public Event83() : base()
    {
        EventName = "挚友认可E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -10";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -10
        emp.AddTempRelation(target, -10);
        target.AddTempRelation(emp, -10);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "表示理解" + TargetName + "的意思，但是更需要别人的认可和理解";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "指责" + TargetName + "在居高临下的教我做事，这样帮不上什么忙";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "对" + SelfName + "表示愿意理解对方，也希望对方重新振作";
        else
            content = content = "" + SelfName + "认为我教不了对方什么，我只能表示无奈";
        return content;
    }
}
public class Event84 : Event
{
    public Event84() : base()
    {
        EventName = "挚友深刻A";
        FriendRequire = 2;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.深刻交谈;
        SuccessDescription = " /";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event85().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event87().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到挚友" + TargetName + "，对方主动询问我最近有什么收获";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到挚友" + TargetName + "，对方主动询问我最近有什么收获";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到挚友" + SelfName + "，对方看起来很有干劲";
        else
            content = content = "在" + placeName + "见到挚友" + SelfName + "，对方看起来十分得意";
        return content;
    }
}
public class Event85 : Event
{
    public Event85() : base()
    {
        EventName = "挚友深刻B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5, 双方获得情绪“愉悦”×1";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5 , 双方获得情绪“愉悦”×1
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);
        emp.AddTempEmotion(EColor.LYellow);
        target.AddTempEmotion(EColor.LYellow);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event86().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "向" + TargetName + "分享了一些最近有趣的经验，对方称赞了我的成长";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "随便向" + TargetName + "讲了讲最近做的事情，对方只是点头";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "向我分享了一些进展，能够明显感受到对方的成长";
        else
            content = content = "" + SelfName + "随意地聊聊最近做的事情，我点头附和对方";
        return content;
    }
}
public class Event86 : Event
{
    public Event86() : base()
    {
        EventName = "挚友深刻C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +10 ";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +10 
        emp.AddTempRelation(target, +10);
        target.AddTempRelation(emp, +10);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "从我的想法里引申出更多值得思考的问题，彼此聊得很开心";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "说了些想法，我也肯定了对方的思考";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "从" + SelfName + "的想法中发现更多值得探讨的问题，聊的很愉快";
        else
            content = content = "向" + SelfName + "反馈了自己的想法，也获得了对方的认可";
        return content;
    }
}
public class Event87 : Event
{
    public Event87() : base()
    {
        EventName = "挚友深刻D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event88().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "向" + TargetName + "分享了一些我最近的成功经验，对方也点点头";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "向" + TargetName + "分享了一些我最近的成功经验，对方却希望我低调";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "似乎太看重自己做到的事情，但是又不方便提醒对方";
        else
            content = content = "" + SelfName + "似乎太看重自己做的事情，于是我提醒对方应该谦虚";
        return content;
    }
}
public class Event88 : Event
{
    public Event88() : base()
    {
        EventName = "挚友深刻E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -10";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -10
        emp.AddTempRelation(target, -10);
        target.AddTempRelation(emp, -10);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "对" + TargetName + "的提醒感到震撼，我决定收敛一些";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "似乎有些嫉妒我的成就，于是我劝对方多努力";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "表示我的建议很有道理";
        else
            content = content = "" + SelfName + "傲慢地劝我向对方看齐，我后悔提建议了";
        return content;
    }
}
public class Event89 : Event
{
    public Event89() : base()
    {
        EventName = "挚友乐事A";
        FriendRequire = 2;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.分享乐事;
        SuccessDescription = " /";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event90().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event92().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到挚友" + TargetName + "，对方主动问我最近有什么开心事";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到挚友" + TargetName + "，对方主动问我最近有什么开心事";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到挚友" + SelfName + "，主动问对方有什么开心事";
        else
            content = content = "在" + placeName + "见到挚友" + SelfName + "，对方很开心，我于是问问原因";
        return content;
    }
}
public class Event90 : Event
{
    public Event90() : base()
    {
        EventName = "挚友乐事B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event91().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我向" + TargetName + "讲述了最近开心的事，对方想起了一些有趣的回忆";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我向" + TargetName + "讲述了最近开心的事，对方也有些高兴";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "的快乐使我想起了一些从前开心的事";
        else
            content = content = "" + SelfName + "说了很多有趣的事情，真不错";
        return content;
    }
}
public class Event91 : Event
{
    public Event91() : base()
    {
        EventName = "挚友乐事C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5 , 双方获得情绪“愉悦”×1";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5 , 双方获得情绪“愉悦”×1
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);
        emp.AddTempEmotion(EColor.LYellow);
        target.AddTempEmotion(EColor.LYellow);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "和" + TargetName + "说起了大洪水之前彼此童年时的趣事，有些感动";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "感慨那个时代已经过去了，我劝对方抓住今天就好";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在和" + SelfName + "聊到小时候的趣事时我不小心落泪，真令人怀念";
        else
            content = content = "我感慨美好时代已逝去，" + SelfName + "劝我抓住今天就好";
        return content;
    }
}
public class Event92 : Event
{
    public Event92() : base()
    {
        EventName = "挚友乐事D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = "获得情绪“苦涩”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"" + TargetName + "获得情绪“苦涩”×1
        emp.AddTempEmotion(EColor.LBlue);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event93().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我说了几件事，见" + TargetName + "看起来比较累，就终止了聊天";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我一直说个不停，仿佛忘记了" + TargetName + "，忘记了时间";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "发现我比较累，于是就各自去休息了";
        else
            content = content = "" + SelfName + "说个不停，我只好疲惫地听着";
        return content;
    }
}
public class Event93 : Event
{
    public Event93() : base()
    {
        EventName = "挚友乐事E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -10";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -10
        emp.AddTempRelation(target, -10);
        target.AddTempRelation(emp, -10);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "询问" + TargetName + "有什么好玩的事，但是" + TargetName + "似乎不太想说话";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我又想到了一些事情，又讲了一个小时.. 才发现" + TargetName + "已经不见了";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "问我有什么开心的事情，疲惫的我只是摇摇头";
        else
            content = content = "" + SelfName + "的精力无处宣泄又讲了半小时，我趁其不备就溜了";
        return content;
    }
}
public class Event94 : Event
{
    public Event94() : base()
    {
        EventName = "陌路日常A";
        FriendRequire = -1;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.分享日常;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event95().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event97().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到陌路" + TargetName + "，开了几句对方的玩笑";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到陌路" + TargetName + "，开了几句对方的玩笑";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到陌路" + SelfName + "，对方开了个玩笑，竟意外幽默";
        else
            content = content = "在" + placeName + "见到陌路" + SelfName + "，对方在嘲笑我";
        return content;
    }
}
public class Event95 : Event
{
    public Event95() : base()
    {
        EventName = "陌路日常B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event96().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我提出" + TargetName + "应该多说话，没必要总是板着脸，仿佛有人欠钱没还";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "然后开始模仿" + TargetName + "的腔调说话，对方觉得有些尴尬";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "认为我多说话或许会更招人喜欢";
        else
            content = content = "" + SelfName + "模仿我的方式很拙劣，建议提升演技";
        return content;
    }
}
public class Event96 : Event
{
    public Event96() : base()
    {
        EventName = "陌路日常C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +10 ";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +10 
        emp.AddTempRelation(target, +10);
        target.AddTempRelation(emp, +10);


        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "认为是因为我太烦人了，所以对方才板着脸，我承认是这样";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "调侃" + TargetName + "，不过即便你多说话，大家倒是也不会更喜欢你";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "提出" + SelfName + "总是说个没完，所以我才会严肃，对方也认同了";
        else
            content = content = "" + SelfName + "跟我开了个无聊的玩笑，以图拉近关系";
        return content;
    }
}
public class Event97 : Event
{
    public Event97() : base()
    {
        EventName = "陌路日常D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5, 双方获得情绪“厌恶”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5, 双方获得情绪“厌恶”×1
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);
        emp.AddTempEmotion(EColor.LRed);
        target.AddTempEmotion(EColor.LRed);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event98().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "说完我转身就走了，只把背影留给" + TargetName + "";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "模仿" + TargetName + "和老板说话的语气，嘲笑对方太怂了";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "还算知趣，转身逃跑了，否则对方一定会被羞辱";
        else
            content = content = "" + SelfName + "只会进行幼稚的表演，我打算回击对方";
        return content;
    }
}
public class Event98 : Event
{
    public Event98() : base()
    {
        EventName = "陌路日常E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5, 获得情绪“厌恶”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5, 获得情绪“厌恶”×1
        
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);
        emp.AddTempEmotion(EColor.LRed);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "向" + TargetName + "解释自己是开玩笑的，老板面前都是怂货";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "讽刺我是刻薄幼稚的蠢货，我只承认自己有时候很刻薄";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "的道歉还算及时，于是我不予追究，并表示下不为例";
        else
            content = content = "一针见血地指出" + SelfName + "刻薄幼稚又愚蠢的本质，对方哑口无言";
        return content;
    }
}
public class Event99 : Event
{
    public Event99() : base()
    {
        EventName = "陌路安慰A";
        FriendRequire = -1;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.寻求安慰;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event100().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event102().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到陌路" + TargetName + "，对方说我看起来很倒霉";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到陌路" + TargetName + "，对方询问我又犯错了吗？";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到陌路" + SelfName + "，对方看起来很倒霉";
        else
            content = content = "在" + placeName + "见到陌路" + SelfName + "，很高兴对方似乎又犯错了";
        return content;
    }
}
public class Event100 : Event
{
    public Event100() : base()
    {
        EventName = "陌路安慰B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +10";
        FailDescription = "获得情绪“苦涩”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +10
        emp.AddTempRelation(target, +10);
        target.AddTempRelation(emp, +10);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event101().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"" + SelfName + "获得情绪“苦涩”×1
        emp.AddTempEmotion(EColor.LBlue);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "自顾自地说，工作中的事，不能当真的";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "表示犯蠢会被开除的，我只好沉默";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "认为" + SelfName + "因为工作的事不高兴实在是太不值得了";
        else
            content = content = "提醒" + SelfName + "犯错太多早晚会被开除";
        return content;
    }
}
public class Event101 : Event
{
    public Event101() : base()
    {
        EventName = "陌路安慰C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +10 ";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +10 
        emp.AddTempRelation(target, +10);
        target.AddTempRelation(emp, +10);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "质问" + TargetName + "那么什么更重要，对方回答：你的生活";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "回应" + TargetName + "可不是每个人都这么不切实际的，你的食物哪儿来的？";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "追问什么比工作重要？我回答：每一天的生活";
        else
            content = content = "被" + SelfName + "认为我不切实际，看清了工作的意义";
        return content;
    }
}
public class Event102 : Event
{
    public Event102() : base()
    {
        EventName = "陌路安慰D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5,获得情绪“厌恶”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5,获得情绪“厌恶”×1
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);
        emp.AddTempEmotion(EColor.LRed);
        target.AddTempEmotion(EColor.LRed);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event103().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我对" + TargetName + "说：和你一样，每天都是废物";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "指出我本来就是那种会不停犯相同错误的人，无药可救";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "说我也是废物，我回应：看你的模样，倒霉的是你吧";
        else
            content = content = "指出" + SelfName + "是犯重复错误的无药可救的人";
        return content;
    }
}
public class Event103 : Event
{
    public Event103() : base()
    {
        EventName = "陌路安慰E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5, 双方获得情绪“厌恶”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5, 双方获得情绪“厌恶”×1
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);
        emp.AddTempEmotion(EColor.LRed);
        target.AddTempEmotion(EColor.LRed);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "向" + TargetName + "指出：你的话伤害不了我，再练练吧";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "指出" + TargetName + "这种混账说的话只能反过来理解";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "认为我的话无法影响对方，我知道对方已受到影响";
        else
            content = content = "" + SelfName + "指出我说的话只能反着理解，看来对方生气了，哈哈";
        return content;
    }
}
public class Event104 : Event
{
    public Event104() : base()
    {
        EventName = "陌路认可A";
        FriendRequire = -1;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.认可交谈;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event105().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event107().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到陌路" + TargetName + "，对方表示我的工作能力太差了";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到陌路" + TargetName + "，对方表示老板雇我是在浪费时间";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到陌路" + SelfName + "，对方的工作效率太低了";
        else
            content = content = "在" + placeName + "见到陌路" + SelfName + "，对方的工作效率太低了";
        return content;
    }
}
public class Event105 : Event
{
    public Event105() : base()
    {
        EventName = "陌路认可B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event106().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "问我：能不能认真点，应该能做到吧？";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我回应：关你什么事，对方转身离开了";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "询问" + SelfName + "能不能认真完成手头的简单工作";
        else
            content = content = "我无法忍受" + SelfName + "这样低效率的负面典型，转身回去工作了";
        return content;
    }
}
public class Event106 : Event
{
    public Event106() : base()
    {
        EventName = "陌路认可C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +15 ";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +15 
        emp.AddTempRelation(target, +15);
        target.AddTempRelation(emp, +15);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我叹了口气表示不能，" + TargetName + "上手用pad给我演示了做法";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我表示，如果没有" + TargetName + "耽误我的时间当然可以做到";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "弱到看不过去了，于是我上手演示了一下怎么做";
        else
            content = content = "" + SelfName + "逞强地认为自己能完成，实际上对方根本做不到";
        return content;
    }
}
public class Event107 : Event
{
    public Event107() : base()
    {
        EventName = "陌路认可D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5, 双方获得情绪“厌恶”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5,双方获得情绪“厌恶”×1
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);
        emp.AddTempEmotion(EColor.LRed);
        target.AddTempEmotion(EColor.LRed);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event108().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "反驳" + TargetName + "：老板不也雇了你吗？看来浪费时间是老板的爱好";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "指出我是那种脆弱的需要别人认可，却注定得不到的人";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "回击我，表示老板雇我也是浪费时间";
        else
            content = content = "向" + SelfName + "指出，对方的本质就是一个脆弱又自我的混蛋";
        return content;
    }
}
public class Event108 : Event
{
    public Event108() : base()
    {
        EventName = "陌路认可E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5, 双方获得情绪“厌恶”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5, 双方获得情绪“厌恶”×1
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);
        emp.AddTempEmotion(EColor.LRed);
        target.AddTempEmotion(EColor.LRed);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "向" + TargetName + "回应：那又怎样，不认可我的人多了你算老几";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "向" + TargetName + "大吼：你给我闭嘴";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "承认了自己就是那样，看来对方有点自知之明";
        else
            content = content = "" + SelfName + "被戳中了内心，因此冲我大吼";
        return content;
    }
}
public class Event109 : Event
{
    public Event109() : base()
    {
        EventName = "陌路深刻A";
        FriendRequire = -1;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.深刻交谈;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event110().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event112().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到陌路" + TargetName + "，遭到对方嘲讽我近来取得的成果";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到陌路" + TargetName + "，对方嘲讽我的做事方法本质上是谬误";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到陌路" + SelfName + "，对方貌似颇为得意";
        else
            content = content = "在" + placeName + "见到陌路" + SelfName + "，对方十分张狂，于是我出言相讥";
        return content;
    }
}
public class Event110 : Event
{
    public Event110() : base()
    {
        EventName = "陌路深刻B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +10, 双方获得情绪“愉悦”×1";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +10, 双方获得情绪“愉悦”×1
        emp.AddTempRelation(target, +10);
        target.AddTempRelation(emp, +10);
        emp.AddTempEmotion(EColor.LYellow);
        target.AddTempEmotion(EColor.LYellow);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event111().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我直接指出了" + TargetName + "如果提升哪些环节，就可以效率陡增";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "嘲笑" + TargetName + "是不是很羡慕自己，对方轻蔑地看了我一眼";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "向我提出了一些工作建议，必许承认对方说的有道理";
        else
            content = content = "" + SelfName + "竟然认为我羡慕对方的成就，我表示了不屑便离开了";
        return content;
    }
}
public class Event111 : Event
{
    public Event111() : base()
    {
        EventName = "陌路深刻C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +10 ";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +10 
        emp.AddTempRelation(target, +10);
        target.AddTempRelation(emp, +10);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "竟然进一步咨询我还有哪些技巧，我不徐不疾地讲解了一下";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "表面上说我的建议不过如此，实际上却很佩服";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "将所有技巧倾囊相授，对方的坦诚让我感到意外";
        else
            content = content = "认为" + SelfName + "没有本事，不过也就是最近有点进步而已";
        return content;
    }
}
public class Event112 : Event
{
    public Event112() : base()
    {
        EventName = "陌路深刻D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5,己方获得情绪“苦涩”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5 获得情绪“苦涩”×1
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);
        emp.AddTempEmotion(EColor.LBlue);
        target.AddTempEmotion(EColor.LBlue);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event113().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我反驳" + TargetName + "的工作方法也不见得哪里高明";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "被" + TargetName + "指出了我工作方法中的漏洞，一时间不知如何回应";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "认为我的工作方法并没有比对方的高明";
        else
            content = content = "指出" + SelfName + "工作方法中的漏洞，对方哑口无言";
        return content;
    }
}
public class Event113 : Event
{
    public Event113() : base()
    {
        EventName = "陌路深刻E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5, 双方获得情绪“苦涩”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5 双方获得情绪“苦涩”×1
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);
        emp.AddTempEmotion(EColor.LBlue);
        target.AddTempEmotion(EColor.LBlue);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "思考一番后，我承认" + TargetName + "说到有道理，这样我也获得了成长";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "强硬地指出" + TargetName + "没有做出什么成果，因此不配与我相提并论";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "承认了我的洞察力，自认甘拜下风";
        else
            content = content = "" + SelfName + "认为我不配指责对方，我则指出对方不讲道理狗急跳墙";
        return content;
    }
}
public class Event114 : Event
{
    public Event114() : base()
    {
        EventName = "陌路乐事A";
        FriendRequire = -1;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.分享乐事;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event115().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event117().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到陌路" + TargetName + "，对方问我在傻乐什么";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到陌路" + TargetName + "，对方说我笑的脸抽筋了";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到陌路" + SelfName + "，对方的笑声很刺耳";
        else
            content = content = "在" + placeName + "见到陌路" + SelfName + "，我看到对方开心就很不开心";
        return content;
    }
}
public class Event115 : Event
{
    public Event115() : base()
    {
        EventName = "陌路乐事B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +5";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +5
        emp.AddTempRelation(target, +5);
        target.AddTempRelation(emp, +5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event116().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我说起最近开心的事，" + TargetName + "追问然后呢？";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我说其最近开心的事，" + TargetName + "表示就这？";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "说的事情有点意思，所以我追问后来的进展";
        else
            content = content = "" + SelfName + "说的事情很无聊，全是些经历：购物、开会什么的";
        return content;
    }
}
public class Event116 : Event
{
    public Event116() : base()
    {
        EventName = "陌路乐事C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +10 双方获得情绪“愉悦”×1";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +10  双方获得情绪“愉悦”×1
        emp.AddTempRelation(target, +10);
        target.AddTempRelation(emp, +10);
        emp.AddTempEmotion(EColor.LYellow);
        target.AddTempEmotion(EColor.LYellow);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "等我说完，" + TargetName + "忍不住笑了出来";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "等我说完，" + TargetName + "嘲笑了我讲故事的能力";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "确实很有意思，我没绷住“噗嗤”乐了出来";
        else
            content = content = "等" + SelfName + "说完，我表示对方讲故事能力太差了";
        return content;
    }
}
public class Event117 : Event
{
    public Event117() : base()
    {
        EventName = "陌路乐事D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5，己方获得情绪“苦涩”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5 获得情绪“苦涩”×1
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);
        emp.AddTempEmotion(EColor.LBlue);
        target.AddTempEmotion(EColor.LBlue);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event118().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我指出" + TargetName + "不应该上班的时候乱逛，对方却说是我在乱转";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "表示不要高兴的太早，否则难受时会格外伤心";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "认为我不应在上班时乱转，明明是对方跑到我这里的";
        else
            content = content = "我讽刺" + SelfName + "是个情绪化的人，所以小心笑着笑着哭起来";
        return content;
    }
}
public class Event118 : Event
{
    public Event118() : base()
    {
        EventName = "陌路乐事E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5，双方获得情绪“苦涩”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5 双方获得情绪“苦涩”×1
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);
        emp.AddTempEmotion(EColor.LBlue);
        target.AddTempEmotion(EColor.LBlue);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "对方表示自己先走了，不想看到我的脸，我真讨厌对方" + TargetName + "";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我指出对方是那种看不惯别人开心，心胸狭隘的人，被" + TargetName + "怒斥";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "对" + SelfName + "表示我先走了，不打扰了";
        else
            content = content = "对方认为我见不得别人好，我怒斥对方狗嘴吐不出象牙";
        return content;
    }
}
public class Event119 : Event
{
    public Event119() : base()
    {
        EventName = "仇人日常A";
        FriendRequire = -2;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.分享日常;
        SuccessDescription = " /";
        FailDescription = " 双方获得情绪“苦涩”x1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event120().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方获得情绪“苦涩”x1
        emp.AddTempEmotion(EColor.LBlue);
        target.AddTempEmotion(EColor.LBlue);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event122().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到仇人" + TargetName + "，感觉对方打扰了自己的兴致";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到仇人" + TargetName + "，察觉对方正在打量自己";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到仇人" + TargetName + "，感到遇见对方真是倒霉";
        else
            content = content = "在" + placeName + "见到仇人" + SelfName + "，察觉对方正在打量自己";
        return content;
    }
}
public class Event120 : Event
{
    public Event120() : base()
    {
        EventName = "仇人日常B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event121().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我假装没看到" + TargetName + "，继续忙自己的事情";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我和" + TargetName + "各自转身离开了，仿佛没看到对方";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "我继续忙自己的事情，无视" + SelfName + "这个人的存在";
        else
            content = content = "我和" + SelfName + "各自转身离开了，仿佛没看到对方";
        return content;
    }
}
public class Event121 : Event
{
    public Event121() : base()
    {
        EventName = "仇人日常C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +10 ";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +10 
        emp.AddTempRelation(target, +10);
        target.AddTempRelation(emp, +10);


        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我和其他同事吐槽一个上级，" + TargetName + "听到后转头过来表达共鸣";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "没有和" + TargetName + "发生冲突，我心里感到一些轻松";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "提出" + SelfName + "总是说个没完，所以我才会严肃，对方也认同了";
        else
            content = content = "看着" + SelfName + "远去，我心想今天放这个蠢货一马";
        return content;
    }
}
public class Event122 : Event
{
    public Event122() : base()
    {
        EventName = "仇人日常D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5，双方获得情绪“厌恶“x1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5, 双方获得情绪“厌恶“x1
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);
        emp.AddTempEmotion(EColor.LRed);
        target.AddTempEmotion(EColor.LRed);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event123().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我转身离开了，没有和" + TargetName + "说一句话";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我对" + TargetName + "说：你看什么啊，好好干你的活吧";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "我转身离开了，没有和" + SelfName + "说一句话";
        else
            content = content = "" + SelfName + "对我无礼咒骂，我准备反击这个无能的人";
        return content;
    }
}
public class Event123 : Event
{
    public Event123() : base()
    {
        EventName = "仇人日常E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "向" + TargetName + "解释自己是开玩笑的，老板面前都是怂货";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "嘲讽我说，想看我什么时候被开除，我回应：在你之后";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "的道歉还算及时，于是我不予追究，并表示下不为例";
        else
            content = content = "我提出想看" + SelfName + "什么时候会被扔下船，对方气红了脸";
        return content;
    }
}
public class Event124 : Event
{
    public Event124() : base()
    {
        EventName = "仇人安慰A";
        FriendRequire = -2;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.寻求安慰;
        SuccessDescription = " /";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event125().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event127().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到仇人" + TargetName + "，不小心被对方看到状态不好的时候";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到仇人" + TargetName + "，不小心被对方看到状态不好的时候";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到仇人" + SelfName + "，对方看起来失魂落魄";
        else
            content = content = "在" + placeName + "见到仇人" + SelfName + "，对方看起来刚经历惨败";
        return content;
    }
}
public class Event125 : Event
{
    public Event125() : base()
    {
        EventName = "仇人安慰B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方获得情绪“苦涩”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event126().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方获得情绪“苦涩”×1
        emp.AddTempEmotion(EColor.LBlue);
        target.AddTempEmotion(EColor.LBlue);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我对" + TargetName + "说，让对方离远点，对方站在那里不动";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我要求" + TargetName + "离我远点，对方嘲讽我太晦气然后离开了";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "让我走开，我却打算好好欣赏下对方的惨状";
        else
            content = content = "" + SelfName + "让我走开，我表示不愿意和倒霉的人待在一块儿";
        return content;
    }
}
public class Event126 : Event
{
    public Event126() : base()
    {
        EventName = "仇人安慰C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +10 ";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +10 
        emp.AddTempRelation(target, +10);
        target.AddTempRelation(emp, +10);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "说：如果我是你，绝不会在讨厌的人面前，表现地像个孬种";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我对" + TargetName + "竖了中指，然后对方离开了";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "我对" + SelfName + "说：别让我可怜你，否则你就太失败了";
        else
            content = content = "那个蠢货对我竖中指，" + SelfName + "果然是这种脆弱的垃圾";
        return content;
    }
}
public class Event127 : Event
{
    public Event127() : base()
    {
        EventName = "仇人安慰D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5，双方获得情绪“厌恶”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5，双方获得情绪“厌恶”×1
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);
        emp.AddTempEmotion(EColor.LRed);
        target.AddTempEmotion(EColor.LRed);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event128().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "建议我辞职，我无视了对方";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "称，建议我转到对方所在部门，这样方便教我做事";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "我建议" + SelfName + "直接辞职，对方无视了我";
        else
            content = content = "我建议" + SelfName + "直接到我手下工作，这样起码不至于浪费时间";
        return content;
    }
}
public class Event128 : Event
{
    public Event128() : base()
    {
        EventName = "仇人安慰E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -10，双方获得情绪“厌恶”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -10，双方获得情绪“厌恶”×1
        emp.AddTempRelation(target, -10);
        target.AddTempRelation(emp, -10);
        emp.AddTempEmotion(EColor.LRed);
        target.AddTempEmotion(EColor.LRed);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "向" + TargetName + "指出：如果你下个月还没被开除，我会考虑的";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我对" + TargetName + "回应，还是我来先教教你怎么做人吧，混蛋";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "说先小心我自己被开除，真是逞强的蠢货";
        else
            content = content = "" + SelfName + "表示要教我做人，真是个无可救药的蠢货";
        return content;
    }
}
public class Event129 : Event
{
    public Event129() : base()
    {
        EventName = "仇人认可";
        FriendRequire = -2;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.认可交谈;
        SuccessDescription = " /";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event130().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event132().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到仇人" + TargetName + "，对方正在一脸戏谑地看着我";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到仇人" + TargetName + "，对方正在一脸戏谑地看着我";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到仇人" + SelfName + "，对方看起来异常纠结";
        else
            content = content = "在" + placeName + "见到仇人" + SelfName + "，对方迷茫的样子好像第一天上班";
        return content;
    }
}
public class Event130 : Event
{
    public Event130() : base()
    {
        EventName = "仇人认可B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event131().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "问我是不是分不清左右手了";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我注意到" + TargetName + "的眼神，转身离开了";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "我问" + SelfName + "是不是分不清左右手了";
        else
            content = content = "" + SelfName + "对我感到恐惧，于是逃走了";
        return content;
    }
}
public class Event131 : Event
{
    public Event131() : base()
    {
        EventName = "仇人认可C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +10";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +10
        emp.AddTempRelation(target, +10);
        target.AddTempRelation(emp, +10);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "说了句：别让我瞧不起你，转身离开了";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我对" + TargetName + "说：对，要不你跪下帮我系鞋带吧";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "的样子配不上我讨厌，于是我告诫对方振作点";
        else
            content = content = "" + SelfName + "承认了自己是个低能儿";
        return content;
    }
}
public class Event132 : Event
{
    public Event132() : base()
    {
        EventName = "仇人认可D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5，双方获得情绪“厌恶”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5，双方获得情绪“厌恶”×1
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);
        emp.AddTempEmotion(EColor.LRed);
        target.AddTempEmotion(EColor.LRed);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event133().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我斥责" + TargetName + "：回去干你的活，对方离开了";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "走过来说：你是我见过最无能的人";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "我没有兴致和" + SelfName + "闲聊，直接转身离开了";
        else
            content = content = "我走向" + SelfName + "并指出：不明白为什么有你这么脑残的人";
        return content;
    }
}
public class Event133 : Event
{
    public Event133() : base()
    {
        EventName = "仇人认可E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5，双方获得情绪“厌恶”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5,双方获得情绪“厌恶”×1
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);
        emp.AddTempEmotion(EColor.LRed);
        target.AddTempEmotion(EColor.LRed);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "向" + TargetName + "回应：彼此彼此";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我立刻向" + TargetName + "挥舞了一拳，将对方吓了一跳";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "被我的气势震慑住了";
        else
            content = content = "" + SelfName + "被戳中了内心因此攻击我，但是被我灵巧躲过";
        return content;
    }
}
public class Event134 : Event
{
    public Event134() : base()
    {
        EventName = "仇人深刻A";
        FriendRequire = -2;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.深刻交谈;
        SuccessDescription = " /";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event135().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event137().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到仇人" + TargetName + "，正好当时我在和别的同事分享经验";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到仇人" + TargetName + "，正好当时我在和别的同事分享经验";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到仇人" + SelfName + "，对方正跟别人介绍“成功经验”";
        else
            content = content = "在" + placeName + "见到仇人" + SelfName + "，对方正唾沫横飞地跟别人吹牛";
        return content;
    }
}
public class Event135 : Event
{
    public Event135() : base()
    {
        EventName = "仇人深刻B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方获得情绪“苦涩”x1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event136().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方获得情绪“苦涩”x1
        emp.AddTempEmotion(EColor.LBlue);
        target.AddTempEmotion(EColor.LBlue);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "说到某个工作细节我忘记了，" + TargetName + "竟然接上了我要讲的内容";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "问我是否忘记前段时间谁哭丧着脸，我表示对方没资格评判";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "说到一半忘词了，我就接上了对方要分享的内容";
        else
            content = content = "我表示不久前" + SelfName + "一副惨兮兮的样子，现在真以为自己成功了？";
        return content;
    }
}
public class Event136 : Event
{
    public Event136() : base()
    {
        EventName = "仇人深刻C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +10 ";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +10 
        emp.AddTempRelation(target, +10);
        target.AddTempRelation(emp, +10);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "一步步娓娓道来，竟然和我交流了起来，令人意外";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我转身离开了，只留" + TargetName + "继续在那里吹牛";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "我不自觉地把自己的想法讲了，并意外地和" + SelfName + "交流起来";
        else
            content = content = "" + SelfName + "嫉妒我的见识和能力，于是识趣地离开了";
        return content;
    }
}
public class Event137 : Event
{
    public Event137() : base()
    {
        EventName = "仇人深刻D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event138().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "认为我在吹牛，我直接无视了对方继续我的讨论";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "认为我在误人子弟，我表示对方什么都不懂";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "我直接指出" + SelfName + "在吹牛，对方只好假装没听见";
        else
            content = content = "我一针见血地奉劝" + SelfName + "不要误人子弟，但对方不为所动";
        return content;
    }
}
public class Event138 : Event
{
    public Event138() : base()
    {
        EventName = "仇人深刻E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "不敢与我争辩，只好溜走了";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "我与" + TargetName + "激辩一阵之后，发现刚才的同事都走了";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "继续自说自话，我没必要和顽石较劲，所以离开了";
        else
            content = content = "虽然我据理力争，但是" + SelfName + "骂街的态度使周围同事都走了";
        return content;
    }
}
public class Event139 : Event
{
    public Event139() : base()
    {
        EventName = "仇人乐事A";
        FriendRequire = -2;
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.分享乐事;
        SuccessDescription = " /";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event140().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event142().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "在" + placeName + "见到仇人" + TargetName + "，我故意提高笑声让对方听见";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "在" + placeName + "见到仇人" + TargetName + "，我故意提高笑声让对方听见";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "在" + placeName + "见到仇人" + SelfName + "，对方高兴的样子很蠢";
        else
            content = content = "在" + placeName + "见到仇人" + SelfName + "，对方高兴的样子很讨厌";
        return content;
    }
}
public class Event140 : Event
{
    public Event140() : base()
    {
        EventName = "仇人乐事B";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
        new Event141().StartEvent(emp, 0, target);
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "我把买来的食物分给周围的同事，顺手递给" + TargetName + "一块";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "跟我说小点声，我稍微降低了音量";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "" + SelfName + "高兴地把买来的食物分给周围同事，也给了我一份";
        else
            content = content = "我要求" + SelfName + "说话声音小点儿，对方声音小了些";
        return content;
    }
}
public class Event141 : Event
{
    public Event141() : base()
    {
        EventName = "仇人乐事C";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " 双方好感度   +10";
        FailDescription = " /";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"双方好感度   +10
        emp.AddTempRelation(target, +10);
        target.AddTempRelation(emp, +10);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "接过去吃了，也听我聊了会儿开心的事";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "不识好歹地拒绝了我，并转身离开";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "我接过" + SelfName + "给的食物，然后听对方聊了会儿";
        else
            content = content = "我拒绝了" + SelfName + "的嗟来之食，转身离开了";
        return content;
    }
}
public class Event142 : Event
{
    public Event142() : base()
    {
        EventName = "仇人乐事D";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5，对方获得情绪“苦涩”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5，获得情绪“苦涩”×1
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);
        emp.AddTempEmotion(EColor.LBlue);
        target.AddTempEmotion(EColor.LBlue);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
        new Event143().StartEvent(emp, 0, target);
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "要求我不要打扰对方工作，我转身离开了";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "要求我不要打扰对方工作，我告诉对方赶紧换个地方";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "我要求" + SelfName + "不要打扰我的工作，对方转身溜走了";
        else
            content = content = "我要求" + SelfName + "降低音量，对方竟然让我换个地方工作";
        return content;
    }
}
public class Event143 : Event
{
    public Event143() : base()
    {
        EventName = "仇人乐事E";
        DescriptionCount = 1;
        SingleResult = false;
        RequiredCondition = EventCondition.无;
        SuccessDescription = " /";
        FailDescription = " 双方好感度   -5，双方获得情绪“苦涩”×1";
    }

    protected override void SuccessResult(Employee emp, Employee target = null)
    {
        //"/

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, true, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, true));
    }

    protected override void FailResult(Employee emp, Employee target = null)
    {
        //"双方好感度   -5，双方获得情绪“苦涩”×1
        emp.AddTempRelation(target, -5);
        target.AddTempRelation(emp, -5);
        emp.AddTempEmotion(EColor.LBlue);
        target.AddTempEmotion(EColor.LBlue);

        int posbContent = Random.Range(1, DescriptionCount + 1);
        emp.InfoDetail.AddHistory(SelfDescription(emp, target, false, posbContent));
        target.InfoDetail.AddHistory(TargetDescription(emp, target, false));
    }

    public override string SelfDescription(Employee Emp, Employee targetEmp, bool success, int index)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
        {
            if (index == 1)
                content = "" + TargetName + "为了挽回面子顶撞了我一句，然后灰溜溜认输离开了";
            else if (index == 2)
            {

            }
        }
        else
        {
            if (index == 1)
                content = "" + TargetName + "举手要袭击我，幸好被周围同事制止";
        }
        return content;
    }

    public override string TargetDescription(Employee Emp, Employee targetEmp, bool success, int index = 0)
    {
        string content = "";
        SetNames(Emp, targetEmp);
        if (success == true)
            content = content = "我离开前向" + SelfName + "指出：办公室不是你的茶话会。";
        else
            content = content = "我抬手准备教训" + SelfName + "这个混蛋，却被同事们拉开了";
        return content;
    }
}