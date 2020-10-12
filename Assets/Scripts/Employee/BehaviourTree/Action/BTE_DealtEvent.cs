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
        //处理时间，完成后return success

        //if (!ThisEmp.HasEvent)
        //{
        //    return TaskStatus.Failure;
        //}

        return TaskStatus.Running;
    }
}
