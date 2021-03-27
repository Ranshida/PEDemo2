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
    public int HireNum = 2;
    public bool MajorSuccess = false;

}

public class DepControl : MonoBehaviour
{
    public float DepBaseMajorSuccessRate = 0;
    public float DepBaseMajorFailureRate = 0;
    [HideInInspector] public int FailProgress = 0, EfficiencyLevel = 0, SpType;
    //BuildingMode用于区分建筑模式  ActiveMode用于区分激活方式 1直接激活 2选择员工 3选择部门 4选择区域
    [HideInInspector] public int ProducePointLimit = 20, ActiveMode = 1, Mode1EffectValue = -1, Mode2EffectValue = -2;
    [HideInInspector] public bool SurveyStart = false;
    public int DepFaith = 50;
    public int StaminaExtra = 0;//特质导致的每周体力buff
    public int ManageValue;//管理值
    public int ManageUse;//作为下属时的管理值消耗
    public int EmpLimit;
    public int SubDepValue;//附加建筑作用效果值
    public int BaseWorkStatus;//基本工作状态
    public float Efficiency = 0, SalaryMultiply = 1.0f, BuildingPayMultiply = 1.0f, SpProgress = 0;
    public float StaminaCostRate = 1.0f;//体力消耗率（默认100%）
    public bool MajorSuccess = false, canWork = false, DefautDep = false;

    private int CurrentFinishedTaskNum = 0, PreFinishedTaskNum = 0, NoEmpTime = 0;
    private int SubDepLimit = 2;
    private bool NoEmp;
    private bool DetailPanelOpened = false;
    private bool CheatMode = false, TipShowed = false;

    public Area TargetArea;
    public Employee Manager;
    public Transform EmpContent, PerkContent, EmpPanel, SRateDetailPanel, SubDepPanel;
    public GameObject OfficeWarning, DetailButton, RangeWarning;
    public Building building = null;
    public GameControl GC;
    public DepSelect DS;
    public DepControl CommandingOffice;
    public Text Text_DepName, Text_Task, Text_Progress, Text_Quality, Text_Time, Text_Office, Text_Efficiency, Text_LevelDownTime, 
        Text_SRateDetail, Text_DetailInfo, Text_ManagerStatus, Text_SubDepCreate, Text_SubDepStatusA, Text_SubDepStatusB;
    public Text Text_DepFaith, Text_DepMode, Text_SkillRequire, Text_TaskTime, Text_SRate, Text_MSRate, Text_MFRate, Text_FinishedTaskNum, 
        Text_DepCost, Text_TotalSubValue, Text_CurrentSubValue;
    public Button TargetSelectButton, ModeChangeButton;

    public List<DepControl> SubDeps = new List<DepControl>();//附属建筑，现在用于中央厨房和茶水间
    public List<ProduceBuff> produceBuffs = new List<ProduceBuff>();
    public List<Employee> CurrentEmps = new List<Employee>();
    public List<DepControl> InRangeOffices = new List<DepControl>();
    public List<DepControl> ControledDeps = new List<DepControl>();
    public List<DepControl> PreAffectedDeps = new List<DepControl>();//在上一轮作用中影响到的部门
    public List<PerkInfo> CurrentPerks = new List<PerkInfo>();
    public int[] DepHeadHuntStatus = new int[15];

    private void Start()
    {
        //默认建筑(CEO办公室)的面板设置
        if (DefautDep)
        {
            UIManager.Instance.OnAddNewWindow(EmpPanel.GetComponent<WindowBaseControl>());
            if (SRateDetailPanel != null)
                UIManager.Instance.OnAddNewWindow(SRateDetailPanel.GetComponent<WindowBaseControl>());
        }
    }

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
            Text_Progress.text = "每周心力<color=green>+10</color>";
        else if (DepFaith >= 60)
            Text_Progress.text = "每周心力<color=green>+5</color>";
        else if (DepFaith >= 40)
            Text_Progress.text = "每周心力-0";
        else if (DepFaith >= 20)
            Text_Progress.text = "每周心力<color=red>-5</color>";
        else
            Text_Progress.text = "每周心力<color=red>-10</color>";
        if (DetailPanelOpened == true)
        {
            if (CommandingOffice != null)
            {
                if (CommandingOffice.Manager != null)
                {
                    int TotalNum = 0;
                    foreach(DepControl dep in CommandingOffice.ControledDeps)
                    {
                        TotalNum += dep.ManageUse;
                    }
                    int LimitNum = CommandingOffice.ManageValue;
                    Text_Office.text = "由 " + CommandingOffice.Manager.Name + "(" + TotalNum + "/" + LimitNum + ")" +
                        "(" + CommandingOffice.Text_DepName.text + ") 管理";
                }
                else
                {
                    Text_Office.text = "由 " + CommandingOffice.Text_DepName.text + "(空) 管理";
                    Text_ManagerStatus.text = "上司业务能力等级:——";
                }

                //茶水间特殊文字描述
                Text_TotalSubValue.text = "剩余可分配体力供给:" + CommandingOffice.SubDepValue;
                Text_CurrentSubValue.text = "当前供给体力:" + SubDepValue;
            }
            else
            {
                Text_Office.text = "无管理者";
                Text_ManagerStatus.text = "上司业务能力等级:——";
            }
            Text_DepFaith.text = "部门信念:" + DepFaith;
            Text_DepMode.text = "部门模式:" + building.Function_A;
            string skill = "";//1技术 2市场 3产品 4观察 5坚韧 6强壮 7管理 8人力 9财务 10决策 11行业 12谋略 13说服 14魅力 15八卦
            for (int i = 0; i < 2; i++)
            {
                int type = building.effectValue;
                if (i == 1)
                    type = building.effectValue2;

                if (type == 1)
                    skill = "技术";
                else if (type == 2)
                    skill = "市场";
                else if (type == 3)
                    skill = "产品";
                else if (type == 4)
                    skill = "观察";
                else if (type == 5)
                    skill = "坚韧";
                else if (type == 6)
                    skill = "强壮";
                else if (type == 7)
                    skill = "管理";
                else if (type == 8)
                    skill = "人力";
                else if (type == 9)
                    skill = "财务";
                else if (type == 10)
                    skill = "决策";
                else if (type == 11)
                    skill = "行业";
                else if (type == 12)
                    skill = "谋略";
                else if (type == 13)
                    skill = "说服";
                else if (type == 14)
                    skill = "魅力";
                else if (type == 15)
                    skill = "八卦";
                if (i == 0)
                    Text_SkillRequire.text = "所需技能:" + skill;
                else
                    Text_SkillRequire.text += "/" + skill;
            }
            Text_TaskTime.text = "生产周期:" + ProducePointLimit;

            Text_SRate.text = "成功率:" + Mathf.Round((CountSuccessRate()) * 100) + "%";
            Text_MSRate.text = "大成功率:" + ((DepBaseMajorSuccessRate) * 100) + "%";
            Text_MFRate.text = "重大失误率:" + ((DepBaseMajorFailureRate) * 100) + "%";
            Text_FinishedTaskNum.text = "上季度业务成功数:" + PreFinishedTaskNum + "\n\n" + "本季度业务成功数:" + CurrentFinishedTaskNum;
            Text_DepCost.text = "运营成本:" + (CalcCost(1) + CalcCost(2));
        }
    }

    //目前不只是制作，还有很多别的跟事件相关的功能都写在这儿
    public void Produce()
    {
        //空置判定
        if (CurrentEmps.Count == 0 && NoEmp == false)
        {
            NoEmpTime += 1;
            if (NoEmpTime > 16)
            {
                NoEmp = true;
                NoEmpTime = 0;
                AddPerk(new Perk111(null));
            }
        }

        //工作结算
        if (canWork == true)
        {
            //没有员工或在需要目标时没有目标的情况下直接return，防止完成任务
            if ((ActiveMode == 4 && TargetArea == null) || CurrentEmps.Count == 0)
            {
                UpdateUI();
                RemoveAllProvidePerk();
                return;
            }
            //基础经验获取
            EmpsGetExp();
            //进度增加，部分建筑资源不足时暂停
            float Pp = 0.75f + Efficiency;
            foreach (Employee e in CurrentEmps)
            {
                if (e.InfoDetail.Entity.OutCompany == false)
                    Pp += 0.25f;
            }

            SpProgress += Pp;

            if (SpProgress >= ProducePointLimit)
            {
                //完成生产
                float BaseSuccessRate = CountSuccessRate();
                float Posb = Random.Range(0.0f, 1.0f);
                //作弊模式必定成功
                if (CheatMode == true)
                    Posb = BaseSuccessRate - 1;
                //成功和大成功
                if (Posb <= BaseSuccessRate)
                {
                    CurrentFinishedTaskNum += 1;
                    if (Random.Range(0.0f, 1.0f) < DepBaseMajorSuccessRate)
                    {
                        //大成功
                        MajorSuccess = true;
                        //额外经验获取
                        if (CurrentEmps.Count > 0)
                        {
                            for (int i = 0; i < (int)(ProducePointLimit / Pp); i++)
                            {
                                EmpsGetExp();
                            }
                        }
                        BuildingActive();
                    }
                    else
                    {
                        MajorSuccess = false;
                        BuildingActive();
                    }

                }
                //失败和大失败
                else
                {
                    SpProgress = 0;
                    float Posb2 = Random.Range(0.0f, 1.0f);
                    if (Posb2 < DepBaseMajorFailureRate)
                    {
                        //大失败
                        GC.CreateMessage(Text_DepName.text + " 工作中发生重大失误");
                        AddPerk(new Perk105(null));
                        GC.QC.Init(Text_DepName.text + "发生重大失误!\n(成功率:" + Mathf.Round((CountSuccessRate()) * 100) +
                            "% 重大失误率:" + ((DepBaseMajorFailureRate) * 100) + "%)\n部门信念严重降低");
                    }
                    else
                    {
                        //失败
                        GC.CreateMessage(Text_DepName.text + " 工作失败");
                        if (TipShowed == false)
                        {
                            TipShowed = true;
                            GC.QC.Init(Text_DepName.text + "业务生产失败!\n(成功率:" + Mathf.Round((CountSuccessRate()) * 100) + "%)");
                        }
                    }
                }
                RemoveSpecialBuffs();
            }

            UpdateUI();
        }
        else
            RemoveAllProvidePerk();
    }

    public void UpdateUI()
    {
        if (building.Type == BuildingType.茶水间)
        {//茶水间的特殊信息设定
            Text_Quality.text = "体力供给:" + SubDepValue + "/周";
            return;
        }
        if (canWork == true)
        {
            float Pp = 0;
            if (CurrentEmps.Count > 0)
            {
                Pp = 0.75f + Efficiency;
                foreach (Employee e in CurrentEmps)
                {
                    if (e.InfoDetail != null && e.InfoDetail.Entity != null)
                    {
                        if (e.InfoDetail.Entity.OutCompany == false)
                            Pp += 0.25f;
                    }
                }
            }
            Text_Task.text = "当前任务:" + building.Function_A;
            //成功率计算
            Text_Quality.text = "成功率:" + Mathf.Round((CountSuccessRate()) * 100) + "%";

            int limit = ProducePointLimit;
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

    int calcTime(float pp, float total)
    {
        float time = 0;
        if (pp != 0)
        {
            time = total / pp;
            if (time < 1)
                time = 1;
        }
        return (int)Mathf.Round(time);
    }

    //显示员工面板并关闭其他部门面板
    public void ShowEmpInfoPanel()
    {
        for (int i = 0; i < GC.CurrentDeps.Count; i++)
        {
            GC.CurrentDeps[i].EmpPanel.GetComponent<WindowBaseControl>().SetWndState(false);
            GC.CurrentDeps[i].DetailPanelOpened = false;
        }
        EmpPanel.gameObject.GetComponent<WindowBaseControl>().SetWndState(true);
        DetailPanelOpened = true;
    }

    //部门人数和技能需求检测
    public bool CheckEmpNum()
    {
        if (CurrentEmps.Count < EmpLimit)
            return true;
        else
            return false;
    }
    public bool CheckSkillType(Employee emp)
    {
        foreach(int type in emp.Professions)
        {
            if (type == building.effectValue || type == building.effectValue2)
                return true;
        }
        return false;
    }

    //每工时所有员工获取一份经验
    public void EmpsGetExp()
    {
        for (int i = 0; i < CurrentEmps.Count; i++)
        {
            //判断有几个技能符合
            int type1 = 0, type2 = 0;
            foreach(int a in CurrentEmps[i].Professions)
            {
                if (a == building.effectValue)
                    type1 = a;
                if (a == building.effectValue2)
                    type2 = a;
            }
            if (type1 != 0 && type2 != 0)
            {
                CurrentEmps[i].GainExp(AdjustData.EmpExpDoubleSkillObtain, type1);
                CurrentEmps[i].GainExp(AdjustData.EmpExpDoubleSkillObtain, type2);
            }
            else
            {
                if (type1 != 0)
                    CurrentEmps[i].GainExp(AdjustData.EmpExpObtain, type1);
                if (type2 != 0)
                    CurrentEmps[i].GainExp(AdjustData.EmpExpObtain, type2);
            }
        }
    }

    //删除建筑时重置所有相关数据
    public void ClearDep()
    {
        RemoveAllProvidePerk();//清除所有可能提供的perk
        //清空员工
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

        //附加建筑引用删除
        if(building.Type == BuildingType.茶水间 && CommandingOffice != null)
        {
            CommandingOffice.RemoveSubDep(this);
        }
        //目前仅有中央厨房有附加建筑
        if(SubDeps.Count > 0)
        {
            List<DepControl> d = new List<DepControl>();
            foreach(DepControl dep in SubDeps)
            {
                d.Add(dep);
            }
            foreach(DepControl dep in d)
            {
                GC.BM.DismantleBuilding(dep.building);
            }
        }

        GC.HourEvent.RemoveListener(Produce);
        GC.WeeklyEvent.RemoveListener(FaithEffect);
        foreach(PerkInfo perk in CurrentPerks)
        {
            perk.CurrentPerk.RemoveAllListeners();
            if (perk.CurrentPerk.ProvideDep != null)
                perk.CurrentPerk.ProvideDep.PreAffectedDeps.Remove(this);
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
        Destroy(this.gameObject);
    }

    //计算生产成功率
    public float CountSuccessRate()
    {
        //1-3职业技能 4观察 5坚韧 6强壮 7管理 8人力 9财务 10决策 11行业 12谋略 13说服 14魅力 15八卦
        int WorkStatus = BaseWorkStatus;
        Text_SRateDetail.text = "基础工作状态:" + BaseWorkStatus;
        //技能影响
        for (int i = 0; i < CurrentEmps.Count; i++)
        {
            int value = 0;
            int EValue = 0;
            foreach (int a in CurrentEmps[i].Professions)
            {
                if(a == building.effectValue || a == building.effectValue2)
                {
                    value = CurrentEmps[i].BaseAttributes[a - 1] + CurrentEmps[i].ExtraAttributes[a - 1];
                    if (value == 0)
                        EValue -= 1;
                    else if (value <= 3)
                        EValue += 0;
                    else if (value <= 6)
                        EValue += 1;
                    else if (value <= 9)
                        EValue += 2;
                    else if (value >= 10)
                        EValue += 3;
                }
            }
            //文字显示
            Text_SRateDetail.text += CurrentEmps[i].Name + "技能:" + EValue;
            //员工额外效果B
            ExtraEffectDescription(CurrentEmps[i]);
            Text_SRateDetail.text += "\n";
            WorkStatus += EValue;
        }
        
        //高管技能影响
        if (CommandingOffice != null && CommandingOffice.Manager != null)
        {
            int value = 0;
            int EValue = 0;
            foreach (int a in CommandingOffice.Manager.Professions)
            {
                if (a == building.effectValue || a == building.effectValue2)
                {
                    value = CommandingOffice.Manager.BaseAttributes[a - 1] + CommandingOffice.Manager.ExtraAttributes[a - 1];
                    if (value == 0)
                        EValue -= 1;
                    else if (value <= 3)
                        EValue += 0;
                    else if (value <= 6)
                        EValue += 1;
                    else if (value <= 9)
                        EValue += 2;
                    else if (value >= 10)
                        EValue += 3;
                }
            }
            //文字显示
            Text_SRateDetail.text += "高管" + CommandingOffice.Manager.Name + "技能:" + EValue;
            //员工额外效果B
            ExtraEffectDescription(CommandingOffice.Manager);
            Text_SRateDetail.text += "\n";
            WorkStatus += EValue;
            #region 旧判定
            //int value = 0;
            //value = CommandingOffice.Manager.BaseAttributes[type - 1];
            //float EValue = 0;
            //if (value <= 5)
            //{
            //    EValue -= 0.15f;
            //    Text_ManagerStatus.text = "上司业务能力等级:草包管理";
            //}
            //else if (value <= 9)
            //{
            //    EValue -= 0.1f;
            //    Text_ManagerStatus.text = "上司业务能力等级:胡乱指挥";
            //}
            //else if (value <= 13)
            //{
            //    EValue += 0.05f;
            //    Text_ManagerStatus.text = "上司业务能力等级:外行领导";
            //}
            //else if (value <= 17)
            //{
            //    EValue += 0;
            //    Text_ManagerStatus.text = "上司业务能力等级:普通管理者";
            //}
            //else if (value <= 21)
            //{
            //    EValue += 0.04f;
            //    Text_ManagerStatus.text = "上司业务能力等级:优秀管理";
            //}
            //else if (value > 21)
            //{
            //    EValue += 0.08f;
            //    Text_ManagerStatus.text = "上司业务能力等级:业界领袖";
            //}
            //BaseWorkStatus += EValue;
            //BaseWorkStatus += CommandingOffice.Manager.ExtraSuccessRate;
            #endregion

        }
        //if (Mathf.Abs(GC.SC.ExtraSuccessRate) > 0.001f)
        //    Text_SRateDetail.text += "\n头脑风暴效果:" + (GC.SC.ExtraSuccessRate * 100) + "%";
        //if (Mathf.Abs(Efficiency) > 0.001f)
        //    Text_SRateDetail.text += "\n额外效果:" + (Efficiency * 100) + "%";
        //if (building.Type == BuildingType.人力资源部 && GC.HireSuccessExtra > 0.001f)
        //{
        //    Text_SRateDetail.text += "\n额外招聘成功率:" + (GC.HireSuccessExtra * 100) + "%";
        //    BaseWorkStatus += GC.HireSuccessExtra;
        //}
        //if (Mathf.Abs(GC.BaseDepExtraSuccessRate) > 0.001f)
        //{
        //    if (building.Type == BuildingType.技术部门 || building.Type == BuildingType.市场部门 || building.Type == BuildingType.产品部门 || building.Type == BuildingType.公关营销部)
        //    {
        //        Text_SRateDetail.text += "\n额外业务成功率:" + (GC.BaseDepExtraSuccessRate * 100) + "%";
        //        BaseWorkStatus += GC.BaseDepExtraSuccessRate;
        //    }
        //}
        Text_SRateDetail.text += "\n——————\n每当生产的剩余时间归零的时候进行一次判定，如果判定成功则产出当前正在进行的" +
    "业务，同时有一定几率出现大成功，如果失败则没有产出，同时还会有一定几率出现重大失误";

        float SRate = 0.4f;
        if (WorkStatus <= 1)
            SRate = 0.4f;
        else if (WorkStatus <= 4)
            SRate = 0.6f;
        else if (WorkStatus <= 7)
            SRate = 0.8f;
        else
            SRate = 1.0f;

        return SRate;
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
        CountSuccessRate();
        SRateDetailPanel.transform.position = Input.mousePosition;
        SRateDetailPanel.gameObject.GetComponent<WindowBaseControl>().SetWndState(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(SRateDetailPanel.gameObject.GetComponent<RectTransform>());
    }

    //显示细节面板并更新信息
    public void ShowDetailPanel(int type)
    {
        Text_DetailInfo.gameObject.SetActive(true);
        Text_SRateDetail.gameObject.SetActive(false);
        if (type == 0)
        {
            Text_DetailInfo.text = "成功率:" + (GC.SC.ExtraSuccessRate * 100) + "%";
            Text_DetailInfo.text += "大成功率:" + (DepBaseMajorSuccessRate * 100) + "%";
        }
        else if (type == 1)
        {
            Text_DetailInfo.text = "信念基础值:" + (int.Parse(building.FaithBonus) + 50);
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
                    Text_DetailInfo.text += "\n业务干扰 -30";
                else if (info.CurrentPerk.Num == 121)
                    Text_DetailInfo.text += "\n勇气赞歌 +15";
                else if (info.CurrentPerk.Num == 124)
                    Text_DetailInfo.text += "\n无意义争执 -30";
            }
            Text_DetailInfo.text += "\n——————\n部门中所有员工的心力都会受到部门信念的影响\n      信念>=80 心力每周+10\n" +
                "80>信念>=60 心力每周+5\n40>信念>=20 心力每周-5\n      信念<=20 心力每周-10";
        }
        else if (type == 2)
        {
            int limit = ProducePointLimit;
            Text_DetailInfo.text = "标准生产周期:" + limit;
            Text_DetailInfo.text += "\n" + building.Description_A;
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
            Text_DetailInfo.text += "\n——————\n当部门生产成功时，有一定几率转化为大成功，部门中员工会的经验翻倍" +
                "部分部门会同时生产出更多业务";
        }
        else if (type == 6)
        {
            Text_DetailInfo.text = "基础重大失误率:" + Mathf.Round(DepBaseMajorFailureRate * 100) + "%";
            Text_DetailInfo.text += "\n——————\n当部门生产失败时，有一定几率转化为重大失误，严重降低部门信念";
        }
        else if (type == 7)
        {
            Text_DetailInfo.text = "建筑维护费:" + CalcCost(2) + "\n员工工资:" + CalcCost(1);
        }
        else if (type == 8)
        {
            if (CommandingOffice == null)
                return;
            if (CommandingOffice.Manager == null)
                return;
            Text_DetailInfo.text = "管理能力:" + CommandingOffice.Manager.Manage;
            if (CommandingOffice.Manager.Manage < 6)
                Text_DetailInfo.text += "\n管理等级:组长 (管理能力 < 6)";
            else if (CommandingOffice.Manager.Manage < 11)
                Text_DetailInfo.text += "\n管理等级:初级管理者 (6 <= 管理能力 < 11)";
            else if (CommandingOffice.Manager.Manage < 16)
                Text_DetailInfo.text += "\n管理等级:中层管理者 (11 <= 管理能力 < 16)";
            else if (CommandingOffice.Manager.Manage < 21)
                Text_DetailInfo.text += "\n管理等级:精英管理者 (16 <= 管理能力 < 21)";
            else
                Text_DetailInfo.text += "\n管理等级:管理大师 (21 <= 管理能力)";
            Text_DetailInfo.text += "\n管理能力:" + CommandingOffice.Manager.Manage;
            Text_DetailInfo.text += "\n直属下属:";
            foreach (DepControl dep in CommandingOffice.ControledDeps)
            {
                Text_DetailInfo.text += "\n" + dep.Text_DepName.text;
            }
        }
        else if (type == 9)
        {
            if (CommandingOffice == null)
                return;
            if (CommandingOffice.Manager == null)
                return;
            int value = 0;
            value = CommandingOffice.Manager.BaseAttributes[building.effectValue - 1] + CommandingOffice.Manager.ExtraAttributes[building.effectValue - 1];
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
        SRateDetailPanel.gameObject.GetComponent<WindowBaseControl>().SetWndState(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(SRateDetailPanel.gameObject.GetComponent<RectTransform>());

    }
    //关闭细节面板
    public void CloseSRateDetailPanel()
    {
        SRateDetailPanel.gameObject.GetComponent<WindowBaseControl>().SetWndState(false);
    }

    //部门目标的选择
    public void StartTargetSelect()
    {
        if (ActiveMode == 4)
        {
            foreach(Area a in building.CurrentArea.NeighborAreas)
            {
                if (a.gridList[0].Lock == false)
                    a.AS.gameObject.SetActive(true);
            }
            building.CurrentArea.AS.gameObject.SetActive(true);
            GC.AC.CancelButton.SetActive(true);
            GC.AreaSelectMode = 1;
            GC.CurrentDep = this;
        }
    }

    //确定选择
    public void SelectTarget(Area a)
    {
        TargetArea = a;
        UpdateUI();
    }

    public void BuildingActive()
    {
        if (ActiveMode == 4)
        {
            int value = 1;
            if (MajorSuccess == true)
                value = 2;
            //先清除旧perk再找目标
            RemoveAllProvidePerk();
            foreach(Building b in building.effect.AffectedBuildings)
            {
                if(b.CurrentArea == TargetArea)
                {
                    PreAffectedDeps.Add(b.Department);
                }
            }
            //确定自己是否在范围内
            if (building.CurrentArea == TargetArea)
                PreAffectedDeps.Add(this);

            if (building.Type == BuildingType.兴趣社团)
            {
                foreach(DepControl dep in PreAffectedDeps)
                {
                    Perk45 newperk = new Perk45(null);
                    newperk.ProvideDep = this;
                    newperk.TargetDep = dep;
                    newperk.TempValue1 = value;
                    dep.AddPerk(newperk);
                }
            }
            else if (building.Type == BuildingType.心理咨询室)
            {
                foreach (DepControl dep in PreAffectedDeps)
                {
                    Perk144 newperk = new Perk144(null);
                    newperk.ProvideDep = this;
                    newperk.TargetDep = dep;
                    newperk.TempValue1 = value;
                    dep.AddPerk(newperk);
                }
            }
            else if (building.Type == BuildingType.会计办公室)
            {
                foreach (DepControl dep in PreAffectedDeps)
                {
                    Perk44 newperk = new Perk44(null);
                    newperk.ProvideDep = this;
                    newperk.TargetDep = dep;
                    newperk.TempValue1 = value;
                    dep.AddPerk(newperk);
                }
            }
            else if (building.Type == BuildingType.机械检修中心)
            {
                foreach (DepControl dep in PreAffectedDeps)
                {
                    Perk146 newperk = new Perk146(null);
                    newperk.ProvideDep = this;
                    newperk.TargetDep = dep;
                    newperk.TempValue1 = value;
                    dep.AddPerk(newperk);
                }
            }
            else if (building.Type == BuildingType.体能研究室)
            {
                foreach (DepControl dep in PreAffectedDeps)
                {
                    Perk147 newperk = new Perk147(null);
                    newperk.ProvideDep = this;
                    newperk.TargetDep = dep;
                    newperk.TempValue1 = value;
                    dep.AddPerk(newperk);
                }
            }
        }
        else
        {
            if (building.Type == BuildingType.混沌创意营)
            {
                GC.CreateItem(5);
                if(MajorSuccess == true)
                    GC.CreateItem(5);
            }
            else if (building.Type == BuildingType.机械自动化中心)
            {
                GC.CreateItem(6);
                if (MajorSuccess == true)
                    GC.CreateItem(6);
            }
            else if (building.Type == BuildingType.秘书处)
            {
                GC.CreateItem(1);
                if (MajorSuccess == true)
                    GC.CreateItem(1);
            }
            else if (building.Type == BuildingType.私人安保)
            {
                GC.CreateItem(2);
                if (MajorSuccess == true)
                    GC.CreateItem(2);
            }
            else if (building.Type == BuildingType.智库小组)
            {
                GC.CreateItem(3);
                if (MajorSuccess == true)
                    GC.CreateItem(3);
            }
            else if (building.Type == BuildingType.脑机实验室)
            {
                GC.CreateItem(4);
                if (MajorSuccess == true)
                    GC.CreateItem(4);
            }
            else if (building.Type == BuildingType.人力资源部)
            {
                GC.HC.ExtraHireOption += 1;
                if (MajorSuccess == true)
                    GC.HC.ExtraHireOption += 1;
            }
        }

        GC.SelectedDep = null;
        GC.CurrentDep = null;
        GC.CurrentEmpInfo = null;
        GC.TotalEmpPanel.SetWndState(false);
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
                    //普通状态的消除
                    if (p.CurrentPerk.ProvideDep == null)
                    {
                        p.CurrentPerk.RemoveEffect();
                        break;
                    }
                    //如果是建筑提供的状态，且提供者是自己时才消除
                    else if (p.CurrentPerk.ProvideDep == perk.ProvideDep)
                    {
                        p.CurrentPerk.RemoveEffect();
                        break;
                    }
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

        //部门状态添加临时判定
        if (perk.Num >= 143 && perk.Num <= 149)
            newPerk.CurrentPerk.ProvideDep = this;
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
    //移除所有自身提供的Perk
    public void RemoveAllProvidePerk()
    {
        foreach(DepControl dep in PreAffectedDeps)
        {
            foreach(PerkInfo info in dep.CurrentPerks)
            {
                if(info.CurrentPerk.ProvideDep == this)
                {
                    info.CurrentPerk.RemoveEffect();
                    break;
                }
            }
        }
        PreAffectedDeps.Clear();
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
        if (CommandingOffice != null && CommandingOffice.Manager != null)
            TempEmps.Add(CommandingOffice.Manager);

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
    //显示影响范围内部门的员工的面板
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

    //删除持续时间为业务时间和头脑风暴时间的perk
    public void RemoveSpecialBuffs(int type = 0)
    {
        List<Perk> PerkList = new List<Perk>();
        //业务时间
        if (type == 0)
        {
            foreach (PerkInfo perk in CurrentPerks)
            {
                if (perk.CurrentPerk.Num == 71)
                    PerkList.Add(perk.CurrentPerk);
            }
        }
        //头脑风暴时间
        else if (type == 1)
        {
            foreach (PerkInfo perk in CurrentPerks)
            {
                if (perk.CurrentPerk.Num >= 149 && perk.CurrentPerk.Num <= 153)
                    PerkList.Add(perk.CurrentPerk);
            }
        }

        if (PerkList.Count > 0)
        {
            foreach (Perk perk in PerkList)
            {
                for (int i = 0; i < perk.Level; i++)
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

    //附加建筑(茶水间)相关功能
    //附加建筑建造
    public void CreateSubDep()
    {
        if (SubDeps.Count == 2)
            return;
        GC.BM.EnterBuildMode();
        GC.BM.StartBuildNew(BuildingType.茶水间);
        Building nb = GC.BM.Temp_Building;
        DepControl nd = GC.CreateDep(nb);
        nb.Department = nd;
        nd.CommandingOffice = this;
        SubDeps.Add(nd);
        Text_SubDepCreate.text = "建造茶水间 (" + SubDeps.Count + "/2)";
        if (SubDeps.Count == 1)
        {
            SubDeps[0].SubDepValue = 100;
            SubDeps[0].UpdateUI();
        }
    }

    //附加建筑拆除时移除各种引用
    public void RemoveSubDep(DepControl dep)
    {
        SubDeps.Remove(dep);
        Text_SubDepCreate.text = "建造茶水间 (" + SubDeps.Count + "/2)";
        SubDepValue += dep.SubDepValue;
        GC.WeeklyEvent.RemoveListener(SubDepEffect);
    }

    //修改体力供给点数
    public void SubValueChange(bool Add)
    {
        if (Add == true)
        {
            if(CommandingOffice != null && CommandingOffice.SubDepValue > 0)
            {
                CommandingOffice.SubDepValue -= 10;
                SubDepValue += 10;
            }
        }
        else
        {
            if (CommandingOffice != null && SubDepValue > 0)
            {
                SubDepValue -= 10;
                CommandingOffice.SubDepValue += 10;
            }
        }
        UpdateUI();
    }

    //距离检测
    public void SubDepDistanceCheck()
    {
        if(CommandingOffice != null)
        {
            int distance = Mathf.Abs(building.X10 - CommandingOffice.building.X10) + Mathf.Abs(building.Z10 - CommandingOffice.building.Z10);
            if (distance > 80)
            {
                RangeWarning.SetActive(true);
                canWork = false;
            }
            else
            {
                RangeWarning.SetActive(false);
                canWork = true;
            }
        }
    }

    //茶水间体力补充效果
    public void SubDepEffect()
    {
        if (SubDepValue == 0 || canWork == false)
            return;
        List<Employee> AffectedEmps = new List<Employee>();
        foreach (Building b in building.effect.AffectedBuildings)
        {
            if (b.Department != null)
            {
                foreach (Employee e in b.Department.CurrentEmps)
                {
                    //只取体力不满的员工
                    if (e.Stamina != e.StaminaLimit + e.StaminaLimitExtra)
                        AffectedEmps.Add(e);
                }
            }
        }
        if (AffectedEmps.Count == 0)
            return;
        for(int i = 0; i < SubDepValue; i++)
        {
            //根据体力提供点数循环，每次随机给一个体力不满的员工+1体力，直到没有体力不满的人或者提供体力达到上限
            int num = Random.Range(0, AffectedEmps.Count);
            AffectedEmps[num].Stamina += 1;
            if (AffectedEmps[num].Stamina == AffectedEmps[num].StaminaLimit + AffectedEmps[num].StaminaLimitExtra)
                AffectedEmps.RemoveAt(num);
            if (AffectedEmps.Count == 0)
                break;
        }

    }

    //——————————————————————————————————————————————————————————
    //以下是办公室改版后从原OfficeControl复制过来的方法
    //管理监测
    public void CheckManage()
    {
        int value = ManageValue;
        if (canWork == false)
            value = 0;

        int TotalUsage = 0;
        for (int i = 0; i < ControledDeps.Count; i++)
        {
            TotalUsage += ControledDeps[i].ManageUse;
            if (TotalUsage <= value)
            {
                ControledDeps[i].canWork = true;
                ControledDeps[i].OfficeWarning.SetActive(false);
            }
            else
            {
                ControledDeps[i].canWork = false;
                ControledDeps[i].OfficeWarning.SetActive(true);
            }
            ControledDeps[i].UpdateUI();
        }
    }

    //放入和移除高管时调用
    public void SetOfficeStatus()
    {
        if (Manager != null)
        {
            //Text_EmpName.text = "当前高管:" + CurrentManager.Name;
            if (building.Type == BuildingType.高管办公室 || building.Type == BuildingType.CEO办公室)
            {
                //if (BuildingMode == 1)
                //    Text_MAbility.text = "决策:" + CurrentManager.Decision;
                //else if (OfficeMode == 2 || OfficeMode == 4)
                //    Text_MAbility.text = "人力:" + CurrentManager.HR;
                //else if (OfficeMode == 3 || OfficeMode == 5)
                //    Text_MAbility.text = "管理:" + CurrentManager.Manage;
                ManageValue = Manager.Manage;
                Manager.InfoDetail.CreateStrategy();
                Manager.NoPromotionTime = 0;
                for (int i = 0; i < Manager.InfoDetail.PerksInfo.Count; i++)
                {
                    if (Manager.InfoDetail.PerksInfo[i].CurrentPerk.Num == 32)
                    {
                        Manager.InfoDetail.PerksInfo[i].CurrentPerk.RemoveEffect();
                        break;
                    }
                }
                CheckManage();
                UpdateUI();
            }
        }
        else
        {
            //Text_EmpName.text = "当前高管:无";
            //Text_MAbility.text = "能力:--";
            ManageValue = 0;
        }

    }
}
