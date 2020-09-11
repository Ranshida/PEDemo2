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
}

public class Int2
{
    public int x { get; set; }
    public int y { get; set; }

    public Vector2 Trans2Vector2()
    {
        return new Vector2(x, y);
    }
}
public class Int3
{
    public int x { get; set; }
    public int y { get; set; }
    public int z { get; set; }

    public Vector3 Trans2Vector3()
    {
        return new Vector3(x, y, z);
    }
}
public class Int4
{
    public int x { get; set; }
    public int y { get; set; }
    public int z { get; set; }
    public int w { get; set; }

    public Vector4 Trans2Vector4()
    {
        return new Vector4(x, y, z, w);
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