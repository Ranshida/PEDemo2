using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 将网格划分为多个区域
/// </summary>
public class Areas : MonoBehaviour
{
    public List<Area> AreaLists;

    public void Init()
    {
        foreach (Area area in AreaLists)
        {
            area.Init();
        }
    }

    [System.Serializable]
    public class Area
    {
        public List<Grid> gridList;

        public Vector3 centerPosition;

        public void Init()
        {
            centerPosition = Vector3.zero;
            foreach (Grid grid in gridList)
            {
                centerPosition += grid.transform.position;
            }
            centerPosition /= gridList.Count;
        }
    }
}
