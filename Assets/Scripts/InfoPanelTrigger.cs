using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanelTrigger : MonoBehaviour
{
    public InfoPanel info;
    public string ContentA = "", ContentB = "", ContentC = "";

    float Timer = 0;
    bool ShowPanel;

    public void Init()
    {
        info = GameControl.Instance.infoPanel;
    }

    private void Start()
    {
        info = GameControl.Instance.infoPanel;
    }

    private void Update()
    {
        if(info == null)
            info = GameControl.Instance.infoPanel;
        if (ShowPanel == true)
        {
            if (Timer < 0.25f)
                Timer += Time.deltaTime;
            else
            {
                info.Text_Name.text = ContentA;
                info.Text_Description.text = ContentB;
                info.Text_ExtraInfo.text = ContentC;

                if (info.Visible == false)
                    info.ShowPanel();
                info.transform.position = Input.mousePosition;
            }
        }
    }

    public void PointerEnter()
    {
        ShowPanel = true;
    }

    public void PointerExit()
    {
        ShowPanel = false;
        info.ClosePanel();
        Timer = 0;
    }
}
