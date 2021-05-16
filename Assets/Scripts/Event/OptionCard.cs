using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionCard
{
    public int Num = 0;
    public int Correction = 0;
    public bool RandomPerk = false ;//是否会获得随机负面特质
    public bool DebuffCard = false;//是否为负面抉择卡
    public bool AddDebuffPerk = false;//是否会产生负面状态
    public string Name, Description;

    public OptionCard()
    {
        
    }

    public virtual void StartEffect(Employee emp)
    {

    }

    public virtual void SelectEffectActive()
    {

    }
}

public class OptionCard1 : OptionCard
{
    public OptionCard1() : base()
    {
        Num = 1;
        Correction = 1;
        Name = "辩解";
        Description = "所在事业部获得状态“推卸责任”×1";
        DebuffCard = true;
        AddDebuffPerk = true;
    }

    public override void StartEffect(Employee emp)
    {
        if (emp != null && emp.CurrentDivision != null)
        {
            emp.CurrentDivision.AddPerk(new Perk117());
        }
    }
}

public class OptionCard2 : OptionCard
{
    public OptionCard2() : base()
    {
        Num = 2;
        Correction = 1;
        Name = "坦诚";
        Description = "";     
        DebuffCard = false;
    }

}

public class OptionCard3 : OptionCard
{
    public OptionCard3() : base()
    {
        Num = 3;
        Correction = 3;
        Name = "煤气灯";
        Description = "目标员工获得随机负面特质";
        RandomPerk = true;
        DebuffCard = true;
    }
}

public class OptionCard4 : OptionCard
{
    public OptionCard4() : base()
    {
        Num = 4;
        Correction = 0;
        Name = "沉默";
        Description = "";
        DebuffCard = true;
    }
}

public class OptionCard5 : OptionCard
{
    public OptionCard5() : base()
    {
        Num = 5;
        Correction = 3;
        Name = "推卸责任";
        Description = "所在事业部获得状态“推卸责任”×1,目标员工获得随机负面特质";
        RandomPerk = true;
        DebuffCard = true;
        AddDebuffPerk = true;
    }

    public override void StartEffect(Employee emp)
    {
        if (emp != null && emp.CurrentDivision != null)
        {
            emp.CurrentDivision.AddPerk(new Perk117());
        }
    }
}

public class OptionCard6 : OptionCard
{
    public OptionCard6() : base()
    {
        Num = 6;
        Correction = 2;
        Name = "转移话题";
        Description = "所在事业部获得状态“推卸责任”×1,目标员工获得随机负面特质";
        RandomPerk = true;
        DebuffCard = true;
    }
}

public class OptionCard7 : OptionCard
{
    public OptionCard7() : base()
    {
        Num = 7;
        Correction = 4;
        Name = "捏造事实";
        Description = "所在事业部获得状态“怀疑情绪”×1,目标员工获得随机负面特质";
        RandomPerk = true;
        DebuffCard = true;
        AddDebuffPerk = true;
    }

    public override void StartEffect(Employee emp)
    {
        if (emp != null && emp.CurrentDivision != null)
        {
            emp.CurrentDivision.AddPerk(new Perk118());
        }
    }
}

public class OptionCard8 : OptionCard
{
    public OptionCard8() : base()
    {
        Num = 8;
        Correction = 0;
        Name = "另辟蹊径";
        Description = "重新抽取3张抉择卡";
    }
}//没做完

public class OptionCard9 : OptionCard
{
    public OptionCard9() : base()
    {
        Num = 9;
        Correction = 0;
        Name = "寻求共识";
        Description = "下张抉择卡的修正翻倍";
        DebuffCard = false;
    }

    public override void StartEffect(Employee emp)
    {
        if (emp != null && emp.CurrentDivision != null)
        {

        }
    }
}//没做完

public class OptionCard10 : OptionCard
{
    public OptionCard10() : base()
    {
        Num = 10;
        Correction = 0;
        Name = "缓和情绪";
        Description = "随机去掉1个待施加的负面状态";    
        DebuffCard = false;
    }

    public override void StartEffect(Employee emp)
    {
        if (emp != null && emp.CurrentDivision != null)
        {

        }
    }
}//没做完

public class OptionCard11 : OptionCard
{
    public OptionCard11() : base()
    {
        Num = 11;
        Correction = 1;
        Name = "基本演绎法";
        Description = "选中后消耗自身并额外获得1张抉择卡";
        DebuffCard = false;
    }

    public override void SelectEffectActive()
    {
        
    }
}//没做完

public class OptionCard12 : OptionCard
{
    public OptionCard12() : base()
    {
        Num = 12;
        Correction = 2;
        Name = "一语中的";
        Description = "";
        DebuffCard = false;
    }

    public override void StartEffect(Employee emp)
    {
        if (emp != null && emp.CurrentDivision != null)
        {

        }
    }
}

public class OptionCard13 : OptionCard
{
    public OptionCard13() : base()
    {
        Num = 13;
        Correction = 0;
        Name = "聚焦";
        Description = "选择并去掉1张抉择卡，重新抽取2张";
        DebuffCard = false;
    }

    public override void StartEffect(Employee emp)
    {
        if (emp != null && emp.CurrentDivision != null)
        {

        }
    }
}//没做完

public class OptionCard14 : OptionCard
{
    public OptionCard14() : base()
    {
        Num = 14;
        Correction = 2;
        Name = "呵斥";
        Description = "所在事业部获得状态“状态低迷”×1";       
        DebuffCard = true;
        AddDebuffPerk = true;
    }

    public override void StartEffect(Employee emp)
    {
        if (emp != null && emp.CurrentDivision != null)
        {
            emp.CurrentDivision.AddPerk(new Perk119());
        }
    }
}

public class OptionCard15 : OptionCard
{
    public OptionCard15() : base()
    {
        Num = 15;
        Correction = 2;
        Name = "一言堂";
        Description = "所在事业部获得状态“寒蝉效应”×1";
        DebuffCard = true;
        AddDebuffPerk = true;
    }

    public override void StartEffect(Employee emp)
    {
        if (emp != null && emp.CurrentDivision != null)
        {
            emp.CurrentDivision.AddPerk(new Perk120());
        }
    }
}

public class OptionCard16 : OptionCard
{
    public OptionCard16() : base()
    {
        Num = 16;
        Correction = 2;
        Name = "撒币";
        Description = "所在事业部获得状态“成本飙升”×1";
        DebuffCard = true;
        AddDebuffPerk = true;
    }

    public override void StartEffect(Employee emp)
    {
        if (emp != null && emp.CurrentDivision != null)
        {
            emp.CurrentDivision.AddPerk(new Perk121());
        }
    }
}