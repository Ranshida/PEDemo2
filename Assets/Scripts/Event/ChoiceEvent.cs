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
    public List<Employee> MDebuffEmps = new List<Employee>();

    private void Update()
    {
        if (Self != null)
            Text_TargetEmps[0].text = "目标员工:" + Self.Name + "             当前心力:" + Self.Mentality;
        if (MDebuffEmps.Count > 0)
        {
            for (int i = 0; i < MDebuffEmps.Count; i++)
            {
                if (MDebuffEmps[i] != null)
                    Text_TargetEmps[i + 1].text = MDebuffEmps[i].Name + "    " + MDebuffEmps[i].Mentality + "/" + MDebuffEmps[i].MentalityLimit;
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
            int PerkCorrection = CurrentEvent.CalcDivisionPerk(Self);

            if (FaithCorrection > 0)
                Text_Correction.text += "事业部信念: +" + FaithCorrection + "修正\n";
            else if (FaithCorrection < 0)
                Text_Correction.text += "事业部信念: " + FaithCorrection + "修正\n";

            if (ManageCorrection > 0)
                Text_Correction.text += "事业部管理: +" + ManageCorrection + "修正\n";
            else if (ManageCorrection < 0)
                Text_Correction.text += "事业部管理: " + ManageCorrection + "修正\n";

            if (PerkCorrection > 0)
                Text_Correction.text += "事业部状态: +" + PerkCorrection + "修正\n";
            else if (PerkCorrection < 0)
                Text_Correction.text += "事业部状态: " + PerkCorrection + "修正\n";
        }
        foreach(PerkInfo perk in Self.InfoDetail.PerksInfo)
        {
            if (perk.CurrentPerk.Num == 152)
            {
                Text_Correction.text += Self.Name + "自闭特质:-3修正";
                break;
            }
        }

        //负面特质修正
        if (CurrentEvent.AgreementEvent == true && EC.GC.CPC.CurrentDebuffPerks.ContainsKey(151))
        {
            Text_Correction.text += "阴谋论者特质:- " + (2 * EC.GC.CPC.CurrentDebuffPerks[150].Count) + "修正";
            TotalCorrection -= (2 * EC.GC.CPC.CurrentDebuffPerks[150].Count);
        }

        for (int i = 0; i < SelectedOptions.Count; i++)
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
        EC.CurrentChoiceEvent = this;
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
        EC.CurrentChoiceEvent = null;
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

        if (e.MentalityDebuffValue > 0)
        {
            MDebuffEmps.Clear();
            List<Employee> PosbEmps = new List<Employee>();
            foreach (Employee me in GameControl.Instance.CurrentEmployees)
            {
                if (me != emp)
                    PosbEmps.Add(me);
            }

            if (PosbEmps.Count < e.MentalityDebuffCount)
            {
                foreach (Employee re in PosbEmps)
                {
                    MDebuffEmps.Add(re);
                }
            }
            else
            {
                for (int i = 0; i < e.MentalityDebuffCount; i++)
                {
                    int num = Random.Range(0, PosbEmps.Count);
                    MDebuffEmps.Add(PosbEmps[num]);
                    PosbEmps.RemoveAt(num);
                }
            }

            for (int i = 0; i < MDebuffEmps.Count; i++)
            {
                Text_TargetEmps[i + 1].gameObject.SetActive(true);
            }
            Text_TargetEmps[9].gameObject.SetActive(true);
            Text_TargetEmps[9].text = "失败时以下员工将会损失" + e.MentalityDebuffValue + "点心力：";
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
        print("Create");
        option.RandomOption(this);
    }

    //检查部门和高管提供的修正
    void CheckSpecialCorrection()
    {
        if (Self.CurrentDep != null || Self.CurrentDivision != null)
        {
            TotalCorrection += CurrentEvent.CalcDivisionFaith(Self);
            TotalCorrection += CurrentEvent.CalcDivisionManage(Self);
            TotalCorrection += CurrentEvent.CalcDivisionPerk(Self);
        }
        foreach (PerkInfo perk in Self.InfoDetail.PerksInfo)
        {
            if (perk.CurrentPerk.Num == 152)
            {
                TotalCorrection -= 3;
                break;
            }
        }
        if (CurrentEvent.AgreementEvent == true && EC.GC.CPC.CurrentDebuffPerks.ContainsKey(151))
        {
            TotalCorrection -= (2 * EC.GC.CPC.CurrentDebuffPerks[150].Count);
        }
        CheckCorrectionUI();
    }

    //显示目标员工详细信息
    public void ShowTargetEmpInfo(int num)
    {
        if (num == 0 && Self != null)
            Self.InfoDetail.ShowPanel();
        else if (MDebuffEmps.Count > 0)
            MDebuffEmps[num - 1].InfoDetail.ShowPanel();
    }

    //随机员工心力Debuff
    public void MentalityDebuff()
    {
        //心力debuff
        if (MDebuffEmps.Count > 0)
        {
            foreach (Employee emp in MDebuffEmps)
            {
                emp.Mentality -= CurrentEvent.MentalityDebuffValue;
            }
        }
    }
}
