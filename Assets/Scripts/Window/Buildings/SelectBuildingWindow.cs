using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 选中建筑时的浮动面板
/// </summary>
public class SelectBuildingWindow : WindowRoot
{
    private BuildingManage _Manage;

    //选中建筑时的面板
    Transform btn_Detail;
    Transform btn_BuildMode;
    Transform btn_Move;

    private void Awake()
    {
        btn_Detail = transform.Find("Btn_Detail");
        btn_BuildMode = transform.Find("Btn_BuildMode");
        btn_Move = transform.Find("Btn_Move");
    }

    protected override void OnActive()
    {
        _Manage = BuildingManage.Instance;

        if (_Manage.InBuildMode)
        {
            OnEnterBuildMode();
        }
        else
        {
            _Manage.OpenDetailPanel();
            //OnQuitBuildMode();
        }
    }

    protected override void OnButton(string btnName)
    {
        switch (btnName)
        {
            case "Btn_BuildMode":
                _Manage.EnterBuildMode();
                break;
            case "Btn_Detail":
                _Manage.OpenDetailPanel();
                break;
            case "Btn_Move":
                _Manage.MoveBuilding();
                break;
            default:
                break;
        }
    }

    public void OnEnterBuildMode()
    {
        Building select = _Manage.SelectBuilding;
        if (!select)
        {
            return;
        }

        gameObject.SetActive(true);
        transform.Find("Text").GetComponent<Text>().text = select.Type.ToString();
        btn_Detail.gameObject.SetActive(false);
        btn_BuildMode.gameObject.SetActive(false);
        btn_Move.gameObject.SetActive(true);
    }

    //(尝试)退出建造模式
    public void OnQuitBuildMode()
    {
        Building select = _Manage.SelectBuilding;
        if (!select)
        {
            return;
        }

        gameObject.SetActive(true);
        transform.Find("Text").GetComponent<Text>().text = select.Type.ToString();
        btn_Detail.gameObject.SetActive(true);
        btn_BuildMode.gameObject.SetActive(true);
        btn_Move.gameObject.SetActive(false);

        if (select.Department != null)
        {
            btn_Detail.GetComponent<Button>().interactable = true;
            btn_Detail.GetComponent<PointingSelf>().ClearAll();
        }
        else
        {
            btn_Detail.GetComponent<Button>().interactable = false;
            btn_Detail.GetComponent<PointingSelf>().StartPointing = () => { btn_Detail.GetComponent<InfoPanelTrigger>().PointerEnter(); };
            btn_Detail.GetComponent<PointingSelf>().EndPointing = () => { btn_Detail.GetComponent<InfoPanelTrigger>().PointerExit(); };
        }
    }
}
