using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OfficeControl : MonoBehaviour
{
    public int ManageValue = 0, Progress = 100;

    public Employee CurrentManager;
    public DepSelect DS;
    public GameControl GC;
    public Building building;
    public Text Text_OfficeName, Text_EmpName, Text_MAbility, Text_Progress;
    public Button ActiveButton;

    public List<DepControl> ControledDeps = new List<DepControl>();

    

    //放入和移除高管时调用
    public void SetOfficeStatus()
    {
        if (CurrentManager != null)
        {
            Text_EmpName.text = "当前高管:" + CurrentManager.Name;
            if (building.Type == BuildingType.高管办公室 || building.Type == BuildingType.CEO办公室)
            {
                Text_MAbility.text = "管理:" + CurrentManager.Manage;
                ManageValue = CurrentManager.Manage + GC.ManageExtra;
                CurrentManager.InfoDetail.CreateStrategy();
            }
            else if (building.effectValue == 1)
                Text_MAbility.text = "人力:" + CurrentManager.HR;
            else if (building.effectValue == 2)
                Text_MAbility.text = "八卦:" + CurrentManager.Gossip;
            else if (building.effectValue == 3)
                Text_MAbility.text = "强壮:" + CurrentManager.Strength;
            else if (building.effectValue == 4)
                Text_MAbility.text = "谋略:" + CurrentManager.Strategy;
            else if (building.effectValue == 5)
                Text_MAbility.text = "行业:" + CurrentManager.Forecast;
            else if (building.effectValue == 6)
                Text_MAbility.text = "决策:" + CurrentManager.Decision;
            else if (building.effectValue == 7)
                Text_MAbility.text = "财务:" + CurrentManager.Finance;
            else if (building.effectValue == 8)
                Text_MAbility.text = "管理:" + CurrentManager.Finance;

        }
        else
        {
            Text_EmpName.text = "当前高管:无";
            if (building.effectValue == 1)
                Text_MAbility.text = "人力:--";
            else if (building.effectValue == 2)
                Text_MAbility.text = "八卦:--";
            else if (building.effectValue == 3)
                Text_MAbility.text = "强壮:--";
            else if (building.effectValue == 4)
                Text_MAbility.text = "谋略:--";
            else if (building.effectValue == 5)
                Text_MAbility.text = "行业:--";
            else if (building.effectValue == 6)
                Text_MAbility.text = "决策:--";
            else if (building.effectValue == 7)
                Text_MAbility.text = "财务:--";
            else if (building.effectValue == 8)
                Text_MAbility.text = "管理:--";
            ManageValue = 0;
        }

    }

    public void CheckManage()
    {
        if (CurrentManager != null)
            ManageValue = CurrentManager.Manage + GC.ManageExtra;
        for(int i = 0; i < ControledDeps.Count; i++)
        {
            if (i < ManageValue)
            {
                ControledDeps[i].canWork = true;
                ControledDeps[i].OfficeWarning.SetActive(false);
            }
            else
            {
                ControledDeps[i].canWork = false;
                ControledDeps[i].OfficeWarning.SetActive(true);
            }
        }
    }

    public void SetName()
    {
        int num = 0;
        GC.HourEvent.AddListener(TimePass);
        if (building.Type != BuildingType.高管办公室 && building.Type != BuildingType.CEO办公室)
        {
            Text_Progress.gameObject.SetActive(true);
            ActiveButton.gameObject.SetActive(true);
            num = 1;
        }
        for (int i = 0; i < GC.CurrentOffices.Count; i++)
        {
            if (GC.CurrentOffices[i].building.Type == building.Type)
                num += 1;
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

        if (building.Type == BuildingType.人力资源部A)
        {       
            int value = 0;
            if (Posb < 0.2f - extra)
                value = 20;
            else if (Posb < 0.8f - extra2)
                value = 10;
            else
                value = 5;
            value = (int)(value * GC.HRBuildingMentalityExtra);
            DepControl d = GC.CurrentDep;
            for (int i = 0; i < d.CurrentEmps.Count; i++)
            {
                d.CurrentEmps[i].Mentality += value;
                int StarNum = Random.Range(0, 6);
                if (d.CurrentEmps[i].Stars[StarNum] < d.CurrentEmps[i].StarLimit[StarNum] * 5)
                {
                    d.CurrentEmps[i].Stars[StarNum] += 1;                   
                }
            }
        }
        else if(building.Type == BuildingType.心理咨询室)
        {
            //还没写！
        }
        else if(building.Type == BuildingType.按摩房)
        {
            List<DepControl> TDep = new List<DepControl>();
            for(int i = 0; i < building.effect.AffectedBuildings.Count; i++)
            {
                if(building.effect.AffectedBuildings[i].Department != null)
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
            for(int a = 0; a < TDep.Count; a++)
            {
                for (int i = 0; i < TDep[a].CurrentEmps.Count; i++)
                {
                    TDep[a].CurrentEmps[i].Stamina += value;
                }
            }
            if (TDep.Count == 0)
                ActiveSuccess = false;
        }
        else if(building.Type == BuildingType.健身房)
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
        else if(building.Type == BuildingType.目标修正小组)
        {
            if (GC.SC.TargetDep != null && building.effect.AffectedBuildings.Contains(GC.SC.TargetDep.building) && GC.SC.SelectedDices.Count == 3)
            {
                GC.SC.TotalValue = 0;
                for (int i = 0; i < GC.SC.SelectedDices.Count; i++)
                {
                    GC.SC.SelectedDices[i].RandomValue();
                    GC.SC.TotalValue += GC.SC.SelectedDices[i].value;
                }
            }
            else
                ActiveSuccess = false;
        }
        else if (building.Type == BuildingType.档案管理室)
        {
            if (GC.SC.TargetDep != null && building.effect.AffectedBuildings.Contains(GC.SC.TargetDep.building) && GC.SC.SelectedDices.Count == 1)
            {
                GC.SC.SelectedDices[0].value += 1;
                GC.SC.SelectedDices[0].Text_Value.text = GC.SC.SelectedDices[0].value.ToString();
                GC.SC.TotalValue += 1;
            }
            else
                ActiveSuccess = false;
        }
        else if (building.Type == BuildingType.效能研究室)
        {
            if (GC.SC.TargetDep != null && building.effect.AffectedBuildings.Contains(GC.SC.TargetDep.building))
            {
                GC.SC.Sp1Active = true;
            }
            else
                ActiveSuccess = false;
        }
        else if (building.Type == BuildingType.财务部)
        {
            if (GC.SC.TargetDep != null && building.effect.AffectedBuildings.Contains(GC.SC.TargetDep.building) && GC.SC.SelectedDices.Count == 1)
            {
                GC.SC.SelectedDices[0].value -= 1;
                GC.SC.SelectedDices[0].Text_Value.text = GC.SC.SelectedDices[0].value.ToString();
                GC.SC.TotalValue -= 1;
            }
            else
                ActiveSuccess = false;
        }
        else if (building.Type == BuildingType.战略咨询部B)
        {
            if (GC.SC.TargetDep != null && building.effect.AffectedBuildings.Contains(GC.SC.TargetDep.building) && GC.SC.SelectedDices.Count == 2)
            {
                GC.SC.Dices.Remove(GC.SC.SelectedDices[0]);
                GC.SC.Dices.Remove(GC.SC.SelectedDices[1]);
                Destroy(GC.SC.SelectedDices[0].gameObject);
                Destroy(GC.SC.SelectedDices[1].gameObject);
                GC.SC.SelectedDices.Clear();
                GC.SC.TotalValue = 0;
                GC.SC.CreateDice(2, 6);
            }
            else
                ActiveSuccess = false;
        }
        else if (building.Type == BuildingType.精确标准委员会)
        {
            if (GC.SC.TargetDep != null && building.effect.AffectedBuildings.Contains(GC.SC.TargetDep.building) && GC.SC.SelectedDices.Count == 1)
            {
                int num = GC.SC.SelectedDices[0].value;
                GC.SC.Dices.Remove(GC.SC.SelectedDices[0]);
                Destroy(GC.SC.SelectedDices[0].gameObject);
                GC.SC.SelectedDices.Clear();
                GC.SC.CreateDice(num, 1);
                GC.SC.TotalValue = 0;
            }
            else
                ActiveSuccess = false;
        }
        else if (building.Type == BuildingType.高级财务部A)
        {
            if (GC.SC.TargetDep != null && building.effect.AffectedBuildings.Contains(GC.SC.TargetDep.building) && GC.Money > 100000)
            {
                GC.Money -= 100000;
                GC.SC.CreateDice(1);
            }
            else
                ActiveSuccess = false;
        }
        else if (building.Type == BuildingType.高级财务部B)
        {
            if (GC.Money > 100000)
            {
                GC.Money -= 100000;
                GC.CurrentEmpInfo.emp.Mentality += 15;
                GC.SelectMode = 1;
                GC.TotalEmpContent.parent.parent.gameObject.SetActive(false);
            }
            else
            {
                ActiveSuccess = false;
                GC.SelectMode = 1;
                GC.TotalEmpContent.parent.parent.gameObject.SetActive(false);
            }
        }

        if (ActiveSuccess == true)
        {
            Progress = 0;
            Text_Progress.text = "激活进度:0%";
            ActiveButton.interactable = false;
            GC.Stamina -= building.StaminaRequest;
        }
    }

    public void ShowAvailableDeps()
    {
        //需要选择部门的建筑物特效，无需选择就直接施加效果
        if (GC.Stamina >= building.StaminaRequest)
        {
            GC.CurrentOffice = this;
            GC.SelectMode = 5;
            if (building.Type == BuildingType.人力资源部A)
                GC.ShowDepSelectPanel(GC.CurrentDeps);
            else if (building.Type == BuildingType.高级财务部B)
                GC.TotalEmpContent.parent.parent.gameObject.SetActive(true);
            else
                BuildingActive();
        }
    }

    public void TimePass()
    {
        if (building.Type != BuildingType.高管办公室 && building.Type != BuildingType.CEO办公室)
        {
            if (CurrentManager != null && Progress < 100)
            {
                if (building.effectValue == 1)
                    Progress += CurrentManager.HR;
                else if (building.effectValue == 2)
                    Progress += CurrentManager.Gossip;
                else if (building.effectValue == 3)
                    Progress += CurrentManager.Strength;
                else if (building.effectValue == 4)
                    Progress += CurrentManager.Strategy;
                else if (building.effectValue == 5)
                    Progress += CurrentManager.Forecast;
                else if (building.effectValue == 6)
                    Progress += CurrentManager.Decision;
                else if (building.effectValue == 7)
                    Progress += CurrentManager.Finance;
                else if (building.effectValue == 8)
                    Progress += CurrentManager.Manage;

                if (Progress >= 100)
                {
                    Progress = 100;
                    ActiveButton.interactable = true;
                }
                Text_Progress.text = "激活进度:" + Progress + "%";
            }
        }
        else
        {
            if (CurrentManager != null)
                CurrentManager.InfoDetail.ReChargeStrategy();
        }
    }
}
