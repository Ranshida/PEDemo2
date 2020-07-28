using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StrategyControl : MonoBehaviour
{

    public int[] CurrentStrNum = new int[5];
    public int[] StrLimitNum = new int[5];

    public StrategyInfo InfoPrefabA, InfoPrefabB, CurrentStrategy;
    public GameObject NewStrPanel;
    public Transform StrategyContent;
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
            }
            NewStrPanel.SetActive(true);
        }
    }

    public void CompleteStrategy()
    {
        if(CurrentStrategy.Str != null)
        {
            bool Success = false;
            List<Task> ts = new List<Task>();
            for(int i = 0; i < CurrentStrategy.Str.RequestTasks.Count; i++)
            {
                int num = 0;
                for(int j = 0; j < GC.FinishedTask.Count; j++)
                {
                    if (CurrentStrategy.Str.RequestTasks[i].TaskType == GC.FinishedTask[j].TaskType &&
                       CurrentStrategy.Str.RequestTasks[i].Num == GC.FinishedTask[j].Num &&
                       CurrentStrategy.Str.RequestTasks[i].Value <= GC.FinishedTask[j].Value)
                    {
                        num += 1;
                        ts.Add(GC.FinishedTask[j]);
                        if (num == CurrentStrategy.Str.RequestNum[i])
                        {
                            Success = true;
                            break;
                        }

                    }
                    else if (j == GC.FinishedTask.Count - 1)
                        Success = false;
                }
                if (Success == false)
                    break;
            }
            if(Success == true)
            {
                for(int i = 0; i < ts.Count; i++)
                {
                    GC.FinishedTask.Remove(ts[i]);
                }
                StrategyInfo newS = Instantiate(InfoPrefabB, StrategyContent);
                newS.Str = CurrentStrategy.Str;
                newS.SC = this;
                newS.TimeLeft = 12;
                newS.UpdateUI();
                GC.WeeklyEvent.AddListener(newS.TimePass);

                CurrentStrategy.Str = null;
                CurrentStrategy.UpdateUI();
                GC.WeeklyEvent.RemoveListener(CurrentStrategy.TimePass);
            }
            print(Success);
        }
    }

    public void UpdateUI()
    {
        for(int i = 0; i < 5; i++)
        {
            Texts_Strnum[i].text = "已用:" + CurrentStrNum[i] + "可用:" + StrLimitNum[i];
        }
    }

    public void CheckStrNum()
    {

    }

    public void StrategyFail()
    {
        if (GC.Money > 0)
            GC.Money -= (int)(GC.Money * 0.2f);
    }
}
