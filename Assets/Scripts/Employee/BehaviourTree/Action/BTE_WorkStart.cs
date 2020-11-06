using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// 开始工作，前往工位
/// 到达后检测事件等情况
/// 更新时间2020.10.12
/// </summary>
[TaskCategory("Employee")]
public class BTE_WorkStart : BTE_Action
{
    public SharedBool Movable;
    public SharedFloat StopDistance;
    public SharedVector3 Destination;

    
    public override TaskStatus OnUpdate()
    {
        Movable.Value = true;
        StopDistance.Value = 0.1f;
        Destination.Value = FindWorkPosition();

        if (!ThisEntity.OffWork)
        {
            return TaskStatus.Success;
        }
        return TaskStatus.Running;
    }
}
