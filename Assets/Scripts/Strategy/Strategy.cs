using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StrategyType
{
    人力, 管理, 执行, 文化, 研发
}

public class Strategy
{
    //Type:1提升n人心力 2增加产品分 3生产产品原型图 4生产营销文案 5生产程序迭代 6发生n次文化事件 7完成n次研究或调研
    public int RequestType, RequestValue, CultureRequire = 0, FaithRequire = 0;// 文化 -独裁 +开源 ;信仰 -机械 +人文
    public string Name = "", EffectDescription = "", RequestDescription = "";
    public StrategyType Type;

    public List<int> RequestTasks = new List<int>();
    public List<int> RequestNum;

    public Strategy()
    {

    }

    public virtual void Effect(GameControl GC)
    {

    }

    public virtual void EffectRemove(GameControl GC)
    {

    }
}

//人才数据库
public class Strategy1_1 : Strategy
{
    public Strategy1_1() : base()
    {
        Type = StrategyType.人力;
        Name = "人才数据库";
        EffectDescription = "招聘成功率+30%";
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.HireSuccessExtra += 0.3f;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.HireSuccessExtra -= 0.3f;
    }
}

//成长型思维
public class Strategy1_2 : Strategy
{
    public Strategy1_2() : base()
    {
        Type = StrategyType.人力;
        Name = "成长型思维";
        EffectDescription = "高管部门、人力部门心力加成效果提高100%";
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.HRBuildingMentalityExtra += 1.0f;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.HRBuildingMentalityExtra -= 1.0f;
    }
}

//积极心理学
public class Strategy1_3 : Strategy
{
    int TempValue;
    public Strategy1_3() : base()
    {
        Type = StrategyType.人力;
        Name = "积极心理学";
        EffectDescription = "士气+20";

        //RequestTasks.Add(8);

        //RequestNum = new List<int> { 3 };
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        TempValue = 100 - GC.Morale;
        if (TempValue > 20)
            TempValue = 20;
        GC.Morale += TempValue;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.Morale -= TempValue;
    }
}

//首席人力官
public class Strategy1_4 : Strategy
{
    public Strategy1_4() : base()
    {
        Type = StrategyType.人力;
        Name = "首席人力官";
        EffectDescription = "招募时可招募2人";
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.HC.MaxHireNum += 1;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.HC.MaxHireNum -= 1;
    }
}

//结构优化
public class Strategy1_5 : Strategy
{
    public Strategy1_5() : base()
    {
        Type = StrategyType.人力;
        Name = "结构优化";
        EffectDescription = "所有员工工资减少50%";
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.TotalSalaryMultiply -= 0.5f;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.TotalSalaryMultiply += 0.5f;
    }
}

//晨会制度
public class Strategy2_1 : Strategy
{
    public Strategy2_1() : base()
    {
        Type = StrategyType.管理;
        Name = "晨会制度";
        EffectDescription = "每次动员时使用的第一个基础技能效果翻倍";
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.SC.AdvanceMobilize = true;
        //GC.MobilizeExtraMent += 10;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.SC.AdvanceMobilize = false;
        //GC.MobilizeExtraMent -= 10;
    }
}

//稳健中层
public class Strategy2_2 : Strategy
{
    public Strategy2_2() : base()
    {
        Type = StrategyType.管理;
        Name = "稳健中层";
        EffectDescription = "高管管理能力+2点";
        RequestDescription = "产品分累计增加30分";
        RequestType = 2;
        RequestValue = 30;
        //RequestDescription = "2个平庸及以上可行性调研 \n 2个平庸及以上用户访谈 ";

        //RequestTasks.Add(2);

        //RequestTasks.Add(8);

        //RequestNum = new List<int> { 2, 2 };
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.ExtraDice += 1;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.ExtraDice -= 1;
    }
}

//特别小组
public class Strategy2_3 : Strategy
{
    DepControl dep1, dep2;
    public Strategy2_3() : base()
    {
        Type = StrategyType.管理;
        Name = "特别小组";
        EffectDescription = "随机选择2个非业务部门，增加其成功率20%";
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        List<DepControl> deps = new List<DepControl>();
        foreach(DepControl dep in GC.CurrentDeps)
        {
            if (dep.building.Type != BuildingType.技术部门 && dep.building.Type != BuildingType.市场部门
                && dep.building.Type != BuildingType.产品部门 && dep.building.Type != BuildingType.公关营销部)
                deps.Add(dep);
        }
        if (deps.Count > 0)
        {
            dep1 = deps[Random.Range(0, deps.Count)];
            deps.Remove(dep1);
            dep1.Efficiency += 0.2f;
            if (deps.Count > 0)
            {
                dep2 = deps[Random.Range(0, deps.Count)];
                dep2.Efficiency += 0.2f;
            }
        }
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        if (dep1 != null)
            dep1.Efficiency -= 0.2f;
        if (dep2 != null)
            dep2.Efficiency -= 0.2f;
    }
}

//扁平管理
public class Strategy2_4 : Strategy
{
    public Strategy2_4() : base()
    {
        Type = StrategyType.管理;
        Name = "扁平管理";
        EffectDescription = " 所有基础业务部门技能成功率+15%";
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.BaseDepExtraSuccessRate += 0.15f;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.BaseDepExtraSuccessRate -= 0.15f;
    }
}

//财务审核
public class Strategy2_5 : Strategy
{
    public Strategy2_5() : base()
    {
        Type = StrategyType.管理;
        Name = "财务审核";
        EffectDescription = "所有建筑维护费减少50%";
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.TotalBuildingPayMultiply /= 2;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.TotalBuildingPayMultiply *= 2;
    }
}

//自发机制
public class Strategy2_6 : Strategy
{
    public Strategy2_6() : base()
    {
        Type = StrategyType.管理;
        Name = "自发机制";
        EffectDescription = "“精进”和“团结”状态的持续时间提高至4m";
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.ProduceBuffBonus = true;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.ProduceBuffBonus = false;
    }
}

//可持续发展
public class Strategy2_7 : Strategy
{
    public Strategy2_7() : base()
    {
        Type = StrategyType.管理;
        Name = "可持续发展";
        EffectDescription = "健身房”大成功时获得“强化”或“铁人”状态的概率上升为80%";
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.GymBuffBonus = true;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.GymBuffBonus= false;
    }
}

//996加班制
public class Strategy3_1 : Strategy
{
    int TempValue = 0;
    public Strategy3_1() : base()
    {
        Type = StrategyType.执行;
        Name = "996加班制";
        EffectDescription = "可以选择加班,加班状态下一天会工作12小时,但是当天的工作的每小时会扣除1体力,士气-30";
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.WorkOvertimeToggle.gameObject.SetActive(true);
        TempValue = 100 - GC.Morale;
        if (TempValue > 30)
            TempValue = 30;
        GC.Morale -= TempValue;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.WorkOvertimeToggle.isOn = false;
        GC.WorkOvertimeToggle.gameObject.SetActive(false);   
        GC.Morale += TempValue;
    }
}

//舆论战
public class Strategy3_2 : Strategy
{
    public Strategy3_2() : base()
    {
        Type = StrategyType.执行;
        Name = "舆论战";
        EffectDescription = "占领时的骰子修正+1";
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.NeutralOccupieBonus = true;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.NeutralOccupieBonus = false;
    }
}

//内线爆料
public class Strategy3_3 : Strategy
{
    public Strategy3_3() : base()
    {
        Type = StrategyType.执行;
        Name = "内线爆料";
        EffectDescription = "占领时的骰子修正+2";
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.TargetOccupieBonus = true;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.TargetOccupieBonus = false;
    }
}

//KPI制度
public class Strategy3_4 : Strategy
{
    public Strategy3_4() : base()
    {
        Type = StrategyType.执行;
        Name = "KPI制度";
        EffectDescription = "所有部门重大失误率-30%";
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.SC.ExtraMajorFailureRate -= 0.3f;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.SC.ExtraMajorFailureRate += 0.3f;
    }
}

//团队核心
public class Strategy3_5 : Strategy
{
    public Strategy3_5() : base()
    {
        Type = StrategyType.执行;
        Name = "团队核心";
        EffectDescription = "CEO一票算作两票";
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.CEOExtraVote = true;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.CEOExtraVote = false;
    }
}

//车库文化
public class Strategy4_1 : Strategy
{
    public Strategy4_1() : base()
    {
        Type = StrategyType.文化;
        Name = "车库文化";
        EffectDescription = "人力栏位+1";
        RequestDescription = "发生5次文化事件";
        RequestType = 6;
        RequestValue = 5;
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        //旧插槽版策略树检查
        GC.StrC.StrLimitNum[0] += 1;
        GC.StrC.UpdateUI();
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.StrC.StrLimitNum[0] -= 1;
        GC.StrC.UpdateUI();
    }
}

//狼性文化
public class Strategy4_2 : Strategy
{
    public Strategy4_2() : base()
    {
        Type = StrategyType.文化;
        Name = "狼性文化";
        EffectDescription = "执行栏位+2 士气-10";
        RequestDescription = "发生10次文化事件";
        RequestType = 6;
        RequestValue = 10;
        //RequestDescription = "2个平庸及以上可行性调研 \n 2个平庸及以上公关谈判";

        //RequestTasks.Add(2);

        //RequestTasks.Add(3);

        //RequestNum = new List<int> { 2, 2 };
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.StrC.StrLimitNum[2] += 2;
        GC.Morale -= 10;
        //GC.StrC.StrLimitNum[0] += 3;
        //GC.StrC.StrLimitNum[1] += 1;
        GC.StrC.UpdateUI();
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.StrC.StrLimitNum[2] -= 2;
        GC.Morale += 10;
        //GC.StrC.StrLimitNum[0] -= 3;
        //GC.StrC.StrLimitNum[1] -= 1;
        //GC.StrC.CheckStrNum();
        GC.StrC.UpdateUI();
    }
}

//多元文化
public class Strategy4_3 : Strategy
{
    public Strategy4_3() : base()
    {
        Type = StrategyType.文化;
        Name = "多元文化";
        EffectDescription = "人力栏位+3 \n 管理栏位+1";
        RequestDescription = "发生10次文化事件";
        RequestType = 6;
        RequestValue = 10;
        //EffectDescription = "解锁新型文化相关办公室";
        //RequestDescription = "1个平庸及以上可行性调研 \n 2个平庸及以上公关谈判";

        //RequestTasks.Add(2);

        //RequestTasks.Add(3);

        //RequestNum = new List<int> { 1, 2 };
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.StrC.StrLimitNum[0] += 3;
        GC.StrC.StrLimitNum[1] += 1;
        GC.StrC.UpdateUI();
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.StrC.StrLimitNum[0] -= 3;
        GC.StrC.StrLimitNum[1] -= 1;
        GC.StrC.UpdateUI();
    }
}

//技术主义
public class Strategy4_4 : Strategy
{
    public Strategy4_4() : base()
    {
        Type = StrategyType.文化;

        Name = "技术主义";
        EffectDescription = "执行栏位+3";
        RequestDescription = "发生10次文化事件";
        RequestType = 6;
        RequestValue = 10;
        //Name = "性别友好!!";
        //EffectDescription = "开源文化偏好者发生事件正向修正+1";
        //RequestDescription = "1个平庸及以上可行性调研 \n 2个平庸及以上公关谈判";

        //RequestTasks.Add(2);

        //RequestTasks.Add(3);

        //RequestNum = new List<int> { 1, 2 };
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.StrC.StrLimitNum[4] += 3;
        GC.StrC.UpdateUI();
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.StrC.StrLimitNum[4] -= 3;
        GC.StrC.UpdateUI();
    }
}

//官僚主义
public class Strategy4_5 : Strategy
{
    public Strategy4_5() : base()
    {
        Type = StrategyType.文化;

        Name = "官僚主义";
        EffectDescription = "管理栏位+1 \n 执行栏位+1";
        RequestDescription = "发生10次文化事件";
        RequestType = 6;
        RequestValue = 10;       
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.StrC.StrLimitNum[1] += 1;
        GC.StrC.StrLimitNum[2] += 1;
        GC.StrC.UpdateUI();
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.StrC.StrLimitNum[1] -= 1;
        GC.StrC.StrLimitNum[2] -= 1;
        GC.StrC.UpdateUI();
    }
}

//科学传统
public class Strategy4_6 : Strategy
{
    public Strategy4_6() : base()
    {
        Type = StrategyType.文化;

        Name = "科学传统";
        EffectDescription = "研发栏位+1";
        RequestDescription = "发生10次文化事件";
        RequestType = 6;
        RequestValue = 10;
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.StrC.StrLimitNum[4] += 1;
        GC.StrC.UpdateUI();
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.StrC.StrLimitNum[4] -= 1;
        GC.StrC.UpdateUI();
    }
}

//分权
public class Strategy4_7 : Strategy
{
    public Strategy4_7() : base()
    {
        Type = StrategyType.文化;

        Name = "分权";
        EffectDescription = "管理栏位+1";
        RequestDescription = "发生10次文化事件";
        RequestType = 6;
        RequestValue = 10;
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.StrC.StrLimitNum[1] += 1;
        GC.StrC.UpdateUI();
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.StrC.StrLimitNum[1] -= 1;
        GC.StrC.UpdateUI();
    }
}

//分布式协作
public class Strategy4_8 : Strategy
{
    public Strategy4_8() : base()
    {
        Type = StrategyType.文化;

        Name = "分布式协作";
        EffectDescription = "管理栏位+3";
        RequestDescription = "发生10次文化事件";
        RequestType = 6;
        RequestValue = 10;
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.StrC.StrLimitNum[1] += 3;
        GC.StrC.UpdateUI();
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.StrC.StrLimitNum[1] -= 3;
        GC.StrC.UpdateUI();
    }
}

//搜集文献
public class Strategy5_1 : Strategy
{
    public Strategy5_1() : base()
    {
        Type = StrategyType.研发;
        Name = "搜集文献";
        EffectDescription = "研发成功率+10%";
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        //GC.ResearchExtraSuccessRate += 0.1f;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        //GC.ResearchExtraSuccessRate -= 0.1f;
    }
}

//像素级拷贝
public class Strategy5_2 : Strategy
{
    public Strategy5_2() : base()
    {
        Type = StrategyType.研发;
        Name = "像素级拷贝";
        EffectDescription = "研发部门生产周期-4";
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        //GC.ResearchExtraTimeBoost += 4;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        //GC.ResearchExtraTimeBoost -= 4;
    }
}

//前沿观察
public class Strategy5_3 : Strategy
{
    public Strategy5_3() : base()
    {
        Type = StrategyType.研发;
        Name = "前沿观察";
        EffectDescription = "研发成功率+20%";
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        //GC.ResearchExtraSuccessRate += 0.2f;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        //GC.ResearchExtraSuccessRate -= 0.2f;
    }
}

//学术国际化
public class Strategy5_4 : Strategy
{
    public Strategy5_4() : base()
    {
        Type = StrategyType.研发;
        Name = "学术国际化";
        EffectDescription = "研发成功率+15%,研发部门生产周期-2";
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        //GC.ResearchExtraSuccessRate += 0.15f;
        //GC.ResearchExtraTimeBoost += 2;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        //GC.ResearchExtraSuccessRate -= 0.15f;
        //  GC.ResearchExtraTimeBoost -= 2;
    }
}

//快乐探索者
public class Strategy5_5 : Strategy
{
    public Strategy5_5() : base()
    {
        Type = StrategyType.研发;
        Name = "快乐探索者";
        EffectDescription = "科研人员心力每月+20";
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.ResearchExtraMentality = true;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.ResearchExtraMentality = false;
    }
}

public static class StrategyData
{
    public static List<Strategy> Strategies = new List<Strategy>()
    {
        new Strategy1_1(),
        new Strategy1_2(),
        new Strategy1_3(),
        new Strategy1_4(),
        new Strategy1_5(),
        new Strategy2_1(),
        new Strategy2_2(),
        new Strategy2_3(),
        new Strategy2_4(),
        new Strategy2_5(),
        new Strategy2_6(),
        new Strategy2_7(),
        new Strategy3_1(),
        new Strategy3_2(),
        new Strategy3_3(),
        new Strategy3_4(),
        new Strategy3_5(),
        new Strategy5_1(),
        new Strategy5_2(),
        new Strategy5_3(),
        new Strategy5_4(),
        new Strategy5_5()
    };
}
