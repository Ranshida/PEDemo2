using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour
{
    public bool Visible = false;

    public Text Text_Name, Text_Description, Text_ExtraInfo;
    public CanvasGroup CG;

    private void Start()
    {
        CG = this.GetComponent<CanvasGroup>();
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
