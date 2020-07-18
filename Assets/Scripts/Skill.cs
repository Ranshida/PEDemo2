using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill
{
    public int StaminaCost, DiceCost, StaminaExtra, DiceExtra, EffectMode = 1;
    public string Name, Description;

    public Employee TargetEmp;

    public Skill(Employee Emp)
    {
        TargetEmp = Emp;
    }

    public virtual bool ConditionCheck()
    {
        return true;
    }

    public virtual void StartEffect()
    {
        TargetEmp.Stamina -= (StaminaCost - StaminaExtra);
    }
}

//潜能分析
public class Skill1 : Skill
{
    public Skill1(Employee Emp) : base(Emp)
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
    public Skill2(Employee Emp) : base(Emp)
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
    public Skill3(Employee Emp) : base(Emp)
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
    public Skill4(Employee Emp) : base(Emp)
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
    public Skill5(Employee Emp) : base(Emp)
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
    public Skill6(Employee Emp) : base(Emp)
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
    public Skill7(Employee Emp) : base(Emp)
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
    public Skill8(Employee Emp) : base(Emp)
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
    public Skill9(Employee Emp) : base(Emp)
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
    public Skill10(Employee Emp) : base(Emp)
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
    public Skill11(Employee Emp) : base(Emp)
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
        TargetEmp.CurrentDep.Efficiency += 0.2f;
        for(int i = 0; i < TargetEmp.CurrentDep.CurrentEmps.Count; i++)
        {
            TargetEmp.CurrentDep.CurrentEmps[i].Mentality -= 40;
        }
    }
}

//应急班次
public class Skill12 : Skill
{
    public Skill12(Employee Emp) : base(Emp)
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
        TargetEmp.CurrentDep.Efficiency += 0.2f;
        for (int i = 0; i < TargetEmp.CurrentDep.CurrentEmps.Count; i++)
        {
            TargetEmp.CurrentDep.CurrentEmps[i].InfoDetail.AddPerk(new Perk4(TargetEmp.CurrentDep.CurrentEmps[i]), true);
        }
    }
}

//大局观
public class Skill13 : Skill
{
    public Skill13(Employee Emp) : base(Emp)
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
        TargetEmp.CurrentDep.Efficiency += 0.2f;
        TargetEmp.Mentality -= 80;
    }
}

//平常之心
public class Skill14 : Skill
{
    public Skill14(Employee Emp) : base(Emp)
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
        TargetEmp.CurrentDep.Efficiency += 0.5f;
    }
}

//打破常规
public class Skill15 : Skill
{
    public Skill15(Employee Emp) : base(Emp)
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
        TargetEmp.CurrentDep.Efficiency += 0.2f;
        for(int i = 0; i < TargetEmp.CurrentDep.CurrentEmps.Count; i++)
        {
            TargetEmp.CurrentDep.CurrentEmps[i].Stamina -= Random.Range(0, 31);
        }
    }
}

//高效
public class Skill16 : Skill
{
    public Skill16(Employee Emp) : base(Emp)
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
    public Skill17(Employee Emp) : base(Emp)
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
    public Skill18(Employee Emp) : base(Emp)
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
    public Skill19(Employee Emp) : base(Emp)
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
    public Skill20(Employee Emp) : base(Emp)
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