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
        //之前寻找自身、邻近区域建筑的功能
        //if (CurrentBuilding.EffectRangeType > 0)
        //{
        //    if (CurrentBuilding.EffectRangeType > 1)
        //    {
        //        foreach (Area a in CurrentBuilding.ContainsGrids[0].BelongedArea.NeighborAreas)
        //        {
        //            foreach (Grid grid in a.gridList)
        //            {
        //                effectGirds.Add(grid);
        //            }
        //        }
        //    }
        //    foreach (Grid grid in CurrentBuilding.ContainsGrids[0].BelongedArea.gridList)
        //    {
        //        effectGirds.Add(grid);
        //    }
        //}

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

    //自己影响到了目标（原本的添加/移除范围内的高管办公室方法）
    public void OnAffectNew(Building building)
    {
        BuildingType T = CurrentBuilding.Type; //自己
        BuildingType T2 = building.Type;       //目标
    }
    //自己不再影响目标
    public void OnRemoveAffect(Building building)
    {
        BuildingType T = CurrentBuilding.Type;  //自己
        BuildingType T2 = building.Type;        //目标
    }

}
