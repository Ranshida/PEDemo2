using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// 员工的特殊工作，中断自己的事件和工作
/// </summary>
[TaskCategory("Employee")]
public class BTE_SpecialWork : BTE_Action
{
    public SharedVector3 Destination;
    public SharedFloat StopDistance;
    public SharedBool Movable;

    public override TaskStatus OnUpdate()
    {
        if (!ThisEntity.IsSpying)
            return TaskStatus.Failure;

        //可以移动，精确寻找位置
        Movable.Value = true;
        StopDistance.Value = 1f;

        //设置寻路点至公司出口
        if (ThisEntity.IsSpying)
            Destination.Value = BuildingManage.Instance.ExitPos.position;

        
        return TaskStatus.Running;
    }
}
