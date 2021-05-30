using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单元格容器
/// 管理子类全部单元格
/// 更新时间2020.9.5
/// </summary>
public class GridContainer : MonoBehaviour
{
    public static GridContainer Instance;
    public int xInput;       //Editor输入值
    public int zInput;       //Editor输入值

    public int MinX;         //解锁区域的最小X值
    public int MaxX;         //解锁区域的最大X值
    public int MinZ;         //解锁区域的最小Z值
    public int MaxZ;         //解锁区域的最大Z值

    public Transform WayPoint;
    public List<Grid> GridList;                               //包含所有单元格的列表
    public Dictionary<int, Dictionary<int, Grid>> GridDict;   //包含所有单元格的二重字典（x,z）
    public List<WayPoint> AllWayPoint { get; private set; }
    
    public Areas Areas { get; private set; }      //按区域划分的格子列表

    private void Awake()
    {
        Instance = this;
       
        AllWayPoint = Function.ReturnChildList<WayPoint>(WayPoint);
        GridDict = new Dictionary<int, Dictionary<int, Grid>>();

        //刷新网格状态
        foreach (Grid grid in GridList)
        {
            if (GridDict.ContainsKey(grid.X))
            {
                GridDict[grid.X].Add(grid.Z, grid);
            }
            else
            {
                GridDict.Add(grid.X, new Dictionary<int, Grid>());
                GridDict[grid.X].Add(grid.Z, grid);
            }
            grid.RefreshGrid();

        }

        //激活可用的寻路点
        foreach (var wp in AllWayPoint)
        {
            if (GetGrid(wp.transform.position.x, wp.transform.position.z, out Grid grid))
            {
                wp.Active = !grid.Lock;
            }
        }

        //构造区域面积
        MinX = 5;
        MaxX = 200;
        MinZ = 20;
        MaxZ = 100;
        Areas = GetComponent<Areas>();
        Areas.Init();
    }

    //根据坐标返回所处网格
    public bool GetGrid(float posX, float posZ, out Grid grid)
    {
        grid = null;
        int gridX = 0;
        int gridZ = 0;
        if (posX > 0)
            gridX = (int)(posX / 10);
        if (posZ > 0)
            gridZ = (int)(posZ / 10);

        if (GridDict.TryGetValue(gridX, out Dictionary<int, Grid> dict))
        {
            if (dict.TryGetValue(gridZ, out grid))
            {
                return true;
            }
        }
        return false;
    }

    public void UnlockGrids(int id)
    {
        Areas.AreaLists[id + 1].UnlockGrid();
        BuildingManage.Instance.InitBuilding(BuildingType.原型图画室, new Int2(Areas.AreaLists[id + 1].gridList[0].X, Areas.AreaLists[id + 1].gridList[0].Z));
        foreach (var wp in AllWayPoint)
        {
            if (GetGrid(wp.transform.position.x,wp.transform.position.z,out Grid grid))
            {
                wp.Active = !grid.Lock;
            }
        }
    }
}
