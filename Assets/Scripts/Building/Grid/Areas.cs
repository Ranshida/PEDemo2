using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 将网格划分为多个区域
/// </summary>
public class Areas : MonoBehaviour
{
    public AreaSelect ASPrefab;
    public Transform ASContent;
    public GameObject CancelButton;

    public List<Area> AreaLists;
    public List<AreaSelect> ASList = new List<AreaSelect>();

    public void Init()
    {
        foreach (Area area in AreaLists)
        {
            area.Init();
            AreaSelect AS = Instantiate(ASPrefab, ASContent);
            AS.area = area;
            area.AS = AS;
            AS.gameObject.SetActive(false);
            ASList.Add(AS);
            foreach (int index in area.NeighborIndex)
            {
                area.NeighborAreas.Add(AreaLists[index]);
            }
        }
    }

    public void CloseAllAS()
    {
        foreach (AreaSelect index in ASList)
        {
            index.gameObject.SetActive(false);
        }
        CancelButton.SetActive(false);
    }

    //显示可用的选项
    public void ShowAvailableAS()
    {
        foreach (AreaSelect index in ASList)
        {
            if (index.area.gridList[0].Lock == false)
                index.gameObject.SetActive(true);
        }
        CancelButton.SetActive(true);
    }
}
[System.Serializable]
public class Area
{
    public List<int> NeighborIndex;
    public List<Area> NeighborAreas;
    public List<Grid> gridList;

    public CrystalArea CA;
    public AreaSelect AS;
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
