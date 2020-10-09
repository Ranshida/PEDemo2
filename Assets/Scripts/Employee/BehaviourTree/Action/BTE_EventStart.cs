using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// 开始执行事件
/// </summary>
[TaskCategory("Employee")]
public class BTE_EventStart : BTE_Action
{
    public SharedBool MoveFlag;

    protected override void AfterOnStart()
    {
        //设置寻路点至公司出口
        //解除当前的行为
    }
    public override TaskStatus OnUpdate()
    {
        if (!ThisEmp.HasEvent)
        {
            return TaskStatus.Failure;
        }

        if (ThisEmp.SelfEvent.HaveTarget)
        {
            MoveFlag.Value = true;
            ThisEmp.Destination = ThisEmp.SelfEvent.Target.transform.position;
        }
    
        return TaskStatus.Running;
    }
}
