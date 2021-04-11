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
    public bool Lock;      //锁定状态
    public bool IsPutting; //正在尝试放置
    public int X;
    public int Z;
    public GridType Type;
    public int[,] Location { get { return new int[X, Z]; } }

    public Area BelongedArea;
    public Building BelongBuilding { get; private set; } = null;

    public void Build(Building building)
    {
        Type = GridType.已放置;
        BelongBuilding = building;
        RefreshGrid();
    }

    public void Dismantle()
    {
        Type = Grid.GridType.可放置;
        BelongBuilding = null;
        RefreshGrid();
    }

    public void Unlock()
    {
        Lock = false;

        //部分空白格子没有从属Area所以要略过
        if (BelongedArea != null && BelongedArea.DC != null)
        {
            BelongedArea.DC.gameObject.SetActive(true);
            BelongedArea.DC.Locked = false;
        }
        RefreshGrid();
    }

    //刷新格子 位置、名称、颜色
    public void RefreshGrid()
    {
        transform.position = new Vector3(X * 10 + 5, 0, Z * 10 + 5);
        transform.name = "Grid(" + X + "," + Z + ")";
        if (Lock)
        {
            Function.SetMaterial(transform, ResourcesLoader.LoadMaterial("Material/MaLingyu/TransWhite"), true);
        }
        else if(IsPutting)
        {
            Function.SetMaterial(transform, ResourcesLoader.LoadMaterial("Material/MaLingyu/TransBlue"), true);
        }
        else
        {
            switch (Type)
            {
                case Grid.GridType.障碍:
                    Function.SetMaterial(transform, ResourcesLoader.LoadMaterial("Material/MaLingyu/TransBlack"), true);
                    break;
                case Grid.GridType.道路:
                    Function.SetMaterial(transform, ResourcesLoader.LoadMaterial("Material/MaLingyu/TransGray"), true);
                    break;
                case Grid.GridType.可放置:
                    Function.SetMaterial(transform, ResourcesLoader.LoadMaterial("Material/MaLingyu/TransGreen", true));
                    break;
                case Grid.GridType.已放置:
                    Function.SetMaterial(transform, ResourcesLoader.LoadMaterial("Material/MaLingyu/Transparent"), true);
                    break;
                default:
                    break;
            }
        }
    }
}
