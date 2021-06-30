using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmbitionSelect : MonoBehaviour
{
    public Employee TargetEmp;
    public Text Text_Emp;

    public void SetEmp(Employee e)
    {
        TargetEmp = e;
        UIManager.Instance.OnAddNewWindow(this.gameObject.GetComponent<WindowBaseControl>());
        Text_Emp.text = "为员工" + e.Name + "选择一个新的岗位优势";
    }

    public void AddProfession(int num)
    {
        if (TargetEmp == null)
            GameControl.Instance.CreateMessage("员工不存在");
        else
        {
            TargetEmp.Professions.Add(num);
            TargetEmp.InfoDetail.CheckProfessions();
        }
        Destroy(this.gameObject);
    }
}
