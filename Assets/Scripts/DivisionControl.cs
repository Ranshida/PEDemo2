using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DivisionControl : MonoBehaviour
{
    public int Faith = 0;//事业部信念
    public int Efficiency = 0;//事业部效率
    public int ExtraEfficiency = 0;//部门提供的额外效率
    public int ExtraFaith = 0;//部门提供的额外信仰加成
    public int ExtraWorkStatus = 0;//部门提供的额外工作状态加成
    public int ExtraManage = 0;//部门提供的额外高管管理能力加成
    public int WorkStatus = 0;//事业部工作状态
    public int ExtraCost = 0;//额外成本加成
    public int ExtraProduceTime = 0;//额外生产周期加成
    public int ExtraExp = 0;//每回合为管理下的部门的员工额外提供的经验值
    public int ManagerExtraExp = 0;//高管每回合为除自己以外的员工提供的额外经验值

    //特效相关
    public int MentalityBonus = 0;//每回合部门内员工心力变化量

    public string DivName;
    public bool canWork = true;
    public bool Locked = false;//事业部是否处于未解锁状态
    public bool StatusShowed = false;//是否显示信息面板
    private bool DetailPanelShowed = false;//是否显示详细信息面板

    public InfoPanelTrigger WorkStatusInfo, EfficiencyInfo, FaithInfo;
    public DepControl CWDep;//商战建筑
    public GameControl GC;
    public DepSelect DS;
    public GameObject DetailPanel;
    public Transform PerkContent, DepContent;
    public Area CurrentArea;
    public Text Text_DivName, Text_DivPanelName, Text_Faith, Text_Efficiency, Text_WorkStatus, Text_Manager, Text_Cost, Text_Status;
    public Employee Manager;
    public Image EfficiencyBarFill, WorkStatusBarFill, FaithFill;

    public List<PerkInfo> CurrentPerks = new List<PerkInfo>();
    public List<DepControl> CurrentDeps = new List<DepControl>();
    public List<RectTransform> FillMarkers = new List<RectTransform>();
    public List<CWCardInfo> CWCards = new List<CWCardInfo>();
    public List<CWCard> PreCards = new List<CWCard>() { null, null, null };//上回合的插槽状态

    private void Update()
    {
        if(CurrentArea != null)
            Text_DivName.transform.position = Function.World2ScreenPoint(CurrentArea.topPosition);

        //各种信息更新
        if (Manager == null)
            Text_DivName.text = DivName + "(无高管)";
        else
            Text_DivName.text = DivName;

        int CurrentEfficiency = Efficiency + ExtraEfficiency;
        int CurrentFaith = Faith + ExtraFaith;
        int CurrentWorkStatus = WorkStatus + ExtraWorkStatus;

        if (DetailPanelShowed == true)
        {
            Text_Faith.text = "部门信念:" + CurrentFaith;
            if (CurrentFaith <= -90)
                Text_Faith.text += "              <color=red>事件修正-4</color>";
            else if (CurrentFaith <= -30)
                Text_Faith.text += "              <color=red>事件修正-2</color>";
            else if (CurrentFaith <= -10)
                Text_Faith.text += "              <color=red>事件修正-1</color>";
            else if (CurrentFaith <= 10)
                Text_Faith.text += "              事件修正+0";
            else if (CurrentFaith <= 30)
                Text_Faith.text += "              事件修正+1";
            else if (CurrentFaith <= 90)
                Text_Faith.text += "              事件修正+2";
            else
                Text_Faith.text += "              事件修正+4";

            Text_Efficiency.text = "效率:" + (CurrentEfficiency);
            if (CurrentEfficiency < 0)
                Text_Efficiency.text += "                <color=red>无法正常生产</color>";

            Text_WorkStatus.text = "工作状态:" + CurrentWorkStatus;
            if (CurrentWorkStatus < 0)
                Text_WorkStatus.text += "                <color=red>无法正常生产</color>";

            if (Manager != null)
                Text_Manager.text = "当前高管:" + Manager.Name + "\n\n\n管理能力:" + Manager.Manage + "\n决策能力:" + Manager.Decision;
            else 
                Text_Manager.text = "当前高管:无";
            int empCost = CalcCost(1), depCost = CalcCost(2);
            Text_Cost.text = "成本:" + (empCost + depCost + ExtraCost) + "/回合\n\n员工工资:" + empCost + "/回合\n建筑维护费:" + depCost + "/回合";

            //效率和工作状态调填充

            if (CurrentEfficiency < 0)
            {
                EfficiencyBarFill.color = Color.red;
                if (CurrentEfficiency < -10)
                    EfficiencyBarFill.fillAmount = 0.08f;
                else
                    EfficiencyBarFill.fillAmount = (CurrentEfficiency + 10) / 20f;

            }
            else
            {
                EfficiencyBarFill.color = Color.green;
                if (CurrentEfficiency > 10)
                    EfficiencyBarFill.fillAmount = 0.92f;
                else
                    EfficiencyBarFill.fillAmount = (CurrentEfficiency + 10) / 20f;
            }
            FillMarkers[0].anchoredPosition = new Vector2(FillMarkers[0].parent.GetComponent<RectTransform>().sizeDelta.x * EfficiencyBarFill.fillAmount, 0);


            if (CurrentWorkStatus < 0)
            {
                WorkStatusBarFill.color = Color.red;
                if (CurrentWorkStatus < -10)
                    WorkStatusBarFill.fillAmount = 0.08f;
                else
                    WorkStatusBarFill.fillAmount = (CurrentWorkStatus + 10) / 20f;

            }
            else
            {
                WorkStatusBarFill.color = Color.green;
                if (CurrentWorkStatus > 10)
                    WorkStatusBarFill.fillAmount = 0.92f;
                else
                    WorkStatusBarFill.fillAmount = (CurrentWorkStatus + 10) / 20f;
            }
            FillMarkers[1].anchoredPosition = new Vector2(FillMarkers[1].parent.GetComponent<RectTransform>().sizeDelta.x * WorkStatusBarFill.fillAmount, 0);


            if (CurrentFaith <= -90)
            {
                FaithFill.fillAmount = 0.06f;
                FaithFill.color = new Color(1, 0, 0);
            }
            else if (CurrentFaith <= -30)
            {
                FaithFill.fillAmount = (float)(100 + CurrentFaith) / 200;
                FaithFill.color = new Color(1, 0.4f, 0.4f);
            }
            else if (CurrentFaith < 0)
            {
                FaithFill.fillAmount = (float)(100 + CurrentFaith) / 200;
                FaithFill.color = new Color(1, 0.8f, 0.8f);
            }
            else if (CurrentFaith <= 30)
            {
                FaithFill.fillAmount = (float)CurrentFaith / 200 + 0.5f;
                FaithFill.color = new Color(0.8f, 1, 0.8f);
            }
            else if (CurrentFaith <= 90)
            {
                FaithFill.fillAmount = (float)CurrentFaith / 200 + 0.5f;
                FaithFill.color = new Color(0.4f, 1, 0.4f);
            }
            else
            {
                FaithFill.fillAmount = 1;
                FaithFill.color = new Color(0, 1, 0);
            }
            FillMarkers[2].anchoredPosition = new Vector2(FillMarkers[2].parent.GetComponent<RectTransform>().sizeDelta.x * FaithFill.fillAmount, 0);

        }

        if (StatusShowed == true)
        {
            Text_Status.text = "工作状态:" + CurrentWorkStatus;
            if (CurrentWorkStatus < 0)
                Text_Status.text += "<color=red> 工作停工</color>";

            Text_Status.text += "\n效率:" + CurrentEfficiency;
            if (CurrentEfficiency < 0)
                Text_Status.text += "<color=red> 工作停工</color>";

            if (CurrentFaith <= -90)
                Text_Status.text += "\n<color=#FF0000>信念:" + CurrentFaith + "</color>";
            else if (CurrentFaith <= -30)
                Text_Status.text += "\n<color=#FF6666>信念:" + CurrentFaith + "</color>";
            else if (CurrentFaith < 0)
                Text_Status.text += "\n<color=#FFCCCC>信念:" + CurrentFaith + "</color>";
            else if (CurrentFaith == 0)
                Text_Status.text += "\n<color=#FFFFFF>信念:" + CurrentFaith + "</color>";
            else if (CurrentFaith <= 30)
                Text_Status.text += "\n<color=#CCFFCC>信念:" + CurrentFaith + "</color>";
            else if (CurrentFaith <= 90)
                Text_Status.text += "\n<color=#66FF66>信念:" + CurrentFaith + "</color>";
            else
                Text_Status.text += "\n<color=#00FF00>信念:" + CurrentFaith + "</color>";

            Text_Status.text += "\n成本:" + (CalcCost(1) + CalcCost(2)) + "/回合";
        }
    }

    public void Produce()
    {
        //开始前先检测属性
        ExtraStatusCheck();
        //检测插槽
        for (int i = 0; i < 3; i++)
        {
            if (CWCards[i].CurrentCard != PreCards[i])
            {
                AddPerk(new Perk126());                
            }
            PreCards[i] = CWCards[i].CurrentCard;
        }

        //关于每回合额外经验，现在分为两种，ExtraExp是给包括高管的员工提供的，ManagerExtraExp是给高管以外的员工提供的
        //能工作且有高管时属于正常工作，每回合会提供固定的5点经验，无法工作时没有这个5点经验加成
        //不管是给高管还是普通员工添加经验，在加上额外经验和固定经验（能工作时+5）后大于0才能添加

        //无法工作时也德额外经验加成和心力加成
        if (canWork == false || Manager == null)
        {
            if (ExtraExp + ManagerExtraExp > 0 || MentalityBonus != 0)
            {
                foreach (DepControl dep in CurrentDeps)
                {
                    foreach (Employee emp in dep.CurrentEmps)
                    {
                        emp.GainExp(ExtraExp + ManagerExtraExp);
                        if (MentalityBonus != 0)
                            emp.Mentality += MentalityBonus;
                    }
                }
            }
            if (ExtraExp > 0 && Manager != null)
                Manager.GainExp(ExtraExp);
            return;
        }

        //能工作时的经验和体力加成以及部门的工作效果，先算高管的再算整体的
        int exp = ExtraExp + 5;
        if (exp > 0)
            Manager.GainExp(exp);

        //先给高管计算并添加经验，再计算加上高管额外经验的总经验
        exp += ManagerExtraExp;
        //部门的生产效果
        foreach (DepControl dep in CurrentDeps)
        {
            dep.Produce();
            if(exp > 0)
            {
                foreach(Employee emp in dep.CurrentEmps)
                {
                    emp.GainExp(exp);
                    if (MentalityBonus != 0)
                        emp.Mentality += MentalityBonus;
                }
            }
        }

    }

    public void SetDetailPanel(bool showed)
    {
        DetailPanel.GetComponent<WindowBaseControl>().SetWndState(showed);
        DetailPanelShowed = showed;
    }

    public void SetManager(bool RemoveManager, Employee emp = null)
    {
        //撤掉高管
        if(RemoveManager == true)
        {
            if (Manager != null)
            {
                foreach (PerkInfo perk in Manager.InfoDetail.PerksInfo)
                {
                    if (perk.CurrentPerk.DivisionPerk == true)
                        perk.CurrentPerk.DeActiveSpecialEffect();
                }
                Manager.CurrentDivision = null;
            }
            Manager = null;
            Text_DivName.color = Color.red;
        }
        //任命高管
        else
        {
            Manager = emp;
            emp.CurrentDivision = this;
            Text_DivName.color = Color.black;
            foreach(PerkInfo perk in Manager.InfoDetail.PerksInfo)
            {
                if (perk.CurrentPerk.DivisionPerk == true)
                    perk.CurrentPerk.ActiveSpecialEffect();
            }
        }
    }

    //部门移动、销毁或者属性变化时检测额外效率以及检测商战卡牌的效果
    public void ExtraStatusCheck()
    {
        ExtraEfficiency = 0;
        ExtraFaith = 0;
        ExtraWorkStatus = 0;
        foreach(DepControl dep in CurrentDeps)
        {
            ExtraEfficiency += dep.ExtraEfficiency;
            ExtraFaith += dep.ExtraFaith;
            ExtraWorkStatus += dep.ExtraWorkStatus;
        }

        foreach (CWCardInfo info in CWCards)
        {
            if (info.CurrentCard != null)
            {
                ExtraWorkStatus += info.CurrentCard.WorkStatusDebuff[info.CurrentLevel - 1];
                ExtraEfficiency += info.CurrentCard.EfficiencyDebuff[info.CurrentLevel - 1] * info.CurrentNum;
            }
        }

        if (ExtraEfficiency + Efficiency < 0 || WorkStatus + ExtraWorkStatus < 0)
            canWork = false;
        else
            canWork = true;
    }

    public int CalcCost(int type)
    {
        int cost = 0;
        foreach(DepControl dep in CurrentDeps)
        {
            cost += dep.CalcCost(type);
        }
        return cost;
    }

    //添加Perk
    public void AddPerk(Perk perk)
    {
        //同类Perk检测
        foreach (PerkInfo p in CurrentPerks)
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
        PerkInfo newPerk = Instantiate(GC.PerkInfoPrefab, PerkContent);
        newPerk.CurrentPerk = perk;
        newPerk.CurrentPerk.BaseTime = perk.TimeLeft;
        newPerk.CurrentPerk.Info = newPerk;
        newPerk.Text_Name.text = perk.Name;
        newPerk.CurrentPerk.TargetDiv = this;
        newPerk.info = GC.infoPanel;
        CurrentPerks.Add(newPerk);
        newPerk.CurrentPerk.AddEffect();
        newPerk.SetColor();

        PerkSort();
    }

    //移除Perk
    public void RemovePerk(int num)
    {
        foreach (PerkInfo info in CurrentPerks)
        {
            if (info.CurrentPerk.Num == num)
            {
                info.CurrentPerk.RemoveEffect();
                break;
            }
        }
    }

    //状态排序
    void PerkSort()
    {
        if (CurrentPerks.Count == 0)
            return;
        List<PerkInfo> newPerkList = new List<PerkInfo>();
        for (int i = 0; i < 5; i++)
        {
            PerkColor c = PerkColor.None;
            if (i == 0)
                c = PerkColor.White;
            else if (i == 1)
                c = PerkColor.Orange;
            else if (i == 2)
                c = PerkColor.Grey;
            else if (i == 3)
                c = PerkColor.Blue;
            foreach (PerkInfo p in CurrentPerks)
            {
                if (p.CurrentPerk.perkColor == c)
                    newPerkList.Add(p);
            }
        }
        for (int i = 0; i < newPerkList.Count; i++)
        {
            newPerkList[i].transform.SetSiblingIndex(i);
        }
    }

    public void SetWorkStatusDetail()
    {
        string content = "";
        foreach (PerkInfo info in CurrentPerks)
        {
            if (info.CurrentPerk.perkColor == PerkColor.White)
            {
                if (info.CurrentPerk.TempValue1 >= 0)
                    content += "事业部状态" + info.CurrentPerk.Name + " +" + info.CurrentPerk.TempValue1 + "\n";
                else
                    content += "事业部状态" + info.CurrentPerk.Name + info.CurrentPerk.TempValue1 + "\n";
            }
        }
        foreach (DepControl dep in CurrentDeps)
        {
            foreach (Employee emp in dep.CurrentEmps)
            {
                foreach (PerkInfo perk in emp.InfoDetail.PerksInfo)
                {
                    if (perk.CurrentPerk.DepPerk == true || perk.CurrentPerk.DivisionPerk == true)
                    {
                        if (perk.CurrentPerk.TempValue1 > 0)
                            content += emp.Name + "特质 " + perk.CurrentPerk.Name + " +" + perk.CurrentPerk.TempValue1 + "\n";
                        else if (perk.CurrentPerk.TempValue1 < 0)
                            content += emp.Name + "特质 " + perk.CurrentPerk.Name + " " + perk.CurrentPerk.TempValue1 + "\n";
                    }                       
                }
            }
        }
        if (Manager != null)
        {
            foreach (PerkInfo perk in Manager.InfoDetail.PerksInfo)
            {
                if (perk.CurrentPerk.DepPerk == true || perk.CurrentPerk.DivisionPerk == true)
                {
                    if (perk.CurrentPerk.TempValue1 > 0)
                        content += Manager.Name + "特质 " + perk.CurrentPerk.Name + " +" + perk.CurrentPerk.TempValue1 + "\n";
                    else if (perk.CurrentPerk.TempValue1 < 0)
                        content += Manager.Name + "特质 " + perk.CurrentPerk.Name + " " + perk.CurrentPerk.TempValue1 + "\n";
                }
            }
        }
        foreach (DepControl dep in CurrentDeps)
        {
            if (dep.ExtraWorkStatus > 0)
                content += dep.Text_DepName.text + "额外效果 +" + dep.ExtraWorkStatus + "\n";
            else if (dep.ExtraWorkStatus < 0)
                content += dep.Text_DepName.text + "额外效果 " + dep.ExtraWorkStatus + "\n";
        }
        foreach (CWCardInfo card in CWCards)
        {
            if (card.CurrentCard != null)
                content += "卡牌" + card.CurrentCard.Name + " " + card.CurrentCard.WorkStatusDebuff[card.CurrentLevel - 1] + "\n";
        }
        WorkStatusInfo.ContentB = content;
    }

    public void SetEfficiencyDetail()
    {
        string content = "";
        foreach (PerkInfo info in CurrentPerks)
        {
            if (info.CurrentPerk.perkColor == PerkColor.Grey)
            {
                if (info.CurrentPerk.TempValue2 >= 0)
                    content += "事业部状态" + info.CurrentPerk.Name + " +" + info.CurrentPerk.TempValue2 + "\n";
                else
                    content += "事业部状态" + info.CurrentPerk.Name + info.CurrentPerk.TempValue2 + "\n";
            }
        }
        foreach (DepControl dep in CurrentDeps)
        {
            foreach (Employee emp in dep.CurrentEmps)
            {
                foreach (PerkInfo perk in emp.InfoDetail.PerksInfo)
                {
                    if (perk.CurrentPerk.DepPerk == true || perk.CurrentPerk.DivisionPerk == true)
                    {
                        if (perk.CurrentPerk.TempValue2 > 0)
                            content += emp.Name + "特质 " + perk.CurrentPerk.Name + " +" + perk.CurrentPerk.TempValue2 + "\n";
                        else if (perk.CurrentPerk.TempValue2 < 0)
                            content += emp.Name + "特质 " + perk.CurrentPerk.Name + " " + perk.CurrentPerk.TempValue2 + "\n";
                    }
                }
            }
        }
        if (Manager != null)
        {
            foreach (PerkInfo perk in Manager.InfoDetail.PerksInfo)
            {
                if (perk.CurrentPerk.DepPerk == true || perk.CurrentPerk.DivisionPerk == true)
                {
                    if (perk.CurrentPerk.TempValue2 > 0)
                        content += Manager.Name + "特质 " + perk.CurrentPerk.Name + " +" + perk.CurrentPerk.TempValue2 + "\n";
                    else if (perk.CurrentPerk.TempValue2 < 0)
                        content += Manager.Name + "特质 " + perk.CurrentPerk.Name + " " + perk.CurrentPerk.TempValue2 + "\n";
                }
            }
        }
        foreach (DepControl dep in CurrentDeps)
        {

            if (dep.ExtraEfficiency > 0)
                content += dep.Text_DepName.text + "额外效果 +" + dep.ExtraEfficiency + "\n";
            else if (dep.ExtraEfficiency < 0)
                content += dep.Text_DepName.text + "额外效果 " + dep.ExtraEfficiency + "\n";
        }
        foreach (CWCardInfo card in CWCards)
        {
            if (card.CurrentCard != null && card.CurrentNum != 0)
                content += "卡牌" + card.CurrentCard.Name + "效果 " + (card.CurrentCard.EfficiencyDebuff[card.CurrentLevel - 1] * card.CurrentNum) + "\n";
        }
        EfficiencyInfo.ContentB = content;
    }

    public void SetFaithDetail()
    {
        string content = "";
        foreach (PerkInfo info in CurrentPerks)
        {
            if (info.CurrentPerk.perkColor == PerkColor.Orange)
            {
                if (info.CurrentPerk.TempValue3 >= 0)
                    content += "事业部状态" + info.CurrentPerk.Name + " +" + info.CurrentPerk.TempValue3 + "\n";
                else
                    content += "事业部状态" + info.CurrentPerk.Name + info.CurrentPerk.TempValue3 + "\n";
            }
        }
        foreach (DepControl dep in CurrentDeps)
        {
            foreach (Employee emp in dep.CurrentEmps)
            {
                foreach (PerkInfo perk in emp.InfoDetail.PerksInfo)
                {
                    if (perk.CurrentPerk.DepPerk == true || perk.CurrentPerk.DivisionPerk == true)
                    {
                        if (perk.CurrentPerk.TempValue3 > 0)
                            content += emp.Name + "特质 " + perk.CurrentPerk.Name + " +" + perk.CurrentPerk.TempValue3 + "\n";
                        else if (perk.CurrentPerk.TempValue3 < 0)
                            content += emp.Name + "特质 " + perk.CurrentPerk.Name + " " + perk.CurrentPerk.TempValue3 + "\n";
                    }
                }
            }
        }
        if (Manager != null)
        {
            foreach (PerkInfo perk in Manager.InfoDetail.PerksInfo)
            {
                if (perk.CurrentPerk.DepPerk == true || perk.CurrentPerk.DivisionPerk == true)
                {
                    if (perk.CurrentPerk.TempValue3 > 0)
                        content += Manager.Name + "特质 " + perk.CurrentPerk.Name + " +" + perk.CurrentPerk.TempValue3 + "\n";
                    else if (perk.CurrentPerk.TempValue3 < 0)
                        content += Manager.Name + "特质 " + perk.CurrentPerk.Name + " " + perk.CurrentPerk.TempValue3 + "\n";
                }
            }
        }
        foreach (DepControl dep in CurrentDeps)
        {

            if (dep.ExtraFaith > 0)
                content += dep.Text_DepName.text + "额外效果 +" + dep.ExtraFaith + "\n";
            else if (dep.ExtraFaith < 0)
                content += dep.Text_DepName.text + "额外效果 " + dep.ExtraFaith + "\n";
        }
        FaithInfo.ContentB = content;
    }
}
