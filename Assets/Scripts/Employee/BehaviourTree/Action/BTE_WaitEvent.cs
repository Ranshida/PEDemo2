using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// 等待事件执行
/// 更新时间2020.10.12
/// </summary>
public class BTE_WaitEvent : BTE_Action
{
    public SharedBool Movable;
    public SharedVector3 Destination;
    public SharedFloat StopDistance;
    
    public override TaskStatus OnUpdate()
    {
        //事件开始执行
        if (ThisEntity.CurrentEvent != null && ThisEntity.CurrentEvent.isSolving)
            return TaskStatus.Success;

        //没有事件了
        if (ThisEntity.EventStage == 0)
            return TaskStatus.Failure;

        //刚刚添加了事件
        if (ThisEntity.EventStage == 1) 
        {
            //独立事件 => 直接执行
            if (!ThisEntity.CurrentEvent.HaveTarget)
            {
                ThisEntity.SolveEvent();
                return TaskStatus.Running;
            }

            //主动发起方去寻找被动方 => 找到后执行
            if (ThisEntity.CurrentEvent.Self == ThisEntity.ThisEmp)
            {
                if (!ThisEntity.CurrentEvent.TargetEntity)
                {
                    Debug.LogError("有目标的事件，但是没有目标对象");
                }
                Movable.Value = true;
                StopDistance.Value = 1;
                Destination.Value = ThisEntity.CurrentEvent.TargetEntity.transform.position;

                //满足开始执行事件的条件（TODO）
                if (Function.XZDistance(ThisEntity.transform.position, Destination.Value) < 8)
                {
                    ThisEntity.SolveEvent();
                    return TaskStatus.Running;
                }
            }
        }

        return TaskStatus.Running;
    }
}
