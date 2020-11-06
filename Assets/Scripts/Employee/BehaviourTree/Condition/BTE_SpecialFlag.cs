using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// 员工特殊情况
/// 更新时间2020.10.15
/// </summary>
[TaskCategory("Employee")]
public class BTE_SpecialFlag : BTE_Condition
{
    public override TaskStatus OnUpdate()
    {
        return ThisEmp.IsSpying ? TaskStatus.Success : TaskStatus.Failure;
    }
}