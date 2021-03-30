using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FOEControl : MonoBehaviour
{
    public int CurrentTurn;//当前回合数
    public int NeutralMarket = 80;

    public List<FOECompany> Companies = new List<FOECompany>();
}
