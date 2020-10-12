using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// 存在事件类，等待处理
/// </summary>
[TaskCategory("Employee")]
public class BTE_HasEvent : BTE_Condition
{
    public override TaskStatus OnUpdate()
    {
        return !ThisEmp.Available ? TaskStatus.Success : TaskStatus.Failure;
    }
}
