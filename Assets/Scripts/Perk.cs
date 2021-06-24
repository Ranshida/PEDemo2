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
    public bool OptionCardPerk = false;//是否为生成抉择卡的特质
    public string Name, Description;
    public EffectType effectType;
    public PerkColor perkColor = PerkColor.None;

    public Employee TargetEmp;
    public DepControl TargetDep;
    public DivisionControl TargetDiv;
    public PerkInfo Info;
    public OptionCardInfo card;

    public int TempValue1, TempValue2, TempValue3;//用于检测和保存的一些数值，在事业部影响效果中，1储存工作状态 2储存效率 3储存信念
    public float TempValue4;

    private bool StackAdded = false;

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
            GameControl.Instance.TurnEvent.AddListener(TimePass);
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
        if (card != null)
        {
            GameControl.Instance.OCL.CurrentOptions.Remove(card);
            Object.Destroy(card.gameObject);
            card = null;
        }
        if (canStack == false || Level == 0)
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
        GameControl.Instance.TurnEvent.RemoveListener(TimePass);
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

//整理癖
public class Perk1 : Perk
{
    public Perk1() : base()
    {
        Name = "整理癖";
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

//包容
public class Perk2 : Perk
{
    public Perk2() : base()
    {
        Name = "包容";
        Description = "成为核心团队成员后生效，增加公司不满栏位上限1点";
        TimeLeft = -1;
        Num = 2;
        ManagePerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        GameControl.Instance.DissatisfiedLimit += 1;
    }

    public override void DeActiveSpecialEffect()
    {
        GameControl.Instance.DissatisfiedLimit -= 1;
    }
}

//调停高手
public class Perk3 : Perk
{
    public Perk3() : base()
    {
        Name = "调停高手";
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

//正义伙伴
public class Perk4 : Perk
{
    public Perk4() : base()
    {
        Name = "正义伙伴";
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

//稳健
public class Perk5 : Perk
{
    public Perk5() : base()
    {
        Name = "稳健";
        Description = "成为高管后生效，管理的事业部信念+10";
        TimeLeft = -1;
        Num = 5;
        TempValue3 = 10;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        TargetEmp.CurrentDivision.Faith += TempValue3;
    }

    public override void DeActiveSpecialEffect()
    {
        TargetEmp.CurrentDivision.Faith -= TempValue3;
    }
}

//决断者
public class Perk6 : Perk
{
    public Perk6() : base()
    {
        Name = "决断者";
        Description = "成为高管后生效，管理的事业部效率+2";
        TimeLeft = -1;
        Num = 6;
        TempValue2 = 2;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        TargetEmp.CurrentDivision.Efficiency += TempValue2;
    }

    public override void DeActiveSpecialEffect()
    {
        TargetEmp.CurrentDivision.Efficiency -= TempValue2;
    }
}

//睿智
public class Perk7 : Perk
{
    public Perk7() : base()
    {
        Name = "睿智";
        Description = "成为高管后生效，管理的事业部工作状态+2";
        TimeLeft = -1;
        Num = 7;
        TempValue1 = 2;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        TargetEmp.CurrentDivision.WorkStatus += TempValue1;
    }

    public override void DeActiveSpecialEffect()
    {
        TargetEmp.CurrentDivision.WorkStatus -= TempValue1;
    }
}

//黄金账本
public class Perk8 : Perk
{
    public Perk8() : base()
    {
        Name = "黄金账本";
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

//流程优化专家
public class Perk9 : Perk
{
    public Perk9() : base()
    {
        Name = "流程优化专家";
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

//手段老练(未实装)
public class Perk10 : Perk
{
    public Perk10() : base()
    {
        Name = "手段老练";
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

//绩效教练
public class Perk11 : Perk
{
    public Perk11() : base()
    {
        Name = "绩效教练";
        Description = "成为高管后生效，管理的事业部中员工的经验值每回合额外获得2点经验";
        TimeLeft = -1;
        Num = 11;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        TargetEmp.CurrentDivision.ManagerExtraExp += 2;
    }

    public override void DeActiveSpecialEffect()
    {
        TargetEmp.CurrentDivision.ManagerExtraExp -= 2;
    }
}

//高效
public class Perk12 : Perk
{
    public Perk12() : base()
    {
        Name = "高效";
        Description = "所在事业部效率+1";
        TimeLeft = -1;
        Num = 12;
        TempValue2 = 1;
        DepPerk = true;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
            TargetEmp.CurrentDep.CurrentDivision.Efficiency += TempValue2;
        else if (TargetEmp.CurrentDivision != null)
            TargetEmp.CurrentDivision.Efficiency += TempValue2;
    }

    public override void DeActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
            TargetEmp.CurrentDep.CurrentDivision.Efficiency -= TempValue2;
        else if (TargetEmp.CurrentDivision != null)
            TargetEmp.CurrentDivision.Efficiency -= TempValue2;
    }
}

//奇思妙想
public class Perk13 : Perk
{
    public Perk13() : base()
    {
        Name = "奇思妙想";
        Description = "所在事业部工作状态+1";
        TimeLeft = -1;
        Num = 13;
        TempValue1 = 1;
        DepPerk = true;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
            TargetEmp.CurrentDep.CurrentDivision.WorkStatus += TempValue1;
        else if (TargetEmp.CurrentDivision != null)
            TargetEmp.CurrentDivision.WorkStatus += TempValue1;
    }

    public override void DeActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
            TargetEmp.CurrentDep.CurrentDivision.WorkStatus -= TempValue1;
        else if (TargetEmp.CurrentDivision != null)
            TargetEmp.CurrentDivision.WorkStatus -= TempValue1;
    }
}

//乐观
public class Perk14 : Perk
{
    public Perk14() : base()
    {
        Name = "乐观";
        Description = "所在事业部信念+10";
        TimeLeft = -1;
        Num = 14;
        TempValue3 = 10;
        DepPerk = true;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
            TargetEmp.CurrentDep.CurrentDivision.Faith += TempValue3;
        else if (TargetEmp.CurrentDivision != null)
            TargetEmp.CurrentDivision.Faith += TempValue3;
    }

    public override void DeActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
            TargetEmp.CurrentDep.CurrentDivision.Faith -= TempValue3;
        else if (TargetEmp.CurrentDivision != null)
            TargetEmp.CurrentDivision.Faith -= TempValue3;
    }
}

//人形扑满
public class Perk15 : Perk
{
    public Perk15() : base()
    {
        Name = "人形扑满";
        Description = "所在事业部成本-5";
        TimeLeft = -1;
        Num = 15;
        DepPerk = true;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
            TargetEmp.CurrentDep.CurrentDivision.ExtraCost -= 5;
        else if (TargetEmp.CurrentDivision != null)
            TargetEmp.CurrentDivision.ExtraCost -= 5;
    }

    public override void DeActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
            TargetEmp.CurrentDep.CurrentDivision.ExtraCost += 5;
        else if (TargetEmp.CurrentDivision != null)
            TargetEmp.CurrentDivision.ExtraCost += 5;
    }
}

//冷静
public class Perk16 : Perk
{
    public Perk16() : base()
    {
        Name = "冷静";
        Description = "自身参与的事件修正+1";
        TimeLeft = -1;
        Num = 16;
    }
}

//开朗
public class Perk17 : Perk
{
    public Perk17() : base()
    {
        Name = "开朗";
        Description = "自身参与的事件修正+1";
        TimeLeft = -1;
        Num = 17;
    }

    public override void ImmEffect()
    {
        TargetEmp.SelfEventCorrection += 1;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.SelfEventCorrection -= 1;
    }
}

//敏感
public class Perk18 : Perk
{
    public Perk18() : base()
    {
        Name = "敏感";
        Description = "更容易产生情绪（获取情绪时有50%概率多获得1个）";
        TimeLeft = -1;
        Num = 18;
    }

    public override void ImmEffect()
    {
        TargetEmp.MultiEmotion = true;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.MultiEmotion = false;
    }
}

//热情(未实装)
public class Perk19 : Perk
{
    public Perk19() : base()
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
    public Perk20() : base()
    {
        Name = "电波系";
        Description = "办公室中存在两名持有此特质的员工时效率+2";
        TimeLeft = -1;
        Num = 20;
        TempValue2 = 0;
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
        {
            TempValue2 = 2;
            if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
                TargetEmp.CurrentDep.CurrentDivision.Efficiency += TempValue2;
        }
    }

    public override void DeActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
            TargetEmp.CurrentDep.CurrentDivision.Efficiency -= TempValue2;
    }
}

//耐心
public class Perk21 : Perk
{
    public Perk21() : base()
    {
        Name = "耐心";
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
    public Perk22() : base()
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

//心如止水
public class Perk23 : Perk
{
    public Perk23() : base()
    {
        Name = "心如止水";
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

//预言家
public class Perk24 : Perk
{
    public Perk24() : base()
    {
        Name = "预言家";
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

//谋士
public class Perk25 : Perk
{
    public Perk25() : base()
    {
        Name = "谋士";
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

//扭曲现实力场
public class Perk26 : Perk
{
    public Perk26() : base()
    {
        Name = "扭曲现实力场";
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

//谈判专家
public class Perk27 : Perk
{
    public Perk27() : base()
    {
        Name = "谈判专家";
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

//同理心
public class Perk28 : Perk
{
    public Perk28() : base()
    {
        Name = "同理心";
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

//坦诚
public class Perk29 : Perk
{
    public Perk29() : base()
    {
        Name = "坦诚";
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

//全局之眼
public class Perk30 : Perk
{
    public Perk30() : base()
    {
        Name = "全局之眼";
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

//端水大师
public class Perk31 : Perk
{
    public Perk31() : base()
    {
        Name = "端水大师";
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

//制衡之手
public class Perk32 : Perk
{
    public Perk32() : base()
    {
        Name = "制衡之手";
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
    public Perk35() : base()
    {
        Name = "自由者";
        Description = "厌恶管理，所在事业部高管管理-1";
        TimeLeft = -1;
        Num = 35;
        DepPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
            TargetEmp.CurrentDep.CurrentDivision.ExtraManage -= 1;
    }

    public override void DeActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
            TargetEmp.CurrentDep.CurrentDivision.ExtraManage += 1;
    }
}

//服从
public class Perk36 : Perk
{
    public Perk36() : base()
    {
        Name = "服从";
        Description = "所在事业部工作状态-1，效率+1";
        TimeLeft = -1;
        Num = 36;
        TempValue1 = -1;
        TempValue2 = 1;
        DepPerk = true;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
        {
            TargetEmp.CurrentDep.CurrentDivision.WorkStatus += TempValue1;
            TargetEmp.CurrentDep.CurrentDivision.Efficiency += TempValue2;
        }
        else if (TargetEmp.CurrentDivision != null)
        {
            TargetEmp.CurrentDivision.WorkStatus += TempValue1;
            TargetEmp.CurrentDivision.Efficiency += TempValue2;
        }
    }

    public override void DeActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
        {
            TargetEmp.CurrentDep.CurrentDivision.WorkStatus -= TempValue1;
            TargetEmp.CurrentDep.CurrentDivision.Efficiency -= TempValue2;
        }
        else if (TargetEmp.CurrentDivision != null)
        {
            TargetEmp.CurrentDivision.WorkStatus -= TempValue1;
            TargetEmp.CurrentDivision.Efficiency -= TempValue2;
        }
    }
}

//待遇要求高
public class Perk37 : Perk
{
    public Perk37() : base()
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
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
            TargetEmp.CurrentDep.CurrentDivision.ExtraCost += 5;
        else if (TargetEmp.CurrentDivision != null)
            TargetEmp.CurrentDivision.ExtraCost += 5;
    }

    public override void DeActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
            TargetEmp.CurrentDep.CurrentDivision.ExtraCost -= 5;
        else if (TargetEmp.CurrentDivision != null)
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
    public Perk38() : base()
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
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
            TargetEmp.CurrentDep.CurrentDivision.ExtraCost += 5;
        else if (TargetEmp.CurrentDivision != null)
            TargetEmp.CurrentDivision.ExtraCost += 5;
    }

    public override void DeActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
            TargetEmp.CurrentDep.CurrentDivision.ExtraCost -= 5;
        else if (TargetEmp.CurrentDivision != null)
            TargetEmp.CurrentDivision.ExtraCost -= 5;
    }
}

//用爱发电
public class Perk39 : Perk
{
    public Perk39() : base()
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
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
            TargetEmp.CurrentDep.CurrentDivision.ExtraCost -= 5;
        else if (TargetEmp.CurrentDivision != null)
            TargetEmp.CurrentDivision.ExtraCost -= 5;
    }

    public override void DeActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
            TargetEmp.CurrentDep.CurrentDivision.ExtraCost += 5;
        else if (TargetEmp.CurrentDivision != null)
            TargetEmp.CurrentDivision.ExtraCost += 5;
    }
}

//意见领袖
public class Perk40 : Perk
{
    public Perk40() : base()
    {
        Name = "意见领袖";
        Description = "所在事业部信念+10，所在事业部高管管理-1";
        TimeLeft = -1;
        Num = 40;
        TempValue3 = 10;
        DepPerk = true;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
        {
            TargetEmp.CurrentDep.CurrentDivision.Faith += TempValue3;
            TargetEmp.CurrentDep.CurrentDivision.ExtraManage -= 1;
        }
        else if (TargetEmp.CurrentDivision != null)
        {
            TargetEmp.CurrentDivision.Faith += TempValue3;
            TargetEmp.CurrentDivision.ExtraManage -= 1;
        }
    }

    public override void DeActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
        {
            TargetEmp.CurrentDep.CurrentDivision.Faith -= TempValue3;
            TargetEmp.CurrentDep.CurrentDivision.ExtraManage += 1;
        }
        else if (TargetEmp.CurrentDivision != null)
        {
            TargetEmp.CurrentDivision.Faith -= TempValue3;
            TargetEmp.CurrentDivision.ExtraManage += 1;
        }
    }
}

//返工受害者
public class Perk41 : Perk
{
    public Perk41() : base()
    {
        Name = "返工受害者";
        Description = "所在事业部工作状态+1，信念-10";
        TimeLeft = -1;
        Num = 41;
        TempValue1 = 1;
        TempValue3 = -10;
        DepPerk = true;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
        {
            TargetEmp.CurrentDep.CurrentDivision.WorkStatus += TempValue1;
            TargetEmp.CurrentDep.CurrentDivision.Faith += TempValue3;
        }
        else if (TargetEmp.CurrentDivision != null)
        {
            TargetEmp.CurrentDivision.WorkStatus += TempValue1;
            TargetEmp.CurrentDivision.Faith += TempValue3;
        }
    }

    public override void DeActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
        {
            TargetEmp.CurrentDep.CurrentDivision.WorkStatus -= TempValue1;
            TargetEmp.CurrentDep.CurrentDivision.Faith -= TempValue3;
        }
        else if (TargetEmp.CurrentDivision != null)
        {
            TargetEmp.CurrentDivision.WorkStatus -= TempValue1;
            TargetEmp.CurrentDivision.Faith -= TempValue3;
        }
    }
}

//滔滔不绝
public class Perk42 : Perk
{
    public Perk42() : base()
    {
        Name = "滔滔不绝";
        Description = "所在事业部效率-1，自身坚韧+1";
        TimeLeft = -1;
        Num = 42;
        TempValue2 = -1;
        DepPerk = true;
        DivisionPerk = true;
    }

    public override void ActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
            TargetEmp.CurrentDep.CurrentDivision.Efficiency += TempValue2;
        else if (TargetEmp.CurrentDivision != null)
            TargetEmp.CurrentDivision.Efficiency += TempValue2;
    }

    public override void DeActiveSpecialEffect()
    {
        if (TargetEmp.CurrentDep != null && TargetEmp.CurrentDep.CurrentDivision != null)
            TargetEmp.CurrentDep.CurrentDivision.Efficiency -= TempValue2;
        else if (TargetEmp.CurrentDivision != null)
            TargetEmp.CurrentDivision.Efficiency -= TempValue2;
    }

    public override void ImmEffect()
    {
        TargetEmp.Tenacity += 1;
    }
}

//见多识广
public class Perk43 : Perk
{
    public Perk43() : base()
    {
        Name = "见多识广";
        Description = "参与个人事件修正+1";
        TimeLeft = -1;
        Num = 43;
    }

    public override void ImmEffect()
    {
        TargetEmp.SelfEventCorrection += 1;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.SelfEventCorrection -= 1;
    }
}

//御宅族
public class Perk46 : Perk
{
    public Perk46() : base()
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
    public Perk47() : base()
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
    public Perk48() : base()
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
    public Perk49() : base()
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
    public Perk50() : base()
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
    public Perk51() : base()
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

//回避冲突
public class Perk52 : Perk
{
    public Perk52() : base()
    {
        Name = "甩锅高手";
        Description = "获得负面抉择卡-推卸责任”";
        TimeLeft = -1;
        Num = 52;
    }

    public override void ImmEffect()
    {
        GameControl.Instance.OCL.AddOptionCard(new OptionCard5(), TargetEmp);
        card = GameControl.Instance.OCL.CurrentOptions[GameControl.Instance.OCL.CurrentOptions.Count - 1];
    }
}

//闪烁其词
public class Perk53 : Perk
{
    public Perk53() : base()
    {
        Name = "官僚主义";
        Description = "获得负面抉择卡-转移话题";
        TimeLeft = -1;
        Num = 53;
    }

    public override void ImmEffect()
    {
        GameControl.Instance.OCL.AddOptionCard(new OptionCard6(), TargetEmp);
        card = GameControl.Instance.OCL.CurrentOptions[GameControl.Instance.OCL.CurrentOptions.Count - 1];
    }
}

//拒绝沟通
public class Perk54 : Perk
{
    public Perk54() : base()
    {
        Name = "拒绝沟通";
        Description = "获得抉择卡“拒绝沟通”";
        TimeLeft = -1;
        Num = 54;
    }

    public override void ImmEffect()
    {
        GameControl.Instance.OCL.AddOptionCard(new OptionCard3(), TargetEmp);
        card = GameControl.Instance.OCL.CurrentOptions[GameControl.Instance.OCL.CurrentOptions.Count - 1];
    }
}


#region 旧perk

//精进
public class Perk33 : Perk
{
    public Perk33() : base()
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
    public Perk34() : base()
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

//虐待传闻
public class Perk44 : Perk
{
    public Perk44() : base()
    {
        Name = "虐待传闻";
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
    public Perk45() : base()
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
        }
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
    }
}

//虐待传闻
public class Perk55 : Perk
{
    public Perk55() : base()
    {
        Name = "虐待传闻";
        Description = "由“虐待传闻”事件产生，所有个人事件修正-5";
        TimeLeft = 6;
        Num = 55;
        canStack = true;
    }
    public override void ImmEffect()
    {
        TargetEmp.SelfEventCorrection -= 5;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.SelfEventCorrection += 5;
    }

}

//机械傀儡
public class Perk56 : Perk
{
    public Perk56() : base()
    {
        Name = "机械傀儡";
        Description = "由“机械傀儡”事件产生，事业部信念-30点";
        TimeLeft = 6;
        Num = 56;
        canStack = true;
        TempValue3 = -30;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDiv.Faith += TempValue3;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Faith -= TempValue3;
    }
}

//发奋涂墙
public class Perk57 : Perk
{
    public Perk57() : base()
    {
        Name = "发奋涂墙";
        Description = "由“发奋涂墙”事件产生，事业部信念-30点";
        TimeLeft = 6;
        Num = 57;
        canStack = true;
        TempValue3 = -30;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDiv.Faith += TempValue3;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Faith -= TempValue3;
    }
}

//警惕人工智能
public class Perk58 : Perk
{
    public Perk58() : base()
    {
        Name = "警惕人工智能";
        Description = "由“警惕人工智能”事件产生，事业部信念-30点";
        TimeLeft = 6; 
        Num = 58;
        canStack = true;
        TempValue3 = -30;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDiv.Faith += TempValue3;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Faith -= TempValue3;
    }
}

//拖延工期
public class Perk59 : Perk
{
    public Perk59() : base()
    {
        Name = "拖延工期";
        Description = "由“拖延工期”事件产生，事业部效率-3点";
        TimeLeft = 6;
        Num = 59;
        canStack = true;
        perkColor = PerkColor.Grey;
    }
    public override void ImmEffect()
    {
        TargetDiv.Efficiency -= 3;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Efficiency += 3;
    }
}

//占领公厕
public class Perk60 : Perk
{
    public Perk60() : base()
    {
        Name = "占领公厕";
        Description = "由“占领公厕”事件产生，事业部工作状态-3点";
        TimeLeft = 6;
        Num = 60;
        canStack = true;
        TempValue1 = -3;
        perkColor = PerkColor.White;
    }
    public override void ImmEffect()
    {
        TargetDiv.WorkStatus += TempValue1;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.WorkStatus -= TempValue1;
    }
}

//上班捕鱼
public class Perk61 : Perk
{
    public Perk61() : base()
    {
        Name = "上班捕鱼";
        Description = "由“上班捕鱼”事件产生，事业部工作状态-3点";
        TimeLeft = 6;
        Num = 61;
        TempValue1 = -3;
        canStack = true;
        perkColor = PerkColor.White;
    }
    public override void ImmEffect()
    {
        TargetDiv.WorkStatus += TempValue1;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.WorkStatus -= TempValue1;
    }
}

//信号不良
public class Perk62 : Perk
{
    public Perk62() : base()
    {
        Name = "信号不良";
        Description = "由“信号不良”事件产生，事业部效率-3点";
        TimeLeft = 6;
        Num = 62;
        canStack = true;
        perkColor = PerkColor.Grey;
    }
    public override void ImmEffect()
    {
        TargetDiv.Efficiency -= 3;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Efficiency += 3;
    }
}

//食堂降价
public class Perk63 : Perk
{
    public Perk63() : base()
    {
        Name = "食堂降价";
        Description = "由“食堂降价”事件产生，事业部运行成本+50";
        TimeLeft = 6;
        Num = 63;
        canStack = true;
        perkColor = PerkColor.Blue;
    }
    public override void ImmEffect()
    {
        TargetDiv.ExtraCost += 50;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.ExtraCost -= 50;
    }
}

//要求涨工资
public class Perk64 : Perk
{
    public Perk64() : base()
    {
        Name = "要求涨工资";
        Description = "由“要求涨工资”事件产生，事业部运行成本+50";
        TimeLeft = 6;
        Num = 64;
        canStack = true;
        perkColor = PerkColor.Blue;
    }
    public override void ImmEffect()
    {
        TargetDiv.ExtraCost += 50;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.ExtraCost -= 50;
    }
}

//仙人掌大战
public class Perk65 : Perk
{
    public Perk65() : base()
    {
        Name = "仙人掌大战";
        Description = "由“仙人掌大战”事件产生，所有个人事件修正+5";
        TimeLeft = 6;
        Num = 65;
        canStack = true;
    }

    public override void ImmEffect()
    {
        TargetEmp.SelfEventCorrection += 5;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.SelfEventCorrection -= 5;
    }
}

//明星见面会
public class Perk66 : Perk
{
    public Perk66() : base()
    {
        Name = "明星见面会";
        Description = "由“明星员工见面会”事件产生，每回合额外获得3点经验";
        TimeLeft = 6;
        Num = 66;
        canStack = true;
    }

    public override void ImmEffect()
    {
        TargetEmp.ExtraExp += 3;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.ExtraExp += 3;
    }
}

//电音派对
public class Perk67 : Perk
{
    public Perk67() : base()
    {
        Name = "电音派对";
        Description = "由“电音派对”事件产生，事业部工作状态+3点";
        TimeLeft = 6;
        Num = 67;
        TempValue1 = 3;
        canStack = true;
        perkColor = PerkColor.White;
    }
    public override void ImmEffect()
    {
        TargetDiv.WorkStatus += TempValue1;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.WorkStatus -= TempValue1;
    }
}

//读书会
public class Perk68 : Perk
{
    public Perk68() : base()
    {
        Name = "读书会";
        Description = "由“读书会”事件产生，事业部效率+3点";
        TimeLeft = 6;
        Num = 68;
        canStack = true;
        perkColor = PerkColor.Grey;
    }
    public override void ImmEffect()
    {
        TargetDiv.Efficiency += 3;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Efficiency -= 3;
    }
}

//甲板烧烤
public class Perk69 : Perk
{
    public Perk69() : base()
    {
        Name = "甲板烧烤";
        Description = "由“甲板烧烤”事件产生，事业部信念+30点";
        TimeLeft = 6;
        Num = 69;
        canStack = true;
        TempValue3 = 30;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDiv.Faith += TempValue3;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Faith -= TempValue3;
    }
}

//冥想
public class Perk70 : Perk
{
    public Perk70() : base()
    {
        Name = "冥想";
        Description = "由“冥想”事件产生，事业部信念+30点";
        TimeLeft = 6;
        Num = 70;
        canStack = true;
        TempValue3 = 30;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDiv.Faith += TempValue3;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Faith -= TempValue3;
    }
}

//夜跑
public class Perk71 : Perk
{
    public Perk71() : base()
    {
        Name = "夜跑";
        Description = "由“夜跑”事件产生，所有个人事件修正+5";
        TimeLeft = 6;
        Num = 71;
        canStack = true;
    }

    public override void ImmEffect()
    {
        TargetEmp.SelfEventCorrection += 5;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.SelfEventCorrection -= 5;
    }
}

//管线设计
public class Perk72 : Perk
{
    public Perk72() : base()
    {
        Name = "管线设计";
        Description = "由“管线设计”事件产生，每回合额外获得3点经验";
        TimeLeft = 6;
        Num = 72;
        canStack = true;
    }

    public override void ImmEffect()
    {
        TargetEmp.ExtraExp += 3;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.ExtraExp += 3;
    }
}

//资源共享
public class Perk73 : Perk
{
    public Perk73() : base()
    {
        Name = "资源共享";
        Description = "由“资源共享”事件产生，事业部效率+3点";
        TimeLeft = 6;
        Num = 73;
        canStack = true;
        perkColor = PerkColor.Grey;
    }
    public override void ImmEffect()
    {
        TargetDiv.Efficiency += 3;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Efficiency -= 3;
    }
}

//产品创意
public class Perk74 : Perk
{
    public Perk74() : base()
    {
        Name = "产品创意";
        Description = "由“产品创意”事件产生，事业部信念+30点";
        TimeLeft = 6;
        Num = 74;
        canStack = true;
        TempValue3 = 30;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDiv.Faith += TempValue3;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Faith -= TempValue3;
    }
}

//设施老旧
public class Perk75 : Perk
{
    public Perk75() : base()
    {
        Name = "设施老旧";
        Description = "由“设施老旧”事件产生，士气-10点";
        TimeLeft = 6;
        Num = 75;
        canStack = true;
    }

    public override void ImmEffect()
    {
        GameControl.Instance.Morale -= 10;
    }

    public override void RemoveEffect()
    {
        GameControl.Instance.Morale += 10;
    }
}

//谁是内鬼
public class Perk76 : Perk
{
    public Perk76() : base()
    {
        Name = "谁是内鬼";
        Description = "由“谁是内鬼”事件产生，事业部信念-30点";
        TimeLeft = 6;//特质
        Num = 76;
        canStack = true;
        TempValue3 = -30;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDiv.Faith += TempValue3;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Faith -= TempValue3;
    }
}

//谣言
public class Perk77 : Perk
{
    public Perk77() : base()
    {
        Name = "谣言";
        Description = "由“谣言”事件产生，公司士气-10点";
        TimeLeft = 6;//特质
        Num = 77;
        canStack = true;
    }

    public override void ImmEffect()
    {
        GameControl.Instance.Morale -= 10;
    }

    public override void RemoveEffect()
    {
        GameControl.Instance.Morale += 10;
    }
}

//舆论谴责
public class Perk78 : Perk
{
    public Perk78() : base()
    {
        Name = "舆论谴责";
        Description = "由“舆论谴责”事件产生，公司士气-10点";
        TimeLeft = 6;
        Num = 78;
        canStack = true;
    }
    public override void ImmEffect()
    {
        GameControl.Instance.Morale -= 10;
    }

    public override void RemoveEffect()
    {
        GameControl.Instance.Morale += 10;
    }
}

//用户流失
public class Perk79 : Perk
{
    public Perk79() : base()
    {
        Name = "用户流失";
        Description = "由“用户流失”事件产生，事业部信念-30点";
        TimeLeft = 6;
        Num = 79;
        canStack = true;
        TempValue3 = -30;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDiv.Faith += TempValue3;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Faith -= TempValue3;
    }
}

//夸大事实
public class Perk80 : Perk
{
    public Perk80() : base()
    {
        Name = "夸大事实";
        Description = "由“夸大事实”事件产生，公司士气-10点";
        TimeLeft = 6;//特质
        Num = 80;
        canStack = true;
    }
    public override void ImmEffect()
    {
        GameControl.Instance.Morale -= 10;
    }

    public override void RemoveEffect()
    {
        GameControl.Instance.Morale += 10;
    }
}

//设备维修
public class Perk81 : Perk
{
    public Perk81() : base()
    {
        Name = "设备维修";
        Description = "由“设备维修”事件产生，事业部成本+50";
        TimeLeft = 6;
        Num = 81;
        canStack = true;
        perkColor = PerkColor.Blue;
    }
    public override void ImmEffect()
    {
        TargetDiv.ExtraCost += 50;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.ExtraCost -= 50;
    }
}

//盗版软件
public class Perk82 : Perk
{
    public Perk82() : base()
    {
        Name = "盗版软件";
        Description = "由“盗版软件”事件产生，公司士气-10点";
        TimeLeft = 6;
        Num = 82;
        canStack = true;
    }
    public override void ImmEffect()
    {
        GameControl.Instance.Morale -= 10;
    }

    public override void RemoveEffect()
    {
        GameControl.Instance.Morale += 10;
    }
}

//人人自危
public class Perk83 : Perk
{
    public Perk83() : base()
    {
        Name = "人人自危";
        Description = "由“人人自危”事件产生，事业部信念-30点";
        TimeLeft = 6;
        Num = 83;
        canStack = true;
        TempValue3 = -30;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDiv.Faith += TempValue3;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Faith -= TempValue3;
    }
}

//琢磨跳槽
public class Perk84 : Perk
{
    public Perk84() : base()
    {
        Name = "琢磨跳槽";
        Description = "由“琢磨跳槽”事件产生，公司士气-10点";
        TimeLeft = 6;
        Num = 84;
        canStack = true;
    }
    public override void ImmEffect()
    {
        GameControl.Instance.Morale -= 10;
    }

    public override void RemoveEffect()
    {
        GameControl.Instance.Morale += 10;
    }
}

//要求公开财务
public class Perk85 : Perk
{
    public Perk85() : base()
    {
        Name = "要求公开财务";
        Description = "由“要求公开财务”事件产生，公司士气-10点";
        TimeLeft = 6;
        Num = 85;
        canStack = true;
    }
    public override void ImmEffect()
    {
        GameControl.Instance.Morale -= 10;
    }

    public override void RemoveEffect()
    {
        GameControl.Instance.Morale += 10;
    }
}

//出现幻觉
public class Perk86 : Perk
{
    public Perk86() : base()
    {
        Name = "出现幻觉";
        Description = "由“出现幻觉”事件产生，事业部信念-30点";
        TimeLeft = 6;
        Num = 86;
        canStack = true;
        TempValue3 = -30;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDiv.Faith += TempValue3;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Faith -= TempValue3;
    }
}

//迷路
public class Perk87 : Perk
{
    public Perk87() : base()
    {
        Name = "迷路";
        Description = "由“迷路”事件产生，事业部信念-30点";
        TimeLeft = 6;
        Num = 87;
        canStack = true;
        TempValue3 = -30;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDiv.Faith += TempValue3;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Faith -= TempValue3;
    }
}

//资料丢失
public class Perk88 : Perk
{
    public Perk88() : base()
    {
        Name = "资料丢失";
        Description = "由“资料丢失”事件产生，公司士气-10点";
        TimeLeft = 6;
        Num = 88;
        canStack = true;
    }
    public override void ImmEffect()
    {
        GameControl.Instance.Morale -= 10;
    }

    public override void RemoveEffect()
    {
        GameControl.Instance.Morale += 10;
    }
}

//额外工作
public class Perk89 : Perk
{
    public Perk89() : base()
    {
        Name = "额外工作";
        Description = "由“额外工作”事件产生，公司士气-10点";
        TimeLeft = 6;
        Num = 89;
        canStack = true;
    }
    public override void ImmEffect()
    {
        GameControl.Instance.Morale -= 10;
    }

    public override void RemoveEffect()
    {
        GameControl.Instance.Morale += 10;
    }
}

//打吊瓶加班
public class Perk90 : Perk
{
    public Perk90() : base()
    {
        Name = "打吊瓶加班";
        Description = "事业部中每名员工每回合心力下降10点";
        TimeLeft = -1;
        Num = 90;
        canStack = true;
    }

    public override void ImmEffect()
    {
        TargetDiv.MentalityBonus -= 10;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.MentalityBonus += 10;
    }
}

//混沌
public class Perk91 : Perk
{
    public Perk91() : base()
    {
        Name = "混沌";
        Description = "事业部信念-20";
        TimeLeft = -1;//特质
        Num = 91;
        canStack = true;
        TempValue3 = -20;
        perkColor = PerkColor.Orange;
    }

    public override void ImmEffect()
    {
        TargetDiv.Faith += TempValue3;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Faith -= TempValue3;
    }
}

//经费不足
public class Perk92 : Perk
{
    public Perk92() : base()
    {
        Name = "经费不足";
        Description = "领导力水晶的决策成功率-10%";
        TimeLeft = -1;//特质
        Num = 92;
        canStack = true;
    }
    public override void ImmEffect()
    {
        MonthMeeting.Instance.CrystalExtraSuccessRate -= 0.1f;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        MonthMeeting.Instance.CrystalExtraSuccessRate += 0.1f;
    }
}

//被害妄想症
public class Perk93 : Perk
{
    public Perk93() : base()
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
    public Perk94() : base()
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
    public Perk95() : base()
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
    public Perk96() : base()
    {
        Name = "开悟";
        Description = "不产生红色系情绪，愤怒、反感、侮辱等，且心力爆炸后不产生负面特质";
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

//拒绝洗脑
public class Perk97 : Perk
{
    public Perk97() : base()
    {
        Name = "拒绝洗脑";
        Description = "不满栏位上限-1，认同栏位上限+1";
        TimeLeft = -1;
        Num = 97;
        canStack = true;
    }
    public override void ImmEffect()
    {
        GameControl.Instance.ApproveLimit += 1;
        GameControl.Instance.DissatisfiedLimit -= 1;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        GameControl.Instance.ApproveLimit -= 1;
        GameControl.Instance.DissatisfiedLimit += 1;
    }
}

//组织调整
public class Perk98 : Perk
{
    public Perk98() : base()
    {
        Name = "组织调整";
        Description = "";
        TimeLeft = -1;
        Num = 98;
        canStack = true;
    }
    public override void ImmEffect()
    {
        GameControl.Instance.StandbyEmpLimit -= 1; 
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        GameControl.Instance.StandbyEmpLimit += 1;
    }
}

//并肩作战
public class Perk99 : Perk
{
    public Perk99() : base()
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
    public Perk100() : base()
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
    public Perk101() : base()
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
    public Perk102() : base()
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
    public Perk103() : base()
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
    public Perk104() : base()
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

//我放哪儿了？
public class Perk105 : Perk
{
    public Perk105() : base()
    {
        Name = "我放哪儿了？";
        Description = "事业部中每名员工每回合获取经验减少3点";
        TimeLeft = -1;
        Num = 105;
        canStack = true;
        perkColor = PerkColor.None;
    }
    public override void ImmEffect()
    {
        TargetDiv.ExtraExp -= 3;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.ExtraExp += 3;
    }
}

//仓鼠之家
public class Perk106 : Perk
{
    public Perk106() : base()
    {
        Name = "仓鼠之家";
        Description = "公司物品栏+1";
        TimeLeft = -1;
        Num = 106;
        canStack = true;
    }
    public override void ImmEffect()
    {
        GameControl.Instance.ItemLimit += 1;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        GameControl.Instance.ItemLimit -= 1;
    }
}

//看展览
public class Perk107 : Perk
{
    public Perk107() : base()
    {
        Name = "看展览";
        Description = "公司所有部门生产或充能周期+1回合";
        TimeLeft = -1;
        Num = 107;
        canStack = true;
    }
    public override void ImmEffect()
    {
        foreach(DivisionControl div in GameControl.Instance.CurrentDivisions)
        {
            div.ExtraProduceTime += 1;
        }
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        foreach (DivisionControl div in GameControl.Instance.CurrentDivisions)
        {
            div.ExtraProduceTime -= 1;
        }
    }
}

//特立独行
public class Perk108 : Perk
{
    public Perk108() : base()
    {
        Name = "特立独行";
        Description = "获得正面抉择卡-另辟蹊径";
        TimeLeft = -1;
        Num = 108;
        canStack = false;
    }

    public override void ImmEffect()
    {
        GameControl.Instance.OCL.AddOptionCard(new OptionCard8(), TargetEmp);
    }
}

//谈判专家
public class Perk109 : Perk
{
    public Perk109() : base()
    {
        Name = "谈判专家";
        Description = "获得正面抉择卡-寻求共识";
        TimeLeft = -1;
        Num = 109;
        canStack = false;
    }

    public override void ImmEffect()
    {
        GameControl.Instance.OCL.AddOptionCard(new OptionCard9(), TargetEmp);
    }
}

//同理心
public class Perk110 : Perk
{
    public Perk110() : base()
    {
        Name = "同理心";
        Description = "获得正面抉择卡-缓和情绪";
        TimeLeft = -1;
        Num = 110;
        canStack = false;
    }
    public override void ImmEffect()
    {
        GameControl.Instance.OCL.AddOptionCard(new OptionCard10(), TargetEmp);
    }
}

//侦探
public class Perk111 : Perk
{
    public Perk111() : base()
    {
        Name = "侦探";
        Description = "获得正面抉择卡-基本演绎法";
        TimeLeft = -1;
        Num = 111;
        canStack = false;
    }
    public override void ImmEffect()
    {
        GameControl.Instance.OCL.AddOptionCard(new OptionCard11(), TargetEmp);
    }
}

//沉思者
public class Perk112 : Perk
{
    public Perk112() : base()
    {
        Name = "沉思者";
        Description = "获得正面抉择卡-一语中的";
        TimeLeft = -1;
        Num = 112;
        canStack = false;
    }
    public override void ImmEffect()
    {
        GameControl.Instance.OCL.AddOptionCard(new OptionCard12(), TargetEmp);
    }
}

//老练
public class Perk113 : Perk
{
    public Perk113() : base()
    {
        Name = "老练";
        Description = "获得正面抉择卡-聚焦";
        TimeLeft = -1;
        Num = 113;
        canStack = false;
    }
    public override void ImmEffect()
    {
        GameControl.Instance.OCL.AddOptionCard(new OptionCard13(), TargetEmp);
    }
}

//暴躁
public class Perk114 : Perk
{
    public Perk114() : base()
    {
        Name = "暴躁";
        Description = "获得负面抉择卡-呵斥";
        TimeLeft = -1;
        Num = 114;
        canStack = false;
    }
    public override void ImmEffect()
    {
        GameControl.Instance.OCL.AddOptionCard(new OptionCard14(), TargetEmp);
        card = GameControl.Instance.OCL.CurrentOptions[GameControl.Instance.OCL.CurrentOptions.Count - 1];
    }
}

//专断
public class Perk115 : Perk
{
    public Perk115() : base()
    {
        Name = "专断";
        Description = "获得负面抉择卡-一言堂";
        TimeLeft = -1;
        Num = 115;
        canStack = false;
    }
    public override void ImmEffect()
    {
        GameControl.Instance.OCL.AddOptionCard(new OptionCard15(), TargetEmp);
        card = GameControl.Instance.OCL.CurrentOptions[GameControl.Instance.OCL.CurrentOptions.Count - 1];
    }
}

//拜金主义
public class Perk116 : Perk
{
    public Perk116() : base()
    {
        Name = "拜金主义";
        Description = "获得负面抉择卡-撒币";
        TimeLeft = -1;
        Num = 116;
        canStack = false;
    }
    public override void ImmEffect()
    {
        GameControl.Instance.OCL.AddOptionCard(new OptionCard16(), TargetEmp);
        card = GameControl.Instance.OCL.CurrentOptions[GameControl.Instance.OCL.CurrentOptions.Count - 1];
    }
}

//推卸责任
public class Perk117 : Perk
{
    public Perk117() : base()
    {
        Name = "推卸责任";
        Description = "事业部信念-10";
        TimeLeft = 6;
        Num = 117;
        canStack = true;
        TempValue3 = -10;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDiv.Faith += TempValue3;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Faith -= TempValue3;
    }
}

//怀疑情绪
public class Perk118 : Perk
{
    public Perk118() : base()
    {
        Name = "怀疑情绪";
        Description = "事业部信念-15";
        TimeLeft = 9;
        Num = 118;
        canStack = true;
        TempValue3 = -15;
        perkColor = PerkColor.Orange;
    }
    public override void ImmEffect()
    {
        TargetDiv.Faith += TempValue3;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Faith -= TempValue3;
    }
}

//状态低迷
public class Perk119 : Perk
{
    public Perk119() : base()
    {
        Name = "状态低迷";
        Description = "事业部工作状态-2";
        TimeLeft = 6;
        Num = 119;
        TempValue1 = -2;
        canStack = true;
        perkColor = PerkColor.White;
    }
    public override void ImmEffect()
    {
        TargetDiv.WorkStatus += TempValue1;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.WorkStatus -= TempValue1;
    }
}

//放轻松
public class Perk120 : Perk
{
    public Perk120() : base()
    {
        Name = "放轻松";
        Description = "提高工作状态1点";
        TimeLeft = 3;
        Num = 120;
        canStack = false;
        TempValue1 = 1;
        perkColor = PerkColor.White;
    }

    public override void ImmEffect()
    {
        TargetDiv.WorkStatus += TempValue1;
        Description = "提高工作状态" + TempValue1 + "点";
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.WorkStatus -= TempValue1;
    }
}

//勇气赞歌
public class Perk121 : Perk
{
    public Perk121() : base()
    {
        Name = "勇气赞歌";
        Description = "信念+15";
        TimeLeft = 3;
        Num = 121;
        canStack = false;
        TempValue3 = 15;
        TempValue1 = 1;
        perkColor = PerkColor.Orange;
    }

    public override void ImmEffect()
    {
        TempValue3 = TempValue1 * 15;
        Description = "信念+" + TempValue3;
        TargetDiv.Faith += TempValue3;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Faith -= TempValue3;
    }
}

//加班加班
public class Perk122 : Perk
{
    public Perk122() : base()
    {
        Name = "加班加班";
        Description = "事业部效率+1";
        TimeLeft = 3;
        Num = 122;
        canStack = false;
        TempValue1 = 1;
        TempValue2 = 1;
        perkColor = PerkColor.Grey;
    }

    public override void ImmEffect()
    {
        TempValue2 = TempValue1 * 1;
        TargetDiv.Efficiency += TempValue2;
        Description = "事业部效率+" + TempValue2;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Efficiency -= TempValue2;
    }
}

//流程优化
public class Perk123 : Perk
{
    public Perk123() : base()
    {
        Name = "流程优化";
        Description = "事业部成本-5";
        TimeLeft = 3;
        Num = 123;
        canStack = false;
        TempValue1 = 1;
        perkColor = PerkColor.Blue;
    }

    public override void ImmEffect()
    {
        TargetDiv.ExtraCost -= TempValue1 * 5;
        Description = "部门成本-" + (TempValue1 * 5);
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.ExtraCost += TempValue1 * 5;
    }
}

//无意义争执
public class Perk124 : Perk
{
    public Perk124() : base()
    {
        Name = "无意义争执";
        Description = "信念-30";
        TimeLeft = 3;
        Num = 124;
        canStack = false;
        TempValue3 = -30;
        TempValue1 = 1;
        perkColor = PerkColor.Orange;
    }

    public override void ImmEffect()
    {
        TempValue3 = -30 * TempValue1;
        TargetDiv.Faith += TempValue3;
        Description = "信念" + TempValue3;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Faith -= TempValue3;
    }
}

//领导指令
public class Perk125 : Perk
{
    public Perk125() : base()
    {
        Name = "领导指令";
        Description = "部门内全部员工每周结束时体力-10(效果没加，描述是临时放着的)";
        TimeLeft = 3;
        Num = 125;
        canStack = false;
    }
}

//业务生疏
public class Perk126 : Perk
{
    public Perk126() : base()
    {
        Name = "业务生疏";
        Description = "事业部效率-5";
        TimeLeft = 3;
        Num = 126;
        TempValue2 = -5;
        perkColor = PerkColor.Grey;
        canStack = false;
    }

    public override void ImmEffect()
    {
        TargetDiv.Efficiency += TempValue2;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Efficiency -= TempValue2;
    }
}

//寒蝉效应
public class Perk128 : Perk
{
    public Perk128() : base()
    {
        Name = "寒蝉效应";
        Description = "事业部效率-2";
        TimeLeft = 6;
        Num = 128;
        canStack = true;
        perkColor = PerkColor.Grey;
    }

    public override void ImmEffect()
    {
        TargetDiv.Efficiency -= 2;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Efficiency += 2;
    }
}

//成本飙升
public class Perk129 : Perk
{
    public Perk129() : base()
    {
        Name = "成本飙升";
        Description = "事业部成本上升25";
        TimeLeft = 6;
        Num = 129;
        canStack = true;
        perkColor = PerkColor.Blue;
    }

    public override void ImmEffect()
    {
        TargetDiv.ExtraCost += 25;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.ExtraCost -= 25;
    }
}

//救火队员
public class Perk130 : Perk
{
    public Perk130() : base()
    {
        Name = "救火队员";
        Description = "加入特别小组后额外增加10%成功率";
        TimeLeft = -1;
        Num = 130;
        canStack = false;
    }
}

//养尊处优
public class Perk131 : Perk
{
    public Perk131() : base()
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
    public Perk132() : base()
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
    public Perk133() : base()
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
    public Perk134() : base()
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
    public Perk135() : base()
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
    public Perk136() : base()
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
    public Perk137() : base()
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
    public Perk138() : base()
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
    public Perk139() : base()
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
    public Perk140() : base()
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
    public Perk141() : base()
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
    public Perk142() : base()
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
    public Perk143() : base()
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
        //TargetDep.BaseWorkStatus -= 3 * TempValue1;
        //TargetDep.DepBaseMajorSuccessRate += 0.3f * TempValue1;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        //TargetDep.BaseWorkStatus += 3 * TempValue1;
        //TargetDep.DepBaseMajorSuccessRate -= 0.3f * TempValue1;
    }
}


//必胜信念
public class Perk144 : Perk
{
    public Perk144() : base()
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
    public Perk145() : base()
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
    public Perk146() : base()
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
    public Perk147() : base()
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
        //TargetDep.StaminaCostRate -= 0.3f * TempValue1;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        //TargetDep.StaminaCostRate += 0.3f * TempValue1;
    }
}

//自由之翼
public class Perk148 : Perk
{
    public Perk148() : base()
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
    public Perk149() : base()
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
        //foreach(int a in TargetEmp.Professions)
        //{
        //    if (a != 0)
        //        limit++;
        //}
        //TempValue1 = Random.Range(0, limit);
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
    public Perk150() : base()
    {
        Name = "胜利开发";
        Description = "提升工作状态1点";
        TimeLeft = 3;
        Num = 150;
        canStack = true;
        TempValue1 = 1;
        perkColor = PerkColor.White;
    }

    public override void ImmEffect()
    {
        TargetDiv.WorkStatus += TempValue1;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.WorkStatus -= TempValue1;
    }
}

//节省支出
public class Perk151 : Perk
{
    public Perk151() : base()
    {
        Name = "节省支出";
        Description = "降低成本3点";
        TimeLeft = 3;
        Num = 151;
        canStack = true;
        perkColor = PerkColor.Blue;
    }

    public override void ImmEffect()
    {
        TargetDiv.ExtraCost -= 3;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.ExtraCost += 3;
    }
}

//信仰充值
public class Perk152 : Perk
{
    public Perk152() : base()
    {
        Name = "信仰充值";
        Description = "提升信念10点";
        TimeLeft = 3;
        Num = 152;
        canStack = true;
        perkColor = PerkColor.Orange;
    }

    public override void ImmEffect()
    {
        TargetDiv.Faith += 10;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Faith -= 10;
    }
}

//效率至上
public class Perk153 : Perk
{
    public Perk153() : base()
    {
        Name = "效率至上";
        Description = "提升效率1点";
        TimeLeft = 3;
        Num = 153;
        canStack = true;
        perkColor = PerkColor.Grey;
    }

    public override void ImmEffect()
    {
        TargetDiv.Efficiency += 1;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Efficiency -= 1;
    }
}

//996
public class Perk154 : Perk
{
    public Perk154() : base()
    {
        Name = "996";
        Description = "提升效率1点";
        TimeLeft = 1;
        Num = 154;
        canStack = true;
        perkColor = PerkColor.Grey;
    }

    public override void ImmEffect()
    {
        TargetDiv.Efficiency += 1;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetDiv.Efficiency -= 1;
    }
}

#endregion
public static class PerkData
{

    //一般特质
    public static List<Perk> PerkList = new List<Perk>()
    {
        new Perk12(), new Perk13(), new Perk14(), new Perk15(), new Perk16(), new Perk17(),
        new Perk18(), new Perk20(), new Perk21(), new Perk22(),
        new Perk23(), new Perk108(), new Perk109(), new Perk110(), new Perk111(), new Perk112(), new Perk113(), new Perk130()
    };

    //经历特质
    public static List<Perk> DefaultPerkList = new List<Perk>()
    {
        new Perk46(), new Perk47(), new Perk48(), new Perk49(), new Perk50(), new Perk51()
    };

    //职业特质
    public static List<Perk> OccupationPerkList = new List<Perk>()
    {
        new Perk35(), new Perk36(), new Perk37(), new Perk38(), new Perk39(), new Perk40(),
        new Perk41(), new Perk42(), new Perk43()
    };

    //管理特质
    public static List<Perk> ManagePerkList = new List<Perk>()
    {
         new Perk1(), new Perk2(), new Perk4(), new Perk5(), new Perk6(),
        new Perk7(), new Perk8(), new Perk9(), new Perk11()
        , new Perk24(), new Perk25(), new Perk26(), new Perk27(), new Perk28(),
        new Perk29(), new Perk30(), new Perk31(), new Perk32()
    };

    //正面特质（产生正面抉择卡的特质）
    public static List<Perk> OptionCardPerkList = new List<Perk>()
    {
        new Perk108(), new Perk109(), new Perk110(), new Perk111(), new Perk112(), new Perk113()
    };

    public static List<Perk> DebuffPerkList = new List<Perk>()
    {
        new Perk52(), new Perk53(), new Perk54(), new Perk114(), new Perk115(), new Perk116()
    };
}