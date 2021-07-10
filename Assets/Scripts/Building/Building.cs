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
    public string[] Functions = new string[3]; //建筑物各等级功能描述
    public string[] Debuffs = new string[3];   //各等级负面效果
    public string Require_A;                   //技能需求
    public string Description;                 //建筑整体功能的描述
    public int MaintainCost;                   //维护费用
    public int PurchaseCost;                   //购买费用
    public string Jobs;                        //岗位数量
    public string Str_Type;                    //建筑类型
    public string WeakEffect;                  //弱化效果
    public string ExtraInfo;                   //额外的物品和状态信息
    public int[] EmpCount = new int[3];//各等级功能所需员工数量

    public int X10, Z10;//建筑中点位坐标值*10
    public int Length;
    public int Width;
    public int StaminaRequest = 0;
    public int effectValue = 0, effectValue2 = 0;//1技术 2市场 3产品 4观察 5坚韧 6强壮 7管理 8人力 9财务 10决策 11行业 12谋略 13说服 14魅力 15八卦 16行政
    public bool BuildingSet { get; private set; } = false;   //设置完毕不再动
    public bool Moving { get; private set; } = false;        //移动中
    public bool CanDismantle = true;//建筑模式下是否可以拆除
    public bool CanMove = true;//建筑模式下是否可以选中并移动
    public bool IndependentBuilding = false;//是否为独立建筑（直接放置在场景中，没有所属的事业部）

    public Area CurrentArea;
    public DivisionControl AttachedDivision;//这个变量有赋值时，该建筑只能放置在对应的区域内
    public DepControl Department; //BM赋值
    public Building MasterBuilding;//作为附加建筑时自身的父建筑
    //public BuildingEffect effect; //旧的BuildingEffect
    public Transform WarePanel;
    private Transform m_Decoration;   //修饰物，建造后删除

    public List<Grid> ContainsGrids;   //所包含的格子
    public List<Transform> WorkPos;
    //public List<BuildingEffect> EffectBuildings = new List<BuildingEffect>(); //旧的影响范围内的建筑链表


    private void Start()
    {
        if (IndependentBuilding)
        {
            LoadPrefab(ExcelTool.ReadAsString(Application.streamingAssetsPath + "/Excel/BuildingFunction.xlsx"));

            Department = GameControl.Instance.CreateDep(this);
            Department.DivPanel.gameObject.SetActive(false);
        }
    }
    //BUildingManager加载此建筑预制体
    public void LoadPrefab(string[,] value)
    {
        WorkPos = Function.ReturnChildList(transform.Find("WorkPosition"));
        m_Decoration = transform.Find("Decoration");

        if (ID == 1)
            return;
        Name = value[ID, 1];
        Str_Type = value[ID, 2];
        Size = value[ID, 3];
        Require_A = value[ID, 4];
        Jobs = value[ID, 5];
        PurchaseCost = int.Parse(value[ID, 6]);
        MaintainCost = int.Parse(value[ID, 7]);
        Description = value[ID, 8];
        EmpCount[0] = int.Parse(value[ID, 9]);
        Functions[0] = value[ID, 10];
        Debuffs[0] = value[ID, 11];
        EmpCount[1] = int.Parse(value[ID, 12]);
        Functions[1] = value[ID, 13];
        Debuffs[1] = value[ID, 14];
        EmpCount[2] = int.Parse(value[ID, 15]);
        Functions[2] = value[ID, 16];
        Debuffs[2] = value[ID, 17];
        WeakEffect = value[ID, 18];
        ExtraInfo = value[ID, 19];

        //技能需求转化
        if (Require_A != "/")
            effectValue = (int)(System.Enum.Parse(typeof(ProfessionType), Require_A));

        //Type = (BuildingType)System.Enum.Parse(typeof(BuildingType), Name);
        string[] size = Size.Split('×');
        Length = int.Parse(size[0]);
        Width = int.Parse(size[1]);
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

        //effect = new BuildingEffect(this);//确认影响范围
        
        //Destroy(m_Decoration);
    }

    public void Move()
    {
        //effect.RemoveAffect();//移动前清除影响范围内的建筑
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
            GameControl.Instance.CreateMessage("该建筑无法手动拆除");
            return false;
        }

        if (Random.Range(0.0f, 1.0f) < AdjustData.BuildingDismentleEventPosb)
            GameControl.Instance.EC.CreateEventGroup(EventData.BuildingDismentleEventGroup[Random.Range(0, EventData.BuildingDismentleEventGroup.Count)]);
        CanDismantle = false;
        if (!Moving)
        {
            //重置影响范围
            //effect.RemoveAffect();
            //刷新网格
            foreach (Grid grid in ContainsGrids)
                grid.Dismantle();
        }

        GameControl.Instance.CPC.DebuffEffect(144);
        GameControl.Instance.CPC.DebuffEffect(145);
        //销毁前的准备工作
        if (Department)
        {
            Department.ClearDep();
        }
        BuildingManage.Instance.ConstructedBuildings.Remove(this);
        Destroy(gameObject);

        return true;
    }
}
