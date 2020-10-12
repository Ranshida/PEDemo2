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
    public SharedBool MoveFlag;
    public SharedVector3 Destination;
    public SharedFloat StopDistance;

    private float arrivedTimer;

    protected override void AfterOnStart()
    {
        if (ThisEmp.CurrentEvent != null) 
        {
            ThisEmp.EventList.Insert(0, ThisEmp.CurrentEvent);
            ThisEmp.CurrentEvent = null;
        }
        MoveFlag.Value = true;
        //设置寻路点至公司出口
        //解除当前的行为
    }
    public override TaskStatus OnUpdate()
    {
        MoveFlag.Value = true;
        StopDistance.Value = 0.1f;

        if (ThisEmp.InfoDetail.emp.CurrentDep != null)
            Destination.Value = ThisEmp.InfoDetail.emp.CurrentDep.building.WorkPos[ThisEmp.InfoDetail.emp.CurrentDep.CurrentEmps.IndexOf(ThisEmp.InfoDetail.emp)].position;
        else if (ThisEmp.InfoDetail.emp.CurrentOffice != null)
            Destination.Value = ThisEmp.InfoDetail.emp.CurrentOffice.building.WorkPos[0].position;

        if (Function.XZDistance(ThisEmp.transform.position, Destination.Value) < 0.2f)  
            arrivedTimer += Time.deltaTime;
        else
            arrivedTimer = 0;

        if (arrivedTimer > 0.5f) 
        {
            //到达工位，遍历事件，有事件执行事件，没有事件继续干活
            ThisEmp.CheckEvents();
            return TaskStatus.Failure;
        }
        return TaskStatus.Running;
    }
}
