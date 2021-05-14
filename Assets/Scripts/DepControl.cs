using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HireType
{
    public int HireNum = 3;
    public bool MajorSuccess = false;
}

public class DepControl : MonoBehaviour
{
    public float DepBaseMajorSuccessRate = 0;
    public float DepBaseMajorFailureRate = 0;
    [HideInInspector] public int FailProgress = 0, EfficiencyLevel = 0, SpType;
    //BuildingMode用于区分建筑模式  ActiveMode用于区分激活方式 0无法激活 1直接激活 2选择员工 3选择部门 4选择区域 5卡牌生产
    [HideInInspector] public int ProducePointLimit = 20, ActiveMode = 1, Mode2EffectValue = -2;
    [HideInInspector] public bool SurveyStart = false;
    public int DepLevel = 0;//部门等级,用于计算效果
    public int DepFaith = 50;
    public int StaminaExtra = 0;//特质导致的每周体力buff
    public int ManageValue;//管理值
    public int ManageUse;//作为下属时的管理值消耗
    public int EmpLimit;
    public int BaseWorkStatus;//基本工作状态
    public int ExtraEfficiency = 0;//提供给事业部的额外效率加成
    public int ExtraFaith = 0;//提供给事业部的额外信念加成
    public int ExtraWorkStatus = 0;//提供给事业部的额外工作状态加成
    public int ExtraProduceLimit = 0;//额外生产/充能周期
    public float Efficiency = 0, SalaryMultiply = 1.0f, BuildingPayMultiply = 1.0f, SpProgress = 0;
    public float StaminaCostRate = 1.0f;//体力消耗率（默认100%）
    public bool MajorSuccess = false, canWork = false, DefautDep = false;

    private int NoEmpTime = 0;
    private bool NoEmp;
    private bool CheatMode = false, TipShowed = false, isWeakend = false;

    Action WeakAction, UnWeakAction, AddLevelEffect = () => { }, RemoveLevelEffect = () => { }, DepEffect = () => { };//弱化效果，弱化清除效果，等级增加效果，等级减少效果，部门激活效果
    public Area TargetArea;
    public Employee Manager;
    public Transform EmpContent, PerkContent, EmpPanel, DivPanel, EmpMarkerContent, EmpEffectContent;
    public GameObject EmpEffectPrefab, EmpEffect2Prefab, EmpMarkerPrefab;
    public Button ActiveButton;
    public Building building = null;
    public GameControl GC;
    public DepSelect DS;
    public DepControl CommandingOffice;
    public DivisionControl CurrentDivision;
    public Text Text_DepName, Text_DepName2, Text_DepName3, Text_WeakEffect, Text_InfoProgress, Text_DivProgress, Text_DepFunction;


    public List<Employee> CurrentEmps = new List<Employee>();
    public List<Building> SubBuildings = new List<Building>();//保存的所有附加建筑
    public List<PerkInfo> CurrentPerks = new List<PerkInfo>();
    public List<Image> EmpMarkers = new List<Image>();
    public List<Image> EmpEffectMarkers = new List<Image>();
    public int[] DepHeadHuntStatus = new int[15];

    private void Start()
    {
        //默认建筑(CEO办公室)的面板设置
        if (DefautDep)
        {
            UIManager.Instance.OnAddNewWindow(EmpPanel.GetComponent<WindowBaseControl>());
        }
    }

    private void Update()
    {
        if (building == null)
            return;

        transform.position = Function.World2ScreenPoint(building.transform.position + new Vector3(building.Length * 7.5f, 0, building.Width * 5));
        float scale = Mathf.Lerp(2.2f, 1, CameraController.Instance.height / CameraController.Instance.maxHeight);
        transform.localScale = new Vector3(scale, scale, scale);
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
                AddPerk(new Perk111());
            }
        }

        //工作结算
        if (canWork == true)
        {
            //没有员工的情况下直接return，防止完成任务
            if (CurrentEmps.Count == 0)
                return;
            //基础经验获取
            EmpsGetExp();
            //进度增加
            if (SpProgress < (ProducePointLimit + ExtraProduceLimit + CurrentDivision.ExtraProduceTime))
                SpProgress += 1;

            if (SpProgress >= (ProducePointLimit + ExtraProduceLimit + CurrentDivision.ExtraProduceTime))
            {
                #region 旧生产判定
                ////完成生产
                //float BaseSuccessRate = CountSuccessRate();
                //float Posb = Random.Range(0.0f, 1.0f);
                ////作弊模式必定成功
                //if (CheatMode == true)
                //    Posb = BaseSuccessRate - 1;
                ////成功和大成功
                //if (Posb <= BaseSuccessRate)
                //{
                //    if (Random.Range(0.0f, 1.0f) < DepBaseMajorSuccessRate)
                //    {
                //        //大成功
                //        MajorSuccess = true;
                //        //额外经验获取
                //        if (CurrentEmps.Count > 0)
                //        {
                //            for (int i = 0; i < (int)(ProducePointLimit / Efficiency); i++)
                //            {
                //                EmpsGetExp();
                //            }
                //        }
                //        BuildingActive();
                //    }
                //    else
                //    {
                //        MajorSuccess = false;
                //        BuildingActive();
                //    }

                //}
                ////失败和大失败
                //else
                //{
                //    SpProgress = 0;
                //    float Posb2 = Random.Range(0.0f, 1.0f);
                //    if (Posb2 < DepBaseMajorFailureRate)
                //    {
                //        //大失败
                //        GC.CreateMessage(Text_DepName.text + " 工作中发生重大失误");
                //        AddPerk(new Perk105(null));
                //        GC.QC.Init(Text_DepName.text + "发生重大失误!\n(成功率:" + Mathf.Round((CountSuccessRate()) * 100) +
                //            "% 重大失误率:" + ((DepBaseMajorFailureRate) * 100) + "%)\n部门信念严重降低");
                //    }
                //    else
                //    {
                //        //失败
                //        GC.CreateMessage(Text_DepName.text + " 工作失败");
                //        if (TipShowed == false)
                //        {
                //            TipShowed = true;
                //            GC.QC.Init(Text_DepName.text + "业务生产失败!\n(成功率:" + Mathf.Round((CountSuccessRate()) * 100) + "%)");
                //        }
                //    }
                //}
                #endregion
                if (ActiveMode == 1)
                    BuildingActive();
                else if (ActiveMode != 5)
                    ActiveButton.interactable = true;
                RemoveSpecialBuffs();
            }
        }
        UpdateUI();
    }

    public void UpdateUI()
    {
        if(ActiveMode == 1)
        {
            Text_InfoProgress.text = "生产进度:" + SpProgress + "/" + (ProducePointLimit + ExtraProduceLimit + CurrentDivision.ExtraProduceTime);
            Text_DivProgress.text = "生产进度:" + SpProgress + "/" + (ProducePointLimit + ExtraProduceLimit + CurrentDivision.ExtraProduceTime);
        }
        else if (ActiveMode == 2 || ActiveMode == 3 || ActiveMode == 4)
        {
            Text_InfoProgress.text = "充能进度:" + SpProgress + "/" + (ProducePointLimit + ExtraProduceLimit + CurrentDivision.ExtraProduceTime);
            Text_DivProgress.text = "充能进度:" + SpProgress + "/" + (ProducePointLimit + ExtraProduceLimit + CurrentDivision.ExtraProduceTime);
        }
    }

    public int calcTime(float pp, float total)
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
        }
        EmpPanel.gameObject.GetComponent<WindowBaseControl>().SetWndState(true);
    }

    //显示当前所处的事业部面板并关闭其他事业部面板
    public void ShowDivisionPanel()
    {
        if(CurrentDivision != null)
        {
            for (int i = 0; i < GC.CurrentDivisions.Count; i++)
            {
                GC.CurrentDivisions[i].SetDetailPanel(false);
            }
            CurrentDivision.SetDetailPanel(true);
        }
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
            if (type == building.effectValue)
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

        GC.HourEvent.RemoveListener(Produce);
        GC.WeeklyEvent.RemoveListener(FaithEffect);
        foreach(PerkInfo perk in CurrentPerks)
        {
            perk.CurrentPerk.RemoveAllListeners();
        }

        GC.CurrentDeps.Remove(this);
        if (CurrentDivision != null)
            RemoveDivision();

        Destroy(DivPanel.gameObject);
        Destroy(DS.gameObject);
        Destroy(EmpPanel.gameObject);
        Destroy(this.gameObject);
    }

    //计算生产成功率
    public float CountSuccessRate()
    {
        //目前旧的部分全删了
        float SRate = 0.4f;

        return SRate;
    }

    //手动激活部门技能
    public void StartBuildingActive()
    {
        //有未处理事件时不能继续
        if (GC.EC.UnfinishedEvents.Count > 0)
            return;
        GC.CurrentDep = this;
        if (ActiveMode == 2)
        {
            GC.SelectMode = 5;
            GC.Text_EmpSelectTip.text = "选择一个员工";
            GC.TotalEmpPanel.SetWndState(true);
        }
    }

    public void BuildingActive()
    {
        int value = 1;
        if (MajorSuccess == true)
            value = 2;

        SpProgress = 0;
        ActiveButton.interactable = false;

        DepEffect();

        GC.UpdateResourceInfo();
        GC.SelectedDep = null;
        GC.CurrentDep = null;
        GC.CurrentEmpInfo = null;
        GC.TotalEmpPanel.SetWndState(false);
        SpProgress = 0;
        GC.ResetSelectMode();
        UpdateUI();
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
        newPerk.SetColor();

        PerkSort();
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

    //状态排序
    void PerkSort()
    {
        if (CurrentPerks.Count == 0)
            return;
        List<PerkInfo> newPerkList = new List<PerkInfo>();
        for(int i = 0; i < 5; i++)
        {
            PerkColor c = PerkColor.None;
            if (i == 0)
                c = PerkColor.White;
            else if (i == 1)
                c = PerkColor.Orange;
            else if (i == 2)
                c = PerkColor.Grey;
            else if (i == 3)
                c = PerkColor.Blue;
            foreach(PerkInfo p in CurrentPerks)
            {
                if (p.CurrentPerk.perkColor == c)
                    newPerkList.Add(p);
            }
        }
        for(int i = 0; i < newPerkList.Count; i++)
        {
            newPerkList[i].transform.SetSiblingIndex(i);
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
                AddPerk(new Perk99());
            else
                RemovePerk(99);
            if (BadRelation == true)
                AddPerk(new Perk100());
            else
                RemovePerk(100);

            if (RPos == TempEmps.Count || RNeg == TempEmps.Count)
            {
                RemovePerk(101);
                AddPerk(new Perk102());
            }
            else
            {
                RemovePerk(102);
                AddPerk(new Perk101());
            }
            if (CPos == TempEmps.Count || CNeg == TempEmps.Count)
            {
                RemovePerk(103);
                AddPerk(new Perk104());
            }
            else
            {
                RemovePerk(104);
                AddPerk(new Perk103());
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
        {
            AddPerk(new Perk110());
            Efficiency -= 0.25f;
        }
        else
        {
            AddPerk(new Perk108());
            Efficiency += 0.25f;
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

        EmpEffectCheck();
    }
    //部门等级变化
    private void SetDepLevel(int level)
    {
        //变换前等级比现在高时清空高级效果,低的时候添加额外效果
        if (DepLevel > level)
        {
            RemoveLevelEffect();
            DepLevel = level;
        }
        else if (DepLevel < level)
        {
            DepLevel = level;
            AddLevelEffect();
        }
    }

    //检测员工技能和数量效果
    public void EmpEffectCheck()
    {
        bool weak = false;
        //设置人员数量效果图标
        for (int i = 0; i < EmpLimit; i++)
        {
            if (CurrentEmps.Count > i)
            {
                //符合技能需求的标绿
                if (CheckSkillType(CurrentEmps[i]) == true)
                {
                    EmpEffectMarkers[i].color = Color.green;
                    EmpMarkers[i].color = Color.green;
                }
                //不符合的标红
                else
                {
                    EmpEffectMarkers[i].color = Color.red;
                    EmpMarkers[i].color = Color.red;
                    weak = true;
                }
                EmpEffectMarkers[i].gameObject.GetComponentInChildren<Text>().text = CurrentEmps[i].Name;
            }
            //超出人数的标白
            else
            {
                EmpEffectMarkers[i].color = Color.white;
                EmpMarkers[i].color = Color.white;
                EmpEffectMarkers[i].gameObject.GetComponentInChildren<Text>().text = "";
            }
        }
        //检测削弱效果
        if (weak != isWeakend)
        {
            isWeakend = weak;
            if (weak == true)
            {
                WeakAction();
                Text_WeakEffect.gameObject.SetActive(true);
            }
            else
            {
                UnWeakAction();
                Text_WeakEffect.gameObject.SetActive(false);
            }
        }
        if (building.EmpCount[2] != 0 && CurrentEmps.Count >= (building.EmpCount[0] + building.EmpCount[1] + building.EmpCount[2]))
            SetDepLevel(3);
        else if (building.EmpCount[1] != 0 && CurrentEmps.Count >= (building.EmpCount[0] + building.EmpCount[1]))
            SetDepLevel(2);
        else if (building.EmpCount[0] != 0 && CurrentEmps.Count >= building.EmpCount[0])
            SetDepLevel(1);
        else
            SetDepLevel(0);

        CurrentDivision.DepExtraCheck();
        UpdateUI();
    }

    //设置人员上限和人员标记以及一些额外效果
    public void SetDepStatus(int empLimit)
    {
        EmpLimit = empLimit;
        for (int i = 0; i < empLimit; i++)
        {
            Image image = Instantiate(EmpMarkerPrefab, EmpMarkerContent).GetComponent<Image>();
            EmpMarkers.Add(image);
        }

        int EmpIndex = 0;
        for (int i = 0; i < 3; i++)
        {
            if (building.EmpCount[i] == 1)
            {
                InitMarker(building.Functions[i], building.Debuffs[i], EmpIndex);
                EmpIndex += 1;
            }
            else if (building.EmpCount[i] == 2)
            {
                InitDoubleMarker(building.Functions[i], building.Debuffs[i], EmpIndex);
                EmpIndex += 2;
            }
        }
        Text_DepFunction.text = building.Description;
        Text_WeakEffect.gameObject.GetComponent<InfoPanelTrigger>().ContentB = building.WeakEffect;

        if (building.Type == BuildingType.自动化研究中心)
        {
            WeakAction = () => { ExtraEfficiency -= 1; };
            UnWeakAction = () => { ExtraEfficiency += 1; };
            AddLevelEffect = () =>
            {
                if (DepLevel == 1)
                    ExtraEfficiency += 1;
                else if (DepLevel == 2)
                {
                    ExtraEfficiency += 1;
                    AddSubBulding();
                }
                else if (DepLevel == 3)
                {
                    ExtraEfficiency += 2;
                    AddSubBulding();
                }
            };
            RemoveLevelEffect = () =>
            {
                if (DepLevel == 1)
                    ExtraEfficiency -= 1;
                else if (DepLevel == 2)
                {
                    ExtraEfficiency -= 1;
                    RemoveSubBuilding();
                }
                else if (DepLevel == 3)
                {
                    ExtraEfficiency -= 2;
                    RemoveSubBuilding();
                }
            };
            ActiveMode = 0;
        }
        else if (building.Type == BuildingType.企业历史展览)
        {
            WeakAction = () => { ExtraFaith -= 10; };
            UnWeakAction = () => { ExtraFaith += 10; };
            AddLevelEffect = () =>
            {
                if (DepLevel == 1)
                    ExtraFaith += 10;
                else if (DepLevel == 2)
                {
                    ExtraFaith += 5;
                    CurrentDivision.AddPerk(new Perk107());
                }
                else if (DepLevel == 3)
                {
                    ExtraFaith += 10;
                    CurrentDivision.AddPerk(new Perk107());
                }
            };
            RemoveLevelEffect = () =>
            {
                if (DepLevel == 1)
                    ExtraFaith -= 10;
                else if (DepLevel == 2)
                {
                    ExtraFaith -= 5;
                    CurrentDivision.RemovePerk(107);
                }
                else if (DepLevel == 3)
                {
                    ExtraFaith -= 10;
                    CurrentDivision.RemovePerk(107);
                }
            };
            ActiveMode = 0;
        }
        else if (building.Type == BuildingType.福报宣传中心)
        {
            WeakAction = () => { ExtraProduceLimit += 2; };
            UnWeakAction = () => { ExtraProduceLimit -= 2; };
            DepEffect = () => { GC.CreateItem(1); };
            AddLevelEffect = () =>
            {
                if (DepLevel == 1)
                {
                    canWork = true;
                    ProducePointLimit = 6;
                }
                else if (DepLevel == 2)
                {
                    ProducePointLimit = 3;
                    CurrentDivision.AddPerk(new Perk90());
                }
                else if (DepLevel == 3)
                {
                    ProducePointLimit = 1;
                    CurrentDivision.AddPerk(new Perk90());
                }
            };
            RemoveLevelEffect = () =>
            {
                if (DepLevel == 1)
                {
                    canWork = false;
                    ProducePointLimit = 0;
                }
                else if (DepLevel == 2)
                {
                    ProducePointLimit = 6;
                    CurrentDivision.RemovePerk(90);
                }
                else if (DepLevel == 3)
                {
                    ProducePointLimit = 3;
                    CurrentDivision.RemovePerk(90);
                }
            };
            ActiveMode = 1;
        }
        else if (building.Type == BuildingType.混沌创意营)
        {
            WeakAction = () => { ExtraWorkStatus += 2; };
            UnWeakAction = () => { ExtraWorkStatus -= 2; };
            AddLevelEffect = () =>
            {
                if (DepLevel == 1)
                    ExtraWorkStatus += 2;
                else if (DepLevel == 2)
                {
                    ExtraWorkStatus += 1;
                    CurrentDivision.AddPerk(new Perk91());
                }
                else if (DepLevel == 3)
                {
                    ExtraWorkStatus += 2;
                    CurrentDivision.AddPerk(new Perk91());
                }
            };
            RemoveLevelEffect = () =>
            {
                if (DepLevel == 1)
                    ExtraWorkStatus -= 2;
                else if (DepLevel == 2)
                {
                    ExtraWorkStatus -= 1;
                    CurrentDivision.RemovePerk(91);
                }
                else if (DepLevel == 3)
                {
                    ExtraWorkStatus -= 2;
                    CurrentDivision.RemovePerk(91);
                }
            };
            ActiveMode = 0;
        }
        else if (building.Type == BuildingType.会计办公室)
        {
            WeakAction = () => { };
            UnWeakAction = () => { };
            AddLevelEffect = () =>
            {
                if (DepLevel == 1)
                    GC.ExtraExpense -= 20;
                else if (DepLevel == 2)
                {
                    GC.ExtraExpense -= 15;
                    GC.AddPerk(new Perk92());
                }
                else if (DepLevel == 3)
                {
                    GC.ExtraExpense -= 25;
                    GC.AddPerk(new Perk92());
                    GC.AddPerk(new Perk92());
                }
            };
            RemoveLevelEffect = () =>
            {
                if (DepLevel == 1)
                    GC.ExtraExpense += 20;
                else if (DepLevel == 2)
                {
                    GC.ExtraExpense += 15;
                    GC.RemovePerk(92);
                }
                else if (DepLevel == 3)
                {
                    GC.ExtraExpense += 25;
                    GC.RemovePerk(92);
                    GC.RemovePerk(92);
                }
            };
            ActiveMode = 0;
        }//没弄完
        else if (building.Type == BuildingType.心理咨询室)
        {
            WeakAction = () => { ExtraProduceLimit += 1; };
            UnWeakAction = () => { ExtraProduceLimit -= 1; };
            AddLevelEffect = () =>
            {
                if (DepLevel == 1)
                {
                    ProducePointLimit  = 3;
                    canWork = true;
                }
                else if (DepLevel == 2)
                {
                    ProducePointLimit = 1;
                    GC.AddPerk(new Perk97());
                }
            };
            RemoveLevelEffect = () =>
            {
                if (DepLevel == 1)
                {
                    ProducePointLimit = 0;
                    canWork = false;
                }
                else if (DepLevel == 2)
                {
                    ProducePointLimit = 3;
                    GC.RemovePerk(97);
                }
            };
            DepEffect = () =>
            {
                GC.CurrentEmpInfo.emp.Mentality += 15;
            };
            ActiveMode = 2;
        }
        else if (building.Type == BuildingType.智库小组)
        {
            WeakAction = () => { ExtraProduceLimit += 1; };
            UnWeakAction = () => { ExtraProduceLimit -= 1; };
            AddLevelEffect = () =>
            {
                if (DepLevel == 1)
                {
                    ProducePointLimit = 3;
                    canWork = true;
                }
                else if (DepLevel == 2)
                {
                    ProducePointLimit = 1;
                    GC.AddPerk(new Perk98());
                    GC.AddPerk(new Perk98());
                }
            };
            RemoveLevelEffect = () =>
            {
                if (DepLevel == 1)
                {
                    ProducePointLimit = 0;
                    canWork = false;
                }
                else if (DepLevel == 2)
                {
                    ProducePointLimit = 3;
                    CurrentDivision.RemovePerk(98);
                    CurrentDivision.RemovePerk(98);
                }
            };
            DepEffect = () =>
            {
                GC.CreateItem(2);
            };
            ActiveMode = 2;
        }//状态没弄完
        else if (building.Type == BuildingType.仓库)
        {
            WeakAction = () => 
            {
                if (DepLevel > 0)
                    GC.RemovePerk(106);
            };
            UnWeakAction = () => 
            {
                if (DepLevel > 0)
                    GC.AddPerk(new Perk106());
            };
            AddLevelEffect = () =>
            {
                if (DepLevel == 1)
                {
                    if (isWeakend == false)
                        GC.AddPerk(new Perk106());
                }
                else if (DepLevel == 2)
                {
                    CurrentDivision.AddPerk(new Perk105());
                    GC.AddPerk(new Perk106());
                    GC.AddPerk(new Perk106());
                }
            };
            RemoveLevelEffect = () =>
            {
                if (DepLevel == 1)
                {
                    CurrentDivision.RemovePerk(106);
                }
                else if (DepLevel == 2)
                {
                    GC.RemovePerk(106);
                    GC.RemovePerk(106);
                    CurrentDivision.RemovePerk(105);
                }
            };
            ActiveMode = 0;
        }//状态没弄完
        else if (building.Type == BuildingType.原型图画室)
        {
            ExtraEfficiency = -4;
            WeakAction = () => { ExtraEfficiency -= 2; };
            UnWeakAction = () => { ExtraEfficiency += 2; };
            AddLevelEffect = () =>
            {
                if (DepLevel == 1)
                {
                    ExtraEfficiency -= 4;
                    canWork = true;
                }
                else if (DepLevel == 2)
                    ExtraEfficiency += 1;
            };
            RemoveLevelEffect = () =>
            {
                if (DepLevel == 1)
                {
                    ExtraEfficiency += 4;
                    canWork = false;
                }
                else if (DepLevel == 2)
                    ExtraEfficiency -= 1;
            };
            ActiveMode = 5;
        }
        else if (building.Type == BuildingType.算法小组)
        {
            ExtraEfficiency = -4;
            WeakAction = () => { ExtraWorkStatus -= 2; };
            UnWeakAction = () => { ExtraWorkStatus += 2; };
            AddLevelEffect = () =>
            {
                if (DepLevel == 1)
                {
                    ExtraWorkStatus -= 5;
                    canWork = true;
                }
                else if (DepLevel == 2)
                    ExtraWorkStatus += 1;
            };
            RemoveLevelEffect = () =>
            {
                if (DepLevel == 1)
                {
                    ExtraWorkStatus += 5;
                    canWork = false;
                }
                else if (DepLevel == 2)
                    ExtraWorkStatus -= 1;
            };
            ActiveMode = 5;
        }
        //需要手动激活的才显示激活按钮
        if (ActiveMode == 2 || ActiveMode == 3 || ActiveMode == 4)
            ActiveButton.gameObject.SetActive(true);
    }
    //生成单图标记
    void InitMarker(string describe, string debuff, int index)
    {
        EmpEffect effect = Instantiate(EmpEffectPrefab, EmpEffectContent).GetComponent<EmpEffect>();
        effect.dep = this;
        effect.InitEffect(describe);
        effect.InitDebuff(debuff);
        EmpEffectMarkers.Add(effect.Marker1);
        effect.EmpIndex = index;
    }
    //生成双图标记
    void InitDoubleMarker(string describe, string debuff, int index)
    {
        EmpEffect effect = Instantiate(EmpEffect2Prefab, EmpEffectContent).GetComponent<EmpEffect>();
        effect.dep = this;
        effect.InitEffect(describe);
        effect.InitDebuff(debuff);
        EmpEffectMarkers.Add(effect.Marker1);
        EmpEffectMarkers.Add(effect.Marker2);
        effect.EmpIndex = index;
    }

    void AddSubBulding()
    {
        GC.BM.EnterBuildMode();
        CurrentDivision.SetDetailPanel(false);
        GC.TotalEmpPanel.SetWndState(false);
        GC.ResetSelectMode();
        GC.HC.StorePanel.SetWndState(false);
        Building b = Instantiate(GC.BM.SubDepPrefab);
        b.CanDismantle = false;
        SubBuildings.Add(b);
        b.MasterBuilding = building;
        GC.BM.StartBuild(b);
    }
    void RemoveSubBuilding()
    {
        if (SubBuildings.Count == 0)
            return;
        int a = SubBuildings.Count - 1;
        if (SubBuildings[a].WarePanel != null)
        {
            GC.BM.BuildingWindow.warePanels.Remove(SubBuildings[a].WarePanel);
            Destroy(SubBuildings[a].WarePanel.gameObject);
        }
        SubBuildings[a].CanDismantle = true;
        GC.BM.DismantleBuilding(SubBuildings[a]);
        SubBuildings.RemoveAt(a);
    }

    //作弊模式
    public void ToggleCheatMode(bool value)
    {
        CheatMode = value;
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

    public int CalcCost(int type, bool CalcMeetingResult = false)
    {
        int value = 0;
        //工资
        if (type == 1)
        {
            float multiply;

            //计算月会临时效果（显示用）
            if (CalcMeetingResult == true && building.CurrentArea.CA != null && building.CurrentArea.CA.BlueCount > 0)
                multiply = GC.TotalSalaryMultiply * (SalaryMultiply - (0.25f * building.CurrentArea.CA.BlueCount));
            else
                multiply = GC.TotalSalaryMultiply * SalaryMultiply;

            foreach (Employee emp in CurrentEmps)
            {
                value += (int)(emp.InfoDetail.CalcSalary() * multiply);
            }
        }
        //维护费
        else if (type == 2)
        {
            float multiply;

            //计算月会临时效果（显示用）
            if (CalcMeetingResult == true && building.CurrentArea.CA != null && building.CurrentArea.CA.BlueCount > 0)
                multiply = GC.TotalSalaryMultiply * (BuildingPayMultiply - (0.25f * building.CurrentArea.CA.BlueCount));
            else
                multiply = GC.TotalBuildingPayMultiply * BuildingPayMultiply;

            value = (int)(building.MaintainCost * multiply);
        }
        return value;
    }

    //事业部相关
    public void SetDivision(DivisionControl DC)
    {
        if (CurrentDivision != null)
            RemoveDivision();
        DivPanel.transform.parent = DC.DepContent;
        DC.CurrentDeps.Add(this);
        CurrentDivision = DC;
        CurrentDivision.DepExtraCheck();
        CurrentDivision.Text_DivName.gameObject.SetActive(true);
    }

    //从现有的事业部中移除
    void RemoveDivision()
    {
        if (CurrentDivision == null)
            return;
        CurrentDivision.CurrentDeps.Remove(this);
        CurrentDivision.DepExtraCheck();
        if (CurrentDivision.CurrentDeps.Count == 0)
        {
            CurrentDivision.Text_DivName.gameObject.SetActive(false);
            CurrentDivision.SetManager(true);
        }
    }


}
