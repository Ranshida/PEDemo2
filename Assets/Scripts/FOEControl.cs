using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FOEControl : MonoBehaviour
{
    public GameControl GC;
    public Text Text_SelectNum, Text_FoeMoney, Text_FoeRequestMoney, Text_Result;

    public int FOEMoney = 0, FOEMoneyRequest;
    public int NegotiateSelectNum = 0, FOEEventProtection = 0;
    public float NegotiateEfficiency = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        GC.WeeklyEvent.AddListener(HourPass);
        FOEMoneyRequest = 100;
    }

    private void Update()
    {
        Text_FoeMoney.text = "对手金钱:" + FOEMoney + " +30/周";
        Text_FoeRequestMoney.text = "下次干扰所需金钱:" + FOEMoneyRequest;
        Text_Result.text = "对手金钱-" + (int)(NegotiateSelectNum * 10 * NegotiateEfficiency);
        Text_SelectNum.text = NegotiateSelectNum.ToString();
        if (NegotiateSelectNum > 0)
            Text_Result.gameObject.SetActive(true);
        else
            Text_Result.gameObject.SetActive(false);
    }

    public void ChangeSelectNum(bool Increase)
    {
        if(Increase == true)
        {
            if (GC.FinishedTask[3] > NegotiateSelectNum)
                NegotiateSelectNum += 1;
        }
        else
        {
            if (NegotiateSelectNum > 0)
                NegotiateSelectNum -= 1;
        }
    }
    public void ConfirmOperation()
    {
        if(NegotiateSelectNum > 0)
        {
            FOEMoney -= (int)(NegotiateSelectNum * 10 * NegotiateEfficiency);
            GC.FinishedTask[3] -= NegotiateSelectNum;
            GC.UpdateResourceInfo();
        }
        ResetSelect();
    }
    public void ResetSelect()
    {
        NegotiateSelectNum = 0;
    }

    void HourPass()
    {
        FOEMoney += 30;
        if (FOEMoney >= FOEMoneyRequest)
            FOEEvent();
    }
    void FOEEvent()
    {
        FOEMoney = 0;
        FOEMoneyRequest = Random.Range(100, 501);
        if (FOEEventProtection > 0)
            FOEEventProtection -= 1;
        else
        {
            int type = Random.Range(1, 9);
            if (type == 1)
            {
                if (GC.PrC.CurrentProduct.Count > 0)
                {
                    int num = Random.Range(0, GC.PrC.CurrentProduct.Count);
                    GC.PrC.CurrentProduct[num].Score[3] -= 100;
                    GC.PrC.CurrentProduct[num].UpdateUI();
                    GC.CreateMessage("由于敌对公司骇客攻击，我们的一个产品安全评分下降100");
                }
            }
            else if (type == 2)
            {
                if (GC.PrC.CurrentProduct.Count > 0)
                {
                    int num = Random.Range(0, GC.PrC.CurrentProduct.Count);
                    for (int i = 0; i < 5; i++)
                    {
                        GC.PrC.CurrentProduct[num].User[i] -= (int)(GC.PrC.CurrentProduct[num].User[i] * 0.04f);
                    }
                    GC.PrC.CurrentProduct[num].UpdateUI();
                    GC.CreateMessage("我们一个产品的用户被敌对公司夺取,用户流失20%");
                }
            }
            else if (type == 3)
            {
                for (int i = 0; i < GC.CurrentDeps.Count; i++)
                {
                    if (GC.CurrentDeps[i].type == EmpType.Tech)
                    {
                        new ProduceBuff(-0.5f, GC.CurrentDeps[i], 96);
                    }
                }
                GC.CreateMessage("遇到技术阻碍，所有技术部门效率下降50%，持续3个月");
            }
            else if (type == 4)
            {
                GC.FinishedTask[4] -= 6;
                if (GC.FinishedTask[4] < 0)
                    GC.FinishedTask[4] = 0;
                GC.UpdateResourceInfo();
            }
            else if (type == 5)
            {
                List<EmpInfo> e = new List<EmpInfo>();
                for (int i = 0; i < GC.CurrentOffices.Count; i++)
                {
                    if (GC.CurrentOffices[i].building.Type == BuildingType.高管办公室 && GC.CurrentOffices[i].CurrentManager != null)
                        e.Add(GC.CurrentOffices[i].CurrentManager.InfoDetail);
                }
                if (e.Count > 0)
                {
                    int num = Random.Range(0, e.Count);
                    GC.CreateMessage("由于敌对公司的猎头行动,高管" + e[num].emp.Name + "离职了");
                    e[num].Fire();
                }
            }
            else if (type == 6)
            {
                GC.Morale -= 15;
                GC.CreateMessage("由于受到敌对公司的强力干扰，士气下降15");
            }
            else if (type == 7)
            {
                GC.DoubleMobilizeCost += 1;
                GC.CreateMessage("由于公司内鬼影响，下一次头脑风暴所需点数上升100%，层数:" + GC.DoubleMobilizeCost);
            }
            else if (type == 8)
            {
                GC.MeetingBlockTime += 96;
                GC.CreateMessage("由于敌对公司的全面打击，3个月时间内无法布置战略");
            }
        }

    }
}
