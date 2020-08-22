using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Product : MonoBehaviour
{
    public int TotalProfit;

    public int[,] ScoreExtra = new int[5, 4]; //x:农民 工人 学生 白领 商贩, y:美学 功能性 流畅度 安全性 *负数的话直接给负值*
    public int[,] CurrentScore = new int[5, 4];
    public int[,] ScoreLevel = new int[5, 4];
    public string[,] LevelText = new string[5, 4];
    public float[,] ScorePercent = new float[5, 4]
        {
            {0.05f, 0.1f, 0.6f, 0.25f },
            {0.1f, 0.4f, 0.35f, 0.15f },
            {0.4f, 0.35f, 0.15f, 0.1f },
            {0.5f, 0.15f, 0.05f, 0.3f },
            {0, 0.35f, 0.2f, 0.45f }
        };
    public bool[,] ShowScoreLevel = new bool[5, 4];
    public int[] User = new int[5];
    public int[] Score = new int[4];
    public int[] Profit = new int[5];

    public Block TargetBlock;
    public ProductControl PC;
    public GameObject CheckPanel;

    public Text[] Text_Total = new Text[6];
    public Text[] Text_Farmer = new Text[6];
    public Text[] Text_Worker = new Text[6];
    public Text[] Text_Student = new Text[6];
    public Text[] Text_WhiteColor = new Text[6];
    public Text[] Text_Merchant = new Text[6];

    private void Start()
    {
        UpdateScoreLevel();
    }

    public void CalcUser()
    {
        CurrentScore[0, 0] = (int)(Score[0] * ScorePercent[0, 0] + ScoreExtra[0, 0]);
        CurrentScore[0, 1] = (int)(Score[1] * ScorePercent[0, 1] + ScoreExtra[0, 1]);
        CurrentScore[0, 2] = (int)(Score[2] * ScorePercent[0, 2] + ScoreExtra[0, 2]);
        CurrentScore[0, 3] = (int)(Score[3] * ScorePercent[0, 3] + ScoreExtra[0, 3]);

        CurrentScore[1, 0] = (int)(Score[0] * ScorePercent[1, 0] + ScoreExtra[1, 0]);
        CurrentScore[1, 1] = (int)(Score[1] * ScorePercent[1, 1] + ScoreExtra[1, 1]);
        CurrentScore[1, 2] = (int)(Score[2] * ScorePercent[1, 2] + ScoreExtra[1, 2]);
        CurrentScore[1, 3] = (int)(Score[3] * ScorePercent[1, 3] + ScoreExtra[1, 3]);

        CurrentScore[2, 0] = (int)(Score[0] * ScorePercent[2, 0] + ScoreExtra[2, 0]);
        CurrentScore[2, 1] = (int)(Score[1] * ScorePercent[2, 1] + ScoreExtra[2, 1]);
        CurrentScore[2, 2] = (int)(Score[2] * ScorePercent[2, 2] + ScoreExtra[2, 2]);
        CurrentScore[2, 3] = (int)(Score[3] * ScorePercent[2, 3] + ScoreExtra[2, 3]);

        CurrentScore[3, 0] = (int)(Score[0] * ScorePercent[3, 0] + ScoreExtra[3, 0]);
        CurrentScore[3, 1] = (int)(Score[1] * ScorePercent[3, 1] + ScoreExtra[3, 1]);
        CurrentScore[3, 2] = (int)(Score[2] * ScorePercent[3, 2] + ScoreExtra[3, 2]);
        CurrentScore[3, 3] = (int)(Score[3] * ScorePercent[3, 3] + ScoreExtra[3, 3]);

        CurrentScore[4, 0] = (int)(Score[0] * ScorePercent[4, 0] + ScoreExtra[4, 0]);
        CurrentScore[4, 1] = (int)(Score[1] * ScorePercent[4, 1] + ScoreExtra[4, 1]);
        CurrentScore[4, 2] = (int)(Score[2] * ScorePercent[4, 2] + ScoreExtra[4, 2]);
        CurrentScore[4, 3] = (int)(Score[3] * ScorePercent[4, 3] + ScoreExtra[4, 3]);
    }

    public void AddUser(float Multip = 0.1f)
    {
        CalcUser();
        int[] u = new int[5];
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                u[i] += CurrentScore[i, j];
            }
            u[i] = (int)(u[i] * Multip);
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
        //原直接更新具体数字代码
        //for (int i = 0; i < 4; i++)
        //{
        //    Text_Farmer[i].text = (CurrentScore[0, i]).ToString();
        //    Text_Worker[i].text = (CurrentScore[1, i]).ToString();
        //    Text_Student[i].text = (CurrentScore[2, i]).ToString();
        //    Text_WhiteColor[i].text = (CurrentScore[3, i]).ToString();
        //    Text_Merchant[i].text = (CurrentScore[4, i]).ToString();
        //}

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
        for (int i = 0; i < 5; i++)
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

    //更新用户喜好文本
    public void UpdateScoreLevel()
    {
        SetScoreLevel();
        for (int i = 0; i < 4; i++)
        {
            Text_Farmer[i].text = LevelText[0, i];
            if (ShowScoreLevel[0, i] == true)
                Text_Farmer[i].gameObject.SetActive(true);
            else
                Text_Farmer[i].gameObject.SetActive(false);

            Text_Worker[i].text = LevelText[1, i];
            if (ShowScoreLevel[1, i] == true)
                Text_Worker[i].gameObject.SetActive(true);
            else
                Text_Worker[i].gameObject.SetActive(false);

            Text_Student[i].text = LevelText[2, i];
            if (ShowScoreLevel[2, i] == true)
                Text_Student[i].gameObject.SetActive(true);
            else
                Text_Student[i].gameObject.SetActive(false);

            Text_WhiteColor[i].text = LevelText[3, i];
            if (ShowScoreLevel[3, i] == true)
                Text_WhiteColor[i].gameObject.SetActive(true);
            else
                Text_WhiteColor[i].gameObject.SetActive(false);

            Text_Merchant[i].text = LevelText[4, i];
            if (ShowScoreLevel[4, i] == true)
                Text_Merchant[i].gameObject.SetActive(true);
            else
                Text_Merchant[i].gameObject.SetActive(false);
        }
    }

    //特殊事件
    public void SpecialEvent(int num)
    {
        if (num == 1)
        {
            ChangePercent(-0.2f, 0.15f, 0.1f, -0.05f, 3, 0);
            ChangePercent(0, -0.05f, 0.1f, -0.05f, 2, 1);
            ChangePercent(0, 0.1f, -0.05f, -0.05f, 3, 2);
            ChangePercent(-0.05f, -0.05f, -0.05f, 0.15f, -2, 3);
            ChangePercent(-0.05f, -0.05f, -0.1f, 0.2f, -2, 4);
        }
        else if (num == 2)
        {
            ChangePercent(0.1f, -0.05f, -0.1f, 0.05f, -1, 0);
            ChangePercent(0, -0.05f, -0.05f, 0.1f, -1, 1);
            ChangePercent(0, -0.05f, -0.05f, 0.1f, -1, 2);
            ChangePercent(0.05f, -0.1f, -0.05f, 0.1f, -1, 3);
            ChangePercent(0.05f, -0.05f, -0.1f, 0.1f, -3, 4);
        }
        else if (num == 3)
        {
            ChangePercent(0.1f, 0.05f, 0.05f, -0.2f, 2, 0);
            ChangePercent(0.05f, 0.15f, -0.1f, -0.1f, 1, 1);
            ChangePercent(0.05f, -0.05f, 0.1f, -0.1f, 1, 2);
            ChangePercent(0.1f, 0.15f, -0.05f, -0.2f, 3, 3);
            ChangePercent(0.1f, 0.2f, -0.1f, -0.1f, 2, 4);
        }
        for(int i = 0; i < 5; i++)
        {
            for(int j = 0; j < 4; j++)
            {
                ShowScoreLevel[i, j] = false;
            }
        }
        CalcUser();
        UpdateUI();
        UpdateScoreLevel();
    }

    //改变用户喜好
    void ChangePercent(float a, float b, float c, float d, int profit, int type)
    {
        float[] num = new float[4] { a, b, c, d };
        bool canChange = true;
        for(int i = 0; i < 4; i++)
        {
            if (num[i] < 0 && ScorePercent[type, i] + num[i] < 0)
                canChange = false;
        }
        if (canChange == true)
        {
            for (int i = 0; i < 4; i++)
            {
                ScorePercent[type, i] += num[i];
            }
        }
        Profit[type] += profit;
        if (Profit[type] < 0)
            Profit[type] = 0;
    }

    //给用户喜好程度排序
    void SetScoreLevel()
    {
        //先设置顺序（无视等值）
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                int num = 1;
                for (int k = 0; k < 4; k++)
                {
                    if (j == k)
                    {
                        if (k == 3)
                            ScoreLevel[i, j] = num;
                        continue;
                    }
                    else
                    {
                        if (ScorePercent[i, j] < ScorePercent[i, k])
                            num += 1;
                        if (k == 3)
                            ScoreLevel[i, j] = num;
                    }
                }

            }
        }
        //再循环一遍排除等值问题
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                for (int k = j + 1; k < 4; k++)
                {
                    if (ScoreLevel[i, k] == ScoreLevel[i, j])
                        ScoreLevel[i, k] += 1;
                }
            }
        }
        //最后循环一次确定文案
        //for (int i = 0; i < 5; i++)
        //{
        //    for (int j = 0; j < 4; j++)
        //    {
        //        int a = ScoreLevel[i, j];
        //        if (a == 1)
        //            LevelText[i, j] = "最关注";
        //        else if (a == 2)
        //            LevelText[i, j] = "关注";
        //        else if (a == 3)
        //            LevelText[i, j] = "不关注";
        //        else if (a == 4)
        //            LevelText[i, j] = "最不关注";
        //    }
        //}
    }

    //通过用户访谈获取用户喜好信息
    public void CheckUserStatus(int num)
    {
        float a, b, c, d;
        if(PC.GC.FinishedTask[8] >= num)
        {
            PC.GC.FinishedTask[8] -= num;
            PC.GC.UpdateResourceInfo();
            if (num == 1)
            {
                a = 0.4f;
                b = 0.3f;
                c = 0.2f;
                d = 0.1f;
            }
            else if (num == 3)
            {
                a = 0.3f;
                b = 0.3f;
                c = 0.3f;
                d = 0.1f;
            }
            else if (num == 5)
            {
                a = 0.2f;
                b = 0.2f;
                c = 0.5f;
                d = 0.1f;
            }
            else if (num == 7)
            {
                a = 0.1f;
                b = 0.2f;
                c = 0.6f;
                d = 0.1f;
            }
            else
            {
                a = 0.1f;
                b = 0.1f;
                c = 0.6f;
                d = 0.2f;
            }

            for (int t = 0; t < 10; t++)
            {
                float Posb = Random.Range(0.0f, 1.0f);
                int type = Random.Range(0, 5);
                if (Posb <= a)
                {

                }
                else if (Posb <= a + b)
                {
                    int e = Random.Range(3, 5);
                    for (int i = 0; i < 4; i++)
                    {
                        if (ScoreLevel[type, i] == e)
                        {
                            LevelText[type, i] = "不关注";
                            ShowScoreLevel[type, i] = true;
                        }
                    }
                }
                else if (Posb <= a + b + c)
                {
                    int e = Random.Range(1, 3);
                    for (int i = 0; i < 4; i++)
                    {
                        if (ScoreLevel[type, i] == e)
                        {
                            LevelText[type, i] = "可能关注";
                            ShowScoreLevel[type, i] = true;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (ScoreLevel[type, i] == 1)
                        {
                            LevelText[type, i] = "最关注";
                            ShowScoreLevel[type, i] = true;
                        }
                    }
                }
            }
            UpdateScoreLevel();
            CheckPanel.SetActive(false);
        }      
    }
}
