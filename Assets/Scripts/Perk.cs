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
    public int TimeLeft, Num = 0, Level = 1, BaseTime;  //Time单位时,BaseTime用于可叠加Perk清除层数时重置时间
    public bool Positive = true, canStack = false;
    public string Name, Description;
    public EffectType effectType;

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

    public Perk Clone()
    {
        return (Perk)this.MemberwiseClone();
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
    int Value = -1;
    public Perk3(Employee Emp) : base(Emp)
    {
        Name = "启发";
        if (TargetEmp.CurrentDep != null)
            Value = TargetEmp.CurrentDep.building.effectValue - 1;
        else if (TargetEmp.CurrentOffice != null)
            Value = TargetEmp.CurrentOffice.building.effectValue - 1;
        Description = "随机增加当前工作领域1点热情";
        TimeLeft = 64;
        Num = 3;
    }

    public override void ImmEffect()
    {
        if (Value != -1)
        {
            Value /= 3;
            if (TargetEmp.Stars[Value] < TargetEmp.StarLimit[Value])
            {
                TargetEmp.InfoDetail.AddPerk(new Perk109(TargetEmp), true);
            }
        }
    }

    public override void RemoveEffect()
    {
        for (int i = 0; i < TargetEmp.InfoDetail.PerksInfo.Count; i++)
        {
            if (TargetEmp.InfoDetail.PerksInfo[i].CurrentPerk.Num == 12)
            {
                TargetEmp.InfoDetail.AddPerk(new Perk3(TargetEmp), true);
                break;
            }
        }
        base.RemoveEffect();
    }
}

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
        //叠加时重置
        Value = -1;
        if (TargetEmp.CurrentDep != null)
            Value = TargetEmp.CurrentDep.building.effectValue - 1;
        else if (TargetEmp.CurrentOffice != null)
            Value = TargetEmp.CurrentOffice.building.effectValue - 1;
        if (Value != -1)
        { 
            Value /= 3;
            if (TargetEmp.Stars[Value] < TargetEmp.StarLimit[Value])
            {
                TargetEmp.Stars[Value] += 1;
                StarNum.Add(Value);
            }
        }
    }

    public override void RemoveEffect()
    {
        if (StarNum.Count > 0)
        {
            TargetEmp.Stars[StarNum[0]] -= 1;
            if (TargetEmp.Stars[StarNum[0]] < 0)
                TargetEmp.Stars[StarNum[0]] = 0;
            StarNum.RemoveAt(0);
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
        Value1 = Random.Range(1, 6);
        Value2 = Random.Range(1, 6);
        while(Value2 == Value1)
        {
            Value2 = Random.Range(0, 6);
        }
        if (Value1 == 1)
            SName1 = "技术技能";
        else if (Value1 == 2)
            SName1 = "基础技能";
        else if (Value1 == 3)
            SName1 = "经营技能";
        else if (Value1 == 4)
            SName1 = "战略技能";
        else
            SName1 = "社交技能";

        if (Value2 == 1)
            SName2 = "技术技能";
        else if (Value2 == 2)
            SName2 = "基础技能";
        else if (Value2 == 3)
            SName2 = "经营技能";
        else if (Value2 == 4)
            SName2 = "战略技能";
        else
            SName2 = "社交技能";

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
        TargetEmp.CharacterTendency[1] = 0;
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

//内鬼
public class Perk29 : Perk
{
    public Perk29(Employee Emp) : base(Emp)
    {
        Name = "内鬼";
        Description = "谋划野心动机+50";
        TimeLeft = -1;
        Num = 29;
    }
    public override void ImmEffect()
    {

    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
    }
}

//成功率上升
public class Perk30 : Perk
{
    public Perk30(Employee Emp) : base(Emp)
    {
        Name = "成功率上升";
        Description = "部门生产和办公室业务的成功率+1%";
        TimeLeft = 32;
        Num = 30;
    }
    public override void ImmEffect()
    {
        TargetEmp.ExtraSuccessRate += 0.01f;
        TempValue4 = 0.01f;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.ExtraSuccessRate -= 0.01f;
    }
}

//成功率下降
public class Perk31 : Perk
{
    public Perk31(Employee Emp) : base(Emp)
    {
        Name = "成功率下降";
        Description = "部门生产和办公室业务的成功率-1%";
        TimeLeft = 32;
        Num = 31;
    }
    public override void ImmEffect()
    {
        TargetEmp.ExtraSuccessRate -= 0.01f;
        TempValue4 = -0.01f;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.ExtraSuccessRate += 0.01f;
    }
}

//请求升职
public class Perk32 : Perk
{
    public Perk32(Employee Emp) : base(Emp)
    {
        Name = "请求升职";
        Description = "持续时间内没有升职会额外获得蓝色情绪";
        TimeLeft = 16;
        Num = 32;
    }
    public override void ImmEffect()
    {
        
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        if (TargetEmp.CurrentOffice == null)
            TargetEmp.AddEmotion(EColor.Blue);
        else if (TargetEmp.CurrentOffice.building.Type != BuildingType.CEO办公室 || TargetEmp.CurrentOffice.building.Type != BuildingType.高管办公室)
            TargetEmp.AddEmotion(EColor.Blue);
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
    }
    public override void ImmEffect()
    {
        TargetEmp.InfoA.GC.BuildingPay -= 3;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.InfoA.GC.BuildingPay += 3;
    }
}

//强化
public class Perk35 : Perk
{
    private int ReduceValue = 2;
    public Perk35(Employee Emp) : base(Emp)
    {
        Name = "强化";
        Description = "强壮+2点，但最高不超过25";
        TimeLeft = 32;
        Num = 35;
        canStack = true;
    }
    public override void ImmEffect()
    {
        if (TargetEmp.Strength >= 25)
            ReduceValue = 0;
        else
        {
            TargetEmp.Strength += 2;
            if(TargetEmp.Strength > 25)
            {
                ReduceValue = TargetEmp.Strength - 25;
                TargetEmp.Strength = 25;
            }
        }
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.Strength -= ReduceValue;
    }
}

//麻木
public class Perk36 : Perk
{
    public Perk36(Employee Emp) : base(Emp)
    {
        Name = "麻木";
        Description = "抵消一次情绪添加";
        TimeLeft = -1;
        Num = 36;
        canStack = true;
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
    }
}

//铁人
public class Perk37 : Perk
{
    private int ReduceValue = 2;
    public Perk37(Employee Emp) : base(Emp)
    {
        Name = "铁人";
        Description = "坚韧+2点，但最高不超过25";
        TimeLeft = 32;
        Num = 37;
        canStack = true;
    }
    public override void ImmEffect()
    {
        if (TargetEmp.Tenacity >= 25)
            ReduceValue = 0;
        else
        {
            TargetEmp.Tenacity += 2;
            if (TargetEmp.Tenacity > 25)
            {
                ReduceValue = TargetEmp.Tenacity - 25;
                TargetEmp.Tenacity = 25;
            }
        }
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        TargetEmp.Tenacity -= ReduceValue;
    }
}

//自我提升
public class Perk38 : Perk
{
    public Perk38(Employee Emp) : base(Emp)
    {
        Name = "自我提升";
        Description = "工作成功率+5%";
        TimeLeft = 32;
        Num = 38;
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

//蛊惑
public class Perk39 : Perk
{
    public Perk39(Employee Emp) : base(Emp)
    {
        Name = "蛊惑";
        Description = "被蛊惑了";
        TimeLeft = -1;
        Num = 39;
    }
}

//迷茫
public class Perk40 : Perk
{
    public Perk40(Employee Emp) : base(Emp)
    {
        Name = "迷茫";
        Description = "很迷茫";
        TimeLeft = -1;
        Num = 40;
    }
}

//忠诚
public class Perk41 : Perk
{
    public Perk41(Employee Emp) : base(Emp)
    {
        Name = "忠诚";
        Description = "很忠诚";
        TimeLeft = -1;
        Num = 41;
    }
}

#region 部分初始随机特质
//打工人
public class Perk42 : Perk
{
    public Perk42(Employee Emp) : base(Emp)
    {
        Name = "打工人";
        Description = "增加初始及公司场景随机比例各10%";
        TimeLeft = -1;
        Num = 42;
    }
}

//社交狂热
public class Perk43 : Perk
{
    public Perk43(Employee Emp) : base(Emp)
    {
        Name = "社交狂热";
        Description = "增加个人场景随机比例10%，减少公司场景10%";
        TimeLeft = -1;
        Num = 43;
    }
}

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
        Description = "员工工资-5%";
        TimeLeft = 64;
        Num = 44;
        canStack = true;
    }
}

//团结
public class Perk45 : Perk
{
    public Perk45(Employee Emp) : base(Emp)
    {
        Name = "团结";
        Description = "办公室成功率+1%";
        TimeLeft = 64;
        Num = 45;
        canStack = false;
    }
    public override void ImmEffect()
    {
        if (TargetDep != null)
        {
            TargetDep.Efficiency += TempValue4;
            Description = "办公室成功率+" + (TempValue4 * 100) + "%";
        }
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        if (TargetDep != null)
        {
            TargetDep.Efficiency -= TempValue4;
        }
    }
}

//受到启发
public class Perk46 : Perk
{
    public Perk46(Employee Emp) : base(Emp)
    {
        Name = "受到启发";
        Description = "获得一个当前工作领域的热情，解锁事件";
        TimeLeft = 64;
        Num = 46;
        canStack = false;
    }
}

//顺利
public class Perk47 : Perk
{
    public Perk47(Employee Emp) : base(Emp)
    {
        Name = "顺利";
        Description = "解锁事件，并为所在部门附着进步状态，每个顺利+1进步";
        TimeLeft = 64;
        Num = 47;
        canStack = true;
    }
    public override void ImmEffect()
    {
        if (TargetEmp.CurrentDep != null)
            TargetEmp.CurrentDep.AddPerk(new Perk48(null));
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
    }
}

//顺利-进步
public class Perk48 : Perk
{
    public Perk48(Employee Emp) : base(Emp)
    {
        Name = "顺利-进步";
        Description = "每个进步+1% 部门成功率,持续到当前业务结束";
        TimeLeft = -1;//持续到当前业务结束
        Num = 48;
        canStack = true;
    }
    public override void ImmEffect()
    {
        if (TargetDep != null)
        {
            TargetDep.Efficiency += 0.01f;
        }
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        if (TargetDep != null)
        {
            TargetDep.Efficiency -= 0.01f;
        }
    }
}

//悔恨
public class Perk49 : Perk
{
    public Perk49(Employee Emp) : base(Emp)
    {
        Name = "悔恨";
        Description = "解锁事件，并为所在部门附着混乱状态，每个悔恨+1混乱";
        TimeLeft = 64;
        Num = 49;
        canStack = true;
    }
    public override void ImmEffect()
    {
        if (TargetEmp.CurrentDep != null)
            TargetEmp.CurrentDep.AddPerk(new Perk50(null));
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
    }
}

//悔恨-混乱
public class Perk50 : Perk
{
    public Perk50(Employee Emp) : base(Emp)
    {
        Name = "悔恨-混乱";
        Description = "每个混乱-1% 部门成功率,持续到当前业务结束";
        TimeLeft = -1;//持续到当前业务结束
        Num = 50;
        canStack = true;
    }
    public override void ImmEffect()
    {
        if (TargetDep != null)
        {
            TargetDep.Efficiency -= 0.01f;
        }
    }
    public override void RemoveEffect()
    {
        base.RemoveEffect();
        if (TargetDep != null)
        {
            TargetDep.Efficiency += 0.01f;
        }
    }
}

//困惑
public class Perk51 : Perk
{
    public Perk51(Employee Emp) : base(Emp)
    {
        Name = "困惑";
        Description = "解锁事件，公司日常";
        TimeLeft = 64;
        Num = 51;
        canStack = true;
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
        Description = "心力每周下降20点";
        TimeLeft = -1;//特质
        Num = 95;
        canStack = false;
        effectType = EffectType.Weekly;
    }
    public override void ContinuousEffect()
    {
        TargetEmp.Mentality -= 20;
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
            if (TargetDep.CommandingOffice != null && TargetDep.CommandingOffice.CurrentManager != null)
                TempEmps.Add(TargetDep.CommandingOffice.CurrentManager);
            for (int i = 0; i < TempEmps.Count; i++)
            {
                for(int j = i + 1; j < TempEmps.Count; j++)
                {
                    if (TempEmps[i].FindRelation(TempEmps[j]).FriendValue > 0)
                        TempValue1 += 5;
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
            if (TargetDep.CommandingOffice != null && TargetDep.CommandingOffice.CurrentManager != null)
                TempEmps.Add(TargetDep.CommandingOffice.CurrentManager);
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
        Description = "部门信念-15";
        TimeLeft = -1;
        Num = 101;
        canStack = false;
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
        Description = "部门信念+15";
        TimeLeft = -1;
        Num = 102;
        canStack = false;
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
        Description = "部门信念-15";
        TimeLeft = -1;
        Num = 103;
        canStack = false;
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
        Description = "部门信念+15";
        TimeLeft = -1;
        Num = 104;
        canStack = false;
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

public static class PerkData
{
    public static List<Perk> PerkList = new List<Perk>()
    {
        new Perk42(null), new Perk43(null), new Perk82(null), new Perk83(null), new Perk84(null), new Perk85(null), new Perk86(null),
        new Perk87(null), new Perk88(null), new Perk89(null), new Perk90(null), new Perk91(null), new Perk92(null)
    };
}