using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 建筑详细信息面板
/// </summary>
public class BuildingDescription : WindowRoot
{
    public Text str_BuildingName;
    public Text str_BuildingSize;
    public Text str_BuildingRange;
    public Text str_Jobs;
    public Text str_Func1;
    public Text str_Require1;
    public Text str_Description1;
    public Text str_Require2;


    private void Awake()
    {
        str_BuildingName = transform.Find("str_BuildingName").GetComponent<Text>();
        str_BuildingSize = transform.Find("str_BuildingSize").GetComponent<Text>();
        str_BuildingRange = transform.Find("str_BuildingRange").GetComponent<Text>();
        str_Jobs = transform.Find("str_Jobs").GetComponent<Text>();
        str_Func1 = transform.Find("str_Func1").GetComponent<Text>();
        str_Require1 = transform.Find("str_Require1").GetComponent<Text>();
        str_Description1 = transform.Find("str_Description1").GetComponent<Text>();
        str_Require2 = transform.Find("str_Require2").GetComponent<Text>();
    }

    public void ShowInfo(Building building, bool right = true)
    {
        if (right == true)
            transform.localPosition = new Vector3(350, transform.localPosition.y);
        else
            transform.localPosition = new Vector3(-350, transform.localPosition.y);

        ShowInfo(building);
    }

    void ShowInfo(Building building)
    {
        str_BuildingName.text = building.Name;
        str_BuildingSize.text = building.Size;
        str_Jobs.text = building.Jobs;
        str_Func1.text = building.Functions[0];
        str_Require1.text = building.Require_A;
        str_Description1.text = building.Description;
    }

    public void ShowInfo_Decorate(bool right)
    {
        if (right)
            transform.localPosition = new Vector3(550, transform.localPosition.y);
        else
            transform.localPosition = new Vector3(-550, transform.localPosition.y);

        str_BuildingName.text = "装饰建筑物";
        str_BuildingSize.text = "随机大小";
        str_BuildingRange.text = "";
        str_Jobs.text = "";
        str_Func1.text = "随机装饰建筑";
        str_Require1.text = "无";
        str_Description1.text = "随机获得一个装饰建筑物，不同的装饰建筑物可以解锁不同事件";
        str_Require2.text = "";
    }
}
