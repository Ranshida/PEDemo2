using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Building : MonoBehaviour
{
    public int ID;                             //见建筑物Excel中的行数
    public string Name;   
    public string Size;                        //尺寸str（a×b）
    public string Function_A;                  //建筑的功能1
    public string Require_A;                   //技能需求
    public string Description_A;               //建筑功能1的描述
    public string Time_A;                      //建筑功能1的基础生产周期
    public string Result_A;                    //建筑功能1具体效果描述
    public string Function_B;
    public string Require_B;
    public string Description_B;
    public string Time_B;
    public string Result_B;
    public string Function_C;
    public string Require_C;
    public string Description_C;


    public string EffectRange_str;                        //影响范围str
    public string Jobs;                             //岗位数量

    public int Length;
    public int Width;
    public int EffectRange;
    public bool BuildingSet { get; private set; } = false;   //设置完毕不再动
    public bool Moving { get; private set; } = false;        //移动中
    public int effectValue = 1;//1技术 2市场 3产品 4观察 5坚韧 6强壮 7管理 8人力 9财务 10决策 11行业 12谋略 13说服 14魅力 15八卦
    public int StaminaRequest = 0;
    public BuildingType Type;    //现在是创建时赋值，需改为预制体赋值或子类构造
    public string Str_Type;
    private Transform m_Decoration;   //修饰物，建造后删除
    public int Pay;               //维护费用
    public bool CanLottery;    //基础建筑物，可以直接建造

    public bool CanDismantle = true;
    public List<Grid> ContainsGrids;   //所包含的格子
    public DepControl Department; //BM赋值
    public OfficeControl Office;  //BM赋值
    public BuildingEffect effect;
    public List<Transform> WorkPos;
    public List<BuildingEffect> EffectBuildings = new List<BuildingEffect>();

    //BUildingManager加载此建筑预制体
    public void LoadPrefab(string[,] value)
    {
        WorkPos = Function.ReturnChildList(transform.Find("WorkPosition"));
        m_Decoration = transform.Find("Decoration");

        if (ID < 3)
            return;
        Name = value[ID, 1];
        Str_Type = value[ID, 2];
        Size = value[ID, 5];
        EffectRange_str = value[ID, 6];
        Jobs = value[ID, 4];
        Function_A = value[ID, 11];
        Require_A = value[ID, 13];
        Description_A = value[ID, 14];
        Time_A = value[ID, 12];
        Result_A = value[ID, 15];
        Function_B = value[ID, 16];
        Require_B = value[ID, 18];
        Description_B = value[ID, 19];
        Time_B = value[ID, 17];
        Result_B = value[ID, 20];
        Function_C = value[ID, 21];
        Require_C = value[ID, 23];
        Description_C = value[ID, 24];

        Type = (BuildingType)System.Enum.Parse(typeof(BuildingType), Name);
        string[] size = Size.Split('×');
        Length = int.Parse(size[0]);
        Width = int.Parse(size[1]);
        Pay = 10;
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
        CanDismantle = false;
        BuildingSet = true;
        Moving = false;
        ContainsGrids = new List<Grid>();
        foreach (Grid grid in grids)
        {
            ContainsGrids.Add(grid);
            grid.Build(this);
        }

        effect = new BuildingEffect(this);

        Destroy(m_Decoration);
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
        if (Office)
        {
            Office.ClearOffice();
        }

        Destroy(gameObject);

        return true;
    }
}
