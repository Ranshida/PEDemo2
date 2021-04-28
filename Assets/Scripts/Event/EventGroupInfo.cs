using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventGroupInfo : MonoBehaviour
{
    public int Stage = 1;

    public EventGroup TargetEventGroup;

    //进入下一个事件阶段
    public void NextStage()
    {
        Stage += 1;

        //检查是否完成了所有阶段
        if (Stage <= TargetEventGroup.StageCount)
        {

        }
        else
        {

        }
    }
}
