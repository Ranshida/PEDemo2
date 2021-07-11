using UnityEngine;
using UnityEngine.UI;

public class PerkInfo : MonoBehaviour
{
    public Perk CurrentPerk;
    public EmpInfo empInfo;
    public InfoPanel info ;
    public Text Text_Name, Text_Count;

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
                if (info.Visible == false)
                    info.ShowPanel();
                info.transform.position = Input.mousePosition;
            }
        }
    }

    public void PointerEnter()
    {
        if (CurrentPerk != null)
        {
            info.Text_Name.text = CurrentPerk.Name;
            if (CurrentPerk.CompanyDebuffPerk == false)
            {
                if (CurrentPerk.Level > 1)
                    info.Text_Name.text += "(" + CurrentPerk.Level + "层)";
                if (CurrentPerk.TimeLeft != -1)
                    info.Text_ExtraInfo.text = "剩余" + CurrentPerk.TimeLeft + "回合";
                else
                    info.Text_ExtraInfo.text = "剩余∞回合";

                info.Text_Description.text = CurrentPerk.Description;
            }
            else
            {
                info.Text_ExtraInfo.text = "持有员工:\n";
                foreach (Employee emp in GameControl.Instance.CPC.CurrentDebuffPerks[CurrentPerk.Num])
                {
                    info.Text_ExtraInfo.text += emp.Name + " ";
                }

                info.Text_Description.text = CurrentPerk.SetSpecialDescription(GameControl.Instance.CPC.CurrentDebuffPerks[CurrentPerk.Num].Count);
            }
        }
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
        //从对应链表中移除自身
        if (empInfo != null)
            empInfo.PerksInfo.Remove(this);
        else if (CurrentPerk.TargetDiv != null)
            CurrentPerk.TargetDiv.CurrentPerks.Remove(this);
        else
            GameControl.Instance.CPC.CurrentPerksInfo.Remove(this);

        info.ClosePanel();
        Destroy(this.gameObject);
    }

    public void SetColor()
    {
        if (CurrentPerk == null)
            return;

        if (CurrentPerk.perkColor != PerkColor.None)
            GetComponent<Outline>().enabled = true;

        if (CurrentPerk.perkColor == PerkColor.Blue)
            Text_Name.color = Color.blue;
        else if (CurrentPerk.perkColor == PerkColor.Grey)
            Text_Name.color = Color.grey;
        else if (CurrentPerk.perkColor == PerkColor.White)
            Text_Name.color = Color.white;
        else if (CurrentPerk.perkColor == PerkColor.Orange)
            Text_Name.color = new Color(1, 0.6f, 0);
    }
}
