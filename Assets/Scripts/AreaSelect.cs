using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaSelect : MonoBehaviour
{
    public Area area;

    private void Update()
    {
        if (area != null)
        {
            transform.position = Function.World2ScreenPoint(area.centerPosition);
        }
    }

    public void SelectArea()
    {

        if (GameControl.Instance.AreaSelectMode == 2)
        {
            GameControl.Instance.CurrentItem.TargetArea = area;
            GameControl.Instance.CurrentItem.UseItem();
        }
        //选择后关闭所有按钮
        GameControl.Instance.AC.CloseAllAS();
    }
}
