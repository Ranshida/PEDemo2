using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DepSelect : MonoBehaviour
{
    public DepControl DC;
    public GameControl GC;
    public DivisionControl DivC;
    public Text Text_DepName;

    public void Select()
    {
        if (DC != null)
            GC.SelectDep(DC);
        else
            GC.SelectDivManager(DivC);
    }

    public void Select2()
    {
        GC.SelectDep();
    }
}
