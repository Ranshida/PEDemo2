using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FOEControl : MonoBehaviour
{
    public GameControl GC;
    public GameObject CloseButton, EndButton, ActionPanel, SpyPanel, NeutralAttackSign;
    public Text Text_Info, Text_FightTime, Text_NeutralMarket, Text_Turn;
    public CanvasGroup PanelCanvasGroup;

    public Text[] Text_Costs = new Text[5];
    public Text[] Text_Ranking = new Text[4];
    public Text[] Text_ExtraResult = new Text[4];
    public Text[] Text_ResultButtons = new Text[3];

    public List<FOECompany> Companies = new List<FOECompany>(); //0位是玩家

    public int NeutralMarket = 20, Turn = 1;

    int[] PlayerResultNum = new int[3];
    int FightTime = 96;
    bool FightBegins = false, ResultConfirmed = false;

    private void Start()
    {
        GC.HourEvent.AddListener(TimePass);
    }
    private void Update()
    {
        Text_Turn.text = "第" + Turn + "回合";
        Text_NeutralMarket.text = "剩余自由市场数:" + NeutralMarket;
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
            //此处为临时添加建筑随机的部分
            GC.BM.Lottery(3);
            return;



            foreach(DepControl dep in GC.CurrentDeps)
            {
                dep.ResetFinishedTaskNum();
            }
            FightStart();
        }
        GC.Text_WarTime.text = "距离下次商战还剩" + (FightTime / 8) + "周";
    }

    //商战开始
    void FightStart()
    {
        GC.AskPause(this);
        FightBegins = true;
        ResultConfirmed = false;
        Companies[0].ResetStatus();
        Companies[0].NeutralSkillNum = 0;
        Companies[0].FOESkillNum = 0;
        ActionPanel.GetComponent<WindowBaseControl>().SetWndState(true);
        Text_Info.text = "";
        NeutralMarket = 20;
        Turn = 1;
        PanelCanvasGroup.interactable = true;

        //AI先行动
        for (int i = 1; i < 4; i++)
        {
            Companies[i].ResetStatus();
            if (Companies[i].Morale > 0)
                Companies[i].SetAction();
        }
        if (NeutralMarket > 0)
            NeutralAttackSign.SetActive(true);
        else
            NeutralAttackSign.SetActive(false);
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
            if (Companies[i].Morale > 0)
                Companies[i].SetAction();
            else
            {
                FinishNum += 1;
                Companies[i].ControledMarket = -1;
                continue;
            }
            if (Companies[i].ActionFinish == true)
                FinishNum += 1;
        }
        if (NeutralMarket > 0)
            NeutralAttackSign.SetActive(true);
        else
            NeutralAttackSign.SetActive(false);
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

    public void CreateResult()
    {
        foreach(Text text in Text_ResultButtons)
        {
            text.text = "?";
        }
        List<int> ResultPosb = new List<int>() { 1, 2, 3 };
        int posb = Random.Range(0, 3);
        PlayerResultNum[0] = ResultPosb[posb];
        ResultPosb.RemoveAt(posb);

        posb = Random.Range(0, 2);
        PlayerResultNum[1] = ResultPosb[posb];
        ResultPosb.RemoveAt(posb);

        PlayerResultNum[2] = ResultPosb[0];

    }

    public void RandomResult(int posb)
    {
        if (ResultConfirmed == true)
            return;

        string[] ResultContents = new string[3];

        int ResultNum = PlayerResultNum[posb - 1];
        if (Companies[0].Ranking == 1)
        {
            ResultContents[0] = "士气+10";
            ResultContents[1] = "士气+5";
            ResultContents[2] = "无";
            if (ResultNum == 1)
            {
                GC.Morale += 10;
                Text_ExtraResult[Companies[0].Ranking - 1].text += "士气+10";
            }
            else if (ResultNum == 2)
            {
                GC.Morale += 5;
                Text_ExtraResult[Companies[0].Ranking - 1].text += "士气+5";
            }
            else if (posb == 3)
            {
                Text_ExtraResult[Companies[0].Ranking - 1].text += "无";
            }
        }
        else if (Companies[0].Ranking == 2)
        {
            ResultContents[0] = "士气+5";
            ResultContents[1] = "士气-5";
            ResultContents[2] = "无";
            if (ResultNum == 1)
            {
                GC.Morale += 5;
                Text_ExtraResult[Companies[0].Ranking - 1].text += "士气+5";
            }
            else if (ResultNum == 2)
            {
                GC.Morale -= 5;
                Text_ExtraResult[Companies[0].Ranking - 1].text += "士气-5";
            }
            else if (ResultNum == 3)
            {
                Text_ExtraResult[Companies[0].Ranking - 1].text += "无";
            }
        }
        else if (Companies[0].Ranking == 3)
        {
            ResultContents[0] = "士气-5";
            ResultContents[1] = "随机部门获得业务干扰状态";
            ResultContents[2] = "无";
            if (ResultNum == 1)
            {
                GC.Morale -= 5;
                Text_ExtraResult[Companies[0].Ranking - 1].text += "士气-5";
            }
            else if (ResultNum == 2)
            {
                AddDebuff();
                Text_ExtraResult[Companies[0].Ranking - 1].text += "业务干扰";
            }
            else if (ResultNum == 3)
            {
                Text_ExtraResult[Companies[0].Ranking - 1].text += "无";
            }
        }
        else if (Companies[0].Ranking == 4)
        {
            ResultContents[0] = "士气-10";
            ResultContents[1] = "士气-10";
            ResultContents[2] = "随机部门获得业务干扰状态";
            if (ResultNum == 1)
            {
                GC.Morale -= 10;
                Text_ExtraResult[Companies[0].Ranking - 1].text += "士气-10";
            }
            else if (ResultNum == 2)
            {
                GC.Morale -= 10;
                Text_ExtraResult[Companies[0].Ranking - 1].text += "士气-10";
            }
            else if (ResultNum == 3)
            {
                AddDebuff();
                Text_ExtraResult[Companies[0].Ranking - 1].text += "业务干扰";
            }
        }
        Text_ResultButtons[0].text = ResultContents[PlayerResultNum[0] - 1];
        Text_ResultButtons[1].text = ResultContents[PlayerResultNum[1] - 1];
        Text_ResultButtons[2].text = ResultContents[PlayerResultNum[2] - 1];
        ResultConfirmed = true;
    }

    //商战结束
    public void FightFinish()
    {
        for (int i = 0; i < 4; i++)
        {
            if(i == 0)
                Text_ExtraResult[Companies[i].Ranking - 1].text = "额外效果:";
            if(Companies[i].Morale <= 0)
            {
                Text_ExtraResult[Companies[i].Ranking - 1].text = "";
                continue;
            }
            if (Companies[i].Ranking == 1)
            {
                Companies[i].AddPoint(4);
                Text_Ranking[0].text = Companies[i].Text_Name.text;
                if(i > 0)
                {//非玩家才能这么弄
                    int Posb = Random.Range(1, 4);
                    if(Posb == 1)
                    {
                        Companies[i].Morale += 10;
                        Text_ExtraResult[0].text = "额外效果:士气+10";
                    }
                    else if (Posb == 2)
                    {
                        Companies[i].Morale += 5;
                        Text_ExtraResult[0].text = "额外效果:士气+5";
                    }
                    else if (Posb == 3)
                    {
                        Text_ExtraResult[0].text = "额外效果:无";
                    }                    
                }
            }
            else if (Companies[i].Ranking == 2)
            {
                Companies[i].AddPoint(3);
                Text_Ranking[1].text = Companies[i].Text_Name.text;
                if (i > 0)
                {//非玩家才能这么弄
                    int Posb = Random.Range(1, 4);
                    if (Posb == 1)
                    {
                        Companies[i].Morale += 5;
                        Text_ExtraResult[1].text = "额外效果:士气+5";
                    }
                    else if (Posb == 2)
                    {
                        Companies[i].Morale -= 5;
                        Text_ExtraResult[1].text = "额外效果:士气-5";
                    }
                    else if (Posb == 3)
                    {
                        Text_ExtraResult[1].text = "额外效果:无";
                    }
                }
            }
            else if (Companies[i].Ranking == 3)
            {
                Companies[i].AddPoint(2);
                Text_Ranking[2].text = Companies[i].Text_Name.text;
                if (i > 0)
                {//非玩家才能这么弄
                    int Posb = Random.Range(1, 4);
                    if (Posb == 1)
                    {
                        Companies[i].Morale -= 5;
                        Text_ExtraResult[2].text = "额外效果:士气-5";
                    }
                    else if (Posb == 2)
                    {
                        Companies[i].Morale -= 5;
                        Text_ExtraResult[2].text = "额外效果:士气-5";
                    }
                    else if (Posb == 3)
                    {
                        Text_ExtraResult[2].text = "额外效果:无";
                    }
                }
            }
            else if (Companies[i].Ranking == 4)
            {
                Companies[i].AddPoint(1);
                Text_Ranking[3].text = Companies[i].Text_Name.text;
                if (i > 0)
                {//非玩家才能这么弄
                    int Posb = Random.Range(1, 4);
                    if (Posb == 1)
                    {
                        Companies[i].Morale -= 10;
                        Text_ExtraResult[3].text = "额外效果:士气-10";
                    }
                    else if (Posb == 2)
                    {
                        Companies[i].Morale -= 5;
                        Text_ExtraResult[3].text = "额外效果:士气-5";
                    }
                    else if (Posb == 3)
                    {
                        Companies[i].Morale -= 10;
                        Text_ExtraResult[3].text = "额外效果:士气-10";
                    }
                }
            }
        }
        FightBegins = false;
        GC.BM.Lottery(3);
    }
    public void ResetTime()
    {
        GC.RemovePause(this);
        ActionPanel.GetComponent<WindowBaseControl>().SetWndState(false);
        GC.QC.Finish(7);
        PanelCanvasGroup.interactable = false;
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
            GC.CreateMessage("自由市场不足");
            return;
        }
        GC.FinishedTask[0] -= Companies[0].CostA;
        Text_Info.text += "\n[第" + Turn + "回合]玩家消耗" + Companies[0].CostA + "程序迭代占领了一个自由市场";
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
        SpyPanel.GetComponent<WindowBaseControl>().SetWndState(true);
        ActionPanel.GetComponent<WindowBaseControl>().SetWndState(true);
        if (FightBegins == false)
            PanelCanvasGroup.interactable = false;
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
