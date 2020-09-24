using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProduceBuff
{
    public float Value;
    public int TimeLeft;
    public DepControl TargetDep;

    public ProduceBuff(float value, DepControl Dep, int Time = 32)
    {
        Value = value;
        TimeLeft = Time;
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
public class HireType
{
    public int Level = 0, Type;
    public int[] HeadHuntStatus = new int[15];
    public HireType(int type)
    {
        Type = type;
    }
    public void SetHeadHuntStatus(int[] Hst)
    {
        for(int i = 0; i < HeadHuntStatus.Length; i++)
        {
            HeadHuntStatus[i] = Hst[i];
            if (Hst[i] == 1)
                Level += 1;
        }
    }
}

public class DepControl : MonoBehaviour
{
    static public int StandardProducePoint = 50;
    [HideInInspector] public int SpProgress = 0, SpTotalProgress, FailProgress = 0, TaskChangeTime = 0, EfficiencyLevel = 0, LevelDownTime = 0, SpType, SpTime;//SPTime:CEO技能
    [HideInInspector] public bool SurveyStart = false, Failed = false, TaskChange = false, WorkStart = false; //WorkStart专门用于特殊业务
    public int EmpLimit;
    public float Efficiency = 1.0f, FailRate = 0.3f;
    public bool canWork = false;

    [HideInInspector] public Research CurrentResearch;
    public Transform EmpContent, EmpPanel, LabPanel;
    public GameObject OfficeWarning;
    public Building building = null;
    public GameControl GC;
    public Task CurrentTask;
    public DepSelect DS;
    public OfficeControl CommandingOffice;
    public Text Text_DepName, Text_Task, Text_Progress, Text_Quality, Text_Time, Text_Office, Text_Efficiency, Text_LevelDownTime;
    public Button SurveyButton;
    public EmpType type;

    public Research[] Researches = new Research[3];
    public Skill[] DSkillSetA = new Skill[6], DSkillSetB = new Skill[6], DSkillSetC = new Skill[6];
    public List<ProduceBuff> produceBuffs = new List<ProduceBuff>();
    public List<Employee> CurrentEmps = new List<Employee>();
    public List<OfficeControl> InRangeOffices = new List<OfficeControl>();
    public int[] DepHeadHuntStatus = new int[15];

    private void Update()
    {
        if (type == EmpType.Science)
            Text_Efficiency.text = "效率:" + (Efficiency + GC.EfficiencyExtraScience) * 100 + "%";
        else if (type != EmpType.HR)
            Text_Efficiency.text = "效率:" + (Efficiency + GC.EfficiencyExtraNormal) * 100 + "%";
    }

    public void StartTaskManage()
    {
        if (type != EmpType.HR)
        {
            GC.PC.gameObject.SetActive(true);
            GC.PC.SetName(this);
        }
        else
        {
            GC.PC2.gameObject.SetActive(true);
            GC.PC2.SetHire(this);
        }
    }

    public void SetTask(Task task)
    {
        int Pp = CountProducePower((int)type + 1);
        if(CurrentTask != null)
        {
            TaskChangeTime = 16;
            TaskChange = true;
        }
        CurrentTask = task;
        UpdateUI(Pp);
    }

    //目前不只是制作，还有很多别的跟事件相关的功能都写在这儿
    public void Produce()
    {
        if (canWork == true)
        {
            if (Failed == false)
            {
                if (CurrentTask != null && TaskChange == false)
                {
                    int Pp = CountProducePower((int)type + 1);
                    CurrentTask.Progress += Pp;
                    if (CurrentTask.Progress >= StandardProducePoint * 10)
                    {
                        GC.CreateMessage(Text_DepName.text + " 完成了 " + CurrentTask.TaskName + " 的生产");
                        GC.FinishedTask[(int)CurrentTask.TaskType * 3 + CurrentTask.Num - 1] += 1;
                        GC.UpdateResourceInfo();
                        EmpsGetExp(CurrentTask.Num);
                        CurrentTask.Progress = 0;
                        if (CurrentTask.Num == 1 && (int)CurrentTask.TaskType == 2)
                            GC.StrC.SolveStrRequest(3, 1);
                        else if (CurrentTask.Num == 2 && (int)CurrentTask.TaskType == 1)
                            GC.StrC.SolveStrRequest(4, 1);
                        else if (CurrentTask.Num == 1 && (int)CurrentTask.TaskType == 0)
                            GC.StrC.SolveStrRequest(5, 1);
                    }
                    UpdateUI(Pp);
                }
                else if (CurrentResearch != null)
                {
                    int Pp = CountProducePower(6);
                    CurrentResearch.CurrentProgress += Pp;
                    CurrentResearch.UpdateUI();
                    if (CurrentResearch.CurrentProgress >= CurrentResearch.Progress)
                    {
                        GC.CreateMessage(Text_DepName.text + " 完成了 " + CurrentResearch.Text_Name.text + " 的研究");
                        EmpsGetExp(2, CurrentResearch.Progress / 2);
                        CurrentResearch.ResearchFinish();
                        CurrentResearch.ExtraButton.gameObject.SetActive(false);                       
                        CurrentResearch = null;
                        GC.StrC.SolveStrRequest(7, 1);
                    }
                    UpdateUI(Pp);
                }
                else if (SurveyStart == true)
                {
                    int Pp = CountProducePower(6);
                    SpProgress += Pp;
                    if (SpProgress >= StandardProducePoint * 10)
                    {
                        GC.CreateMessage(Text_DepName.text + " 完成了调研");
                        RandomResearch();
                        SurveyButton.interactable = true;
                        SurveyStart = false;
                        EmpsGetExp(2);
                    }                    
                    UpdateUI(Pp);
                    GC.StrC.SolveStrRequest(7, 1);
                }
                else if (WorkStart == true)
                {
                    if(type == EmpType.HR)
                    {
                        int Pp = CountProducePower(5);
                        SpProgress += Pp;
                        if(SpProgress >= SpTotalProgress)
                        {
                            GC.CreateMessage(Text_DepName.text + " 完成了 招聘");
                            //GC.FinishedTask[9] += 1; 原人力资源部2相关
                            //GC.UpdateResourceInfo();
                            EmpsGetExp(8);
                            HireType ht = new HireType(SpType);
                            ht.SetHeadHuntStatus(DepHeadHuntStatus);
                            GC.HC.AddHireTypes(ht);
                            SpType = 0;
                            SpProgress = 0;
                            SpTotalProgress = 0;
                            WorkStart = false;
                        }
                        UpdateUI(Pp);
                    }
                }

            }
            else
            {
                int Pp = CountProducePower(4);
                FailProgress -= Pp;
                if (FailProgress <= 0)
                {
                    GC.CreateMessage(Text_DepName.text + "完成了 失误处理");
                    Failed = false;
                    FailProgress = 0;
                }
                UpdateUI(Pp);
            }
        }
        if(TaskChange == true)
        {
            TaskChangeTime -= 1;
            if(TaskChangeTime < 1)
            {
                GC.CreateMessage(Text_DepName.text + " 完成了生产线调整");
                TaskChange = false;
                TaskChangeTime = 0;
            }
            UpdateUI();
        }
        if(LevelDownTime > 0)
        {
            LevelDownTime -= 1;
            Text_LevelDownTime.text = "降级时间:" + LevelDownTime + "时";
            if (LevelDownTime == 0 && EfficiencyLevel > 1)
            {
                EfficiencyLevel -= 1;
                Efficiency -= 0.2f;
                GC.CreateMessage(Text_DepName.text + " 的头脑风暴等级下降了");
            }
        }
        if (SpTime > 0)
            SpTime -= 1;
    }

    public void UpdateUI()
    {
        if (Failed == true)
        {
            int Pp = CountProducePower(4);
            Text_Task.text = "当前任务: 失误处理";
            Text_Progress.text = "生产力:" + Pp + "/时";
            Text_Quality.text = "剩余失误点: " + FailProgress;
            Text_Time.text = "剩余时间:" + calcTime(Pp, FailProgress) + "时";
        }
        else if (type == EmpType.Science)
        {
            int Pp = CountProducePower(6);
            if (CurrentResearch != null)
            {
                Text_Task.text = "当前任务:" + CurrentResearch.Text_Name.text;
                Text_Progress.text = "研究速度:" + Pp + "/时";
                Text_Quality.text = "研究进度: " + CurrentResearch.CurrentProgress + "/" + CurrentResearch.Progress;
                Text_Time.text = "剩余时间:" + calcTime(Pp, CurrentResearch.Progress - CurrentResearch.CurrentProgress) + "时";
            }
            else if (SurveyStart == true)
            {
                Text_Task.text = "当前任务: 调研";
                Text_Progress.text = "研究速度:" + Pp + "/时";
                Text_Quality.text = "研究进度: " + SpProgress + "/" + (StandardProducePoint * 10);
                Text_Time.text = "剩余时间:" + calcTime(Pp, StandardProducePoint * 10 - SpProgress) + "时";
            }
            else
            {
                Text_Task.text = "当前任务: 无";
                Text_Progress.text = "研究速度:" + Pp + "/时";
                Text_Quality.text = "研究进度:----";
                Text_Time.text = "剩余时间:----";
            }
        }
        else if (TaskChange == true)
        {
            Text_Task.text = "当前任务: 切换目标";
            Text_Progress.text = "生产力:----";
            Text_Quality.text = "当前进度:----";
            Text_Time.text = "剩余时间:" + TaskChangeTime + "时";
        }
        else if (CurrentTask != null)
        {
            int Pp = CountProducePower((int)type + 1);
            Text_Task.text = "当前任务:" + CurrentTask.TaskName;
            Text_Progress.text = "生产力:" + Pp + "/时";
            Text_Quality.text = "当前进度:" + CurrentTask.Progress + "/" + StandardProducePoint * 10;
            Text_Time.text = "剩余时间:" + calcTime(Pp, StandardProducePoint * 10 - CurrentTask.Progress) + "时";
        }
        else if (type == EmpType.HR && WorkStart == true)
        {
            int Pp = CountProducePower(5);
            Text_Task.text = "当前任务:招聘";
            Text_Progress.text = "生产力:" + Pp + "/时";
            Text_Quality.text = "当前进度:" + SpProgress + "/" + SpTotalProgress;
            Text_Time.text = "剩余时间:" + calcTime(Pp, SpTotalProgress - SpProgress) + "时";
        }
        else
        {
            Text_Task.text = "当前任务: 无";
            Text_Progress.text = "生产力:----";
            Text_Quality.text = "当前进度:----";
            Text_Time.text = "当前进度:----";
        }
    }

    void UpdateUI(int Pp)
    {
        if(Failed == true)
        {
            Text_Task.text = "当前任务: 失误处理";
            Text_Progress.text = "生产力:" + Pp + "/时";
            Text_Quality.text = "剩余失误点: " + FailProgress;
            Text_Time.text = "剩余时间:" + calcTime(Pp, FailProgress) + "时";
        }
        else if(type == EmpType.Science)
        {
            if (CurrentResearch != null)
            {
                Text_Task.text = "当前任务:" + CurrentResearch.Text_Name.text;
                Text_Progress.text = "研究速度:" + Pp + "/时";
                Text_Quality.text = "研究进度: " + CurrentResearch.CurrentProgress + "/" + CurrentResearch.Progress;
                Text_Time.text = "剩余时间:" + calcTime(Pp, CurrentResearch.Progress - CurrentResearch.CurrentProgress) + "时";
            }
            else if (SurveyStart == true)
            {
                Text_Task.text = "当前任务: 调研";
                Text_Progress.text = "研究速度:" + Pp + "/时";
                Text_Quality.text = "研究进度: " + SpProgress + "/" + (StandardProducePoint * 10);
                Text_Time.text = "剩余时间:" + calcTime(Pp, StandardProducePoint * 10 - SpProgress) + "时";
            }
            else
            {
                Text_Task.text = "当前任务: 无";
                Text_Progress.text = "研究速度:" + Pp + "/时";
                Text_Quality.text = "研究进度:----";
                Text_Time.text = "剩余时间:----";
            }
        }
        else if (TaskChange == true)
        {
            Text_Task.text = "当前任务: 切换目标";
            Text_Progress.text = "生产力:----";
            Text_Quality.text = "当前进度:----";
            Text_Time.text = "剩余时间:" + TaskChangeTime + "时";
        }
        else if (CurrentTask != null)
        {
            Text_Task.text = "当前任务:" + CurrentTask.TaskName;
            Text_Progress.text = "生产力:" + Pp + "/时";
            Text_Quality.text = "当前进度:" + CurrentTask.Progress + "/" + StandardProducePoint * 10;
            Text_Time.text = "剩余时间:" + calcTime(Pp, StandardProducePoint * 10 - CurrentTask.Progress) + "时";
        }
        else if (type == EmpType.HR && WorkStart == true)
        {
            Text_Task.text = "当前任务:招聘";
            Text_Progress.text = "生产力:" + Pp + "/时";
            Text_Quality.text = "当前进度:" + SpProgress + "/" + SpTotalProgress;
            Text_Time.text = "剩余时间:" + calcTime(Pp, SpTotalProgress - SpProgress) + "时";
        }
        else
        {
            Text_Task.text = "当前任务: 无";
            Text_Progress.text = "生产力:----";
            Text_Quality.text = "当前进度:----";
            Text_Time.text = "当前进度:----";
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
        //1-3对应3个生产技能
        if (num == 1)
        {
            for(int i = 0; i < CurrentEmps.Count; i++)
            {
                if (CurrentEmps[i].canWork == true)
                    power += CurrentEmps[i].Skill1 + CurrentEmps[i].SkillExtra1;
            }
        }
        else if (num == 2)
        {
            for (int i = 0; i < CurrentEmps.Count; i++)
            {
                if (CurrentEmps[i].canWork == true)
                    power += CurrentEmps[i].Skill2 + CurrentEmps[i].SkillExtra2;
            }
        }
        else if (num == 3)
        {
            for (int i = 0; i < CurrentEmps.Count; i++)
            {
                if (CurrentEmps[i].canWork == true)
                    power += CurrentEmps[i].Skill3 + CurrentEmps[i].SkillExtra3;
            }
        }
        //取最高
        else if (num == 4)
        {
            for (int i = 0; i < CurrentEmps.Count; i++)
            {
                if (CurrentEmps[i].canWork == true)
                {
                    int temp = CurrentEmps[i].Skill1;
                    if (CurrentEmps[i].Skill2 > temp)
                        temp = CurrentEmps[i].Skill2;
                    if (CurrentEmps[i].Skill3 > temp)
                        temp = CurrentEmps[i].Skill3;
                    power += temp;
                }
            }
        }
        else if (num == 5)
        {
            //人力资源部B计算
            for (int i = 0; i < CurrentEmps.Count; i++)
            {
                if (CurrentEmps[i].canWork == true)
                    power += CurrentEmps[i].HR;
            }
        }
        //技术和产品里取最高（科研用）
        else if (num == 6)
        {
            for (int i = 0; i < CurrentEmps.Count; i++)
            {
                if (CurrentEmps[i].canWork == true)
                {
                    int temp = CurrentEmps[i].Skill1;
                    if (CurrentEmps[i].Skill3 > temp)
                        temp = CurrentEmps[i].Skill3;
                    power += temp;
                }
            }
        }
        float extraEfficiency = 0;
        if (type != EmpType.Science)
            extraEfficiency = GC.EfficiencyExtraNormal;
        else if (type == EmpType.Science)
            extraEfficiency = GC.EfficiencyExtraScience;
        else if (type == EmpType.HR)
            extraEfficiency = GC.HireEfficiencyExtra;

        float FinalEfficiency = Efficiency + extraEfficiency;
        if (FinalEfficiency < 0)
            FinalEfficiency = 0;
        return (int)(power * FinalEfficiency);
    }

    int calcTime(int pp, int total)
    {
        int time = 0;
        if (pp != 0)
        {
            time = total / pp;
            if (total % pp > 0)
                time += 1;
        }        
        return time;
    }

    //开始动员
    public void StartMobilize()
    {
        GC.SC.gameObject.SetActive(true);
        GC.SC.SetDice(this);
        ShowEmpInfoPanel();
    }

    //显示员工面板并关闭其他部门面板
    public void ShowEmpInfoPanel()
    {
        for (int i = 0; i < GC.CurrentDeps.Count; i++)
        {
            GC.CurrentDeps[i].EmpPanel.gameObject.SetActive(false);
        }
        EmpPanel.gameObject.SetActive(true);
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
        SurveyStart = true;
        SpProgress = 0;
        CurrentResearch = null;
        for (int i = 0; i < 3; i++)
        {
            Researches[i].ResetUI();
            Researches[i].ExtraButton.gameObject.SetActive(false);
            Researches[i].SelectButton.gameObject.SetActive(false);
        }
        UpdateUI(CountProducePower(6));
    }

    public void RandomResearch()
    {
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

    public void FailCheck(bool ForceFail = false)
    {
        float Posb = Random.Range(0.0f, 1.0f);
        float totalObservation = 0;
        for(int i = 0; i < CurrentEmps.Count; i++)
        {
            totalObservation += CurrentEmps[i].Observation;
        }
        float ActualFailRate = FailRate - (totalObservation * GC.Morale * 0.0001f) - GC.ExtrafailRate;
        if(Posb < ActualFailRate || ForceFail == true)
        {
            Failed = true;
            FailProgress += Random.Range(50, 120);
            if (type != EmpType.Market && type != EmpType.Operate && type != EmpType.Product && type != EmpType.Tech)
                FailProgress = 50;
            UpdateUI(CountProducePower(4));
            GC.CreateMessage(Text_DepName.text + " 发生了失误");
        }
    }

    public void EmpsGetExp(int type, int value = 250)
    {
        for(int i = 0; i < CurrentEmps.Count; i++)
        {
            CurrentEmps[i].GainExp(value, type);
        }
    }
    

    //旧人力资源部2功能 已过时
    public void StartSpecialBusiness(int type)
    {
        if(type == 1 && WorkStart == false && GC.Mentality >= 20)
        {
            WorkStart = true;
            GC.Mentality -= 20;
            UpdateUI();
        }
    }
}
