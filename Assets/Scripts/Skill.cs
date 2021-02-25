using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{ Basic, Defense, Upgrade}




//此技能系统为旧版头脑风暴相关
//目前相关玩法均已删除
//此脚本仅留作参考

#region 旧技能



public class Skill
{
    public SkillType Type = SkillType.Basic;
    public int StaminaCost, DiceCost, StaminaExtra, DiceExtra; //Extra为需要减掉的额外值，所以额外值越大实际消耗越低
    public int EffectMode = 1; //EffectMode为2时需要选择作用员工 为3时需要选择骰子 为4时需要选择技能 为5时需要选择两名员工 为6时需要选择技能再选择员工
    //DiceExtra指减少的点数，为负时代表需要更多点数
    public string Name, Description;

    public bool ManageSkill = false;//头脑风暴版本2，用来判断是否为管理人员专用技能

    public Employee TargetEmp = null;
    protected SkillControl sc;

    public virtual bool ConditionCheck()
    {
        return true;
    }

    public virtual void StartEffect()
    {
        TargetEmp.Stamina -= (StaminaCost - StaminaExtra);
        if (TargetEmp.InfoDetail.GC.SC.NoStaminaCost == true)//删除无体力消耗效果
        {
            TargetEmp.InfoDetail.GC.SC.NoStaminaCost = false;
            for (int i = 0; i < TargetEmp.InfoDetail.GC.SC.CurrentSkills.Count; i++)
            {
                TargetEmp.InfoDetail.GC.SC.CurrentSkills[i].skill.StaminaExtra = 0;
            }
        }

        if (TargetEmp.InfoDetail.GC.SC.DoubleCost == true)//删除双倍消耗效果
        {
            TargetEmp.InfoDetail.GC.SC.DoubleCost = false;
            for (int i = 0; i < TargetEmp.InfoDetail.GC.SC.CurrentSkills.Count; i++)
            {
                TargetEmp.InfoDetail.GC.SC.CurrentSkills[i].skill.StaminaExtra = 0;
            }
        }
        sc = GameControl.Instance.SC;
    }

    public Skill Clone()
    {
        return (Skill)this.MemberwiseClone();
    }
}

//以上为第二版技能，下面为第三版

//发表看法
public class Skill32 : Skill
{
    public Skill32()
    {
        Name = "发表看法";
        Description = "基本技,产生5点头脑风暴点数";
        StaminaCost = 20;
        DiceCost = 6;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        if (sc.Sp2Multiply > 0)
        {
            TargetEmp.InfoA.GC.SC.CauseDamage(10);
            sc.Sp2Multiply = 0;
        }
        else
            TargetEmp.InfoA.GC.SC.CauseDamage(5);
    }
}

//分析
public class Skill33 : Skill
{
    public Skill33()
    {
        Name = "分析";
        Description = "基本技,产生9点头脑风暴点数";
        StaminaCost = 40;
        DiceCost = 5;
        EffectMode = 1;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        if (sc.Sp2Multiply > 0)
        {
            TargetEmp.InfoA.GC.SC.CauseDamage(18);
            sc.Sp2Multiply = 0;
        }
        else
            TargetEmp.InfoA.GC.SC.CauseDamage(9);
    }
}

//边际利润
public class Skill34 : Skill
{
    public Skill34()
    {
        Name = "边际利润";
        Description = "产生5点头脑风暴点数，发动后本回合每使用一颗骰子造成1点头脑风暴点数";
        StaminaCost = 40;
        DiceCost = 6;
        EffectMode = 1;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoA.GC.SC.CauseDamage(5);
        TargetEmp.InfoA.GC.SC.ExtraDiceDamage += 1;
    }
}

//创意
public class Skill35 : Skill
{
    public Skill35()
    {
        Name = "创意";
        Description = "基本技,产生7点头脑风暴点数";
        StaminaCost = 0;
        DiceCost = 1;
        EffectMode = 1;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        if (sc.Sp2Multiply > 0)
        {
            TargetEmp.InfoA.GC.SC.CauseDamage(14);
            sc.Sp2Multiply = 0;
        }
        else
            TargetEmp.InfoA.GC.SC.CauseDamage(7);
    }
}

//随机应变
public class Skill36 : Skill
{
    public Skill36()
    {
        Name = "随机应变";
        Description = "选择一颗骰子并产生等于该骰子点数的头脑风暴点数";
        StaminaCost = 30;
        DiceCost = 0;
        EffectMode = 1;
        ManageSkill = true;
    }

    public override bool ConditionCheck()
    {
        //只能选中一个骰子
        if (TargetEmp.InfoDetail.GC.SC.SelectedDices.Count == 1)
            return true;
        else
            return false;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        int D = 0;
        for(int i = 0; i < TargetEmp.InfoDetail.GC.SC.SelectedDices.Count; i++)
        {
            D += TargetEmp.InfoDetail.GC.SC.SelectedDices[i].value;
        }
        TargetEmp.InfoDetail.GC.SC.CauseDamage(D);
    }
}

//连续推理
public class Skill37 : Skill
{
    public Skill37()
    {
        Name = "连续推理";
        Description = "基本技,产生2点头脑风暴点数，连续产生5次";
        StaminaCost = 20;
        DiceCost = 8;
        EffectMode = 1;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        if (sc.Sp2Multiply > 0)
        {
            for (int i = 0; i < 5; i++)
            {
                TargetEmp.InfoA.GC.SC.CauseDamage(4);
            }
            sc.Sp2Multiply = 0;
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                TargetEmp.InfoA.GC.SC.CauseDamage(2);
            }
        }             
    }
}

//坚定信念
public class Skill38 : Skill
{
    public Skill38()
    {
        Name = "坚定信念";
        Description = "选择一名员工，将其信心转化为头脑风暴点数";
        StaminaCost = 40;
        DiceCost = 10;
        EffectMode = 2;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.SC.CauseDamage(TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp.Confidence);
        TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp.Confidence = 0;
    }
}

//妥协姿态
public class Skill39 : Skill
{
    public Skill39()
    {
        Name = "妥协姿态";
        Description = "基本技,选择1个技能禁用2回合，产生7点头脑风暴点数";
        StaminaCost = 20;
        DiceCost = 6;
        EffectMode = 4;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.SC.TargetSkill.LockTime += 2;
        TargetEmp.InfoDetail.GC.SC.TargetSkill.SkillLock(false);
        TargetEmp.InfoDetail.GC.SC.SkillLockBonusCheck();

        if (sc.Sp2Multiply > 0)
        {
            TargetEmp.InfoDetail.GC.SC.CauseDamage(14);
            sc.Sp2Multiply = 0;
        }
        else
            TargetEmp.InfoDetail.GC.SC.CauseDamage(7);
    }
}

//深刻洞察
public class Skill40 : Skill
{
    public Skill40()
    {
        Name = "深刻洞察";
        Description = "造成3点洞察";
        StaminaCost = 30;
        DiceCost = 10;
        EffectMode = 1;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.SC.DotValue += 3;
    }
}

//和解
public class Skill41 : Skill
{
    public Skill41()
    {
        Name = "和解";
        Description = "基本技,选择1名员工回复20点心力";
        StaminaCost = 50;
        DiceCost = 5;
        EffectMode = 2;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        if (sc.Sp2Multiply > 0)
        {
            TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp.Mentality += 40;
            sc.Sp2Multiply = 0;
        }
        else
            TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp.Mentality += 20;

    }
}

//熟练按摩
public class Skill42 : Skill
{
    public Skill42()
    {
        Name = "熟练按摩";
        Description = "基本技,选择一名员工回复15点体力";
        StaminaCost = 20;
        DiceCost = 4;
        EffectMode = 2;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        if (sc.Sp2Multiply > 0)
        {
            TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp.Stamina += 30;
            sc.Sp2Multiply = 0;
        }
        else
            TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp.Stamina += 15;
    }
}

//大师级按摩
public class Skill43 : Skill
{
    public Skill43()
    {
        Name = "大师级按摩";
        Description = "选择一名员工，体力翻倍";
        StaminaCost = 40;
        DiceCost = 10;
        EffectMode = 2;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp.Stamina += TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp.Stamina;
    }
}

//私下承诺
public class Skill44 : Skill
{
    public Skill44()
    {
        Name = "私下承诺";
        Description = "基本技,选择一名员工，信心+10";
        StaminaCost = 20;
        DiceCost = 3;
        EffectMode = 2;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        if (sc.Sp2Multiply > 0)
        {
            TargetEmp.InfoDetail.emp.Confidence += 20;
            sc.Sp2Multiply = 0;
        }
        else
            TargetEmp.InfoDetail.emp.Confidence += 10;
    }
}

//扭曲现实力场
public class Skill45 : Skill
{
    public Skill45()
    {
        Name = "扭曲现实力场";
        Description = "将一名员工信心翻倍";
        StaminaCost = 30;
        DiceCost = 8;
        EffectMode = 2;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp.InfoDetail.emp.Confidence += TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp.Confidence;
    }
}

//权衡利弊
public class Skill46 : Skill
{
    public Skill46()
    {
        Name = "权衡利弊";
        Description = "选择一名员工心力-30点，再选择一名员工信心+40";
        StaminaCost = 10;
        DiceCost = 8;
        EffectMode = 5;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp.Mentality -= 30;
        TargetEmp.InfoDetail.GC.CurrentEmpInfo2.emp.Confidence += 40;
    }
}

//折衷选择
public class Skill47 : Skill
{
    public Skill47()
    {
        Name = "折衷选择";
        Description = "选择1个技能禁用2回合，指定一名员工产生15点信心";
        StaminaCost = 20;
        DiceCost = 6;
        EffectMode = 6;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.SC.TargetSkill.LockTime += 2;
        TargetEmp.InfoDetail.GC.SC.TargetSkill.SkillLock(false);
        TargetEmp.InfoDetail.GC.SC.SkillLockBonusCheck();
        TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp.Confidence += 15;
    }
}

//解放思维
public class Skill48 : Skill
{
    public Skill48()
    {
        Name = "解放思维";
        Description = "会议增加1点想象力";
        StaminaCost = 30;
        DiceCost = 11;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.SC.ExtraDamage += 1;
    }
}

//会议重点
public class Skill49 : Skill
{
    public Skill49()
    {
        Name = "会议重点";
        Description = "下一个基础技能产生的头脑风暴点数翻倍";
        StaminaCost = 20;
        DiceCost = 5;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.SC.Sp2Multiply += 1;
    }
}

//头脑雷暴
public class Skill50 : Skill
{
    public Skill50()
    {
        Name = "头脑雷暴";
        Description = "当前洞察翻倍";
        StaminaCost = 30;
        DiceCost = 10;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.SC.DotValue *= 2;
    }
}

//精打细算
public class Skill51 : Skill
{
    public Skill51()
    {
        Name = "精打细算";
        Description = "选中1颗点数大于2的骰子，并将其分成3个随机点数的骰子";
        StaminaCost = 20;
        DiceCost = 10;
        EffectMode = 3;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        DiceControl d = TargetEmp.InfoDetail.GC.SC.TargetDice;
        int valueA, valueB, valueC, Value = d.value;
        valueA = Random.Range(1, Value - 1);
        Value -= valueA;
        valueB = Random.Range(1, Value);
        valueC = Value - valueB;
        TargetEmp.InfoDetail.GC.SC.CreateDice(1, valueA);
        TargetEmp.InfoDetail.GC.SC.CreateDice(1, valueB);
        TargetEmp.InfoDetail.GC.SC.CreateDice(1, valueC);
        TargetEmp.InfoDetail.GC.SC.Dices.Remove(d);
        Object.Destroy(d.gameObject);
    }
}

//以退为进
public class Skill52 : Skill
{
    public Skill52()
    {
        Name = "以退为进";
        Description = "本回合每禁用一个技能，额外获得2颗骰子";
        StaminaCost = 15;
        DiceCost = 0;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.SC.SkillLockBonus += 1;
    }
}

//放轻松
public class Skill53 : Skill
{
    public Skill53()
    {
        Name = "放轻松";
        Description = "下一次使用的技能体力消耗减半";
        StaminaCost = 30;
        DiceCost = 10;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.SC.NoStaminaCost = true;
        for(int i = 0; i < TargetEmp.InfoDetail.GC.SC.CurrentSkills.Count; i++)
        {
            TargetEmp.InfoDetail.GC.SC.CurrentSkills[i].skill.StaminaExtra +=
                ((TargetEmp.InfoDetail.GC.SC.CurrentSkills[i].skill.StaminaCost - TargetEmp.InfoDetail.GC.SC.CurrentSkills[i].skill.StaminaExtra) / 2);
        }
    }
}

//从容应对
public class Skill54 : Skill
{
    public Skill54()
    {
        Name = "从容应对";
        Description = "额外获得2颗骰子";
        StaminaCost = 40;
        DiceCost = 6;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.SC.CreateDice(2);
    }
}

//模仿
public class Skill55 : Skill
{
    public Skill55()
    {
        Name = "模仿";
        Description = "选择1颗骰子并获得其复制";
        StaminaCost = 15;
        DiceCost = 6;
        EffectMode = 3;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.SC.CreateDice(1, TargetEmp.InfoDetail.GC.SC.TargetDice.value);
    }
}

//递归思想
public class Skill56 : Skill
{
    public Skill56()
    {
        Name = "递归思想";
        Description = "选择一枚骰子，将所有骰子变成该点数";
        StaminaCost = 30;
        DiceCost = 6;
        EffectMode = 3;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        for(int i = 0; i < TargetEmp.InfoDetail.GC.SC.Dices.Count; i++)
        {
            if (TargetEmp.InfoDetail.GC.SC.Dices[i].toggle.interactable == true)
                TargetEmp.InfoDetail.GC.SC.Dices[i].SetValue(TargetEmp.InfoDetail.GC.SC.TargetDice.value);
        }
    }
}

//步步为营
public class Skill57 : Skill
{
    public Skill57()
    {
        Name = "步步为营";
        Description = "本回合每使用1个技能额外获得1颗骰子";
        StaminaCost = 30;
        DiceCost = 10;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.SC.Sp1Multiply += 1;
    }
}

//破局
public class Skill58 : Skill
{
    public Skill58()
    {
        Name = "破局";
        Description = "丢弃所有骰子，重新获得等数量的骰子";
        StaminaCost = 30;
        DiceCost = 6;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        for (int i = 0; i < TargetEmp.InfoDetail.GC.SC.Dices.Count; i++)
        {
            if (TargetEmp.InfoDetail.GC.SC.Dices[i].toggle.interactable == true)
                TargetEmp.InfoDetail.GC.SC.Dices[i].SetValue(Random.Range(1, 7));
        }
    }
}

//深刻洞察+
public class Skill59 : Skill
{
    public Skill59()
    {
        Name = "深刻洞察+";
        Description = "造成5点洞察";
        StaminaCost = 30;
        DiceCost = 10;
        EffectMode = 1;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.SC.DotValue += 5;
    }
}

//发表看法+
public class Skill60 : Skill
{
    public Skill60()
    {
        Name = "发表看法+";
        Description = "基本技,产生9点头脑风暴点数";
        StaminaCost = 20;
        DiceCost = 6;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        if (sc.Sp2Multiply > 0)
        {
            TargetEmp.InfoA.GC.SC.CauseDamage(18);
            sc.Sp2Multiply = 0;
        }
        else
            TargetEmp.InfoA.GC.SC.CauseDamage(9);
    }
}

//私下承诺+
public class Skill61 : Skill
{
    public Skill61()
    {
        Name = "私下承诺+";
        Description = "基本技,选择一名员工，信心+20";
        StaminaCost = 20;
        DiceCost = 3;
        EffectMode = 2;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        if (sc.Sp2Multiply > 0)
        {
            TargetEmp.InfoDetail.emp.Confidence += 40;
            sc.Sp2Multiply = 0;
        }
        else
            TargetEmp.InfoDetail.emp.Confidence += 20;

    }
}

//熟练按摩+
public class Skill62 : Skill
{
    public Skill62()
    {
        Name = "熟练按摩+";
        Description = "基本技,选择一名员工回复30点体力";
        StaminaCost = 20;
        DiceCost = 4;
        EffectMode = 2;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        if (sc.Sp2Multiply > 0)
        {
            TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp.Stamina += 60;
            sc.Sp2Multiply = 0;
        }
        else
            TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp.Stamina += 30;
    }
}

//和解+
public class Skill63 : Skill
{
    public Skill63()
    {
        Name = "和解+";
        Description = "基本技,选择2名员工回复20点心力";
        StaminaCost = 50;
        DiceCost = 5;
        EffectMode = 5;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        if (sc.Sp2Multiply > 0)
        {
            TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp.Mentality += 40;
            TargetEmp.InfoDetail.GC.CurrentEmpInfo2.emp.Mentality += 40;
            sc.Sp2Multiply = 0;
        }
        else
        {
            TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp.Mentality += 20;
            TargetEmp.InfoDetail.GC.CurrentEmpInfo2.emp.Mentality += 20;
        }
    }
}

//边际利润+
public class Skill64 : Skill
{
    public Skill64()
    {
        Name = "边际利润+";
        Description = "产生5点头脑风暴点数，发动后本回合每使用一颗骰子造成3点头脑风暴点数";
        StaminaCost = 40;
        DiceCost = 6;
        EffectMode = 1;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoA.GC.SC.CauseDamage(5);
        TargetEmp.InfoA.GC.SC.ExtraDiceDamage += 3;
    }
}

//分析+
public class Skill65 : Skill
{
    public Skill65()
    {
        Name = "分析+";
        Description = "基本技,产生13点头脑风暴点数";
        StaminaCost = 40;
        DiceCost = 5;
        EffectMode = 1;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        if (sc.Sp2Multiply > 0)
        {
            TargetEmp.InfoA.GC.SC.CauseDamage(26);
            sc.Sp2Multiply = 0;
        }
        else
            TargetEmp.InfoA.GC.SC.CauseDamage(13);
    }
}

//随机应变+
public class Skill66 : Skill
{
    public Skill66()
    {
        Name = "随机应变+";
        Description = "产生等于已选骰子点数和的头脑风暴点数";
        StaminaCost = 15;
        DiceCost = 0;
        EffectMode = 1;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        int D = 0;
        for (int i = 0; i < TargetEmp.InfoDetail.GC.SC.SelectedDices.Count; i++)
        {
            D += TargetEmp.InfoDetail.GC.SC.SelectedDices[i].value;
        }
        TargetEmp.InfoDetail.GC.SC.CauseDamage(D);
    }
}

//放轻松+
public class Skill67 : Skill
{
    public Skill67()
    {
        Name = "放轻松+";
        Description = "下一次使用的技能不消耗体力";
        StaminaCost = 30;
        DiceCost = 10;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.SC.NoStaminaCost = true;
        for (int i = 0; i < TargetEmp.InfoDetail.GC.SC.CurrentSkills.Count; i++)
        {
            TargetEmp.InfoDetail.GC.SC.CurrentSkills[i].skill.StaminaExtra = TargetEmp.InfoDetail.GC.SC.CurrentSkills[i].skill.StaminaCost;
        }
    }
}

//步步为营+
public class Skill68 : Skill
{
    public Skill68()
    {
        Name = "步步为营+";
        Description = "本回合每使用1个技能额外获得2颗骰子";
        StaminaCost = 30;
        DiceCost = 10;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.SC.Sp1Multiply += 2;
    }
}

#endregion
