using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FOEControl : MonoBehaviour
{
    public GameControl GC;
    public GameObject CloseButton, EndButton, ActionPanel, CloseButton2, SpyPanel;
    public CanvasGroup canvasGroup;
    public Text Text_Info, Text_FightTime, Text_NeutralMarket, Text_Turn, Text_Result;

    public Text[] Text_Costs = new Text[5];

    public List<FOECompany> Companies = new List<FOECompany>(); //0位是玩家

    public int NeutralMarket = 20, Turn = 1;

    int FightTime = 96;
    bool FightBegins = false;

    private void Start()
    {
        GC.HourEvent.AddListener(TimePass);
    }
    private void Update()
    {
        Text_Turn.text = "第" + Turn + "回合";
        Text_NeutralMarket.text = "剩余中立市场数:" + NeutralMarket;
        for(int i = 0; i < 3; i++)
        {
            Text_Costs[i].text = "消耗程序迭代:" + Companies[0].CostB;
        }
        Text_Costs[3].text = "消耗程序迭代:" + Companies[0].CostA;
        Text_Costs[4].text = "消耗传播:" + Companies[0].CostC;
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
        Companies[0].ResetStatus();
        Companies[0].NeutralSkillNum = 0;
        Companies[0].FOESkillNum = 0;
        ActionPanel.SetActive(true);
        Text_Info.text = "";
        NeutralMarket = 20;
        Turn = 1;

        //AI先行动
        for (int i = 1; i < 4; i++)
        {
            Companies[i].ResetStatus();
            Companies[i].SetAction();
        }
        SetRank();
    }

    //下一回合
    public void NextTurn()
    {
        int FinishNum = 0;
        Turn += 1;
        if (Companies[0].ActionFinish == true)
            FinishNum = 1;
        for (int i = 1; i < 4; i++)
        {
            Companies[i].SetAction();
            if (Companies[i].ActionFinish == true)
                FinishNum += 1;
        }
        SetRank();
        Companies[0].ResetPlayerStatus();
        if (FinishNum == 4)
        {
            FightBegins = false;

        }
    }

    //玩家不再行动
    public void PlayerEnd()
    {
        int CycleNum = 0;
        Companies[0].ActionFinish = true;
        while(FightBegins == true)
        {
            CycleNum += 1;
            if (CycleNum == 1000)
                break;
            NextTurn();
        }
        if (CycleNum == 1000)
            print("死循环了");
        FightFinish();
    }
    //商战结束
    public void FightFinish()
    {
        for (int i = 0; i < 4; i++)
        {
            if (i == 0)
                Text_Result.text = "名次:" + Companies[0].Ranking;
            if(Companies[i].Ranking == 1)
            {
                Companies[i].AddPoint(4);
                if(i == 0)
                {
                    Text_Result.text += "\n获得积分:4";
                    int posb = Random.Range(1, 4);
                    if (posb == 1)
                    {
                        GC.Morale += 10;
                        Text_Result.text += "\n额外随机效果:士气+10";
                    }
                    else if (posb == 2)
                    {
                        GC.Morale += 5;
                        Text_Result.text += "\n额外随机效果:士气+5";
                    }
                    else if (posb == 3)
                        Text_Result.text += "\n额外随机效果:无";
                }
            }
            else if (Companies[i].Ranking == 2)
            {
                Companies[i].AddPoint(3);
                if (i == 0)
                {
                    Text_Result.text += "\n获得积分:4";
                    int posb = Random.Range(1, 4);
                    if (posb == 1)
                    {
                        GC.Morale += 5;
                        Text_Result.text += "\n额外随机效果:士气+5";
                    }
                    else if (posb == 2)
                    {
                        GC.Morale -= 5;
                        Text_Result.text += "\n额外随机效果:士气-5";
                    }
                    else if (posb == 3)
                        Text_Result.text += "\n额外随机效果:无";
                }
            }
            else if (Companies[i].Ranking == 3)
            {
                Companies[i].AddPoint(2);
                if (i == 0)
                {
                    Text_Result.text += "\n获得积分:2";
                    int posb = Random.Range(1, 4);
                    if (posb == 1)
                    {
                        GC.Morale -= 5;
                        Text_Result.text += "\n额外随机效果:士气-5";
                    }
                    else if (posb == 2)
                    {
                        AddDebuff();
                        Text_Result.text += "\n额外随机效果:随机部门获得业务干扰状态";
                    }
                    else if (posb == 3)
                        Text_Result.text += "\n额外随机效果:无";
                }
            }
            else if (Companies[i].Ranking == 4)
            {
                Companies[i].AddPoint(1);
                if (i == 0)
                {
                    Text_Result.text += "\n获得积分:1";
                    int posb = Random.Range(1, 4);
                    if (posb == 1)
                    {
                        GC.Morale -= 10;
                        Text_Result.text += "\n额外随机效果:士气-10";
                    }
                    else if (posb == 2)
                    {
                        GC.Morale -= 10;
                        Text_Result.text += "\n额外随机效果:士气-10";
                    }
                    else if (posb == 3)
                    {
                        AddDebuff();
                        Text_Result.text += "\n额外随机效果:随机部门获得业务干扰状态";
                    }
                }
            }
        }
        FightBegins = false;

    }
    public void ResetTime()
    {
        GC.RemovePause(this);
        ActionPanel.SetActive(false);
    }

    public void AttackNeutral()
    {
        if (GC.FinishedTask[0] < Companies[0].CostA)
        {
            GC.CreateMessage("资源不足");
            return;
        }
        if (NeutralMarket < 1)
        {
            GC.CreateMessage("中立市场不足");
            return;
        }
        GC.FinishedTask[0] -= Companies[0].CostA;
        Text_Info.text += "\n[第" + Turn + "回合]玩家消耗" + Companies[0].CostA + "程序迭代占领了一个中立市场";
        Companies[0].CostA *= 3;
        Companies[0].ControledMarket += 1;
        NeutralMarket -= 1;
        SetRank();
        GC.UpdateResourceInfo();
    }
    public void AttackFOE(int num)
    {
        if (GC.FinishedTask[0] < Companies[0].CostB)
        {
            GC.CreateMessage("资源不足");
            return;
        }
        if (Companies[num].ControledMarket < 1)
        {
            GC.CreateMessage("剩余市场不足");
            return;
        }
        if (Companies[num].Shield > 0)
        {
            Companies[num].Shield -= 1;
            Text_Info.text += "\n[第" + Turn + "回合]玩家消耗" + Companies[0].CostB + "程序迭代削弱了" + Companies[num].Text_Name.text + "一层护盾";
        }
        else
        {
            Companies[num].ControledMarket -= 1;
            Companies[0].ControledMarket += 1;
            Text_Info.text += "\n[第" + Turn + "回合]玩家消耗" + Companies[0].CostB + "程序迭代抢夺了" + Companies[num].Text_Name.text + "一个市场";
        }
        GC.FinishedTask[0] -= Companies[0].CostB;
        Companies[0].CostB += 5;
        SetRank();
        GC.UpdateResourceInfo();
    }

    public void AddShield()
    {
        if (GC.FinishedTask[3] < Companies[0].CostC)
        {
            GC.CreateMessage("资源不足");
            return;
        }
        GC.FinishedTask[3] -= Companies[0].CostC;
        Text_Info.text += "\n[第" + Turn + "回合]玩家消耗" + Companies[0].CostC + "传播添加了一层护盾";
        Companies[0].Shield += 1;
        Companies[0].CostC += 10;
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

    //新商战

    public void SetRank()
    {
        FOECompany[] TempC = new FOECompany[4];
        for(int i = 0; i < 4; i++)
        {
            TempC[i] = Companies[i];
        }
        for(int i = 0; i < 3; i++)
        {
            for(int j = 0; j < 3 - i; j++)
            {
                if (TempC[j].ControledMarket < TempC[j + 1].ControledMarket)
                {
                    FOECompany TC = TempC[j];
                    TempC[j] = TempC[j + 1];
                    TempC[j + 1] = TC;
                }
            }
        }
        for(int i = 0; i < 4; i++)
        {
            TempC[i].Ranking = i + 1;
        }
    }

    void AddDebuff()
    {
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
            deps[num].AddPerk(new Perk116(null));
            GC.CreateMessage(deps[num].Text_DepName.text + "受到对手干扰,信念-30");
        }
    }
}
