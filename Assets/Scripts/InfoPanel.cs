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
            if (rect.anchoredPosition.x > 540)
                rect.pivot = new Vector2(1, 1);
            else
                rect.pivot = new Vector2(0, 1);
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
