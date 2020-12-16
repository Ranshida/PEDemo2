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
    public SharedVector3 Destination;
    public SharedFloat StopDistance;
    public SharedBool Movable;

    public override TaskStatus OnUpdate()
    {
        if (ThisEntity.EventStage > 0)
        {
            return TaskStatus.Success;
        }

        Movable.Value = true;
        StopDistance.Value = 0.5f;
        Destination.Value = FindWorkPosition();
        return TaskStatus.Running;
    }
}
