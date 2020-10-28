using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum BuildingType
{
    技术部门, 市场部门, 产品部门, 运营部门, 研发部门, 创业车库, 高管办公室, CEO办公室, 会议室,
    人力资源部A, 人力资源部B,中央监控室, 心理咨询室,
    体能研究室, 按摩房, 健身房,
    战略咨询部A, pr部门,
    目标修正小组, 档案管理室, 效能研究室, 财务部, 战略咨询部B, 精确标准委员会,
    高级财务部A, 高级财务部B, 
}

public class BuildingManage : MonoBehaviour
{
    public static BuildingManage Instance;
    public Transform ExitPos;

    //初始化
    private GameObject lotteryBuilding;    //抽卡的UI按钮图标
    private GameObject wareBuilding;      //仓库中的临时建筑
    private GameObject[] buildingPrefabs;  //建筑预制体
    private Dictionary<BuildingType, GameObject> m_AllBuildingDict;   //<建筑类型，预制体> 的字典
    private Dictionary<BuildingType, GameObject> m_SelectDict;   //<建筑类型，预制体> 的字典

    //准备建造的建筑
    private bool m_CanBuild;
    private List<Grid> temp_Grids;
    private Building temp_Building; 
    private GameObject CEOBuilding;

    //建造面板
    public Button btn_FinishBuild;
    public Button btn_EnterBuildMode;
    public Transform lotteryPanel;   //抽卡面板
    public List<Transform> lotteryUI;   //抽卡面板
    public List<Transform> warePanels;  //仓库中存储的待建建筑
    public List<Building> wareBuildings;  //仓库中存储的待建建筑
    public Transform warePanel;     //仓库面板
    public Transform wareBuildingsPanel;     //仓库面板
    public Transform selectBuildingPanel;     //仓库面板
    private bool m_InBuildingMode;          //建造中

    //默认建筑（CEO办公室）
    private List<Building> ConstructedBuildings = new List<Building>();
    public int CEOPositionX;
    public int CEOPositionZ;
    public OfficeControl CEOOffice;

    //选中的建筑
    private Building m_SelectBuilding;
    private GameObject m_EffectHalo;

    //屏幕射线位置
    public static Vector3 AimingPosition { get; private set; } = Vector3.zero; 

    private void Awake()
    {
        Instance = this;
        m_InBuildingMode = false;
        lotteryUI = new List<Transform>();
        warePanels = new List<Transform>();
        wareBuildings = new List<Building>();
        temp_Grids = new List<Grid>();
        m_AllBuildingDict = new Dictionary<BuildingType, GameObject>();
        m_SelectDict = new Dictionary<BuildingType, GameObject>();
       
        //加载建筑预制体，加入建筑字典
        lotteryBuilding = ResourcesLoader.LoadPrefab("Prefabs/UI/Building/LotteryBuilding");
        wareBuilding = ResourcesLoader.LoadPrefab("Prefabs/UI/Building/WareBuilding");
        buildingPrefabs = ResourcesLoader.LoadAll<GameObject>("Prefabs/Scene/Buildings");
        CEOBuilding = ResourcesLoader.LoadPrefab("Prefabs/Scene/Buildings/CEO办公室");
        m_EffectHalo = Instantiate(ResourcesLoader.LoadPrefab("Prefabs/Scene/EffectHalo"), transform);
        foreach (GameObject prefab in buildingPrefabs)
        {
            Building building = prefab.GetComponent<Building>();
            m_AllBuildingDict.Add(building.Type, prefab);
        }
        foreach (GameObject prefab in buildingPrefabs)
        {
            Building building = prefab.GetComponent<Building>();
            if (building.Type == BuildingType.技术部门 || building.Type == BuildingType.产品部门 || building.Type == BuildingType.市场部门 || building.Type == BuildingType.目标修正小组 || 
                building.Type == BuildingType.高级财务部A || building.Type == BuildingType.高级财务部B || building.Type == BuildingType.精确标准委员会 || building.Type == BuildingType.效能研究室 || 
                building.Type == BuildingType.高管办公室 || building.Type == BuildingType.人力资源部A || building.Type == BuildingType.人力资源部B || 
                building.Type == BuildingType.财务部 || building.Type == BuildingType.档案管理室 || building.Type == BuildingType.按摩房 || building.Type == BuildingType.健身房)
            {
                m_SelectDict.Add(building.Type, prefab);
            }
        }
    }
   
    private void Start()
    {
        lotteryPanel = transform.Find("LotteryPanel");
        warePanel = transform.Find("WarePanel");
        wareBuildingsPanel = warePanel.Find("Scroll View/Viewport/Content");
        selectBuildingPanel = transform.Find("SelectBuilding");
        btn_EnterBuildMode = transform.Find("Btn_BuildMode").GetComponent<Button>();
        btn_FinishBuild = transform.Find("WarePanel/Btn_Finish").GetComponent<Button>();

        m_EffectHalo.SetActive(false);
        InitBuilding();
    }
    //初始化默认建筑
    void InitBuilding()
    {
        GameObject buildingGo = Instantiate(CEOBuilding);
        temp_Building = buildingGo.GetComponent<Building>();
        //确定名称
        temp_Building.Text_DepName.text = "CEO办公室";

        //已经选择建筑，检测网格可否可以建造
        if (temp_Building)
        {
            //检查覆盖到的网格
            Grid grid;
            Dictionary<int, Grid> gridDict;
            for (int i = 0; i < temp_Building.Length; i++)
            {
                if (GridContainer.Instance.GridDict.TryGetValue(CEOPositionX + i, out gridDict))
                {
                    for (int j = 0; j < temp_Building.Width; j++)
                    {
                        if (gridDict.TryGetValue(CEOPositionZ + j, out grid))
                        {
                            if (!gridDict[CEOPositionZ + j].Lock && gridDict[CEOPositionZ + j].Type == Grid.GridType.可放置)
                            {
                                temp_Grids.Add(gridDict[CEOPositionZ + j]);
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
                temp_Building.transform.position = new Vector3(CEOPositionX * 10, 0, CEOPositionZ * 10);
            }
            //不能覆盖全部网格 => 不能建造
            else
                temp_Building.transform.position = AimingPosition + new Vector3(-5, 0, -5);
        }

        if (!m_CanBuild)
            Debug.LogError("无法建造，检查坐标");

        CEOOffice.building = temp_Building;  //互相引用
        temp_Building.Office = CEOOffice;    //互相引用
        temp_Building.effectValue = 8;

        //确定建筑已摆放完毕
        temp_Building.Build(temp_Grids);

        //获取建筑相互影响情况
        temp_Building.effect.GetEffectBuilding();

        //对自身周围建筑造成影响
        temp_Building.effect.Affect();

        //周围建筑对自身造成影响 
        for (int i = 0; i < temp_Building.effect.AffectedBuildings.Count; i++)
        {
            temp_Building.effect.AffectedBuildings[i].effect.Affect();
        }

        //在链表上保存新建筑
        ConstructedBuildings.Add(temp_Building);

        temp_Building = null;
    }

    private void Update()
    {
        if (!GridContainer.Instance)
            return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            Lottery(3);
        }


        //屏幕射线命中地面
        if (CameraController.TerrainHit && !CameraController.IsPointingUI)
            AimingPosition = new Vector3(CameraController.TerrainRaycast.point.x, 0, CameraController.TerrainRaycast.point.z);
        else
            AimingPosition = new Vector3(-1000, 0, 0);
        //鼠标所属网格的X坐标
        int tempX;
        if (AimingPosition.x > 0)
            tempX = (int)(AimingPosition.x / 10);
        else
            tempX = (int)(AimingPosition.x / 10) - 1;
        //鼠标所属网格的Z坐标
        int tempZ;
        if (AimingPosition.z > 0)
            tempZ = (int)(AimingPosition.z / 10);
        else
            tempZ = (int)(AimingPosition.z / 10) - 1;

        //建造模式下
        if (m_InBuildingMode)
        {
            if (warePanels.Count > 0 || temp_Building)
                btn_FinishBuild.interactable = false;
            else
                btn_FinishBuild.interactable = true;
            //尝试退出建造模式
            if (btn_FinishBuild.interactable && (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Escape)))
            {
                btn_FinishBuild.onClick.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                //确定建造当前建筑
                if (temp_Building && m_CanBuild)
                    BuildConfirm();
                //选中建筑
                else if (GridContainer.Instance.GridDict.TryGetValue(tempX, out Dictionary<int, Grid> dict))
                {
                    if (dict.TryGetValue(tempZ, out Grid grid))
                    {
                        if (grid.Type == Grid.GridType.已放置)
                        {
                            m_SelectBuilding = GridContainer.Instance.GridDict[tempX][tempZ].belongBuilding;
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
                    m_SelectBuilding = null;
                //拆除建筑，先不做
                //if (GridContainer.Instance.GridDict[tempX][tempZ].Type == Grid.GridType.已放置)
                //{
                //GridContainer.Instance.GridDict[tempX][tempZ].belongBuilding.Dismantle();
                //}
            }

            //刷新临时建筑网格
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
                    if (GridContainer.Instance.GridDict.TryGetValue(tempX + i, out gridDict))
                    {
                        for (int j = 0; j < temp_Building.Width; j++)
                        {
                            if (gridDict.TryGetValue(tempZ + j, out grid))
                            {
                                if (!gridDict[tempZ + j].Lock && gridDict[tempZ + j].Type == Grid.GridType.可放置)
                                {
                                    temp_Grids.Add(gridDict[tempZ + j]);
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
                    temp_Building.transform.position = new Vector3(tempX * 10, 0, tempZ * 10);
                    return;
                }
                //不能覆盖全部网格 => 不能建造
                else
                    temp_Building.transform.position = AimingPosition + new Vector3(-5, 0, -5);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                //选中
                if (GridContainer.Instance.GridDict.TryGetValue(tempX, out Dictionary<int, Grid> dict))
                {
                    if (dict.TryGetValue(tempZ, out Grid grid))
                    {
                        if (grid.Type == Grid.GridType.已放置)
                            m_SelectBuilding = GridContainer.Instance.GridDict[tempX][tempZ].belongBuilding;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                //取消选中
                if (m_SelectBuilding)
                    m_SelectBuilding = null;
            }
        }

        //建筑的辐射范围光环
        if (m_SelectBuilding)
        {
            m_EffectHalo.SetActive(true);
            m_EffectHalo.transform.position = m_SelectBuilding.transform.position + new Vector3(m_SelectBuilding.Length * 5, 0.2f, m_SelectBuilding.Width * 5);
            m_EffectHalo.transform.localScale = new Vector3(m_SelectBuilding.Length + 8, 1, m_SelectBuilding.Width + 8);
            if (!m_InBuildingMode)
                btn_EnterBuildMode.gameObject.SetActive(true);
        }
        else
        {
            m_EffectHalo.SetActive(false);
            selectBuildingPanel.gameObject.SetActive(false);
        }
    }
    
    //抽奖选择建筑
    public void Lottery(int count)
    {
        if (lotteryUI.Count > 0) 
            Debug.LogError("已经在抽建筑了");

        m_SelectBuilding = null;
        lotteryPanel.gameObject.SetActive(true);
        EnterBuildMode();

        List<Building> buildingList = new List<Building>();     //所有可用的抽卡列表 10+个
        List<Building> tempBuildings = new List<Building>();    //可选的建筑 count个
        foreach (KeyValuePair<BuildingType, GameObject> building in m_SelectDict)
        {
            buildingList.Add(building.Value.GetComponent<Building>());
        }
        for (int i = 0; i < count; i++)
        {
            Building temp = buildingList[Random.Range(0, buildingList.Count)];
            tempBuildings.Add(temp);
            buildingList.Remove(temp);
        }
        foreach (Building building in tempBuildings)
        {
            Transform panel = GameObject.Instantiate(lotteryBuilding, lotteryPanel).transform;
            lotteryUI.Add(panel);
            panel.name = building.Type.ToString();
            panel.GetComponentInChildren<Text>().text = building.Type.ToString();
            panel.GetComponent<Button>().onClick.AddListener(() =>
            {
                StartBuildNew(building.Type);
                lotteryPanel.gameObject.SetActive(false);
                warePanel.gameObject.SetActive(true);
                foreach (Transform ui in lotteryUI)
                {
                    Destroy(ui.gameObject);
                }
                lotteryUI.Clear();
            });
        }
    }

    public void MoveBuilding()
    {
        m_SelectBuilding.Move();
        temp_Building = m_SelectBuilding;
    }

    public void DismantleBuilding()
    {
        m_SelectBuilding.Dismantle();
    }

    //进入建造模式
    public void EnterBuildMode()
    {
        if (m_SelectBuilding)
        {
            selectBuildingPanel.gameObject.SetActive(true);
            selectBuildingPanel.Find("Text").GetComponent<Text>().text = m_SelectBuilding.Type.ToString();
        }
        m_InBuildingMode = true;
        GameControl.Instance.ForceTimePause = true;
    }
    //(尝试)退出建造模式
    public void TryQuitBuildMode()
    {
        //没有将全部建筑摆放完毕
        if (temp_Building || warePanels.Count > 0) //  ||仓库不为空
        {
            return;
        }
        warePanel.gameObject.SetActive(false);
        lotteryPanel.gameObject.SetActive(false);
        m_InBuildingMode = false;
        GameControl.Instance.ForceTimePause = false;
    }
    
    //开始建造（按建筑类型创造新建筑）
    public void StartBuildNew(BuildingType type)
    {
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
        temp_Building.Text_DepName.text = temp_Building.Type.ToString() + DepNum;
    }
    
    //开始建造（已生成过的建筑）
    public void StartBuild(Building building)
    {
        temp_Building = building;
        temp_Building.gameObject.SetActive(true);
    }

    //确定建筑
    public void BuildConfirm()
    {
        //金钱相关暂时保留
        if (GameControl.Instance.Money < 100)
            return;

        //新的建筑
        if (!temp_Building.Moving)
        {
            //在链表上保存新建筑
            ConstructedBuildings.Add(temp_Building);
            GameControl.Instance.Money -= 100;
            GameControl.Instance.BuildingPay += 50;
            GameControl.Instance.Money -= 100;

            BuildingType T = temp_Building.Type;
            //生产部门创建
            if (T == BuildingType.技术部门 || T == BuildingType.市场部门 || T == BuildingType.产品部门 || T == BuildingType.运营部门)
            {
                //新建部门必须要保留的
                temp_Building.Department = GameControl.Instance.CreateDep((int)T + 1);//根据Type创建对应的生产部门面板
                temp_Building.Department.building = temp_Building;//把SelectBuilding赋值给新的部门面板
            }
            else if (T == BuildingType.研发部门)
            {
                temp_Building.Department = GameControl.Instance.CreateDep(4);
                temp_Building.Department.building = temp_Building;
            }
            else if (T == BuildingType.人力资源部B)
            {
                temp_Building.Department = GameControl.Instance.CreateDep(5);
                temp_Building.Department.building = temp_Building;
            }

            //办公室创建
            else if (T == BuildingType.高管办公室 || T == BuildingType.CEO办公室)
            {
                temp_Building.Office = GameControl.Instance.CreateOffice(temp_Building);
                temp_Building.effectValue = 8;
            }
            else if (T == BuildingType.人力资源部A)
            {
                temp_Building.Office = GameControl.Instance.CreateOffice(temp_Building);
                temp_Building.effectValue = 1;
                temp_Building.StaminaRequest = 10;
            }
            else if (T == BuildingType.按摩房 || T == BuildingType.健身房)
            {
                temp_Building.Office = GameControl.Instance.CreateOffice(temp_Building);
                temp_Building.effectValue = 3;
                if (T == BuildingType.健身房)
                    temp_Building.StaminaRequest = 10;
            }
            else if (T == BuildingType.战略咨询部B || T == BuildingType.精确标准委员会)
            {
                temp_Building.Office = GameControl.Instance.CreateOffice(temp_Building);
                temp_Building.effectValue = 5;
                temp_Building.StaminaRequest = 20;
            }
            else if (T == BuildingType.目标修正小组)
            {
                temp_Building.Office = GameControl.Instance.CreateOffice(temp_Building);
                temp_Building.effectValue = 6;
                temp_Building.StaminaRequest = 20;
            }
            else if (T == BuildingType.财务部 || T == BuildingType.高级财务部A || T == BuildingType.高级财务部B)
            {
                temp_Building.Office = GameControl.Instance.CreateOffice(temp_Building);
                temp_Building.effectValue = 7;
                temp_Building.StaminaRequest = 10;
            }
            else if (T == BuildingType.档案管理室 || T == BuildingType.效能研究室)
            {
                temp_Building.Office = GameControl.Instance.CreateOffice(temp_Building);
                temp_Building.effectValue = 8;
                temp_Building.StaminaRequest = 10;
                if (T == BuildingType.效能研究室)
                    temp_Building.StaminaRequest = 20;
            }
        }

        //确定建筑已摆放完毕
        temp_Building.Build(temp_Grids);

        //获取建筑相互影响情况
        temp_Building.effect.GetEffectBuilding();

        //对自身周围建筑造成影响
        temp_Building.effect.Affect();

        //周围建筑对自身造成影响 
        for (int i = 0; i < temp_Building.effect.AffectedBuildings.Count; i++)
        {
            temp_Building.effect.AffectedBuildings[i].effect.Affect();
        }
        temp_Building = null;
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
}
