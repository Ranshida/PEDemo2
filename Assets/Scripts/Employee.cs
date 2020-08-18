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
            mentality = value;
            if (mentality > 100)
                mentality = 100;
            else if (mentality < 0)
                mentality = 0;
            if (mentality < 50 && WantLeave == true)
                InfoA.Fire();
        }
    }

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

    public void InitStatus(EmpType type)
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
        Skill1 = Random.Range(1, 6);
        Skill2 = Random.Range(1, 6);
        Skill3 = Random.Range(1, 6);
        Observation = Random.Range(1, 6);
        Tenacity = Random.Range(1, 6);
        Strength = Random.Range(1, 6);
        Manage = Random.Range(1, 6);
        HR = Random.Range(1, 6);
        Finance = Random.Range(1, 6);
        Decision = Random.Range(1, 6);
        Forecast = Random.Range(1, 6);
        Strategy = Random.Range(1, 6);
        Convince = Random.Range(1, 6);
        Charm = Random.Range(1, 6);
        Gossip = Random.Range(1, 6);
        for(int i = 0; i < 5; i++)
        {
            Character[i] = Random.Range(-3, 3);
            if (i == 4)
                Character[4] = Random.Range(0, 3);
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
