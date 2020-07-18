using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DepSelect : MonoBehaviour
{
    public DepControl DC;
    public OfficeControl OC;
    public GameControl GC;
    public Text Text_DepName;

    public void Select()
    {
        GC.SelectDep(DC);
    }

    public void Select2()
    {
        GC.SelectDep();
    }

    public void Select3()
    {
        GC.SelectDep(OC);
    }
}
