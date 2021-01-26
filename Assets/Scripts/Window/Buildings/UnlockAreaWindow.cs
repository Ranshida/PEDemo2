using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 提示解锁新区域的弹窗面板
/// </summary>
public class UnlockAreaWindow : WindowRoot
{
    private BuildingManage _Manage;

    private Button btn_Yes;

    protected override void InitSpecific()
    {
        _Manage = BuildingManage.Instance;
        btn_Yes = transform.Find("Btn_Yes").GetComponent<Button>();
    }

    protected override void UpdateSpecific()
    {
        if (GameControl.Instance.Money >= 2000)
            btn_Yes.interactable = true;
        else
            btn_Yes.interactable = false;
    }

    protected override void OnButton(string btnName)
    {
        switch (btnName)
        {
            case "Btn_Yes":
                 _Manage.UnLockArea();
                break;
            case "Btn_No":
                SetWndState(false);
                break;
            default:
                break;
        }
    }
}
