using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Building : MonoBehaviour
{
    public int Length;
    public int Width;
    public bool BuildingSet { get; private set; } = false;   //设置完毕不再动
    public bool Moving { get; private set; } = false;        //移动中
    public int effectValue = 1, StaminaRequest = 0; //1人力 2八卦 3强壮 4谋略 5行业 6决策 7财务 8管理
    public BuildingType Type;    //现在是创建时赋值，需改为预制体赋值或子类构造
    private GameObject m_Decoration;   //修饰物，建造后删除

    public List<Grid> ContainsGrids { get; private set; }   //所包含的格子
    public DepControl Department; //BM赋值
    public OfficeControl Office;  //BM赋值
    public BuildingEffect effect;
    public TextMesh Text_DepName;
    public List<Transform> WorkPos;
    public List<BuildingEffect> EffectBuildings = new List<BuildingEffect>();

    private void Awake()
    {
        ContainsGrids = new List<Grid>();
        effect = new BuildingEffect(this);
        WorkPos = new List<Transform>();
        WorkPos = Function.ReturnChildList(transform.Find("WorkPosition"));
        m_Decoration = transform.Find("Decoration").gameObject;
        Text_DepName = transform.Find("Description").GetComponent<TextMesh>();
    }

    //确定建造
    public void Build(List<Grid> grids)
    {
        BuildingSet = true;
        Moving = false;
        foreach (Grid grid in grids)
        {
            ContainsGrids.Add(grid);
            grid.Build(this);
        }
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
        //重置影响范围
        effect.RemoveAffect();
        //刷新网格
        foreach (Grid grid in ContainsGrids)
            grid.Dismantle();

        //销毁前的准备工作
        if (Department)
        {

        }
        if (Office)
        {

        }

        Destroy(gameObject);
    }
}
