using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 月会
/// </summary>
public class MonthMeeting : MonoBehaviour
{
    public bool MeetingStart = false;
    public float CrystalExtraSuccessRate = 0;

    public static MonthMeeting Instance;

    public DynamicWindow dynamicWindow;
    public MeetingWindow MeetingWindow;
    public CrystalPanel CrystalPanel;

    public Dictionary<CrystalType, int> CrystalDict;

    private void Awake()
    {
        Instance = this;
        CrystalDict = new Dictionary<CrystalType, int>();
        CrystalDict.Add(CrystalType.White, 0);
        CrystalDict.Add(CrystalType.Orange, 0);
        CrystalDict.Add(CrystalType.Gray, 0);
        CrystalDict.Add(CrystalType.Black, 0);
        CrystalDict.Add(CrystalType.Blue, 0);
    }

    private void Start()
    {
        GameControl.Instance.MonthlyEvent.AddListener(StartMeeting);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartMeeting();
        }
    }

    public void StartMeeting()
    {
        //开始月会
        //已经开始了就不执行
        if (MeetingStart == true)
            return;
        MeetingWindow.SetWndState();
        MeetingStart = true;
        foreach(CompanyItem item in GameControl.Instance.Items)
        {
            if (item.item.Type == CompanyItemType.MonthMeeting && item.item.ActiveType != 2)
                item.button.interactable = true;
        }
    }

    public void OnStartMeeting()
    {
        //统计所有管理成员
        List<Employee> managers = new List<Employee>();
        foreach (EmpBSInfo info in GameControl.Instance.BSC.EmpSelectInfos)
        {
            if (info.emp != null)
            {
                managers.Add(info.emp);
            }
        }

        //统计每个Boss的发言和水晶统计
        Dictionary<Employee, List<CrystalType>> managerCrystalDict = new Dictionary<Employee, List<CrystalType>>();
        foreach (Employee employee in managers)
        {
            bool success;
            int result = 0;     // 1成功  2大成功  3失败  4大失败
            CrystalType crystal = CrystalType.None;
            float successPercent = 0.5f + CrystalExtraSuccessRate;
            successPercent += employee.Decision * 0.1f;
            successPercent = Mathf.Clamp(successPercent, 0, 1);
            float rand = Random.Range(0f, 1f);
            success = successPercent >= rand;

            if (success)
            {
                bool major = Random.Range(0f, 1f) < 0.2f;
                result = 1;
                crystal = GetCrystal_Success(employee);
                managerCrystalDict.Add(employee, new List<CrystalType>() { crystal});
                CrystalDict[crystal] += 1;
                if (major)
                {
                    result = 2;
                    managerCrystalDict[employee].Add(crystal);
                    CrystalDict[crystal] += 1;
                }
            }
            else
            {
                bool major = Random.Range(0f, 1f) < 0.2f;
                result = 3;
                managerCrystalDict.Add(employee, new List<CrystalType>());
                if (major)
                {
                    result = 4;
                    crystal = CrystalType.Black;
                    managerCrystalDict[employee].Add(crystal);
                    CrystalDict[crystal] += 1;
                }
            }

            string dialogue = "";
            switch (crystal)
            {
                case CrystalType.None:
                    dialogue = "没什么想法。";
                    break;
                case CrystalType.White:
                    dialogue = "别紧绷着各位，打开思路吧~";
                    break;
                case CrystalType.Orange:
                    dialogue = "勇气是人类的赞歌！";
                    break;
                case CrystalType.Gray:
                    dialogue = "加班！加班！加班！";
                    break;
                case CrystalType.Blue:
                    dialogue = "标准化才是解决之道。";
                    break;
                case CrystalType.Black:
                    dialogue = "不要你觉得，我要我觉得。";
                    break;
                default:
                    break;
            }

            string detail = employee.Name;
            detail += "\n决策" + employee.Decision + "级";
            detail += "\n成功率50%+" + employee.Decision+"0%";
            detail += "\n大成功率" + (successPercent * 100 * 0.2f).ToString() + "%";
            detail += "\n失败率" + ((1 - successPercent) * 100).ToString() + "%";

            MeetingWindow.AddDiaLogue(dialogue, detail, result,crystal);
        }

        //UI显示
        //foreach (KeyValuePair<Employee, List<CrystalType>> item in managerCrystalDict)
        //{
        //    foreach (CrystalType crystal in item.Value)
        //    {
        //        string name = item.Key.Name;
        //        string dialogue = "";
        //        switch (crystal)
        //        {
        //            case CrystalType.None:
        //                Debug.LogError("错误水晶类型");
        //                break;
        //            case CrystalType.White:
        //                dialogue = "：“别紧绷着各位，打开思路吧~”（获得白色水晶×1）";
        //                break;
        //            case CrystalType.Orange:
        //                dialogue = "：“勇气是人类的赞歌！”（获得橙色水晶×1）";
        //                break;
        //            case CrystalType.Gray:
        //                dialogue = "：“加班！加班！加班！”（获得灰色水晶×1）";
        //                break;
        //            case CrystalType.Blue:
        //                dialogue = "：“标准化才是解决之道。”（获得蓝色水晶×1）";
        //                break;
        //            case CrystalType.Black:
        //                dialogue = "：“不要你觉得，我要我觉得。”（获得黑色水晶×1）";
        //                break;
        //            default:
        //                break;
        //        }
        //        MeetingWindow.AddDiaLogue(name + dialogue);
        //    }
        //}
        MeetingWindow.ShowResult(CrystalDict);
    }
    CrystalType GetCrystal_Success(Employee emp)
    {
        List<CrystalType> crystal = new List<CrystalType>();

        if (emp.CharacterTendency[0] < 0) 
        {
            crystal.Add(CrystalType.Gray);
        }
        else if (emp.CharacterTendency[0] > 0)
        {
            crystal.Add(CrystalType.White);
        }
        else
        {
            crystal.Add(CrystalType.Gray);
            crystal.Add(CrystalType.White);
        }

        if (emp.CharacterTendency[1] < 0) 
        {
            crystal.Add(CrystalType.Blue);
        }
        else if (emp.CharacterTendency[1] > 0) 
        {
            crystal.Add(CrystalType.Orange);
        }
        else
        {
            crystal.Add(CrystalType.Blue);
            crystal.Add(CrystalType.Orange);
        }
        return crystal[Random.Range(0, crystal.Count)];
    } 

    /// <summary>
    /// 结束会议，检查当前水晶，开始放置
    /// </summary>
    public void EndMeeting()
    {
        CrystalPanel.SetWndState();
        foreach (CompanyItem item in GameControl.Instance.Items)
        {
            if (item.item.Type == CompanyItemType.MonthMeeting)
            {
                if (item.item.ActiveType == 2)
                    item.button.interactable = true;
                else
                    item.button.interactable = false;
            }
        }
    }

    //将一个水晶放到位置上
    public void PutCrystal(CrystalType type)
    {
        CrystalDict[type] -= 1;
    }
    //将水晶放回仓库
    public void RecycleCrystal(CrystalType type)
    {
        CrystalDict[type] += 1;
    }

    //结算水晶
    public void SettleArea(List<CrystalType> crystals, Area area)
    {
        CrystalPanel.SetWndState(false);
        //各色水晶数量
        int NumA = 0, NumB = 0, NumC = 0, NumD = 0, NumE = 0;
        foreach (CrystalType crystal in crystals)
        {
            switch (crystal)
            {
                case CrystalType.None:
                    break;
                case CrystalType.White:
                    NumA += 1;
                    break;
                case CrystalType.Orange:
                    NumB += 1;
                    break;
                case CrystalType.Gray:
                    NumC += 1;
                    break;
                case CrystalType.Blue:
                    NumD += 1;
                    break;
                case CrystalType.Black:
                    NumE += 1;
                    break;
                default:
                    break;
            }
        }
        if (NumA > 0)
        {
            Debug.Log(area.DC.DivName + "获得白色水晶");
            Perk newPerk = new Perk120();
            newPerk.TempValue1 = NumA;
            area.DC.AddPerk(newPerk);
        }
        if (NumB > 0)
        {
            Debug.Log(area.DC.DivName + "获得橙色水晶");
            Perk newPerk = new Perk121();
            newPerk.TempValue1 = NumB;
            area.DC.AddPerk(newPerk);
        }
        if (NumC > 0)
        {
            Debug.Log(area.DC.DivName + "获得灰色水晶");
            Perk newPerk = new Perk122();
            newPerk.TempValue1 = NumC;
            area.DC.AddPerk(newPerk);
        }
        if (NumD > 0)
        {
            Debug.Log(area.DC.DivName + "获得蓝色水晶");
            Perk newPerk = new Perk123();
            newPerk.TempValue1 = NumD;
            area.DC.AddPerk(newPerk);
        }
        if (NumE > 0)
        {
            Debug.Log(area.DC.DivName + "获得黑色水晶");
            Perk newPerk = new Perk124();
            newPerk.TempValue1 = NumE;
            area.DC.AddPerk(newPerk);
        }


        if (crystals.Count == 1)
        {
            Debug.Log(area.DC.DivName + "单指令");
            area.DC.AddPerk(new Perk125());
        }
    }

    //结束会议
    public void EndPutting()
    {
        CrystalPanel.SetWndState(false);
        MeetingStart = false;
        GameControl.Instance.MonthMeetingTime = 3;
        GameControl.Instance.UpdateUI();
        GameControl.Instance.CheckButtonName();
        CrystalDict = new Dictionary<CrystalType, int>();
        CrystalDict.Add(CrystalType.White, 0);
        CrystalDict.Add(CrystalType.Orange, 0);
        CrystalDict.Add(CrystalType.Gray, 0);
        CrystalDict.Add(CrystalType.Black, 0);
        CrystalDict.Add(CrystalType.Blue, 0);
    }

    public static string GetCrystalChineseName(CrystalType type)
    {
        switch (type)
        {
            case CrystalType.None:
                return "空";
            case CrystalType.White:
                return "白色水晶";
            case CrystalType.Orange:
                return "橙色水晶";
            case CrystalType.Gray:
                return "灰色水晶";
            case CrystalType.Blue:
                return "蓝色水晶";
            case CrystalType.Black:
                return "黑色水晶";
            default:
                return "错误";
        }
    }

    public static Color GetCrystalColor(CrystalType type)
    {
        switch (type)
        {
            case CrystalType.None:
                return new Color(1, 1, 1);
            case CrystalType.White:
                return new Color(1, 1, 1);
            case CrystalType.Orange:
                return new Color(1, 0.3f, 0);
            case CrystalType.Gray:
                return new Color(0.55f, 0.55f, 0.55f);
            case CrystalType.Blue:
                return new Color(0, 0.6f, 1);
            case CrystalType.Black:
                return new Color(0, 0, 0);
            default:
                return new Color(1, 1, 1);
        }
    }
}

public enum CrystalType
{
    None,
    White,
    Orange,
    Gray,
    Blue,
    Black,
}