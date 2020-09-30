using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// 下班信号
/// </summary>
[TaskCategory("Employee")]
public class BTE_OffWorkFlag : BTE_Condition
{
    public override TaskStatus OnUpdate()
    {
        return ThisEmp.OffWork ? TaskStatus.Success : TaskStatus.Failure;
    }
}
