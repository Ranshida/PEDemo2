using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FOEControl : MonoBehaviour
{
    public GameControl GC;
    public GameObject CloseButton, EndButton, ActionPanel, CloseButton2, SpyPanel;
    public Text Text_Info, Text_FightTime;
    public CanvasGroup canvasGroup;

    public List<FOECompany> Companies = new List<FOECompany>(); //0位是玩家


    int FightTime = 96;
    bool FightBegins = false;

    private void Start()
    {
        GC.HourEvent.AddListener(TimePass);
    }
    private void Update()
    {
        Text_Info.text = "中立占领次数:" + Companies[0].SelfMarketAttackLimit + "\n反占领次数:" + Companies[0].FoeMarketAttackLimit +
            "\n\n中立占领成功率:" + ((0.5f + Companies[0].NeutralSkillNum * 0.1f) * 100) + "%\n" + "反占领成功率:"
            + ((0.2f + Companies[0].FOESkillNum * 0.2f + Companies[0].NeutralSkillNum * 0.1f) * 100) + "%";
        if (FightBegins == false)
            Text_FightTime.text = "下一次商战:" + FightTime + "时后开始";
    }


    void TimePass()
    {
        FightTime -= 1;
        if(FightTime == 0)
        {
            FightTime = 96;
            FightStart();
        }
    }
    //仅显示面板
    public void ShowPanel()
    {
        if(FightBegins == false)
        {
            canvasGroup.interactable = false;
            ActionPanel.SetActive(true);
            CloseButton2.SetActive(true);
        }
    }

    //商战开始
    void FightStart()
    {
        canvasGroup.interactable = true;
        CloseButton2.SetActive(false);
        GC.ForceTimePause = true;
        FightBegins = true;
        Companies[0].ResetStatus();
        Companies[0].NeutralSkillNum = 0;
        Companies[0].FOESkillNum = 0;
        EndButton.SetActive(true);
        CloseButton.SetActive(false);
        ActionPanel.SetActive(true);
    }
    //玩家回合结束，开始敌方回合
    public void PlayerEnd()
    {
        for(int i = 1; i < 4; i++)
        {
            Companies[i].SetAction();
        }
        CloseButton.SetActive(true);
    }
    //商战结束
    public void FightFinish()
    {
        GC.ForceTimePause = false;
        for(int i = 1; i < 4; i++)
        {
            Companies[i].TotalActionPoint += 1;
        }
        GC.Income = Companies[0].ControledMarket * 50;
        FightBegins = false;
    }

    //添加中立占领和反占领次数
    public void AddLimitNum(bool NeutralTarget)
    {
        if(GC.FinishedTask[0] >= 3)
        {
            GC.FinishedTask[0] -= 3;
            if (NeutralTarget == true)
                Companies[0].SelfMarketAttackLimit += 3;
            else
                Companies[0].FoeMarketAttackLimit += 3;
        }
    }

    public void UseSkill(int type)
    {
        //Neutral
        if(type == 1)
        {
            if(GC.FinishedTask[0] >= 5)
            {
                GC.FinishedTask[0] -= 5;
                Companies[0].NeutralSkillNum += 1;
            }
        }
        //FOE
        else if (type == 2)
        {
            if(GC.FinishedTask[3] >= 1)
            {
                GC.FinishedTask[3] -= 1;
                Companies[0].FOESkillNum += 1;
            }
        }
        GC.UpdateResourceInfo();
    }

    public void AttackNeutral()
    {
        if (GC.FinishedTask[3] < 2)
        {
            GC.CreateMessage("资源不足");
            return;
        }
        if (Companies[0].NeutralMarket < 1)
        {
            GC.CreateMessage("中立市场不足");
            return;
        }
        if(Companies[0].SelfMarketAttackLimit < 1)
        {
            GC.CreateMessage("中立占领次数不足");
            return;
        }
        Companies[0].SelfMarketAttackLimit -= 1;
        GC.FinishedTask[3] -= 2;
        GC.UpdateResourceInfo();
        if (Random.Range(0.0f, 1.0f) < Companies[0].NeutralSkillNum * 0.1f + 0.5f)
        {
            Companies[0].ControledMarket += 1;
            Companies[0].NeutralMarket -= 1;
            GC.CreateMessage("中立占领成功");
        }
        else
            GC.CreateMessage("中立占领失败");



    }
    public void AttackFOE(int num)
    {
        if (GC.FinishedTask[3] < 2)
        {
            GC.CreateMessage("资源不足");
            return;
        }
        if (Companies[num].ControledMarket < 1)
        {
            GC.CreateMessage("剩余市场不足");
            return;
        }
        if(Companies[0].FoeMarketAttackLimit < 1)
        {
            GC.CreateMessage("反占领次数不足");
            return;
        }
        Companies[0].FoeMarketAttackLimit -= 1;
        GC.FinishedTask[3] -= 2;
        GC.UpdateResourceInfo();
        if (Random.Range(0.0f, 1.0f) < (Companies[0].FOESkillNum * 0.2f) + (Companies[0].NeutralSkillNum * 0.1f) + 0.2f)
        {
            Companies[0].ControledMarket += 1;
            Companies[num].ControledMarket -= 1;
            GC.CreateMessage("反占领成功");
        }
        else
            GC.CreateMessage("反占领失败");


    }

    public void StartRearch()
    {
        if (GC.FinishedTask[2] >= 5 && GC.FinishedTask[7] >= 5)
        {
            if (Random.Range(0.0f, 1.0f) < 0.35f)
            {
                Companies[0].NeutralMarket += 10;
                GC.CreateMessage("研发成功");
            }
            else
                GC.CreateMessage("研发失败");
            GC.UpdateResourceInfo();
        }
        else
            GC.CreateMessage("资源不足");
    }

    public void ShowSpyPanel()
    {
        SpyPanel.SetActive(true);
        ActionPanel.SetActive(true);
        if (FightTime > 0)
            CloseButton2.SetActive(true);
    }
    public void SetSpy(int num)
    {
        if (GC.CurrentEmpInfo == null)
            return;
        Companies[num].ExtraActionPoint += 8;
        GC.CurrentEmpInfo.emp.SpyTime += 32;
        GC.CurrentEmpInfo.emp.InfoDetail.Entity.SetBusy();
        GC.ResetSelectMode();
    }
}
