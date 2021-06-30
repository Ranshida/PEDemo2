using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EmpEffect : MonoBehaviour
{
    public int MaxPro1 = 0, MaxPro2 = 0;//最大岗位优势计数

    public Image Marker1, Marker2;
    public Text Text_Effect, Text_Debuff, Text_Pro1, Text_Pro2;
    public InfoPanelTrigger iptEffect, iptDebuff;
    public DepControl dep;
    public GameObject EmpTip, ReplaceButton, ReplaceButton2, ProButton1, ProButton2;
    public EmpEffect DepMarker1, DepMarker2;
    private Employee TargetEmp, TargetEmp2;

    //根据文本分割确定显示内容
    public void InitEffect(string content)
    {
        string[] str = content.Split('|');
        if (str.Length == 2)
        {
            Text_Effect.text = str[0];
            iptEffect.enabled = true;
            iptEffect.ContentB = str[1];
        }
        else
        {
            Text_Effect.text = content;
            iptEffect.enabled = false;
        }
    }
    public void InitDebuff(string content)
    {
        string[] str = content.Split('|');
        if (str.Length == 2)
        {
            Text_Debuff.text = str[0];
            iptDebuff.enabled = true;
            iptDebuff.ContentB = str[1];
        }
        else
        {
            Text_Debuff.text = content;
            iptDebuff.enabled = false;
        }
    }

    //替换员工(如果有的话)
    public void ReplaceEmp(int num)
    {
        //不处于调整回合时无法使用
        if (dep.GC.AdjustTurn != 0 && dep.building.IndependentBuilding == false)
        {
            dep.GC.CreateMessage("未处于调整回合");
            return;
        }
        dep.GC.SelectMode = 13;
        dep.GC.CurrentDep = dep;
        if (num == 1)
        {
            if (TargetEmp != null)
                dep.GC.CurrentEmpInfo = TargetEmp.InfoDetail;
            else
                dep.GC.CurrentEmpInfo = null;
        }
        else
        {
            if (TargetEmp2 != null)
                dep.GC.CurrentEmpInfo = TargetEmp2.InfoDetail;
            else
                dep.GC.CurrentEmpInfo = null;
        }

        //独立建筑在非调整回合只能用待命的员工
        if (dep.building.IndependentBuilding == true)
        {
            foreach (Employee e in dep.GC.CurrentEmployees)
            {
                //非调整回合时只能用待命的员工
                if (dep.GC.AdjustTurn != 0 && (e.CurrentDep != null || e.CurrentDivision != null))
                {
                    e.InfoB.gameObject.SetActive(false);
                    continue;
                }
                //心理咨询室只能放符合条件的
                if (dep.building.Type == BuildingType.心理咨询室 && dep.CheckSkillType(e) == 0)
                    e.InfoB.gameObject.SetActive(false);
            }
        }

        dep.GC.TotalEmpPanel.SetWndState(true);
    }

    public void SelectEmp(int num)
    {
        if (num == 1)
        {
            if (EmpTip != null)
                EmpTip.SetActive(false);
            if (TargetEmp != null)
                TargetEmp.InfoDetail.ShowPanel();       
        }
        else
        {
            if (TargetEmp2 != null)
                TargetEmp2.InfoDetail.ShowPanel();
            if (EmpTip != null)
                EmpTip.SetActive(false);
        }
    }

    //显示/隐藏按钮提示
    public void ShowTip(int num)
    {
        if (num == 1)
        {
            if (TargetEmp != null)
            {
                EmpTip.transform.position = Marker1.transform.position;
                EmpTip.SetActive(true);
            }
        }
        else
        {
            if (TargetEmp2 != null)
            {
                EmpTip.transform.position = Marker2.transform.position;
                EmpTip.SetActive(true);
            }
        }
    }
    public void HideTip()
    {
        EmpTip.SetActive(false);
    }

    //商店部门购买面板中隐藏各种选项
    public void HideOptions()
    {
        ReplaceButton.SetActive(false);
        if (ReplaceButton2 != null)
            ReplaceButton2.SetActive(false);
        Marker1.gameObject.GetComponent<Button>().enabled = false;
        Marker1.gameObject.GetComponent<EventTrigger>().enabled = false;
        if (Marker2 != null)
        {
            Marker2.gameObject.GetComponent<Button>().enabled = false;
            Marker2.gameObject.GetComponent<EventTrigger>().enabled = false;
        }
    }

    //设置最高岗位优势等级，并根据等级设置各物体是否显示
    public void SetProfessionCount(int value, Employee emp, Image img)
    {
        if (img == Marker1)
        {
            if (value > 1)
            {
                ProButton1.SetActive(true);
                DepMarker1.ProButton1.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                ProButton1.SetActive(false);
                DepMarker1.ProButton1.transform.parent.gameObject.SetActive(false);
            }
            MaxPro1 = value;
            TargetEmp = emp;
            DepMarker1.TargetEmp = emp;
        }
        else
        {
            if (value > 1)
            {
                ProButton2.SetActive(true);
                DepMarker2.ProButton1.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                ProButton2.SetActive(false);
                DepMarker2.ProButton1.transform.parent.gameObject.SetActive(false);
            }
            MaxPro2 = value;
            TargetEmp2 = emp;
            DepMarker2.TargetEmp = emp;
        }


        if (emp != null && value < emp.ProfessionUse)
            emp.ProfessionUse = value;

        if (emp != null && emp.ProfessionUse == 0)
            emp.ProfessionUse = 1;
        UpdateProCountUI();
    }

    //增加岗位优势等级
    public void AddPCount(int num)
    {
        if (TargetEmp == null)
            return;

        //超过上限时也不能设置
        int totalcount = dep.CurrentEmps.Count;
        foreach (Employee emp in dep.CurrentEmps)
        {
            if (emp.ProfessionUse > 1)
                totalcount += (emp.ProfessionUse - 1);
        }
        if (totalcount >= dep.EmpLimit)
            return;

        if (num == 1 && TargetEmp.ProfessionUse < MaxPro1)
            TargetEmp.ProfessionUse += 1;     

        else if (num == 2 && TargetEmp2.ProfessionUse < MaxPro2)
            TargetEmp2.ProfessionUse += 1;
                 
        dep.EmpEffectCheck();
        UpdateProCountUI();
    }

    //减少岗位优势等级
    public void ReducePCount(int num)
    {
        if (TargetEmp == null)
            return;
        if (num == 1 && TargetEmp.ProfessionUse > 1)
            TargetEmp.ProfessionUse -= 1;
        else if (num == 2 && TargetEmp2.ProfessionUse > 1)
            TargetEmp2.ProfessionUse -= 1;
        dep.EmpEffectCheck();
        UpdateProCountUI();
    }

    //更新岗位优势ui
    public void UpdateProCountUI()
    {
        if (TargetEmp != null)
        {
            Text_Pro1.text = TargetEmp.ProfessionUse + "/" + MaxPro1;
            DepMarker1.Text_Pro1.text = TargetEmp.ProfessionUse + "/" + MaxPro1;
        }
        if (TargetEmp2 != null)
        {
            Text_Pro2.text = TargetEmp2.ProfessionUse + "/" + MaxPro2;
            DepMarker2.Text_Pro1.text = TargetEmp2.ProfessionUse + "/" + MaxPro2;
        }
    }

    public void SetDepMarkerRef()
    {
        if (DepMarker1 != null)
        {
            DepMarker1.ProButton1.GetComponent<Button>().onClick.AddListener(() =>
            {
                AddPCount(1);
            });
            DepMarker1.ProButton2.GetComponent<Button>().onClick.AddListener(() =>
            {
                ReducePCount(1);
            });
        }
        
        if (DepMarker2 != null)
        {
            DepMarker2.ProButton1.GetComponent<Button>().onClick.AddListener(() =>
            {
                AddPCount(2);
            });
            DepMarker2.ProButton2.GetComponent<Button>().onClick.AddListener(() =>
            {
                ReducePCount(2);
            });
        }
    }
}
