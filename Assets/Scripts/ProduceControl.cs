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
    public Text Text_DepName;
    public Dropdown dropdown, dopdown2;

    DepControl CurrentDep;

    int TaskNum = 1, HireType = 1, HireLevel = 1;

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
        CurrentDep.WorkStart = true;
        CurrentDep.SpType = HireType;
        CurrentDep.SpTotalProgress = HireLevel * 100;
        CurrentDep.SpProgress = 0;
        CurrentDep.UpdateUI();
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
