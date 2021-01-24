using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingWindow : WindowRoot
{
    private BuildingManage _Manage;
    private bool InBuildMode { get { return _Manage.InBuildMode; } }
    private Building TempBuilding { get { return _Manage.Temp_Building; } }

    //初始化
    private GameObject lotteryBuilding;        //抽卡的UI按钮图标
    private BuildingDescription description;   //抽卡的建筑描述
    private GameObject wareBuilding;       //仓库中的临时建筑

    //建造面板        
    public Button btn_NewArea;
    private Button btn_Dismantle;
    private Button btn_FinishBuild;
    private Transform unlockGridPanel;        //解锁网格的询问面板

    //仓库面板
    private List<Transform> warePanels;     //仓库中存储的待建建筑
    private Transform warePanel;            //仓库面板
    private Transform wareBuildingsPanel;   //仓库面板

    //抽卡面板
    private Transform lotteryPanel;
    private List<Transform> lotteryUI;

    //选中建筑时的面板
    private Transform selectBuildingPanel;
    Transform btn_Detail;
    Transform btn_BuildMode;
    Transform btn_Move;

    public bool Lotterying { get; private set; }
    InfoPanelTrigger m_PanelTrigger;

    private void Awake()
    {
        lotteryUI = new List<Transform>();
        warePanels = new List<Transform>();

        lotteryBuilding = ResourcesLoader.LoadPrefab("Prefabs/UI/Building/LotteryBuilding");
        wareBuilding = ResourcesLoader.LoadPrefab("Prefabs/UI/Building/WareBuilding");

        unlockGridPanel = transform.Find("UnlockNewArea");
        lotteryPanel = transform.Find("LotteryPanel");
        warePanel = transform.Find("WarePanel");
        wareBuildingsPanel = warePanel.Find("Scroll View/Viewport/Content");

        btn_FinishBuild = transform.Find("WarePanel/Btn_Finish").GetComponent<Button>();
        btn_Dismantle = transform.Find("WarePanel/Btn_Dismantle").GetComponent<Button>();


        selectBuildingPanel = transform.Find("SelectBuilding");
        btn_Detail = selectBuildingPanel.transform.Find("Btn_Detail");
        btn_BuildMode = selectBuildingPanel.transform.Find("Btn_BuildMode");
        btn_Move = selectBuildingPanel.transform.Find("Btn_Move");

        description = transform.Find("BuildingDescriptionPanel").GetComponent<BuildingDescription>();
        description.Init();
    }

    protected override void InitSpecific()
    {
        _Manage = BuildingManage.Instance;

        warePanel.Find("Btn_技术部门").GetComponent<LotteryBuilding>().Init(description, BuildingType.技术部门, new BoolenValue(true, false));
        warePanel.Find("Btn_市场部门").GetComponent<LotteryBuilding>().Init(description, BuildingType.市场部门, new BoolenValue(true, false));
        warePanel.Find("Btn_产品部门").GetComponent<LotteryBuilding>().Init(description, BuildingType.产品部门, new BoolenValue(true, false));
        warePanel.Find("Btn_公关营销部").GetComponent<LotteryBuilding>().Init(description, BuildingType.公关营销部, new BoolenValue(true, false));
        warePanel.Find("Btn_高管办公室").GetComponent<LotteryBuilding>().Init(description, BuildingType.高管办公室, new BoolenValue(true, false));
        warePanel.Find("Btn_人力资源部").GetComponent<LotteryBuilding>().Init(description, BuildingType.人力资源部, new BoolenValue(true, false));
      
        
        m_PanelTrigger = btn_FinishBuild.GetComponent<InfoPanelTrigger>();
        //m_PanelTrigger.Init();
    }

    protected override void Update()
    {
        base.Update();

        if (InBuildMode)
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

            btn_Detail.gameObject.SetActive(false);
            btn_BuildMode.gameObject.SetActive(false);
            btn_Move.gameObject.SetActive(true);
        }
        else
        {
            btn_Detail.gameObject.SetActive(true);
            btn_BuildMode.gameObject.SetActive(true);
            btn_Move.gameObject.SetActive(false);
        }
    }

    protected override void OnButton(string btnName)
    {
        switch (btnName)
        {
            case "Btn_Finish":
                TryQuitBuildMode();
                break;
            case "Btn_Dismantle":
                _Manage.DismantleBuilding(_Manage.Temp_Building);
                break;
            case "Btn_技术部门":
                BuildBasicBuilding(BuildingType.技术部门);
                break;   
            case "Btn_市场部门":
                BuildBasicBuilding(BuildingType.市场部门);
                break;   
            case "Btn_产品部门":
                BuildBasicBuilding(BuildingType.产品部门);
                break;    
            case "Btn_公关营销部":
                BuildBasicBuilding(BuildingType.公关营销部);
                break;   
            case "Btn_高管办公室":
                BuildBasicBuilding(BuildingType.高管办公室);
                break; 
            case "Btn_人力资源部":
                BuildBasicBuilding(BuildingType.人力资源部);
                break;
            case "Btn_GiveUp":
                GiveUpLottery();
                break;   
            case "Btn_BuildMode":
                _Manage.EnterBuildMode();
                break;   
            case "Btn_Detail":
                _Manage.OpenDetailPanel();
                break;
            case "Btn_Move":
                _Manage.MoveBuilding();
                break;
            case "Btn_UnlockYes":
                UnlockNewArea();
                break;
            default:
                break;
        }
    }

    public void OnSelectBuilding(Building select)
    {
        selectBuildingPanel.gameObject.SetActive(true);
        selectBuildingPanel.Find("Text").GetComponent<Text>().text = select.Type.ToString();

     
        if (!InBuildMode)
        {
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
        else
        {
            btn_Detail.gameObject.SetActive(false);
            btn_BuildMode.gameObject.SetActive(false);
            btn_Move.gameObject.SetActive(true);
        }
    }

    public void OnUnselectBuilding()
    {
        selectBuildingPanel.gameObject.SetActive(false);
    }

    public void MoveBuilding()
    {
        _Manage.MoveBuilding();
        selectBuildingPanel.gameObject.SetActive(false);
    }

    public void Lottery(List<BuildingType> tempBuildings)
    {
        Lotterying = true;
        lotteryPanel.gameObject.SetActive(true);

        foreach (BuildingType type in tempBuildings)
        {
            Transform panel = GameObject.Instantiate(lotteryBuilding, lotteryPanel.Find("List")).transform;
            lotteryUI.Add(panel);
            LotteryBuilding lottery = panel.GetComponent<LotteryBuilding>();
            lottery.Init(description, type);

            Action action = () =>
            {
                //装饰设施
                if (type == BuildingType.空)
                {
                    List<Building> decorate = new List<Building>();
                    foreach (KeyValuePair<BuildingType, Building> building in _Manage.m_AllBuildings)
                    {
                        if (building.Value.Str_Type == "装饰设施")
                        {
                            decorate.Add(building.Value);
                        }
                    }
                    _Manage.StartBuildNew(decorate[UnityEngine.Random.Range(0, decorate.Count)].Type);
                }
                //普通建筑
                else
                {
                    _Manage.StartBuildNew(type);
                }
                QuestControl.Instance.Finish(11);
            };

            panel.transform.GetComponent<Button>().onClick.AddListener(() =>
            {
                action();
                Lotterying = false; 
                lotteryPanel.gameObject.SetActive(false);
                warePanel.gameObject.SetActive(true);
                description.gameObject.SetActive(false);
                foreach (Transform ui in lotteryUI)
                {
                    Destroy(ui.gameObject);
                }
                lotteryUI.Clear();
            });
        }
    } 
    
    //放弃这次抽奖
    public void GiveUpLottery()
    {
        Lotterying = false;
        QuestControl.Instance.Finish(11);
        lotteryPanel.gameObject.SetActive(false);
        foreach (Transform ui in lotteryUI)
        {
            Destroy(ui.gameObject);
        }
        lotteryUI.Clear();
        TryQuitBuildMode();
    }

    public void OnEnterBuildMode()
    {
        warePanel.gameObject.SetActive(true);
        if (_Manage.SelectBuilding)
        {
            selectBuildingPanel.gameObject.SetActive(true);
            selectBuildingPanel.Find("Text").GetComponent<Text>().text = _Manage.SelectBuilding.Type.ToString();

            description.gameObject.SetActive(false);
        }
    }

    //(尝试)退出建造模式
    public bool TryQuitBuildMode()
    {
        if (Lotterying || warePanels.Count > 0)
        {
            return false;
        }

        bool quit = BuildingManage.Instance.TryQuitBuildMode();
        if (!quit)
        {
            return false;
        }

        warePanel.gameObject.SetActive(false);
        selectBuildingPanel.gameObject.SetActive(false);
        return true;
    }

    public void PutIntoWare(Building building)
    {
        //放到仓库里
        Transform panel = GameObject.Instantiate(wareBuilding, wareBuildingsPanel).transform;
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

    //按钮方法，打开询问解锁新区域的面板
    public void AskUnlockArea()
    {
        //弹出面板询问是否要继续，继续需要花2000块
        unlockGridPanel.gameObject.SetActive(true);

        Button btn_Yes = unlockGridPanel.Find("Btn_UnlockYes").GetComponent<Button>();
        if (GameControl.Instance.Money > 2000)
            btn_Yes.interactable = true;
        else
            btn_Yes.interactable = false;
    }
    private int m_Area = 0;
    //确定解锁新区域
    public void UnlockNewArea()
    {
        if (GameControl.Instance.Money < 2000)
        {
            Debug.Log("金钱不够");
            return;
        }
        GameControl.Instance.Money -= 2000;
        GridContainer.Instance.UnlockGrids(m_Area);
        m_Area++;
        if (m_Area >= 3)
        {
            btn_NewArea.gameObject.SetActive(false);
        }
    }

    //建造基础建筑按钮
    public void BuildBasicBuilding(BuildingType type)
    {
        BuildingManage.Instance.StartBuildNew(type);
    }
}
