using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 抽建筑面板
/// </summary>
public class LotteryWindow : WindowRoot
{
    private BuildingManage _Manage;

    private List<Transform> lotteryUI;
    private GameObject lotteryBuilding;        //抽卡的UI按钮图标

    private void Awake()
    {
        lotteryBuilding = ResourcesLoader.LoadPrefab("Prefabs/UI/Building/LotteryBuilding");
    }

    protected override void InitSpecific()
    {
        _Manage = BuildingManage.Instance;
        lotteryUI = new List<Transform>();
    }

    protected override void OnButton(string btnName)
    {
        switch (btnName)
        {
            case "Btn_GiveUp":
                GiveUpLottery();
                break;
            default:
                break;
        }
    }


    public void Lottery(List<BuildingType> tempBuildings)
    {
        foreach (BuildingType type in tempBuildings)
        {
            Transform panel = GameObject.Instantiate(lotteryBuilding, transform.Find("List")).transform;
            lotteryUI.Add(panel);

            if (type == BuildingType.空)
                panel.GetComponentInChildren<Text>().text = "装饰建筑";
            else
                panel.GetComponentInChildren<Text>().text = type.ToString();

            _Manage.AddInfoListener(panel, type);

            Action action = () =>
            {
                //装饰设施 
                _Manage.EnterBuildMode();
                if (type == BuildingType.空)
                {
                    List<Building> decorate = new List<Building>();
                    foreach (KeyValuePair<BuildingType, Building> building in _Manage.m_AllBuildings)
                    {
                        if (building.Value.Str_Type == "装饰设施")
                        {
                            decorate.Add(building.Value);
                        }
                    }
                    _Manage.StartBuildNew(decorate[UnityEngine.Random.Range(0, decorate.Count)].Type);
                }
                //普通建筑
                else
                {
                    _Manage.StartBuildNew(type);
                }
                QuestControl.Instance.Finish(11);
                foreach (Transform ui in lotteryUI)
                {
                    Destroy(ui.gameObject);
                }
                lotteryUI.Clear();
            };

            panel.transform.GetComponent<Button>().onClick.AddListener(() =>
            {
                action();
                _Manage.FinishLottery();
            });
        }
    }

    //放弃这次抽奖
    public void GiveUpLottery()
    {
        QuestControl.Instance.Finish(11);
        foreach (Transform ui in lotteryUI)
        {
            Destroy(ui.gameObject);
        }
        lotteryUI.Clear();
        _Manage.FinishLottery();
    }
}
