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
    public string Name, EffectDescription, RequestDescription;
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
        //EffectDescription = "招募高级人才的概率+10% \n \n \n威信+5";
        //RequestDescription = "2个低劣及以上用户访谈";
        EffectDescription = "普通招聘消耗-30%";
        RequestDescription = "提升10人心力";
        RequestType = 1;
        RequestValue = 10;

        //RequestTasks.Add(8);
        //RequestNum = new List<int> { 2 };
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.PC2.HireCostRate = 0.7f;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.PC2.HireCostRate = 1.0f;
    }
}

//成长型思维
public class Strategy1_2 : Strategy
{
    public Strategy1_2() : base()
    {
        Type = StrategyType.人力;
        Name = "成长型思维";
        EffectDescription = "HR部门心力加成效果提高100%";
        RequestDescription = "提升10人心力";
        RequestType = 1;
        RequestValue = 10;
        //RequestDescription = "3个平庸及以上用户访谈";

        //RequestTasks.Add(8);

        //RequestNum = new List<int> { 3 };
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.HRBuildingMentalityExtra += 1.0f;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.HRBuildingMentalityExtra += 1.0f;
    }
}

//积极心理学
public class Strategy1_3 : Strategy
{
    public Strategy1_3() : base()
    {
        Type = StrategyType.人力;
        Name = "积极心理学";
        EffectDescription = "士气+20";
        RequestDescription = "提升10人心力";
        RequestType = 1;
        RequestValue = 10;

        //RequestTasks.Add(8);

        //RequestNum = new List<int> { 3 };
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.Morale += 20;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.Morale -= 20;
    }
}

//首席人力官
public class Strategy1_4 : Strategy
{
    public Strategy1_4() : base()
    {
        Type = StrategyType.人力;
        Name = "首席人力官";
        EffectDescription = "降低猎头生产力需求30%";
        RequestDescription = "提升10人心力";
        RequestType = 1;
        RequestValue = 10;

        //RequestTasks.Add(8);

        //RequestNum = new List<int> { 3 };
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.PC2.HeadHuntCostRate = 0.7f;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.PC2.HeadHuntCostRate = 1.0f;
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
        RequestDescription = "产品分累计增加30分";
        RequestType = 2;
        RequestValue = 30;
        //EffectDescription = "每次动员时心力损耗减少10点 \n 威信+5";
        //RequestDescription = "1个低劣及以上可行性调研 \n 1个低劣以上用户访谈";

        //RequestTasks.Add(2);

        //RequestTasks.Add(8);

        //RequestNum = new List<int> { 1, 1 };
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
        GC.ManageExtra += 2;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.ManageExtra -= 2;
    }
}

//组织研究!!
public class Strategy2_3 : Strategy
{
    public Strategy2_3() : base()
    {
        Type = StrategyType.管理;
        Name = "组织研究!!";
        EffectDescription = "解锁新型办公室(这个战略可没法做时长限制)";
        RequestDescription = "2个平庸及以上可行性调研 \n 1个平庸及以上用户访谈 ";

        RequestTasks.Add(2);

        RequestTasks.Add(8);

        RequestNum = new List<int> { 2, 1 };
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        MonoBehaviour.print("2_3Use");
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        MonoBehaviour.print("2_3Remove");
    }
}

//扁平管理
public class Strategy2_4 : Strategy
{
    public Strategy2_4() : base()
    {
        Type = StrategyType.管理;
        Name = "扁平管理";
        EffectDescription = "一般部门技能成功率+20%";
        RequestDescription = "产品分累计增加30分";
        RequestType = 2;
        RequestValue = 30;
        //RequestDescription = "2个平庸及以上用户访谈 \n 2个平庸及以上公关谈判 ";

        //RequestTasks.Add(8);

        //RequestTasks.Add(3);


        //RequestNum = new List<int> { 2, 2 };
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.BuildingSkillSuccessExtra += 0.2f;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.BuildingSkillSuccessExtra -= 0.2f;
    }
}

//扁平管理
public class Strategy2_5 : Strategy
{
    public Strategy2_5() : base()
    {
        Type = StrategyType.管理;
        Name = "财务审核";
        EffectDescription = "所有建筑维护费减少50%";
        RequestDescription = "产品分累计增加30分";
        RequestType = 2;
        RequestValue = 30;
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.BuildingMaintenanceCostRate /= 2;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.BuildingMaintenanceCostRate *= 2;
    }
}

//996加班制
public class Strategy3_1 : Strategy
{
    public Strategy3_1() : base()
    {
        Type = StrategyType.执行;
        Name = "996加班制";
        EffectDescription = "所有业务部门效率+20% \n 士气-15";
        RequestDescription = "2个低劣及以上公关谈判";

        RequestTasks.Add(3);

        RequestNum = new List<int> { 2 };
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.EfficiencyExtraNormal += 0.2f;
        GC.Morale -= 15;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.EfficiencyExtraNormal -= 0.2f;
        GC.Morale += 15;
    }
}

//舆论战
public class Strategy3_2 : Strategy
{
    public Strategy3_2() : base()
    {
        Type = StrategyType.执行;
        Name = "舆论战";
        EffectDescription = "公关谈判效果增加50%";
        RequestDescription = "生产15次营销文案";
        RequestType = 4;
        RequestValue = 15;
        //RequestDescription = "3个平庸及以上公关谈判";

        //RequestTasks.Add(3);

        //RequestNum = new List<int> { 3 };
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        //GC.foeControl.NegotiateEfficiency += 0.5f;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        //GC.foeControl.NegotiateEfficiency -= 0.5f;
    }
}

//内线爆料
public class Strategy3_3 : Strategy
{
    public Strategy3_3() : base()
    {
        Type = StrategyType.执行;
        Name = "内线爆料";
        EffectDescription = "公关效果+20%";
        RequestDescription = "生产15次营销文案";
        RequestType = 4;
        RequestValue = 15;
        //RequestDescription = "3个平庸及以上公关谈判";

        //RequestTasks.Add(3);

        //RequestNum = new List<int> { 3 };
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        //GC.foeControl.FOEEventProtection += 2;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        MonoBehaviour.print("3_3Remove");
    }
}

//KPI制度
public class Strategy3_4 : Strategy
{
    public Strategy3_4() : base()
    {
        Type = StrategyType.执行;
        Name = "KPI制度";
        EffectDescription = "部门失误率-20%";
        RequestDescription = "生产15次程序迭代";
        RequestType = 5;
        RequestValue = 15;
        //RequestDescription = "1个平庸及以上公关谈判 \n 1个平庸及以上可行性调研 \n 1个平庸及以上用户访谈";

        //RequestTasks.Add(3);

        //RequestTasks.Add(2);

        //RequestTasks.Add(8);

        //RequestNum = new List<int> { 1, 1, 1 };
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.ExtrafailRate += 0.2f;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.ExtrafailRate -= 0.2f;
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
        RequestDescription = "完成3次研究或调研";
        RequestType = 7;
        RequestValue = 3;

        //RequestDescription = "2个低劣及以上可行性调研";

        //RequestTasks.Add(2);

        //RequestNum = new List<int> { 2 };
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.ResearchSuccessRateExtra += 0.1f;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.ResearchSuccessRateExtra -= 0.1f;
    }
}

//像素级拷贝
public class Strategy5_2 : Strategy
{
    public Strategy5_2() : base()
    {
        Type = StrategyType.研发;
        Name = "像素级拷贝";
        EffectDescription = "研发部门效率+15%";
        RequestDescription = "完成5次研究或调研";
        RequestType = 7;
        RequestValue = 5;
        //RequestDescription = "3个平庸及以上可行性调研";

        //RequestTasks.Add(2);

        //RequestNum = new List<int> { 3 };
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.EfficiencyExtraScience += 0.15f;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.EfficiencyExtraScience -= 0.15f;
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
        RequestDescription = "完成5次研究或调研";
        RequestType = 7;
        RequestValue = 5;
        //RequestDescription = "3个平庸及以上可行性调研";

        //RequestTasks.Add(2);

        //RequestNum = new List<int> { 3 };
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.ResearchSuccessRateExtra += 0.2f;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.ResearchSuccessRateExtra -= 0.2f;
    }
}

//学术国际化
public class Strategy5_4 : Strategy
{
    public Strategy5_4() : base()
    {
        Type = StrategyType.研发;
        Name = "学术国际化";
        EffectDescription = "研发部门效率+30%";
        RequestDescription = "完成7次研究或调研";
        RequestType = 7;
        RequestValue = 7;
        //RequestDescription = "4个平庸及以上可行性调研";

        //RequestTasks.Add(2);

        //RequestNum = new List<int> { 4 };
    }

    public override void Effect(GameControl GC)
    {
        base.Effect(GC);
        GC.EfficiencyExtraScience += 0.3f;
    }

    public override void EffectRemove(GameControl GC)
    {
        base.EffectRemove(GC);
        GC.EfficiencyExtraScience -= 0.3f;
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
        new Strategy2_1(),
        new Strategy2_2(),
        //new Strategy2_3(),
        new Strategy2_4(),
        new Strategy2_5(),
        //new Strategy3_1(),
        new Strategy3_2(),
        //new Strategy3_3(),
        new Strategy3_4(),
        new Strategy5_1(),
        new Strategy5_2(),
        new Strategy5_3(),
        new Strategy5_4(),
    };
}
