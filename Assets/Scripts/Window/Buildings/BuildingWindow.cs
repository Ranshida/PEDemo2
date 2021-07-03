using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingWindow : WindowRoot
{
    private BuildingManage _Manage;
    private Building TempBuilding { get { return _Manage.Temp_Building; } }

    //初始化
    private GameObject wareBuilding;       //仓库中的临时建筑

    //建造面板        
    private Button btn_Dismantle;
    private Button btn_FinishBuild;

    //仓库面板
    public List<Transform> warePanels;     //仓库中存储的待建建筑
    private Transform wareBuildingParent;   //仓库面板

    InfoPanelTrigger m_PanelTrigger;

    private void Awake()
    {
        warePanels = new List<Transform>();

        wareBuilding = ResourcesLoader.LoadPrefab("Prefabs/UI/Building/WareBuilding");
        wareBuildingParent = transform.Find("Scroll View/Viewport/Content");
        btn_FinishBuild = transform.Find("Btn_Finish").GetComponent<Button>();
        btn_Dismantle = transform.Find("Btn_Dismantle").GetComponent<Button>();
    }

    protected override void OnActive()
    {
        _Manage = BuildingManage.Instance;

        //BuildingManage.Instance.AddInfoListener(transform.Find("Btn_技术部门"), BuildingType.技术部门);
        //BuildingManage.Instance.AddInfoListener(transform.Find("Btn_市场部门"), BuildingType.市场部门);
        //BuildingManage.Instance.AddInfoListener(transform.Find("Btn_产品部门"), BuildingType.产品部门);
        //BuildingManage.Instance.AddInfoListener(transform.Find("Btn_公关营销部"), BuildingType.公关营销部);
        //BuildingManage.Instance.AddInfoListener(transform.Find("Btn_高管办公室"), BuildingType.高管办公室);
        BuildingManage.Instance.AddInfoListener(transform.Find("Btn_人力资源部"), BuildingType.仓库);
        
        m_PanelTrigger = btn_FinishBuild.GetComponent<InfoPanelTrigger>();
    }

    protected override void UpdateSpecific()
    {
        if (warePanels.Count > 0 || _Manage.Temp_Building)
        {
            m_PanelTrigger.enabled = true;
            btn_FinishBuild.interactable = false;
            btn_FinishBuild.GetComponent<PointingSelf>().StartPointing = () => { btn_FinishBuild.GetComponent<InfoPanelTrigger>().PointerEnter(); };
            btn_FinishBuild.GetComponent<PointingSelf>().EndPointing = () => { btn_FinishBuild.GetComponent<InfoPanelTrigger>().PointerExit(); };
        }
        else
        {
            m_PanelTrigger.enabled = false;
            btn_FinishBuild.interactable = true;
            btn_FinishBuild.GetComponent<PointingSelf>().ClearAll();
        }

        //尝试退出建造模式
        if (btn_FinishBuild.interactable && (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Escape)))
        {
            btn_FinishBuild.onClick.Invoke();
        }

        btn_Dismantle.gameObject.SetActive(false);
        if (TempBuilding)
        {
            if (TempBuilding.CanDismantle)
                btn_Dismantle.gameObject.SetActive(true);
        }
    }

    protected override void OnButton(string btnName)
    {
        switch (btnName)
        {
            case "Btn_Finish":
                _Manage.TryQuitBuildMode();
                break;
            case "Btn_Dismantle":
                _Manage.DismantleBuilding(_Manage.Temp_Building);
                break;
            case "Btn_技术部门":
                //_Manage.StartBuildNew(BuildingType.技术部门);
                break;   
            case "Btn_市场部门":
                //_Manage.StartBuildNew(BuildingType.市场部门);
                break;   
            case "Btn_产品部门":
                //_Manage.StartBuildNew(BuildingType.产品部门);
                break;    
            case "Btn_公关营销部":
                //_Manage.StartBuildNew(BuildingType.公关营销部);
                break;   
            case "Btn_高管办公室":
                //_Manage.StartBuildNew(BuildingType.高管办公室);
                break; 
            case "Btn_人力资源部":
                _Manage.StartBuildNew(BuildingType.自动化研究中心);
                break;
            default:
                break;
        }
    }

    public void PutIntoWare(Building building)
    {
        //放到仓库里
        Transform panel = GameObject.Instantiate(wareBuilding, wareBuildingParent).transform;
        building.WarePanel = panel;
        warePanels.Add(panel);
        panel.name = building.Type.ToString();
        panel.GetComponentInChildren<Text>().text = building.Type.ToString();
        panel.GetComponent<Button>().onClick.AddListener(() =>
        {
            _Manage.StartBuild(building);
            warePanels.Remove(panel);
            Destroy(panel.gameObject);
        });
    }
}
