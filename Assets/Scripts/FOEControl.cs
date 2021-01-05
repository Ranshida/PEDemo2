using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FOEControl : MonoBehaviour
{
    public GameControl GC;
    public GameObject CloseButton, EndButton, ActionPanel, CloseButton2, SpyPanel, TipA, TipB;
    public Text Text_Info, Text_FightTime;
    public CanvasGroup canvasGroup;

    public List<FOECompany> Companies = new List<FOECompany>(); //0位是玩家
    public int DamageA, DamageB;//A对玩家的士气伤害 B对玩家的干扰伤害

    int FightTime = 96;
    bool FightBegins = false;

    private void Start()
    {
        GC.HourEvent.AddListener(TimePass);
    }
    private void Update()
    {
        Text_Info.text = "\n中立占领成功率:" + ((0.5f + Companies[0].NeutralSkillNum * 0.1f) * 100) + "%\n" + "反占领成功率:"
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
            foreach(DepControl dep in GC.CurrentDeps)
            {
                dep.ResetFinishedTaskNum();
            }
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
        GC.AskPause(this);
        FightBegins = true;
        TipA.SetActive(false);
        TipB.SetActive(false);
        Companies[0].ResetStatus();
        Companies[0].NeutralSkillNum = 0;
        Companies[0].FOESkillNum = 0;
        EndButton.SetActive(false);
        CloseButton.SetActive(true);
        ActionPanel.SetActive(true);
        DamageA = 0;
        DamageB = 0;
        //AI先行动
        for (int i = 1; i < 4; i++)
        {
            if (Companies[i].Morale > 0)
                Companies[i].SetAction();
        }

        //随机对玩家的两种伤害效果
        if (Random.Range(0.0f, 1.0f) < 0.5f)
        {
            GC.Morale -= DamageA;
            GC.CreateMessage("玩家的士气被对手削弱了" + DamageA + "点");
        }
        else
        {
            Perk p = new Perk116(null);
            p.TempValue1 = DamageB * 5;

            List<DepControl> deps = new List<DepControl>();
            foreach (DepControl dep in GC.CurrentDeps)
            {
                if (dep.building.Type == BuildingType.产品部门 || dep.building.Type == BuildingType.技术部门 || dep.building.Type == BuildingType.市场部门
                     || dep.building.Type == BuildingType.公关营销部 || dep.building.Type == BuildingType.人力资源部)
                    deps.Add(dep);
            }
            if (deps.Count > 0)
            {
                int num = Random.Range(0, deps.Count);
                deps[num].AddPerk(p);
                GC.CreateMessage(deps[num].Text_DepName.text + "受到对手干扰,信念-" + p.TempValue1);
            }
        }
    }
    //玩家回合结束
    public void PlayerEnd()
    {
        CloseButton.SetActive(true);
    }
    //商战结束
    public void FightFinish()
    {
        GC.RemovePause(this);
        //for(int i = 1; i < 4; i++)
        //{
        //    Companies[i].TotalActionPoint += 1;
        //}
        if (Companies[0].ControledMarket < 10)
            GC.Income = 0;
        else if (Companies[0].ControledMarket < 25)
            GC.Income = 100;
        else if (Companies[0].ControledMarket < 40)
            GC.Income = 200;
        else if (Companies[0].ControledMarket < 60)
            GC.Income = 400;
        else if (Companies[0].ControledMarket < 80)
            GC.Income = 600;
        else if (Companies[0].ControledMarket < 110)
            GC.Income = 900;
        else if (Companies[0].ControledMarket >= 110)
            GC.Income = 1200;
        FightBegins = false;
        TipA.SetActive(false);
        TipB.SetActive(false);
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
        if (GC.FinishedTask[0] < 3)
        {
            GC.CreateMessage("资源不足");
            return;
        }
        if (Companies[0].NeutralMarket < 1)
        {
            GC.CreateMessage("中立市场不足");
            return;
        }
        //if(Companies[0].SelfMarketAttackLimit < 1)
        //{
        //    GC.CreateMessage("中立占领次数不足");
        //    return;
        //}
        Companies[0].SelfMarketAttackLimit -= 1;
        GC.FinishedTask[0] -= 3;
        GC.UpdateResourceInfo();
        float Bonus = 0;
        if (GC.NeutralOccupieBonus == true)
            Bonus = 0.1f;
        if (Random.Range(0.0f, 1.0f) < Companies[0].NeutralSkillNum * 0.1f + 0.5f + Bonus)
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
        if (Companies[num].Morale <= 0)
        {
            GC.CreateMessage("该对手已被消灭");
            return;
        }
        if (GC.FinishedTask[0] < 5)
        {
            GC.CreateMessage("资源不足");
            return;
        }
        if (Companies[num].ControledMarket < 1)
        {
            GC.CreateMessage("剩余市场不足");
            return;
        }
        //if(Companies[0].FoeMarketAttackLimit < 1)
        //{
        //    GC.CreateMessage("反占领次数不足");
        //    return;
        //}
        Companies[0].FoeMarketAttackLimit -= 1;
        GC.FinishedTask[0] -= 5;
        GC.UpdateResourceInfo();
        float Bonus = 0;
        if (GC.TargetOccupieBonus == true)
            Bonus = 0.2f;
        if (Random.Range(0.0f, 1.0f) < (Companies[0].FOESkillNum * 0.2f) + (Companies[0].NeutralSkillNum * 0.1f) + 0.2f)
        {
            Companies[0].ControledMarket += 1;
            Companies[num].ControledMarket -= 1;
            GC.CreateMessage("反占领成功");
        }
        else
            GC.CreateMessage("反占领失败");
    }

    public void AttackMorale(int num)
    {
        if (GC.FinishedTask[3] < 1)
        {
            GC.CreateMessage("资源不足");
            return;
        }
        if (Companies[num].Morale <= 0)
        {
            GC.CreateMessage("该对手已被消灭");
            return;
        }
        GC.FinishedTask[3] -= 1;
        Companies[num].Morale -= 5;
        GC.UpdateResourceInfo();
    }
    public void RestoreMorale()
    {
        if (GC.FinishedTask[3] < 3)
        {
            GC.CreateMessage("资源不足");
            return;
        }
        if(GC.Morale == 100)
        {
            GC.CreateMessage("士气已满");
            return;
        }
        GC.FinishedTask[3] -= 3;
        GC.Morale += 5;
        GC.UpdateResourceInfo();
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
        Companies[num].ExtraActionPoint += 2;
        GC.CurrentEmpInfo.emp.SpyTime += 32;
        GC.CurrentEmpInfo.emp.InfoDetail.Entity.SetBusy();
        GC.ResetSelectMode();
    }
}
