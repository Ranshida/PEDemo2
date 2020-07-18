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
                if (info.gameObject.activeSelf == false)
                    info.gameObject.SetActive(true);
                if (CurrentPerk != null)
                {
                    info.Text_Name.text = CurrentPerk.Name;
                    info.Text_Description.text = CurrentPerk.Description;
                    info.Text_ExtraInfo.text = "剩余" + CurrentPerk.TimeLeft + "周";
                }
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
        info.gameObject.SetActive(false);
        Timer = 0;
    }

    public void RemovePerk()
    {
        empInfo.PerksInfo.Remove(this);
        Destroy(this.gameObject);
    }
}
