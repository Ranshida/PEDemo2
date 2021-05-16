using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventControl : MonoBehaviour
{
    public int ExhaustedCount = 0;
    public int EventGroupIndex = 0; //当前处理到的事件组编号
    public bool StartEventQueue = false;//是否已经开始处理事件（心力爆炸判定后是否开始事件流程）

    public GameObject ManageVotePanel;
    public GameControl GC;
    public Transform ManageVoteContent, EventGroupContent;
    public VoteCell VoteCellPrefab;
    public Text Text_MeetingName, Text_ManageVoteResult;
    public ChoiceEvent ChoiceEventPrefab;
    public EventGroupInfo EventGroupPrefab, CurrentEventGroup;

    private List<VoteCell> VCells = new List<VoteCell>();
    public List<ChoiceEvent> UnfinishedEvents = new List<ChoiceEvent>();
    public List<EventGroupInfo> CurrentEventGroups = new List<EventGroupInfo>();

    private void Start()
    {
        if (GC == null)
            GC = GameControl.Instance;
        CreateEventGroup(EventData.SpecialEventGroups[0]);
    }

    //换高管投票检测
    public bool ManagerVoteCheck(Employee emp, bool Dismissal = false, bool Fire = false)
    {
        //CEO不检测
        if (emp.isCEO == true)
            return true;

        int VoteNum = 1;
        int AgreeNum = 1;
        if (GC.CEOExtraVote == true)
            AgreeNum += 1;
        ManageVotePanel.GetComponent<WindowBaseControl>().SetWndState(true);
        for(int i = 0; i < VCells.Count; i++)
        {
            Destroy(VCells[i].gameObject);
        }
        VCells.Clear();

        if (Dismissal == true)
            Text_MeetingName.text = "解除" + emp.Name + "高管职务会议投票结果";
        else
            Text_MeetingName.text = "任命" + emp.Name + "高管职务会议投票结果";
        if (Fire == true)
            Text_MeetingName.text = "开除员工" + emp.Name + "会议投票结果";

        foreach (EmpBSInfo info in GC.BSC.EmpSelectInfos)
        {
            if (info.emp == null)
                continue;

            Employee E = info.emp, CEO = GC.CurrentEmployees[0];
            //CEO和自己不投票
            if (E.isCEO == true || E == emp)
                continue;
            VoteCell V = Instantiate(VoteCellPrefab, ManageVoteContent);
            int VoteValue = 0;
            string Description = "和目标关系", Description2 = "和CEO关系";
            V.Text_Name.text = E.Name;
            VCells.Add(V);
            //和目标的关系
            Relation r = E.FindRelation(emp);
            //友情关系检测
            if (r.FriendValue == -2)
            {
                if (Dismissal == false)
                {
                    VoteValue -= 4;
                    Description += "\n仇敌-4";
                }
                else
                {
                    VoteValue += 4;
                    Description += "\n仇敌+4";
                }
            }
            else if (r.FriendValue == -1)
            {
                if (Dismissal == false)
                {
                    VoteValue -= 2;
                    Description += "\n陌路-2";
                }
                else
                {
                    VoteValue += 2;
                    Description += "\n陌路+2";
                }
            }
            else if (r.FriendValue == 1)
            {
                if (Dismissal == false)
                {
                    VoteValue += 1;
                    Description += "\n朋友+1";
                }
                else
                {
                    VoteValue -= 1;
                    Description += "\n朋友-1";
                }
            }
            else if (r.FriendValue == 2)
            {
                if (Dismissal == false)
                {
                    VoteValue += 3;
                    Description += "\n挚友+3";
                }
                else
                {
                    VoteValue -= 3;
                    Description += "\n挚友-3";
                }
            }
            //恋爱关系检测
            if (r.LoveValue == 1)
            {
                if (Dismissal == false)
                {
                    VoteValue += 1;
                    Description += "\n倾慕+1";
                }
                else
                {
                    VoteValue -= 1;
                    Description += "\n倾慕-1";
                }
            }
            else if (r.LoveValue == 2)
            {
                if (Dismissal == false)
                {
                    VoteValue -= 1;
                    Description += "\n追求者-1";
                }
                else
                {
                    VoteValue += 1;
                    Description += "\n追求者+1";
                }
            }
            else if (r.LoveValue == 3)
            {
                if (Dismissal == false)
                {
                    VoteValue += 2;
                    Description += "\n情侣+2";
                }
                else
                {
                    VoteValue -= 2;
                    Description += "\n情侣-2";
                }
            }
            else if (r.LoveValue == 4)
            {
                if (Dismissal == false)
                {
                    VoteValue += 3;
                    Description += "\n情侣+3";
                }
                else
                {
                    VoteValue -= 3;
                    Description += "\n情侣-3";
                }
            }
            //师徒关系检测 (可能方向有问题)
            if (r.MasterValue == 1)
            {
                if (Dismissal == false)
                {
                    VoteValue += 1;
                    Description += "\n门徒+1";
                }
                else
                {
                    VoteValue -= 1;
                    Description += "\n门徒-1";
                }
            }
            else if (r.MasterValue == 2)
            {
                if (Dismissal == false)
                {
                    VoteValue += 2;
                    Description += "\n师承+2";
                }
                else
                {
                    VoteValue -= 2;
                    Description += "\n师承-2";
                }
            }
            //派系检测
            if (E.CurrentClique != null && E.CurrentClique.Members.Contains(emp) == true)
            {
                if (Dismissal == false)
                {
                    VoteValue += 3;
                    Description += "\n同派系+3";
                }
                else
                {
                    VoteValue -= 3;
                    Description += "\n同派系-3";
                }
            }
            //文化检测
            if (E.CharacterTendency[0] * emp.CharacterTendency[0] == 1)
            {
                if (Dismissal == false)
                {
                    VoteValue += 1;
                    Description += "\n文化一致+1";
                }
                else
                {
                    VoteValue -= 1;
                    Description += "\n文化一致-1";
                }
            }
            else if (E.CharacterTendency[0] * emp.CharacterTendency[0] == -1)
            {
                if (Dismissal == false)
                {
                    VoteValue -= 2;
                    Description += "\n文化相反-2";
                }
                else
                {
                    VoteValue += 2;
                    Description += "\n文化相反+2";
                }
            }
            //信仰检测
            if (E.CharacterTendency[1] * emp.CharacterTendency[1] == 1)
            {
                if (Dismissal == false)
                {
                    VoteValue += 1;
                    Description += "\n信仰一致+1";
                }
                else
                {
                    VoteValue -= 1;
                    Description += "\n信仰一致-1";
                }
            }
            else if (E.CharacterTendency[1] * emp.CharacterTendency[1] == -1)
            {
                if (Dismissal == false)
                {
                    VoteValue -= 2;
                    Description += "\n信仰相反-2";
                }
                else
                {
                    VoteValue += 2;
                    Description += "\n信仰相反+2";
                }
            }

            //和CEO的关系
            r = E.FindRelation(CEO);
            //友情关系检测
            if (r.FriendValue == -2)
            {
                VoteValue -= 4;
                Description2 += "\n仇敌-4";
            }
            else if (r.FriendValue == -1)
            {
                VoteValue -= 2;
                Description2 += "\n陌路-2";
            }
            else if (r.FriendValue == 1)
            {
                VoteValue += 1;
                Description2 += "\n朋友+1";
            }
            else if (r.FriendValue == 2)
            {
                VoteValue += 3;
                Description2 += "\n挚友+3";
            }
            //恋爱关系检测
            if (r.LoveValue == 1)
            {
                VoteValue += 1;
                Description2 += "\n倾慕+1";
            }
            else if (r.LoveValue == 2)
            {
                VoteValue -= 1;
                Description2 += "\n追求者-1";
            }
            else if (r.LoveValue == 3)
            {
                VoteValue += 2;
                Description2 += "\n情侣+2";
            }
            else if (r.LoveValue == 4)
            {
                VoteValue += 3;
                Description2 += "\n情侣+3";
            }
            //师徒关系检测 (可能方向有问题)
            if (r.MasterValue == 1)
            {
                VoteValue += 1;
                Description2 += "\n门徒+1";
            }
            else if (r.MasterValue == 2)
            {
                VoteValue += 2;
                Description2 += "\n师承+2";
            }
            //派系检测
            if (E.CurrentClique != null && E.CurrentClique.Members.Contains(CEO) == true)
            {
                VoteValue += 3;
                Description2 += "\n同派系+3";
            }
            //文化检测
            if (E.CharacterTendency[0] * emp.CharacterTendency[0] == 1)
            {
                VoteValue += 1;
                Description2 += "\n文化一致+1";
            }
            else if (E.CharacterTendency[0] * emp.CharacterTendency[0] == -1)
            {
                VoteValue -= 2;
                Description2 += "\n文化相反-2";
            }
            //信仰检测
            if (E.CharacterTendency[1] * emp.CharacterTendency[1] == 1)
            {
                VoteValue += 1;
                Description2 += "\n信仰一致+1";
            }
            else if (E.CharacterTendency[0] * emp.CharacterTendency[0] == -1)
            {
                VoteValue -= 2;
                Description2 += "\n信仰相反-2";
            }

            //支持判定
            if (E.ObeyTime > 0)
            {
                E.ObeyTime = 0;
                VoteValue += 50;
                Description2 += "\n支持+50";
            }

            //计算技能影响（分就职和离职两种）
            int ExtraValue = ExtraValue = (int)(emp.Manage * 0.1f); ;
            if (Dismissal == true)
            {
                VoteValue += ExtraValue;
                Description += "\n管理技能+" + ExtraValue;
            }
            else
            {
                VoteValue -= ExtraValue;
                Description += "\n管理技能-" + ExtraValue;
            }

            //确定结果
            VoteNum += 1;
            if (VoteValue >= 0)
            {
                V.image.color = Color.green;
                V.Text_Value.text = "+" + VoteValue;
                AgreeNum += 1;
            }
            else
            {
                V.image.color = Color.red;
                V.Text_Value.text = "-" + Mathf.Abs(VoteValue);
            }
            V.Text_Vote.text = Description;
            V.Text_Vote2.text = Description2;

        }

        if ((AgreeNum * 2) >= VoteNum)
        {
            Text_ManageVoteResult.text = "通过";
            return true;
        }
        else
        {
            Text_ManageVoteResult.text = "未通过";
            return false;
        }
    }

    //生成一个抉择事件
    public void StartChoiceEvent(Event e, Employee self, EventGroupInfo egi = null)
    {
        ChoiceEvent c = Instantiate(ChoiceEventPrefab, GC.transform);
        UIManager.Instance.OnAddNewWindow(c.GetComponent<WindowBaseControl>());
        c.EC = this;
        c.CurrentEvent = e;
        c.SetEventInfo(e, self, egi);
        UnfinishedEvents.Add(c);
    }

    //生成一个事件组
    public void CreateEventGroup(EventGroup e)
    {
        EventGroupInfo newEventGroup = Instantiate(EventGroupPrefab, GC.transform);
        newEventGroup.EC = this;
        newEventGroup.SetEvent(e);
        CurrentEventGroups.Add(newEventGroup);
        UIManager.Instance.OnAddNewWindow(newEventGroup.DetailPanel.GetComponent<WindowBaseControl>());
        newEventGroup.transform.parent = EventGroupContent;
        newEventGroup.UpdateUI();
    }

    //判断是否能够进入特殊事件
    public void StartSpecialEvent()
    {
        //有未处理的心态爆炸则必须先处理，没有进入事件流程时不继续
        if (ExhaustedCount != 0 || StartEventQueue == false)
            return;

        //此处可能要先重置一下各种数据？
        //此处开始，先弹出第一个事件组的特殊事件
        if (EventGroupIndex < CurrentEventGroups.Count)
        {
            CurrentEventGroups[EventGroupIndex].StartGroupEvent();
        }
        //处理所有已经完成的事件组
        else
        {
            List<EventGroupInfo> FinishList = new List<EventGroupInfo>();
            foreach(EventGroupInfo egi in CurrentEventGroups)
            {
                if (egi.Stage > egi.TargetEventGroup.StageCount)
                    FinishList.Add(egi);
            }
            foreach(EventGroupInfo egi in FinishList)
            {
                CurrentEventGroups.Remove(egi);
                Destroy(egi.DetailPanel.gameObject);
                Destroy(egi.gameObject);
            }
            FinishList.Clear();
        }
    }

    //判断是否完成了抉择事件并进入一般事件流程
    public void ChoiceEventCheck(bool groupEvent)
    {
        //判断是否为事件组事件
        //不是的话看一下所有一般抉择事件是否都完成，完成后进入下一阶段(情绪回合-1、结算不满/认同、结算一般事件)
        if (groupEvent == false)
        {
            if (UnfinishedEvents.Count == 0)
            {
                foreach(Employee e in GC.CurrentEmployees)
                {
                    e.EmotionTimePass();
                }
                GC.CheckEventProgress();
                foreach (Employee e in GC.CurrentEmployees)
                {
                    EmpManager.Instance.AddEvent(e);
                }
                StartEventQueue = false;
            }
        }

        //是的话看一下是否遍历了所有事件组事件，是的话删除所有已经完成的事件组并进入下一阶段
        else
        {
            EventGroupIndex += 1;
            StartSpecialEvent();
        }
    }
}
