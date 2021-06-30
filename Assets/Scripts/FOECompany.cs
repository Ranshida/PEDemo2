using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FOECompany : MonoBehaviour
{
    public int UserCount = 0, Ranking, Type = 1;//Type:1~3 = A~C公司用户获取策略
    public bool isPlayer = false;

    public Text Text_Ranking, Text_User;
    private GameControl GC;

    //用户获取
    public void GainUser()
    {
        if (isPlayer == true)
        {
            if (GC == null)
                GC = GameControl.Instance;
            GC.foeControl.PlayerCardCount = new int[3] { 0, 0, 0 };
            foreach (DivisionControl div in GC.CurrentDivisions)
            {
                if (div.CWDep != null && div.canWork == true && div.CWDep.canWork == true && GC.TotalEfficiency > 0 && GC.TotalWorkStatus > 0)
                {
                    foreach (CWCardInfo card in div.CWCards)
                    {
                        if (card.CurrentCard != null && card.CurrentLevel > 0 && card.CurrentNum > 0)
                            GC.foeControl.PlayerCardCount[card.CurrentLevel - 1] += card.CurrentNum;
                    }
                }
            }
        }
        else
        {
            int[] UOb;
            float[] Posb;
            if (Type == 1)
            {
                UOb = AdjustData.UserObtain_A;
                Posb = AdjustData.UserPosb_A;
            }
            else if (Type == 2)
            {
                UOb = AdjustData.UserObtain_B;
                Posb = AdjustData.UserPosb_B;
            }
            else
            {
                UOb = AdjustData.UserObtain_C;
                Posb = AdjustData.UserPosb_C;
            }

            float r = Random.Range(0.0f, 1.0f);
            if (r < Posb[0])
                UserCount += UOb[0];
            else if (r < Posb[2])
                UserCount += UOb[1];
            else if (r < Posb[3])
                UserCount += UOb[2];
            else
                UserCount += UOb[3];
            if (UserCount < 0)
                UserCount = 0;
        }
        UpdateUI();
    }

    public void UpdateUI()
    {
        Text_Ranking.text = Ranking.ToString();
        Text_User.text = UserCount.ToString();
    }
}
