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
        if (ThisEmp.Available)
            return TaskStatus.Failure;

        MoveFlag.Value = false;
        if (!ThisEmp.CurrentEvent.isSolving)
            ThisEmp.DealtEvent();

        //没有当前事件，返回工作
        return TaskStatus.Running;
    }
}
