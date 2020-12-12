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

    protected override void AfterOnStart()
    {
        MoveFlag.Value = false;
    }

    public override TaskStatus OnUpdate()
    {
        //为空则返回了
        if (ThisEntity.CurrentEvent == null) 
            return TaskStatus.Failure;

        //没有当前事件，返回工作
        return TaskStatus.Running;
    }
}
