using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LotteryBuilding : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    BuildingDescription Description;
    Building ThisBuilding;

    public void Init(BuildingDescription desctiption, BuildingType building, Action clickAction)
    {
        Description = desctiption;
        if (BuildingManage.Instance.m_AllBuildingDict.TryGetValue(building,out GameObject go))
        {
            ThisBuilding = go.GetComponent<Building>();
        }

        if (building == BuildingType.空)
        {
            transform.name = "装饰设施";
        }
        else
        {
            transform.name = building.ToString();
        }


        transform.GetComponentInChildren<Text>().text = name;
        transform.GetComponent<Button>().onClick.AddListener(() =>
        {
            clickAction();
            Description.gameObject.SetActive(false);
        });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Description.gameObject.SetActive(true);
        if (ThisBuilding)
        {
            Description.ShowInfo(this, ThisBuilding);
        }
        else
        {
            Description.ShowInfo_Decorate(this);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Description.gameObject.SetActive(false);
    }
}
