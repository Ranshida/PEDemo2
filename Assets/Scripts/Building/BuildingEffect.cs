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
        ////当前建筑覆盖的角落网格
        //Int2 minLocation = null;   //左下角坐标
        //Int2 maxLocation = null;   //右上角坐标
        //foreach (Grid temp_Grid in CurrentBuilding.ContainsGrids)
        //{
        //    if (minLocation == null)
        //        minLocation = new Int2(temp_Grid.X, temp_Grid.Z);
        //    else
        //    {
        //        if (temp_Grid.X < minLocation.X)
        //            minLocation.X = temp_Grid.X;
        //        if (temp_Grid.Z < minLocation.Y)
        //            minLocation.Y = temp_Grid.Z;
        //    }

        //    if (maxLocation == null)
        //        maxLocation = new Int2(temp_Grid.X, temp_Grid.Z);
        //    else
        //    {
        //        if (temp_Grid.X > maxLocation.X)
        //            maxLocation.X = temp_Grid.X;
        //        if (temp_Grid.Z > maxLocation.Y)
        //            maxLocation.Y = temp_Grid.Z;
        //    }
        //}

        ////辐射到的网格
        //Dictionary<int, Grid> gridDict;
        //List<Grid> effectGirds = new List<Grid>();
        //for (int i = minLocation.X - AffectRange; i < maxLocation.X + AffectRange; i++)
        //{
        //    if (GridContainer.Instance.GridDict.TryGetValue(i, out gridDict))
        //    {
        //        for (int j = minLocation.Y - AffectRange; j < maxLocation.Y + AffectRange; j++)
        //        {
        //            if (gridDict.TryGetValue(j, out Grid grid))
        //            {
        //                effectGirds.Add(grid);
        //            }
        //        }
        //    }
        //}

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
                        dep.OfficeWarning.SetActive(true);
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

    //遍历m_TargetBuildings，对每个目标建筑设置影响
    //public void Affect()
    //{
    //    Debug.Log("!!!!!" + CurrentBuilding.name);
    //    foreach (var b in m_TargetBuildings)
    //    {
    //        Debug.Log(b + "  Affect  ");
    //    }
    //    for (int i = 0; i < m_TargetBuildings.Count; i++)
    //    {
    //        if (AffectedBuildings.Contains(m_TargetBuildings[i]))
    //            continue;

    //        BuildingType T = CurrentBuilding.Type;
    //        BuildingType T2 = m_TargetBuildings[i].Type;
    //        if (T == BuildingType.高管办公室 || T == BuildingType.CEO办公室)
    //        {
    //            if (T2 != BuildingType.高管办公室 && T2 != BuildingType.CEO办公室)
    //            {
    //                if (m_TargetBuildings[i].Department != null)
    //                    m_TargetBuildings[i].Department.InRangeOffices.Add(CurrentBuilding.Office);
    //                else if (m_TargetBuildings[i].Office != null)
    //                    m_TargetBuildings[i].Office.InRangeOffices.Add(CurrentBuilding.Office);
    //            }
    //        }


    //        if (!AffectedBuildings.Contains(m_TargetBuildings[i]))
    //            AffectedBuildings.Add(m_TargetBuildings[i]);
    //    }
    //    m_TargetBuildings.Clear();
    //}
    //public void RemoveAffect()
    //{
    //    //这里可能存在问题
    //    for (int i = 0; i < AffectedBuildings.Count; i++)
    //    {
    //        BuildingType T = CurrentBuilding.Type;
    //        BuildingType T2 = AffectedBuildings[i].Type;
    //        if (T == BuildingType.高管办公室 || T == BuildingType.CEO办公室)
    //        {
    //            if (T2 != BuildingType.高管办公室 && T2 != BuildingType.CEO办公室)
    //            {
    //                if (AffectedBuildings[i].Department != null)
    //                    AffectedBuildings[i].Department.InRangeOffices.Remove(CurrentBuilding.Office);
    //                else if (AffectedBuildings[i].Office != null)
    //                    AffectedBuildings[i].Office.InRangeOffices.Remove(CurrentBuilding.Office);
    //            }
    //        }
    //        else if (T != BuildingType.高管办公室 && T != BuildingType.CEO办公室)
    //        {
    //            if (T2 == BuildingType.高管办公室 || T2 == BuildingType.CEO办公室)
    //            {
    //                if (CurrentBuilding.Department != null)
    //                    CurrentBuilding.Department.InRangeOffices.Remove(CurrentBuilding.Office);
    //                else if (CurrentBuilding.Office != null)
    //                    CurrentBuilding.Office.InRangeOffices.Remove(CurrentBuilding.Office);
    //            }
    //        }
    //        if (AffectedBuildings[i].effect.AffectedBuildings.Contains(CurrentBuilding))
    //            AffectedBuildings[i].effect.AffectedBuildings.Remove(CurrentBuilding);
    //    }
    //    AffectedBuildings.Clear();
    //}

    //public void GetEffectBuilding()
    //{

    //}
}
