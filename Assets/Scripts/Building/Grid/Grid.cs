using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 网格类
/// 记录坐标和状态
/// </summary>
public class Grid : MonoBehaviour
{
    public enum GridType
    {
        障碍,
        道路,
        可放置,
        已放置
    }
    public bool Lock;
    public int X;
    public int Z;
    public GridType Type;
    public int[,] Location { get { return new int[X, Z]; } }

    public void InitInEditor(int _x, int _z)
    {
        X = _x;
        Z = _z;
        transform.position = new Vector3(X * 10 + 5, 0, Z * 10 + 5);
        transform.name = "Grid(" + X + "," + Z + ")";
        Transform child = transform.Find("GridDebug");
       
    }   
}
