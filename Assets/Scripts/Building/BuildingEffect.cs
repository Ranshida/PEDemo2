using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 建筑之间相互影响的Buff效果
/// 更新时间2020.9.12
/// </summary>
[System.Serializable]
public class BuildingEffect
{
    public int AffectRange { get { return CurrentBuilding.EffectRange; } }  //影响上下左右4个单位的建筑
    public Building CurrentBuilding;  //所属建筑

    public List<Building> AffectedBuildings = new List<Building>();   //自身影响到建筑

    //建造建筑时直接构造
    public BuildingEffect(Building building)
    {
        CurrentBuilding = building;
        AffectedBuildings = new List<Building>();
        FindAffect();
    }

    public void FindAffect()
    {
        //设置自身所在区域
        CurrentBuilding.CurrentArea = CurrentBuilding.ContainsGrids[0].BelongedArea;

        List<Grid> effectGirds = new List<Grid>();
        if (CurrentBuilding.EffectRangeType > 0)
        {
            if (CurrentBuilding.EffectRangeType > 1)
            {
                foreach (Area a in CurrentBuilding.ContainsGrids[0].BelongedArea.NeighborAreas)
                {
                    foreach (Grid grid in a.gridList)
                    {
                        effectGirds.Add(grid);
                    }
                }
            }
            foreach (Grid grid in CurrentBuilding.ContainsGrids[0].BelongedArea.gridList)
            {
                effectGirds.Add(grid);
            }
        }

        //辐射到的建筑
        foreach (Grid tempGrid in effectGirds)
        {
            if (tempGrid.BelongBuilding == CurrentBuilding || tempGrid.BelongBuilding == null)
                continue;

            if (AffectedBuildings.Contains(tempGrid.BelongBuilding))
                continue;

            AffectedBuildings.Add(tempGrid.BelongBuilding);
            OnAffectNew(tempGrid.BelongBuilding);
        }
    }

    public void RemoveAffect()
    {
        //不再影响这些
        foreach (Building affect in AffectedBuildings)
        {
            OnRemoveAffect(affect);
        }


        //遍历场景中所有建筑
        foreach (Building temp in BuildingManage.Instance.ConstructedBuildings)
        {
            //这个建筑曾经影响到自己
            if (temp.effect.AffectedBuildings.Contains(CurrentBuilding))
            {
                temp.effect.AffectedBuildings.Remove(CurrentBuilding);
                temp.effect.OnRemoveAffect(CurrentBuilding);
            }
        }
        AffectedBuildings.Clear();
    }

    //自己影响到了目标
    public void OnAffectNew(Building building)
    {
        BuildingType T = CurrentBuilding.Type; //自己
        BuildingType T2 = building.Type;       //目标
        if (T == BuildingType.CEO办公室 || T == BuildingType.高管办公室)
        {
            if (T2 != BuildingType.CEO办公室 && building.Department != null)
                building.Department.InRangeOffices.Add(CurrentBuilding.Department);
        }
        else
        {
            //装饰建筑效果
            if (CurrentBuilding.Department != null && building.Department == null)
                ActiveBuildingEffect(CurrentBuilding, building);
            if (CurrentBuilding.Department == null && building.Department != null)
                ActiveBuildingEffect(building, CurrentBuilding);
        }
    }

    //自己不再影响目标
    public void OnRemoveAffect(Building building)
    {
        BuildingType T = CurrentBuilding.Type;  //自己
        BuildingType T2 = building.Type;        //目标
        if (T == BuildingType.CEO办公室 || T == BuildingType.高管办公室)
        {
            if (building.Department != null && T2 != BuildingType.CEO办公室)
            {
                building.Department.InRangeOffices.Remove(CurrentBuilding.Department);
                if (building.Department.CommandingOffice == CurrentBuilding.Department)
                {
                    foreach (DepControl dep in CurrentBuilding.Department.ControledDeps)
                    {
                        dep.CommandingOffice = null;
                        dep.canWork = false;
                    }
                    CurrentBuilding.Department.ControledDeps.Clear();
                }
            }
        }
        else
        {
            //装饰建筑效果
            if (CurrentBuilding.Department != null && building.Department == null)
                RemoveBuildingEffect(CurrentBuilding, building);
            if (CurrentBuilding.Department == null && building.Department != null)
                RemoveBuildingEffect(building, CurrentBuilding);
        }
    }

    //添加/移除装饰性建筑效果
    public void ActiveBuildingEffect(Building B1, Building B2)
    {
        //B1是收到效果的建筑, B2是提供效果的建筑       
        int level = 0;
        if (B2.Type == BuildingType.室内温室)
            level = 3;
        else if (B2.Type == BuildingType.整修楼顶)
            level = 4;
        else if (B2.Type == BuildingType.游戏厅)
            level = 3;
        else if (B2.Type == BuildingType.营养师定制厨房)
            level = 4;
        else if (B2.Type == BuildingType.咖啡bar)
            level = 3;
        else if (B2.Type == BuildingType.花盆)
            level = 1;
        else if (B2.Type == BuildingType.长椅)
            level = 2;
        else if (B2.Type == BuildingType.自动贩卖机)
            level = 1;

        if (level < 1 || B1.Department == null)
            return;

        for (int i = 0; i < level; i++)
        {
            B1.Department.AddPerk(new Perk114(null));
        }

    }
    public void RemoveBuildingEffect(Building B1, Building B2)
    {
        //B1是收到效果的建筑, B2是提供效果的建筑       
        int level = 0;
        if (B2.Type == BuildingType.室内温室)
            level = 3;
        else if (B2.Type == BuildingType.整修楼顶)
            level = 4;
        else if (B2.Type == BuildingType.游戏厅)
            level = 3;
        else if (B2.Type == BuildingType.营养师定制厨房)
            level = 4;
        else if (B2.Type == BuildingType.咖啡bar)
            level = 3;
        else if (B2.Type == BuildingType.花盆)
            level = 1;
        else if (B2.Type == BuildingType.长椅)
            level = 2;
        else if (B2.Type == BuildingType.自动贩卖机)
            level = 1;

        if (level < 1 || B1.Department == null)
            return;
        for (int i = 0; i < level; i++)
        {
            B1.Department.RemovePerk(114);
        }
    }
}
