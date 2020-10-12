using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// 员工下班
/// 更新时间 before2020.10
/// </summary>
[TaskCategory("Employee")]
public class BTE_OffWork : BTE_Action
{
    public SharedVector3 Destination;
    public SharedFloat StopDistance;
    public SharedBool Movable;

    private float arrivedTimer;
    
    public override TaskStatus OnUpdate()
    {
        //可以移动，精确寻找位置
        Movable.Value = true;
        StopDistance.Value = 1f;

        //设置寻路点至公司出口
        Destination.Value = BuildingManage.Instance.ExitPos.position;

        if (!ThisEmp.OffWork) 
        {
            return TaskStatus.Failure;
        }
        return TaskStatus.Running;
    }
}
