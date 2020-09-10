using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HireControl : MonoBehaviour
{
    public GameControl GC;
    //(Hire)招聘后信息转移
    public void SetInfoPanel()
    {
        GC.CurrentEmpInfo.HireButton.interactable = false;

        EmpInfo ED = Instantiate(GC.EmpDetailPrefab, GC.EmpDetailContent);
        GC.CurrentEmpInfo.CopyStatus(ED);

        EmpInfo EI1 = Instantiate(GC.EmpInfoPrefab, GC.TotalEmpContent);
        GC.CurrentEmpInfo.CopyStatus(EI1);

        EmpInfo EI2 = Instantiate(GC.EmpInfoPrefab, GC.TotalEmpContent);
        GC.CurrentEmpInfo.CopyStatus(EI2);

        EI1.DetailInfo = ED;
        EI2.DetailInfo = ED;
        ED.emp.InfoA = EI1;
        ED.emp.InfoB = EI2;
        ED.emp.InfoDetail = ED;
        ED.emp.InitRelation();
        ED.SetSkillName();
        //创建员工实体
        ED.Entity = Instantiate(GC.EmpEntityPrefab, GC.BM.ExitPos.position, Quaternion.Euler(0, 0, 0), GC.BM.EntityContent);
        ED.Entity.SetInfo(ED);

        //注意应放在初始化人际关系后再添加至链表
        GC.CurrentEmployees.Add(GC.CurrentEmpInfo.emp);
        //复制特质
        for (int i = 0; i < GC.CurrentEmpInfo.PerksInfo.Count; i++)
        {
            GC.CurrentEmpInfo.PerksInfo[i].CurrentPerk.AddEffect();
            GC.CurrentEmpInfo.PerksInfo[i].transform.parent = ED.PerkContent;
        }
        ED.PerksInfo = GC.CurrentEmpInfo.PerksInfo;
        //复制能力
        for (int i = 0; i < GC.CurrentEmpInfo.SkillsInfo.Count; i++)
        {
            GC.CurrentEmpInfo.SkillsInfo[i].transform.parent = ED.SkillContent;
        }
        ED.SkillsInfo = GC.CurrentEmpInfo.SkillsInfo;
        //复制战略
        for (int i = 0; i < GC.CurrentEmpInfo.StrategiesInfo.Count; i++)
        {
            GC.CurrentEmpInfo.StrategiesInfo[i].transform.parent = ED.StrategyContent;
        }
        ED.StrategiesInfo = GC.CurrentEmpInfo.StrategiesInfo;
    }
}
