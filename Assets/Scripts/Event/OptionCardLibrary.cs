using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionCardLibrary : MonoBehaviour
{
    public OptionCardInfo OptionCardPrefab;
    public Transform TotalOptionContent, DisplayContent;
    public GameObject DisplayPanel;
    public Text Text_EmpName, Text_EmpOccupation, Text_EmpDivision, Text_EmpDep, Text_LibraryInfo;
    public Image Photo, DebuffRateImage;
    private Employee DisplayedEmp;

    public List<OptionCardInfo> CurrentOptions = new List<OptionCardInfo>();
    public List<OptionCardInfo> DisplayOptions = new List<OptionCardInfo>();

    public void AddStaticOptions(OptionCard oc)
    {
        OptionCardInfo newCard = Instantiate(OptionCardPrefab, TotalOptionContent);
        newCard.SetBaseInfo(oc);
        newCard.OCL = this;
        CurrentOptions.Add(newCard);
        UpdateUI();
    }

    public void AddOptionCard(OptionCard oc, Employee emp)
    {
        OptionCardInfo newCard = Instantiate(OptionCardPrefab, TotalOptionContent);
        newCard.SetInfo(oc, emp);
        newCard.OCL = this;
        CurrentOptions.Add(newCard);
        UpdateUI();
    }

    public void ShowEmpOptions(Employee emp)
    {
        DisplayedEmp = emp;
        DisplayPanel.gameObject.SetActive(true);
        Photo.sprite = EmpManager.Instance.EmpFaces[emp.InfoDetail.Entity.FaceType];
        Text_EmpName.text = "姓名:" + emp.Name;
        Text_EmpOccupation.text = "职业:" + emp.Occupation;
        if (emp.CurrentDep != null)
        {
            Text_EmpDivision.text = "事业部:" + emp.CurrentDep.CurrentDivision.DivName;
            Text_EmpDep.text = "部门:" + emp.CurrentDep.Text_DepName.text;
        }
        else if (emp.CurrentDivision != null)
        {
            Text_EmpDivision.text = "事业部:" + emp.CurrentDep.CurrentDivision.DivName;
            Text_EmpDep.text = "部门:无";
        }
        else
        {
            Text_EmpDivision.text = "事业部:无";
            Text_EmpDep.text = "部门:无";
        }
        foreach(OptionCardInfo oc in DisplayOptions)
        {
            Destroy(oc.gameObject);
        }
        DisplayOptions.Clear();
        foreach(OptionCardInfo oc in CurrentOptions)
        {
            if(oc.Emp == emp)
            {
                OptionCardInfo newCard = Instantiate(OptionCardPrefab, DisplayContent);
                newCard.SetBaseInfo(oc.OC);
                DisplayOptions.Add(newCard);
            }
        }
    }

    public void ShowEmpDetail()
    {
        if (DisplayedEmp != null)
        {
            DisplayedEmp.InfoDetail.ShowPanel();
        }
    }

    public void UpdateUI()
    {
        if(CurrentOptions.Count == 0)
        {
            Text_LibraryInfo.text = "总抉择卡数:0\n负面抉择卡数:0\n负面概率:0%\n正面抉择卡数:0\n正面概率:0%";
            return;
        }
        int DebuffCount = 0;
        float DebuffRate = 0;
        foreach(OptionCardInfo oc in CurrentOptions)
        {
            if (oc.OC.DebuffCard == true)
                DebuffCount += 1;
        }
        DebuffRate = (float)DebuffCount / (float)CurrentOptions.Count;
        DebuffRateImage.fillAmount = DebuffRate;
        DebuffRate = Mathf.Round(DebuffRate * 100);
        Text_LibraryInfo.text = "总抉择卡数:" + CurrentOptions.Count + "\n负面抉择卡数:" + DebuffCount
            + "\n负面概率:" + DebuffRate + "%" + "\n正面抉择卡数:" + (CurrentOptions.Count - DebuffCount)
            + "\n正面概率:" + (100 - DebuffRate) + "%";
    }
}
