using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 供放置水晶的区域UI
/// </summary>
public class CrystalArea : MonoBehaviour
{
    public int BlueCount, WhiteCount, OrangeCount, GrayCount, BlackCount;

    public CrystalPanel parentPanel;
    public DynamicWindow dynamicWindow;
    public Area Area;

    List<Transform> buttons;
    Text txt_;        //体力消耗
    public Dictionary<Transform, CrystalType> CrystalDict;

    public void Init(CrystalPanel parent, Area area)
    {
        parentPanel = parent;
        Area = area;

        buttons = new List<Transform>();
        for(int i = 1; i < 10; i++)
        {
            string name = "Btn_" + i;
            buttons.Add(transform.Find(name));
            if (i > 3)
                buttons[i - 1].gameObject.SetActive(false);
        }
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
        BlueCount = 0;
        GrayCount = 0;
        OrangeCount = 0;
        WhiteCount = 0;
        BlackCount = 0;
        foreach (CrystalType type in CrystalDict.Values)
        {
            if (type != CrystalType.None)
            {
                count++;
            }
            if (type == CrystalType.Blue)
                BlueCount++;
            else if (type == CrystalType.Gray)
                GrayCount++;
            else if (type == CrystalType.Orange)
                OrangeCount++;
            else if (type == CrystalType.White)
                WhiteCount++;
            else if (type == CrystalType.Black)
                BlackCount++;
        }
        txt_.text = "区域体力：+" + count + "0/月";
    }

    public void SelectCrystal(Transform self)
    {
        //观察模式下不能选
        if (MonthMeeting.Instance.MeetingStart == false)
            return;
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

    public void ResetAllCrystal()
    {
        foreach (Transform item in buttons)
        {
            Image img = item.Find("Crystal").GetComponent<Image>();
            img.gameObject.SetActive(false);
        }
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

    //增加一个槽位
    public void AddOption()
    {
        foreach(Transform button in buttons)
        {
            if(button.gameObject.activeSelf == false)
            {
                button.gameObject.SetActive(true);
                break;
            }
        }
    }
}
