using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EmpType
{
    Tech, Market, Product, Operate, Science, HR
}
public enum EColor
{//L开头为对应1级浅色情绪
    White, Gray, LYellow, LRed, LBlue, LOrange, LPurple, LGreen, Yellow, Red, Blue, Orange, Purple, Green, None
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
    static public int HeadHuntLevel = 11, AttributeLimit = 25;

    //1技术 2市场 3产品 Ob观察 Te坚韧 Str强壮 Ma管理 HR人力 Fi财务 De决策 Fo行业 St谋略 Co说服 Ch魅力 Go八卦
    #region 所有属性

    public int Skill1
    {
        get
        {
            int Num = BaseAttributes[0] + ExtraAttributes[0];
            if (Num > AttributeLimit)
                Num = AttributeLimit;
            return Num;
        }
        set { BaseAttributes[0] = value; }
    }
    public int Skill2
    {
        get
        {
            int Num = BaseAttributes[1] + ExtraAttributes[1];
            if (Num > AttributeLimit)
                Num = AttributeLimit;
            return Num;
        }
        set { BaseAttributes[1] = value; }
    }
    public int Skill3
    {
        get
        {
            int Num = BaseAttributes[2] + ExtraAttributes[2];
            if (Num > AttributeLimit)
                Num = AttributeLimit;
            return Num;
        }
        set { BaseAttributes[2] = value; }
    }
    public int Observation
    {
        get
        {
            int Num = BaseAttributes[3] + ExtraAttributes[3];
            if (Num > AttributeLimit)
                Num = AttributeLimit;
            return Num;
        }
        set { BaseAttributes[3] = value; }
    }
    public int Tenacity
    {
        get
        {
            int Num = BaseAttributes[4] + ExtraAttributes[4];
            if (Num > AttributeLimit)
                Num = AttributeLimit;
            return Num;
        }
        set { BaseAttributes[4] = value; }
    }
    public int Strength
    {
        get
        {
            int Num = BaseAttributes[5] + ExtraAttributes[5];
            if (Num > AttributeLimit)
                Num = AttributeLimit;
            return Num;
        }
        set { BaseAttributes[5] = value; }
    }
    public int Manage
    {
        get
        {
            int Num = BaseAttributes[6] + ExtraAttributes[6];
            if (Num > AttributeLimit)
                Num = AttributeLimit;
            return Num;
        }
        set { BaseAttributes[6] = value; }
    }
    public int HR
    {
        get
        {
            int Num = BaseAttributes[7] + ExtraAttributes[7];
            if (Num > AttributeLimit)
                Num = AttributeLimit;
            return Num;
        }
        set { BaseAttributes[7] = value; }
    }
    public int Finance
    {
        get
        {
            int Num = BaseAttributes[8] + ExtraAttributes[8];
            if (Num > AttributeLimit)
                Num = AttributeLimit;
            return Num;
        }
        set { BaseAttributes[8] = value; }
    }
    public int Decision
    {
        get
        {
            int Num = BaseAttributes[9] + ExtraAttributes[9];
            if (Num > AttributeLimit)
                Num = AttributeLimit;
            return Num;
        }
        set { BaseAttributes[9] = value; }
    }
    public int Forecast
    {
        get
        {
            int Num = BaseAttributes[10] + ExtraAttributes[10];
            if (Num > AttributeLimit)
                Num = AttributeLimit;
            return Num;
        }
        set { BaseAttributes[10] = value; }
    }
    public int Strategy
    {
        get
        {
            int Num = BaseAttributes[11] + ExtraAttributes[11];
            if (Num > AttributeLimit)
                Num = AttributeLimit;
            return Num;
        }
        set { BaseAttributes[11] = value; }
    }
    public int Convince
    {
        get
        {
            int Num = BaseAttributes[12] + ExtraAttributes[12];
            if (Num > AttributeLimit)
                Num = AttributeLimit;
            return Num;
        }
        set { BaseAttributes[12] = value; }
    }
    public int Charm
    {
        get
        {
            int Num = BaseAttributes[13] + ExtraAttributes[13];
            if (Num > AttributeLimit)
                Num = AttributeLimit;
            return Num;
        }
        set { BaseAttributes[13] = value; }
    }
    public int Gossip
    {
        get
        {
            int Num = BaseAttributes[14] + ExtraAttributes[14];
            if (Num > AttributeLimit)
                Num = AttributeLimit;
            return Num;
        }
        set { BaseAttributes[14] = value; }
    }
    //体力  心力
    public int Stamina
    {
        get { return stamina; }
        set
        {
            stamina = value;
            if (stamina > StaminaLimit)
                stamina = StaminaLimit;
            else if (stamina < 0)
                stamina = 0;
        }
    }
    public int Mentality
    {
        get { return mentality; }
        set
        {
            mentality = value;
            if (mentality > MentalityLimit)
                mentality = MentalityLimit;
            else if (mentality <= 0)
            {
                mentality = 0;
                Exhausted();
            }
            if (mentality < 50 && WantLeave == true)
                InfoA.Fire();
            if (isCEO == true)
                GameControl.Instance.Text_Mentality.text = "心力:" + mentality;
        }
    }
    public int StaminaLimit { get { return 100 + (Strength * 5) + StaminaLimitExtra; } set { StaminaLimit = value;} } //体力上限
    public int MentalityLimit { get { return 100 + (Tenacity * 5) + MentalityLimitExtra; } set { MentalityLimit = value; } } // 心力上限
    public int StaminaLimitExtra = 0; //体力上限额外值
    public int MentalityLimitExtra = 0; //心理上限额外值
    #endregion

    public int SkillExtra1, SkillExtra2, SkillExtra3, SalaryExtra = 0, Age, EventTime, ObeyTime, NoPromotionTime = 0, NoMarriageTime = 0,
        VacationTime = 0, SpyTime = 0, StarType = 0;
    public int Confidence;//信心，头脑风暴中的护盾
    public int NewRelationTargetTime = 1;
    public float ExtraSuccessRate = 0, SalaryMultiple = 1.0f;


    public int[] BaseAttributes = new int[15];
    public int[] ExtraAttributes = new int[15];
    public int[] Stars = new int[5];
    public int[] StarLimit = new int[5];
    public int[] SkillExp = new int[15];
    public int[] Levels = new int[5]; //0职业(业务) 1基础 2经营 3战略 4社交
    public int[] CharacterTendency = new int[4];//(0文化 -独裁 +开源) (1信仰-机械 +人文) (2道德-功利主义 +绝对律令) (3行事-随心所欲 +严格守序)
    public float[] Character = new float[5]; //0文化 1信仰 2道德 3行事 4信念
    public float[] Request = new float[4];
    public int[] BaseMotivation = new int[4];//0工作学习 1心体恢复 2谋划野心 3关系交往

    public string Name;
    public bool WantLeave = false, isCEO = false, canWork = true, SupportCEO, isSpy = false; //WantLeave没有在使用
    public EmpType Type;

    public EmpInfo InfoA, InfoB, InfoDetail;
    public DepControl CurrentDep;
    public OfficeControl CurrentOffice;
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
    public void InitStatus(EmpType type, int[] Hst, int AgeRange)
    {
        int[] Nst = new int[15];
        #region
        int r1 = Random.Range(0, 5), r2 = Random.Range(0, 5);
        while (r1 == r2)
        {
            r2 = Random.Range(0, 5);
        }
        int count1 = 4, count2 = 2;

        //HST = 0  (0,3)随机技能
        //HST = 1  (0,2)随机技能
        //HST = 2  (5,9)随机技能
        for (int i = 0; i < 6; i++)
        {
            int k = i;
            if (i < 3)
                i = r1 * 3 + i;
            else
                i = r2 * 3 + i - 3;

            if (count1 > 0 && count2 > 0)
            {
                int c = Random.Range(1, 3);
                if (c == 1)
                {
                    Nst[i] = 1;
                    count1 -= 1;
                }
                else
                {
                    Nst[i] = 2;
                    count2 -= 1;
                }
            }
            else if (count1 > 0)
            {
                Nst[i] = 1;
                count1 -= 1;
            }
            else
            {
                Nst[i] = 2;
                count2 -= 1;
            }
            i = k;
        }

        #endregion

        Type = type;

        //初始化技能树类型

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

        //职业技能
        if (Nst[0] == 0)
            Skill1 = Random.Range(0, 4);
        else if (Nst[0] == 1)
            Skill1 = Random.Range(0, 3);
        else if (Nst[0] == 2)
            Skill1 = Random.Range(5, 10);

        if (Nst[1] == 0)
            Skill2 = Random.Range(0, 4);
        else if (Nst[1] == 1)
            Skill2 = Random.Range(0, 3);
        else if (Nst[1] == 2)
            Skill2 = Random.Range(5, 10);

        if (Nst[2] == 0)
            Skill3 = Random.Range(0, 4);
        else if (Nst[2] == 1)
            Skill3 = Random.Range(0, 3);
        else if (Nst[2] == 2)
            Skill3 = Random.Range(5, 10);
        Levels[0] = Skill1 + Skill2 + Skill3;

        //基础技能
        if (Nst[3] == 0)
            Observation = Random.Range(0, 4);
        else if (Nst[3] == 1)
            Observation = Random.Range(0, 3);
        else if (Nst[3] == 2)
            Observation = Random.Range(5, 10);

        if (Nst[4] == 0)
            Tenacity = Random.Range(0, 4);
        else if (Nst[4] == 1)
            Tenacity = Random.Range(0, 3);
        else if (Nst[4] == 2)
            Tenacity = Random.Range(5, 10);

        if (Nst[5] == 0)
            Strength = Random.Range(0, 4);
        else if (Nst[5] == 1)
            Strength = Random.Range(0, 3);
        else if (Nst[5] == 2)
            Strength = Random.Range(5, 10);
        Levels[1] = Observation + Tenacity + Strength;

        //经营技能
        if (Nst[6] == 0)
            Manage = Random.Range(0, 4);
        else if (Nst[6] == 1)
            Manage = Random.Range(0, 3);
        else if (Nst[6] == 2)
            Manage = Random.Range(5, 10);

        if (Nst[7] == 0)
            HR = Random.Range(0, 4);
        else if (Nst[7] == 1)
            HR = Random.Range(0, 3);
        else if (Nst[7] == 2)
            HR = Random.Range(5, 10);

        if (Nst[8] == 0)
            Finance = Random.Range(0, 4);
        else if (Nst[8] == 1)
            Finance = Random.Range(0, 3);
        else if (Nst[8] == 2)
            Finance = Random.Range(5, 10);
        Levels[2] = Manage + HR + Finance;

        //战略技能
        if (Nst[9] == 0)
            Decision = Random.Range(0, 4);
        else if (Nst[9] == 1)
            Decision = Random.Range(0, 3);
        else if (Nst[9] == 2)
            Decision = Random.Range(5, 10);

        if (Nst[10] == 0)
            Forecast = Random.Range(0, 4);
        else if (Nst[10] == 1)
            Forecast = Random.Range(0, 3);
        else if (Nst[10] == 2)
            Forecast = Random.Range(5, 10);

        if (Nst[11] == 0)
            Strategy = Random.Range(0, 4);
        else if (Nst[11] == 1)
            Strategy = Random.Range(0, 3);
        else if (Nst[11] == 2)
            Strategy = Random.Range(5, 10);
        Levels[3] = Decision + Forecast + Strategy;

        //社交技能        
        if (Nst[12] == 0)
            Convince = Random.Range(0, 4);
        else if (Nst[12] == 1)
            Convince = Random.Range(0, 3);
        else if (Nst[12] == 2)
            Convince = Random.Range(5, 10);

        if (Nst[13] == 0)
            Charm = Random.Range(0, 4);
        else if (Nst[13] == 1)
            Charm = Random.Range(0, 3);
        else if (Nst[13] == 2)
            Charm = Random.Range(5, 10);

        if (Nst[14] == 0)
            Gossip = Random.Range(0, 4);
        else if (Nst[14] == 1)
            Gossip = Random.Range(0, 3);
        else if (Nst[14] == 2)
            Gossip = Random.Range(5, 10);
        Levels[4] = Convince + Charm + Gossip;

        //确定年龄
        if (AgeRange == 0)
            Age = Random.Range(20, 25);
        else
            Age = 25 + AgeRange * 5;

        InitStar();
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
        Skill1 = 5; Skill2 = 5; Skill3 = 5;
        Observation = 5; Tenacity = 5; Strength = 5; Manage = 9; HR = 5; Finance = 5; Decision = 5;
        Forecast = 5; Strategy = 5; Convince = 5; Charm = 5; Gossip = 5; Age = 25;
        Name = "CEO";

        //确定热情(Star)和天赋(StarLimit)
        InitStar();
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
        Stamina = StaminaLimit;
        Mentality = MentalityLimit;
    }

    public void InitStar(int type = -1)
    {
        float Posb = Random.Range(0.0f, 1.0f);
        int SNum1 = 0, SNum2 = Random.Range(0, 5);
        //确定特殊天赋
        if (type == -1)
            StarType = Random.Range(1, 14);
        else
            StarType = type;
        if (StarType == 1)
            SNum1 = 1;
        else if (StarType <= 4)
            SNum1 = 0;
        else if (StarType <= 6)
            SNum1 = 2;
        else if (StarType <= 8)
            SNum1 = 1;
        else if (StarType <= 10)
            SNum1 = 3;
        else if (StarType <= 13)
            SNum1 = 2;

        while (SNum1 == SNum2)
        {
            SNum2 = Random.Range(0, 5);
        }
        //根据随机的方案设置天赋
        if(Posb < 0.2f)
        {
            StarLimit[SNum1] = 5;
        }
        else if (Posb < 0.4f)
        {
            StarLimit[SNum1] = 3;
            StarLimit[SNum2] = 2;
        }
        else if (Posb < 0.7f)
        {
            for (int i = 0; i < 5; i++)
            {
                StarLimit[i] = 1;
            }
            StarLimit[SNum1] = 2;
        }
        else
        {
            StarLimit[SNum1] = 2;
            StarLimit[SNum2] = 1;
        }
    }

    public void GainExp(int value, int type)
    {
        //1-3职业技能 4观察 5坚韧 6强壮 7管理 8人力 9财务 10决策 11行业 12谋略 13说服 14魅力 15八卦
        int SNum = 0, SkillLevel = 0;
        if (type <= 3)
            SNum = 0;
        else if (type <= 6)
            SNum = 1;
        else if (type <= 9)
            SNum = 2;
        else if (type <= 12)
            SNum = 3;
        else
            SNum = 4;
        #region 设定等级
        if (type == 1)
            SkillLevel = Skill1;
        else if (type == 2)
            SkillLevel = Skill2;
        else if (type == 3)
            SkillLevel = Skill3;
        else if (type == 4)
            SkillLevel = Observation;
        else if (type == 5)
            SkillLevel = Tenacity;
        else if (type == 6)
            SkillLevel = Strength;
        else if (type == 7)
            SkillLevel = Manage;
        else if (type == 8)
            SkillLevel = HR;
        else if (type == 9)
            SkillLevel = Finance;
        else if (type == 10)
            SkillLevel = Decision;
        else if (type == 11)
            SkillLevel = Forecast;
        else if (type == 12)
            SkillLevel = Strategy;
        else if (type == 13)
            SkillLevel = Convince;
        else if (type == 14)
            SkillLevel = Charm;
        else if (type == 15)
            SkillLevel = Gossip;
        #endregion

        float AgePenalty = 0;
        if (Age > 23)
            AgePenalty = (Age - 23) * 0.05f;
        int ExtraValue = 0;
        if (Stars[SNum] == 1)
            ExtraValue = 1;
        else if (Stars[SNum] == 2)
            ExtraValue = 3;
        else if (Stars[SNum] == 3)
            ExtraValue = 6;
        else if (Stars[SNum] == 4)
            ExtraValue = 10;
        else if (Stars[SNum] >= 5)
            ExtraValue = 15;

        SkillExp[type - 1] += (value + ExtraValue);

        int StartExp = 50, ExtraExp = 50, ExtraLevel = 0;
        if(SkillLevel < 10)
        {
            StartExp = 50;
            ExtraExp = 50;
            ExtraLevel = 0;
        }
        else if (SkillLevel < 15)
        {
            StartExp = 500;
            ExtraExp = 100;
            ExtraLevel = 10;
        }
        else if (SkillLevel < 20)
        {
            StartExp = 1000;
            ExtraExp = 200;
            ExtraLevel = 15;
        }
        else
        {
            StartExp = 2000;
            ExtraExp = 300;
            ExtraLevel = 20;
        }
        //旧等级提升条件
        //if (SkillExp[type - 1] >= StartExp + ((SkillLevel - ExtraLevel) * ExtraExp) && SkillLevel < 25)

        if (SkillExp[type - 1] >= (SkillLevel * 50) && SkillLevel < 25)
        {
            InfoDetail.AddPerk(new Perk58(this), true);
            SkillExp[type - 1] = 0;
            Levels[SNum] += 1;
            string SkillName;
            if (type == 1)
            {
                Skill1 += 1;
                SkillName = "技术技能";
            }
            else if (type == 2)
            { 
                Skill2 += 1;
                SkillName = "市场技能";
            }
            else if (type == 3)
            { 
                Skill3 += 1;
                SkillName = "产品技能3";
            }
            else if (type == 4)
            { 
                Observation += 1;
                SkillName = "观察";
            }
            else if (type == 5)
            { 
                Tenacity += 1;
                Mentality += 5;
                SkillName = "坚韧";
            }
            else if (type == 6)
            { 
                Strength += 1;
                Stamina += 5;
                SkillName = "强壮";
            }
            else if (type == 7)
            { 
                Manage += 1;
                SkillName = "管理";
                if (CurrentOffice != null)
                    CurrentOffice.CheckManage();
            }
            else if (type == 8)
            { 
                HR += 1;
                SkillName = "人力";
            }
            else if (type == 9)
            { 
                Finance += 1;
                SkillName = "财务";
            }
            else if (type == 10)
            { 
                Decision += 1;
                SkillName = "决策";
            }
            else if (type == 11)
            { 
                Forecast += 1;
                SkillName = "行业";
            }
            else if (type == 12)
            { 
                Strategy += 1;
                SkillName = "谋略";
            }
            else if (type == 13)
            { 
                Convince += 1;
                SkillName = "说服";
            }
            else if (type == 14)
            { 
                Charm += 1;
                SkillName = "魅力";
            }
            else
            { 
                Gossip += 1;
                SkillName = "八卦";
            }
            InfoDetail.GC.CreateMessage(Name + "的 " + SkillName + " 技能提升了");
            InfoDetail.ST.SkillCheck();
        }
    }

    public void EventCheck()
    {
        //Request[0] += (300 - stamina - mentality - InfoDetail.GC.Morale) * 0.05f;
        //Request[1] += Mathf.Abs(Character[0]) + Mathf.Abs(Character[4]);
        //Request[2] += Mathf.Abs(Character[2]) + Mathf.Abs(Character[3]) + Mathf.Abs(Character[4]);
        //Request[3] += Mathf.Abs(Character[1]) + Mathf.Abs(Character[3]) + Mathf.Abs(Character[4]);
        //for(int i = 0; i < 4; i++)
        //{
        //    if(Request[i] >= 100)
        //    {
        //        InfoDetail.Entity.AddEvent(i + 1);
        //        Request[i] = 0;
        //        break;
        //    }
        //}
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

    EmpEntity RandomEventTarget()
    {
        EmpEntity E = null;
        List<Employee> EL = new List<Employee>();
        for(int i = 0; i < RelationTargets.Count; i++)
        {
            EL.Add(RelationTargets[i]);
        }
        if (CurrentDep != null)
        {
            for (int i = 0; i < CurrentDep.CurrentEmps.Count; i++)
            {
                if (CurrentDep.CurrentEmps[i] != this)
                    EL.Add(CurrentDep.CurrentEmps[i]);
            }
            if (CurrentDep.CommandingOffice != null && CurrentDep.CommandingOffice.CurrentManager != null)
                EL.Add(CurrentDep.CommandingOffice.CurrentManager);
        }
        else if (CurrentOffice != null && CurrentOffice.CommandingOffice != null && CurrentOffice.CommandingOffice.CurrentManager != null)
            EL.Add(CurrentOffice.CommandingOffice.CurrentManager);
        if (EL.Count > 0)
            E = EL[Random.Range(0, EL.Count)].InfoDetail.Entity;
        return E;
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

    public int CheckMotivation(int type)
    {
        //0工作学习 1心体恢复 2谋划野心 3关系交往
        int value = 0;
        if(type == 0)
        {
            if (CurrentDep != null)
            {
                //同事亦朋友&同事即仇敌
                for (int i = 0; i < CurrentDep.CurrentEmps.Count; i++)
                {
                    if (CurrentDep.CurrentEmps[i] == this)
                        continue;
                    else
                    {
                        int num = FindRelation(CurrentDep.CurrentEmps[i]).FriendValue;
                        if (num < 0)
                            value -= 10;
                        else if (num > 0)
                            value += 10;
                    }
                }
            }
            //严格守序偏好
            if (Character[3] > 0)
                value += (int)(Character[3] * 0.25f);
            //开源文化倾向
            if (Character[1] > 0)
                value += (int)(Character[1] * 0.25f);
            value += BaseMotivation[0];
        }
        else if (type == 1)
        {
            //心力体力数值
            if (Mentality < 20)
                value += 50;
            else if (Mentality < 40)
                value += 15;
            if (Stamina < 20)
                value += 20;
            else if (Stamina < 40)
                value += 10;
            //独裁文化倾向
            if (Character[0] < 0)
                value += (int)(Mathf.Abs(Character[0]) * 0.25f);
            //严格守序偏好
            if (Character[3] > 0)
                value += (int)(Character[3] * 0.25f);
            value += BaseMotivation[1];
        }
        else if (type == 2)
        {
            //和上级关系
            if (CurrentDep != null && CurrentDep.CommandingOffice != null && CurrentDep.CommandingOffice.CurrentManager != null)
            {
                int num = FindRelation(CurrentDep.CommandingOffice.CurrentManager).FriendValue;
                if (num == -2)
                    value += 15;
                else if (num == -1)
                    value += 5;
            }
            //随心所欲偏好
            if (Character[3] < 0)
                value += (int)(Mathf.Abs(Character[3]) * 0.25f);
            //低于平均工资
            if (isCEO == false && InfoDetail.CalcSalary() < InfoDetail.GC.Salary / (InfoDetail.GC.CurrentEmployees.Count - 1))
                value += 10;
            //一年之内无晋升-----------
            if (NoPromotionTime >= 384)
                value += 10;
            //独裁文化倾向
            if (Character[0] < 0)
                value += (int)(Mathf.Abs(Character[0]) * 0.25f);
            value += BaseMotivation[2];
        }
        else if (type == 3)
        {
            //开源文化倾向
            if (Character[0] > 0)
                value += (int)(Character[0] * 0.25f);
            //没有朋友
            if (Relations.Count == 0)
                value += 10;
            for (int i = 0; i < Relations.Count; i++)
            {
                if (Relations[i].FriendValue > 0)
                    break;
                if (i == Relations.Count - 1)
                    value += 10;
            }
            //办公室中独自一人
            if (CurrentDep != null && CurrentDep.CurrentEmps.Count == 1)
                value += 25;
            else if (CurrentOffice != null)
                value += 25;
            //随心所以偏好
            if (Character[3] < 0)
                value += (int)(Mathf.Abs(Character[3]) * 0.25f);
            //单身5年以上
            if (NoMarriageTime >= 1920)
                value += 15;
            value += BaseMotivation[3];
        }

        return value;
    }
    public string CheckMotivationContent(int type)
    {
        //0工作学习 1心体恢复 2谋划野心 3关系交往
        string Content = "";
        int value = 0;
        if (type == 0)
        {
            //同事亦朋友&同事即仇敌
            if (CurrentDep != null)
            {
                int ValueA = 0, ValueB = 0;
                for (int i = 0; i < CurrentDep.CurrentEmps.Count; i++)
                {
                    if (CurrentDep.CurrentEmps[i] == this)
                        continue;
                    else
                    {
                        int num = FindRelation(CurrentDep.CurrentEmps[i]).FriendValue;
                        if (num < 0)
                        {
                            value -= 10;
                            ValueA += 10;
                        }
                        else if (num > 0)
                        {
                            value += 10;
                            ValueB += 10;
                        }
                    }
                }
                if (ValueA > 0)
                    Content += "  同事即仇敌+" + ValueA;
                if (ValueB > 0)
                    Content += "  同事亦朋友+" + ValueB;
            }
            //严格守序偏好
            if (Character[3] > 0)
            {
                value += (int)(Character[3] * 0.25f);
                Content += "  严格守序偏好+" + (int)(Character[3] * 0.25f);
            }
            //开源文化倾向
            if (Character[1] > 0)
            { 
                value += (int)(Character[1] * 0.25f);
                Content += "  开源文化倾向+" + (int)(Character[1] * 0.25f);
            }
            for(int i = 0; i < InfoDetail.PerksInfo.Count; i++)
            {
                if (InfoDetail.PerksInfo[i].CurrentPerk.Num == 5)
                    Content += "  奋斗逼+15";
                else if (InfoDetail.PerksInfo[i].CurrentPerk.Num == 6)
                    Content += "  欧洲人-15";
                else if (InfoDetail.PerksInfo[i].CurrentPerk.Num == 7)
                    Content += "  抑郁-20";
            }
            value += BaseMotivation[0];
            Content = "工作学习场景:" + value + "\n" + Content;
        }
        else if (type == 1)
        {
            //心力体力数值
            if (Mentality < 20)
            {
                value += 50;
                Content += "  心力小于20 +50";
            }
            else if (Mentality < 40)
            { 
                value += 15;
                Content += "  心力小于40 +15";
            }
            if (Stamina < 20)
            { 
                value += 20;
                Content += "  体力小于20 +20";
            }
            else if (Stamina < 40)
            { 
                value += 10;
                Content += "  体力小于40 +10";
            }
            //独裁文化倾向
            if (Character[0] < 0)
            {
                value += (int)(Mathf.Abs(Character[0]) * 0.25f);
                Content += "  独裁文化倾向+" + (int)(Mathf.Abs(Character[0]) * 0.25f);
            }
            //严格守序偏好
            if (Character[3] > 0)
            {
                value += (int)(Character[3] * 0.25f);
                Content += "  严格守序偏好+" + (int)(Character[3] * 0.25f);
            }
            for (int i = 0; i < InfoDetail.PerksInfo.Count; i++)
            {
                if (InfoDetail.PerksInfo[i].CurrentPerk.Num == 9)
                    Content += "  元气满满+15";
                else if (InfoDetail.PerksInfo[i].CurrentPerk.Num == 10)
                    Content += "  狂热-15";
            }
            value += BaseMotivation[1];
            Content = "心体恢复场景:" + value + "\n" + Content;
        }
        else if (type == 2)
        {
            //和上级关系
            if (CurrentDep != null && CurrentDep.CommandingOffice != null && CurrentDep.CommandingOffice.CurrentManager != null)
            {
                int num = FindRelation(CurrentDep.CommandingOffice.CurrentManager).FriendValue;
                if (num == -2)
                {
                    value += 15;
                    Content += "  与上级关系仇人+15";
                }
                else if (num == -1)
                {
                    value += 5;
                    Content += "  与上级关系陌路+5";
                }
            }
            //随心所欲偏好
            if (Character[3] < 0)
            {
                value += (int)(Mathf.Abs(Character[3]) * 0.25f);
                Content += "  随心所欲偏好+" + (int)(Mathf.Abs(Character[3]) * 0.25f);
            }
            //低于平均工资
            if (isCEO == false && InfoDetail.CalcSalary() < InfoDetail.GC.Salary / (InfoDetail.GC.CurrentEmployees.Count - 1))
            {
                value += 10;
                Content += "低于平均工资+10";
            }
            //一年之内无晋升
            if (NoPromotionTime >= 384)
            {
                value += 10;
                Content += "  一年之内无晋升+10";
            }
            //独裁文化倾向
            if (Character[0] < 0)
            {
                value += (int)(Mathf.Abs(Character[0]) * 0.25f);
                Content += "  独裁文化倾向+" + (int)(Mathf.Abs(Character[0]) * 0.25f);
            }
            for (int i = 0; i < InfoDetail.PerksInfo.Count; i++)
            {
                if (InfoDetail.PerksInfo[i].CurrentPerk.Num == 12)
                    Content += "  鹰视狼顾+20";
                else if (InfoDetail.PerksInfo[i].CurrentPerk.Num == 13)
                    Content += "  平凡之路-15";
                else if (InfoDetail.PerksInfo[i].CurrentPerk.Num == 14)
                    Content += "  复仇者+15";
            }
            value += BaseMotivation[2];
            Content = "谋划野心场景:" + value + "\n" + Content;
        }
        else if (type == 3)
        {
            //开源文化倾向
            if (Character[0] > 0)
            {
                value += (int)(Character[0] * 0.25f);
                Content += "  开源文化倾向+" + (int)(Character[0] * 0.25f);
            }
            //没有朋友
            if(Relations.Count == 0)
            {
                value += 10;
                Content += "没有朋友+10";
            }
            for (int i = 0; i < Relations.Count; i++)
            {
                if (Relations[i].FriendValue > 0)
                    break;
                if (i == Relations.Count - 1)
                {
                    value += 10;
                    Content += "没有朋友+10";
                }
            }
            //办公室中独自一人
            if (CurrentDep != null && CurrentDep.CurrentEmps.Count == 1)
            {
                value += 25;
                Content += "  部门中独自一人+25";
            }
            else if (CurrentOffice != null)
            {
                value += 25;
                Content += "  办公室中独自一人+25";
            }
            //随心所以偏好
            if (Character[3] < 0)
            {
                value += (int)(Mathf.Abs(Character[3]) * 0.25f);
                Content += "  随心所欲偏好+" + (int)(Mathf.Abs(Character[3]) * 0.25f);
            }
            //单身5年以上
            if (NoMarriageTime >= 1920)
            {
                value += 15;
                Content += "单身5年以上+15";
            }
            for (int i = 0; i < InfoDetail.PerksInfo.Count; i++)
            {
                if (InfoDetail.PerksInfo[i].CurrentPerk.Num == 16)
                    Content += "  好色+15";
                else if (InfoDetail.PerksInfo[i].CurrentPerk.Num == 17)
                    Content += "  社交狂人+20";
                else if (InfoDetail.PerksInfo[i].CurrentPerk.Num == 22)
                    Content += "  友尽-10";
                else if (InfoDetail.PerksInfo[i].CurrentPerk.Num == 24)
                    Content += "  我恋爱了+25";
            }
            value += BaseMotivation[3];
            Content = "关系交往场景:" + value + "\n" + Content;
        }

        return Content;
    }

    public void AddEvent()
    {
        int value1 = CheckMotivation(0), value2 = CheckMotivation(1), value3 = CheckMotivation(2), value4 = CheckMotivation(3), Motiv = 0;
        int Posb = Random.Range(1, (value1 + value2 + value3 + value4));
        List<Event> EF = new List<Event>(), E = new List<Event>();

        if (Posb < value1)
        {
            EventData.CopyList(EF, EventData.StudyForceEvents);
            EventData.CopyList(E, EventData.StudyEvents);
            Motiv = value1;
            InfoDetail.Entity.ShowTips(2);
        }
        else if (Posb < value2)
        {
            EventData.CopyList(EF, EventData.RecoverForceEvent);
            EventData.CopyList(E, EventData.RecoverEvent);
            Motiv = value2;
            InfoDetail.Entity.ShowTips(3);
        }
        else if (Posb < value3)
        {
            EventData.CopyList(EF, EventData.AmbitionForceEvent);
            EventData.CopyList(E, EventData.AmbitionEvent);
            Motiv = value3;
            InfoDetail.Entity.ShowTips(1);
        }
        else
        {
            EventData.CopyList(EF, EventData.SocialForceEvent);
            EventData.CopyList(E, EventData.SocialEvent);
            Motiv = value4;
            InfoDetail.Entity.ShowTips(4);
        }
        //先检测排他事件
        while (EF.Count > 0)
        {
            Event e = EF[Random.Range(0, EF.Count)];
            e.Self = this;
            if (e.ConditionCheck(Motiv) == true)
            {
                InfoDetail.Entity.AddEvent(e);
                return;
            }
            else
            {
                EF.Remove(e);
            }
        }
        while (E.Count > 0)
        {
            Event e = E[Random.Range(0, E.Count)];
            e.Self = this;
            if (e.ConditionCheck(Motiv) == true)
            {
                InfoDetail.Entity.AddEvent(e);
                return;
            }
            else
            {
                E.Remove(e);
            }
        }
    }

    public void TimePass()
    {
        EventTimePass();
        //假期计时
        if (VacationTime > 0)
        {
            VacationTime -= 1;
            Stamina += 2;
            if (VacationTime == 0)
                EndVacation();
        }
        if (SpyTime > 0)
        {
            SpyTime -= 1;
            if (SpyTime == 0)
                InfoDetail.Entity.SetFree();
        }

        if (CurrentOffice == null)
            NoPromotionTime += 1;
        else if (CurrentOffice.building.Type != BuildingType.CEO办公室 || CurrentOffice.building.Type != BuildingType.高管办公室)
            NoPromotionTime += 1;

        if (Lover == null)
            NoMarriageTime += 1;
        else if (FindRelation(Lover).LoveValue < 3)
            NoMarriageTime += 1;

        //清除Items
        if (CurrentItems.Count > 0)
        {
            List<EmpItem> DesItems = new List<EmpItem>();
            for (int i = 0; i < CurrentItems.Count; i++)
            {
                if (CurrentItems[i].TimeLeft > 0)
                    CurrentItems[i].TimeLeft -= 1;
                if (CurrentItems[i].TimeLeft == 0)
                    DesItems.Add(CurrentItems[i]);
            }
            for(int i = 0; i < DesItems.Count; i++)
            {
                CurrentItems.Remove(DesItems[i]);
            }
            DesItems.Clear();
        }
        //计算经验加成
        if(CurrentBuffs.Count > 0)
        {
            List<EmpBuff> RemoveBonus = new List<EmpBuff>();
            foreach(EmpBuff buff in CurrentBuffs)
            {
                if (buff.Type < 16)
                    GainExp(buff.Value, buff.Type);
                buff.Time -= 1;
                if (buff.Time <= 0)
                    RemoveBonus.Add(buff);
            }
            foreach(EmpBuff buff in RemoveBonus)
            {
                if (buff.Type == 16)
                {
                    StaminaLimitExtra -= buff.Value;
                    Stamina += 0;
                }
                CurrentBuffs.Remove(buff);
            }
            RemoveBonus.Clear();
        }
    }
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

    void EndVacation()
    {
        VacationTime = 0;
        InfoDetail.Entity.SetFree();
        if (isCEO == true)
        {
            InfoDetail.GC.CC.SkillButton.interactable = true;
            foreach(DepControl dep in InfoDetail.GC.CurrentDeps)
            {
                dep.RemovePerk(115);
            }
        }
    }

    //增减情绪
    public void AddEmotion(EColor C)
    {
        //遇到麻木等状态抵消一次情绪添加
        for(int i = 0; i < InfoDetail.PerksInfo.Count; i++)
        {
            if ((C == EColor.LRed || C == EColor.Red) && InfoDetail.PerksInfo[i].CurrentPerk.Num == 96)
                return;
            else if(InfoDetail.PerksInfo[i].CurrentPerk.Num == 36)
            {
                InfoDetail.PerksInfo[i].CurrentPerk.RemoveEffect();
                return;
            }
        }
        if (C == EColor.LYellow)
        {
            for (int i = 0; i < CurrentEmotions.Count; i++)
            {
                if (CurrentEmotions[i].color == EColor.LRed)
                {
                    AddEmotion(EColor.LOrange);
                    AddEmotion(EColor.LOrange);
                    ReduceEmotion(CurrentEmotions[i]);
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.LBlue)
                {
                    AddEmotion(EColor.LGreen);
                    AddEmotion(EColor.LGreen);
                    ReduceEmotion(CurrentEmotions[i]);
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
                    AddEmotion(EColor.LOrange);
                    AddEmotion(EColor.LOrange);
                    ReduceEmotion(CurrentEmotions[i]);
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.LBlue)
                {
                    AddEmotion(EColor.LPurple);
                    AddEmotion(EColor.LPurple);
                    ReduceEmotion(CurrentEmotions[i]);
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
                    AddEmotion(EColor.LGreen);
                    AddEmotion(EColor.LGreen);
                    ReduceEmotion(CurrentEmotions[i]);
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.LRed)
                {
                    AddEmotion(EColor.LPurple);
                    AddEmotion(EColor.LPurple);
                    ReduceEmotion(CurrentEmotions[i]);
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
                if(CurrentEmotions[i].color == EColor.LOrange)
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
        else if (C == EColor.Yellow)
        {
            for (int i = 0; i < CurrentEmotions.Count; i++)
            {
                if (CurrentEmotions[i].color == EColor.Red)
                {
                    AddEmotion(EColor.Orange);
                    AddEmotion(EColor.Orange);
                    ReduceEmotion(CurrentEmotions[i]);
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.Blue)
                {
                    AddEmotion(EColor.Green);
                    AddEmotion(EColor.Green);
                    ReduceEmotion(CurrentEmotions[i]);
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.Purple)
                {
                    ReduceEmotion(CurrentEmotions[i]);
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.Yellow)
                {
                    CurrentEmotions[i].Level += 1;
                    return;
                }
            }
        }
        else if (C == EColor.Red)
        {
            for (int i = 0; i < CurrentEmotions.Count; i++)
            {
                if (CurrentEmotions[i].color == EColor.Yellow)
                {
                    AddEmotion(EColor.Orange);
                    AddEmotion(EColor.Orange);
                    ReduceEmotion(CurrentEmotions[i]);
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.Blue)
                {
                    AddEmotion(EColor.Purple);
                    AddEmotion(EColor.Purple);
                    ReduceEmotion(CurrentEmotions[i]);
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.Green)
                {
                    ReduceEmotion(CurrentEmotions[i]);
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.Red)
                {
                    CurrentEmotions[i].Level += 1;
                    return;
                }
            }
        }
        else if (C == EColor.Blue)
        {
            for (int i = 0; i < CurrentEmotions.Count; i++)
            {
                if (CurrentEmotions[i].color == EColor.Yellow)
                {
                    AddEmotion(EColor.Green);
                    AddEmotion(EColor.Green);
                    ReduceEmotion(CurrentEmotions[i]);
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.Red)
                {
                    AddEmotion(EColor.Purple);
                    AddEmotion(EColor.Purple);
                    ReduceEmotion(CurrentEmotions[i]);
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.Orange)
                {
                    ReduceEmotion(CurrentEmotions[i]);
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.Blue)
                {
                    CurrentEmotions[i].Level += 1;
                    return;
                }
            }
        }
        else if (C == EColor.Orange)
        {
            for (int i = 0; i < CurrentEmotions.Count; i++)
            {
                if (CurrentEmotions[i].color == EColor.Orange)
                {
                    CurrentEmotions[i].Level += 1;
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.Blue)
                {
                    ReduceEmotion(CurrentEmotions[i]);
                    return;
                }
            }
        }
        else if (C == EColor.Purple)
        {
            for (int i = 0; i < CurrentEmotions.Count; i++)
            {
                if (CurrentEmotions[i].color == EColor.Purple)
                {
                    CurrentEmotions[i].Level += 1;
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.Yellow)
                {
                    ReduceEmotion(CurrentEmotions[i]);
                    return;
                }
            }
        }
        else if (C == EColor.Green)
        {
            for (int i = 0; i < CurrentEmotions.Count; i++)
            {
                if (CurrentEmotions[i].color == EColor.Green)
                {
                    CurrentEmotions[i].Level += 1;
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.Red)
                {
                    ReduceEmotion(CurrentEmotions[i]);
                    return;
                }
            }
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
    //减层数
    void ReduceEmotion(Emotion E)
    {
        E.Level -= 1;
        if (E.Level <= 0)
            InfoDetail.RemoveEmotionInfo(E);
    }

    //根据已知目标确定一个事件
    public Event RandomEvent(Employee e)
    {
        Event NewEvent = null;
        List<Event> AddEvents = new List<Event>();
        List<Event> TempEvents = new List<Event>();
        List<Event> PosbEvents = new List<Event>();
        for (int i = 0; i < 4; i++)
        {
            if (i == 0)
                PosbEvents = EventData.E4;
            else if (i == 1)
                PosbEvents = EventData.E3;
            else if (i == 2)
                PosbEvents = EventData.E2;
            else
                PosbEvents = EventData.E1;
            for (int j = 0; j < PosbEvents.Count; j++)
            {
                PosbEvents[j].Self = this;
                PosbEvents[j].Target = e;
                if (PosbEvents[j].ConditionCheck(-1) == true)
                {
                    TempEvents.Add(PosbEvents[j].Clone());
                }
                PosbEvents[j].Self = null;
                PosbEvents[j].Target = null;
            }
            if (TempEvents.Count > 0)
            {
                int num = 5 - AddEvents.Count;
                if (TempEvents.Count < num)
                    num = TempEvents.Count;
                for (int j = 0; j < num; j++)
                {
                    AddEvents.Add(TempEvents[Random.Range(0, num)]);
                }
            }
            if (AddEvents.Count >= 5)
                break;
            TempEvents.Clear();
        }
        if (AddEvents.Count > 0)
        {
            float MaxPosb = 0, Posb = 0;
            if (AddEvents.Count == 1)
            {
                NewEvent = AddEvents[0];
                return NewEvent;
            }
            else if (AddEvents.Count == 2)
                MaxPosb = 0.7f;
            else if (AddEvents.Count == 3)
                MaxPosb = 0.8f;
            else if (AddEvents.Count == 4)
                MaxPosb = 0.9f;
            else if (AddEvents.Count == 5)
                MaxPosb = 1.0f;
            Posb = Random.Range(0.0f, MaxPosb);
            if (Posb < 0.5f)
                NewEvent = AddEvents[0];
            else if (Posb < 0.7f)
                NewEvent = AddEvents[1];
            else if (Posb < 0.8f)
                NewEvent = AddEvents[2];
            else if (Posb < 0.9f)
                NewEvent = AddEvents[3];
            else if (Posb < 1.0f)
                NewEvent = AddEvents[4];
        }
        //子事件检测
        if (NewEvent != null)
        {
            Event TempEvent = NewEvent.SubEventCheck();
            if (TempEvent != null)
                NewEvent = TempEvent.Clone();
        }
        if (NewEvent == null)
            MonoBehaviour.print("无合适事件");
        return NewEvent;
    }

    //寻找其他可用员工
    public List<Employee> FindAnotherEmp()
    {
        List<Employee> EL = new List<Employee>();
        if(CurrentOffice == null && CurrentDep == null)
        {
            for(int i = 0; i < GameControl.Instance.CurrentEmployees.Count; i++)
            {
                if (GameControl.Instance.CurrentEmployees[i].CurrentOffice == null && 
                    GameControl.Instance.CurrentEmployees[i].CurrentDep == null && GameControl.Instance.CurrentEmployees[i] != this)
                    EL.Add(GameControl.Instance.CurrentEmployees[i]);
            }
        }
        else if (CurrentDep != null)
        {
            for(int i = 0; i < CurrentDep.CurrentEmps.Count; i++)
            {
                if (CurrentDep.CurrentEmps[i] != this)
                    EL.Add(CurrentDep.CurrentEmps[i]);
            }
        }
        return EL;
    }

    //心力爆炸
    public void Exhausted()
    {
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
