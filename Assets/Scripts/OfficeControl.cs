using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OfficeControl : MonoBehaviour
{
    static public float OfficeBaseSuccessRate = 1.0f;
    static public float OfficeBaseMajorSuccessRate = 0.2f;
    static public float OfficeBaseMajorFailureRate = 0.4f;

    public bool CanWork = false;
    public int ManageValue = 0, Progress = 100, MaxProgress = 96;
    public int OfficeMode = 1;//1决策 战略充能 2人力 心力回复(说服) 3管理 加启发 4招聘 5行业 刷新建筑

    public GameObject OfficeWarning;
    public Transform SRateDetailPanel;
    public Employee CurrentManager;
    public OfficeControl CommandingOffice;
    public DepSelect DS;
    public GameControl GC;
    public Building building;
    public Text Text_OfficeName, Text_EmpName, Text_MAbility, Text_Progress, Text_OfficeMode, Text_TimeLeft, Text_SuccessRate, Text_SRateDetail;
    public Button ActiveButton, ModeChangeButton, SetCommaindgOfficeButton;

    public List<DepControl> ControledDeps = new List<DepControl>();
    public List<OfficeControl> ControledOffices = new List<OfficeControl>();
    public List<OfficeControl> InRangeOffices = new List<OfficeControl>();

    private void Start()
    {
        if (building.Type == BuildingType.CEO办公室)
        {
            GC.HourEvent.AddListener(TimePass);
        }
        Progress = 0;
        UpdateUI();
    }

    //放入和移除高管时调用
    public void SetOfficeStatus()
    {
        if (CurrentManager != null)
        {
            Text_EmpName.text = "当前高管:" + CurrentManager.Name;
            if (building.Type == BuildingType.高管办公室 || building.Type == BuildingType.CEO办公室)
            {
                if (OfficeMode == 1)
                    Text_MAbility.text = "决策:" + CurrentManager.Decision;
                else if (OfficeMode == 2 || OfficeMode == 4)
                    Text_MAbility.text = "人力:" + CurrentManager.HR;
                else if (OfficeMode == 3 || OfficeMode == 5)
                    Text_MAbility.text = "管理:" + CurrentManager.Manage;
                ManageValue = CurrentManager.Manage;
                CurrentManager.InfoDetail.CreateStrategy();
                CurrentManager.NoPromotionTime = 0;
                for(int i = 0; i < CurrentManager.InfoDetail.PerksInfo.Count; i++)
                {
                    if (CurrentManager.InfoDetail.PerksInfo[i].CurrentPerk.Num == 32)
                    {
                        CurrentManager.InfoDetail.PerksInfo[i].CurrentPerk.RemoveEffect();
                        break;
                    }
                }
                CheckManage();
                UpdateUI();
            }
        }
        else
        {
            Text_EmpName.text = "当前高管:无";
            Text_MAbility.text = "能力:--";
            ManageValue = 0;
        }

    }

    public void CheckManage()
    {
        SetManageValue();
        int value = ManageValue;
        if (CanWork == false)
            value = 0;
        for(int i = 0; i < ControledDeps.Count; i++)
        {
            if (i < value)
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
        for (int i = 0; i < ControledOffices.Count; i++)
        {
            if (i < value - ControledDeps.Count)
            {
                ControledOffices[i].CanWork = true;
                ControledOffices[i].OfficeWarning.SetActive(false);
                GC.QC.Finish(9);
            }
            else
            {
                ControledOffices[i].CanWork = false;
                ControledOffices[i].OfficeWarning.SetActive(true);
            }
            ControledOffices[i].CheckManage();
            ControledOffices[i].UpdateUI();
        }
    }

    void SetManageValue()
    {
        if (CurrentManager != null)
        {
            if (CurrentManager.Manage < 6)
                ManageValue = 1;
            else if (CurrentManager.Manage < 11)
                ManageValue = 2;
            else if (CurrentManager.Manage < 16)
                ManageValue = 3;
            else if (CurrentManager.Manage < 21)
                ManageValue = 4;
            else
                ManageValue = 5;
        }
        else
            ManageValue = 0;
    }

    public void SetName()
    {
        int num = 1;
        GC.HourEvent.AddListener(TimePass);
        if (building.Type != BuildingType.高管办公室 && building.Type != BuildingType.CEO办公室)
        {
            Text_Progress.gameObject.SetActive(true);
            SetCommaindgOfficeButton.gameObject.SetActive(true);
            OfficeWarning.SetActive(true);
            ModeChangeButton.gameObject.SetActive(false);
            ActiveButton.gameObject.SetActive(true);
            Text_SuccessRate.gameObject.SetActive(false);
            Text_OfficeMode.gameObject.SetActive(false);
            Text_TimeLeft.gameObject.SetActive(false);
            num = 1;
        }
        Text_OfficeName.text = building.Type.ToString() + num;

    }

    public void BuildingActive()
    {
        //建筑物特效
        bool ActiveSuccess = true;
        float Posb = Random.Range(0.0f, 1.0f);
        float extra = GC.BuildingSkillSuccessExtra, extra2 = 0;
        if (extra > 0.2f)
            extra2 = extra - 0.2f;

        if (building.Type == BuildingType.心理咨询室)
        {
            //还没写！
        }
        else if (building.Type == BuildingType.茶水间)
        {
            List<DepControl> TDep = new List<DepControl>();
            for (int i = 0; i < building.effect.AffectedBuildings.Count; i++)
            {
                if (building.effect.AffectedBuildings[i].Department != null)
                {
                    TDep.Add(building.effect.AffectedBuildings[i].Department);
                }
            }
            int value = 0;
            if (Posb < 0.2f - extra)
                value = 50;
            else if (Posb < 0.8f - extra2)
                value = 30;
            else
                value = 10;
            for (int a = 0; a < TDep.Count; a++)
            {
                for (int i = 0; i < TDep[a].CurrentEmps.Count; i++)
                {
                    TDep[a].CurrentEmps[i].Stamina += value;
                }
            }
            if (TDep.Count == 0)
                ActiveSuccess = false;
        }
        else if (building.Type == BuildingType.中央厨房)
        {
            List<DepControl> TDep = new List<DepControl>();
            for (int i = 0; i < building.effect.AffectedBuildings.Count; i++)
            {
                if (building.effect.AffectedBuildings[i].Department != null)
                {
                    TDep.Add(building.effect.AffectedBuildings[i].Department);
                }
            }
            int value = 0;
            if (Posb < 0.2f)
                value = 30;
            else if (Posb < 0.8f)
                value = 20;
            else
                value = 10;
            for (int a = 0; a < TDep.Count; a++)
            {
                for (int i = 0; i < TDep[a].CurrentEmps.Count; i++)
                {
                    TDep[a].CurrentEmps[i].Stamina += value;
                    float Posb2 = Random.Range(0.0f, 1.0f);
                    if (Posb2 < 0.3f)
                        TDep[a].CurrentEmps[i].InfoDetail.AddPerk(new Perk3(TDep[a].CurrentEmps[i]));
                }
            }
            if (TDep.Count == 0)
                ActiveSuccess = false;
        }
        if (ActiveSuccess == true)
        {
            Progress = 0;
            Text_Progress.text = "激活进度:0%";
            ActiveButton.interactable = false;
            GC.Stamina -= building.StaminaRequest;
        }
    }

    //需要选择部门的建筑物特效，无需选择就直接施加效果
    public void ShowAvailableDeps()
    {
        if (GC.Stamina >= building.StaminaRequest)
        {
            //GC.CurrentOffice = this;
            //GC.SelectMode = 5;
            //if (building.Type == BuildingType.人力资源部A)
            //    GC.ShowDepSelectPanel(GC.CurrentDeps);
            //else if (building.Type == BuildingType.高级财务部B)
            //    GC.TotalEmpContent.parent.parent.gameObject.SetActive(true);
            //else
            BuildingActive();
        }
    }

    //显示所有能当上级的办公室
    public void ShowAvailableOffices()
    {
        GC.SelectMode = 8;
        //GC.ShowDepSelectPanel(this);
        //GC.CurrentOffice = this;
    }

    public void ShowModeSelectPanel()
    {
        //GC.OfficeModeSelectPanel.GetComponent<WindowBaseControl>().SetWndState(true);
        //GC.CurrentOffice = this;
        //if (building.Type == BuildingType.CEO办公室)
        //{
        //    GC.OfficeModeBuildingOptionButton.SetActive(true);
        //    GC.OfficeModeTalkOptionButton.SetActive(false);
        //}
        //else
        //{
        //    GC.OfficeModeBuildingOptionButton.SetActive(false);
        //    GC.OfficeModeTalkOptionButton.SetActive(true);
        //}
    }

    public float CountSuccessRate()
    {
        float BaseSRate = OfficeBaseSuccessRate;
        Text_SRateDetail.text = "";
        if(CurrentManager != null)
        {
            int value = 0;
            float EValue = 0;
            if (OfficeMode == 2 || OfficeMode == 4)
            {
                value = CurrentManager.HR;
                Text_SRateDetail.text += CurrentManager.Name + "人力技能:";
            }
            else if (OfficeMode == 3 || OfficeMode == 5)
            { 
                value = CurrentManager.Manage;
                Text_SRateDetail.text += CurrentManager.Name + "管理技能:";
            }
            else if (OfficeMode == 1)
            { 
                value = CurrentManager.Decision;
                Text_SRateDetail.text += CurrentManager.Name + "决策技能:";
            }
            if (value <= 5)
                EValue -= 0.15f;
            else if (value <= 9)
                EValue -= 0.1f;
            else if (value <= 13)
                EValue += 0.05f;
            else if (value <= 17)
                EValue += 0;
            else if (value <= 21)
                EValue += 0.04f;
            else if (value > 21)
                EValue += 0.08f;
            BaseSRate += EValue;
            BaseSRate += CurrentManager.ExtraSuccessRate;

            //文字显示
            Text_SRateDetail.text += (EValue * 100) + "%";
            if (Mathf.Abs(CurrentManager.ExtraSuccessRate) > 0.001f)
            {
                foreach (PerkInfo perk in CurrentManager.InfoDetail.PerksInfo)
                {
                    int a = perk.CurrentPerk.Num;
                    if (a == 30 || a == 31 || a == 33 || a == 38 || a == 94)
                    {
                        Text_SRateDetail.text += "\n" + CurrentManager.Name + perk.CurrentPerk.Name + "状态: " + (perk.CurrentPerk.TempValue4 * perk.CurrentPerk.Level * 100) + "%";
                    }
                }
            }

            if (Mathf.Abs(GC.SC.ExtraSuccessRate) > 0.001f)
                Text_SRateDetail.text += "\n动员效果:" + (GC.SC.ExtraSuccessRate * 100) + "%";

            if (OfficeMode == 4 && Mathf.Abs(GC.HireSuccessExtra) > 0.001f)
            {
                Text_SRateDetail.text += "\n额外招聘成功率:" + (GC.HireSuccessExtra * 100) + "%";
                BaseSRate += GC.HireSuccessExtra;
            }
        }
        //高管的效果
        if (CommandingOffice != null)
        {
            if (CommandingOffice.CurrentManager != null)
            {
                int value = 0;
                float EValue = 0;
                Text_SRateDetail.text += "\n";
                if (OfficeMode == 2 || OfficeMode == 4)
                {
                    value = CommandingOffice.CurrentManager.HR;
                    Text_SRateDetail.text += "上级高管" + CommandingOffice.CurrentManager.Name + "人力技能:";
                }
                else if (OfficeMode == 3 || OfficeMode == 5)
                {
                    value = CommandingOffice.CurrentManager.Manage;
                    Text_SRateDetail.text += "上级高管" + CommandingOffice.CurrentManager.Name + "管理技能:";
                }
                else if (OfficeMode == 1)
                {
                    value = CommandingOffice.CurrentManager.Decision;
                    Text_SRateDetail.text += "上级高管" + CommandingOffice.CurrentManager.Name + "决策技能:";
                }
                if (value <= 5)
                    EValue -= 0.15f;
                else if (value <= 9)
                    EValue -= 0.1f;
                else if (value <= 13)
                    EValue += 0.05f;
                else if (value <= 17)
                    EValue += 0;
                else if (value <= 21)
                    EValue += 0.04f;
                else if (value > 21)
                    EValue += 0.08f;
                BaseSRate += EValue;
                BaseSRate += CommandingOffice.CurrentManager.ExtraSuccessRate;

                //文字显示
                Text_SRateDetail.text += (EValue * 100) + "%";
                if (Mathf.Abs(CommandingOffice.CurrentManager.ExtraSuccessRate) > 0.001f)
                {
                    foreach (PerkInfo perk in CommandingOffice.CurrentManager.InfoDetail.PerksInfo)
                    {
                        int a = perk.CurrentPerk.Num;
                        if (a == 30 || a == 31 || a == 33 || a == 38 || a == 94)
                        {
                            Text_SRateDetail.text += "\n" + CommandingOffice.CurrentManager.Name + perk.CurrentPerk.Name + "状态: " + (perk.CurrentPerk.TempValue4 * perk.CurrentPerk.Level * 100) + "%";
                        }
                    }
                }
            }
        }
        return BaseSRate + GC.SC.ExtraSuccessRate;
    }

    public void TimePass()
    {
        if (building.Type != BuildingType.高管办公室 && building.Type != BuildingType.CEO办公室)
        {
            if (CurrentManager != null && Progress < 100 && CanWork == true)
            {
                //if (building.effectValue == 1)
                //    Progress += CurrentManager.HR;
                //else if (building.effectValue == 2)
                //    Progress += CurrentManager.Gossip;
                //else if (building.effectValue == 3)
                //    Progress += CurrentManager.Strength;
                //else if (building.effectValue == 4)
                //    Progress += CurrentManager.Strategy;
                //else if (building.effectValue == 5)
                //    Progress += CurrentManager.Forecast;
                //else if (building.effectValue == 6)
                //    Progress += CurrentManager.Decision;
                //else if (building.effectValue == 7)
                //    Progress += CurrentManager.Finance;
                //else if (building.effectValue == 8)
                //    Progress += CurrentManager.Manage;

                //if (Progress >= 100)
                //{
                //    Progress = 100;
                //    ActiveButton.interactable = true;
                //}
                //Text_Progress.text = "激活进度:" + Progress + "%";
            }
        }
        else
        {
            if (CurrentManager != null)
            {
                //原本的战略充能
                CurrentManager.InfoDetail.ReChargeStrategy();

                UpdateUI();
                //高管（CEO）办公室的四种模式效果

                if (Progress < MaxProgress)
                {
                    if (building.Type == BuildingType.高管办公室 && CanWork == false)
                        return;
                    Progress += 1;
                }
                else
                {
                    bool MajorSuccess = false;
                    Progress = 0;
                    //成功和大成功
                    if (Random.Range(0.0f, 1.0f) < CountSuccessRate())
                    {
                        if (Random.Range(0.0f, 1.0f) < OfficeBaseMajorSuccessRate + GC.SC.ExtraMajorSuccessRate)
                            MajorSuccess = true;
                    }
                    //失败和大失败
                    else
                    {
                        if (Random.Range(0.0f, 1.0f) < OfficeBaseMajorFailureRate + GC.SC.ExtraMajorFailureRate)
                        {
                            //CurrentManager.Mentality -= 20;
                            GC.CreateMessage(Text_OfficeName.text + "工作发生严重失误");
                        }
                        else
                            GC.CreateMessage(Text_OfficeName.text + "充能失败");
                        return;
                    }
                    if (OfficeMode == 1)
                    {
                        CurrentManager.InfoDetail.DirectAddStrategy();
                        if(MajorSuccess == true)
                            CurrentManager.InfoDetail.DirectAddStrategy();
                        GC.CreateMessage("(" + Text_OfficeName.text + "添加了新战略");
                    }
                    else if (OfficeMode == 2)
                    {
                        Employee E = null;
                        for (int i = 0; i < ControledDeps.Count; i++)
                        {
                            for (int j = 0; j < ControledDeps[i].CurrentEmps.Count; j++)
                            {
                                if (E == null)
                                    E = ControledDeps[i].CurrentEmps[j];
                                else if (E != null && ControledDeps[i].CurrentEmps[j].Mentality < E.Mentality)
                                    E = ControledDeps[i].CurrentEmps[j];
                            }
                        }
                        if (E != null)
                        {
                            int value = 10;
                            if (MajorSuccess == true)
                                value = 20;
                            E.Mentality += (int)(value * GC.HRBuildingMentalityExtra);
                            GC.CreateMessage("(" + Text_OfficeName.text + ")" + E.Name + "回复了" + value + "点心力");
                        }

                    }
                    else if (OfficeMode == 3)
                    {
                        List<Employee> E = new List<Employee>();
                        for (int i = 0; i < ControledDeps.Count; i++)
                        {
                            for (int j = 0; j < ControledDeps[i].CurrentEmps.Count; j++)
                            {
                                E.Add(ControledDeps[i].CurrentEmps[j]);
                            }
                        }
                        if (E.Count > 0)
                        {
                            Employee E2 = E[Random.Range(0, E.Count)];
                            E2.InfoDetail.AddPerk(new Perk3(E2), true);
                            if(MajorSuccess == true)
                                E2.InfoDetail.AddPerk(new Perk3(E2), true);
                            GC.CreateMessage("(" + Text_OfficeName.text + ")" + E2.Name + "获得了启发");
                        }

                    }
                    else if (OfficeMode == 4)
                    {
                        HireType ht = new HireType();
                        if (MajorSuccess == true)
                            ht.HireNum = 5;
                        else
                            ht.HireNum = 3;
                        GC.HC.AddHireTypes(ht);
                        GC.CreateMessage("(" + Text_OfficeName.text + ")完成了招聘");
                    }
                    else if (OfficeMode == 5)
                    {
                        if (MajorSuccess == true)
                            GC.BM.Lottery(4);
                        else
                            GC.BM.Lottery(3);
                        GC.CreateMessage("(" + Text_OfficeName.text + ")完成了组织研究");
                    }
                }

            }
        }

        //经验获取
        if (CurrentManager != null)
        {
                CurrentManager.GainExp(5, building.effectValue);
        }
    }

    public void UpdateUI()
    {
        if (CurrentManager == null)
        {
            Text_SuccessRate.text = "成功率:----";
            Text_TimeLeft.text = "剩余时间:----";
        }
        else
        {
            float BaseSRate = CountSuccessRate();
            Text_SuccessRate.text = "成功率:" + Mathf.Round(BaseSRate * 100) + "%";
            Text_TimeLeft.text = "剩余时间:" + (MaxProgress - Progress) + "时";
        }
    }

    public void ClearOffice()
    {
        if(CurrentManager != null)
        {
            GC.CurrentEmpInfo = CurrentManager.InfoA;
            GC.CurrentEmpInfo.transform.parent = GC.StandbyContent;
            GC.ResetOldAssignment();
        }
        for(int i = 0; i < ControledDeps.Count; i++)
        {
            ControledDeps[i].CommandingOffice = null;
            ControledDeps[i].AddPerk(new Perk110(null));
            ControledDeps[i].FaithRelationCheck();
        }
        for (int i = 0; i < ControledOffices.Count; i++)
        {
            ControledOffices[i].CommandingOffice = null;
        }
        if(CommandingOffice != null)
        {
            CommandingOffice.ControledOffices.Remove(this);
            CommandingOffice.CheckManage();
        }
        GC.HourEvent.RemoveListener(TimePass);
        //GC.CurrentOffices.Remove(this);
        if (SRateDetailPanel != null)
            Destroy(SRateDetailPanel.gameObject);
        Destroy(DS.gameObject);
        Destroy(this.gameObject);
    }

    public void ShowSRateDetailPanel()
    {
        CountSuccessRate();
        SRateDetailPanel.transform.position = Input.mousePosition;
        SRateDetailPanel.gameObject.GetComponent<WindowBaseControl>().SetWndState(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(SRateDetailPanel.gameObject.GetComponent<RectTransform>());
    }

    public void CloseSRateDetailPanel()
    {
        SRateDetailPanel.gameObject.GetComponent<WindowBaseControl>().SetWndState(false);
    }
}
