using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BSRouteNode : MonoBehaviour
{
    public bool isEnd = false;  //是否为会议结束节点
    public bool isElite = false;//是否为精英节点
    public int NodeType = 1;//0无功能节点 1工作状态（成功率？）  2预算（支出）  3信念  4效率  5休息
    public int NodeLevel = 1;

    public Text Text_Name;
    public GameObject EliteMarker;
    public BrainStormControl BSC;
    public Button SelectButton;

    public List<BSRouteNode> NextNodes = new List<BSRouteNode>(); //之后可解锁的节点
    public List<BSRouteNode> PreNodes = new List<BSRouteNode>();  //之前的节点

    private void Start()
    {        
        //临时用于设定固定节点的名称
        SetNodeName();
        //设定初始可互动节点
        if(NodeType == 0 && isEnd == false)
        {
            foreach (BSRouteNode node in NextNodes)
            {
                node.SelectButton.interactable = true;
            }
            SelectButton.interactable = false;
        }
        foreach (BSRouteNode node in NextNodes)
        {
            node.PreNodes.Add(this);
        }
    }

    //选择一个节点
    public void SelectNode()
    {
        BSC.CurrentNode = this;
        BSC.ShowRouteNotice();
    }

    //确认选择该节点
    public void ConfirmNode()
    {
        BSC.StageCount += 1;
        //先设定按钮状态
        foreach (BSRouteNode node in NextNodes)
        {
            node.SelectButton.interactable = true;
        }
        foreach (BSRouteNode node in PreNodes)
        {
            foreach (BSRouteNode node2 in node.NextNodes)
            {
                node2.SelectButton.interactable = false;
            }
        }
        SelectButton.interactable = false;

        //休息的效果
        if (NodeType == 5)
        {
            BSC.SkillType = -1;
            BSC.StartSelectEmp();
            //休息不刷怪
            return;
        }

        int level = NodeLevel;
        if (NodeLevel == 1)
        {
            level += (GameControl.Instance.Turn - 1) / 10;
            if (level > 5)
                level = 5;
        }
        else if (NodeLevel == 2)
        {
            level += (GameControl.Instance.Turn - 1) / 10;
            if (level > 6)
                level = 6;
        }
        else if (NodeLevel == 3)
        {
            level += (GameControl.Instance.Turn - 1) / 10;
            if (level > 7)
                level = 7;
        }
        BSC.SetBossLevel(level);
    }

    //随机节点类型（暂时不用）
    public void RandomType()
    {
        SetNodeName();
    }
    //根据自身类型设定节点名称
    void SetNodeName()
    {
        InfoPanelTrigger info = this.GetComponent<InfoPanelTrigger>();
        if (isElite == true)
            NodeLevel = 3;
        else
            NodeLevel = Random.Range(1, 3);
        if (NodeType == 1)
        {
            Text_Name.text = NodeLevel + "级成功率";
            info.ContentB = "胜利后获得物品《胜利开发者的全新计划》，使用后可施加“胜利开发”的部门状态，" +
                "持续到下一次头脑风暴，并提升其工作状态1点，可叠加";
        }
        else if (NodeType == 2)
        {
            Text_Name.text = NodeLevel + "级支出";
            info.ContentB = "胜利后获得物品《预算新编》，使用后可施加“节省支出”的部门状态，" +
                "持续到下一次头脑风暴，并降低人员工资10%，可叠加";
        }
        else if (NodeType == 3)
        {
            Text_Name.text = NodeLevel + "级信念";
            info.ContentB = "胜利后获得物品《致全体同事的一封信》，使用后施加“信仰充值”的部门状态，" +
                "持续到下一次头脑风暴，并提升信念5点，可叠加";
        }
        else if (NodeType == 4)
        {
            Text_Name.text = NodeLevel + "级效率";
            info.ContentB = "胜利后获得物品《加班技术大全》，使用后施加“效率至上”的部门状态，" +
                "持续到下一次头脑风暴，并提升效率5%，可叠加";
        }
        else if (NodeType == 5)
        {
            Text_Name.text = "休息";
            info.ContentB = "选择1人吃零食，回复20%心力";
        }

        else if (NodeType == 0)
        {
            if (isEnd == false)
            {
                Text_Name.text = "开始会议";
                info.ContentB = "起始节点";
            }
            else
            {
                Text_Name.text = "结束会议";
                info.ContentB = "结束节点，到达后头脑风暴结束";
            }
        }

        if (isElite == true)
            EliteMarker.SetActive(true);
    }
}
