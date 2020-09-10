using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StrategyControl : MonoBehaviour
{

    public int TimeLeft = 0, CultureTimeLeft = 0;

    public int[] CurrentStrNum = new int[5];
    public int[] StrLimitNum = new int[5];
    public int[] BlockTime = new int[3];

    public StrategyInfo InfoPrefabA, InfoPrefabB, InfoPrefabC, CurrentStrategy, ChargePrefab, VotePrefab, ActivePrefab;//前四个是旧版Prefab
    public GameObject NewStrPanel, VotePanel, SkillConfirmButton;
    public Transform StrategyContent, UnfinishedStrategyContent, VoteContent, ActiveStrsContent;
    public GameControl GC;
    public Button NewStrButton, MeetingButton;
    public Text Text_Time, Text_CTime, Text_StrategyStatus;

    public List<StrategyInfo> StrInfos = new List<StrategyInfo>(), VoteStrs = new List<StrategyInfo>(), ActiveStrs = new List<StrategyInfo>()
        , VoteSelectStrs = new List<StrategyInfo>();
    public StrategyInfo[] NewStrs = new StrategyInfo[3];
    public StrategyInfo[] CurrentStrs = new StrategyInfo[3];
    public Text[] Texts_Strnum = new Text[5];
    public Text[] Texts_BlockTime = new Text[3];


    bool canChangeCulture = true;
    int VoteSeqNum = 0, MeetingSkillNum = 0, StrSelectNum = 0, FinishedStrNum = 0;
    List<Strategy> TempStrs = new List<Strategy>();

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

    //public bool CompleteStrategy()
    //{
    //    bool Success = false;
    //    if (CurrentStrategy.Str != null)
    //    {
    //        List<Task> ts = new List<Task>();
    //        for(int i = 0; i < CurrentStrategy.Str.RequestTasks.Count; i++)
    //        {
    //            int num = 0;
    //            for(int j = 0; j < GC.FinishedTask.Count; j++)
    //            {
    //                if (CurrentStrategy.Str.RequestTasks[i].TaskType == GC.FinishedTask[j].TaskType &&
    //                   CurrentStrategy.Str.RequestTasks[i].Num == GC.FinishedTask[j].Num &&
    //                   CurrentStrategy.Str.RequestTasks[i].Value <= GC.FinishedTask[j].Value)
    //                {
    //                    num += 1;
    //                    ts.Add(GC.FinishedTask[j]);
    //                    if (num == CurrentStrategy.Str.RequestNum[i])
    //                    {
    //                        Success = true;
    //                        break;
    //                    }

    //                }
    //                else if (j == GC.FinishedTask.Count - 1)
    //                {
    //                    Success = false;
    //                    return false;
    //                }
    //            }
    //        }
    //        if(Success == true)
    //        {
    //            for(int i = 0; i < ts.Count; i++)
    //            {
    //                GC.FinishedTask.Remove(ts[i]);
    //            }
    //            StrategyInfo newS = Instantiate(InfoPrefabB, StrategyContent);
    //            newS.Str = CurrentStrategy.Str;
    //            newS.SC = this;
    //            newS.TimeLeft = 96;
    //            newS.UpdateUI();
    //            GC.HourEvent.AddListener(newS.TimePass);

    //            CurrentStrategy.Str = null;
    //            CurrentStrategy.UpdateUI();
    //            GC.HourEvent.RemoveListener(CurrentStrategy.TimePass);
    //        }
    //    }
    //    return Success;
    //}

    public void UpdateUI()
    {
        for(int i = 0; i < 5; i++)
        {
            //Texts_Strnum[i].text = "已用:" + CurrentStrNum[i] + "可用:" + StrLimitNum[i];
            Texts_Strnum[i].text = "槽位:" + StrLimitNum[i];
        }
        Text_Time.text = "距离下次会议召开还剩:" + TimeLeft + "时";
        Text_CTime.text = "距离下次文化变更还剩:" + CultureTimeLeft + "时";
        Text_StrategyStatus.text = "未完成战略:" + ActiveStrs.Count + "\n剩余时间:" + TimeLeft;
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
        if (GC.MeetingBlockTime == 0)
            StartVote();
    }
    void StartVote()
    {
        if (canChangeCulture == true)
        {
            VoteSeqNum = 3;
            canChangeCulture = false;
            CultureTimeLeft = -1;
        }

        if (StrLimitNum[VoteSeqNum] > 0)
        {
            int typenum = 0;
            for (int i = 0; i < StrInfos.Count; i++)
            {
                if (VoteSeqNum == (int)StrInfos[i].Str.Type)
                {
                    //还要写一些选择相关的功能
                    StrategyInfo newStr = Instantiate(VotePrefab, VoteContent);
                    newStr.SC = this;
                    newStr.Str = StrInfos[i].Str;
                    newStr.TargetStr = StrInfos[i];
                    newStr.VoteNum = Random.Range(1, GC.CurrentEmployees.Count + 1);//+1代表CEO也会投票,同时避免(1,0)的情况
                    newStr.UpdateUI();
                    VoteStrs.Add(newStr);
                    typenum += 1;
                }
            }
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
        CheckVoteStatus();
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
            if (CultureTimeLeft == -1)
                CultureTimeLeft = 1152;
            FinishedStrNum = 0;
            if (ActiveStrs.Count > 0)
                Text_StrategyStatus.gameObject.SetActive(true);
            UpdateUI();
            VotePanel.gameObject.SetActive(false);
        }
    }
    //确定选出的战略
    void CheckVoteStatus()
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
            if (TempStrInfos[i].Str.Type == StrategyType.文化)
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
            StrSelectNum = 2;            
        }
        else if (num == 3 && GC.Stamina >= 20)
        {
            for(int i = 0; i < VoteStrs.Count; i++)
            {
                if (VoteStrs[i].Active == true)
                    VoteStrs[i].VoteNum = 0;
                else
                    VoteStrs[i].VoteNum = Random.Range(1, GC.CurrentEmployees.Count + 1);
                VoteStrs[i].UpdateUI();
            }
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

    public void TimePass()
    {
        //旧时限部分
        #region
        //for(int i = 0; i < BlockTime.Length; i++)
        //{
        //    if(BlockTime[i] > 0)
        //    {
        //        BlockTime[i] -= 1;
        //        if (BlockTime[i] == 0)
        //        {
        //            Texts_BlockTime[i].gameObject.SetActive(false);
        //            NewStrs[i].gameObject.SetActive(true);
        //            NewStrs[i].Active = false;
        //            GC.Morale += 10;
        //        }
        //        else
        //            Texts_BlockTime[i].text = "剩余封锁时间:" + BlockTime[i] + "时";
        //    }
        //}

        //if(TimeLeft > 0)
        //{
        //    TimeLeft -= 1;
        //    if(TimeLeft == 0)
        //    {
        //        NewStrButton.interactable = true;
        //        for(int i = 0; i < CurrentStrs.Length; i++)
        //        {
        //            if (CurrentStrs[i].Used == false)
        //            {
        //                CurrentStrs[i].Str.EffectRemove(GC);
        //                CurrentStrs[i].Used = true;
        //                CurrentStrs[i].UseButton.interactable = false;
        //                GC.HourEvent.RemoveListener(CurrentStrs[i].TimePass);
        //                if (CurrentStrs[i].Active == false)
        //                {
        //                    GC.Morale -= 10;
        //                    BlockTime[CurrentStrs[i].StrNum] = 96;
        //                    NewStrs[CurrentStrs[i].StrNum].Active = true;
        //                    NewStrs[CurrentStrs[i].StrNum].gameObject.SetActive(false);
        //                    Texts_BlockTime[CurrentStrs[i].StrNum].gameObject.SetActive(true);
        //                }
        //            }
        //        }
        //    }
        //}
        #endregion
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
        if(CultureTimeLeft > 0)
        {
            CultureTimeLeft -= 1;
            if (CultureTimeLeft == 0)
                canChangeCulture = true;
        }
        UpdateUI();
    }

    public void SolveStrRequest(int type, int value)
    {
        for(int i = 0; i < ActiveStrs.Count; i++)
        {
            if(ActiveStrs[i].Str.RequestType == type && ActiveStrs[i].Active == false)
            {
                ActiveStrs[i].CurrentRequestValue += value;
                if (ActiveStrs[i].CurrentRequestValue >= ActiveStrs[i].Str.RequestValue)
                {
                    ActiveStrs[i].Active = true;
                    FinishedStrNum += 1;
                    if (FinishedStrNum == ActiveStrs.Count)
                        Text_StrategyStatus.gameObject.SetActive(false);
                }
                ActiveStrs[i].UpdateUI();
                
                break;
            }
        }
    }
}
