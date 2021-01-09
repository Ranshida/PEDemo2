using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum BuildingType
{
    技术部门, 市场部门, 产品部门, 公关营销部, 高管办公室, CEO办公室, 研发部门, 会议室, 智库小组, 人力资源部,
    心理咨询室, 财务部, 体能研究室, 按摩房, 健身房, 宣传中心, 科技工作坊, 绩效考评中心, 员工休息室, 人文沙龙,
    兴趣社团, 电子科技展, 冥想室, 特别秘书处, 成人再教育所, 职业再发展中心, 中央监控室, 谍战中心, 室内温室, 整修楼顶,
    游戏厅, 营养师定制厨房, 咖啡bar, 花盆, 长椅, 自动贩卖机, 空
}

public class BuildingManage : MonoBehaviour
{
    public static BuildingManage Instance;
    public Transform ExitPos;

    //初始化
    private GameObject lotteryBuilding;        //抽卡的UI按钮图标
    private BuildingDescription description;   //抽卡的建筑描述
    private GameObject wareBuilding;       //仓库中的临时建筑
    private GameObject[] buildingPrefabs;  //建筑预制体
    private Dictionary<BuildingType, Building> m_SelectDict;        //可以抽取的建筑字典  <建筑类型，预制体> 的字典
    public Dictionary<BuildingType, GameObject> m_AllBuildingDict;   //  <建筑类型，预制体> 的字典
    public string[,] BuildingExcelValue;

    //准备建造的建筑
    private int m_GridX;
    private int m_GridZ;
    private bool m_CanBuild;
    private List<Grid> temp_Grids;
    private Building temp_Building;
    private GameObject CEOBuilding;

    //建造面板        
    public GameObject SelectedPanel;
    public Button btn_NewArea;
    private Button btn_Dismantle;
    private Button btn_FinishBuild;
    private Transform unlockGridPanel;        //解锁网格的询问面板
    private Transform lotteryPanel;   //抽卡面板
    private Transform lotteryList;    //抽卡表
    private List<Transform> lotteryUI;   //抽卡面板
    private List<Transform> warePanels;  //仓库中存储的待建建筑
    private List<Building> wareBuildings;  //仓库中存储的待建建筑    
    private Transform warePanel;     //仓库面板
    private Transform wareBuildingsPanel;     //仓库面板
    private Transform selectBuildingPanel;     //仓库面板
    private bool m_InBuildingMode;          //建造中


    //默认建筑（CEO办公室）
    public List<Building> ConstructedBuildings { get; private set; } = new List<Building>();
    public OfficeControl CEOOffice;

    //选中的建筑
    private Building m_SelectBuilding;
    private GameObject m_EffectHalo;

    //屏幕射线位置
    public Vector3 AimingPosition = Vector3.zero;

    InfoPanelTrigger m_PanelTrigger;

    private void Awake()
    {
        Instance = this;
        m_InBuildingMode = false;
        lotteryUI = new List<Transform>();
        warePanels = new List<Transform>();
        wareBuildings = new List<Building>();
        temp_Grids = new List<Grid>();
        m_AllBuildingDict = new Dictionary<BuildingType, GameObject>();
        m_SelectDict = new Dictionary<BuildingType, Building>();

        //加载建筑预制体，加入建筑字典

        lotteryBuilding = ResourcesLoader.LoadPrefab("Prefabs/UI/Building/LotteryBuilding");
        wareBuilding = ResourcesLoader.LoadPrefab("Prefabs/UI/Building/WareBuilding");
        buildingPrefabs = ResourcesLoader.LoadAll<GameObject>("Prefabs/Scene/Buildings");
        CEOBuilding = ResourcesLoader.LoadPrefab("Prefabs/Scene/Buildings/CEO办公室");
        m_EffectHalo = Instantiate(ResourcesLoader.LoadPrefab("Prefabs/Scene/EffectHalo"), transform);
        BuildingExcelValue = ExcelTool.ReadAsString(Application.streamingAssetsPath + "/Excel/BuildingFunction.xlsx");

        foreach (GameObject prefab in buildingPrefabs)
        {
            Building building = prefab.GetComponent<Building>();
            building.LoadPrefab(BuildingExcelValue);
            if (building.Type != BuildingType.空)
                m_AllBuildingDict.Add(building.Type, prefab);
        }
        foreach (GameObject prefab in buildingPrefabs)
        {
            Building building = prefab.GetComponent<Building>();
            ////基础建筑物不能通过抽卡获取
            //if (building.CanLottery)
            //{
            //    m_SelectDict.Add(building.Type, building);
            //}
            if (building.Type == BuildingType.心理咨询室 || building.Type == BuildingType.健身房 || building.Type == BuildingType.绩效考评中心
                || building.Type == BuildingType.员工休息室 || building.Type == BuildingType.冥想室 || building.Type == BuildingType.宣传中心 || building.Type == BuildingType.兴趣社团)
            {
                m_SelectDict.Add(building.Type, building);
            }
        }
    }

    private void Start()
    {
        unlockGridPanel = transform.Find("UnlockNewArea");
           lotteryPanel = transform.Find("LotteryPanel");
        lotteryList = lotteryPanel.Find("List");
        warePanel = transform.Find("WarePanel");
        wareBuildingsPanel = warePanel.Find("Scroll View/Viewport/Content");
        selectBuildingPanel = transform.Find("SelectBuilding");
        btn_FinishBuild = transform.Find("WarePanel/Btn_Finish").GetComponent<Button>();
        btn_Dismantle = transform.Find("WarePanel/Btn_Dismantle").GetComponent<Button>();
        description = transform.Find("BuildingDescriptionPanel").GetComponent<BuildingDescription>();
        description.Init();
        m_PanelTrigger = btn_FinishBuild.GetComponent<InfoPanelTrigger>();
        m_PanelTrigger.Init();

        warePanel.Find("Btn_技术部门").GetComponent<LotteryBuilding>().Init(description, BuildingType.技术部门, () => { BuildBasicBuilding(BuildingType.技术部门); }, new BoolenValue(true, false)) ;
        warePanel.Find("Btn_市场部门").GetComponent<LotteryBuilding>().Init(description, BuildingType.市场部门, () => { BuildBasicBuilding(BuildingType.市场部门); }, new BoolenValue(true, false));
        warePanel.Find("Btn_产品部门").GetComponent<LotteryBuilding>().Init(description, BuildingType.产品部门, () => { BuildBasicBuilding(BuildingType.产品部门); }, new BoolenValue(true, false));
        warePanel.Find("Btn_公关营销部").GetComponent<LotteryBuilding>().Init(description, BuildingType.公关营销部, () => { BuildBasicBuilding(BuildingType.公关营销部); }, new BoolenValue(true, false));
        warePanel.Find("Btn_高管办公室").GetComponent<LotteryBuilding>().Init(description, BuildingType.高管办公室, () => { BuildBasicBuilding(BuildingType.高管办公室); }, new BoolenValue(true, false));
        warePanel.Find("Btn_人力资源部").GetComponent<LotteryBuilding>().Init(description, BuildingType.人力资源部, () => { BuildBasicBuilding(BuildingType.人力资源部); }, new BoolenValue(true, false));

        //warePanel.Find("Btn_技术部门").GetComponent<Button>().onClick.AddListener(() => { BuildBasicBuilding(BuildingType.技术部门); });
        //warePanel.Find("Btn_市场部门").GetComponent<Button>().onClick.AddListener(() => { BuildBasicBuilding(BuildingType.市场部门); });
        //warePanel.Find("Btn_产品部门").GetComponent<Button>().onClick.AddListener(() => { BuildBasicBuilding(BuildingType.产品部门); });
        //warePanel.Find("Btn_公关营销部").GetComponent<Button>().onClick.AddListener(() => { BuildBasicBuilding(BuildingType.公关营销部); });
        //warePanel.Find("Btn_高管办公室").GetComponent<Button>().onClick.AddListener(() => { BuildBasicBuilding(BuildingType.高管办公室); });
        //warePanel.Find("Btn_人力资源部").GetComponent<Button>().onClick.AddListener(() => { BuildBasicBuilding(BuildingType.人力资源部); });
            
        m_EffectHalo.SetActive(false);
        InitBuilding(BuildingType.CEO办公室, new Int2(3, 8));
        //InitBuilding(BuildingType.人力资源部, new Int2(0, 8));
    }

    //初始化默认建筑
    void InitBuilding(BuildingType type, Int2 leftDownGird)
    {
        StartBuildNew(type);
        
        Grid grid;
        Dictionary<int, Grid> gridDict;
        for (int i = 0; i < temp_Building.Length; i++)
        {
            //if (GridContainer.Instance.GridDict.TryGetValue(CEOPositionX + i, out gridDict))
            if (GridContainer.Instance.GridDict.TryGetValue(leftDownGird.X + i, out gridDict))
            {
                for (int j = 0; j < temp_Building.Width; j++)
                {
                    if (gridDict.TryGetValue(leftDownGird.Y + j, out grid))
                    {
                        if (!gridDict[leftDownGird.Y + j].Lock && gridDict[leftDownGird.Y + j].Type == Grid.GridType.可放置)
                        {
                            temp_Grids.Add(gridDict[leftDownGird.Y + j]);
                        }
                    }
                }
            }
        }

        //全部覆盖到网格 => 可以建造
        if (temp_Grids.Count == temp_Building.Width * temp_Building.Length)
        {
            temp_Building.transform.position = new Vector3(leftDownGird.X * 10, 0, leftDownGird.Y * 10);
            BuildConfirm(temp_Building, temp_Grids);
        }
        else
        {
            Debug.LogError("无法建造，检查坐标");
        }
        temp_Grids.Clear();
        temp_Building = null;
    }

    private void Update()
    {
        if (!GridContainer.Instance)
            return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            Lottery(4);
        }

        //屏幕射线命中地面
        if (CameraController.BuildingHit && !CameraController.IsPointingUI)
                AimingPosition = new Vector3(CameraController.TerrainRaycast.point.x, 0, CameraController.TerrainRaycast.point.z);
            else
                AimingPosition = new Vector3(-1000, 0, 0);
        //鼠标所属网格的X坐标
        if (AimingPosition.x > 0)
            m_GridX = (int)(AimingPosition.x / 10);
        else
            m_GridX = (int)(AimingPosition.x / 10) - 1;
        //鼠标所属网格的Z坐标
        if (AimingPosition.z > 0)
            m_GridZ = (int)(AimingPosition.z / 10);
        else
            m_GridZ = (int)(AimingPosition.z / 10) - 1;
        btn_Dismantle.gameObject.SetActive(false);

        //建造模式下
        if (m_InBuildingMode)
        {
            if (warePanels.Count > 0 || temp_Building)
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
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                //确定建造当前建筑
                if (temp_Building && m_CanBuild)
                {
                    BuildConfirm(temp_Building, temp_Grids);
                    temp_Building = null;
                }
                //选中建筑
                else if (GridContainer.Instance.GridDict.TryGetValue(m_GridX, out Dictionary<int, Grid> dict))
                {
                    if (dict.TryGetValue(m_GridZ, out Grid grid))
                    {
                        if (grid.Type == Grid.GridType.已放置)
                        {
                            SelectBuilding(GridContainer.Instance.GridDict[m_GridX][m_GridZ].BelongBuilding);
                            selectBuildingPanel.gameObject.SetActive(true);
                            selectBuildingPanel.Find("Text").GetComponent<Text>().text = m_SelectBuilding.Type.ToString();
                        }
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                //取消建造当前建筑
                if (temp_Building)
                    BuildCancel();
                //取消选中
                if (m_SelectBuilding)
                    UnselectBuilding();
            }

            //刷新临时建筑网格和字段
            foreach (Grid grid in temp_Grids)
            {
                grid.IsPutting = false;
                grid.RefreshGrid();
            }
            temp_Grids.Clear();
            m_CanBuild = false;

            //已经选择建筑，检测网格可否可以建造
            if (temp_Building)
            {
                //检查覆盖到的网格
                Grid grid;
                Dictionary<int, Grid> gridDict;
                for (int i = 0; i < temp_Building.Length; i++)
                {
                    if (GridContainer.Instance.GridDict.TryGetValue(m_GridX + i, out gridDict))
                    {
                        for (int j = 0; j < temp_Building.Width; j++)
                        {
                            if (gridDict.TryGetValue(m_GridZ + j, out grid))
                            {
                                if (!gridDict[m_GridZ + j].Lock && gridDict[m_GridZ + j].Type == Grid.GridType.可放置)
                                {
                                    temp_Grids.Add(gridDict[m_GridZ + j]);
                                }
                            }
                        }
                    }
                }

                //全部覆盖到网格 => 可以建造
                if (temp_Grids.Count == temp_Building.Width * temp_Building.Length)
                {
                    m_CanBuild = true;
                    foreach (Grid tempGrid in temp_Grids)
                    {
                        tempGrid.IsPutting = true;
                        tempGrid.RefreshGrid();
                    }
                    temp_Building.transform.position = new Vector3(m_GridX * 10, 0, m_GridZ * 10);
                    return;
                }
                //不能覆盖全部网格 => 不能建造
                else
                    temp_Building.transform.position = AimingPosition + new Vector3(-5, 0, -5);

                if (temp_Building.CanDismantle)
                {
                    btn_Dismantle.gameObject.SetActive(true);
                    btn_Dismantle.onClick.RemoveAllListeners();
                    btn_Dismantle.onClick.AddListener(() => { DismantleBuilding(temp_Building); });
                }
            }
        }
        //非建筑模式下
        else
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                //选中
                if (GridContainer.Instance.GridDict.TryGetValue(m_GridX, out Dictionary<int, Grid> dict))
                {
                    if (dict.TryGetValue(m_GridZ, out Grid grid))
                    {
                        if (grid.Type == Grid.GridType.已放置)
                            SelectBuilding(GridContainer.Instance.GridDict[m_GridX][m_GridZ].BelongBuilding);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                //取消选中
                if (m_SelectBuilding)
                    UnselectBuilding();
            }
        }

        //建筑的辐射范围光环
        if (m_SelectBuilding)
        {
            m_EffectHalo.transform.position = m_SelectBuilding.transform.position + new Vector3(m_SelectBuilding.Length * 5, 0.2f, m_SelectBuilding.Width * 5);
            m_EffectHalo.transform.localScale = new Vector3(m_SelectBuilding.Length + m_SelectBuilding.EffectRange * 2, 1, m_SelectBuilding.Width + m_SelectBuilding.EffectRange * 2);
        }

        if (GridContainer.Instance.GridDict.TryGetValue(m_GridX, out Dictionary<int, Grid> dicttt))
        {
            if (dicttt.TryGetValue(m_GridZ, out Grid grid))
            {
                if (grid.Type == Grid.GridType.已放置)
                {
                    Building mouseBuilding = GridContainer.Instance.GridDict[m_GridX][m_GridZ].BelongBuilding;
                    if (mouseBuilding.Department)
                    {
                        DynamicWindow.Instance.SetEmpName(mouseBuilding.Department.Text_DepName.text, mouseBuilding.transform, Vector3.up * 15 + new Vector3(mouseBuilding.Length * 5, 0, mouseBuilding.Width * 5));
                    }
                    else if (mouseBuilding.Office)
                    {
                        DynamicWindow.Instance.SetEmpName(mouseBuilding.Office.Text_OfficeName.text, mouseBuilding.transform, Vector3.up * 15 + new Vector3(mouseBuilding.Length * 5, 0, mouseBuilding.Width * 5));
                    }
                }
            }
        }
    }
    void SelectBuilding(Building building)
    {
        m_SelectBuilding = building;
        m_EffectHalo.SetActive(true);

        if (!m_InBuildingMode)
        {
            SelectedPanel.gameObject.SetActive(true);
            SelectedPanel.transform.Find("Text").GetComponent<Text>().text = m_SelectBuilding.Type.ToString();
            if (m_SelectBuilding.Department != null)
            {
                SelectedPanel.transform.Find("Btn_Detail").gameObject.SetActive(true);
            }
            else
            {
                SelectedPanel.transform.Find("Btn_Detail").gameObject.SetActive(false);
            }
        }
        else
            SelectedPanel.gameObject.SetActive(false);

      
    }
    void UnselectBuilding()
    {
        m_EffectHalo.SetActive(false);
        selectBuildingPanel.gameObject.SetActive(false);
        SelectedPanel.gameObject.SetActive(false);
        m_SelectBuilding = null;
    }

    public void OpenDetailPanel()
    {
        if (m_SelectBuilding && m_SelectBuilding.Department != null)
        {
            m_SelectBuilding.Department.ShowEmpInfoPanel();
        }
        else{
            Debug.LogError("错误的调用位置");
        }
        UnselectBuilding();
    }

    //抽奖选择建筑
    public void Lottery(int count)
    {
        if (lotteryUI.Count > 0)
        {
            Debug.LogError("已经在抽建筑了");
            return;
        }
        UnselectBuilding();
        lotteryPanel.gameObject.SetActive(true);
        EnterBuildMode();

        List<BuildingType> buildingList = new List<BuildingType>();     //所有可用的抽卡列表 10+个
        List<BuildingType> tempBuildings = new List<BuildingType>();    //可选的建筑 count个
        foreach (KeyValuePair<BuildingType, Building> building in m_SelectDict)
        {
            if (building.Value.Str_Type != "装饰设施") 
            {
                buildingList.Add(building.Key);
            }
        }

        //buildingList.Add(BuildingType.空); buildingList.Add(BuildingType.空); buildingList.Add(BuildingType.空); buildingList.Add(BuildingType.空); buildingList.Add(BuildingType.空);
        for (int i = 0; i < count; i++)
        {
            BuildingType temp = buildingList[UnityEngine.Random.Range(0, buildingList.Count)];
            tempBuildings.Add(temp);
            buildingList.Remove(temp);
            if (temp == BuildingType.空)
            {
                buildingList.Remove(BuildingType.空); buildingList.Remove(BuildingType.空); buildingList.Remove(BuildingType.空);  buildingList.Remove(BuildingType.空); buildingList.Remove(BuildingType.空);
            } 
        }
        foreach (BuildingType type in tempBuildings)
        {
            Transform panel = GameObject.Instantiate(lotteryBuilding, lotteryList).transform;
            lotteryUI.Add(panel);
            LotteryBuilding lottery = panel.GetComponent<LotteryBuilding>();
            Action action = () =>
            {
                //装饰设施
                if (type == BuildingType.空)
                {
                    List<Building> decorate = new List<Building>();
                    foreach (KeyValuePair<BuildingType, Building> building in m_SelectDict) 
                    {
                        if (building.Value.Str_Type == "装饰设施")
                        {
                            decorate.Add(building.Value);
                        }
                    }
                    StartBuildNew(decorate[UnityEngine.Random.Range(0, decorate.Count)].Type);
                }
                //普通建筑
                else
                {
                    StartBuildNew(type);
                }

                lotteryPanel.gameObject.SetActive(false);
                warePanel.gameObject.SetActive(true);
                foreach (Transform ui in lotteryUI)
                {
                    Destroy(ui.gameObject);
                }
                lotteryUI.Clear();
            };

            lottery.Init(description, type, action);
        }
    }
    //放弃这次抽奖
    public void GiveUpLottery()
    {
        TryQuitBuildMode();
        lotteryPanel.gameObject.SetActive(false);
        foreach (Transform ui in lotteryUI)
        {
            Destroy(ui.gameObject);
        }
        lotteryUI.Clear();
    }

    //建造基础建筑按钮
    public void BuildBasicBuilding(BuildingType type)
    {
        StartBuildNew(type);
    }


    //进入建造模式
    public void EnterBuildMode()
    {
        if (m_SelectBuilding)
        {
            selectBuildingPanel.gameObject.SetActive(true);
            selectBuildingPanel.Find("Text").GetComponent<Text>().text = m_SelectBuilding.Type.ToString();

            description.gameObject.SetActive(false);
        }
        m_InBuildingMode = true;
        AskPause();
    }
    //(尝试)退出建造模式
    public void TryQuitBuildMode()
    {
        //没有将全部建筑摆放完毕
        if (temp_Building || lotteryPanel.gameObject.activeSelf || warePanels.Count > 0) //  ||仓库不为空
        {
            return;
        }
        m_InBuildingMode = false;
        UnselectBuilding();
        warePanel.gameObject.SetActive(false);
        selectBuildingPanel.gameObject.SetActive(false);
        SelectedPanel.gameObject.SetActive(false);
        RemovePause();
    }

    //开始建造（按建筑类型创造新建筑）
    void StartBuildNew(BuildingType type)
    {
        if (temp_Building)
            BuildCancel();
        //Init
        GameObject buildingGo = Instantiate(m_AllBuildingDict[type]);
        temp_Building = buildingGo.GetComponent<Building>();

        //确定名称
        int DepNum = 1;
        for (int i = 0; i < ConstructedBuildings.Count; i++)
        {
            if (ConstructedBuildings[i].Type == type)
                DepNum += 1;
        }
    }

    //开始建造（已生成过的建筑）
    void StartBuild(Building building)
    {
        if (temp_Building)
            BuildCancel();

        temp_Building = building;
        temp_Building.gameObject.SetActive(true);
    }

    //确定建筑
    void BuildConfirm(Building building, List<Grid> grids)
    {
        //新的建筑
        if (!building.Moving)
        {
            if (GameControl.Instance.Money < 100)
                return;

            //在链表上保存新建筑
            ConstructedBuildings.Add(building);
            GameControl.Instance.Money -= 100;
            //GameControl.Instance.BuildingPay += building.Pay;

            BuildingType T = building.Type;
            //生产部门创建
            if (T == BuildingType.技术部门 || T == BuildingType.市场部门 || T == BuildingType.产品部门 || T == BuildingType.公关营销部
                || T == BuildingType.研发部门 || T == BuildingType.智库小组 || T == BuildingType.人力资源部
                || T == BuildingType.心理咨询室 || T == BuildingType.财务部 || T == BuildingType.体能研究室
                || T == BuildingType.按摩房 || T == BuildingType.健身房 || T == BuildingType.宣传中心 || T == BuildingType.科技工作坊
                || T == BuildingType.绩效考评中心 || T == BuildingType.员工休息室 || T == BuildingType.人文沙龙 || T == BuildingType.兴趣社团
                || T == BuildingType.电子科技展 || T == BuildingType.冥想室 || T == BuildingType.特别秘书处 || T == BuildingType.成人再教育所
                || T == BuildingType.职业再发展中心 || T == BuildingType.中央监控室 || T == BuildingType.谍战中心)
            {
                building.Department = GameControl.Instance.CreateDep(building);//根据Type创建对应的生产部门面板
            }
            //办公室创建
            else if (T == BuildingType.高管办公室)
            {
                building.Office = GameControl.Instance.CreateOffice(building);
                building.effectValue = 10;
            }
            else if (T == BuildingType.CEO办公室)
            {
                building.Office = CEOOffice;    //互相引用
                CEOOffice.building = building;  //互相引用
                building.effectValue = 10;
            }
        }

        //确定建筑已摆放完毕
        building.Build(grids);
        foreach (Building temp in ConstructedBuildings)
        {
            temp.effect.FindAffect();
        }

        //获取建筑相互影响情况
        //building.effect.GetEffectBuilding();

        ////对自身周围建筑造成影响
        //building.effect.Affect();

        ////周围建筑对自身造成影响 
        //for (int i = 0; i < building.effect.AffectedBuildings.Count; i++)
        //{
        //    building.effect.AffectedBuildings[i].effect.Affect();
        //}
    }

    public void MoveBuilding()
    {
        if (temp_Building)
            BuildCancel();
        m_SelectBuilding.Move();
        temp_Building = m_SelectBuilding;
        selectBuildingPanel.gameObject.SetActive(false);
        SelectedPanel.gameObject.SetActive(false);
        UnselectBuilding();
    }

    //拆除建筑
    public void DismantleBuilding(Building building)
    {
        if (building.Dismantle())
        {
            if (building.BuildingSet)
            {
                //GameControl.Instance.BuildingPay -= building.Pay;
            }
        }
    }
    public void DismantleBuilding()
    {
        if (m_SelectBuilding.Dismantle())
        {
            if (m_SelectBuilding.BuildingSet)
            {
                //GameControl.Instance.BuildingPay -= m_SelectBuilding.Pay;
            }
        }
    }

    //取消摆放
    public void BuildCancel()
    {
        //放到仓库里
        Transform panel = GameObject.Instantiate(wareBuilding, wareBuildingsPanel).transform;
        warePanels.Add(panel);
        Building building = temp_Building;
        panel.name = temp_Building.Type.ToString();
        panel.GetComponentInChildren<Text>().text = temp_Building.Type.ToString();
        panel.GetComponent<Button>().onClick.AddListener(() =>
        {
            StartBuild(building);
            warePanels.Remove(panel);
            Destroy(panel.gameObject);
        });
        //temp设为空
        temp_Building.gameObject.SetActive(false);
        temp_Building = null;
    }

    //按钮方法，打开询问解锁新区域的面板
    public void AskUnlockArea()
    {
        //弹出面板询问是否要继续，继续需要花2000块
        unlockGridPanel.gameObject.SetActive(true);

        Button btn_Yes = unlockGridPanel.Find("Btn_Yes").GetComponent<Button>();
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

    //int pauseCount = 0;
    private void AskPause()
    {
        GameControl.Instance.AskPause(this);
        //pauseCount++;
    }

    private void RemovePause()
    {
        GameControl.Instance.RemovePause(this);
        //pauseCount--;
        //if (pauseCount == 0)
        //{
            
        //}
    }
}
