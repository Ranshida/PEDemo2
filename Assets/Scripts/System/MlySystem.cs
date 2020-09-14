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

    private bool canBuild;
    private List<Grid> selectGrids;
    private Vector3 AimingPosition = Vector3.zero;  //Aiming屏幕射线位置
    private Ray rayAiming; //实际瞄准的位置，与任何物体碰撞
    private Building temp_Building;

    private GameObject[] buildingPrefabs;  //建筑预制体
    private Dictionary<BuildingType, GameObject> buildingDict;   //<建筑类型，预制体> 的字典

    private void Start()
    {
        selectGrids = new List<Grid>();
        buildingDict = new Dictionary<BuildingType, GameObject>();
        //加载建筑预制体，加入建筑字典
        buildingPrefabs = ResourcesLoader.LoadAll<GameObject>("Prefabs/MaLingyu/Buildings");
        foreach (GameObject prefab in buildingPrefabs)
        {
            Building building = prefab.GetComponent<Building>();
            buildingDict.Add(building.Type, prefab);
        }
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
                temp_Building.Build(selectGrids);
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
        foreach (Grid grid in selectGrids)
        {
            grid.IsPutting = false;
            grid.RefreshGrid();
        }
        selectGrids.Clear();
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
    
  

    //button
    public void UnlockGrids(int id)
    {
        GridContainer.Instance.UnlockGrids(id);
    }
}
