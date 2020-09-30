using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// 员工下班
/// </summary>
[TaskCategory("Employee")]
public class BTE_OffWork : BTE_Action
{
    public SharedBool MoveFlag;

    protected override void AfterOnStart()
    {
        MoveFlag.Value = true;
        //设置寻路点至公司出口
        //解除当前的行为
    }
    public override TaskStatus OnUpdate()
    {
        MoveFlag.Value = true;
        ThisEmp.Destination = BuildingManage.Instance.ExitPos.position;

        if (!ThisEmp.OffWork)
        {
            return TaskStatus.Failure;
        }
        return TaskStatus.Running;
    }
}
