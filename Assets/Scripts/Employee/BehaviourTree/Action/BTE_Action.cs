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
}
