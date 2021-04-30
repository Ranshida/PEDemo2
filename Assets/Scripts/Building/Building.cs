using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Building : MonoBehaviour
{
    public BuildingType Type;    //现在是创建时赋值，需改为预制体赋值或子类构造

    public int ID;                             //见建筑物Excel中的行数
    public string Name;   
    public string Size;                        //尺寸str（a×b）
    public string Function_A;                  //建筑的功能1
    public string Require_A;                   //技能需求
    public string Description_A;               //建筑功能1的描述
    public string Time_A;                      //建筑功能1的基础生产周期
    public string Result_A;                    //建筑功能1具体效果描述
    public string Require_B;
    public int MaintainCost;                   //维护费用
    public int PurchaseCost;                   //购买费用
    public string Manage;                      //占用管理
    public string FaithBonus;                  //信念减益  
    public string WorkStatus_str;              //工作状态
    public string MajorSuccess_str;            //大成功率
    public string Failure_str;                 //失误率
    public string EffectRange_str;                        //影响范围str
    public string Jobs;                             //岗位数量
    public string Str_Type;

    public int X10, Z10;//建筑中点位坐标值*10
    public int Length;
    public int Width;
    public int EffectRange;
    public int EffectRangeType = 2;//0无范围 1自身范围 2自身+邻居
    public int StaminaRequest = 0;
    public int effectValue = 0, effectValue2 = 0;//1技术 2市场 3产品 4观察 5坚韧 6强壮 7管理 8人力 9财务 10决策 11行业 12谋略 13说服 14魅力 15八卦 16行政
    public bool BuildingSet { get; private set; } = false;   //设置完毕不再动
    public bool Moving { get; private set; } = false;        //移动中
    public bool CanLottery;    //基础建筑物，可以直接建造
    public bool CanDismantle = true;

    public Area CurrentArea;
    public DepControl Department; //BM赋值
    public BuildingEffect effect;
    private Transform m_Decoration;   //修饰物，建造后删除

    public List<Grid> ContainsGrids;   //所包含的格子
    public List<Transform> WorkPos;
    public List<BuildingEffect> EffectBuildings = new List<BuildingEffect>();

    //BUildingManager加载此建筑预制体
    public void LoadPrefab(string[,] value)
    {
        WorkPos = Function.ReturnChildList(transform.Find("WorkPosition"));
        m_Decoration = transform.Find("Decoration");

        if (ID < 3)
            return;
        Name = value[ID, 2];
        Str_Type = value[ID, 3];
        Jobs = value[ID, 5];
        Manage = value[ID, 6];
        Size = value[ID, 7];
        EffectRange_str = value[ID, 8];
        MaintainCost = int.Parse(value[ID, 9]);
        PurchaseCost = int.Parse(value[ID, 10]);
        Time_A = value[ID, 11];
        FaithBonus = value[ID, 12];
        WorkStatus_str = value[ID, 13];
        MajorSuccess_str = value[ID, 14];
        Failure_str = value[ID, 15];
        Function_A = value[ID, 16];
        Require_A = value[ID, 17];
        Require_B = value[ID, 18];
        Description_A = value[ID, 19];
        Result_A = value[ID, 20];

        //工作状态转化
        if (WorkStatus_str == "/")
            WorkStatus_str = "0";

        //信念减益字符的转化
        if (FaithBonus == "/")
            FaithBonus = "0";

        //影响范围转化
        EffectRangeType = int.Parse(EffectRange_str);

        //大成功率和失败率转化
        if (MajorSuccess_str == "/")
        {
            MajorSuccess_str = "0";
        }
        if (Failure_str == "/")
        {
            Failure_str = "0";
        }

        //工时转化
        if (Time_A == "/")
            Time_A = "0";

        //技能需求转化
        if (Require_A != "/")
            effectValue = (int)(System.Enum.Parse(typeof(ProfessionType), Require_A));
        if (Require_B != "/")
            effectValue2 = (int)(System.Enum.Parse(typeof(ProfessionType), Require_B));

        //Type = (BuildingType)System.Enum.Parse(typeof(BuildingType), Name);
        string[] size = Size.Split('×');
        Length = int.Parse(size[0]);
        Width = int.Parse(size[1]);
        CanLottery = Str_Type != "基础办公室" && Type != BuildingType.CEO办公室 && Type != BuildingType.人力资源部;
        if (!int.TryParse(EffectRange_str, out EffectRange))
        {
            if (EffectRange_str == "/")
                EffectRange = 0;
            if (EffectRange_str == "全公司")
                EffectRange = 999;
        }
       
        if (Type == BuildingType.CEO办公室)
        {
            CanDismantle = false;
        }
    }
    
    //确定建造
    public void Build(List<Grid> grids)
    {
        BuildingSet = true;
        Moving = false;
        ContainsGrids = new List<Grid>();
        int xMax = grids[0].X, xMin = grids[0].X, zMax = grids[0].Z, zMin = grids[0].Z;
        foreach (Grid grid in grids)
        {
            ContainsGrids.Add(grid);
            grid.Build(this);

            if (grid.X > xMax)
                xMax = grid.X;
            if (grid.X < xMin)
                xMin = grid.X;
            if (grid.Z > zMax)
                zMax = grid.Z;
            if (grid.Z < zMin)
                zMin = grid.Z;
        }
        X10 = ((xMax - xMin) * 10 / 2) + (xMin * 10);
        Z10 = ((zMax - zMin) * 10 / 2) + (zMin * 10);

        effect = new BuildingEffect(this);
        
        //Destroy(m_Decoration);
    }

    public void Move()
    {
        effect.RemoveAffect();
        Moving = true;
        foreach (Grid grid in ContainsGrids)
        {
            grid.Dismantle();
        }
    }

    //拆除
    public bool Dismantle()
    {
        if (!CanDismantle)
        {
            Debug.Log("不能拆除这个建筑");
            return false;
        }

        CanDismantle = false;
        if (!Moving)
        {
            //重置影响范围
            effect.RemoveAffect();
            //刷新网格
            foreach (Grid grid in ContainsGrids)
                grid.Dismantle();
        }

        //销毁前的准备工作
        if (Department)
        {
            Department.ClearDep();
        }
        BuildingManage.Instance.ConstructedBuildings.Remove(this);
        if (BuildingManage.Instance.ConstructedBuildings.Count == 0)
            GameControl.Instance.GameOverPanel.GetComponent<WindowBaseControl>().SetWndState(true);
        Destroy(gameObject);

        return true;
    }
}
