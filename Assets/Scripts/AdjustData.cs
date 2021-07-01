using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class AdjustData
{
    #region 事件相关
    static public float BuildingDismentleEventPosb = 0.5f;//建筑拆除时产生负面事件组的概率
    static public float EmpFireEventPosb = 0.5f;//开除员工时产生负面事件组的概率
    #endregion

    #region 员工升级相关
    static public int[] ExpRequire = new int[] { 50, 50, 50, 50, 50 };//员工各等级升级所需经验
    static public int CoreMemberExp = 70;//核心团队获取经验量
    static public int CoreMemberUpgradePointLimit = 6;//核心团队获得升级点数所需要的解决的事件数量

    static public float AmbitionTypeAPosb = 0.2f, AmbitionTypeBPosb = 0.4f, AmbitionTypeCPosb = 0.4f;//志向模板1-3的概率
    //模板1-3（A-C）的强度（1弱3强）和4（D）岗位优势模板的强度
    static public int[] AmbitionTypeA = new int[] { 2, 1, 2, 1, 3 }, AmbitionTypeB = new int[] { 1, 1, 2, 2, 3 };
    static public int[] AmbitionTypeC = new int[] { 2, 2, 1, 1, 2 }, AmbitionTypeD = new int[] { 0, 1, 0, 2, 0 };
    //储存所有模板的链表
    static public List<int[]> AmbitionTypes = new List<int[]>() { AmbitionTypeA, AmbitionTypeB, AmbitionTypeC, AmbitionTypeD };
    #endregion

    #region 融资相关
    //融资数值相关
    //天使、A、B、C、上市投资轮的各等级用户意愿判定阈值
    static public int[] UserLevel1 = new int[] { 3, 4, 5, 6 }, UserLevel2 = new int[] { 10, 12, 14, 16 }
    , UserLevel3 = new int[] { 22, 25, 28, 31 }, UserLevel4 = new int[] { 40, 44, 48, 52 }, UserLevel5 = new int[] { 65, 70, 75, 80 };
    //士气等级阈值
    static public int[] MoraleLevel = new int[] { 100, 80, 60, 40 };
    //人均成本等级阈值
    static public int[] CostLevel = new int[] { 20, 30, 40};

    //根据用户数量等级确定的意愿加成
    static public int[] UserResult = new int[] { 0, 1, 2, 3, 4 };
    //商战排名1-4时的意愿加成
    static public int[] RankResult = new int[] { 3, 2, 1, 0 };
    //士气从最高到最低的意愿加成
    static public int[] MoraleResult = new int[] { 0, -1, -2, -3, -5 };
    //人均成本的意愿加成
    static public int[] CostResult = new int[] { 3, 2, 1, 0 };

    //各轮投资的收入
    static public int[] InvestIncome = new int[] { 50, 100, 200, 400, 800 };
    //投资意愿等级阈值
    static public int[] InvestWillLevel = new int[] { 3, 5, 7, 9 };
    #endregion

    #region 商战相关
    //敌对公司A用户获取
    static public int[] UserObtain_A = new int[] { -1, 0, 1, 2 };
    //敌对公司A用户获取概率
    static public float[] UserPosb_A = new float[] { 0.25f, 0.25f, 0.25f, 0.25f };

    //敌对公司B用户获取
    static public int[] UserObtain_B = new int[] { -3, 1, 2, 5 };
    //敌对公司B用户获取概率
    static public float[] UserPosb_B = new float[] { 0.15f, 0.25f, 0.45f, 0.15f };

    //敌对公司C用户获取
    static public int[] UserObtain_C = new int[] { -2, 3, 3, 5 };
    //敌对公司C用户获取概率
    static public float[] UserPosb_C = new float[] { 0.25f, 0.25f, 0.25f, 0.25f };

    //各轮融资玩家用户获取概率                   (初始、1级牌加成、2级牌加成、3级牌加成)
    static public float[] PUserPosb1 = new float[] { 0.5f, 0.2f, 0.3f, 0.5f };//天使轮
    static public float[] PUserPosb2 = new float[] { 0.4f, 0.15f, 0.25f, 0.4f };//A轮
    static public float[] PUserPosb3 = new float[] { 0.2f, 0.12f, 0.2f, 0.3f };//B轮
    static public float[] PUserPosb4 = new float[] { 0f, 0.1f, 0.15f, 0.25f };//C轮
    static public float[] PUserPosb5 = new float[] { -0.3f, 0.08f, 0.12f, 0.2f };//上市

    //各轮融资阶段玩家用户上限
    static public int[] PlayerUserLimit = new int[] { 10, 22, 40, 65 };
    #endregion
}
