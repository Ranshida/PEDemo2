using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// 员工正常工作
/// </summary>
[TaskCategory("Employee")]
public class BTE_OnWork : BTE_Action
{
    public SharedBool MoveFlag;
    public SharedVector3 Destination;
    public SharedFloat StopDistance;
    
    public override TaskStatus OnUpdate()
    {
        MoveFlag.Value = true;
        StopDistance.Value = 0.1f;
        if (ThisEmp.InfoDetail.emp.CurrentDep != null)
        {
            Destination.Value = ThisEmp.InfoDetail.emp.CurrentDep.building.WorkPos[ThisEmp.InfoDetail.emp.CurrentDep.CurrentEmps.IndexOf(ThisEmp.InfoDetail.emp)].position;
        }
        else if (ThisEmp.InfoDetail.emp.CurrentOffice != null)
        {
            Destination.Value = ThisEmp.InfoDetail.emp.CurrentOffice.building.WorkPos[0].position;
        }
        return TaskStatus.Running;
    }
}
