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
    public string Function_B;
    public string Require_B;
    public string Description_B;
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
    public int effectValue = 1, StaminaRequest = 0; //1人力 2八卦 3强壮 4谋略 5行业 6决策 7财务 8管理
    public BuildingType Type;    //现在是创建时赋值，需改为预制体赋值或子类构造
    private Transform m_Decoration;   //修饰物，建造后删除
    public int Pay;               //维护费用
    public bool BasicBuilding;    //基础建筑物，可以直接建造

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
        Size = value[ID, 5];
        EffectRange_str = value[ID, 6];
        Jobs = value[ID, 4];
        Function_A = value[ID, 11];
        Require_A = value[ID, 13];
        Description_A = value[ID, 14];
        Function_B = value[ID, 16];
        Require_B = value[ID, 18];
        Description_B = value[ID, 19];
        Function_C = value[ID, 21];
        Require_C = value[ID, 23];
        Description_C = value[ID, 24];

        Type = (BuildingType)System.Enum.Parse(typeof(BuildingType), Name);
        string[] size = Size.Split('×');
        Length = int.Parse(size[0]);
        Width = int.Parse(size[1]);
        Pay = 100;
        BasicBuilding = value[ID, 2] == "基础建筑物";
        if (!int.TryParse(EffectRange_str, out EffectRange))
        {
            if (EffectRange_str == "/")
                EffectRange = 0;
            if (EffectRange_str == "全公司")
                EffectRange = 999;
        }
    }
    
    //确定建造
    public void Build(List<Grid> grids)
    {
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
    public void Dismantle()
    {
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
    }
}
