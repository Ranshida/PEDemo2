using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// 等待事件执行
/// 更新时间2020.10.12
/// </summary>
public class BTE_WaitEvent : BTE_Action
{
    public SharedBool Movable;
    public SharedVector3 Destination;
    public SharedFloat StopDistance;
    
    public override TaskStatus OnUpdate()
    {
        //没有事件了?
        if (ThisEntity.CurrentEvent == null)
            return TaskStatus.Success;

        return TaskStatus.Running;
    }
}
