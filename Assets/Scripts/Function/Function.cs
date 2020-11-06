using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用方法
/// 更新时间2020.9.5
/// </summary>
public static class Function
{
    //设置物体及子物体材质
    public static void SetMaterial(Transform transform, Material material, bool childs = true)
    {
        MeshRenderer[] renderers = transform.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.material = material;
        }
    }

    //返回子物体列表
    public static List<Transform> ReturnChildList(Transform parent)
    {
        List<Transform> list = new List<Transform>();
        foreach (Transform transform in parent)
        {
            list.Add(transform);
        }
        return list;
    }
    public static List<T> ReturnChildList<T>(Transform parent) where T : MonoBehaviour
    {
        List<T> list = new List<T>();
        foreach (Transform transform in parent)
        {
            list.Add(transform.GetComponent<T>());
        }
        return list;
    }

    //世界坐标转屏幕坐标
    public static Vector3 World2ScreenPoint(Vector3 worldPos)
    {
        return Camera.main.WorldToScreenPoint(worldPos);
    }

    //返回XZ平面上的距离，无视y轴（高度）
    public static float XZDistance(Transform transA, Transform transB)
    {
        return Vector3.Distance(transA.position, new Vector3(transB.position.x, transA.position.y, transB.position.z));
    }
    public static float XZDistance(Vector3 posA, Vector3 posB)
    {
        return Vector3.Distance(posA, new Vector3(posB.x, posA.y, posB.z));
    }

    //返回PositionB相对PositionA在XZ平面上的方向，无视y轴
    public static Vector3 XZDirection(Vector3 positionA, Vector3 positionB)
    {
        return positionB - new Vector3(positionA.x, positionB.y, positionA.z);
    }
    public static Vector3 XZDirection(Transform transA, Transform transB)
    {
        return transB.position - new Vector3(transA.position.x, transB.position.y, transA.position.z);
    }
}

public class Int2
{
    public int X { get; set; }
    public int Y { get; set; }

    public Int2(int x,int y)
    {
        X = x;
        Y = y;
    }

    public Vector2 Trans2Vector2()
    {
        return new Vector2(X, Y);
    }
}
public class Int3
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }

    public Int3(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Vector3 Trans2Vector3()
    {
        return new Vector3(X, Y, Z);
    }
}
public class Int4
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public int W { get; set; }

    public Int4(int x, int y, int z, int w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }
    public Vector4 Trans2Vector4()
    {
        return new Vector4(X, Y, Z, W);
    }
}
public class Float2
{
    public float x { get; set; }
    public float y { get; set; }

    public Vector2 Trans2Vector2()
    {
        return new Vector2(x, y);
    }
}
public class Float3
{
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }

    public Vector3 Trans2Vector3()
    {
        return new Vector3(x, y, z);
    }
}
public class Float4
{
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
    public float w { get; set; }

    public Vector4 Trans2Vector4()
    {
        return new Vector4(x, y, z, w);
    }
}
public class FloatValue
{
    public float Value { get; set; }
    public bool Infinity { get; set; }

    public FloatValue(bool infinity, int value = 100)
    {
        Infinity = infinity;
        Value = value;
    }

    public void Additive(int value)
    {
        Value += value;
    }
}

public class IntValue
{
    public int Value { get; set; }
    public bool Infinity { get; set; }

    public IntValue(bool infinity, int value = 100)
    {
        Infinity = infinity;
        Value = value;
    }

    public void Additive(int value)
    {
        Value += value;
    }
}

public class Vector3Value
{
    public bool HasValue { get; set; } = false;
    public Vector3 Value { get; set; }

    public Vector3Value(bool hasValue = false, Vector3 value = default)
    {
        HasValue = hasValue;
        Value = value;
    }
}