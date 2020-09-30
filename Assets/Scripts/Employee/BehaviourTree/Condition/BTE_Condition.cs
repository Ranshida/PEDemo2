using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// 员工的事件条件基类
/// </summary>
[TaskCategory("Employee")]
public class BTE_Condition : Conditional
{
    public SharedBehaviour EmpEntity;
    protected EmpEntity ThisEmp;

    public override void OnStart()
    {
        ThisEmp = EmpEntity.Value as EmpEntity;
        AfterOnStart();
    }
    protected virtual void AfterOnStart() { }
}
