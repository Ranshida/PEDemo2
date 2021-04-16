using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    Imm, Dayily, Weekly, Monthly
}
public enum PerkColor
{
    None, White, Orange, Grey, Blue
}
//基类
public class Perk
{
    public int TimeLeft, Num = 0, Level = 1, BaseTime;  //Time单位小时,BaseTime用于可叠加Perk清除层数时重置时间
    public bool canStack = false;//是否可叠加
    public bool ManagePerk = false;//是否为在成为核心团队成员后奏效
    public bool DivisionPerk = false;//是否在成为高管后奏效
    public bool DepPerk = false;//是否为编入部门后生效
    public string Name, Description;
    public EffectType effectType;
    public PerkColor perkColor = PerkColor.None;

    public Employee TargetEmp;
    public DepControl TargetDep;
    public PerkInfo Info;

    public int TempValue1, TempValue2, TempValue3;//用于检测和保存的一些数值
    public float TempValue4;

    private bool StackAdded = false;

    public Perk(Employee Emp)
    {
        TargetEmp = Emp;
    }

    public void AddEffect()
    {
        //叠加特质只添加一次监听
        if (StackAdded == false)
        {
            if (effectType == EffectType.Dayily)
            {
                GameControl.Instance.DailyEvent.AddListener(ContinuousEffect);
            }
            else if (effectType == EffectType.Weekly)
            {
                GameControl.Instance.WeeklyEvent.AddListener(ContinuousEffect);
            }
            else if (effectType == EffectType.Monthly)
            {
                GameControl.Instance.MonthlyEvent.AddListener(ContinuousEffect);
            }
            GameControl.Instance.HourEvent.AddListener(TimePass);
            StackAdded = true;
        }
        ImmEffect();
    }

    public virtual void RemoveEffect()
    {

        //可叠加的时间清零后直接重置
        if (canStack == true)
        {
            Level -= 1;
            TimeLeft = BaseTime;
        }
        if (canStack == false || Level == 0 || Num == 48 || Num == 50)
        {
            Info.RemovePerk();
            RemoveAllListeners();
        }
    }

    public void RemoveAllListeners()
    {
        if (effectType == EffectType.Dayily)
        {
            GameControl.Instance.DailyEvent.RemoveListener(ContinuousEffect);
        }
        else if (effectType == EffectType.Weekly)
        {
            GameControl.Instance.WeeklyEvent.RemoveListener(ContinuousEffect);
        }
        else if (effectType == EffectType.Monthly)
        {
            GameControl.Instance.MonthlyEvent.RemoveListener(ContinuousEffect);
        }
        GameControl.Instance.HourEvent.RemoveListener(TimePass);
    }

    public virtual void TimePass()
    {
        if (TimeLeft >= 0)
        {
            TimeLeft -= 1;
            if (TimeLeft < 0)
                RemoveEffect();
        }
    }

    public virtual void ContinuousEffect()
    {
        //造成持续影响的效果
    }

    public virtual void ImmEffect()
    {
        //立刻造成影响的效果
    }

    public virtual void ActiveSpecialEffect()
    {
        //管理特质效果施加
    }

    public virtual void DeActiveSpecialEffect()
    {
        //管理特质效果解除
    }

    public Perk Clone()
    {
        return (Perk)this.MemberwiseClone();
    }

}

//档案维护
public class Perk1 : Perk
{
    public Perk1(Employee Emp) : base(Emp)
    {
        Name = "档案维护";
        Description = "成为核心团队成员后生效，增加公司1个物品槽";
        TimeLeft = -1;
        Num = 1;
        ManagePerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        GameControl.Instance.ItemLimit += 1;
    }

    public override void DeActiveSpecialEffect()
    {
        GameControl.Instance.ItemLimit -= 1;
    }
}

//缓和措施
public class Perk2 : Perk
{
    public Perk2(Employee Emp) : base(Emp)
    {
        Name = "缓和措施";
        Description = "成为核心团队成员后生效，增加公司不满栏位上限1点";
        TimeLeft = -1;
        Num = 2;
        ManagePerk = true;
    }

    public override void ActiveSpecialEffect()
    {

    }

    public override void DeActiveSpecialEffect()
    {

    }
}

//矛盾处理
public class Perk3 : Perk
{
    public Perk3(Employee Emp) : base(Emp)
    {
        Name = "矛盾处理";
        Description = "成为核心团队成员后生效，成功处理不满事件后额外消除1个不满";
        TimeLeft = -1;
        Num = 3;
        ManagePerk = true;
    }

    public override void ActiveSpecialEffect()
    {

    }

    public override void DeActiveSpecialEffect()
    {

    }
}

//值得信任
public class Perk4 : Perk
{
    public Perk4(Employee Emp) : base(Emp)
    {
        Name = "值得信任";
        Description = "成为核心团队成员后生效，增加CEO坚韧2点";
        TimeLeft = -1;
        Num = 4;
        ManagePerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        GameControl.Instance.CC.CEO.Tenacity += 2;
    }

    public override void DeActiveSpecialEffect()
    {
        GameControl.Instance.CC.CEO.Tenacity -= 2;
    }
}

//沟通与协调
public class Perk5 : Perk
{
    public Perk5(Employee Emp) : base(Emp)
    {
        Name = "沟通与协调";
        Description = "成为高管后生效，管理的事业部信念+10";
        TimeLeft = -1;
        Num = 5;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        TargetEmp.CurrentDivision.Faith += 10;
    }

    public override void DeActiveSpecialEffect()
    {
        TargetEmp.CurrentDivision.Faith -= 10;
    }
}

//组织行为学
public class Perk6 : Perk
{
    public Perk6(Employee Emp) : base(Emp)
    {
        Name = "组织行为学";
        Description = "成为高管后生效，管理的事业部效率+1";
        TimeLeft = -1;
        Num = 6;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        TargetEmp.CurrentDivision.Efficiency += 10;
    }

    public override void DeActiveSpecialEffect()
    {
        TargetEmp.CurrentDivision.Efficiency -= 10;
    }
}

//项目管理
public class Perk7 : Perk
{
    public Perk7(Employee Emp) : base(Emp)
    {
        Name = "项目管理";
        Description = "成为高管后生效，管理的事业部工作状态+1";
        TimeLeft = -1;
        Num = 7;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        TargetEmp.CurrentDivision.WorkStatus += 10;
    }

    public override void DeActiveSpecialEffect()
    {
        TargetEmp.CurrentDivision.WorkStatus -= 10;
    }
}

//报表分析
public class Perk8 : Perk
{
    public Perk8(Employee Emp) : base(Emp)
    {
        Name = "报表分析";
        Description = "成为高管后生效，管理的事业部成本-20";
        TimeLeft = -1;
        Num = 8;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        TargetEmp.CurrentDivision.ExtraCost -= 20;
    }

    public override void DeActiveSpecialEffect()
    {
        TargetEmp.CurrentDivision.ExtraCost += 20;
    }
}

//战略管理
public class Perk9 : Perk
{
    public Perk9(Employee Emp) : base(Emp)
    {
        Name = "战略管理";
        Description = "成为高管后生效，管理的事业部中所有充能建筑的生产周期-1回合";
        TimeLeft = -1;
        Num = 9;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        TargetEmp.CurrentDivision.ExtraProduceTime -= 1;
    }

    public override void DeActiveSpecialEffect()
    {
        TargetEmp.CurrentDivision.ExtraProduceTime += 1;
    }
}

//调度
public class Perk10 : Perk
{
    public Perk10(Employee Emp) : base(Emp)
    {
        Name = "调度";
        Description = "成为高管后生效，管理的事业部的状态效果额外增加50%";
        TimeLeft = -1;
        Num = 10;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {

    }

    public override void DeActiveSpecialEffect()
    {

    }
}

//概率论入门
public class Perk11 : Perk
{
    public Perk11(Employee Emp) : base(Emp)
    {
        Name = "概率论入门";
        Description = "成为高管后生效，管理的事业部中员工的经验值每回合额外获得1点经验";
        TimeLeft = -1;
        Num = 11;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        TargetEmp.CurrentDivision.ExtraExp += 1;
    }

    public override void DeActiveSpecialEffect()
    {
        TargetEmp.CurrentDivision.ExtraExp -= 1;
    }
}

//高效
public class Perk12 : Perk
{
    public Perk12(Employee Emp) : base(Emp)
    {
        Name = "高效";
        Description = "所在事业部效率+1";
        TimeLeft = -1;
        Num = 12;
        DepPerk = true;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null)
            TargetEmp.CurrentDep.CurrentDivision.Efficiency += 1;
        else
            TargetEmp.CurrentDivision.Efficiency += 1;
    }

    public override void DeActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null)
            TargetEmp.CurrentDep.CurrentDivision.Efficiency -= 1;
        else
            TargetEmp.CurrentDivision.Efficiency -= 1;
    }
}

//投入
public class Perk13 : Perk
{
    public Perk13(Employee Emp) : base(Emp)
    {
        Name = "投入";
        Description = "所在事业部工作状态+1";
        TimeLeft = -1;
        Num = 13;
        DepPerk = true;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null)
            TargetEmp.CurrentDep.CurrentDivision.WorkStatus += 1;
        else
            TargetEmp.CurrentDivision.WorkStatus += 1;
    }

    public override void DeActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null)
            TargetEmp.CurrentDep.CurrentDivision.WorkStatus -= 1;
        else
            TargetEmp.CurrentDivision.WorkStatus -= 1;
    }
}

//鼓舞
public class Perk14 : Perk
{
    public Perk14(Employee Emp) : base(Emp)
    {
        Name = "鼓舞";
        Description = "所在事业部信念+10";
        TimeLeft = -1;
        Num = 14;
        DepPerk = true;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null)
            TargetEmp.CurrentDep.CurrentDivision.Faith += 10;
        else
            TargetEmp.CurrentDivision.Faith += 10;
    }

    public override void DeActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null)
            TargetEmp.CurrentDep.CurrentDivision.Faith -= 10;
        else
            TargetEmp.CurrentDivision.Faith -= 10;
    }
}

//节约
public class Perk15 : Perk
{
    public Perk15(Employee Emp) : base(Emp)
    {
        Name = "节约";
        Description = "所在事业部成本-5";
        TimeLeft = -1;
        Num = 15;
        DepPerk = true;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null)
            TargetEmp.CurrentDep.CurrentDivision.ExtraCost -= 5;
        else
            TargetEmp.CurrentDivision.ExtraCost -= 5;
    }

    public override void DeActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null)
            TargetEmp.CurrentDep.CurrentDivision.ExtraCost += 5;
        else
            TargetEmp.CurrentDivision.ExtraCost += 5;
    }
}

//乐观
public class Perk16 : Perk
{
    public Perk16(Employee Emp) : base(Emp)
    {
        Name = "乐观";
        Description = "自身参与的事件修正+1";
        TimeLeft = -1;
        Num = 16;
    }
}

//开朗
public class Perk17 : Perk
{
    public Perk17(Employee Emp) : base(Emp)
    {
        Name = "开朗";
        Description = "自身参与的事件修正+1";
        TimeLeft = -1;
        Num = 17;
    }
}

//敏感
public class Perk18 : Perk
{
    public Perk18(Employee Emp) : base(Emp)
    {
        Name = "敏感";
        Description = "更容易产生情绪（获取情绪时有50%概率多获得1个）";
        TimeLeft = -1;
        Num = 18;
    }
}

//热情
public class Perk19 : Perk
{
    public Perk19(Employee Emp) : base(Emp)
    {
        Name = "热情";
        Description = "每回合额外获取1点经验";
        TimeLeft = -1;
        Num = 19;
    }

    public override void ImmEffect()
    {
        TargetEmp.ExtraExp += 1;
    }

    public override void RemoveEffect()
    {
        TargetEmp.ExtraExp -= 1;
        base.RemoveEffect();
    }
}

//电波系
public class Perk20 : Perk
{
    public Perk20(Employee Emp) : base(Emp)
    {
        Name = "电波系";
        Description = "办公室中存在两名持有此特质的员工时效率+2";
        TimeLeft = -1;
        Num = 20;
        DepPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        int num = 0;
        foreach(Employee emp in TargetEmp.CurrentDep.CurrentEmps)
        {
            if (emp == TargetEmp)
                continue;
            foreach(PerkInfo perk in emp.InfoDetail.PerksInfo)
            {
                if (perk.CurrentPerk.Num == 20)
                {
                    num += 1;
                    break;
                }
            }
        }
        if (num == 1)
            TargetEmp.CurrentDep.CurrentDivision.Efficiency += 2;
    }

    public override void DeActiveSpecialEffect()
    {
        int num = 0;
        foreach (Employee emp in TargetEmp.CurrentDep.CurrentEmps)
        {
            if (emp == TargetEmp)
                continue;
            foreach (PerkInfo perk in emp.InfoDetail.PerksInfo)
            {
                if (perk.CurrentPerk.Num == 20)
                {
                    num += 1;
                    break;
                }
            }
        }
        if (num == 1)
            TargetEmp.CurrentDep.CurrentDivision.Efficiency -= 2;
    }
}

//有耐心
public class Perk21 : Perk
{
    public Perk21(Employee Emp) : base(Emp)
    {
        Name = "有耐心";
        Description = "坚韧+1";
        TimeLeft = -1;
        Num = 21;
    }

    public override void ImmEffect()
    {
        TargetEmp.Tenacity += 1;
    }

    public override void RemoveEffect()
    {
        TargetEmp.Tenacity -= 1;
        base.RemoveEffect();
    }
}

//持之以恒
public class Perk22 : Perk
{
    public Perk22(Employee Emp) : base(Emp)
    {
        Name = "持之以恒";
        Description = "坚韧+2";
        TimeLeft = -1;
        Num = 22;
    }

    public override void ImmEffect()
    {
        TargetEmp.Tenacity += 2;
    }

    public override void RemoveEffect()
    {
        TargetEmp.Tenacity -= 2;
        base.RemoveEffect();
    }
}

//经历过打击
public class Perk23 : Perk
{
    public Perk23(Employee Emp) : base(Emp)
    {
        Name = "经历过打击";
        Description = "坚韧+3";
        TimeLeft = -1;
        Num = 23;
    }

    public override void ImmEffect()
    {
        TargetEmp.Tenacity += 3;
    }

    public override void RemoveEffect()
    {
        TargetEmp.Tenacity -= 3;
        base.RemoveEffect();
    }
}

//数据模型
public class Perk24 : Perk
{
    public Perk24(Employee Emp) : base(Emp)
    {
        Name = "数据模型";
        Description = "决策+1";
        TimeLeft = -1;
        Num = 24;
    }

    public override void ImmEffect()
    {
        TargetEmp.Decision += 1;
    }

    public override void RemoveEffect()
    {
        TargetEmp.Decision -= 1;
        base.RemoveEffect();
    }
}

//应用统计学
public class Perk25 : Perk
{
    public Perk25(Employee Emp) : base(Emp)
    {
        Name = "应用统计学";
        Description = "决策+1";
        TimeLeft = -1;
        Num = 25;
    }

    public override void ImmEffect()
    {
        TargetEmp.Decision += 1;
    }

    public override void RemoveEffect()
    {
        TargetEmp.Decision -= 1;
        base.RemoveEffect();
    }
}

//运筹学
public class Perk26 : Perk
{
    public Perk26(Employee Emp) : base(Emp)
    {
        Name = "运筹学";
        Description = "决策+2";
        TimeLeft = -1;
        Num = 26;
    }

    public override void ImmEffect()
    {
        TargetEmp.Decision += 2;
    }

    public override void RemoveEffect()
    {
        TargetEmp.Decision -= 2;
        base.RemoveEffect();
    }
}

//部门领导
public class Perk27 : Perk
{
    public Perk27(Employee Emp) : base(Emp)
    {
        Name = "部门领导";
        Description = "管理+1";
        TimeLeft = -1;
        Num = 27;
    }

    public override void ImmEffect()
    {
        TargetEmp.Manage += 1;
    }

    public override void RemoveEffect()
    {
        TargetEmp.Manage -= 1;
        base.RemoveEffect();
    }
}

//自主创业
public class Perk28 : Perk
{
    public Perk28(Employee Emp) : base(Emp)
    {
        Name = "自主创业";
        Description = "管理+1";
        TimeLeft = -1;
        Num = 28;
    }

    public override void ImmEffect()
    {
        TargetEmp.Manage += 1;
    }

    public override void RemoveEffect()
    {
        TargetEmp.Manage -= 1;
        base.RemoveEffect();
    }
}

//管培生
public class Perk29 : Perk
{
    public Perk29(Employee Emp) : base(Emp)
    {
        Name = "管培生";
        Description = "管理+1";
        TimeLeft = -1;
        Num = 29;
    }

    public override void ImmEffect()
    {
        TargetEmp.Manage += 1;
    }

    public override void RemoveEffect()
    {
        TargetEmp.Manage -= 1;
        base.RemoveEffect();
    }
}

//小组长
public class Perk30 : Perk
{
    public Perk30(Employee Emp) : base(Emp)
    {
        Name = "小组长";
        Description = "管理+1";
        TimeLeft = -1;
        Num = 30;
    }

    public override void ImmEffect()
    {
        TargetEmp.Manage += 1;
    }

    public override void RemoveEffect()
    {
        TargetEmp.Manage -= 1;
        base.RemoveEffect();
    }
}

//学生会干部
public class Perk31 : Perk
{
    public Perk31(Employee Emp) : base(Emp)
    {
        Name = "学生会干部";
        Description = "管理+1";
        TimeLeft = -1;
        Num = 31;
    }

    public override void ImmEffect()
    {
        TargetEmp.Manage += 1;
    }

    public override void RemoveEffect()
    {
        TargetEmp.Manage -= 1;
        base.RemoveEffect();
    }
}

//社团领袖
public class Perk32 : Perk
{
    public Perk32(Employee Emp) : base(Emp)
    {
        Name = "社团领袖";
        Description = "管理+1";
        TimeLeft = -1;
        Num = 32;
    }

    public override void ImmEffect()
    {
        TargetEmp.Manage += 1;
    }

    public override void RemoveEffect()
    {
        TargetEmp.Manage -= 1;
        base.RemoveEffect();
    }
}

//自由者
public class Perk35 : Perk
{
    public Perk35(Employee Emp) : base(Emp)
    {
        Name = "自由者";
        Description = "厌恶管理，所在事业部高管管理-1";
        TimeLeft = -1;
        Num = 35;
        DepPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        TargetEmp.CurrentDep.CurrentDivision.ExtraManage -= 1;
    }

    public override void DeActiveSpecialEffect()
    {
        TargetEmp.CurrentDep.CurrentDivision.ExtraManage += 1;
    }
}

//服从
public class Perk36 : Perk
{
    public Perk36(Employee Emp) : base(Emp)
    {
        Name = "服从";
        Description = "所在事业部工作状态-1，效率+1";
        TimeLeft = -1;
        Num = 36;
        DepPerk = true;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null)
        {
            TargetEmp.CurrentDep.CurrentDivision.WorkStatus -= 1;
            TargetEmp.CurrentDep.CurrentDivision.Efficiency += 1;
        }
        else
        {
            TargetEmp.CurrentDivision.WorkStatus -= 1;
            TargetEmp.CurrentDivision.Efficiency += 1;
        }
    }

    public override void DeActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null)
        {
            TargetEmp.CurrentDep.CurrentDivision.WorkStatus += 1;
            TargetEmp.CurrentDep.CurrentDivision.Efficiency -= 1;
        }
        else
        {
            TargetEmp.CurrentDivision.WorkStatus += 1;
            TargetEmp.CurrentDivision.Efficiency -= 1;
        }
    }
}

//待遇要求高
public class Perk37 : Perk
{
    public Perk37(Employee Emp) : base(Emp)
    {
        Name = "待遇要求高";
        Description = "管理+1，所在事业部成本+5";
        TimeLeft = -1;
        Num = 37;
        DepPerk = true;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null)
            TargetEmp.CurrentDep.CurrentDivision.ExtraCost += 5;
        else
            TargetEmp.CurrentDivision.ExtraCost += 5;
    }

    public override void DeActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null)
            TargetEmp.CurrentDep.CurrentDivision.ExtraCost -= 5;
        else
            TargetEmp.CurrentDivision.ExtraCost -= 5;
    }

    public override void ImmEffect()
    {
        TargetEmp.Manage += 1;
    }
}

//盗窃癖
public class Perk38 : Perk
{
    public Perk38(Employee Emp) : base(Emp)
    {
        Name = "盗窃癖";
        Description = "所在事业部成本+5";
        TimeLeft = -1;
        Num = 38;
        DepPerk = true;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null)
            TargetEmp.CurrentDep.CurrentDivision.ExtraCost += 5;
        else
            TargetEmp.CurrentDivision.ExtraCost += 5;
    }

    public override void DeActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null)
            TargetEmp.CurrentDep.CurrentDivision.ExtraCost -= 5;
        else
            TargetEmp.CurrentDivision.ExtraCost -= 5;
    }
}

//用爱发电
public class Perk39 : Perk
{
    public Perk39(Employee Emp) : base(Emp)
    {
        Name = "用爱发电";
        Description = "所在事业部成本-5";
        TimeLeft = -1;
        Num = 39;
        DepPerk = true;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null)
            TargetEmp.CurrentDep.CurrentDivision.ExtraCost -= 5;
        else
            TargetEmp.CurrentDivision.ExtraCost -= 5;
    }

    public override void DeActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null)
            TargetEmp.CurrentDep.CurrentDivision.ExtraCost += 5;
        else
            TargetEmp.CurrentDivision.ExtraCost += 5;
    }
}

//意见领袖
public class Perk40 : Perk
{
    public Perk40(Employee Emp) : base(Emp)
    {
        Name = "意见领袖";
        Description = "所在事业部信念+10，所在事业部高管管理-1";
        TimeLeft = -1;
        Num = 40;
        DepPerk = true;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null)
        {
            TargetEmp.CurrentDep.CurrentDivision.ExtraCost -= 5;
            TargetEmp.CurrentDep.CurrentDivision.ExtraManage -= 1;
        }
        else
            TargetEmp.CurrentDivision.ExtraCost -= 5;
    }

    public override void DeActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null)
        {
            TargetEmp.CurrentDep.CurrentDivision.ExtraCost += 5;
            TargetEmp.CurrentDep.CurrentDivision.ExtraManage += 1;
        }
        else
            TargetEmp.CurrentDivision.ExtraCost += 5;
    }
}

//经常重做
public class Perk41 : Perk
{
    public Perk41(Employee Emp) : base(Emp)
    {
        Name = "经常重做";
        Description = "所在事业部工作状态+1，信念-10";
        TimeLeft = -1;
        Num = 41;
        DepPerk = true;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null)
        {
            TargetEmp.CurrentDep.CurrentDivision.WorkStatus += 1;
            TargetEmp.CurrentDep.CurrentDivision.Faith -= 10;
        }
        else
        {
            TargetEmp.CurrentDivision.WorkStatus += 1;
            TargetEmp.CurrentDivision.Faith -= 10;
        }
    }

    public override void DeActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null)
        {
            TargetEmp.CurrentDep.CurrentDivision.WorkStatus -= 1;
            TargetEmp.CurrentDep.CurrentDivision.Faith += 10;
        }
        else
        {
            TargetEmp.CurrentDivision.WorkStatus -= 1;
            TargetEmp.CurrentDivision.Faith += 10;
        }
    }
}

//滔滔不绝
public class Perk42 : Perk
{
    public Perk42(Employee Emp) : base(Emp)
    {
        Name = "滔滔不绝";
        Description = "所在事业部效率-1，自身坚韧+1";
        TimeLeft = -1;
        Num = 42;
        DepPerk = true;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null)
            TargetEmp.CurrentDep.CurrentDivision.Efficiency -= 1;
        else
            TargetEmp.CurrentDivision.Efficiency -= 1;
    }

    public override void DeActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null)
            TargetEmp.CurrentDep.CurrentDivision.Efficiency += 1;
        else
            TargetEmp.CurrentDivision.Efficiency += 1;
    }

    public override void ImmEffect()
    {
        TargetEmp.Tenacity += 1;
    }
}

//见多识广
public class Perk43 : Perk
{
    public Perk43(Employee Emp) : base(Emp)
    {
        Name = "见多识广";
        Description = "参与个人事件修正+1";
        TimeLeft = -1;
        Num = 43;
    }
}

//御宅族
public class Perk46 : Perk
{
    public Perk46(Employee Emp) : base(Emp)
    {
        Name = "御宅族";
        Description = "初始坚韧2点，初始决策1点，初始管理0点";
        TimeLeft = -1;
        Num = 46;
    }

    public override void ImmEffect()
    {
        TargetEmp.Tenacity = 2;
        TargetEmp.Decision = 1;
        TargetEmp.Manage = 0;
    }
}

//钓鱼爱好者
public class Perk47 : Perk
{
    public Perk47(Employee Emp) : base(Emp)
    {
        Name = "钓鱼爱好者";
        Description = "初始坚韧3点，初始决策0点，初始管理0点";
        TimeLeft = -1;
        Num = 47;
    }

    public override void ImmEffect()
    {
        TargetEmp.Tenacity = 3;
        TargetEmp.Decision = 0;
        TargetEmp.Manage = 0;
    }
}

//孩子王
public class Perk48 : Perk
{
    public Perk48(Employee Emp) : base(Emp)
    {
        Name = "孩子王";
        Description = "初始坚韧0点，初始决策0点，初始管理2点";
        TimeLeft = -1;
        Num = 48;
    }

    public override void ImmEffect()
    {
        TargetEmp.Tenacity = 0;
        TargetEmp.Decision = 0;
        TargetEmp.Manage = 2;
    }
}

//乐队成员
public class Perk49 : Perk
{
    public Perk49(Employee Emp) : base(Emp)
    {
        Name = "乐队成员";
        Description = "初始坚韧0点，初始决策2点，初始管理0点";
        TimeLeft = -1;
        Num = 49;
    }

    public override void ImmEffect()
    {
        TargetEmp.Tenacity = 0;
        TargetEmp.Decision = 2;
        TargetEmp.Manage = 0;
    }
}

//社会人
public class Perk50 : Perk
{
    public Perk50(Employee Emp) : base(Emp)
    {
        Name = "社会人";
        Description = "初始坚韧2点，初始决策0点，初始管理1点";
        TimeLeft = -1;
        Num = 50;
    }

    public override void ImmEffect()
    {
        TargetEmp.Tenacity = 2;
        TargetEmp.Decision = 0;
        TargetEmp.Manage = 1;
    }
}

//平衡强迫症
public class Perk51 : Perk
{
    public Perk51(Employee Emp) : base(Emp)
    {
        Name = "平衡强迫症";
        Description = "初始坚韧1点，初始决策1点，初始管理1点";
        TimeLeft = -1;
        Num = 51;
    }

    public override void ImmEffect()
    {
        TargetEmp.Tenacity = 1;
        TargetEmp.Decision = 1;
        TargetEmp.Manage = 1;
    }
}

#region 旧perk

//热情
public class Perk109 : Perk
{
    int Value = -1;
    List<int> StarNum = new List<int>();
    public Perk109(Employee Emp) : base(Emp)
    {
        Name = "热情";
        Description = "对应领域热情+1";
        TimeLeft = 192;
        Num = 109;
        canStack = true;
    }

    public override void ImmEffect()
    {
        //旧的热情判定
        ////叠加时重置
        //Value = -1;
        //if (TargetEmp.CurrentDep != null)
        //    Value = TargetEmp.CurrentDep.building.effectValue - 1;
        //else if (TargetEmp.CurrentOffice != null)
        //    Value = TargetEmp.CurrentOffice.building.effectValue - 1;
        //if (Value != -1)
        //{ 
        //    Value /= 3;
        //    if (TargetEmp.Stars[Value] < TargetEmp.StarLimit[Value])
        //    {
        //        TargetEmp.Stars[Value] += 1;
        //        StarNum.Add(Value);
        //    }
        //}
    }

    public override void RemoveEffect()
    {
        //旧的热情判定
        //if (StarNum.Count > 0)
        //{
        //    TargetEmp.Stars[StarNum[0]] -= 1;
        //    if (TargetEmp.Stars[StarNum[0]] < 0)
        //        TargetEmp.Stars[StarNum[0]] = 0;
        //    StarNum.RemoveAt(0);
        //}
        base.RemoveEffect();
    }
}


//精进
public class Perk33 : Perk
{
    public Perk33(Employee Emp) : base(Emp)
    {
        Name = "精进";
        Description = "生产和办公室技能充能成功率+5%";
        TimeLeft = 32;
        Num = 33;
        perkColor = PerkColor.White;
    }
    public override void ImmEffect()
    {
        TargetEmp.ExtraSuccessRate += 0.05f;
        TempValue4 = 0.05f;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.ExtraSuccessRate -= 0.05f;
    }
}

//削减支出
public class Perk34 : Perk
{
    public Perk34(Employee Emp) : base(Emp)
    {
        Name = "削减支出";
        Description = "公司建筑维护费用-5%";
        TimeLeft = 32;
        Num = 34;
        canStack = true;
        perkColor = PerkColor.Blue;
    }
    public override void ImmEffect()
    {
        if (TargetDep != null)
        {
            TargetDep.BuildingPayMultiply -= 0.05f;
        }
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        if (TargetDep != null)
        {
            TargetDep.BuildingPayMultiply += 0.05f;
        }
    }
}

#region 部分初始随机特质

//孤僻
public class Perk82 : Perk
{
    public Perk82(Employee Emp) : base(Emp)
    {
        Name = "孤僻";
        Description = "降低认识新人事件组发生权重1";
        TimeLeft = -1;//特质
        Num = 82;
        canStack = false;
    }
}

//野心家
public class Perk83 : Perk
{
    public Perk83(Employee Emp) : base(Emp)
    {
        Name = "野心家";
        Description = "提高野心事件组发生权重1";
        TimeLeft = -1;//特质
        Num = 83;
        canStack = false;
    }
}

//梦想家
public class Perk84 : Perk
{
    public Perk84(Employee Emp) : base(Emp)
    {
        Name = "梦想家";
        Description = "提高愿望事件组发生权重1";
        TimeLeft = -1;//特质
        Num = 80;
        canStack = false;
    }
}

//革命者
public class Perk85 : Perk
{
    public Perk85(Employee Emp) : base(Emp)
    {
        Name = "革命者";
        Description = "提高诉求不满事件组发生权重1";
        TimeLeft = -1;//特质
        Num = 85;
        canStack = false;
    }
}

//虚荣者
public class Perk86 : Perk
{
    public Perk86(Employee Emp) : base(Emp)
    {
        Name = "虚荣者";
        Description = "提高认可交谈事件组发生权重1";
        TimeLeft = -1;//特质
        Num = 86;
        canStack = false;
    }
}

//社交达人
public class Perk87 : Perk
{
    public Perk87(Employee Emp) : base(Emp)
    {
        Name = "社交达人";
        Description = "提高分享日常、分享快乐事件组发生权重1";
        TimeLeft = -1;//特质
        Num = 87;
        canStack = false;
    }
}

//倾诉者
public class Perk88 : Perk
{
    public Perk88(Employee Emp) : base(Emp)
    {
        Name = "倾诉者";
        Description = "提高寻求安慰事件组发生权重1";
        TimeLeft = -1;//特质
        Num = 88;
        canStack = false;
    }
}

//躁狂
public class Perk89 : Perk
{
    public Perk89(Employee Emp) : base(Emp)
    {
        Name = "躁狂";
        Description = "提高无聊事件组发生权重1";
        TimeLeft = -1;//特质
        Num = 89;
        canStack = false;
    }
}

//思想家
public class Perk90 : Perk
{
    public Perk90(Employee Emp) : base(Emp)
    {
        Name = "思想家";
        Description = "提高深刻交谈事件组发生权重1";
        TimeLeft = -1;//特质
        Num = 90;
        canStack = false;
    }
}

//谦逊
public class Perk91 : Perk
{
    public Perk91(Employee Emp) : base(Emp)
    {
        Name = "谦逊";
        Description = "不会出现骄傲事件组";
        TimeLeft = -1;//特质
        Num = 91;
        canStack = false;
    }
}

//冷静
public class Perk92 : Perk
{
    public Perk92(Employee Emp) : base(Emp)
    {
        Name = "冷静";
        Description = "不会出现狂想事件组";
        TimeLeft = -1;//特质
        Num = 92;
        canStack = false;
    }
}
#endregion

//结构优化
public class Perk44 : Perk
{
    public Perk44(Employee Emp) : base(Emp)
    {
        Name = "结构优化";
        Description = "降低人员工资和建筑维护费用25%";
        TimeLeft = 64;
        Num = 44;
        canStack = false;
        perkColor = PerkColor.Blue;
        TempValue1 = 1;
    }
    public override void ImmEffect()
    {
        TargetDep.SalaryMultiply -= 0.25f * TempValue1;
        TargetDep.BuildingPayMultiply -= 0.25f * TempValue1;
        Description = "降低人员工资和建筑维护费用" + 25 * TempValue1 + "%";
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.SalaryMultiply += 0.25f * TempValue1;
        TargetDep.BuildingPayMultiply += 0.25f * TempValue1;
    }
}

//团结
public class Perk45 : Perk
{
    public Perk45(Employee Emp) : base(Emp)
    {
        Name = "团结";
        Description = "存在时“并肩作战”效果×3";
        TimeLeft = 64;
        Num = 45;
        canStack = false;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        if (TargetDep != null)
        {
            Description = "存在时“并肩作战”效果×3";
            TargetDep.FaithRelationCheck();
        }
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.FaithRelationCheck();
    }
}

//烦恼
public class Perk52 : Perk
{
    public Perk52(Employee Emp) : base(Emp)
    {
        Name = "烦恼";
        Description = "解锁事件，公司日常";
        TimeLeft = 64;
        Num = 52;
        canStack = true;
    }
}

//炫耀
public class Perk53 : Perk
{
    public Perk53(Employee Emp) : base(Emp)
    {
        Name = "炫耀";
        Description = "解锁事件，公司日常";
        TimeLeft = 64;
        Num = 53;
        canStack = true;
    }
}

//尴尬
public class Perk54 : Perk
{
    public Perk54(Employee Emp) : base(Emp)
    {
        Name = "尴尬";
        Description = "解锁事件，公司日常";
        TimeLeft = 64;
        Num = 54;
        canStack = true;
    }
}

//愿望
public class Perk55 : Perk
{
    public Perk55(Employee Emp) : base(Emp)
    {
        Name = "愿望";
        Description = "解锁事件，公司港口行为树";
        TimeLeft = 64;
        Num = 55;
        canStack = true;
    }
}

//野心
public class Perk56 : Perk
{
    public Perk56(Employee Emp) : base(Emp)
    {
        Name = "野心";
        Description = "解锁事件，公司港口行为树";
        TimeLeft = 64;
        Num = 56;
        canStack = true;
    }
}

//不满
public class Perk57 : Perk
{
    public Perk57(Employee Emp) : base(Emp)
    {
        Name = "不满";
        Description = "解锁事件，公司港口行为树";
        TimeLeft = 64;
        Num = 57;
        canStack = true;
    }
}

//成长
public class Perk58 : Perk
{
    public Perk58(Employee Emp) : base(Emp)
    {
        Name = "成长";
        Description = "解锁事件，1年内每升一级获取1个，下年清空";
        TimeLeft = -1; //到年底
        Num = 58;
        canStack = true;
    }
}

//深刻交谈
public class Perk59 : Perk
{
    public Perk59(Employee Emp) : base(Emp)
    {
        Name = "深刻交谈";
        Description = "解锁事件，个人港口行为树";
        TimeLeft = 64;
        Num = 59;
        canStack = true;
    }
}

//分享乐事
public class Perk60 : Perk
{
    public Perk60(Employee Emp) : base(Emp)
    {
        Name = "分享乐事";
        Description = "解锁事件，个人港口行为树";
        TimeLeft = 64;
        Num = 60;
        canStack = true;
    }
}

//认可交谈
public class Perk61 : Perk
{
    public Perk61(Employee Emp) : base(Emp)
    {
        Name = "认可交谈";
        Description = "解锁事件，个人港口行为树";
        TimeLeft = 64;
        Num = 61;
        canStack = true;
    }
}

//分享日常
public class Perk62 : Perk
{
    public Perk62(Employee Emp) : base(Emp)
    {
        Name = "分享日常";
        Description = "解锁事件，个人港口行为树";
        TimeLeft = 64;
        Num = 62;
        canStack = true;
    }
}

//寻求安慰
public class Perk63 : Perk
{
    public Perk63(Employee Emp) : base(Emp)
    {
        Name = "寻求安慰";
        Description = "解锁事件，个人港口行为树";
        TimeLeft = 64;
        Num = 63;
        canStack = true;
    }
}

//无聊
public class Perk64 : Perk
{
    public Perk64(Employee Emp) : base(Emp)
    {
        Name = "无聊";
        Description = "解锁事件，个人港口行为树";
        TimeLeft = 64;
        Num = 64;
        canStack = true;
    }
}

//受到信任
public class Perk65 : Perk
{
    public Perk65(Employee Emp) : base(Emp)
    {
        Name = "受到信任";
        Description = "解锁事件，公司普通行为树";
        TimeLeft = 64;
        Num = 65;
        canStack = true;
    }
}

//受到质疑
public class Perk66 : Perk
{
    public Perk66(Employee Emp) : base(Emp)
    {
        Name = "受到质疑";
        Description = "解锁事件，公司普通行为树";
        TimeLeft = 64;
        Num = 66;
        canStack = true;
    }
}

//受到赞扬
public class Perk67 : Perk
{
    public Perk67(Employee Emp) : base(Emp)
    {
        Name = "受到赞扬";
        Description = "解锁事件，公司普通行为树";
        TimeLeft = 64;
        Num = 67;
        canStack = true;
    }
}

//受到批评
public class Perk68 : Perk
{
    public Perk68(Employee Emp) : base(Emp)
    {
        Name = "受到批评";
        Description = "解锁事件，公司普通行为树";
        TimeLeft = 64;
        Num = 68;
        canStack = true;
    }
}

//遭到敌意
public class Perk69 : Perk
{
    public Perk69(Employee Emp) : base(Emp)
    {
        Name = "遭到敌意";
        Description = "解锁事件，公司普通行为树";
        TimeLeft = 64;
        Num = 69;
        canStack = true;
    }
}

//理想
public class Perk70 : Perk
{
    public Perk70(Employee Emp) : base(Emp)
    {
        Name = "理想";
        Description = "解锁事件，公司普通行为树";
        TimeLeft = -1;//永久，直到完成
        Num = 70;
        canStack = false;
    }
}

//充分讨论
public class Perk71 : Perk
{
    public Perk71(Employee Emp) : base(Emp)
    {
        Name = "充分讨论";
        Description = "部门成功率上升5%";
        TimeLeft = -1;//持续到当前业务结束
        Num = 71;
        canStack = true;
        perkColor = PerkColor.White;
    }
    public override void ImmEffect()
    {
        if (TargetDep != null)
        {
            TargetDep.Efficiency += 0.05f;
        }
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        if (TargetDep != null)
        {
            TargetDep.Efficiency -= 0.05f;
        }
    }
}

//期待转岗
public class Perk72 : Perk
{
    public Perk72(Employee Emp) : base(Emp)
    {
        Name = "期待转岗";
        Description = "持续时间结束时没有消除则员工信念-10";
        TimeLeft = 64;
        Num = 72;
        canStack = false;
    }
}

//期待升职
public class Perk73 : Perk
{
    public Perk73(Employee Emp) : base(Emp)
    {
        Name = "期待升职";
        Description = "持续时间结束时没有消除则员工信念-8";
        TimeLeft = 64;
        Num = 73;
        canStack = false;
    }
}

//精神恍惚
public class Perk74 : Perk
{
    public Perk74(Employee Emp) : base(Emp)
    {
        Name = "精神恍惚";
        Description = "心力爆炸获得，失误率+50%";
        TimeLeft = 96;
        Num = 74;
        canStack = false;
    }
}

//善辩
public class Perk75 : Perk
{
    public Perk75(Employee Emp) : base(Emp)
    {
        Name = "善辩";
        Description = "加事件判定正面修正+1";
        TimeLeft = -1;//特质
        Num = 75;
        canStack = false;
    }
}

//说客
public class Perk76 : Perk
{
    public Perk76(Employee Emp) : base(Emp)
    {
        Name = "说客";
        Description = "加事件判定正面修正+2";
        TimeLeft = -1;//特质
        Num = 76;
        canStack = false;
    }
}

//雄辩家
public class Perk77 : Perk
{
    public Perk77(Employee Emp) : base(Emp)
    {
        Name = "雄辩家";
        Description = "加事件判定正面修正+3";
        TimeLeft = -1;//特质
        Num = 77;
        canStack = false;
    }
}

//出众
public class Perk78 : Perk
{
    public Perk78(Employee Emp) : base(Emp)
    {
        Name = "出众";
        Description = "加事件判定正面修正+1";
        TimeLeft = -1;//特质
        Num = 78;
        canStack = false;
    }
}

//领袖气质
public class Perk79 : Perk
{
    public Perk79(Employee Emp) : base(Emp)
    {
        Name = "领袖气质";
        Description = "加事件判定正面修正+2";
        TimeLeft = -1;//特质
        Num = 79;
        canStack = false;
    }
}

//传奇
public class Perk80 : Perk
{
    public Perk80(Employee Emp) : base(Emp)
    {
        Name = "传奇";
        Description = "加事件判定正面修正+3";
        TimeLeft = -1;//特质
        Num = 80;
        canStack = false;
    }
}

//认识新人
public class Perk81 : Perk
{
    public Perk81(Employee Emp) : base(Emp)
    {
        Name = "认识新人";
        Description = "认识新人的次数，作为标记使用";
        TimeLeft = -1;
        Num = 81;
        canStack = true;
    }
}


//被害妄想症
public class Perk93 : Perk
{
    public Perk93(Employee Emp) : base(Emp)
    {
        Name = "被害妄想症";
        Description = "所有公司日常类的事件正面修正-2";
        TimeLeft = -1;//特质
        Num = 93;
        canStack = false;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.ExhaustedCount.Remove(Num);
    }
}

//刚愎自用
public class Perk94: Perk
{
    public Perk94(Employee Emp) : base(Emp)
    {
        Name = "刚愎自用";
        Description = "所在部门成功率-10%";
        TimeLeft = -1;//特质
        Num = 94;
        canStack = false;
    }
    public override void ImmEffect()
    {
        TargetEmp.ExtraSuccessRate -= 0.1f;
        TempValue4 = -0.1f;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.ExtraSuccessRate += 0.1f;
        TargetEmp.ExhaustedCount.Remove(Num);
    }
}

//重度抑郁
public class Perk95 : Perk
{
    public Perk95(Employee Emp) : base(Emp)
    {
        Name = "重度抑郁";
        Description = "心力每周下降10点";
        TimeLeft = -1;//特质
        Num = 95;
        canStack = false;
        effectType = EffectType.Weekly;
    }
    public override void ContinuousEffect()
    {
        TargetEmp.Mentality -= 10;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.ExhaustedCount.Remove(Num);
    }
}

//开悟
public class Perk96 : Perk
{
    public Perk96(Employee Emp) : base(Emp)
    {
        Name = "开悟";
        Description = "不产生红色系情绪，愤怒、反感、侮辱等";
        TimeLeft = -1;//特质
        Num = 96;
        canStack = false;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.ExhaustedCount.Remove(Num);
    }
}

//转岗无望
public class Perk97 : Perk
{
    public Perk97(Employee Emp) : base(Emp)
    {
        Name = "转岗无望";
        Description = "部门信念-15";
        TimeLeft = 96;
        Num = 97;
        canStack = true;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDep.DepFaith -= 15;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.DepFaith += 15;
    }
}

//升职无望
public class Perk98 : Perk
{
    public Perk98(Employee Emp) : base(Emp)
    {
        Name = "升职无望";
        Description = "部门信念-15";
        TimeLeft = 96;
        Num = 98;
        canStack = true;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDep.DepFaith -= 15;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.DepFaith += 15;
    }
}

//并肩作战
public class Perk99 : Perk
{
    public Perk99(Employee Emp) : base(Emp)
    {
        Name = "并肩作战";
        Description = "部门内每存在一组好友/挚友关系，信念+5";
        TimeLeft = -1;
        Num = 99;
        canStack = false;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        if(TargetDep != null)
        {
            List<Employee> TempEmps = new List<Employee>();
            foreach (Employee emp in TargetDep.CurrentEmps)
            {
                TempEmps.Add(emp);
            }
            if (TargetDep.CommandingOffice != null && TargetDep.CommandingOffice.Manager != null)
                TempEmps.Add(TargetDep.CommandingOffice.Manager);
            for (int i = 0; i < TempEmps.Count; i++)
            {
                for(int j = i + 1; j < TempEmps.Count; j++)
                {
                    if (TempEmps[i].FindRelation(TempEmps[j]).FriendValue > 0)
                        TempValue2 += 5;
                }
            }
            TempValue1 = TempValue2;
            foreach(PerkInfo perk in TargetDep.CurrentPerks)
            {
                if(perk.CurrentPerk.Num == 45)
                {
                    TempValue1 = TempValue2 * 3;
                    break;
                }
            }
            TargetDep.DepFaith += TempValue1;
            Description += "(+" + TempValue1 + ")";
        }
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.DepFaith -= TempValue1;
    }
}

//同室操戈
public class Perk100 : Perk
{
    public Perk100(Employee Emp) : base(Emp)
    {
        Name = "同室操戈";
        Description = "部门内每存在一组陌路/仇人关系，信念-10";
        TimeLeft = -1;
        Num = 100;
        canStack = false;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        if (TargetDep != null)
        {
            List<Employee> TempEmps = new List<Employee>();
            foreach (Employee emp in TargetDep.CurrentEmps)
            {
                TempEmps.Add(emp);
            }
            if (TargetDep.CommandingOffice != null && TargetDep.CommandingOffice.Manager != null)
                TempEmps.Add(TargetDep.CommandingOffice.Manager);
            for (int i = 0; i < TempEmps.Count; i++)
            {
                for (int j = i + 1; j < TempEmps.Count; j++)
                {
                    if (TempEmps[i].FindRelation(TempEmps[j]).FriendValue < 0)
                        TempValue1 += 10;
                }
            }
            TargetDep.DepFaith -= TempValue1;
            Description += "(-" + TempValue1 + ")";
        }
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.DepFaith += TempValue1;
    }
}

//信仰不一
public class Perk101 : Perk
{
    public Perk101(Employee Emp) : base(Emp)
    {
        Name = "信仰不一";
        Description = "同事和直属上级中存在信仰与其他人不同的，部门信念-15";
        TimeLeft = -1;
        Num = 101;
        canStack = false;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDep.DepFaith -= 15;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.DepFaith += 15;
    }
}

//信仰一致
public class Perk102 : Perk
{
    public Perk102(Employee Emp) : base(Emp)
    {
        Name = "信仰一致";
        Description = "同事和直属上级中信仰全部一致，部门信念+15";
        TimeLeft = -1;
        Num = 102;
        canStack = false;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDep.DepFaith += 15;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.DepFaith -= 15;
    }
}

//文化不一
public class Perk103 : Perk
{
    public Perk103(Employee Emp) : base(Emp)
    {
        Name = "文化不一";
        Description = "同事和直属上级中存在文化与其他人不同的，部门信念-15";
        TimeLeft = -1;
        Num = 103;
        canStack = false;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDep.DepFaith -= 15;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.DepFaith += 15;
    }
}

//文化一致
public class Perk104 : Perk
{
    public Perk104(Employee Emp) : base(Emp)
    {
        Name = "文化一致";
        Description = "同事和直属上级中文化全部一致，部门信念+15";
        TimeLeft = -1;
        Num = 104;
        canStack = false;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDep.DepFaith += 15;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.DepFaith -= 15;
    }
}

//重大失误
public class Perk105 : Perk
{
    public Perk105(Employee Emp) : base(Emp)
    {
        Name = "重大失误";
        Description = "部门信念-20";
        TimeLeft = 96;
        Num = 105;
        canStack = true;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDep.DepFaith -= 20;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.DepFaith += 20;
    }
}

//心理咨询
public class Perk106 : Perk
{
    public Perk106(Employee Emp) : base(Emp)
    {
        Name = "心理咨询";
        Description = "部门信念+25";
        TimeLeft = 64;
        Num = 106;
        canStack = true;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDep.DepFaith += 25;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.DepFaith -= 25;
    }
}

//遗憾
public class Perk107 : Perk
{
    public Perk107(Employee Emp) : base(Emp)
    {
        Name = "遗憾";
        Description = "愿望没有被满足而产生的遗憾";
        TimeLeft = -1;
        Num = 107;
        canStack = true;
    }
}

//生疏磨合
public class Perk108 : Perk
{
    public Perk108(Employee Emp) : base(Emp)
    {
        Name = "生疏磨合";
        Description = "部门内有新转岗进来一人，信念-15";
        TimeLeft = 16;
        Num = 108;
        canStack = true;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDep.DepFaith -= 15;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.DepFaith += 15;
    }
}

//紧急调离
public class Perk110 : Perk
{
    public Perk110(Employee Emp) : base(Emp)
    {
        Name = "紧急调离";
        Description = "部门内有新转岗转出一人，信念-15";
        TimeLeft = 16;
        Num = 110;
        canStack = true;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDep.DepFaith -= 15;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.DepFaith += 15;
    }
}

//空置部门
public class Perk111 : Perk
{
    public Perk111(Employee Emp) : base(Emp)
    {
        Name = "空置部门";
        Description = "降低部门30点信念,移入员工1周后移除";
        TimeLeft = -1;
        Num = 111;
        canStack = false;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDep.DepFaith -= 30;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.DepFaith += 30;
    }
}

//相互帮助
public class Perk112 : Perk
{
    public Perk112(Employee Emp) : base(Emp)
    {
        Name = "相互帮助";
        Description = "提高部门2点信念";
        TimeLeft = 16;
        Num = 112;
        canStack = true;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDep.DepFaith += 2;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.DepFaith -= 2;
    }
}

//产生矛盾
public class Perk113 : Perk
{
    public Perk113(Employee Emp) : base(Emp)
    {
        Name = "产生矛盾";
        Description = "降低部门2点信念";
        TimeLeft = 16;
        Num = 113;
        canStack = true;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDep.DepFaith -= 2;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.DepFaith += 2;
    }
}

//环境舒适
public class Perk114 : Perk
{
    public Perk114(Employee Emp) : base(Emp)
    {
        Name = "环境舒适";
        Description = "提高部门2点信念";
        TimeLeft = -1;
        Num = 114;
        canStack = true;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDep.DepFaith += 2;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.DepFaith -= 2;
    }
}

//老板摸鱼
public class Perk115 : Perk
{
    public Perk115(Employee Emp) : base(Emp)
    {
        Name = "老板摸鱼";
        Description = "降低部门35点信念,持续到CEO放假结束";
        TimeLeft = -1;
        Num = 115;
        canStack = false;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDep.DepFaith -= 35;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.DepFaith += 35;
    }
}

//业务干扰
public class Perk116 : Perk
{
    public Perk116(Employee Emp) : base(Emp)
    {
        Name = "业务干扰";
        Description = "降低部门30点信念";
        TimeLeft = 96;
        Num = 116;
        canStack = false;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDep.DepFaith -= 30;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.DepFaith += 30;
    }
}

//亲自指导
public class Perk117 : Perk
{
    public Perk117(Employee Emp) : base(Emp)
    {
        Name = "亲自指导";
        Description = "部门成功率上升45%";
        TimeLeft = 128;
        Num = 117;
        canStack = false;
        perkColor = PerkColor.White;
    }
    public override void ImmEffect()
    {
        TargetDep.Efficiency += 0.45f;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.Efficiency -= 0.45f;
    }
}

//心力低下
public class Perk119 : Perk
{
    public Perk119(Employee Emp) : base(Emp)
    {
        Name = "心力低下";
        Description = "员工心力<20";
        TimeLeft = -1;
        Num = 119;
        canStack = false;
    }
}

//放轻松
public class Perk120 : Perk
{
    public Perk120(Employee Emp) : base(Emp)
    {
        Name = "放轻松";
        Description = "成功率上升10%";
        TimeLeft = 31;
        Num = 120;
        canStack = false;
        TempValue1 = 1;
        perkColor = PerkColor.White;
    }

    public override void ImmEffect()
    {
        TargetDep.BaseWorkStatus += TempValue1;
        Description = "成功率上升" + TempValue1 + "点";
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.BaseWorkStatus -= TempValue1;
    }
}

//勇气赞歌
public class Perk121 : Perk
{
    public Perk121(Employee Emp) : base(Emp)
    {
        Name = "勇气赞歌";
        Description = "信念+15";
        TimeLeft = 31;
        Num = 121;
        canStack = false;
        TempValue1 = 15;
        perkColor = PerkColor.Orange;
    }

    public override void ImmEffect()
    {
        TargetDep.DepFaith += TempValue1;
        Description = "信念+" + TempValue1;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.DepFaith -= TempValue1;
    }
}

//加班加班
public class Perk122 : Perk
{
    public Perk122(Employee Emp) : base(Emp)
    {
        Name = "加班加班";
        Description = "效率上升25%";
        TimeLeft = 31;
        Num = 122;
        canStack = false;
        TempValue4 = 0.25f;
        perkColor = PerkColor.Grey;
    }

    public override void ImmEffect()
    {
        TargetDep.Efficiency += TempValue4;
        Description = "效率上升" + (TempValue4 * 100) + "%";
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.Efficiency -= TempValue4;
    }
}

//流程优化
public class Perk123 : Perk
{
    public Perk123(Employee Emp) : base(Emp)
    {
        Name = "流程优化";
        Description = "部门成本-25%";
        TimeLeft = 31;
        Num = 123;
        canStack = false;
        TempValue4 = 0.25f;
        perkColor = PerkColor.Blue;
    }

    public override void ImmEffect()
    {
        TargetDep.SalaryMultiply -= TempValue4;
        TargetDep.BuildingPayMultiply -= TempValue4;
        Description = "部门成本-" + (TempValue4 * 100) + "%";
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.SalaryMultiply += TempValue4;
        TargetDep.BuildingPayMultiply += TempValue4;
    }
}

//无意义争执
public class Perk124 : Perk
{
    public Perk124(Employee Emp) : base(Emp)
    {
        Name = "无意义争执";
        Description = "信念-30";
        TimeLeft = 31;
        Num = 124;
        canStack = false;
        TempValue1 = 30;
        perkColor = PerkColor.Orange;
    }

    public override void ImmEffect()
    {
        TargetDep.DepFaith -= TempValue1;
        Description = "信念-" + TempValue1;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.DepFaith += TempValue1;
    }
}

//单领导指令
public class Perk125 : Perk
{
    public Perk125(Employee Emp) : base(Emp)
    {
        Name = "单领导指令";
        Description = "部门内全部员工每周结束时体力-10";
        TimeLeft = 31;
        Num = 125;
        canStack = false;
    }

    public override void ImmEffect()
    {
        TargetDep.StaminaExtra -= 10;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.StaminaExtra += 10;
    }
}

//双领导指令
public class Perk126 : Perk
{
    public Perk126(Employee Emp) : base(Emp)
    {
        Name = "双领导指令";
        Description = "部门内全部员工每周结束时体力-20";
        TimeLeft = 31;
        Num = 126;
        canStack = false;
    }

    public override void ImmEffect()
    {
        TargetDep.StaminaExtra -= 20;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.StaminaExtra += 20;
    }
}

//三领导指令
public class Perk127 : Perk
{
    public Perk127(Employee Emp) : base(Emp)
    {
        Name = "三领导指令";
        Description = "部门内全部员工每周结束时体力-30";
        TimeLeft = 31;
        Num = 127;
        canStack = false;
    }

    public override void ImmEffect()
    {
        TargetDep.StaminaExtra -= 30;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.StaminaExtra += 30;
    }
}

//身体羸弱
public class Perk128 : Perk
{
    public Perk128(Employee Emp) : base(Emp)
    {
        Name = "身体羸弱";
        Description = "强壮-2";
        TimeLeft = -1;
        Num = 128;
        canStack = false;
    }

    public override void ImmEffect()
    {
        TargetEmp.Strength -= 2;
        if (TargetEmp.Strength < 0)
            TargetEmp.Strength = 0;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.Strength += 2;

    }
}

//狮子之心
public class Perk129 : Perk
{
    public Perk129(Employee Emp) : base(Emp)
    {
        Name = "狮子之心";
        Description = "坚韧+2";
        TimeLeft = -1;
        Num = 129;
        canStack = false;
    }

    public override void ImmEffect()
    {
        TargetEmp.Tenacity += 2;
        if (TargetEmp.Tenacity < 0)
            TargetEmp.Tenacity = 0;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.Tenacity -= 2;

    }
}

//热爱运动
public class Perk130 : Perk
{
    public Perk130(Employee Emp) : base(Emp)
    {
        Name = "热爱运动";
        Description = "强壮+2";
        TimeLeft = -1;
        Num = 130;
        canStack = false;
    }

    public override void ImmEffect()
    {
        TargetEmp.Strength += 2;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.Strength -= 2;
        if (TargetEmp.Strength < 0)
            TargetEmp.Strength = 0;
    }
}

//养尊处优
public class Perk131 : Perk
{
    public Perk131(Employee Emp) : base(Emp)
    {
        Name = "养尊处优";
        Description = "坚韧-2";
        TimeLeft = -1;
        Num = 131;
        canStack = false;
    }

    public override void ImmEffect()
    {
        TargetEmp.Tenacity -= 2;
        if (TargetEmp.Tenacity < 0)
            TargetEmp.Tenacity = 0;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.Tenacity += 2;

    }
}

//结构化思维
public class Perk132 : Perk
{
    public Perk132(Employee Emp) : base(Emp)
    {
        Name = "结构化思维";
        Description = "决策+1";
        TimeLeft = -1;
        Num = 132;
        canStack = false;
    }

    public override void ImmEffect()
    {
        TargetEmp.Decision += 1;
        if (TargetEmp.Decision < 0)
            TargetEmp.Decision = 0;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.Decision -= 1;
    }
}

//孤僻
public class Perk133 : Perk
{
    public Perk133(Employee Emp) : base(Emp)
    {
        Name = "孤僻";
        Description = "管理-1，第一份工作技能等级+1";
        TimeLeft = -1;
        Num = 133;
        canStack = false;
    }

    public override void ImmEffect()
    {
        TargetEmp.Manage -= 2;
        if (TargetEmp.Manage < 0)
            TargetEmp.Manage = 0;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.Manage += 2;
    }
}


//魔鬼筋肉人(金色特质)
public class Perk134 : Perk
{
    public Perk134(Employee Emp) : base(Emp)
    {
        Name = "魔鬼筋肉人(金)";
        Description = "成为核心团队成员后生效，增加CEO强壮2点";
        TimeLeft = -1;
        Num = 134;
        canStack = false;
    }

    public override void ImmEffect()
    {

    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();

    }
}


//智囊(金色特质)
public class Perk135 : Perk
{
    public Perk135(Employee Emp) : base(Emp)
    {
        Name = "智囊(金)";
        Description = "成为核心团队成员后生效，增加CEO决策2点";
        TimeLeft = -1;
        Num = 135;
        canStack = false;
        ManagePerk = true;
    }

    public override void ImmEffect()
    {

    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();

    }
}

//脱口秀演员(金色特质)
public class Perk136 : Perk
{
    public Perk136(Employee Emp) : base(Emp)
    {
        Name = "脱口秀演员(金)";
        Description = "成为核心团队成员后生效，增加CEO坚韧2点";
        TimeLeft = -1;
        Num = 136;
        canStack = false;
        ManagePerk = true;
    }

    public override void ImmEffect()
    {

    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();

    }
}

//管理咨询师(金色特质)
public class Perk137 : Perk
{
    public Perk137(Employee Emp) : base(Emp)
    {
        Name = "管理咨询师(金)";
        Description = "成为核心团队成员后生效，增加CEO管理2点";
        TimeLeft = -1;
        Num = 137;
        canStack = false;
        ManagePerk = true;
    }

    public override void ImmEffect()
    {

    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();

    }
}

//怠惰(金色特质)
public class Perk138 : Perk
{
    public Perk138(Employee Emp) : base(Emp)
    {
        Name = "怠惰(金)";
        Description = "成为核心团队成员后生效，减少CEO强壮2点";
        TimeLeft = -1;
        Num = 138;
        canStack = false;
        ManagePerk = true;
    }

    public override void ImmEffect()
    {

    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();

    }
}

//神经质(金色特质)
public class Perk139 : Perk
{
    public Perk139(Employee Emp) : base(Emp)
    {
        Name = "神经质(金)";
        Description = "成为核心团队成员后生效，减少CEO坚韧2点";
        TimeLeft = -1;
        Num = 139;
        canStack = false;
        ManagePerk = true;
    }

    public override void ImmEffect()
    {

    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();

    }
}

//独断专行(金色特质)
public class Perk140 : Perk
{
    public Perk140(Employee Emp) : base(Emp)
    {
        Name = "独断专行(金)";
        Description = "自身管理-1，成为核心团队成员后生效，减少CEO管理2点";
        TimeLeft = -1;
        Num = 140;
        canStack = false;
        ManagePerk = true;
    }

    public override void ImmEffect()
    {
        TargetEmp.Manage -= 1;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.Manage += 1;
    }
}

//选择困难症(金色特质)
public class Perk141 : Perk
{
    public Perk141(Employee Emp) : base(Emp)
    {
        Name = "选择困难症(金)";
        Description = "成为核心团队成员后生效，减少CEO决策2点";
        TimeLeft = -1;
        Num = 141;
        canStack = false;
        ManagePerk = true;
    }

    public override void ImmEffect()
    {

    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
    }
}

//瓶颈
public class Perk142 : Perk
{
    public Perk142(Employee Emp) : base(Emp)
    {
        Name = "瓶颈";
        Description = "每拥有1层“瓶颈”特质，升级时所需经验增加800点";
        TimeLeft = -1;
        Num = 142;
        canStack = true;
    }

    public override void ImmEffect()
    {
        //TargetEmp.ExtraExp += AdjustData.BottleneckValue;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        //TargetEmp.ExtraExp -= AdjustData.BottleneckValue;
    }
}

//混乱与创造
public class Perk143 : Perk
{
    public Perk143(Employee Emp) : base(Emp)
    {
        Name = "混乱与创造";
        Description = "部门工作状态-3，大成功率增加30%";
        TimeLeft = 64;
        Num = 143;
        canStack = false;
        TempValue1 = 1;
        perkColor = PerkColor.White;
    }

    public override void ImmEffect()
    {
        TargetDep.BaseWorkStatus -= 3 * TempValue1;
        TargetDep.DepBaseMajorSuccessRate += 0.3f * TempValue1;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.BaseWorkStatus += 3 * TempValue1;
        TargetDep.DepBaseMajorSuccessRate -= 0.3f * TempValue1;
    }
}


//必胜信念
public class Perk144 : Perk
{
    public Perk144(Employee Emp) : base(Emp)
    {
        Name = "必胜信念";
        Description = "部门信念下降30，工作状态+2";
        TimeLeft = -1;
        Num = 144;
        canStack = false;
        TempValue1 = 1;
        perkColor = PerkColor.White;
    }

    public override void ImmEffect()
    {
        TargetDep.BaseWorkStatus += 2 * TempValue1;
        TargetDep.DepFaith -= 30 * TempValue1;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.BaseWorkStatus -= 2 * TempValue1;
        TargetDep.DepFaith += 30 * TempValue1;
    }
}

//机器人员工
public class Perk145 : Perk
{
    public Perk145(Employee Emp) : base(Emp)
    {
        Name = "机器人员工";
        Description = "部门效率上升25%";
        TimeLeft = 64;
        Num = 145;
        canStack = false;
        TempValue1 = 1;
        perkColor = PerkColor.Grey;
    }

    public override void ImmEffect()
    {
        TargetDep.Efficiency += 0.25f * TempValue1;
        //Perk146额外检测
        foreach (PerkInfo perk in TargetDep.CurrentPerks)
        {
            if (perk.CurrentPerk.Num == 146)
            {
                TargetDep.Efficiency += 0.15f * perk.CurrentPerk.TempValue1;
                break;
            }
        }
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.Efficiency -= 0.25f * TempValue1;
        //Perk146额外检测
        foreach (PerkInfo perk in TargetDep.CurrentPerks)
        {
            if (perk.CurrentPerk.Num == 146)
            {
                TargetDep.Efficiency -= 0.15f * perk.CurrentPerk.TempValue1;
                break;
            }
        }
    }
}

//机械检修
public class Perk146 : Perk
{
    public Perk146(Employee Emp) : base(Emp)
    {
        Name = "机械检修";
        Description = "拥有机器人员工时效率额外+15%";
        TimeLeft = -1;
        Num = 146;
        canStack = false;
        TempValue1 = 1;
        perkColor = PerkColor.Grey;
    }

    public override void ImmEffect()
    {
        foreach(PerkInfo perk in TargetDep.CurrentPerks)
        {
            if(perk.CurrentPerk.Num == 145)
            {
                TargetDep.Efficiency += 0.15f * TempValue1;
                break;
            }
        }
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        foreach (PerkInfo perk in TargetDep.CurrentPerks)
        {
            if (perk.CurrentPerk.Num == 145)
            {
                TargetDep.Efficiency -= 0.15f * TempValue1;
                break;
            }
        }
    }
}

//肌体强化
public class Perk147 : Perk
{
    public Perk147(Employee Emp) : base(Emp)
    {
        Name = "肌体强化";
        Description = "部门内员工每周体力消耗减少30%";
        TimeLeft = -1;
        Num = 147;
        canStack = false;
        TempValue1 = 1;
    }

    public override void ImmEffect()
    {
        TargetDep.StaminaCostRate -= 0.3f * TempValue1;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.StaminaCostRate += 0.3f * TempValue1;
    }
}

//自由之翼
public class Perk148 : Perk
{
    public Perk148(Employee Emp) : base(Emp)
    {
        Name = "自由之翼";
        Description = "随机增加1点天赋";
        TimeLeft = -1;
        Num = 148;
        canStack = false;
        TempValue1 = 1;
    }

    public override void ImmEffect()
    {
        TempValue1 = Random.Range(0, 3);
        //TargetEmp.StarLimit[TargetEmp.Professions[TempValue1] - 1] += 1 * TempValue1;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        //TargetEmp.StarLimit[TargetEmp.Professions[TempValue1] - 1] -= 1 * TempValue1;
    }
}

//脑机芯片
public class Perk149 : Perk
{
    public Perk149(Employee Emp) : base(Emp)
    {
        Name = "脑机芯片";
        Description = "随机一项职业技能增加3点";
        TimeLeft = 64;
        Num = 149;
        canStack = false;
        TempValue1 = 1;
    }

    public override void ImmEffect()
    {
        int limit = 0;
        foreach(int a in TargetEmp.Professions)
        {
            if (a != 0)
                limit++;
        }
        TempValue1 = Random.Range(0, limit);
        //TargetEmp.ExtraAttributes[TargetEmp.Professions[TempValue1] - 1] += 3 * TempValue1;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        //TargetEmp.ExtraAttributes[TargetEmp.Professions[TempValue1] - 1] -= 3 * TempValue1;
    }
}

//胜利开发
public class Perk150 : Perk
{
    public Perk150(Employee Emp) : base(Emp)
    {
        Name = "胜利开发";
        Description = "提升其工作状态1点，持续到下一次头脑风暴";
        TimeLeft = -1;
        Num = 150;
        canStack = true;
        perkColor = PerkColor.White;
    }

    public override void ImmEffect()
    {
        TargetDep.BaseWorkStatus += 1;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.BaseWorkStatus -= 1;
    }
}

//节省支出
public class Perk151 : Perk
{
    public Perk151(Employee Emp) : base(Emp)
    {
        Name = "节省支出";
        Description = "降低人员工资10%，持续到下一次头脑风暴";
        TimeLeft = -1;
        Num = 151;
        canStack = true;
        perkColor = PerkColor.Blue;
    }

    public override void ImmEffect()
    {
        TargetDep.SalaryMultiply -= 0.1f;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.SalaryMultiply += 0.1f;
    }
}

//信仰充值
public class Perk152 : Perk
{
    public Perk152(Employee Emp) : base(Emp)
    {
        Name = "信仰充值";
        Description = "提升信念5点，持续到下一次头脑风暴";
        TimeLeft = -1;
        Num = 152;
        canStack = true;
        perkColor = PerkColor.Orange;
    }

    public override void ImmEffect()
    {
        TargetDep.DepFaith += 5;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.DepFaith -= 5;
    }
}

//效率至上
public class Perk153 : Perk
{
    public Perk153(Employee Emp) : base(Emp)
    {
        Name = "效率至上";
        Description = "提升效率5%，持续到下一次头脑风暴";
        TimeLeft = -1;
        Num = 153;
        canStack = true;
        perkColor = PerkColor.Grey;
    }

    public override void ImmEffect()
    {
        TargetDep.Efficiency += 0.05f;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDep.Efficiency -= 0.05f;
    }
}
#endregion
public static class PerkData
{

    //一般特质
    public static List<Perk> PerkList = new List<Perk>()
    {
        new Perk12(null), new Perk13(null), new Perk14(null), new Perk15(null), new Perk16(null), new Perk17(null),
        new Perk18(null), new Perk19(null), new Perk20(null), new Perk21(null), new Perk22(null),
        new Perk23(null)
    };

    //经历特质
    public static List<Perk> DefaultPerkList = new List<Perk>()
    {
        new Perk46(null), new Perk47(null), new Perk48(null), new Perk49(null), new Perk50(null), new Perk51(null)
    };

    //职业特质
    public static List<Perk> OccupationPerkList = new List<Perk>()
    {
        new Perk35(null), new Perk36(null), new Perk37(null), new Perk38(null), new Perk39(null), new Perk40(null),
        new Perk41(null), new Perk42(null), new Perk43(null)
    };

    //管理特质
    public static List<Perk> ManagePerkList = new List<Perk>()
    {
         new Perk1(null), new Perk2(null), new Perk3(null), new Perk4(null), new Perk5(null), new Perk6(null),
        new Perk7(null), new Perk8(null), new Perk9(null), new Perk10(null), new Perk11(null)
        , new Perk24(null), new Perk25(null), new Perk26(null), new Perk27(null), new Perk28(null),
        new Perk29(null), new Perk30(null), new Perk31(null), new Perk32(null)
    };
}