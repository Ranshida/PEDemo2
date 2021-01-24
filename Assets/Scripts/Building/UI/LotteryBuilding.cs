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
    BoolenValue Right;

    public void Init(BuildingDescription desctiption, BuildingType building, BoolenValue right = null)
    {
        Description = desctiption;
        Right = right;

        if (building == BuildingType.空)
            transform.GetComponentInChildren<Text>().text = "装饰建筑";
        else
            transform.GetComponentInChildren<Text>().text = building.ToString();

        if (BuildingManage.Instance.m_AllBuildingPrefab.TryGetValue(building,out GameObject go))
        {
            ThisBuilding = go.GetComponent<Building>();
        }

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Description.gameObject.SetActive(true);
        if (ThisBuilding)
        {
            Description.ShowInfo(this, ThisBuilding, Right);
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
