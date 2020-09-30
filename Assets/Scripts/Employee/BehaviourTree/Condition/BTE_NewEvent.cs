﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// 下班信号
/// </summary>
[TaskCategory("Employee")]
public class BTE_NewEvent : BTE_Condition
{
    public override TaskStatus OnUpdate()
    {
        if (ThisEmp.CurrentEvent != null) 
        {
            //被动事件优先

            //主动时间次要
        }

        return ThisEmp.CurrentEvent != null ? TaskStatus.Success : TaskStatus.Failure;
    }

}