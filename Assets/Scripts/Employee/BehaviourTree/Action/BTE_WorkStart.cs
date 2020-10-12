using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// 开始工作，前往工位
/// 到达后检测事件等情况
/// 更新时间2020.10.12
/// </summary>
[TaskCategory("Employee")]
public class BTE_WorkStart : BTE_Action
{
    public SharedBool MoveFlag;

    protected override void AfterOnStart()
    { 
        //设置寻路点至工位
        MoveFlag.Value = true;
    }
    public override TaskStatus OnUpdate()
    {
        MoveFlag.Value = true;
        ThisEmp.Destination = BuildingManage.Instance.ExitPos.position;

        if (!ThisEmp.OffWork)
        {
            return TaskStatus.Success;
        }
        return TaskStatus.Running;
    }
}
