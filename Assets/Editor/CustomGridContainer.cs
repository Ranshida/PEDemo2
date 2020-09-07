using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridContainer))]
public class CustomGridContainer : Editor
{
    GridContainer script;
    public override void OnInspectorGUI()
    {
        script = (GridContainer)target;
        script.xInput = EditorGUILayout.IntField("X轴坐标：", script.xInput);
        script.zInput = EditorGUILayout.IntField("Z轴坐标：", script.zInput);
        if (GUILayout.Button("检测当前网格"))
        {
            CheckGrid();
        }
        if (GUILayout.Button("生成新网格（销毁当前所有）"))
        {
            NewGrid();
        }
    }

    private void CheckGrid()
    {
        foreach (Grid temp in script.GridList)
        {
            RefreshGrid(temp);
        }
    }

    //生成新网格
    private void NewGrid()
    {
        List<Transform> tempList = new List<Transform>();
        foreach (Transform child in script.transform)
            tempList.Add(child);
        foreach (Transform temp in tempList)
            DestroyImmediate(temp.gameObject);

        GameObject gridPrefab = ResourcesLoader.LoadPrefab("Prefabs/MaLingyu/Grid");
        script.GridList = new List<Grid>();
        script.InitInEditor(script.xInput, script.zInput);
        
        for (int i = 0; i < script.zInput; i++)
        {
            for (int j = 0; j < script.xInput; j++)
            {
                GameObject gridGo = Instantiate(gridPrefab, new Vector3(j * 10 + 5, 0, i * 10 + 5), Quaternion.identity);
                Grid grid = gridGo.GetComponent<Grid>();
                InitGrid(grid, j, i);
            }
        }
    }

    //初始化格子
    private void InitGrid(Grid grid, int x, int z)
    {
        if (!script.GridList.Contains(grid))
            script.GridList.Add(grid);
    
        grid.X = x;
        grid.Z = z;
        grid.transform.position = new Vector3(x * 10 + 5, 0, z * 10 + 5);
        grid.transform.name = "Grid(" + x + "," + z + ")";
        grid.transform.parent = script.transform;
        Function.SetMaterial(grid.transform, ResourcesLoader.LoadMaterial("Material/MaLingyu/TransWhite"));
        switch (grid.Type)
        {
            case Grid.GridType.障碍:
                Function.SetMaterial(grid.transform, ResourcesLoader.LoadMaterial("Material/MaLingyu/TransBlack"));
                break;
            case Grid.GridType.道路:
                Function.SetMaterial(grid.transform, ResourcesLoader.LoadMaterial("Material/MaLingyu/TransGray"));
                break;
            case Grid.GridType.可放置:
                Function.SetMaterial(grid.transform, ResourcesLoader.LoadMaterial("Material/MaLingyu/TransGreen"));
                break;
            case Grid.GridType.已放置:
                Function.SetMaterial(grid.transform, ResourcesLoader.LoadMaterial("Material/MaLingyu/TransRed"));
                break;
            default:
                break;
        }
    }

    private void RefreshGrid(Grid grid)
    {
        grid.transform.position = new Vector3(grid.X * 10 + 5, 0, grid.Z * 10 + 5);
        grid.transform.name = "Grid(" + grid.X + "," + grid.Z + ")";
        grid.transform.parent = script.transform;
        Transform child = grid.transform.Find("GridDebug");
        switch (grid.Type)
        {
            case Grid.GridType.障碍:
                Function.SetMaterial(child, ResourcesLoader.LoadMaterial("Material/MaLingyu/TransBlack"));
                break;
            case Grid.GridType.道路:
                Function.SetMaterial(child, ResourcesLoader.LoadMaterial("Material/MaLingyu/TransGray"));
                break;
            case Grid.GridType.可放置:
                Function.SetMaterial(child, ResourcesLoader.LoadMaterial("Material/MaLingyu/TransGreen"));
                break;
            case Grid.GridType.已放置:
                Function.SetMaterial(child, ResourcesLoader.LoadMaterial("Material/MaLingyu/TransRed"));
                break;
            default:
                break;
        }
    }
}
