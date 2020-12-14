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
    [HideInInspector] public int SpProgress = 0, SpTotalProgress, FailProgress = 0, EfficiencyLevel = 0, SpType;
    //BuildingMode用于区分建筑模式  ActiveMode用于区分激活方式 1直接激活 2选择员工 3选择部门
    [HideInInspector] public int EmpLimit, ProducePointLimit = 20, ActiveMode = 1, BuildingMode = 1;
    [HideInInspector] public bool SurveyStart = false;
    public float Efficiency = 0, SalaryMultiply = 1.0f;
    public bool canWork = false;

    private bool MajorSuccess = false;

    public Transform EmpContent, EmpPanel, LabPanel, SRateDetailPanel;
    public GameObject OfficeWarning;
    public Building building = null;
    public GameControl GC;
    public Task CurrentTask;
    public DepSelect DS;
    public OfficeControl CommandingOffice;
    public Text Text_DepName, Text_Task, Text_Progress, Text_Quality, Text_Time, Text_Office, Text_Efficiency, Text_LevelDownTime, Text_SRateDetail;
    public Button SurveyButton, ActiveButton, ModeChangeButton;
    public EmpType type;

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
            //基础经验获取
            EmpsGetExp(building.effectValue, 5);
            //进度增加，部分建筑资源不足时暂停
            int Pp = 0;
            foreach(Employee e in CurrentEmps)
            {
                if (e.InfoDetail.Entity.OutCompany == false)
                    Pp++;
            }
            if (building.Type == BuildingType.技术部门)
            {
                if (GC.FinishedTask[6] > 0)
                    SpProgress += Pp;
            }
            else if (building.Type == BuildingType.公关营销部)
            {
                if (GC.FinishedTask[4] > 4)
                    SpProgress += Pp;
            }
            else
                SpProgress += Pp;

            int limit = ProducePointLimit;
            //研发部门额外Buff
            if (building.Type == BuildingType.研发部门)
                limit -= GC.ResearchExtraTimeBoost;

            if (SpProgress >= limit)
            {
                //完成生产
                float BaseSuccessRate = 0.5f + CountSuccessRate(building.effectValue);
                float Posb = Random.Range(0.0f, 1.0f);
                //成功和大成功
                if (Posb <= BaseSuccessRate)
                {
                    if (Random.Range(0.0f, 1.0f) < 0.2f + GC.SC.ExtraMajorSuccessRate)
                    {
                        //大成功
                        //额外经验获取
                        EmpsGetExp(building.effectValue, 5 * ProducePointLimit / CurrentEmps.Count);
                        //分部门效果
                        if (building.Type == BuildingType.技术部门)
                        {
                            GC.FinishedTask[0] += 2;
                            GC.FinishedTask[6] -= 1;
                            SpProgress = 0;
                            GC.CreateMessage(Text_DepName.text + " 完美完成了程序迭代的生产");
                        }
                        else if (building.Type == BuildingType.市场部门)
                        {
                            GC.FinishedTask[4] += 2;
                            SpProgress = 0;
                            GC.CreateMessage(Text_DepName.text + " 完美完成了营销文案的生产");
                        }
                        else if (building.Type == BuildingType.产品部门)
                        {
                            GC.FinishedTask[6] += 2;
                            SpProgress = 0;
                            GC.CreateMessage(Text_DepName.text + " 完美完成了原型图的生产");
                        }
                        //其他需要激活的建筑
                        else
                        {
                            MajorSuccess = true;
                            ActiveButton.interactable = true;
                            GC.CreateMessage(Text_DepName.text + " 已完美充能");
                        }
                        GC.UpdateResourceInfo();
                    }
                    else
                    {
                        //成功
                        if (building.Type == BuildingType.技术部门)
                        {
                            GC.FinishedTask[0] += 1;
                            GC.FinishedTask[6] -= 1;
                            SpProgress = 0;
                            GC.CreateMessage(Text_DepName.text + " 完成了程序迭代的生产");
                        }
                        else if (building.Type == BuildingType.市场部门)
                        {
                            GC.FinishedTask[4] += 1;
                            SpProgress = 0;
                            GC.CreateMessage(Text_DepName.text + " 完成了营销文案的生产");
                        }
                        else if (building.Type == BuildingType.产品部门)
                        {
                            GC.FinishedTask[6] += 1;
                            SpProgress = 0;
                            GC.CreateMessage(Text_DepName.text + " 完成了原型图的生产");
                        }
                        //其他需要激活的建筑
                        else
                        {
                            MajorSuccess = false;
                            ActiveButton.interactable = true;
                            GC.CreateMessage(Text_DepName.text + " 已成功充能");
                        }
                        GC.UpdateResourceInfo();                       
                    }

                }
                //失败和大失败
                else
                {
                    SpProgress = 0;
                    if (Random.Range(0.0f, 1.0f) < 0.5f + GC.SC.ExtraMajorFailureRate)
                    {
                        //大失败
                        GC.CreateMessage(Text_DepName.text + " 工作中发生重大失误,部门员工心力-20");
                        for (int i = 0; i < CurrentEmps.Count; i++)
                        {
                            CurrentEmps[i].Mentality -= 20;
                        }
                    }
                    else
                    {
                        //失败
                        GC.CreateMessage(Text_DepName.text + " 工作失败");
                    }
                }
            }

            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        if (canWork == true)
        {
            //旧的生产力结算
            //int Pp = CountProducePower((int)type + 1);
            int Pp = 0;
            foreach (Employee e in CurrentEmps)
            {
                if (e.InfoDetail != null && e.InfoDetail.Entity != null)
                {
                    if (e.InfoDetail.Entity.OutCompany == false)
                        Pp++;
                }
            }
            if (building.Type == BuildingType.技术部门 || building.Type == BuildingType.市场部门 || building.Type == BuildingType.产品部门)
                Text_Task.text = "当前任务:基础资源生产";
            else if (ActiveMode == 1)
                Text_Task.text = "当前任务:" + building.Function_A;
            else if (ActiveMode == 2)
                Text_Task.text = "当前任务:" + building.Function_B;
            Text_Progress.text = "生产力:" + Pp + "/时";
            //成功率计算
            Text_Quality.text = "成功率:" + ((0.5f + CountSuccessRate(building.effectValue)) * 100) + "%";

            int limit = ProducePointLimit;
            //研发部门额外Buff
            if (building.Type == BuildingType.研发部门)
                limit -= GC.ResearchExtraTimeBoost;
            Text_Time.text = "剩余时间:" + calcTime(Pp, limit - SpProgress) + "时";
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
        if (type < 4)
        {
            for (int i = 0; i < CurrentEmps.Count; i++)
            {
                int value = 0;
                float EValue = 0;
                if (building.effectValue == 1)
                    value = CurrentEmps[i].Skill1;
                else if (building.effectValue == 2)
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
                if (building.effectValue == 1)
                    value = CommandingOffice.CurrentManager.Skill1;
                else if (building.effectValue == 2)
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
        if (GC.SC.ExtraSuccessRate > 0.001f)
            Text_SRateDetail.text += "\n动员效果:" + (GC.SC.ExtraSuccessRate * 100) + "%";
        if (Efficiency > 0.001f)
            Text_SRateDetail.text += "\n额外效果:" + (Efficiency * 100) + "%";
        if (building.Type == BuildingType.人力资源部 && GC.HireSuccessExtra > 0.001f)
        {
            Text_SRateDetail.text += "\n额外招聘成功率:" + (GC.HireSuccessExtra * 100) + "%";
            BaseSuccessRate += GC.HireSuccessExtra;
        }
        if (GC.BaseDepExtraSuccessRate > 0.001f)
        {
            if (building.Type == BuildingType.技术部门 || building.Type == BuildingType.市场部门 || building.Type == BuildingType.产品部门 || building.Type == BuildingType.公关营销部)
            {
                Text_SRateDetail.text += "\n额外业务成功率:" + (GC.BaseDepExtraSuccessRate * 100) + "%";
                BaseSuccessRate += GC.BaseDepExtraSuccessRate;
            }
        }
        if(GC.ResearchExtraSuccessRate > 0.001f && building.Type == BuildingType.研发部门)
        {
            Text_SRateDetail.text += "\n额外研发成功率:" + (GC.ResearchExtraSuccessRate * 100) + "%";
            BaseSuccessRate += GC.ResearchExtraSuccessRate;
        }
        return BaseSuccessRate + GC.SC.ExtraSuccessRate + Efficiency;
    }

    public void ShowSRateDetailPanel()
    {
        if (type == EmpType.HR)
            CountSuccessRate(8);
        else
            CountSuccessRate(building.effectValue);
        SRateDetailPanel.transform.position = Input.mousePosition;
        SRateDetailPanel.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(SRateDetailPanel.gameObject.GetComponent<RectTransform>());
    }

    public void CloseSRateDetailPanel()
    {
        SRateDetailPanel.gameObject.SetActive(false);
    }

    //展开部门模式改变面板
    public void StartChangeMode()
    {
        GC.DepModeSelectPanel.SetActive(true);
        GC.CurrentDep = this;
        GC.Text_DepMode1.text = building.Description_A;
        GC.Text_DepMode2.text = building.Description_B;
    }

    //部门模式改变
    public void ChangeBuildingMode(int num)
    {
        ActiveMode = num;
        SpProgress = 0;
        ActiveButton.interactable = false;
        GC.DepModeSelectPanel.SetActive(false);
        UpdateUI();
    }

    //展开确认面板并更新内容
    public void StartActive()
    {
        GC.DepSkillConfirmPanel.SetActive(true);
    }

    //可执行检测+后续面板展开
    public void ConfirmActive()
    {
        //额外的消耗检测，不满足条件不继续
        if(building.Type == BuildingType.公关营销部)
        {
            if (ActiveMode == 1 && GC.FinishedTask[4] < 5)
                return;
            else if (ActiveMode == 2 && (GC.FinishedTask[4] < 1 || GC.Money < 300))
                return;
        }

        //关闭面板
        GC.DepSkillConfirmPanel.SetActive(false);
        if (ActiveMode == 1)
        {
            BuildingActive();
            return;
        }
        else if (ActiveMode == 2)
        {
            GC.CurrentDep = this;
            GC.SelectMode = 5;
            GC.TotalEmpContent.parent.parent.gameObject.SetActive(true);
        }
        else if (ActiveMode == 3)
        {
            GC.CurrentDep = this;
            GC.SelectMode = 5;
            GC.ShowDepSelectPanel(AvailableDeps());
        }
        if (building.Type == BuildingType.按摩房 || building.Type == BuildingType.健身房 || building.Type == BuildingType.宣传中心 ||
            building.Type == BuildingType.科技工作坊 || building.Type == BuildingType.绩效考评中心 || building.Type == BuildingType.心理咨询室
            || building.Type == BuildingType.员工休息室 || building.Type == BuildingType.成人再教育所)
        {
            ShowAvailableEmps();
        }
        else if (building.Type == BuildingType.冥想室)
        {
            List<DepControl> Deps = AvailableDeps();
            List<Employee> Emps = new List<Employee>();
            for (int i = 0; i < Deps.Count; i++)
            {
                for (int j = 0; j < Deps[i].CurrentEmps.Count; j++)
                {
                    Emps.Add(Deps[i].CurrentEmps[j]);
                }
            }
            int count = 3;
            if (ActiveMode == 2)
                count = 1;
            for (int i = 0; i < GC.CurrentEmployees.Count; i++)
            {
                if (Emps.Contains(GC.CurrentEmployees[i]) == true)
                {
                    for (int j = 0; j < Emps[i].CurrentEmotions.Count; j++)
                    {
                        if (Emps[i].CurrentEmotions[j].color == EColor.DGreen && Emps[i].CurrentEmotions[j].Level >= count)
                        {
                            Emps[i].InfoB.gameObject.SetActive(true);
                            break;
                        }
                    }
                }
                else
                    Emps[i].InfoB.gameObject.SetActive(false);
            }
        }
        else if (building.Type == BuildingType.特别秘书处)
        {
            List<DepControl> Deps = AvailableDeps();
            List<Employee> Emps = new List<Employee>();
            for (int i = 0; i < Deps.Count; i++)
            {
                for (int j = 0; j < Deps[i].CurrentEmps.Count; j++)
                {
                    Emps.Add(Deps[i].CurrentEmps[j]);
                }
            }
            EColor ColorA = EColor.DRed, ColorB = EColor.Red;
            if (ActiveMode == 2)
            {
                ColorA = EColor.DBlue;
                ColorB = EColor.Blue;
            }
            for (int i = 0; i < GC.CurrentEmployees.Count; i++)
            {
                if (Emps.Contains(GC.CurrentEmployees[i]) == true)
                {
                    int num = 0;
                    for (int j = 0; j < Emps[i].CurrentEmotions.Count; j++)
                    {
                        if ((Emps[i].CurrentEmotions[j].color == ColorA || Emps[i].CurrentEmotions[j].color == ColorB) && Emps[i].CurrentEmotions[j].Level >= 2)
                            num += 1;
                        if (num == 2)
                        {
                            Emps[i].InfoB.gameObject.SetActive(true);
                            break;
                        }
                    }
                }
                else
                    Emps[i].InfoB.gameObject.SetActive(false);
            }
        }
        else if (building.Type == BuildingType.职业再发展中心)
        {
            List<DepControl> Deps = AvailableDeps();
            List<Employee> Emps = new List<Employee>();
            for (int i = 0; i < Deps.Count; i++)
            {
                for (int j = 0; j < Deps[i].CurrentEmps.Count; j++)
                {
                    Emps.Add(Deps[i].CurrentEmps[j]);
                }
            }
            int count = 3;
            if (ActiveMode == 2)
                count = 1;
            for (int i = 0; i < GC.CurrentEmployees.Count; i++)
            {
                if (Emps.Contains(GC.CurrentEmployees[i]) == true)
                {
                    for (int j = 0; j < Emps[i].CurrentEmotions.Count; j++)
                    {
                        if (Emps[i].CurrentEmotions[j].color == EColor.DGreen && Emps[i].CurrentEmotions[j].Level >= count)
                        {
                            Emps[i].InfoB.gameObject.SetActive(true);
                            break;
                        }
                    }
                }
                else
                    Emps[i].InfoB.gameObject.SetActive(false);
            }
        }
        else if (building.Type == BuildingType.谍战中心 && GC.CurrentEmpInfo != null)
        {
            if (ActiveMode == 1)
            {
                List<DepControl> Deps = AvailableDeps();
                List<Employee> Emps = new List<Employee>();
                for (int i = 0; i < Deps.Count; i++)
                {
                    for (int j = 0; j < Deps[i].CurrentEmps.Count; j++)
                    {
                        Emps.Add(Deps[i].CurrentEmps[j]);
                    }
                }
                for (int i = 0; i < GC.CurrentEmployees.Count; i++)
                {
                    if (Emps.Contains(GC.CurrentEmployees[i]) == true)
                    {
                        for (int j = 0; j < Emps[i].InfoDetail.PerksInfo.Count; j++)
                        {
                            if (Emps[i].InfoDetail.PerksInfo[j].CurrentPerk.Num == 39 || Emps[i].InfoDetail.PerksInfo[j].CurrentPerk.Num == 41)
                            {
                                Emps[i].InfoB.gameObject.SetActive(true);
                                break;
                            }
                        }
                    }
                    else
                        Emps[i].InfoB.gameObject.SetActive(false);
                }
            }
            else
                ShowAvailableEmps();
        }
    }

    public void BuildingActive()
    {
        if (building.Type == BuildingType.智库小组 && GC.SelectedDep != null)
        {
            int PTime = 32;
            if (GC.ProduceBuffBonus == true)
                PTime = 128;
            if (MajorSuccess == true)
                new ProduceBuff(0.05f, GC.SelectedDep, PTime);
            new ProduceBuff(0.05f, GC.SelectedDep, PTime);
        }
        else if (building.Type == BuildingType.人力资源部)
        {
            HireType ht = new HireType(0);
            ht.SetHeadHuntStatus();
            if (MajorSuccess == true)
                ht.MajorSuccess = true;
            GC.HC.AddHireTypes(ht);
        }
        else if (building.Type == BuildingType.公关营销部)
        {
            if(BuildingMode == 1)
            {
                GC.FinishedTask[4] -= 5;
                if (MajorSuccess == true)
                    GC.CreateMessage("获得了2个传播");
                else
                    GC.CreateMessage("获得了1个传播");
            }
            else if (BuildingMode == 2)
            {
                GC.Money -= 300;
                GC.FinishedTask[4] -= 1;
                if (MajorSuccess == true)
                    GC.CreateMessage("获得了2个传播");
                else
                    GC.CreateMessage("获得了1个传播");
            }
        }
        else if (building.Type == BuildingType.研发部门)
        {
            if (MajorSuccess == true)
                GC.FinishedTask[7] += 2;
            else if (MajorSuccess == false)
                GC.FinishedTask[7] += 1;
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
            float BuffPosb = 0.4f;
            if (GC.GymBuffBonus == true)
                BuffPosb = 0.8f;
            if (BuildingMode == 1)
            {
                if (MajorSuccess == true)
                {
                    GC.CurrentEmpInfo.emp.Mentality += (int)(GC.CurrentEmpInfo.emp.Stamina * 0.15f);
                    if (Random.Range(0.0f, 1.0f) < BuffPosb)
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
                    if (Random.Range(0.0f, 1.0f) < BuffPosb)
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
        else if (building.Type == BuildingType.人文沙龙)
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
        else if (building.Type == BuildingType.兴趣社团)
        {
            int Count = 0, PTime = 32;
            if (GC.ProduceBuffBonus == true)
                PTime = 128;
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
            new ProduceBuff(value * Count, GC.SelectedDep, PTime);
        }
        else if (building.Type == BuildingType.电子科技展)
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
            if (MajorSuccess == false)
                GC.CurrentEmpInfo.emp.InfoDetail.ST.ChangeSkillTree();
            else
                GC.SkillTreeSelectPanel.SetActive(true);
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
            GC.foeControl.ShowSpyPanel();
            if (MajorSuccess == false)
                GC.CurrentEmpInfo.emp.Mentality -= 30;
        }

        GC.SelectedDep = null;
        GC.CurrentDep = null;
        GC.CurrentEmpInfo = null;
        GC.TotalEmpContent.parent.parent.gameObject.SetActive(true);
        ActiveButton.interactable = false;
        SpProgress = 0;
        GC.ResetSelectMode();
    }

    List<DepControl> AvailableDeps()
    {
        List<DepControl> L = new List<DepControl>();
        for(int i = 0; i < building.effect.AffectedBuildings.Count; i++)
        {
            if (building.effect.AffectedBuildings[i].Department != null)
                L.Add(building.effect.AffectedBuildings[i].Department);
        }
        return L;
    }

    void ShowAvailableEmps()
    {
        List<DepControl> Deps = AvailableDeps();
        List<Employee> Emps = new List<Employee>();
        for(int i = 0; i < Deps.Count; i++)
        {
            for(int j = 0; j < Deps[i].CurrentEmps.Count; j++)
            {
                Emps.Add(Deps[i].CurrentEmps[j]);
            }
        }
        for(int i = 0; i < GC.CurrentEmployees.Count; i++)
        {
            if (Emps.Contains(GC.CurrentEmployees[i]) == true)
                Emps[i].InfoB.gameObject.SetActive(true);
            else
                Emps[i].InfoB.gameObject.SetActive(false);
        }
    }
}
