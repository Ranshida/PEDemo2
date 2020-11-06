using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 员工AI无工作，漫步时的寻路点
/// </summary>
public class WayPoint : MonoBehaviour
{
    public Grid BelongGrid;
    [SerializeField]private  List<WayPoint> connectWayPoint;    //与此相连的其他路点

    public void Start()
    {
        if (!GridContainer.Instance.GetGrid(transform.position.x, transform.position.z, out BelongGrid))
            Debug.LogError("路点不属于任意一个网格");
    }

    //Get一个随机连接的路点
    public WayPoint GetConnect()
    {
        return connectWayPoint[Random.Range(0, connectWayPoint.Count)];
    }
}
