using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 临时页面系统
/// 更新时间2020.9.8
/// </summary>
public class MlySystem : MonoBehaviour
{
    public static MlySystem Instance;

    private void Awake()
    {
        Instance = this;
    }

    public Transform buildingWindow;

    private bool canBuild;
    private List<Grid> buildGrid;
    public Vector3 AimingPosition { get; private set; } = Vector3.zero;  //Aiming屏幕射线位置
    private Ray rayAiming; //实际瞄准的位置，与任何物体碰撞
    private Building temp_Building;
    private GameObject building_2_2;
    private GameObject building_2_3;
    private GameObject building_3_2;
    private GameObject building_3_3;
    private GameObject building_2_4;
    private GameObject building_4_2;

    private void Start()
    {
        buildGrid = new List<Grid>();
        building_2_2 = ResourcesLoader.LoadPrefab("Prefabs/MaLingyu/2_2");
        building_2_3 = ResourcesLoader.LoadPrefab("Prefabs/MaLingyu/2_3");
        building_3_2 = ResourcesLoader.LoadPrefab("Prefabs/MaLingyu/3_2");
        building_3_3 = ResourcesLoader.LoadPrefab("Prefabs/MaLingyu/3_3");
        building_2_4 = ResourcesLoader.LoadPrefab("Prefabs/MaLingyu/2_4");
        building_4_2 = ResourcesLoader.LoadPrefab("Prefabs/MaLingyu/4_2");
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
                temp_Building.Build(buildGrid);
                temp_Building = null;
            }
        }
        //拆除建筑
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (GridContainer.Instance.GridDict[tempX][tempZ].Type == Grid.GridType.已放置)
            {
                GridContainer.Instance.GridDict[tempX][tempZ].belongBuilding.Dismantle();
            }
        }


        //刷新临时建筑网格
        foreach (Grid grid in buildGrid)
        {
            grid.IsPutting = false;
            grid.RefreshGrid();
        }
        buildGrid.Clear();
        canBuild = false;


        //检测可否建造建筑
        if (GridContainer.Instance && temp_Building)
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
                                buildGrid.Add(gridDict[tempZ + j]);
                            }
                        }
                    }
                }
            }

            //全部覆盖到网格 => 可以建造
            if (buildGrid.Count == temp_Building.Width * temp_Building.Length)
            {
                canBuild = true;
                foreach (Grid tempGrid in buildGrid)
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
    
    public void Build(int id)
    {
        switch (id)
        {
            case 1:
                NewTempBuilding(building_2_2);
                break;
            case 2:
                NewTempBuilding(building_2_3);
                break;
            case 3:
                NewTempBuilding(building_3_2);
                break;
            case 4:
                NewTempBuilding(building_3_3);
                break;
            case 5:
                NewTempBuilding(building_2_4);
                break;
            case 6:
                NewTempBuilding(building_4_2);
                break;
            default:
                break;
        }
    }

    public void UnlockGrids(int id)
    {
        GridContainer.Instance.UnlockGrids(id);
    }

    private void NewTempBuilding(GameObject prefab)
    {
        if (temp_Building)
            Destroy(temp_Building.gameObject);

        GameObject buildingGo = Instantiate(prefab);
        temp_Building = buildingGo.GetComponent<Building>();
    }
}
