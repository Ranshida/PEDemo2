using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// 处理事件中
/// </summary>
[TaskCategory("Employee")]
public class BTE_DealtEvent : BTE_Action
{
    public SharedBool MoveFlag;

    public override TaskStatus OnUpdate()
    {
        if (ThisEntity.CurrentEvent == null) 
            return TaskStatus.Failure;

        MoveFlag.Value = false;
        if (!ThisEntity.CurrentEvent.isSolving)
            ThisEntity.SolveEvent();

        //没有当前事件，返回工作
        return TaskStatus.Running;
    }
}
