using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProduceBuff
{
    public float Value;
    public int TimeLeft;
    public DepControl TargetDep;

    public ProduceBuff(float value, DepControl Dep)
    {
        Value = value;
        TimeLeft = 32;
        TargetDep = Dep;
        Dep.produceBuffs.Add(this);
        Dep.Efficiency += Value;
        TargetDep.GC.HourEvent.AddListener(TimePass);
    }

    public void TimePass()
    {
        TimeLeft -= 1;
        if(TimeLeft < 1)
        {
            TargetDep.GC.HourEvent.RemoveListener(TimePass);
            TargetDep.Efficiency -= Value;
            TargetDep.produceBuffs.Remove(this);
        }
    }
}

public class DepControl : MonoBehaviour
{
    static public int StandardProducePoint = 50;
    [HideInInspector] public int SurveyProgress = 0, FailProgress = 0;
    [HideInInspector] public bool SurveyStart = false, Failed = false;
    public int EmpLimit;
    public float Efficiency = 1.0f, FailRate = 0.3f;
    public bool canWork = false;

    [HideInInspector] public Research CurrentResearch;
    public Transform EmpContent, EmpPanel, LabPanel;
    public GameObject OfficeWarning;
    public Building building;
    public GameControl GC;
    public Task CurrentTask;
    public DepSelect DS;
    public OfficeControl CommandingOffice;
    public Text Text_DepName, Text_Task, Text_Progress, Text_Quality, Text_Time, Text_Office;
    public Button SurveyButton;
    public EmpType type;

    public Research[] Researches = new Research[3];
    public List<ProduceBuff> produceBuffs = new List<ProduceBuff>();
    public List<Employee> CurrentEmps = new List<Employee>();
    public List<OfficeControl> InRangeOffices = new List<OfficeControl>();


    public void StartTaskManage()
    {
        GC.PC.gameObject.SetActive(true);
        GC.PC.SetName(this);
    }

    public void SetTask(Task task)
    {
        if (Failed == false)
        {
            Text_Task.text = "当前任务:" + task.TaskName;
            Text_Progress.text = "进度:" + task.Progress + " + " + CountProducePower(task.Num) + "/天";
        }
        Text_Quality.text = "当前进度:无";
        Text_Time.text = "剩余时间:" + task.HourLeft + "时";
        CurrentTask = task;
    }
    void ResetText()
    {
        Text_Task.text = "当前任务:无";
        Text_Progress.text = "进度:000 + 000/时";
        Text_Quality.text = "当前进度:无";
        Text_Time.text = "剩余时间:--时";
    }

    public void Produce()
    {
        if (canWork == true)
        {
            if (Failed == false)
            {
                if (CurrentTask != null)
                {
                    int Pp = CountProducePower(CurrentTask.Num);
                    CurrentTask.Progress += Pp;
                    Text_Progress.text = "进度:" + CurrentTask.Progress + " + " + Pp + "/时";
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
                else if (CurrentResearch != null)
                {
                    int Pp = CountProducePower(4);
                    CurrentResearch.CurrentProgress += Pp;
                    Text_Progress.text = "当前进度: " + (int)((float)CurrentResearch.CurrentProgress / (float)(CurrentResearch.Progress) * 100) + "%";
                    CurrentResearch.UpdateUI();
                    if (CurrentResearch.CurrentProgress >= CurrentResearch.Progress)
                    {
                        CurrentResearch.ResearchFinish();
                        CurrentResearch.ExtraButton.gameObject.SetActive(false);
                        CurrentResearch = null;
                        Text_Task.text = "当前任务: 无";
                        Text_Progress.text = "当前进度:----";
                    }
                }
                else if (SurveyStart == true)
                {
                    int Pp = CountProducePower(4);
                    SurveyProgress += Pp;
                    Text_Progress.text = "当前进度: " + (int)((float)SurveyProgress / (float)(StandardProducePoint * 10) * 100) + "%";
                    Text_Quality.text = "调研进度: " + SurveyProgress + "/" + (StandardProducePoint * 10);
                    if (SurveyProgress >= StandardProducePoint * 10)
                    {
                        RandomResearch();
                        SurveyButton.interactable = true;
                        Text_Task.text = "当前任务: 无";
                        Text_Progress.text = "当前进度:----";
                        SurveyStart = false;
                    }
                }
            }
            else
            {
                int Pp = CountProducePower(4);
                FailProgress -= Pp;
                if (FailProgress > 0)
                    Text_Progress.text = "剩余失误点: " + FailProgress + " -" + Pp + "/时";
                else
                {
                    Failed = false;
                    FailProgress = 0;
                    if (CurrentTask != null)
                    {
                        Pp = CountProducePower(CurrentTask.Num);
                        Text_Progress.text = "进度:" + CurrentTask.Progress + " + " + Pp + "/时";
                        Text_Task.text = "当前任务:" + CurrentTask.TaskName;
                    }
                    else
                    {
                        Text_Task.text = "当前任务: 无";
                        Text_Progress.text = "当前进度:----";
                    }
                }
            }
        }
        OneHourPass();
    }

    public void OneHourPass()
    {
        if (CurrentTask != null && canWork == true && Failed == false)
        {
            CurrentTask.HourLeft -= 1;
            if(CurrentTask.HourLeft < 1)
            {
                TaskFinish();
            }
            else
                Text_Time.text = "剩余时间:" + CurrentTask.HourLeft + "时";
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
            GC.UpdateResourceInfo();
        }
        CurrentTask = null;
        ResetText();
    }

    public void ShowProducePower()
    {
        if(CurrentTask != null)
        {
            int p = CountProducePower(CurrentTask.Num);
            Text_Progress.text = "进度:" + CurrentTask.Progress + " + " + p + "/天";
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
        else if (num == 3)
        {
            for (int i = 0; i < CurrentEmps.Count; i++)
            {
                power += CurrentEmps[i].Skill3 + CurrentEmps[i].SkillExtra3;
            }
        }
        else
        {
            for (int i = 0; i < CurrentEmps.Count; i++)
            {
                int temp = CurrentEmps[i].Skill1;
                if (CurrentEmps[i].Skill2 > temp)
                    temp = CurrentEmps[i].Skill2;
                if (CurrentEmps[i].Skill3 > temp)
                    temp = CurrentEmps[i].Skill3;
                power += temp;
            }
        }
        float extraEfficiency = 0;
        if (type != EmpType.Science)
            extraEfficiency = GC.EfficiencyExtraNormal;
        else if(type == EmpType.Science)
            extraEfficiency = GC.EfficiencyExtraScience;
        return (int)(power * (Efficiency + extraEfficiency));
    }

    public void StartMobilize()
    {
        if (GC.Mentality >= 20)
        {
            GC.Mentality -= 20 - GC.MobilizeExtraMent;
            GC.SC.gameObject.SetActive(true);
            GC.SC.SetDice(this);
        }
    }

    public bool CheckEmpNum()
    {
        if (CurrentEmps.Count < EmpLimit)
            return true;
        else
            return false;        
    }

    public void StartSurvey()
    {
        //开始调研
        Text_Task.text = "当前任务: 调研";
        Text_Progress.text = "当前进度: 0%";
        Text_Quality.text = "调研进度: 0/" + (StandardProducePoint * 10);
        SurveyStart = true;
        SurveyProgress = 0;
        CurrentResearch = null;
        for (int i = 0; i < 3; i++)
        {
            Researches[i].ResetUI();
            Researches[i].ExtraButton.gameObject.SetActive(false);
            Researches[i].SelectButton.gameObject.SetActive(false);
        }
    }

    public void RandomResearch()
    {
        Text_Task.text = "当前任务: 无";
        Text_Progress.text = "当前进度:----";
        for(int i = 0; i < 3; i++)
        {
            Researches[i].SetResearch(Random.Range(1, 15));
            Researches[i].GC = GC;
        }
    }

    public void ResearchExtra()
    {
        if(CurrentResearch != null)
        {
            CurrentResearch.Progress += CurrentResearch.ExtraProgress;
            CurrentResearch.SuccessRate += CurrentResearch.ExtraRate;
            CurrentResearch.UpdateUI();
            CurrentResearch.ExtraButton.gameObject.SetActive(false);
        }
    }

    public void FailCheck()
    {
        float Posb = Random.Range(0.0f, 1.0f);
        float totalObservation = 0;
        for(int i = 0; i < CurrentEmps.Count; i++)
        {
            totalObservation += CurrentEmps[i].Observation;
        }
        float ActualFailRate = FailRate - (totalObservation * GC.Morale * 0.0001f);
        print(Posb);
        print(ActualFailRate);
        if(Posb < ActualFailRate)
        {
            Failed = true;
            FailProgress += Random.Range(50, 120);
            Text_Task.text = "当前任务:失误处理";
            Text_Time.text = "剩余时间:--时";
        }
    }
}
