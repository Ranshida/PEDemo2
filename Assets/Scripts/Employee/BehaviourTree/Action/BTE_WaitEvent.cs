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

    protected override void AfterOnStart()
    {
        //独立事件 => 直接执行
        if (!ThisEmp.CurrentEvent.HaveTarget)
        {
            Movable.Value = false;
        }

        //主动事件 => 寻找目标后执行
        else if (ThisEmp.CurrentEvent.SelfEntity == ThisEmp)
        {
            Movable.Value = true;
            Destination.Value = ThisEmp.CurrentEvent.Target.InfoDetail.Entity.transform.position;
            StopDistance.Value = 8;
            //寻找目标
        }
        //被动事件 => 等待被找到后执行
        else
        {
            Movable.Value = true;
            Destination.Value = FindWorkPosition();
            StopDistance.Value = 8;
            //正常寻找工作岗位
        }
    }

    public override TaskStatus OnUpdate()
    {
        //独立事件 => 直接执行
        if (!ThisEmp.CurrentEvent.HaveTarget)
        {
            ThisEmp.DealtEvent();
            return TaskStatus.Success;
        }

        //主动事件 => 寻找目标后执行
        if (ThisEmp.CurrentEvent.Self.InfoDetail.Entity == ThisEmp)
        {      
            Destination.Value = ThisEmp.CurrentEvent.Target.InfoDetail.Entity.transform.position;

            //距离近 开始执行事件
            if (Function.XZDistance(ThisEmp.transform.position, Destination.Value) < 10) 
                ThisEmp.DealtEvent();
        }
        //被动事件 => 等待被找到后执行
        else
            Destination.Value = FindWorkPosition();

        //事件开始执行
        if (ThisEmp.CurrentEvent.isSolving)
            return TaskStatus.Success;
        return TaskStatus.Running;
    }
}
