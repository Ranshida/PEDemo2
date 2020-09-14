using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Building : MonoBehaviour
{
    public int Length;
    public int Width;
    public bool BuildingSet { get; private set; }   //设置完毕不再动
    public int effectValue = 1, StaminaRequest = 0; //1人力 2八卦 3强壮 4谋略 5行业 6决策 7财务 8管理
    public BuildingType Type;    //现在是创建时赋值，需改为预制体赋值或子类构造
    private GameObject m_Decoration;   //修饰物，建造后删除

    public List<Grid> ContainsGrids { get; private set; }   //所包含的格子
    public DepControl Department; //BM赋值
    public OfficeControl Office;  //BM赋值
    public BuildingEffect effect;
    public TextMesh Text_DepName;
    public Transform[] WorkPos = new Transform[4];
    public List<BuildingEffect> EffectBuildings = new List<BuildingEffect>();

    private void Awake()
    {
        ContainsGrids = new List<Grid>();
        effect = new BuildingEffect(this);
        m_Decoration = transform.Find("Decoration").gameObject;
        Text_DepName = transform.Find("Description").GetComponent<TextMesh>();
    }

    //确定建造
    public void Build(List<Grid> grids)
    {
        BuildingSet = true;
        foreach (Grid grid in grids)
        {
            ContainsGrids.Add(grid);
            grid.Build(this);
        }
        Destroy(m_Decoration);
    }

    //拆除
    public void Dismantle()
    {
        foreach (Grid grid in ContainsGrids)
        {
            grid.Dismantle();
        }
        //销毁前的准备工作（辐射范围等）
        Destroy(gameObject);
    }

    //public void SetSize(int x, int y)
    //{
    //    Rt.localScale = new Vector3(x, y);
    //    effect.Rt.localScale = new Vector3(x + 8, y + 8);
    //    effect.transform.parent = this.transform;
    //    effect.Rt.anchoredPosition = new Vector2(0, 0);
    //}

    //public void DragStart()
    //{
    //    if (BuildingSet == false)
    //    {
    //        Offset = transform.position - Input.mousePosition;
    //        BuildingManage.Instance.ControlPanel.SetActive(false);
    //    }
    //}

    //public void Drag()
    //{
    //    if (BuildingSet == false)
    //    {
    //        transform.position = Input.mousePosition + Offset;
    //        CheckPos();
    //    }
    //}

    //public void DragEnd()
    //{
    //    if (BuildingSet == false)
    //    {
    //        BuildingManage.Instance.ControlPanel.transform.position = transform.position;
    //        BuildingManage.Instance.ControlPanel.SetActive(true);
    //    }
    //}

    //public void Rotate()
    //{
    //    if (Rotated == false)
    //    {
    //        Rt.rotation = Quaternion.Euler(new Vector3(0, 0, -90));
    //        Rt.pivot = new Vector2(1, 0);
    //        this.gameObject.GetComponent<BoxCollider2D>().offset *= new Vector2(-1, 1);
    //        Text_DepName.gameObject.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(0, 0, 0));
    //        Rotated = true;
    //    }
    //    else
    //    {
    //        Rt.pivot = new Vector2(0, 0);
    //        Rt.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
    //        this.gameObject.GetComponent<BoxCollider2D>().offset *= new Vector2(-1, 1);
    //        Text_DepName.gameObject.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(0, 0, 0));
    //        Rotated = false;
    //    }
    //    CheckPos();
    //}
    //void CheckPos()
    //{
    //    if (Rotated == false)
    //    {
    //        if (Rt.anchoredPosition.x < 0)
    //            Rt.anchoredPosition = new Vector2(0, Rt.anchoredPosition.y);
    //        else if (Rt.anchoredPosition.x > 1120 - Rt.localScale.x * 80)
    //            Rt.anchoredPosition = new Vector2(1120 - Rt.localScale.x * 80, Rt.anchoredPosition.y);
    //        if (Rt.anchoredPosition.y < 0)
    //            Rt.anchoredPosition = new Vector2(Rt.anchoredPosition.x, 0);
    //        else if (Rt.anchoredPosition.y > 800 - Rt.localScale.y * 80)
    //            Rt.anchoredPosition = new Vector2(Rt.anchoredPosition.x, 800 - Rt.localScale.y * 80);
    //    }
    //    else
    //    {
    //        if (Rt.anchoredPosition.x < 0)
    //            Rt.anchoredPosition = new Vector2(0, Rt.anchoredPosition.y);
    //        else if (Rt.anchoredPosition.x > 1120 - Rt.localScale.y * 80)
    //            Rt.anchoredPosition = new Vector2(1120 - Rt.localScale.y * 80, Rt.anchoredPosition.y);
    //        if (Rt.anchoredPosition.y < 0)
    //            Rt.anchoredPosition = new Vector2(Rt.anchoredPosition.x, 0);
    //        else if (Rt.anchoredPosition.y > 800 - Rt.localScale.x * 80)
    //            Rt.anchoredPosition = new Vector2(Rt.anchoredPosition.x, 800 - Rt.localScale.x * 80);
    //    }

    //    XPos = (int)(Rt.anchoredPosition.x / 80);
    //    YPos = (int)(Rt.anchoredPosition.y / 80);

    //    Rt.anchoredPosition = new Vector2(XPos * 80, YPos * 80);
    //}

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if(collision.gameObject.tag == "BuildingEffect" && BuildingSet == false && collision.gameObject != effect.gameObject)
    //    {
    //        EffectBuildings.Add(collision.gameObject.GetComponent<BuildingEffect>());
    //    }
    //}

    //private void OnTriggerStay2D(Collider2D collision)
    //{
    //    if (collision.gameObject.tag == "Building" || collision.gameObject.tag == "BuildingBorder")
    //    {
    //        BM.ConfirmButton.interactable = false;
    //    }
    //}

    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (collision.gameObject.tag == "Building" || collision.gameObject.tag == "BuildingBorder")
    //        BM.ConfirmButton.interactable = true;

    //    if(collision.gameObject.tag == "BuildingEffect" && BuildingSet == false && collision.gameObject != effect.gameObject)
    //    {
    //        EffectBuildings.Remove(collision.gameObject.GetComponent<BuildingEffect>());
    //    }
    //}
}
