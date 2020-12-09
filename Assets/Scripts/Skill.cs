using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{ Basic, Defense, Upgrade}

public class Skill
{
    public SkillType Type = SkillType.Basic;
    public int StaminaCost, DiceCost, StaminaExtra, DiceExtra;
    public int EffectMode = 1; //EffectMode为2时需要选择作用员工 为3时需要选择骰子 为4时需要选择技能 为5时需要选择两名员工 为6时需要选择技能再选择员工
    //DiceExtra指减少的点数，为负时代表需要更多点数
    public string Name, Description;

    public bool ManageSkill = false;//头脑风暴版本2，用来判断是否为管理人员专用技能

    public Employee TargetEmp = null;

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
                TargetEmp.InfoDetail.GC.SC.CurrentSkills[i].skill.DiceExtra = 0;
            }
        }
    }

    public Skill Clone()
    {
        return (Skill)this.MemberwiseClone();
    }
}

#region 旧技能
//潜能分析
public class Skill1 : Skill
{
    public Skill1()
    {
        Name = "潜能分析";
        Description = "选择1名员工回复20点体力";
        StaminaCost = 20;
        DiceCost = 6;
        EffectMode = 2;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoA.GC.CurrentEmpInfo.emp.Stamina += 20;
    }
}

//最大化效能
public class Skill2 : Skill
{
    public Skill2()
    {
        Name = "最大化效能";
        Description = "集体员工获得“硬撑”特质 \n 硬撑:强壮+2，心力下降20点";
        StaminaCost = 20;
        DiceCost = 3;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        for (int i = 0; i < TargetEmp.CurrentDep.CurrentEmps.Count; i++)
        {
            TargetEmp.CurrentDep.CurrentEmps[i].InfoDetail.AddPerk(new Perk1(TargetEmp.CurrentDep.CurrentEmps[i]), true);
        }
    }
}

//体能催化
public class Skill3 : Skill
{
    public Skill3()
    {
        Name = "体能催化";
        Description = "选择一名员工获得“斯巴达”特质 \n 斯巴达:强壮+5";
        StaminaCost = 10;
        DiceCost = 2;
        EffectMode = 2;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.CurrentEmpInfo.AddPerk(new Perk2(TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp), true);
    }
}

//全面部署
public class Skill4 : Skill
{
    public Skill4()
    {
        Name = "全面部署";
        Description = "同部门内随机两人体力消耗减少20点";
        StaminaCost = 20;
        DiceCost = 10;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        int max = TargetEmp.CurrentDep.CurrentEmps.Count;
        EmpInfo Target1, Target2 = null;
        if (max == 1)
            Target1 = TargetEmp.CurrentDep.CurrentEmps[0].InfoDetail;
        else if(max == 2)
        {
            Target1 = TargetEmp.CurrentDep.CurrentEmps[0].InfoDetail;
            Target2 = TargetEmp.CurrentDep.CurrentEmps[1].InfoDetail;
        }
        else
        {
            int Random1 = Random.Range(0, max);
            int Random2 = Random.Range(0, max);
            while(Random1 == Random2)
            {
                Random2 = Random.Range(0, max);
            }
            Target1 = TargetEmp.CurrentDep.CurrentEmps[Random1].InfoDetail;
            Target2 = TargetEmp.CurrentDep.CurrentEmps[Random2].InfoDetail;
        }
        for(int i = 0; i < Target1.SkillsInfo.Count; i++)
        {
            Target1.SkillsInfo[i].skill.StaminaExtra += 20;
            if (Target1.SkillsInfo[i].skill.StaminaCost < Target1.SkillsInfo[i].skill.StaminaExtra)
                Target1.SkillsInfo[i].skill.StaminaExtra = Target1.SkillsInfo[i].skill.StaminaCost;
        }
        if(Target2 != null)
        {
            for (int i = 0; i < Target2.SkillsInfo.Count; i++)
            {
                Target2.SkillsInfo[i].skill.StaminaExtra += 20;
                if (Target2.SkillsInfo[i].skill.StaminaCost < Target2.SkillsInfo[i].skill.StaminaExtra)
                    Target2.SkillsInfo[i].skill.StaminaExtra = Target2.SkillsInfo[i].skill.StaminaCost;
            }
        }
    }
}

//突发奇想
public class Skill5 : Skill
{
    public Skill5()
    {
        Name = "突发奇想";
        Description = "减少1人下一次所使用能力的体力消耗30点";
        StaminaCost = 20;
        DiceCost = 2;
        EffectMode = 2;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        for(int i = 0; i < TargetEmp.InfoA.GC.CurrentEmpInfo.SkillsInfo.Count; i++)
        {
            Skill S = TargetEmp.InfoA.GC.CurrentEmpInfo.SkillsInfo[i].skill;
            S.StaminaExtra += 30;
            if (S.StaminaCost < S.StaminaExtra)
                S.StaminaExtra = S.StaminaCost;
        }
    }
}

//支持
public class Skill6 : Skill
{
    public Skill6()
    {
        Name = "支持";
        Description = "本次动员增加2颗管理骰子";
        StaminaCost = 50;
        DiceCost = 7;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoA.GC.SC.CreateDice(2);
    }
}

//倾听
public class Skill7 : Skill
{
    public Skill7()
    {
        Name = "倾听";
        Description = "选择一名员工，减少其能力所需管理骰点数2点";
        StaminaCost = 15;
        DiceCost = 6;
        EffectMode = 2;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        for (int i = 0; i < TargetEmp.InfoA.GC.CurrentEmpInfo.SkillsInfo.Count; i++)
        {
            Skill S = TargetEmp.InfoA.GC.CurrentEmpInfo.SkillsInfo[i].skill;
            S.DiceExtra += 2;
            if (S.DiceCost < S.DiceExtra)
                S.DiceExtra = S.DiceCost;
        }
    }
}

//振奋
public class Skill8 : Skill
{
    public Skill8()
    {
        Name = "振奋";
        Description = "降低同部门所有人能力所需管理骰点数1点";
        StaminaCost = 15;
        DiceCost = 6;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        for(int i = 0; i < TargetEmp.CurrentDep.CurrentEmps.Count; i++)
        {
            for (int j = 0; j < TargetEmp.CurrentDep.CurrentEmps[i].InfoDetail.SkillsInfo.Count; j++)
            {
                Skill S = TargetEmp.CurrentDep.CurrentEmps[i].InfoDetail.SkillsInfo[j].skill;
                S.DiceExtra += 1;
                if (S.DiceCost < S.DiceExtra)
                    S.DiceExtra = S.DiceCost;
            }
        }
    }
}

//洗脑
public class Skill9 : Skill
{
    public Skill9()
    {
        Name = "洗脑";
        Description = "降低1名员工所需管理骰点数4点，员工有一定几率“狂热” \n " +
            "狂热:每日心力+10，每日体力-10";
        StaminaCost = 20;
        DiceCost = 4;
        EffectMode = 2;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        for (int i = 0; i < TargetEmp.InfoA.GC.CurrentEmpInfo.SkillsInfo.Count; i++)
        {
            Skill S = TargetEmp.InfoA.GC.CurrentEmpInfo.SkillsInfo[i].skill;
            S.DiceExtra += 4;
            if (S.DiceCost < S.DiceExtra)
                S.DiceExtra = S.DiceCost;
        }
        float Posb = Random.Range(0, 1);
        if (Posb < 0.3f)
            TargetEmp.InfoA.GC.CurrentEmpInfo.AddPerk(new Perk12(TargetEmp.InfoA.GC.CurrentEmpInfo.emp), true);
    }
}

//严苛标准
public class Skill10 : Skill
{
    public Skill10()
    {
        Name = "严苛标准";
        Description = "提升1名员工能力所需管理骰点数2点";
        StaminaCost = 15;
        DiceCost = 6;
        EffectMode = 2;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        for (int i = 0; i < TargetEmp.InfoA.GC.CurrentEmpInfo.SkillsInfo.Count; i++)
        {
            Skill S = TargetEmp.InfoA.GC.CurrentEmpInfo.SkillsInfo[i].skill;
            S.DiceExtra -= 2;
        }
     }
}

//决断
public class Skill11 : Skill
{
    public Skill11()
    {
        Name = "决断";
        Description = "部门效率+20%，全体员工心力-40";
        StaminaCost = 30;
        DiceCost = 6;
        EffectMode = 1;
    }

    public override bool ConditionCheck()
    {
        for (int i = 0; i < TargetEmp.CurrentDep.CurrentEmps.Count; i++)
        {
            if (TargetEmp.CurrentDep.CurrentEmps[i].Mentality < 40)
                return false;
        }
        return true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        new ProduceBuff(0.2f, TargetEmp.CurrentDep);
        for(int i = 0; i < TargetEmp.CurrentDep.CurrentEmps.Count; i++)
        {
            TargetEmp.CurrentDep.CurrentEmps[i].Mentality -= 40;
        }
    }
}

//应急班次
public class Skill12 : Skill
{
    public Skill12()
    {
        Name = "应急班次";
        Description = "部门效率+20%，部门全体员工获得一层“疲劳”特质 \n 疲劳:每月生病几率增加20%";
        StaminaCost = 30;
        DiceCost = 3;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        new ProduceBuff(0.2f, TargetEmp.CurrentDep);
        for (int i = 0; i < TargetEmp.CurrentDep.CurrentEmps.Count; i++)
        {
            TargetEmp.CurrentDep.CurrentEmps[i].InfoDetail.AddPerk(new Perk4(TargetEmp.CurrentDep.CurrentEmps[i]), true);
        }
    }
}

//大局观
public class Skill13 : Skill
{
    public Skill13()
    {
        Name = "大局观";
        Description = "降低本人心力80点，部门效率+20%";
        StaminaCost = 20;
        DiceCost = 12;
        EffectMode = 1;
    }
    public override bool ConditionCheck()
    {
        if (TargetEmp.Mentality < 80)
            return false;
        else
            return true;
    }
    public override void StartEffect()
    {
        base.StartEffect();
        new ProduceBuff(0.2f, TargetEmp.CurrentDep);
        TargetEmp.Mentality -= 80;
    }
}

//平常之心
public class Skill14 : Skill
{
    public Skill14()
    {
        Name = "平常之心";
        Description = "部门效率+50%，部门全体员工心力均大于80时才能发动";
        StaminaCost = 30;
        DiceCost = 10;
        EffectMode = 1;
    }

    public override bool ConditionCheck()
    {
        for (int i = 0; i < TargetEmp.CurrentDep.CurrentEmps.Count; i++)
        {
            if (TargetEmp.CurrentDep.CurrentEmps[i].Mentality < 80)
                return false;
        }
        return true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        new ProduceBuff(0.5f, TargetEmp.CurrentDep);
    }
}

//打破常规
public class Skill15 : Skill
{
    public Skill15()
    {
        Name = "打破常规";
        Description = "部门效率+20%，随机减少部门所有员工体力0-30点";
        StaminaCost = 20;
        DiceCost = 5;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        new ProduceBuff(0.2f, TargetEmp.CurrentDep);
        for (int i = 0; i < TargetEmp.CurrentDep.CurrentEmps.Count; i++)
        {
            TargetEmp.CurrentDep.CurrentEmps[i].Stamina -= Random.Range(0, 31);
        }
    }
}

//高效
public class Skill16 : Skill
{
    public Skill16()
    {
        Name = "高效";
        Description = "获得“高效运行”特质 \n 高效运行:职业技能+3，每日体力-10";
        StaminaCost = 30;
        DiceCost = 3;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.AddPerk(new Perk5(TargetEmp), true);
    }
}

//超越极限
public class Skill17 : Skill
{
    public Skill17()
    {
        Name = "超越极限";
        Description = "选择一名员工获得“超越极限”特质 \n 超越极限:职业技能+100%，每日体力-20";
        StaminaCost = 50;
        DiceCost = 2;
        EffectMode = 2;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.CurrentEmpInfo.AddPerk(new Perk6(TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp), true);
    }
}

//剩余价值
public class Skill18 : Skill
{
    public Skill18()
    {
        Name = "剩余价值";
        Description = "每增加1点“疲劳”特质，职业技能+2";
        StaminaCost = 20;
        DiceCost = 2;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        for(int i = 0; i < TargetEmp.InfoDetail.PerksInfo.Count; i++)
        {
            if (TargetEmp.InfoDetail.PerksInfo[i].CurrentPerk.Name == "疲劳")
            {
                TargetEmp.SkillExtra1 += 2;
                TargetEmp.SkillExtra2 += 2;
                TargetEmp.SkillExtra3 += 2;
            }
        }
    }
}

//压力转化
public class Skill19 : Skill
{
    public Skill19()
    {
        Name = "压力转化";
        Description = "获得“压力”，职业技能+1";
        StaminaCost = 5;
        DiceCost = 3;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.SkillExtra1 += 1;
        TargetEmp.SkillExtra2 += 1;
        TargetEmp.SkillExtra3 += 1;
        TargetEmp.InfoDetail.AddPerk(new Perk7(TargetEmp), true);
    }
}

//压力转化
public class Skill20 : Skill
{
    public Skill20()
    {
        Name = "压力转化";
        Description = "获得“压力”，职业技能+1";
        StaminaCost = 20;
        DiceCost = 2;
        EffectMode = 2;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp.SkillExtra1 += 4;
        TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp.SkillExtra2 += 4;
        TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp.SkillExtra3 += 4;
        TargetEmp.InfoDetail.GC.CurrentEmpInfo.AddPerk(new Perk8(TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp), true);
    }
}
//以上为旧技能，21开始为新技能


//基础1
public class Skill21 : Skill
{
    public Skill21()
    {
        Name = "基础1";
        Description = "产生5点头脑风暴点数";
        StaminaCost = 10;
        DiceCost = 1;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoA.GC.SC.CurrentPoint += 5;
    }
}

//基础2
public class Skill22 : Skill
{
    public Skill22()
    {
        Name = "基础2";
        Description = "产生10点头脑风暴点数";
        StaminaCost = 20;
        DiceCost = 2;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoA.GC.SC.CurrentPoint += 10;
    }
}

//防御1
public class Skill23 : Skill
{
    public Skill23()
    {
        Name = "防御1";
        Description = "接下来两次使用技能时不发生事件";
        StaminaCost = 20;
        DiceCost = 3;
        EffectMode = 1;
        Type = SkillType.Defense;
    }

    public override void StartEffect()
    {
        base.StartEffect();
    }
}

//基础3
public class Skill24 : Skill
{
    public Skill24()
    {
        Name = "基础3";
        Description = "产生n*3点头脑风暴点数(n=发动后剩余骰子的数量)";
        StaminaCost = 20;
        DiceCost = 4;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        SkillControl S = TargetEmp.InfoA.GC.SC;
        int num = S.Dices.Count - S.SelectedDices.Count;
        S.CurrentPoint += (num * 3);
    }
}

//增幅1
public class Skill25 : Skill
{
    public Skill25()
    {
        Name = "增幅1";
        Description = "下一个基础技能触发前（包括下一个基础技能本身）所有产生的头脑风暴点数翻倍";
        StaminaCost = 20;
        DiceCost = 5;
        EffectMode = 1;
        Type = SkillType.Upgrade;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoA.GC.SC.CurrentPoint *= 2;
        TargetEmp.InfoA.GC.SC.Sp2Multiply += 1;
    }
}

//增幅2
public class Skill26 : Skill
{
    public Skill26()
    {
        Name = "增幅2";
        Description = "接下来每使用掉一颗骰子产生1点头脑风暴点数";
        StaminaCost = 20;
        DiceCost = 6;
        EffectMode = 1;
        Type = SkillType.Upgrade;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoA.GC.SC.Sp3Multiply += 1;
    }
}

//基础4
public class Skill27 : Skill
{
    public Skill27()
    {
        Name = "基础4";
        Description = "产生n*0.1点头脑风暴点数（n为当前心力值）";
        StaminaCost = 40;
        DiceCost = 1;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoA.GC.SC.CurrentPoint += (int)(TargetEmp.Mentality * 0.1f);
    }
}

//基础5
public class Skill28 : Skill
{
    public Skill28()
    {
        Name = "基础5";
        Description = "产生n点头脑风暴点数（n为当前已消耗体力值）";
        StaminaCost = 40;
        DiceCost = 2;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoA.GC.SC.CurrentPoint += (100 - TargetEmp.Stamina + StaminaCost + StaminaExtra);
    }
}

//防御2
public class Skill29 : Skill
{
    public Skill29()
    {
        Name = "防御2";
        Description = "全队回复20点心力";
        StaminaCost = 50;
        DiceCost = 3;
        EffectMode = 1;
        Type = SkillType.Defense;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        for(int i = 0; i < TargetEmp.CurrentDep.CurrentEmps.Count; i++)
        {
            TargetEmp.CurrentDep.CurrentEmps[i].Mentality += 20;
        }
    }
}

//防御3
public class Skill30 : Skill
{
    public Skill30()
    {
        Name = "防御3";
        Description = "指定一名员工回复20点心力";
        StaminaCost = 15;
        DiceCost = 4;
        EffectMode = 2;
        Type = SkillType.Defense;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp.Mentality += 20;
    }
}

//基础6
public class Skill31 : Skill
{
    public Skill31()
    {
        Name = "基础6";
        Description = "额外消耗20心力,产生12点头脑风暴点数";
        StaminaCost = 20;
        DiceCost = 6;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoA.GC.SC.CurrentPoint += 12;
        TargetEmp.Mentality -= (20 + StaminaExtra);
    }
}
#endregion

//以上为第二版技能，下面为第三版

//发表看法
public class Skill32 : Skill
{
    public Skill32()
    {
        Name = "发表看法";
        Description = "产生5点头脑风暴点数";
        StaminaCost = 20;
        DiceCost = 6;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoA.GC.SC.CauseDamage(5);
    }
}

//分析
public class Skill33 : Skill
{
    public Skill33()
    {
        Name = "分析";
        Description = "产生9点头脑风暴点数";
        StaminaCost = 40;
        DiceCost = 5;
        EffectMode = 1;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoA.GC.SC.CauseDamage(9);
    }
}

//边际利润
public class Skill34 : Skill
{
    public Skill34()
    {
        Name = "分析";
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
        Description = "产生7点头脑风暴点数";
        StaminaCost = 0;
        DiceCost = 1;
        EffectMode = 1;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoA.GC.SC.CauseDamage(7);
    }
}

//随机应变
public class Skill36 : Skill
{
    public Skill36()
    {
        Name = "随机应变";
        Description = "产生等于已选骰子点数和的头脑风暴点数";
        StaminaCost = 30;
        DiceCost = 0;
        EffectMode = 1;
        ManageSkill = true;
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
        Description = "产生2点头脑风暴点数，连续产生5次";
        StaminaCost = 20;
        DiceCost = 8;
        EffectMode = 1;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        for(int i = 0; i < 5; i++)
        {
            TargetEmp.InfoA.GC.SC.CauseDamage(2);
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
        Description = "选择1个技能禁用2回合，产生7点头脑风暴点数";
        StaminaCost = 20;
        DiceCost = 6;
        EffectMode = 4;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.SC.TargetSkill.LockTime += 2;
        TargetEmp.InfoDetail.GC.SC.TargetSkill.gameObject.SetActive(false);
        TargetEmp.InfoDetail.GC.SC.SkillLockBonusCheck();
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
        Description = "选择1名员工回复20点心力";
        StaminaCost = 50;
        DiceCost = 5;
        EffectMode = 2;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp.Mentality += 20;
    }
}

//熟练按摩
public class Skill42 : Skill
{
    public Skill42()
    {
        Name = "熟练按摩";
        Description = "选择一名员工回复15点体力";
        StaminaCost = 20;
        DiceCost = 4;
        EffectMode = 2;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
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
        Description = "选择一名员工，信心+10";
        StaminaCost = 20;
        DiceCost = 3;
        EffectMode = 2;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
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
        TargetEmp.InfoDetail.GC.SC.TargetSkill.gameObject.SetActive(false);
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
            TargetEmp.InfoDetail.GC.SC.CurrentSkills[i].skill.StaminaExtra = TargetEmp.InfoDetail.GC.SC.CurrentSkills[i].skill.StaminaCost / 2;
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
        Description = "产生9点头脑风暴点数";
        StaminaCost = 20;
        DiceCost = 6;
        EffectMode = 1;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoA.GC.SC.CauseDamage(9);
    }
}

//私下承诺+
public class Skill61 : Skill
{
    public Skill61()
    {
        Name = "私下承诺+";
        Description = "选择一名员工，信心+20";
        StaminaCost = 20;
        DiceCost = 3;
        EffectMode = 2;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.emp.Confidence += 20;
    }
}

//熟练按摩+
public class Skill62 : Skill
{
    public Skill62()
    {
        Name = "熟练按摩+";
        Description = "选择一名员工回复30点体力";
        StaminaCost = 20;
        DiceCost = 4;
        EffectMode = 2;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp.Stamina += 30;
    }
}

//和解+
public class Skill63 : Skill
{
    public Skill63()
    {
        Name = "和解+";
        Description = "选择2名员工回复20点心力";
        StaminaCost = 50;
        DiceCost = 5;
        EffectMode = 5;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
        TargetEmp.InfoDetail.GC.CurrentEmpInfo.emp.Mentality += 20;
        TargetEmp.InfoDetail.GC.CurrentEmpInfo2.emp.Mentality += 20;
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
        Description = "产生13点头脑风暴点数";
        StaminaCost = 40;
        DiceCost = 5;
        EffectMode = 1;
        ManageSkill = true;
    }

    public override void StartEffect()
    {
        base.StartEffect();
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
        StaminaCost = 30;
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

public static class SkillData
{
    public static List<Skill> Skills = new List<Skill>()
    {
        new Skill1(), new Skill2(), new Skill3(), new Skill4(), new Skill5(), new Skill6(), new Skill7(),
        new Skill8(), new Skill9(), new Skill10(), new Skill11(), new Skill12(), new Skill13(), new Skill14(),
        new Skill5(), new Skill6(), new Skill7(), new Skill8(), new Skill9(), new Skill20(),
        new Skill21(), new Skill22(), new Skill23(), new Skill24(), new Skill25(), new Skill26(), new Skill27(),
        new Skill28(), new Skill29(), new Skill30(), new Skill31()
    };
}
