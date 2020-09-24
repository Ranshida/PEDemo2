using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    Imm, Dayily, Weekly, Monthly
}
//基类
public class Perk
{
    public int TimeLeft, Num = 0;  //单位周
    public bool Positive = true;
    public string Name, Description;
    public EffectType effectType;

    public Employee TargetEmp;
    public PerkInfo Info;

    protected int TempValue1, TempValue2, TempValue3;

    public Perk(Employee Emp)
    {
        TargetEmp = Emp;
    }

    public void AddEffect()
    {
        if(effectType == EffectType.Dayily)
        {
            TargetEmp.InfoA.GC.DailyEvent.AddListener(ContinuousEffect);
        }
        else if (effectType == EffectType.Weekly)
        {
            TargetEmp.InfoA.GC.WeeklyEvent.AddListener(ContinuousEffect);
        }
        else if (effectType == EffectType.Monthly)
        {
            TargetEmp.InfoA.GC.MonthlyEvent.AddListener(ContinuousEffect);
        }
        TargetEmp.InfoA.GC.HourEvent.AddListener(TimePass);

        //else if (EType == EffectType.Weekly)
        //{
        //    TargetEmp.InfoA.GC.WeeklyEvent.AddListener(ContinuousEffect);
        //}

        ImmEffect();
    }

    public virtual void RemoveEffect()
    {
        if (effectType == EffectType.Dayily)
        {
            TargetEmp.InfoA.GC.DailyEvent.RemoveListener(ContinuousEffect);
        }
        else if (effectType == EffectType.Weekly)
        {
            TargetEmp.InfoA.GC.WeeklyEvent.RemoveListener(ContinuousEffect);
        }
        else if (effectType == EffectType.Monthly)
        {
            TargetEmp.InfoA.GC.MonthlyEvent.RemoveListener(ContinuousEffect);
        }
        TargetEmp.InfoA.GC.HourEvent.RemoveListener(TimePass);

        Info.RemovePerk();
        //else if (EType == EffectType.Weekly)
        //{
        //    TargetEmp.InfoA.GC.WeeklyEvent.RemoveListener(ContinuousEffect);
        //}
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

}

//受伤
public class Perk1 : Perk
{
    public Perk1(Employee Emp) : base(Emp)
    {
        Name = "受伤";
        Description = "强壮-2";
        TimeLeft = 64;
        Num = 1;
    }

    public override void ImmEffect()
    {
        TargetEmp.Strength -= 2;
    }

    public override void RemoveEffect()
    {
        TargetEmp.Strength += 2;
        base.RemoveEffect();
    }
}

//斯巴达
public class Perk2 : Perk
{
    public Perk2(Employee Emp) : base(Emp)
    {
        Name = "斯巴达";
        Description = "强壮+1";
        TimeLeft = 64;
        Num = 2;
    }

    public override void ImmEffect()
    {
        TargetEmp.Strength += 1;
    }

    public override void RemoveEffect()
    {
        TargetEmp.Strength -= 1;
        base.RemoveEffect();
    }
}

//启发
public class Perk3 : Perk
{
    int Value = 0;
    public Perk3(Employee Emp) : base(Emp)
    {
        Name = "启发";
        Value = Random.Range(1, 16);
        string SName;
        //1 - 3职业技能 4观察 5坚韧 6强壮 7管理 8人力 9财务 10决策 11行业 12谋略 13说服 14魅力 15八卦
        if (Value == 1)
            SName = "技术技能";
        else if (Value == 2)
            SName = "市场技能";
        else if (Value == 3)
            SName = "产品技能";
        else if (Value == 4)
            SName = "观察";
        else if (Value == 5)
            SName = "坚韧";
        else if (Value == 6)
            SName = "强壮";
        else if (Value == 7)
            SName = "管理";
        else if (Value == 8)
            SName = "人力";
        else if (Value == 9)
            SName = "财务";
        else if (Value == 10)
            SName = "决策";
        else if (Value == 11)
            SName = "行业";
        else if (Value == 12)
            SName = "谋略";
        else if (Value == 13)
            SName = "说服";
        else if (Value == 14)
            SName = "魅力";
        else
            SName = "八卦";
        Description = "随机增加某项技能(" + SName + ")1点热情";
        TimeLeft = 96;
        Num = 3;
    }

    public override void ImmEffect()
    {
        if(TargetEmp.Stars[Value - 1] < TargetEmp.StarLimit[(Value - 1) / 3] * 5)
        {
            TargetEmp.Stars[Value - 1] += 1;
        }
    }

    public override void RemoveEffect()
    {
        TargetEmp.Stars[Value - 1] -= 1;
        if (TargetEmp.Stars[Value - 1] < 0)
            TargetEmp.Stars[Value - 1] = 0;
        for(int i = 0; i < TargetEmp.InfoDetail.PerksInfo.Count; i++)
        {
            if(TargetEmp.InfoDetail.PerksInfo[i].CurrentPerk.Num == 12)
            {
                TargetEmp.InfoDetail.AddPerk(new Perk3(TargetEmp), true);
                break;
            }
        }
        base.RemoveEffect();
    }
}

//头脑风暴
public class Perk4 : Perk
{
    int Value1 = 0, Value2 = 0;
    public Perk4(Employee Emp) : base(Emp)
    {
        Name = "头脑风暴";
        string SName1, SName2;
        Value1 = Random.Range(1, 16);
        Value2 = Random.Range(1, 16);
        while(Value2 == Value1)
        {
            Value2 = Random.Range(0, 16);
        }
        if (Value1 == 1)
            SName1 = "技术技能";
        else if (Value1 == 2)
            SName1 = "市场技能";
        else if (Value1 == 3)
            SName1 = "产品技能";
        else if (Value1 == 4)
            SName1 = "观察";
        else if (Value1 == 5)
            SName1 = "坚韧";
        else if (Value1 == 6)
            SName1 = "强壮";
        else if (Value1 == 7)
            SName1 = "管理";
        else if (Value1 == 8)
            SName1 = "人力";
        else if (Value1 == 9)
            SName1 = "财务";
        else if (Value1 == 10)
            SName1 = "决策";
        else if (Value1 == 11)
            SName1 = "行业";
        else if (Value1 == 12)
            SName1 = "谋略";
        else if (Value1 == 13)
            SName1 = "说服";
        else if (Value1 == 14)
            SName1 = "魅力";
        else
            SName1 = "八卦";

        if (Value2 == 1)
            SName2 = "技术技能";
        else if (Value2 == 2)
            SName2 = "市场技能";
        else if (Value2 == 3)
            SName2 = "产品技能";
        else if (Value2 == 4)
            SName2 = "观察";
        else if (Value2 == 5)
            SName2 = "坚韧";
        else if (Value2 == 6)
            SName2 = "强壮";
        else if (Value2 == 7)
            SName2 = "管理";
        else if (Value2 == 8)
            SName2 = "人力";
        else if (Value2 == 9)
            SName2 = "财务";
        else if (Value2 == 10)
            SName2 = "决策";
        else if (Value2 == 11)
            SName2 = "行业";
        else if (Value2 == 12)
            SName2 = "谋略";
        else if (Value2 == 13)
            SName2 = "说服";
        else if (Value2 == 14)
            SName2 = "魅力";
        else
            SName2 = "八卦";

        Description = "随机增加某项技能(" + SName1 + "," + SName2 + ")1点热情";
        TimeLeft = 96;
        Num = 4;
    }

    public override void ImmEffect()
    {
        if (TargetEmp.Stars[Value1 - 1] < TargetEmp.StarLimit[Value1 - 1] * 5)
        {
            TargetEmp.Stars[Value1 - 1] += 1;
        }
        if (TargetEmp.Stars[Value2 - 1] < TargetEmp.StarLimit[Value2 - 1] * 5)
        {
            TargetEmp.Stars[Value2 - 1] += 1;
        }
    }

    public override void RemoveEffect()
    {
        TargetEmp.Stars[Value1 - 1] -= 1;
        TargetEmp.Stars[Value2 - 1] -= 1;
        if (TargetEmp.Stars[Value1 - 1] < 0)
            TargetEmp.Stars[Value1 - 1] = 0;
        if (TargetEmp.Stars[Value2 - 1] < 0)
            TargetEmp.Stars[Value2 - 1] = 0;
        base.RemoveEffect();
    }
}

//奋斗逼
public class Perk5 : Perk
{
    public Perk5(Employee Emp) : base(Emp)
    {
        Name = "奋斗逼";
        Description = "工作学习动机 +15";
        TimeLeft = -1;
        Num = 5;
    }
    public override void ImmEffect()
    {
        TargetEmp.BaseMotivation[0] += 15;
    }
    public override void RemoveEffect()
    {
        TargetEmp.BaseMotivation[0] -= 15;
        base.RemoveEffect();
    }
}

//欧洲人
public class Perk6 : Perk
{
    public Perk6(Employee Emp) : base(Emp)
    {
        Name = "欧洲人";
        Description = "工作学习动机 -15";
        TimeLeft = -1;
        Num = 6;
    }
    public override void ImmEffect()
    {
        TargetEmp.BaseMotivation[0] -= 15;
    }
    public override void RemoveEffect()
    {
        TargetEmp.BaseMotivation[0] += 15;
        base.RemoveEffect();
    }
}

//抑郁
public class Perk7 : Perk
{
    public Perk7(Employee Emp) : base(Emp)
    {
        Name = "抑郁";
        Description = "工作学习动机-20，心体恢复动机-20，每周心力-10";
        TimeLeft = 96;
        effectType = EffectType.Weekly;
        Num = 7;
    }

    public override void ContinuousEffect()
    {
        TargetEmp.Mentality -= 10;
    }
    public override void ImmEffect()
    {
        TargetEmp.BaseMotivation[0] -= 20;
        TargetEmp.BaseMotivation[1] -= 20;
    }
    public override void RemoveEffect()
    {
        TargetEmp.BaseMotivation[0] += 20;
        TargetEmp.BaseMotivation[1] += 20;
        base.RemoveEffect();
    }
}

//重度抑郁症
public class Perk8 : Perk
{
    public Perk8(Employee Emp) : base(Emp)
    {
        Name = "重度抑郁症";
        Description = "每天心力-20";
        TimeLeft = -1;
        effectType = EffectType.Weekly;
        Num = 8;
    }

    public override void ContinuousEffect()
    {
        TargetEmp.Mentality -= 20;
    }
}

//元气满满
public class Perk9 : Perk
{
    public Perk9(Employee Emp) : base(Emp)
    {
        Name = "元气满满";
        Description = "心体恢复动机+15";
        TimeLeft = -1;
        Num = 9;
    }
    public override void ImmEffect()
    {
        TargetEmp.BaseMotivation[1] += 15;
    }
    public override void RemoveEffect()
    {
        TargetEmp.BaseMotivation[1] -= 15;
        base.RemoveEffect();
    }
}

//狂热
public class Perk10 : Perk
{
    public Perk10(Employee Emp) : base(Emp)
    {
        Name = "狂热";
        Description = "心体恢复动机-15，每月都有50%几率进入狂信者状态";
        TimeLeft = -1;
        Num = 10;
        effectType = EffectType.Monthly;
    }
    public override void ContinuousEffect()
    {
        if(Random.Range(0.0f, 1.0f) < 0.5f)
        {
            TargetEmp.InfoDetail.AddPerk(new Perk11(TargetEmp), true);
        }
    }
    public override void ImmEffect()
    {
        TargetEmp.BaseMotivation[1] -= 15;
    }
    public override void RemoveEffect()
    {
        TargetEmp.BaseMotivation[1] += 15;
        base.RemoveEffect();
    }
}

//狂信者
public class Perk11 : Perk
{
    public Perk11(Employee Emp) : base(Emp)
    {
        Name = "狂信者";
        Description = "狂热地热衷传教";
        TimeLeft = 64;
        Num = 11;
    }
}

//鹰视狼顾
public class Perk12 : Perk
{
    public Perk12(Employee Emp) : base(Emp)
    {
        Name = "鹰视狼顾";
        Description = "谋划野心动机+20，当启发状态消失时获得启发*1";
        TimeLeft = -1;
        Num = 12;
    }
    public override void ImmEffect()
    {
        TargetEmp.BaseMotivation[3] += 20;
    }
    public override void RemoveEffect()
    {
        TargetEmp.BaseMotivation[3] -= 20;
        base.RemoveEffect();
    }
}

//平凡之路
public class Perk13 : Perk 
{
    public Perk13(Employee Emp) : base(Emp)
    {
        Name = "平凡之路";
        Description = "谋划野心动机-15";
        TimeLeft = -1;
        Num = 13;
    }
    public override void ImmEffect()
    {
        TargetEmp.BaseMotivation[2] -= 15;
    }
    public override void RemoveEffect()
    {
        TargetEmp.BaseMotivation[2] += 15;
        base.RemoveEffect();
    }
}

//复仇者
public class Perk14 : Perk
{
    public Perk14(Employee Emp) : base(Emp)
    {
        Name = "复仇者";
        Description = "谋划野心动机+15";
        TimeLeft = -1;
        Num = 14;
    }
    public override void ImmEffect()
    {
        TargetEmp.BaseMotivation[2] += 15;
    }
    public override void RemoveEffect()
    {
        TargetEmp.BaseMotivation[2] -= 15;
        base.RemoveEffect();
    }
}

//咖啡成瘾没做

//好色
public class Perk16 : Perk
{
    public Perk16(Employee Emp) : base(Emp)
    {
        Name = "好色";
        Description = "关系交往动机+15";
        TimeLeft = -1;
        Num = 16;
    }
    public override void ImmEffect()
    {
        TargetEmp.BaseMotivation[3] += 15;
    }
    public override void RemoveEffect()
    {
        TargetEmp.BaseMotivation[3] -= 15;
        base.RemoveEffect();
    }
}

//社交狂人
public class Perk17 : Perk
{
    public Perk17(Employee Emp) : base(Emp)
    {
        Name = "社交狂人";
        Description = "关系交往动机+20，魅力+5";
        TimeLeft = -1;
        Num = 17;
    }

    public override void ImmEffect()
    {
        TargetEmp.Charm += 5;
        TargetEmp.BaseMotivation[3] += 20;
    }

    public override void RemoveEffect()
    {
        TargetEmp.Charm -= 5;
        TargetEmp.BaseMotivation[3] -= 20;
        base.RemoveEffect();
    }
}

//反社会人格
public class Perk19 : Perk
{
    public Perk19(Employee Emp) : base(Emp)
    {
        Name = "反社会人格";
        Description = "魅力-5，说服-5，关系交往动机-40";
        TimeLeft = -1;
        Num = 19;
    }

    public override void ImmEffect()
    {
        TargetEmp.Charm -= 5;
        TargetEmp.Convince -= 5;
        TargetEmp.BaseMotivation[3] -= 40;
    }

    public override void RemoveEffect()
    {
        TargetEmp.Charm += 5;
        TargetEmp.Convince += 5;
        TargetEmp.BaseMotivation[3] += 40;
        base.RemoveEffect();
    }
}


//失恋
public class Perk20 : Perk
{
    public Perk20(Employee Emp) : base(Emp)
    {
        Name = "失恋";
        Description = "关系交往动机-20";
        TimeLeft = 384;
        Num = 20;
    }
    public override void ImmEffect()
    {
        TargetEmp.BaseMotivation[3] -= 20;
    }
    public override void RemoveEffect()
    {
        TargetEmp.BaseMotivation[3] += 20;
        base.RemoveEffect();
    }
}

//逐出师门
public class Perk21 : Perk
{
    public Perk21(Employee Emp) : base(Emp)
    {
        Name = "逐出师门";
        Description = "关系交往动机-10，坚韧-2";
        TimeLeft = 384;
        Num = 21;
    }
    public override void ImmEffect()
    {
        TargetEmp.Tenacity -= 2;
        TargetEmp.BaseMotivation[3] -= 10;
    }

    public override void RemoveEffect()
    {
        TargetEmp.Tenacity += 2;
        TargetEmp.BaseMotivation[3] += 10;
        base.RemoveEffect();
    }
}

//友尽
public class Perk22 : Perk
{
    public Perk22(Employee Emp) : base(Emp)
    {
        Name = "友尽";
        Description = "关系交往动机-10";
        TimeLeft = 384;
        Num = 22;
    }
    public override void ImmEffect()
    {
        TargetEmp.BaseMotivation[3] -= 10;
    }
    public override void RemoveEffect()
    {
        TargetEmp.BaseMotivation[3] -= 10;
        base.RemoveEffect();
    }
}

//离婚
public class Perk23 : Perk
{
    public Perk23(Employee Emp) : base(Emp)
    {
        Name = "离婚";
        Description = "关系交往动机+10";
        TimeLeft = 384;
        Num = 23;
    }
    public override void ImmEffect()
    {
        TargetEmp.BaseMotivation[3] += 10;
    }
    public override void RemoveEffect()
    {
        TargetEmp.BaseMotivation[3] -= 10;
        base.RemoveEffect();
    }
}

//我恋爱了!
public class Perk24 : Perk
{
    public Perk24(Employee Emp) : base(Emp)
    {
        Name = "我恋爱了!";
        Description = "关系交往动机+40";
        TimeLeft = 96;
        Num = 24;
    }
    public override void ImmEffect()
    {
        TargetEmp.BaseMotivation[3] += 40;
    }
    public override void RemoveEffect()
    {
        TargetEmp.BaseMotivation[3] -= 40;
        base.RemoveEffect();
    }
}

//爱的艺术
public class Perk25 : Perk
{
    public Perk25(Employee Emp) : base(Emp)
    {
        Name = "爱的艺术";
        Description = "坚韧+5";
        TimeLeft = -1;
        Num = 25;
    }
    public override void ImmEffect()
    {
        TargetEmp.Tenacity += 2;
    }

    public override void RemoveEffect()
    {
        TargetEmp.Tenacity -= 5;
        base.RemoveEffect();
    }
}

//悟者
public class Perk26 : Perk
{
    public Perk26(Employee Emp) : base(Emp)
    {
        Name = "悟者";
        Description = "信仰永远为中间偏好，观察+5，人力+5";
        TimeLeft = -1;
        Num = 26;
    }
    public override void TimePass()
    {
        base.TimePass();
        TargetEmp.Character[1] = 0;
    }
    public override void ImmEffect()
    {
        TargetEmp.Observation += 5;
        TargetEmp.HR += 5;
    }

    public override void RemoveEffect()
    {
        TargetEmp.Observation -= 5;
        TargetEmp.HR -= 5;
        base.RemoveEffect();
    }
}

//暗恋
public class Perk27 : Perk
{
    public Perk27(Employee Emp) : base(Emp)
    {
        Name = "暗恋";
        Description = "关系交往动机+15";
        TimeLeft = -1;
        Num = 27;
    }
    public override void ImmEffect()
    {
        TargetEmp.BaseMotivation[3] += 15;
    }
    public override void RemoveEffect()
    {
        TargetEmp.BaseMotivation[3] -= 15;
        base.RemoveEffect();
    }
}

//罢工
public class Perk28 : Perk
{
    public Perk28(Employee Emp) : base(Emp)
    {
        Name = "罢工";
        Description = "不会参与工作";
        TimeLeft = 32;
        Num = 28;
    }
    public override void ImmEffect()
    {
        TargetEmp.canWork = false;
        if (TargetEmp.CurrentDep != null)
            TargetEmp.CurrentDep.UpdateUI();
    }
    public override void RemoveEffect()
    {
        TargetEmp.canWork = true;
        if (TargetEmp.CurrentDep != null)
            TargetEmp.CurrentDep.UpdateUI();
        base.RemoveEffect();
    }
}