using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CompanyItemType
{
    Default, MonthMeeting, Fight 
}

public class CompanyItem : MonoBehaviour
{

    public GameControl GC;
    public Text Text_Name;
    public Button button;
    public InfoPanelTrigger info;
    public Item item;

    public void SetType(int num)
    {
        item = ItemData.Items[num - 1].Clone();
        Text_Name.text = item.Name;
        info.ContentB = item.Description;
        info.ContentA = Text_Name.text;
        //月会物品创建时设定为无法激活，如果创建时正在开月会则改为可激活
        if (item.Type == CompanyItemType.MonthMeeting)
        {
            button.interactable = false;
            if (MonthMeeting.Instance.MeetingStart == true)
                button.interactable = true;
        }
    }

    public void SelectItem()
    {
        //有未处理事件时不能继续
        if (GC.EC.UnfinishedEvents.Count > 0)
            return;
        GC.CurrentItem = this;
        if (item.ActiveType == 2)
        {
            GC.AC.ShowAvailableAS();
            GC.AreaSelectMode = 2;
        }
        else if (item.ActiveType == 3)
        {
            GC.SelectMode = 11;
            GC.TotalEmpPanel.SetWndState(true);
            GC.Text_EmpSelectTip.gameObject.SetActive(true);
            GC.Text_EmpSelectTip.text = "选择一个员工";
        }
        else if (item.ActiveType == 4)
        {
            GC.SelectMode = 11;
            GC.ShowDepSelectPanel(GC.CurrentDeps);
        }
        else if (item.ActiveType == 5)
        {
            GC.SelectMode = 11;
            GC.ShowDivSelectPanel();
        }
    }

    public void UseItem()
    {
        item.StartEffect();
        GC.CurrentItem = null;
        DeleteItem();
    }

    public void DeleteItem()
    {
        GC.Items.Remove(this);
        Destroy(this.gameObject);
    }
}

public class Item
{
    public CompanyItemType Type = CompanyItemType.Default;

    public string Name;
    public string Description;
    public int ActiveType, Num;//1选择水晶 2选择区域 3选择员工 4选择部门 5选择事业部

    public Crystal TargetCrystal;
    public DepControl TargetDep;
    public DivisionControl TargetDiv;
    public Area TargetArea;
    public EmpInfo TargetEmp;

    public virtual void StartEffect()
    {

    }

    public Item Clone()
    {
        return (Item)this.MemberwiseClone();
    }
}

public class Item1 : Item
{
    public Item1() : base()
    {
        Name = "福报自修手册";
        Description = "施加“996”状态，持续1回合，事业部效率+1";
        ActiveType = 5;
    }

    public override void StartEffect()
    {
        TargetDiv.AddPerk(new Perk153());
    }

}

public class Item2 : Item
{
    public Item2() : base()
    {
        Name = "管理咨询报告";
        Description = "月会时可以使用，选择一个领导力水晶，并获得额外一个与之相同的水晶";
        ActiveType = 1;
        Type = CompanyItemType.MonthMeeting;
    }

    public override void StartEffect()
    {
        MeetingWindow m = MonthMeeting.Instance.MeetingWindow;
        CrystalType cType = TargetCrystal.type;
        GameObject go = MonoBehaviour.Instantiate(m.crystalPrefab, m.CrystalResult);
        m.crystalList.Add(go);
        go.GetComponent<Image>().color = MonthMeeting.GetCrystalColor(cType);
        go.GetComponent<Crystal>().type = cType;
        MonthMeeting.Instance.CrystalDict[cType] += 1;
    }

}

public class Item3 : Item
{
    public Item3() : base()
    {
        Name = "《加班技术大全》";
        Description = "选择特定事业部，施加“效率至上”的状态，持续3回合，提升效率1点，可叠加";
        ActiveType = 5;
    }

    public override void StartEffect()
    {
        TargetDiv.AddPerk(new Perk153());
    }

}

public class Item4 : Item
{
    public Item4() : base()
    {
        Name = "《致全体同事的一封信》";
        Description = "选择特定事业部，施加“信仰充值”的状态，持续3回合，并提升信念10点，可叠加";
        ActiveType = 5;
    }

    public override void StartEffect()
    {
        TargetDiv.AddPerk(new Perk152());
    }

}

public class Item5 : Item
{
    public Item5() : base()
    {
        Name = "《预算新编》";
        Description = "选择特定事业部，施加“节省支出”的状态，持续3回合，降低3成本，可叠加";
        ActiveType = 5;
    }

    public override void StartEffect()
    {
        TargetDiv.AddPerk(new Perk151());
    }

}

public class Item6 : Item
{
    public Item6() : base()
    {
        Name = "《胜利开发者的全新计划》";
        Description = "选择特定事业部，施加“胜利开发”的状态，持续3回合，并提升其工作状态1点，可叠加";
        ActiveType = 5;
    }

    public override void StartEffect()
    {
        TargetDiv.AddPerk(new Perk150());
    }

}


//以下为留档用
public class Item117 : Item
{
    public Item117() : base()
    {
        Name = "管理咨询报告2";
        Description = "在月会时可以使用，使用时选择一个建筑空间，增加其领导力槽位1个";
        ActiveType = 2;
        Type = CompanyItemType.MonthMeeting;
    }

    public override void StartEffect()
    {
        TargetArea.CA.AddOption();
    }

}

public class Item118 : Item
{
    public Item118() : base()
    {
        Name = "特殊决议";
        Description = "选择并删除一个水晶，再随机添加一个水晶";
        ActiveType = 5;
        Type = CompanyItemType.MonthMeeting;
    }

    public override void StartEffect()
    {
        MeetingWindow m = MonthMeeting.Instance.MeetingWindow;
        MonthMeeting.Instance.CrystalDict[TargetCrystal.type] -= 1;
        m.crystalList.Remove(TargetCrystal.gameObject);
        Object.Destroy(TargetCrystal.gameObject);

        CrystalType cType = (CrystalType)Random.Range(1, 6);
        GameObject go = MonoBehaviour.Instantiate(m.crystalPrefab, m.CrystalResult);
        m.crystalList.Add(go);
        go.GetComponent<Image>().color = MonthMeeting.GetCrystalColor(cType);
        go.GetComponent<Crystal>().type = cType;
        MonthMeeting.Instance.CrystalDict[cType] += 1;
    }

}

static public class ItemData
{
    static public List<Item> Items = new List<Item>()
    {
        new Item1(), new Item2(), new Item3(), new Item4(), new Item5(), new Item6()
    };
}
