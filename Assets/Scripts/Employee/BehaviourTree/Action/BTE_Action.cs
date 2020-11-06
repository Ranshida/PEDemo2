using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class BTE_Action : Action
{
    public SharedBehaviour EmpEntity;
    protected EmpEntity ThisEntity;

    public override void OnStart()
    {
        ThisEntity = EmpEntity.Value as EmpEntity;
        AfterOnStart();
    }
    protected virtual void AfterOnStart() { }

    //返回工作岗位，如果没有工作则随机位置
    protected Vector3 FindWorkPosition()
    {
        if (ThisEntity.WorkPosition.HasValue)
        {
            return ThisEntity.WorkPosition.Value;
        }
        return ThisEntity.NextWP.transform.position;
    }
}
