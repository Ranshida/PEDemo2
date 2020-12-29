using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 存储所有存档的Key和存档本身的数据
/// </summary>
public class FilesKey
{
    public bool[] allFiles = new bool[100];

    public Dictionary<int, FilesData> KeysDict;

    public FilesKey()
    {
        KeysDict = new Dictionary<int, FilesData>();
    }
}

/// <summary>
/// 存档数据
/// </summary>
public class FilesData
{
    public string FileKey;       //读取时用的Key

    public string FileName;      //自定义存档的名字（默认为FileKey）
    public string Date;          //三次元时间
    public string GameTime;      //游戏时间   年/月/周
    public string GameVersion;   //游戏版本

    //有参构造函数是不能被保存的，只能用Init的方法
    public void Init(string key)
    {
        FileKey = key;
        FileName = key;
        Date = System.DateTime.Now.ToString("f");
    }
}