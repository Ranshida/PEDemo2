using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProduceBuff
{
    public float Value;
    public int TimeLeft;
    public bool SpecialCheck = false;//用于检测持续到业务结束才消除的Buff
    public DepControl TargetDep;

    public ProduceBuff(float value, DepControl Dep, int Time = 32)
    {
        Value = value;
        TimeLeft = Time;
        TargetDep = Dep;
        Dep.produceBuffs.Add(this);
        Dep.Efficiency += Value;
        TargetDep.GC.HourEvent.AddListener(TimePass);
        //持续到业务结束的检测
        if (Time == -1)
            SpecialCheck = true;
    }

    public void TimePass()
    {
        if (SpecialCheck == true)
            return;
        TimeLeft -= 1;
        if (TimeLeft < 1)
            RemoveBuff();
    }

    public void RemoveBuff()
    {
        TargetDep.GC.HourEvent.RemoveListener(TimePass);
        TargetDep.produceBuffs.Remove(this);
        TargetDep.Efficiency -= Value;
    }
}
public class HireType
{
    public int Level = 0, Type, HireNum = 3; //Type目前已经没什么用了
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
    static public float DepBaseSuccessRate = 1.0f;
    static public float DepBaseMajorSuccessRate = 0.2f;
    static public float DepBaseMajorFailureRate = 0.2f;
    [HideInInspector] public int SpProgress = 0, SpTotalProgress, FailProgress = 0, EfficiencyLevel = 0, SpType;
    //BuildingMode用于区分建筑模式  ActiveMode用于区分激活方式 1直接激活 2选择员工 3选择部门
    [HideInInspector] public int EmpLimit, ProducePointLimit = 20, ActiveMode = 1, BuildingMode = 1, Mode1EffectValue = -1, Mode2EffectValue = -2;
    [HideInInspector] public bool SurveyStart = false;
    public int DepFaith = 50;
    public float Efficiency = 0, SalaryMultiply = 1.0f, BuildingPayMultiply = 1.0f;
    public bool MajorSuccess = false, canWork = false;

    private int CurrentFinishedTaskNum = 0, PreFinishedTaskNum = 0, NoEmpTime = 0;
    private bool NoEmp;
    private bool DetailPanelOpened = false;
    private bool CheatMode = false;

    public Transform EmpContent, PerkContent, EmpPanel, LabPanel, SRateDetailPanel;
    public GameObject OfficeWarning;
    public Building building = null;
    public GameControl GC;
    public Task CurrentTask;
    public DepSelect DS;
    public OfficeControl CommandingOffice;
    public Text Text_DepName, Text_Task, Text_Progress, Text_Quality, Text_Time, Text_Office, Text_Efficiency, Text_LevelDownTime, 
        Text_SRateDetail, Text_DetailInfo, Text_ManagerStatus;
    public Text Text_DepFaith, Text_DepMode, Text_SkillRequire, Text_TaskTime, Text_SRate, Text_MSRate, Text_MFRate, Text_FinishedTaskNum, Text_DepCost;
    public Button SurveyButton, ActiveButton, ModeChangeButton;
    public EmpType type;

    public List<ProduceBuff> produceBuffs = new List<ProduceBuff>();
    public List<Employee> CurrentEmps = new List<Employee>();
    public List<OfficeControl> InRangeOffices = new List<OfficeControl>();
    public List<PerkInfo> CurrentPerks = new List<PerkInfo>();
    public int[] DepHeadHuntStatus = new int[15];

    private void Update()
    {
        //if (type == EmpType.Science)
        //    Text_Efficiency.text = "效率:" + (Efficiency + GC.EfficiencyExtraScience) * 100 + "%";
        //else if (type != EmpType.HR)
        //    Text_Efficiency.text = "效率:" + (Efficiency + GC.EfficiencyExtraNormal) * 100 + "%";
        //if (type != EmpType.HR)
        //{
        //    Text_Efficiency.text = "动员等级:" + EfficiencyLevel;
        //}
        if (DepFaith >= 80)
            Text_Progress.text = "每周体力+10";
        else if (DepFaith >= 60)
            Text_Progress.text = "每周体力+5";
        else if (DepFaith >= 40)
            Text_Progress.text = "每周体力-0";
        else if (DepFaith >= 20)
            Text_Progress.text = "每周体力-5";
        else
            Text_Progress.text = "每周体力-10";
        if (DetailPanelOpened == true)
        {
            if (CommandingOffice != null)
            {
                if (CommandingOffice.CurrentManager != null)
                {
                    int TotalNum = CommandingOffice.ControledDeps.Count + CommandingOffice.ControledOffices.Count;
                    int LimitNum = CommandingOffice.ManageValue;
                    Text_Office.text = "由 " + CommandingOffice.CurrentManager.Name + "(" + TotalNum + "/" + LimitNum + ")" +
                        "(" + CommandingOffice.Text_OfficeName.text + ") 管理";
                }
                else
                {
                    Text_Office.text = "由 " + CommandingOffice.Text_OfficeName.text + "(空) 管理";
                    Text_ManagerStatus.text = "上司业务能力等级:——";
                }
            }
            else
            {
                Text_Office.text = "无管理者";
                Text_ManagerStatus.text = "上司业务能力等级:——";
            }
            Text_DepFaith.text = "部门信念:" + DepFaith;
            if (BuildingMode == 1)
                Text_DepMode.text = "部门模式:" + building.Function_A;
            else if (BuildingMode == 2)
                Text_DepMode.text = "部门模式:" + building.Function_B;
            Text_Efficiency.text = "动员等级:" + GC.SC.MobLevel;
            string skill = "";//1技术 2市场 3产品 4观察 5坚韧 6强壮 7管理 8人力 9财务 10决策 11行业 12谋略 13说服 14魅力 15八卦
            if (building.effectValue == 1)
                skill = "技术";
            else if (building.effectValue == 2)
                skill = "市场";
            else if (building.effectValue == 3)
                skill = "产品";
            else if (building.effectValue == 4)
                skill = "观察";
            else if (building.effectValue == 5)
                skill = "坚韧";
            else if (building.effectValue == 6)
                skill = "强壮";
            else if (building.effectValue == 7)
                skill = "管理";
            else if (building.effectValue == 8)
                skill = "人力";
            else if (building.effectValue == 9)
                skill = "财务";
            else if (building.effectValue == 10)
                skill = "决策";
            else if (building.effectValue == 11)
                skill = "行业";
            else if (building.effectValue == 12)
                skill = "谋略";
            else if (building.effectValue == 13)
                skill = "说服";
            else if (building.effectValue == 14)
                skill = "魅力";
            else if (building.effectValue == 15)
                skill = "八卦";
            Text_SkillRequire.text = "所需技能:" + skill;

            int Pp = 0;
            foreach (Employee e in CurrentEmps)
            {
                if (e.InfoDetail.Entity.OutCompany == false)
                    Pp++;
            }
            int limit = ProducePointLimit;
            //研发部门额外Buff
            if (building.Type == BuildingType.研发部门)
                limit -= GC.ResearchExtraTimeBoost;
            if (Pp > 0)
                Text_TaskTime.text = "生产周期:" + (limit / Pp);
            else
                Text_TaskTime.text = "生产周期:--";

            Text_SRate.text = "成功率:" + Mathf.Round((DepBaseSuccessRate + CountSuccessRate(building.effectValue)) * 100) + "%";
            Text_MSRate.text = "大成功率:" + ((DepBaseMajorSuccessRate + GC.SC.ExtraMajorSuccessRate) * 100) + "%";
            Text_MFRate.text = "重大失误率:" + ((DepBaseMajorFailureRate + GC.SC.ExtraMajorFailureRate) * 100) + "%";
            Text_FinishedTaskNum.text = "上季度业务成功数:" + PreFinishedTaskNum + "\n\n" + "本季度业务成功数:" + CurrentFinishedTaskNum;
            Text_DepCost.text = "运营成本:" + (CalcCost(1) + CalcCost(2));
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
        //工作结算
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
                if (BuildingMode == 1 && GC.FinishedTask[4] >= 2)
                    SpProgress += Pp;
                else if (BuildingMode == 2 && GC.FinishedTask[4] >= 1 && GC.Money >= 300)
                    SpProgress += Pp;
            }
            else
            {
                if (ActiveButton.interactable == false)
                    SpProgress += Pp;
            }
            int limit = ProducePointLimit;
            //研发部门额外Buff
            if (building.Type == BuildingType.研发部门)
                limit -= GC.ResearchExtraTimeBoost;

            if (SpProgress >= limit && ActiveButton.interactable == false)
            {
                //完成生产
                float BaseSuccessRate = DepBaseSuccessRate + CountSuccessRate(building.effectValue);
                float Posb = Random.Range(0.0f, 1.0f);
                //作弊模式必定成功
                if (CheatMode == true)
                    Posb = BaseSuccessRate - 1;
                //成功和大成功
                if (Posb <= BaseSuccessRate)
                {
                    CurrentFinishedTaskNum += 1;
                    if (Random.Range(0.0f, 1.0f) < DepBaseMajorSuccessRate + GC.SC.ExtraMajorSuccessRate)
                    {
                        //大成功
                        //额外经验获取
                        if (CurrentEmps.Count > 0)
                        {
                            for (int i = 0; i < ProducePointLimit / CurrentEmps.Count; i++)
                            {
                                EmpsGetExp(building.effectValue, 5);
                            }
                        }
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
                            GC.FinishedTask[4] += 6;
                            SpProgress = 0;
                            GC.CreateMessage(Text_DepName.text + " 完美完成了营销文案的生产");
                        }
                        else if (building.Type == BuildingType.产品部门)
                        {
                            GC.FinishedTask[6] += 6;
                            SpProgress = 0;
                            GC.CreateMessage(Text_DepName.text + " 完美完成了原型图的生产");
                        }
                        else if (building.Type == BuildingType.公关营销部)
                        {
                            MajorSuccess = true;
                            BuildingActive();
                            SpProgress = 0;
                        }
                        else if (building.Type == BuildingType.人力资源部)
                        {
                            MajorSuccess = true;
                            BuildingActive();
                            SpProgress = 0;
                            GC.CreateMessage(Text_DepName.text + " 完美完成了招聘");
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
                            GC.FinishedTask[4] += 3;
                            SpProgress = 0;
                            GC.CreateMessage(Text_DepName.text + " 完成了营销文案的生产");
                        }
                        else if (building.Type == BuildingType.产品部门)
                        {
                            GC.FinishedTask[6] += 3;
                            SpProgress = 0;
                            GC.CreateMessage(Text_DepName.text + " 完成了原型图的生产");
                        }
                        else if (building.Type == BuildingType.公关营销部)
                        {
                            MajorSuccess = false;
                            BuildingActive();
                            SpProgress = 0;
                        }
                        else if (building.Type == BuildingType.人力资源部)
                        {
                            MajorSuccess = false;
                            BuildingActive();
                            SpProgress = 0;
                            GC.CreateMessage(Text_DepName.text + " 完成了招聘");
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
                    float Posb2 = Random.Range(0.0f, 1.0f);
                    if (Posb2 < DepBaseMajorFailureRate + GC.SC.ExtraMajorFailureRate)
                    {
                        //大失败
                        GC.CreateMessage(Text_DepName.text + " 工作中发生重大失误");
                        AddPerk(new Perk105(null));
                    }
                    else
                    {
                        //失败
                        GC.CreateMessage(Text_DepName.text + " 工作失败");
                    }
                }
                RemoveSpecialBuffs();
            }

            UpdateUI();
        }
        //空置判定
        if(CurrentEmps.Count == 0 && NoEmp == false)
        {
            NoEmpTime += 1;
            if(NoEmpTime > 16)
            {
                NoEmp = true;
                NoEmpTime = 0;
                AddPerk(new Perk111(null));
            }
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
            //成功率计算
            Text_Quality.text = "成功率:" + Mathf.Round((DepBaseSuccessRate + CountSuccessRate(building.effectValue)) * 100) + "%";

            int limit = ProducePointLimit;
            //研发部门额外Buff
            if (building.Type == BuildingType.研发部门)
                limit -= GC.ResearchExtraTimeBoost;
            Text_Time.text = "剩余时间:" + calcTime(Pp, limit - SpProgress) + "时";
        }
        else
        {
            Text_Task.text = "当前任务: 无";
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
            GC.CurrentDeps[i].DetailPanelOpened = false;
        }
        EmpPanel.gameObject.SetActive(true);
        DetailPanelOpened = true;
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

        GC.HourEvent.RemoveListener(Produce);
        GC.WeeklyEvent.RemoveListener(FaithEffect);
        foreach(PerkInfo perk in CurrentPerks)
        {
            perk.CurrentPerk.RemoveAllListeners();
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
        Text_SRateDetail.text = "基础成功率:" + Mathf.Round(DepBaseSuccessRate * 100) + "%\n";
        //技能影响
        for (int i = 0; i < CurrentEmps.Count; i++)
        {
            int value = 0;
            value = CurrentEmps[i].BaseAttributes[type - 1];
            float EValue = 0;
            if (value <= 5)
                EValue -= 0.15f;
            else if (value <= 9)
                EValue -= 0.1f;
            else if (value <= 13)
                EValue -= 0.05f;
            else if (value <= 17)
                EValue += 0;
            else if (value <= 21)
                EValue += 0.04f;
            else if (value > 21)
                EValue += 0.08f;
            //文字显示
            Text_SRateDetail.text += CurrentEmps[i].Name + "技能:" + (EValue * 100) + "%";
            //员工额外效果B
            ExtraEffectDescription(CurrentEmps[i]);

            Text_SRateDetail.text += "\n";
            BaseSuccessRate += EValue;
            BaseSuccessRate += CurrentEmps[i].ExtraSuccessRate;
        }
        
        //高管技能影响
        if (CommandingOffice != null && CommandingOffice.CurrentManager != null)
        {
            int value = 0;
            value = CommandingOffice.CurrentManager.BaseAttributes[type - 1];
            float EValue = 0;
            if (value <= 5)
            {
                EValue -= 0.15f;
                Text_ManagerStatus.text = "上司业务能力等级:草包管理";
            }
            else if (value <= 9)
            {
                EValue -= 0.1f;
                Text_ManagerStatus.text = "上司业务能力等级:胡乱指挥";
            }
            else if (value <= 13)
            {
                EValue += 0.05f;
                Text_ManagerStatus.text = "上司业务能力等级:外行领导";
            }
            else if (value <= 17)
            {
                EValue += 0;
                Text_ManagerStatus.text = "上司业务能力等级:普通管理者";
            }
            else if (value <= 21)
            {
                EValue += 0.04f;
                Text_ManagerStatus.text = "上司业务能力等级:优秀管理";
            }
            else if (value > 21)
            {
                EValue += 0.08f;
                Text_ManagerStatus.text = "上司业务能力等级:业界领袖";
            }
            BaseSuccessRate += EValue;
            BaseSuccessRate += CommandingOffice.CurrentManager.ExtraSuccessRate;
            //文字显示
            Text_SRateDetail.text += "高管" + CommandingOffice.CurrentManager.Name + "技能:" + (EValue * 100) + "%";
            //高管额外效果B
            ExtraEffectDescription(CommandingOffice.CurrentManager, true);
        }
        if (Mathf.Abs(GC.SC.ExtraSuccessRate) > 0.001f)
            Text_SRateDetail.text += "\n动员效果:" + (GC.SC.ExtraSuccessRate * 100) + "%";
        if (Mathf.Abs(Efficiency) > 0.001f)
            Text_SRateDetail.text += "\n额外效果:" + (Efficiency * 100) + "%";
        if (building.Type == BuildingType.人力资源部 && GC.HireSuccessExtra > 0.001f)
        {
            Text_SRateDetail.text += "\n额外招聘成功率:" + (GC.HireSuccessExtra * 100) + "%";
            BaseSuccessRate += GC.HireSuccessExtra;
        }
        if (Mathf.Abs(GC.BaseDepExtraSuccessRate) > 0.001f)
        {
            if (building.Type == BuildingType.技术部门 || building.Type == BuildingType.市场部门 || building.Type == BuildingType.产品部门 || building.Type == BuildingType.公关营销部)
            {
                Text_SRateDetail.text += "\n额外业务成功率:" + (GC.BaseDepExtraSuccessRate * 100) + "%";
                BaseSuccessRate += GC.BaseDepExtraSuccessRate;
            }
        }
        if(Mathf.Abs(GC.ResearchExtraSuccessRate) > 0.001f && building.Type == BuildingType.研发部门)
        {
            Text_SRateDetail.text += "\n额外研发成功率:" + (GC.ResearchExtraSuccessRate * 100) + "%";
            BaseSuccessRate += GC.ResearchExtraSuccessRate;
        }
        
        return BaseSuccessRate + GC.SC.ExtraSuccessRate + Efficiency;
    }
    //额外效果描述
    void ExtraEffectDescription(Employee emp, bool isManager = false)
    {
        if(Mathf.Abs(emp.ExtraSuccessRate) > 0.001f)
        {
            foreach (PerkInfo perk in emp.InfoDetail.PerksInfo)
            {
                int a = perk.CurrentPerk.Num;
                string eName = "";
                if (isManager == true)
                    eName = "高管";
                eName += emp.Name;
                if (a == 30 || a == 31 || a == 33 || a == 38 || a == 94)
                {
                    Text_SRateDetail.text += "\n" + eName + perk.CurrentPerk.Name + "状态: " + (perk.CurrentPerk.TempValue4 * perk.CurrentPerk.Level * 100) + "%";
                }
            }
        }
    }

    //详细信息显示
    public void ShowSRateDetailPanel()
    {
        Text_DetailInfo.gameObject.SetActive(false);
        Text_SRateDetail.gameObject.SetActive(true);
        CountSuccessRate(building.effectValue);
        SRateDetailPanel.transform.position = Input.mousePosition;
        SRateDetailPanel.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(SRateDetailPanel.gameObject.GetComponent<RectTransform>());
    }
    public void ShowDetailPanel(int type)
    {
        Text_DetailInfo.gameObject.SetActive(true);
        Text_SRateDetail.gameObject.SetActive(false);
        if (type == 0)
        {
            Text_DetailInfo.text = "成功率:" + (GC.SC.ExtraSuccessRate * 100) + "%";
            Text_DetailInfo.text = "大成功率:" + (GC.SC.ExtraMajorSuccessRate * 100) + "%";
        }
        else if (type == 1)
        {
            Text_DetailInfo.text = "信念基础值:50";
            foreach (PerkInfo info in CurrentPerks)
            {
                if (info.CurrentPerk.Num == 97)
                    Text_DetailInfo.text += "\n转岗无望 -" + (info.CurrentPerk.Level * 15);
                else if (info.CurrentPerk.Num == 98)
                    Text_DetailInfo.text += "\n升职无望 -" + (info.CurrentPerk.Level * 15);
                else if (info.CurrentPerk.Num == 99)
                    Text_DetailInfo.text += "\n并肩作战 +" + info.CurrentPerk.TempValue1;
                else if (info.CurrentPerk.Num == 100)
                    Text_DetailInfo.text += "\n同室操戈 -" + info.CurrentPerk.TempValue1;
                else if (info.CurrentPerk.Num == 101)
                    Text_DetailInfo.text += "\n信仰不一 -15";
                else if (info.CurrentPerk.Num == 102)
                    Text_DetailInfo.text += "\n信仰一致 +15";
                else if (info.CurrentPerk.Num == 103)
                    Text_DetailInfo.text += "\n文化不一 -15";
                else if (info.CurrentPerk.Num == 104)
                    Text_DetailInfo.text += "\n文化一致 +15";
                else if (info.CurrentPerk.Num == 104)
                    Text_DetailInfo.text += "\n文化一致 +15";
                else if (info.CurrentPerk.Num == 105)
                    Text_DetailInfo.text += "\n重大失误 -" + (info.CurrentPerk.Level * 20);
                else if (info.CurrentPerk.Num == 106)
                    Text_DetailInfo.text += "\n心理咨询 +25";
                else if (info.CurrentPerk.Num == 108)
                    Text_DetailInfo.text += "\n生疏磨合 -" + (info.CurrentPerk.Level * 15);
                else if (info.CurrentPerk.Num == 110)
                    Text_DetailInfo.text += "\n紧急调离 -" + (info.CurrentPerk.Level * 15);
                else if (info.CurrentPerk.Num == 111)
                    Text_DetailInfo.text += "\n空置部门 -30";
                else if (info.CurrentPerk.Num == 115)
                    Text_DetailInfo.text += "\n老板摸鱼 -35";
                else if (info.CurrentPerk.Num == 116)
                    Text_DetailInfo.text += "\n业务干扰 -" + info.CurrentPerk.TempValue1;
            }
            if (DepFaith >= 80)
                Text_DetailInfo.text += "\n——————\n每周员工心力+10";
            else if (DepFaith >= 60)
                Text_DetailInfo.text += "\n——————\n每周员工心力+5";
            else if (DepFaith >= 40)
                Text_DetailInfo.text += "\n——————\n每周员工心力+0";
            else if(DepFaith >= 20)
                Text_DetailInfo.text += "\n——————\n每周员工心力-5";
            else
                Text_DetailInfo.text += "\n——————\n每周员工心力-10";
        }
        else if (type == 2)
        {
            int limit = ProducePointLimit;
            //研发部门额外Buff
            if (building.Type == BuildingType.研发部门)
                limit -= GC.ResearchExtraTimeBoost;
            Text_DetailInfo.text = "标准生产周期:" + limit;
            if (BuildingMode == 1)
                Text_DetailInfo.text += "\n" + building.Description_A;
            else if (BuildingMode == 2)
                Text_DetailInfo.text += "\n" + building.Description_B;
        }
        else if (type == 3)
            Text_DetailInfo.text = "生产周期 = 业务生产周期 / 部门员工数";
        else if (type == 4)
        {
            ShowSRateDetailPanel();
            return;
        }
        else if (type == 5)
        {
            Text_DetailInfo.text = "基础大成功率:" + Mathf.Round(DepBaseMajorSuccessRate * 100) + "%";
            if (GC.SC.ExtraMajorSuccessRate > 0.0001)
                Text_DetailInfo.text += "\n额外动员效果:" + (GC.SC.ExtraMajorSuccessRate * 100) + "%";
        }
        else if (type == 6)
        {
            Text_DetailInfo.text = "基础重大失误率:" + Mathf.Round(DepBaseMajorFailureRate * 100) + "%";
            if (GC.SC.ExtraMajorSuccessRate > 0.0001)
                Text_DetailInfo.text += "\nKPI制度:" + (GC.SC.ExtraMajorFailureRate * 100) + "%";
        }
        else if (type == 7)
        {
            Text_DetailInfo.text = "建筑维护费:" + CalcCost(2) + "\n员工工资:" + CalcCost(1);
        }
        else if (type == 8)
        {
            if (CommandingOffice == null)
                return;
            if (CommandingOffice.CurrentManager == null)
                return;
            Text_DetailInfo.text = "管理能力:" + CommandingOffice.CurrentManager.Manage;
            if (CommandingOffice.CurrentManager.Manage < 6)
                Text_DetailInfo.text += "\n管理等级:组长 (管理能力 < 6) \n可管理部门数:1";
            else if (CommandingOffice.CurrentManager.Manage < 11)
                Text_DetailInfo.text += "\n管理等级:初级管理者 (6 <= 管理能力 < 11) \n可管理部门数:2";
            else if (CommandingOffice.CurrentManager.Manage < 16)
                Text_DetailInfo.text += "\n管理等级:中层管理者 (11 <= 管理能力 < 16) \n可管理部门数:3";
            else if (CommandingOffice.CurrentManager.Manage < 21)
                Text_DetailInfo.text += "\n管理等级:精英管理者 (16 <= 管理能力 < 21) \n可管理部门数:4";
            else
                Text_DetailInfo.text += "\n管理等级:管理大师 (21 <= 管理能力) \n可管理部门数:5";
            Text_DetailInfo.text += "\n直属下属:";
            foreach (DepControl dep in CommandingOffice.ControledDeps)
            {
                Text_DetailInfo.text += "\n" + dep.Text_DepName.text;
            }
            foreach (OfficeControl office in CommandingOffice.ControledOffices)
            {
                Text_DetailInfo.text += "\n" + office.Text_OfficeName.text;
            }
        }
        else if (type == 9)
        {
            if (CommandingOffice == null)
                return;
            if (CommandingOffice.CurrentManager == null)
                return;
            int value = 0;
            value = CommandingOffice.CurrentManager.BaseAttributes[building.effectValue - 1];
            if (value <= 5)
            {
                Text_DetailInfo.text = "部门成功率:-15%";
                Text_DetailInfo.text += "\n当前生产所需技能 <= 5";
            }
            else if (value <= 9)
            {
                Text_DetailInfo.text = "部门成功率:-5%";
                Text_DetailInfo.text += "\n5 <当前生产所需技能 <= 9";
            }
            else if (value <= 13)
            {
                Text_DetailInfo.text = "部门成功率:+0%";
                Text_DetailInfo.text += "\n9 <当前生产所需技能 <= 13";
            }
            else if (value <= 17)
            {
                Text_DetailInfo.text = "部门成功率:+5%";
                Text_DetailInfo.text += "\n13 <当前生产所需技能 <= 17";
            }
            else if (value <= 21)
            {
                Text_DetailInfo.text = "部门成功率:+15%";
                Text_DetailInfo.text += "\n17 <当前生产所需技能 <= 21";
            }
            else if (value > 21)
            {
                Text_DetailInfo.text = "部门成功率:+20%";
                Text_DetailInfo.text += "\n当前生产所需技能 > 21";
            }
            Text_DetailInfo.text += "\n(当前等级:" + value + ")";
        }
        else if (type == 10)
            Text_DetailInfo.text = "目前没有为此办公室指定上司或上司管理能力不足\n点击“详细信息”左侧面板中的“更换”可更换上司";
        else if (type == 11)
            Text_DetailInfo.text = "部门中每增加一名员工生产力+1";
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
        GC.Text_DepMode1.text = building.Function_A;
        GC.Text_DepMode1.transform.parent.parent.GetComponent<Text>().text = "技能需求:" + building.Require_A + "\n生产周期:" + 
            building.Time_A + "\n功能描述:" + building.Description_A + "\n" + building.Result_A;
        GC.Text_DepMode2.text = building.Function_B;
        GC.Text_DepMode2.transform.parent.parent.GetComponent<Text>().text = "技能需求:" + building.Require_B + "\n生产周期:" +
            building.Time_B + "\n功能描述:" + building.Description_B + "\n" + building.Result_B;
    }

    //部门模式改变
    public void ChangeBuildingMode(int num)
    {
        BuildingMode = num;
        SpProgress = 0;
        ActiveButton.interactable = false;
        GC.DepModeSelectPanel.SetActive(false);
        if (num == 1)
        {
            building.effectValue = Mode1EffectValue;
            ProducePointLimit = int.Parse(building.Time_A);
            if (building.Type == BuildingType.人力资源部)
                Efficiency -= 0.1f;
        }
        else if (num == 2 && Mode2EffectValue != -1)
        {
            building.effectValue = Mode2EffectValue;
            ProducePointLimit = int.Parse(building.Time_B);
            if (building.Type == BuildingType.人力资源部)
                Efficiency += 0.1f;
        }
        UpdateUI();
    }

    //展开确认面板并更新内容
    public void StartActive()
    {
        GC.DepSkillConfirmPanel.SetActive(true);
        if (BuildingMode == 1)
            GC.Text_DepSkillDescribe.text = building.Description_A;
        else if (BuildingMode == 2)
            GC.Text_DepSkillDescribe.text = building.Description_B;
        GC.CurrentDep = this;
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
        if (building.Type == BuildingType.宣传中心 || building.Type == BuildingType.科技工作坊 || building.Type == BuildingType.绩效考评中心
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
                        if (Emps[i].CurrentEmotions[j].color == EColor.Green && Emps[i].CurrentEmotions[j].Level >= count)
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
            EColor ColorA = EColor.Red, ColorB = EColor.LRed;
            if (ActiveMode == 2)
            {
                ColorA = EColor.Blue;
                ColorB = EColor.LBlue;
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
                        if (Emps[i].CurrentEmotions[j].color == EColor.Green && Emps[i].CurrentEmotions[j].Level >= count)
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
            {
                if (BuildingMode == 1)
                    ht.HireNum = 5;
                else
                    ht.HireNum = 4;
            }
            else
            {
                if (BuildingMode == 1)
                    ht.HireNum = 3;
                else
                    ht.HireNum = 2;
            }
            GC.HC.AddHireTypes(ht);
        }
        else if (building.Type == BuildingType.公关营销部)
        {
            if(BuildingMode == 1)
            {
                GC.FinishedTask[4] -= 2;
                if (MajorSuccess == true)
                {
                    GC.CreateMessage("获得了2个传播");
                    GC.FinishedTask[3] += 2;
                }
                else
                {
                    GC.CreateMessage("获得了1个传播");
                    GC.FinishedTask[3] += 1;
                }
            }
            else if (BuildingMode == 2)
            {
                GC.Money -= 300;
                GC.FinishedTask[4] -= 1;
                if (MajorSuccess == true)
                {
                    GC.CreateMessage("获得了2个传播");
                    GC.FinishedTask[3] += 2;
                }
                else
                {
                    GC.CreateMessage("获得了1个传播");
                    GC.FinishedTask[3] += 1; 
                }
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
                    GC.CurrentEmpInfo.emp.Mentality += (int)(GC.CurrentEmpInfo.emp.MentalityLimit * 0.75f);
                else
                    GC.CurrentEmpInfo.emp.Mentality += (int)(GC.CurrentEmpInfo.emp.MentalityLimit * 0.4f);
            }
            else if (BuildingMode == 2)
            {
                int value = 0;
                if (MajorSuccess == true)
                {
                    GC.CurrentEmpInfo.emp.Mentality += (int)(GC.CurrentEmpInfo.emp.MentalityLimit * 0.6f);
                    value = 20;
                }
                else
                {
                    GC.CurrentEmpInfo.emp.Mentality += (int)(GC.CurrentEmpInfo.emp.MentalityLimit * 0.25f);
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
                GC.SelectedDep.AddPerk(new Perk44(null));
                if(MajorSuccess == true)
                    GC.SelectedDep.AddPerk(new Perk44(null));
            }
            else if (BuildingMode == 2)
            {
                GC.SelectedDep.AddPerk(new Perk34(null));
                if (MajorSuccess == true)
                    GC.SelectedDep.AddPerk(new Perk34(null));
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
                    GC.CurrentEmpInfo.emp.Stamina += (int)(GC.CurrentEmpInfo.emp.StaminaLimit * 0.25f);
                else
                    GC.CurrentEmpInfo.emp.Stamina += (int)(GC.CurrentEmpInfo.emp.StaminaLimit * 0.1f);
            }
            else if (BuildingMode == 2)
            {
                if (MajorSuccess == true)
                    GC.CurrentEmpInfo.emp.Stamina += (int)(GC.CurrentEmpInfo.emp.StaminaLimit * 0.15f);
                else
                    GC.CurrentEmpInfo.emp.Stamina += (int)(GC.CurrentEmpInfo.emp.StaminaLimit * 0.05f);
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
                    GC.CurrentEmpInfo.emp.Stamina += (int)(GC.CurrentEmpInfo.emp.StaminaLimit * 0.75f);
                    if (Random.Range(0.0f, 1.0f) < BuffPosb)
                        GC.CurrentEmpInfo.DetailInfo.AddPerk(new Perk35(GC.CurrentEmpInfo.emp), true);
                }
                else
                    GC.CurrentEmpInfo.emp.Stamina += (int)(GC.CurrentEmpInfo.emp.StaminaLimit * 0.4f);
            }
            else if (BuildingMode == 2)
            {
                if (MajorSuccess == true)
                {
                    GC.CurrentEmpInfo.emp.Stamina += (int)(GC.CurrentEmpInfo.emp.StaminaLimit * 0.75f);
                    if (Random.Range(0.0f, 1.0f) < BuffPosb)
                        GC.CurrentEmpInfo.DetailInfo.AddPerk(new Perk37(GC.CurrentEmpInfo.emp), true);
                }
                else
                    GC.CurrentEmpInfo.emp.Stamina += (int)(GC.CurrentEmpInfo.emp.StaminaLimit * 0.4f);
            }
        }
        else if (building.Type == BuildingType.宣传中心 && GC.CurrentEmpInfo != null)
        {
            GC.CurrentEmpInfo.emp.AddEmotion(EColor.Red);
            if (MajorSuccess == true)
                GC.CurrentEmpInfo.emp.AddEmotion(EColor.Red);
        }
        else if (building.Type == BuildingType.科技工作坊 && GC.CurrentEmpInfo != null)
        {
            GC.CurrentEmpInfo.emp.AddEmotion(EColor.Yellow);
            if (MajorSuccess == true)
                GC.CurrentEmpInfo.emp.AddEmotion(EColor.Yellow);
        }
        else if (building.Type == BuildingType.绩效考评中心 && GC.CurrentEmpInfo != null)
        {
            if (BuildingMode == 1)
            {
                GC.CurrentEmpInfo.emp.AddEmotion(EColor.LBlue);
                if (MajorSuccess == true)
                {
                    GC.CurrentEmpInfo.emp.AddEmotion(EColor.LBlue);
                    if (Random.Range(0.0f, 1.0f) < 0.8f)
                        GC.CurrentEmpInfo.DetailInfo.AddPerk(new Perk38(GC.CurrentEmpInfo.emp), true);
                }
            }
            else if (BuildingMode == 2)
            {
                GC.CurrentEmpInfo.emp.AddEmotion(EColor.Yellow);
                GC.CurrentEmpInfo.emp.AddEmotion(EColor.Yellow);
                if (MajorSuccess == true)
                {
                    GC.CurrentEmpInfo.emp.AddEmotion(EColor.Yellow);
                    GC.CurrentEmpInfo.emp.AddEmotion(EColor.Yellow);
                }
            }
        }
        else if (building.Type == BuildingType.员工休息室 && GC.CurrentEmpInfo != null)
        {
            GC.CurrentEmpInfo.emp.AddEmotion(EColor.LYellow);
            if (MajorSuccess == true)
                GC.CurrentEmpInfo.emp.AddEmotion(EColor.LYellow);
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
        else if (building.Type == BuildingType.兴趣社团 && GC.SelectedDep != null)
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
            //旧团结部分，需要先修改作用效果
            //Perk newPerk = new Perk45(null);
            //newPerk.TimeLeft = PTime;
            //newPerk.TempValue4 = value * Count;
            GC.SelectedDep.AddPerk(new Perk45(null));
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
                    GC.CurrentEmpInfo.emp.RemoveEmotion(EColor.Green);
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
                GC.CurrentEmpInfo.emp.RemoveEmotion(EColor.Green);
                GC.CurrentEmpInfo.DetailInfo.AddPerk(new Perk3(GC.CurrentEmpInfo.emp), true);
                if (MajorSuccess == true)
                    GC.CurrentEmpInfo.DetailInfo.AddPerk(new Perk3(GC.CurrentEmpInfo.emp), true);
            }
        }
        else if (building.Type == BuildingType.特别秘书处 && GC.CurrentEmpInfo != null)
        {
            if (BuildingMode == 1)
            {
                GC.CurrentEmpInfo.emp.RemoveEmotion(EColor.Red);
                GC.CurrentEmpInfo.emp.RemoveEmotion(EColor.Red);
                GC.CurrentEmpInfo.emp.RemoveEmotion(EColor.LRed);
                GC.CurrentEmpInfo.emp.RemoveEmotion(EColor.LRed);
            }
            else if (BuildingMode == 2)
            {
                GC.CurrentEmpInfo.emp.RemoveEmotion(EColor.Blue);
                GC.CurrentEmpInfo.emp.RemoveEmotion(EColor.Blue);
                GC.CurrentEmpInfo.emp.RemoveEmotion(EColor.LBlue);
                GC.CurrentEmpInfo.emp.RemoveEmotion(EColor.LBlue);
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
        GC.TotalEmpContent.parent.parent.gameObject.SetActive(false);
        ActiveButton.interactable = false;
        SpProgress = 0;
        GC.ResetSelectMode();
    }

    //完成的业务数量整理
    public void ResetFinishedTaskNum()
    {
        PreFinishedTaskNum = CurrentFinishedTaskNum;
        CurrentFinishedTaskNum = 0;
    }

    public void SetDetailPanelOpened(bool value)
    {
        DetailPanelOpened = value;
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
        newPerk.CurrentPerk.TargetDep = this;
        newPerk.info = GC.infoPanel;
        CurrentPerks.Add(newPerk);
        newPerk.CurrentPerk.AddEffect();
    }
    //移除Perk
    public void RemovePerk(int num)
    {
        foreach(PerkInfo info in CurrentPerks)
        {
            if(info.CurrentPerk.Num == num)
            {
                info.CurrentPerk.RemoveEffect();
                break;
            }
        }
    }
    //检测信念
    public void FaithRelationCheck()
    {
        List<Employee> TempEmps = new List<Employee>();
        foreach(Employee emp in CurrentEmps)
        {
            TempEmps.Add(emp);
        }
        //如果有高管加上高管
        if (CommandingOffice != null && CommandingOffice.CurrentManager != null)
            TempEmps.Add(CommandingOffice.CurrentManager);

        if (TempEmps.Count > 1)
        {
            bool GoodRelation = false, BadRelation = false;
            int CPos = 0, CNeg = 0, RPos = 0, RNeg = 0;
            for (int i = 0; i < TempEmps.Count; i++)
            {
                for (int j = i + 1; j < TempEmps.Count; j++)
                {
                    if (TempEmps[i].FindRelation(TempEmps[j]).FriendValue > 0)
                    {
                        GoodRelation = true;
                        break;
                    }
                    else if (TempEmps[i].FindRelation(TempEmps[j]).FriendValue < 0)
                    {
                        BadRelation = true;
                        break;
                    }
                }
                //文化倾向计数
                if (TempEmps[i].CharacterTendency[0] == -1)
                    CNeg += 1;
                else if (TempEmps[i].CharacterTendency[0] == 1)
                    CPos += 1;
                //信仰倾向计数
                if (TempEmps[i].CharacterTendency[1] == -1)
                    RNeg += 1;
                else if (TempEmps[i].CharacterTendency[1] == 1)
                    RPos += 1;
            }
            if (GoodRelation == true)
                AddPerk(new Perk99(null));
            else
                RemovePerk(99);
            if (BadRelation == true)
                AddPerk(new Perk100(null));
            else
                RemovePerk(100);

            if (RPos == TempEmps.Count || RNeg == TempEmps.Count)
            {
                RemovePerk(101);
                AddPerk(new Perk102(null));
            }
            else
            {
                RemovePerk(102);
                AddPerk(new Perk101(null));
            }
            if (CPos == TempEmps.Count || CNeg == TempEmps.Count)
            {
                RemovePerk(103);
                AddPerk(new Perk104(null));
            }
            else
            {
                RemovePerk(104);
                AddPerk(new Perk103(null));
            }
        }
        //无人时清空所有
        else
        {
            RemovePerk(99);
            RemovePerk(100);
            RemovePerk(101);
            RemovePerk(102);
            RemovePerk(103);
            RemovePerk(104);
        }

    }
    //信念效果
    public void FaithEffect()
    {
        //员工关系检查放在这里，每周进行一次
        FaithRelationCheck();

        int value = 0;
        if (DepFaith >= 80)
            value = 10;
        else if (DepFaith >= 60)
            value = 5;
        else if (DepFaith >= 40)
            value = 0;
        else if (DepFaith >= 20)
            value = -5;
        else
            value = -10;
        foreach (Employee emp in CurrentEmps)
        {
            emp.Mentality += value;
        }
    }
    //人员调动
    public void EmpMove(bool MoveOut)
    {
        if (MoveOut == true)
            AddPerk(new Perk110(null));
        else
        {
            AddPerk(new Perk108(null));
            //设定空置部门状态
            if (NoEmp == true)
            {
                foreach (PerkInfo info in CurrentPerks)
                {
                    if (info.CurrentPerk.Num == 111)
                    {
                        info.CurrentPerk.TimeLeft = 8;
                        break;
                    }
                }
            }
            NoEmp = false;
            NoEmpTime = 0;
        }
        FaithRelationCheck();
    }
    //作弊模式
    public void ToggleCheatMode(bool value)
    {
        CheatMode = value;
    }

    //查找自己影响范围内的部门
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
    //现实影响范围内部门的员工的面板
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
                GC.CurrentEmployees[i].InfoB.gameObject.SetActive(true);
            else
                GC.CurrentEmployees[i].InfoB.gameObject.SetActive(false);
        }
    }

    //删除持续时间为业务时间的perk
    void RemoveSpecialBuffs()
    {
        List<Perk> PerkList = new List<Perk>();
        foreach(PerkInfo perk in CurrentPerks)
        {
            if (perk.CurrentPerk.Num == 48 || perk.CurrentPerk.Num == 50 || perk.CurrentPerk.Num == 71)
                PerkList.Add(perk.CurrentPerk);
        }
        if (PerkList.Count > 0)
        {
            foreach(Perk perk in PerkList)
            {
                for(int i = 0; i < perk.Level; i++)
                {
                    perk.RemoveEffect();
                }
            }
            PerkList.Clear();
        }
    }

    public int CalcCost(int type)
    {
        int value = 0;
        //工资
        if (type == 1)
        {
            foreach (Employee emp in CurrentEmps)
            {
                value += (int)(emp.InfoDetail.CalcSalary() * GC.TotalSalaryMultiply * SalaryMultiply);
            }
        }
        //维护费
        else if (type == 2)
            value = (int)(building.Pay * GC.TotalBuildingPayMultiply * BuildingPayMultiply);
        return value;
    }
}
