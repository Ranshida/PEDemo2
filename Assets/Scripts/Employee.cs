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
        Forecast, Strategy, Convince, Charm, Gossip, SalaryExtra = 0, Age, EventTime, ObeyTime, NoPromotionTime = 0, NoMarriageTime = 0;
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
    public int[] Levels = new int[5]; //0职业(业务) 1基础 2经营 3战略 4社交
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
    public List<Relation> Relations = new List<Relation>();

    int mentality, stamina;

    //初始化员工属性
    public void InitStatus(EmpType type, int[] Hst, int AgeRange)
    {
        Type = type;
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

        stamina = 100;
        mentality = 100;

        #region 旧招聘
        ////职业技能
        //if (Hst[0] == 0)
        //{
        //    if (AgeRange == 0)
        //        Skill1 = Random.Range(1, 6);
        //    else
        //        Skill1 = Random.Range(1, 4);
        //}
        //else
        //    Skill1 = HeadHuntLevel;

        //if (Hst[1] == 0)
        //{
        //    if (AgeRange == 0)
        //        Skill2 = Random.Range(1, 6);
        //    else
        //        Skill2 = Random.Range(1, 4);
        //}
        //else
        //    Skill2 = HeadHuntLevel;

        //if (Hst[2] == 0)
        //{
        //    if (AgeRange == 0)
        //        Skill3 = Random.Range(1, 6);
        //    else
        //        Skill3 = Random.Range(1, 4);
        //}
        //else
        //    Skill3 = HeadHuntLevel;
        //Levels[0] = Skill1 + Skill2 + Skill3;
        ////基础技能
        //if (Hst[3] == 0)
        //{
        //    if (AgeRange == 0)
        //        Observation = Random.Range(1, 6);
        //    else
        //        Observation = Random.Range(1, 4);
        //}
        //else
        //    Observation = HeadHuntLevel;

        //if (Hst[4] == 0)
        //{
        //    if (AgeRange == 0)
        //        Tenacity = Random.Range(1, 6);
        //    else
        //        Tenacity = Random.Range(1, 4);
        //}
        //else
        //    Tenacity = HeadHuntLevel;

        //if (Hst[5] == 0)
        //{
        //    if (AgeRange == 0)
        //        Strength = Random.Range(1, 6);
        //    else
        //        Strength = Random.Range(1, 4);
        //}
        //else
        //    Strength = HeadHuntLevel;
        //Levels[1] = Observation + Tenacity + Strength;
        ////经营技能
        //if (Hst[6] == 0)
        //{
        //    if (AgeRange == 0)
        //        Manage = Random.Range(1, 6);
        //    else
        //        Manage = Random.Range(1, 4);
        //}
        //else
        //    Manage = HeadHuntLevel;

        //if (Hst[7] == 0)
        //{
        //    if (AgeRange == 0)
        //        HR = Random.Range(1, 6);
        //    else
        //        HR = Random.Range(1, 4);
        //}
        //else
        //    HR = HeadHuntLevel;

        //if (Hst[8] == 0)
        //{
        //    if (AgeRange == 0)
        //        Finance = Random.Range(1, 6);
        //    else
        //        Finance = Random.Range(1, 4);
        //}
        //else
        //    Finance = HeadHuntLevel;
        //Levels[2] = Manage + HR + Finance;
        ////战略技能
        //if (Hst[9] == 0)
        //{
        //    if (AgeRange == 0)
        //        Decision = Random.Range(1, 6);
        //    else
        //        Decision = Random.Range(1, 4);
        //}
        //else
        //    Decision = HeadHuntLevel;

        //if (Hst[10] == 0)
        //{
        //    if (AgeRange == 0)
        //        Forecast = Random.Range(1, 6);
        //    else
        //        Forecast = Random.Range(1, 4);
        //}
        //else
        //    Forecast = HeadHuntLevel;

        //if (Hst[11] == 0)
        //{
        //    if (AgeRange == 0)
        //        Strategy = Random.Range(1, 6);
        //    else
        //        Strategy = Random.Range(1, 4);
        //}
        //else
        //    Strategy = HeadHuntLevel;
        //Levels[3] = Decision + Forecast + Strategy;
        ////社交技能        
        //if (Hst[12] == 0)
        //{
        //    if (AgeRange == 0)
        //        Convince = Random.Range(1, 6);
        //    else
        //        Convince = Random.Range(1, 4);
        //}
        //else
        //    Convince = HeadHuntLevel;

        //if (Hst[13] == 0)
        //{
        //    if (AgeRange == 0)
        //        Charm = Random.Range(1, 6);
        //    else
        //        Charm = Random.Range(1, 4);
        //}
        //else
        //    Charm = HeadHuntLevel;

        //if (Hst[14] == 0)
        //{
        //    if (AgeRange == 0)
        //        Gossip = Random.Range(1, 6);
        //    else
        //        Gossip = Random.Range(1, 4);
        //}
        //else
        //    Gossip = HeadHuntLevel;
        //Levels[4] = Convince + Charm + Gossip;
        #endregion

        //职业技能
        if (Hst[0] == 0)
            Skill1 = Random.Range(0, 4);
        else if (Hst[0] == 1)
            Skill1 = Random.Range(0, 3);
        else if (Hst[0] == 2)
            Skill1 = Random.Range(5, 10);

        if (Hst[1] == 0)
            Skill2 = Random.Range(0, 4);
        else if (Hst[1] == 1)
            Skill2 = Random.Range(0, 3);
        else if (Hst[1] == 2)
            Skill2 = Random.Range(5, 10);

        if (Hst[2] == 0)
            Skill3 = Random.Range(0, 4);
        else if (Hst[2] == 1)
            Skill3 = Random.Range(0, 3);
        else if (Hst[2] == 2)
            Skill3 = Random.Range(5, 10);
        Levels[0] = Skill1 + Skill2 + Skill3;

        //基础技能
        if (Hst[3] == 0)
            Observation = Random.Range(0, 4);
        else if (Hst[3] == 1)
            Observation = Random.Range(0, 3);
        else if (Hst[3] == 2)
            Observation = Random.Range(5, 10);

        if (Hst[4] == 0)
            Tenacity = Random.Range(0, 4);
        else if (Hst[4] == 1)
            Tenacity = Random.Range(0, 3);
        else if (Hst[4] == 2)
            Tenacity = Random.Range(5, 10);

        if (Hst[5] == 0)
            Strength = Random.Range(0, 4);
        else if (Hst[5] == 1)
            Strength = Random.Range(0, 3);
        else if (Hst[5] == 2)
            Strength = Random.Range(5, 10);
        Levels[1] = Observation + Tenacity + Strength;

        //经营技能
        if (Hst[6] == 0)
            Manage = Random.Range(0, 4);
        else if (Hst[6] == 1)
            Manage = Random.Range(0, 3);
        else if (Hst[6] == 2)
            Manage = Random.Range(5, 10);

        if (Hst[7] == 0)
            HR = Random.Range(0, 4);
        else if (Hst[7] == 1)
            HR = Random.Range(0, 3);
        else if (Hst[7] == 2)
            HR = Random.Range(5, 10);

        if (Hst[8] == 0)
            Finance = Random.Range(0, 4);
        else if (Hst[8] == 1)
            Finance = Random.Range(0, 3);
        else if (Hst[8] == 2)
            Finance = Random.Range(5, 10);
        Levels[2] = Manage + HR + Finance;

        //战略技能
        if (Hst[9] == 0)
            Decision = Random.Range(0, 4);
        else if (Hst[9] == 1)
            Decision = Random.Range(0, 3);
        else if (Hst[9] == 2)
            Decision = Random.Range(5, 10);

        if (Hst[10] == 0)
            Forecast = Random.Range(0, 4);
        else if (Hst[10] == 1)
            Forecast = Random.Range(0, 3);
        else if (Hst[10] == 2)
            Forecast = Random.Range(5, 10);

        if (Hst[11] == 0)
            Strategy = Random.Range(0, 4);
        else if (Hst[11] == 1)
            Strategy = Random.Range(0, 3);
        else if (Hst[11] == 2)
            Strategy = Random.Range(5, 10);
        Levels[3] = Decision + Forecast + Strategy;

        //社交技能        
        if (Hst[12] == 0)
            Convince = Random.Range(0, 4);
        else if (Hst[12] == 1)
            Convince = Random.Range(0, 3);
        else if (Hst[12] == 2)
            Convince = Random.Range(5, 10);

        if (Hst[13] == 0)
            Charm = Random.Range(0, 4);
        else if (Hst[13] == 1)
            Charm = Random.Range(0, 3);
        else if (Hst[13] == 2)
            Charm = Random.Range(5, 10);

        if (Hst[14] == 0)
            Gossip = Random.Range(0, 4);
        else if (Hst[14] == 1)
            Gossip = Random.Range(0, 3);
        else if (Hst[14] == 2)
            Gossip = Random.Range(5, 10);
        Levels[4] = Convince + Charm + Gossip;

        //确定年龄
        if (AgeRange == 0)
            Age = Random.Range(20, 25);
        else
            Age = 25 + AgeRange * 5;
        
        //确定热情(Star)和天赋(StarLimit)
        int TopStartNum = (Random.Range(0, 5)), TopStartNum2 = (Random.Range(0, 5));
        while (TopStartNum == TopStartNum2)
        {
            TopStartNum2 = (Random.Range(0, 5));
        }
        for (int i = 0; i < 5; i++)
        {
            //旧的天赋随机值
            //if (i == TopStartNum)
            //    StarLimit[i] = Random.Range(1, 6);
            //else
            //    StarLimit[i] = Random.Range(0, 3);

            if (i == TopStartNum || i == TopStartNum2)
                StarLimit[i] = Random.Range(1, 5);
            else
                StarLimit[i] = Random.Range(0, 2);

            if (AgeRange == 0)
                Stars[i] = Random.Range(0, StarLimit[i] * 5 + 1);
            else
                Stars[i] = 0;
        }
        //确定倾向
        for (int i = 0; i < 5; i++)
        {
            Character[i] = Random.Range(-100, 101);
            if (i == 4)
                Character[4] = Random.Range(0, 101);
        }

        EventTime = Random.Range(6, 12);
    }

    //初始化CEO属性
    public void InitCEOStatus()
    {
        isCEO = true;
        Skill1 = 5; Skill2 = 5; Skill3 = 5;
        Observation = 5; Tenacity = 5; Strength = 5; Manage = 5; HR = 5; Finance = 5; Decision = 5;
        Forecast = 5; Strategy = 5; Convince = 5; Charm = 5; Gossip = 5; Age = 25;
        stamina = 100;
        mentality = 100;

        Name = "CEO";

        //确定热情(Star)和天赋(StarLimit)
        int TopStarNum = Random.Range(0, 5), TopStarNum2 = Random.Range(0, 5);
        while(TopStarNum == TopStarNum2)
        {
            TopStarNum2 = Random.Range(0, 5);
        }
        for (int i = 0; i < 5; i++)
        {
            if (i == TopStarNum || i == TopStarNum2)
                StarLimit[i] = Random.Range(1, 6);
            else
                StarLimit[i] = Random.Range(0, 3);
            Stars[i] = Random.Range(0, StarLimit[i] * 5 + 1);
        }
        //确定倾向
        for (int i = 0; i < 5; i++)
        {
            Character[i] = Random.Range(-100, 101);
            if (i == 4)
                Character[4] = Random.Range(0, 101);
        }
        EventTime = Random.Range(6, 12);
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
        float Efficiency = 1 - AgePenalty + (Stars[SNum] / 5) * 0.2f;
        if (Efficiency < 0)
            Efficiency = 0;
        SkillExp[type - 1] += (int)(value * Efficiency);

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
        r.UpdateUI();
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
        EventTime -= 1;
        if (EventTime == 0)
        {
            AddEvent();
            EventTime = Random.Range(12, 21);
        }

        if (CurrentOffice == null)
            NoPromotionTime += 1;
        else if (CurrentOffice.building.Type != BuildingType.CEO办公室 || CurrentOffice.building.Type != BuildingType.高管办公室)
            NoPromotionTime += 1;

        if (Lover == null)
            NoMarriageTime += 1;
        else if (FindRelation(Lover).LoveValue < 3)
            NoMarriageTime += 1;
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
