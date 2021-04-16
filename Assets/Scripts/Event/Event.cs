using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventType
{
    工作学习, 心体恢复, 谋划野心, 关系交往
}
//基类
public abstract class Event
{
    public virtual bool IsJudgeEvent { get { return false; } }

    public int FailureNum = 0;//用来检测干预事件发生前是大失败还是失败
    public int MinFaith = 101;
    public int MotivationRequire = 3; //1弱 2弱中 3弱中强 4中 5中强 6强
    public int MoralRequire = 0; // 0无 1功利主义 2中庸 3绝对律令
    public int ReligionRequire = 0; // 0无 1机械 2中庸 3人文
    public int RelationRequire = 0; //0无 1朋友 2挚友 3徒弟  4师傅 5倾慕 6追求 7情侣 8伴侣 -1陌路 -2仇人
    public int PerkRequire = 0;     //特质需求(Perk编号)
    public int PerkRemoveNumber;    //要移除的特质 （20.12.5新增）
    public int TimeLeft = 2;
    public bool HaveTarget = true;  //是否有目标
    public bool isSolving = false;  //开始处理
    public string EventName = "无";
    public string ResultText = "无";
    public string ObjectText = "无";
    public int Weight = 1;          //事件权重
    public int perkUsed = -1;    //出现多个满足条件的状态时，使用的状态编号（20.12.9新增）
    public string perkUsedName = "未知状态";
    public int result = 0;

    public GameControl GC;
    public Employee Self, Target;
    public EmpEntity SelfEntity { get { return Self.InfoDetail.Entity; } }
    public EmpEntity TargetEntity { get { return Target.InfoDetail.Entity; } }
    public Building TargetBuilding;
    //public BuildingType BuildingRequire = BuildingType.空; //暂时用空表示没有需求
    public List<BuildingType> BuildingRequires = new List<BuildingType>(); //暂时用空表示没有需求
    public List<EColor> SelfEmotionRequire = new List<EColor>(); //自身颜色需求
    public List<EColor> TargetEmotionRequire = new List<EColor>(); //对方颜色需求
    public List<Event> SubEvents = new List<Event>(); //可能的子事件
    public List<Employee> Targets = new List<Employee>();

    public Event()
    {
        if (GameControl.Instance != null)
            GC = GameControl.Instance;
    }

    //开始处理事件
    public virtual void StartSolve()
    {
        isSolving = true;
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

        //情绪检测
        if (EmotionCheck() == false)
            return false;

        //关系检测
        if (HaveTarget == true)
        {
            if (RelationCheck() == false)
                return false;
        }

        //特质检测
        if (PerkCheck() == false)
            return false;

        //动机检测
        if (Motivation != -1)
        {
            if (MotivationCheck(Motivation) == false)
                return false;
        }
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
        if (BuildingRequires.Count == 0)
            return true;

        for (int i = 0; i < GC.CurrentDeps.Count; i++)
        {
            foreach (BuildingType type in BuildingRequires)
            {
                if (GC.CurrentDeps[i].building.Type == type)
                {
                    return true;
                }
            }
        }
        return false;


        //if (BuildingRequire == BuildingType.空)
        //    return true;
        //else
        //{
        //    for (int i = 0; i < GC.CurrentDeps.Count; i++)
        //    {
        //        if (GC.CurrentDeps[i].building.Type == BuildingRequire)
        //        {
        //            TargetBuilding = GC.CurrentDeps[i].building;
        //            return true;
        //        }
        //    }
        //    for (int i = 0; i < GC.CurrentOffices.Count; i++)
        //    {
        //        if (GC.CurrentOffices[i].building.Type == BuildingRequire)
        //        {
        //            TargetBuilding = GC.CurrentOffices[i].building;
        //            return true;
        //        }
        //    }
        //    return false;
        //}
    }
    //关系要求
    public virtual bool RelationCheck()
    {
        if (Target == null)
        {
            if (Self.Relations.Count > 0)
            {
                Target = Self.Relations[Random.Range(0, Self.Relations.Count)].Target;
                return true;
            }
            return false;
        }
        else
        {
            if (RelationRequire == 0)
                return true;
            else if (RelationRequire < 3)
            {
                if (Self.FindRelation(Target).FriendValue == RelationRequire)
                    return true;
                else
                    return false;
            }
            else if (RelationRequire < 5)
            {
                if (Self.FindRelation(Target).MasterValue == RelationRequire - 2)
                    return true;
                else
                    return false;
            }
            else if (RelationRequire < 9)
            {
                if (Self.FindRelation(Target).LoveValue == RelationRequire - 4)
                    return true;
                else
                    return false;
            }
            return false;
        }
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
    //情绪要求
    public virtual bool EmotionCheck()
    {
        if (SelfEmotionRequire.Count == 0 && TargetEmotionRequire.Count == 0)
            return true;
        else if (TargetEmotionRequire.Count == 0)
        {
            for(int i = 0; i < SelfEmotionRequire.Count; i++)
            {
                for(int j = 0; j < Self.CurrentEmotions.Count; j++)
                {
                    if (Self.CurrentEmotions[j].color == SelfEmotionRequire[i])
                        return true;
                }
            }
            return false;
        }
        else if (SelfEmotionRequire.Count == 0)
        {
            for (int i = 0; i < TargetEmotionRequire.Count; i++)
            {
                for (int j = 0; j < Target.CurrentEmotions.Count; j++)
                {
                    if (Target.CurrentEmotions[j].color == TargetEmotionRequire[i])
                        return true;
                }
            }
            return false;
        }
        else
        {
            for (int i = 0; i < TargetEmotionRequire.Count; i++)
            {
                for (int j = 0; j < Target.CurrentEmotions.Count; j++)
                {
                    if (Target.CurrentEmotions[j].color == TargetEmotionRequire[i])
                        break;
                }
                if (i == TargetEmotionRequire.Count - 1)
                    return false;
            }

            for (int i = 0; i < SelfEmotionRequire.Count; i++)
            {
                for (int j = 0; j < Self.CurrentEmotions.Count; j++)
                {
                    if (Self.CurrentEmotions[j].color == SelfEmotionRequire[i])
                        return true;
                }
            }
            return false;
        }
    }
    //特质需求
    public virtual bool PerkCheck()
    {
        if (PerkRequire == 0)
            return true;
        else
        {
            for(int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
            {
                if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == PerkRequire)
                    return true;
            }
            return false;
        }
    }
    //根据特质设置事件权重
    protected void SetWeight(int num, int value)
    {
        foreach (PerkInfo perk in Self.InfoDetail.PerksInfo)
        {
            if (perk.CurrentPerk.Num == num)
            {
                Weight += value;
                return;
            }
        }
    }
    public virtual void SetWeight()
    {
        
    }

    public virtual void PerkRemoveCheck()
    {
        if (PerkRemoveNumber == 0)
            return;
        else
        {
            for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
            {
                if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == PerkRemoveNumber)
                {
                    Self.InfoDetail.PerksInfo[i].CurrentPerk.RemoveEffect();
                    return;
                }
            }
        }
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
        else if (MoralRequire == 1 && Self.CharacterTendency[2] == -1)
            return true;
        else if (MoralRequire == 2 && Self.CharacterTendency[2] == 0)
            return true;
        else if (MoralRequire == 3 && Self.CharacterTendency[2] == 1)
            return true;

        return false;
    }
    public bool ReligionCheck()
    {
        if (ReligionRequire == 0)
            return true;
        else if (ReligionRequire == 1 && Self.CharacterTendency[1] == -1)
            return true;
        else if (ReligionRequire == 2 && Self.CharacterTendency[1] == 0)
            return true;
        else if (ReligionRequire == 3 && Self.CharacterTendency[1] == 1)
            return true;

        return false;
    }
    //子事件检测
    public Event SubEventCheck()
    {
        if (SubEvents.Count == 0)
            return null;
        Event E = null;
        List<Event> EL = new List<Event>();
        for (int i = 0; i < SubEvents.Count; i++)
        {
            SubEvents[i].Self = Self;
            SubEvents[i].Target = Target;
            if (SubEvents[i].ConditionCheck(-1) == true)
                EL.Add(SubEvents[i]);
        }
        if (EL.Count > 0)
            E = EL[Random.Range(0, EL.Count)];
        return E;
    }

    //执行时间效果
    public virtual void EventFinish()
    {
        isSolving = false;
        //再检测一下事件是否还有效
        if (HaveTarget == true && Target == null)
        {
            return;
        }
        result = FindResult();
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
        int value = Random.Range(1, 20);
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
    //情绪点数判定
    public int EmotionBonus()
    {
        int dRed = 0, dYellow = 0, dPurple = 0, dGreen = 0, red = 0, yellow = 0, purple = 0, green = 0;
        foreach (Emotion e in Self.CurrentEmotions)
        {
            if (e.color == EColor.Red)
            {
                dRed += e.Level;
            }
            else if (e.color == EColor.Yellow)
            {
                dYellow += e.Level;
            }
            else if (e.color == EColor.Purple)
            {
                dPurple += e.Level;
            }
            else if (e.color == EColor.Green)
            {
                dGreen += e.Level;
            }
            else if (e.color == EColor.LRed)
            {
                red += e.Level;
            }
            else if (e.color == EColor.LYellow)
            {
                yellow += e.Level;
            }
            else if (e.color == EColor.LPurple)
            {
                purple += e.Level;
            }
            else if (e.color == EColor.LGreen)
            {
                green += e.Level;
            }
        }
        if(Target!=null && HaveTarget)
        {
            foreach (Emotion e in Target.CurrentEmotions)
            {
                if (e.color == EColor.Red)
                {
                    dRed += e.Level;
                }
                else if (e.color == EColor.Yellow)
                {
                    dYellow += e.Level;
                }
                else if (e.color == EColor.Purple)
                {
                    dPurple += e.Level;
                }
                else if (e.color == EColor.Green)
                {
                    dGreen += e.Level;
                }
                else if (e.color == EColor.LRed)
                {
                    red += e.Level;
                }
                else if (e.color == EColor.LYellow)
                {
                    yellow += e.Level;
                }
                else if (e.color == EColor.LPurple)
                {
                    purple += e.Level;
                }
                else if (e.color == EColor.LGreen)
                {
                    green += e.Level;
                }
            }
        }
        return dYellow * 2 - dRed * 2 - dPurple * 2 + dGreen * 2 + yellow - red - purple + green;
    }
    //特质点数判定
    public int PerksBonus()
    {
        int Extra = 0;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 75 | p.CurrentPerk.Num == 78)
            {
                Extra += 1;
            }
            else if (p.CurrentPerk.Num == 76 | p.CurrentPerk.Num == 79)
            {
                Extra += 2;
            }
            else if (p.CurrentPerk.Num == 77 | p.CurrentPerk.Num == 80)
            {
                Extra += 3;
            }
        }
        return Extra;
    }
    //关系点数判定
    public int RelationBonus(bool Reverse = false)
    {
        int Value = 0;
        if (Reverse == false && Target != null)
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
            if (Self.CharacterTendency[0] * Target.CharacterTendency[0] == 1)
                Value += 1;
            else if (Self.CharacterTendency[0] * Target.CharacterTendency[0] == -1)
                Value -= 1;

            if (Self.CharacterTendency[1] * Target.CharacterTendency[1] == 1)
                Value += 1;
            else if (Self.CharacterTendency[1] * Target.CharacterTendency[1] == -1)
                Value -= 1;
        }
        else if (Reverse == true && Target != null)
        {
            if (Self.CharacterTendency[0] * Target.CharacterTendency[0] == 1)
                Value -= 1;
            else if (Self.CharacterTendency[0] * Target.CharacterTendency[0] == -1)
                Value += 1;

            if (Self.CharacterTendency[1] * Target.CharacterTendency[1] == 1)
                Value -= 1;
            else if (Self.CharacterTendency[1] * Target.CharacterTendency[1] == -1)
                Value += 1;
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
                Value += 4;
            else if (GC.Morale > 60)
                Value += 2;
            else if (GC.Morale < 40)
                Value -= 2;
            else if (GC.Morale < 20)
                Value -= 4;
        }
        else if (BonusType == 2)
        {
            if (GC.Morale > 80)
                Value += 2;
            else if (GC.Morale > 60)
                Value += 1;
            else if (GC.Morale < 40)
                Value -= 2;
            else if (GC.Morale < 20)
                Value -= 4;
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
        if (Target != null)
        {
            Self.InfoDetail.AddHistory(ResultText);
            if (ObjectText != "无")
            {
                Target.InfoDetail.AddHistory(ObjectText);
            }
            else
            {
                Target.InfoDetail.AddHistory(ResultText);
            }
        }
        else if (Targets.Count > 0)
        {
            Self.InfoDetail.AddHistory("自己与一群人参与了" + EventName + "事件" + ResultText);
            for (int i = 0; i < Targets.Count; i++)
            {
                Targets[i].InfoDetail.AddHistory("自己与一群人参与了" + EventName + "事件" + ResultText);
            }
        }
        else
            Self.InfoDetail.AddHistory(ResultText);

    }

    //复制当前类(利用新建基类覆盖 如new Event = Storage[num].Clone())
    public virtual Event Clone()
    {
        return (Event)this.MemberwiseClone();
    }
}

/// <summary>
/// 矛盾事件的基类，其他矛盾事件需要继承这个类
/// </summary>
public abstract class JudgeEvent : Event
{
    public override bool IsJudgeEvent { get { return true; } }

    public abstract void OnAccept();       //当玩家选择接受矛盾结果

    public abstract void OnRefuse();       //当玩家选择拒绝矛盾结果

    public override void Success(float Posb)
    {
        //子类如果重写，这里要保留（会弹出面板）
        EmpManager.Instance.JudgeEvent(this, true);
    }

    public override void MajorSuccess(float Posb)
    {
        //子类如果重写，这里要保留（会弹出面板）
        EmpManager.Instance.JudgeEvent(this, true);
    }

}

/// <summary>
/// 判定事件（要求转岗）
/// 命名随意，但要继承JudgeEvent
/// </summary>
public class JudgeEvent_01 : JudgeEvent
{
    public JudgeEvent_01() : base()
    {
        EventName = "要求转岗";

        //需求的建筑Add到BuildingRequires即可，如果没有需求什么都不写就行了
        BuildingRequires.Add(BuildingType.高管办公室);

        Weight = 3;               //设置事件默认权重
    }

    public override void OnAccept()
    {
        //持续转岗Buff
    }

    public override void OnRefuse()
    {
        //信念-10
    }
}

//20.12.7新增需要策划写的内容
public class Event_Test : Event
{
    public Event_Test() : base()
    {
        EventName = "测试用";
        //需求的建筑Add到BuildingRequires即可，如果没有需求什么都不写就行了
        BuildingRequires.Add(BuildingType.中央监控室);
        BuildingRequires.Add(BuildingType.产品部门);

        Weight = 3;               //设置事件默认权重
        PerkRemoveNumber = 5;     //要移除的特质
    }

    //假如有特质会影响事件发生的权重
    public override void SetWeight()
    {
        SetWeight(1, -1);  //员工有第1种特质，则权值-1
        SetWeight(5, 2);   //员工有第5种特质，则权值+2
    }
}

public class Event3_1 : Event
{
    public Event3_1() : base()
    {
        EventName = "扯皮";
        HaveTarget = true;
        Weight = 3;
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //if (Self.CurrentDep != null)
        //{
        //    if (Self.CurrentDep.DepFaith <= 30)
        //    {
                if (EmpManager.Instance.FindColleague(Self).Contains(Target) == true)
                {
                    return true;
                }
        //    }
        //}
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 93)
            {
                Extra -= 2;
            }
        }
        Extra += RelationBonus() + MoraleBonus() + EmotionBonus() + PerksBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void EventFinish()
    {
        base.EventFinish();
        EmpManager.Instance.ShowEventBubble(this);
    }

    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 10;
        Target.Mentality -= 10;
        for (int i = 0; i < 4; i++)
        {
            //Self.InfoDetail.AddPerk(new Perk49(Self), true);
        }
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "与同事" + Target.Name + "对于某个工作细节产生分歧，";
        ResultText += "双方互相不停指责,获得事件状态 悔恨*4，双方心力下降10点";
        GC.CreateMessage(Self.Name + "与同事" + Target.Name + "对于某个工作细节\n产生分歧，互相不停指责");
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.InfoDetail.AddPerk(new Perk52(Self), true);
        Target.InfoDetail.AddPerk(new Perk52(Target), true);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "与同事" + Target.Name + "对于某个工作细节产生分歧，";
        ResultText += "双方感到一丝烦恼";
        GC.CreateMessage(Self.Name + "与同事" + Target.Name + "对于某个工作细节\n产生分歧，双方感到一丝烦恼");
    }

}
public class Event3_2 : Event
{
    public Event3_2() : base()
    {
        EventName = "甩锅三连";
        HaveTarget = true;
        Weight = 3;
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //if (Self.CurrentDep != null)
        //{
        //    if (Self.CurrentDep.DepFaith <= 30)
        //    {
                if (EmpManager.Instance.FindBoss(Self) == Target | EmpManager.Instance.FindBoss(Target) == Self)
                {
                    return true;
                }
        //    }
        //}
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 93)
            {
                Extra -= 2;
            }
        }
        Extra += RelationBonus() + MoraleBonus() + EmotionBonus() + PerksBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void EventFinish()
    {
        base.EventFinish();
        EmpManager.Instance.ShowEventBubble(this);
    }

    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 10;
        Target.Mentality -= 10;
        if(EmpManager.Instance.FindBoss(Self) == Target)
        {
            for (int i = 0; i < 4; i++)
            {
                //Self.InfoDetail.AddPerk(new Perk49(Self), true);
                //Target.InfoDetail.AddPerk(new Perk49(Target), true);
            }
        }else if(EmpManager.Instance.FindBoss(Target) == Self)
        {
            for (int i = 0; i < 4; i++)
            {
                //Target.InfoDetail.AddPerk(new Perk49(Target), true);
            }
        }
        if (EmpManager.Instance.FindBoss(Self) == Target)
        {
            ResultText = "在" + SelfEntity.StandGridName() + "，下级" + Self.Name + "向上司" + Target.Name + "反应当前工作问题，";
        }
        else if (EmpManager.Instance.FindBoss(Target) == Self)
        {
            ResultText = "在" + SelfEntity.StandGridName() + "，下级" + Target.Name + "向上司" + Self.Name + "反应当前工作问题，";
        }
        ResultText += "上司将责任完全推给下级，下级获得事件状态 悔恨*4，双方心力下降10点";
        GC.CreateMessage(Self.Name+"与"+Target.Name+"互相甩锅");
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.InfoDetail.AddPerk(new Perk52(Self), true);
        Target.InfoDetail.AddPerk(new Perk52(Target), true);
        if (EmpManager.Instance.FindBoss(Self) == Target)
        {
            ResultText = "在" + SelfEntity.StandGridName() + "，下级" + Self.Name + "向上司" + Target.Name + "反应当前工作问题，";
        }
        else if (EmpManager.Instance.FindBoss(Target) == Self)
        {
            ResultText = "在" + SelfEntity.StandGridName() + "，下级" + Target.Name + "向上司" + Self.Name + "反应当前工作问题，";
        }
        ResultText += "上司表示自己负次要责任，双方获得事件状态 烦恼*1";
        GC.CreateMessage(Self.Name + "与" + Target.Name + "互相甩锅");
    }
}
public class Event3_3 : Event
{
    public Event3_3() : base()
    {
        EventName = "骄傲";
        HaveTarget = true;
        Weight = 3;
        RelationRequire = 0;
        SelfEmotionRequire = new List<EColor>() {EColor.LOrange};
    }
    public override bool SpecialCheck()
    {
        //if (Self.CurrentDep != null)
        //{
        //    if (Self.CurrentDep.DepFaith <= 30)
        //    {
                    return true;
        //    }
        //}
        //return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 93)
            {
                Extra -= 2;
            }
        }
        Extra += RelationBonus() + MoraleBonus() + EmotionBonus() + PerksBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void EventFinish()
    {
        base.EventFinish();
        EmpManager.Instance.ShowEventBubble(this);
    }

    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.InfoDetail.AddPerk(new Perk54(Self), true);
        Target.InfoDetail.AddPerk(new Perk54(Target), true);
        Self.Mentality -= 7;
        Target.Mentality -= 7;
        ResultText = "在" + SelfEntity.StandGridName() + "，由于产生骄傲情绪，" + Self.Name + "路过" + Target.Name + "，并向同事" + Target.Name + "分享起自己的人生智慧,";
        ResultText += Target.Name+ "指出了其中的错误，双方获得事件状态 尴尬*1，双方心力下降7点";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.InfoDetail.AddPerk(new Perk53(Self), true);
        Target.InfoDetail.AddPerk(new Perk53(Target), true);
        ResultText = "在" + SelfEntity.StandGridName() + "，由于产生骄傲情绪，" + Self.Name + "路过" + Target.Name + "，并向同事" + Target.Name + "分享起自己的人生智慧,";
        ResultText += Target.Name + "尴尬地默许了，双方获得事件状态 炫耀*1";
    }
}
public class Event3_4 : Event
{
    public Event3_4() : base()
    {
        EventName = "狂想";
        HaveTarget = true;
        Weight = 3;
        RelationRequire = 0;
        SelfEmotionRequire = new List<EColor>() { EColor.LOrange };
    }
    public override bool SpecialCheck()
    {
        //if (Self.CurrentDep != null)
        //{
        //    if (Self.CurrentDep.DepFaith <= 30)
        //    {
                return true;
        //    }
        //}
        //return false;
    }
    public override void EventFinish()
    {
        base.EventFinish();
        EmpManager.Instance.ShowEventBubble(this);
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 93)
            {
                Extra -= 2;
            }
        }
        Extra += RelationBonus() + MoraleBonus() + EmotionBonus() + PerksBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.InfoDetail.AddPerk(new Perk53(Self), true);
        ResultText = "在" + SelfEntity.StandGridName() + "，由于" + Self.Name + "进入一种狂想的状态，向附近的同事" + Target.Name + "说个不停，";
        ResultText += Target.Name + "隐约听到一些洞见，获得事件状态 顺利*1";
        GC.CreateMessage(Self.Name + "产生了狂想");
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        for (int i = 0; i < 3; i++)
        {
            //Self.InfoDetail.AddPerk(new Perk49(Self), true);
            //Target.InfoDetail.AddPerk(new Perk49(Target), true);
        }
        Self.Mentality -= 3;
        Target.Mentality -= 3;
        ResultText = "在" + SelfEntity.StandGridName() + "，由于" + Self.Name + "进入一种狂想的状态，向附近的同事" + Target.Name + "说个不停，";
        ResultText += Target.Name + "建议"+Self.Name+ "去看医生，获得事件状态 悔恨*3，双方心力下降3点";
        GC.CreateMessage(Self.Name + "产生了狂想");
    }
}
public class Event3_5 : Event
{
    public Event3_5() : base()
    {
        EventName = "讨论工作";
        HaveTarget = true;
        Weight = 3;
        RelationRequire = 0;
    }
    public override void EventFinish()
    {
        base.EventFinish();
        EmpManager.Instance.ShowEventBubble(this);
    }
    public override bool SpecialCheck()
    {
        //if (Self.CurrentDep != null)
        //{
        //    if (Self.CurrentDep.DepFaith >= 30)
        //    {
        //        if (Target.CurrentDep != null)
        //        {
        //            if (Target.CurrentDep.DepFaith >= 30)
        //            {
                        if (EmpManager.Instance.FindColleague(Self).Contains(Target))
                        {
                            return true;
                        }
        //            }
        //        }
        //    }
        //}
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 93)
            {
                Extra -= 2;
            }
        }
        Extra += RelationBonus() + MoraleBonus() + EmotionBonus() + PerksBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        //Self.InfoDetail.AddPerk(new Perk47(Self), true);
        bool huiyishi = false;
        for (int i = 0; i < GC.CurrentDeps.Count; i++)
        {
            foreach (BuildingType type in BuildingRequires)
            {
                if (GC.CurrentDeps[i].building.Type == BuildingType.会议室)
                {
                    huiyishi = true;
                }
            }
        }
        if (false)//原本是判断是否存在会议室
        {
            ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "与同事" + Target.Name + "在会议室一起讨论了工作，";
            ResultText += "双方就难点达成共识，获得事件状态 顺利*1，所在部门获得充分讨论*1";
            GC.CreateMessage(Self.Name + "与同事" + Target.Name + "就难点达成共识");
            if (Target.CurrentDep != null)
                Target.CurrentDep.AddPerk(new Perk71(null));
        }
        else
        {
            ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "与同事" + Target.Name + "一起讨论了工作，";
            ResultText += "双方就难点达成共识，获得事件状态 顺利*1";
            GC.CreateMessage(Self.Name + "与同事" + Target.Name + "就难点达成共识");
        }
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        //Self.InfoDetail.AddPerk(new Perk51(Self), true);
        //Target.InfoDetail.AddPerk(new Perk51(Target), true);
        Self.Mentality -= 7;
        Target.Mentality -= 7;
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "与同事" + Target.Name + "一起讨论了工作，";
        ResultText += "双方对目标产生分歧，双方获得事件状态 困惑*1，双方心力下降7点";
        GC.CreateMessage(Self.Name + "与同事" + Target.Name + "对目标产生分歧");
    }
}
public class Event3_6 : Event
{
    public Event3_6() : base()
    {
        EventName = "分配工作";
        HaveTarget = true;
        Weight = 3;
        RelationRequire = 0;
        //BuildingRequires = new List<BuildingType>() { BuildingType.会议室 };
    }
    public override bool SpecialCheck()
    {
        //if (Self.CurrentDep != null)
        //{
        //    if (Self.CurrentDep.DepFaith >= 30)
        //    {
        //        if (Target.CurrentDep != null)
        //        {
        //            if (Target.CurrentDep.DepFaith >= 30)
        //            {
                        if (EmpManager.Instance.FindBoss(Self) == Target | EmpManager.Instance.FindBoss(Target) == Self)
                        {
                            return true;
                        }
        //            }
        //        }
        //    }
        //}
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 93)
            {
                Extra -= 2;
            }
        }
        Extra += RelationBonus() + MoraleBonus() + EmotionBonus() + PerksBonus();
        return Extra;
    }
    public override void EventFinish()
    {
        base.EventFinish();
        EmpManager.Instance.ShowEventBubble(this);
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        string shuchu = "";
        base.Success(Posb);

        if (EmpManager.Instance.FindBoss(Self) == Target)
        {
            for (int i = 0; i < 2; i++)
            {
                //Self.InfoDetail.AddPerk(new Perk47(Self), true);
                //Target.InfoDetail.AddPerk(new Perk47(Target), true);
            }
        }
        else if (EmpManager.Instance.FindBoss(Target) == Self)
        {
            for (int i = 0; i < 2; i++)
            {
                //Target.InfoDetail.AddPerk(new Perk47(Target), true);
            }
        }
        for (int i = 0; i < 2; i++)
        {
            //Self.InfoDetail.AddPerk(new Perk47(Self), true);
            //Target.InfoDetail.AddPerk(new Perk47(Target), true);
        }
        bool huiyishi = false;
        for (int i = 0; i < GC.CurrentDeps.Count; i++)
        {
            foreach (BuildingType type in BuildingRequires)
            {
                if (GC.CurrentDeps[i].building.Type == BuildingType.会议室)
                {
                    huiyishi = true;
                }
            }
        }
        if (false)//原本是判断是否存在会议室
        {
            if (EmpManager.Instance.FindBoss(Self) == Target)
            {
                ResultText = "上司" + Target.Name + "在会议室向下级" + Self.Name + "提出下一阶段工作计划，";
                shuchu = "上司" + Target.Name;
            }
            else if (EmpManager.Instance.FindBoss(Target) == Self)
            {
                ResultText = "上司" + Self.Name + "在会议室向下级" + Target.Name + "提出下一阶段工作计划，";
                shuchu = "上司" + Self.Name;
            }
            if (EmpManager.Instance.FindBoss(Self) == Target)
            {
                if (Self.CurrentDep != null)
                    Self.CurrentDep.AddPerk(new Perk71(null));
                ResultText += "下级" + Self.Name + "表示理解并认可，下级获得事件状态 顺利*2，所在部门获得充分讨论*1";
                shuchu += "给" + Self.Name + "分配工作并取得了认可";
            }
            else if (EmpManager.Instance.FindBoss(Target) == Self)
            {
                if (Target.CurrentDep != null)
                    Target.CurrentDep.AddPerk(new Perk71(null));
                ResultText += "下级" + Target.Name + "表示理解并认可，下级获得事件状态 顺利*2，所在部门获得充分讨论*1";
                shuchu += "给" + Target.Name + "分配工作并取得了认可";
            }
        }
        else
        {
            if (EmpManager.Instance.FindBoss(Self) == Target)
            {
                ResultText = "上司" + Target.Name + "向下级" + Self.Name + "提出下一阶段工作计划，";
                shuchu = "上司" + Target.Name;
            }
            else if (EmpManager.Instance.FindBoss(Target) == Self)
            {
                ResultText = "上司" + Self.Name + "向下级" + Target.Name + "提出下一阶段工作计划，";
                shuchu = "上司" + Self.Name;
            }
            if (EmpManager.Instance.FindBoss(Self) == Target)
            {
                ResultText += "下级" + Self.Name + "表示理解并认可，下级获得事件状态 顺利*2";
                shuchu += "给" + Self.Name + "分配工作并取得了认可";
            }
            else if (EmpManager.Instance.FindBoss(Target) == Self)
            {
                ResultText += "下级" + Target.Name + "表示理解并认可，下级获得事件状态 顺利*2";
                shuchu += "给" + Target.Name + "分配工作并取得了认可";
            }
        }
        GC.CreateMessage(shuchu);
    }
    public override void Failure(float Posb)
    {
        string shuchu = "";
        base.Failure(Posb);
        Self.InfoDetail.AddPerk(new Perk52(Self), true);
        Target.InfoDetail.AddPerk(new Perk52(Target), true);
        Self.Mentality -= 7;
        Target.Mentality -= 7;
        if (EmpManager.Instance.FindBoss(Self) == Target)
        {
            ResultText = "上司" + Target.Name + "向下级" + Self.Name + "提出下一阶段工作计划，";
            shuchu = "上司" + Target.Name;
        }
        else if (EmpManager.Instance.FindBoss(Target) == Self)
        {
            ResultText = "上司" + Self.Name + "向下级" + Target.Name + "提出下一阶段工作计划，";
            shuchu = "上司" + Self.Name;
        }
        if (EmpManager.Instance.FindBoss(Self) == Target)
        {
            ResultText += "下级"+Self.Name+"认为不属于自己的工作范围，双方获得事件状态 烦恼*1，双方心力下降7点";
            shuchu += "给" + Self.Name + "分配工作但没有取得认可";
        }
        else if (EmpManager.Instance.FindBoss(Target) == Self)
        {
            ResultText += "下级"+Target.Name+"认为不属于自己的工作范围，双方获得事件状态 烦恼*1，双方心力下降7点";
            shuchu += "给" +Target.Name + "分配工作但没有取得认可";
        }
        GC.CreateMessage(shuchu);
    }
}
public class Event3_7 : Event
{
    public Event3_7() : base()
    {
        EventName = "开会商讨";
        HaveTarget = true;
        Weight = 3;
        RelationRequire = 0;
        //BuildingRequires = new List<BuildingType>() { BuildingType.会议室 };
    }
    public override bool SpecialCheck()
    {
        //if (Self.CurrentDep != null)
        //{
        //    if (Self.CurrentDep.DepFaith >= 70)
        //    {
        //        if (Target.CurrentDep != null)
        //        {
                    //if (Target.CurrentDep.DepFaith >= 70)
                    //{
                        if (EmpManager.Instance.FindBoss(Self) == Target | EmpManager.Instance.FindBoss(Target) == Self)
                        {
                            return true;
                        }
                    //}
        //        }
        //    }
        //}
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 93)
            {
                Extra -= 2;
            }
        }
        Extra += RelationBonus() + MoraleBonus() + EmotionBonus() + PerksBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void EventFinish()
    {
        base.EventFinish();
        EmpManager.Instance.ShowEventBubble(this);
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);

        if (EmpManager.Instance.FindBoss(Self) == Target)
        {
            for (int i = 0; i < 4; i++)
            {
                //Self.InfoDetail.AddPerk(new Perk47(Self), true);
                //Target.InfoDetail.AddPerk(new Perk47(Target), true);
            }
        }
        else if (EmpManager.Instance.FindBoss(Target) == Self)
        {
            for (int i = 0; i < 4; i++)
            {
                //Target.InfoDetail.AddPerk(new Perk47(Target), true);
            }
        }
        bool huiyishi = false;
        for (int i = 0; i < GC.CurrentDeps.Count; i++)
        {
            foreach (BuildingType type in BuildingRequires)
            {
                if (GC.CurrentDeps[i].building.Type == BuildingType.会议室)
                {
                    huiyishi = true;
                }
            }
        }
        if (false)//原本是判断是否存在会议室
        {
            if (EmpManager.Instance.FindBoss(Self) == Target)
            {
                if (Self.CurrentDep != null)
                    Self.CurrentDep.AddPerk(new Perk71(null));
                ResultText = "上司" + Target.Name + "约下级" + Self.Name + "在会议室商讨工作流程";
            }
            else if (EmpManager.Instance.FindBoss(Target) == Self)
            {
                if (Target.CurrentDep != null)
                    Target.CurrentDep.AddPerk(new Perk71(null));
                ResultText = "上司" + Self.Name + "约下级" + Target.Name + "在会议室商讨工作流程";
            }
            ResultText += "双方进行极富成效地流程梳理！下级获得事件状态 顺利*4，下级所在部门获得充分讨论*1";
            GC.CreateMessage(Self.Name + "与" + Target.Name + "进行了极富成效地流程梳理");
        }
        else
        {
            if (EmpManager.Instance.FindBoss(Self) == Target)
            {
                ResultText = "上司" + Target.Name + "约下级" + Self.Name + "商讨工作流程，";
            }
            else if (EmpManager.Instance.FindBoss(Target) == Self)
            {
                ResultText = "上司" + Self.Name + "约下级" + Target.Name + "商讨工作流程，";
            }
            ResultText += "双方进行极富成效地流程梳理！下级获得事件状态 顺利*4";
            GC.CreateMessage(Self.Name + "与" + Target.Name + "进行了极富成效地流程梳理");
        }
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        //Self.InfoDetail.AddPerk(new Perk51(Self), true);
        //Target.InfoDetail.AddPerk(new Perk51(Target), true);
        Self.Mentality -= 3;
        Target.Mentality -= 3;
        if (EmpManager.Instance.FindBoss(Self) == Target)
        {
            ResultText = "上司" + Target.Name + "约下级" + Self.Name + "商讨工作流程，";
        }
        else if (EmpManager.Instance.FindBoss(Target) == Self)
        {
            ResultText = "上司" + Self.Name + "约下级" + Target.Name + "商讨工作流程，";
        }
        ResultText += "双方暂未达成共识，双方获得事件状态 困惑*1，双方心力下降3点";
        GC.CreateMessage(Self.Name + "与" + Target.Name + "分配工作暂未达成共识");
    }
}
public class Event3_8 : Event
{
    public Event3_8() : base()
    {
        EventName = "互相协助";
        HaveTarget = true;
        Weight = 3;
        RelationRequire = 0;
        //BuildingRequires = new List<BuildingType>() { BuildingType.会议室 };
    }
    public override bool SpecialCheck()
    {
        //if (Self.CurrentDep != null)
        //{
        //    if (Self.CurrentDep.DepFaith >= 70)
        //    {
        //        if (Target.CurrentDep != null)
        //        {
        //            if (Target.CurrentDep.DepFaith >= 70)
        //            {
                        if (EmpManager.Instance.FindColleague(Self).Contains(Target) == true)
                        {
                            return true;
                        }
        //            }
        //        }
        //    }
        //}
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 93)
            {
                Extra -= 2;
            }
        }
        Extra += RelationBonus() + MoraleBonus() + EmotionBonus() + PerksBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void EventFinish()
    {
        base.EventFinish();
        EmpManager.Instance.ShowEventBubble(this);
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        for (int i = 0; i < 2; i++)
        {
            //Self.InfoDetail.AddPerk(new Perk47(Self), true);
            //Target.InfoDetail.AddPerk(new Perk47(Target), true);
        }
        bool huiyishi = false;
        for (int i = 0; i < GC.CurrentDeps.Count; i++)
        {
            foreach (BuildingType type in BuildingRequires)
            {
                if (GC.CurrentDeps[i].building.Type == BuildingType.会议室)
                {
                    huiyishi = true;
                }
            }
        }
        if (false)//原本是判断是否存在会议室
        {
            if (Target.CurrentDep != null)
                Target.CurrentDep.AddPerk(new Perk71(null));
            ResultText = Self.Name + "约同事" + Target.Name + "在会议室商量工作计划";
            ResultText += "双方分工明确，获得事件状态 顺利*2，所在部门获得充分讨论*1";
            GC.CreateMessage(Self.Name + "约同事" + Target.Name + "\n在会议室商量工作计划\n双方分工明确");
        }
        else
        {
            ResultText = Self.Name + "约同事" + Target.Name + "商量工作计划";
            ResultText += "双方分工明确，获得事件状态 顺利*2";
            GC.CreateMessage(Self.Name + "约同事" + Target.Name + "\n在会议室商量工作计划\n双方分工明确");
        }
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        //Self.InfoDetail.AddPerk(new Perk51(Self), true);
        //Target.InfoDetail.AddPerk(new Perk51(Target), true);
        Self.Mentality -= 3;
        Target.Mentality -= 3;
        ResultText = Self.Name + "约同事" + Target.Name + "商量工作计划";
        ResultText += "双方存在分歧，双方获得事件状态 困惑*1，双方心力下降3点";
        GC.CreateMessage(Self.Name + "约同事" + Target.Name + "\n在会议室商量工作计划\n双方存在分歧");
    }
}
public class Event3_9 : Event
{
    public Event3_9() : base()
    {
        EventName = "工作瓶颈";
        HaveTarget = true;
        Weight = 3;
        RelationRequire = 0;
        HaveTarget = false;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        //Self.InfoDetail.AddPerk(new Perk51(Self), true);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "独自工作遇到瓶颈";
        ResultText += "只好绕过问题，获得事件状态 困惑*1";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        //Self.InfoDetail.AddPerk(new Perk49(Self), true);
        Self.Mentality -= 5;
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "独自工作遇到瓶颈";
        ResultText += "导致无法有效推进，获得事件状态 悔恨*1，心力下降5点";
    }
}
public class Event3_10 : Event
{
    public Event3_10() : base()
    {
        EventName = "诉求不满";
        HaveTarget = true;
        Weight = 3;
        RelationRequire = 0;
        HaveTarget = false;
    }
    public override void SetWeight()
    {
        SetWeight(85, 1);
    }
    public override bool SpecialCheck()
    {
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            Perk perk = Self.InfoDetail.PerksInfo[i].CurrentPerk;
            if (perk.Num == 52 || perk.Num == 54 || perk.Num == 68 || perk.Num == 69)
            {
                return true;
            }
        }
        return false;
    }

    public override void StartSolve()
    {
        base.StartSolve();
        foreach (PerkInfo info in Self.InfoDetail.PerksInfo)
        {
            if (info.CurrentPerk.Num == perkUsed)
            {
                perkUsedName = info.CurrentPerk.Name;
                info.CurrentPerk.RemoveEffect();
                return;
            }
        }
    }

    public override int FindResult()
    {
        return 3;
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        //Self.InfoDetail.AddPerk(new Perk51(Self), true);
        
        ResultText = "由于" + perkUsedName + "的情绪在" + Self.Name + "心中的持续酝酿，" + Self.Name + "渐渐感到对公司不满，获得事件状态 不满*1，消除事件状态" + perkUsedName + "×1";
    }

    public override Event Clone()
    {
        Event clone = (Event)this.MemberwiseClone();

        List<int> pList = new List<int>();
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            Perk perk = Self.InfoDetail.PerksInfo[i].CurrentPerk;
            if (perk.Num == 52 || perk.Num == 54 || perk.Num == 68 || perk.Num == 69)
            {
                pList.Add(perk.Num);
            }
        }
        clone.perkUsed = pList[Random.Range(0, pList.Count)];

        return clone;
    }
}
public class Event3_11 : Event
{
    public Event3_11() : base()
    {
        EventName = "有效工作";
        HaveTarget = true;
        Weight = 3;
        RelationRequire = 0;
        HaveTarget = false;
    }
    public override bool SpecialCheck()
    {
        if (Self.Mentality >= 30)
        {
            return true;
        }
        return false;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        //Self.InfoDetail.AddPerk(new Perk3(Self), true);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "独自工作进展顺利，";
        ResultText += "并受到了一些启发，获得事件状态 受到启发*1";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.InfoDetail.AddPerk(new Perk52(Self), true);
        Self.Mentality -= 3;
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "独自工作进展顺利，";
        ResultText += "但遇到了一些小困难，获得事件状态 烦恼*1，心力下降3点";
    }


}
public class Event3_12 : Event
{
    public Event3_12() : base()
    {
        EventName = "成就成长";
        HaveTarget = true;
        Weight = 3;
        RelationRequire = 0;
        HaveTarget = false;
    }
    public override bool SpecialCheck()
    {
        List<int> pList = new List<int>();
        //if (Self.Mentality < 60)
        //{
        //    return false;
        //}

        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            Perk perk = Self.InfoDetail.PerksInfo[i].CurrentPerk;
            if ((perk.Num == 47 && perk.Level >= 2) || perk.Num == 51 || perk.Num == 65 || perk.Num == 69) 
            {
                return true;
            }
        }
        return false;
    }

    public override void StartSolve()
    {
        base.StartSolve();
        foreach (PerkInfo info in Self.InfoDetail.PerksInfo)
        {
            if (info.CurrentPerk.Num == perkUsed)
            {
                perkUsedName = info.CurrentPerk.Name;
                if (perkUsed == 47 && info.CurrentPerk.Level >= 2)
                {
                    info.CurrentPerk.RemoveEffect();
                    info.CurrentPerk.RemoveEffect();
                    return;
                }
                else
                {
                    info.CurrentPerk.RemoveEffect();
                    return;
                }
            }
        }
    }

    public override int FindResult()
    {
        return 3;
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.InfoDetail.AddPerk(new Perk55(Self), true);
        if(perkUsed == 47)
        {
            ResultText = "由于" + perkUsedName + "的情绪在" + Self.Name + "心中的持续酝酿，" + Self.Name + "产生了新的愿望，获得事件状态 愿望*1，消除事件状态" + perkUsedName + "×2";
        }else
        {
            ResultText = "由于" + perkUsedName + "的情绪在" + Self.Name + "心中的持续酝酿，" + Self.Name + "产生了新的愿望，获得事件状态 愿望*1，消除事件状态" + perkUsedName + "×1";
        }
    }


    public override Event Clone()
    {
        Event clone = (Event)this.MemberwiseClone();

           List<int> pList = new List<int>();
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            Perk perk = Self.InfoDetail.PerksInfo[i].CurrentPerk;
            if ((perk.Num == 47 && perk.Level >= 2) || perk.Num == 51 || perk.Num == 65 || perk.Num == 69)
            {
                pList.Add(perk.Num);
            }
        }
        clone.perkUsed = pList[Random.Range(0, pList.Count)];

        return clone;
    }
}
public class Event3_13 : Event
{
    public Event3_13() : base()
    {
        EventName = "野心";
        HaveTarget = true;
        Weight = 3;
        RelationRequire = 0;
        HaveTarget = false;
    }
    public override void SetWeight()
    {
        SetWeight(83, 1);
    }
    public override bool SpecialCheck()
    {
        List<int> pList = new List<int>();
        //if (Self.Mentality < 60)
        //{
        //    return false;
        //}

        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            Perk perk = Self.InfoDetail.PerksInfo[i].CurrentPerk;
            if ((perk.Num == 49 && perk.Level >= 2) || perk.Num == 53 || perk.Num == 66 || perk.Num == 69)
            {
                return true;
            }
        }
        return false;
    }

    public override Event Clone()
    {
        Event clone = (Event)this.MemberwiseClone();

        List<int> pList = new List<int>();
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            Perk perk = Self.InfoDetail.PerksInfo[i].CurrentPerk;
            if ((perk.Num == 49 && perk.Level >= 2) || perk.Num == 53 || perk.Num == 66 || perk.Num == 69)
            {
                pList.Add(perk.Num);
            }
        }
        clone.perkUsed = pList[Random.Range(0, pList.Count)];

        return clone;
    }
    public override void StartSolve()
    {
        base.StartSolve();
        foreach (PerkInfo info in Self.InfoDetail.PerksInfo)
        {
            if (info.CurrentPerk.Num == perkUsed)
            {
                perkUsedName = info.CurrentPerk.Name;
                if(perkUsed == 49 && info.CurrentPerk.Level >= 2)
                {
                    info.CurrentPerk.RemoveEffect();
                    info.CurrentPerk.RemoveEffect();
                    return;
                }
                else
                {
                    info.CurrentPerk.RemoveEffect();
                    return;
                }
            }
        }
    }
    public override int FindResult()
    {
        return 3;
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.InfoDetail.AddPerk(new Perk56(Self), true);
        if (perkUsed == 49)
        {
            ResultText = "由于" + perkUsedName + "的情绪在" + Self.Name + "心中的持续酝酿，野心在" + Self.Name + "的内心萌发，获得事件状态 野心*1，消除事件状态" + perkUsedName + "×2";
        }
        else
        {
            ResultText = "由于" + perkUsedName + "的情绪在" + Self.Name + "心中的持续酝酿，野心在" + Self.Name + "的内心萌发，获得事件状态 野心*1，消除事件状态" + perkUsedName + "×1";
        }
    }
}
public class Event3_14 : Event
{
    public Event3_14() : base()
    {
        EventName = "理想";
        HaveTarget = true;
        Weight = 4;
        RelationRequire = 0;
    }
    public override void SetWeight()
    {
        SetWeight(84, 1);
    }
    public override bool SpecialCheck()
    {
        List<int> pList = new List<int>();
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            if(Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 70)
            {
                return false;
            }else if(Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 55)
            {
                pList.Add(i);
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 58& Self.InfoDetail.PerksInfo[i].CurrentPerk.Level>=2)
            {
                pList.Add(i);
            }
        }
        if (pList.Count >= 2)
        {
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += RelationBonus() + MoraleBonus() + EmotionBonus() + PerksBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 12)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.InfoDetail.AddPerk(new Perk70(Self), true);
        //Self.InfoDetail.AddPerk(new Perk3(Self), true);
        PerkInfo yuanwang = null, chengzhang = null;
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 55)
            {
                yuanwang = Self.InfoDetail.PerksInfo[i];
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 58 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 2)
            {
                chengzhang = Self.InfoDetail.PerksInfo[i];
            }
        }
        if (yuanwang)
        {
            yuanwang.CurrentPerk.RemoveEffect();
        }
        if (chengzhang)
        {
            chengzhang.CurrentPerk.RemoveEffect();
            chengzhang.CurrentPerk.RemoveEffect();
        }
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "与" + Target.Name + "交流着职场生涯";
        ResultText += Self.Name + "渐渐意识到自己的理想,获得事件状态 理想*1，受到启发*1，消除事件状态：愿望*1 成长*2  ";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.InfoDetail.AddPerk(new Perk69(Self), true);
        Self.InfoDetail.AddPerk(new Perk107(Self), true);
        Self.Mentality -= 8;
        PerkInfo yuanwang = null, chengzhang = null;
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 55)
            {
                yuanwang = Self.InfoDetail.PerksInfo[i];
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 58 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 2)
            {
                chengzhang = Self.InfoDetail.PerksInfo[i];
            }
        }
        if (yuanwang)
        {
            yuanwang.CurrentPerk.RemoveEffect();
        }
        if (chengzhang)
        {
            chengzhang.CurrentPerk.RemoveEffect();
            chengzhang.CurrentPerk.RemoveEffect();
        }
        if (Target.CurrentDep != null)
            Target.CurrentDep.AddPerk(new Perk113(null));
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "与" + Target.Name + "交流着职场生涯";
        ResultText +=  Self.Name +"被"+Target.Name+ "嘲笑是妄想家，获得事件状态 遭到敌意*1，遗憾×1,己方信念下降8,消除事件状态： 愿望*1 成长*2，部门获得“产生矛盾”状态×1";
    }
}
public class Event3_15 : Event
{
    public Event3_15() : base()
    {
        EventName = "讨论理想";
        HaveTarget = true;
        Weight = 2;
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        List<int> pList = new List<int>();
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 55)
            {
                pList.Add(i);
            }else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 70)
            {
                pList.Add(i);
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 58 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 2)
            {
                pList.Add(i);
            }
        }
        if (pList.Count >= 3)
        {
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += RelationBonus() + MoraleBonus() + EmotionBonus() + PerksBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 12)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.InfoDetail.AddPerk(new Perk55(Self), true);
        Self.InfoDetail.AddPerk(new Perk67(Self), true);
        PerkInfo yuanwang = null, chengzhang = null;
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 55)
            {
                yuanwang = Self.InfoDetail.PerksInfo[i];
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 58 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 2)
            {
                chengzhang = Self.InfoDetail.PerksInfo[i];
            }
        }
        if (yuanwang)
        {
            yuanwang.CurrentPerk.RemoveEffect();
        }
        if (chengzhang)
        {
            chengzhang.CurrentPerk.RemoveEffect();
            chengzhang.CurrentPerk.RemoveEffect();
        }
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "与" + Target.Name + "讨论理想";
        if (Random.Range(0.0f, 1.0f) <= 0.0f)
        {
            Self.InfoDetail.AddPerk(new Perk107(Self), true);
            ResultText += Target.Name + "对" + Self.Name + "表示赞许,获得事件状态 愿望*1，受到赞扬*1,遗憾*1，消除事件状态：愿望*1 成长*2 ";
        }
        else
        {
            ResultText += Target.Name + "对" + Self.Name + "表示赞许,获得事件状态 愿望*1，受到赞扬*1，消除事件状态：愿望*1 成长*2 ";
        }
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.InfoDetail.AddPerk(new Perk68(Self), true);
        Self.Mentality -= 5;
        PerkInfo yuanwang = null, chengzhang = null;
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 55)
            {
                yuanwang = Self.InfoDetail.PerksInfo[i];
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 58 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 2)
            {
                chengzhang = Self.InfoDetail.PerksInfo[i];
            }
        }
        if (yuanwang)
        {
            yuanwang.CurrentPerk.RemoveEffect();
        }
        if (chengzhang)
        {
            chengzhang.CurrentPerk.RemoveEffect();
            chengzhang.CurrentPerk.RemoveEffect();
        }
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "与" + Target.Name + "讨论理想";
        if (Random.Range(0.0f, 1.0f)<=1.0f)
        {
            Self.InfoDetail.AddPerk(new Perk107(Self), true);
            ResultText += Target.Name + "认为" + Self.Name + "只管赚钱就好，获得事件状态 受到批评*1,遗憾*1，己方信念下降5，消除事件状态：愿望*1 成长*2 ";
        }
        else
        {
            ResultText += Target.Name + "认为" + Self.Name + "只管赚钱就好，获得事件状态 受到批评*1，己方信念下降5，消除事件状态：愿望*1 成长*2 ";
        }
        if (Target.CurrentDep != null)
            Target.CurrentDep.AddPerk(new Perk71(null));
        ResultText += "，部门获得“产生矛盾”状态×1";
    }
}
public class Event3_16 : JudgeEvent
{
    public Event3_16() : base()
    {
        EventName = "要求转岗";
        HaveTarget = true;
        Weight = 3;
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        if(EmpManager.Instance.FindBoss(Self) != Target)
        {
            return false;
        }
        List<int> pList = new List<int>();
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 55 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 2)
            {
                pList.Add(i);
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 70)
            {
                pList.Add(i);
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 58 )
            {
                pList.Add(i);
            }
        }
        if (pList.Count >= 3)
        {
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += RelationBonus() + MoraleBonus() + EmotionBonus() + PerksBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 12)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.InfoDetail.AddPerk(new Perk55(Self), true);
        Self.InfoDetail.AddPerk(new Perk67(Self), true);
        PerkInfo yuanwang = null, chengzhang = null;
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 55)
            {
                yuanwang = Self.InfoDetail.PerksInfo[i];
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 58 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 1)
            {
                chengzhang = Self.InfoDetail.PerksInfo[i];
            }
        }
        if (yuanwang)
        {
            yuanwang.CurrentPerk.RemoveEffect();
        }
        if (chengzhang)
        {
            chengzhang.CurrentPerk.RemoveEffect();
        }
        //弹出窗口询问玩家
        bool canAccept = true;
        foreach(PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if(p.CurrentPerk.Num == 72)
            {
                canAccept = false;
                //这种情况下不能接受
            }
        }
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "希望上司" + Target.Name + "为其转岗";
        if (Random.Range(0.0f, 1.0f) <= 0.0f)
        {
            Self.InfoDetail.AddPerk(new Perk107(Self), true);
            ResultText += "上司" + Target.Name + "表示赞许和支持，并上报CEO，获得事件状态 受到赞扬*1,遗憾*1，消除事件状态：愿望*1 成长*1 ";
        }
        else
        {
            ResultText += "上司" + Target.Name + "表示赞许和支持，并上报CEO，获得事件状态 受到赞扬*1，消除事件状态：愿望*1 成长*1 ";
        }
        GC.CreateMessage(Self.Name + "请求上司" + Target.Name+ "为其转岗，\n上司支持并上报CEO");
        EmpManager.Instance.JudgeEvent(this, canAccept);
    }
    public override void OnAccept()
    {
        Self.InfoDetail.AddPerk(new Perk72(Self), true);
        Self.InfoDetail.AddHistory("CEO答应了 " + Self.Name + " 的转岗请求，员工获得状态“期待转岗”，限时2个月");
    }
    public override void OnRefuse()
    {
        Self.ChangeCharacter(4, -10);
        GC.CreateMessage(Self.Name + "请求转岗被CEO拒绝，信念-8");
        Self.InfoDetail.AddHistory(Self.Name + "请求转岗被CEO拒绝，信念-8");
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.InfoDetail.AddPerk(new Perk68(Self), true);
        Self.Mentality -= 10;
        PerkInfo yuanwang = null, chengzhang = null;
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 55)
            {
                yuanwang = Self.InfoDetail.PerksInfo[i];
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 58 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 1)
            {
                chengzhang = Self.InfoDetail.PerksInfo[i];
            }
        }
        if (yuanwang)
        {
            yuanwang.CurrentPerk.RemoveEffect();
        }
        if (chengzhang)
        {
            chengzhang.CurrentPerk.RemoveEffect();
        }
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "希望上司" + Target.Name + "为其转岗";
        if (Random.Range(0.0f, 1.0f) <= 1.0f)
        {
            Self.InfoDetail.AddPerk(new Perk107(Self), true);
            ResultText += "上司" + Target.Name + "拒绝了" + Self.Name + "的请求，并叮嘱好好工作,获得事件状态 受到批评*1,遗憾*1，己方信念下降10，消除事件状态：愿望*1 成长*1 ";
        }
        else
        {
            ResultText += "上司" + Target.Name + "拒绝了" + Self.Name + "的请求，并叮嘱好好工作,获得事件状态 受到批评*1，己方信念下降10，消除事件状态：愿望*1 成长*1 ";
        }
        GC.CreateMessage(Self.Name + "请求上司" + Target.Name + "为其转岗，\n被上司拒绝");
        if (Target.CurrentDep != null)
        {
            Target.CurrentDep.AddPerk(new Perk113(null));
            Target.CurrentDep.AddPerk(new Perk113(null));
        }
        ResultText += "，部门获得“产生矛盾”状态×2";
    }
}
public class Event3_17 : Event
{
    public Event3_17() : base()
    {
        EventName = "讨论转岗";
        HaveTarget = true;
        Weight = 3;
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        if (EmpManager.Instance.FindColleague(Self).Contains(Target) == false)
        {
            return false;
        }
        List<int> pList = new List<int>();
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 55 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 1)
            {
                pList.Add(i);
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 58 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 2)
            {
                pList.Add(i);
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 107)
            {
                pList.Add(i);
            }
        }
        if (pList.Count >= 3)
        {
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += RelationBonus() + MoraleBonus() + EmotionBonus() + PerksBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 12)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        //Self.InfoDetail.AddPerk(new Perk3(Self), true);
        Self.InfoDetail.AddPerk(new Perk55(Self), true);
        PerkInfo yuanwang = null, chengzhang = null, yihan = null;
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 55)
            {
                yuanwang = Self.InfoDetail.PerksInfo[i];
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 58 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 1)
            {
                chengzhang = Self.InfoDetail.PerksInfo[i];
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 107 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 1)
            {
                yihan = Self.InfoDetail.PerksInfo[i];
            }
        }
        if (yuanwang)
        {
            yuanwang.CurrentPerk.RemoveEffect();
        }
        if (chengzhang)
        {
            chengzhang.CurrentPerk.RemoveEffect();
        }
        if (yihan)
        {
            yihan.CurrentPerk.RemoveEffect();
        }
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "与同事" + Target.Name + "讨论转岗到其他部门";
        if (Random.Range(0.0f, 1.0f) <= 0.0f)
        {
            Self.InfoDetail.AddPerk(new Perk107(Self), true);
            ResultText += "同事" + Target.Name + "赞许" + Self.Name + "的勇气，获得事件状态 受到启发*1，愿望*1,遗憾*1，消除事件状态：愿望*1 成长*1 遗憾*1 ";
        }
        else
        {
            ResultText += "同事" + Target.Name + "赞许" + Self.Name + "的勇气，获得事件状态 受到启发*1，愿望*1，消除事件状态：愿望*1 成长*1 遗憾*1 ";
        }
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.InfoDetail.AddPerk(new Perk69(Self), true);
        Self.Mentality -= 8;
        PerkInfo yuanwang = null, chengzhang = null;
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 55)
            {
                yuanwang = Self.InfoDetail.PerksInfo[i];
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 58 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 1)
            {
                chengzhang = Self.InfoDetail.PerksInfo[i];
            }
        }
        if (yuanwang)
        {
            yuanwang.CurrentPerk.RemoveEffect();
        }
        if (chengzhang)
        {
            chengzhang.CurrentPerk.RemoveEffect();
        }
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "与同事" + Target.Name + "讨论转岗到其他部门";
        if (Random.Range(0.0f, 1.0f) <= 1.0f)
        {
            Self.InfoDetail.AddPerk(new Perk107(Self), true);
            ResultText += "同事" + Target.Name + "取笑" + Self.Name + "的鲁莽，获得事件状态 遭到敌意*1，遗憾*1，己方信念下降8，消除事件状态：愿望*1 成长*1 ";
        }
        else
        {
            ResultText += "同事" + Target.Name + "取笑" + Self.Name + "的鲁莽，获得事件状态 遭到敌意*1，己方信念下降8，消除事件状态：愿望*1 成长*1 ";
        }
        if (Target.CurrentDep != null)
            Target.CurrentDep.AddPerk(new Perk113(null));
        ResultText += "，部门获得“产生矛盾”状态×1";
    }
}
public class Event3_18 : JudgeEvent
{
    public Event3_18() : base()
    {
        EventName = "请求升职";
        HaveTarget = true;
        Weight = 5;
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        if (EmpManager.Instance.FindBoss(Self) != Target)
        {
            return false;
        }
        List<int> pList = new List<int>();
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 55 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 1)
            {
                pList.Add(i);
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 58 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 2)
            {
                pList.Add(i);
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 107 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 2)
            {
                pList.Add(i);
            }
        }
        if (pList.Count >= 3 )
        {
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += RelationBonus() + MoraleBonus() + EmotionBonus() + PerksBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 12)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.InfoDetail.AddPerk(new Perk65(Self), true);
        PerkInfo yuanwang = null, chengzhang = null, yihan = null;
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 55)
            {
                yuanwang = Self.InfoDetail.PerksInfo[i];
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 58 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 2)
            {
                chengzhang = Self.InfoDetail.PerksInfo[i];
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 107 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 2)
            {
                yihan = Self.InfoDetail.PerksInfo[i];
            }
        }
        if (yuanwang)
        {
            yuanwang.CurrentPerk.RemoveEffect();
        }
        if (chengzhang)
        {
            chengzhang.CurrentPerk.RemoveEffect();
            chengzhang.CurrentPerk.RemoveEffect();
        }
        if (yihan)
        {
            yihan.CurrentPerk.RemoveEffect();
            yihan.CurrentPerk.RemoveEffect();
        }
        //弹出窗口询问玩家
        bool canAccept = true;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 72)
            {
                canAccept = false;
                //这种情况下不能接受
            }
        }
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "希望得到上司" + Target.Name + "的升职推荐";
        if (Random.Range(0.0f, 1.0f) <= 0.0f)
        {
            Self.InfoDetail.AddPerk(new Perk107(Self), true);
            ResultText += "上司肯定其成长，并上报CEO，获得事件状态 受到信任*1,遗憾*1，消除事件状态：愿望*1 成长*4 遗憾*2";
        }
        else
        {
            ResultText += "上司肯定其成长，并上报CEO，获得事件状态 受到信任*1，消除事件状态：愿望*1 成长*4 遗憾*2";
        }
        GC.CreateMessage(Self.Name+"请求升职被上司接受，请求CEO");
        EmpManager.Instance.JudgeEvent(this, canAccept);
    }
    public override void OnAccept()
    {
        Self.InfoDetail.AddPerk(new Perk73(Self), true);
        Self.InfoDetail.AddHistory("CEO答应了 "+Self.Name + " 的升职请求，员工获得状态“期待升职”，限时2个月");
    }
    public override void OnRefuse()
    {
        Self.ChangeCharacter(4, -8);
        GC.CreateMessage(Self.Name + "请求升职被CEO拒绝，信念-8");
        Self.InfoDetail.AddHistory(Self.Name + "请求升职被CEO拒绝，信念-8");
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.InfoDetail.AddPerk(new Perk66(Self), true);
        Self.Mentality -= 12;
        PerkInfo yuanwang = null, chengzhang = null, yihan = null;
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 55)
            {
                yuanwang = Self.InfoDetail.PerksInfo[i];
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 58 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 2)
            {
                chengzhang = Self.InfoDetail.PerksInfo[i];
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 107 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 2)
            {
                yihan = Self.InfoDetail.PerksInfo[i];
            }
        }
        if (yuanwang)
        {
            yuanwang.CurrentPerk.RemoveEffect();
        }
        if (chengzhang)
        {
            chengzhang.CurrentPerk.RemoveEffect();
            chengzhang.CurrentPerk.RemoveEffect();
        }
        if (yihan)
        {
            yihan.CurrentPerk.RemoveEffect();
            yihan.CurrentPerk.RemoveEffect();
        }
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "希望得到上司" + Target.Name + "的升职推荐";
        if (Random.Range(0.0f, 1.0f) <= 0.0f)
        {
            Self.InfoDetail.AddPerk(new Perk107(Self), true);
            ResultText += "上司" + Target.Name + "质疑" + Self.Name + "的能力并拒绝推荐，获得事件状态 受到质疑*1,遗憾*1，己方信念下降12，消除事件状态：愿望*1 成长*4 遗憾*2";
        }
        else
        {
            ResultText += "上司" + Target.Name + "质疑" + Self.Name + "的能力并拒绝推荐，获得事件状态 受到质疑*1，己方信念下降12，消除事件状态：愿望*1 成长*4 遗憾*2";
        }
        GC.CreateMessage(Self.Name + "请求升职被拒绝");
        if (Target.CurrentDep != null)
        {
            Target.CurrentDep.AddPerk(new Perk113(null));
            Target.CurrentDep.AddPerk(new Perk113(null));
            Target.CurrentDep.AddPerk(new Perk113(null));
        }
        ResultText += "，部门获得“产生矛盾”状态×3";
    }
}
public class Event3_19 : JudgeEvent
{
    public Event3_19() : base()
    {
        EventName = "请求加薪";
        HaveTarget = true;
        Weight = 5;
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        if (EmpManager.Instance.FindBoss(Self) != Target)
        {
            return false;
        }
        List<int> pList = new List<int>();
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 55 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 1)
            {
                pList.Add(i);
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 58 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 1)
            {
                pList.Add(i);
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 107 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 2)
            {
                pList.Add(i);
            }
        }
        if (pList.Count >= 3)
        {
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += RelationBonus() + MoraleBonus() + EmotionBonus() + PerksBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 12)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.InfoDetail.AddPerk(new Perk65(Self), true);
        PerkInfo yuanwang = null, chengzhang = null, yihan = null;
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 55)
            {
                yuanwang = Self.InfoDetail.PerksInfo[i];
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 58 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 1)
            {
                chengzhang = Self.InfoDetail.PerksInfo[i];
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 107 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 2)
            {
                yihan = Self.InfoDetail.PerksInfo[i];
            }
        }
        if (yuanwang)
        {
            yuanwang.CurrentPerk.RemoveEffect();
        }
        if (chengzhang)
        {
            chengzhang.CurrentPerk.RemoveEffect();
        }
        if (yihan)
        {
            yihan.CurrentPerk.RemoveEffect();
            yihan.CurrentPerk.RemoveEffect();
        }
        //弹出窗口询问玩家
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "请求上司" + Target.Name + "为其加薪";
        if (Random.Range(0.0f, 1.0f) <= 0.0f)
        {
            Self.InfoDetail.AddPerk(new Perk107(Self), true);
            ResultText += "上司表示早有此意，并上报CEO，获得事件状态 受到信任*1，遗憾*1，消除事件状态： 愿望*1 成长*2 遗憾*2";
        }
        else
        {
            ResultText += "上司表示早有此意，并上报CEO，获得事件状态 受到信任*1，消除事件状态： 愿望*1 成长*2 遗憾*2";
        }
        GC.CreateMessage(Self.Name + "请求加薪被上司接受，请求CEO");
        EmpManager.Instance.JudgeEvent(this, true);
    }
    public override void OnAccept()
    {
        Self.SalaryMultiple += 0.2f;//工资上涨1.2倍
        GC.CreateMessage(Self.Name + "涨工资成功，工资上涨20%");
        Self.InfoDetail.AddHistory(Self.Name + "涨工资成功，工资上涨20%");
    }
    public override void OnRefuse()
    {
        Self.ChangeCharacter(4, -8);
        GC.CreateMessage(Self.Name+"涨工资被拒，信念-8");
        Self.InfoDetail.AddHistory(Self.Name + "涨工资被拒，信念-8");
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.InfoDetail.AddPerk(new Perk66(Self), true);
        Self.Mentality -= 12;
        PerkInfo yuanwang = null, chengzhang = null, yihan = null;
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 55)
            {
                yuanwang = Self.InfoDetail.PerksInfo[i];
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 58 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 1)
            {
                chengzhang = Self.InfoDetail.PerksInfo[i];
            }
            else if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 107 & Self.InfoDetail.PerksInfo[i].CurrentPerk.Level >= 2)
            {
                yihan = Self.InfoDetail.PerksInfo[i];
            }
        }
        if (yuanwang)
        {
            yuanwang.CurrentPerk.RemoveEffect();
        }
        if (chengzhang)
        {
            chengzhang.CurrentPerk.RemoveEffect();
        }
        if (yihan)
        {
            yihan.CurrentPerk.RemoveEffect();
            yihan.CurrentPerk.RemoveEffect();
        }
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "请求上司" + Target.Name + "为其加薪";
        if (Random.Range(0.0f, 1.0f) <= 0.0f)
        {
            Self.InfoDetail.AddPerk(new Perk107(Self), true);
            ResultText += "上司" + Target.Name + "质疑" + Self.Name + "的能力并拒绝加薪，获得事件状态 受到质疑*1,遗憾*1， 己方信念下降12，消除事件状态：愿望*1 成长*2 遗憾*2";
        }
        else
        {
            ResultText += "上司" + Target.Name + "质疑" + Self.Name + "的能力并拒绝加薪，获得事件状态 受到质疑*1， 己方信念下降12，消除事件状态：愿望*1 成长*2 遗憾*2";
        }
        GC.CreateMessage(Self.Name + "请求加薪被上司拒绝");
        if (Target.CurrentDep != null)
        {
            Target.CurrentDep.AddPerk(new Perk113(null));
            Target.CurrentDep.AddPerk(new Perk113(null));
            Target.CurrentDep.AddPerk(new Perk113(null));
        }
        ResultText += "，部门获得“产生矛盾”状态×3";
    }
}
public class Event3_20 : Event
{
    public Event3_20() : base()
    {
        EventName = "感到无聊";
        HaveTarget = true;
        Weight = 2;
        RelationRequire = 0;
        HaveTarget = false;
    }
    public override void SetWeight()
    {
        SetWeight(89, 1);
    }
    public override int FindResult()
    {
            return 3;
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.InfoDetail.AddPerk(new Perk64(Self), true);
        ResultText = "在"+SelfEntity.StandGridName()+"，"+Self.Name+"感到百无聊赖，获得事件状态 无聊*1";
    }
}
public class Event3_21 : Event
{
    public Event3_21() : base()
    {
        EventName = "寻求安慰";
        HaveTarget = true;
        Weight = 3;
        RelationRequire = 0;
        HaveTarget = false;
    }
    public override void SetWeight()
    {
        SetWeight(88, 1);
    }
    public override bool SpecialCheck()
    {
        List<int> pList = new List<int>();
        //if (Self.Mentality < 30)
        //{
        //    return false;
        //}
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            Perk perk = Self.InfoDetail.PerksInfo[i].CurrentPerk;
            if ((perk.Num == 49 && perk.Level >= 2) || perk.Num == 68 || perk.Num == 66 || perk.Num == 69||perk.Num ==52)
            {
                return true;
            }
        }
        return false;
    }
    public override int FindResult()
    {
        return 3;
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.InfoDetail.AddPerk(new Perk63(Self), true);
        if (perkUsed == 49)
        {
            ResultText = "由于" + perkUsedName + "的情绪在" + Self.Name + "心中的持续酝酿，" + Self.Name + "期望寻求安慰，获得事件状态 寻求安慰*1，消除事件状态" + perkUsedName + "×2";
        }
        else
        {
            ResultText = "由于" + perkUsedName + "的情绪在" + Self.Name + "心中的持续酝酿，" + Self.Name + "期望寻求安慰，获得事件状态 寻求安慰*1，消除事件状态" + perkUsedName + "×1";
        }
    }
    public override void StartSolve()
    {
        base.StartSolve();
        foreach (PerkInfo info in Self.InfoDetail.PerksInfo)
        {
            if (info.CurrentPerk.Num == perkUsed)
            {
                perkUsedName = info.CurrentPerk.Name;
                if (perkUsed == 49 && info.CurrentPerk.Level >= 2)
                {
                    info.CurrentPerk.RemoveEffect();
                    info.CurrentPerk.RemoveEffect();
                    return;
                }
                else
                {
                    info.CurrentPerk.RemoveEffect();
                    return;
                }
            }
        }
    }

    public override Event Clone()
    {
        Event clone = (Event)this.MemberwiseClone();

        List<int> pList = new List<int>();
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            Perk perk = Self.InfoDetail.PerksInfo[i].CurrentPerk;
            if ((perk.Num == 49 && perk.Level >= 2) || perk.Num == 68 || perk.Num == 66 || perk.Num == 69 || perk.Num == 52)
            {
                pList.Add(perk.Num);
            }
        }
        clone.perkUsed = pList[Random.Range(0, pList.Count)];

        return clone;
    }
}
public class Event3_22 : Event
{
    public Event3_22() : base()
    {
        EventName = "认可交谈";
        HaveTarget = true;
        Weight = 3;
        RelationRequire = 0;
        HaveTarget = false;
    }
    public override void SetWeight()
    {
        SetWeight(86, 1);
    }
    public override bool SpecialCheck()
    {
        List<int> pList = new List<int>();
        //if (Self.Mentality < 30)
        //{
        //    return false;
        //}
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            Perk perk = Self.InfoDetail.PerksInfo[i].CurrentPerk;
            if (perk.Num == 53 || perk.Num == 66 || perk.Num == 69)
            {
                return true;
            }
        }
        return false;
    }
    public override int FindResult()
    {
        return 3;
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.InfoDetail.AddPerk(new Perk61(Self), true);
        ResultText = "由于" + perkUsedName + "的情绪在" + Self.Name + "心中的持续酝酿，" + Self.Name + "渐渐渴望获得别人认可，获得事件状态 认可交谈*1，消除事件状态" + perkUsedName + "×1";
    }

    public override Event Clone()
    {
        Event clone = (Event)this.MemberwiseClone();

        List<int> pList = new List<int>();
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            Perk perk = Self.InfoDetail.PerksInfo[i].CurrentPerk;
            if (perk.Num == 53 || perk.Num == 66 || perk.Num == 69)
            {
                pList.Add(perk.Num);
            }
        }
        clone.perkUsed = pList[Random.Range(0, pList.Count)];

        return clone;
    }
    public override void StartSolve()
    {
        base.StartSolve();
        foreach (PerkInfo info in Self.InfoDetail.PerksInfo)
        {
            if (info.CurrentPerk.Num == perkUsed)
            {
                perkUsedName = info.CurrentPerk.Name;
                info.CurrentPerk.RemoveEffect();
                return;
            }
        }
    }
}
public class Event3_23 : Event
{
    public Event3_23() : base()
    {
        EventName = "分享日常";
        HaveTarget = true;
        Weight = 3;
        RelationRequire = 0;
        HaveTarget = false;
    }
    public override void SetWeight()
    {
        SetWeight(87, 1);
    }
    public override bool SpecialCheck()
    {
        List<int> pList = new List<int>();
        //if (Self.Mentality < 30)
        //{
        //    return false;
        //}
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            Perk perk = Self.InfoDetail.PerksInfo[i].CurrentPerk;
            if (perk.Num == 51 || perk.Num == 52 || perk.Num == 54)
            {
                return true;
            }
        }
        return false;
    }
    public override int FindResult()
    {
        return 3;
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.InfoDetail.AddPerk(new Perk62(Self), true);
        ResultText = "由于" + perkUsedName + "的情绪在" + Self.Name + "心中的持续酝酿，" + Self.Name + "想聊聊自己的日常，获得事件状态 分享日常*1，消除事件状态" + perkUsedName + "×1";
    }

    public override Event Clone()
    {
        Event clone = (Event)this.MemberwiseClone();

        List<int> pList = new List<int>();
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            Perk perk = Self.InfoDetail.PerksInfo[i].CurrentPerk;
            if (perk.Num == 51 || perk.Num == 52 || perk.Num == 54)
            {
                pList.Add(perk.Num);
            }
        }
        clone.perkUsed = pList[Random.Range(0, pList.Count)];

        return clone;
    }
    public override void StartSolve()
    {
        base.StartSolve();
        foreach (PerkInfo info in Self.InfoDetail.PerksInfo)
        {
            if (info.CurrentPerk.Num == perkUsed)
            {
                perkUsedName = info.CurrentPerk.Name;
                info.CurrentPerk.RemoveEffect();
                return;
            }
        }
    }
}
public class Event3_24 : Event
{
    public Event3_24() : base()
    {
        EventName = "深刻交谈";
        HaveTarget = true;
        Weight = 3;
        RelationRequire = 0;
        HaveTarget = false;
    }
    public override void SetWeight()
    {
        SetWeight(90, 1);
    }
    public override bool SpecialCheck()
    {
        List<int> pList = new List<int>();
        //if (Self.Mentality < 60)
        //{
        //    return false;
        //}
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            Perk perk = Self.InfoDetail.PerksInfo[i].CurrentPerk;
            if ((perk.Num == 49 && perk.Level >= 2) || perk.Num == 52 || perk.Num == 51 || perk.Num == 46)
            {
                return true;
            }
        }
        return false;
    }
    public override int FindResult()
    {
        return 3;
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.InfoDetail.AddPerk(new Perk59(Self), true);
        if (perkUsed == 49)
        {
            ResultText = "由于" + perkUsedName + "的情绪在" + Self.Name + "心中的持续酝酿，" + Self.Name + "渴望与别人进行深刻交谈，获得事件状态 深刻交谈*1，消除事件状态" + perkUsedName + "×2";
        }
        else
        {
            ResultText = "由于" + perkUsedName + "的情绪在" + Self.Name + "心中的持续酝酿，" + Self.Name + "渴望与别人进行深刻交谈，获得事件状态 深刻交谈*1，消除事件状态" + perkUsedName + "×1";
        }
    }

    public override Event Clone()
    {
        Event clone = (Event)this.MemberwiseClone();

        List<int> pList = new List<int>();
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            Perk perk = Self.InfoDetail.PerksInfo[i].CurrentPerk;
            if ((perk.Num == 49 && perk.Level >= 2) || perk.Num == 52 || perk.Num == 51 || perk.Num == 46)
            {
                pList.Add(perk.Num);
            }
        }
        clone.perkUsed = pList[Random.Range(0, pList.Count)];

        return clone;
    }
    public override void StartSolve()
    {
        base.StartSolve();
        foreach (PerkInfo info in Self.InfoDetail.PerksInfo)
        {
            if (info.CurrentPerk.Num == perkUsed)
            {
                perkUsedName = info.CurrentPerk.Name;
                if (perkUsed == 49 && info.CurrentPerk.Level >= 2)
                {
                    info.CurrentPerk.RemoveEffect();
                    info.CurrentPerk.RemoveEffect();
                    return;
                }
                else
                {
                    info.CurrentPerk.RemoveEffect();
                    return;
                }
            }
        }
    }
}
public class Event3_25 : Event
{
    public Event3_25() : base()
    {
        EventName = "分享快乐";
        HaveTarget = true;
        Weight = 5;
        RelationRequire = 0;
        HaveTarget = false;
    }
    public override void SetWeight()
    {
        SetWeight(87, 1);
    }
    public override bool SpecialCheck()
    {
        List<int> pList = new List<int>();
        //if (Self.Mentality < 60)
        //{
        //    return false;
        //}
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            Perk perk = Self.InfoDetail.PerksInfo[i].CurrentPerk;
            if ((perk.Num == 47 && perk.Level >= 2) || perk.Num == 65 || perk.Num == 67)
            {
                return true;
            }
        }
        return false;
    }
    public override int FindResult()
    {
        return 3;
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.InfoDetail.AddPerk(new Perk60(Self), true);
        if (perkUsed == 47)
        {
            ResultText = "由于" + perkUsedName + "的情绪在" + Self.Name + "心中的持续酝酿，" + Self.Name + "想要与别人分享自己的快乐，获得事件状态 分享快乐*1 ，消除事件状态" + perkUsedName + "×2";
        }
        else
        {
            ResultText = "由于" + perkUsedName + "的情绪在" + Self.Name + "心中的持续酝酿，" + Self.Name + "想要与别人分享自己的快乐，获得事件状态 分享快乐*1 ，消除事件状态" + perkUsedName + "×1";
        }
    }

    public override Event Clone()
    {
        Event clone = (Event)this.MemberwiseClone();

        List<int> pList = new List<int>();
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            Perk perk = Self.InfoDetail.PerksInfo[i].CurrentPerk;
            if ((perk.Num == 47 && perk.Level >= 2) || perk.Num == 65 || perk.Num == 67)
            {
                pList.Add(perk.Num);
            }
        }
        clone.perkUsed = pList[Random.Range(0, pList.Count)];

        return clone;
    }
    public override void StartSolve()
    {
        base.StartSolve();
        foreach (PerkInfo info in Self.InfoDetail.PerksInfo)
        {
            if (info.CurrentPerk.Num == perkUsed)
            {
                perkUsedName = info.CurrentPerk.Name;
                if (perkUsed == 47 && info.CurrentPerk.Level >= 2)
                {
                    info.CurrentPerk.RemoveEffect();
                    info.CurrentPerk.RemoveEffect();
                    return;
                }
                else
                {
                    info.CurrentPerk.RemoveEffect();
                    return;
                }
            }
        }
    }
}
public class Event3_26 : Event
{
    public Event3_26() : base()
    {
        EventName = "陌路恶意玩笑";
        HaveTarget = true;
        Weight = 4;
        RelationRequire = 0;
        PerkRequire = 59;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint >= 0)
        {
            return false;
        }
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus() + PerksBonus()+MoraleBonus(2);
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        PerkInfo shenkejiaotan = null;
        foreach(PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if(p.CurrentPerk.Num == 59)
            {
                shenkejiaotan = p;
            }
        }
        if (shenkejiaotan)
        {
            shenkejiaotan.CurrentPerk.RemoveEffect();
        }
        Self.AddEmotion(EColor.LYellow);
        Self.ChangeRelation(Target, 15);
        Target.ChangeRelation(Self, 15);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "遇到了一些麻烦想要和人聊聊，";
        ObjectText = ResultText + ""+Target.Name+"拿"+Self.Name+"开了玩笑，但对方并没有觉得冒犯，双方好感度+15";
        ResultText += "被"+Target.Name+"拿他开了玩笑，但是他反而很开心，双方好感度+15，获得情绪状态：愉悦×1，消除事件状态：深刻交谈×1";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        PerkInfo shenkejiaotan = null;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 59)
            {
                shenkejiaotan = p;
            }
        }
        if (shenkejiaotan)
        {
            shenkejiaotan.CurrentPerk.RemoveEffect();
        }
        Self.AddEmotion(EColor.LBlue);
        Self.ChangeRelation(Target,-15);
        Target.ChangeRelation(Self, -15);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "遇到了一些麻烦想要和人聊聊，";
        ObjectText = ResultText + ""+Target.Name+"拿"+Self.Name+"开了玩笑，"+Target.Name+"拿对方开了玩笑，成功冒犯了对方，双方好感度-15";
        ResultText += "被"+Target.Name+"看穿开了玩笑，他非常难过，双方好感度-15，获得情绪状态：苦涩×1，消除事件状态：深刻交谈×1";
    }
}
public class Event3_27 : Event
{
    public Event3_27() : base()
    {
        EventName = "陌路不认可";
        HaveTarget = true;
        Weight = 5;
        RelationRequire = 0;
        PerkRequire = 61;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint >= 0)
        {
            return false;
        }
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus() + PerksBonus() + MoraleBonus(2);
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        PerkInfo renkejiaotan = null;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 61)
            {
                renkejiaotan = p;
            }
        }
        if (renkejiaotan)
        {
            renkejiaotan.CurrentPerk.RemoveEffect();
        }
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "找人聊了聊自己想要做的事情，";
        ObjectText = ResultText + Target.Name + "听到之后不以为意，好感度不变";
        ResultText += Target.Name+ "听到后不以为意，好感度不变，消除事件状态：认可交谈×1";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        PerkInfo renkejiaotan = null;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 61)
            {
                renkejiaotan = p;
            }
        }
        if (renkejiaotan)
        {
            renkejiaotan.CurrentPerk.RemoveEffect();
        }
        Self.AddEmotion(EColor.LRed);
        Self.AddEmotion(EColor.LRed);
        Self.ChangeRelation(Target, -15);
        Target.ChangeRelation(Self, -15);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "找人聊了聊自己想要做的事情，";
        ObjectText = ResultText + Target.Name + "听到之后嘲笑了"+Self.Name+"一番，双方好感度-15";
        ResultText += Target.Name+"听到之后嘲笑了"+Self.Name+"一番，双方好感度-15，获得情绪状态：反感×2，消除事件状态：认可交谈×1";
    }
}
public class Event3_28 : Event
{
    public Event3_28() : base()
    {
        EventName = "陌路批评";
        HaveTarget = true;
        Weight = 6;
        RelationRequire = -1;
        PerkRequire = 62;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint >= -30)
        {
            return false;
        }
        if (Self.FindRelation(Target).FriendValue != -1 && Self.FindRelation(Target).FriendValue != -2)
        {
            return false;
        }
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra +=CRBonus()+PerksBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        PerkInfo info = null;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 62)
            {
                info = p;
            }
        }
        if (info)
        {
            info.CurrentPerk.RemoveEffect();
        }
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "给别人讲了自己今天发生的事情，";
        ObjectText = ResultText + Target.Name + "没当回事，好感度不变";
        ResultText += "被"+Target.Name+"说这都不是什么大不了的事情，好感度不变，消除事件状态：分享日常×1";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        PerkInfo info = null;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 62)
            {
                info = p;
            }
        }
        if (info)
        {
            info.CurrentPerk.RemoveEffect();
        }
        Self.AddEmotion(EColor.LBlue);
        Self.ChangeRelation(Target, -15);
        Target.ChangeRelation(Self, -15);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "给别人讲了自己今天发生的事情，";
        ObjectText = ResultText + Target.Name + "讥讽了"+Self.Name+"，双方好感度-15";
        ResultText += "被"+Target.Name+"说这些都是"+Self.Name+"自己的问题，双方好感度-15，获得情绪状态：苦涩×1，消除事件状态：分享日常×1";
    }
}
public class Event3_29 : Event
{
    public Event3_29() : base()
    {
        EventName = "陌路泼冷水";
        HaveTarget = true;
        Weight = 6;
        RelationRequire = -1;
        PerkRequire = 60;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint >= -30)
        {
            return false;
        }
        if (Self.FindRelation(Target).FriendValue != -1 && Self.FindRelation(Target).FriendValue != -2)
        {
            return false;
        }
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus() + PerksBonus() + MoraleBonus(2);
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        PerkInfo info = null;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 60)
            {
                info = p;
            }
        }
        if (info)
        {
            info.CurrentPerk.RemoveEffect();
        }
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "给别人讲了自己今天发生的事情，";
        ObjectText = ResultText + Target.Name + "不以为意，好感度不变";
        ResultText += Target.Name + "听到后说“哦”，好感度不变，消除事件状态：分享乐事×1";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        PerkInfo info = null;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 60)
            {
                info = p;
            }
        }
        if (info)
        {
            info.CurrentPerk.RemoveEffect();
        }
        Self.AddEmotion(EColor.LRed);
        Self.AddEmotion(EColor.LRed);
        Self.ChangeRelation(Target, -15);
        Target.ChangeRelation(Self, -15);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "给别人讲了自己今天发生的事情，";
        ObjectText = ResultText + Target.Name + "冷嘲热讽了一番，双方好感度-15";
        ResultText += Target.Name + "听到后嘲笑了一番，双方好感度-15，获得情绪状态：侮辱×2，消除事件状态：分享乐事×1";
    }
}
public class Event3_30 : Event
{
    public Event3_30() : base()
    {
        EventName = "仇敌侮辱";
        HaveTarget = true;
        Weight = 7;
        RelationRequire = -2;
        PerkRequire = 61;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint >= -60)
        {
            return false;
        }
        if (Self.FindRelation(Target).FriendValue != -2)
        {
            return false;
        }
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus() + PerksBonus() + MoraleBonus(2);
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        PerkInfo info = null;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 61)
            {
                info = p;
            }
        }
        if (info)
        {
            info.CurrentPerk.RemoveEffect();
        }
        Self.AddEmotion(EColor.LRed);
        Self.AddEmotion(EColor.LRed);
        Self.ChangeRelation(Target, -15);
        Target.ChangeRelation(Self, -15);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "正在向别人展示自己的成果，";
        ObjectText = ResultText + Target.Name + "否定了"+Self.Name+"，双方好感度-15";
        ResultText += Target.Name + "说毫无价值，双方好感度-15，获得情绪状态：侮辱×2，消除事件状态：认可交谈×1";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        PerkInfo info = null;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 61)
            {
                info = p;
            }
        }
        if (info)
        {
            info.CurrentPerk.RemoveEffect();
        }
        Self.AddEmotion(EColor.LBlue);
        Self.ChangeRelation(Target, -15);
        Target.ChangeRelation(Self, -15);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "正在向别人展示自己的成果，";
        ObjectText = ResultText + Target.Name + "嘲笑了"+Self.Name+"，双方好感度-15";
        ResultText += Target.Name + "说建议上司开除，双方好感度-15，获得情绪状态：辛酸×1，消除事件状态：认可交谈×1";
    }
}
public class Event3_31 : Event
{
    public Event3_31() : base()
    {
        EventName = "仇敌落井下石";
        HaveTarget = true;
        Weight = 7;
        RelationRequire = -2;
        PerkRequire = 63;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint >= -60)
        {
            return false;
        }
        if (Self.FindRelation(Target).FriendValue != -2)
        {
            return false;
        }
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus() + PerksBonus() + MoraleBonus(2);
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        PerkInfo info = null;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 63)
            {
                info = p;
            }
        }
        if (info)
        {
            info.CurrentPerk.RemoveEffect();
        }
        Self.AddEmotion(EColor.LRed);
        Self.AddEmotion(EColor.LRed);
        Self.ChangeRelation(Target, -15);
        Target.ChangeRelation(Self, -15);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "情绪低落想要寻求安慰，";
        ObjectText = ResultText + Target.Name + "嘲笑了"+Self.Name+"，双方好感度-15";
        ResultText += Target.Name + "看到后嘲笑了"+Self.Name+"一番，双方好感度-15，获得情绪状态：反感×2，消除事件状态：寻求安慰×1";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        PerkInfo info = null;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 63)
            {
                info = p;
            }
        }
        if (info)
        {
            info.CurrentPerk.RemoveEffect();
        }
        Self.AddEmotion(EColor.LRed);
        Self.AddEmotion(EColor.LRed);
        Self.ChangeRelation(Target, -15);
        Target.ChangeRelation(Self, -15);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "情绪低落想要寻求安慰，";
        ObjectText = ResultText + Target.Name + "公开讥讽了"+Self.Name+"，双方好感度-15";
        ResultText += Target.Name + "看到之后当众讥讽了"+Self.Name+"，双方好感度-15，获得情绪状态：侮辱×2，消除事件状态：寻求安慰×1";
    }
}
public class Event3_32 : Event
{
    public Event3_32() : base()
    {
        EventName = "朋友日常";
        HaveTarget = true;
        Weight = 4;
        RelationRequire = 0;
        PerkRequire = 62;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint <= 5)
        {
            return false;
        }
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus() + PerksBonus() + MoraleBonus(2);
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        PerkInfo info = null;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 62)
            {
                info = p;
            }
        }
        if (info)
        {
            info.CurrentPerk.RemoveEffect();
        }
        Self.AddEmotion(EColor.LYellow);
        Self.ChangeRelation(Target, 15);
        Target.ChangeRelation(Self, 15);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "跟" + Target.Name + "分享了日常，";
        ObjectText = ResultText + Target.Name + "觉得还挺有趣的，双方好感度+15";
        ResultText += "两人谈了一会儿，双方好感度+15，获得情绪状态：愉悦×1，消除事件状态：分享日常×1";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        PerkInfo info = null;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 62)
            {
                info = p;
            }
        }
        if (info)
        {
            info.CurrentPerk.RemoveEffect();
        }
        Self.AddEmotion(EColor.LRed);
        Self.AddEmotion(EColor.LRed);
        Self.ChangeRelation(Target, -15);
        Target.ChangeRelation(Self, -15);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "跟" + Target.Name + "分享了日常，";
        ObjectText = ResultText + Target.Name + "完全不懂为什么"+Self.Name+"说这些，双方好感度-15";
        ResultText += Target.Name + "有些走神，双方好感度-15，获得情绪状态：反感×2，消除事件状态：分享日常×1";
    }
}
public class Event3_33 : Event
{
    public Event3_33() : base()
    {
        EventName = "朋友认可";
        HaveTarget = true;
        Weight = 5;
        RelationRequire = 0;
        PerkRequire = 61;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint <= 5)
        {
            return false;
        }
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus() + PerksBonus() + MoraleBonus(2);
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        PerkInfo info = null;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 61)
            {
                info = p;
            }
        }
        if (info)
        {
            info.CurrentPerk.RemoveEffect();
        }
        Self.AddEmotion(EColor.LYellow);
        Self.ChangeRelation(Target, 15);
        Target.ChangeRelation(Self, 15);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "向" + Target.Name + "寻求认可，";
        ObjectText = ResultText + Target.Name + "认可了"+Self.Name+"，双方好感度+15";
        ResultText += Target.Name + "肯定了"+Self.Name+"，双方好感度+15，获得情绪状态：愉悦×1，消除事件状态：认可交谈×1";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        PerkInfo info = null;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 61)
            {
                info = p;
            }
        }
        if (info)
        {
            info.CurrentPerk.RemoveEffect();
        }
        Self.AddEmotion(EColor.LRed);
        Self.AddEmotion(EColor.LRed);
        Self.ChangeRelation(Target, -15);
        Target.ChangeRelation(Self, -15);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "向" + Target.Name + "寻求认可，";
        ObjectText = ResultText + Target.Name + "敷衍了"+Self.Name+"，双方好感度-15";
        ResultText += Target.Name + "漫不经心地夸了两句"+Self.Name+"，双方好感度-15，获得情绪状态：反感×2，消除事件状态：认可交谈×1";
    }
}
public class Event3_34 : Event
{
    public Event3_34() : base()
    {
        EventName = "朋友乐事";
        HaveTarget = true;
        Weight = 5;
        RelationRequire = 0;
        PerkRequire = 60;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint <= 40)
        {
            return false;
        }
        if(Self.FindRelation(Target).FriendValue!=1 && Self.FindRelation(Target).FriendValue != 2)
        {
            return false;
        }
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus() + PerksBonus() + MoraleBonus(2);
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        PerkInfo info = null;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 60)
            {
                info = p;
            }
        }
        if (info)
        {
            info.CurrentPerk.RemoveEffect();
        }
        Self.AddEmotion(EColor.LYellow);
        Self.ChangeRelation(Target, 15);
        Target.ChangeRelation(Self, 15);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "跟朋友" + Target.Name + "分享今天发生的高兴事，";
        ObjectText = ResultText + Target.Name + "也觉得很开心，双方好感度+15";
        ResultText += Target.Name + "也很开心，双方好感度+15，获得情绪状态：愉悦×1，消除事件状态：分享乐事×1";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        PerkInfo info = null;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 60)
            {
                info = p;
            }
        }
        if (info)
        {
            info.CurrentPerk.RemoveEffect();
        }
        Self.AddEmotion(EColor.LBlue);
        Self.ChangeRelation(Target, -15);
        Target.ChangeRelation(Self, -15);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "跟朋友" + Target.Name + "分享今天发生的高兴事，";
        ObjectText = ResultText + Target.Name + "觉得很无聊，双方好感度-15";
        ResultText += Target.Name + "有点犯困，双方好感度-15，获得情绪状态：苦涩×1，消除事件状态：分享乐事×1";
    }
}
public class Event3_35 : Event
{
    public Event3_35() : base()
    {
        EventName = "朋友安慰";
        HaveTarget = true;
        Weight = 6;
        RelationRequire = 1;
        PerkRequire = 63;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint <=40)
        {
            return false;
        }
        if (Self.FindRelation(Target).FriendValue != 1 && Self.FindRelation(Target).FriendValue != 2)
        {
            return false;
        }
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus() + PerksBonus() + MoraleBonus(2);
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        PerkInfo info = null;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 63)
            {
                info = p;
            }
        }
        if (info)
        {
            info.CurrentPerk.RemoveEffect();
        }
        Self.AddEmotion(EColor.LYellow);
        Self.ChangeRelation(Target, 15);
        Target.ChangeRelation(Self, 15);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "向朋友" + Target.Name + "寻求安慰，";
        ObjectText = ResultText + Target.Name + "安慰了"+Self.Name+"，双方好感度+15";
        ResultText += Target.Name + "安慰了"+Self.Name+"，双方好感度+15，获得情绪状态：愉悦×1，消除事件状态：寻求安慰×1";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        PerkInfo info = null;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 63)
            {
                info = p;
            }
        }
        if (info)
        {
            info.CurrentPerk.RemoveEffect();
        }
        Self.AddEmotion(EColor.LRed);
        Self.AddEmotion(EColor.LRed);
        Self.ChangeRelation(Target, -15);
        Target.ChangeRelation(Self, -15);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "向朋友" + Target.Name + "寻求安慰，";
        ObjectText = ResultText + Target.Name + "让"+Self.Name+"回去反思，双方好感度-15";
        ResultText += Target.Name + "指点了"+Self.Name+"一番，双方好感度-15，获得情绪状态：侮辱×2，消除事件状态：寻求安慰×1";
    }
}
public class Event3_36 : Event
{
    public Event3_36() : base()
    {
        EventName = "挚友之助";
        HaveTarget = true;
        Weight = 7;
        RelationRequire = 2;
        PerkRequire = 63;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint <= 80)
        {
            return false;
        }
        if ( Self.FindRelation(Target).FriendValue != 2)
        {
            return false;
        }
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus() + PerksBonus() + MoraleBonus(2);
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        PerkInfo info = null;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 63)
            {
                info = p;
            }
        }
        if (info)
        {
            info.CurrentPerk.RemoveEffect();
        }
        Self.AddEmotion(EColor.LYellow);
        Self.ChangeRelation(Target, 15);
        Target.ChangeRelation(Self, 15);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "向挚友" + Target.Name + "寻求帮助，";
        ObjectText = ResultText + Target.Name + "帮了忙，双方好感度+15";
        ResultText += Target.Name + "鼎力相助，双方好感度+15，获得情绪状态：愉悦×1，消除事件状态：寻求安慰×1";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        PerkInfo info = null;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 63)
            {
                info = p;
            }
        }
        if (info)
        {
            info.CurrentPerk.RemoveEffect();
        }
        Self.AddEmotion(EColor.LBlue);
        Self.ChangeRelation(Target, -15);
        Target.ChangeRelation(Self, -15);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "向挚友" + Target.Name + "寻求帮助，";
        ObjectText = ResultText + Target.Name + "推脱了，双方好感度-15";
        ResultText += Target.Name + "声称自己有事，双方好感度-15，获得情绪状态：苦涩×1，消除事件状态：寻求安慰×1";
    }
}
public class Event3_37 : Event
{
    public Event3_37() : base()
    {
        EventName = "挚友深谈";
        HaveTarget = true;
        Weight = 7;
        RelationRequire = 2;
        PerkRequire =59;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint <= 80)
        {
            return false;
        }
        if (Self.FindRelation(Target).FriendValue != 2)
        {
            return false;
        }
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus() + PerksBonus() + MoraleBonus(2);
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        PerkInfo info = null;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 59)
            {
                info = p;
            }
        }
        if (info)
        {
            info.CurrentPerk.RemoveEffect();
        }
        Self.AddEmotion(EColor.LYellow);
        Self.ChangeRelation(Target, 15);
        Target.ChangeRelation(Self, 15);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "跟挚友" + Target.Name + "聊了很多心里话，";
        ObjectText = ResultText + Target.Name + "听完了，双方好感度+15";
        ResultText += Target.Name + "温柔地听着"+Self.Name+"讲完，双方好感度+15，获得情绪状态：愉悦×1，消除事件状态：深刻交谈×1";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        PerkInfo info = null;
        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 59)
            {
                info = p;
            }
        }
        if (info)
        {
            info.CurrentPerk.RemoveEffect();
        }
        Self.AddEmotion(EColor.LBlue);
        Self.ChangeRelation(Target, -15);
        Target.ChangeRelation(Self, -15);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "跟挚友" + Target.Name + "聊了很多心里话，";
        ObjectText = ResultText + Target.Name + "没有心思听下去，双方好感度-15";
        ResultText += Target.Name + "岔开了话题，双方好感度-15，获得情绪状态：辛酸×1，消除事件状态：深刻交谈×1";
    }
}
public class Event3_38 : Event
{
    //这个事件比较特殊，判定之前HaveTarget == false，Target == null;
    //判定之后，HaveTarget == true，Target != null。

    public Event3_38() : base()
    {
        EventName = "认识新人";
        HaveTarget = false;
        Weight = 4;
        RelationRequire = 0;
    }
    public override void SetWeight()
    {
        foreach(PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if(p.CurrentPerk.Num == 81)
            {
                if (p.CurrentPerk.Level >= 12)
                {
                    SetWeight(81, -4);
                }
                else if(p.CurrentPerk.Level >= 9)
                {
                    SetWeight(81, -3);
                }
                else if (p.CurrentPerk.Level >= 6)
                {
                    SetWeight(81, -2);
                }
                else if (p.CurrentPerk.Level >= 3)
                {
                    SetWeight(81, -1);
                }
            }
        }
    }
    public override bool SpecialCheck()
    {
        //重新随机一个不认识的人作为Target
        List<Relation> allRelation = Function.CopyList(Self.Relations);
        allRelation = Function.RandomSortList(allRelation);

        foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 64 & p.CurrentPerk.Level >= 1)
            {//无聊×1
                foreach (Relation relation in allRelation)
                {
                    if (relation.KnowEachOther == false)
                    {
                        Target = relation.Target;
                        break;
                    }
                }

                if (Target != null)
                {
                    return true;
                }
            }
        }

        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus() + PerksBonus() + MoraleBonus(2);
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);

        Self.AddEmotion(EColor.LYellow);
        Self.ChangeRelation(Target, 15);
        Self.FindRelation(Target).KnowTarget();
        Target.ChangeRelation(Self, 15);
        Target.FindRelation(Self).KnowTarget();
        Self.InfoDetail.AddPerk(new Perk81(Self), true);
        ResultText = Self.Name + "因无聊闲逛，在" + SelfEntity.StandGridName() + "认识" + Target.Name + "，";
        if (Random.Range(0.0f, 1.0f) <= 0.5)
        {
            PerkInfo info = null;
            foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
            {
                if (p.CurrentPerk.Num == 64)
                {
                    info = p;
                }
            }
            if (info)
            {
                info.CurrentPerk.RemoveEffect();
            }
            ObjectText = ResultText + Target.Name + "就这样认识了" + Self.Name + "，双方认识 双方好感+15";
            ResultText += Target.Name + "双方觉得颇为投缘，双方认识 双方好感+15，消除事件状态：无聊×1，获得情绪状态：愉悦×1";
        }
        else
        {
            ObjectText = ResultText + Target.Name + "就这样认识了" + Self.Name + "，双方认识 双方好感+15";
            ResultText += Target.Name + "双方觉得颇为投缘，双方认识 双方好感+15，获得情绪状态：愉悦×1";
        }
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.LRed);
        Self.AddEmotion(EColor.LRed);
        Self.ChangeRelation(Target, -15);
        Self.FindRelation(Target).KnowTarget();
        Target.ChangeRelation(Self, -15);
        Target.FindRelation(Self).KnowTarget();
        Self.InfoDetail.AddPerk(new Perk81(Self), true);
        ResultText = Self.Name + "因无聊闲逛，在" + SelfEntity.StandGridName() + "认识" + Target.Name + "，";
        if (Random.Range(0.0f, 1.0f) <= 0.5)
        {
            PerkInfo info = null;
            foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
            {
                if (p.CurrentPerk.Num == 64)
                {
                    info = p;
                }
            }
            if (info)
            {
                info.CurrentPerk.RemoveEffect();
            }
            ObjectText = ResultText + Target.Name + "觉得" + Self.Name + "很烦，双方认识 双方好感-15";
            ResultText += Target.Name + "双方第一印象很差，双方认识 双方好感-15，消除事件状态：无聊×1，获得情绪状态：反感×2";
        }
        else
        {
            ObjectText = ResultText + Target.Name + "觉得" + Self.Name + "很烦，双方认识 双方好感-15";
            ResultText += Target.Name + "双方第一印象很差，双方认识 双方好感-15，获得情绪状态：反感×2";
        }
    }

    public override Event Clone()
    {
        Event clone = (Event)this.MemberwiseClone();
        clone.HaveTarget = true;
        return clone;
    }
}
public class Event3_39 : Event
{
    public Event3_39() : base()
    {
        EventName = "闲聊";
        HaveTarget = true;
        Weight = 2;
        RelationRequire =0;
    }
    public override bool SpecialCheck()
    {
        //if (Self.FindRelation(Target).KnowEachOther)
        //{
        //    foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
        //    {
        //        if (p.CurrentPerk.Num == 64 & p.CurrentPerk.Level >= 1)
        //        {//无聊×1
        //            return true;
        //        }
        //    }
        //}
        //return false;
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus() + PerksBonus() + MoraleBonus(2);
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.AddEmotion(EColor.LYellow);
        Self.ChangeRelation(Target, 15);
        Target.ChangeRelation(Self, 15);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "和" + Target.Name + "在走廊遇见闲聊了一会儿，";
        if (Random.Range(0.0f, 1.0f) <= 0.5)
        {
            PerkInfo info = null;
            foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
            {
                if (p.CurrentPerk.Num == 64)
                {
                    info = p;
                }
            }
            if (info)
            {
                info.CurrentPerk.RemoveEffect();
            }
            ObjectText = ResultText + "两人互相很有兴趣，双方好感度+15";
            ResultText += "双方聊得挺高兴的，" + Self.Name + "对" + Target.Name + "更加好奇，双方好感度+15，获得情绪状态：愉悦×1，消除事件状态：无聊×1";
        }
        else
        {
            ObjectText = ResultText + "两人互相很有兴趣，双方好感度+15";
            ResultText += "双方聊得挺高兴的，" + Self.Name + "对" + Target.Name + "更加好奇，双方好感度+15，获得情绪状态：愉悦×1";
        }
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.LBlue);
        Self.ChangeRelation(Target, -15);
        Target.ChangeRelation(Self, -15);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "和" + Target.Name + "在走廊遇见闲聊了一会儿，";
        if (Random.Range(0.0f, 1.0f) <= 0.5)
        {
            PerkInfo info = null;
            foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
            {
                if (p.CurrentPerk.Num == 64)
                {
                    info = p;
                }
            }
            if (info)
            {
                info.CurrentPerk.RemoveEffect();
            }
            ObjectText = ResultText + "两人互相没什么兴趣，双方好感度-15";
            ResultText += "双方聊的不太高兴，" + Target.Name + "没什么兴趣的样子，双方好感度-15，获得情绪状态：辛酸×1，消除事件状态：无聊×1";
        }
        else
        {
            ObjectText = ResultText + "两人互相没什么兴趣，双方好感度-15";
            ResultText += "双方聊的不太高兴，" + Target.Name + "没什么兴趣的样子，双方好感度-15，获得情绪状态：辛酸×1";
        }
    }
}
public class Event3_40 : Event
{
    public Event3_40() : base()
    {
        EventName = "特殊建筑互动";
        HaveTarget = false;
        Weight = 3;
        RelationRequire = 0;
        BuildingRequires = new List<BuildingType>() { BuildingType.室内温室, BuildingType.整修楼顶, BuildingType.游戏厅, BuildingType.营养师定制厨房, BuildingType.咖啡bar, BuildingType.花盆, BuildingType.长椅, BuildingType.自动贩卖机 };
        //此处员工应该随机移动到上面这个列表里存在的房间
    }
    public override bool SpecialCheck()
    {
            foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
            {
                if (p.CurrentPerk.Num == 64 & p.CurrentPerk.Level >= 2)
                {//无聊×1
                    return true;
                }
            }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus() + PerksBonus() + MoraleBonus(2);
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.AddEmotion(EColor.LYellow);
        if (Random.Range(0.0f, 1.0f) <= 0.5)
        {
            PerkInfo info = null;
            foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
            {
                if (p.CurrentPerk.Num == 64)
                {
                    info = p;
                }
            }
            if (info)
            {
                info.CurrentPerk.RemoveEffect();
            }
            ResultText = Self.Name + "自己闲逛，逛到了" + SelfEntity.StandGridName() + "那里，";
            ResultText += "待了一会儿，感觉心情舒畅，获得情绪状态：愉悦×1，消除事件状态：无聊×1";
        }
        else
        {
            ResultText = Self.Name + "自己闲逛，逛到了" + SelfEntity.StandGridName() + "那里，";
            ResultText += "待了一会儿，感觉心情舒畅，获得情绪状态：愉悦×1";
        }
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.LBlue);
        if (Random.Range(0.0f, 1.0f) <= 0.5)
        {
            PerkInfo info = null;
            foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
            {
                if (p.CurrentPerk.Num == 64)
                {
                    info = p;
                }
            }
            if (info)
            {
                info.CurrentPerk.RemoveEffect();
            }
            ResultText = Self.Name + "自己闲逛，逛到了" + SelfEntity.StandGridName() + "那里，";
            ResultText += "待了一会儿，觉得更无聊了，获得情绪状态：苦涩×1，消除事件状态：无聊×1";
        }
        else
        {
            ResultText = Self.Name + "自己闲逛，逛到了" + SelfEntity.StandGridName() + "那里，";
            ResultText += "待了一会儿，觉得更无聊了，获得情绪状态：苦涩×1";
        }
    }
}
public class Event3_41 : Event
{
    public Event3_41() : base()
    {
        EventName = "建筑互动";
        HaveTarget = true;
        Weight = 3;
        RelationRequire = 0;
        BuildingRequires = new List<BuildingType>() { BuildingType.室内温室, BuildingType.整修楼顶, BuildingType.游戏厅, BuildingType.营养师定制厨房, BuildingType.咖啡bar, BuildingType.花盆, BuildingType.长椅, BuildingType.自动贩卖机 };
        //两人应该一起走到上面BuildingRequires中的随机一个房间
    }
    public override bool SpecialCheck()
    {
        if (Self.FindRelation(Target).KnowEachOther)
        {
            foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
            {
                if (p.CurrentPerk.Num == 64 & p.CurrentPerk.Level >= 2)
                {//无聊×1
                    return true;
                }
            }
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus() + PerksBonus() + MoraleBonus(2);
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.AddEmotion(EColor.LYellow);
        Self.ChangeRelation(Target, 15);
        Target.ChangeRelation(Self, 15);
        ResultText = Self.Name + "在" + SelfEntity.StandGridName() + "碰到了" + Target.Name + "，";
        if (Random.Range(0.0f, 1.0f) <= 0.5)
        {
            PerkInfo info = null;
            foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
            {
                if (p.CurrentPerk.Num == 64)
                {
                    info = p;
                }
            }
            if (info)
            {
                info.CurrentPerk.RemoveEffect();
            }
            ObjectText = ResultText + "两人聊得很开心，双方好感度+15";
            ResultText += "跟" + Target.Name + "一起在" + SelfEntity.StandGridName() + "度过了一段愉快的时光，双方好感度+15，获得情绪状态：愉悦×1，消除事件状态：无聊×1";
        }
        else
        {
            ObjectText = ResultText + "两人聊得很开心，双方好感度+15";
            ResultText += "跟" + Target.Name + "一起在" + SelfEntity.StandGridName() + "度过了一段愉快的时光，双方好感度+15，获得情绪状态：愉悦×1";
        }
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.LRed);
        Self.AddEmotion(EColor.LRed);
        Self.ChangeRelation(Target, -15);
        Target.ChangeRelation(Self, -15);
        ResultText = Self.Name+ "在"+SelfEntity.StandGridName()+"碰到了"+Target.Name+"，";
        if (Random.Range(0.0f, 1.0f) <= 0.5)
        {
            PerkInfo info = null;
            foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
            {
                if (p.CurrentPerk.Num == 64)
                {
                    info = p;
                }
            }
            if (info)
            {
                info.CurrentPerk.RemoveEffect();
            }
            ObjectText = ResultText + "感叹冤家路窄，双方好感度-15";
            ResultText += "觉得挺别扭的，双方好感度-15，获得情绪状态：反感×2，消除事件状态：无聊×1";
        }
        else
        {
            ObjectText = ResultText + "感叹冤家路窄，双方好感度-15";
            ResultText += "觉得挺别扭的，双方好感度-15，获得情绪状态：反感×2";
        }
    }
}
public class Event3_42 : JudgeEvent
{
    public Event3_42() : base()
    {
        EventName = "请假翘班";
        HaveTarget = true;
        Weight = 5;
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        if (EmpManager.Instance.FindBoss(Self) == Target)
        {
            foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
            {
                if (p.CurrentPerk.Num == 64 & p.CurrentPerk.Level >= 3)
                {//无聊×3
                    return true;
                }
            }
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus() + PerksBonus() + MoraleBonus(2);
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.AddEmotion(EColor.LYellow);
        if (Random.Range(0.0f, 1.0f) <= 0.5)
        {
            PerkInfo info = null;
            foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
            {
                if (p.CurrentPerk.Num == 64)
                {
                    info = p;
                }
            }
            if (info)
            {
                info.CurrentPerk.RemoveEffect();
            }
            ResultText = Self.Name + "感到无聊想要请个假，";
            ResultText += "上司准许" + Self.Name + "请假，并上报CEO，获得情绪状态：愉悦×1，消除事件状态：无聊×1";
        }
        else
        {
            ResultText = Self.Name + "感到无聊想要请个假，";
            ResultText += "上司准许" + Self.Name + "请假，并上报CEO，获得情绪状态：愉悦×1";
        }
        //此时应弹出对话框询问玩家
        EmpManager.Instance.JudgeEvent(this, true);
    }
    public override void OnAccept()
    {
        //放假8工时
        GC.CurrentEmpInfo = Self.InfoDetail;
        GC.CC.SetEmpVacationTime(8);
        GC.CurrentEmpInfo = null;
        GC.CreateMessage(Self.Name + "请假成功，放假8个工时");
        Self.InfoDetail.AddHistory(Self.Name + "请假成功");
    }
    public override void OnRefuse()
    {
        Self.AddEmotion(EColor.LBlue);
        GC.CreateMessage(Self.Name + "请假被CEO拒绝，获得辛酸情绪×1");
        Self.InfoDetail.AddHistory(Self.Name + "请假被CEO拒绝，获得辛酸情绪×1");
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.LRed);
        Self.AddEmotion(EColor.LRed);
        if (Random.Range(0.0f, 1.0f) <= 0.5)
        {
            PerkInfo info = null;
            foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
            {
                if (p.CurrentPerk.Num == 64)
                {
                    info = p;
                }
            }
            if (info)
            {
                info.CurrentPerk.RemoveEffect();
            }
            ResultText = Self.Name + "感到无聊想要请个假，";
            ResultText += "上司回绝了" + Self.Name + "的请假请求，获得情绪状态：侮辱×2，消除事件状态：无聊×1";
        }
        else
        {
            ResultText = Self.Name + "感到无聊想要请个假，";
            ResultText += "上司回绝了" + Self.Name + "的请假请求，获得情绪状态：侮辱×2";
        }
    }
}
public class Event3_43 : Event
{
    public Event3_43() : base()
    {
        EventName = "约周末";
        HaveTarget = true;
        Weight = 5;
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        if (Self.FindRelation(Target).KnowEachOther)
        {
            foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
            {
                if (p.CurrentPerk.Num == 64 & p.CurrentPerk.Level >= 3)
                {//无聊×3
                    return true;
                }
            }
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus() + PerksBonus() + MoraleBonus(2);
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            return 2;
        }
        else
        {
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.AddEmotion(EColor.LYellow);
        Self.ChangeRelation(Target, 15);
        Target.ChangeRelation(Self, 15);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "想要约" + Target.Name + "周末一起出去玩";
        if (Random.Range(0.0f, 1.0f) <= 0.5)
        {
            PerkInfo info = null;
            foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
            {
                if (p.CurrentPerk.Num == 64)
                {
                    info = p;
                }
            }
            if (info)
            {
                info.CurrentPerk.RemoveEffect();
            }
            ObjectText = ResultText + Target.Name + "答应了" + Self.Name + "，双方好感度+15";
            ResultText += Target.Name + "欣然接受，双方好感度+15，获得情绪状态：愉悦×1，消除事件状态：无聊×1";
        }
        else
        {
            ObjectText = ResultText + Target.Name + "答应了" + Self.Name + "，双方好感度+15";
            ResultText += Target.Name + "欣然接受，双方好感度+15，获得情绪状态：愉悦×1";
        }
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        ResultText = "在" + SelfEntity.StandGridName() + "，" + Self.Name + "想要约" + Target.Name + "周末一起出去玩";
        if (Random.Range(0,2) == 0)
        {
            Self.AddEmotion(EColor.LRed);
            Self.ChangeRelation(Target, -15);
            Target.ChangeRelation(Self, -15);
            if (Random.Range(0.0f, 1.0f) <= 0.5)
            {
                PerkInfo info = null;
                foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
                {
                    if (p.CurrentPerk.Num == 64)
                    {
                        info = p;
                    }
                }
                if (info)
                {
                    info.CurrentPerk.RemoveEffect();
                }
                ObjectText = ResultText + Target.Name + "没有答应" + Self.Name + "，双方好感度-15";
                ResultText += Target.Name + "没有接受，双方好感度-15，获得情绪状态：反感×1，消除事件状态：无聊×1";
            }
            else
            {
                ObjectText = ResultText + Target.Name + "没有答应" + Self.Name + "，双方好感度-15";
                ResultText += Target.Name + "没有接受，双方好感度-15，获得情绪状态：反感×1";
            }
        }
        else
        {
            Self.AddEmotion(EColor.LBlue);
            Self.ChangeRelation(Target, -15);
            Target.ChangeRelation(Self, -15);
            if (Random.Range(0.0f, 1.0f) <= 0.5)
            {
                PerkInfo info = null;
                foreach (PerkInfo p in Self.InfoDetail.PerksInfo)
                {
                    if (p.CurrentPerk.Num == 64)
                    {
                        info = p;
                    }
                }
                if (info)
                {
                    info.CurrentPerk.RemoveEffect();
                }
                ObjectText = ResultText + Target.Name + "没有答应" + Self.Name + "，双方好感度-15";
                ResultText += Target.Name + "没有接受，双方好感度-15，获得情绪状态：苦涩×1，消除事件状态：无聊×1";
            }
            else
            {
                ObjectText = ResultText + Target.Name + "没有答应" + Self.Name + "，双方好感度-15";
                ResultText += Target.Name + "没有接受，双方好感度-15，获得情绪状态：苦涩×1";
            }
        }
    }
}
public class Event3_44 : Event
{
    public Event3_44() : base()
    {
        EventName = "希望与对方结成恋人";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
        Weight = 3;
        //条件：没有恋人
    }
    public override bool RelationCheck()
    {
        if (Self.Lover != null)
            return false;
        if (Self.FindRelation(Target).RPoint < 60)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus() + PerksBonus() + MoraleBonus(2);
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        if (value < 10)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.FindRelation(Target).LoveValue = 1;
        Target.FindRelation(Self).LoveValue = 1;
        Self.Lover = Target;
        Target.Lover = Self;
        ResultText = Self.Name + "希望与" + Target.Name + "结为恋人，双方结成恋人";
        ObjectText = Self.Name + "希望与" + Target.Name + "结为恋人，双方结成恋人";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.LBlue);
        Self.ChangeRelation(Target, -15);
        Target.ChangeRelation(Self, -15);
        ResultText = Self.Name + "希望与" + Target.Name + "结为恋人，被对方拒绝，获得蓝色情绪，双方好感度下降15点";
        ObjectText = Self.Name + "希望与" + Target.Name + "结为恋人，被对方拒绝，获得蓝色情绪，双方好感度下降15点";
    }
}
public class Event3_45 : Event
{
    public Event3_45() : base()
    {
        EventName = "结成朋友关系";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
        Weight = 5;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 40)
            return false;
        if (Self.FindRelation(Target).FriendValue == 1 | Self.FindRelation(Target).FriendValue == 2)
        {
            return false;
        }
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus() + PerksBonus() + MoraleBonus(2);
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        ResultText = "成功,";
        return 3;
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.FindRelation(Target).FriendValue = 1;
        Target.FindRelation(Self).FriendValue = 1;
        ResultText = Self.Name + "跟" + Target.Name + "结成朋友关系";
        ObjectText = Self.Name + "跟" + Target.Name + "结成朋友关系";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeRelation(Target, -15);
        Target.ChangeRelation(Self, -15);
        ResultText = Self.Name + "跟" + Target.Name + "没能结成朋友关系，好感度下降15点";
        ObjectText = Self.Name + "跟" + Target.Name + "没能结成朋友关系，好感度下降15点";
    }
}
public class Event3_46 : Event
{
    public Event3_46() : base()
    {
        EventName = "结成挚友";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 1;
        Weight = 5;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 60)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus() + PerksBonus() + MoraleBonus(2);
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        return 3;
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.FindRelation(Self).FriendValue = 2;
        Self.FindRelation(Target).FriendValue = 2;
        ResultText = Self.Name + "与" + Target.Name + "结成了挚友";
        ObjectText = Self.Name + "与" + Target.Name + "结成了挚友";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeRelation(Target, -15);
        Target.ChangeRelation(Self, -15);
        ResultText = Self.Name + "想要与" + Target.Name + "结成挚友，但被对方婉拒了，好感度-15";
        ObjectText = Self.Name + "想要与" + Target.Name + "结成挚友，但被对方婉拒了，好感度-15";
    }
}

public class Event3_47 : Event
{
    public Event3_47() : base()
    {
        EventName = "结成陌路";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
        Weight = 5;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint >-30)
            return false;
        if(Self.FindRelation(Target).FriendValue == -1| Self.FindRelation(Target).FriendValue == -2)
        {
            return false;
        }
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus() + PerksBonus() + MoraleBonus(2);
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        return 2;
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeRelation(Target, -5);
        Target.ChangeRelation(Self, -5);
        ResultText = Self.Name + "与" + Target.Name + "关系缓和了";
        ObjectText = Self.Name + "与" + Target.Name + "关系缓和了";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.FindRelation(Self).FriendValue = -1;
        Self.FindRelation(Target).FriendValue = -1;
        ResultText = Self.Name + "与" + Target.Name + "结为陌路";
        ObjectText = Self.Name + "与" + Target.Name + "结为陌路";
    }
}

public class Event3_48 : Event
{
    public Event3_48() : base()
    {
        EventName = "结成仇人";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = -1;
        Weight = 5;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint > -60)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus() + PerksBonus() + MoraleBonus(2);
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(1, 20);
        value += ExtraValue();
        return 2;
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeRelation(Target, 5);
        Target.ChangeRelation(Self, 5);
        ResultText = Self.Name + "与" + Target.Name + "关系缓和了";
        ObjectText = Self.Name + "与" + Target.Name + "关系缓和了";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.FindRelation(Self).FriendValue = -2;
        Self.FindRelation(Target).FriendValue = -2;
        ResultText = Self.Name + "与" + Target.Name + "结为仇人";
        ObjectText = Self.Name + "与" + Target.Name + "结为仇人";
    }
}
public class Event3_49 : Event
{
    public Event3_49() : base()
    {
        EventName = "心力爆炸";
        HaveTarget = true;
        Weight = 5;
        RelationRequire = 0;
        HaveTarget = false;
    }
    public override bool SpecialCheck()
    {
        if (Self.Mentality <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public override int FindResult()
    {
        return 3;
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        ResultText = Self.Name + "心力爆炸";
        Self.Mentality += 50;
        Self.Exhausted();
    }
}
public class Event3_50 : Event
{
    public Event3_50() : base()
    {
        EventName = "找到上司";
        HaveTarget = true;
        Weight = 10;
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        return false;
        if(EmpManager.Instance.FindBoss(Self) == Target)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public override int FindResult()
    {
        return 3;
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        GC.CreateMessage(Self.Name + "找到上司了");
        ResultText = Self.Name + "找到上司了";
    }
}
static public class EventData
{
    //初始序列
    public static List<Event> InitialList = new List<Event>()
    {
       new Event3_1(),new Event3_2(),new Event3_3(),new Event3_4(),new Event3_5(),new Event3_6(),new Event3_7(),new Event3_8(),new Event3_45(),new Event3_46(),new Event3_47(),new Event3_48()
    };

    //公司序列
    public static List<Event> CompanyList = new List<Event>()
    {
         new Event3_9(),new Event3_10(),new Event3_11(),new Event3_12(),new Event3_13(),new Event3_14(),new Event3_15(),new Event3_16(),new Event3_17(),new Event3_18(),new Event3_19(),new Event3_45(),new Event3_46(),new Event3_47(),new Event3_48()
    };

    //个人关系序列
    public static List<Event> RelationList = new List<Event>()
    {
        new Event3_39(),new Event3_20(),new Event3_21(),new Event3_22(),new Event3_23(),new Event3_24(),new Event3_25(),new Event3_26(),new Event3_27(),new Event3_28(),new Event3_29(),new Event3_30(),new Event3_31(),new Event3_32(),new Event3_33(),new Event3_34(),new Event3_35(),new Event3_36(),new Event3_37(),new Event3_38(),new Event3_40(),new Event3_41(),new Event3_42(),new Event3_43(),new Event3_44(),new Event3_45(),new Event3_46(),new Event3_47(),new Event3_48(),new Event3_49()
    };
}

public class EventGroup
{
    public bool Lock = false;
    public List<Event> eventList;

    /// <summary>
    /// 构造事件组
    /// </summary>
    /// <param name="list">包含的事件列表</param>
    /// <param name="lockGroup">锁定状态</param>
    public EventGroup(List<Event> list, bool lockGroup = false)
    {
        eventList = list;
        Lock = lockGroup;
    }
}