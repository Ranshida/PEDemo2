using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StrategyControl : MonoBehaviour
{

    public int TimeLeft = 0, CEOVote = 1;

    public int[] CurrentStrNum = new int[5];
    public int[] StrLimitNum = new int[5];
    public int[] BlockTime = new int[3];

    public StrategyInfo InfoPrefabA, InfoPrefabC, CurrentStrategy, ChargePrefab, VotePrefab, ActivePrefab;//前四个是旧版Prefab
    public GameObject NewStrPanel, VotePanel, SkillConfirmButton;
    public Transform StrategyContent, UnfinishedStrategyContent, VoteContent, ActiveStrsContent;
    public GameControl GC;
    public Button NewStrButton, MeetingButton;
    public Text Text_Time;

    public List<StrategyInfo> StrInfos = new List<StrategyInfo>(), VoteStrs = new List<StrategyInfo>(), ActiveStrs = new List<StrategyInfo>()
        , VoteSelectStrs = new List<StrategyInfo>();
    public StrategyInfo[] NewStrs = new StrategyInfo[3];
    public StrategyInfo[] CurrentStrs = new StrategyInfo[3];
    public Text[] Texts_Strnum = new Text[5];
    public Text[] Texts_BlockTime = new Text[3];


    bool canChangeCulture = true;
    int VoteSeqNum = 0, MeetingSkillNum = 0, StrSelectNum = 0, FinishedStrNum = 0;
    List<Strategy> TempStrs = new List<Strategy>();
    List<Employee> Managers = new List<Employee>();

    private void Start()
    {
        UpdateUI();
        GC.HourEvent.AddListener(TimePass);
    }

    public void RefreshNewStrs()
    {
        if(GC.CurrentEmployees.Count > 0)
        {
            for(int i = 0; i < 3; i++)
            {
                EmpInfo e = GC.CurrentEmployees[Random.Range(0, GC.CurrentEmployees.Count)].InfoDetail;
                NewStrs[i].Str = e.StrategiesInfo[Random.Range(0, 3)].Str;
                NewStrs[i].UpdateUI();
                NewStrs[i].UseButton.interactable = true;
            }
            NewStrPanel.SetActive(true);
        }
    }

    public void UpdateUI()
    {
        for(int i = 0; i < 5; i++)
        {
            //Texts_Strnum[i].text = "已用:" + CurrentStrNum[i] + "可用:" + StrLimitNum[i];
            Texts_Strnum[i].text = "槽位:" + StrLimitNum[i];
        }
        Text_Time.text = "距离下次会议召开还剩:" + TimeLeft + "时";
    }

    public void CheckStrNum()
    {
        List<StrategyInfo> RemoveInfos = new List<StrategyInfo>();
        for (int i = 0; i < 5; i++)
        {
            if(CurrentStrNum[i] > StrLimitNum[i])
            {
                int num = CurrentStrNum[i] - StrLimitNum[i];
                for(int j = 0; j < StrInfos.Count; j++)
                {
                    if((int)StrInfos[j].Str.Type == i)
                    {
                        num -= 1;
                        RemoveInfos.Add(StrInfos[j]);
                    }
                    if (num == 0)
                        break;
                }
            }
        }
        for(int i = 0; i < RemoveInfos.Count; i++)
        {
            RemoveInfos[i].ToggleUsage();
        }
        RemoveInfos.Clear();
    }

    public void NewStrategyConfirm()
    {
        for(int i = 0; i < 3; i++)
        {
            if (NewStrs[i].Active == false)
            {
                if (CurrentStrs[i] != null)
                    Destroy(CurrentStrs[i].gameObject);
                CurrentStrs[i] = NewStrs[i].SelectStrategy();
                CurrentStrs[i].StrNum = i;
            }
        }
        TimeLeft = 96;
        NewStrButton.interactable = false;
    }

    public void StrategyFail()
    {
        if (GC.Money > 0)
            GC.Money -= (int)(GC.Money * 0.2f);
    }

    //战略-开会相关部分
    //开始开会投票
    public void InitVote()
    {
        Managers.Clear();
        foreach(Employee emp in GC.CurrentEmployees)
        {
            if (emp.CurrentDivision != null)
                Managers.Add(emp);
        }
        canChangeCulture = true;
        CultureStrCheck();
        if (GC.MeetingBlockTime == 0)
            StartVote();
    }
    void StartVote()
    {
        if (canChangeCulture == true)
        {
            VoteSeqNum = 3;
            canChangeCulture = false;
        }

        if (StrLimitNum[VoteSeqNum] > 0)
        {
            int typenum = 0;
            //依次生成
            for (int i = 0; i < StrInfos.Count; i++)
            {
                if (VoteSeqNum == (int)StrInfos[i].Str.Type)
                {
                    //还要写一些选择相关的功能
                    StrategyInfo newStr = Instantiate(VotePrefab, VoteContent);
                    newStr.SC = this;
                    newStr.Str = StrInfos[i].Str;
                    newStr.TargetStr = StrInfos[i];
                    newStr.UpdateUI();
                    VoteStrs.Add(newStr);
                    typenum += 1;
                }
            }
            ManagerVote();
            if (typenum == 0)
                NextVote();
            else
                CheckVoteStatus();
        }
        else
            NextVote();
        
    }
    //下一轮开会投票
    public void NextVote()
    {
        CheckVoteStatus(true);
        SkillConfirmButton.SetActive(false);
        for (int i = 0; i < VoteStrs.Count; i++)
        {
            if (VoteStrs[i].Active == true)
            {
                TempStrs.Add(VoteStrs[i].Str);
                StrInfos.Remove(VoteStrs[i].TargetStr);//在待激活面板中删除对应战略
            }
            Destroy(VoteStrs[i].TargetStr.gameObject);
            Destroy(VoteStrs[i].gameObject);
        }
        VoteStrs.Clear();

        if (VoteSeqNum == 3)
            VoteSeqNum = 0;
        else
        {
            VoteSeqNum += 1;
            if (VoteSeqNum == 3)
                VoteSeqNum += 1;
        }
        if (VoteSeqNum < 5)
            StartVote();
        else
        {
            for(int i = 0; i < TempStrs.Count; i++)
            {
                StrategyInfo NewStr = Instantiate(ActivePrefab, ActiveStrsContent);
                NewStr.SC = this;
                NewStr.Str = TempStrs[i];
                NewStr.UpdateUI();
                //如果是文化战略就已经激活过了，不能再次激活
                if (TempStrs[i].Type != StrategyType.文化)
                    NewStr.Str.Effect(GC);
                ActiveStrs.Add(NewStr);
            }
            MeetingButton.interactable = false;
            TempStrs.Clear();
            TimeLeft = 384;
            UpdateUI();
            VotePanel.gameObject.SetActive(false);
        }
    }

    //高管进行投票
    void ManagerVote()
    {
        if (VoteStrs.Count == 0)
            return;
        CEOVote = 1;

        //先重置票数
        foreach (StrategyInfo s in VoteStrs)
        {
            s.VoteNum = 0;
            s.VoteButton.gameObject.SetActive(true);
            s.UnVoteButton.gameObject.SetActive(false);
            s.UpdateUI();
        }
        foreach(Employee e in Managers)
        {
            StrategyInfo Ts = null;
            //如果是文化战略先判定倾向是否相同，其他战略直接随机
            if (VoteSeqNum == 3)
            {
                int value = 0;
                foreach (StrategyInfo s in VoteStrs)
                {
                    if (s.Str.CultureRequire == -1)
                    {
                        if (e.CharacterTendency[0] == -1 && Mathf.Abs(e.Character[0]) > value)
                        {
                            value = (int)Mathf.Abs(e.Character[0]);
                            Ts = s;
                        }
                    }
                    else if (s.Str.CultureRequire == 1)
                    {
                        if (e.CharacterTendency[0] == 1 && Mathf.Abs(e.Character[0]) > value)
                        {
                            value = (int)Mathf.Abs(e.Character[0]);
                            Ts = s;
                        }
                    }
                    else if (s.Str.FaithRequire == -1)
                    {
                        if (e.CharacterTendency[1] == -1 && Mathf.Abs(e.Character[1]) > value)
                        {
                            value = (int)Mathf.Abs(e.Character[1]);
                            Ts = s;
                        }
                    }
                    else if (s.Str.FaithRequire == 1)
                    {
                        if (e.CharacterTendency[1] == 1 && Mathf.Abs(e.Character[1]) > value)
                        {
                            value = (int)Mathf.Abs(e.Character[1]);
                            Ts = s;
                        }
                    }
                }
            }
            else
            {
                //如果有自己的战略则从自己的战略中随机一个
                List<StrategyInfo> Ls = new List<StrategyInfo>();
                foreach (StrategyInfo s in VoteStrs)
                {
                    if (s.Owner == e)
                        Ls.Add(s);
                }
                if (Ls.Count > 0)
                {
                    Ts = Ls[Random.Range(0, Ls.Count)];
                    int index = VoteStrs.IndexOf(Ts);
                    //从头检测自己之前是否有同类战略,有的话合并
                    for (int i = 0; i < index; i++)
                    {
                        if(VoteStrs[i].Str == Ts.Str)
                        {
                            Ts.gameObject.SetActive(false);
                            Ts = VoteStrs[i];
                            break;
                        }
                    }
                }
                else
                    Ts = VoteStrs[Random.Range(0, VoteStrs.Count)];
            }
            if (Ts != null)
            {
                Ts.VoteNum += 1;
                Ts.UpdateUI();
            }
        }
    }
    //确定选出的战略
    public void CheckVoteStatus(bool ImmActive = false)
    {
        List<StrategyInfo> TempStrInfos = new List<StrategyInfo>();
        for (int i = 0; i < VoteStrs.Count; i++)
        {
            VoteStrs[i].Active = false;//先重置状态
            VoteStrs[i].ActiveMarker.SetActive(false);
        }
        for (int i = 0; i < StrLimitNum[VoteSeqNum]; i++)
        {
            if (VoteStrs.Count > 0)
            {
                StrategyInfo TStr = VoteStrs[0];
                for (int j = 0; j < VoteStrs.Count; j++)
                {
                    //相等的话取靠前的
                    if (TStr.VoteNum < VoteStrs[j].VoteNum)
                        TStr = VoteStrs[j];

                    if (j == VoteStrs.Count - 1 && TStr.VoteNum != 0)
                    {
                        TempStrInfos.Add(TStr);
                        TStr.Active = true;
                        TStr.ActiveMarker.SetActive(true);
                        VoteStrs.Remove(TStr);
                        //暂时移除被选中的战略，防止下一轮循环被拿来比较
                    }
                }
            }
            else
                break;
        }

        for(int i = 0; i < TempStrInfos.Count; i++)
        {
            VoteStrs.Add(TempStrInfos[i]);//重新将被选中的战略放回
            //如果是文化战略则及时生效
            if (TempStrInfos[i].Str.Type == StrategyType.文化 && ImmActive == true)
                TempStrInfos[i].Str.Effect(GC);
        }
    }

    //开会技能相关
    //技能选择
    public void SetMeetingSkill(int num)
    {
        if(num == 1 && GC.Stamina >= 10)
        {
            StartStrSelect();
            MeetingSkillNum = 1;
            StrSelectNum = 1;           
        }
        else if (num == 2 && GC.Stamina >= 10)
        {
            StartStrSelect();
            MeetingSkillNum = 2;
            StrSelectNum = 1;            
        }
        else if (num == 3 && GC.Stamina >= 20)
        {
            ManagerVote();
            CheckVoteStatus();
            GC.Stamina -= 20;
        }
        else if (num == 4 && GC.Stamina >= 20)
        {
            StartStrSelect();
            MeetingSkillNum = 4;
            StrSelectNum = 1;            
        }
        else if (num == 5 && GC.Stamina >= 30)
        {
            StartStrSelect();
            MeetingSkillNum = 1;
            StrSelectNum = 2;           
        }
    }
    //进入选择模式
    void StartStrSelect()
    {       
        for(int i = 0; i < VoteStrs.Count; i++)
        {
            VoteStrs[i].StrSelectToggle.interactable = true;
            VoteStrs[i].StrSelectToggle.isOn = false;
        }
        VoteSelectStrs.Clear();
    }
    public void SkillStrSelect(StrategyInfo sStr, bool add)
    {
        if(add == true)
        {
            VoteSelectStrs.Add(sStr);
            if(VoteSelectStrs.Count >= StrSelectNum)
            {
                for(int i = 0; i < VoteStrs.Count; i++)
                {
                    VoteStrs[i].StrSelectToggle.interactable = false;
                }
            }
        }
        else
        {
            if (VoteSelectStrs.Contains(sStr))
                VoteSelectStrs.Remove(sStr);
            if (VoteSelectStrs.Count >= StrSelectNum)
            {
                for (int i = 0; i < VoteStrs.Count; i++)
                {
                    VoteStrs[i].StrSelectToggle.interactable = true;
                }
            }
        }
        if (VoteSelectStrs.Count > 0)
            SkillConfirmButton.SetActive(true);
        else
            SkillConfirmButton.SetActive(false);
    }
    public void SkillConfirm()
    {
        bool SkillActive = false;//各种技能需要选择的战略不一样，确定选择了对应战略再发动技能
        //因为开会不暂停所以此处要再检测一遍体力
        if(MeetingSkillNum == 1 && GC.Stamina >= 10)
        {
            if (VoteSelectStrs[0].Active == true)
            {
                SkillActive = true;
                GC.Stamina -= 10;
                List<StrategyInfo> TempInfos = new List<StrategyInfo>();
                for (int i = 0; i < VoteStrs.Count; i++)
                {
                    if (VoteStrs[i].Active == false)
                        TempInfos.Add(VoteStrs[i]);                   
                }

                if(TempInfos.Count <= 3)
                {
                    for(int i = 0; i < TempInfos.Count; i++)
                    {
                        VoteStrs.Remove(TempInfos[i]);
                        Destroy(TempInfos[i].gameObject);
                    }
                }
                else
                {
                    for(int i = 0; i < 3; i++)
                    {
                        int DesNum = Random.Range(0, TempInfos.Count);
                        VoteStrs.Remove(TempInfos[DesNum]);
                        Destroy(TempInfos[DesNum].gameObject);
                    }
                }
                VoteSelectStrs[0].VoteNum = 0;
                VoteSelectStrs[0].UpdateUI();
                CheckVoteStatus();
            }
        }
        else if(MeetingSkillNum == 2 && GC.Stamina >= 10)
        {
            bool allCheck = true;            
            for(int i = 0; i < VoteSelectStrs.Count; i++)
            {
                if(VoteSelectStrs[i].Active == true)
                {
                    allCheck = false;
                    break;
                }
            }
            if(allCheck == true)
            {
                SkillActive = true;
                GC.Stamina -= 10;
                for (int i = 0; i < VoteSelectStrs.Count; i++)
                {
                    VoteStrs.Remove(VoteSelectStrs[i]);
                    Destroy(VoteSelectStrs[i]);
                }
                CheckVoteStatus();
            }
        }
        else if (MeetingSkillNum == 4 && GC.Stamina >= 20)
        {
            if(VoteSelectStrs.Count == 1)
            {
                SkillActive = true;
                GC.Stamina -= 20;
                VoteSelectStrs[0].VoteNum += (int)(VoteSelectStrs[0].VoteNum * 0.5f);
                VoteSelectStrs[0].UpdateUI();
                CheckVoteStatus();
            }
        }
        else if (MeetingSkillNum == 5 && GC.Stamina >= 30)
        {
            if((VoteSelectStrs[0].Active == true && VoteSelectStrs[1].Active == false) || (VoteSelectStrs[0].Active == false && VoteSelectStrs[1].Active == true))
            {
                SkillActive = true;
                GC.Stamina -= 30;
                int num1 = VoteSelectStrs[0].VoteNum, num2 = VoteSelectStrs[1].VoteNum;
                VoteSelectStrs[0].VoteNum = num2;
                VoteSelectStrs[1].VoteNum = num1;
                VoteSelectStrs[0].UpdateUI();
                VoteSelectStrs[1].UpdateUI();
                CheckVoteStatus();
            }
        }

        if (SkillActive == true)
        {
            for (int i = 0; i < VoteStrs.Count; i++)
            {
                VoteStrs[i].StrSelectToggle.interactable = false;
                VoteStrs[i].StrSelectToggle.isOn = false;
            }
            VoteSelectStrs.Clear();
            SkillConfirmButton.gameObject.SetActive(false);
        }
    }
    void CultureStrCheck()
    {
        int CA = 0, CB = 0, FA = 0, FB = 0;
        foreach(Employee e in GC.CurrentEmployees)
        {
            if (e.CharacterTendency[1] == 1)
                FA += 1;
            else if (e.CharacterTendency[1] == -1)
                FB += 1;
            if (e.CharacterTendency[0] == 1)
                CA += 1;
            else if (e.CharacterTendency[0] == -1)
                CB += 1;
        }
        if (FA >= 5)
            DirecAddStrategy(new Strategy4_1());
        if (FA >= 10)
            DirecAddStrategy(new Strategy4_3());
        if (FB >= 5)
            DirecAddStrategy(new Strategy4_6());
        if (FB >= 10)
            DirecAddStrategy(new Strategy4_4());
        if (CA >= 5)
            DirecAddStrategy(new Strategy4_7());
        if (CA >= 10)
            DirecAddStrategy(new Strategy4_8());
        if (CB >= 5)
            DirecAddStrategy(new Strategy4_2());
        if (CB >= 10)
            DirecAddStrategy(new Strategy4_5());

    }
    void DirecAddStrategy(Strategy s)
    {
        StrategyInfo NewStr = Instantiate(GC.StrC.ChargePrefab, GC.StrC.StrategyContent);
        CurrentStrategy = NewStr;
        NewStr.SC = GC.StrC;
        GC.StrC.StrInfos.Add(NewStr);
        NewStr.Str = s;
        CurrentStrategy.Text_Progress.text = "充能完毕";
        CurrentStrategy.RechargeComplete = true;
        NewStr.UpdateUI();
    }


    public void TimePass()
    {
        if(TimeLeft > 0)
        {
            TimeLeft -= 1;
            if (TimeLeft == 0)
            {
                MeetingButton.interactable = true;
                for(int i = 0; i < ActiveStrs.Count; i++)
                {             
                    ActiveStrs[i].Str.EffectRemove(GC);
                    Destroy(ActiveStrs[i].gameObject);
                }
                ActiveStrs.Clear();
            }
        }
        UpdateUI();
    }
}
