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
