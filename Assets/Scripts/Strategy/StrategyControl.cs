using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StrategyControl : MonoBehaviour
{

    public int[] CurrentStrNum = new int[5];
    public int[] StrLimitNum = new int[5];

    public StrategyInfo InfoPrefabA, InfoPrefabB, InfoPrefabC,CurrentStrategy;
    public GameObject NewStrPanel;
    public Transform StrategyContent, UnfinishedStrategyContent;
    public GameControl GC;

    public List<StrategyInfo> StrInfos = new List<StrategyInfo>();
    public StrategyInfo[] NewStrs = new StrategyInfo[3];
    public Text[] Texts_Strnum = new Text[5];

    private void Start()
    {
        UpdateUI();
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
            Texts_Strnum[i].text = "已用:" + CurrentStrNum[i] + "可用:" + StrLimitNum[i];
        }
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
            NewStrs[i].SelectStrategy();
        }
    }

    public void StrategyFail()
    {
        if (GC.Money > 0)
            GC.Money -= (int)(GC.Money * 0.2f);
    }
}
