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
    public int AffectRange { get; private set; } = 4;  //影响上下左右4个单位的建筑
    public Building CurrentBuilding;  //所属建筑

    List<Building> m_TargetBuildings = new List<Building>();
    public List<Building> AffectedBuildings = new List<Building>();
  

    public BuildingEffect(Building building)
    {
        CurrentBuilding = building;
    }

    //遍历m_TargetBuildings，对每个目标建筑设置影响
    public void Affect()
    {
        Debug.Log("!!!!!" + CurrentBuilding.name);
        foreach (var b in m_TargetBuildings)
        {
            Debug.Log(b + "  Affect  ");
        }
        for (int i = 0; i < m_TargetBuildings.Count; i++)
        {
            if (AffectedBuildings.Contains(m_TargetBuildings[i]))
                continue;

            BuildingType T = CurrentBuilding.Type;
            BuildingType T2 = m_TargetBuildings[i].Type;
            if (T == BuildingType.技术部门 || T == BuildingType.市场部门 || T == BuildingType.产品部门 || T == BuildingType.运营部门)
            {
                if ((T2 == BuildingType.技术部门 || T2 == BuildingType.市场部门 || T2 == BuildingType.产品部门 || T2 == BuildingType.运营部门) && T != T2)
                {
                    m_TargetBuildings[i].Department.Efficiency += 0.1f;
                }
            }
            else if (T == BuildingType.高管办公室 || T == BuildingType.CEO办公室) 
            {
                if (T2 != BuildingType.高管办公室 && T2 != BuildingType.CEO办公室)
                {
                    if (m_TargetBuildings[i].Department != null)
                        m_TargetBuildings[i].Department.InRangeOffices.Add(CurrentBuilding.Office);
                    else if (m_TargetBuildings[i].Office != null)
                        m_TargetBuildings[i].Office.InRangeOffices.Add(CurrentBuilding.Office);
                }
            }
            if (!AffectedBuildings.Contains(m_TargetBuildings[i]))
                AffectedBuildings.Add(m_TargetBuildings[i]);
        }
        m_TargetBuildings.Clear();
    }
    public void RemoveAffect()
    {
        //这里可能存在问题
        for (int i = 0; i < AffectedBuildings.Count; i++)
        {
            BuildingType T = CurrentBuilding.Type;
            BuildingType T2 = AffectedBuildings[i].Type;
            if (T == BuildingType.技术部门 || T == BuildingType.市场部门 || T == BuildingType.产品部门 || T == BuildingType.运营部门)
            {
                if ((T2 == BuildingType.技术部门 || T2 == BuildingType.市场部门 || T2 == BuildingType.产品部门 || T2 == BuildingType.运营部门) && T != T2)
                {
                    AffectedBuildings[i].Department.Efficiency -= 0.1f;
                    CurrentBuilding.Department.Efficiency -= 0.1f;
                }
            }
            else if (T == BuildingType.高管办公室 || T == BuildingType.CEO办公室)
            {
                if (T2 != BuildingType.高管办公室 && T2 != BuildingType.CEO办公室)
                {
                    if (AffectedBuildings[i].Department != null)
                        AffectedBuildings[i].Department.InRangeOffices.Remove(CurrentBuilding.Office);
                    else if (AffectedBuildings[i].Office != null)
                        AffectedBuildings[i].Office.InRangeOffices.Remove(CurrentBuilding.Office);
                }
            }
            else if (T != BuildingType.高管办公室 && T != BuildingType.CEO办公室)
            {
                if (T2 == BuildingType.高管办公室 || T2 == BuildingType.CEO办公室)
                {
                    if (CurrentBuilding.Department != null)
                        CurrentBuilding.Department.InRangeOffices.Remove(CurrentBuilding.Office);
                    else if (CurrentBuilding.Office != null)
                        CurrentBuilding.Office.InRangeOffices.Remove(CurrentBuilding.Office);
                }
            }
        }
        AffectedBuildings.Clear();
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
            if (tempGrid.belongBuilding == CurrentBuilding || tempGrid.belongBuilding == null || m_TargetBuildings.Contains(tempGrid.belongBuilding)) 
                continue;

            m_TargetBuildings.Add(tempGrid.belongBuilding);
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
}
