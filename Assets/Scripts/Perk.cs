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
    public int TimeLeft;  //单位周
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

    public void TimePass()
    {
        TimeLeft -= 1;
        if (TimeLeft < 0)
            RemoveEffect();
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

//硬撑
public class Perk1 : Perk
{
    public Perk1(Employee Emp) : base(Emp)
    {
        Name = "硬撑";
        Description = "强壮+2，心力下降20点";
        TimeLeft = 32;
    }

    public override void ImmEffect()
    {
        TargetEmp.Strength += 2;
        TargetEmp.Mentality -= 20;
    }

    public override void RemoveEffect()
    {
        TargetEmp.Strength -= 2;
        TargetEmp.Mentality += 20;
        base.RemoveEffect();
    }
}

//斯巴达
public class Perk2 : Perk
{
    public Perk2(Employee Emp) : base(Emp)
    {
        Name = "斯巴达";
        Description = "强壮+5";
        TimeLeft = 32;
    }

    public override void ImmEffect()
    {
        TargetEmp.Strength += 5;
    }

    public override void RemoveEffect()
    {
        TargetEmp.Strength -= 5;
        base.RemoveEffect();
    }
}

//怪力
public class Perk3 : Perk
{
    public Perk3(Employee Emp) : base(Emp)
    {
        Name = "怪力";
        Description = "强壮+2";
        TimeLeft = 96;
    }

    public override void ImmEffect()
    {
        TargetEmp.Strength += 2;
    }

    public override void RemoveEffect()
    {
        TargetEmp.Strength -= 2;
        base.RemoveEffect();
    }
}

//疲劳
public class Perk4 : Perk
{
    public Perk4(Employee Emp) : base(Emp)
    {
        Name = "疲劳";
        Description = "每月生病几率增加20%";
        TimeLeft = 96;
        effectType = EffectType.Monthly;
    }

    public override void ContinuousEffect()
    {
        float Posb = Random.Range(0, 1);
        if(Posb > 0.2f)
        {
            Info.empInfo.AddPerk(new Perk15(TargetEmp), true);
        }
    }

    public override void RemoveEffect()
    {
        TargetEmp.Strength -= 5;
        base.RemoveEffect();
    }
}

//高效运行
public class Perk5 : Perk
{
    public Perk5(Employee Emp) : base(Emp)
    {
        Name = "高效运行";
        Description = "职业技能+3，每周体力-10";
        TimeLeft = 32;
        effectType = EffectType.Weekly;
    }

    public override void ImmEffect()
    {
        TargetEmp.SkillExtra1 += 3;
        TargetEmp.SkillExtra2 += 3;
        TargetEmp.SkillExtra3 += 3;
    }

    public override void ContinuousEffect()
    {
        TargetEmp.Stamina -= 10;
    }

    public override void RemoveEffect()
    {
        TargetEmp.SkillExtra1 -= 3;
        TargetEmp.SkillExtra2 -= 3;
        TargetEmp.SkillExtra3 -= 3;
        base.RemoveEffect();
    }
}

//超越极限
public class Perk6 : Perk
{
    public Perk6(Employee Emp) : base(Emp)
    {
        Name = "超越极限";
        Description = "职业技能+100%，每周体力-20";
        TimeLeft = 32;
        effectType = EffectType.Weekly;
    }

    public override void ImmEffect()
    {
        TempValue1 = TargetEmp.Skill1;
        TempValue2 = TargetEmp.Skill2;
        TempValue3 = TargetEmp.Skill3;
        TargetEmp.SkillExtra1 += TempValue1;
        TargetEmp.SkillExtra2 += TempValue2;
        TargetEmp.SkillExtra3 += TempValue3;
    }

    public override void ContinuousEffect()
    {
        TargetEmp.Stamina -= 20;
    }

    public override void RemoveEffect()
    {
        TargetEmp.SkillExtra1 -= TempValue1;
        TargetEmp.SkillExtra2 -= TempValue2;
        TargetEmp.SkillExtra3 -= TempValue3;
        base.RemoveEffect();
    }
}

//压力
public class Perk7 : Perk
{
    public Perk7(Employee Emp) : base(Emp)
    {
        Name = "压力";
        Description = "职业技能+2，每周心力-30";
        TimeLeft = 24;
        effectType = EffectType.Weekly;
    }

    public override void ImmEffect()
    {
        TargetEmp.SkillExtra1 += 2;
        TargetEmp.SkillExtra2 += 2;
        TargetEmp.SkillExtra3 += 2;
    }

    public override void ContinuousEffect()
    {
        TargetEmp.Mentality -= 30;
    }

    public override void RemoveEffect()
    {
        TargetEmp.SkillExtra1 -= 2;
        TargetEmp.SkillExtra2 -= 2;
        TargetEmp.SkillExtra3 -= 2;
        base.RemoveEffect();
    }
}

//逃亡主义
public class Perk8 : Perk
{
    public Perk8(Employee Emp) : base(Emp)
    {
        Name = "逃亡主义";
        Description = "心力小于50时，立刻辞职";
        TimeLeft = 72000;
    }

    public override void ImmEffect()
    {
        TargetEmp.WantLeave = true;
    }

    public override void RemoveEffect()
    {
        TargetEmp.WantLeave = false;
        base.RemoveEffect();
    }
}

//麻木
public class Perk9 : Perk
{
    public Perk9(Employee Emp) : base(Emp)
    {
        Name = "麻木";
        Description = "韧性+2";
        TimeLeft = 96;
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

//压抑
//**未完成**

public class Perk10 : Perk
{
    public Perk10(Employee Emp) : base(Emp)
    {
        Name = "压抑";
        Description = "韧性+1，热情-1";
        TimeLeft = 96;
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

//11反抗精神
//**未加入**

//狂热
public class Perk12 : Perk
{
    public Perk12(Employee Emp) : base(Emp)
    {
        Name = "高效运行";
        Description = "职业技能+3，每周体力-10";
        TimeLeft = 32;
        effectType = EffectType.Weekly;
    }

    public override void ContinuousEffect()
    {
        TargetEmp.Stamina -= 10;
        TargetEmp.Mentality += 10;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
    }
}

//13咖啡成瘾者
//**未加入**

//14懒惰
//**策划案还没写**

//生病
public class Perk15 : Perk 
{
    public Perk15(Employee Emp) : base(Emp)
    {
        Name = "生病";
        Description = "每周心力-20，强壮-5";
        effectType = EffectType.Weekly;
        TimeLeft = 32;
    }

    public override void ImmEffect()
    {
        TargetEmp.Strength -= 5;
    }

    public override void ContinuousEffect()
    {
        TargetEmp.Mentality -= 20;
    }

    public override void RemoveEffect()
    {
        TargetEmp.Strength += 5;
        base.RemoveEffect();
    }
}

//烦躁
public class Perk16 : Perk
{
    public Perk16(Employee Emp) : base(Emp)
    {
        Name = "烦躁";
        Description = "每周心力-20";
        TimeLeft = 32;
        effectType = EffectType.Weekly;
    }

    public override void ContinuousEffect()
    {
        TargetEmp.Mentality -= 20;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
    }
}