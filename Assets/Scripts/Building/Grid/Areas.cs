using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 将网格划分为多个区域
/// </summary>
public class Areas : MonoBehaviour
{
    public List<Area> AreaLists;
    public AreaSelect ASPrefab;

    public void Init()
    {
        foreach (Area area in AreaLists)
        {
            area.Init();
            //AreaSelect AS = Instantiate(ASPrefab, ASContent);
            //AS.area = area;
            foreach (int index in area.NeighborIndex)
            {
                area.NeighborAreas.Add(AreaLists[index]);
            }
        }
    }
}
[System.Serializable]
public class Area
{
    public List<int> NeighborIndex;
    public List<Area> NeighborAreas;
    public List<Grid> gridList;

    public Vector3 centerPosition;
    public Vector3 topPosition;

    public void Init()
    {
        centerPosition = Vector3.zero;
        foreach (Grid grid in gridList)
        {
            centerPosition += grid.transform.position;
            grid.BelongedArea = this;
        }
        centerPosition /= gridList.Count;

        float posX = 0;
        topPosition = Vector3.zero;
        foreach (Grid grid in gridList)
        {
            if (grid.transform.position.z > topPosition.z)
            {
                topPosition = new Vector3(0, 0, grid.transform.position.z);
            }
            posX += grid.transform.position.x;
        }
        posX /= gridList.Count;
        topPosition = new Vector3(posX, 0, topPosition.z);


    }
}
