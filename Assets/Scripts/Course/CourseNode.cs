using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CourseNodeType
{
    突发事件, 积极事件, Epiphany科考船, 货运浮岛平台, 环洋游轮, 走私船, 无
}

public class CourseNode : MonoBehaviour
{
    public int CityNum = 0;//没有所属是0 1-7是A-G城
    public int TurnCount = 0;//特殊回合事件限制，用于员工招聘和走私船的限时
    public bool HaveBar;//是否为酒馆格
    public bool HaveCasino;//是否为赌场格
    public bool RefreshNode;//是否为刷新格
    public bool Visited = false;//用于在城市格上判断一轮内是否已经造访过
    public bool HaveEmp = false;//城市内是否有员工

    public CourseNodeType Type = CourseNodeType.无;
    public CourseControl CC;
    public GameObject Arrow;
    public Text Text_Name, Text_Turn, Text_EmpTurn, Text_City;
    public Image TypeImage;
    public Button NodeSelectButton;
    public Sprite Accident, PositiveEvent, SmugglerShip, CruiseShip, ResearchShip, CargoShip; 

    public List<CourseNode> NextNodes = new List<CourseNode>();
    public List<CourseNode> PreNodes = new List<CourseNode>();
    public List<CourseNode> CityNeighborNodes = new List<CourseNode>();

    private void Start()
    {
        //根据前一个节点的引用
        foreach (CourseNode nodes in NextNodes)
        {
            nodes.PreNodes.Add(this);
        }
    }

    public void SelectNode()
    {
        //本方法绑定在Node预制体按钮上
        foreach (CourseNode node in CC.CurrentNode.NextNodes)
        {
            node.NodeSelectButton.interactable = false;
            node.Arrow.SetActive(false);
        }
        foreach (CourseNode node in CC.CurrentNode.PreNodes)
        {
            node.NodeSelectButton.interactable = false;
            node.Arrow.SetActive(false);
        }
        CC.MoveToNextNode(this);
    }

    public void SetImage()
    {
        if (Type == CourseNodeType.无)
            TypeImage.gameObject.SetActive(false);
        else
        {
            TypeImage.gameObject.SetActive(true);
            if (Type == CourseNodeType.Epiphany科考船)
                TypeImage.sprite = ResearchShip;
            else if (Type == CourseNodeType.环洋游轮)
                TypeImage.sprite = CruiseShip;
            else if (Type == CourseNodeType.积极事件)
                TypeImage.sprite = PositiveEvent;
            else if (Type == CourseNodeType.突发事件)
                TypeImage.sprite = Accident;
            else if (Type == CourseNodeType.货运浮岛平台)
                TypeImage.sprite = CargoShip;
            else if (Type == CourseNodeType.走私船)
                TypeImage.sprite = SmugglerShip;
        }
    }

    //设置员工招募相关信息
    public void SetEmpInfo()
    {
        TurnCount = 5;
        if (CityNeighborNodes.Count == 0)
        {
            Text_EmpTurn.gameObject.SetActive(true);
            Text_EmpTurn.text = "5";
        }
        else
        {
            CityNeighborNodes[0].Text_EmpTurn.gameObject.SetActive(true);
            CityNeighborNodes[0].Text_EmpTurn.text = "5";
        }
        HaveEmp = true;
    }
    //移除员工图标
    public void RemoveEmpInfo()
    {
        TurnCount = 0;
        if (CityNeighborNodes.Count == 0)
        {
            Text_EmpTurn.gameObject.SetActive(false);
            Text_EmpTurn.text = "";
        }
        else
        {
            CityNeighborNodes[0].Text_EmpTurn.gameObject.SetActive(false);
            CityNeighborNodes[0].Text_EmpTurn.text = "";
        }
        HaveEmp = false;
    }

    public void RemoveSmugglerInfo()
    {
        Type = CourseNodeType.无;
        SetImage();
        Text_Turn.text = "";
        Text_Name.text = "";
        foreach (DepInfo info in CC.GC.HC.DepInfos)
        {
            if (info.CurrentNode == this)
                info.gameObject.SetActive(false);
        }
        foreach (CItemPurchaseInfo info in CC.GC.HC.ItemInfos)
        {
            if (info.CurrentNode == this)
                info.gameObject.SetActive(false);
        }
        CC.GC.HC.NodeCheck();
        TurnCount = 0;
    }

    //回合结束后节点限时效果的结算
    public void TurnPass()
    {
        TurnCount -= 1;
        if (HaveEmp == true)
        {
            if (TurnCount == 0)
            {
                if (CityNeighborNodes.Count == 0)
                    Text_EmpTurn.gameObject.SetActive(false);
                else
                    CityNeighborNodes[0].Text_EmpTurn.gameObject.SetActive(false);
                foreach(EmpInfo info in CC.GC.HC.HireInfos)
                {
                    info.gameObject.SetActive(false);
                }
                HaveEmp = false;
            }
            else
            {
                if (CityNeighborNodes.Count == 0)
                    Text_EmpTurn.text = TurnCount.ToString();
                else
                    CityNeighborNodes[0].Text_EmpTurn.text = TurnCount.ToString();
            }

        }
        else if (Type == CourseNodeType.走私船)
        {
            if (TurnCount == 0)
                RemoveSmugglerInfo();
            else
                Text_Turn.text = TurnCount.ToString();
        }
    }

    //踩到了格子后激活效果
    public void ActiveNode()
    {
        //非城市格
        if (CityNum == 0)
        {
            if (Type == CourseNodeType.Epiphany科考船)
            {
                CC.GC.SelectMode = 12;
                CC.EventType = 0;
                CC.GC.Text_EmpSelectTip.text = "选择一个员工去除一个随机负面特质";
                CC.GC.Text_EmpSelectTip.gameObject.SetActive(true);
                foreach (Employee emp in CC.GC.CurrentEmployees)
                {
                    bool HaveDebuffPerk = false;
                    foreach (PerkInfo perk in emp.InfoDetail.PerksInfo)
                    {
                        int n = perk.CurrentPerk.Num;
                        if (n == 52 || n == 53 || n == 54 || n == 114 || n == 115 || n == 116)
                        {
                            HaveDebuffPerk = true;
                            break;
                        }
                    }
                    if (HaveDebuffPerk == false)
                        emp.InfoB.gameObject.SetActive(false);
                }
                CC.GC.TotalEmpPanel.SetWndState(true);
                Type = CourseNodeType.无;
                SetImage();
            }
            else if (Type == CourseNodeType.环洋游轮)
            {
                CC.GC.SelectMode = 12;
                CC.EventType = -1;
                CC.GC.Text_EmpSelectTip.text = "选择一个员工完全恢复心力";
                CC.GC.Text_EmpSelectTip.gameObject.SetActive(true);
                CC.GC.TotalEmpPanel.SetWndState(true);
                Type = CourseNodeType.无;
                SetImage();
            }
            else if (Type == CourseNodeType.积极事件)
            {
                CC.CreatePositiveEvent();
                Type = CourseNodeType.无;
                SetImage();
            }
            else if (Type == CourseNodeType.突发事件)
            {
                CC.StartAccidentEvent();
                Type = CourseNodeType.无;
                SetImage();
            }
        }
        else
        {
            if (RefreshNode == true)
            {
                CC.RefreshMap();
                CC.canRefesh = false;
            }
            //如果已经被拜访过就不产生任何效果
            if (Visited == false)
            {
                if (HaveBar == true)
                    CC.GC.HC.Refresh();
                if (HaveCasino == true)
                    CC.RefreshSmuggler();
                Visited = true;
            }
        }
    }

    //路过格子后激活的效果
    public void PassByNode()
    {
        if (CityNum != 7 && CityNum != 0)
            CC.canRefesh = true;
    }
}
