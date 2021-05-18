using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FOEControl : MonoBehaviour
{
    private bool warStart = false;
    private int InvestWill = 0;//投资意愿
    public int InvestLevel = 1;//当前投资轮等级

    public int[] PlayerCardCount;

    public InfoPanelTrigger ipt;
    public WindowBaseControl IWPanel;
    public GameObject WarPanel, WarStartButton, WarEndButton;
    public GameControl GC;
    public FOECompany PlayerCompany;
    public Text Text_InvestorInfo, Text_InvestorAttitude, Text_User, Text_UserDetail, Text_Ranking, Text_RankingDetail, Text_Morale,
        Text_MoraleDetail, Text_Cost, Text_CostDetail, Text_CWResult;

    public List<FOECompany> Companies = new List<FOECompany>();

    private void Start()
    {
        GC = GameControl.Instance;
        ipt.ContentB = "不屑 投资意愿<=" + AdjustData.InvestWillLevel[0] + "(无法谈判)" +
            "\n冷淡 " + AdjustData.InvestWillLevel[0] + "<投资意愿<=" + AdjustData.InvestWillLevel[1] +
            "\n一般 " + AdjustData.InvestWillLevel[1] + "<投资意愿<=" + AdjustData.InvestWillLevel[2] +
            "\n欣赏 " + AdjustData.InvestWillLevel[2] + "<投资意愿<=" + AdjustData.InvestWillLevel[3] +
            "\n欢迎 " + AdjustData.InvestWillLevel[3] + "<投资意愿" + "\n意愿级别越高谈判越容易";
    }

    public void CountInvestLevel()
    {
        InvestWill = 0;

        #region 用户数检查
        int u = PlayerCompany.UserCount;
        int[] uL, uR = AdjustData.UserRult;
        if (InvestLevel == 1)
            uL = AdjustData.UserLevel1;
        else if (InvestLevel == 2)
            uL = AdjustData.UserLevel2;
        else if (InvestLevel == 3)
            uL = AdjustData.UserLevel3;
        else if (InvestLevel == 4)
            uL = AdjustData.UserLevel4;
        else
            uL = AdjustData.UserLevel5;

        string UserText1, UserText2, UserText3, UserText4, UserText5;
        UserText1 = "<color=black>用户数<" + uL[0] + " 意愿+"+ uR[0] +"</color>";
        UserText2 = "\n<color=black>" + uL[0] + "<=用户数<"+ uL[1] + " 意愿+" + uR[1] + "</color>";
        UserText3 = "\n<color=black>" + uL[1] + "<=用户数<" + uL[2] + " 意愿+" + uR[2] + "</color>";
        UserText4 = "\n<color=black>" + uL[2] + "<=用户数<" + uL[3] + " 意愿+" + uR[3] + "</color>";
        UserText5 = "\n<color=black>" + uL[3] + "<=用户数 意愿+" + uR[4] + "</color>";

        if (u < uL[0])
        {
            UserText1 = "<color=green>用户数<" + uL[0] + " 意愿+" + uR[0] + "</color>";
            InvestWill += uR[0];
        }
        else if (u < uL[1])
        {
            UserText2 = "\n<color=green>" + uL[0] + "<=用户数<" + uL[1] + " 意愿+" + uR[1] + "</color>";
            InvestWill += uR[1];
        }
        else if (u < uL[2])
        {
            UserText3 = "\n<color=green>" + uL[1] + "<=用户数<" + uL[2] + " 意愿+" + uR[2] + "</color>";
            InvestWill += uR[2];
        }
        else if (u < uL[3])
        {
            UserText4 = "\n<color=green>" + uL[2] + "<=用户数<" + uL[3] + " 意愿+" + uR[3] + "</color>";
            InvestWill += uR[3];
        }
        else
        {
            UserText5 = "\n<color=green>" + uL[3] + "<=用户数 意愿+" + uR[4] + "</color>";
            InvestWill += uR[4];
        }

        Text_User.text = "当前用户数:" + u;
        Text_UserDetail.text = UserText1 + UserText2 + UserText3 + UserText4 + UserText5;
        #endregion

        #region 商战排名检查
        string RankText1, RankText2, RankText3, RankText4;
        int[] rR = AdjustData.RankResult;
        RankText1 = "<color=black>商战排名1 意愿+"+ rR[0] +"</color>";
        RankText2 = "\n<color=black>商战排名2 意愿+" + rR[1] + "</color>";
        RankText3 = "\n<color=black>商战排名3 意愿+" + rR[2] + "</color>";
        RankText4 = "\n<color=black>商战排名>=4 意愿+" + rR[3] + "</color>";

        if (PlayerCompany.Ranking == 1)
        {
            RankText1 = "<color=green>商战排名1 意愿+" + rR[0] + "</color>";
            InvestWill += rR[0];
        }
        else if (PlayerCompany.Ranking == 2)
        {
            RankText2 = "\n<color=green>商战排名2 意愿+" + rR[1] + "</color>";
            InvestWill += rR[1];
        }
        else if (PlayerCompany.Ranking == 3)
        {
            RankText3 = "\n<color=green>商战排名3 意愿+" + rR[2] + "</color>";
            InvestWill += rR[2];
        }
        else if (PlayerCompany.Ranking >= 4)
        {
            RankText4 = "\n<color=green>商战排名>=4 意愿+" + rR[3] + "</color>";
            InvestWill += rR[3];
        }
        Text_Ranking.text = "当前商战排名:" + PlayerCompany.Ranking;
        Text_RankingDetail.text = RankText1 + RankText2 + RankText3 + RankText4;
        #endregion

        #region 士气检查
        string MoraleText1, MoraleText2, MoraleText3, MoraleText4, MoraleText5;
        int[] mR = AdjustData.MoraleResult, mL = AdjustData.MoraleLevel;
        MoraleText1 = "<color=black>士气="+ mL[0] +" 意愿+"+ mR[0] +"</color>";
        MoraleText2 = "\n<color=black>" + mL[1] + "<=士气<" + mL[0] + " 意愿" + mR[1] + "</color>";
        MoraleText3 = "\n<color=black>" + mL[2] + "<=士气<" + mL[1] + " 意愿" + mR[2] + "</color>";
        MoraleText4 = "\n<color=black>" + mL[3] + "<=士气<" + mL[2] + " 意愿" + mR[3] + "</color>";
        MoraleText5 = "\n<color=black>士气<" + mL[3] + " 意愿" + mR[4] + "</color>";

        if (GC.Morale >= mL[0])
        {
            MoraleText1 = "<color=green>士气=" + mL[0] + " 意愿+" + mR[0] + "</color>";
            InvestWill += mR[0];
        }
        else if (GC.Morale >= mL[1])
        {
            MoraleText2 = "\n<color=green>" + mL[1] + "<=士气<" + mL[0] + " 意愿" + mR[1] + "</color>";
            InvestWill += mR[1];
        }
        else if (GC.Morale >= mL[2])
        {
            MoraleText3 = "\n<color=green>" + mL[2] + "<=士气<" + mL[1] + " 意愿" + mR[2] + "</color>";
            InvestWill += mR[2];
        }
        else if (GC.Morale >= mL[3])
        {
            MoraleText4 = "\n<color=green>" + mL[3] + "<=士气<" + mL[2] + " 意愿" + mR[3] + "</color>";
            InvestWill += mR[3];
        }
        else
        {
            MoraleText5 = "\n<color=green>士气<" + mL[3] + " 意愿" + mR[4] + "</color>";
            InvestWill += mR[4];
        }

        Text_Morale.text = "当前公司士气:" + GC.Morale;
        Text_MoraleDetail.text = MoraleText1 + MoraleText2 + MoraleText3 + MoraleText4 + MoraleText5;
        #endregion

        #region 人均成本检查
        string CostText1, CostText2, CostText3, CostText4;
        int[] cR = AdjustData.CostResult, cL = AdjustData.CostLevel;
        CostText1 = "<color=black>人均成本<=" + cL[0] + " 意愿+" + cR[0] + "</color>";
        CostText2 = "\n<color=black>" + cL[0] + "<人均成本<=" + cL[1] + " 意愿" + cR[1] + "</color>";
        CostText3 = "\n<color=black>" + cL[1] + "<人均成本<=" + cL[2] + " 意愿" + cR[2] + "</color>";
        CostText4 = "\n<color=black>" + cL[2] + "<人均成本 意愿" + cR[3] + "</color>";
        int AvgCost = GC.CalcCost() / GC.CurrentEmployees.Count;


        if (AvgCost <= cL[0])
        {
            CostText1 = "<color=green>人均成本<=" + cL[0] + " 意愿+" + cR[0] + "</color>";
            InvestWill += cR[0];
        }
        else if (AvgCost <= cL[1])
        {
            CostText2 = "\n<color=green>" + cL[0] + "<人均成本<=" + cL[1] + " 意愿" + cR[1] + "</color>";
            InvestWill += cR[1];
        }
        else if (AvgCost <= cL[2])
        {
            CostText3 = "\n<color=green>" + cL[1] + "<人均成本<=" + cL[2] + " 意愿" + cR[2] + "</color>";
            InvestWill += cR[2];
        }
        else
        {
            CostText4 = "\n<color=green>" + cL[2] + "<人均成本 意愿" + cR[3] + "</color>";
            InvestWill += cR[3];
        }

        Text_Cost.text = "公司人均成本:" + AvgCost;
        Text_CostDetail.text = CostText1 + CostText2 + CostText3 + CostText4;
        #endregion

        UpdateInvestorInfo();
    }

    //投资人信息更新
    public void UpdateInvestorInfo()
    {
        Text_InvestorInfo.text = "投资人名称:投资人A";

        if (InvestLevel == 1)
            Text_InvestorInfo.text += "\n投资轮次:天使轮";
        else if (InvestLevel == 2)
            Text_InvestorInfo.text += "\n投资轮次:A轮";
        else if (InvestLevel == 3)
            Text_InvestorInfo.text += "\n投资轮次:B轮";
        else if (InvestLevel == 4)
            Text_InvestorInfo.text += "\n投资轮次:C轮";
        else if (InvestLevel == 5)
            Text_InvestorInfo.text += "\n投资轮次:上市";

        Text_InvestorInfo.text += "\n融资金额:" + AdjustData.InvestIncome[InvestLevel - 1] + "/月\n";

        int[] iwL = AdjustData.InvestWillLevel;
        if (InvestWill <= iwL[0])
        {
            Text_InvestorInfo.text += "投资意愿:不屑(" + InvestWill + ")";
            Text_InvestorAttitude.text = "没听说过，不要浪费我的时间";
        }
        else if (InvestWill <= iwL[1])
        {
            Text_InvestorInfo.text += "投资意愿:冷淡(" + InvestWill + ")";
            Text_InvestorAttitude.text = "下一位，你叫什么来着";
        }
        else if (InvestWill <= iwL[2])
        {
            Text_InvestorInfo.text += "投资意愿:一般(" + InvestWill + ")";
            Text_InvestorAttitude.text = "例行公事，你可以过来聊聊";
        }
        else if (InvestWill <= iwL[3])
        {
            Text_InvestorInfo.text += "投资意愿:欣赏(" + InvestWill + ")";
            Text_InvestorAttitude.text = "十分欣赏你们公司，恳切洽谈";
        }
        else
        {
            Text_InvestorInfo.text += "投资意愿:欢迎(" + InvestWill + ")";
            Text_InvestorAttitude.text = "就是你了，印钞机！独角兽！";
        }
    }

    public void PrepareCWar()
    {
        if (warStart == false)
        {
            WarPanel.SetActive(true);
            IWPanel.SetWndState(true);
            warStart = true;
        }
    }

    public void StartCWar()
    {
        foreach(FOECompany c in Companies)
        {
            c.GainUser();
        }

        //玩家用户获取
        float[] BasePosb;
        if (InvestLevel == 1)
            BasePosb = AdjustData.PUserPosb1;
        else if (InvestLevel == 2)
            BasePosb = AdjustData.PUserPosb2;
        else if (InvestLevel == 3)
            BasePosb = AdjustData.PUserPosb3;
        else if (InvestLevel == 4)
            BasePosb = AdjustData.PUserPosb4;
        else
            BasePosb = AdjustData.PUserPosb5;

        float Posb = BasePosb[0] + (BasePosb[1] * PlayerCardCount[0]) + (BasePosb[2] * PlayerCardCount[1]) + (BasePosb[3] * PlayerCardCount[2]);
        int count = 0;
        if (Random.Range(0.0f, 1.0f) < Posb)
        {
            PlayerCompany.UserCount += 1;
            if (InvestLevel < 5 && PlayerCompany.UserCount > AdjustData.PlayerUSerLimit[InvestLevel - 1])
                PlayerCompany.UserCount = AdjustData.PlayerUSerLimit[InvestLevel - 1];
            else
                count += 1;
        }
        Text_CWResult.gameObject.SetActive(true);
        Text_CWResult.text = "生产1级卡牌:" + PlayerCardCount[0] + "\n生产2级卡牌:" + PlayerCardCount[1] + "\n生产3级卡牌" 
            + PlayerCardCount[2] + "\n单次用户获取概率:" + (Posb * 100) + "%\n本次商战用户获取量:" + count;
        WarStartButton.SetActive(false);
        WarEndButton.SetActive(true);

        warStart = false;
        GC.WarTime = 3;
        GC.CheckButtonName();
        RankingSort();
    }

    void RankingSort()
    {
        List<FOECompany> CList = new List<FOECompany>(), SortList = new List<FOECompany>();
        foreach(FOECompany c in Companies)
        {
            CList.Add(c);
        }
        FOECompany target = null;
        int num = 0;
        for (int i = 0; i < 4; i++)
        {
            foreach(FOECompany c in CList)
            {
                if (c.UserCount >= num)
                {
                    target = c;
                    num = c.UserCount;
                }
            }
            print(target.name);
            SortList.Add(target);
            CList.Remove(target);
            num = 0;
            //SortList[i].Ranking = i + 1;
            if (i == 0)
                SortList[0].Ranking = 1;
            else if (SortList[i].UserCount == SortList[i - 1].UserCount)
                SortList[i].Ranking = SortList[i - 1].Ranking;
            else
                SortList[i].Ranking = SortList[i - 1].Ranking + 1;
        }
        for(int i = 0; i < SortList.Count; i++)
        {
            SortList[i].transform.SetSiblingIndex(i);
        }
    }

    public void StartNegotiate()
    {
        //有未处理事件时不能继续
        if (GC.EC.UnfinishedEvents.Count > 0)
            return;
        CountInvestLevel();
        if (InvestWill <= AdjustData.InvestWillLevel[0])
        {
            GC.CreateMessage("按照当前投资意愿无法开始融资");
            return;
        }
        else if (InvestWill <= AdjustData.InvestWillLevel[1])
            GC.BSC.StartEventBossFight(3);
        else if (InvestWill <= AdjustData.InvestWillLevel[2])
            GC.BSC.StartEventBossFight(2);
        else if (InvestWill <= AdjustData.InvestWillLevel[3])
            GC.BSC.StartEventBossFight(2);
        else
            GC.BSC.StartEventBossFight(1);
    }

    public void InvestComplete()
    {
        InvestLevel += 1;
        CountInvestLevel();
        GC.Income = AdjustData.InvestIncome[InvestLevel - 2];
    }
}
