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
        if (ThisEmp.Available)
            return TaskStatus.Failure;

        //事件开始执行
        if (ThisEmp.CurrentEvent.isSolving)
            return TaskStatus.Success;

        //独立事件 => 直接执行
        if (!ThisEmp.CurrentEvent.HaveTarget)
        {
            Movable.Value = false;
            ThisEmp.DealtEvent();
        }
        //主动事件 => 寻找目标后执行
        else if (ThisEmp.CurrentEvent.SelfEntity == ThisEmp)
        {
            Movable.Value = true;
            StopDistance.Value = 8;
            Destination.Value = ThisEmp.CurrentEvent.TargetEntity.transform.position;

            //距离近 开始执行事件
            if (Function.XZDistance(ThisEmp.transform.position, Destination.Value) < 10) 
                ThisEmp.DealtEvent();
        }
        //被动事件 => 等待被找到后执行
        else
        {
            StopDistance.Value = 0.5f;
            Movable.Value = true;
            Destination.Value = FindWorkPosition();
        }
       

      
        return TaskStatus.Running;
    }
}
