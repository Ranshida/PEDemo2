﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CWCard
{
    public DivisionControl CurrentDiv;
    public string Name, Description;

    public ProfessionType TypeRequire;
    public int EmpNumRequire;

    public List<int> EfficiencyDebuff = new List<int>();
    public List<int> WorkStatusDebuff = new List<int>(); //这俩一般直接就是负值

    //弱化效果的添加和移除
    public virtual void AddDebuff()
    {

    }
    public virtual void RemoveDebuff()
    {

    }

    public CWCard Clone()
    {
        return (CWCard)this.MemberwiseClone();
    }

    static public List<CWCard> CWCardData = new List<CWCard>()
    {
        new CWCard1(), new CWCard2()
    };
}

public class CWCard1 : CWCard
{
    public CWCard1() : base()
    {
        Name = "原型图";
        Description = "原型图原型图原型图";
        EfficiencyDebuff = new List<int>() { -2, -2, -3};
        WorkStatusDebuff = new List<int>() { -2, -4, -7 };
        TypeRequire = ProfessionType.产品设计;
        EmpNumRequire = 2;
    }

    public override void AddDebuff()
    {
        CurrentDiv.Efficiency -= 2;
    }

    public override void RemoveDebuff()
    {
        CurrentDiv.Efficiency += 2;
    }
}

public class CWCard2 : CWCard
{
    public CWCard2() : base()
    {
        Name = "大数据";
        Description = "大数据大数据大数据";
        EfficiencyDebuff = new List<int>() { -2, -2, -2 };
        WorkStatusDebuff = new List<int>() { -3, -5, -8 };
        TypeRequire = ProfessionType.工程学;
        EmpNumRequire = 2;
    }

    public override void AddDebuff()
    {
        CurrentDiv.WorkStatus -= 2;
    }

    public override void RemoveDebuff()
    {
        CurrentDiv.WorkStatus += 2;
    }
}

