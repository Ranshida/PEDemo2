using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IterationControl : MonoBehaviour
{
    public Text[] Text_Counts = new Text[16];
    public Text[] Text_Value = new Text[5];
    public Transform ButtonTab1, ButtonTab2;
    public GameControl GC;

    int Art, Function, Fluence, Secure, Profit;
    int IterationNum = 0, TotalSelectNum = 0;

    int[,] Value = new int[4, 4]; //类型,品质

    public void RefreshPanel()
    {
        for(int i = 0; i < Text_Counts.Length; i++)
        {
            Text_Counts[i].text = "0";
        }
        for(int i = 0; i < Text_Value.Length; i++)
        {
            Text_Value[i].text = "0";
        }
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Value[i, j] = 0;
            }
        }
        IterationNum = 0;
        TotalSelectNum = 0;
        ButtonCheck();
    }

    public void AddBad(int type)
    {
        int n = NumSet(type);
        if (GC.ResCount[n, 0] > Value[type - 1, 0])
        {
            Value[type - 1, 0] += 1;
            NumCheck(false, type);
        }
        UpdateInfo();
    }
    public void ReduceBad(int type)
    {
        int n = NumSet(type);
        if (Value[type - 1, 0] > 0)
        {
            Value[type - 1, 0] -= 1;
            NumCheck(true, type);
        }
        UpdateInfo();
    }
    public void AddNormal(int type)
    {
        int n = NumSet(type);
        if (GC.ResCount[n, 1] > Value[type - 1, 1])
        {
            Value[type - 1, 1] += 1;
            NumCheck(false, type);
        }
        UpdateInfo();
    }
    public void ReduceNormal(int type)
    {
        int n = NumSet(type);
        if (Value[type - 1, 1] > 0)
        {
            Value[type - 1, 1] -= 1;
            NumCheck(true, type);
        }
        UpdateInfo();
    }
    public void AddGood(int type)
    {
        int n = NumSet(type);
        if (GC.ResCount[n, 2] > Value[type - 1, 2])
        {
            Value[type - 1, 2] += 1;
            NumCheck(false, type);
        }
        UpdateInfo();
    }
    public void ReduceGood(int type)
    {
        int n = NumSet(type);
        if (Value[type - 1, 2] > 0)
        {
            Value[type - 1, 2] -= 1;
            NumCheck(true, type);
        }
        UpdateInfo();
    }
    public void AddPerfect(int type)
    {
        int n = NumSet(type);
        if (GC.ResCount[n, 3] > Value[type - 1, 3])
        {
            Value[type - 1, 3] += 1;
            NumCheck(false, type);
        }
        UpdateInfo();
    }
    public void ReducePerfect(int type)
    {
        int n = NumSet(type);
        if (Value[type - 1, 3] > 0)
        {
            Value[type - 1, 3] -= 1;
            NumCheck(true, type);
        }
        UpdateInfo();
    }

    void UpdateInfo()
    {
        for(int i = 0; i < Text_Counts.Length; i++)
        {
            Text_Counts[i].text = Value[i % 4, i / 4].ToString();
        }
        int a = 0, b = 0, c = 0, d = 0, e = 0;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                int m;
                if (j == 0)
                    m = 10;
                else if (j == 1)
                    m = 20;
                else if (j == 2)
                    m = 40;
                else
                    m = 100;

                if (i == 0)
                {
                    c += Value[i, j] * m;
                    d += Value[i, j] * m;
                }
                else if(i == 1)
                {
                    a += Value[i, j] * m;
                    b += Value[i, j] * m;
                }
                else if (i == 2)
                {
                    a += Value[i, j] * m;
                    e += Value[i, j] * m;
                }
                else if (i == 3)
                {
                    b += Value[i, j] * m;
                    c += Value[i, j] * m;
                    e += Value[i, j] * m;
                }
            }
        }
        Art = a; Function = b; Fluence = c; Secure = d; Profit = e;
        Text_Value[0].text = Art.ToString();
        Text_Value[1].text = Function.ToString();
        Text_Value[2].text = Fluence.ToString();
        Text_Value[3].text = Secure.ToString();
        Text_Value[4].text = Profit.ToString();
    }

    public void ButtonCheck()
    {
        if(TotalSelectNum >= 4)
        {
            foreach(Transform child in ButtonTab1)
            {
                child.gameObject.GetComponent<Button>().interactable = false;
            }
            foreach (Transform child in ButtonTab2)
            {
                child.gameObject.GetComponent<Button>().interactable = false;
            }
        }
        else if(IterationNum < 1)
        {
            foreach (Transform child in ButtonTab1)
            {
                child.gameObject.GetComponent<Button>().interactable = true;
            }
            foreach (Transform child in ButtonTab2)
            {
                child.gameObject.GetComponent<Button>().interactable = false;
            }
        }
        else
        {
            foreach (Transform child in ButtonTab1)
            {
                child.gameObject.GetComponent<Button>().interactable = true;
            }
            foreach (Transform child in ButtonTab2)
            {
                child.gameObject.GetComponent<Button>().interactable = true;
            }
        }
    }

    int NumSet(int type)
    {
        if (type == 1)
            return 0;
        else if (type == 2)
            return 6;
        else if (type == 3)
            return 4;
        else
            return 8;
    }

    void NumCheck(bool Reduce , int type)
    {
        if (Reduce == false)
        {
            TotalSelectNum += 1;
            if (type == 1)
                IterationNum += 1;
        }
        else
        {
            TotalSelectNum -= 1;
            if (type == 1)
                IterationNum -= 1;
        }
        ButtonCheck();
    }
}
