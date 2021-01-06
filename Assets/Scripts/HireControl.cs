using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HireControl : MonoBehaviour
{
    public int MaxHireNum = 1;

    public GameControl GC;
    public Button HireRefreshButton;
    public Text Text_HireButtonText;
    public EmpInfo EmpInfoPrefab, EmpDetailPrefab, CEOInfoPrefab;

    public EmpInfo[] HireInfos = new EmpInfo[5];
    List<HireType> HireTypes = new List<HireType>();

    int CurrentHireNum;

    private void Start()
    {
        InitCEO();
        AddHireTypes(new HireType(1));
        RefreshHire();
        //初始的员工
        for (int i = 0; i < 5; i++)
        {
            if (i < 2)
            {
                //下面两个StarType是为了临时设定技能树
                if (i == 0)
                {
                    HireInfos[i].emp.StarType = 2;
                    HireInfos[i].emp.InitStar(2);
                    HireInfos[i].emp.Character[0] = GC.CurrentEmployees[0].Character[0];
                    HireInfos[i].emp.Character[1] = GC.CurrentEmployees[0].Character[1];
                    HireInfos[i].emp.ChangeCharacter(0, 0);
                }
                else if (i == 1)
                {
                    HireInfos[i].emp.StarType = 6;
                    HireInfos[i].emp.InitStar(6);
                    HireInfos[i].emp.Character[0] = GC.CurrentEmployees[0].Character[0];
                    HireInfos[i].emp.Character[1] = GC.CurrentEmployees[0].Character[1];
                    HireInfos[i].emp.ChangeCharacter(0, 0);
                }
                GC.CurrentEmpInfo = HireInfos[i];
                SetInfoPanel();
                GC.CurrentEmpInfo.emp.InfoA.transform.parent = GC.StandbyContent;
            }
            HireInfos[i].gameObject.SetActive(false);
        }
        GC.CurrentEmpInfo = null;
    }

    //添加从人力资源部获得的招聘
    public void AddHireTypes(HireType ht)
    {
        HireTypes.Add(ht);
        HireRefreshButton.interactable = true;
        Text_HireButtonText.color = Color.red;
    }

    //刷新招聘
    public void RefreshHire()
    {
        if (HireTypes.Count > 0)
        {
            CurrentHireNum = 0;
            EmpType EType;
            if (HireTypes[0].Type == 1)
                EType = EmpType.Tech;
            else if (HireTypes[0].Type == 2)
                EType = EmpType.Market;
            else if (HireTypes[0].Type == 3)
                EType = EmpType.Product;
            else
                EType = EmpType.Operate;

            for (int i = 0; i < 5; i++)
            {
                HireInfos[i].gameObject.SetActive(true);
                foreach (Transform child in HireInfos[i].PerkContent)
                {
                    Destroy(child.gameObject);
                }
                foreach (Transform child in HireInfos[i].SkillContent)
                {
                    Destroy(child.gameObject);
                }
                foreach (Transform child in HireInfos[i].StrategyContent)
                {
                    Destroy(child.gameObject);
                }
                HireInfos[i].PerksInfo.Clear();
                HireInfos[i].SkillsInfo.Clear();
                HireInfos[i].StrategiesInfo.Clear();
                HireInfos[i].CreateEmp(EType, HireTypes[0].HST, HireTypes[0].Level);

                if(i > HireTypes[0].HireNum - 1)
                {
                    HireInfos[i].gameObject.SetActive(false);
                }
            }
            HireTypes.RemoveAt(0);
            if (HireTypes.Count < 1)
                HireRefreshButton.interactable = false;
        }
        if(HireTypes.Count == 0)
            Text_HireButtonText.color = Color.black;
    }

    //(Hire)招聘后信息转移和创建信息面板
    public void SetInfoPanel()
    {
        GC.CurrentEmpInfo.HireButton.interactable = false;

        EmpInfo ED = Instantiate(EmpDetailPrefab, GC.EmpDetailContent);
        GC.CurrentEmpInfo.CopyStatus(ED);

        EmpInfo EI1 = Instantiate(EmpInfoPrefab, GC.TotalEmpContent);
        GC.CurrentEmpInfo.CopyStatus(EI1);

        EmpInfo EI2 = Instantiate(EmpInfoPrefab, GC.TotalEmpContent);
        GC.CurrentEmpInfo.CopyStatus(EI2);

        EI1.DetailInfo = ED;
        EI2.DetailInfo = ED;
        ED.DetailInfo = ED;
        ED.emp.InfoA = EI1;
        ED.emp.InfoB = EI2;
        ED.emp.InfoDetail = ED;
        ED.emp.InitRelation();
        GC.HourEvent.AddListener(ED.emp.TimePass);
        ED.SetSkillName();
        //检测技能
        ED.ST.InitSkill();

        //创建员工实体
        ED.Entity = EmpManager.Instance.CreateEmp(BuildingManage.Instance.ExitPos.position);
        ED.Entity.SetInfo(ED);

        //注意应放在初始化人际关系后再添加至链表
        GC.CurrentEmployees.Add(GC.CurrentEmpInfo.emp);
        //复制特质
        for (int i = 0; i < GC.CurrentEmpInfo.PerksInfo.Count; i++)
        {
            GC.CurrentEmpInfo.PerksInfo[i].CurrentPerk.AddEffect();
            GC.CurrentEmpInfo.PerksInfo[i].transform.parent = ED.PerkContent;
            ED.PerksInfo.Add(GC.CurrentEmpInfo.PerksInfo[i]);
        }
        GC.CurrentEmpInfo.PerksInfo.Clear();

        //删除展示用能力
        for (int i = 0; i < GC.CurrentEmpInfo.SkillsInfo.Count; i++)
        {
            Destroy(GC.CurrentEmpInfo.SkillsInfo[i].gameObject);
        }
        GC.CurrentEmpInfo.SkillsInfo.Clear();
        //复制战略
        for (int i = 0; i < GC.CurrentEmpInfo.StrategiesInfo.Count; i++)
        {
            GC.CurrentEmpInfo.StrategiesInfo[i].transform.parent = ED.StrategyContent;
            ED.StrategiesInfo.Add(GC.CurrentEmpInfo.StrategiesInfo[i]);
        }
        GC.CurrentEmpInfo.StrategiesInfo.Clear();
        //创建招聘历史
        if (GC.CC.CEO != null)
            GC.CC.CEO.InfoDetail.AddHistory("招聘了" + ED.emp.Name);
        HideOptions();
    }

    //初始化CEO
    public void InitCEO()
    {
        Employee emp = new Employee();
        emp.InitCEOStatus();

        EmpInfo ED = Instantiate(EmpDetailPrefab, GC.EmpDetailContent);
        ED.GC = GC;
        ED.emp = emp;
        ED.SetSkillName();
        GC.CC.CEO = emp;

        EmpInfo EI1 = Instantiate(CEOInfoPrefab, GC.TotalEmpContent);
        ED.CopyStatus(EI1);

        EmpInfo EI2 = Instantiate(CEOInfoPrefab, GC.TotalEmpContent);
        ED.CopyStatus(EI2);

        emp.InfoDetail = ED;
        emp.InfoA = EI1;
        emp.InfoB = EI2;
        EI1.DetailInfo = ED;
        EI2.DetailInfo = ED;
        EI1.transform.parent = GC.StandbyContent;
        emp.InitRelation();
        //创建特质和技能
        emp.StarType = 13;//临时设定CEO技能树
        emp.InitStar(13);
        ED.InitSkillAndStrategy();

        //创建员工实体
        ED.Entity = EmpManager.Instance.CreateEmp(BuildingManage.Instance.ExitPos.position);
        ED.Entity.SetInfo(ED);

        GC.CurrentEmployees.Add(emp);

        emp.CurrentOffice = GC.CurrentOffices[0];
        GC.CurrentOffices[0].CurrentManager = emp;
        GC.CurrentOffices[0].SetOfficeStatus();
        GC.HourEvent.AddListener(emp.TimePass);
    }

    //招聘后隐藏其它选项
    public void HideOptions()
    {
        CurrentHireNum += 1;
        if (CurrentHireNum >= MaxHireNum)
        {
            for (int i = 0; i < 5; i++)
            {
                HireInfos[i].gameObject.SetActive(false);
            }
        }
    }
}
