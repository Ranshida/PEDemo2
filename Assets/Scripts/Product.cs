using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Product : MonoBehaviour
{
    public int TotalProfit;

    public int[,] ScoreExtra = new int[5, 4]; //x:农民 工人 学生 白领 商贩, y:美学 功能性 流畅度 安全性 *负数的话直接给负值*
    public int[,] CurrentScore = new int[5, 4];
    public int[] User = new int[5];
    public int[] Score = new int[4];
    public int[] Profit = new int[5];

    public Block TargetBlock;
    public ProductControl PC;

    public Text[] Text_Total = new Text[6];
    public Text[] Text_Farmer = new Text[6];
    public Text[] Text_Worker = new Text[6];
    public Text[] Text_Student = new Text[6];
    public Text[] Text_WhiteColor = new Text[6];
    public Text[] Text_Merchant = new Text[6];

    public void CalcUser()
    {
        CurrentScore[0, 0] = (int)(Score[0] * 0.05f + ScoreExtra[0, 0]);
        CurrentScore[0, 1] = (int)(Score[1] * 0.1f + ScoreExtra[0, 1]);
        CurrentScore[0, 2] = (int)(Score[2] * 0.6f + ScoreExtra[0, 2]);
        CurrentScore[0, 3] = (int)(Score[3] * 0.25f + ScoreExtra[0, 3]);

        CurrentScore[1, 0] = (int)(Score[0] * 0.1f + ScoreExtra[1, 0]);
        CurrentScore[1, 1] = (int)(Score[1] * 0.4f + ScoreExtra[1, 1]);
        CurrentScore[1, 2] = (int)(Score[2] * 0.35f + ScoreExtra[1, 2]);
        CurrentScore[1, 3] = (int)(Score[3] * 0.15f + ScoreExtra[1, 3]);

        CurrentScore[2, 0] = (int)(Score[0] * 0.4f + ScoreExtra[2, 0]);
        CurrentScore[2, 1] = (int)(Score[1] * 0.35f + ScoreExtra[2, 1]);
        CurrentScore[2, 2] = (int)(Score[2] * 0.15f + ScoreExtra[2, 2]);
        CurrentScore[2, 3] = (int)(Score[3] * 0.10f + ScoreExtra[2, 3]);

        CurrentScore[3, 0] = (int)(Score[0] * 0.5f + ScoreExtra[3, 0]);
        CurrentScore[3, 1] = (int)(Score[1] * 0.15f + ScoreExtra[3, 1]);
        CurrentScore[3, 2] = (int)(Score[2] * 0.05f + ScoreExtra[3, 2]);
        CurrentScore[3, 3] = (int)(Score[3] * 0.30f + ScoreExtra[3, 3]);

        CurrentScore[4, 0] = (int)(Score[0] * 0 + ScoreExtra[4, 0]);
        CurrentScore[4, 1] = (int)(Score[1] * 0.35f + ScoreExtra[4, 1]);
        CurrentScore[4, 2] = (int)(Score[2] * 0.2f + ScoreExtra[4, 2]);
        CurrentScore[4, 3] = (int)(Score[3] * 0.45f + ScoreExtra[4, 3]);
    }

    public void AddUser()
    {
        CalcUser();
        int[] u = new int[5];
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                u[i] += CurrentScore[i, j];
            }
            if (u[i] > TargetBlock.User[i])
                u[i] = TargetBlock.User[i];
            else if (u[i] < 0 && Mathf.Abs(u[i]) > User[i])
                u[i] = User[i] * -1;

            TargetBlock.User[i] -= u[i];
            User[i] += u[i];
        }
        UpdateUI();
    }
    public void UpdateUI()
    {
        for (int i = 0; i < 4; i++)
        {
            Text_Farmer[i].text = (CurrentScore[0, i]).ToString();
            Text_Worker[i].text = (CurrentScore[1, i]).ToString();
            Text_Student[i].text = (CurrentScore[2, i]).ToString();
            Text_WhiteColor[i].text = (CurrentScore[3, i]).ToString();
            Text_Merchant[i].text = (CurrentScore[4, i]).ToString();
        }

        Text_Farmer[4].text = User[0].ToString();
        Text_Worker[4].text = User[1].ToString();
        Text_Student[4].text = User[2].ToString();
        Text_WhiteColor[4].text = User[3].ToString();
        Text_Merchant[4].text = User[4].ToString();

        Text_Farmer[5].text = (User[0] * Profit[0]) + "/月";
        Text_Worker[5].text = (User[1] * Profit[1]) + "/月";
        Text_Student[5].text = (User[2] * Profit[2]) + "/月";
        Text_WhiteColor[5].text = (User[3] * Profit[3]) + "/月";
        Text_Merchant[5].text = (User[4] * Profit[4]) + "/月";

        Text_Total[0].text = Score[0].ToString();
        Text_Total[1].text = Score[1].ToString();
        Text_Total[2].text = Score[2].ToString();
        Text_Total[3].text = Score[3].ToString();
        int total = 0;
        int profit = 0;
        for(int i = 0; i < 5; i++)
        {
            total += User[i];
            profit += User[i] * Profit[i];
        }
        TotalProfit = profit;
        Text_Total[4].text = total.ToString();
        Text_Total[5].text = profit.ToString() + "/月";

    }

    public void OpenICPanel()
    {
        PC.IC.TargetProduct = this;
        PC.IC.gameObject.SetActive(true);
        PC.IC.RefreshPanel();
    }
}
