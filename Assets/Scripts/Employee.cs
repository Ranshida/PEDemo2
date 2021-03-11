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
    static public int AttributeLimit = 10;

    //1.技术 2.市场 3.产品 4.Ob观察 5.Te坚韧 6.Str强壮 7.Ma管理 8.HR人力 9.Fi财务 10.De决策 
    //11.Fo行业 12.St谋略 13.Co说服 14.Ch魅力 15.Go八卦 16.Ad行政
    #region 所有属性

    public int Tech
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
    public int Market
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
    public int Product
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
    public int Decision //!!决策的技能上限与其他技能不同
    {
        get
        {
            int Num = BaseAttributes[9] + ExtraAttributes[9];
            if (Num > 5)
                Num = 5;
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
    public int Admin
    {
        get
        {
            int Num = BaseAttributes[15] + ExtraAttributes[15];
            if (Num > AttributeLimit)
                Num = AttributeLimit;
            return Num;
        }
        set { BaseAttributes[15] = value; }
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
            //低心力的效果
            if (InfoDetail != null)
            {
                if (mentality >= 20 && value < 20)
                {
                    InfoDetail.AddPerk(new Perk119(this));

                    if(CurrentDep == null || GameControl.Instance.SC.FightStart == true)
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
            if (mentality > MentalityLimit)
                mentality = MentalityLimit;
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
    public int SkillPoint = 0; //当前可分配的技能点
    public int SchoolType, MajorType;//初始学校和专业类型
    public int ExtraExp = 0;//升级所需的额外经验
    #endregion

    public int SalaryExtra = 0, Age, EventTime, ObeyTime, NoPromotionTime = 0, NoMarriageTime = 0,
        VacationTime = 0, SpyTime = 0, CoreMemberTime;//放假时间、间谍时间、核心成员说服CD时间
    public int Confidence;//信心，头脑风暴中的护盾
    public int SkillLimitTime;//头脑风暴中技能禁用的回合数
    public int NewRelationTargetTime = 1;
    public float ExtraSuccessRate = 0, SalaryMultiple = 1.0f;


    public int[] BaseAttributes = new int[16]; 
    public int[] ExtraAttributes = new int[16];//基础和额外属性的数组存储,便于一些情况下的查找和使用
    public int[] Professions = new int[3];//用于保存员工的三个行业技能的类型
    public int[] StarLimit = new int[16];
    public int[] SkillExp = new int[16];
    public int[] Levels = new int[5]; //0职业(业务) 1基础 2经营 3战略 4社交 (已过时)
    public int[] CharacterTendency = new int[4];//(0文化 -独裁 +开源) (1信仰-机械 +人文) (2道德-功利主义 +绝对律令) (3行事-随心所欲 +严格守序)
    public int[] CurrentSkillType = new int[3];
    public float[] Character = new float[5]; //0文化 1信仰 2道德 3行事 4信念

    public string Name;
    public bool isCEO = false, SupportCEO;

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
    public void InitStatus()
    {
        int[] Nst = {1, 2, 3, 8, 9, 11, 12, 13, 15, 16};//Nst:几个专业技能对应的编号
        int r1 = Random.Range(0, 10), r2 = Random.Range(0, 10), r3 = Random.Range(0, 10);
        while (r1 == r2)
        {
            r2 = Random.Range(0, 10);
        }
        while (r3 == r2 || r3 == r1)
            r3 = Random.Range(0, 10);

        r1 = Nst[r1];
        r2 = Nst[r2];
        r3 = Nst[r3];
        Professions[0] = r1;
        Professions[1] = r2;
        Professions[2] = r3;

        float YearPosb = Random.Range(0.0f, 1.0f);
        int WorkYear = 0;
        if (YearPosb < 0.6f)
            WorkYear = Random.Range(0, 3);
        else if (YearPosb < 0.9f)
            WorkYear = Random.Range(3, 8);
        else
            WorkYear = Random.Range(8, 15);

        Age = 20 + WorkYear;

        //根据工龄随机技能
        if(WorkYear <= 2)
        {
            SetAttributes(r1, WorkYear);
            SkillPoint = WorkYear * 2;
        }
        else if (WorkYear < 7)
        {
            if(Random.Range(1, 11) <= 2)
            {
                SetAttributes(r1, WorkYear);
            }
            else
            {
                int SkillAValue = Random.Range(1, WorkYear + 1);
                SetAttributes(r1, SkillAValue);
                SetAttributes(r2, WorkYear - SkillAValue);
            }
            SkillPoint = WorkYear + 3;
        }
        else
        {
            if (Random.Range(1, 11) <= 2)
            {
                SetAttributes(r1, WorkYear);
            }
            else if (Random.Range(1, 11) <= 2)
            {
                int SkillAValue = Random.Range(1, WorkYear + 1);
                SetAttributes(r1, SkillAValue);
                SetAttributes(r2, WorkYear - SkillAValue);
            }
            else
            {
                int SkillAValue = Random.Range(1, WorkYear + 1);
                int SkillBValue = Random.Range(1, WorkYear - SkillAValue);
                SetAttributes(r1, SkillAValue);
                SetAttributes(r2, SkillBValue);
                SetAttributes(r3, WorkYear - SkillAValue - SkillBValue);
            }
        }

        //随机天赋
        for(int i = 0; i < 3; i++)
        {
            int type = 0;
            if (i == 0)
                type = r1;
            else if (i == 1)
                type = r2;
            else
                type = r3;
            float Posb = Random.Range(0.0f, 1.0f);
            if (Posb < 0.4f)
                StarLimit[type - 1] = 0;
            else if (Posb < 0.7f)
                StarLimit[type - 1] = 1;
            else if (Posb < 0.9f)
                StarLimit[type - 1] = 2;
            else 
                StarLimit[type - 1] = 3;
        }


        //随机出身

        //1.随机学校
        SchoolType = Random.Range(1, 4);
        if (SchoolType == 1)
            SetAttributes(r1, BaseAttributes[r1 - 1] + 1);
        else if (SchoolType == 3)
        {
            if (BaseAttributes[r1 - 1] > 0)
                SetAttributes(r1, BaseAttributes[r1 - 1] - 1);
            Tenacity -= 2;
        }

        //2.随机专业
        MajorType = Random.Range(0, 10);
        MajorType = Nst[MajorType];
        if(r1 == MajorType)
            SetAttributes(r1, BaseAttributes[r1 - 1] + 2);
        else if (r2 == MajorType)
            SetAttributes(r2, BaseAttributes[r2 - 1] + 2);
        else if (r3 == MajorType)
            SetAttributes(r3, BaseAttributes[r3 - 1] + 2);


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
        Tenacity = 3;
        Strength = 3;
        SkillPoint = 7;

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

        //再次随机剩下的两个专业技能类型并设为0级
        int[] Nst = { 1, 2, 3, 8, 9, 11, 12, 13, 15, 16 };//Nst:几个专业技能对应的编号
        Professions[0] = AdjustData.CEOProfessionType;
        SetAttributes(Professions[0], AdjustData.CEOProfessionValue);
        int r1 = Professions[0], r2 = Random.Range(0, 10), r3 = Random.Range(0, 10);
        r2 = Nst[r2];
        r3 = Nst[r3];
        while (r1 == r2)
        {
            r2 = Random.Range(0, 10);
            r2 = Nst[r2];
        }
        while (r3 == r2 || r3 == r1)
        {
            r3 = Random.Range(0, 10);
            r3 = Nst[r3];
        }
        Professions[1] = r2;
        Professions[2] = r3;
        SetAttributes(r2, 0);
        SetAttributes(r3, 0);

        //随机天赋
        for (int j = 0; j < 3; j++)
        {
            int type = 0;
            if (j == 0)
                type = r1;
            else if (j == 1)
                type = r2;
            else
                type = r3;
            float Posb = Random.Range(0.0f, 1.0f);
            if (Posb < 0.4f)
                StarLimit[type - 1] = 0;
            else if (Posb < 0.7f)
                StarLimit[type - 1] = 1;
            else if (Posb < 0.9f)
                StarLimit[type - 1] = 2;
            else
                StarLimit[type - 1] = 3;
        }


        //随机学校
        SchoolType = 2;

        //随机专业
        MajorType = Random.Range(0, 10);
        MajorType = Nst[MajorType];

        Stamina = StaminaLimit;
        Mentality = MentalityLimit;
    }

    //随机分配已有技能点
    public void RandomSkillAssign()
    {
        Tenacity = 3;
        Strength = 3;
        Manage = 0;
        Decision = 0;
        for (int i = 0; i < SkillPoint; i++)
        {
            int type = Random.Range(0, 4);
            if (type == 0)
                Tenacity += 1;
            else if (type == 1)
                Strength += 1;
            else if (type == 2)
                Manage += 1;
            else
                Decision += 1;
        }
        SkillPoint = 0;
        Stamina = StaminaLimit;
        Mentality = MentalityLimit;
    }

    public void GainExp(int value, int type)
    {
        //1-3职业技能 4观察 5坚韧 6强壮 7管理 8人力 9财务 10决策 11行业 12谋略 13说服 14魅力 15八卦
        int SkillLevel = 0;
        #region 设定等级
        if (type == 1)
            SkillLevel = Tech;
        else if (type == 2)
            SkillLevel = Market;
        else if (type == 3)
            SkillLevel = Product;
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

        SkillExp[type - 1] += value;

        //达到上限后不继续判断
        if (SkillLevel >= AttributeLimit)
            return;
        if (SkillExp[type - 1] >= AdjustData.ExpLimit[SkillLevel])
        {
            InfoDetail.AddPerk(new Perk58(this), true);
            SkillExp[type - 1] = 0;
            string SkillName;
            if (type == 1)
            {
                Tech += 1;
                SkillName = "技术技能";
            }
            else if (type == 2)
            { 
                Market += 1;
                SkillName = "市场技能";
            }
            else if (type == 3)
            { 
                Product += 1;
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
            else if (type == 15)
            { 
                Gossip += 1;
                SkillName = "八卦";
            }
            else
            {
                Admin += 1;
                SkillName = "行政";
            }
            InfoDetail.GC.CreateMessage(Name + "的 " + SkillName + " 技能提升了");
            BottleNeckCheck();
            SkillPoint += 1;
            if (InfoDetail != null)
                InfoDetail.CheckMarker();
        }
    }

    public void BottleNeckCheck()
    {
        int pValue = BaseAttributes[Professions[0] - 1] + BaseAttributes[Professions[1] - 1] + BaseAttributes[Professions[2] - 1];
        if (InfoDetail == null)
        {
            Debug.Log("无InfoDetail时获取经验");
            return;
        }
        if (pValue >= 5 && ExtraExp < AdjustData.BottleneckValue)
            InfoDetail.AddPerk(new Perk142(this));
        if (pValue >= 10 && ExtraExp < AdjustData.BottleneckValue * 2)
            InfoDetail.AddPerk(new Perk142(this));
        if (pValue >= 15 && ExtraExp < AdjustData.BottleneckValue * 3)
            InfoDetail.AddPerk(new Perk142(this));
        if (pValue >= 20 && ExtraExp < AdjustData.BottleneckValue * 4)
            InfoDetail.AddPerk(new Perk142(this));
        if (pValue >= 25 && ExtraExp < AdjustData.BottleneckValue * 5)
            InfoDetail.AddPerk(new Perk142(this));
        InfoDetail.SkillPointCheck();
    }

    //根据编号设置属性
    public void SetAttributes(int type, int value)
    {    //1.技术 2.市场 3.产品 4.Ob观察 5.Te坚韧 6.Str强壮 7.Ma管理 8.HR人力 9.Fi财务 10.De决策 
         //11.Fo行业 12.St谋略 13.Co说服 14.Ch魅力 15.Go八卦 16.Ad行政
        if (type == 1)
            Tech = value;
        else if (type == 2)
            Market = value;
        else if (type == 3)
            Product = value;
        else if (type == 4)
            Observation = value;
        else if (type == 5)
            Tenacity = value;
        else if (type == 6)
            Strength = value;
        else if (type == 7)
            Manage = value;
        else if (type == 8)
            HR = value;
        else if (type == 9)
            Finance = value;
        else if (type == 10)
            Decision = value;
        else if (type == 11)
            Forecast = value;
        else if (type == 12)
            Strategy = value;
        else if (type == 13)
            Convince = value;
        else if (type == 14)
            Charm = value;
        else if (type == 15)
            Gossip = value;
        else if (type == 16)
            Admin = value;
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
        EventTimePass();
        //假期计时
        if (VacationTime > 0)
        {
            VacationTime -= 1;
            Stamina += 2;
            if (VacationTime == 0)
                EndVacation();
        }
        //核心成员说服CD倒计时
        if (CoreMemberTime > 0)
            CoreMemberTime -= 1;
        //间谍时间倒计时
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
                    if (CurrentEmotions[i].Level < 2)
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
                    if (CurrentEmotions[i].Level < 2)
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
                    if (CurrentEmotions[i].Level < 2)
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
                    if (CurrentEmotions[i].Level < 2)
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
                    if (CurrentEmotions[i].Level < 2)
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
                    if (CurrentEmotions[i].Level < 2)
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
                    ReduceEmotion(CurrentEmotions[i]);
                    AddEmotion(EColor.Orange);
                    AddEmotion(EColor.Orange);
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.Blue)
                {
                    ReduceEmotion(CurrentEmotions[i]);
                    AddEmotion(EColor.Green);
                    AddEmotion(EColor.Green);
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
                    ReduceEmotion(CurrentEmotions[i]);
                    AddEmotion(EColor.Orange);
                    AddEmotion(EColor.Orange);
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.Blue)
                {
                    ReduceEmotion(CurrentEmotions[i]);
                    AddEmotion(EColor.Purple);
                    AddEmotion(EColor.Purple);
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
                    ReduceEmotion(CurrentEmotions[i]);
                    AddEmotion(EColor.Green);
                    AddEmotion(EColor.Green);
                    return;
                }
                else if (CurrentEmotions[i].color == EColor.Red)
                {
                    ReduceEmotion(CurrentEmotions[i]);
                    AddEmotion(EColor.Purple);
                    AddEmotion(EColor.Purple);
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
