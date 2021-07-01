using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CWCard
{
    public DivisionControl CurrentDiv;
    public string Name, Description;

    public ProfessionType TypeRequire;
    public int EmpNumRequire, WeakDebuff_Efficiency = 0, WeakDebuff_WorkStatus = 0;//岗位优势需求数量；效率弱化减益；工作状态弱化减益

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
        WeakDebuff_Efficiency = -2;
        TypeRequire = ProfessionType.产品设计;
        EmpNumRequire = 1;
    }

    public override void AddDebuff()
    {
        CurrentDiv.Efficiency += WeakDebuff_Efficiency;
    }

    public override void RemoveDebuff()
    {
        CurrentDiv.Efficiency -= WeakDebuff_Efficiency;
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
        WeakDebuff_WorkStatus = -2;
        TypeRequire = ProfessionType.工程学;
        EmpNumRequire = 1;
    }

    public override void AddDebuff()
    {
        CurrentDiv.WorkStatus += WeakDebuff_WorkStatus;
    }

    public override void RemoveDebuff()
    {
        CurrentDiv.WorkStatus -= WeakDebuff_WorkStatus;
    }
}

