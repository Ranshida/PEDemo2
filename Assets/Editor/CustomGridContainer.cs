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
#pragma warning disable CS0618 // 类型或成员已过时
        script.WayPoint = EditorGUILayout.ObjectField("寻路点父物体" ,script.WayPoint, typeof(Transform)) as Transform;
#pragma warning restore CS0618 // 类型或成员已过时
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
        serializedObject.ApplyModifiedProperties();
    }

    private void CheckGrid()
    {
        foreach (Grid temp in script.GridList)
        {
            temp.RefreshGrid();
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

        GameObject gridPrefab = ResourcesLoader.LoadPrefab("Prefabs/Scene/Grid");
        script.GridList = new List<Grid>();
        
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
        grid.transform.parent = script.transform;
        grid.RefreshGrid();
    }
}
