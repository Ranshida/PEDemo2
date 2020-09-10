using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EmpType
{
    Tech, Market, Product, Operate, Science, HR
}

public class Employee
{
    static public int HeadHuntLevel = 11;
    public int Skill1, Skill2, Skill3, SkillExtra1, SkillExtra2, SkillExtra3, 
        Observation, Tenacity, Strength, Manage, HR, Finance, Decision, 
        Forecast, Strategy, Convince, Charm, Gossip, SalaryExtra = 0, Age;
    public int Stamina
    {
        get { return stamina; }
        set
        {
            stamina = value;
            if (stamina > 100)
                stamina = 100;
            else if (stamina < 0)
                stamina = 0;
        }
    }
    public int Mentality
    {
        get { return mentality; }
        set
        {
            if(mentality - value < 0)
            {
                InfoDetail.GC.StrC.SolveStrRequest(1, 1);
            }
            mentality = value;
            if (mentality > 100)
                mentality = 100;
            else if (mentality < 0)
                mentality = 0;
            if (mentality < 50 && WantLeave == true)
                InfoA.Fire();
        }
    }

    public int[] Stars = new int[5];
    public int[] StarLimit = new int[5];
    public int[] SkillExp = new int[15];
    public int[] Levels = new int[15];
    public float[] Character = new float[5]; //0文化 1信仰 2道德 3行事 4信念
    public float[] Request = new float[4];

    public string Name;
    public bool WantLeave = false;
    public EmpType Type;

    public EmpInfo InfoA, InfoB, InfoDetail;
    public DepControl CurrentDep;
    public OfficeControl CurrentOffice;
    public Employee Master, Lover;

    public List<Employee> Students = new List<Employee>();
    public List<Relation> Relations = new List<Relation>();

    int mentality, stamina;

    public void InitStatus(EmpType type, int[] Hst, int AgeRange)
    {
        Type = type;
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
        Stamina = 100;
        mentality = 100;

        //职业技能
        if (Hst[0] == 0)
        {
            if (AgeRange == 0)
                Skill1 = Random.Range(1, 6);
            else
                Skill1 = Random.Range(1, 4);
        }
        else
            Skill1 = HeadHuntLevel;

        if (Hst[1] == 0)
        {
            if (AgeRange == 0)
                Skill2 = Random.Range(1, 6);
            else
                Skill2 = Random.Range(1, 4);
        }
        else
            Skill2 = HeadHuntLevel;

        if (Hst[2] == 0)
        {
            if (AgeRange == 0)
                Skill3 = Random.Range(1, 6);
            else
                Skill3 = Random.Range(1, 4);
        }
        else
            Skill3 = HeadHuntLevel;
        Levels[0] = Skill1 + Skill2 + Skill3;
        //基础技能
        if (Hst[3] == 0)
        {
            if (AgeRange == 0)
                Observation = Random.Range(1, 6);
            else
                Observation = Random.Range(1, 4);
        }
        else
            Observation = HeadHuntLevel;

        if (Hst[4] == 0)
        {
            if (AgeRange == 0)
                Tenacity = Random.Range(1, 6);
            else
                Tenacity = Random.Range(1, 4);
        }
        else
            Tenacity = HeadHuntLevel;

        if (Hst[5] == 0)
        {
            if (AgeRange == 0)
                Strength = Random.Range(1, 6);
            else
                Strength = Random.Range(1, 4);
        }
        else
            Strength = HeadHuntLevel;
        Levels[1] = Observation + Tenacity + Strength;
        //经营技能
        if (Hst[6] == 0)
        {
            if (AgeRange == 0)
                Manage = Random.Range(1, 6);
            else
                Manage = Random.Range(1, 4);
        }
        else
            Manage = HeadHuntLevel;

        if (Hst[7] == 0)
        {
            if (AgeRange == 0)
                HR = Random.Range(1, 6);
            else
                HR = Random.Range(1, 4);
        }
        else
            HR = HeadHuntLevel;

        if (Hst[8] == 0)
        {
            if (AgeRange == 0)
                Finance = Random.Range(1, 6);
            else
                Finance = Random.Range(1, 4);
        }
        else
            Finance = HeadHuntLevel;
        Levels[2] = Manage + HR + Finance;
        //战略技能
        if (Hst[9] == 0)
        {
            if (AgeRange == 0)
                Decision = Random.Range(1, 6);
            else
                Decision = Random.Range(1, 4);
        }
        else
            Decision = HeadHuntLevel;

        if (Hst[10] == 0)
        {
            if (AgeRange == 0)
                Forecast = Random.Range(1, 6);
            else
                Forecast = Random.Range(1, 4);
        }
        else
            Forecast = HeadHuntLevel;

        if (Hst[11] == 0)
        {
            if (AgeRange == 0)
                Strategy = Random.Range(1, 6);
            else
                Strategy = Random.Range(1, 4);
        }
        else
            Strategy = HeadHuntLevel;
        Levels[3] = Decision + Forecast + Strategy;
        //社交技能        
        if (Hst[12] == 0)
        {
            if (AgeRange == 0)
                Convince = Random.Range(1, 6);
            else
                Convince = Random.Range(1, 4);
        }
        else
            Convince = HeadHuntLevel;

        if (Hst[13] == 0)
        {
            if (AgeRange == 0)
                Charm = Random.Range(1, 6);
            else
                Charm = Random.Range(1, 4);
        }
        else
            Charm = HeadHuntLevel;

        if (Hst[14] == 0)
        {
            if (AgeRange == 0)
                Gossip = Random.Range(1, 6);
            else
                Gossip = Random.Range(1, 4);
        }
        else
            Gossip = HeadHuntLevel;
        Levels[4] = Convince + Charm + Gossip;

        //确定年龄
        if (AgeRange == 0)
            Age = Random.Range(20, 25);
        else
            Age = 25 + AgeRange * 5;
        
        //确定热情(Star)和天赋(StarLimit)
        int TopStartNum = (Random.Range(0, 5));
        for(int i = 0; i < 5; i++)
        {
            if (i == TopStartNum)
                StarLimit[i] = Random.Range(1, 6);
            else
                StarLimit[i] = Random.Range(0, 3);
            if (AgeRange == 0)
                Stars[i] = Random.Range(0, StarLimit[i] * 5 + 1);
            else
                Stars[i] = 0;
        }
        //确定倾向
        for (int i = 0; i < 5; i++)
        {
            Character[i] = Random.Range(-3, 3);
            if (i == 4)
                Character[4] = Random.Range(0, 3);
        }
    }

    public void GainExp(int value, int type)
    {
        //1-3职业技能 4观察 5坚韧 6强壮 7管理 8人力 9财务 10决策 11行业 12谋略 13说服 14魅力 15八卦
        int SNum = 0;
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

        float AgePenalty = 0;
        if (Age > 23)
            AgePenalty = (Age - 23) * 0.05f;
        float Efficiency = 1 - AgePenalty + (Stars[SNum] / 5) * 0.2f;
        if (Efficiency < 0)
            Efficiency = 0;
        SkillExp[type - 1] += (int)(value * Efficiency);

        int StartExp = 50, ExtraExp = 50, ExtraLevel = 0;
        if(Levels[type - 1] < 10)
        {
            StartExp = 50;
            ExtraExp = 50;
            ExtraLevel = 0;
        }
        else if (Levels[type - 1] < 15)
        {
            StartExp = 500;
            ExtraExp = 100;
            ExtraLevel = 10;
        }
        else if (Levels[type - 1] < 20)
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

        if (SkillExp[type - 1] >= StartExp + ((Levels[type - 1] - ExtraLevel) * ExtraExp) && Levels[type - 1] < 25)
        {
            SkillExp[type - 1] = 0;
            Levels[type - 1] += 1;
            string SkillName;
            if (type == 1)
            {
                Skill1 += 1;
                SkillName = "职业技能1";
            }
            else if (type == 2)
            { 
                Skill2 += 1;
                SkillName = "职业技能2";
            }
            else if (type == 3)
            { 
                Skill3 += 1;
                SkillName = "职业技能3";
            }
            else if (type == 4)
            { 
                Observation += 1;
                SkillName = "观察";
            }
            else if (type == 5)
            { 
                Tenacity += 1;
                SkillName = "坚韧";
            }
            else if (type == 6)
            { 
                Strength += 1;
                SkillName = "强壮";
            }
            else if (type == 7)
            { 
                Manage += 1;
                SkillName = "管理";
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
        }



    }

    public void EventCheck()
    {
        Request[0] += (300 - stamina - mentality - InfoDetail.GC.Morale) * 0.05f;
        Request[1] += Mathf.Abs(Character[0]) + Mathf.Abs(Character[4]);
        Request[2] += Mathf.Abs(Character[2]) + Mathf.Abs(Character[3]) + Mathf.Abs(Character[4]);
        Request[3] += Mathf.Abs(Character[1]) + Mathf.Abs(Character[3]) + Mathf.Abs(Character[4]);
        for(int i = 0; i < 4; i++)
        {
            if(Request[i] >= 100)
            {
                InfoDetail.Entity.AddEvent(i + 1);
                Request[i] = 0;
                break;
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

    //改变跟目标的好感度并检查关系
    public void ChangeRelation(Employee target, int value)
    {
        Relation r = FindRelation(target);
        r.RPoint += value;
        if (r.RPoint > 100)
            r.RPoint = 100;
        else if (r.RPoint < 0)
            r.RPoint = 0;
        r.RelationCheck();
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
