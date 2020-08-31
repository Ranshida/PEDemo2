using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StrategyInfo : MonoBehaviour
{
    public bool Active = false; //原本用于检测是否放在插槽上，现在用来检测是否完成/是否被封锁,在投票中判断是否被选中
    public bool Used = false;
    public bool RechargeComplete = false;
    public int TimeLeft, Type, StrNum, CurrentRequestValue = 0;//Type1-4已废弃, 5:充能完毕 6:投票用 7:已启用
    public int VoteNum = 0;

    public Strategy Str;
    public StrategyInfo TargetStr;//开会功能中用于保存自身代表的，保存在待使用面板的战略
    public StrategyControl SC;
    public InfoPanel info;
    public Text Text_Name, Text_Time, Text_Type, Text_EffectDescription, Text_RequestDescription, Text_Complete, Text_Progress;
    public Button UseButton, AbolishButton;
    public Toggle StrSelectToggle;
    public GameObject ActiveMarker;

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
            if (Type == 6)
                Text_Progress.text = "得票数:" + VoteNum;
            else if (Type == 7)
            {
                if (Active == false)
                {
                    if (Str.RequestType == 1)
                        Text_Progress.text = "提升 " + CurrentRequestValue + "/" + Str.RequestValue + " 人心力";
                    else if (Str.RequestType == 2)
                        Text_Progress.text = "增加产品分 " + CurrentRequestValue + "/" + Str.RequestValue;
                    else if (Str.RequestType == 3)
                        Text_Progress.text = "生产产品原型图 " + CurrentRequestValue + "/" + Str.RequestValue;
                    else if (Str.RequestType == 4)
                        Text_Progress.text = "生产产品营销文案 " + CurrentRequestValue + "/" + Str.RequestValue;
                    else if (Str.RequestType == 5)
                        Text_Progress.text = "生产产品程序迭代 " + CurrentRequestValue + "/" + Str.RequestValue;
                    else if (Str.RequestType == 6)
                        Text_Progress.text = "发生 " + CurrentRequestValue + "/" + Str.RequestValue + " 次事件";
                    else if (Str.RequestType == 7)
                        Text_Progress.text = "完成 " + CurrentRequestValue + "/" + Str.RequestValue + " 次研究或调研";
                }
                else
                    Text_Progress.text = "已完成";
            }
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
        if (Active == false)
        {
            if (SC.CurrentStrNum[t] < SC.StrLimitNum[t])
            {
                Active = true;
                UseButton.gameObject.SetActive(false);
                AbolishButton.gameObject.SetActive(true);
                SC.CurrentStrNum[t] += 1;
                SC.StrInfos.Add(this);
                Str.Effect(SC.GC);
            }
        }
        else
        {
            Active = false;
            UseButton.gameObject.SetActive(true);
            AbolishButton.gameObject.SetActive(false);
            SC.CurrentStrNum[t] -= 1;
            SC.StrInfos.Remove(this);
            Str.EffectRemove(SC.GC);
        }
        SC.UpdateUI();
    }

    public StrategyInfo SelectStrategy()
    {
        StrategyInfo newS = Instantiate(SC.InfoPrefabC, SC.UnfinishedStrategyContent);
        newS.TimeLeft = 96;
        newS.Str = Str;
        newS.SC = SC;
        newS.UpdateUI();
        Str.Effect(SC.GC);
        SC.GC.HourEvent.AddListener(newS.TimePass);
        UseButton.interactable = false;
        Str = null;
        UpdateUI();
        return newS;
    }

    public void TimePass()
    {
        TimeLeft -= 1;
        UpdateUI();
        //if (TimeLeft < 1)
        //{
        //    if (Type == 2)
        //    {
        //        if (Used == true)
        //            ToggleUsage();
        //        SC.GC.HourEvent.RemoveListener(TimePass);
        //        Destroy(this.gameObject);
        //    }
        //    else if (Type == 4)
        //    {
        //        Str = null;
        //        UpdateUI();
        //        SC.GC.HourEvent.RemoveListener(TimePass);
        //        UseButton.gameObject.SetActive(false);
        //        AbolishButton.gameObject.SetActive(true);
        //        SC.GC.Morale -= 10;
        //        SC.GC.Money -= (int)(SC.GC.Money * 0.2f);
        //    }
        //}
        //else
        //    UpdateUI();
    }

    public void CompleteStrategy()
    {
        bool Success = true;
        if (Str != null)
        {
            for (int i = 0; i < Str.RequestTasks.Count; i++)
            {
                if (SC.GC.FinishedTask[Str.RequestTasks[i]] < Str.RequestNum[i])
                {
                    Success = false;
                    break;
                }
            }
            if (Success == true)
            {
                for (int i = 0; i < Str.RequestTasks.Count; i++)
                {
                    SC.GC.FinishedTask[Str.RequestTasks[i]] -= Str.RequestNum[i];
                }
                //StrategyInfo newS = Instantiate(SC.InfoPrefabB, SC.StrategyContent);
                //newS.Str = Str;
                //newS.SC = SC;
                //newS.TimeLeft = 96;
                //newS.UpdateUI();
                //SC.GC.HourEvent.AddListener(newS.TimePass);

                SC.GC.HourEvent.RemoveListener(TimePass);
                //Destroy(this.gameObject);
                UseButton.interactable = false;
                Active = true;
                Text_Complete.text = "已完成";
            }
        }
    }

    public void SelectStr(bool check)
    {
        SC.SkillStrSelect(this, check);
    }

    public void DeleteStrategy()
    {
        Destroy(this.gameObject);
    }
}
