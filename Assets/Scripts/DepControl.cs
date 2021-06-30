using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DepControl : MonoBehaviour
{

    //BuildingMode用于区分建筑模式  ActiveMode用于区分激活方式 0无法激活 1直接激活 2选择员工 3选择部门 4选择区域 5卡牌生产
    [HideInInspector] public int ProducePointLimit = 20, ActiveMode = 1, Mode2EffectValue = -2;
    [HideInInspector] public int DepFaith = 50, BaseWorkStatus;//基本工作状态;

    public int DepLevel = 0;//部门等级,用于计算效果
    public int StaminaExtra = 0;//特质导致的每周体力buff
    public int EmpLimit;
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
    private int PCLevel = 1;//心理咨询室的等级
    private int PsychoCSlotNum = 0;//当前选中的心力回复槽位，用于将选中的员工放置在对应的槽位上
    private bool TipShowed = false, isWeakend = false;

    Action WeakAction, UnWeakAction, AddLevelEffect = () => { }, RemoveLevelEffect = () => { }, DepEffect = () => { };//弱化效果，弱化清除效果，等级增加效果，等级减少效果，部门激活效果
    public Area TargetArea;
    public Employee Manager;
    public Transform EmpContent, PerkContent, EmpPanel, DivPanel, EmpMarkerContent, EmpEffectContent;
    public GameObject EmpEffectPrefab, EmpEffect2Prefab, EmpMarkerPrefab, PsychoCUpgradeButton;//最后一个是心理咨询室升级按钮
    public Button ActiveButton;
    public Building building = null;
    public GameControl GC;
    public DepSelect DS;
    public DepControl CommandingOffice;
    public DivisionControl CurrentDivision;
    public Text Text_DepName, Text_DepName2, Text_DepName3, Text_WeakEffect, Text_InfoProgress, Text_DivProgress, Text_DepFunction,
        Text_StopWork;

    public List<Employee> AffectedEmps = new List<Employee>() { null, null, null};
    public List<Employee> CurrentEmps = new List<Employee>();
    public List<Building> SubBuildings = new List<Building>();//保存的所有附加建筑
    public List<PerkInfo> CurrentPerks = new List<PerkInfo>();
    public List<Image> EmpMarkers = new List<Image>();
    public List<Image> EmpEffectMarkers = new List<Image>();
    //心理咨询室相关
    public List<GameObject> PsychoCPanel = new List<GameObject>();//3个槽位的面板
    public List<GameObject> PsychoCSelectButtons = new List<GameObject>();//3个槽位的选人按钮
    public List<Text> Text_PCInfos = new List<Text>();//3个槽位的进驻人信息
    public List<GameObject> PsychoCRemoveButtons = new List<GameObject>();//3个槽位的删除按钮

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
        else if ((ActiveMode == 2 || ActiveMode == 3 || ActiveMode == 4) && building.Type != BuildingType.心理咨询室)
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
        if (CurrentDivision != null)
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
    public int CheckSkillType(Employee emp)
    {
        if (building.Type != BuildingType.商战建筑)
        {
            int count = 0;
            foreach (int type in emp.Professions)
            {
                if (type == building.effectValue)
                    count += 1;
            }
            return count;
        }
        else
        {
            int EmptyCount = 0;
            int professionCount = 0;//当满足多个卡时取满足的最多的那张卡
            foreach (CWCardInfo info in CurrentDivision.CWCards)
            {
                if (info.CurrentCard != null)
                {
                    int cardProCount = 0;
                    foreach (int type in emp.Professions)
                    {
                        if (type == (int)info.CurrentCard.TypeRequire)
                            cardProCount += 1;
                    }
                    if (cardProCount > professionCount)
                        professionCount = cardProCount;
                }
                else
                    EmptyCount += 1;
            }
            //没有卡则返回真
            if (EmptyCount == CurrentDivision.CWCards.Count)
                return 1;
            else
                return professionCount;
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
        {
            GC.CreateMessage("有未处理的抉择事件");
            return;
        }
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
            //有多重岗位优势后可能一次性降低1等级以上,所以要进行降低等级数量的检测
            for (int i = 0; i < DepLevel - level; i++)
            {
                RemoveLevelEffect();
                DepLevel -= 1;
            }
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
        int PExtra = 0;//额外岗位优势替代的员工数量
        for (int i = 0; i < EmpLimit; i++)
        {
            //超过图片index的话直接结束（在之前的遍历中会遍历可能略过的黄色图标）
            if (i + PExtra >= EmpLimit)
                break;
            EmpEffect Ee = EmpEffectMarkers[i + PExtra].transform.parent.gameObject.GetComponent<EmpEffect>();
            if (CurrentEmps.Count > i)
            {
                int pCount = CheckSkillType(CurrentEmps[i]);
                //符合技能1项需求的标绿
                if (pCount == 1)
                {
                    EmpEffectMarkers[i + PExtra].color = Color.green;
                    EmpMarkers[i + PExtra].color = Color.green;
                    EmpEffectMarkers[i + PExtra].GetComponent<Outline>().enabled = false;
                    EmpMarkers[i + PExtra].GetComponent<Outline>().enabled = false;
                }
                //符合多项需求的标绿并加黄框
                else if (pCount > 1)
                {
                    EmpEffectMarkers[i + PExtra].color = Color.green;
                    EmpMarkers[i + PExtra].color = Color.green;
                    EmpEffectMarkers[i + PExtra].GetComponent<Outline>().enabled = true;
                    EmpMarkers[i + PExtra].GetComponent<Outline>().enabled = true;
                }
                //不符合的标红
                else
                {
                    EmpEffectMarkers[i + PExtra].color = Color.red;
                    EmpMarkers[i + PExtra].color = Color.red;
                    weak = true;
                    EmpEffectMarkers[i + PExtra].GetComponent<Outline>().enabled = false;
                    EmpMarkers[i + PExtra].GetComponent<Outline>().enabled = false;
                }
                Ee.SetProfessionCount(pCount, CurrentEmps[i], EmpEffectMarkers[i + PExtra]);
                EmpEffectMarkers[i + PExtra].gameObject.GetComponentInChildren<Text>().text = CurrentEmps[i].Name;
                //设置额外岗位优势效果
                if (CurrentEmps[i].ProfessionUse > 1)
                {
                    for (int j = i + PExtra + 1; j < i + CurrentEmps[i].ProfessionUse + PExtra; j++)
                    {
                        if (j >= EmpLimit)
                            break;
                        EmpEffectMarkers[j].color = Color.yellow;
                        EmpMarkers[j].color = Color.yellow;

                        EmpEffectMarkers[j].GetComponent<Outline>().enabled = false;
                        EmpMarkers[j].GetComponent<Outline>().enabled = false;

                        EmpEffect Ee2 = EmpEffectMarkers[j].transform.parent.gameObject.GetComponent<EmpEffect>();
                        print(EmpEffectMarkers[j]);
                        Ee2.SetProfessionCount(0, null, EmpEffectMarkers[j]);

                        EmpEffectMarkers[j].gameObject.GetComponentInChildren<Text>().text = "";
                    }
                    PExtra += (CurrentEmps[i].ProfessionUse - 1);
                }
            }
            //超出人数的标白
            else
            {
                EmpEffectMarkers[i + PExtra].color = Color.white;
                EmpMarkers[i + PExtra].color = Color.white;
                EmpEffectMarkers[i + PExtra].gameObject.GetComponentInChildren<Text>().text = "";
                EmpEffectMarkers[i + PExtra].GetComponent<Outline>().enabled = false;
                EmpMarkers[i + PExtra].GetComponent<Outline>().enabled = false;
                Ee.SetProfessionCount(0, null, EmpEffectMarkers[i + PExtra]);
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
        if (building.EmpCount[2] != 0 && CurrentEmps.Count + PExtra >= (building.EmpCount[0] + building.EmpCount[1] + building.EmpCount[2]))
            SetDepLevel(3);
        else if (building.EmpCount[1] != 0 && CurrentEmps.Count + PExtra >= (building.EmpCount[0] + building.EmpCount[1]))
            SetDepLevel(2);
        else if (building.EmpCount[0] != 0 && CurrentEmps.Count + PExtra >= building.EmpCount[0])
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
        if (CurrentDivision != null)
            CurrentDivision.ExtraStatusCheck();
        UpdateUI();
    }

    //设置人员上限和人员标记以及一些额外效果
    public void SetDepStatus()
    {
        EmpLimit = int.Parse(building.Jobs);

        //心理咨询室有自己的面板
        if (building.Type != BuildingType.心理咨询室)
        {
            for (int i = 0; i < EmpLimit; i++)
            {
                Image image = Instantiate(EmpMarkerPrefab, EmpMarkerContent).GetComponent<Image>();
                EmpMarkers.Add(image);
                EmpEffect Ee = image.gameObject.GetComponent<EmpEffect>();
                Ee.dep = this;
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
                    GC.ExtraCost -= 20;
                else if (DepLevel == 2)
                {
                    GC.ExtraCost -= 15;
                    GC.AddPerk(new Perk92());
                }
                else if (DepLevel == 3)
                {
                    GC.ExtraCost -= 25;
                    GC.AddPerk(new Perk92());
                    GC.AddPerk(new Perk92());
                }
            };
            RemoveLevelEffect = () =>
            {
                if (DepLevel == 1)
                    GC.ExtraCost += 20;
                else if (DepLevel == 2)
                {
                    GC.ExtraCost += 15;
                    GC.RemovePerk(92);
                }
                else if (DepLevel == 3)
                {
                    GC.ExtraCost += 25;
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
                    PsychoCSelectButtons[0].SetActive(true);
                    Text_PCInfos[0].transform.parent.gameObject.SetActive(true);
                }
                else if (DepLevel == 2)
                {
                    PsychoCSelectButtons[1].SetActive(true);
                    Text_PCInfos[1].transform.parent.gameObject.SetActive(true);
                }
                else if (DepLevel == 3)
                {
                    PsychoCSelectButtons[2].SetActive(true);
                    Text_PCInfos[2].transform.parent.gameObject.SetActive(true);
                }
            };
            RemoveLevelEffect = () =>
            {
                int MaxNum = 0;
                foreach (Employee emp in CurrentEmps)
                {
                    MaxNum += emp.ProfessionUse;
                }
                //先移除多余槽位上的员工
                for (int i = 0; i < 3; i++)
                {
                    if (AffectedEmps[i] != null && i >= MaxNum)
                        RemovePCTarget(i);
                }
                if (DepLevel == 1)
                {
                    PsychoCSelectButtons[0].SetActive(false);
                    Text_PCInfos[0].transform.parent.gameObject.SetActive(false);
                }
                else if (DepLevel == 2)
                {
                    PsychoCSelectButtons[1].SetActive(false);
                    Text_PCInfos[1].transform.parent.gameObject.SetActive(false);
                }
                else if (DepLevel == 3)
                {
                    PsychoCSelectButtons[2].SetActive(false);
                    Text_PCInfos[2].transform.parent.gameObject.SetActive(false);
                }
            };
            DepEffect = () =>
            {
                AddTarget();
            };
            ActiveMode = 2;
            //初始改为1岗位
            EmpLimit = 1;
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
        }
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
        else if (building.Type == BuildingType.动力舱)
        {
            WeakAction = () => { };
            UnWeakAction = () => { };
            AddLevelEffect = () =>
            {
                if (DepLevel == 1)
                    GC.CrC.SetPower(1);
                else if (DepLevel == 2)
                    GC.CrC.SetPower(2);
                else if (DepLevel == 3)
                    GC.CrC.SetPower(3);
            };
            RemoveLevelEffect = () =>
            {
                if (DepLevel == 1)
                    GC.CrC.SetPower(0);
                else if (DepLevel == 2)
                    GC.CrC.SetPower(1);
                else if (DepLevel == 3)
                    GC.CrC.SetPower(2);
            };
            ActiveMode = 5;
        }

        //需要手动激活的才显示激活按钮
        if ((ActiveMode == 2 || ActiveMode == 3 || ActiveMode == 4) && building.Type != BuildingType.心理咨询室)
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
        effect.DepMarker1 = EmpMarkers[index].GetComponent<EmpEffect>();
        effect.SetDepMarkerRef();
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
        effect.DepMarker1 = EmpMarkers[index].GetComponent<EmpEffect>();
        effect.DepMarker2 = EmpMarkers[index + 1].GetComponent<EmpEffect>();
        effect.SetDepMarkerRef();
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
            //DivPanel.gameObject.SetActive(false);
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


    //心理咨询室相关
    //————————————————————————————————————————
    //移除一个回复槽位上的员工
    public void RemovePCTarget(int num)
    {
        AffectedEmps[num].CurrentDep = null;
        AffectedEmps[num] = null;
        PsychoCRemoveButtons[num].SetActive(false);
        PsychoCSelectButtons[num].SetActive(true);
    }

    //设定一个槽位开始选择员工
    public void StartPCSelect(int num)
    {
        PsychoCSlotNum = num;
        GC.CurrentDep = this;
        GC.SelectMode = 5;
        GC.Text_EmpSelectTip.text = "选择一个员工";
        GC.TotalEmpPanel.SetWndState(true);
        //可调整回合中所有人应该都可以进入
        if (GC.AdjustTurn == 0)
        {
            foreach (Employee emp in GC.CurrentEmployees)
            {
                if (emp.CurrentDep == this)
                    emp.InfoB.gameObject.SetActive(false);
            }
        }
        //非调整回合只能让待命员工进入
        else
        {
            foreach (Employee emp in GC.CurrentEmployees)
            {
                if (emp.CurrentDep != null || emp.CurrentDivision != null)
                    emp.InfoB.gameObject.SetActive(false);
            }
        }
    }

    //向回复槽位添加一个员工
    private void AddTarget()
    {
        Employee e = GC.CurrentEmpInfo.emp;
        GC.ResetOldAssignment(e);
        AffectedEmps[PsychoCSlotNum] = e;
        e.CurrentDep = this;
        PsychoCSelectButtons[PsychoCSlotNum].SetActive(false);
        PsychoCRemoveButtons[PsychoCSlotNum].SetActive(true);
        Text_PCInfos[PsychoCSlotNum].text = GC.CurrentEmpInfo.emp.Name + "\n心力:" + GC.CurrentEmpInfo.emp.Mentality + "/" + GC.CurrentEmpInfo.emp.MentalityLimit;
    }

    //心理咨询室升级
    public void AddPCLevel()
    {
        if (GC.Money >= 300)
        {
            PsychoCPanel[PCLevel].SetActive(true);
            EmpMarkers[PCLevel].gameObject.SetActive(true);
            PCLevel += 1;
            EmpLimit += 1;
            GC.Money -= 300;
            if (PCLevel == 3)
                PsychoCUpgradeButton.SetActive(false);
        }
        else
            GC.CreateMessage("金钱不足");
    }

    //恢复心力
    public void RestoreMentality()
    {
        for(int i = 0; i < 3; i++)
        {
            if (AffectedEmps[i] != null)
            {
                AffectedEmps[i].Mentality += 1000;
                RemovePCTarget(i);
            }
        }
    }

    //进入下一回合后关闭移除按钮
    public void CloseRemoveButton()
    {
        foreach(GameObject g in PsychoCRemoveButtons)
        {
            g.SetActive(false);
        }
    }
}
