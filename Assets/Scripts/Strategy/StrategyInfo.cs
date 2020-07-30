using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StrategyInfo : MonoBehaviour
{
    public bool Used = false;
    public int TimeLeft, Type;

    public Strategy Str;
    public StrategyControl SC;
    public InfoPanel info;
    public Text Text_Name, Text_Time, Text_Type, Text_EffectDescription, Text_RequestDescription;
    public Button UseButton, AbolishButton;

    float Timer = 0;
    bool ShowPanel = false;

    void Update()
    {
        if (ShowPanel == true)
        {
            if (Timer < 0.25f)
                Timer += Time.deltaTime;
            else
            {
                if (Str != null)
                {
                    info.Text_Name.text = Str.Name;
                    info.Text_Description.text = Str.EffectDescription;
                    info.Text_ExtraInfo.text = Str.RequestDescription;
                }
                if (info.Visible == false)
                    info.ShowPanel();
                info.transform.position = Input.mousePosition;
            }
        }
    }

    public void PointerEnter()
    {
        ShowPanel = true;
    }

    public void PointerExit()
    {
        ShowPanel = false;
        info.ClosePanel();
        Timer = 0;
    }

    public void UpdateUI()
    {
        if (Str != null)
        {
            Text_Name.text = Str.Name;
            Text_Type.text = Str.Type.ToString();
            Text_EffectDescription.text = Str.EffectDescription;
            if (Text_RequestDescription != null)
                Text_RequestDescription.text = Str.RequestDescription;
            if (Text_Time != null)
                Text_Time.text = "剩余" + TimeLeft + "时";
        }
        else
        {
            Text_Name.text = "无";
            Text_Type.text = "--";
            Text_EffectDescription.text = "--";
            if (Text_RequestDescription != null)
                Text_RequestDescription.text = "--";
            if (Text_Time != null)
                Text_Time.text = "剩余--时";
        }
    }

    public void ToggleUsage()
    {
        int t = (int)Str.Type;
        if (Used == false)
        {
            if (SC.CurrentStrNum[t] < SC.StrLimitNum[t])
            {
                Used = true;
                UseButton.gameObject.SetActive(false);
                AbolishButton.gameObject.SetActive(true);
                SC.CurrentStrNum[t] += 1;
                SC.StrInfos.Add(this);
                Str.Effect(SC.GC);
            }
        }
        else
        {
            Used = false;
            UseButton.gameObject.SetActive(true);
            AbolishButton.gameObject.SetActive(false);
            SC.CurrentStrNum[t] -= 1;
            SC.StrInfos.Remove(this);
            Str.EffectRemove(SC.GC);
        }
        SC.UpdateUI();
    }

    public void SelectStrategy()
    {
        StrategyInfo newS = Instantiate(SC.InfoPrefabC, SC.UnfinishedStrategyContent);
        newS.TimeLeft = 96;
        newS.Str = Str;
        newS.SC = SC;
        newS.UpdateUI();
        SC.GC.HourEvent.AddListener(newS.TimePass);
        UseButton.interactable = false;
        Str = null;
        UpdateUI();
    }

    public void TimePass()
    {
        TimeLeft -= 1;
        if (TimeLeft < 1)
        {
            if (Type == 2)
            {
                if (Used == true)
                    ToggleUsage();
                SC.GC.HourEvent.RemoveListener(TimePass);
                Destroy(this.gameObject);
            }
            else if (Type == 4)
            {
                Str = null;
                UpdateUI();
                SC.GC.HourEvent.RemoveListener(TimePass);
                UseButton.gameObject.SetActive(false);
                AbolishButton.gameObject.SetActive(true);
                SC.GC.Morale -= 10;
                SC.GC.Money -= (int)(SC.GC.Money * 0.2f);
            }
        }
        else
            UpdateUI();
    }

    public void CompleteStrategy()
    {
        bool Success = false;
        if (Str != null)
        {
            List<Task> ts = new List<Task>();
            for (int i = 0; i < Str.RequestTasks.Count; i++)
            {
                int num = 0;
                for (int j = 0; j < SC.GC.FinishedTask.Count; j++)
                {
                    if (Str.RequestTasks[i].TaskType == SC.GC.FinishedTask[j].TaskType &&
                       Str.RequestTasks[i].Num == SC.GC.FinishedTask[j].Num &&
                       Str.RequestTasks[i].Value <= SC.GC.FinishedTask[j].Value)
                    {
                        num += 1;
                        ts.Add(SC.GC.FinishedTask[j]);
                        if (num == Str.RequestNum[i])
                        {
                            Success = true;
                            break;
                        }

                    }
                    else if (j == SC.GC.FinishedTask.Count - 1)
                    {
                        Success = false;
                    }
                }
                if (Success == false)
                    break;
            }
            if (Success == true)
            {
                for (int i = 0; i < ts.Count; i++)
                {
                    SC.GC.FinishedTask.Remove(ts[i]);
                }
                StrategyInfo newS = Instantiate(SC.InfoPrefabB, SC.StrategyContent);
                newS.Str = Str;
                newS.SC = SC;
                newS.TimeLeft = 96;
                newS.UpdateUI();
                SC.GC.HourEvent.AddListener(newS.TimePass);

                SC.GC.HourEvent.RemoveListener(TimePass);
                Destroy(this.gameObject);
            }
        }
    }

    public void DeleteStrategy()
    {
        Destroy(this.gameObject);
    }
}
