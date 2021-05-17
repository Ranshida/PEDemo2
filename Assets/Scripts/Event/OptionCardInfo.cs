using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionCardInfo : MonoBehaviour
{
    private int SelectType = 0;//确认选择（按钮）类型
    public bool Selected = false;//(在抉择事件面板)是否被选中
    public bool NoEffect = false;//是否不会产生负面状态（抉择卡10效果）

    public Outline outline;
    public OptionCardLibrary OCL;
    public OptionCard OC;
    public Employee Emp;
    public EventGroup EG;
    public ChoiceEvent CurrentEvent;
    public Perk ProvidePerk;//抉择卡效果中可能提供的随机特质的存储
    public Text Text_Name, Text_Description, Text_Correction, Text_Emp, Text_Index;

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

    public void SelectOptionCard(bool SkipSelectEffect = false)
    {
        //抉择卡库界面信息展示
        if(SelectType == 1 && Emp != null)
        {
            OCL.ShowEmpOptions(Emp);
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
            if (Selected == true)
            {
                Selected = false;
                outline.enabled = false;
                CurrentEvent.SelectedOptions.Remove(this);
                if (CurrentEvent.BonusCards.Count > 0)
                {
                    foreach (int num in CurrentEvent.BonusCards)
                    {
                        if (num == CurrentEvent.SelectedOptions.Count)
                        {
                            CurrentEvent.TotalCorrection -= OC.Correction * 2;
                            break;
                        }
                    }
                }
                else
                    CurrentEvent.TotalCorrection -= OC.Correction;

                //重新设置自身和其他抉择卡编号
                Text_Index.text = "";
                for (int i = 0; i < CurrentEvent.SelectedOptions.Count; i++)
                {
                    CurrentEvent.SelectedOptions[i].Text_Index.text = (i + 1).ToString();
                }

                if (SkipSelectEffect == false)
                    SelectEffectCheck(false);              
            }
            else
            {
                Selected = true;
                outline.enabled = true;
                if (CurrentEvent.BonusCards.Count > 0)
                {
                    foreach(int num in CurrentEvent.BonusCards)
                    {
                        if (num == CurrentEvent.SelectedOptions.Count)
                        {
                            CurrentEvent.TotalCorrection += OC.Correction * 2;
                            break;
                        }
                    }
                }
                else
                    CurrentEvent.TotalCorrection += OC.Correction;
                CurrentEvent.SelectedOptions.Add(this);
                if (SkipSelectEffect == false)
                    SelectEffectCheck(true);
                Text_Index.text = CurrentEvent.SelectedOptions.Count.ToString();
            }
            CurrentEvent.CheckCorrectionUI();
        }
    }

    void SelectEffectCheck(bool Active)
    {//效果细节根据num在Option里找对应描述
        if (Active == true)
        {
            //先判断选择一张替换的效果
            if (CurrentEvent.SelectedOption != null && CurrentEvent.SelectedOption.OC.Num == 13 && CurrentEvent.SelectedOption != this)
            {
                SelectOptionCard(true);
                RandomOption(CurrentEvent);
                CurrentEvent.SelectedOption.SelectOptionCard(true);
                CurrentEvent.SelectedOption.RandomOption(CurrentEvent);
                CurrentEvent.SelectedOption = null;
                return;
            }

            if (OC.Num == 8)
            {
                foreach (OptionCardInfo info in CurrentEvent.Options)
                {
                    if (info.Selected == true)
                        info.SelectOptionCard();
                }
                foreach (OptionCardInfo info in CurrentEvent.Options)
                {
                    info.RandomOption(CurrentEvent);
                }
            }
            else if (OC.Num == 9)
                CurrentEvent.BonusCards.Add(CurrentEvent.SelectedOptions.IndexOf(this) + 1);
            else if (OC.Num == 10)
                CurrentEvent.NoDebuffPerkCount += 1;
            else if (OC.Num == 11)
            {
                SelectOptionCard();
                int correction = OC.Correction;
                if (CurrentEvent.BonusCards.Count > 0)
                {
                    foreach (int num in CurrentEvent.BonusCards)
                    {
                        if (num == CurrentEvent.SelectedOptions.Count)
                        {
                            correction *= 2;
                            break;
                        }
                    }
                }
                CurrentEvent.TotalCorrection += correction;
                CurrentEvent.ExtraCorrectionContent += OC.Name + ":  +" + correction + "修正\n";
                RandomOption(CurrentEvent);
            }
            else if (OC.Num == 13)
            {
                CurrentEvent.SelectedOption = this;
                CurrentEvent.TipPanel.SetActive(true);
            }
        }
        else if (Active == false)
        {
            if (OC.Num == 9)
                CurrentEvent.BonusCards.Remove(CurrentEvent.SelectedOptions.IndexOf(this) + 1);
            else if (OC.Num == 10)
                CurrentEvent.NoDebuffPerkCount -= 1;
            else if (OC.Num == 13)
            {
                CurrentEvent.SelectedOption = null;
                CurrentEvent.TipPanel.SetActive(false);
            }
        }
    }

    public void RandomOption(ChoiceEvent CEvent)
    {
        OptionCardInfo info = GameControl.Instance.OCL.CurrentOptions[Random.Range(0, GameControl.Instance.OCL.CurrentOptions.Count)];
        SetInfo(info, CEvent);

        //有随机负面特质效果的抉择卡随机一个特质
        if (OC.RandomPerk == true)
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
        if (ProvidePerk == null || emp == null)
            return;

        emp.InfoDetail.AddPerk(ProvidePerk);
    }
}
