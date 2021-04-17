using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DivisionControl : MonoBehaviour
{
    public int Faith = 0;//事业部信念
    public int Efficiency = 0;//事业部效率
    public int ExtraEfficiency = 0;//部门提供的额外效率
    public int ExtraManage = 0;//部门提供的额外高管管理能力加成
    public int WorkStatus = 0;//事业部工作状态
    public int ExtraCost = 0;//额外成本加成
    public int ExtraProduceTime = 0;//额外生产周期加成
    public int ExtraExp = 0;//每回合为管理下的部门的员工额外提供的经验值
    public string DivName;
    public bool canWork = true;
    public bool Locked = false;//事业部是否处于未解锁状态
    public bool StatusShowed = false;//是否显示信息面板
    private bool DetailPanelShowed = false;//是否显示详细信息面板

    public GameControl GC;
    public DepSelect DS;
    public GameObject DetailPanel;
    public Transform PerkContent, DepContent;
    public Area CurrentArea;
    public Text Text_DivName, Text_DivPanelName, Text_Faith, Text_Efficiency, Text_WorkStatus, Text_Manager, Text_Cost, Text_Status;
    public Employee Manager;
    public Image EfficiencyBarFill, WorkStatusBarFill, FaithFill;

    public List<DepControl> CurrentDeps = new List<DepControl>();
    public List<RectTransform> FillMarkers = new List<RectTransform>();

    private void Update()
    {
        if(CurrentArea != null)
            Text_DivName.transform.position = Function.World2ScreenPoint(CurrentArea.topPosition);

        //各种信息更新
        if (Manager == null)
            Text_DivName.text = DivName + "(无高管)";
        else
            Text_DivName.text = DivName;

        if (DetailPanelShowed == true)
        {
            Text_Faith.text = "部门信念:" + Faith + "\n\n事件修正:+0";

            Text_Efficiency.text = "效率:" + (Efficiency + ExtraEfficiency);
            if (Efficiency + ExtraEfficiency < 0)
                Text_Efficiency.text += "\n\n效率等级:0\n工作停止";
            else if (Efficiency + ExtraEfficiency < 4)
                Text_Efficiency.text += "\n\n效率等级:1\n产率:1";
            else if (Efficiency + ExtraEfficiency < 9)
                Text_Efficiency.text += "\n\n效率等级:2\n产率:2";
            else
                Text_Efficiency.text += "\n\n效率等级:3\n产率:3";

            Text_WorkStatus.text = "工作状态:" + WorkStatus;
            if (WorkStatus < 0)
                Text_WorkStatus.text += "\n\n工作等级:0\n工作停止";
            else if (WorkStatus < 4)
                Text_WorkStatus.text += "\n\n工作等级:1\n";
            else if (WorkStatus < 9)
                Text_WorkStatus.text += "\n\n工作等级:2\n";
            else
                Text_WorkStatus.text += "\n\n工作等级:3\n";

            if (Manager != null)
                Text_Manager.text = "当前高管:" + Manager.Name;
            else
                Text_Manager.text = "当前高管:无";

            int empCost = CalcCost(1), depCost = CalcCost(2);
            Text_Cost.text = "成本:" + (empCost + depCost + ExtraCost) + "/回合\n\n员工工资:" + empCost + "/回合\n建筑维护费:" + depCost + "/回合";

            //效率和工作状态调填充
            if (ExtraEfficiency + Efficiency < 0)
            {
                EfficiencyBarFill.fillAmount = 0.06f;
                EfficiencyBarFill.color = new Color(1, 0, 0);
            }
            else if (ExtraEfficiency + Efficiency < 4)
            {
                EfficiencyBarFill.fillAmount = (float)(ExtraEfficiency + Efficiency) / 10 + 0.1f;
                EfficiencyBarFill.color = new Color(1, 1, 1);
            }
            else if (ExtraEfficiency + Efficiency < 9)
            {
                EfficiencyBarFill.fillAmount = (float)(ExtraEfficiency + Efficiency) / 10 + 0.1f;
                EfficiencyBarFill.color = new Color(1, 1, 0);
            }
            else
            {
                EfficiencyBarFill.fillAmount = 1;
                EfficiencyBarFill.color = new Color(0, 1, 0);
            }
            FillMarkers[0].anchoredPosition = new Vector2(FillMarkers[0].parent.GetComponent<RectTransform>().sizeDelta.x * EfficiencyBarFill.fillAmount, 0);


            if (WorkStatus < 0)
            {
                WorkStatusBarFill.fillAmount = 0.06f;
                WorkStatusBarFill.color = new Color(1, 0, 0);
            }
            else if (WorkStatus < 4)
            {
                WorkStatusBarFill.fillAmount = (float)(WorkStatus) / 10 + 0.1f;
                WorkStatusBarFill.color = new Color(1, 1, 1);
            }
            else if (WorkStatus < 9)
            {
                WorkStatusBarFill.fillAmount = (float)(WorkStatus) / 10 + 0.1f;
                WorkStatusBarFill.color = new Color(1, 1, 0);
            }
            else
            {
                WorkStatusBarFill.fillAmount = 1;
                WorkStatusBarFill.color = new Color(0, 1, 0);
            }
            FillMarkers[1].anchoredPosition = new Vector2(FillMarkers[1].parent.GetComponent<RectTransform>().sizeDelta.x * WorkStatusBarFill.fillAmount, 0);


            if (Faith <= -90)
            {
                FaithFill.fillAmount = 0.06f;
                FaithFill.color = new Color(1, 0, 0);
            }
            else if (Faith <= -30)
            {
                FaithFill.fillAmount = (float)(100 + Faith) / 200;
                FaithFill.color = new Color(1, 0.4f, 0.4f);
            }
            else if (Faith < 0)
            {
                FaithFill.fillAmount = (float)(100 + Faith) / 200;
                FaithFill.color = new Color(1, 0.8f, 0.8f);
            }
            else if (Faith <= 30)
            {
                FaithFill.fillAmount = (float)Faith / 200 + 0.5f;
                FaithFill.color = new Color(0.8f, 1, 0.8f);
            }
            else if (Faith <= 90)
            {
                FaithFill.fillAmount = (float)Faith / 200 + 0.5f;
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
            if (WorkStatus < 0)
                Text_Status.text = "<color=#FF0000>工作状态:等级0 工作停工</color>";
            else if (WorkStatus < 4)
                Text_Status.text = "<color=#FFFFFF>工作状态:等级1</color>";
            else if (WorkStatus < 9)
                Text_Status.text = "<color=#FFFF00>工作状态:等级2</color>";
            else
                Text_Status.text = "<color=#00FF00>工作状态:等级3</color>";

            if (Efficiency + ExtraEfficiency < 0)
                Text_Status.text += "\n<color=#FF0000>效率:等级0 工作停止</color>";
            else if (Efficiency + ExtraEfficiency < 4)
                Text_Status.text += "\n<color=#FFFFFF>效率:等级1</color>";
            else if (Efficiency + ExtraEfficiency < 9)
                Text_Status.text += "\n<color=#FFFF00>效率:等级2</color>";
            else
                Text_Status.text += "\n<color=#00FF00>效率:等级3</color>";

            if (Faith <= -90)
                Text_Status.text += "\n<color=#FF0000>信念:" + Faith + "</color>";
            else if (Faith <= -30)
                Text_Status.text += "\n<color=#FF6666>信念:" + Faith + "</color>";
            else if (Faith < 0)
                Text_Status.text += "\n<color=#FFCCCC>信念:" + Faith + "</color>";
            else if (Faith == 0)
                Text_Status.text += "\n<color=#FFFFFF>信念:" + Faith + "</color>";
            else if (Faith <= 30)
                Text_Status.text += "\n<color=#CCFFCC>信念:" + Faith + "</color>";
            else if (Faith <= 90)
                Text_Status.text += "\n<color=#66FF66>信念:" + Faith + "</color>";
            else
                Text_Status.text += "\n<color=#00FF00>信念:" + Faith + "</color>";

            Text_Status.text += "\n成本:" + (CalcCost(1) + CalcCost(2)) + "/回合";
        }
    }

    public void Produce()
    {
        if (canWork == false || Manager == null)
            return;

        foreach (DepControl dep in CurrentDeps)
        {
            dep.Produce();
            if(ExtraExp > 0)
            {
                foreach(Employee emp in dep.CurrentEmps)
                {
                    emp.GainExp(ExtraExp, 0);
                }
            }
        }
        GC.Money -= (CalcCost(1) + CalcCost(2) + ExtraCost);
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

    //部门移动、销毁或者属性变化时检测额外效率
    public void DepExtraCheck()
    {
        ExtraEfficiency = 0;
        foreach(DepControl dep in CurrentDeps)
        {
            ExtraEfficiency += dep.ExtraEfficiency;
        }
        if (ExtraEfficiency + Efficiency < 0)
            canWork = false;
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
}
