using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionCard
{
    public int Correction = 0;
    public bool RandomPerk = false;//是否会获得随机负面特质
    public bool DebuffCard = false;//是否为负面抉择卡
    public string Name, Description;

    public OptionCard()
    {
        MonoBehaviour.print("Shit");
    }

    public virtual void StartEffect(Employee emp)
    {

    }
}

public class OptionCard1 : OptionCard
{
    public OptionCard1() : base()
    {
        Correction = 1;
        Name = "回避冲突";
        Description = "所在事业部获得状态“怀疑”×1，目标员工获得随机负面特质";
        RandomPerk = true;
        DebuffCard = true;
    }

}

public class OptionCard2 : OptionCard
{
    public OptionCard2() : base()
    {
        Correction = 2;
        Name = "闪烁其词";
        Description = "所在事业部获得状态“怀疑”×3，目标员工获得随机负面特质";
        RandomPerk = true;
        DebuffCard = true;
    }

}

public class OptionCard3 : OptionCard
{
    public OptionCard3() : base()
    {
        Correction = 3;
        Name = "拒绝沟通";
        Description = "所在事业部获得状态“怀疑”×5，目标员工获得随机负面特质";
        RandomPerk = true;
        DebuffCard = true;
    }

}
