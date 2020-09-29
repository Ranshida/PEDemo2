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
    protected override void AfterOnStart()
    {
        //设置寻路点至公司出口
        //解除当前的行为
    }
}
