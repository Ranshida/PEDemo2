using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

/// <summary>
/// 员工的行进间状态
/// 更新时间2020.9.19
/// </summary>
public class BTE_Moving : Action
{
    public Vector3 dir;

    // cache the navmeshagent component
    private NavMeshAgent navMeshAgent;

    public override void OnStart()
    {
        navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
    }

    public override TaskStatus OnUpdate()
    {
        if (navMeshAgent == null || !BuildingManage.Instance)
        {
            Debug.LogWarning("NavMeshAgent is null");
            return TaskStatus.Running;
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            navMeshAgent.SetDestination(BuildingManage.Instance.AimingPosition);
        }

        return TaskStatus.Running;
    }
}
