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
    public int StopWorkTime//停工时间
    {
        get
        {
            if (stopWorkTime == 0)
                Text_StopWork.gameObject.SetActive(false);
            else
            {
                Text_StopWork.gameObject.SetActive(true);
                Text_StopWork.text = "部门停工" + stopWorkTime + "回合";
            }
            return stopWorkTime;
        }
        set { stopWorkTime = value; }
    }

    public bool canWork = false, DefautDep = false;

    private int stopWorkTime = 0;//停工时间
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
    public Text Text_DepName, Text_DepName2, Text_DepName3, Text_WeakEffect, Text_InfoProgress, Text_DivProgress, Text_DepFunction,
        Text_StopWork;


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
        if (stopWorkTime > 0)
        {
            stopWorkTime -= 1;
            return;
        }

        //工作结算
        if (canWork == true)
        {
            //没有员工的情况下直接return，防止完成任务
            if (CurrentEmps.Count == 0)
                return;
            //进度增加
            if (SpProgress < (ProducePointLimit + ExtraProduceLimit + CurrentDivision.ExtraProduceTime))
                SpProgress += 1;

            if (SpProgress >= (ProducePointLimit + ExtraProduceLimit + CurrentDivision.ExtraProduceTime))
            {
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
        if (building.Type != BuildingType.商战建筑)
        {
            foreach (int type in emp.Professions)
            {
                if (type == building.effectValue)
                    return true;
            }
        }
        else
        {
            int EmptyCount = 0;
            foreach (CWCardInfo info in CurrentDivision.CWCards)
            {
                if (info.CurrentCard != null)
                {
                    foreach (int type in emp.Professions)
                    {
                        if (type == (int)info.CurrentCard.TypeRequire)
                            return true;
                    }
                }
                else
                    EmptyCount += 1;
            }
            //没有卡则返回真
            if (EmptyCount == CurrentDivision.CWCards.Count)
                return true;
        }
        return false;
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

    //部门等级变化
    private void SetDepLevel(int level)
    {
        //变换前等级比现在高时清空高级效果,低的时候添加额外效果
        if (DepLevel > level)
        {
            RemoveLevelEffect();
            print("RemoveLevel");
            DepLevel = level;
        }
        else if (DepLevel < level)
        {
            DepLevel = level;
            AddLevelEffect();
            print("AddLevel");
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

        if (CurrentEmps.Count == 0)
            weak = true;

        //检测削弱效果和等级效果
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

                //商战建筑检测事业部卡牌
        if (building.Type == BuildingType.商战建筑)
        {
            foreach (CWCardInfo info in CurrentDivision.CWCards)
            {
                if (info.CurrentCard != null)
                    info.WeakCheck(this);
            }
        }

        CurrentDivision.ExtraStatusCheck();
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
        }
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
        }
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
        else if (building.Type == BuildingType.商战建筑)
        {
            WeakAction = () => { };
            UnWeakAction = () => { };
            AddLevelEffect = () =>
            {
                if (DepLevel == 1)
                    canWork = true;
            };
            RemoveLevelEffect = () =>
            {
                if (DepLevel == 1)
                    canWork = false;
            };
            ActiveMode = 5;
        }
        //else if (building.Type == BuildingType.算法小组)
        //{
        //    ExtraEfficiency = -4;
        //    WeakAction = () => { ExtraWorkStatus -= 2; };
        //    UnWeakAction = () => { ExtraWorkStatus += 2; };
        //    AddLevelEffect = () =>
        //    {
        //        if (DepLevel == 1)
        //        {
        //            ExtraWorkStatus -= 5;
        //            canWork = true;
        //        }
        //        else if (DepLevel == 2)
        //            ExtraWorkStatus += 1;
        //    };
        //    RemoveLevelEffect = () =>
        //    {
        //        if (DepLevel == 1)
        //        {
        //            ExtraWorkStatus += 5;
        //            canWork = false;
        //        }
        //        else if (DepLevel == 2)
        //            ExtraWorkStatus -= 1;
        //    };
        //    ActiveMode = 5;
        //}
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
        CurrentDivision.ExtraStatusCheck();
        CurrentDivision.Text_DivName.gameObject.SetActive(true);

        //自身是商战建筑时在DivisionControl中设定相关引用
        if (building.Type == BuildingType.商战建筑)
        {
            DC.CWDep = this;
            DivPanel.gameObject.SetActive(false);
        }
        EmpEffectCheck();
    }

    //从现有的事业部中移除
    void RemoveDivision()
    {
        if (CurrentDivision == null)
            return;
        if (isWeakend == true)
        {
            isWeakend = false;
            UnWeakAction();
        }
        CurrentDivision.CurrentDeps.Remove(this);
        CurrentDivision.ExtraStatusCheck();
        //（旧）事业部内没有部门时移除高管
        if (CurrentDivision.CurrentDeps.Count == 0)
        {
            CurrentDivision.Text_DivName.gameObject.SetActive(false);
            CurrentDivision.SetManager(true);
        }
    }


}
