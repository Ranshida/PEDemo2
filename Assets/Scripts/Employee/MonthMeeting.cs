using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 月会
/// </summary>
public class MonthMeeting : MonoBehaviour
{
    public static MonthMeeting Instance;

    public MeetingWindow MeetingWindow;
    public CrystalPanel CrystalPanel;

    public Dictionary<CrystalType, int> CrystalDict { get; private set; }

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

    public void StartMeeting()
    {
        //开始月会
        MeetingWindow.SetWndState();
    }

    public void OnStartMeeting()
    {
        //统计所有管理成员
        List<Employee> managers = new List<Employee>();
        foreach (OfficeControl office in GameControl.Instance.CurrentOffices)
        {
            if (office.CurrentManager != null)
            {
                managers.Add(office.CurrentManager);
            }
        }

        //统计每个Boss的发言和水晶统计
        Dictionary<Employee, List<CrystalType>> managerCrystalDict = new Dictionary<Employee, List<CrystalType>>();
        foreach (Employee employee in managers)
        {
            bool success;
            float successPercent = 0.5f;
            successPercent += employee.Decision * 0.1f;
            float rand = Random.Range(0f, 1f);
            success = successPercent >= rand;

            if (success)
            {
                bool major = Random.Range(0f, 1f) < 0.2f;

                CrystalType crystal = GetCrystal_Success(employee);
                managerCrystalDict.Add(employee, new List<CrystalType>() { crystal});
                CrystalDict[crystal] += 1;
                if (major)
                {
                    CrystalType crystal2 = GetCrystal_Success(employee);
                    managerCrystalDict[employee].Add(crystal2);
                    CrystalDict[crystal] += 1;
                }
            }
            else
            {
                bool major = Random.Range(0f, 1f) < 0.2f;
                managerCrystalDict.Add(employee, new List<CrystalType>());
                if (major)
                {
                    CrystalType crystal = CrystalType.Black;
                    managerCrystalDict[employee].Add(crystal);
                    CrystalDict[crystal] += 1;
                }
            }
        }

        //UI显示
        foreach (KeyValuePair<Employee, List<CrystalType>> item in managerCrystalDict)
        {
            foreach (CrystalType crystal in item.Value)
            {
                string name = item.Key.Name;
                string dialogue = "";
                switch (crystal)
                {
                    case CrystalType.None:
                        Debug.LogError("错误水晶类型");
                        break;
                    case CrystalType.White:
                        dialogue = "：“别紧绷着各位，打开思路吧~”（获得白色水晶×1）";
                        break;
                    case CrystalType.Orange:
                        dialogue = "：“勇气是人类的赞歌！”（获得橙色水晶×1）";
                        break;
                    case CrystalType.Gray:
                        dialogue = "：“加班！加班！加班！”（获得灰色水晶×1）";
                        break;
                    case CrystalType.Blue:
                        dialogue = "：“标准化才是解决之道。”（获得蓝色水晶×1）";
                        break;
                    case CrystalType.Black:
                        dialogue = "：“不要你觉得，我要我觉得。”（获得黑色水晶×1）";
                        break;
                    default:
                        break;
                }
                MeetingWindow.AddDiaLogue(name + dialogue);
            }
        }
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
    public void SettleArea(List<CrystalType> crystals, Areas.Area area)
    {
        List<Building> buildings = new List<Building>();
        foreach (Grid grid in area.gridList)
        {
            if (grid.BelongBuilding != null && !buildings.Contains(grid.BelongBuilding))
            {
                buildings.Add(grid.BelongBuilding);
            }
        }

        foreach (Building building in buildings)
        {
            DepControl dep = building.Department;
            if (!dep)
            {
                break;
            }

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
                Debug.Log(dep.name + "获得白色水晶");
                Perk newPerk = new Perk120(null);
                newPerk.TempValue4 = 0.1f * NumA;
                dep.AddPerk(newPerk);
            }
            if (NumB > 0)
            {
                Debug.Log(dep.name + "获得橙色水晶");
                Perk newPerk = new Perk121(null);
                newPerk.TempValue1 = 15 * NumB;
                dep.AddPerk(newPerk);
            }
            if (NumC > 0)
            {
                Debug.Log(dep.name + "获得灰色水晶");
                Perk newPerk = new Perk122(null);
                newPerk.TempValue4 = 0.1f * NumC;
                dep.AddPerk(newPerk);
            }
            if (NumD > 0)
            {
                Debug.Log(dep.name + "获得蓝色水晶");
                Perk newPerk = new Perk123(null);
                newPerk.TempValue4 = 0.25f * NumD;
                dep.AddPerk(newPerk);
            }
            if (NumE > 0)
            {
                Debug.Log(dep.name + "获得黑色水晶");
                Perk newPerk = new Perk124(null);
                newPerk.TempValue1 = 30 * NumE;
                dep.AddPerk(newPerk);
            }


            if (crystals.Count == 1)
            {
                Debug.Log(dep.name + "单指令");
                dep.AddPerk(new Perk125(null));
            }
            else if (crystals.Count == 2)
            {
                Debug.Log(dep.name + "二指令");
                dep.AddPerk(new Perk125(null));
            }
            else if (crystals.Count == 3)
            {
                Debug.Log(dep.name + "三指令");
                dep.AddPerk(new Perk125(null));
            }
        }
    }

    //结束会议
    public void EndPutting()
    {
        CrystalPanel.SetWndState(false);
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