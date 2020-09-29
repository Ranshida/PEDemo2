using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FOECompany : MonoBehaviour
{
    public bool Closed = false, isPlayer = false;

    public FOEControl FC;
    public FOECompany TargetCompany;
    public Text Text_CompanyName, Text_Status, Text_Target, Text_SkillName;
    public GameObject ClosedText;

    public int Morale = 100, Money = 0, Income = 28;
    public int NextSkill = 0;//1攻击1 2攻击2 3干扰1 4回复1

    private void Start()
    {
        if (isPlayer == false)
        {
            FindTarget();
            RandomSkill();
            FC.GC.MonthlyEvent.AddListener(TimePass);
            FC.GC.YearEvent.AddListener(IncreaseIncome);
        }
    }
    private void Update()
    {
        Text_Status.text = "金钱:" + Money + " + " + Income + "/月\n" + "士气:" + Morale;
        if (TargetCompany != null)
            Text_Target.text = "目标:" + TargetCompany.Text_CompanyName.text;
        if (NextSkill == 1)
            Text_SkillName.text = "下一技能:攻击1(降低目标士气5,140金钱)";
        else if (NextSkill == 2)
            Text_SkillName.text = "下一技能:攻击2(降低目标士气15,350金钱)";
        else if (NextSkill == 3)
            Text_SkillName.text = "下一技能:干扰1(发展内鬼,140金钱)";
        else if (NextSkill == 4)
            Text_SkillName.text = "下一技能:回复1(回复自身士气10,70金钱)";

    }

    public void TimePass()
    {
        Money += Income;
        if(TargetCompany != null)
        {
            if(NextSkill == 1 && Money >= 140)
            {
                Money -= 140;
                if (TargetCompany.isPlayer == true)
                    FC.GC.Morale -= 5;
                else
                    TargetCompany.Morale -= 5;
                TargetCompany.CloseCheck();
                FindTarget();
                RandomSkill();
                Text_Target.gameObject.SetActive(false);
                Text_SkillName.gameObject.SetActive(false);
            }
            else if (NextSkill == 2 && Money >= 350)
            {
                Money -= 350;
                if (TargetCompany.isPlayer == true)
                    FC.GC.Morale -= 15;
                else
                    TargetCompany.Morale -= 15;
                TargetCompany.CloseCheck();
                FindTarget();
                RandomSkill();
                Text_Target.gameObject.SetActive(false);
                Text_SkillName.gameObject.SetActive(false);
            }
            else if (NextSkill == 3 && Money >= 140)
            {
                Money -= 140;
                if(FC.GC.CurrentEmployees.Count > 1)
                {
                    Employee E = FC.GC.CurrentEmployees[Random.Range(1, FC.GC.CurrentEmployees.Count)];
                    if(E.isSpy == false)
                    {
                        E.isSpy = true;
                        E.BaseMotivation[2] += 50;
                    }
                }
                FindTarget();
                RandomSkill();
                Text_Target.gameObject.SetActive(false);
                Text_SkillName.gameObject.SetActive(false);
            }
            else if (NextSkill == 4 && Money >= 70)
            {
                Money -= 70;
                Morale += 10;
                if (Morale >= 100)
                    Morale = 100;
                FindTarget();
                RandomSkill();
                Text_Target.gameObject.SetActive(false);
                Text_SkillName.gameObject.SetActive(false);
            }
        }
    }
    public void IncreaseIncome()
    {
        Income += 2;
    }
    public void CloseCheck()
    {
        if (Morale <= 0 && isPlayer == false)
        {
            ClosedText.SetActive(true);
            Closed = true;
            FC.ActiveCompanyCount -= 1;
            FC.GC.HourEvent.RemoveListener(TimePass);
            FC.GC.YearEvent.RemoveListener(IncreaseIncome);
            FC.TargetSelectButtons[FC.CurrentCompanies.IndexOf(this)].gameObject.SetActive(false);
        }
    }
    void FindTarget()
    {
        if (FC.ActiveCompanyCount == 1)
            TargetCompany = FC.CurrentCompanies[3];
        else
        {
            List<FOECompany> N = new List<FOECompany>();
            for(int i = 0; i < FC.CurrentCompanies.Count; i++)
            {
                if (FC.CurrentCompanies[i] == this || FC.CurrentCompanies[i].Closed == true)
                    continue;
                else
                    N.Add(FC.CurrentCompanies[i]);
            }
            if (N.Count > 0)
                TargetCompany = N[Random.Range(0, N.Count)];
        }
    }
    void RandomSkill()
    {
        int MoraleValue = 100 - Morale;
        int Posb;
        if(TargetCompany.isPlayer == true)
        {
            Posb = Random.Range(1, 41 + MoraleValue);
            if (Posb < 25)
                NextSkill = 1;
            else if (Posb < 35)
                NextSkill = 2;
            else if (Posb < 40)
                NextSkill = 3;
            else
                NextSkill = 4;
        }
        else
        {
            Posb = Random.Range(1, 36 + MoraleValue);
            if (Posb < 25)
                NextSkill = 1;
            else if (Posb < 35)
                NextSkill = 2;
            else
                NextSkill = 4;
        }

            
    }
}
