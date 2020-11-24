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

    public void Init(BuildingDescription desctiption, Building building, string name, Action clickAction)
    {
        Description = desctiption;
        ThisBuilding = building;
        transform.name = name;
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
        Description.ShowInfo(this, ThisBuilding);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Description.gameObject.SetActive(false);
    }
}
