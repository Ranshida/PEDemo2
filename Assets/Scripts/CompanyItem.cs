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
    public CompanyItemType Type;

    public int Num, ActiveType = 1;//1选择水晶 2选择区域 3选择员工 4选择部门

    public GameControl GC;
    public Text Text_Name;
    public Button button;
    public InfoPanelTrigger info;

    public Crystal TargetCrystal;
    public DepControl TargetDep;
    public Area TargetArea;
    public EmpInfo TargetEmp;

    public void SetType(int num)
    {
        Num = num;
        if (Num == 1)
        {
            Text_Name.text = "特殊决议";
            info.ContentB = "月会时可以使用，选择一个领导力水晶将其取消，并随机获得一个新的水晶";
            ActiveType = 1;
            Type = CompanyItemType.MonthMeeting;
        }
        else if (Num == 2)
        {
            Text_Name.text = "强迫妥协";
            info.ContentB = "在月会时可以使用，选择一个领导力水晶，并将所有水晶都变成这枚水晶";
            ActiveType = 1;
            Type = CompanyItemType.MonthMeeting;
        }
        else if (Num == 3)
        {
            Text_Name.text = "管理咨询报告";
            info.ContentB = "在月会时可以使用，使用时选择一个建筑空间，增加其领导力槽位1个";
            ActiveType = 2;
            Type = CompanyItemType.MonthMeeting;
        }
        else if (Num == 4)
        {
            Text_Name.text = "脑机芯片";
            info.ContentB = "为1名员工施加状态“脑机芯片”1层，该名员工随机一项职业技能增加3点，维持2个月";
            ActiveType = 3;
        }
        else if (Num == 5)
        {
            Text_Name.text = "混沌创意方案";
            info.ContentB = "使用后为部门添加一个状态“混乱与创造”，部门工作状态-3，大成功率增加30%";
            ActiveType = 4;
        }
        else if (Num == 6)
        {
            Text_Name.text = "机器人员工";
            info.ContentB = "使用后为部门添加一个“机器人员工”状态，提升机器人所在部门效率25%";
            ActiveType = 4;
        }
        else if (Num == 7)
        {
            Text_Name.text = "《胜利开发者的全新计划》";
            info.ContentB = "点击使用后选择特定部门，施加“胜利开发”的部门状态，持续到下一次头脑风暴，并提升其工作状态1点，可叠加";
            ActiveType = 4;
        }
        else if (Num == 8)
        {
            Text_Name.text = "《预算新编》";
            info.ContentB = "点击使用后选择特定部门，施加“节省支出”的部门状态，持续到下一次头脑风暴，并降低人员工资10%，可叠加";
            ActiveType = 4;
        }
        else if (Num == 9)
        {
            Text_Name.text = "《致全体同事的一封信》";
            info.ContentB = "点击使用后选择特定部门，施加“信仰充值”的部门状态，持续到下一次头脑风暴，并提升信念5点，可叠加";
            ActiveType = 4;
        }
        else if (Num == 10)
        {
            Text_Name.text = "《加班技术大全》";
            info.ContentB = "点击使用后选择特定部门，施加“效率至上”的部门状态，持续到下一次头脑风暴，并提升效率5%，可叠加";
            ActiveType = 4;
        }

        info.ContentA = Text_Name.text;
        //月会物品创建时设定为无法激活，如果创建时正在开月会则改为可激活
        if (Type == CompanyItemType.MonthMeeting)
        {
            button.interactable = false;
            if (MonthMeeting.Instance.MeetingStart == true)
                button.interactable = true;
        }
    }

    public void SelectItem()
    {
        GC.CurrentItem = this;
        if (ActiveType == 2)
        {
            GC.AC.ShowAvailableAS();
            GC.AreaSelectMode = 2;
        }
        else if (ActiveType == 3)
        {
            GC.SelectMode = 11;
            GC.TotalEmpPanel.SetWndState(true);
            GC.Text_EmpSelectTip.gameObject.SetActive(true);
            GC.Text_EmpSelectTip.text = "选择一个员工";
        }
        else if (ActiveType == 4)
        {
            GC.SelectMode = 11;
            GC.ShowDepSelectPanel(GC.CurrentDeps);
        }
    }

    public void UseItem()
    {
        //特殊决议
        if(Num == 1)
        {
            MeetingWindow m = MonthMeeting.Instance.MeetingWindow;
            MonthMeeting.Instance.CrystalDict[TargetCrystal.type] -= 1;
            m.crystalList.Remove(TargetCrystal.gameObject);
            Destroy(TargetCrystal.gameObject);

            CrystalType cType =(CrystalType)Random.Range(1, 6);
            GameObject go = Instantiate(m.crystalPrefab, m.CrystalResult);
            m.crystalList.Add(go);
            go.GetComponent<Image>().color = MonthMeeting.GetCrystalColor(cType);
            go.GetComponent<Crystal>().type = cType;
            MonthMeeting.Instance.CrystalDict[cType] += 1;
        }
        //强迫妥协
        else if (Num == 2)
        {
            MeetingWindow m = MonthMeeting.Instance.MeetingWindow;

            CrystalType cType = TargetCrystal.type;
            foreach(GameObject o in m.crystalList)
            {
                Crystal c = o.GetComponent<Crystal>();
                if(c.type != cType)
                {
                    MonthMeeting.Instance.CrystalDict[c.type] -= 1;
                    MonthMeeting.Instance.CrystalDict[cType] += 1;
                    o.GetComponent<Image>().color = MonthMeeting.GetCrystalColor(cType);
                }
            }
        }
        //管理咨询报告
        else if (Num == 3)
            TargetArea.CA.AddOption();
        //脑机芯片
        else if (Num == 4)
            TargetEmp.AddPerk(new Perk149(TargetEmp.emp));
        //混沌创意方案
        else if (Num == 5)
            TargetDep.AddPerk(new Perk143(null));
        //机器人员工
        else if (Num == 6)
            TargetDep.AddPerk(new Perk145(null));
        //《胜利开发者的全新计划》
        else if (Num == 7)
            TargetDep.AddPerk(new Perk150(null));
        //《预算新编》
        else if (Num == 8)
            TargetDep.AddPerk(new Perk151(null));
        //《致全体同事的一封信》
        else if (Num == 9)
            TargetDep.AddPerk(new Perk152(null));
        //《加班技术大全》
        else if (Num == 10)
            TargetDep.AddPerk(new Perk153(null));
        GC.CurrentItem = null;
        DeleteItem();
    }

    public void DeleteItem()
    {
        GC.Items.Remove(this);
        Destroy(this.gameObject);
    }
}
