using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventGroup : Event
{
    public int ExtraStage = 0;//事件组生成后待机(不产生效果)的回合数
    public int StageCount = 6;//事件组总阶段数
    public int BSBossLevel = 2;//头脑风暴boss等级
    public int BSTurnCorrection = 2;
    public int ResourceTurnCorretion = 1;
    public int MoneyRequest = 0;//资源消耗选项-金钱消耗
    public bool DebuffEvent = true;//是否为负面事件组

    public List<int> ItemTypeRequest = new List<int>();//物品类型需求
    public List<int> ItemValueRequest = new List<int>();//物品数量需求

    //特别小组修正相关
    public float ST_BaseRate = 0.3f;//基础成功率
    public float ST_EmpRate = 0.1f;//每增加一名员工提供的额外成功率

    public float ST_SkillRate = 0.1f;//技能需求额外成功率
    public int ST_SkillCount = 3;//技能需求阈值
    public int ST_SkillType = 1;//1决策 2管理 3坚韧
    public int ST_TurnCorrection = 1;//特别小组成功后跳过的回合数

    public float ST_ProfessionRate = 0.2f;//岗位优势额外成功率
    public int ST_ProfessionCount = 3;//岗位优势需求人数
    public ProfessionType ST_ProfessionType = ProfessionType.工程学;//岗位优势需求类型


    public EventGroup() : base()
    {
        isEventGroup = true;
    }

    public void UseResource(EventGroupInfo egi)
    {
        //先检测金钱
        if (MoneyRequest > 0)
        {
            if (egi.EC.GC.Money >= MoneyRequest)
                egi.EC.GC.Money -= MoneyRequest;
            else
                return;
        }
        //再检测是否有足够物品
        if (ItemTypeRequest.Count > 0)
        {
            List<CompanyItem> Items = new List<CompanyItem>();
            for(int i = 0; i < ItemTypeRequest.Count; i++)
            {
                int count = 0;
                foreach(CompanyItem CItem in egi.EC.GC.Items)
                {
                    if (CItem.item.Num == ItemTypeRequest[i])
                    {
                        count += 1;
                        Items.Add(CItem);
                    }
                }
                if (count < ItemValueRequest[i])
                    return;
            }
            foreach(CompanyItem i in Items)
            {
                i.DeleteItem();
            }
        }
        //都满足则生效
        egi.ResolveStage(ResourceTurnCorretion);
        egi.UseResourceButton.interactable = false;
    }

    public override void StartEvent(Employee emp, int ExtraCorrection = 0, Employee target = null, EventGroupInfo egi = null)
    {
        //如果判定成功就不继续
        if (FindResult(emp, ExtraCorrection, target) == 1)
        {
            egi.StageMarker[egi.Stage - 1].color = Color.green;            
            if (DebuffEvent == true)
            {
                QuestControl.Instance.Init("判定成功，未产生负面效果");
                return;
            }
            else
                QuestControl.Instance.Init("判定成功," + ResultDescription(emp, target, egi.RandomEventNum));
        }
        else
        {
            egi.StageMarker[egi.Stage - 1].color = Color.red;
            if (DebuffEvent == false)
            {
                QuestControl.Instance.Init("判定失败，未产生效果");
                return;
            }
            else
                QuestControl.Instance.Init("判定失败," + ResultDescription(emp, target, egi.RandomEventNum));
        }

        if (egi.RandomEventNum == 1)
            EffectA(emp, egi, target);
        else if (egi.RandomEventNum == 2)
            EffectB(emp, egi, target);
        else if (egi.RandomEventNum == 3)
            EffectC(emp, egi, target);
        else if (egi.RandomEventNum == 4)
            EffectD(emp, egi, target);
        else if (egi.RandomEventNum == 5)
            EffectE(emp, egi, target);
        else if (egi.RandomEventNum == 6)
            EffectF(emp, egi, target);
    }

    protected virtual void EffectA(Employee emp, EventGroupInfo egi = null, Employee target = null )
    {

    }
    protected virtual void EffectB(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {

    }
    protected virtual void EffectC(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {

    }
    protected virtual void EffectD(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {

    }
    protected virtual void EffectE(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {

    }
    protected virtual void EffectF(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {

    }
}

public class EventGroup1 : EventGroup
{
    public EventGroup1() : base()
    {
        EventName = "冷酷管理";
        StageCount = 6;
        ExtraStage = 2;
        SubEventNames[0] = "晕船风波";
        SubEventNames[1] = "异常电波";
        SubEventNames[2] = "虐待传闻";
        SubEventNames[3] = "要求解释";
        SubEventNames[4] = "仙人掌中毒";
        SubEventNames[5] = "蹭网风波";
        DebuffEvent = true;

        ST_BaseRate = 0.3f;//基础成功率
        ST_EmpRate = 0.05f;//每增加一名员工提供的额外成功率

        ST_SkillRate = 0.1f;//技能需求额外成功率
        ST_SkillCount = 5;//技能需求阈值
        ST_SkillType = 1;//1决策 2管理 3坚韧

        ST_ProfessionRate = 0.2f;//岗位优势额外成功率
        ST_ProfessionCount = 3;//岗位优势需求人数
        ST_ProfessionType = ProfessionType.工程学;//岗位优势需求类型

        MoneyRequest = 50;
        ItemTypeRequest = new List<int>() { };
        ItemValueRequest = new List<int>() { };
    }

    protected override void EffectA(Employee emp, EventGroupInfo egi = null, Employee target = null )
    {
        DivisionControl div = egi.TargetDivision;

        if (div != null)
        {
            foreach(DepControl dep in div.CurrentDeps)
            {
                foreach (Employee e in dep.CurrentEmps)
                {
                    e.AddEmotion(EColor.Red);
                }
            }
            if (div.Manager != null)
                div.Manager.AddEmotion(EColor.Red);
        }
    }
    protected override void EffectB(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        DivisionControl div = egi.TargetDivision;

        if (div != null)
        {
            foreach (DepControl dep in div.CurrentDeps)
            {
                foreach (Employee e in dep.CurrentEmps)
                {
                    e.AddEmotion(EColor.Red);
                }
            }
            if (div.Manager != null)
                div.Manager.AddEmotion(EColor.Red);
        }
    }
    protected override void EffectC(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        emp.InfoDetail.AddPerk(new Perk55());
    }
    protected override void EffectD(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        egi.TargetDivision.AddPerk(new Perk56());
    }
    protected override void EffectE(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        egi.TargetDivision.AddPerk(new Perk57());
    }
    protected override void EffectF(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        egi.TargetDivision.AddPerk(new Perk58());
    }

    public override string EventDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = SelfName + "在公司论坛中发帖称由于CEO不顾员工感受长期航行导致自己晕船严重影响正常生活";
        else if (index == 2)
            content = SelfName + "在公司论坛中跟帖称自己总是在工作中收到异常的信号，电脑上总是出现奇怪的英文字符，尤其是在自己睡着之后最为明显（实际上是上班睡觉压键盘上了）";
        else if (index == 3)
            content = SelfName + "在社交媒体上发布信息宣称自己在公司中受到了暴力虐待和排挤，引起各界广泛关注";
        else if (index == 4)
            content = SelfName + "在办公室中宣称CEO已经被机器替换了大脑，根据CEO的一贯表现，员工已经产生了怀疑";
        else if (index == 5)
            content = SelfName + "带领同事将午饭吃的仙人掌泥涂在了办公室的墙面上，成为一种暗示CEO脱离群众不吃仙人掌的行为艺术";
        else if (index == 6)
            content = "以" + SelfName + "为代表的同事们在走廊中抗议重复无意义的工作将会成为人工智能统治世界的嫁衣";

        return content;
    }

    public override string ResultDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = DivName + "所有员工主导情绪变为“愤怒”";
        else if (index == 2)
            content = DivName + "所有员工主导情绪变为“愤怒”";
        else if (index == 3)
            content = SelfName + "获得“虐待传闻”状态，所有个人事件修正-5，持续6个回合";
        else if (index == 4)
            content = DivName + "获得状态“机械傀儡”×1，每个“机械傀儡”导致信念下降30点，持续6回合";
        else if (index == 5)
            content = DivName + "获得状态“发奋涂墙”×1，每个“发奋涂墙”导致信念下降30点，持续6回合";
        else if (index == 6)
            content = DivName + "获得状态“警惕人工智能”×1，每个“警惕人工智能”导致信念下降30点，持续6回合";

        return content;
    }
}

public class EventGroup2 : EventGroup
{
    public EventGroup2() : base()
    {
        EventName = "KPI压力";
        StageCount = 4;
        ExtraStage = 1;
        SubEventNames[0] = "拖延工期";
        SubEventNames[1] = "占领公厕";
        SubEventNames[2] = "上班捕鱼";
        SubEventNames[3] = "信号不良";
        DebuffEvent = true;

        ST_BaseRate = 0.3f;//基础成功率
        ST_EmpRate = 0.1f;//每增加一名员工提供的额外成功率

        ST_SkillRate = 0.1f;//技能需求额外成功率
        ST_SkillCount = 3;//技能需求阈值
        ST_SkillType = 3;//1决策 2管理 3坚韧

        ST_ProfessionRate = 0.2f;//岗位优势额外成功率
        ST_ProfessionCount = 2;//岗位优势需求人数
        ST_ProfessionType = ProfessionType.战略分析;//岗位优势需求类型

        MoneyRequest = 50;
        ItemTypeRequest = new List<int>() { };
        ItemValueRequest = new List<int>() { };
    }

    protected override void EffectA(Employee emp, EventGroupInfo egi = null, Employee target = null )
    {
        egi.TargetDivision.AddPerk(new Perk59());
    }
    protected override void EffectB(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        egi.TargetDivision.AddPerk(new Perk60());
    }
    protected override void EffectC(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        egi.TargetDivision.AddPerk(new Perk61());
    }
    protected override void EffectD(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        egi.TargetDivision.AddPerk(new Perk62());
    }

    public override string EventDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = SelfName + "故意拖延工期，拒绝交付已经完成的工作，但他的同事们都很乐意把自己的工作稍微推一推";
        else if (index == 2)
            content = "在监控摄像中我们可以看到" + SelfName + "正在带领一批同事长期霸占公司的公共厕所带薪如厕，同时剥夺其他人的使用权";
        else if (index == 3)
            content = SelfName + "上班时间带领同事前往甲板捕鱼以示抗议，如果放任事态发展接下来食堂将全面断鱼";
        else if (index == 4)
            content = SelfName + "表示自己最近的工作状态不佳全都是公司的网络信号不良导致的，没有达到KPI应该是公司的问题";

        return content;
    }

    public override string ResultDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = DivName + "获得“拖延工期”状态×1，每个“拖延工期”使效率下降3点，持续6个月";
        else if (index == 2)
            content = DivName + "获得“占领公厕”状态×1，每个“占领公厕”使工作状态下降3点，持续6回合";
        else if (index == 3)
            content = DivName + "获得“上班捕鱼”状态×1，每个“摸鱼”使工作状态下降3点，持续6回合";
        else if (index == 4)
            content = DivName + "获得“信号不良”状态×1，每个“信号不良”使效率下降3点，持续6个月";

        return content;
    }
}
public class EventGroup3 : EventGroup
{
    public EventGroup3() : base()
    {
        EventName = "待遇低下";
        StageCount = 6;
        ExtraStage = 2;
        SubEventNames[0] = "食堂降价";
        SubEventNames[1] = "要求涨工资";
        SubEventNames[2] = "敲桌子";
        SubEventNames[3] = "罢工";
        SubEventNames[4] = "离职威胁";
        SubEventNames[5] = "拒绝命令";
        DebuffEvent = true;

        ST_BaseRate = 0.3f;//基础成功率
        ST_EmpRate = 0.05f;//每增加一名员工提供的额外成功率

        ST_SkillRate = 0.2f;//技能需求额外成功率
        ST_SkillCount = 5;//技能需求阈值
        ST_SkillType = 2;//1决策 2管理 3坚韧

        ST_ProfessionRate = 0.2f;//岗位优势额外成功率
        ST_ProfessionCount = 3;//岗位优势需求人数
        ST_ProfessionType = ProfessionType.财务;//岗位优势需求类型

        MoneyRequest = 50;
        ItemTypeRequest = new List<int>() { };
        ItemValueRequest = new List<int>() { };
    }

    protected override void EffectA(Employee emp, EventGroupInfo egi = null, Employee target = null )
    {
        egi.TargetDivision.AddPerk(new Perk63());
    }
    protected override void EffectB(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        egi.TargetDivision.AddPerk(new Perk64());
    }
    protected override void EffectC(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        egi.TargetDep.StopWorkTime += 6;
    }
    protected override void EffectD(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        egi.TargetDep.StopWorkTime += 6;
    }
    protected override void EffectE(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        emp.InfoDetail.Fire(false);
    }
    protected override void EffectF(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        emp.InfoDetail.Fire(false);
    }

    public override string EventDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = SelfName + "带领办公室员工在食堂中集体抗议食堂收费的事情，指出在“虚构天堂有限公司”，人们吃午饭不需要花自己的钱";
        else if (index == 2)
            content = SelfName + "在办公室前对所有员工大肆宣传自己的功绩，如果有同样的作为，在“虚构天堂有限公司”已经工资涨到衣食无忧了";
        else if (index == 3)
            content = SelfName + "跟一些同事一直在办公室里敲桌子，自己不工作的同时还严重影响周围同事工作";
        else if (index == 4)
            content = SelfName + "号召员工夺回属于自己的与“虚构天堂有限公司”一样每个月放30天假的权益";
        else if (index == 5)
            content = SelfName + "威胁称自己将要跳槽去“虚构天堂有限公司”";
        else if (index == 6)
            content = SelfName + "表示除非给他“虚构天堂有限公司”一样的待遇，否则分给他的工作就别想推进了";

        return content;
    }

    public override string ResultDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = DivName + "获得状态“食堂降价”×1，每个“食堂降价”使事业部运行成本增加50";
        else if (index == 2)
            content = DivName + "获得状态“要求涨工资”×1，每个“要求涨工资”使事业部运行成本增加50";
        else if (index == 3)
            content = DepName + "停工6回合";
        else if (index == 4)
            content = DepName + "停工6回合";
        else if (index == 5)
            content = SelfName + "离职";
        else if (index == 6)
            content = SelfName + "离职";

        return content;
    }
}
public class EventGroup4 : EventGroup
{
    public EventGroup4() : base()
    {
        EventName = "甲板嘉年华";
        StageCount = 6;
        ExtraStage = 1;
        SubEventNames[0] = "脱口秀";
        SubEventNames[1] = "仙人掌大战";
        SubEventNames[2] = "明星员工见面会";
        SubEventNames[3] = "电音派对";
        SubEventNames[4] = "读书会";
        SubEventNames[5] = "甲板烧烤";
        DebuffEvent = false;


        ST_BaseRate = 0.3f;//基础成功率
        ST_EmpRate = 0.05f;//每增加一名员工提供的额外成功率

        ST_SkillRate = 0.2f;//技能需求额外成功率
        ST_SkillCount = 5;//技能需求阈值
        ST_SkillType = 1;//1决策 2管理 3坚韧

        ST_ProfessionRate = 0.2f;//岗位优势额外成功率
        ST_ProfessionCount = 3;//岗位优势需求人数
        ST_ProfessionType = ProfessionType.市场营销;//岗位优势需求类型

        MoneyRequest = 50;
        ItemTypeRequest = new List<int>() { };
        ItemValueRequest = new List<int>() { };
    }

    protected override void EffectA(Employee emp, EventGroupInfo egi = null, Employee target = null )
    {
        foreach (DepControl dep in egi.TargetDivision.CurrentDeps)
        {
            foreach(Employee e in dep.CurrentEmps)
            {
                e.AddEmotion(EColor.Yellow);
            }
        }
        if (egi.TargetDivision.Manager != null)
            egi.TargetDivision.Manager.AddEmotion(EColor.Yellow);
    }
    protected override void EffectB(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        DivisionControl div = egi.TargetDivision;
        if (div != null)
        {
            foreach (DepControl dep in div.CurrentDeps)
            {
                foreach (Employee e in dep.CurrentEmps)
                {
                    e.InfoDetail.AddPerk(new Perk65());
                }
            }
            if (div.Manager != null)
                div.Manager.InfoDetail.AddPerk(new Perk65());
        }
    }
    protected override void EffectC(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        DivisionControl div = egi.TargetDivision;
        if (div != null)
        {
            foreach (DepControl dep in div.CurrentDeps)
            {
                foreach (Employee e in dep.CurrentEmps)
                {
                    e.InfoDetail.AddPerk(new Perk66());
                }
            }
            if (div.Manager != null)
                div.Manager.InfoDetail.AddPerk(new Perk66());
        }
    }
    protected override void EffectD(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        egi.TargetDivision.AddPerk(new Perk67());
    }
    protected override void EffectE(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        egi.TargetDivision.AddPerk(new Perk68());
    }
    protected override void EffectF(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        egi.TargetDivision.AddPerk(new Perk69());
    }

    public override string EventDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = SelfName + "举办了一场甲板脱口秀，欢笑声盖过了引擎的轰鸣";
        else if (index == 2)
            content = "在" + SelfName + "的组织下公司员工进行了异常仙人掌大战，他们拿着仙人掌击剑，穿好护具之后互相投掷仙人球，看起来就像是小学上体育课的孩子们";
        else if (index == 3)
            content = "明星员工" + SelfName + "在嘉年华上办了一场见面会，意图帮助同事们解决问题，有求必应，有问必答。";
        else if (index == 4)
            content = "“我是本场party的主持人" + SelfName + "，这是我们的DJ！”伴随着尖叫声和开场白，电音派对即将开始";
        else if (index == 5)
            content = "主持人" + SelfName + "在台上宣布规则，台下的人们举起手抢着要坐上中间那把扶手椅，虽然看起来像是死刑";
        else if (index == 6)
            content = "大厨" + SelfName + "正马不停蹄地从同事手中接过烤串，他成了一个无情的烧烤机器";

        return content;
    }

    public override string ResultDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = "失败无效果，成功则" + DivName + "所有人情绪变为“振奋”";
        else if (index == 2)
            content = "失败无效果，成功则" + DivName + "中所有人获得“仙人掌大战”状态，所有一般事件修正+5，持续6个回合";
        else if (index == 3)
            content = "失败无效果，成功则全公司所有人获得“明星见面会”状态，每回合额外获得3点经验，持续6回合";
        else if (index == 4)
            content = "失败无效果，成功则" + DivName + "获得状态“电音派对”×1，每个“电音派对”使事业部工作状态+3，持续6回合";
        else if (index == 5)
            content = "失败无效果，成功则" + DivName + "获得状态“读书会”×1，每个“读书会”使事业部效率+3，持续6回合";
        else if (index == 6)
            content = "失败无效果，成功则" + DivName + "获得“甲板烧烤”状态×1，每个“甲板烧烤”使事业部信念+30，持续6回合";

        return content;
    }
}
public class EventGroup5 : EventGroup
{
    public EventGroup5() : base()
    {
        EventName = "社团活动";
        StageCount = 4;
        ExtraStage = 1;
        SubEventNames[0] = "冥想";
        SubEventNames[1] = "排球赛";
        SubEventNames[2] = "夜跑";
        SubEventNames[3] = "辩论会";
        DebuffEvent = false;

        ST_BaseRate = 0.3f;//基础成功率
        ST_EmpRate = 0.1f;//每增加一名员工提供的额外成功率

        ST_SkillRate = 0.1f;//技能需求额外成功率
        ST_SkillCount = 3;//技能需求阈值
        ST_SkillType = 1;//1决策 2管理 3坚韧

        ST_ProfessionRate = 0.2f;//岗位优势额外成功率
        ST_ProfessionCount = 2;//岗位优势需求人数
        ST_ProfessionType = ProfessionType.财务;//岗位优势需求类型

        MoneyRequest = 50;
        ItemTypeRequest = new List<int>() { };
        ItemValueRequest = new List<int>() { };
    }

    protected override void EffectA(Employee emp, EventGroupInfo egi = null, Employee target = null )
    {
        egi.TargetDivision.AddPerk(new Perk70());
    }
    protected override void EffectB(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        foreach (DepControl dep in egi.TargetDivision.CurrentDeps)
        {
            foreach (Employee e in dep.CurrentEmps)
            {
                e.AddEmotion(EColor.Yellow);
            }
        }
        if (egi.TargetDivision.Manager != null)
            egi.TargetDivision.Manager.AddEmotion(EColor.Yellow);
    }
    protected override void EffectC(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        foreach (DepControl dep in egi.TargetDivision.CurrentDeps)
        {
            foreach (Employee e in dep.CurrentEmps)
            {
                e.InfoDetail.AddPerk(new Perk71());
            }
        }
        if (egi.TargetDivision.Manager != null)
            egi.TargetDivision.Manager.InfoDetail.AddPerk(new Perk71());
    }
    protected override void EffectD(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        int num = Random.Range(0, PerkData.OptionCardPerkList.Count);
        Perk perk = PerkData.OptionCardPerkList[num].Clone();
        emp.InfoDetail.AddPerk(perk);
    }

    public override string EventDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = SelfName + "和同事们正闭目冥想，甚至有些进入忘我的状态，悬浮在天空中";
        else if (index == 2)
            content = SelfName + "在规则中明确宣布本次排球比赛不得使用V字斩";
        else if (index == 3)
            content = SelfName + "带领的夜跑队伍越来越大，吸引了很多一边跟着跑一边问是不是着火了的同事";
        else if (index == 4)
            content = SelfName + "在辩论会上眉飞色舞，裁判已经在考虑是否要在规则中加入参赛者必须佩戴头盔的规定";

        return content;
    }

    public override string ResultDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = "失败无效果，成功则" + DivName + "获得“冥想”状态×1，每个“冥想”使事业部信念+30，持续6回合";
        else if (index == 2)
            content = "失败无效果，成功则" + DivName + "所有人情绪变为“振奋”";
        else if (index == 3)
            content = "失败无效果，成功则" + DivName + "中所有人获得“夜跑”状态，所有一般事件修正+5，持续6个回合";
        else if (index == 4)
            content = "失败无效果，成功则员工获得随机正面特质";

        return content;
    }
}
public class EventGroup6 : EventGroup
{
    public EventGroup6() : base()
    {
        EventName = "合作工坊";
        StageCount = 4;
        ExtraStage = 1;
        SubEventNames[0] = "骇客工坊";
        SubEventNames[1] = "管线设计";
        SubEventNames[2] = "资源共享";
        SubEventNames[3] = "产品创意";
        DebuffEvent = false;

        ST_BaseRate = 0.3f;//基础成功率
        ST_EmpRate = 0.1f;//每增加一名员工提供的额外成功率

        ST_SkillRate = 0.1f;//技能需求额外成功率
        ST_SkillCount = 3;//技能需求阈值
        ST_SkillType = 1;//1决策 2管理 3坚韧

        ST_ProfessionRate = 0.2f;//岗位优势额外成功率
        ST_ProfessionCount = 2;//岗位优势需求人数
        ST_ProfessionType = ProfessionType.产品设计;//岗位优势需求类型

        MoneyRequest = 50;
        ItemTypeRequest = new List<int>() { };
        ItemValueRequest = new List<int>() { };
    }

    protected override void EffectA(Employee emp, EventGroupInfo egi = null, Employee target = null )
    {
        int num = Random.Range(0, PerkData.OptionCardPerkList.Count);
        Perk perk = PerkData.OptionCardPerkList[num].Clone();
        emp.InfoDetail.AddPerk(perk);
    }
    protected override void EffectB(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        foreach(Employee e in GameControl.Instance.CurrentEmployees)
        {
            e.InfoDetail.AddPerk(new Perk72());
        }
    }
    protected override void EffectC(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        egi.TargetDivision.AddPerk(new Perk73());
    }
    protected override void EffectD(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        egi.TargetDivision.AddPerk(new Perk74());
    }

    public override string EventDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = "“哔哔 哔 哔哔哔哔 哔哔”——收到一封来自" + SelfName + "的邮件，在分析明白究竟说了什么值钱，杀毒软件就已经崩溃了";
        else if (index == 2)
            content = SelfName + "在幻灯片中介绍了其他公司领先的工作流程设计理念，提倡使用新的工作管线";
        else if (index == 3)
            content = "为促进互相理解，" + SelfName + "提议同事们各自将自己珍藏的资源共享给其他同事，今天正在介绍世界毁灭前的经典动画葫芦娃";
        else if (index == 4)
            content = SelfName + "在业余时间举办了创意工坊，号召所有同事";

        return content;
    }

    public override string ResultDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = "失败无效果，成功则员工获得随机正面特质";
        else if (index == 2)
            content = "失败无效果，成功则全公司所有人获得“管线设计”状态，每回合额外获得3点经验，持续6回合";
        else if (index == 3)
            content = "失败无效果，成功则" + DivName + "获得状态“资源共享”×1，每个“资源共享”使事业部效率+3，持续6回合";
        else if (index == 4)
            content = "失败无效果，成功则" + DivName + "获得“产品创意”状态×1，每个“产品创意”使事业部信念+30，持续6回合";

        return content;
    }
}
public class EventGroup7 : EventGroup
{
    public EventGroup7() : base()
    {
        EventName = "业务封锁";
        StageCount = 6;
        ExtraStage = 2;
        SubEventNames[0] = "失去信心";
        SubEventNames[1] = "设施老旧";
        SubEventNames[2] = "供给不足";
        SubEventNames[3] = "寻找坐标";
        SubEventNames[4] = "谁是内鬼";
        SubEventNames[5] = "恶意断网";
        DebuffEvent = true;

        ST_BaseRate = 0.3f;//基础成功率
        ST_EmpRate = 0.05f;//每增加一名员工提供的额外成功率

        ST_SkillRate = 0.2f;//技能需求额外成功率
        ST_SkillCount = 5;//技能需求阈值
        ST_SkillType = 1;//1决策 2管理 3坚韧

        ST_ProfessionRate = 0.2f;//岗位优势额外成功率
        ST_ProfessionCount = 3;//岗位优势需求人数
        ST_ProfessionType = ProfessionType.工程学;//岗位优势需求类型

        MoneyRequest = 50;
        ItemTypeRequest = new List<int>() { };
        ItemValueRequest = new List<int>() { };
    }

    protected override void EffectA(Employee emp, EventGroupInfo egi = null, Employee target = null )
    {
        emp.InfoDetail.Fire(false);
    }
    protected override void EffectB(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        GameControl.Instance.AddPerk(new Perk75());
    }
    protected override void EffectC(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        egi.TargetDep.StopWorkTime += 6;
    }
    protected override void EffectD(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        egi.TargetDep.StopWorkTime += 6;
    }
    protected override void EffectE(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        egi.TargetDivision.AddPerk(new Perk76());
    }
    protected override void EffectF(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        egi.TargetDep.StopWorkTime += 6;
    }

    public override string EventDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = SelfName + "非常担心公司目前在商战中遭受的重创无法挽回";
        else if (index == 2)
            content = SelfName + "对同事讲如果公司不升级办公设施，下一次商战依然会失败";
        else if (index == 3)
            content = SelfName + "汇报了自己所在部门面临上游供给不足的状况";
        else if (index == 4)
            content = "对手公司控制了我公司的GPS联网信息，如果不能解决问题，" + SelfName + "接下来两个月都回不去家了";
        else if (index == 5)
            content = SelfName + "怀疑本次商战的失败是由于事业部中有内鬼导致的";
        else if (index == 6)
            content = SelfName + "收到匿名邮件称商战的对手公司已经掌控了我公司的后台接口，即将切断" + DivName + "与网络的连接";

        return content;
    }

    public override string ResultDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = SelfName + "离职";
        else if (index == 2)
            content = "公司获得状态“设施老旧”×1，每个“设施老旧”状态降低士气10点，持续6回合";
        else if (index == 3)
            content = DepName + "停工6回合";
        else if (index == 4)
            content = DepName + "停工6回合";
        else if (index == 5)
            content = DivName + "获得状态“谁是内鬼”×1，每个“谁是内鬼”导致信念下降30点，持续6回合";
        else if (index == 6)
            content = DepName + "停工6回合";

        return content;
    }
}
public class EventGroup8 : EventGroup
{
    public EventGroup8() : base()
    {
        EventName = "公关危机";
        StageCount = 4;
        ExtraStage = 1;
        SubEventNames[0] = "谣言";
        SubEventNames[1] = "舆论谴责";
        SubEventNames[2] = "用户流失";
        SubEventNames[3] = "段子横飞";
        BSTurnCorrection = 1;
        DebuffEvent = true;

        ST_BaseRate = 0.3f;//基础成功率
        ST_EmpRate = 0.1f;//每增加一名员工提供的额外成功率

        ST_SkillRate = 0.1f;//技能需求额外成功率
        ST_SkillCount = 3;//技能需求阈值
        ST_SkillType = 1;//1决策 2管理 3坚韧

        ST_ProfessionRate = 0.2f;//岗位优势额外成功率
        ST_ProfessionCount = 2;//岗位优势需求人数
        ST_ProfessionType = ProfessionType.市场营销;//岗位优势需求类型

        MoneyRequest = 50;
        ItemTypeRequest = new List<int>() { };
        ItemValueRequest = new List<int>() { };
    }

    protected override void EffectA(Employee emp, EventGroupInfo egi = null, Employee target = null )
    {
        GameControl.Instance.AddPerk(new Perk77());
    }
    protected override void EffectB(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        GameControl.Instance.AddPerk(new Perk78());
    }
    protected override void EffectC(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        egi.TargetDivision.AddPerk(new Perk79());
    }
    protected override void EffectD(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        DivisionControl div = egi.TargetDivision;

        if (div != null)
        {
            foreach (DepControl dep in div.CurrentDeps)
            {
                foreach (Employee e in dep.CurrentEmps)
                {
                    e.AddEmotion(EColor.Red);
                }
            }
            if (div.Manager != null)
                div.Manager.AddEmotion(EColor.Red);
        }
    }

    public override string EventDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = "公司中关于经营状态下滑的谣言导致人心惶惶，" + SelfName + "希望CEO给出解释";
        else if (index == 2)
            content = "网络上频繁出现的关于我公司蔑视人情恶意破坏用户体验甚至挤占良心对手公司市场的言论导致" + SelfName + "为代表的员工们人心惶惶";
        else if (index == 3)
            content = SelfName + "对于商战失败所带来的用户流失感到十分焦虑";
        else if (index == 4)
            content = SelfName + "和同事们非常恐慌，因为自己家的亲戚在视频电话中对关于我公司的段子信手拈来";

        return content;
    }

    public override string ResultDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = "公司获得状态“谣言”×1，每个“谣言”状态降低士气10点，持续6回合";
        else if (index == 2)
            content = "公司获得状态“舆论谴责”×1，每个“舆论谴责”状态降低士气10点，持续6回合";
        else if (index == 3)
            content = DivName + "获得状态“用户流失”×1，每个“用户流失”导致信念下降30点，持续6回合";
        else if (index == 4)
            content = DivName + "所有员工主导情绪变为“愤怒”";

        return content;
    }
}
public class EventGroup9 : EventGroup
{
    public EventGroup9() : base()
    {
        EventName = "市场监管";
        StageCount = 4;
        ExtraStage = 1;
        SubEventNames[0] = "账户冻结";
        SubEventNames[1] = "夸大事实";
        SubEventNames[2] = "设备维修";
        SubEventNames[3] = "盗版软件";
        DebuffEvent = true;

        ST_BaseRate = 0.3f;//基础成功率
        ST_EmpRate = 0.1f;//每增加一名员工提供的额外成功率

        ST_SkillRate = 0.1f;//技能需求额外成功率
        ST_SkillCount = 3;//技能需求阈值
        ST_SkillType = 1;//1决策 2管理 3坚韧

        ST_ProfessionRate = 0.2f;//岗位优势额外成功率
        ST_ProfessionCount = 2;//岗位优势需求人数
        ST_ProfessionType = ProfessionType.人力资源;//岗位优势需求类型

        MoneyRequest = 50;
        ItemTypeRequest = new List<int>() { };
        ItemValueRequest = new List<int>() { };
    }

    protected override void EffectA(Employee emp, EventGroupInfo egi = null, Employee target = null )
    {
        GameControl.Instance.Money -= (GameControl.Instance.Money / 2);
    }
    protected override void EffectB(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        GameControl.Instance.AddPerk(new Perk80());
    }
    protected override void EffectC(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        egi.TargetDivision.AddPerk(new Perk81());
    }
    protected override void EffectD(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        GameControl.Instance.AddPerk(new Perk82());
    }

    public override string EventDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = SelfName + "询问CEO关于公司账户被冻结一事是否有计划";
        else if (index == 2)
            content = SelfName + "正在和同事分析在市场监管的调查报告中看到的关于公司对内和对外公布的营收数据有较大出入的问题";
        else if (index == 3)
            content = SelfName + "坚决要求CEO履行市场监管部门提出的设备维护标准";
        else if (index == 4)
            content = SelfName + "和同事们谴责公司使用盗版软件的问题";

        return content;
    }

    public override string ResultDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = "损失一半金钱（自动计算出具体数字）";
        else if (index == 2)
            content = "公司获得状态“夸大事实”×1，每个“夸大事实”状态降低士气10点，持续6回合";
        else if (index == 3)
            content = DivName + "获得状态“设备维修”×1，每个“设备维修”使事业部运行成本增加50";
        else if (index == 4)
            content = "公司获得状态“盗版软件”×1，每个“盗版软件”状态降低士气10点，持续6回合";

        return content;
    }
}
public class EventGroup10 : EventGroup
{
    public EventGroup10() : base()
    {
        EventName = "裁员恐慌";
        StageCount = 2;
        ExtraStage = 1;
        SubEventNames[0] = "人人自危";
        SubEventNames[1] = "琢磨跳槽";
        SubEventNames[2] = "猎头上门";
        DebuffEvent = true;

        ST_BaseRate = 0.3f;//基础成功率
        ST_EmpRate = 0.1f;//每增加一名员工提供的额外成功率

        ST_SkillRate = 0.1f;//技能需求额外成功率
        ST_SkillCount = 3;//技能需求阈值
        ST_SkillType = 1;//1决策 2管理 3坚韧

        ST_ProfessionRate = 0.2f;//岗位优势额外成功率
        ST_ProfessionCount = 2;//岗位优势需求人数
        ST_ProfessionType = ProfessionType.人力资源;//岗位优势需求类型

        MoneyRequest = 50;
        ItemTypeRequest = new List<int>() { };
        ItemValueRequest = new List<int>() { };
    }

    protected override void EffectA(Employee emp, EventGroupInfo egi = null, Employee target = null )
    {
        egi.TargetDivision.AddPerk(new Perk83());
    }
    protected override void EffectB(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        GameControl.Instance.AddPerk(new Perk84());
    }
    protected override void EffectC(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        foreach(DepControl dep in egi.TargetDivision.CurrentDeps)
        {
            foreach(Employee e in dep.CurrentEmps)
            {
                for (int i = 0; i < 6; i++)
                {
                    e.AddEmotion(EColor.LBlue);
                }
            }
        }
    }

    public override string EventDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = SelfName + "对同事表示自己非常担心自己也会丢掉工作";
        else if (index == 2)
            content = SelfName + "在论坛中发帖称已经有同事在向其他公司投简历了，并且贴出了详细的教程";
        else if (index == 3)
            content = SelfName + "对CEO表示已经有猎头找上他了";

        return content;
    }

    public override string ResultDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = DivName + "获得状态“人人自危”×1，每个“人人自危”导致信念下降30点，持续6回合";
        else if (index == 2)
            content = "公司获得状态“琢磨跳槽”×1，每个“琢磨跳槽”状态降低士气10点，持续6回合";
        else if (index == 3)
            content = DivName + "所有员工获得6个“苦涩”情绪";

        return content;
    }
}
public class EventGroup11 : EventGroup
{
    public EventGroup11() : base()
    {
        EventName = "怀疑经营情况";
        StageCount = 2;
        ExtraStage = 1;
        SubEventNames[0] = "要求公开财务";
        SubEventNames[1] = "出现幻觉";
        BSTurnCorrection = 1;
        DebuffEvent = true;

        ST_BaseRate = 0.3f;//基础成功率
        ST_EmpRate = 0.1f;//每增加一名员工提供的额外成功率

        ST_SkillRate = 0.1f;//技能需求额外成功率
        ST_SkillCount = 3;//技能需求阈值
        ST_SkillType = 1;//1决策 2管理 3坚韧

        ST_ProfessionRate = 0.2f;//岗位优势额外成功率
        ST_ProfessionCount = 1;//岗位优势需求人数
        ST_ProfessionType = ProfessionType.财务;//岗位优势需求类型

        MoneyRequest = 50;
        ItemTypeRequest = new List<int>() { };
        ItemValueRequest = new List<int>() { };
    }

    protected override void EffectA(Employee emp, EventGroupInfo egi = null, Employee target = null )
    {
        GameControl.Instance.AddPerk(new Perk85());
    }
    protected override void EffectB(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        egi.TargetDivision.AddPerk(new Perk86());
    }

    public override string EventDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = SelfName + "在公司论坛中发帖称公司如果不公开财务信息则说明公司可能会持续裁员";
        else if (index == 2)
            content = SelfName + "因为员工的离职陷入了悲痛之中，昨天晚上梦见了公司账户已经见底";

        return content;
    }

    public override string ResultDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = "公司获得状态“要求公开财务”×1，每个“要求公开财务”状态降低士气10点，持续6回合";
        else if (index == 2)
            content = DivName + "获得状态“出现幻觉”×1，每个“出现幻觉”导致信念下降30点，持续6回合";

        return content;
    }
}
public class EventGroup12 : EventGroup
{
    public EventGroup12() : base()
    {
        EventName = "工作停滞";
        StageCount = 2;
        ExtraStage = 1;
        SubEventNames[0] = "迷路";
        SubEventNames[1] = "资料丢失";
        SubEventNames[2] = "担心裁员";
        BSTurnCorrection = 1;
        DebuffEvent = true;

        ST_BaseRate = 0.3f;//基础成功率
        ST_EmpRate = 0.1f;//每增加一名员工提供的额外成功率

        ST_SkillRate = 0.1f;//技能需求额外成功率
        ST_SkillCount = 3;//技能需求阈值
        ST_SkillType = 1;//1决策 2管理 3坚韧

        ST_ProfessionRate = 0.2f;//岗位优势额外成功率
        ST_ProfessionCount = 2;//岗位优势需求人数
        ST_ProfessionType = ProfessionType.工程学;//岗位优势需求类型

        MoneyRequest = 50;
        ItemTypeRequest = new List<int>() { };
        ItemValueRequest = new List<int>() { };
    }

    protected override void EffectA(Employee emp, EventGroupInfo egi = null, Employee target = null )
    {
        egi.TargetDivision.AddPerk(new Perk87());
    }
    protected override void EffectB(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        GameControl.Instance.AddPerk(new Perk88());
    }
    protected override void EffectC(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        DivisionControl div = egi.TargetDivision;

        if (div != null)
        {
            foreach (DepControl dep in div.CurrentDeps)
            {
                foreach (Employee e in dep.CurrentEmps)
                {
                    e.AddEmotion(EColor.Purple);
                }
            }
            if (div.Manager != null)
                div.Manager.AddEmotion(EColor.Purple);
        }
    }

    public override string EventDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = "由于部门裁撤，" + SelfName + "和同事们每次路过之前部门所在的区域附近都会迷路";
        else if (index == 2)
            content = "被裁撤的部门带走了" + SelfName + "和同事们一起工作时准备的一些资料";
        else if (index == 3)
            content = SelfName + "和同事们非常担心今天是删除部门，明天就是删除他们";

        return content;
    }

    public override string ResultDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = DivName + "获得状态“迷路”×1，每个“迷路”导致信念下降30点，持续6回合";
        else if (index == 2)
            content = "公司获得状态“资料丢失”×1，每个“资料丢失”状态降低士气10点，持续6回合";
        else if (index == 3)
            content = DivName + "所有员工主导情绪变为“沮丧”";

        return content;
    }
}
public class EventGroup13 : EventGroup
{
    public EventGroup13() : base()
    {
        EventName = "出现幻觉";
        StageCount = 2;
        ExtraStage = 1;
        SubEventNames[0] = "怀念建筑";
        SubEventNames[1] = "额外工作";
        BSTurnCorrection = 1;
        DebuffEvent = true;

        ST_BaseRate = 0.3f;//基础成功率
        ST_EmpRate = 0.1f;//每增加一名员工提供的额外成功率

        ST_SkillRate = 0.1f;//技能需求额外成功率
        ST_SkillCount = 3;//技能需求阈值
        ST_SkillType = 1;//1决策 2管理 3坚韧

        ST_ProfessionRate = 0.2f;//岗位优势额外成功率
        ST_ProfessionCount = 2;//岗位优势需求人数
        ST_ProfessionType = ProfessionType.人力资源;//岗位优势需求类型

        MoneyRequest = 50;
        ItemTypeRequest = new List<int>() { };
        ItemValueRequest = new List<int>() { };
    }

    protected override void EffectA(Employee emp, EventGroupInfo egi = null, Employee target = null )
    {
        DivisionControl div = egi.TargetDivision;

        if (div != null)
        {
            foreach (DepControl dep in div.CurrentDeps)
            {
                foreach (Employee e in dep.CurrentEmps)
                {
                    e.AddEmotion(EColor.Purple);
                }
            }
            if (div.Manager != null)
                div.Manager.AddEmotion(EColor.Purple);
        }
    }
    protected override void EffectB(Employee emp, EventGroupInfo egi = null, Employee target = null)
    {
        GameControl.Instance.AddPerk(new Perk89());
    }

    public override string EventDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = SelfName + "和事业部的同事非常怀念当时跟被裁撤的部门一起工作的日子";
        else if (index == 2)
            content = "由于删除了建筑物，" + SelfName + "所在的事业部需要处理本不属于本部门的额外工作";

        return content;
    }

    public override string ResultDescription(Employee Emp, Employee targetEmp, int index, EventGroupInfo egi = null)
    {
        string content = "";
        SetNames(Emp, targetEmp, egi);
        if (Emp != null)
            SelfName = Emp.Name;
        if (targetEmp != null)
            TargetName = targetEmp.Name;

        if (index == 1)
            content = DivName + "全体员工主导情绪变为“沮丧”";
        else if (index == 2)
            content = "公司获得状态“额外工作”×1，每个“额外工作”状态降低士气10点，持续6回合";

        return content;
    }
}
