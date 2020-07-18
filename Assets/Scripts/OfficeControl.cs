using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OfficeControl : MonoBehaviour
{
    public Employee CurrentManager;
    public DepSelect DS;
    public Building building;
    public Text Text_OfficeName, Text_EmpName, Text_MAbility;
    public int ManageValue = 0;

    public List<DepControl> ControledDeps = new List<DepControl>();

    public void SetOfficeStatus()
    {
        if (CurrentManager != null)
        {
            Text_EmpName.text = "当前高管:" + CurrentManager.Name;
            Text_MAbility.text = "管理能力:" + CurrentManager.Manage;
            ManageValue = CurrentManager.Manage;
        }
        else
        {
            Text_EmpName.text = "当前高管:无";
            Text_MAbility.text = "管理能力:--";
            ManageValue = 0;
        }

    }

    public void CheckManage()
    {
        if (CurrentManager != null)
            ManageValue = CurrentManager.Manage;
        for(int i = 0; i < ControledDeps.Count; i++)
        {
            if (i < ManageValue)
            {
                ControledDeps[i].canWork = true;
                ControledDeps[i].OfficeWarning.SetActive(false);
            }
            else
            {
                ControledDeps[i].canWork = false;
                ControledDeps[i].OfficeWarning.SetActive(true);
            }
        }
    }
}
