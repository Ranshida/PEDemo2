using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class BTE_Action : Action
{
    public SharedBehaviour EmpEntity;
    protected EmpEntity ThisEmp;

    public override void OnStart()
    {
        ThisEmp = EmpEntity.Value as EmpEntity;
        AfterOnStart();
    }
    protected virtual void AfterOnStart() { }

    //返回工作岗位，如果没有工作则随机位置
    protected Vector3 FindWorkPosition()
    {
        if (ThisEmp.InfoDetail.emp.CurrentDep != null)
        {
            return ThisEmp.InfoDetail.emp.CurrentDep.building.WorkPos[ThisEmp.InfoDetail.emp.CurrentDep.CurrentEmps.IndexOf(ThisEmp.InfoDetail.emp)].position;
        }
        else if (ThisEmp.InfoDetail.emp.CurrentOffice != null)
        {
            return ThisEmp.InfoDetail.emp.CurrentOffice.building.WorkPos[0].position;
        }
        //TODO设为随机位置
        return Vector3.zero;
    }
}
