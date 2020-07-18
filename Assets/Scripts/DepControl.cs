using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DepControl : MonoBehaviour
{
    static public int StandardProducePoint = 500;
    public float Efficiency = 1.0f;
    public bool canWork = false;

    public Transform EmpContent, EmpPanel;
    public GameObject OfficeWarning;
    public Building building;
    public GameControl GC;
    public Task CurrentTask;
    public DepSelect DS;
    public OfficeControl CommandingOffice;
    public Text Text_DepName, Text_Task, Text_Progress, Text_Quality, Text_Time, Text_Office;
    public EmpType type;

    public List<Employee> CurrentEmps = new List<Employee>();
    public List<OfficeControl> InRangeOffices = new List<OfficeControl>();


    public void StartTaskManage()
    {
        GC.PC.gameObject.SetActive(true);
        GC.PC.SetName(this);
    }

    public void SetTask(Task task)
    {
        Text_Task.text = "当前任务:" + task.TaskName;
        Text_Progress.text = "进度:" + task.Progress + " + " + CountProducePower(task.Num) * 5 + "/天";
        Text_Quality.text = "当前进度:无";
        Text_Time.text = "剩余时间:" + task.DayLeft + "天";
        CurrentTask = task;
    }
    void ResetText()
    {
        Text_Task.text = "当前任务:无";
        Text_Progress.text = "进度:000 + 000/天";
        Text_Quality.text = "当前进度:无";
        Text_Time.text = "剩余时间:--天";
    }

    public void Produce()
    {
        if (CurrentTask != null && canWork == true)
        {
            int Pp = CountProducePower(CurrentTask.Num);
            CurrentTask.Progress += Pp;
            Text_Progress.text = "进度:" + CurrentTask.Progress + " + " + Pp * 5 + "/天";
            string quality;
            float p = CurrentTask.Progress;
            if (p < StandardProducePoint)
                quality = "未完成";
            else if (p < StandardProducePoint * 2)
                quality = "低劣";
            else if (p < StandardProducePoint * 4)
                quality = "平庸";
            else if (p < StandardProducePoint * 8)
                quality = "优良";
            else
                quality = "完美";
            Text_Quality.text = "当前进度:" + quality;
        }
    }

    public void OneDayPass()
    {
        if(CurrentTask != null)
        {
            CurrentTask.DayLeft -= 1;
            if(CurrentTask.DayLeft < 1)
            {
                TaskFinish();
            }
            else
                Text_Time.text = "剩余时间:" + CurrentTask.DayLeft + "天";
        }
    }

    public void TaskFinish()
    {
        float p = CurrentTask.Progress;
        if (p < StandardProducePoint)
            CurrentTask.Value = 0;
        else if (p < StandardProducePoint * 2)
            CurrentTask.Value = 1;
        else if (p < StandardProducePoint * 4)
            CurrentTask.Value = 2;
        else if (p < StandardProducePoint * 8)
            CurrentTask.Value = 3;
        else
            CurrentTask.Value = 4;
        if(CurrentTask.Value > 0)
        {
            GC.FinishedTask.Add(CurrentTask);
        }
        CurrentTask = null;
        ResetText();
    }

    public void ShowProducePower()
    {
        if(CurrentTask != null)
        {
            int p = CountProducePower(CurrentTask.Num);
            Text_Progress.text = "进度:" + CurrentTask.Progress + " + " + p * 5 + "/天";
        }
    }

    public void ShowAvailableOffices()
    {
        GC.SelectMode = 3;
        GC.ShowDepSelectPanel(this);
        GC.CurrentDep = this;
    }

    public int CountProducePower(int num)
    {
        int power = 0;
        if (num == 1)
        {
            for(int i = 0; i < CurrentEmps.Count; i++)
            {
                power += CurrentEmps[i].Skill1 + CurrentEmps[i].SkillExtra1;
            }
        }
        else if (num == 2)
        {
            for (int i = 0; i < CurrentEmps.Count; i++)
            {
                power += CurrentEmps[i].Skill2 + CurrentEmps[i].SkillExtra2;
            }
        }
        else
        {
            for (int i = 0; i < CurrentEmps.Count; i++)
            {
                power += CurrentEmps[i].Skill3 + CurrentEmps[i].SkillExtra3;
            }
        }
        return (int)(power * Efficiency);
    }

    public void StartMobilize()
    {
        GC.SC.gameObject.SetActive(true);
        GC.SC.SetDice(this);
    }
}
