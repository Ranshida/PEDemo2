using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FOEControl : MonoBehaviour
{
    public GameControl GC;
    public GameObject TargetSelectPanel;
    public Text Text_SelectNum, Text_FoeMoney, Text_FoeRequestMoney, Text_Result, Text_Efficiency;
    public List<FOECompany> CurrentCompanies = new List<FOECompany>();
    public Button[] TargetSelectButtons = new Button[3];

    public int ActiveCompanyCount = 3;
    public int FOEMoney = 0, FOEMoneyRequest;
    public int NegotiateSelectNum = 0, FOEEventProtection = 0, SkillEfficiency = 1;
    public float NegotiateEfficiency = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        GC.WeeklyEvent.AddListener(HourPass);
        FOEMoneyRequest = 100;
        ActiveCompanyCount = 3;
    }

    private void Update()
    {
        //Text_FoeMoney.text = "对手金钱:" + FOEMoney + " +30/周";
        //Text_FoeRequestMoney.text = "下次干扰所需金钱:" + FOEMoneyRequest;
        //Text_Result.text = "对手金钱-" + (int)(NegotiateSelectNum * 10 * NegotiateEfficiency);
        //Text_SelectNum.text = NegotiateSelectNum.ToString();
        //if (NegotiateSelectNum > 0)
        //    Text_Result.gameObject.SetActive(true);
        //else
        //    Text_Result.gameObject.SetActive(false);
        Text_Efficiency.text = "技能效率" + SkillEfficiency * 100 + "%";
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
        //FOEMoney += 30;
        //if (FOEMoney >= FOEMoneyRequest)
        //    FOEEvent();
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
                    if ((GC.CurrentOffices[i].building.Type == BuildingType.高管办公室 || GC.CurrentOffices[i].building.Type == BuildingType.CEO办公室) && GC.CurrentOffices[i].CurrentManager != null)
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

    //如果要改消耗，既要改检测(if部分)也要改需求(if括号内减业务的部分)，攻击1的需求和效果在下面的SetTarget()函数中
    //FinishedTask各项所代表的的业务可以在其定义处的注释查找
    public void UseSkill(int type)
    {        
        if(type == 1 && GC.FinishedTask[3] >= 2)//攻击1检测
        {
            TargetSelectPanel.SetActive(true);
        }
        else if (type == 2 && GC.FinishedTask[3] >= 1 && GC.FinishedTask[2] >= 2)//回复1检测
        {
            //回复1需求
            GC.FinishedTask[3] -= 1;
            GC.FinishedTask[2] -= 2;
            //回复1效果
            GC.Morale += 10 * SkillEfficiency;

            SkillEfficiency = 1;
        }
        else if (type == 3 && GC.FinishedTask[3] >= 1 && GC.FinishedTask[2] >= 1 && GC.Morale < 20)//破釜沉舟检测
        {
            //破釜沉舟需求
            GC.FinishedTask[3] -= 1;
            GC.FinishedTask[2] -= 1;
            //破釜沉舟效果
            SkillEfficiency *= 2;
        }
        GC.UpdateResourceInfo();
    }

    public void SetTarget(int type)
    {
        if (GC.CEOSkillNum != 19)
        {
            //攻击1需求
            GC.FinishedTask[3] -= 2;
            //攻击1效果
            CurrentCompanies[type - 1].Morale -= 5 * SkillEfficiency;
            CurrentCompanies[type - 1].CloseCheck();
            SkillEfficiency = 1;
            GC.UpdateResourceInfo();
        }
        else
        {
            GC.CEOSkillNum = 15;
            GC.CEOSkillNum = 0;
            GC.SelectMode = 0;
            int Posb = Random.Range(1, 7);
            Posb += (int)(GC.CurrentEmpInfo.emp.Strategy * 0.2f);
            if (Posb >= 4)
            {
                CurrentCompanies[type - 1].Text_Target.gameObject.SetActive(true);
                CurrentCompanies[type - 1].Text_SkillName.gameObject.SetActive(true);
                GC.CreateMessage("内鬼行动成功,获得了" + CurrentCompanies[type - 1].Text_CompanyName.text + "的信息");
            }
            else
                GC.CreateMessage("内鬼行动失败");
        }
    }
}
