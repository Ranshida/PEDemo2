using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class AdjustData
{
    static public int BottleneckValue = 800; //每层瓶颈提供的额外经验需求

    static public int[] ExpLimit = { 400,600,800,1000,1200,1400,1600,1800,2000,2200}; //各等级(1-10)升级所需经验

}
