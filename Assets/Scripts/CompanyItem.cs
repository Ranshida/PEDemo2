using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CompanyItemType
{
    Default, MonthMeeting, Fight 
}

public class CompanyItem
{
    public CompanyItemType Type;
    public CompanyItem(CompanyItemType type)
    {
        Type = type;
    }
}
