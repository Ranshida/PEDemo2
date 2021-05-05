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

    //检测事业部加成
    protected virtual int CalcDivision(Employee emp, Employee target, EventGroupInfo egi = null)
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
        }

        return result;
    }

    //检测特质加成
    protected virtual int CalcPerk(Employee emp)
    {
        int result = 0;
        foreach(PerkInfo perk in emp.InfoDetail.PerksInfo)
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

public class Event1 : Event
{
    public Event1() : base()
    {
        EventName = "互相协助";
        LevelRequire = new List<int>() { 1, 2, 3};
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
        int result = CalcEmotion(emp) + CalcDivision(emp, target, egi) + CalcPerk(emp);
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

public static class EventData
{
    //公司日常事件序列(无状态条件事件)
    public static List<Event> CompanyRoutineEventA = new List<Event>()
    {

    };

    //公司日常事件序列(状态条件事件)
    public static List<Event> CompanyRoutineEventB = new List<Event>()
    {
        new Event1()
    };

    //公司一般事件序列
    public static List<Event> CompanyNormalEvent = new List<Event>()
    {
        new EventC1()
    };

    //个人港口事件序列
    public static List<Event> EmpPortEvent = new List<Event>()
    {

    };

    //个人事件序列(只记第一个事件)，且Event14比较特殊故不计入
    public static List<Event> EmpPersonalEvent = new List<Event>()
    {
        
    };

    //特殊事件组
    public static List<EventGroup> SpecialEventGroups = new List<EventGroup>()
    {
        new EventGroup1()
    };
}