using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompanyPerkControl : MonoBehaviour
{
    public PerkInfo PerkInfoPrefab_Company;
    public GameControl GC;
    public Transform PerkContent, DebuffPerkContent;

    public List<PerkInfo> CurrentPerksInfo = new List<PerkInfo>();
    public Dictionary<int, List<Employee>> CurrentDebuffPerks = new Dictionary<int, List<Employee>>();

    //添加Perk
    public void AddPerk(Perk perk)
    {
        //同类Perk检测
        foreach (PerkInfo p in CurrentPerksInfo)
        {
            if (p.CurrentPerk.Num == perk.Num)
            {
                //可叠加的进行累加
                if (perk.canStack == true)
                {
                    p.CurrentPerk.Level += 1;
                    p.CurrentPerk.AddEffect();
                    return;
                }
                //不可叠加的清除
                else
                {
                    p.CurrentPerk.RemoveEffect();
                    break;
                }
            }
        }
        PerkInfo newPerk = Instantiate(PerkInfoPrefab_Company, PerkContent);
        newPerk.CurrentPerk = perk;
        newPerk.CurrentPerk.BaseTime = perk.TimeLeft;
        newPerk.CurrentPerk.Info = newPerk;
        newPerk.Text_Name.text = perk.Name;
        newPerk.info = GC.infoPanel;
        CurrentPerksInfo.Add(newPerk);
        newPerk.CurrentPerk.AddEffect();
        newPerk.SetColor();
    }

    //移除Perk
    public void RemovePerk(int num)
    {
        foreach (PerkInfo info in CurrentPerksInfo)
        {
            if (info.CurrentPerk.Num == num)
            {
                info.CurrentPerk.RemoveEffect();
                break;
            }
        }
    }

    //添加负面Perk（因为用了Clone所以需要重新对Perk本身进行各项赋值）
    public void AddDebuffPerk(Perk perk)
    {
        if (CurrentDebuffPerks.ContainsKey(perk.Num) == false)
        {
            perk.Info = Instantiate(PerkInfoPrefab_Company, DebuffPerkContent);
            perk.Info.CurrentPerk = perk;
            perk.Info.Text_Name.text = perk.Name;
            perk.Info.info = GC.infoPanel;
            CurrentPerksInfo.Add(perk.Info);
            CurrentDebuffPerks.Add(perk.Num, new List<Employee>() { perk.TargetEmp });
        }
        else
            CurrentDebuffPerks[perk.Num].Add(perk.TargetEmp);
        
    }

    //移除负面特质
    public void RemoveDebuffPerk(int num, Employee emp)
    {
        CurrentDebuffPerks[num].Remove(emp);
        if (CurrentDebuffPerks[num].Count == 0)
        {
            foreach (PerkInfo p in CurrentPerksInfo)
            {
                if (p.CurrentPerk.Num == num)
                {
                    CurrentPerksInfo.Remove(p);
                    Destroy(p.gameObject);
                    break;
                }
            }
            CurrentDebuffPerks.Remove(num);
        }
    }

    //负面buff检测，其中134的检测放在航线按钮上了
    public void DebuffEffect(int num)
    {
        foreach (PerkInfo p in CurrentPerksInfo)
        {
            if (p.CurrentPerk.Num == num)
            {
                if (CurrentDebuffPerks.ContainsKey(num) == false || CurrentDebuffPerks[num].Count == 0)
                {
                    Debug.LogError("辞典有问题");
                    return;
                }
                p.CurrentPerk.ActiveCompanyDebuffEffect(CurrentDebuffPerks[num]);
                return;
            }
        }
    }
}
