﻿using System.Collections;
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
    public int AffectRange { get; private set; } = 4;  //影响上下左右4个单位的建筑
    public Building CurrentBuilding;  //所属建筑

    List<Building> m_TargetBuildings = new List<Building>();
    public List<Building> AffectedBuildings = new List<Building>();
  

    public BuildingEffect(Building building)
    {
        CurrentBuilding = building;
    }

    public void Affect()
    {
        Debug.Log("Affect");
        for (int i = 0; i < m_TargetBuildings.Count; i++)
        {
            BuildingType T = CurrentBuilding.Type;
            BuildingType T2 = m_TargetBuildings[i].Type;
            if (T == BuildingType.技术部门 || T == BuildingType.市场部门 || T == BuildingType.产品部门 || T == BuildingType.运营部门)
            {
                if ((T2 == BuildingType.技术部门 || T2 == BuildingType.市场部门 || T2 == BuildingType.产品部门
                    || T2 == BuildingType.运营部门) && T != T2)
                {
                    m_TargetBuildings[i].Department.Efficiency += 0.1f;
                }
            }
            else if(T == BuildingType.高管办公室)
            {
                if (T2 == BuildingType.技术部门 || T2 == BuildingType.市场部门 || T2 == BuildingType.产品部门
                    || T2 == BuildingType.运营部门 || T2 == BuildingType.研发部门)
                {
                    m_TargetBuildings[i].Department.InRangeOffices.Add(CurrentBuilding.Office);
                }
            }
            if (!AffectedBuildings.Contains(m_TargetBuildings[i]))
                AffectedBuildings.Add(m_TargetBuildings[i]);
        }
        m_TargetBuildings.Clear();
    }

    public void GetEffectBuilding()
    {
        //当前建筑覆盖的角落网格
        Int2 minLocation = null;   //左下角坐标
        Int2 maxLocation = null;   //右上角坐标
        foreach (Grid temp_Grid in CurrentBuilding.ContainsGrids)
        {
            if (minLocation == null)
                minLocation = new Int2(temp_Grid.X, temp_Grid.Z);
            else
            {
                if (temp_Grid.X < minLocation.X) 
                    minLocation.X = temp_Grid.X;
                if (temp_Grid.Z < minLocation.Y) 
                    minLocation.Y = temp_Grid.Z;
            }

            if (maxLocation == null)
                maxLocation = new Int2(temp_Grid.X, temp_Grid.Z);
            else
            {
                if (temp_Grid.X > maxLocation.X)
                    maxLocation.X = temp_Grid.X;
                if (temp_Grid.Z > maxLocation.Y)
                    maxLocation.Y = temp_Grid.Z;
            }
        }

        //辐射到的网格
        Dictionary<int, Grid> gridDict;
        List<Grid> effectGirds = new List<Grid>();
        for (int i = minLocation.X - AffectRange; i < maxLocation.X + AffectRange; i++) 
        {
            if (GridContainer.Instance.GridDict.TryGetValue(i, out gridDict))
            {
                for (int j = minLocation.Y - AffectRange; j < maxLocation.Y + AffectRange; j++)
                {
                    if (gridDict.TryGetValue(j, out Grid grid))
                    {
                        effectGirds.Add(grid);
                    }
                }
            }
        }

        //辐射到的建筑
        foreach (Grid tempGrid in effectGirds)
        {
            if (tempGrid.belongBuilding == CurrentBuilding || tempGrid.belongBuilding == null) 
                continue;

            if (!m_TargetBuildings.Contains(tempGrid.belongBuilding))
            {
                m_TargetBuildings.Add(tempGrid.belongBuilding);
            }

            for (int i = 0; i < m_TargetBuildings.Count; i++)
            {
                m_TargetBuildings[i].effect.AddBuilding(CurrentBuilding);
            }
        }
    }

    public void AddBuilding(Building building)
    {
        m_TargetBuildings.Add(building);
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if(collision.gameObject.tag == "Building")
    //    {
    //        Building b = collision.GetComponent<Building>();
    //        if (b != CurrentBuilding)
    //            m_TargetBuildings.Add(b);
    //    }
    //}

    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (collision.gameObject.tag == "Building")
    //    {
    //        m_TargetBuildings.Remove(collision.GetComponent<Building>());
    //    }
    //}
}
