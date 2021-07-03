using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceEvent : MonoBehaviour
{
    public int TotalCorrection = 0; //当前提供的总修正
    public string ExtraCorrectionContent;//基本演绎法+1效果的描述
    public bool DoubleCorrection = false;//抉择卡9寻求共识，下张卡修正翻倍的效果

    public Text Text_EventName, Text_Correction, Text_EventResult, Text_EventDescrition, Text_Condition, Text_Tip, Text_OptionCardTip;
    public Transform OptionContent;
    public OptionCardInfo OptionPrefab;
    public EventControl EC;
    public EventGroupInfo EGI;
    public Event CurrentEvent;
    public Employee Self;//自身和目标员工

    //抉择卡特效相关
    public int NoDebuffPerkCount = 0;//随机消除添加负面状态的数量
    public OptionCardInfo SelectedOption;//当前被选中的抉择卡用于选择并去掉1张抉择卡，重新抽取2张的功能

    public List<OptionCardInfo> SelectedOptions = new List<OptionCardInfo>();//当前已选中的卡
    public List<OptionCardInfo> Options = new List<OptionCardInfo>();//所有可选的卡（手牌）
    public List<OptionCardInfo> UsedOptions = new List<OptionCardInfo>();//所有打出的卡（弃牌堆）
    public List<OptionCardInfo> ExtraSelectedOptions = new List<OptionCardInfo>();//选择后激活特殊效果的卡
    public List<Text> Text_TargetEmps = new List<Text>();

    private void Update()
    {
        if (Self != null)
            Text_TargetEmps[0].text = "目标员工:" + Self.Name + "             当前心力:" + Self.Mentality;
        if (EGI != null && EGI.MDebuffEmps.Count > 0)
        {
            for (int i = 0; i < EGI.MDebuffEmps.Count; i++)
            {
                if (EGI.MDebuffEmps[i] != null)
                    Text_TargetEmps[i + 1].text = EGI.MDebuffEmps[i].Name + "    " + EGI.MDebuffEmps[i].Mentality + "/" + EGI.MDebuffEmps[i].MentalityLimit;
                else
                    Text_TargetEmps[i + 1].text = "无对象";
            }
        }
    }

    public void CheckCorrectionUI()
    {
        Text_Correction.text = "当前修正:" + TotalCorrection + "\n";
        if(Self.CurrentDep != null || Self.CurrentDivision != null)
        {
            int FaithCorrection = CurrentEvent.CalcDivisionFaith(Self);
            int ManageCorrection = CurrentEvent.CalcDivisionManage(Self);

            if (FaithCorrection > 0)
                Text_Correction.text += "事业部信念: +" + FaithCorrection + "修正\n";
            else if (FaithCorrection < 0)
                Text_Correction.text += "事业部信念: " + FaithCorrection + "修正\n";
            if (ManageCorrection > 0)
                Text_Correction.text += "事业部管理: +" + ManageCorrection + "修正\n";
            else if (ManageCorrection < 0)
                Text_Correction.text += "事业部管理: " + ManageCorrection + "修正\n";
        }
        foreach(PerkInfo perk in Self.InfoDetail.PerksInfo)
        {
            if (perk.CurrentPerk.Num == 16)
            {
                Text_Correction.text += Self.Name + "冷静特质:+1修正";
                break;
            }
        }
        for(int i = 0; i < SelectedOptions.Count; i++)
        {
            OptionCardInfo option = SelectedOptions[i];
            int correction = option.OC.Correction;
            if (SelectedOptions[i].DoubleCorrection == true)
                correction *= 2;

            if (correction > 0)
                Text_Correction.text += option.OC.Name + ":  +" + correction + "修正\n";
            else if (correction < 0)
                Text_Correction.text += option.OC.Name + ":  " + correction + "修正\n";
        }
        Text_Correction.text += ExtraCorrectionContent;

        Text_Condition.text = "初始成功条件:\n1D20 > " + CurrentEvent.FailLimitValue + "\n当前成功条件:\n1D20 > " 
            + (CurrentEvent.FailLimitValue - TotalCorrection);
    }

    //完成抉择
    public void ConfirmChoice()
    {
        EC.UnfinishedEvents.Remove(this);

        //消除提供负面状态效果的功能(抉择卡10缓和情绪)
        if (NoDebuffPerkCount > 0)
        {
            List<OptionCardInfo> DebuffPerkInfos = new List<OptionCardInfo>();
            foreach (OptionCardInfo option in SelectedOptions)
            {
                if (option.OC.AddDebuffPerk == true)
                    DebuffPerkInfos.Add(option);
            }
            if (DebuffPerkInfos.Count > 0)
            {
                if (NoDebuffPerkCount >= DebuffPerkInfos.Count)
                {
                    foreach (OptionCardInfo option in DebuffPerkInfos)
                    {
                        option.NoEffect = true;
                    }
                }
                else
                {
                    for (int i = 0; i < NoDebuffPerkCount; i++)
                    {
                        OptionCardInfo info = DebuffPerkInfos[Random.Range(0, DebuffPerkInfos.Count)];
                        info.NoEffect = true;
                        DebuffPerkInfos.Remove(info);
                    }
                }
            }
        }

        foreach (OptionCardInfo option in SelectedOptions)
        {
            if (option.NoEffect == false)
            {
                option.OC.StartEffect(Self);
                option.TargetAddPerk(Self);
            }
        }
        if (EGI == null)
        {
            CurrentEvent.StartEvent(Self, TotalCorrection);
            EC.ChoiceEventCheck(false);
        }
        else
        {
            if (EGI.Target == null || EGI.TargetDep == null || EGI.TargetDivision == null)
            {
                Debug.Log("丢失目标");
                EGI.UpdateUI();
            }
            CurrentEvent.StartEvent(Self, TotalCorrection, null, EGI);
            EGI.FinishStage += 1;
            EGI.NextStage();
            EC.ChoiceEventCheck(true);
        }
        Destroy(this.gameObject);
    }

    //取消选择一张后刷新两张的效果
    public void CancelSelect()
    {
        if (SelectedOption != null && SelectedOption.OC.Num == 13)
            SelectedOption.SelectOptionCard();
            
    }

    public void SetEventInfo(Event e, Employee emp, EventGroupInfo info = null)
    {
        int ContentIndex = Random.Range(1, e.DescriptionCount + 1);
        CurrentEvent = e;
        EGI = info;
        Self = emp;
        if (info != null)
        {
            Text_EventName.text = e.SubEventNames[info.RandomEventNum - 1] + "\n所属事件组:" + e.EventName;
            if (info.TargetEventGroup.DebuffEvent == true)
                Text_EventResult.text = "失败效果:" + e.ResultDescription(emp, null, info.RandomEventNum);
            else
                Text_EventResult.text = "成功效果:" + e.ResultDescription(emp, null, info.RandomEventNum);
            Text_EventDescrition.text = e.EventDescription(emp, null, info.RandomEventNum, info);
            if (EGI.MDebuffEmps.Count > 0)
            {
                for (int i = 0; i < EGI.MDebuffEmps.Count; i++)
                {
                    Text_TargetEmps[i + 1].gameObject.SetActive(true);
                }
                Text_TargetEmps[9].gameObject.SetActive(true);
                Text_TargetEmps[9].text = "失败时以下员工将会损失" + info.TargetEventGroup.MentalityDebuffValue + "点心力：";
            }
        }
        else
        {
            Text_EventName.text = e.EventName;
            if (e.SuccessDescription != null)
                Text_EventResult.text = "成功效果:" + e.SuccessDescription;
            else
                Text_EventResult.text = "失败效果:" + e.FailDescription;
            Text_EventDescrition.text = e.EventDescription(emp, null, 1);
        }
        CheckSpecialCorrection();

        //
        foreach(OptionCardInfo oci in EC.GC.OCL.CurrentOptions)
        {
            Options.Add(oci);
        }
        for (int i = 0; i < 3; i++)
        {
            CreateOption();
        }
    }

    //新建一个抉择卡
    public void CreateOption()
    {
        OptionCardInfo option = Instantiate(OptionPrefab, OptionContent);
        option.RandomOption(this);
    }

    //检查部门和高管提供的修正
    void CheckSpecialCorrection()
    {
        if (Self.CurrentDep != null || Self.CurrentDivision != null)
        {
            TotalCorrection += CurrentEvent.CalcDivisionFaith(Self);
            TotalCorrection += CurrentEvent.CalcDivisionManage(Self);
        }
        foreach (PerkInfo perk in Self.InfoDetail.PerksInfo)
        {
            if (perk.CurrentPerk.Num == 16)
            {
                TotalCorrection += 1;
                break;
            }
        }
        CheckCorrectionUI();
    }

    //显示目标员工详细信息
    public void ShowTargetEmpInfo(int num)
    {
        if (num == 0 && Self != null)
            Self.InfoDetail.ShowPanel();
        else if (EGI != null && EGI.MDebuffEmps.Count > 0)
            EGI.MDebuffEmps[num - 1].InfoDetail.ShowPanel();
    }
}
