using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CItemPurchaseInfo : MonoBehaviour
{
    public Text Text_Name, Text_Description, Text_Node;
    public Item CurrentItem;
    public Button PurchaseButton;
    public CourseNode CurrentNode;

    public void SetInfo(CourseNode node)
    {
        CurrentItem = ItemData.Items[Random.Range(0, ItemData.Items.Count)];
        CurrentNode = node;

        Text_Name.text = CurrentItem.Name;
        Text_Description.text = CurrentItem.Description;
        if (node.Type == CourseNodeType.货运浮岛平台)
            Text_Node.text = "位于货运浮岛平台";
        else if (node.Type == CourseNodeType.走私船)
            Text_Node.text = "位于" + node.Text_Name.text;

        this.gameObject.SetActive(true);
    }

    public void PurchaseItem()
    {
        if (GameControl.Instance.Money >= 50)
            GameControl.Instance.Money -= 50;
        else
        {
            GameControl.Instance.CreateMessage("金钱不足");
            return;
        }
        GameControl.Instance.CreateItem(CurrentItem.Num);
        this.gameObject.SetActive(false);
    }
}
