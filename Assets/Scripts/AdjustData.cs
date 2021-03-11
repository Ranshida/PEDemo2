using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class AdjustData
{
    static public int BottleneckValue = 800; //每层瓶颈提供的额外经验需求

    static public int[] ExpLimit = { 400,600,800,1000,1200,1400,1600,1800,2000,2200}; //各等级(1-10)升级所需经验

    //1.技术 2.市场 3.产品 8.HR人力 9.Fi财务 11.Fo行业 12.St谋略 13.Co说服 15.Go八卦 16.Ad行政
    static public int CEOProfessionType = 13; //CEO专场能力类型（编号见上面）
    static public int CEOProfessionValue = 4; //CEO专场能力数值（编号见上面）

    static public int EmpAProfessionType = 1; //默认员工A专场能力类型（编号见上面）
    static public int EmpAProfessionValue = 4; //默认员工A专场能力数值（编号见上面）

    static public int EmpBProfessionType = 8; //默认员工B专场能力类型（编号见上面）
    static public int EmpBProfessionValue = 4; //默认员工B专场能力数值（编号见上面）
}
