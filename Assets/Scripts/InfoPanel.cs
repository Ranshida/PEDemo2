using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour
{
    public bool Visible = false;

    public Text Text_Name, Text_Description, Text_ExtraInfo;
    public CanvasGroup CG;

    RectTransform rect;

    private void Start()
    {
        CG = this.GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if(Visible == true)
        {
            int pX = 1, pY = 1;
            if (rect.anchoredPosition.x > 540)
                pX = 1;
            else
                pX = 0;
            if (rect.sizeDelta.y - rect.anchoredPosition.y > 540)
                pY = 0;
            else
                pY = 1;
            rect.pivot = new Vector2(pX, pY);
            transform.position = Input.mousePosition;
        }
    }

    public void ShowPanel()
    {
        //通过强制刷新，及时根据文本大小调整框体尺寸
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());
        CG.alpha = 1;
        Visible = true;
    }

    public void ClosePanel()
    {
        Visible = false;
        CG.alpha = 0;
    }
}
