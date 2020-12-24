using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 存储所有存档的Key和存档本身的数据
/// </summary>
public class FilesKey
{
    public bool[] allFiles = new bool[128];
    //Key：存档的index    Value：存档属性
    public Dictionary<int, FilesData> KeysDict;

    public FilesKey()
    {
        KeysDict = new Dictionary<int, FilesData>();
        for (int i = 0; i < allFiles.Length; i++)
        {
            KeysDict.Add(i, null);
        }
    }
}

/// <summary>
/// 存档本身的数据
/// 这些信息应当显示在读取游戏的面板上
/// </summary>
public class FilesData
{
    public string FileKey;       //读取时用的Key

    public string FileName;      //自定义存档的名字（默认为FileKey）
    public string Date;          //三次元时间
    public string GameTime;      //游戏时间   年/月/周
    public string GameVersion;   //游戏版本

    public FilesData(string key)
    {
        FileKey = key;
        FileName = key;
        Date = System.DateTime.Now.ToString("f");
    }

    public FilesData(string key, string name)
    {
        FileKey = key;
        FileName = name;
        Date = System.DateTime.Now.ToString("f");
    }
}