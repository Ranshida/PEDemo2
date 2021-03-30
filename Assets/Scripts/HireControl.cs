using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HireControl : MonoBehaviour
{
    public int MaxHireNum = 1;//单次招聘最多选择的人数
    public int ExtraHireOption = 0;//刷新时额外提供的人选数量，单次最多3，多余的下次继承

    public Transform TotalEmpContent;
    public GameControl GC;
    public Button HireRefreshButton;
    public Text Text_HireButtonText;
    public EmpInfo EmpInfoPrefab, EmpDetailPrefab, CEOInfoPrefab;

    public EmpInfo[] HireInfos = new EmpInfo[5];
    List<HireType> HireTypes = new List<HireType>();

    int CurrentHireNum;//用于计算单次招聘已选择的人数

    private void Start()
    {
        InitCEO();
        AddHireTypes(new HireType());
        RefreshHire();
        //初始的员工
        for (int i = 0; i < 5; i++)
        {
            if (i < 2)
            {
                //先重置所有专业
                for(int j = 0; j < HireInfos[i].emp.Professions.Length; j++)
                {
                    HireInfos[i].emp.Professions[j] = 0;
                }
                //设定初始获得的两个员工的属性
                if (i == 0)
                {
                    HireInfos[i].emp.Character[0] = 0;
                    HireInfos[i].emp.Character[1] = 0;
                    HireInfos[i].emp.ChangeCharacter(0, 0);
                    HireInfos[i].emp.Professions[0] = AdjustData.EmpAProfessionType;
                    HireInfos[i].emp.SetAttributes(HireInfos[i].emp.Professions[0], AdjustData.EmpAProfessionValue);
                }
                else if (i == 1)
                {
                    HireInfos[i].emp.Character[0] = GC.CurrentEmployees[0].Character[0];
                    HireInfos[i].emp.Character[1] = GC.CurrentEmployees[0].Character[1];
                    HireInfos[i].emp.ChangeCharacter(0, 0);
                    HireInfos[i].emp.Professions[0] = AdjustData.EmpBProfessionType;
                    HireInfos[i].emp.SetAttributes(HireInfos[i].emp.Professions[0], AdjustData.EmpBProfessionValue);
                }

                //再次随机剩下的两个专业技能类型并设为0级
                int[] Nst = { 1, 2, 3, 8, 9, 11, 12, 13, 15, 16 };//Nst:几个专业技能对应的编号
                int r1 = HireInfos[i].emp.Professions[0], r2 = Random.Range(0, 10);
                r2 = Nst[r2];
                while (r1 == r2)
                {
                    r2 = Random.Range(0, 10);
                    r2 = Nst[r2];
                }
                HireInfos[i].emp.Professions[1] = r2;
                HireInfos[i].emp.Professions[2] = 0;
                HireInfos[i].emp.SetAttributes(r2, 0);

                //重新随机天赋
                for (int j = 0; j < 2; j++)
                {
                    int type = 0;
                    if (j == 0)
                        type = r1;
                    else if (j == 1)
                        type = r2;
                    float Posb = Random.Range(0.0f, 1.0f);
                    if (Posb < 0.4f)
                        HireInfos[j].emp.StarLimit[type - 1] = 0;
                    else if (Posb < 0.7f)
                        HireInfos[j].emp.StarLimit[type - 1] = 1;
                    else if (Posb < 0.9f)
                        HireInfos[j].emp.StarLimit[type - 1] = 2;
                    else
                        HireInfos[j].emp.StarLimit[type - 1] = 3;
                }

                //随机分配技能点
                HireInfos[i].emp.SkillPoint = 7;
                HireInfos[i].emp.RandomSkillAssign();

                //设定一些固定属性
                HireInfos[i].emp.Age = 24;
                HireInfos[i].emp.SchoolType = 2;
                HireInfos[i].emp.MajorType = Random.Range(0, 10);
                HireInfos[i].emp.MajorType = Nst[HireInfos[i].emp.MajorType];

                GC.CurrentEmpInfo = HireInfos[i];
                SetInfoPanel();
                GC.CurrentEmpInfo.emp.InfoA.transform.parent = GC.StandbyContent;
                if (i == 0)
                {
                    GC.CurrentEmpInfo.emp.AddEmotion(EColor.Red);
                    GC.CurrentEmpInfo.emp.AddEmotion(EColor.Red);
                }
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
            if(ExtraHireOption > 2)
            {
                HireTypes[0].HireNum += 2;
                ExtraHireOption -= 2;
            }
            else
            {
                HireTypes[0].HireNum += ExtraHireOption;
                ExtraHireOption = 0;
            }

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
                HireInfos[i].CreateEmp();

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

        EmpInfo ED = UIManager.Instance.NewWindow(EmpDetailPrefab.gameObject).GetComponent<EmpInfo>();
        GC.CurrentEmpInfo.CopyStatus(ED);

        EmpInfo EI1 = Instantiate(EmpInfoPrefab, TotalEmpContent);
        GC.CurrentEmpInfo.CopyStatus(EI1);

        EmpInfo EI2 = Instantiate(EmpInfoPrefab, TotalEmpContent);
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

        //检查是否存在瓶颈
        ED.emp.BottleNeckCheck();

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

        EmpInfo ED = UIManager.Instance.NewWindow(EmpDetailPrefab.gameObject).GetComponent<EmpInfo>();
        ED.GC = GC;
        ED.emp = emp;
        ED.SetSkillName();
        GC.CC.CEO = emp;

        EmpInfo EI1 = Instantiate(CEOInfoPrefab, TotalEmpContent);
        ED.CopyStatus(EI1);

        EmpInfo EI2 = Instantiate(CEOInfoPrefab, TotalEmpContent);
        ED.CopyStatus(EI2);

        emp.InfoDetail = ED;
        emp.InfoA = EI1;
        emp.InfoB = EI2;
        EI1.DetailInfo = ED;
        EI2.DetailInfo = ED;
        EI1.transform.parent = GC.StandbyContent;
        emp.InitRelation();
        //创建特质和技能
        ED.InitStrategy();

        //创建员工实体
        ED.Entity = EmpManager.Instance.CreateEmp(BuildingManage.Instance.ExitPos.position);
        ED.Entity.SetInfo(ED);

        GC.CurrentEmployees.Add(emp);

        emp.CurrentDep = GC.CurrentDeps[0];
        GC.CurrentDeps[0].CurrentEmps.Add(emp);
        GC.CurrentDeps[0].Manager = emp;
        GC.CurrentDeps[0].SetOfficeStatus();
        GC.HourEvent.AddListener(emp.TimePass);

        //将CEO添加到核心团队
        GC.BSC.CEOInfo.EmpJoin(emp);
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
