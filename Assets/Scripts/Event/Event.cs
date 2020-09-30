﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventType
{
    工作学习, 心体恢复, 谋划野心, 关系交往
}

//基类
public class Event
{
    public int FailureNum = 0;//用来检测干预事件发生前是大失败还是失败
    public int MinFaith = 101;
    public int MotivationRequire = 3; //1弱 2弱中 3弱中强 4中 5中强 6强
    public int MoralRequire = 0; // 0无 1功利主义 2中庸 3绝对律令
    public int ReligionRequire = 0; // 0无 1机械 2中庸 3人文
    public int TimeLeft = 4;
    public bool HaveTarget = true;
    public string EventName = "无";
    public string ResultText = "无";


    public GameControl GC;
    public Employee Self, Target;
    public Building TargetBuilding;
    public BuildingType BuildingRequire = BuildingType.运营部门; //暂时用运营部门表示没有需求

    public List<Employee> Targets = new List<Employee>();

    public Event()
    {
        if (GameControl.Instance != null)
            GC = GameControl.Instance;
    }

    //可行性检测
    public virtual bool ConditionCheck(int Motivation)
    {
        //防止没设置上
        if (GC == null)
            GC = GameControl.Instance;

        //建筑需求检测
        if (BuildingCheck() == false)
            return false;

        //最低信念检测
        if (Self.Character[4] >= MinFaith)
            return false;

        //关系检测
        if (RelationCheck() == false)
            return false;

        //动机检测
        if (MotivationCheck(Motivation) == false)
            return false;

        //道德检测
        if (MoralCheck() == false)
            return false;

        //信仰检测
        if (ReligionCheck() == false)
            return false;

        //特殊检测
        if (SpecialCheck() == false)
            return false;

        return true;
    }
    //建筑要求
    public virtual bool BuildingCheck()
    {
        if (BuildingRequire == BuildingType.运营部门)
            return true;
        else
        {
            for(int i = 0; i < GC.CurrentDeps.Count; i++)
            {
                if(GC.CurrentDeps[i].building.Type == BuildingRequire)
                {
                    TargetBuilding = GC.CurrentDeps[i].building;
                    return true;
                }
            }
            for(int i = 0; i < GC.CurrentOffices.Count; i++)
            {
                if(GC.CurrentOffices[i].building.Type == BuildingRequire)
                {
                    TargetBuilding = GC.CurrentOffices[i].building;
                    return true;
                }
            }
            return false;
        }
    }
    //关系要求
    public virtual bool RelationCheck()
    {
        if(HaveTarget == true)
        {
            if (Self.Relations.Count > 0)
            {
                Target = Self.Relations[Random.Range(0, Self.Relations.Count)].Target;
                return true;
            }
            return false;
        }
        return true;
    }
    //动机要求
    public virtual bool MotivationCheck(int Mo)
    {
        if (MotivationRequire == 1)
        {
            if (Mo < 20)
                return true;
        }
        else if (MotivationRequire == 2)
        {
            if (Mo < 40)
                return true;
        }
        else if (MotivationRequire == 3)
            return true;
        else if (MotivationRequire == 4)
        {
            if (Mo >= 20 && Mo < 40)
                return true;
        }
        else if (MotivationRequire == 5)
        {
            if (Mo >= 20)
                return true;
        }
        else if (MotivationRequire == 6)
        {
            if (Mo >= 40)
                return true;
        }
            return false;
    }
    //其他要求
    public virtual bool SpecialCheck()
    {
        return true;
    }
    public bool MoralCheck()
    {
        if (MoralRequire == 0)
            return true;
        else if (MoralRequire == 1 && Self.Character[2] <= -50)
            return true;
        else if (MoralRequire == 2 && Self.Character[2] > -50 && Self.Character[2] > 50)
            return true;
        else if (MoralRequire == 3 && Self.Character[2] >= 50)
            return true;

        return false;
    }
    public bool ReligionCheck()
    {
        if (ReligionRequire == 0)
            return true;
        else if (ReligionRequire == 1 && Self.Character[1] <= -50)
            return true;
        else if (ReligionRequire == 2 && Self.Character[1] > -50 && Self.Character[1] < 50)
            return true;
        else if (ReligionRequire == 3 && Self.Character[1] >= 50)
            return true;

        return false;
    }

    //执行时间效果
    public virtual void EventFinish()
    {
        //再检测一下事件是否还有效
        if (HaveTarget == true && Target == null)
        {
            return;
        }
        int result = FindResult();
        float Posb = Random.Range(0.0f, 1.0f);
        if (result == 1)
            MajorFailure(Posb);
        else if (result == 2)
            Failure(Posb);
        else if (result == 3)
            Success(Posb);
        else if (result == 4)
            MajorSuccess(Posb);
        AddHistory();
        MonoBehaviour.print(Self.Name + "发生了事件" + EventName);
    }
    //事件结果判定
    public virtual int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();

        if (value <= 2)
        {
            ResultText = "大失败,";
            return 1;
        }
        else if (value <= 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else if (value < 12)
        {
            ResultText = "成功,";
            return 3;
        }
        else
        {
            ResultText = "大成功,";
            return 4;
        }
        //1大失败 2失败 3成功 4大成功
    }
    //四种结果
    public virtual void MajorFailure(float Posb)
    {

    }
    public virtual void Failure(float Posb)
    {

    }
    public virtual void Success(float Posb)
    {

    }
    public virtual void MajorSuccess(float Posb)
    {

    }
    //特殊点数判定
    public virtual int ExtraValue()
    {
        return 0;
    }
    //关系点数判定
    public int RelationBonus(bool Reverse = false)
    {
        int Value = 0;
        if(Reverse == false && Target != null)
        {
            Relation r = Self.FindRelation(Target);
            if (r.FriendValue == -2)
                Value -= 4;
            else if (r.FriendValue == -1)
                Value -= 2;
            else if (r.FriendValue == 1)
                Value += 1;
            else if (r.FriendValue == 2)
                Value += 3;

            if (r.MasterValue == 1)
                Value += 1;
            else if (r.MasterValue == 2)
                Value += 2;

            if (r.LoveValue == 1)
                Value += 1;
            else if (r.LoveValue == 2)
                Value -= 1;
            else if (r.LoveValue == 3)
                Value += 2;
            else if (r.LoveValue == 4)
                Value += 3;

            if (Self.CurrentClique == Target.CurrentClique)
                Value += 3;
        }
        else if (Reverse == true && Target != null)
        {
            Relation r = Self.FindRelation(Target);
            if (r.FriendValue == -2)
                Value += 4;
            else if (r.FriendValue == -1)
                Value += 2;
            else if (r.FriendValue == 1)
                Value -= 1;
            else if (r.FriendValue == 2)
                Value -= 3;

            if (r.MasterValue == 1)
                Value -= 1;
            else if (r.MasterValue == 2)
                Value -= 2;

            if (r.LoveValue == 1)
                Value -= 1;
            else if (r.LoveValue == 2)
                Value += 1;
            else if (r.LoveValue == 3)
                Value -= 2;
            else if (r.LoveValue == 4)
                Value += 1;
        }
        return Value;
    }
    public int CEORelationBonus(Employee Emp)
    {
        int Value = 0;
        Relation r = Emp.FindRelation(GC.CurrentEmployees[0]);
        if (r.FriendValue == -2)
            Value -= 4;
        else if (r.FriendValue == -1)
            Value -= 2;
        else if (r.FriendValue == 1)
            Value += 1;
        else if (r.FriendValue == 2)
            Value += 3;

        if (r.MasterValue == 1)
            Value += 1;
        else if (r.MasterValue == 2)
            Value += 2;

        if (r.LoveValue == 1)
            Value += 1;
        else if (r.LoveValue == 2)
            Value -= 1;
        else if (r.LoveValue == 3)
            Value += 2;
        else if (r.LoveValue == 4)
            Value += 3;

        if (Self.CurrentClique == Target.CurrentClique)
            Value += 3;

        return Value;
    }
    //文化信仰点数判定
    public int CRBonus(bool Reverse = false)
    {
        int Value = 0;
        if (Reverse == false && Target != null)
        {
            if ((Self.Character[0] >= 50 && Target.Character[0] >= 50) || (Self.Character[0] <= -50 && Target.Character[0] <= -50))
                Value += 1;
            else if ((Self.Character[0] >= 50 && Target.Character[0] <= -50) || (Self.Character[0] <= -50 && Target.Character[0] >= 50))
                Value -= 2;

            if ((Self.Character[1] >= 50 && Target.Character[1] >= 50) || (Self.Character[1] <= -50 && Target.Character[1] <= -50))
                Value += 1;
            else if ((Self.Character[1] >= 50 && Target.Character[1] <= -50) || (Self.Character[1] <= -50 && Target.Character[1] >= 50))
                Value -= 2;
        }
        else if (Reverse == true && Target != null)
        {
            if ((Self.Character[0] >= 50 && Target.Character[0] >= 50) || (Self.Character[0] <= -50 && Target.Character[0] <= -50))
                Value -= 1;
            else if ((Self.Character[0] >= 50 && Target.Character[0] <= -50) || (Self.Character[0] <= -50 && Target.Character[0] >= 50))
                Value += 2;

            if ((Self.Character[1] >= 50 && Target.Character[1] >= 50) || (Self.Character[1] <= -50 && Target.Character[1] <= -50))
                Value -= 1;
            else if ((Self.Character[1] >= 50 && Target.Character[1] <= -50) || (Self.Character[1] <= -50 && Target.Character[1] >= 50))
                Value += 2;
        }
        return Value;
    }
    //士气点数判定
    public int MoraleBonus(int BonusType = 1)
    {
        int Value = 0;
        if (BonusType == 1)
        {
            if (GC.Morale > 80)
                Value += 3;
            else if (GC.Morale > 60)
                Value += 1;
            else if (GC.Morale < 40)
                Value -= 2;
            else if (GC.Morale < 20)
                Value -= 4;
        }
        else if (BonusType == 2)
        {
            if (GC.Morale > 80)
                Value -= 3;
            else if (GC.Morale > 60)
                Value -= 1;
            else if (GC.Morale < 40)
                Value += 1;
            else if (GC.Morale < 20)
                Value += 3;
        }
        else if (BonusType == 3)
        {
            if (GC.Morale > 80)
                Value += 1;
            else if (GC.Morale < 40)
                Value -= 2;
        }
        return Value;
    }

    //干预事件相关
    public virtual void AddSolvableEvent()
    {
        TimeLeft = 6;
        GC.EC.CreateEventInfo(this);
    }
    public virtual string SetSolvableEventText(int type)
    {
        return "";
    }
    public virtual string ConfirmEventSelect(int type)
    {
        return "";
    }

    //添加时间历史（暂时）
    public void AddHistory()
    {
        if(Target != null)
        {
            Self.InfoDetail.AddHistory("自己和" + Target.Name + "发生了" + EventName + "事件" + ResultText);
            Target.InfoDetail.AddHistory("参与了" + Self.Name + "的" + EventName + "事件" + ResultText);
        }
        else if(Targets.Count > 0)
        {
            Self.InfoDetail.AddHistory("自己与一群人参与了" + EventName + "事件" + ResultText);
            for(int i = 0; i < Targets.Count; i++)
            {
                Targets[i].InfoDetail.AddHistory("自己与一群人参与了" + EventName + "事件" + ResultText);
            }
        }
        else
            Self.InfoDetail.AddHistory("自己发生了" + EventName + "事件" + ResultText);

    }

    //复制当前类(利用新建基类覆盖 如new Event = Storage[num].Clone())
    public Event Clone()
    {
        return (Event)this.MemberwiseClone();
    }
}

//要求涨工资
public class Event1: Event
{
    public Event1() : base()
    {
        EventName = "要求涨工资";
        BuildingRequire = BuildingType.高管办公室;
        MinFaith = 80;
        MotivationRequire = 1;
    }
    public override bool RelationCheck()
    {
        if (Self.CurrentDep != null && Self.CurrentDep.CommandingOffice != null && Self.CurrentDep.CommandingOffice.CurrentManager != null)
        {
            Target = Self.CurrentDep.CommandingOffice.CurrentManager;
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        if (Self.Levels[0] > 20)
            Extra += 2;
        if (Self.Levels[2] > 20)
            Extra += 1;
        if (Self.Levels[3] > 20)
            Extra += 1;
        Extra += (int)(Self.Convince * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    
    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 20;
        Self.ChangeCharacter(4, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "心力-20，信念-10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 10;
        Self.ChangeCharacter(4, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "心力-10，信念-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.Mentality += 5;
        Self.SalaryExtra += (int)(Self.InfoDetail.CalcSalary() * 0.2f);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "心力+5,工资上涨20%";
        GC.CreateMessage(Self.Name + "的工资根据需求上涨了20%");
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.Mentality += 15;
        Self.SalaryExtra += (int)(Self.InfoDetail.CalcSalary() * 0.4f);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "心力+15,工资上涨40%";
        GC.CreateMessage(Self.Name + "的工资根据需求上涨了40%");
    }
}

//谋求高位
public class Event2 : Event
{
    public Event2() : base()
    {
        EventName = "谋求高位";
        BuildingRequire = BuildingType.CEO办公室;
        MinFaith = 80;
        MotivationRequire = 4;
    }
    public override bool RelationCheck()
    {
        Target = GC.CurrentEmployees[0];
        return true;
    }
    public override bool SpecialCheck()
    {
        if (Self.CurrentOffice != null && (Self.CurrentOffice.building.Type == BuildingType.CEO办公室 || Self.CurrentOffice.building.Type == BuildingType.高管办公室))
            return false;
        else
            return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        if (Self.Levels[2] > 40)
            Extra += 2;
        else if (Self.Levels[2] > 30)
            Extra += 1;
        if (Self.Levels[3] > 20)
            Extra += 2;
        Extra += (int)(Self.Convince * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 20;
        Self.ChangeCharacter(4, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "心力-20，信念-10";
        GC.CreateMessage(Self.Name + "谋求高位大失败");
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 10;
        Self.ChangeCharacter(4, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "心力-10，信念-5";
        GC.CreateMessage(Self.Name + "谋求高位失败");
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeCharacter(4, 5);
        Self.SalaryExtra += (int)(Self.InfoDetail.CalcSalary() * 0.2f);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "信念+5";
        GC.CreateMessage(Self.Name + "谋求高位成功");
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.InfoDetail.AddPerk(new Perk3(Self), true);
        Self.ChangeCharacter(4, 10);
        Self.SalaryExtra += (int)(Self.InfoDetail.CalcSalary() * 0.4f);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "获得状态 启发*1，信念+10";
        GC.CreateMessage(Self.Name + "谋求高位大成功");
    }
}

//加入派系
public class Event3 : Event
{
    public Event3() : base()
    {
        EventName = "加入派系";
        BuildingRequire = BuildingType.运营部门;
        MinFaith = 60;
        MotivationRequire = 4;
    }
    public override bool RelationCheck()
    {
        List<Employee> T = new List<Employee>();
        if (Self.CurrentClique != null || Self.isCEO == true)
            return false;
        for(int i = 0; i < Self.Relations.Count; i++)
        {
            if(Self.Relations[i].Target.CurrentClique != null && Self.Relations[i].Target.isCEO == false)
            {
                if(Self.Relations[i].FriendValue > 0 || Self.Relations[i].MasterValue > 0 || Self.Relations[i].LoveValue > 2)
                {

                    T.Add(Self.Relations[i].Target);
                }
            }
        }
        if(T.Count > 0)
        {
            Target = T[Random.Range(0, T.Count)];
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Gossip * 0.2);
        Extra += (int)(Self.Convince * 0.1);

        Extra += RelationBonus() + CRBonus();
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 20;
        Self.FindRelation(Target).RPoint -= 30;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "与对方的好感-30，心力-20";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 10;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "心力-10";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeCharacter(4, -10);
        Self.CurrentClique = Target.CurrentClique;
        Self.CurrentClique.Members.Add(Self);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "信念-10, 加入派系";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeCharacter(4, -15);
        Self.CurrentClique = Target.CurrentClique;
        Self.CurrentClique.Members.Add(Self);
        Self.CurrentClique.Members[0].ChangeRelation(Self, 20);
        Self.ChangeRelation(Self.CurrentClique.Members[0], 20);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "派系领袖与自己关系+20，信念-15";
    }
}

//搞破坏
public class Event4 : Event
{
    public Event4() : base()
    {
        EventName = "搞破坏";
        BuildingRequire = BuildingType.运营部门;
        MinFaith = 40;
        MotivationRequire = 5;
        MoralRequire = 1;
    }
    public override bool RelationCheck()
    {
        if (Self.CurrentClique == null || Self.isCEO == true)
            return false;
        List<DepControl> D = new List<DepControl>();
        for(int i = 0; i < GC.CurrentDeps.Count; i++)
        {
            for(int j = 0; j < GC.CurrentDeps[i].CurrentEmps.Count; j++)
            {
                if (GC.CurrentDeps[i].CurrentEmps[j].CurrentClique == Self.CurrentClique)
                    break;
                if (j == GC.CurrentDeps[i].CurrentEmps.Count - 1)
                    D.Add(GC.CurrentDeps[i]);
            }
        }
        //这儿可能会有几个问题 1没部门 2部门没人 3事件执行时目标离开部门
        if (D.Count > 0)
        {
            int DepNum = Random.Range(0, D.Count);
            int EmpNum = Random.Range(0, D[DepNum].CurrentEmps.Count);
            Target = D[DepNum].CurrentEmps[EmpNum];
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Strategy * 0.4);
        Extra += (int)(Self.Strength * 0.2);

        Extra += RelationBonus(true) + CRBonus() + MoraleBonus(2);
        return Extra;
    }
    public override void EventFinish()
    {
        if (Self.CurrentClique == null)
            return;
        base.EventFinish();
    }
    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 20;
        Self.FindRelation(Target).RPoint -= 30;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "与对方的好感-30，心力-20";
        GC.CreateMessage(Self.Name + "在试图搞破坏但是没成功");
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 10;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "破坏失败,心力-10";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        if (Target.CurrentDep != null)
            Target.CurrentDep.FailCheck(true);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "破坏成功，该部门增加1个失误";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        if (Target.CurrentDep != null)
        {
            Target.CurrentDep.FailCheck(true);
            Target.CurrentDep.FailCheck(true);
        }
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "破坏成功，该部门增加2个失误";
    }
}

//罢工
public class Event5 : Event
{
    public Event5() : base()
    {
        EventName = "罢工";
        BuildingRequire = BuildingType.运营部门;
        MinFaith = 40;
        MotivationRequire = 5;
        MoralRequire = 3;
    }
    public override bool RelationCheck()
    {
        if (Self.CurrentClique != null && Self.CurrentClique.Members.Count > 5 && Self.CurrentClique.Members[0] == Self)
            return true;
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);
        Extra += (int)(Self.Strategy * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(2);
        return Extra;
    }
    public override void EventFinish()
    {
        if (Self.CurrentClique == null)
            return;
        base.EventFinish();
        for(int i = 0; i < Self.CurrentClique.Members.Count; i++)
        {
            if (i > 0)
                Self.CurrentClique.Members[i].InfoDetail.AddHistory(ResultText);
        }
    }
    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        for(int i = 0; i < Self.CurrentClique.Members.Count; i++)
        {
            Posb = Random.Range(0.0f, 1.0f);
            Self.CurrentClique.Members[i].Mentality -= 20;
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 10);
            else
                Self.ChangeCharacter(3, 10);
        }
        ResultText += "所有派系成员心力-20";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        for (int i = 0; i < Self.CurrentClique.Members.Count; i++)
        {
            Posb = Random.Range(0.0f, 1.0f);
            Self.CurrentClique.Members[i].Mentality -= 10;
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, -10);
            else
                Self.ChangeCharacter(3, -10);
        }
        ResultText += "所有派系成员心力-10";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        for (int i = 0; i < Self.CurrentClique.Members.Count; i++)
        {
            Posb = Random.Range(0.0f, 1.0f);
            Self.CurrentClique.Members[i].InfoDetail.AddPerk(new Perk28(Self.CurrentClique.Members[i]), true);
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 10);
            else
                Self.ChangeCharacter(3, 10);
        }
        GC.CreateMessage(Self.Name + "领导所在派系罢工");
        ResultText += "开展了持续一个月的罢工";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        for (int i = 0; i < Self.CurrentClique.Members.Count; i++)
        {
            Posb = Random.Range(0.0f, 1.0f);
            Self.CurrentClique.Members[i].Mentality += 20;
            Self.CurrentClique.Members[i].InfoDetail.AddPerk(new Perk28(Self.CurrentClique.Members[i]), true);
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 30);
            else
                Self.ChangeCharacter(3, 30);
        }
        GC.CreateMessage(Self.Name + "领导所在派系罢工");
        ResultText += "开展了持续一个月的罢工,所有派系成员心力+20";
    }
}

//建立派系
public class Event6 : Event
{
    public Event6() : base()
    {
        EventName = "建立派系";
        BuildingRequire = BuildingType.运营部门;
        MinFaith = 60;
        MotivationRequire = 6;
    }
    public override bool RelationCheck()
    {
        List<Employee> T = new List<Employee>();
        if (Self.CurrentClique != null || Self.isCEO == true)
            return false;
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].Target.CurrentClique == null && Self.Relations[i].Target.isCEO == false)
            {
                if (Self.Relations[i].FriendValue > 0 || Self.Relations[i].MasterValue > 0 || Self.Relations[i].LoveValue > 2)
                {
                    T.Add(Self.Relations[i].Target);
                }
            }
        }
        if (T.Count > 0)
        {
            Target = T[Random.Range(0, T.Count)];
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);
        Extra += (int)(Self.Strategy * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(2);
        return Extra;
    }
    public override void EventFinish()
    {
        if (Self.CurrentClique != null)
            return;
        base.EventFinish();
    }
    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 20;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "心力-20";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 10;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "心力-10";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.CurrentClique = new Clique();
        Target.CurrentClique = Self.CurrentClique;
        Self.CurrentClique.Members.Add(Self);
        Self.CurrentClique.Members.Add(Target);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, -10);
        Self.ChangeCharacter(4, -10);
        ResultText += "成立了新派系,信念-10";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.CurrentClique = new Clique();
        Target.CurrentClique = Self.CurrentClique;
        Self.CurrentClique.Members.Add(Self);
        Self.CurrentClique.Members.Add(Target);
        Target.ChangeRelation(Self, 20);
        Self.ChangeRelation(Target, 20);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, -10);
        Self.ChangeCharacter(4, -15);
        ResultText += "成立了新派系,双方关系+20,信念-15";
    }
}

//篡权
public class Event7 : Event
{
    public Event7() : base()
    {
        EventName = "篡权";
        BuildingRequire = BuildingType.运营部门;
        MinFaith = 60;
        MotivationRequire = 6;
        MoralRequire = 1;
    }
    public override bool RelationCheck()
    {
        if (Self.CurrentClique != null && Self.CurrentClique.Members[0] == Self)
        {
            Target = GC.CurrentEmployees[0];
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Convince * 0.2);
        Extra += (int)(Self.Strategy * 0.2);

        Extra += RelationBonus(true) + CRBonus() + MoraleBonus(2);
        return Extra;
    }
    public override void EventFinish()
    {
        if (Self.CurrentClique == null)
            return;
        base.EventFinish();
        for (int i = 0; i < Self.CurrentClique.Members.Count; i++)
        {
            if (i > 0)
                Self.CurrentClique.Members[i].InfoDetail.AddHistory(ResultText);
        }
    }
    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        for (int i = 0; i < Self.CurrentClique.Members.Count; i++)
        {
            Posb = Random.Range(0.0f, 1.0f);
            Self.CurrentClique.Members[i].Mentality -= 20;
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 10);
            else
                Self.ChangeCharacter(3, 10);
        }
        ResultText += "所有派系成员心力-20";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        for (int i = 0; i < Self.CurrentClique.Members.Count; i++)
        {
            Posb = Random.Range(0.0f, 1.0f);
            Self.CurrentClique.Members[i].Mentality -= 10;
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, -10);
            else
                Self.ChangeCharacter(3, -10);
        }
        ResultText += "所有派系成员心力-10";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        for (int i = 0; i < Self.CurrentClique.Members.Count; i++)
        {
            GC.CurrentEmployees[0].Mentality -= 10;
            Posb = Random.Range(0.0f, 1.0f);
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 10);
            else
                Self.ChangeCharacter(3, 10);
        }
        GC.CreateMessage(Self.Name + "领导所在派系篡权成功（目前没效果）,CEO心力-10");
        ResultText += "成功篡权,CEO心力-10";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        for (int i = 0; i < Self.CurrentClique.Members.Count; i++)
        {
            GC.CurrentEmployees[0].Mentality -= 20;
            GC.CurrentEmployees[0].InfoDetail.AddPerk(new Perk7(GC.CurrentEmployees[0]), true);
            Posb = Random.Range(0.0f, 1.0f);
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 30);
            else
                Self.ChangeCharacter(3, 30);
        }
        GC.CreateMessage(Self.Name + "领导所在派系篡权大成功（目前没效果）,CEO心力-20,获得抑郁状态");
        ResultText += "成功篡权,CEO心力-20,获得抑郁状态";
    }
}

//健身房
public class Event8 : Event
{
    public Event8() : base()
    {
        EventName = "健身房";
        BuildingRequire = BuildingType.健身房;
        MotivationRequire = 2;
    }
    public override bool RelationCheck()
    {
        if(TargetBuilding != null && TargetBuilding.Office != null && TargetBuilding.Office.CurrentManager != null)
        {
            Target = TargetBuilding.Office.CurrentManager;
            return true;
        }
        return false;
    }
    public override bool SpecialCheck()
    {
        if (Self.Stamina < 70)
            return true;
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        if (Target.Strength > 15)
            Extra += 3;
        else if (Target.Strength > 10)
            Extra += 2;
        else if (Target.Strength > 5)
            Extra += 1;

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 10;
        Self.InfoDetail.AddPerk(new Perk1(Self), true);
        Self.ChangeCharacter(4, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "心力下降10点，获得状态“受伤”，信念-10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 5;
        Self.ChangeCharacter(4, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "心力下降5点，信念-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.Stamina += (int)(Self.Stamina * 0.05f);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "体力回复5%，信念+5";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.Stamina += (int)(Self.Stamina * 0.1f);
        Self.InfoDetail.AddPerk(new Perk2(Self), true);
        Self.ChangeCharacter(4, 10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "获得状态“斯巴达”，体力回复10%，信念+10";
    }
}

//9喝咖啡没做

//按摩
public class Event10 : Event
{
    public Event10() : base()
    {
        EventName = "健身房";
        BuildingRequire = BuildingType.按摩房;
        MotivationRequire = 2;
    }
    public override bool RelationCheck()
    {
        if (TargetBuilding != null && TargetBuilding.Office != null && TargetBuilding.Office.CurrentManager != null)
        {
            Target = TargetBuilding.Office.CurrentManager;
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        if (Target.Strength > 24)
            Extra += 4;
        else if (Target.Strength > 20)
            Extra += 3;
        else if (Target.Strength > 15)
            Extra += 2;
        else if (Target.Strength > 10)
            Extra += 1;

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override bool SpecialCheck()
    {
        if (Self.Stamina < 40)
            return true;
        return false;
    }
    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 10;
        Self.ChangeCharacter(4, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "心力下降10点，信念-10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Stamina += (int)(Self.Stamina * 0.1f);
        Self.ChangeCharacter(4, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "体力回复10%，信念-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.Stamina += (int)(Self.Stamina * 0.3f);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "体力回复30%，信念+5";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.Stamina += (int)(Self.Stamina * 0.5f);
        Self.ChangeCharacter(4, 10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "体力回复50%，信念+10";
    }
}

//倾诉
public class Event11 : Event
{
    public Event11() : base()
    {
        EventName = "倾诉";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 6;
    }
    public override bool RelationCheck()
    {
        List<Employee> T = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].FriendValue > 0 || Self.Relations[i].MasterValue > 0 || Self.Relations[i].LoveValue > 2)
            {
                T.Add(Self.Relations[i].Target);
            }
        }
        if (T.Count > 0)
        {
            Target = T[Random.Range(0, T.Count)];
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        if (Target.HR > 15)
            Extra += 3;
        else if (Target.HR > 10)
            Extra += 2;
        else if (Target.HR > 5)
            Extra += 1;

        if (Self.Tenacity > 20)
            Extra += 2;
        else if (Self.Tenacity > 10)
            Extra += 1;

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 10;
        Self.ChangeRelation(Target, -20);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "心力下降10点，单方面好感-20";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 5;
        Self.ChangeRelation(Target, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "心力下降5点，单方面好感-10";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.Mentality += (int)(Self.Mentality * 0.05f);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "心力回复5%";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.Mentality += (int)(Self.Mentality * 0.1f);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "心力回复10%";
    }
}

//寻找HR沟通
public class Event12 : Event
{
    public Event12() : base()
    {
        EventName = "寻找HR沟通";
        BuildingRequire = BuildingType.人力资源部B;
        MotivationRequire = 5;
    }
    public override bool BuildingCheck()
    {
        for(int i = 0; i < GC.CurrentDeps.Count; i++)
        {
            if(GC.CurrentDeps[i].type == EmpType.HR && GC.CurrentDeps[i].CurrentEmps.Count > 0)
            {
                if (GC.CurrentDeps[i].CurrentEmps.Count == 1)
                    Target = GC.CurrentDeps[i].CurrentEmps[0];
                else
                {
                    int num = Random.Range(0, 2);
                    Target = GC.CurrentDeps[i].CurrentEmps[num];
                }
            }
        }
        if (Target != null)
            return true;
        return false;
    }
    public override bool SpecialCheck()
    {
        if (Self.Mentality < 40)
            return true;
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        if (Target.HR > 24)
            Extra += 4;
        else if (Target.HR > 20)
            Extra += 3;
        else if (Target.HR > 15)
            Extra += 2;
        else if (Target.HR > 10)
            Extra += 1;

        if (Self.Tenacity > 20)
            Extra += 2;
        else if (Self.Tenacity > 10)
            Extra += 1;

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 10;
        Self.ChangeCharacter(4, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "心力下降10点，信念-10";
        GC.CreateMessage(ResultText);
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 5;
        Self.ChangeCharacter(4, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "心力下降5点，信念-5";
        GC.CreateMessage(ResultText);
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.Mentality += (int)(Self.Mentality * 0.1f);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "心力提升10%，信念+5";
        GC.CreateMessage(ResultText);
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.Mentality += (int)(Self.Mentality * 0.2f);
        Self.ChangeCharacter(4, 10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "心力提升20%，信念+10";
        GC.CreateMessage(ResultText);
    }
}

//13寻找心理咨询师沟通没做

//心力爆炸归零事件1
public class Event14 : Event
{
    public Event14() : base()
    {
        EventName = "心力爆炸归零事件1";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 3;
        HaveTarget = false;
    }
    public override bool SpecialCheck()
    {
        if (Self.Mentality <= 0)
        {
            for(int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
            {
                if(Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 7)
                    return true;
            }
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        if (Self.Tenacity > 15)
            Extra += 2;
        else if (Self.Tenacity > 5)
            Extra += 1;

        if (Self.Lover != null && Self.FindRelation(Self.Lover).LoveValue == 4)
            Extra += 1;
        Extra += MoraleBonus(3) + CRBonus();
        return Extra;
    }

    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();

        if (value <= 9)
        {
            ResultText = "大失败";
            return 1;
        }
        else
        {
            ResultText = "大成功";
            return 4;
        }
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality += (int)(Self.Mentality * 0.5f);
        Self.ChangeCharacter(4, -10);
        string PerkName;
        if (Posb < 0.5f)
        {
            Self.InfoDetail.AddPerk(new Perk8(Self), true);
            PerkName = "重度抑郁症";
        }
        else
        {
            Self.InfoDetail.AddPerk(new Perk6(Self), true);
            PerkName = "欧洲人";
        }
        ResultText += "获得特质" + PerkName + "，心力回复50%，信念-10";
        GC.CreateMessage(ResultText);
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.Mentality += (int)(Self.Mentality * 0.5f);
        Self.ChangeCharacter(4, 10);
        Self.InfoDetail.AddPerk(new Perk9(Self), true);

        ResultText += "获得特质“元气满满”，心力回复50%，信念+10";
        GC.CreateMessage(ResultText);
    }
}

//心力爆炸归零事件2
public class Event15 : Event
{
    public Event15() : base()
    {
        EventName = "心力爆炸归零事件2";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 3;
        HaveTarget = false;
    }
    public override bool SpecialCheck()
    {
        if (Self.Mentality <= 0 && Self.RTarget != null)
        {
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        if (Self.Tenacity > 15)
            Extra += 2;
        else if (Self.Tenacity > 5)
            Extra += 1;

        if (Self.Lover != null && Self.FindRelation(Self.Lover).LoveValue == 4)
            Extra += 1;
        Extra += MoraleBonus(3) + CRBonus();
        return Extra;
    }

    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();

        if (value <= 9)
        {
            ResultText = "大失败";
            return 1;
        }
        else
        {
            ResultText = "大成功";
            return 4;
        }
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality += (int)(Self.Mentality * 0.5f);
        Self.ChangeCharacter(4, -10);
        Self.InfoDetail.AddPerk(new Perk19(Self), true);
        ResultText += "获得特质“反社会人格”，心力回复50%，信念-10";
        GC.CreateMessage(ResultText);
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.Mentality += (int)(Self.Mentality * 0.5f);
        Self.ChangeCharacter(4, 10);
        Self.InfoDetail.AddPerk(new Perk25(Self), true);
        ResultText += "获得特质“爱的艺术”，心力回复50%，信念+10";
        GC.CreateMessage(ResultText);
    }
}

//心力爆炸归零事件3
public class Event16 : Event
{
    public Event16() : base()
    {
        EventName = "心力爆炸归零事件3";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 3;
        HaveTarget = false;
        ReligionRequire = 2;
    }
    public override bool SpecialCheck()
    {
        if (Self.Mentality <= 0)
        {
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        if (Self.Tenacity > 15)
            Extra += 2;
        else if (Self.Tenacity > 5)
            Extra += 1;

        if (Self.Lover != null && Self.FindRelation(Self.Lover).LoveValue == 4)
            Extra += 1;
        Extra += MoraleBonus(3) + CRBonus();
        return Extra;
    }

    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();

        if (value <= 9)
        {
            ResultText = "大失败";
            return 1;
        }
        else
        {
            ResultText = "大成功";
            return 4;
        }
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality += (int)(Self.Mentality * 0.5f);
        Self.ChangeCharacter(4, -10);
        Self.InfoDetail.AddPerk(new Perk10(Self), true);
        ResultText += "获得特质“狂热”，心力回复50%，信念-10";
        GC.CreateMessage(ResultText);
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.Mentality += (int)(Self.Mentality * 0.5f);
        Self.ChangeCharacter(4, 10);
        Self.InfoDetail.AddPerk(new Perk26(Self), true);
        ResultText += "获得特质“悟者”，心力回复50%，信念+10";
        GC.CreateMessage(ResultText);
    }
}

//心力爆炸归零事件4
public class Event17 : Event
{
    public Event17() : base()
    {
        EventName = "心力爆炸归零事件4";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 3;
        HaveTarget = false;
    }
    public override bool SpecialCheck()
    {
        if (Self.Mentality <= 0 && Self.CheckMotivation(3) >= 20)
        {
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        if (Self.Tenacity > 15)
            Extra += 2;
        else if (Self.Tenacity > 5)
            Extra += 1;

        if (Self.Lover != null && Self.FindRelation(Self.Lover).LoveValue == 4)
            Extra += 1;
        Extra += MoraleBonus(3) + CRBonus();
        return Extra;
    }

    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();

        if (value <= 9)
        {
            ResultText = "大失败";
            return 1;
        }
        else
        {
            ResultText = "大成功";
            return 4;
        }
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality += (int)(Self.Mentality * 0.5f);
        Self.ChangeCharacter(4, -10);
        Self.InfoDetail.AddPerk(new Perk10(Self), true);
        ResultText += "获得特质“复仇者”，心力回复50%，信念-10";
        GC.CreateMessage(ResultText);
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.Mentality += (int)(Self.Mentality * 0.5f);
        Self.ChangeCharacter(4, 10);
        string PerkName;
        if (Posb < 0.5f)
        {
            Self.InfoDetail.AddPerk(new Perk12(Self), true);
            PerkName = "鹰视狼顾";
        }
        else
        {
            Self.InfoDetail.AddPerk(new Perk13(Self), true);
            PerkName = "平凡之路";
        }
        ResultText += "获得特质" + PerkName + "，心力回复50%，信念+10";
        GC.CreateMessage(ResultText);
    }
}

//派系交谈
public class Event18 : Event
{
    public Event18() : base()
    {
        EventName = "派系交谈";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 2;
    }
    public override bool RelationCheck()
    {
        if (Self.CurrentClique != null && Self.CurrentClique.Members.Count > 1)
        {
            Target = Self.CurrentClique.Members[Random.Range(0, Self.CurrentClique.Members.Count)];
            while(Target == Self)
            {
                Target = Self.CurrentClique.Members[Random.Range(0, Self.CurrentClique.Members.Count)];
            }
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Gossip * 0.4);
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);
        Extra += (int)(Self.Strategy * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(2);
        Extra -= 3;//去除同派系的加成
        return Extra;
    }
    public override void EventFinish()
    {
        if (Self.CurrentClique == null || Target.CurrentClique == null || Self.CurrentClique != Target.CurrentClique)
            return;
        base.EventFinish();
    }
    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.ChangeRelation(Target, -20);
        Target.ChangeRelation(Self, -20);
        Self.ChangeCharacter(4, 10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "双方好感下降20点，信念+10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeRelation(Target, -10);
        Target.ChangeRelation(Self, -10);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "双方好感下降10点，信念+5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeRelation(Target, 10);
        Target.ChangeRelation(Self, 10);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "双方好感上升10点，信念+5";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeRelation(Target, 20);
        Target.ChangeRelation(Self, 20);
        Self.ChangeCharacter(4, 10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "双方好感上升20点，信念+10";
    }
}

//理念交谈!!!!!!!!!!!!!!!!!!!!!!!
public class Event19 : Event
{
    public Event19() : base()
    {
        EventName = "理念交谈";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 3;
        MoralRequire = 3;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.4);
        Extra += (int)(Self.Observation * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override bool SpecialCheck()
    {
        if (Self == GC.CurrentEmployees[0] || Target == GC.CurrentEmployees[0])
            return false;
        return true;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        FailureNum = 1;
        AddSolvableEvent();
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        FailureNum = 2;
        AddSolvableEvent();
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeCharacter(2, 10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "对方绝对律令倾向小幅增加（+10）";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.ChangeCharacter(2, 10);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "对方绝对律令倾向大幅增加（+30），信念+5";
    }
    public override string SetSolvableEventText(int type)
    {
        string Content = "";
        Employee CEO = GC.CurrentEmployees[0];
        if (type == 0)
            Content = Self.Name + "感到闷闷不乐，仿佛头顶聚集着一大片积雨云，直到在走廊上碰到你时，对方仍然在唉声叹气。“难道没有人在乎穷人了吗？”于是" +
              Self.Name + "希望你来主持公道。";
        else if (type == 1)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            if (CEO.Character[2] > 50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "支持" + Self.Name + "(说服,绝对律令)成功率:" + Posb + "%"; 
        }
        else if (type == 2)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            if (CEO.Character[2] < -50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "支持" + Target.Name + "(说服,功利主义)成功率:" + Posb + "%";
        }
        else if (type == 3)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            value += (int)(CEO.Strategy * 0.2f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "激化矛盾" + "(谋略)成功率:" + Posb + "%";
        }
        else if (type == 4)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            value += (int)(CEO.HR * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "调停矛盾" + "(人力)成功率:" + Posb + "%";
        }
        return Content;
    }

    public override string ConfirmEventSelect(int type)
    {
        Employee CEO = GC.CurrentEmployees[0];
        int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
        int RValue = Random.Range(2, 12);
        if (type == 1)
        {
            if (CEO.Character[2] > 50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            if (RValue + value > 8)
            {
                Self.ChangeRelation(CEO, 10);
                Self.ChangeCharacter(4, 5);
                Self.Mentality += 10;
                CEO.ChangeCharacter(2, 10);
                Target.ChangeRelation(CEO, -10);
                Target.ChangeCharacter(4, -5);
                Target.Mentality -= 10;
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO支持,对CEO好感度+10，信念+5，心力+10，CEO在绝对律令倾向+10");
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO反对,对CEO好感度-10，信念-5，心力-10，CEO在绝对律令倾向+10");
                return "(成功) " + Self.Name + "对CEO好感度+10，" + Self.Name + "信念+5 " + Self.Name + "心力+10，CEO在绝对律令倾向+10，"
                    + Target.Name + "对CEO好感度-10，" + Target.Name + "信念-5，" + Target.Name + "心力-10";
            }
        }
        else if (type == 2)
        {
            if (CEO.Character[2] > 50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(CEO, 10);
                Target.ChangeCharacter(4, 5);
                Target.Mentality += 10;
                CEO.ChangeCharacter(2, -10);
                Self.ChangeRelation(CEO, -10);
                Self.ChangeCharacter(4, -5);
                Self.Mentality -= 10;
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO支持,对CEO好感度+10，信念+5，心力+10，CEO在功利主义倾向+10");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO反对,对CEO好感度-10，信念-5，心力-10，CEO在功利主义倾向+10");
                return "(成功) " + Target.Name + "对CEO好感度+10，" + Target.Name + "信念+5 " + Target.Name + "心力+10，CEO在功利主义倾向+10，"
                    + Self.Name + "对CEO好感度-10，" + Self.Name + "信念-5，" + Self.Name + "心力-10";
            }
        }
        else if (type == 3)
        {
            value += (int)(CEO.Strategy * 0.2f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(Self, -20);
                Self.ChangeRelation(Target, -20);
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO挑拨,双方之间好感度-20");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO挑拨,双方之间好感度-20");
                return "(成功) " + Target.Name + "与" + Self.Name + "双方之间好感度-20";
            }
        }
        else if (type == 4)
        {
            value += (int)(CEO.HR * 0.1f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(Self, 10);
                Target.ChangeRelation(CEO, 5);
                Self.ChangeRelation(Target, 10);
                Self.ChangeRelation(CEO, 5);
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO调停,双方之间好感度+10，对CEO好感度+5");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO调停,双方之间好感度+10，对CEO好感度+5");
                return "(成功) " + Target.Name + "与" + Self.Name + "双方之间好感度+10，" + Target.Name + "对CEO好感度+5，" + Self.Name + "对CEO好感度+5";
            }
        }

        if (FailureNum == 1)
        {
            float Posb = Random.Range(0.0f, 1.0f);
            Self.Mentality -= 15;
            Self.ChangeCharacter(2, -30);
            Self.ChangeCharacter(4, -5);
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 30);
            else
                Self.ChangeCharacter(3, -30);
            ResultText += "信念-5，单方面心力下降15点，功利主义倾向大幅增加（+30）";
            AddHistory();
            return "(失败)" + Self.Name + "信念-5，单方面心力下降15点，功利主义倾向大幅增加（+30）";
        }
        else
        {
            float Posb = Random.Range(0.0f, 1.0f);
            Self.Mentality -= 5;
            Self.ChangeCharacter(2, -10);
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 10);
            else
                Self.ChangeCharacter(3, -10);
            ResultText += "单方面心力下降5点，功利主义倾向小幅增加（+10）";
            AddHistory();
            return "(失败)" + Self.Name + "单方面心力下降5点，功利主义倾向小幅增加（+10）";
        }
    }

}

//友善交谈1
public class Event20 : Event
{
    public Event20() : base()
    {
        EventName = "友善交谈1";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 3;
        MoralRequire = 2;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Observation * 0.4);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.ChangeRelation(Target, -20);
        Target.ChangeRelation(Self, -20);
        Self.ChangeCharacter(4, -5);
        Self.Mentality -= 15;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "信念-5，双方好感下降20点，单方面心力下降15点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeRelation(Target, -10);
        Target.ChangeRelation(Self, -10);
        Self.Mentality -= 5;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "双方好感下降10点，单方面心力下降5点";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeRelation(Target, 5);
        Target.ChangeRelation(Self, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "双方好感上升了5点";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeRelation(Target, 15);
        Target.ChangeRelation(Self, 15);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "信念+5，双方好感上升了15点";
    }
}

//友善交谈2!!!!!!!!!!!!!!!!!!!!!
public class Event21 : Event
{
    public Event21() : base()
    {
        EventName = "友善交谈2";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 3;
        MoralRequire = 2;
    }
    public override bool SpecialCheck()
    {
        if (Self == GC.CurrentEmployees[0] || Target == GC.CurrentEmployees[0])
            return false;
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Gossip * 0.4);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        FailureNum = 1;
        AddSolvableEvent();
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        FailureNum = 2;
        AddSolvableEvent();
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeRelation(Target, 10);
        Target.ChangeRelation(Self, 10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "双方好感上升了10点";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeRelation(Target, 20);
        Target.ChangeRelation(Self, 20);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "信念+5，双方好感上升了20点";
    }
    public override string SetSolvableEventText(int type)
    {
        string Content = "";
        Employee CEO = GC.CurrentEmployees[0];
        if (type == 0)
            Content = Target.Name + "向你举报" + Self.Name + "无事生非，在办公室里传播八卦，抹黑CEO，但是被自己强烈谴责，所以向CEO说明情况";
        else if (type == 1)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            value += (int)(CEO.Gossip * 0.2f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "支持" + Self.Name + "(八卦)成功率:" + Posb + "%";
        }
        else if (type == 2)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            value += (int)(CEO.Convince * 0.2f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "支持" + Target.Name + "(说服)成功率:" + Posb + "%";
        }
        else if (type == 3)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            value += (int)(CEO.Strategy * 0.2f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "激化矛盾" + "(谋略)成功率:" + Posb + "%";
        }
        else if (type == 4)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            value += (int)(CEO.HR * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "调停矛盾" + "(人力)成功率:" + Posb + "%";
        }
        return Content;
    }

    public override string ConfirmEventSelect(int type)
    {
        Employee CEO = GC.CurrentEmployees[0];
        int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
        int RValue = Random.Range(2, 12);
        if (type == 1)
        {
            value += (int)(CEO.Gossip * 0.2f);
            if (RValue + value > 8)
            {
                Self.ChangeRelation(CEO, 10);
                Self.ChangeCharacter(4, 5);
                Self.Mentality += 10;
                Target.ChangeRelation(CEO, -10);
                Target.ChangeCharacter(4, -5);
                Target.Mentality -= 10;
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO支持,对CEO好感度+10，信念+5，心力+10");
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO反对,对CEO好感度-10，信念-5，心力-10");
                return "(成功) " + Self.Name + "对CEO好感度+10，" + Self.Name + "信念+5 " + Self.Name + "心力+10，"
                    + Target.Name + "对CEO好感度-10，" + Target.Name + "信念-5，" + Target.Name + "心力-10";
            }
        }
        else if (type == 2)
        {
            value += (int)(CEO.Convince * 0.2f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(CEO, 10);
                Target.ChangeCharacter(4, 5);
                Target.Mentality += 10;
                Self.ChangeRelation(CEO, -10);
                Self.ChangeCharacter(4, -5);
                Self.Mentality -= 10;
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO支持,对CEO好感度+10，信念+5，心力+10");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO反对,对CEO好感度-10，信念-5，心力-10");
                return "(成功) " + Target.Name + "对CEO好感度+10，" + Target.Name + "信念+5 " + Target.Name + "心力+10，"
                    + Self.Name + "对CEO好感度-10，" + Self.Name + "信念-5，" + Self.Name + "心力-10";
            }
        }
        else if (type == 3)
        {
            value += (int)(CEO.Strategy * 0.2f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(Self, -20);
                Self.ChangeRelation(Target, -20);
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO挑拨,双方之间好感度-20");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO挑拨,双方之间好感度-20");
                return "(成功) " + Target.Name + "与" + Self.Name + "双方之间好感度-20";
            }
        }
        else if (type == 4)
        {
            value += (int)(CEO.HR * 0.1f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(Self, 10);
                Target.ChangeRelation(CEO, 5);
                Self.ChangeRelation(Target, 10);
                Self.ChangeRelation(CEO, 5);
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO调停,双方之间好感度+10，对CEO好感度+5");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO调停,双方之间好感度+10，对CEO好感度+5");
                return "(成功) " + Target.Name + "与" + Self.Name + "双方之间好感度+10，" + Target.Name + "对CEO好感度+5，" + Self.Name + "对CEO好感度+5";
            }
        }

        if (FailureNum == 1)
        {
            float Posb = Random.Range(0.0f, 1.0f);
            Self.ChangeRelation(Target, -20);
            Target.ChangeRelation(Self, -20);
            Self.ChangeCharacter(4, -5);
            Self.Mentality -= 15;
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 30);
            else
                Self.ChangeCharacter(3, -30);
            ResultText += "信念-5，双方好感下降20点，单方面心力下降15点";
            AddHistory();
            return "(失败)" + Self.Name + "信念-5，双方好感下降20点，单方面心力下降15点";
        }
        else
        {
            float Posb = Random.Range(0.0f, 1.0f);
            Self.ChangeRelation(Target, -10);
            Target.ChangeRelation(Self, -10);
            Self.Mentality -= 5;
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 10);
            else
                Self.ChangeCharacter(3, -10);
            ResultText += "双方好感下降10点，单方面心力下降5点";
            AddHistory();
            return "(失败)" + Self.Name + "双方好感下降10点，单方面心力下降5点";
        }
    }
}

//友善交谈3
public class Event22 : Event
{
    public Event22() : base()
    {
        EventName = "友善交谈3";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 5;
        MoralRequire = 2;
    }
    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for(int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].RPoint > 30)
                PotentialTargets.Add(Self.Relations[i].Target);
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Observation * 0.4);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.ChangeRelation(Target, -25);
        Target.ChangeRelation(Self, -25);
        Self.ChangeCharacter(4, -5);
        Self.Mentality -= 20;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "信念-5，双方好感下降25点，单方面心力下降20点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeRelation(Target, -15);
        Target.ChangeRelation(Self, -15);
        Self.Mentality -= 10;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "双方好感下降15点，单方面心力下降10点";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeRelation(Target, 15);
        Target.ChangeRelation(Self, 15);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "双方好感上升了15点";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeRelation(Target, 25);
        Target.ChangeRelation(Self, 25);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "信念+5，双方好感上升了25点";
    }
}

//炫耀交谈
public class Event23 : Event
{
    public Event23() : base()
    {
        EventName = "炫耀交谈";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 3;
        MoralRequire = 1;
    }
    public override bool SpecialCheck()
    {
        if (Self == GC.CurrentEmployees[0] || Target == GC.CurrentEmployees[0])
            return false;
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return -10;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        FailureNum = 1;
        AddSolvableEvent();
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        FailureNum = 2;
        AddSolvableEvent();
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeCharacter(2, -10);
        Target.ChangeRelation(Self, 10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "对方功利主义倾向小幅增加（+10）单方面好感+10";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.ChangeCharacter(2, -30);
        Target.ChangeRelation(Self, 20);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "信念+5 对方功利主义倾向小幅增加（+30）单方面好感+20";
    }
    public override string SetSolvableEventText(int type)
    {
        string Content = "";
        Employee CEO = GC.CurrentEmployees[0];
        if (type == 0)
            Content = Self.Name + "到处转来转去焦躁不安，就像是一个待喷发的火山。一边转悠一边还嘟囔着“现在的年轻人太不像话了！”之类的，" +
                "被烦扰的同事偷偷向你报告，希望你能解决一下。";
        else if (type == 1)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            if (CEO.Character[2] < -50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "支持" + Self.Name + "(说服,功利主义)成功率:" + Posb + "%";
        }
        else if (type == 2)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            if (CEO.Character[2] > 50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "支持" + Target.Name + "(说服,绝对律令)成功率:" + Posb + "%";
        }
        else if (type == 3)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            value += (int)(CEO.Strategy * 0.2f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "激化矛盾" + "(谋略)成功率:" + Posb + "%";
        }
        else if (type == 4)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            value += (int)(CEO.HR * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "调停矛盾" + "(人力)成功率:" + Posb + "%";
        }
        return Content;
    }
    public override string ConfirmEventSelect(int type)
    {
        Employee CEO = GC.CurrentEmployees[0];
        int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
        int RValue = Random.Range(2, 12);
        if (type == 1)
        {
            if (CEO.Character[2] < -50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            if (RValue + value > 8)
            {
                Self.ChangeRelation(CEO, 10);
                Self.ChangeCharacter(4, 5);
                Self.Mentality += 10;
                CEO.ChangeCharacter(2, -10);
                Target.ChangeRelation(CEO, -10);
                Target.ChangeCharacter(4, -5);
                Target.Mentality -= 10;
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO支持,对CEO好感度+10，信念+5，心力+10，CEO在功利主义倾向+10");
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO反对,对CEO好感度-10，信念-5，心力-10，CEO在功利主义倾向+10");
                return "(成功) " + Self.Name + "对CEO好感度+10，" + Self.Name + "信念+5 " + Self.Name + "心力+10，CEO在绝对律令倾向+10，"
                    + Target.Name + "对CEO好感度-10，" + Target.Name + "信念-5，" + Target.Name + "心力-10";
            }
        }
        else if (type == 2)
        {
            if (CEO.Character[2] > 50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(CEO, 10);
                Target.ChangeCharacter(4, 5);
                Target.Mentality += 10;
                CEO.ChangeCharacter(2, 10);
                Self.ChangeRelation(CEO, 10);
                Self.ChangeCharacter(4, -5);
                Self.Mentality -= 10;
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO支持,对CEO好感度+10，信念+5，心力+10，CEO在绝对律令倾向+10");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO反对,对CEO好感度-10，信念-5，心力-10，CEO在绝对律令倾向+10");
                return "(成功) " + Target.Name + "对CEO好感度+10，" + Target.Name + "信念+5 " + Target.Name + "心力+10，CEO在绝对律令倾向+10，"
                    + Self.Name + "对CEO好感度-10，" + Self.Name + "信念-5，" + Self.Name + "心力-10";
            }
        }
        else if (type == 3)
        {
            value += (int)(CEO.Strategy * 0.2f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(Self, -20);
                Self.ChangeRelation(Target, -20);
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO挑拨,双方之间好感度-20");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO挑拨,双方之间好感度-20");
                return "(成功) " + Target.Name + "与" + Self.Name + "双方之间好感度-20";
            }
        }
        else if (type == 4)
        {
            value += (int)(CEO.HR * 0.1f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(Self, 10);
                Target.ChangeRelation(CEO, 5);
                Self.ChangeRelation(Target, 10);
                Self.ChangeRelation(CEO, 5);
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO调停,双方之间好感度+10，对CEO好感度+5");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO调停,双方之间好感度+10，对CEO好感度+5");
                return "(成功) " + Target.Name + "与" + Self.Name + "双方之间好感度+10，" + Target.Name + "对CEO好感度+5，" + Self.Name + "对CEO好感度+5";
            }
        }

        if (FailureNum == 1)
        {
            float Posb = Random.Range(0.0f, 1.0f);
            Target.Mentality -= 15;
            Self.ChangeCharacter(2, 30);
            Self.ChangeCharacter(4, -5);
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 30);
            else
                Self.ChangeCharacter(3, -30);
            ResultText += "信念-5，对方心力下降15点，绝对律令倾向大幅增加（+30）";
            AddHistory();
            return "(失败)" + Self.Name + "信念-5，对方心力下降15点，绝对律令倾向大幅增加（+30）";
        }
        else
        {
            float Posb = Random.Range(0.0f, 1.0f);
            Target.Mentality -= 5;
            Self.ChangeCharacter(2, 10);
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 10);
            else
                Self.ChangeCharacter(3, -10);
            ResultText += "对方心力下降5点，绝对律令倾向小幅增加（+10）";
            AddHistory();
            return "(失败)" + Self.Name + "对方心力下降5点，绝对律令倾向小幅增加（+10）";
        }
    }
}

//道德思考
public class Event24 : Event
{
    public Event24() : base()
    {
        EventName = "道德思考";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 4;
        HaveTarget = false;
        MoralRequire = 2;
    }
    public override int ExtraValue()
    {
        return 0;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.ChangeCharacter(2, -30);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "功利主义倾向 大幅增加（+30）";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeCharacter(2, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "功利主义倾向 小幅增加（+10）";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeCharacter(2, 10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "绝对律令倾向 小幅增加（+10）";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeCharacter(2, 30);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "绝对律令倾向 大幅增加（+30）";
    }
}

//交流人的价值
public class Event25 : Event
{
    public Event25() : base()
    {
        EventName = "交流人的价值";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 3;
        ReligionRequire = 3;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.Mentality -= 15;
        Self.ChangeCharacter(1, -30);
        Self.ChangeCharacter(4, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "对方心力下降15点，机械智能信仰大幅增加（+30），信念-5";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.Mentality -= 5;
        Self.ChangeCharacter(1, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "对方心力下降5点，机械智能信仰小幅增加（+10）";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 10);
        Target.ChangeCharacter(1, 10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "对方人文主义信仰小幅增加（+10）单方面好感+10";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.ChangeRelation(Self, 20);
        Target.ChangeCharacter(1, 30);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "对方人文主义信仰小幅增加（+30）单方面好感+20，信念+5";
    }
}

//交流机械的价值
public class Event26 : Event
{
    public Event26() : base()
    {
        EventName = "交流机械的价值";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 3;
        ReligionRequire = 1;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.Mentality -= 15;
        Self.ChangeCharacter(1, 30);
        Self.ChangeCharacter(4, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "对方心力下降15点，人文主义信仰大幅增加（+30），信念-5";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.Mentality -= 5;
        Self.ChangeCharacter(1, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "对方心力下降5点，人文主义信仰小幅增加（+10）";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 10);
        Target.ChangeCharacter(1, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "对方机械智能信仰小幅增加（+10）单方面好感+10";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.ChangeRelation(Self, 20);
        Target.ChangeCharacter(1, -30);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "对方机械智能信仰小幅增加（+30）单方面好感+20，信念+5";
    }
}

//信仰怀疑
public class Event27 : Event
{
    public Event27() : base()
    {
        EventName = "信仰怀疑";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 4;
        HaveTarget = false;
        ReligionRequire = 2;
    }
    public override int ExtraValue()
    {
        return 0;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.ChangeCharacter(1, -30);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "机械智能信仰 大幅增加（+30）";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeCharacter(1, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "机械智能信仰 小幅增加（+10）";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeCharacter(1, 10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "人文主义信仰 小幅增加（+10）";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeCharacter(1, 30);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "人文主义信仰 大幅增加（+30）";
    }
}

//狂热传教1!!!!!!!!!!!!!!!!!!!!!!
public class Event28 : Event
{
    public Event28() : base()
    {
        EventName = "狂热传教1";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 5;
        ReligionRequire = 1;
    }
    public override bool SpecialCheck()
    {
        if (Self == GC.CurrentEmployees[0] || Target == GC.CurrentEmployees[0])
            return false;
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 11)
                return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        FailureNum = 1;
        AddSolvableEvent();
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        FailureNum = 2;
        AddSolvableEvent();
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 5);
        Target.ChangeCharacter(1, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "对方机械智能信仰小幅增加（+10）单方面好感+5";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.ChangeRelation(Self, 10);
        Target.ChangeCharacter(1, -30);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "对方机械智能信仰大幅增加（+30）单方面好感+10，信念+5";
    }
    public override string SetSolvableEventText(int type)
    {
        string Content = "";
        Employee CEO = GC.CurrentEmployees[0];
        if (type == 0)
            Content = Self.Name + "还在四处转悠寻找下一个猎物，“愚蠢！”“迷失的人类！”，" + Self.Name + "一边低声重复着一边抚摸手里的微型机器人，" +
                "不知不觉便走到了你面前，向你抱怨那些拒绝传教的“迷途羔羊”。";
        else if (type == 1)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            if (CEO.Character[1] < -50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "支持" + Self.Name + "(说服,机械飞升)成功率:" + Posb + "%";
        }
        else if (type == 2)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            if (CEO.Character[1] > 50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "支持" + Target.Name + "(说服,人文主义)成功率:" + Posb + "%";
        }
        else if (type == 3)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            value += (int)(CEO.Strategy * 0.2f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "激化矛盾" + "(谋略)成功率:" + Posb + "%";
        }
        else if (type == 4)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            value += (int)(CEO.HR * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "调停矛盾" + "(人力)成功率:" + Posb + "%";
        }
        return Content;
    }
    public override string ConfirmEventSelect(int type)
    {
        Employee CEO = GC.CurrentEmployees[0];
        int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
        int RValue = Random.Range(2, 12);
        if (type == 1)
        {
            if (CEO.Character[1] < -50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            if (RValue + value > 8)
            {
                Self.ChangeRelation(CEO, 10);
                Self.ChangeCharacter(4, 5);
                Self.Mentality += 10;
                CEO.ChangeCharacter(1, -10);
                Target.ChangeRelation(CEO, -10);
                Target.ChangeCharacter(4, -5);
                Target.Mentality -= 10;
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO支持,对CEO好感度+10，信念+5，心力+10，CEO机械飞升倾向+10");
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO反对,对CEO好感度-10，信念-5，心力-10，CEO机械飞升倾向+10");
                return "(成功) " + Self.Name + "对CEO好感度+10，" + Self.Name + "信念+5 " + Self.Name + "心力+10，CEO机械飞升倾向+10，"
                    + Target.Name + "对CEO好感度-10，" + Target.Name + "信念-5，" + Target.Name + "心力-10";
            }
        }
        else if (type == 2)
        {
            if (CEO.Character[1] > 50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(CEO, 10);
                Target.ChangeCharacter(4, 5);
                Target.Mentality += 10;
                CEO.ChangeCharacter(1, 10);
                Self.ChangeRelation(CEO, 10);
                Self.ChangeCharacter(4, -5);
                Self.Mentality -= 10;
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO支持,对CEO好感度+10，信念+5，心力+10，CEO人文主义倾向+10");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO反对,对CEO好感度-10，信念-5，心力-10，CEO人文主义倾向+10");
                return "(成功) " + Target.Name + "对CEO好感度+10，" + Target.Name + "信念+5 " + Target.Name + "心力+10，CEO人文主义倾向+10，"
                    + Self.Name + "对CEO好感度-10，" + Self.Name + "信念-5，" + Self.Name + "心力-10";
            }
        }
        else if (type == 3)
        {
            value += (int)(CEO.Strategy * 0.2f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(Self, -20);
                Self.ChangeRelation(Target, -20);
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO挑拨,双方之间好感度-20");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO挑拨,双方之间好感度-20");
                return "(成功) " + Target.Name + "与" + Self.Name + "双方之间好感度-20";
            }
        }
        else if (type == 4)
        {
            value += (int)(CEO.HR * 0.1f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(Self, 10);
                Target.ChangeRelation(CEO, 5);
                Self.ChangeRelation(Target, 10);
                Self.ChangeRelation(CEO, 5);
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO调停,双方之间好感度+10，对CEO好感度+5");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO调停,双方之间好感度+10，对CEO好感度+5");
                return "(成功) " + Target.Name + "与" + Self.Name + "双方之间好感度+10，" + Target.Name + "对CEO好感度+5，" + Self.Name + "对CEO好感度+5";
            }
        }

        if (FailureNum == 1)
        {
            float Posb = Random.Range(0.0f, 1.0f);
            Target.Mentality -= 15;
            Target.ChangeRelation(Self, -15);
            Self.ChangeCharacter(4, -5);
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 30);
            else
                Self.ChangeCharacter(3, -30);
            ResultText += "信念-5，对方心力-15  对方单方面好感-15";
            AddHistory();
            return "(失败)" + Self.Name + "信念-5，对方心力-15  对方单方面好感-15";
        }
        else
        {
            float Posb = Random.Range(0.0f, 1.0f);
            Target.Mentality -= 5;
            Target.ChangeRelation(Self, -5);
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 10);
            else
                Self.ChangeCharacter(3, -10);
            ResultText += "对方心力-5 对方单方面好感-5";
            AddHistory();
            return "(失败)" + Self.Name + "对方心力-5 对方单方面好感-5";
        }
    }
}

//狂热传教2 重点再查一下!!!!!!!!!!!
public class Event29 : Event
{
    public Event29() : base()
    {
        EventName = "狂热传教2";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 5;
        ReligionRequire = 3;
    }
    public override bool SpecialCheck()
    {
        if (Self == GC.CurrentEmployees[0] || Target == GC.CurrentEmployees[0])
            return false;
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 11)
                return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        FailureNum = 1;
        AddSolvableEvent();
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        FailureNum = 2;
        AddSolvableEvent();
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 5);
        Target.ChangeCharacter(1, 10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "对方人文主义信仰倾向小幅增加（+10%） 单方面好感+5";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.ChangeRelation(Self, 10);
        Target.ChangeCharacter(1, 30);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "对方人文主义信仰大幅增加（+30）单方面好感+10";
    }

    public override string SetSolvableEventText(int type)
    {
        string Content = "";
        Employee CEO = GC.CurrentEmployees[0];
        if (type == 0)
            Content = Self.Name + "两个人的争吵让旁边的同事苦不堪言，虽然" +  Target.Name + "除了“松手！”之外什么也没说。" +
                "你抵达现场时"+ Self.Name + "还在死命拽着" + Target.Name + "不撒手，没完没了地说着什么“人类是永远不会消失的" +
                "除非所有人都信什么机械飞升”之类的话，" + Target.Name + "向你投来了求助的眼神。";
        else if (type == 1)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            if (CEO.Character[1] > 50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "支持" + Self.Name + "(说服,人文主义)成功率:" + Posb + "%";
        }
        else if (type == 2)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            if (CEO.Character[1] < -50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "支持" + Target.Name + "(说服,机械飞升)成功率:" + Posb + "%";
        }
        else if (type == 3)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            value += (int)(CEO.Strategy * 0.2f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "激化矛盾" + "(谋略)成功率:" + Posb + "%";
        }
        else if (type == 4)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            value += (int)(CEO.HR * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "调停矛盾" + "(人力)成功率:" + Posb + "%";
        }
        return Content;
    }

    public override string ConfirmEventSelect(int type)
    {
        Employee CEO = GC.CurrentEmployees[0];
        int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
        int RValue = Random.Range(2, 12);
        if (type == 1)
        {
            if (CEO.Character[1] > 50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            if (RValue + value > 8)
            {
                Self.ChangeRelation(CEO, 10);
                Self.ChangeCharacter(4, 5);
                Self.Mentality += 10;
                CEO.ChangeCharacter(1, 10);
                Target.ChangeRelation(CEO, -10);
                Target.ChangeCharacter(4, -5);
                Target.Mentality -= 10;
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO支持,对CEO好感度+10，信念+5，心力+10，CEO人文主义倾向+10");
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO反对,对CEO好感度-10，信念-5，心力-10，CEO人文主义倾向+10");
                return "(成功) " + Self.Name + "对CEO好感度+10，" + Self.Name + "信念+5 " + Self.Name + "心力+10，CEO人文主义倾向+10，"
                    + Target.Name + "对CEO好感度-10，" + Target.Name + "信念-5，" + Target.Name + "心力-10";
            }
        }
        else if (type == 2)
        {
            if (CEO.Character[1] < -50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(CEO, 10);
                Target.ChangeCharacter(4, 5);
                Target.Mentality += 10;
                CEO.ChangeCharacter(1, -10);
                Self.ChangeRelation(CEO, 10);
                Self.ChangeCharacter(4, -5);
                Self.Mentality -= 10;
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO支持,对CEO好感度+10，信念+5，心力+10，CEO机械飞升倾向+10");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO反对,对CEO好感度-10，信念-5，心力-10，CEO机械飞升倾向+10");
                return "(成功) " + Target.Name + "对CEO好感度+10，" + Target.Name + "信念+5 " + Target.Name + "心力+10，CEO机械飞升倾向+10，"
                    + Self.Name + "对CEO好感度-10，" + Self.Name + "信念-5，" + Self.Name + "心力-10";
            }
        }
        else if (type == 3)
        {
            value += (int)(CEO.Strategy * 0.2f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(Self, -20);
                Self.ChangeRelation(Target, -20);
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO挑拨,双方之间好感度-20");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO挑拨,双方之间好感度-20");
                return "(成功) " + Target.Name + "与" + Self.Name + "双方之间好感度-20";
            }
        }
        else if (type == 4)
        {
            value += (int)(CEO.HR * 0.1f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(Self, 10);
                Target.ChangeRelation(CEO, 5);
                Self.ChangeRelation(Target, 10);
                Self.ChangeRelation(CEO, 5);
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO调停,双方之间好感度+10，对CEO好感度+5");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO调停,双方之间好感度+10，对CEO好感度+5");
                return "(成功) " + Target.Name + "与" + Self.Name + "双方之间好感度+10，" + Target.Name + "对CEO好感度+5，" + Self.Name + "对CEO好感度+5";
            }
        }

        if (FailureNum == 1)
        {
            float Posb = Random.Range(0.0f, 1.0f);
            Target.Mentality -= 15;
            Target.ChangeRelation(Self, -15);
            Self.ChangeCharacter(4, -5);
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 30);
            else
                Self.ChangeCharacter(3, -30);
            ResultText += "信念-5，对方心力-15  对方单方面好感-15";
            AddHistory();
            return "(失败)" + Self.Name + "信念-5，对方心力-15  对方单方面好感-15";
        }
        else
        {
            float Posb = Random.Range(0.0f, 1.0f);
            Target.Mentality -= 5;
            Target.ChangeRelation(Self, -5);
            Self.ChangeCharacter(4, 5);
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 10);
            else
                Self.ChangeCharacter(3, -10);
            ResultText += "信念+5，对方心力-5 对方单方面好感-5";
            AddHistory();
            return "(失败)" + Self.Name + "信念+5，对方心力-5 对方单方面好感-5";
        }
    }
}

//恋爱狂热
public class Event30 : Event
{
    public Event30() : base()
    {
        EventName = "恋爱狂热";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 2;
    }

    public override bool RelationCheck()
    {
        if (Self.Lover != null && Self.FindRelation(Self.Lover).LoveValue == 3)
        {
            Target = Self.Lover;
            return true;
        }
        return false;
    }

    public override bool SpecialCheck()
    {
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 24)
                return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Observation * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.Mentality -= 10;
        Target.ChangeRelation(Self, -15);
        Self.Mentality -= 10;
        Self.ChangeRelation(Target, -15);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "双方好感-15，双方心力-10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.Mentality -= 5;
        Target.ChangeRelation(Self, -5);
        Self.Mentality -= 5;
        Self.ChangeRelation(Target, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "双方好感-5，双方心力-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.Mentality += 5;
        Target.ChangeRelation(Self, 5);
        Self.Mentality += 5;
        Self.ChangeRelation(Target, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "双方好感+5，双方心力+5";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.Mentality += 10;
        Target.ChangeRelation(Self, 15);
        Self.Mentality += 10;
        Self.ChangeRelation(Target, 15);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "双方好感+15，双方心力+10";
    }
}

//挚友搞基互动
public class Event31 : Event
{
    public Event31() : base()
    {
        EventName = "挚友搞基互动";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 2;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].FriendValue == 2)
                PotentialTargets.Add(Self.Relations[i].Target);
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Observation * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.Mentality -= 10;
        Target.ChangeRelation(Self, -15);
        Self.Mentality -= 10;
        Self.ChangeRelation(Target, -15);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "双方好感-15，双方心力-10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.Mentality -= 5;
        Target.ChangeRelation(Self, -5);
        Self.Mentality -= 5;
        Self.ChangeRelation(Target, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "双方好感-5，双方心力-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.Mentality += 5;
        Target.ChangeRelation(Self, 5);
        Self.Mentality += 5;
        Self.ChangeRelation(Target, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "双方好感+5，双方心力+5";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.Mentality += 10;
        Target.ChangeRelation(Self, 15);
        Self.Mentality += 10;
        Self.ChangeRelation(Target, 15);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "双方好感+15，双方心力+10";
    }
}

//潜在发展对象
public class Event32 : Event
{
    public Event32() : base()
    {
        EventName = "潜在发展对象";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 3;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].LoveValue == 1 || Self.Relations[i].Target == Self.RTarget)
                PotentialTargets.Add(Self.Relations[i].Target);
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Observation * 0.2);
        Extra += (int)(Self.Convince * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.ChangeRelation(Self, -15);
        Self.Mentality -= 10;
        Self.ChangeCharacter(4, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "信念-5，对方好感-15，心力-10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.ChangeRelation(Self, -5);
        Self.Mentality -= 5;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "对方好感-5，心力-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 5);
        Self.ChangeRelation(Target, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "双方好感+5";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.Mentality += 5;
        Target.ChangeRelation(Self, 15);
        Self.Mentality += 5;
        Self.ChangeRelation(Target, 15);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "信念+5，双方好感+15，双方心力+5";
    }
}

//拜师  技能加成少了一部分 而且特质里没有师承授业这个东西
public class Event33 : Event
{
    public Event33() : base()
    {
        EventName = "拜师";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 5;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0 || Self.Master != null)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].FriendValue < 1 && Self.Relations[i].LoveValue < 3 && Self.Relations[i].MasterValue == 0 && Self.Relations[i].RPoint > 50)
            {
                Employee t = Self.Relations[i].Target;
                if(t.Skill1 > 15 || t.Skill2 > 15 || t.Skill3 > 15 || t.Observation > 15 || t.Tenacity > 15 || t.Strength > 15 || t.Manage > 15 ||
                    t.HR > 15 || t.Finance > 15 || t.Decision > 15 || t.Forecast > 15 || t.Strategy > 15 || t.Convince > 15 || t.Charm > 15 || t.Gossip > 15)
                {
                    for(int j = 0; j < 5; i++)
                    {
                        if (t.Levels[j] < Self.Levels[j])
                            break;
                        else if (j == 4)
                            PotentialTargets.Add(t);
                    }
                }
            }
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Observation * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.ChangeCharacter(4, -10);
        Self.Mentality -= 30;
        Self.RTarget = Target;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "心力-30，信念-10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeCharacter(4, -5);
        Self.Mentality -= 10;
        Self.RTarget = Target;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);

        ResultText += "心力-10，信念-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeCharacter(4, 5);
        Self.Stamina -= 30;
        if (Self.RTarget == Target)
            Self.RTarget = null;
        Self.Master = Target;
        Self.FindRelation(Target).MasterValue = 2;
        Target.FindRelation(Self).MasterValue = 1;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "信念+5，获得特质师承授业(没有这个特质)，体力-30," + Self.Name + "师承" + Target.Name;
        GC.CreateMessage(ResultText);
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeCharacter(4, 10);
        if (Self.RTarget == Target)
            Self.RTarget = null;
        Self.Master = Target;
        Relation r = Self.FindRelation(Target);
        r.MasterValue = 2;
        r.RPoint += 10;

        r = Target.FindRelation(Self);
        r.MasterValue = 1;
        r.RPoint += 10;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "信念+10，获得特质师承授业(没有这个特质)，双方好感+10," + Self.Name + "师承" + Target.Name;
        GC.CreateMessage(ResultText);
    }
}

//邂逅变为追求或倾慕
public class Event34 : Event
{
    public Event34() : base()
    {
        EventName = "邂逅变为追求或倾慕";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 1;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].FriendValue < 1 && Self.Relations[i].LoveValue == 0 && Self.Relations[i].MasterValue == 0)
            {
                Employee T = Self.Relations[i].Target;
                if (T == null || T.CurrentDep != Self.CurrentDep)
                {
                    if (T.Charm > 10 || T.Levels[0] > 30 || T.Levels[1] > 20 || T.Levels[2] > 20 || T.Levels[3] > 20 || T.Levels[4] > 20)
                        PotentialTargets.Add(T);
                }
            }
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);
        Extra += (int)(Self.Observation * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 10;
        Relation r = Self.FindRelation(Target);
        r.RPoint += 50;
        r.LoveValue = 1;

        r = Target.FindRelation(Self);
        r.RPoint -= 30;
        r.LoveValue = 2;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "心力-10，单方面好感+50，" + Self.Name + "倾慕" + Target.Name + "," + Target.Name + "获得追求者"
            + Self.Name + "单方面好感度-30";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Relation r = Self.FindRelation(Target);
        r.RPoint += 50;
        r.LoveValue = 1;

        r = Target.FindRelation(Self);
        r.RPoint -= 10;
        r.LoveValue = 2;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "单方面好感+50，" + Self.Name + "倾慕" + Target.Name + "," + Target.Name + "获得追求者"
            + Self.Name + "单方面好感度-10";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Relation r = Self.FindRelation(Target);
        r.RPoint += 50;
        r.LoveValue = 1;

        r = Target.FindRelation(Self);
        r.LoveValue = 2;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "单方面好感+50，" + Self.Name + "倾慕" + Target.Name + "," + Target.Name + "获得追求者"
            + Self.Name;
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Relation r = Self.FindRelation(Target);
        r.RPoint += 50;
        r.LoveValue = 1;

        r = Target.FindRelation(Self);
        r.RPoint += 20;
        r.LoveValue = 2;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "单方面好感+50，" + Self.Name + "倾慕" + Target.Name + "," + Target.Name + "获得追求者"
            + Self.Name + "单方面好感度+20";
    }
}

//情侣
public class Event35 : Event
{
    public Event35() : base()
    {
        EventName = "情侣";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 5;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0 || Self.Lover != null)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].RPoint > 50 && Self.Relations[i].LoveValue < 3)
            {
                if (Self.Relations[i].Target.FindRelation(Self).RPoint > 50)
                    PotentialTargets.Add(Self.Relations[i].Target);
            }
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);
        Extra += (int)(Self.Observation * 0.2);

        Extra += CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.ChangeRelation(Self, -10);
        Self.RTarget = Target;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "对方好感-10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.RTarget = Target;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);        
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        if (Self.RTarget == Target)
            Self.RTarget = null;
        Self.Mentality += 5;
        Target.Mentality += 5;
        Self.FindRelation(Target).LoveValue = 3;
        Target.FindRelation(Self).LoveValue = 3;
        Self.Lover = Target;
        Target.Lover = Self;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "双方心力+5，彼此结成情侣关系";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        if (Self.RTarget == Target)
            Self.RTarget = null;
        Self.Mentality += 10;
        Target.Mentality += 10;
        Self.FindRelation(Target).LoveValue = 3;
        Target.FindRelation(Self).LoveValue = 3;
        Self.Lover = Target;
        Target.Lover = Self;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "双方心力+10，彼此结成情侣关系";
    }
}

//伴侣
public class Event36 : Event
{
    public Event36() : base()
    {
        EventName = "伴侣";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 5;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0 || Self.Lover == null)
            return false;
        if (Self.Lover != null && Self.FindRelation(Self.Lover).LoveValue == 4)
            return false;
        if (Self.FindRelation(Self.Lover).RPoint > 80 && Self.Lover.FindRelation(Self).RPoint > 80)
        {
            Target = Self.Lover;
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);
        Extra += (int)(Self.Observation * 0.2);

        Extra += CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.ChangeRelation(Self, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "对方好感-10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "无事发生";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        if (Self.RTarget == Target)
            Self.RTarget = null;
        Self.Mentality += 5;
        Target.Mentality += 5;
        Self.FindRelation(Target).LoveValue = 4;
        Target.FindRelation(Self).LoveValue = 4;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "双方心力+5，彼此结成伴侣关系";
        GC.CreateMessage(ResultText);
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        if (Self.RTarget == Target)
            Self.RTarget = null;
        Self.Mentality += 10;
        Target.Mentality += 10;
        Self.FindRelation(Target).LoveValue = 4;
        Target.FindRelation(Self).LoveValue = 4;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "双方心力+10，彼此结成伴侣关系";
        GC.CreateMessage(ResultText);
    }
}

//朋友
public class Event37 : Event
{
    public Event37() : base()
    {
        EventName = "朋友";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 5;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].RPoint > 50 && Self.Relations[i].LoveValue < 3 && Self.Relations[i].FriendValue < 1)
            {
                if (Self.Relations[i].Target.FindRelation(Self).RPoint > 50)
                    PotentialTargets.Add(Self.Relations[i].Target);
            }
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);
        Extra += (int)(Self.Observation * 0.2);

        Extra += CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.ChangeRelation(Self, -10);
        Self.RTarget = Target;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "对方好感-10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.RTarget = Target;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        if (Self.RTarget == Target)
            Self.RTarget = null;
        Self.Mentality += 5;
        Self.ChangeCharacter(4, 5);
        Target.Mentality += 5;
        Target.ChangeCharacter(4, 5);
        Self.FindRelation(Target).FriendValue = 1;
        Target.FindRelation(Self).FriendValue = 1;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "双方心力+5，双方信念+5，彼此结成朋友关系";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        if (Self.RTarget == Target)
            Self.RTarget = null;
        Self.Mentality += 10;
        Self.ChangeCharacter(4, 10);
        Target.Mentality += 10;
        Target.ChangeCharacter(4, 10);
        Self.FindRelation(Target).FriendValue = 1;
        Target.FindRelation(Self).FriendValue = 1;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "双方心力+10，双方信念+10，彼此结成朋友关系";
    }
}

//挚友
public class Event38 : Event
{
    public Event38() : base()
    {
        EventName = "挚友";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 5;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].RPoint > 80 && Self.Relations[i].FriendValue == 1)
            {
                if (Self.Relations[i].Target.FindRelation(Self).RPoint > 80)
                    PotentialTargets.Add(Self.Relations[i].Target);
            }
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);
        Extra += (int)(Self.Observation * 0.2);

        Extra += CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.ChangeRelation(Self, -10);
        Self.RTarget = Target;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "对方好感-10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.RTarget = Target;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        if (Self.RTarget == Target)
            Self.RTarget = null;
        Self.Mentality += 10;
        Self.ChangeCharacter(4, 5);
        Target.Mentality += 10;
        Target.ChangeCharacter(4, 5);
        Self.FindRelation(Target).FriendValue = 2;
        Target.FindRelation(Self).FriendValue = 2;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "双方心力+10，双方信念+5，彼此结成挚友关系";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        if (Self.RTarget == Target)
            Self.RTarget = null;
        Self.Mentality += 20;
        Self.ChangeCharacter(4, 10);
        Target.Mentality += 20;
        Target.ChangeCharacter(4, 10);
        Self.FindRelation(Target).FriendValue = 2;
        Target.FindRelation(Self).FriendValue = 2;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "双方心力+20，双方信念+10，彼此结成挚友关系";
    }
}

//陌路
public class Event39 : Event
{
    public Event39() : base()
    {
        EventName = "陌路";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 5;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].RPoint < 0)
            {
                PotentialTargets.Add(Self.Relations[i].Target);
            }
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus(true) + MoraleBonus(2);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 10;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "单方面心力-10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 5;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "单方面心力-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeRelation(Target, -10);
        Target.ChangeRelation(Self, -10);
        Self.ChangeCharacter(4, -5);
        Target.ChangeCharacter(4, -5);
        Self.FindRelation(Target).FriendValue = -1;
        Target.FindRelation(Self).FriendValue = -1;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "彼此结成陌路关系，双方关系-10，双方信念-5";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeRelation(Target, -20);
        Target.ChangeRelation(Self, -20);
        Self.ChangeCharacter(4, -10);
        Target.ChangeCharacter(4, -10);
        Self.FindRelation(Target).FriendValue = -1;
        Target.FindRelation(Self).FriendValue = -1;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "彼此结成陌路关系，双方关系-20，双方信念-10";
    }
}

//仇敌
public class Event40 : Event
{
    public Event40() : base()
    {
        EventName = "仇敌";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 5;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].RPoint < -40)
            {
                PotentialTargets.Add(Self.Relations[i].Target);
            }
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus(true) + MoraleBonus(2);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 10;
        Target.Mentality += 5;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "单方面心力-10，对方心力+5";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 5;
        Target.Mentality -= 5;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "双方心力-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeRelation(Target, -10);
        Target.ChangeRelation(Self, -10);
        Self.ChangeCharacter(4, -5);
        Target.ChangeCharacter(4, -5);
        Self.FindRelation(Target).FriendValue = -2;
        Target.FindRelation(Self).FriendValue = -2;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "彼此结成仇敌关系，双方关系-10，双方信念-5";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeRelation(Target, -20);
        Target.ChangeRelation(Self, -20);
        Self.ChangeCharacter(4, -10);
        Target.ChangeCharacter(4, -10);
        Self.FindRelation(Target).FriendValue = -2;
        Target.FindRelation(Self).FriendValue = -2;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "彼此结成仇敌关系，双方关系-20，双方信念-10";
    }
}

//解除关系
public class Event41 : Event
{
    public Event41() : base()
    {
        EventName = "解除关系";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 5;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].RPoint < 20 && (Self.Relations[i].FriendValue > 0 || Self.Relations[i].LoveValue > 2 || Self.Relations[i].MasterValue > 0))
            {
                PotentialTargets.Add(Self.Relations[i].Target);
            }
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;

        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);
        Extra += (int)(Self.Observation * 0.2);
        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.ChangeCharacter(4, -5);
        Target.ChangeCharacter(4, -5);

        Self.ChangeRelation(Target, -10);
        Target.ChangeRelation(Self, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "双方信念-5，双方关系下降10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.ChangeRelation(Self, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "对方关系下降5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Relation r = Self.FindRelation(Target);
        if (r.LoveValue > 2)
        {
            r.LoveValue = 0;
            r = Target.FindRelation(Self);
            r.LoveValue = 0;
            Self.Lover = null;
            Target.Lover = null;
        }
        else if (r.MasterValue > 0)
        {
            r.MasterValue = 0;
            r = Target.FindRelation(Self);
            r.MasterValue = 0;
            if (Self.Master == Target)
                Self.Master = null;
            else if (Target.Master == Self)
                Target.Master = null;
        }
        else if (r.FriendValue > 0)
        {
            r.FriendValue = 0;
            r = Target.FindRelation(Self);
            r.FriendValue = 0;
        }

        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "双方解除了一个亲密关系";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeCharacter(4, 5);
        Target.ChangeCharacter(4, 5);
        Self.Mentality += 10;
        Target.Mentality += 10;
        Relation r = Self.FindRelation(Target);

        if (r.LoveValue > 2)
        {
            r.LoveValue = 0;
            r = Target.FindRelation(Self);
            r.LoveValue = 0;
            Self.Lover = null;
            Target.Lover = null;
        }
        else if (r.MasterValue > 0)
        {
            r.MasterValue = 0;
            r = Target.FindRelation(Self);
            r.MasterValue = 0;
            if (Self.Master == Target)
                Self.Master = null;
            else if (Target.Master == Self)
                Target.Master = null;
        }
        else if (r.FriendValue > 0)
        {
            r.FriendValue = 0;
            r = Target.FindRelation(Self);
            r.FriendValue = 0;
        }

        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "双方解除了一个亲密关系，双方信念+5，双方心力+10";
    }
}

//离职
public class Event42 : Event
{
    public Event42() : base()
    {
        EventName = "离职";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 3;
        HaveTarget = false;
        MinFaith = 20;
    }

    public override void EventFinish()
    {
        //再检测一下事件是否还有效
        if (HaveTarget == true && Target == null)
        {
            return;
        }
        int result = FindResult();
        float Posb = Random.Range(0.0f, 1.0f);
        if (result == 1)
            MajorFailure(Posb);
        else if (result == 2)
            Failure(Posb);
        else if (result == 3)
            Success(Posb);
        else if (result == 4)
            MajorSuccess(Posb);
    }

    public override bool SpecialCheck()
    {
        if (Self.CurrentClique != null)
            return false;
        return true;
    }

    public override int ExtraValue()
    {
        int Extra = MoraleBonus(2);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        ResultText += "从公司离职";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Fire();
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        ResultText += "从公司离职";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Fire();
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "因为某些原因，决定再干一段时间";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "因为某些原因，决定再干一段时间";
    }
}

//寻求建议
public class Event43 : Event
{
    public Event43() : base()
    {
        EventName = "寻求建议";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 1;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].Target.CurrentDep != null && Self.Relations[i].Target.CurrentDep.type != EmpType.HR)
            {
                PotentialTargets.Add(Self.Relations[i].Target);
            }
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        TargetBuilding = Target.CurrentDep.building;
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Target.HR * 0.2);
        Extra += (int)(Target.Forecast * 0.2);
        Extra += (int)(Target.Observation * 0.2);

        if (Self.Levels[0] > 45)
            Extra += 2;
        else if (Self.Levels[1] > 30)
            Extra += 1;

        Extra += CRBonus() + MoraleBonus(1) + RelationBonus();
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.ChangeCharacter(4, -10);
        Self.Mentality -= 20;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "信念-10，心力-20";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeCharacter(4, -5);
        Self.ChangeRelation(Target, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "信念-5，单方面好感-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeCharacter(4, 5);
        Self.InfoDetail.AddPerk(new Perk3(Self), true);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "信念+5，获得 状态 启发";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeCharacter(4, 10);
        Self.InfoDetail.AddPerk(new Perk3(Self), true);
        Self.InfoDetail.AddPerk(new Perk3(Self), true);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "信念+10，获得 状态 启发*2";
    }
}

//交流工作
public class Event44 : Event
{
    public Event44() : base()
    {
        EventName = "交流工作";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 4;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].Target.CurrentDep != null && Self.Relations[i].Target.CurrentDep.type != EmpType.HR)
            {
                PotentialTargets.Add(Self.Relations[i].Target);
            }
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        TargetBuilding = Target.CurrentDep.building;
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Target.HR * 0.1);
        Extra += (int)(Target.Forecast * 0.1);
        Extra += (int)(Target.Observation * 0.1);

        Extra += (int)(Self.HR * 0.1);
        Extra += (int)(Self.Forecast * 0.1);
        Extra += (int)(Self.Observation * 0.1);

        Extra += CRBonus() + MoraleBonus(1) + RelationBonus();
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.ChangeCharacter(4, -10);
        Self.Mentality -= 20;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "信念-10，心力-20";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeCharacter(4, -5);
        Self.ChangeRelation(Target, -5);
        Target.ChangeRelation(Self, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "信念-5，双方好感-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeCharacter(4, 5);
        Self.InfoDetail.AddPerk(new Perk3(Self), true);
        Target.ChangeCharacter(4, 5);
        Target.InfoDetail.AddPerk(new Perk3(Target), true);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "信念+5，双方获得 状态 启发";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeCharacter(4, 10);
        Self.InfoDetail.AddPerk(new Perk3(Self), true);
        Self.InfoDetail.AddPerk(new Perk3(Target), true);
        Target.ChangeCharacter(4, 10);
        Target.InfoDetail.AddPerk(new Perk3(Target), true);
        Target.InfoDetail.AddPerk(new Perk3(Target), true);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "信念+10，双方获得 状态 启发*2";
    }
}

//头脑风暴
public class Event45 : Event
{
    public Event45() : base()
    {
        EventName = "头脑风暴";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 6;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].Target.CurrentDep != null && Self.Relations[i].Target.CurrentDep.type != EmpType.HR)
            {
                PotentialTargets.Add(Self.Relations[i].Target);
            }
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        TargetBuilding = Target.CurrentDep.building;
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Target.Forecast * 0.1);
        Extra += (int)(Target.Observation * 0.1);

        Extra += (int)(Self.Forecast * 0.1);
        Extra += (int)(Self.Observation * 0.1);

        Extra += CRBonus() + MoraleBonus(1) + RelationBonus();
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.ChangeCharacter(4, -10);
        Self.InfoDetail.AddPerk(new Perk7(Self), true);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "信念-10，获得“抑郁”状态";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeCharacter(4, -5);
        Self.ChangeRelation(Target, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "信念-5，单方面好感-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeCharacter(4, 5);
        Self.InfoDetail.AddPerk(new Perk4(Self), true);
        Target.ChangeCharacter(4, 5);
        Target.InfoDetail.AddPerk(new Perk4(Target), true);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "信念+5，双方获得状态 头脑风暴*1";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeCharacter(4, 10);
        Self.ChangeRelation(Target, 10);
        Self.InfoDetail.AddPerk(new Perk4(Self), true);
        Self.InfoDetail.AddPerk(new Perk4(Self), true);
        Target.ChangeCharacter(4, 10);
        Target.ChangeRelation(Self, 10);
        Target.InfoDetail.AddPerk(new Perk4(Target), true);
        Target.InfoDetail.AddPerk(new Perk4(Target), true);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "信念+10，双方获得状态 头脑风暴*2，双方好感+5";
    }
}

//师承授业
public class Event46 : Event
{
    public Event46() : base()
    {
        EventName = "师承授业";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 3;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0 || Self.Master == null)
            return false;

        if(Self.Master != null)
        {
            if(Self.Master.CurrentDep != null && Self.Master.CurrentDep.type != EmpType.HR)
            {
                Target = Self.Master;
                return true;
            }
            else if (Self.Master.CurrentOffice != null && (Self.Master.CurrentOffice.building.Type == BuildingType.CEO办公室 || Self.Master.CurrentOffice.building.Type == BuildingType.高管办公室))
            {
                Target = Self.Master;
                return true;
            }
        }
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Target.Forecast * 0.2);

        Extra += (int)(Self.Observation * 0.2);

        Extra += CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void EventFinish()
    {
        if (Self.Master == null)
            return;
        base.EventFinish();
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.ChangeCharacter(4, -10);
        Self.ChangeRelation(Target, -10);
        Target.ChangeRelation(Self, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "信念-10，双方好感-10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeCharacter(4, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "信念-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeCharacter(4, 5);
        Self.InfoDetail.AddPerk(new Perk4(Self), true);
        Self.InfoDetail.AddPerk(new Perk3(Self), true);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "信念+5，获得状态 头脑风暴*1 状态 启发*1";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeCharacter(4, 10);
        Self.InfoDetail.AddPerk(new Perk4(Self), true);
        Self.InfoDetail.AddPerk(new Perk4(Self), true);
        Self.InfoDetail.AddPerk(new Perk3(Self), true);
        Self.InfoDetail.AddPerk(new Perk3(Self), true);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "信念+10，获得状态 头脑风暴*2 状态 启发*2";
    }
}

//摸鱼
public class Event47 : Event
{
    public Event47() : base()
    {
        EventName = "摸鱼";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 3;
    }
    public override int ExtraValue()
    {
        int Extra = 0;

        Extra += (int)(Self.Strategy * 0.2);
        Extra += (int)(Self.Convince * 0.2);
        Extra += CRBonus() + MoraleBonus(2) + RelationBonus(true);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 20;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "心力-20";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 10;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "心力-10";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.Mentality += 10;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "心力+10";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.Mentality += 20;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "心力+20";
    }
}

//抱怨
public class Event48 : Event
{
    public Event48() : base()
    {
        EventName = "抱怨";
        BuildingRequire = BuildingType.运营部门;
        MotivationRequire = 2;
    }

    public override bool RelationCheck()
    {
        List<Employee> E = new List<Employee>();
        if (Self.CurrentDep == null)
            return false;
        for (int i = 0; i < Self.CurrentDep.CurrentEmps.Count; i++)
        {
            if (Self.CurrentDep.CurrentEmps[i] != Self)
                E.Add(Self.CurrentDep.CurrentEmps[i]);
        }
        if (E.Count > 0)
        {
            Target = E[Random.Range(0, E.Count)];
            return true;
        }
        return false;
    }

    public override int ExtraValue()
    {
        int Extra = 0;
        if (Self.Tenacity > 20)
            Extra += 2;
        else if (Self.Tenacity > 10)
            Extra += 1;
        if (Target.HR > 15)
            Extra += 3;
        else if (Target.HR > 10)
            Extra += 2;
        else if (Target.HR > 5)
            Extra += 1;
        Extra += CRBonus() + MoraleBonus(1) + RelationBonus();
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 20;
        Self.ChangeRelation(Target, -20);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "心力-20，单方面好感-20";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 10;
        Self.ChangeRelation(Target, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "心力-10，单方面好感-10";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.Mentality += 2;
        Self.ChangeRelation(Target, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "心力+2，单方面好感+5";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.Mentality += 5;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "心力+5";
    }
}

static public class EventData
{
    //工作学习事件
    static public List<Event> StudyEvents = new List<Event>()
    {
        new Event43(), new Event44(), new Event45()
    };
    static public List<Event> StudyForceEvents = new List<Event>() { new Event46() };

    //心体恢复事件
    static public List<Event> RecoverEvent = new List<Event>()
    {
        new Event8(), new Event10(), new Event11(), new Event12(), new Event48()
    };
    static public List<Event> RecoverForceEvent = new List<Event>()
    {
        new Event14(), new Event15(), new Event16(), new Event17()
    };

    //谋划野心事件
    static public List<Event> AmbitionEvent = new List<Event>()
    {
        new Event1(), new Event2(), new Event3(), new Event4(), new Event5(), new Event6(), new Event7(), new Event47()
    };
    static public List<Event> AmbitionForceEvent = new List<Event>();

    //关系交往事件
    static public List<Event> SocialEvent = new List<Event>()
    {
        new Event18(), new Event19(), new Event20(), new Event21(), new Event22(), new Event23(), new Event24(),
        new Event25(), new Event26(), new Event27(), new Event31(), new Event32(), new Event33(), new Event34(),
        new Event35(), new Event36(), new Event37(), new Event38(), new Event39(), new Event41(),
    };
    static public List<Event> SocialForceEvent = new List<Event>()
    {
        new Event28(), new Event29(), new Event30(), new Event36(), new Event38(), new Event40()
    };

    static public void CopyList(List<Event> Self, List<Event> Target)
    {
        for(int i = 0; i < Target.Count; i++)
        {
            Self.Add(Target[i]);
        }
    }
}
    