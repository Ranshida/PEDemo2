using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BuildingType
{
    空, 自动化研究中心, 企业历史展览, 福报宣传中心, 混沌创意营, 会计办公室, 心理咨询室, 智库小组, 仓库, 原型图画室, 算法小组 ,自动化工坊
}

public class BuildingManage : MonoBehaviour
{
    public static BuildingManage Instance;
    public Transform ExitPos;
    public List<WindowRoot> childWindows;
    public BuildingWindow BuildingWindow;                //仓库建造
    public LotteryWindow LotteryWindow;                  //抽建筑
    public SelectBuildingWindow SelectBuildingWindow;    //选中建筑
    public UnlockAreaWindow UnlockAreaWindow;    //选中建筑
    public BuildingDescription Description;
    //public 

    public Building SubDepPrefab;//自动化工坊临时预制体引用
    //初始化
    public GameObject[] buildingPrefabs;  //建筑预制体
    public Dictionary<BuildingType, GameObject> m_AllBuildingPrefab;   //  <建筑类型，预制体> 的字典
    public Dictionary<BuildingType, Building> m_AllBuildings;   //  <建筑类型，预制体> 的字典
    public List<BuildingType> SelectList;        //可以抽取的建筑列表
    public string[,] BuildingExcelValue;
    public bool InBuildMode;

    private int m_GridX;
    private int m_GridZ;
    private bool m_CanBuild;
    private List<Grid> temp_Grids;
    public Building Temp_Building { get; private set; }    //准备建造的建筑
    public Building SelectBuilding { get; private set; }    //选中的建筑
    private GameObject m_EffectHalo;   //选中的建筑的影响范围

    public List<Building> ConstructedBuildings = new List<Building>(); //所有已建建筑列表

    //屏幕射线位置
    public Vector3 AimingPosition = Vector3.zero;

    private void Awake()
    {
        Instance = this;
        InBuildMode = false;
        temp_Grids = new List<Grid>();
        m_AllBuildingPrefab = new Dictionary<BuildingType, GameObject>();
        m_AllBuildings = new Dictionary<BuildingType, Building>();
        SelectList = new List<BuildingType>();

        //加载建筑预制体，加入建筑字典
        buildingPrefabs = ResourcesLoader.LoadAll<GameObject>("Prefabs/Scene/Buildings");
        m_EffectHalo = Instantiate(ResourcesLoader.LoadPrefab("Prefabs/Scene/EffectHalo"), transform);
        BuildingExcelValue = ExcelTool.ReadAsString(Application.streamingAssetsPath + "/Excel/BuildingFunction.xlsx");

        foreach (GameObject prefab in buildingPrefabs)
        {
            Building building = prefab.GetComponent<Building>();
            building.LoadPrefab(BuildingExcelValue);
            if (building.Type != BuildingType.空)
            {
                m_AllBuildingPrefab.Add(building.Type, prefab);
                m_AllBuildings.Add(building.Type, building);
            }
        }
        foreach (GameObject prefab in buildingPrefabs)
        {
            Building building = prefab.GetComponent<Building>();
            SelectList.Add(building.Type);
        }
        //装饰建筑临时这样做
        //SelectList.Add(BuildingType.空); SelectList.Add(BuildingType.空); SelectList.Add(BuildingType.空); SelectList.Add(BuildingType.空); SelectList.Add(BuildingType.空);

        childWindows.Add(BuildingWindow);
        childWindows.Add(LotteryWindow);
        childWindows.Add(SelectBuildingWindow);
        childWindows.Add(Description);
    }

    private void Start()
    {
        m_EffectHalo.SetActive(false);
        InitBuilding(BuildingType.原型图画室, new Int2(1, 1));
        //InitBuilding(BuildingType.自动化研究中心, new Int2(10, 11));
    }

    //直接放置一个建筑
    public void InitBuilding(BuildingType type, Int2 leftDownGird)
    {
        StartBuildNew(type);
        Grid grid;
        Dictionary<int, Grid> gridDict;
        for (int i = 0; i < Temp_Building.Length; i++)
        {
            //if (GridContainer.Instance.GridDict.TryGetValue(CEOPositionX + i, out gridDict))
            if (GridContainer.Instance.GridDict.TryGetValue(leftDownGird.X + i, out gridDict))
            {
                for (int j = 0; j < Temp_Building.Width; j++)
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
        if (temp_Grids.Count == Temp_Building.Width * Temp_Building.Length)
        {
            Temp_Building.transform.position = new Vector3(leftDownGird.X * 10, 0, leftDownGird.Y * 10);
            BuildConfirm(Temp_Building, temp_Grids);
        }
        else
        {
            Debug.LogError("无法建造，检查坐标");
        }
        //if (type == BuildingType.原型图画室)
        //{
        //    Temp_Building.CanDismantle = false;
        //    Temp_Building.AttachedArea = Temp_Building.ContainsGrids[0].BelongedArea;
        //}
        temp_Grids.Clear();
        Temp_Building = null;
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
        if (CameraController.BuildingHit && !UISvc.IsPointingUI)
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

        //建造模式下
        if (InBuildMode)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                //确定建造当前建筑
                if (Temp_Building && m_CanBuild)
                {
                    //(目前)附加建筑必须和父建筑在一个事业部内
                    if (Temp_Building.MasterBuilding != null && temp_Grids[0].BelongedArea != Temp_Building.MasterBuilding.CurrentArea)
                    {
                        GameControl.Instance.CreateMessage("与父建筑不在同一事业部");
                        return;
                    }
                    //else if (Temp_Building.AttachedArea != null && temp_Grids[0].BelongedArea != Temp_Building.AttachedArea)
                    //{
                    //    GameControl.Instance.CreateMessage("商战部门无法移动到其他事业部");
                    //    return;
                    //}
                    BuildConfirm(Temp_Building, temp_Grids);
                    Temp_Building = null;
                }
                //选中建筑
                else if (GridContainer.Instance.GridDict.TryGetValue(m_GridX, out Dictionary<int, Grid> dict))
                {
                    if (dict.TryGetValue(m_GridZ, out Grid grid))
                    {
                        if (grid.Type == Grid.GridType.已放置)
                        {
                            SelectABuilding(GridContainer.Instance.GridDict[m_GridX][m_GridZ].BelongBuilding);
                        }
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                //取消建造当前建筑
                if (Temp_Building)
                    BuildCancel();
                //取消选中
                if (SelectBuilding)
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
            if (Temp_Building)
            {
                //检查覆盖到的网格
                Grid grid;
                Dictionary<int, Grid> gridDict;
                for (int i = 0; i < Temp_Building.Length; i++)
                {
                    if (GridContainer.Instance.GridDict.TryGetValue(m_GridX + i, out gridDict))
                    {
                        for (int j = 0; j < Temp_Building.Width; j++)
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
                if (temp_Grids.Count == Temp_Building.Width * Temp_Building.Length)
                {
                    m_CanBuild = true;
                    foreach (Grid tempGrid in temp_Grids)
                    {
                        tempGrid.IsPutting = true;
                        tempGrid.RefreshGrid();
                    }
                    Temp_Building.transform.position = new Vector3(m_GridX * 10, 0, m_GridZ * 10);
                    return;
                }
                //不能覆盖全部网格 => 不能建造
                else
                    Temp_Building.transform.position = AimingPosition + new Vector3(-5, 0, -5);

            
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
                            SelectABuilding(GridContainer.Instance.GridDict[m_GridX][m_GridZ].BelongBuilding);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                //取消选中
                if (SelectBuilding)
                    UnselectBuilding();
            }
        }

        //建筑的辐射范围光环
        if (SelectBuilding)
        {
            m_EffectHalo.transform.position = SelectBuilding.transform.position + new Vector3(SelectBuilding.Length * 5, 0.2f, SelectBuilding.Width * 5);
        }
    }

    void SelectABuilding(Building building)
    {
        SelectBuilding = building;
        m_EffectHalo.SetActive(true);

        SelectBuildingWindow.SetWndState(true);
    }
    void UnselectBuilding()
    {
        m_EffectHalo.SetActive(false);

        SelectBuilding = null;
        SelectBuildingWindow.SetWndState(false);
    }

    public void OpenDetailPanel()
    {
        if (SelectBuilding && SelectBuilding.Department != null)
        {
            SelectBuilding.Department.ShowDivisionPanel();
        }
        else
        {
            Debug.LogError("错误的调用位置");
        }
        UnselectBuilding();
    }

    //抽奖选择建筑
    public bool Lotterying { get; private set; }
    public void Lottery(int count)
    {
        if (Lotterying)
        {
            Debug.LogError("已经在抽建筑了");
            return;
        }

        Lotterying = true;
        UnselectBuilding();
        LotteryWindow.SetWndState();

        List<BuildingType> buildingList = new List<BuildingType>();     //临时存储所有可用的抽卡列表
        List<BuildingType> tempBuildings = new List<BuildingType>();    //可选的建筑 count个
        foreach (BuildingType type in SelectList)
        {
            buildingList.Add(type);
        }
        for (int i = 0; i < count; i++)
        {
            BuildingType temp = buildingList[UnityEngine.Random.Range(0, buildingList.Count)];
            tempBuildings.Add(temp);
            buildingList.Remove(temp);
            if (temp == BuildingType.空)
            {
                buildingList.Remove(BuildingType.空); buildingList.Remove(BuildingType.空); buildingList.Remove(BuildingType.空); buildingList.Remove(BuildingType.空); buildingList.Remove(BuildingType.空);
            } 
        }

        LotteryWindow.Lottery(tempBuildings);
    }

    public void FinishLottery()
    {
        Lotterying = false;
        LotteryWindow.SetWndState(false);
    }

    //建造基础建筑按钮
    public void BuildBasicBuilding(BuildingType type)
    {
        StartBuildNew(type);
    }


    //进入建造模式
    public void EnterBuildMode()
    {
        BuildingWindow.SetWndState();
        InBuildMode = true;
        foreach (WindowRoot item in childWindows)
        {
            item.SendMessage("OnEnterBuildMode", SendMessageOptions.DontRequireReceiver);
        }
        AskPause();
    }
    //(尝试)退出建造模式
    public bool TryQuitBuildMode()
    {
        if (Lotterying)
        {
            return false;
        }
        //没有将全部建筑摆放完毕
        if (Temp_Building) //  ||仓库不为空
        {
            return false;
        }
        InBuildMode = false;
        BuildingWindow.SetWndState(false);
        foreach (WindowRoot item in childWindows)
        {
            item.SendMessage("OnQuitBuildMode", SendMessageOptions.DontRequireReceiver);
        }
        UnselectBuilding();
        RemovePause();
        return true;
    }

    //开始建造（按建筑类型创造新建筑）
    public void StartBuildNew(BuildingType type)
    {
        if (Temp_Building)
            BuildCancel();
        //Init
        GameObject buildingGo = Instantiate(m_AllBuildingPrefab[type]);
        Temp_Building = buildingGo.GetComponent<Building>();
    }

    //开始建造（已生成过的建筑）
    public void StartBuild(Building building)
    {
        if (Temp_Building)
            BuildCancel();

        Temp_Building = building;
        Temp_Building.gameObject.SetActive(true);
    }

    //确定建筑
    void BuildConfirm(Building building, List<Grid> grids)
    {
        //新的建筑
        if (!building.Moving)
        {
            //在链表上保存新建筑
            ConstructedBuildings.Add(building);
            //部门创建
            //附加部门不管这个
            if (building.Type != BuildingType.自动化工坊)
                building.Department = GameControl.Instance.CreateDep(building);
        }


        //确定建筑已摆放完毕
        building.Build(grids);
        //此处是旧的确认影响范围内建筑的函数
        //foreach (Building temp in ConstructedBuildings)
        //{
        //    temp.effect.FindAffect();
        //}
        building.CurrentArea = building.ContainsGrids[0].BelongedArea;
        //设置事业部
        if (building.Department)
            building.Department.SetDivision(building.CurrentArea.DC);

    }

    public void MoveBuilding()
    {
        if (Temp_Building)
            BuildCancel();
        if (SelectBuilding.Department)
        {
            if (SelectBuilding.Department.CurrentEmps.Count > 0)
            {
                QuestControl.Instance.Init("部门内还有员工");
                return;
            }
        }
        SelectBuilding.Move();
        Temp_Building = SelectBuilding;
       
        UnselectBuilding();
    }

    //拆除建筑
    public void DismantleBuilding(Building building)
    {
        if (ConstructedBuildings.Count == 1)
        {
            System.Action DismantleAction = () => { building.Dismantle(); };
            System.Action cancelAction = () => { };
            QuestControl.Instance.Init("拆了最后一个建筑会直接失败", DismantleAction, cancelAction);
        }
        else
            building.Dismantle();
        //if (building.Dismantle())
        //{
        //    if (building.BuildingSet)
        //    {
        //        //GameControl.Instance.BuildingPay -= building.Pay;
        //    }
        //}
    }
    public void DismantleBuilding()
    {
        System.Action DismantleAction = () => { SelectBuilding.Dismantle(); };
        System.Action cancelAction = () => {};
        QuestControl.Instance.Init("拆了最后一个建筑会直接失败", DismantleAction, cancelAction);
        //if (SelectBuilding.Dismantle())
        //{
        //    if (SelectBuilding.BuildingSet)
        //    {
        //        //GameControl.Instance.BuildingPay -= m_SelectBuilding.Pay;
        //    }
        //}
    }

    public void AskUnlockArea()
    {
        //有未处理事件时不能继续
        if (GameControl.Instance.EC.UnfinishedEvents.Count > 0)
            return;
        UnlockAreaWindow.SetWndState();
    }

    //确定解锁新区域
    public Button btn_AskUnlock;
    private int m_Area = 0;
    public void UnLockArea()
    {
        if (GameControl.Instance.Money < 1000)
        {
            Debug.Log("金钱不够");
            return;
        }
        GameControl.Instance.Money -= 1000;
        GridContainer.Instance.UnlockGrids(m_Area);
        m_Area++;
        if (m_Area >= 5)
        {
            btn_AskUnlock.gameObject.SetActive(false);
        }
        UnlockAreaWindow.SetWndState(false);
    }

    //取消摆放
    public void BuildCancel()
    {
        BuildingWindow.PutIntoWare(Temp_Building);
    
        //temp设为空
        Temp_Building.gameObject.SetActive(false);
        Temp_Building = null;
    }



    //int pauseCount = 0;
    private void AskPause()
    {
        return;
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

    public void AddInfoListener(Transform UITrans, BuildingType type)
    {
        Building building = null;
        if (m_AllBuildingPrefab.TryGetValue(type, out GameObject go))
            building = go.GetComponent<Building>();

        UIManager.Instance.OnPointerEnter(UITrans.gameObject, () =>
        {
            if (building)
                ShowBuildingInfo(building, UITrans.localPosition.x < 0);
            else
                ShowDecorateInfo(UITrans.localPosition.x < 0);
        });

        UIManager.Instance.OnPointerExit(UITrans.gameObject, () => { Description.SetWndState(false); });
    }

    public void ShowBuildingInfo(Building building, bool right)
    {
        Description.SetWndState(true);
        Description.ShowInfo(building, right);
    }

    public void ShowDecorateInfo(bool right)
    {
        Description.SetWndState(true);
        Description.ShowInfo_Decorate(right);
    }
}
