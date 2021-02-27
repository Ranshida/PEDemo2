using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 建筑信息显示
/// </summary>
public class BuildingInfo : MonoBehaviour
{
    public Building thisBuilding;
    private DepControl m_Dep;
    private OfficeControl m_Office;

    private Image img_Faith;        //信念
    private Image img_Success;      //成功率
    private Text txt_Efficiency;    //效率
    private Text txt_Cost;          //开销

    private void Awake()
    {
        img_Faith = transform.Find("Img_Faith").GetComponent<Image>();
        img_Success = transform.Find("Img_Success").GetComponent<Image>();
        txt_Efficiency = transform.Find("Txt_Efficiency").GetComponent<Text>();
        txt_Cost = transform.Find("Txt_Cost").GetComponent<Text>();
    }

    public void Init(Building building)
    {
        thisBuilding = building;
        transform.position = Function.World2ScreenPoint(thisBuilding.transform.position + new Vector3(thisBuilding.Length * 7.5f, 0, thisBuilding.Width * 5));
        if (thisBuilding.Department)
        {
            m_Dep = thisBuilding.Department;
            img_Faith.gameObject.SetActive(true);
            img_Success.gameObject.SetActive(true);
            txt_Efficiency.gameObject.SetActive(true);
            txt_Cost.gameObject.SetActive(true);
        }
        if (thisBuilding.Office)
        {
            m_Office = thisBuilding.Office;
            img_Faith.gameObject.SetActive(false);
            img_Success.gameObject.SetActive(true);
            txt_Efficiency.gameObject.SetActive(false);
            txt_Cost.gameObject.SetActive(true);
        }
    }

    public void OnUpdate()
    {
        if (!thisBuilding)
        {
            transform.position = new Vector3(-1000, 0, 0);
            return;
        }

        transform.position = Function.World2ScreenPoint(thisBuilding.transform.position + new Vector3(thisBuilding.Length * 7.5f, 0, thisBuilding.Width * 5));

        //办公室建筑
        if (m_Office)
        {
            Color successColor;
            float success = m_Office.CountSuccessRate();
            if (success >= 1)
                successColor = Color.green;
            else if (success >= 0.8f)
                successColor = Color.yellow;
            else
                successColor = Color.green;

            img_Success.color = successColor;
            txt_Cost.text = thisBuilding.Pay.ToString();
        }
        //部门建筑
        if (m_Dep)
        {
            Color faithColor;
            if (m_Dep.DepFaith>=80)
                faithColor = Color.green;
            else if (m_Dep.DepFaith >=50)
                faithColor = Color.yellow;
            else
                faithColor = Color.green; 
            
            Color successColor;
            float success = DepControl.DepBaseSuccessRate + m_Dep.CountSuccessRate(thisBuilding.effectValue);
            if (success >= 1)
                successColor = Color.green;
            else if (success >= 0.8f)
                successColor = Color.yellow;
            else
                successColor = Color.green;

            img_Faith.color = faithColor;
            img_Success.color = successColor;
            txt_Cost.text = (m_Dep.CalcCost(1) + m_Dep.CalcCost(2)).ToString();
        }
    }
}
