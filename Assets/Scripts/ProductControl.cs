using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProductControl : MonoBehaviour
{
    public int UserType = 1, ProfitType = 1;

    public int[,] ScoreExtra = new int[5, 4];
    public int[] Profit;

    public Product ProductPrefab;
    public Transform ProductContent;
    public GameControl GC;
    public IterationControl IC;

    public List<Block> Blocks = new List<Block>();
    public List<Product> CurrentProduct = new List<Product>();

    public Text[] Text1 = new Text[6];
    public Text[] Text2 = new Text[6];
    public Text[] Text3 = new Text[6];
    public Text[] Text4 = new Text[6];
    public Text[] Text5 = new Text[6];

    private void Start()
    {
        SetValue();
        for(int i = 0; i < 4; i++)
        {
            Blocks.Add(new Block(i + 1));
        }
    }

    public void CreateProduct()
    {
        Product newP = Instantiate(ProductPrefab, ProductContent);
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                newP.ScoreExtra[i, j] = ScoreExtra[i, j];
            }
            newP.Profit[i] = Profit[i];
        }
        newP.TargetBlock = Blocks[UserType - 1];
        newP.PC = this;
        newP.CalcUser();
        newP.UpdateUI();
        CurrentProduct.Add(newP);
    }

    public void UpdateUI()
    {
        if(UserType == 1)
        {
            Text1[0].text = "30%";
            Text2[0].text = "30%";
            Text3[0].text = "15%";
            Text4[0].text = "5%";
            Text5[0].text = "20%";
            Text1[1].text = "x0";
            Text2[1].text = "x0";
            Text3[1].text = "x1";
            Text4[1].text = "x5";
            Text5[1].text = "x0";
        }
        else if(UserType == 2)
        {
            Text1[0].text = "30%";
            Text2[0].text = "10%";
            Text3[0].text = "20%";
            Text4[0].text = "10%";
            Text5[0].text = "30%";
            Text1[1].text = "x5";
            Text2[1].text = "x5";
            Text3[1].text = "x0";
            Text4[1].text = "x10";
            Text5[1].text = "x5";
        }
        else if (UserType == 3)
        {
            Text1[0].text = "10%";
            Text2[0].text = "10%";
            Text3[0].text = "30%";
            Text4[0].text = "10%";
            Text5[0].text = "40%";
            Text1[1].text = "x10";
            Text2[1].text = "x10";
            Text3[1].text = "x20";
            Text4[1].text = "x50";
            Text5[1].text = "x10";
        }
        else if (UserType == 4)
        {
            Text1[0].text = "30%";
            Text2[0].text = "30%";
            Text3[0].text = "15%";
            Text4[0].text = "5%";
            Text5[0].text = "20%";
            Text1[1].text = "x0";
            Text2[1].text = "x0";
            Text3[1].text = "x1";
            Text4[1].text = "x5";
            Text5[1].text = "x0";
        }

        for(int i = 0; i < 4; i++)
        {
            Text1[i + 2].text = ScoreExtra[0, i].ToString();
            Text2[i + 2].text = ScoreExtra[1, i].ToString();
            Text3[i + 2].text = ScoreExtra[2, i].ToString();
            Text4[i + 2].text = ScoreExtra[3, i].ToString();
            Text5[i + 2].text = ScoreExtra[4, i].ToString();
        }
    }

    public void ClearOld()
    {
        for(int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                ScoreExtra[i, j] = 0;
            }
        }
    }

    public void SetValue()
    {
        ClearOld();
        if (UserType == 1)
        {
            for (int i = 0; i < 5; i++)
            {
                ScoreExtra[i, 2] -= 500;
            }
            Profit = new int[5]{0,0,1,5,0};
        }
        else if (UserType == 2)
        {
            for (int i = 0; i < 5; i++)
            {
                ScoreExtra[i, 3] -= 500;
            }
            Profit = new int[5] { 5, 5, 0, 10, 5 };
        }
        else if (UserType == 3)
        {
            for (int i = 0; i < 5; i++)
            {
                ScoreExtra[i, 2] -= 300;
                ScoreExtra[i, 3] -= 300;
            }
            Profit = new int[5] { 10, 10, 20, 50, 10 };
        }
        else if (UserType == 4)
        {
            for (int i = 0; i < 5; i++)
            {
                ScoreExtra[i, 2] -= 200;
            }
            Profit = new int[5] { 0, 0, 1, 5, 0 };
        }

        if (ProfitType == 1)
        {
            ScoreExtra[2, 0] -= 150;
        }
        else if (ProfitType == 2)
        {
            if (UserType == 4)
            {
                ScoreExtra[1, 1] -= 120;
                ScoreExtra[4, 1] -= 120;
            }
            else
            {
                ScoreExtra[1, 1] -= 100;
                ScoreExtra[4, 1] -= 100;
            }
        }
        else if (ProfitType == 3)
        {
            if (UserType == 2)
            {
                ScoreExtra[0, 2] -= 120;
                ScoreExtra[1, 2] -= 120;
                ScoreExtra[4, 2] -= 120;
            }
            else
            {
                ScoreExtra[0, 2] -= 100;
                ScoreExtra[1, 2] -= 100;
                ScoreExtra[4, 2] -= 100;
            }
        }
        else if (ProfitType == 4)
        {
            if (UserType == 1 || UserType == 4)
            {
                ScoreExtra[0, 3] -= 120;
                ScoreExtra[1, 3] -= 120;
            }
            else
            {
                ScoreExtra[0, 3] -= 100;
                ScoreExtra[1, 3] -= 100;
            }
        }

        ScoreExtra[0, 0] = (int)(ScoreExtra[0, 0] * 0.05f);
        ScoreExtra[0, 1] = (int)(ScoreExtra[0, 1] * 0.1f);
        ScoreExtra[0, 2] = (int)(ScoreExtra[0, 2] * 0.6f);
        ScoreExtra[0, 3] = (int)(ScoreExtra[0, 3] * 0.25f);

        ScoreExtra[1, 0] = (int)(ScoreExtra[1, 0] * 0.1f);
        ScoreExtra[1, 1] = (int)(ScoreExtra[1, 1] * 0.4f);
        ScoreExtra[1, 2] = (int)(ScoreExtra[1, 2] * 0.35f);
        ScoreExtra[1, 3] = (int)(ScoreExtra[1, 3] * 0.15f);

        ScoreExtra[2, 0] = (int)(ScoreExtra[2, 0] * 0.4f);
        ScoreExtra[2, 1] = (int)(ScoreExtra[2, 1] * 0.35f);
        ScoreExtra[2, 2] = (int)(ScoreExtra[2, 2] * 0.15f);
        ScoreExtra[2, 3] = (int)(ScoreExtra[2, 3] * 0.1f);

        ScoreExtra[3, 0] = (int)(ScoreExtra[3, 0] * 0.5f);
        ScoreExtra[3, 1] = (int)(ScoreExtra[3, 1] * 0.15f);
        ScoreExtra[3, 2] = (int)(ScoreExtra[3, 2] * 0.05f);
        ScoreExtra[3, 3] = (int)(ScoreExtra[3, 3] * 0.3f);

        ScoreExtra[4, 0] = (int)(ScoreExtra[4, 0] * 0);
        ScoreExtra[4, 1] = (int)(ScoreExtra[4, 1] * 0.35f);
        ScoreExtra[4, 2] = (int)(ScoreExtra[4, 2] * 0.2f);
        ScoreExtra[4, 3] = (int)(ScoreExtra[4, 3] * 0.45f);



        UpdateUI();
    }

    public void SetUserType(int type)
    {
        UserType = type + 1;
        SetValue();
    }

    public void SetProfitType(int type)
    {
        ProfitType = type + 1;
        SetValue();
    }

    public void UserChange()
    {
        int income = 0;
        for(int i = 0; i < CurrentProduct.Count; i++)
        {
            CurrentProduct[i].AddUser();
            income += CurrentProduct[i].TotalProfit;
        }
        GC.Income = income;
    }
}
