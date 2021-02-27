using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 供放置水晶的区域UI
/// </summary>
public class CrystalArea : MonoBehaviour
{
    public CrystalPanel parentPanel;
    public DynamicWindow dynamicWindow;
    public Areas.Area Area;

    List<Transform> buttons;
    Text txt_;        //体力消耗
    public Dictionary<Transform, CrystalType> CrystalDict;

    public void Init(CrystalPanel parent, Areas.Area area)
    {
        parentPanel = parent;
        Area = area;

        buttons = new List<Transform>();
        buttons.Add(transform.Find("Btn_1"));
        buttons.Add(transform.Find("Btn_2"));
        buttons.Add(transform.Find("Btn_3"));
        txt_ = transform.Find("Text").GetComponent<Text>();

        CrystalDict = new Dictionary<Transform, CrystalType>();
        foreach (Transform item in buttons)
        {
            item.Find("Crystal").gameObject.SetActive(false);
            CrystalDict.Add(item, CrystalType.None);
        }
    }

    public void OnLateUpdate()
    {
        transform.position = Function.World2ScreenPoint(Area.topPosition + new Vector3(0, 0, 5));
        int count = 0;
        foreach (CrystalType type in CrystalDict.Values)
        {
            if (type != CrystalType.None)
            {
                count++;
            }
        }
        txt_.text = "区域体力：+" + count + "0/月";
    }

    public void SelectCrystal(Transform self)
    {
        parentPanel.SelectSeat(this, self);
    }

    /// <summary>
    /// 将水晶放置到槽位上，颜色变化
    /// </summary>
    /// <param name="self"></param>
    /// <param name="type"></param>
    public bool TryPutCrystal(Transform self, CrystalType type)
    {
        //foreach (var item in CrystalDict)
        //{
        //    if (item.Value == type)
        //    {  //已有同类
        //        return false;
        //    }
        //}

        if (CrystalDict[self] != CrystalType.None)
        {
            parentPanel.Manager.RecycleCrystal(type);
            CrystalDict[self] = CrystalType.None;
        }

        Image img = self.Find("Crystal").GetComponent<Image>();
        img.gameObject.SetActive(true);
        img.color = MonthMeeting.GetCrystalColor(type);
        parentPanel.Manager.PutCrystal(type);
        CrystalDict[self] = type;
        return true;
    }

    /// <summary>
    /// 将水晶移回仓库
    /// </summary>
    /// <param name="self"></param>
    public void RemoveCrystal(Transform self)
    {
        if (CrystalDict[self] == CrystalType.None) 
        {
            return;
        }

        Image img = self.Find("Crystal").GetComponent<Image>();
        img.gameObject.SetActive(false);
        parentPanel.Manager.RecycleCrystal(CrystalDict[self]);
        CrystalDict[self] = CrystalType.None;
    }

    /// <summary>
    /// 结束，结算水晶
    /// </summary>
    public void Settle()
    {
        List<CrystalType> types = new List<CrystalType>();
        foreach (var item in CrystalDict)
        {
            if (item.Value != CrystalType.None)
            {
                types.Add(item.Value);
            }
        }
        parentPanel.Manager.SettleArea(types, Area);
    }
}
