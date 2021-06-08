using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionCardInfo : MonoBehaviour
{
    private int SelectType = 0;//卡牌预制体类型 1表示抉择卡库中使用的预制体 2表示抉择面板使用的
    public bool Selected = false;//(在抉择事件面板)是否被选中
    public bool NoEffect = false;//是否不会产生负面状态（抉择卡10效果）
    public bool DoubleCorrection = false;//抉择卡9寻求共识，下张卡修正翻倍的效果，这里用来记录是否翻倍，用于UI修正总览处显示

    public OptionCardLibrary OCL;
    public OptionCard OC;
    public Employee Emp;
    public EventGroup EG;
    public ChoiceEvent CurrentEvent;
    public Perk ProvidePerk;//抉择卡效果中可能提供的随机特质的存储
    public Text Text_Name, Text_Description, Text_Correction, Text_Emp, Text_CardContent;

    //设置各种基础信息
    public void SetBaseInfo(OptionCard oc)
    {
        OC = oc;
        Text_Name.text = oc.Name;
        Text_Description.text = oc.Description;
        if (oc.Correction > 0)
        {
            Text_Correction.text = "+" + oc.Correction;
            Text_Correction.transform.parent.gameObject.SetActive(true);
        }
        else if (oc.Correction < 0)
        {
            Text_Correction.text = oc.Correction.ToString();
            Text_Correction.transform.parent.gameObject.SetActive(true);
        }
        else
            Text_Correction.transform.parent.gameObject.SetActive(false);
        if (oc.DebuffCard == true)
            this.gameObject.GetComponent<Image>().color = new Color(0.7f, 0.85f, 1);
        else
            this.gameObject.GetComponent<Image>().color = new Color(1, 1, 0.7f);
    }

    //卡牌库创建信息设定
    public void SetInfo(OptionCard oc, Employee emp)
    {
        SetBaseInfo(oc);
        Emp = emp;
        Text_Emp.text = "持有人:" + emp.Name;
        Text_Emp.gameObject.SetActive(true);
        emp.InfoDetail.OptionCards.Add(this);
        SelectType = 1;
    }

    //抉择事件随机卡牌信息设定
    void SetInfo(OptionCardInfo info, ChoiceEvent Cevent)
    {
        SetBaseInfo(info.OC);
        if (info.Emp != null)
            Emp = info.Emp;
        CurrentEvent = Cevent;
        SelectType = 2;
    }

    public void SelectOptionCard()
    {
        //抉择卡库界面信息展示
        if (SelectType == 1)
        {
            if (Emp != null)
                OCL.ShowEmpOptions(Emp);
            else
                OCL.Text_EmpName.text = "已开除的员工";
        }
        //抉择事件选择面板进行选择
        else if (SelectType == 2)
        {
            //正面事件组只能用正面卡，负面事件组只能用负面卡(选择一张卡删除之类的选择应该不包括在内)
            if (CurrentEvent.EGI != null && CurrentEvent.EGI.TargetEventGroup.DebuffEvent == false && OC.DebuffCard == true && CurrentEvent.SelectedOption == null)
            {
                GameControl.Instance.CreateMessage("抉择卡类型和事件组类型不符");
                return;
            }

            Selected = true;

            //是否有需要额外选择卡牌的抉择卡
            if (CurrentEvent.SelectedOption != null)
            {
                CurrentEvent.ExtraSelectedOptions.Add(this);
                CurrentEvent.SelectedOption.OC.SelectEffectActive(CurrentEvent.SelectedOption);
                this.gameObject.GetComponent<Outline>().enabled = true;
                return;
            }

            //抉择卡9寻求共识，下张卡修正翻倍的效果
            if (CurrentEvent.DoubleCorrection == false)
                CurrentEvent.TotalCorrection += OC.Correction;
            else
            {
                CurrentEvent.TotalCorrection += (OC.Correction * 2);
                DoubleCorrection = true;
                CurrentEvent.DoubleCorrection = false;
                CurrentEvent.Text_Tip.transform.parent.gameObject.SetActive(false);
            }

            this.gameObject.SetActive(false);
            CurrentEvent.SelectedOptions.Add(this);
            CurrentEvent.CheckCorrectionUI();

            //最后看一下有没有特殊效果（重写函数中为空则没效果）
            OC.SelectEffectActive(this);
        }
    }

    //随机一个卡牌
    public void RandomOption(ChoiceEvent CEvent)
    {
        if (CEvent.Options.Count == 0)
        {
            foreach (OptionCardInfo oci in CEvent.UsedOptions)
            {
                CEvent.Options.Add(oci);
            }
            CEvent.UsedOptions.Clear();
        }

        OptionCardInfo info = CEvent.Options[Random.Range(0, CEvent.Options.Count)];
        SetInfo(info, CEvent);
        CEvent.Options.Remove(info);
        CEvent.UsedOptions.Add(info);

        CEvent.Text_OptionCardTip.text = "抽牌堆剩余:" + CEvent.Options.Count + "张";

        //暂时不加负面特质效果
        return;

        //有随机负面特质效果的抉择卡随机一个特质
        if (OC.MentalityEffect == true)
        {
            RandomPerk();
        }
    }

    //随机一个负面特质
    public void RandomPerk()
    {
        int num = Random.Range(0, PerkData.DebuffPerkList.Count);
        ProvidePerk = PerkData.DebuffPerkList[num].Clone();
        Text_Emp.text = "获得特质:" + ProvidePerk.Name + "\n特质效果:" + ProvidePerk.Description;
        Text_Emp.gameObject.SetActive(true);
    }

    //为目标提供负面特质
    public void TargetAddPerk(Employee emp)
    {
        if (OC.MentalityEffect == true && emp != null)
        {
            emp.Mentality -= 20;
            return;
        }
        if (ProvidePerk == null || emp == null)
            return;

        emp.InfoDetail.AddPerk(ProvidePerk);
    }
}
