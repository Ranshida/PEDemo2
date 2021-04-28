using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventGroup : Event
{
    public int ExtraStage = 0;//事件组生成后待机(不产生效果)的回合数
    public int StageCount = 1;//事件组总阶段数

    public EventGroup() : base()
    {
        isEventGroup = true;
    }

    public override void StartEvent(Employee emp, int ExtraCorrection = 0, Employee target = null, int Stage = 0)
    {
        //如果判定成功就不继续
        if (FindResult(emp, ExtraCorrection, target) == 1)
            return;

        if (Stage == 1)
            EffectA(emp, ExtraCorrection, target);
        else if (Stage == 2)
            EffectB(emp, ExtraCorrection, target);
        else if (Stage == 3)
            EffectC(emp, ExtraCorrection, target);
        else if (Stage == 4)
            EffectD(emp, ExtraCorrection, target);
        else if (Stage == 5)
            EffectE(emp, ExtraCorrection, target);
        else if (Stage == 6)
            EffectF(emp, ExtraCorrection, target);
    }

    protected virtual void EffectA(Employee emp, int ExtraCorrection = 0, Employee target = null)
    {

    }
    protected virtual void EffectB(Employee emp, int ExtraCorrection = 0, Employee target = null)
    {

    }
    protected virtual void EffectC(Employee emp, int ExtraCorrection = 0, Employee target = null)
    {

    }
    protected virtual void EffectD(Employee emp, int ExtraCorrection = 0, Employee target = null)
    {

    }
    protected virtual void EffectE(Employee emp, int ExtraCorrection = 0, Employee target = null)
    {

    }
    protected virtual void EffectF(Employee emp, int ExtraCorrection = 0, Employee target = null)
    {

    }
}

public class EventGroup1 : EventGroup
{
    public EventGroup1() : base()
    {

    }
}
