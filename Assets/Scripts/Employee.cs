using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EmpType
{
    Tech, Market, Product, Operate, Science, HR
}

public class Employee
{
    public int Skill1, Skill2, Skill3, SkillExtra1, SkillExtra2, SkillExtra3, 
        Observation, Tenacity, Strength, Manage, HR, Finance, Decision, 
        Forecast, Strategy, Convince, Charm, Gossip, SalaryExtra = 0;
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
    public int[] SkillExp = new int[5];
    public int[] Levels = new int[5];
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

    public void InitStatus(EmpType type, int Level = 3)
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

        int MainNum = 0, MainType = Random.Range(1, 6);
        float PosbA, PosbB, PosbC, PosbD;
        float Posb = Random.Range(0.0f, 1.0f);
        if(Level == 1)
        {
            PosbA = 0.5f; PosbB = 0.8f; PosbC = 0.92f; PosbD = 0.98f;
        }
        else if (Level == 2)
        {
            PosbA = 0.4f; PosbB = 0.7f; PosbC = 0.85f; PosbD = 0.95f;
        }
        else if (Level == 3)
        {
            PosbA = 0.3f; PosbB = 0.6f; PosbC = 0.8f; PosbD = 0.9f;
        }
        else if (Level == 4)
        {
            PosbA = 0.2f; PosbB = 0.4f; PosbC = 0.6f; PosbD = 0.8f;
        }
        else
        {
            PosbA = 0.1f; PosbB = 0.2f; PosbC = 0.4f; PosbD = 0.7f;
        }
        if(InfoA.GC.AdvanceHire == true)
        {
            PosbA -= 0.1f;
            PosbB -= 0.05f;
            PosbD += 0.1f;
        }

        if (Posb < PosbA)
            MainNum = Random.Range(1, 4);
        else if (Posb < PosbB)
            MainNum = Random.Range(4, 7);
        else if (Posb < PosbC)
            MainNum = Random.Range(7, 10);
        else if (Posb < PosbD)
            MainNum = Random.Range(10, 13);
        else
            MainNum = Random.Range(13, 16);
        //职业技能
        int[] n;
        if (MainType == 1)
            n = RandomSkillValue(MainNum);
        else
            n = RandomSkillValue(Random.Range(1, 4));
        Skill1 = n[0];
        Skill2 = n[1];
        Skill3 = n[2];
        Levels[0] = Skill1 + Skill2 + Skill3;
        //基础技能
        if (MainType == 2)
            n = RandomSkillValue(MainNum);
        else
            n = RandomSkillValue(Random.Range(1, 4));
        Observation = n[0];
        Tenacity = n[1];
        Strength = n[2];
        Levels[1] = Observation + Tenacity + Strength;
        //经营技能
        if (MainType == 3)
            n = RandomSkillValue(MainNum);
        else
            n = RandomSkillValue(Random.Range(1, 4));
        Manage = n[0];
        HR = n[1];
        Finance = n[2];
        Levels[2] = Manage + HR + Finance;
        //战略技能
        if (MainType == 4)
            n = RandomSkillValue(MainNum);
        else
            n = RandomSkillValue(Random.Range(1, 4));
        Decision = n[0];
        Forecast = n[1];
        Strategy = n[2];
        Levels[3] = Decision + Forecast + Strategy;
        //社交技能
        if (MainType == 5)
            n = RandomSkillValue(MainNum);
        else
            n = RandomSkillValue(Random.Range(1, 4));
        Convince = n[0];
        Charm = n[1];
        Gossip = n[2];
        Levels[4] = Convince + Charm + Gossip;

        int TopStartNum = (Random.Range(0, 5));
        for(int i = 0; i < 5; i++)
        {
            if (i == TopStartNum)
                StarLimit[i] = Random.Range(1, 6);
            else
                StarLimit[i] = Random.Range(0, 3);
            Stars[i] = Random.Range(0, StarLimit[i] + 1);
        }

        for (int i = 0; i < 5; i++)
        {
            Character[i] = Random.Range(-3, 3);
            if (i == 4)
                Character[4] = Random.Range(0, 3);
        }
    }

    int[] RandomSkillValue(int Value)
    {
        int[] C = new int[3] { 0, 0, 0 };
        int vt = Value;
        while (vt > 0)
        {
            for (int i = 0; i < 3; i++)
            {
                int r = 0;
                if (vt >= 5)
                    r = Random.Range(1, 6);
                else
                    r = Random.Range(1, vt);
                C[i] += r;
                vt -= r;
                if (vt < 1)
                    break;
            }
        }
        return C;
    }

    public void GainExp(int value, int type)
    {
        //1-3职业技能 4观察 5坚韧 6强壮 7管理 8人力 9财务 10决策 11行业 12谋略 13说服 14魅力 15八卦
        if(type <= 3)
        {
            SkillExp[0] += (int)(value * (1 + Stars[0] * 0.2f));
            if(SkillExp[0] >= 50 + Levels[0] * 50)
            {
                SkillExp[0] = 0;
                if (type == 1)
                    Skill1 += 1;
                else if (type == 2)
                    Skill2 += 1;
                else
                    Skill3 += 1;
                Levels[0] += 1;
            }
        }
        else if (type <= 6)
        {
            SkillExp[1] += (int)(value * (1 + Stars[1] * 0.2f));
            if (SkillExp[1] >= 50 + Levels[1] * 50)
            {
                SkillExp[1] = 0;
                if (type == 4)
                    Observation += 1;
                else if (type == 5)
                    Tenacity += 1;
                else
                    Strength += 1;
                Levels[1] += 1;
            }
        }
        else if (type <= 9)
        {
            SkillExp[2] += (int)(value * (1 + Stars[2] * 0.2f));
            if (SkillExp[2] >= 50 + Levels[2] * 50)
            {
                SkillExp[2] = 0;
                if (type == 7)
                    Manage += 1;
                else if (type == 8)
                    HR += 1;
                else
                    Finance += 1;
                Levels[2] += 1;
            }
        }
        else if (type <= 12)
        {
            SkillExp[3] += (int)(value * (1 + Stars[3] * 0.2f));
            if (SkillExp[3] >= 50 + Levels[3] * 50)
            {
                SkillExp[3] = 0;
                if (type == 10)
                    Decision += 1;
                else if (type == 11)
                    Forecast += 1;
                else
                    Strategy += 1;
                Levels[3] += 1;
            }
        }
        else
        {
            SkillExp[4] += (int)(value * (1 + Stars[4] * 0.2f));
            if (SkillExp[4] >= 50 + Levels[4] * 50)
            {
                SkillExp[4] = 0;
                if (type == 13)
                    Convince += 1;
                else if (type == 14)
                    Charm += 1;
                else
                    Gossip += 1;
                Levels[4] += 1;
            }
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
