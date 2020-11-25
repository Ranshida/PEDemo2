using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingDescription : MonoBehaviour
{
    public Text str_BuildingName;
    public Text str_BuildingSize;
    public Text str_BuildingRange;
    public Text str_Jobs;
    public Text str_Func1;
    public Text str_Require1;
    public Text str_Description1;
    public Text str_Func2;
    public Text str_Require2;
    public Text str_Description2;
    public Text str_Func3;
    public Text str_Require3;
    public Text str_Description3;

    public void Init()
    {
        str_BuildingName = transform.Find("str_BuildingName").GetComponent<Text>();
        str_BuildingSize = transform.Find("str_BuildingSize").GetComponent<Text>();
        str_BuildingRange = transform.Find("str_BuildingRange").GetComponent<Text>();
        str_Jobs = transform.Find("str_Jobs").GetComponent<Text>();
        str_Func1 = transform.Find("str_Func1").GetComponent<Text>();
        str_Require1 = transform.Find("str_Require1").GetComponent<Text>();
        str_Description1 = transform.Find("str_Description1").GetComponent<Text>();
        str_Func2 = transform.Find("str_Func2").GetComponent<Text>();
        str_Require2 = transform.Find("str_Require2").GetComponent<Text>();
        str_Description2 = transform.Find("str_Description2").GetComponent<Text>();
        str_Func3 = transform.Find("str_Func3").GetComponent<Text>();
        str_Require3 = transform.Find("str_Require3").GetComponent<Text>();
        str_Description3 = transform.Find("str_Description3").GetComponent<Text>();
    }

    public void ShowInfo(LotteryBuilding lottery, Building building)
    {
        if (lottery.transform.localPosition.x < 0) 
            transform.localPosition = new Vector3(550, transform.localPosition.y);
        else
            transform.localPosition = new Vector3(-550, transform.localPosition.y);

        str_BuildingName.text = building.Name;
        str_BuildingSize.text = building.Size;
        str_BuildingRange.text = building.EffectRange_str;
        str_Jobs.text = building.Jobs;
        str_Func1.text = building.Function_A;
        str_Require1.text = building.Require_A;
        str_Description1.text = building.Description_A;
        str_Func2.text = building.Function_B;
        str_Require2.text = building.Require_B;
        str_Description2.text = building.Description_B;
        str_Func3.text = building.Function_C;
        str_Require3.text = building.Require_C;
        str_Description3.text = building.Description_C;
    }
}
