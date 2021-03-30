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
    private Text txt_Time;

    private void Awake()
    {
        img_Faith = transform.Find("Img_Faith").GetComponent<Image>();
        img_Success = transform.Find("Img_Success").GetComponent<Image>();
        txt_Efficiency = transform.Find("Txt_Efficiency").GetComponent<Text>();
        txt_Cost = transform.Find("Txt_Cost").GetComponent<Text>();
        txt_Time = transform.Find("Txt_Time").GetComponent<Text>();
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
    }

    public void OnUpdate()
    {
        if (!thisBuilding)
        {
            transform.position = new Vector3(-1000, 0, 0);
            return;
        }

        transform.position = Function.World2ScreenPoint(thisBuilding.transform.position + new Vector3(thisBuilding.Length * 7.5f, 0, thisBuilding.Width * 5));
        //部门建筑
        if (m_Dep)
        {
            int faith = m_Dep.DepFaith;
            float success = m_Dep.CountSuccessRate();
            float efficiency = m_Dep.Efficiency;

            if (MonthMeeting.Instance.MeetingStart == true && m_Dep.building.CurrentArea.CA != null)
            {
                if (m_Dep.building.CurrentArea.CA.OrangeCount > 0)
                    faith += m_Dep.building.CurrentArea.CA.OrangeCount * 15;
                if (m_Dep.building.CurrentArea.CA.GrayCount > 0)
                    efficiency += m_Dep.building.CurrentArea.CA.GrayCount * 25;
                if (m_Dep.building.CurrentArea.CA.BlackCount > 0)
                    faith -= m_Dep.building.CurrentArea.CA.BlackCount * 30;
            }

            int time = 0;
            if (m_Dep.CurrentEmps.Count > 0)
                time = m_Dep.calcTime(efficiency, m_Dep.ProducePointLimit);
            else
                time = m_Dep.ProducePointLimit;
            txt_Time.text = "生产周期:" + (time / 8) + "周" + (time % 8) + "时";

            Color faithColor;
            if (faith >= 80)
                faithColor = Color.green;
            else if (faith >= 50)
                faithColor = Color.yellow;
            else
                faithColor = Color.red;

            txt_Efficiency.text = "效率:" + Mathf.Round(efficiency * 100) + "%";

            Color successColor;

            if (success >= 1)
                successColor = Color.green;
            else if (success >= 0.8f)
                successColor = Color.yellow;
            else
                successColor = Color.red;

            img_Faith.color = faithColor;
            img_Success.color = successColor;

            if (MonthMeeting.Instance.MeetingStart == false)
                txt_Cost.text = "开销:" + (m_Dep.CalcCost(1) + m_Dep.CalcCost(2)) + "/月";
            else
                txt_Cost.text = "开销:" + (m_Dep.CalcCost(1, true) + m_Dep.CalcCost(2, true)) + "/月";

            if (m_Dep.building.Type == BuildingType.茶水间)
            {
                txt_Efficiency.text = "体力供给:" + m_Dep.SubDepValue + "/周";
                txt_Time.gameObject.SetActive(false);
                img_Faith.gameObject.SetActive(false);
                img_Success.gameObject.SetActive(false);
            }
            else if (m_Dep.building.Type == BuildingType.CEO办公室 || m_Dep.building.Type == BuildingType.高管办公室)
            {
                txt_Time.gameObject.SetActive(false);
                txt_Efficiency.gameObject.SetActive(false);
                img_Success.gameObject.SetActive(false);
            }
            else
            {
                txt_Time.gameObject.SetActive(true);
                txt_Efficiency.gameObject.SetActive(true);
                img_Success.gameObject.SetActive(true);
            }

        }
    }
}
