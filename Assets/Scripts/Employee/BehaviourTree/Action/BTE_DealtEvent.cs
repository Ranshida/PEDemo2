using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// 处理事件中
/// </summary>
[TaskCategory("Employee")]
public class BTE_DealtEvent : BTE_Action
{
    protected override void AfterOnStart()
    {
        //设置寻路点至公司出口
        //解除当前的行为
    }
}
