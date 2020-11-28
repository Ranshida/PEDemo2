using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Task
{
    //技术:程序迭代;技术研发;可行性调研
    //市场:公关谈判;营销文案;资源拓展
    //产品:原型图;产品研究;用户访谈
    public EmpType TaskType;            //Type:技术、市场、迭代; Num:各类的第1-3项; Value:品质, 最低是取1的低劣
    public int Num, HourLeft, Value;
    public int Progress;
    public string TaskName;
}

public class ProduceControl : MonoBehaviour
{
    public Text Text_DepName, Text_Cost, Text_MoneyCost;
    public Dropdown dropdown, dopdown2;
    public GameControl GC;

    public float HireCostRate = 1.0f, HeadHuntCostRate = 1.0f;
    public int TaskNum = 1, HireType = 1, HireLevel = 1;

    public int[] HeadHuntStatus = new int[15];
    public Toggle[] HeadHuntToggles = new Toggle[15];

    DepControl CurrentDep;

    int HeadHuntSelectNum = 0;
    int HireCost = 200;

    private void Update()
    {
        if (Text_Cost != null)
        {
            if (HeadHuntSelectNum == 0)
                Text_Cost.text = "生产力消耗:" + (int)(HireCost * HireCostRate);
            else
                Text_Cost.text = "生产力消耗:" + (int)(HireCost * HeadHuntCostRate);
        }
        if (Text_MoneyCost != null)
            Text_MoneyCost.text = "金钱消耗:" + HeadHuntSelectNum * 1000;
    }

    public void SetName(DepControl Dep)
    {
        CurrentDep = Dep;
        Text_DepName.text = Dep.Text_DepName.text;
        if(Dep.type == EmpType.Tech)
        {
            dropdown.options[0].text = "程序迭代";
            dropdown.options[1].text = "技术研发";
            dropdown.options[2].text = "可行性调研";
        }
        else if (Dep.type == EmpType.Market)
        {
            dropdown.options[0].text = "公关谈判";
            dropdown.options[1].text = "营销文案";
            dropdown.options[2].text = "资源拓展";
        }
        else
        {
            dropdown.options[0].text = "原型图";
            dropdown.options[1].text = "产品研究";
            dropdown.options[2].text = "用户访谈";
        }
        dropdown.value = 0;
        dopdown2.value = 0;
    }

    public void SetNum(int num)
    {
        TaskNum = num + 1;
    }

    public void SetHire(DepControl Dep)
    {
        CurrentDep = Dep;
        dropdown.value = 0;
        dopdown2.value = 0;
        if (Dep.type == EmpType.HR)
            ResetHeadHunt();
    }
    //猎头相关
    public void ResetHeadHunt()
    {
        for(int i = 0; i < 15; i++)
        {
            HeadHuntToggles[i].isOn = false;
            HeadHuntToggles[i].interactable = true;
        }
        for (int i = 0; i < 15; i++)
        {
            HeadHuntStatus[i] = 0;
        }
        HeadHuntSelectNum = 0;
        HireCost = 200;
    }
    public void SetHeadHunt(int num)
    {
        if(HeadHuntStatus[num] == 0)
        {
            HeadHuntStatus[num] = 1;
            HeadHuntSelectNum += 1;
            if (HeadHuntSelectNum > 0)
                HireCost = 2000 + (HeadHuntSelectNum - 1) * 2000;
            if(HeadHuntSelectNum == 5)
            {
                for(int i = 0; i < 15; i++)
                {
                    if (HeadHuntToggles[i].isOn == false)
                        HeadHuntToggles[i].interactable = false;
                }
            }
        }
        else
        {
            HeadHuntStatus[num] = 0;
            HeadHuntSelectNum -= 1;
            if (HeadHuntSelectNum > 0)
                HireCost = 2000 + (HeadHuntSelectNum - 1) * 2000;
            else
                HireCost = 200;
            if (HeadHuntSelectNum < 5)
            {
                for (int i = 0; i < 15; i++)
                {
                    HeadHuntToggles[i].interactable = true;  
                }
            }
        }
    }

    public void SetHireType(int num)
    {
        HireType = num + 1;
    }

    public void SetHireLevel(int num)
    {
        HireLevel = num + 1;
    }


    public void CreateHire()
    {
        if (GC.Money >= HeadHuntSelectNum * 1000)
        {
            this.gameObject.SetActive(false);
            GC.Money -= HeadHuntSelectNum * 1000;
            //CurrentDep.WorkStart = true;
            CurrentDep.SpType = HireType;
            if (HeadHuntSelectNum == 0)
                CurrentDep.SpTotalProgress = (int)(HireCost * HireCostRate);
            else
                CurrentDep.SpTotalProgress = (int)(HireCost * HeadHuntCostRate);
            for (int i = 0; i < 15; i++)
            {
                CurrentDep.DepHeadHuntStatus[i] = HeadHuntStatus[i];
            }
            CurrentDep.SpProgress = 0;
            CurrentDep.UpdateUI();
            ResetHeadHunt();
        }
    }

    public void CreateTask()
    {
        Task newTask = new Task();
        newTask.TaskType = CurrentDep.type;
        newTask.TaskName = dropdown.options[TaskNum - 1].text;
        newTask.Num = TaskNum;
        CurrentDep.SetTask(newTask);
    }


}
