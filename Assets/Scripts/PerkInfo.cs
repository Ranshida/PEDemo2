using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PerkInfo : MonoBehaviour
{
    public Perk CurrentPerk;
    public EmpInfo empInfo;
    public InfoPanel info ;
    public Text Text_Name;

    float Timer = 0;
    bool ShowPanel;

    private void Update()
    {
        if(ShowPanel == true)
        {
            if (Timer < 0.25f)
                Timer += Time.deltaTime;
            else
            {
                if (CurrentPerk != null)
                {
                    info.Text_Name.text = CurrentPerk.Name;
                    info.Text_Description.text = CurrentPerk.Description;
                    if (CurrentPerk.TimeLeft != -1)
                        info.Text_ExtraInfo.text = "剩余" + (CurrentPerk.TimeLeft / 8) + "周";
                }
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

    public void RemovePerk()
    {
        empInfo.PerksInfo.Remove(this);
        Destroy(this.gameObject);
    }
}
