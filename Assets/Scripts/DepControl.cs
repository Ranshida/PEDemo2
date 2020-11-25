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
            TargetDep.produceBuffs.Remove(this);
            TargetDep.Efficiency -= Value;
        }
    }
}
public class HireType
{
    public int Level = 0, Type; //Type目前已经没什么用了
    public bool MajorSuccess = false;
    public int[] HST = new int[15];
    public HireType(int type)
    {
        Type = type;
    }

    //此处为原本的猎头相关功能
    //public void SetHeadHuntStatus(int[] Hst)
    public void SetHeadHuntStatus()
    {
        ////旧的招聘筛选
        //for(int i = 0; i < HeadHuntStatus.Length; i++)
        //{
        //    HST[i] = Hst[i];
        //    if (Hst[i] == 1)
        //        Level += 1;
        //}

        int r1 = Random.Range(0, 5), r2 = Random.Range(0, 5);
        while (r1 == r2)
        {
            r2 = Random.Range(0, 5);
        }
        int count1 = 4, count2 = 2;

        //HST = 0  (0,3)随机技能
        //HST = 1  (0,2)随机技能
        //HST = 2  (5,9)随机技能
        for (int i = 0; i < 6; i++)
        {
            if (i < 3)
                i = r1 * 3 + i;
            else
                i = r2 * 3 + i - 3;

            if (count1 > 0 && count2 > 0)
            {
                int c = Random.Range(1, 3);
                if (c == 1)
                {
                    HST[i] = 1;
                    count1 -= 1;
                }
                else
                {
                    HST[i] = 2;
                    count2 -= 1;
                }
            }
            else if (count1 > 0)
            {
                HST[i] = 1;
                count1 -= 1;
            }
            else
            {
                HST[i] = 2;
                count2 -= 1;
            }
        }
    }
}

public class DepControl : MonoBehaviour
{
    static public int StandardProducePoint = 50;
    [HideInInspector] public int SpProgress = 0, SpTotalProgress, FailProgress = 0, EfficiencyLevel = 0, SpType, SpTime;//SPTime:CEO技能
    [HideInInspector] public int EmpLimit, ProducePointLimit = 20, ActiveMode = 1, BuildingMode = 1;
    [HideInInspector] public bool SurveyStart = false, WorkStart = false; //WorkStart专门用于特殊业务
    public float Efficiency = 1.0f, ExtraSuccessRate = 0, ExtraMajorSuccessRate = 0;
    public bool canWork = false, EmpChanged = false;

    private bool MajorSuccess = false;

    [HideInInspector] public Research CurrentResearch;
    public Transform EmpContent, EmpPanel, LabPanel, SRateDetailPanel;
    public GameObject OfficeWarning;
    public Building building = null;
    public GameControl GC;
    public Task CurrentTask;
    public DepSelect DS;
    public OfficeControl CommandingOffice;
    public Text Text_DepName, Text_Task, Text_Progress, Text_Quality, Text_Time, Text_Office, Text_Efficiency, Text_LevelDownTime, Text_SRateDetail;
    public Button SurveyButton, ActiveButton;
    public EmpType type;

    public Research[] Researches = new Research[3];
    public Skill[] DSkillSetA = new Skill[6], DSkillSetB = new Skill[6], DSkillSetC = new Skill[6];
    public List<ProduceBuff> produceBuffs = new List<ProduceBuff>();
    public List<Employee> CurrentEmps = new List<Employee>();
    public List<OfficeControl> InRangeOffices = new List<OfficeControl>();
    public int[] DepHeadHuntStatus = new int[15];

    private void Update()
    {
        //if (type == EmpType.Science)
        //    Text_Efficiency.text = "效率:" + (Efficiency + GC.EfficiencyExtraScience) * 100 + "%";
        //else if (type != EmpType.HR)
        //    Text_Efficiency.text = "效率:" + (Efficiency + GC.EfficiencyExtraNormal) * 100 + "%";
        if (type != EmpType.HR)
        {
            Text_Efficiency.text = "动员等级:" + EfficiencyLevel;
        }
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
        CurrentTask = task;
        UpdateUI();
    }

    //目前不只是制作，还有很多别的跟事件相关的功能都写在这儿
    public void Produce()
    {
        if (canWork == true)
        {
            if (CurrentTask != null)
            {
                int Pp = CurrentEmps.Count;
                CurrentTask.Progress += Pp;
                if (CurrentTask.Progress >= ProducePointLimit)
                {
                    CurrentTask.Progress = 0;
                    float BaseSuccessRate = 0.5f + CountSuccessRate(1);
                    float Posb = Random.Range(0.0f, 1.0f);
                    //成功和大成功
                    if (Posb <= BaseSuccessRate)
                    {
                        if (Random.Range(0.0f, 1.0f) < 0.1f + ExtraMajorSuccessRate)
                        {
                            //大成功
                            GC.CreateMessage(Text_DepName.text + " 完美完成了 " + CurrentTask.TaskName + " 的生产");
                            GC.FinishedTask[(int)CurrentTask.TaskType * 3 + CurrentTask.Num - 1] += 2;
                            GC.UpdateResourceInfo();
                            EmpsGetExp(CurrentTask.Num, 100);
                            CurrentTask.Progress = 0;
                            if (CurrentTask.Num == 1 && (int)CurrentTask.TaskType == 2)
                                GC.StrC.SolveStrRequest(3, 2);
                            else if (CurrentTask.Num == 2 && (int)CurrentTask.TaskType == 1)
                                GC.StrC.SolveStrRequest(4, 2);
                            else if (CurrentTask.Num == 1 && (int)CurrentTask.TaskType == 0)
                                GC.StrC.SolveStrRequest(5, 2);
                        }
                        else
                        {
                            //成功
                            GC.CreateMessage(Text_DepName.text + " 完成了 " + CurrentTask.TaskName + " 的生产");
                            GC.FinishedTask[(int)CurrentTask.TaskType * 3 + CurrentTask.Num - 1] += 1;
                            GC.UpdateResourceInfo();
                            EmpsGetExp(CurrentTask.Num, 50);
                            CurrentTask.Progress = 0;
                            if (CurrentTask.Num == 1 && (int)CurrentTask.TaskType == 2)
                                GC.StrC.SolveStrRequest(3, 1);
                            else if (CurrentTask.Num == 2 && (int)CurrentTask.TaskType == 1)
                                GC.StrC.SolveStrRequest(4, 1);
                            else if (CurrentTask.Num == 1 && (int)CurrentTask.TaskType == 0)
                                GC.StrC.SolveStrRequest(5, 1);
                        }
                    }
                    //失败和大失败
                    else
                    {
                        EmpsGetExp(CurrentTask.Num, 50);
                        if (Random.Range(0.0f, 1.0f) < 0.4f)
                        {
                            //大失败
                            GC.CreateMessage(Text_DepName.text + " 生产 " + CurrentTask.TaskName + " 大失败，部门员工心力-20");
                            for (int i = 0; i < CurrentEmps.Count; i++)
                            {
                                CurrentEmps[i].Mentality -= 20;
                            }
                        }
                        else
                        {
                            //失败
                            GC.CreateMessage(Text_DepName.text + " 生产 " + CurrentTask.TaskName + " 失败");
                        }
                    }
                }
                UpdateUI();
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
                UpdateUI();
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
                UpdateUI();
                GC.StrC.SolveStrRequest(7, 1);
            }
            //大部分建筑的激活功能位置
            else if (WorkStart == true)
            {
                if (type == EmpType.HR)
                {
                    int Pp = CurrentEmps.Count;
                    SpProgress += Pp;
                    if (SpProgress >= ProducePointLimit)
                    {
                        SpProgress = 0;
                        WorkStart = false;
                        float BaseSuccessRate = 0.6f + CountSuccessRate(8);
                        float Posb = Random.Range(0.0f, 1.0f);
                        //成功和大成功
                        if (Posb <= BaseSuccessRate)
                        {
                            HireType ht = new HireType(SpType);
                            ht.SetHeadHuntStatus();

                            //此处的数组为原本的猎头相关功能
                            //ht.SetHeadHuntStatus(DepHeadHuntStatus);

                            //大成功
                            if (Random.Range(0.0f, 1.0f) < 0.1f)
                            {
                                ht.MajorSuccess = true;
                                EmpsGetExp(8, 100);
                                GC.CreateMessage(Text_DepName.text + " 招聘 大成功");
                            }
                            //成功
                            else
                            {
                                EmpsGetExp(8, 50);
                                GC.CreateMessage(Text_DepName.text + " 招聘 成功");
                            }

                            GC.HC.AddHireTypes(ht);
                        }
                        //失败和大失败
                        else
                        {
                            EmpsGetExp(8, 50);
                            if (Random.Range(0.0f, 1.0f) < 0.4f)
                            {
                                //大失败
                                for (int i = 0; i < CurrentEmps.Count; i++)
                                {
                                    CurrentEmps[i].Mentality -= 20;
                                    GC.CreateMessage(Text_DepName.text + " 招聘 大失败，部门员工心力-20");
                                }
                            }
                            else
                                GC.CreateMessage(Text_DepName.text + " 招聘 失败");
                        }
                    }
                    UpdateUI();
                }
                else if (SpProgress < ProducePointLimit)
                {
                    int Pp = CurrentEmps.Count;
                    SpProgress += Pp;
                    if (SpProgress >= ProducePointLimit)
                    {
                        SpProgress = 0;
                        WorkStart = false;
                        float BaseSuccessRate = 0.6f + CountSuccessRate(8);
                        float Posb = Random.Range(0.0f, 1.0f);
                        //成功和大成功
                        if (Posb <= BaseSuccessRate)
                        {
                            //大成功
                            if (Random.Range(0.0f, 1.0f) < 0.1f)
                            {
                                EmpsGetExp(building.effectValue, 5 * ProducePointLimit);
                                MajorSuccess = true;
                                GC.CreateMessage(Text_DepName.text + " 充能 大成功");
                            }
                            //成功
                            else
                            {
                                MajorSuccess = false;
                                GC.CreateMessage(Text_DepName.text + " 充能 成功");
                            }
                            ActiveButton.interactable = true;
                        }
                        //失败和大失败
                        else
                        {
                            MajorSuccess = false;
                            if (Random.Range(0.0f, 1.0f) < 0.4f)
                            {
                                //大失败
                                for (int i = 0; i < CurrentEmps.Count; i++)
                                {
                                    CurrentEmps[i].Mentality -= 20;
                                    GC.CreateMessage(Text_DepName.text + " 充能 大失败，部门员工心力-20");
                                }
                            }
                            else
                                GC.CreateMessage(Text_DepName.text + " 充能 失败");
                        }
                    }
                    UpdateUI();
                }
            }
        }
        if (SpTime > 0)
            SpTime -= 1;
    }

    public void UpdateUI()
    {
        if (type == EmpType.Science)
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
        else if (CurrentTask != null)
        {
            //旧的生产力结算
            //int Pp = CountProducePower((int)type + 1);
            int Pp = CurrentEmps.Count;
            Text_Task.text = "当前任务:" + CurrentTask.TaskName;
            Text_Progress.text = "生产力:" + Pp + "/时";

            //旧显示
            //Text_Quality.text = "当前进度:" + CurrentTask.Progress + "/" + StandardProducePoint * 10;
            //Text_Time.text = "剩余时间:" + calcTime(Pp, StandardProducePoint * 10 - CurrentTask.Progress) + "时";

            //成功率计算
            Text_Quality.text = "成功率:" + ((0.5f + CountSuccessRate(1)) * 100) + "%";

            Text_Time.text = "剩余时间:" + calcTime(Pp, ProducePointLimit - CurrentTask.Progress) + "时";
        }
        else if (WorkStart == true)
        {
            if (type == EmpType.HR)
            {
                int Pp = CurrentEmps.Count;
                Text_Task.text = "当前任务:招聘";
                Text_Progress.text = "生产力:" + Pp + "/时";
                Text_Quality.text = "成功率:" + ((0.6f + CountSuccessRate(8)) * 100) + "%";
                Text_Time.text = "剩余时间:" + calcTime(Pp, ProducePointLimit - SpProgress) + "时";
            }
        }
        else
        {
            Text_Task.text = "当前任务: 无";
            Text_Progress.text = "生产力:----";
            Text_Quality.text = "当前进度:----";
            Text_Time.text = "当前进度:----";
        }
    }

    //显示所有能当上级的办公室
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
            for (int i = 0; i < CurrentEmps.Count; i++)
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
        float FinalEfficiency = 1;
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
        UpdateUI();
    }

    public void RandomResearch()
    {
        for (int i = 0; i < 3; i++)
        {
            Researches[i].SetResearch(Random.Range(1, 15));
            Researches[i].GC = GC;
        }
    }

    public void ResearchExtra()
    {
        if (CurrentResearch != null)
        {
            CurrentResearch.Progress += CurrentResearch.ExtraProgress;
            CurrentResearch.SuccessRate += CurrentResearch.ExtraRate;
            CurrentResearch.UpdateUI();
            CurrentResearch.ExtraButton.gameObject.SetActive(false);
        }
    }

    public void FailCheck(bool ForceFail = false)
    {
        //float Posb = Random.Range(0.0f, 1.0f);
        //float totalObservation = 0;
        //for(int i = 0; i < CurrentEmps.Count; i++)
        //{
        //    totalObservation += CurrentEmps[i].Observation;
        //}
        //float ActualFailRate = FailRate - (totalObservation * GC.Morale * 0.0001f) - GC.ExtrafailRate;
        //if(Posb < ActualFailRate || ForceFail == true)
        //{
        //    Failed = true;
        //    FailProgress += Random.Range(50, 120);
        //    if (type != EmpType.Market && type != EmpType.Operate && type != EmpType.Product && type != EmpType.Tech)
        //        FailProgress = 50;
        //    UpdateUI(CountProducePower(4));
        //    GC.CreateMessage(Text_DepName.text + " 发生了失误");
        //}
    }

    public void EmpsGetExp(int type, int value = 250)
    {
        for (int i = 0; i < CurrentEmps.Count; i++)
        {
            CurrentEmps[i].GainExp(value, type);
        }
    }

    //删除建筑时重置所有相关数据
    public void ClearDep()
    {
        List<Employee> ED = new List<Employee>();
        for (int i = 0; i < CurrentEmps.Count; i++)
        {
            ED.Add(CurrentEmps[i]);
        }
        for (int i = 0; i < ED.Count; i++)
        {
            GC.CurrentEmpInfo = ED[i].InfoA;
            GC.CurrentEmpInfo.transform.parent = GC.StandbyContent;
            GC.ResetOldAssignment();
        }
        GC.CurrentDeps.Remove(this);
        if (CommandingOffice != null)
        {
            CommandingOffice.ControledDeps.Remove(this);
            CommandingOffice.CheckManage();
        }
        if (SRateDetailPanel != null)
            Destroy(SRateDetailPanel.gameObject);
        Destroy(DS.gameObject);
        Destroy(EmpPanel.gameObject);
        if (LabPanel != null)
            Destroy(LabPanel.gameObject);
        Destroy(this.gameObject);
    }

    //计算生产成功率
    float CountSuccessRate(int type)
    {
        //1-3职业技能 4观察 5坚韧 6强壮 7管理 8人力 9财务 10决策 11行业 12谋略 13说服 14魅力 15八卦
        float BaseSuccessRate = 0;
        Text_SRateDetail.text = "";
        //业务生产
        if (type < 4 && CurrentTask != null)
        {
            for (int i = 0; i < CurrentEmps.Count; i++)
            {
                int value = 0;
                float EValue = 0;
                if ((int)CurrentTask.TaskType == 0)
                    value = CurrentEmps[i].Skill1;
                else if ((int)CurrentTask.TaskType == 1)
                    value = CurrentEmps[i].Skill2;
                else
                    value = CurrentEmps[i].Skill3;
                if (value <= 5)
                    EValue -= 0.15f;
                else if (value <= 9)
                    EValue -= 0.05f;
                else if (value <= 13)
                    EValue += 0;
                else if (value <= 17)
                    EValue += 0.02f;
                else if (value <= 21)
                    EValue += 0.06f;
                else if (value > 21)
                    BaseSuccessRate += 0.1f;
                //文字显示
                Text_SRateDetail.text += CurrentEmps[i].Name + "技能:" + (EValue * 100) + "%";
                if (CurrentEmps[i].ExtraSuccessRate > 0.001f || CurrentEmps[i].ExtraSuccessRate < -0.001f)
                    Text_SRateDetail.text += " +" + (CurrentEmps[i].ExtraSuccessRate * 100) + "%";
                Text_SRateDetail.text += "\n";
                BaseSuccessRate += EValue;
                BaseSuccessRate += CurrentEmps[i].ExtraSuccessRate;
            }
            if (CommandingOffice != null && CommandingOffice.CurrentManager != null)
            {
                int value = 0;
                float EValue = 0;
                if ((int)CurrentTask.TaskType == 0)
                    value = CommandingOffice.CurrentManager.Skill1;
                else if ((int)CurrentTask.TaskType == 1)
                    value = CommandingOffice.CurrentManager.Skill2;
                else
                    value = CommandingOffice.CurrentManager.Skill3;
                if (value <= 5)
                    EValue -= 0.25f;
                else if (value <= 9)
                    EValue -= 0.15f;
                else if (value <= 13)
                    EValue -= 0.05f;
                else if (value <= 17)
                    EValue += 0;
                else if (value <= 21)
                    EValue += 0.05f;
                else if (value > 21)
                    EValue += 0.1f;
                BaseSuccessRate += EValue;
                BaseSuccessRate += CommandingOffice.CurrentManager.ExtraSuccessRate;
                //文字显示
                Text_SRateDetail.text += "高管" + CommandingOffice.CurrentManager.Name + "技能:" + (EValue * 100) + "%";
                if (CommandingOffice.CurrentManager.ExtraSuccessRate > 0.001f || CommandingOffice.CurrentManager.ExtraSuccessRate < -0.001f)
                    Text_SRateDetail.text += " +" + (CommandingOffice.CurrentManager.ExtraSuccessRate * 100) + "%";
            }
        }
        //招聘
        else
        {
            for (int i = 0; i < CurrentEmps.Count; i++)
            {
                int value = 0;
                if (type == 4)
                    value = CurrentEmps[i].Observation;
                else if (type == 5)
                    value = CurrentEmps[i].Tenacity;
                else if (type == 6)
                    value = CurrentEmps[i].Strength;
                else if (type == 7)
                    value = CurrentEmps[i].Manage;
                else if (type == 8)
                    value = CurrentEmps[i].HR;
                else if (type == 9)
                    value = CurrentEmps[i].Finance;
                else if (type == 10)
                    value = CurrentEmps[i].Decision;
                else if (type == 11)
                    value = CurrentEmps[i].Forecast;
                else if (type == 12)
                    value = CurrentEmps[i].Strategy;
                else if (type == 13)
                    value = CurrentEmps[i].Convince;
                else if (type == 14)
                    value = CurrentEmps[i].Charm;
                else if (type == 15)
                    value = CurrentEmps[i].Gossip;
                float EValue = 0;
                if (value <= 5)
                    EValue -= 0.15f;
                else if (value <= 9)
                    EValue -= 0.05f;
                else if (value <= 13)
                    EValue += 0;
                else if (value <= 17)
                    EValue += 0.02f;
                else if (value <= 21)
                    EValue += 0.06f;
                else if (value > 21)
                    EValue += 0.1f;
                BaseSuccessRate += EValue;
                BaseSuccessRate += CurrentEmps[i].ExtraSuccessRate;
                //文字显示
                Text_SRateDetail.text += CurrentEmps[i].Name + "技能:" + (EValue * 100) + "%";
                if (CurrentEmps[i].ExtraSuccessRate > 0.001f || CurrentEmps[i].ExtraSuccessRate < -0.001f)
                    Text_SRateDetail.text += " +" + (CurrentEmps[i].ExtraSuccessRate * 100) + "%";
                Text_SRateDetail.text += "\n";
                BaseSuccessRate += EValue;
                BaseSuccessRate += CurrentEmps[i].ExtraSuccessRate;
            }
            if (CommandingOffice != null && CommandingOffice.CurrentManager != null)
            {
                int value = 0;
                if (type == 4)
                    value = CommandingOffice.CurrentManager.Observation;
                else if (type == 5)
                    value = CommandingOffice.CurrentManager.Tenacity;
                else if (type == 6)
                    value = CommandingOffice.CurrentManager.Strength;
                else if (type == 7)
                    value = CommandingOffice.CurrentManager.Manage;
                else if (type == 8)
                    value = CommandingOffice.CurrentManager.HR;
                else if (type == 9)
                    value = CommandingOffice.CurrentManager.Finance;
                else if (type == 10)
                    value = CommandingOffice.CurrentManager.Decision;
                else if (type == 11)
                    value = CommandingOffice.CurrentManager.Forecast;
                else if (type == 12)
                    value = CommandingOffice.CurrentManager.Strategy;
                else if (type == 13)
                    value = CommandingOffice.CurrentManager.Convince;
                else if (type == 14)
                    value = CommandingOffice.CurrentManager.Charm;
                else if (type == 15)
                    value = CommandingOffice.CurrentManager.Gossip;
                float EValue = 0;
                if (value <= 5)
                    EValue -= 0.25f;
                else if (value <= 9)
                    EValue -= 0.15f;
                else if (value <= 13)
                    EValue -= 0.05f;
                else if (value <= 17)
                    EValue += 0;
                else if (value <= 21)
                    EValue += 0.05f;
                else if (value > 21)
                    EValue += 0.1f;

                BaseSuccessRate += EValue;
                BaseSuccessRate += CommandingOffice.CurrentManager.ExtraSuccessRate;
                //文字显示
                Text_SRateDetail.text += "高管" + CommandingOffice.CurrentManager.Name + "技能:" + (EValue * 100) + "%";
                if (CommandingOffice.CurrentManager.ExtraSuccessRate > 0.001f || CommandingOffice.CurrentManager.ExtraSuccessRate < -0.001f)
                    Text_SRateDetail.text += " +" + (CommandingOffice.CurrentManager.ExtraSuccessRate * 100) + "%";
            }
        }
        if (ExtraSuccessRate > 0)
            Text_SRateDetail.text += "\n动员效果:" + (ExtraSuccessRate * 100) + "%";
        if (Efficiency > 0)
            Text_SRateDetail.text += "\n额外效果:" + (Efficiency * 100) + "%";
        return BaseSuccessRate + ExtraSuccessRate + Efficiency;
    }

    //开始招聘工作
    public void StartHire()
    {
        //原本是StartTaskManage
        WorkStart = true;
        SpType = 0;
        UpdateUI();
    }

    public void ShowSRateDetailPanel()
    {
        if (type == EmpType.HR)
            CountSuccessRate(8);
        else
            CountSuccessRate(1);
        SRateDetailPanel.transform.position = Input.mousePosition;
        SRateDetailPanel.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(SRateDetailPanel.gameObject.GetComponent<RectTransform>());
    }

    public void CloseSRateDetailPanel()
    {
        SRateDetailPanel.gameObject.SetActive(false);
    }

    public void StartChangeMode()
    {

    }

    public void ChangeBuildingMode(int num)
    {

    }

    public void StartActive()
    {

    }

    public void ConfirmActive()
    {

    }

    public void BuildingActive()
    {
        if (building.Type == BuildingType.智库小组 && GC.SelectedDep != null)
        {
            if (MajorSuccess == true)
                new ProduceBuff(0.05f, GC.SelectedDep);
            new ProduceBuff(0.05f, GC.SelectedDep);
        }
        else if (building.Type == BuildingType.心理咨询室 && GC.CurrentEmpInfo != null)
        {
            if (BuildingMode == 1)
            {
                if (MajorSuccess == true)
                    GC.CurrentEmpInfo.emp.Mentality += (int)(GC.CurrentEmpInfo.emp.Mentality * 0.25f);
                else
                    GC.CurrentEmpInfo.emp.Mentality += (int)(GC.CurrentEmpInfo.emp.Mentality * 0.1f);
            }
            else if (BuildingMode == 2)
            {
                int value = 0;
                if (MajorSuccess == true)
                {
                    GC.CurrentEmpInfo.emp.Mentality += (int)(GC.CurrentEmpInfo.emp.Mentality * 0.15f);
                    value = 20;
                }
                else
                {
                    GC.CurrentEmpInfo.emp.Mentality += (int)(GC.CurrentEmpInfo.emp.Mentality * 0.05f);
                    value = 10;
                }
                for (int i = 0; i < CurrentEmps.Count; i++)
                {
                    CurrentEmps[i].ChangeRelation(GC.CurrentEmpInfo.emp, value);
                }
            }
        }
        else if (building.Type == BuildingType.财务部 && GC.SelectedDep != null)
        {
            if (BuildingMode == 1)
            {
                //新工资计算方式
            }
            else if (BuildingMode == 2)
            {
                GC.CurrentEmployees[0].InfoDetail.AddPerk(new Perk34(GC.CurrentEmployees[0]), true);
                if (MajorSuccess == true)
                    GC.CurrentEmployees[0].InfoDetail.AddPerk(new Perk34(GC.CurrentEmployees[0]), true);
            }
        }
        else if (building.Type == BuildingType.体能研究室 && GC.SelectedDep != null)
        {
            if (BuildingMode == 1)
            {
                for (int i = 0; i < GC.SelectedDep.CurrentEmps.Count; i++)
                {
                    GC.SelectedDep.CurrentEmps[i].InfoDetail.AddPerk(new Perk35(GC.SelectedDep.CurrentEmps[i]), true);
                    GC.SelectedDep.CurrentEmps[i].InfoDetail.AddPerk(new Perk35(GC.SelectedDep.CurrentEmps[i]), true);
                    GC.SelectedDep.CurrentEmps[i].InfoDetail.AddPerk(new Perk36(GC.SelectedDep.CurrentEmps[i]));
                    if (MajorSuccess == true)
                    {
                        GC.SelectedDep.CurrentEmps[i].InfoDetail.AddPerk(new Perk35(GC.SelectedDep.CurrentEmps[i]), true);
                        GC.SelectedDep.CurrentEmps[i].InfoDetail.AddPerk(new Perk36(GC.SelectedDep.CurrentEmps[i]));
                    }
                }
            }
            else if (BuildingMode == 2)
            {
                for (int i = 0; i < GC.SelectedDep.CurrentEmps.Count; i++)
                {
                    GC.SelectedDep.CurrentEmps[i].InfoDetail.AddPerk(new Perk35(GC.SelectedDep.CurrentEmps[i]), true);
                    if (MajorSuccess == true)
                        GC.SelectedDep.CurrentEmps[i].InfoDetail.AddPerk(new Perk35(GC.SelectedDep.CurrentEmps[i]), true);
                }
            }
        }
        else if (building.Type == BuildingType.按摩房 && GC.CurrentEmpInfo != null)
        {
            if (BuildingMode == 1)
            {
                if (MajorSuccess == true)
                    GC.CurrentEmpInfo.emp.Mentality += (int)(GC.CurrentEmpInfo.emp.Stamina * 0.25f);
                else
                    GC.CurrentEmpInfo.emp.Mentality += (int)(GC.CurrentEmpInfo.emp.Stamina * 0.1f);
            }
            else if (BuildingMode == 2)
            {
                if (MajorSuccess == true)
                    GC.CurrentEmpInfo.emp.Mentality += (int)(GC.CurrentEmpInfo.emp.Stamina * 0.15f);
                else
                    GC.CurrentEmpInfo.emp.Mentality += (int)(GC.CurrentEmpInfo.emp.Stamina * 0.05f);
                for (int i = 0; i < CurrentEmps.Count; i++)
                {
                    CurrentEmps[i].ChangeRelation(GC.CurrentEmpInfo.emp, 10);
                }
            }
        }
        else if (building.Type == BuildingType.健身房 && GC.CurrentEmpInfo != null)
        {
            if (BuildingMode == 1)
            {
                if (MajorSuccess == true)
                {
                    GC.CurrentEmpInfo.emp.Mentality += (int)(GC.CurrentEmpInfo.emp.Stamina * 0.15f);
                    if (Random.Range(0.0f, 1.0f) < 0.4f)
                        GC.CurrentEmpInfo.DetailInfo.AddPerk(new Perk35(GC.CurrentEmpInfo.emp), true);
                }
                else
                    GC.CurrentEmpInfo.emp.Mentality += (int)(GC.CurrentEmpInfo.emp.Stamina * 0.05f);
            }
            else if (BuildingMode == 2)
            {
                if (MajorSuccess == true)
                {
                    GC.CurrentEmpInfo.emp.Mentality += (int)(GC.CurrentEmpInfo.emp.Stamina * 0.15f);
                    if (Random.Range(0.0f, 1.0f) < 0.4f)
                        GC.CurrentEmpInfo.DetailInfo.AddPerk(new Perk37(GC.CurrentEmpInfo.emp), true);
                }
                else
                    GC.CurrentEmpInfo.emp.Mentality += (int)(GC.CurrentEmpInfo.emp.Stamina * 0.05f);
            }
        }
        else if (building.Type == BuildingType.宣传中心 && GC.CurrentEmpInfo != null)
        {
            GC.CurrentEmpInfo.emp.AddEmotion(EColor.DRed);
            if (MajorSuccess == true)
                GC.CurrentEmpInfo.emp.AddEmotion(EColor.DRed);
        }
        else if (building.Type == BuildingType.科技工作坊 && GC.CurrentEmpInfo != null)
        {
            GC.CurrentEmpInfo.emp.AddEmotion(EColor.DYellow);
            if (MajorSuccess == true)
                GC.CurrentEmpInfo.emp.AddEmotion(EColor.DYellow);
        }
        else if (building.Type == BuildingType.绩效考评中心 && GC.CurrentEmpInfo != null)
        {
            if (BuildingMode == 1)
            {
                GC.CurrentEmpInfo.emp.AddEmotion(EColor.Blue);
                if (MajorSuccess == true)
                {
                    GC.CurrentEmpInfo.emp.AddEmotion(EColor.Blue);
                    if (Random.Range(0.0f, 1.0f) < 0.8f)
                        GC.CurrentEmpInfo.DetailInfo.AddPerk(new Perk38(GC.CurrentEmpInfo.emp), true);
                }
            }
            else if (BuildingMode == 2)
            {
                GC.CurrentEmpInfo.emp.AddEmotion(EColor.DYellow);
                GC.CurrentEmpInfo.emp.AddEmotion(EColor.DYellow);
                if (MajorSuccess == true)
                {
                    GC.CurrentEmpInfo.emp.AddEmotion(EColor.DYellow);
                    GC.CurrentEmpInfo.emp.AddEmotion(EColor.DYellow);
                }
            }
        }
        else if (building.Type == BuildingType.员工休息室 && GC.CurrentEmpInfo != null)
        {
            GC.CurrentEmpInfo.emp.AddEmotion(EColor.Yellow);
            if (MajorSuccess == true)
                GC.CurrentEmpInfo.emp.AddEmotion(EColor.Yellow);
        }
        else if (building.Type == BuildingType.人文沙龙 && GC.SelectedDep != null)
        {
            int ValueA = 0, ValueB = 0;
            if(BuildingMode == 1)
            {
                if (MajorSuccess == true)
                    ValueA = 30;
                else
                    ValueA = 15;
            }
            else if (BuildingMode == 2)
            {
                if (MajorSuccess == true)
                {
                    ValueA = 20;
                    ValueB = -10;
                }
                else
                {
                    ValueA = 10;
                    ValueB = -5;
                }
            }
            for(int i = 0; i < building.effect.AffectedBuildings.Count; i++)
            {
                if(building.effect.AffectedBuildings[i].Department != null)
                {
                    for(int j = 0; j < building.effect.AffectedBuildings[i].Department.CurrentEmps.Count; j++)
                    {
                        building.effect.AffectedBuildings[i].Department.CurrentEmps[j].ChangeCharacter(1, ValueA);
                        building.effect.AffectedBuildings[i].Department.CurrentEmps[j].ChangeCharacter(0, ValueB);
                    }
                }
                else if (building.effect.AffectedBuildings[i].Office != null && building.effect.AffectedBuildings[i].Office.CurrentManager != null)
                {
                    building.effect.AffectedBuildings[i].Office.CurrentManager.ChangeCharacter(1, ValueA);
                    building.effect.AffectedBuildings[i].Office.CurrentManager.ChangeCharacter(0, ValueB);
                }
            }
        }
        else if (building.Type == BuildingType.兴趣社团 && GC.SelectedDep != null)
        {
            int Count = 0;
            for(int i = 0; i < CurrentEmps.Count; i++)
            {
                for(int j = 0; j < GC.SelectedDep.CurrentEmps.Count; j++)
                {
                    Relation R = CurrentEmps[i].FindRelation(GC.SelectedDep.CurrentEmps[j]);
                    if (R.FriendValue >= 1 || R.LoveValue > 2 || R.MasterValue > 0)
                        Count += 1;
                }
            }
            float value = 0.02f;
            if (MajorSuccess == true)
                value += 0.01f;
            new ProduceBuff(value * Count, GC.SelectedDep);
        }
        else if (building.Type == BuildingType.电子科技展 && GC.SelectedDep != null)
        {
            int ValueA = 0, ValueB = 0;
            if (BuildingMode == 1)
            {
                if (MajorSuccess == true)
                    ValueA = -30;
                else
                    ValueA = -15;
            }
            else if (BuildingMode == 2)
            {
                if (MajorSuccess == true)
                {
                    ValueA = -20;
                    ValueB = -10;
                }
                else
                {
                    ValueA = -10;
                    ValueB = -5;
                }
            }
            for (int i = 0; i < building.effect.AffectedBuildings.Count; i++)
            {
                if (building.effect.AffectedBuildings[i].Department != null)
                {
                    for (int j = 0; j < building.effect.AffectedBuildings[i].Department.CurrentEmps.Count; j++)
                    {
                        building.effect.AffectedBuildings[i].Department.CurrentEmps[j].ChangeCharacter(1, ValueA);
                        building.effect.AffectedBuildings[i].Department.CurrentEmps[j].ChangeCharacter(0, ValueB);
                    }
                }
                else if (building.effect.AffectedBuildings[i].Office != null && building.effect.AffectedBuildings[i].Office.CurrentManager != null)
                {
                    building.effect.AffectedBuildings[i].Office.CurrentManager.ChangeCharacter(1, ValueA);
                    building.effect.AffectedBuildings[i].Office.CurrentManager.ChangeCharacter(0, ValueB);
                }
            }

        }
        else if (building.Type == BuildingType.冥想室 && GC.CurrentEmpInfo != null)
        {
            if (BuildingMode == 1)
            {
                for (int i = 0; i < 3; i++)
                {
                    GC.CurrentEmpInfo.emp.RemoveEmotion(EColor.DGreen);
                }
                int count = 3;
                if (MajorSuccess == true)
                    count = 5;
                for (int i = 0; i < count; i++)
                {
                    GC.CurrentEmpInfo.DetailInfo.AddPerk(new Perk3(GC.CurrentEmpInfo.emp), true);
                }
            }
            else if (BuildingMode == 2)
            {
                GC.CurrentEmpInfo.emp.RemoveEmotion(EColor.DGreen);
                GC.CurrentEmpInfo.DetailInfo.AddPerk(new Perk3(GC.CurrentEmpInfo.emp), true);
                if (MajorSuccess == true)
                    GC.CurrentEmpInfo.DetailInfo.AddPerk(new Perk3(GC.CurrentEmpInfo.emp), true);
            }
        }
        else if (building.Type == BuildingType.特别秘书处 && GC.CurrentEmpInfo != null)
        {
            if (BuildingMode == 1)
            {
                GC.CurrentEmpInfo.emp.RemoveEmotion(EColor.DRed);
                GC.CurrentEmpInfo.emp.RemoveEmotion(EColor.DRed);
                GC.CurrentEmpInfo.emp.RemoveEmotion(EColor.Red);
                GC.CurrentEmpInfo.emp.RemoveEmotion(EColor.Red);
            }
            else if (BuildingMode == 2)
            {
                GC.CurrentEmpInfo.emp.RemoveEmotion(EColor.DBlue);
                GC.CurrentEmpInfo.emp.RemoveEmotion(EColor.DBlue);
                GC.CurrentEmpInfo.emp.RemoveEmotion(EColor.Blue);
                GC.CurrentEmpInfo.emp.RemoveEmotion(EColor.Blue);
            }
            GC.CurrentEmpInfo.DetailInfo.AddPerk(new Perk39(GC.CurrentEmpInfo.emp));
        }
        else if (building.Type == BuildingType.成人再教育所 && GC.CurrentEmpInfo != null)
        {
            if (BuildingMode == 1)
            {
                if (MajorSuccess == true)
                    GC.CurrentEmpInfo.emp.ChangeCharacter(0, -20);
                else
                    GC.CurrentEmpInfo.emp.ChangeCharacter(0, -10);
            }
            else if (BuildingMode == 2)
            {
                if (MajorSuccess == true)
                    GC.CurrentEmpInfo.emp.Mentality -= 10;
                else
                    GC.CurrentEmpInfo.emp.Mentality -= 20;
            }
            GC.CurrentEmpInfo.DetailInfo.AddPerk(new Perk40(GC.CurrentEmpInfo.emp));
        }
        //
        else if (building.Type == BuildingType.职业再发展中心 && GC.CurrentEmpInfo != null)
        {
           
        }
        else if (building.Type == BuildingType.中央监控室 && GC.SelectedDep != null)
        {
            int Count = 5;
            if (MajorSuccess == true)
                Count = 10;
            for (int i = 0; i < building.effect.AffectedBuildings.Count; i++)
            {
                if (building.effect.AffectedBuildings[i].Department != null)
                {
                    for (int j = 0; j < building.effect.AffectedBuildings[i].Department.CurrentEmps.Count; j++)
                    {
                        if (MajorSuccess == false)
                        {
                            List<PerkInfo> N = new List<PerkInfo>();
                            for (int k = 0; k < building.effect.AffectedBuildings[i].Department.CurrentEmps[j].InfoDetail.PerksInfo.Count; k++)
                            {
                                if (building.effect.AffectedBuildings[i].Department.CurrentEmps[j].InfoDetail.PerksInfo[k].CurrentPerk.Num == 3)
                                    N.Add(building.effect.AffectedBuildings[i].Department.CurrentEmps[j].InfoDetail.PerksInfo[k]);
                            }
                            for(int k = 0; k < N.Count; k++)
                            {
                                N[k].RemovePerk();
                            }
                        }
                        for (int k = 0; k < Count; k++)
                        {
                            building.effect.AffectedBuildings[i].Department.CurrentEmps[j].InfoDetail.AddPerk(new Perk36(building.effect.AffectedBuildings[i].Department.CurrentEmps[j]));
                        }
                    }
                }
                else if (building.effect.AffectedBuildings[i].Office != null && building.effect.AffectedBuildings[i].Office.CurrentManager != null)
                {
                    if (MajorSuccess == false)
                    {
                        List<PerkInfo> N = new List<PerkInfo>();
                        for (int k = 0; k < building.effect.AffectedBuildings[i].Office.CurrentManager.InfoDetail.PerksInfo.Count; k++)
                        {
                            if (building.effect.AffectedBuildings[i].Office.CurrentManager.InfoDetail.PerksInfo[k].CurrentPerk.Num == 3)
                                N.Add(building.effect.AffectedBuildings[i].Office.CurrentManager.InfoDetail.PerksInfo[k]);
                        }
                        for (int k = 0; k < N.Count; k++)
                        {
                            N[k].RemovePerk();
                        }
                    }
                    for (int k = 0; k < Count; k++)
                    {
                        building.effect.AffectedBuildings[i].Office.CurrentManager.InfoDetail.AddPerk(new Perk36(building.effect.AffectedBuildings[i].Office.CurrentManager));
                    }
                }
            }
        }
        else if (building.Type == BuildingType.谍战中心 && GC.CurrentEmpInfo != null)
        {


        }

        GC.SelectedDep = null;
        GC.CurrentDep = null;
        GC.CurrentEmpInfo = null;
    }
}
