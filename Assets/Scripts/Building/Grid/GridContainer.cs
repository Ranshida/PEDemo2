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
    public int xInput;       //Editor输入值
    public int zInput;       //Editor输入值
    public int X { get; private set; }   //实际计算值
    public int Z { get; private set; }   //实际计算值
    public List<Grid> GridList;

    public void InitInEditor(int _x, int _z)
    {
        X = _x;
        Z = _z;
    }
}
