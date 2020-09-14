using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public Button ConfirmButton;
    public BuildingEffect BEPrefab;
    public Transform BuildingContent, EntityContent, ExitPos, MaxPos, MinPos;
    public GameObject ControlPanel;

    private bool canBuild;
    private List<Grid> selectGrids;
    private Vector3 AimingPosition = Vector3.zero;  //Aiming屏幕射线位置
    private Ray rayAiming; //实际瞄准的位置，与任何物体碰撞
    private Building temp_Building;

    private GameObject[] buildingPrefabs;  //建筑预制体
    private Dictionary<BuildingType, GameObject> buildingDict;   //<建筑类型，预制体> 的字典
    private GameObject CEOBuilding;

    public Transform optionPanel;   //建造选项面板
    Button[] buildingButton;        //建造选项面板下的所有按钮

    //已建成简述
    public List<Building> ConstructedBuildings = new List<Building>();
    public int CEOPositionX;
    public int CEOPositionZ;
    public OfficeControl CEOOffice;

    private void Awake()
    {
        Instance = this;
    }
   
    private void Start()
    {
        selectGrids = new List<Grid>();
        buildingDict = new Dictionary<BuildingType, GameObject>();

        //注册Button
        buildingButton = optionPanel.GetComponentsInChildren<Button>();
        foreach (Button button in buildingButton)
        {
            switch (button.name)
            {
                case "Btn_技术部门":
                    button.onClick.AddListener(() => { StartBuild(BuildingType.技术部门); optionPanel.gameObject.SetActive(false); });
                break;
                case "Btn_市场部门":
                    button.onClick.AddListener(() => { StartBuild(BuildingType.市场部门); optionPanel.gameObject.SetActive(false); });
                break;
                case "Btn_产品部门":
                    button.onClick.AddListener(() => { StartBuild(BuildingType.产品部门); optionPanel.gameObject.SetActive(false); });
                break;
                case "Btn_运营部门":
                    button.onClick.AddListener(() => { StartBuild(BuildingType.运营部门); optionPanel.gameObject.SetActive(false); });
                break;
                case "Btn_研发部门":
                    button.onClick.AddListener(() => { StartBuild(BuildingType.研发部门); optionPanel.gameObject.SetActive(false); });
                break;
                case "Btn_高管办公室":
                    button.onClick.AddListener(() => { StartBuild(BuildingType.高管办公室); optionPanel.gameObject.SetActive(false); });
                break;
                case "Btn_人力资源部A":
                    button.onClick.AddListener(() => { StartBuild(BuildingType.人力资源部A); optionPanel.gameObject.SetActive(false); });
                break;
                case "Btn_人力资源部B":
                    button.onClick.AddListener(() => { StartBuild(BuildingType.人力资源部B); optionPanel.gameObject.SetActive(false); });
                break;
                case "Btn_按摩房":
                    button.onClick.AddListener(() => { StartBuild(BuildingType.按摩房); optionPanel.gameObject.SetActive(false); });
                break;
                case "Btn_健身房":
                    button.onClick.AddListener(() => { StartBuild(BuildingType.健身房); optionPanel.gameObject.SetActive(false); });
                break;
                case "Btn_目标修正小组":
                    button.onClick.AddListener(() => { StartBuild(BuildingType.目标修正小组); optionPanel.gameObject.SetActive(false); });
                break;
                case "Btn_档案管理室":
                    button.onClick.AddListener(() => { StartBuild(BuildingType.档案管理室); optionPanel.gameObject.SetActive(false); });
                break;
                case "Btn_效能研究室":
                    button.onClick.AddListener(() => { StartBuild(BuildingType.效能研究室); optionPanel.gameObject.SetActive(false); });
                break;
                case "Btn_财务部":
                    button.onClick.AddListener(() => { StartBuild(BuildingType.财务部); optionPanel.gameObject.SetActive(false); });
                break;
                case "Btn_战略咨询部B":
                    button.onClick.AddListener(() => { StartBuild(BuildingType.战略咨询部B); optionPanel.gameObject.SetActive(false); });
                break;
                case "Btn_精确标准委员会":
                    button.onClick.AddListener(() => { StartBuild(BuildingType.精确标准委员会); optionPanel.gameObject.SetActive(false); });
                break;
                case "Btn_高级财务部A":
                    button.onClick.AddListener(() => { StartBuild(BuildingType.高级财务部A); optionPanel.gameObject.SetActive(false); });
                break;
                case "Btn_高级财务部B":
                    button.onClick.AddListener(() => { StartBuild(BuildingType.高级财务部B); optionPanel.gameObject.SetActive(false); });
                break;
                default:
                    break;
            }
        }

        //加载建筑预制体，加入建筑字典
        buildingPrefabs = ResourcesLoader.LoadAll<GameObject>("Prefabs/MaLingyu/Buildings");
        CEOBuilding = ResourcesLoader.LoadPrefab("Prefabs/Malingyu/Buildings/CEO办公室");
        foreach (GameObject prefab in buildingPrefabs)
        {
            Building building = prefab.GetComponent<Building>();
            buildingDict.Add(building.Type, prefab);
        }

        InitBuilding();
    }

    private void Update()
    {
        if (!GridContainer.Instance)
            return;

        //检查鼠标位置及所属网格
        rayAiming = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(rayAiming, out RaycastHit hit, 1000))
            AimingPosition = new Vector3(hit.point.x, 0, hit.point.z);
        else
            Debug.LogError("未碰撞到地面");
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

        //确定建造
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (temp_Building && canBuild)
            {
                BuildConfirm();
            }
        }

        //拆除建筑，先不做
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (GridContainer.Instance.GridDict[tempX][tempZ].Type == Grid.GridType.已放置)
            {
                //GridContainer.Instance.GridDict[tempX][tempZ].belongBuilding.Dismantle();
            }
        }


        //刷新临时建筑网格
        foreach (Grid grid in selectGrids)
        {
            grid.IsPutting = false;
            grid.RefreshGrid();
        }
        selectGrids.Clear();
        canBuild = false;


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
                                selectGrids.Add(gridDict[tempZ + j]);
                            }
                        }
                    }
                }
            }

            //全部覆盖到网格 => 可以建造
            if (selectGrids.Count == temp_Building.Width * temp_Building.Length)
            {
                canBuild = true;
                foreach (Grid tempGrid in selectGrids)
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

    private void InitBuilding()
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
                                selectGrids.Add(gridDict[CEOPositionZ + j]);
                            }
                        }
                    }
                }
            }

            //全部覆盖到网格 => 可以建造
            if (selectGrids.Count == temp_Building.Width * temp_Building.Length)
            {
                canBuild = true;
                foreach (Grid tempGrid in selectGrids)
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

        if (!canBuild)
            Debug.LogError("无法建造，检查坐标");

        CEOOffice.building = temp_Building;  //互相引用
        temp_Building.Office = CEOOffice;    //互相引用
        temp_Building.effectValue = 8;

        //确定建筑已摆放完毕,不能再移动
        temp_Building.Build(selectGrids);

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

    //创建建筑 button
    public void StartBuild(BuildingType type)
    {
        //销毁已框选的建筑
        if (temp_Building)
            Destroy(temp_Building.gameObject);

        //Init
        GameObject buildingGo = Instantiate(buildingDict[type]);
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

    //创建建筑 button
    public void CreateOffice(int num)
    {
        ////如果之前的建筑没有成功建造就删除
        //if (SelectBuilding != null)
        //{
        //    Destroy(SelectBuilding.effect.gameObject);
        //    Destroy(SelectBuilding.gameObject);
        //}

        //ToggleEffectRange();
        //BuildingType type = (BuildingType)num;

        ////创建建筑预制体，设定Building脚本的变量
        //SelectBuilding = Instantiate(BuildingPrefab, BuildingContent);
        //SelectBuilding.Type = type; //枚举第一位是0
        ////创建EffectRange并赋值
      

        ////把控制面板放到新建筑Prefab旁边
        //ControlPanel.transform.position = SelectBuilding.transform.position;
        //ControlPanel.SetActive(true);

    

        //设置尺寸
        //if (type == BuildingType.技术部门 || type == BuildingType.市场部门 || type == BuildingType.产品部门 || type == BuildingType.运营部门)
        //    SelectBuilding.SetSize(2, 3);
        //else if (type == BuildingType.高管办公室 || type == BuildingType.人力资源部A || type == BuildingType.人力资源部B  || type == BuildingType.按摩房)
        //    SelectBuilding.SetSize(2, 2);
        //else if (type == BuildingType.研发部门)
        //    SelectBuilding.SetSize(2, 4);
        //else if (type == BuildingType.健身房)
        //    SelectBuilding.SetSize(3, 3);
        //else if (type == BuildingType.目标修正小组 || type == BuildingType.档案管理室 || type == BuildingType.效能研究室
        //    || type == BuildingType.财务部 || type == BuildingType.战略咨询部B || type == BuildingType.精确标准委员会
        //    || type == BuildingType.高级财务部A || type == BuildingType.高级财务部B)
        //    SelectBuilding.SetSize(2, 3);

    }

    //确定建筑
    public void BuildConfirm()
    {
        //金钱相关暂时保留
        if (GameControl.Instance.Money < 100)
            return;

        GameControl.Instance.Money -= 100;
        GameControl.Instance.BuildingPay += 50;
        ControlPanel.SetActive(false);

        GameControl.Instance.Money -= 100;

        ControlPanel.SetActive(false);

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

        //确定建筑已摆放完毕,不能再移动
        temp_Building.Build(selectGrids);

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

    //取消摆放
    public void BuildingCancel()
    {
        ControlPanel.SetActive(false);
        Destroy(temp_Building.gameObject);
        temp_Building = null;
    }

    //void ToggleEffectRange()
    //{
    //    //开关EffectRange图片显示
    //    for(int i = 0; i < ConstructedBuildings.Count; i++)
    //    {
    //        if (ConstructedBuildings[i].effect.image.enabled == true)
    //            ConstructedBuildings[i].effect.image.enabled = false;
    //        else
    //            ConstructedBuildings[i].effect.image.enabled = true;
    //    }
    //}

    //public void RotateBuilding()
    //{
    //    SelectBuilding.Rotate();
    //    ControlPanel.transform.position = SelectBuilding.transform.position;
    //}
}
