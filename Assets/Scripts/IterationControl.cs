using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IterationControl : MonoBehaviour
{
    public Text[] Text_Counts = new Text[3];
    public Text[] Text_Value = new Text[4];
    public Transform ButtonTab1, ButtonTab2;
    public Product TargetProduct;
    public GameControl GC;
    public Button ConfirmButton;

    int Art, Function, Fluence, Secure;
    int IterationNum = 0, TotalSelectNum = 0;

    int[] Value = new int[3]; //类型,品质

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
        for (int i = 0; i < 3; i++)
        {
            Value[i] = 0;
        }
        IterationNum = 0;
        TotalSelectNum = 0;
        ButtonCheck();
    }

    public void AddBad(int type)
    {
        int n = NumSet(type);
        if (GC.FinishedTask[n] > Value[type - 1])
        {
            Value[type - 1] += 1;
            NumCheck(false, type);
        }
        UpdateInfo();
    }
    public void ReduceBad(int type)
    {
        int n = NumSet(type);
        if (Value[type - 1] > 0)
        {
            Value[type - 1] -= 1;
            NumCheck(true, type);
        }
        UpdateInfo();
    }
    //public void AddNormal(int type)
    //{
    //    int n = NumSet(type);
    //    if (GC.ResCount[n, 1] > Value[type - 1, 1])
    //    {
    //        Value[type - 1, 1] += 1;
    //        NumCheck(false, type);
    //    }
    //    UpdateInfo();
    //}
    //public void ReduceNormal(int type)
    //{
    //    int n = NumSet(type);
    //    if (Value[type - 1, 1] > 0)
    //    {
    //        Value[type - 1, 1] -= 1;
    //        NumCheck(true, type);
    //    }
    //    UpdateInfo();
    //}
    //public void AddGood(int type)
    //{
    //    int n = NumSet(type);
    //    if (GC.ResCount[n, 2] > Value[type - 1, 2])
    //    {
    //        Value[type - 1, 2] += 1;
    //        NumCheck(false, type);
    //    }
    //    UpdateInfo();
    //}
    //public void ReduceGood(int type)
    //{
    //    int n = NumSet(type);
    //    if (Value[type - 1, 2] > 0)
    //    {
    //        Value[type - 1, 2] -= 1;
    //        NumCheck(true, type);
    //    }
    //    UpdateInfo();
    //}
    //public void AddPerfect(int type)
    //{
    //    int n = NumSet(type);
    //    if (GC.ResCount[n, 3] > Value[type - 1, 3])
    //    {
    //        Value[type - 1, 3] += 1;
    //        NumCheck(false, type);
    //    }
    //    UpdateInfo();
    //}
    //public void ReducePerfect(int type)
    //{
    //    int n = NumSet(type);
    //    if (Value[type - 1, 3] > 0)
    //    {
    //        Value[type - 1, 3] -= 1;
    //        NumCheck(true, type);
    //    }
    //    UpdateInfo();
    //}

    void UpdateInfo()
    {
        for(int i = 0; i < Text_Counts.Length; i++)
        {
            Text_Counts[i].text = Value[i].ToString();
        }
        ValueCalc();
        Text_Value[0].text = Art.ToString();
        Text_Value[1].text = Function.ToString();
        Text_Value[2].text = Fluence.ToString();
        Text_Value[3].text = Secure.ToString();
    }

    public void ValueCalc()
    {
        int a = 0, b = 0, c = 0, d = 0;
        a += Value[1] * 2 + Value[2] * 4;
        b += Value[0] * 2 + Value[1] * 4;
        c += Value[0] * 2 + Value[1] * 2 + Value[2] * 2;
        d += Value[0] * 4 + Value[2] * 2;
        Art = a; Function = b; Fluence = c; Secure = d;
    }

    public void ConfirmIteration()
    {
        TargetProduct.Score[0] += Art;
        TargetProduct.Score[1] += Function;
        TargetProduct.Score[2] += Fluence;
        TargetProduct.Score[3] += Secure;
        TargetProduct.CalcUser();
        TargetProduct.AddUser(1.0f);
        TargetProduct.UpdateUI();
        GC.FinishedTask[0] -= Value[0];
        GC.FinishedTask[6] -= Value[1];
        GC.FinishedTask[4] -= Value[0];
        GC.UpdateResourceInfo();
        GC.StrC.SolveStrRequest(2, Art + Function + Fluence + Secure);
    }

    public void ButtonCheck()
    {
        if(TotalSelectNum >= 10)
        {
            foreach(Transform child in ButtonTab1)
            {
                child.gameObject.GetComponent<Button>().interactable = false;
            }
            foreach (Transform child in ButtonTab2)
            {
                child.gameObject.GetComponent<Button>().interactable = false;
            }
            ConfirmButton.interactable = true;
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
            ConfirmButton.interactable = false;
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
            ConfirmButton.interactable = false;
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

        return 0;
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
