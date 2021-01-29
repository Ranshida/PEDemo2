using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 存档类
/// GameRoot通过此类加载玩家存档
/// 注意都不能使用有参构造函数
/// </summary>
public class GameFiles
{
    public Files_Main main;
    public File_Building building;
}

public class Files_Main
{
    public int Money;
}

public class File_Building
{
    public List<Data_Building> buildingList;
    public File_Building()
    {
        buildingList = new List<Data_Building>();
    }
}
public class Data_Building
{
    public int X;
    public int Z;
    public BuildingType Type;
}