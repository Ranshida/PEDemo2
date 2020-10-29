using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;

/// <summary>
/// 员工的行进间状态
/// 更新时间2020.9.19
/// </summary>
[TaskCategory("Employee")]
public class BTE_Moving : BTE_Action
{
    // cache the navmeshagent component
    private NavMeshAgent navMeshAgent;
    public SharedBool MoveFlag;
    public SharedFloat StopDistance;
    public SharedVector3 Destination;

    protected override void AfterOnStart()
    {
        navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
    }

    public override TaskStatus OnUpdate()
    {
        if (!MoveFlag.Value)
        {
            return TaskStatus.Failure;
        }

        //移动速度、暂停
        if (GameControl.Instance.ForceTimePause)
            navMeshAgent.speed = 0;
        else
            navMeshAgent.speed = 20 * GameControl.Instance.TimeMultiply;

        navMeshAgent.stoppingDistance = StopDistance.Value;
        navMeshAgent.SetDestination(Destination.Value);

        return TaskStatus.Running;
    }
}
