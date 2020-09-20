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
    public List<Grid> GridList;                               //包含所有单元格的列表
    public Dictionary<int, Dictionary<int, Grid>> GridDict;   //包含所有单元格的二重字典（x,z）
    public List<Grid> LockGrids_0;
    public List<Grid> LockGrids_1;
    public List<Grid> LockGrids_2;
    
    private void Awake()
    {
        Instance = this;
        GridDict = new Dictionary<int, Dictionary<int, Grid>>();
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
        }
    }

    private void Start()
    {
      
    }

    public void UnlockGrids(int id)
    {
        if (id == 0)
        {
            foreach (Grid grid in LockGrids_0)
            {
                grid.Unlock();
            }
        }
        if (id == 1)
        {
            foreach (Grid grid in LockGrids_1)
            {
                grid.Unlock();
            }
        }
        if (id == 2)
        {
            foreach (Grid grid in LockGrids_2)
            {
                grid.Unlock();
            }
        }
    }
}
