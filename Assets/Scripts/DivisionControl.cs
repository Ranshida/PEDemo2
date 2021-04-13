using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DivisionControl : MonoBehaviour
{
    public int Faith = 0;//事业部信念
    public int Efficiency = 0;//事业部效率
    public int ExtraEfficiency = 0;//部门提供的额外效率
    public int WorkStatus = 0;//事业部工作状态
    public string DivName;
    public bool canWork = true;
    public bool Locked = false;//事业部是否处于未解锁状态
    private bool DetailPanelShowed = false;

    public DepSelect DS;
    public GameObject DetailPanel;
    public Transform PerkContent, DepContent;
    public Area CurrentArea;
    public Text Text_DivName, Text_DivPanelName, Text_Faith, Text_Efficiency, Text_WorkStatus, Text_Manager, Text_Cost;
    public Employee Manager;
    public Image EfficiencyBarFill, WorkStatusBarFill;

    public List<DepControl> CurrentDeps = new List<DepControl>();

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
            Text_Faith.text = "部门信念:" + Faith;

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
                Text_WorkStatus.text += "\n\n工作状态:0\n工作停止";
            else if (WorkStatus < 4)
                Text_WorkStatus.text += "\n\n工作状态:1\n";
            else if (WorkStatus < 9)
                Text_WorkStatus.text += "\n\n工作状态:2\n";
            else
                Text_WorkStatus.text += "\n\n工作状态:3\n";

            if (Manager != null)
                Text_Manager.text = "当前高管:" + Manager.Name;
            else
                Text_Manager.text = "当前高管:无";

            int empCost = CalcCost(1), depCost = CalcCost(2);
            Text_Cost.text = "成本:" + (empCost + depCost) + "/回合\n\n员工工资:" + empCost + "/回合\n建筑维护费:" + depCost + "/回合";

            //效率和工作状态调填充
            if (ExtraEfficiency + Efficiency < 0)
                EfficiencyBarFill.fillAmount = 0.06f;
            else if (ExtraEfficiency + Efficiency < 9)
                EfficiencyBarFill.fillAmount = (float)(ExtraEfficiency + Efficiency) / 10 + 0.1f;
            else
                EfficiencyBarFill.fillAmount = 1;

            if (WorkStatus < 0)
                WorkStatusBarFill.fillAmount = 0.06f;
            else if (WorkStatus < 9)
                WorkStatusBarFill.fillAmount = (float)(WorkStatus) / 10 + 0.1f;
            else
                WorkStatusBarFill.fillAmount = 1;
        }
    }

    public void Produce()
    {
        if (canWork == false || Manager == null)
            return;

        foreach (DepControl dep in CurrentDeps)
        {
            dep.Produce();
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
            Manager.CurrentDivision = null;
            Manager = null;
        }
        //任命高管
        else
        {
            Manager = emp;
            emp.CurrentDivision = this;
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
