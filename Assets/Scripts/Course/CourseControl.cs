using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CourseControl : MonoBehaviour
{
    private int MoveDistance = 2;//当前的移动距离
    private int MaxDistance = 3;//最大航行距离
    private int MinDistance = 2;//最小航行距离
    private bool isMoving = false;//当前是否在移动
    private float MovePercent = 0;//移动了百分之多少，用于lerp计算
    private WeatherType CurrentWeather;//当前天气

    public int EventType = 0; //-1回复心力 0去除负面特质 其他是对应的一系列事件
    public int PowerLevel = 1;//当前的动力等级
    public bool canRefesh = true;//是否能够刷新地图（刷新一次之后必须踩到下一个城市才能再次刷新）
    public bool ShipMoved = false;//判断一回合内是否已经进行过移动
    public bool CEOSkillA = false;//不受天气影响
    public bool CEOSkillB = false;//能选择向前或向后移动一次
    public bool FirstRefresh = true;//是否为第一次刷新（刚进图）
    public float MarkerMoveTime = 1f;//玩家图标在节点间移动的时间

    private CourseNode NextNode;

    public Employee TargetEmp;//事件的目标员工
    public GameControl GC;
    public CourseNode CurrentNode;
    public WindowBaseControl ItemPanel;//物品栏引用，用于在航线面板临时显示物品栏面板
    public GameObject PlayerMarker, CourseEndButton, CloseButton, EventPanel, ExtraMovePanel, CasinoButton, BarButton;
    public Transform NodeTrans;
    public Text Text_Power, Text_Weather, Text_EventName, Text_EventDescription;

    public List<CourseNode> NormalNodes = new List<CourseNode>();
    public List<CourseNode> CityNodes = new List<CourseNode>();

    private void Update()
    {
        if (isMoving == true && NextNode != null)
        {
            MovePercent += 1f / MarkerMoveTime * Time.deltaTime;
            if (MovePercent > 1)
                MovePercent = 1;
            PlayerMarker.transform.position = Vector3.Lerp(CurrentNode.transform.position, NextNode.transform.position, MovePercent);
        }
    }

    //开始航行
    public void SetCourse()
    {
        this.gameObject.GetComponent<WindowBaseControl>().SetWndState(true);
        ShowItemPanel();
        Text_Power.gameObject.SetActive(true);
        MoveDistance = MaxDistance;
        UpdatePowerUI();
    }

    //随机一种天气
    private void RandomWeather()
    {
        CurrentWeather = (WeatherType)Random.Range(0, 7);
        Text_Weather.text = "天气:" + CurrentWeather;
        if (CurrentWeather == WeatherType.晴天)
            Text_Weather.text += "\n\n无影响";
        else if (CurrentWeather == WeatherType.大风)
            Text_Weather.text += "\n\n50%几率额外前进1格";
        else if (CurrentWeather == WeatherType.飓风)
            Text_Weather.text += "\n\n50%几率额外前进2格";
        else if (CurrentWeather == WeatherType.巨浪)
            Text_Weather.text += "\n\n40%几率额外前进1格，40%几率额外后退1格";
        else if (CurrentWeather == WeatherType.大雾)
            Text_Weather.text += "\n\n40%几率额外后退2格";
        else if (CurrentWeather == WeatherType.冰冻)
            Text_Weather.text += "\n\n50%几率冰冻，导致无法移动";
        else if (CurrentWeather == WeatherType.暴雨)
            Text_Weather.text += "\n\n50%几率额外后退1格";
    }

    //应用天气效果并准备开始航行
    public void CheckWeather()
    {//该方法绑定在MoveButton上
        Text_Power.gameObject.SetActive(false);
        FirstRefresh = false;
        CasinoButton.SetActive(false);
        BarButton.SetActive(false);
        CloseButton.SetActive(false);
        ShipMoved = true;
        if (CEOSkillA == true)
        {
            CEOSkillA = false;
            QuestControl.Instance.Init("由于CEO技能效果，没有额外移动影响", ShipMove);
            return;
        }
        float posb = Random.Range(0.0f, 1.0f);
        if (CurrentWeather == WeatherType.大风 && posb < 0.5f)
        {
            MoveDistance += 1;
            QuestControl.Instance.Init("天气效果导致额外前进1格", ShipMove);
        }
        else if (CurrentWeather == WeatherType.飓风 && posb < 0.5f)
        {
            MoveDistance += 2;
            QuestControl.Instance.Init("天气效果导致额外前进2格", ShipMove);
        }
        else if (CurrentWeather == WeatherType.巨浪 && posb < 0.4f)
        {
            MoveDistance += 1;
            QuestControl.Instance.Init("天气效果导致额外前进1格", ShipMove);
        }
        else if (CurrentWeather == WeatherType.巨浪 && posb < 0.8f)
        {
            MoveDistance -= 1;
            QuestControl.Instance.Init("天气效果导致额外后退1格", ShipMove);
        }
        else if (CurrentWeather == WeatherType.大雾 && posb < 0.4f)
        {
            MoveDistance -= 2;
            QuestControl.Instance.Init("天气效果导致额外后退2格", ShipMove);
        }
        else if (CurrentWeather == WeatherType.冰冻 && posb < 0.5f)
        {
            MoveDistance = 0;
            QuestControl.Instance.Init("天气效果导致无法移动", ShipMove);
        }
        else if (CurrentWeather == WeatherType.暴雨 && posb < 0.4f)
        {
            MoveDistance -= 1;
            QuestControl.Instance.Init("天气效果导致额外后退1格", ShipMove);
        }
        else
            QuestControl.Instance.Init("没有额外移动影响", ShipMove);
    }

    //执行移动
    public void ShipMove()
    {
        if (MoveDistance > 0)
        {
            if (CurrentNode.NextNodes.Count == 1)
                MoveToNextNode(CurrentNode.NextNodes[0]);
            else
            {
                foreach (CourseNode node in CurrentNode.NextNodes)
                {
                    node.Arrow.SetActive(true);
                    node.NodeSelectButton.interactable = true;
                }
            }
            MoveDistance -= 1;
        }
        else if (MoveDistance < 0)
        {
            if (CurrentNode.PreNodes.Count == 1)
                MoveToNextNode(CurrentNode.PreNodes[0]);
            else
            {
                foreach (CourseNode node in CurrentNode.PreNodes)
                {
                    node.Arrow.SetActive(true);
                    node.NodeSelectButton.interactable = true;
                }
            }
            MoveDistance += 1;
        }
        else
        {
            if (CEOSkillB == false)
                MoveEnd();
            else
                ExtraMovePanel.SetActive(true);
        }
    }

    public void ExtraMove(int value)
    {
        MoveDistance = value;
        CEOSkillB = false;
        ShipMove();
        ExtraMovePanel.SetActive(false);
    }

    //移动至下一个节点
    public void MoveToNextNode(CourseNode node)
    {
        MovePercent = 0;
        NextNode = node;
        isMoving = true;
        StartCoroutine(MoveTimer());
    }

    //到达下一个节点
    private void ArriveNextNode()
    {
        isMoving = false;
        CurrentNode = NextNode;
        NextNode = null;
        CurrentNode.PassByNode();

        if (MoveDistance != 0)
            ShipMove();
        else
        {
            if (CEOSkillB == false)
                MoveEnd();
            else
                ExtraMovePanel.SetActive(true);
        }
    }

    //度过移动时间
    IEnumerator MoveTimer()
    {
        yield return new WaitForSeconds(MarkerMoveTime + 0.1f);
        ArriveNextNode();
    }

    //移动结束，到达了目的地
    public void MoveEnd()
    {
        //到达目的地后检测
        CurrentNode.ActiveNode();
        GC.HC.NodeCheck();
        CourseEndButton.SetActive(true);
        CloseButton.SetActive(true);
        GC.CheckButtonName();
        if (CEOSkillB == true)
            ExtraMovePanel.SetActive(true);
    }

    //直接在航线界面进入下一回合，事件绑定在CourseEndButton上
    public void CourseEnd()
    {
        this.GetComponent<WindowBaseControl>().SetWndState(false);
        ResetItemPanel();
        GC.NextTurn();
        GC.CheckButtonName();
    }

    public void ShowItemPanel()
    {
        ItemPanel.transform.SetParent(UIManager.Instance.DynamicTrans);
    }
    public void ResetItemPanel()
    {
        ItemPanel.transform.SetParent(UIManager.Instance.BottomTrans);
    }

    //根据动力等级设定移动极限距离
    public void SetPower(int value)
    {
        PowerLevel = value;
        if (PowerLevel == 0)
        {
            MaxDistance = 0;
            MinDistance = 0;
        }
        if (PowerLevel == 1)
        {
            MaxDistance = 3;
            MinDistance = 2;       
        }
        else if (PowerLevel == 2)
        {
            MaxDistance = 4;
            MinDistance = 1;
        }
        else if (PowerLevel == 3)
        {
            MaxDistance = 5;
            MinDistance = 0;
        }
        if (MoveDistance > MaxDistance)
            MoveDistance = MaxDistance;
        if (MoveDistance < MinDistance)
            MoveDistance = MinDistance;
        UpdatePowerUI();
    }
    //改变当前动力
    public void ChangePower(bool Add)
    {
        if (Add == true)
            MoveDistance += 1;
        else if (Add == false)
            MoveDistance -= 1;

        if (MoveDistance > MaxDistance)
            MoveDistance = MaxDistance;
        if (MoveDistance < MinDistance)
            MoveDistance = MinDistance;
        UpdatePowerUI();
    }
    //显示动力信息
    private void UpdatePowerUI()
    {
        Text_Power.text = "<size=28>当前动力:</size> <size=38>" + MoveDistance + "</size>\n<size=22>航行范围: "
            + MinDistance + "-" + MaxDistance + "</size>";
    }

    //设置节点引用
    public void InitNodeRef()
    {
        foreach (Transform child in NodeTrans)
        {
            CourseNode node = child.gameObject.GetComponent<CourseNode>();
            if (node == null)
                continue;
            if (node.CityNum == 0)
                NormalNodes.Add(node);
            else
                CityNodes.Add(node);
        }
        GC.TurnEvent.AddListener(TurnPass);
    }

    //初始化各项数值，因为跟招聘有关放在HireControl中
    public void InitDefaultMap()
    {
        SetPower(0);
        RandomWeather();
        CurrentNode.ActiveNode();
        GC.HC.NodeCheck();
    }

    //回合结束
    public void TurnPass()
    {
        foreach(CourseNode node in NormalNodes)
        {
            node.TurnPass();
        }
        foreach (CourseNode node in CityNodes)
        {
            node.TurnPass();
        }
        Text_Power.gameObject.SetActive(true);
        CourseEndButton.SetActive(false);
        ShipMoved = false;
        RandomWeather();
    }

    //刷新走私船、地图的方法
    public void RefreshSmuggler()
    {
        List<CourseNode> AvailableNodes = new List<CourseNode>();
        foreach (CourseNode node in NormalNodes)
        {
            if (node.Type == CourseNodeType.走私船)
                node.RemoveSmugglerInfo();
            if (node.Type == CourseNodeType.无)
                AvailableNodes.Add(node);
        }
        for (int i = 0; i < 3; i++)
        {
            //前两个刷1建筑，最后一个刷2物品
            int num = Random.Range(0, AvailableNodes.Count);
            AvailableNodes[num].Type = CourseNodeType.走私船;
            AvailableNodes[num].SetImage();
            AvailableNodes[num].TurnCount = 5;
            AvailableNodes[num].Text_Turn.text = "5";
            if (i == 0)
            {
                AvailableNodes[num].Text_Name.text = "黑杰克号";
                GC.HC.DepInfos[1].SetInfo(AvailableNodes[num]);
            }
            else if (i == 1)
            {
                AvailableNodes[num].Text_Name.text = "亚瑟号";
                GC.HC.DepInfos[2].SetInfo(AvailableNodes[num]);
            }
            else if (i == 2)
            {
                AvailableNodes[num].Text_Name.text = "莉莉丝号";
                GC.HC.ItemInfos[0].SetInfo(AvailableNodes[num]);
                GC.HC.ItemInfos[1].SetInfo(AvailableNodes[num]);
            }
            AvailableNodes.RemoveAt(num);
        }
    }
    public void RefreshMap()
    {
        //一圈只能刷新一次
        if (canRefesh == false)
            return;

        //获得新卡牌
        if (FirstRefresh == false)
            GC.CWCL.RefreshNewCard();

        foreach (CourseNode node in CityNodes)
        {
            node.Visited = false;
        }
        List<CourseNode> AvailableNodes = new List<CourseNode>();
        foreach (CourseNode node in NormalNodes)
        {
            if (node.Type != CourseNodeType.走私船)
            {
                node.Type = CourseNodeType.无;
                AvailableNodes.Add(node);
                node.SetImage();
            }
        }
        for(int i = 0; i < 11; i++)
        {
            int num = Random.Range(0, AvailableNodes.Count);
            AvailableNodes[num].Type = CourseNodeType.积极事件;
            AvailableNodes[num].SetImage();
            AvailableNodes.RemoveAt(num);
        }
        for (int i = 0; i < 9; i++)
        {
            int num = Random.Range(0, AvailableNodes.Count);
            AvailableNodes[num].Type = CourseNodeType.突发事件;
            AvailableNodes[num].SetImage();
            AvailableNodes.RemoveAt(num);
        }
        for (int i = 0; i < 3; i++)
        {
            int num = Random.Range(0, AvailableNodes.Count);
            AvailableNodes[num].Type = CourseNodeType.环洋游轮;
            AvailableNodes[num].SetImage();
            AvailableNodes.RemoveAt(num);
        }
        for (int i = 0; i < 2; i++)
        {
            int num = Random.Range(0, AvailableNodes.Count);
            AvailableNodes[num].Type = CourseNodeType.Epiphany科考船;
            AvailableNodes[num].SetImage();
            AvailableNodes.RemoveAt(num);
        }

        int numc = Random.Range(0, AvailableNodes.Count);
        AvailableNodes[numc].Type = CourseNodeType.货运浮岛平台;
        AvailableNodes[numc].SetImage();
        GC.HC.DepInfos[0].SetInfo(AvailableNodes[numc]);
        if (Random.Range(0.0f, 1.0f) < 0.5f)
            GC.HC.ItemInfos[2].SetInfo(AvailableNodes[numc]);
        else
            GC.HC.ItemInfos[2].gameObject.SetActive(false);
        AvailableNodes.RemoveAt(numc);
    }

    //突发事件
    public void StartAccidentEvent()
    {//直接生成一个抉择事件
        GC.EC.StartChoiceEvent(EventData.CourseAccidentEvent[Random.Range(0, EventData.CourseAccidentEvent.Count)], 
            GC.CurrentEmployees[Random.Range(0, GC.CurrentEmployees.Count)]);
    }

    //根据事件类型展开面板并显示对应事件文案
    public void CreatePositiveEvent()
    {
        EventType = Random.Range(1, 3);
        if (EventType == 1)
        {
            Text_EventName.text = "好像是鲸鱼";
            Text_EventDescription.text = "文案：总有一些时刻证明，漫长的旅程是值得的！就像此刻，据说已经灭绝的远古巨兽从船底游过，" +
                "不远处喷起高高的水柱。或许你可以派个人去看看！\n操作：选择一名员工，坐小艇去查看\n效果：立刻恢复心力15点，感觉身心都被治愈了。";
        }
        else if (EventType == 2)
        {
            Text_EventName.text = "落水的人";
            Text_EventDescription.text = "文案：海面上泛起水花，远处似乎有个人溺水了。你把对方救了起来，对方说由于分赃不均，自己被走" +
                "私船的同伴扔下了海。为了感谢你的救命之恩，对方送给你了一个U盘，据说是在旧时代的科研中心打捞时偷偷藏起来的东西，" +
                "里面或许藏着先进的知识。\n操作：将U盘送给一名员工\n效果：该员工获得30点经验值";
        }
        EventPanel.SetActive(true);
        CloseButton.SetActive(false);
    }

    //事件的执行（比如需要选人的打开员工面板选择一个员工）
    public void StartPositiveEvent()
    {
        //以后根据事件的不同可能不是>0的条件了，不同事件也许不需要员工
        if (EventType > 0)
        {
            GC.SelectMode = 12;
            GC.Text_EmpSelectTip.text = "选择一个员工参与事件";
            GC.Text_EmpSelectTip.gameObject.SetActive(true);
            GC.TotalEmpPanel.SetWndState(true);
        }
    }

    //执行事件的效果
    public void EventEffect()
    {
        //回复心力
        if (EventType == -1)
        {
            TargetEmp.Mentality += 10000;
            QuestControl.Instance.Init(TargetEmp.Name + "恢复了全部心力");
        }
        //去除随机负面特质
        else if (EventType == 0)
        {
            List<Perk> DebuffList = new List<Perk>();
            foreach (PerkInfo perk in TargetEmp.InfoDetail.PerksInfo)
            {
                int n = perk.CurrentPerk.Num;
                if (n == 52 || n == 53 || n == 54 || n == 114 || n == 115 || n == 116)
                    DebuffList.Add(perk.CurrentPerk);
            }
            if (DebuffList.Count == 0)
            {
                QuestControl.Instance.Init("没有可以去除的负面特质");
                return;
            }
            Perk p = DebuffList[Random.Range(0, DebuffList.Count)];
            QuestControl.Instance.Init("移除了负面特质 " + p.Name);
            p.RemoveEffect();
        }
        //好像是鲸鱼
        else if (EventType == 1)
        {
            TargetEmp.Mentality += 15;
            QuestControl.Instance.Init(TargetEmp.Name + "心力+15");
        }
        //落水的人
        else if (EventType == 2)
        {
            TargetEmp.GainExp(30);
            QuestControl.Instance.Init(TargetEmp.Name + "获得30点经验值");
        }
        EventPanel.SetActive(false);
        CloseButton.SetActive(true);
    }
}

public enum WeatherType
{
    晴天, 大风, 飓风, 巨浪, 大雾, 冰冻, 暴雨
}
