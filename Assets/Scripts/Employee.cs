using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProfessionType
{//{1, 2, 3, 8, 9, 11, 12, 13, 15, 16};//Nst:几个专业技能对应的编号
    技术, 市场, 产品, 观察, 坚韧, 强壮, 管理, 人力, 财务, 决策, 行业, 谋略, 说服, 魅力, 八卦, 行政
}

public enum OccupationType
{
    超级黑客, 神秘打工仔, 大企业中层, 海盗, 大学毕业生, 论坛版主, 独立开发者, 键盘艺术家, 酒保
}

public enum EColor
{//L开头为对应1级浅色情绪
    White, Gray, LYellow, LRed, LBlue, LOrange, LPurple, LGreen, Yellow, Red, Blue, Orange, Purple, Green, None
}

public enum Ambition
{
    专家, 高管, 打工人, 无
}

public class Emotion
{
    public EColor color;
    public int Level;
    public Emotion(EColor e)
    {
        color = e;
        Level = 1;
    }
}

public class EmpItem
{
    public int Type, TimeLeft;
    public Employee Target;

    public EmpItem(int type, int timeleft = -1, Employee emp = null)
    {
        Type = type;
        TimeLeft = timeleft;
        Target = emp;
    }
}

public class Employee
{
    static public int AttributeLimit = 10;

    //1.技术 2.市场 3.产品 4.Ob观察 5.Te坚韧 6.Str强壮 7.Ma管理 8.HR人力 9.Fi财务 10.De决策 
    //11.Fo行业 12.St谋略 13.Co说服 14.Ch魅力 15.Go八卦 16.Ad行政

    public int Tenacity
    {
        get { return tenacity; }
        set
        {
            if (value < 0)
                tenacity = 0;
            else
                tenacity = value;
        }
    }
    public int Strength
    {
        get { return strength; }
        set
        {
            if (value < 0)
                strength = 0;
            else
                strength = value;
        }
    }
    public int Manage
    {
        get { return manage; }
        set
        {
            if (value < 0)
                manage = 0;
            else
                manage = value;
        }
    }
    public int Decision
    {
        get { return decision; }
        set
        {
            if (value < 0)
                decision = 0;
            else
                decision = value;
        }
    }
    private int tenacity, strength, manage, decision;
    //体力  心力
    public int Stamina
    {
        get { return stamina; }
        set
        {
            stamina = value;
            if (stamina > StaminaLimit + StaminaLimitExtra)
                stamina = StaminaLimit + StaminaLimitExtra;
            else if (stamina < 0)
                stamina = 0;
        }
    }
    public int Mentality
    {
        get { return mentality; }
        set
        {
            //低心力的效果
            if (InfoDetail != null)
            {
                if (mentality >= 20 && value < 20)
                {
                    InfoDetail.AddPerk(new Perk119(this));

                    if(CurrentDep == null)
                        QuestControl.Instance.Init(Name + "出现心力低下现象\n可以调节所在部门的信念或使用CEO技能“安抚”");
                    else if (CurrentDep != null)
                    {
                        System.Action AgreeAction = () => { CurrentDep.ShowEmpInfoPanel(); };
                        QuestControl.Instance.Init(Name + "出现心力低下现象\n可以调节所在部门的信念或使用CEO技能“安抚”\n点击“确认”" +
                            "跳转至员工所在部门详细面板", AgreeAction);
                    }
                }
                else if (mentality < 20 && value >= 20)
                {
                    InfoDetail.RemovePerk(119);
                }
            }

            mentality = value;
            if (mentality > MentalityLimit + MentalityLimitExtra)
                mentality = MentalityLimit + MentalityLimitExtra;
            else if (mentality <= 0)
            {
                mentality = 0;
                if (InfoA.GC.BSC.BSStarted == false)
                    Exhausted();
            }
            //此处的开除不知道是否为现有设定
            //if (mentality < 50)
            //    InfoA.Fire();
        }
    }
    public int StaminaLimit { get { return 100 + (Strength * 5) + StaminaLimitExtra; } set { StaminaLimit = value;} } //体力上限
    public int MentalityLimit { get { return 100 + (Tenacity * 5) + MentalityLimitExtra; } set { MentalityLimit = value; } } // 心力上限
    public int StaminaLimitExtra = 0; //体力上限额外值
    public int MentalityLimitExtra = 0; //心力上限额外值
    public int Exp = 0;//升级所需的经验
    public int ExtraExp = 0;//每回合获得的经验

    public int SalaryExtra = 0, Age, EventTime, ObeyTime, NoPromotionTime = 0, NoMarriageTime = 0,
        VacationTime = 0, SpyTime = 0, CoreMemberTime;//放假时间、间谍时间、核心成员说服CD时间
    public int ManagerExp;//作为高管的经验值，属下员工技能升级时经验+1，攒够10点获得一个技能点数
    public int Confidence;//信心，头脑风暴中的护盾
    public int SkillLimitTime;//头脑风暴中技能禁用的回合数
    public int NewRelationTargetTime = 1;
    public float ExtraSuccessRate = 0, SalaryMultiple = 1.0f;
    public OccupationType Occupation;
    public Ambition ambition;

    public List<int> Professions = new List<int>();//员工的岗位优势
    public List<int[]> CurrentDices = new List<int[]>();//员工的骰子
    public int[] CharacterTendency = new int[4];//(0文化 -独裁 +开源) (1信仰-机械 +人文) (2道德-功利主义 +绝对律令) (3行事-随心所欲 +严格守序)
    public int[] CurrentSkillType = new int[3];
    public float[] Character = new float[5]; //0文化 1信仰 2道德 3行事 4信念

    public string Name;
    public bool isCEO = false, SupportCEO;

    public EmpInfo InfoA, InfoB, InfoDetail;
    public DepControl CurrentDep;
    public DivisionControl CurrentDivision;
    public Employee Master, Lover, RTarget;
    public Clique CurrentClique;

    public List<Employee> Students = new List<Employee>();
    public List<Employee> RelationTargets = new List<Employee>(); 
    public List<Relation> Relations = new List<Relation>();
    public List<EmpItem> CurrentItems = new List<EmpItem>();
    public List<Emotion> CurrentEmotions = new List<Emotion>();
    public List<EmpBuff> CurrentBuffs = new List<EmpBuff>();
    public List<int> ExhaustedCount = new List<int>();

    private int mentality, stamina;

    //初始化员工属性
    public void InitStatus()
    {
        float YearPosb = Random.Range(0.0f, 1.0f);
        int WorkYear = 0;
        if (YearPosb < 0.6f)
            WorkYear = Random.Range(0, 3);
        else if (YearPosb < 0.9f)
            WorkYear = Random.Range(3, 8);
        else
            WorkYear = Random.Range(8, 15);

        Age = 20 + WorkYear;

        RandomOccupation();
        //设定姓名并检查是否重名
        bool nameCheck = false;
        while (nameCheck == false)
        {
            Name = CNName.RandomName();
            nameCheck = true;
            for(int i = 0; i < InfoA.GC.CurrentEmployees.Count; i++)
            {
                if(InfoA.GC.CurrentEmployees[i].Name == Name)
                {
                    nameCheck = false;
                    break;
                }
            }
        }


        //确定倾向
        for (int i = 0; i < 5; i++)
        {
            Character[i] = Random.Range(-100, 101);
            if (i == 4)
                Character[4] = Random.Range(0, 101);
        }
        CheckCharacter();

        EventTime = Random.Range(7,10);
        Stamina = StaminaLimit;
        Mentality = MentalityLimit;
    }

    //初始化CEO属性
    public void InitCEOStatus()
    {
        isCEO = true;
        Age = 24;
        Name = "X";

        //确定倾向
        for (int i = 0; i < 5; i++)
        {
            Character[i] = Random.Range(50, 101);
            if(i == 0 || i == 1)
            {
                if (Random.Range(0.0f, 1.0f) < 0.5f)
                    Character[i] *= -1;
            }
            if (i == 4)
                Character[4] = Random.Range(0, 101);
        }
        CheckCharacter();
        EventTime = 8;

        RandomOccupation();

        Stamina = StaminaLimit;
        Mentality = MentalityLimit;
    }

    //随机一个职业和志向
    void RandomOccupation(int type = -1)
    {
        int num;
        if (type != -1)
            num = type;
        else
            num = 1;

        if (num == 0)
        {
            Occupation = OccupationType.超级黑客;
            Professions.Add(1);
            Professions.Add(12);
            CurrentDices.Add(new int[6] { 3, 3, 3, 3, -1, -1 });
            CurrentDices.Add(new int[6] { 3, 3, 3, 3, -1, -1 });
        }
        else if (num == 1)
        {
            Occupation = OccupationType.神秘打工仔;
            CurrentDices.Add(new int[6] { 1, 1, 1, 1, -1, -1 });
            CurrentDices.Add(new int[6] { 0, 0, 0, 0, -1, -1 });
            CurrentDices.Add(new int[6] { 5, 5, 5, 5, -1, -1 });
        }
        else if (num == 2)
        {
            Occupation = OccupationType.大企业中层;
            Professions.Add(16);
            CurrentDices.Add(new int[6] { 2, 2, 2, 2, -1, -1 });
            CurrentDices.Add(new int[6] { 4, 4, 4, 4, -1, -1 });
        }
        else if (num == 3)
        {
            Occupation = OccupationType.海盗;
            Professions.Add(9);
            CurrentDices.Add(new int[6] { 1, 1, 1, 1, -1, -1 });
            CurrentDices.Add(new int[6] { 0, 0, 0, 0, -1, -1 });
        }
        else if (num == 4)
        {
            Occupation = OccupationType.大学毕业生;
            Professions.Add(2);
            CurrentDices.Add(new int[6] { 4, 4, 4, 4, -1, -1 });
            CurrentDices.Add(new int[6] { 3, 3, 3, 3, -1, -1 });
        }
        else if (num == 5)
        {
            Occupation = OccupationType.论坛版主;
            Professions.Add(11);
            Professions.Add(15);
            CurrentDices.Add(new int[6] { 0, 0, 0, 0, -1, -1 });
            CurrentDices.Add(new int[6] { 5, 5, 5, 5, -1, -1 });
        }
        else if (num == 6)
        {
            Occupation = OccupationType.独立开发者;
            Professions.Add(3);
            CurrentDices.Add(new int[6] { 2, 2, 2, 2, -1, -1 });
            CurrentDices.Add(new int[6] { 3, 3, 3, 3, -1, -1 });
        }
        else if (num == 7)
        {
            Occupation = OccupationType.键盘艺术家;
            Professions.Add(13);
            CurrentDices.Add(new int[6] { 4, 4, 4, 4, -1, -1 });
            CurrentDices.Add(new int[6] { 4, 4, 4, 4, -1, -1 });
        }
        else if (num == 8)
        {
            Occupation = OccupationType.酒保;
            Professions.Add(8);
            CurrentDices.Add(new int[6] { 2, 2, 2, 2, -1, -1 });
        }

        num = Random.Range(0, 4);
        ambition = (Ambition)num;
    }

    public void GainExp(int value, int type)
    {
        Exp += value;
        //上司经验获取
        if (CurrentDep != null && CurrentDep.CommandingOffice != null)
        {
            if (CurrentDep.CommandingOffice.Manager != null)
            {
                CurrentDep.CommandingOffice.Manager.ManagerExp += 1;
                if (CurrentDep.CommandingOffice.Manager.ManagerExp >= 10)
                {
                    CurrentDep.CommandingOffice.Manager.ManagerExp = 0;
                }
            }
        }

    }

    public void InitRelation()
    {
        for(int i = 0; i < InfoDetail.GC.CurrentEmployees.Count; i++)
        {
            Relations.Add(new Relation(InfoDetail.GC.CurrentEmployees[i], this));
            InfoDetail.GC.CurrentEmployees[i].Relations.Add(new Relation(this, InfoDetail.GC.CurrentEmployees[i]));
        }
    }
    //找到发展目标
    public void FindRelationTarget()
    {
        RelationTargets.Clear();

        List<Relation> tempRelation = new List<Relation>();
        foreach (Relation r in Relations)
        {
            //相互认识
            if (r.KnowEachOther)
            {
                tempRelation.Add(r);
            }
        }
        for (int i = 0; i < 3; i++)
        {
            Relation relation = FindRelationTarget(tempRelation);

            //没有认识的人
            if (relation == null)
                break;

            tempRelation.Remove(relation);
            RelationTargets.Add(relation.Target);
        }
        //旧版发展对象
        //List<Employee> EL = new List<Employee>();
        //for (int i = 0; i < InfoDetail.GC.CurrentEmployees.Count; i++)
        //{
        //    if (InfoDetail.GC.CurrentEmployees[i] != this)
        //        EL.Add(InfoDetail.GC.CurrentEmployees[i]);
        //}
        //if (CurrentDep != null)
        //{
        //    for (int i = 0; i < CurrentDep.CurrentEmps.Count; i++)
        //    {
        //        if (CurrentDep.CurrentEmps[i] != this)
        //            EL.Remove(CurrentDep.CurrentEmps[i]);
        //    }
        //    if (CurrentDep.CommandingOffice != null && CurrentDep.CommandingOffice.CurrentManager != null)
        //        EL.Remove(CurrentDep.CommandingOffice.CurrentManager);
        //}
        //else if (CurrentOffice != null && CurrentOffice.CommandingOffice != null && CurrentOffice.CommandingOffice.CurrentManager != null)
        //    EL.Remove(CurrentOffice.CommandingOffice.CurrentManager);
        //for(int i = 0; i < 3; i++)
        //{
        //    if(EL.Count > 1)
        //    {
        //        Employee NE = EL[Random.Range(0, EL.Count)];
        //        RelationTargets.Add(NE);
        //        EL.Remove(NE);
        //    }
        //}
    }

    //返回一个加权后的随机发展对象
    private Relation FindRelationTarget(List<Relation> relations)
    {
        if (relations.Count == 0)
            return null;
        
        int lastValue = 0;
        List<int> valueList = new List<int>();
        List<Relation> Targets = new List<Relation>();
        foreach (Relation relation in relations)
        {
            int value = 1;  //计算权重
            if (Mathf.Abs(relation.FriendValue) == 1)
                value += 2;
            if (Mathf.Abs(relation.FriendValue) == 2)
                value += 5;
            if (relation.LoveValue == 3)
                value += 3;
            if (relation.LoveValue == 4)
                value += 6;
            if (EmpManager.Instance.FindBoss(this) == relation.Target)
                value -= 2;

            lastValue += value;
            valueList.Add(lastValue);
            Targets.Add(relation);
        }
        int posb = Random.Range(0, valueList[valueList.Count - 1] + 1);
        for (int i = 0; i < valueList.Count; i++)
        {
            if (posb <= valueList[i])
            {
                return (Targets[i]);
            }
        }
        return null;
    }

    //改变跟目标的好感度并检查关系
    public void ChangeRelation(Employee target, int value)
    {
        Relation r = FindRelation(target);
        if (r != null)
        {
            r.RPoint += value;
            if (r.RPoint > 100)
                r.RPoint = 100;
        }
        //else if (r.RPoint < 0)
        //    r.RPoint = 0;
        //r.RelationCheck();
    }

    //找到跟目标的关系
    public Relation FindRelation(Employee t)
    {
        for(int i = 0; i < Relations.Count; i++)
        {
            if (Relations[i].Target == t)
                return Relations[i];
        }
        return null;
    }

    //(开除时)清空所有关系,此为暂定方法,因为部分永久关系如夫妻按理说不应该清除
    public void ClearRelations()
    {
        for(int i = 0; i < InfoDetail.GC.CurrentEmployees.Count; i++)
        {
            //从关系列表中移除
            if (InfoDetail.GC.CurrentEmployees[i] != this)
                InfoDetail.GC.CurrentEmployees[i].Relations.Remove(InfoDetail.GC.CurrentEmployees[i].FindRelation(this));
            //从潜在发展对象中移除
            if (InfoDetail.GC.CurrentEmployees[i].RelationTargets.Contains(this))
                InfoDetail.GC.CurrentEmployees[i].RelationTargets.Remove(this);
        }
        //特殊关系移除
        if (Lover != null)
            Lover.Lover = null;
        if (Master != null)
            Master.Students.Remove(this);
        else if (Students.Count > 0)
        {
            for(int i = 0; i < Students.Count; i++)
            {
                Students[i].Master = null;
            }
        }
    }

    //改变性格和信仰
    public void ChangeCharacter(int type, int value)
    {
        //暂时只进行加减运算
        //0文化 -独裁 +开源
        //1信仰 -机械 +人文
        //2道德 -功利主义 +绝对律令
        //3行事 -随心所欲 +严格守序
        //4信念 怀疑 坚定
        if(type != 4)
        {
            Character[type] += value;
            if (Character[type] > 100)
                Character[type] = 100;
            else if (Character[type] < -100)
                Character[type] = -100;
            CheckCharacter();
        }
        else
        {
            Character[4] += value;
            if (Character[4] > 100)
                Character[4] = 100;
            else if (Character[4] < 0)
                Character[4] = 0;
        }
    }

    //检测性格信仰数值是否达到临界值
    void CheckCharacter()
    {
        if (Character[0] >= 20)
            CharacterTendency[0] = 1;
        else if (Character[0] <= -20)
            CharacterTendency[0] = -1;
        else
            CharacterTendency[0] = 0;

        if (Character[1] >= 20)
            CharacterTendency[1] = 1;
        else if (Character[1] <= -20)
            CharacterTendency[1] = -1;
        else
            CharacterTendency[1] = 0;

        if (Character[2] >= 20)
            CharacterTendency[2] = 1;
        else if (Character[2] <= -20)
            CharacterTendency[2] = -1;
        else
            CharacterTendency[2] = 0;

        if (Character[3] >= 20)
            CharacterTendency[3] = 1;
        else if (Character[3] <= -20)
            CharacterTendency[3] = -1;
        else
            CharacterTendency[3] = 0;
        if (CurrentDep != null)
            CurrentDep.FaithRelationCheck();
    }

    //时间流逝后部分基础属性判定
    public void TimePass()
    {
        if (InfoDetail.MainEmotion != null)
        {
            InfoDetail.MainEmotion.TimeLeft -= 1;
            if (InfoDetail.MainEmotion.TimeLeft == 0)
            {
                CurrentEmotions.Remove(InfoDetail.MainEmotion.E);
                InfoDetail.EmotionInfos.Remove(InfoDetail.MainEmotion);
                InfoDetail.MainEmotion.Active = false;
                InfoDetail.MainEmotion.gameObject.SetActive(false);
            }
        }

        //额外获取经验的结算
        if (ExtraExp > 0)
            GainExp(ExtraExp, 0);

        #region 旧时间判定
        //EventTimePass();
        ////假期计时
        //if (VacationTime > 0)
        //{
        //    VacationTime -= 1;
        //    Stamina += 2;
        //    if (VacationTime == 0)
        //        EndVacation();
        //}
        ////核心成员说服CD倒计时
        //if (CoreMemberTime > 0)
        //    CoreMemberTime -= 1;
        ////间谍时间倒计时
        //if (SpyTime > 0)
        //{
        //    SpyTime -= 1;
        //    if (SpyTime == 0)
        //        InfoDetail.Entity.SetFree();
        //}

        //if (CurrentDep == null)
        //    NoPromotionTime += 1;
        //else if (CurrentDep.building.Type != BuildingType.CEO办公室 || CurrentDep.building.Type != BuildingType.高管办公室)
        //    NoPromotionTime += 1;

        //if (Lover == null)
        //    NoMarriageTime += 1;
        //else if (FindRelation(Lover).LoveValue < 3)
        //    NoMarriageTime += 1;

        ////清除Items
        //if (CurrentItems.Count > 0)
        //{
        //    List<EmpItem> DesItems = new List<EmpItem>();
        //    for (int i = 0; i < CurrentItems.Count; i++)
        //    {
        //        if (CurrentItems[i].TimeLeft > 0)
        //            CurrentItems[i].TimeLeft -= 1;
        //        if (CurrentItems[i].TimeLeft == 0)
        //            DesItems.Add(CurrentItems[i]);
        //    }
        //    for(int i = 0; i < DesItems.Count; i++)
        //    {
        //        CurrentItems.Remove(DesItems[i]);
        //    }
        //    DesItems.Clear();
        //}
        ////计算经验加成
        //if(CurrentBuffs.Count > 0)
        //{
        //    List<EmpBuff> RemoveBonus = new List<EmpBuff>();
        //    foreach(EmpBuff buff in CurrentBuffs)
        //    {
        //        if (buff.Type < 16)
        //            GainExp(buff.Value, buff.Type);
        //        buff.Time -= 1;
        //        if (buff.Time <= 0)
        //            RemoveBonus.Add(buff);
        //    }
        //    foreach(EmpBuff buff in RemoveBonus)
        //    {
        //        if (buff.Type == 16)
        //        {
        //            StaminaLimitExtra -= buff.Value;
        //            Stamina += 0;
        //        }
        //        CurrentBuffs.Remove(buff);
        //    }
        //    RemoveBonus.Clear();
        //}
        #endregion
    }

    //事件相关时间判定
    public void EventTimePass()
    {
        EventTime -= 1;
        if (EventTime == 0)
        {
            EventTime = Random.Range(7, 10);
            InfoDetail.Entity.AddEvent(EmpManager.Instance.RandomEventTarget(this, out int index), index);
        }
        //寻找新关系发展目标
        NewRelationTargetTime -= 1;
        if (NewRelationTargetTime == 0)
        {
            FindRelationTarget();
            NewRelationTargetTime = 192;
        }
    }

    //假期结束后的相关判定
    void EndVacation()
    {
        VacationTime = 0;
        InfoDetail.Entity.SetFree();
        if (isCEO == true)
        {
            InfoDetail.GC.CC.SkillButton.interactable = true;
            InfoDetail.GC.CEOVacation = false;
            foreach(DepControl dep in InfoDetail.GC.CurrentDeps)
            {
                dep.RemovePerk(115);
            }
        }
    }

    //增减情绪
    public void AddEmotion(EColor C)
    {
        if (C == EColor.LYellow)
        {
            for (int i = 0; i < CurrentEmotions.Count; i++)
            {
                if (CurrentEmotions[i].color == EColor.LRed)
                {
                    ReduceEmotion(CurrentEmotions[i]);
                    AddEmotion(EColor.LOrange);
                    AddEmotion(EColor.LOrange);
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.LBlue)
                {
                    ReduceEmotion(CurrentEmotions[i]);
                    AddEmotion(EColor.LGreen);
                    AddEmotion(EColor.LGreen);
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.LPurple)
                {
                    ReduceEmotion(CurrentEmotions[i]);
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.LYellow)
                {
                    if (CurrentEmotions[i].Level < 3)
                        CurrentEmotions[i].Level += 1;
                    else
                    {
                        InfoDetail.RemoveEmotionInfo(CurrentEmotions[i]);
                        AddEmotion(EColor.Yellow);
                    }
                    return;
                }
            }
        }
        else if (C == EColor.LRed)
        {
            for (int i = 0; i < CurrentEmotions.Count; i++)
            {
                if (CurrentEmotions[i].color == EColor.LYellow)
                {
                    ReduceEmotion(CurrentEmotions[i]);
                    AddEmotion(EColor.LOrange);
                    AddEmotion(EColor.LOrange);
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.LBlue)
                {
                    ReduceEmotion(CurrentEmotions[i]);
                    AddEmotion(EColor.LPurple);
                    AddEmotion(EColor.LPurple);
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.LGreen)
                {
                    ReduceEmotion(CurrentEmotions[i]);
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.LRed)
                {
                    if (CurrentEmotions[i].Level < 3)
                        CurrentEmotions[i].Level += 1;
                    else
                    {
                        InfoDetail.RemoveEmotionInfo(CurrentEmotions[i]);
                        AddEmotion(EColor.Red);
                    }
                    return;
                }
            }
        }
        else if (C == EColor.LBlue)
        {
            for (int i = 0; i < CurrentEmotions.Count; i++)
            {
                if (CurrentEmotions[i].color == EColor.LYellow)
                {
                    ReduceEmotion(CurrentEmotions[i]);
                    AddEmotion(EColor.LGreen);
                    AddEmotion(EColor.LGreen);
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.LRed)
                {
                    ReduceEmotion(CurrentEmotions[i]);
                    AddEmotion(EColor.LPurple);
                    AddEmotion(EColor.LPurple);
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.LOrange)
                {
                    ReduceEmotion(CurrentEmotions[i]);
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.LBlue)
                {
                    if (CurrentEmotions[i].Level < 3)
                        CurrentEmotions[i].Level += 1;
                    else
                    {
                        InfoDetail.RemoveEmotionInfo(CurrentEmotions[i]);
                        AddEmotion(EColor.Blue);
                    }
                    return;
                }
            }
        }
        else if (C == EColor.LOrange)
        {
            for (int i = 0; i < CurrentEmotions.Count; i++)
            {
                if (CurrentEmotions[i].color == EColor.LOrange)
                {
                    if (CurrentEmotions[i].Level < 3)
                        CurrentEmotions[i].Level += 1;
                    else
                    {
                        InfoDetail.RemoveEmotionInfo(CurrentEmotions[i]);
                        AddEmotion(EColor.Orange);
                    }
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.LBlue)
                {
                    ReduceEmotion(CurrentEmotions[i]);
                    return;
                }
            }
        }
        else if (C == EColor.LPurple)
        {
            for (int i = 0; i < CurrentEmotions.Count; i++)
            {
                if (CurrentEmotions[i].color == EColor.LPurple)
                {
                    if (CurrentEmotions[i].Level < 3)
                        CurrentEmotions[i].Level += 1;
                    else
                    {
                        InfoDetail.RemoveEmotionInfo(CurrentEmotions[i]);
                        AddEmotion(EColor.Purple);
                    }
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.LYellow)
                {
                    ReduceEmotion(CurrentEmotions[i]);
                    return;
                }
            }
        }
        else if (C == EColor.LGreen)
        {
            for (int i = 0; i < CurrentEmotions.Count; i++)
            {
                if (CurrentEmotions[i].color == EColor.LGreen)
                {
                    if (CurrentEmotions[i].Level < 3)
                        CurrentEmotions[i].Level += 1;
                    else
                    {
                        InfoDetail.RemoveEmotionInfo(CurrentEmotions[i]);
                        AddEmotion(EColor.Green);
                    }
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.LRed)
                {
                    ReduceEmotion(CurrentEmotions[i]);
                    return;
                }
            }
        }
        else if (C == EColor.Yellow || C == EColor.Red || C == EColor.Blue || C == EColor.Orange || C == EColor.Purple || C == EColor.Green)
        {
            InfoDetail.SetMainEmotion(C);
            return;
        }
        InfoDetail.AddEmotionInfo(new Emotion(C));

    }
    public void RemoveEmotion(EColor C)
    {
        for(int i = 0; i < CurrentEmotions.Count; i++)
        {
            if(CurrentEmotions[i].color == C)
            {
                ReduceEmotion(CurrentEmotions[i]);
                return;
            }
        }
    }
    //情绪减层数
    void ReduceEmotion(Emotion E)
    {
        E.Level -= 1;
        if (E.Level <= 0)
            InfoDetail.RemoveEmotionInfo(E);
    }

    //心力爆炸
    public void Exhausted()
    {
        if (isCEO == false)
        {
            System.Action AgreeAction = () =>
            {
                MoneyRequest();
            };
            System.Action RefuseAction = () =>
            {
                InfoDetail.Fire(false);
            };
            QuestControl.Instance.Init(Name + "心力爆炸产生了崩溃，向您索赔金钱500，如果不接受，该员工将会离职", AgreeAction, RefuseAction);
        }
        Mentality += 50;
        if (ExhaustedCount.Count >= 4)
        {
            InfoDetail.GC.CreateMessage(Name + "再次心力爆炸但是没有任何效果");
            InfoDetail.AddHistory(Name + "再次心力爆炸但是没有任何效果");
            return;
        }
        int num = 0;
        List<int> PosbNum = new List<int> { 93, 94, 95, 96 };
        foreach(int n in ExhaustedCount)
        {
            if (PosbNum.Contains(n) == true)
            {
                PosbNum.Remove(n);
            }
        }
        num = PosbNum[Random.Range(0, PosbNum.Count)];
        if (num == 93)
            InfoDetail.AddPerk(new Perk93(this), true);
        else if (num == 94)
            InfoDetail.AddPerk(new Perk94(this), true);
        else if (num == 95)
            InfoDetail.AddPerk(new Perk95(this), true);
        else if (num == 96)
            InfoDetail.AddPerk(new Perk96(this), true);
        ExhaustedCount.Add(num);
        InfoDetail.GC.CreateMessage(Name + "心力爆炸,获得了" + InfoDetail.PerksInfo[InfoDetail.PerksInfo.Count - 1].CurrentPerk.Name);
        InfoDetail.AddHistory(Name + "心力爆炸,获得了" + InfoDetail.PerksInfo[InfoDetail.PerksInfo.Count - 1].CurrentPerk.Name);
    }

    //心力爆炸金钱需求
    public void MoneyRequest()
    {
        if(GameControl.Instance.Money >= 500)
        {
            GameControl.Instance.Money -= 500;
        }
        else
        {
            System.Action AgreeAction = () =>
            {
                MoneyRequest();
            };
            System.Action RefuseAction = () =>
            {
                InfoDetail.Fire(false);
            };
            QuestControl.Instance.Init(Name + "心力爆炸产生了崩溃，向您索赔金钱500，如果不接受，该员工将会离职", AgreeAction, RefuseAction);
            GameControl.Instance.CreateMessage("金钱不足!");
        }
    }

    public void DestroyAllInfos()
    {
        if (InfoA != null)
            MonoBehaviour.Destroy(InfoA.gameObject);
        if(InfoB != null)
            MonoBehaviour.Destroy(InfoB.gameObject);
        if (InfoDetail != null)
            MonoBehaviour.Destroy(InfoDetail.gameObject);
    }
}

public class EmpBuff
{
    //目前用于技能经验缓慢增长和减体力上限两个buff
    public int Type = 0;
    public int Time = 0;
    public int Value = 3;
    Employee Target;
    public EmpBuff(Employee emp, int type, int value = 3, int time = 128)
    {
        Target = emp;
        Target.CurrentBuffs.Add(this);
        Type = type;
        Time = time;
        Value = value;
        if (type == 16)
        {
            Target.StaminaLimitExtra += value;
            Target.Stamina += 0;
        }
    }
}

static public class CNName
{
    static List<string> LastName = new List<string>() { "赵", "钱", "孙", "李", "周", "吴", "郑", "王", "冯", "陈", "楮", "卫", "蒋",
       "沈", "韩", "杨","朱", "秦", "尤", "许", "何", "吕", "施", "张", "孔", "曹", "严", "华", "金", "魏", "陶", "姜","戚", "谢",
       "邹", "喻", "柏", "水", "窦", "章", "云", "苏", "潘", "葛", "奚", "范", "彭" };

    static List<string> FirstName = new List<string>() { "子", "安", "建", "天", "小", "德", "文", "嘉", "浩", "昊", "博", "雅", "民",
        "轩", "悦", "文", "楠", "琪", "丽", "翠", "柏", "桐", "蕾", "谷", "灵", "友", "国"};

    static public string RandomName()
    {
        string newName = LastName[Random.Range(0, LastName.Count)];
        newName += FirstName[Random.Range(0, FirstName.Count)];
        if (Random.Range(0,2) == 0)
            newName += FirstName[Random.Range(0, FirstName.Count)];
        return newName;
    }
}
