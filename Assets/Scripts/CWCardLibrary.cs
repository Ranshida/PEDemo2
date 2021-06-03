using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CWCardLibrary : MonoBehaviour
{
    public CWCardInfo SelectedCard;
    public CWCardInfo CWCInfoPrefab;
    public GameObject EmptyOption;
    public Transform CardContent;
    public WindowBaseControl DetailPanel;

    public List<CWCardInfo> CurrentCards = new List<CWCardInfo>(), DetailCards = new List<CWCardInfo>();

    public WindowBaseControl WBC;

    private void Start()
    {
        WBC = this.gameObject.GetComponent<WindowBaseControl>();
    }

    //添加一张卡到库
    public void AddCWCard(CWCard card)
    {
        CWCardInfo info = Instantiate(CWCInfoPrefab, CardContent);
        info.CurrentCard = card;
        info.UpdateUI();
        info.CWCL = this;
        CurrentCards.Add(info);
    }

    //显示详细信息
    public void ShowCardDetail(CWCard card)
    {
        DetailPanel.SetWndState(true);
        foreach (CWCardInfo info in DetailCards)
        {
            info.CurrentCard = card;
            info.UpdateUI();
            info.CurrentCard = null;
        }
    }

    //确定替换的卡牌
    public void ReplaceCard(CWCardInfo info)
    {
        WBC.SetWndState(false);

        //对应卡牌已经在其他插槽上时清空对应插槽
        if (info.CurrentCard.CurrentDiv != null)
        {
            foreach (CWCardInfo i in info.CurrentCard.CurrentDiv.CWCards)
            {
                if (i.CurrentCard == info.CurrentCard)
                {
                    i.SetInfo(null);
                    break;
                }
            }
        }

        //插槽上有卡时先清空
        if (SelectedCard.CurrentCard != null)
            SelectedCard.SetInfo(null);

        //因为还有清空选项，所以要再检测一下
        SelectedCard.SetInfo(info.CurrentCard);
    }

    //清空选项
    public void EmptyCard()
    {
        SelectedCard.SetInfo(null);
        WBC.SetWndState(false);
    }

    //显示库面板，如果是替换卡牌选择就显示清空选项
    public void ShowLibraryPanel(CWCardInfo info)
    {
        WBC.SetWndState(true);
        if (info != null)
        {
            EmptyOption.SetActive(true);
            foreach (CWCardInfo i in CurrentCards)
            {
                if (i.CurrentCard.CurrentDiv == info.DC)
                    i.gameObject.SetActive(false);
                else
                {
                    i.gameObject.SetActive(true);
                    i.UpdateUI();
                }
            }
        }
        else
        {
            EmptyOption.SetActive(false);
            foreach (CWCardInfo i in CurrentCards)
            {
                i.UpdateUI();
                i.gameObject.SetActive(true);
            }
        }
    }

    //卡牌是否都放置进插槽的检测
    public bool CardDivCheck()
    {
        foreach (CWCardInfo info in CurrentCards)
        {
            if (info.CurrentCard.CurrentDiv == null)
                return false;
        }
        return true;
    }
}
