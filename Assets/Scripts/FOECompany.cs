using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FOECompany : MonoBehaviour
{
    public int NeutralMarket = 60, ControledMarket = 0, TotalActionPoint = 18, CurrentActionPoint = 0, StoredActionPoint = 0, ExtraActionPoint;
    public int SelfMarketAttackLimit = 0, FoeMarketAttackLimit = 0, NeutralSkillNum = -1, FOESkillNum = -1;//两种技能的释放数量;

    public Text Text_Status;
    public FOEControl FC;

    private void Start()
    {
        TotalActionPoint = 18;
    }
    private void Update()
    {
        Text_Status.text = "中立市场数:" + NeutralMarket + "\n控制市场数:" + ControledMarket;
    }

    public void SetAction()
    {
        FOECompany Target = null;
        ResetStatus();
        CurrentActionPoint = TotalActionPoint - ExtraActionPoint;
        ExtraActionPoint = 0;
        //研发科技
        while (StoredActionPoint >= 10)
        {
            StoredActionPoint -= 10;
            if (Random.Range(0.0f, 1.0f) < 0.35f)
                NeutralMarket += 10;
        }
        while (CurrentActionPoint >= 13)
        {
            //确定目标是中立还是敌人
            if(NeutralMarket > 0)
            {
                if (NeutralSkillNum == -1)
                    NeutralSkillNum = CalcSkillNum();
                CurrentActionPoint -= 3;
                SelfMarketAttackLimit += 3;
            }
            else
            {
                //确定目标
                foreach(FOECompany foe in FC.Companies)
                {
                    if (foe == this)
                        continue;
                    else if (Target == null)
                        Target = foe;
                    else if (foe.ControledMarket > Target.ControledMarket)
                        Target = foe;
                }
                //没有可占领则直接返回
                if (Target == null || Target.ControledMarket == 0)
                    break;
                if (FOESkillNum == -1)
                    FOESkillNum = CalcSkillNum(false);
                CurrentActionPoint -= 3;
                FoeMarketAttackLimit += 3;
            }

            //中立占领判定
            while(SelfMarketAttackLimit > 0 && CurrentActionPoint > 10 && NeutralMarket > 0)
            {
                SelfMarketAttackLimit -= 1;
                CurrentActionPoint -= 10;
                if(Random.Range(0.0f, 1.0f) < 0.5f + (0.1f * NeutralSkillNum))
                {
                    ControledMarket += 1;
                    NeutralMarket -= 1;
                }
            }
            //反占领判定
            while(FoeMarketAttackLimit > 0 && CurrentActionPoint > 10 && Target.ControledMarket > 0)
            {
                FoeMarketAttackLimit -= 1;
                CurrentActionPoint -= 10;
                //因为Neutral的技能两种占领方式都会造成影响
                if (NeutralSkillNum < 0)
                    NeutralSkillNum = 0;
                if (Random.Range(0.0f, 1.0f) < 0.2f + (0.2f * FOESkillNum + (0.1f * NeutralSkillNum)))
                {
                    ControledMarket += 1;
                    Target.ControledMarket -= 1;
                }
            }
        }
        StoredActionPoint += CurrentActionPoint;
    }

    //计算AI技能使用次数
    int CalcSkillNum(bool NeutralTarget = true)
    {
        int num = -1, tempNum = 0;
        while(num == -1)
        {
            float ResultA = CalcMean(tempNum + 1, NeutralTarget), ResultB = CalcMean(tempNum, NeutralTarget);
            //规划外结果直接停止
            if(ResultB < 0)
            {
                num = 0;
                break;
            }

            if (ResultA > ResultB)
                tempNum += 1;
            else
            {
                num = tempNum;
                break;
            }

            //最多100次循环，防止死机
            if(tempNum > 100)
            {
                print("循环超过100次");
                num = 0;
                break;
            }
        }
        return num;
    }
    float CalcMean(int n, bool NeutralTarget = true)
    {
        float Result = 0;
        int x = CurrentActionPoint;
        if (NeutralTarget == true)
        {
            float MarketNum = (((x - (5 * n)) / 33) * 3) + (((x - (5 * n)) % 33 - 3) / 10);
            if (MarketNum > NeutralMarket)
                MarketNum = NeutralMarket;
            Result = MarketNum * (0.5f + (0.1f * n));
        }
        else
            Result = ((((x - (5 * n)) / 33) * 3) + (((x - (5 * n)) % 33 - 3) / 10)) * (0.2f + (0.2f * n));
        return Result;
    }
    //商战开始时重置各项属性
    public void ResetStatus()
    {
        SelfMarketAttackLimit = 0;
        FoeMarketAttackLimit = 0;
        NeutralSkillNum = -1;
        FOESkillNum = -1;
    }
}
