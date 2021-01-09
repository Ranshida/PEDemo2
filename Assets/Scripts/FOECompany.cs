﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FOECompany : MonoBehaviour
{
    public int NeutralMarket = 60, TotalActionPoint = 18, CurrentActionPoint = 0, StoredActionPoint = 0;
    public int SelfMarketAttackLimit = 0, FoeMarketAttackLimit = 0, NeutralSkillNum = -1, FOESkillNum = -1;//两种技能的释放数量;

    public int Morale = 100;


    //新版本所需变量
    public int ResourceA, ResourceB, ControledMarket = 0, ExtraActionPoint;//A程序迭代 B传播
    public int CostA = 1, CostB = 5, CostC; //A中立占领 B反占领 C护盾 
    public int Level = 0, LevelPoint = 0, Shield = 0;
    public bool isPlayer = false, ActionFinish = false;
    public int Ranking = 1;

    public Text Text_Status, Text_Name;
    public FOEControl FC;

    private void Start()
    {
        TotalActionPoint = 18;
        Morale = 100;
        Level = 0;
    }
    private void Update()
    {
        //if (Morale <= 0)
        //{
        //    Text_Status.text = "已消灭";
        //}
        //else
        //{
        //    if (isPlayer == true)
        //        Morale = FC.GC.Morale;
        //    Text_Status.text = "中立市场:" + NeutralMarket + "\n控制市场:" + ControledMarket + "\n当前士气:" + Morale;
        //}
        Text_Status.text = "融资等级:";
        if (Level == 0)
        {
            Text_Status.text += "--";
            Text_Status.text += "\n升级还需积分:" + (10 - LevelPoint);
        }
        else if (Level == 1)
        {
            Text_Status.text += "天使轮";
            Text_Status.text += "\n升级还需积分:" + (15 - LevelPoint);
        }
        else if (Level == 2)
        {
            Text_Status.text += "A轮";
            Text_Status.text += "\n升级还需积分:" + (20 - LevelPoint);
        }
        else if (Level == 3)
        {
            Text_Status.text += "B轮";
            Text_Status.text += "\n升级还需积分:" + (30 - LevelPoint);
        }
        else if (Level == 4)
        { 
            Text_Status.text += "C轮";
            Text_Status.text += "\n升级还需积分:" + (50 - LevelPoint);
        }
        else if (Level == 5)
        { 
            Text_Status.text += "D轮";
            Text_Status.text += "\n升级还需积分:--";
        }
        if (isPlayer == false)
        {
            Text_Status.text += "\n\n程序迭代:" + ResourceA;
            Text_Status.text += "\n传播:" + ResourceB;
        }
        else
        {
            Text_Status.text += "\n\n程序迭代:" + FC.GC.FinishedTask[0];
            Text_Status.text += "\n传播:" + +FC.GC.FinishedTask[3];
        }
        Text_Status.text += "\n\n当前市场数:" + ControledMarket;
        Text_Status.text += "\n当前名次:" + Ranking;
        Text_Status.text += "\n护盾层数:" + Shield;
    }

    public void SetAction()
    {
        int ActionTime = Random.Range(1, 6);
        int ShieldTime = 2, NeutralAttackTime = 3, FOEAttackTime = 3;
        bool NoTarget = false;
        CostA = 1;
        CostB = 5;
        CostC = 10;
        while(ShieldTime > 0 && ActionTime > 0 && ResourceB >= CostC)
        {
            ResourceB -= CostC;
            FC.Text_Info.text += "\n[第" + FC.Turn + "回合]" + Text_Name.text + "消耗" + CostC + "传播添加了一层护盾"; 
            CostC += 10;
            ActionTime -= 1;
            ShieldTime -= 1;
            Shield += 1;
        }
        while(NeutralAttackTime > 0 && FC.NeutralMarket > 0 && ActionTime > 0 && ResourceA >= CostA)
        {
            ResourceA -= CostA;
            FC.Text_Info.text += "\n[第" + FC.Turn + "回合]" + Text_Name.text + "消耗" + CostA + "程序迭代占领了一个中立市场";
            CostA *= 3;
            FC.NeutralMarket -= 1;
            ControledMarket += 1;
            ActionTime -= 1;
            NeutralAttackTime -= 1;            
        }
        while (FOEAttackTime > 0 && ActionTime > 0 && ResourceA >= CostB)
        {
            FOECompany target = FindTarget();
            if(target == null)
            {
                NoTarget = true;
                break;
            }
            ResourceA -= CostB;
            CostB += 5;
            if (target.Shield > 0)
            {
                target.Shield -= 1;
                FC.Text_Info.text += "\n[第" + FC.Turn + "回合]" + Text_Name.text + "消耗" + CostB + "程序迭代削弱了" + target.Text_Name.text + "一层护盾";
            }
            else
            {
                ControledMarket += 1;
                target.ControledMarket -= 1;
                FC.Text_Info.text += "\n[第" + FC.Turn + "回合]" + Text_Name.text + "消耗" + CostB + "程序迭代抢夺了" + target.Text_Name.text + "一个市场";
            }
            ActionTime -= 1;
            NeutralAttackTime -= 1;
        }
        if(ResourceB < 10 && (ResourceA < 5 || NoTarget == true) && (ResourceA < 1 || FC.NeutralMarket == 0))
        {
            ActionFinish = true;
        }
    }

    public void AddPoint(int value)
    {
        LevelPoint += value;
        //因为积分不会以此获得很多所以不用进行多级检测
        if(Level == 0)
        {
            if (LevelPoint >= 10)
            {
                Level += 1;
                LevelPoint -= 5;
                if (isPlayer == true)
                    FC.GC.Income = 100;
            }
        }
        else if (Level == 1)
        {
            if (LevelPoint >= 15)
            {
                Level += 1;
                LevelPoint -= 10;
                if (isPlayer == true)
                    FC.GC.Income = 200;
            }
        }
        else if (Level == 2)
        {
            if (LevelPoint >= 20)
            {
                Level += 1;
                LevelPoint -= 20;
                if (isPlayer == true)
                    FC.GC.Income = 400;
            }
        }
        else if (Level == 3)
        {
            if (LevelPoint >= 30)
            {
                Level += 1;
                LevelPoint -= 30;
                if (isPlayer == true)
                    FC.GC.Income = 600;
            }
        }
        else if (Level == 4)
        {
            if (LevelPoint >= 50)
            {
                Level += 1;
                LevelPoint -= 50;
                if (isPlayer == true)
                    FC.GC.Income = 900;
            }
        }
    }

    #region 旧商战相关结算
    ////计算AI技能使用次数
    //int CalcSkillNum(bool NeutralTarget = true)
    //{
    //    int num = -1, tempNum = 0;
    //    while(num == -1)
    //    {
    //        float ResultA = CalcMean(tempNum + 1, NeutralTarget), ResultB = CalcMean(tempNum, NeutralTarget);
    //        //规划外结果直接停止
    //        if(ResultB < 0)
    //        {
    //            num = 0;
    //            break;
    //        }

    //        if (ResultA > ResultB)
    //            tempNum += 1;
    //        else
    //        {
    //            num = tempNum;
    //            break;
    //        }

    //        //最多100次循环，防止死机
    //        if(tempNum > 100)
    //        {
    //            print("循环超过100次");
    //            num = 0;
    //            break;
    //        }
    //    }
    //    return num;
    //}
    //float CalcMean(int n, bool NeutralTarget = true)
    //{
    //    float Result = 0;
    //    int x = CurrentActionPoint;
    //    if (NeutralTarget == true)
    //    {
    //        float MarketNum = (((x - (5 * n)) / 33) * 3) + (((x - (5 * n)) % 33 - 3) / 10);
    //        if (MarketNum > NeutralMarket)
    //            MarketNum = NeutralMarket;
    //        Result = MarketNum * (0.5f + (0.1f * n));
    //    }
    //    else
    //        Result = ((((x - (5 * n)) / 33) * 3) + (((x - (5 * n)) % 33 - 3) / 10)) * (0.2f + (0.2f * n));
    //    return Result;
    //}
    //商战开始时重置各项属性
    #endregion
    public void ResetStatus()
    {
        SelfMarketAttackLimit = 0;
        FoeMarketAttackLimit = 0;
        NeutralSkillNum = -1;
        FOESkillNum = -1;

        ControledMarket = 6;
        CostA = 1;
        CostB = 5;
        CostC = 10;
        ActionFinish = false;

        if (Level == 0)
        {
            ResourceA = 15;
            ResourceB = 0;
        }
        else if (Level == 1)
        {
            ResourceA = 20;
            ResourceB = 10;
        }
        else if (Level == 2)
        {
            ResourceA = 30;
            ResourceB = 20;
        }
        else if (Level == 3)
        {
            ResourceA = 60;
            ResourceB = 30;
        }
        else if (Level == 4)
        {
            ResourceA = 80;
            ResourceB = 60;
        }
        else if (Level == 5)
        {
            ResourceA = 100;
            ResourceB = 90;
        }
    }
    public void ResetPlayerStatus()
    {
        CostA = 1;
        CostB = 5;
        CostC = 10;
    }
    FOECompany FindTarget(int type = 1)
    {
        FOECompany target = null;
        List<FOECompany> TList = new List<FOECompany>();
        foreach(FOECompany company in FC.Companies)
        {
            if (company != this && company.ControledMarket > 0)
            {
                TList.Add(company);
            }
        }
        if (TList.Count > 0)
            target = TList[Random.Range(0, TList.Count)];
        return target;
    }
}
