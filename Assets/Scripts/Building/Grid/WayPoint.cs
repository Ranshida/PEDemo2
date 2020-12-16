using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 员工AI无工作，漫步时的寻路点
/// </summary>
public class WayPoint : MonoBehaviour
{
    private List<WayPoint> allWayPoint;
    public bool Active;
    public Grid BelongGrid;
    [SerializeField]private  List<WayPoint> connectWayPoint;    //与此相连的其他路点

    public void Start()
    {
        WayPoint[] wayPoints = transform.parent.GetComponentsInChildren<WayPoint>();
        allWayPoint = Function.Array2List(wayPoints);
        if (!GridContainer.Instance.GetGrid(transform.position.x, transform.position.z, out BelongGrid))
            Debug.LogError("路点不属于任意一个网格");
    }

    //Get一个随机连接的路点
    public WayPoint GetConnect()
    {
        List<WayPoint> activeWayPoint = new List<WayPoint>();
        foreach (WayPoint wp in connectWayPoint)
        {
            if (wp.Active)
            {
                activeWayPoint.Add(wp);
            }
        }

        //没有相连的路点，从所有路点中找一个
        if (activeWayPoint.Count == 0) 
        {
            foreach (WayPoint wp in allWayPoint)
            {
                if (wp.Active)
                {
                    activeWayPoint.Add(wp);
                }
            }
        }
        return activeWayPoint[Random.Range(0, activeWayPoint.Count)];
    }
}
