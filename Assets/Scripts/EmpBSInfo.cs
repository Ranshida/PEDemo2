using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmpBSInfo : MonoBehaviour
{
    public int Type = 1;//1核心团队面板信息，2头脑风暴面板信息


    public BrainStormControl BSC;
    public Employee emp;
    public Transform MarkerContent, PerkContent;
    public GameObject EmpStatus, EmpSetSign;
    public Image Photo;
    public Text Text_Name, Text_Mentality, Text_Dead, Text_SkillLimit, Text_Manage, Text_Decision, Text_Division, Text_Tenacity, Text_Occupation;
    public Button UpgradeDiceButton, AddExpButton;

    public List<BSSkillMarker> SkillMarkers = new List<BSSkillMarker>();
    public List<PerkInfo> CurrentPerks = new List<PerkInfo>();
    public Sprite[] Faces = new Sprite[10];


    private void Update()
    {
        if(emp != null)
        {
            Text_Name.text = emp.Name;
            Text_Mentality.text = "心力" + emp.Mentality + "/" + emp.MentalityLimit;
            //显示退场时就不显示禁用了
            if (Type == 2)
            {
                if (emp.Mentality == 0)
                {
                    Text_Dead.gameObject.SetActive(true);
                    Text_SkillLimit.gameObject.SetActive(false);
                }
                else
                {
                    Text_Dead.gameObject.SetActive(false);
                    if (emp.SkillLimitTime > 0)
                    {
                        Text_SkillLimit.text = "技能被禁用(" + emp.SkillLimitTime + "回合)";
                        Text_SkillLimit.gameObject.SetActive(true);
                    }
                    else
                        Text_SkillLimit.gameObject.SetActive(false);
                }
            }
            else
            {
                Text_Manage.text = "管理:" + emp.Manage;
                Text_Decision.text = "决策:" + emp.Decision;
                Text_Tenacity.text = "坚韧:" + emp.Tenacity;
                Text_Occupation.text = "职业:";
                if (emp.CurrentDivision != null)
                    Text_Division.text = "事业部:" + emp.CurrentDivision.DivName;
                else
                    Text_Division.text = "事业部:" + "无";
            }
        }
    }

    //开始说服员工加入
    public void SetEmp()
    {
        BSC.CurrentBSInfo = this;
        BSC.GC.CC.SetCEOSkill(20);
    }
    //添加对应员工至核心团队
    public void EmpJoin(Employee target)
    {
        emp = target;
        EmpStatus.SetActive(true);
        EmpSetSign.SetActive(false);
        this.GetComponent<Button>().enabled = false;
        if (Type == 1)
        {
            BSC.CoreMembers.Add(target);
            BSC.CurrentBSInfo = null;
        }
        CheckMarker();
        CheckPerk();
        Photo.sprite = Faces[emp.InfoDetail.Entity.FaceType];
        //金色特质检测
        foreach(PerkInfo p in target.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 134)
                BSC.GC.CC.CEO.Strength += 2;
            else if (p.CurrentPerk.Num == 135)
                BSC.GC.CC.CEO.Decision += 2;
            else if (p.CurrentPerk.Num == 136)
                BSC.GC.CC.CEO.Tenacity += 2;
            else if (p.CurrentPerk.Num == 137)
                BSC.GC.CC.CEO.Manage += 2;
            else if (p.CurrentPerk.Num == 138)
                BSC.GC.CC.CEO.Strength -= 2;
            else if (p.CurrentPerk.Num == 139)
                BSC.GC.CC.CEO.Tenacity -= 2;
            else if (p.CurrentPerk.Num == 140)
                BSC.GC.CC.CEO.Manage -= 2;
            else if (p.CurrentPerk.Num == 141)
                BSC.GC.CC.CEO.Decision -= 2;
        }
    }

    //开始说服员工离开
    public void RemoveEmp()
    {
        if(emp.CoreMemberTime > 0)
        {
            BSC.GC.QC.Init("该员工说服冷却中(剩余" + emp.CoreMemberTime + "时),无法进行说服");
            return;
        }
        BSC.CurrentBSInfo = this;
        BSC.GC.CC.SetCEOSkill(21);
    }
    //员工离开核心团队
    public void EmpLeft()
    {
        if (Type == 1)
        {
            BSC.CoreMembers.Remove(emp);
            BSC.CurrentBSInfo = null;
        }
        foreach (PerkInfo p in emp.InfoDetail.PerksInfo)
        {
            if (p.CurrentPerk.Num == 134)
                BSC.GC.CC.CEO.Strength -= 2;
            else if (p.CurrentPerk.Num == 135)
                BSC.GC.CC.CEO.Decision -= 2;
            else if (p.CurrentPerk.Num == 136)
                BSC.GC.CC.CEO.Tenacity -= 2;
            else if (p.CurrentPerk.Num == 137)
                BSC.GC.CC.CEO.Manage -= 2;
            else if (p.CurrentPerk.Num == 138)
                BSC.GC.CC.CEO.Strength += 2;
            else if (p.CurrentPerk.Num == 139)
                BSC.GC.CC.CEO.Tenacity += 2;
            else if (p.CurrentPerk.Num == 140)
                BSC.GC.CC.CEO.Manage += 2;
            else if (p.CurrentPerk.Num == 141)
                BSC.GC.CC.CEO.Decision += 2;
        }
        emp = null;
        EmpStatus.SetActive(false);
        EmpSetSign.SetActive(true);
        this.GetComponent<Button>().enabled = true;
    }
    
    //根据员工能力生成骰子
    public void CreateDices()
    {
        foreach(int num in emp.Professions)
        {
            //没有对应技能不继续算
            if (num == 0)
                continue;
            //技能为0则不继续算
            if (num == 0)
                continue;

            int type = 0;
            if (num == 8)
                type = 0;
            else if (num == 9)
                type = 1;
            else if (num == 16)
                type = 2;
            else if (num == 1 || num == 3 || num == 11)
                type = 3;
            else if (num == 2 || num == 13)
                type = 4;
            else if (num == 12 || num == 15)
                type = 5;

            BSC.InitDice(new int[6] { type, type, type, -1, -1, -1 });
        }
    }

    //设置技能图标
    public void CheckMarker()
    {
        for(int i = 0; i < SkillMarkers.Count; i++)
        {
            Destroy(SkillMarkers[i].gameObject);
        }
        SkillMarkers.Clear();
        foreach (int num in emp.Professions)
        {

            int type = 0;
            if (num == 8)
                type = 0;
            else if (num == 9)
                type = 1;
            else if (num == 16)
                type = 2;
            else if (num == 1 || num == 3 || num == 11)
                type = 3;
            else if (num == 2 || num == 13)
                type = 4;
            else if (num == 12 || num == 15)
                type = 5;

            BSSkillMarker m = Instantiate(BSC.MarkerPrefab, MarkerContent).GetComponent<BSSkillMarker>();
            m.SetInfo(type, 3);
            SkillMarkers.Add(m);
        }
    }

    //检测并刷新特质
    public void CheckPerk()
    {
        foreach(PerkInfo info in CurrentPerks)
        {
            Destroy(info.gameObject);
        }
        CurrentPerks.Clear();

        if(emp != null)
        {
            foreach(PerkInfo info in emp.InfoDetail.PerksInfo)
            {
                if(info.CurrentPerk.ManagePerk == true)
                {
                    PerkInfo newInfo = Instantiate(BSC.GC.PerkInfoPrefab, PerkContent);
                    newInfo.CurrentPerk = info.CurrentPerk;
                    newInfo.Text_Name.text = info.CurrentPerk.Name;
                    newInfo.info = BSC.GC.infoPanel;
                    CurrentPerks.Add(newInfo);
                }
            }
        }
    }

    //调整岗位
    public void StartMove()
    {
        BSC.GC.CurrentEmpInfo = emp.InfoA;
        BSC.GC.SelectMode = 2;
        BSC.GC.ShowDepSelectPanel(emp, true);
    }

    //升级骰子
    public void UpgradeDice()
    {

    }

    //获得经验
    public void AddExp()
    {

    }
}
