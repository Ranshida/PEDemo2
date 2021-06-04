using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 将网格划分为多个区域
/// </summary>
public class Areas : MonoBehaviour
{
    public GameControl GC;
    public AreaSelect ASPrefab;
    public DivisionControl DCPrefab;
    public Transform ASContent, DivContent;
    public GameObject CancelButton;

    public List<Area> AreaLists;
    public List<AreaSelect> ASList = new List<AreaSelect>();

    public void Init()
    {
        int num = 1;
        foreach (Area area in AreaLists)
        {
            area.Init();
            //初始化AS
            AreaSelect AS = Instantiate(ASPrefab, ASContent);
            AS.area = area;
            area.AS = AS;
            AS.gameObject.SetActive(false);
            ASList.Add(AS);

            //初始化DC
            DivisionControl DC = Instantiate(DCPrefab, DivContent);
            UIManager.Instance.OnAddNewWindow(DC.DetailPanel.gameObject.GetComponent<WindowBaseControl>());
            DC.GC = GC;
            GC.TurnEvent.AddListener(DC.Produce);
            DC.CurrentArea = area;
            area.DC = DC;
            DC.DivName = "事业部" + num;
            DC.Text_DivPanelName.text = DC.DivName;
            num += 1;

            DepSelect ds = Instantiate(GC.DepSelectButtonPrefab, GC.DepSelectContent);
            ds.DivC = DC;
            ds.Text_DepName.text = DC.DivName;
            DC.DS = ds;
            ds.GC = GC;

            //添加事业部至GameContral中的链表
            GC.CurrentDivisions.Add(DC);

            if (area.gridList[0].Lock == true)
            {
                DC.gameObject.SetActive(false);
                DC.Locked = true;
            }
        }
        MonthMeeting.Instance.CrystalPanel.InitCrystalPanel();
        AreaLists[0].UnlockGrid();
    }

    public void CloseAllAS()
    {
        foreach (AreaSelect index in ASList)
        {
            index.gameObject.SetActive(false);
        }
        CancelButton.SetActive(false);
    }

    //显示可用的选项
    public void ShowAvailableAS()
    {
        foreach (AreaSelect index in ASList)
        {
            if (index.area.gridList[0].Lock == false)
                index.gameObject.SetActive(true);
        }
        CancelButton.SetActive(true);
    }
}
[System.Serializable]
public class Area
{
    public List<Grid> gridList;

    public CrystalArea CA;
    public AreaSelect AS;
    public DivisionControl DC;
    public Vector3 centerPosition;
    public Vector3 topPosition;

    public void Init()
    {
        centerPosition = Vector3.zero;
        foreach (Grid grid in gridList)
        {
            centerPosition += grid.transform.position;
            grid.BelongedArea = this;
        }
        centerPosition /= gridList.Count;

        float posX = 0;
        topPosition = Vector3.zero;
        foreach (Grid grid in gridList)
        {
            if (grid.transform.position.z > topPosition.z)
            {
                topPosition = new Vector3(0, 0, grid.transform.position.z);
            }
            posX += grid.transform.position.x;
            grid.Type = Grid.GridType.道路;
            grid.RefreshGrid();
        }
        posX /= gridList.Count;
        topPosition = new Vector3(posX, 0, topPosition.z);


    }

    public void UnlockGrid()
    {
        foreach (Grid g in gridList)
        {
            g.Type = Grid.GridType.可放置;
            g.RefreshGrid();
        }
        CA.gameObject.SetActive(true);
    }
}
