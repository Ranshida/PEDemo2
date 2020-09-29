using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// 员工正常工作
/// </summary>
[TaskCategory("Employee")]
public class BTE_OnWork : BTE_Action
{
    protected override void AfterOnStart()
    {
        //设置寻路点至工作建筑

        //如果没有指定岗位则闲逛
    }
}
