using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CWCardInfo : MonoBehaviour
{
    public CWCard CurrentCard;
    public Image img;
    public Text Text_Name, Text_Description, Text_EfficiencyDebuff, Text_WorkStatusDebuff, Text_CardStatus;
    public DivisionControl DC;
    public CWCardLibrary CWCL;
    public GameObject DetailInfo, ShowDetailButton, ReplaceButton;

    public bool Weak = false;
    public int CurrentLevel = 1;
    public int CurrentNum = 1;
    public int InfoType = 1;//1事业部信息 2卡牌库信息 3购买和详细信息

    //修改等级
    public void ChangeLevel(bool Add)
    {
        if (Add == true && CurrentNum > 0)
        {
            if (CurrentLevel < 3)
                CurrentLevel += 1;
        }
        else
        {
            if (CurrentLevel > 1)
                CurrentLevel -= 1;
        }
        if (InfoType == 1)
            DC.ExtraStatusCheck();
        UpdateUI();
    }
    //修改卡牌张数
    public void ChangeNum(bool Add)
    {
        if (Add == true)
        {
            if (CurrentNum < 3)
                CurrentNum += 1;
        }
        else
        {
            if (CurrentNum > 0)
            {
                CurrentNum -= 1;
                if (CurrentNum == 0)
                    CurrentLevel = 1;
            }
        }
        DC.ExtraStatusCheck();
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (CurrentCard != null)
        {
            Text_Name.text = CurrentCard.Name;
            Text_Description.text = CurrentCard.Description;
            if (InfoType == 1)
            {
                Text_WorkStatusDebuff.text = CurrentCard.WorkStatusDebuff[CurrentLevel - 1].ToString();
                Text_EfficiencyDebuff.text = (CurrentCard.EfficiencyDebuff[CurrentLevel - 1] * CurrentNum).ToString();
                Text_CardStatus.text = CurrentNum + "张\n\n" + CurrentLevel + "级";
            }
            else if (InfoType == 2)
            {
                if (CurrentCard.CurrentDiv == null)
                    Text_CardStatus.text = "未放置";
                else
                    Text_CardStatus.text = "当前所在事业部:" + CurrentCard.CurrentDiv.DivName;
            }
            else if (InfoType == 3)
            {
                Text_EfficiencyDebuff.text = "事业部工作状态" + CurrentCard.WorkStatusDebuff[CurrentLevel - 1] 
                    + "\n每1张事业部效率" + CurrentCard.EfficiencyDebuff[CurrentLevel - 1];
                Text_CardStatus.text = "岗位优势为" + CurrentCard.TypeRequire + "的员工" + CurrentCard.EmpNumRequire + "名";
            }
        }
    }

    //设置和移除卡牌
    public void SetInfo(CWCard card)
    {
        //设置卡牌
        if (card != null)
        {
            CurrentCard = card;
            card.CurrentDiv = DC;
            DC.CWDep.EmpEffectCheck();
            DC.ExtraStatusCheck();
            DetailInfo.SetActive(true);
            WeakCheck(DC.CWDep);
        }
        //移除卡牌
        else if(CurrentCard != null)
        {
            DetailInfo.SetActive(false);
            CurrentNum = 1;
            CurrentLevel = 1;
            RemoveWeakEffect();
            CurrentCard.CurrentDiv = null;
            CurrentCard = null;
            DC.ExtraStatusCheck();
        }
        UpdateUI();
    }

    //DepControl的检测调用
    public void WeakCheck(DepControl dep)
    {
        //没人的时候直接移除弱化效果
        if (dep.CurrentEmps.Count == 0)
        {
            RemoveWeakEffect();
            return;
        }

        //有人的时候进行检测
        int count = 0;
        foreach (Employee emp in dep.CurrentEmps)
        {
            foreach (int p in emp.Professions)
            {
                if (p == (int)CurrentCard.TypeRequire)
                {
                    count += 1;
                    continue;
                }
            }
        }
        if (count >= CurrentCard.EmpNumRequire)
            RemoveWeakEffect();
        else
            AddWeakEffect();

    }
    //弱化效果的添加和移除
    private void AddWeakEffect()
    {
        if (Weak == false)
        {
            Weak = true;
            img.color = Color.red;
            CurrentCard.AddDebuff();
        }
        UpdateUI();
    }
    private void RemoveWeakEffect()
    {
        if (Weak == true)
        {
            Weak = false;
            img.color = Color.white;
            CurrentCard.RemoveDebuff();
        }
        UpdateUI();
    }

    //显示/隐藏两个按钮
    public void ShowButtons()
    {
        if (ReplaceButton != null)
            ReplaceButton.SetActive(true);
        ShowDetailButton.SetActive(true);
    }
    public void HideButtons()
    {
        if (ReplaceButton != null)
            ReplaceButton.SetActive(false);
        ShowDetailButton.SetActive(false);
    }

    //显示细节面板
    public void ShowDetailInfoPanel()
    {
        HideButtons();
        DC.GC.CWCL.ShowCardDetail(CurrentCard);
    }

    //选择插槽
    public void StartReplace()
    {
        HideButtons();
        DC.GC.CWCL.SelectedCard = this;
        DC.GC.CWCL.ShowLibraryPanel(this);
    }
    //替换卡牌
    public void ReplaceCard()
    {
        CWCL.ReplaceCard(this);
    }

    //购买卡牌
    public void PurchaseCard()
    {
        CWCL.AddCWCard(CurrentCard);
        this.gameObject.SetActive(false);
    }
    //删除卡牌
    public void DeleteCard()
    {
        if (CurrentCard.CurrentDiv != null)
        {
            foreach (CWCardInfo info in CurrentCard.CurrentDiv.CWCards)
            {
                if (info.CurrentCard == CurrentCard)
                {
                    info.SetInfo(null);
                    break;
                }
            }
        }
        CWCL.CurrentCards.Remove(this);
        Destroy(this.gameObject);
    }
}
