using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class AdjustData
{
    static public List<int> ExpRequire = new List<int>() { 40, 100, 180, 280, 400, 540, 700, 880, 1080, 1300, 0};//员工升级所需经验

    static public int EmpLevelUpExp = 50;//员工升级所需经验
    static public int CoreMemberExp = 70;//核心团队获取经验量
    static public int CoreMemberUpgradePointLimit = 15;//核心团队获得升级点数所需要的解决的事件数量

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
    static public int[] UserRult = new int[] { 0, 1, 2, 3, 4 };
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
    static public int[] PlayerUSerLimit = new int[] { 10, 22, 40, 65 };
    #endregion
}
