using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// 移动信号
/// </summary>
public class BTE_MoveFlag : BTE_Condition
{
    public SharedBool MoveFlag;

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;

        if (MoveFlag.Value)
        {
            return TaskStatus.Success;
        }
        return TaskStatus.Running;
    }

}
