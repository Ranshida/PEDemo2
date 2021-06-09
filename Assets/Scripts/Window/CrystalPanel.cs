using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 水晶放置面板
/// </summary>
public class CrystalPanel : WindowRoot
{
    public MonthMeeting Manager;
    public List<Areas> UnlockedAreaList;
    public Transform BG;
    public Transform Areas;
    public Transform PutButton;

    public Toggle toggle_ShowPanel;
    private Button btn_Finish;
    private Text txt_WhiteChys;
    private Text txt_OrangeChys;
    private Text txt_GrayChys;
    private Text txt_BlueChys;
    private Text txt_BlackChys;

    private Dictionary<CrystalType, Transform> putCrystalDict;

    private GameObject AreaPrefab;     //区域的水晶放置物体
    private List<CrystalArea> CrystalAreaList;

    protected override void OnActive()
    {
        base.OnActive();

        if (Manager.MeetingStart == true)
        {
            btn_Finish.gameObject.SetActive(true);
            BG.gameObject.SetActive(true);
            foreach(CrystalArea area in CrystalAreaList)
            {
                area.ResetAllCrystal();
            }
            AskPause();
        }
        else
        {
            btn_Finish.gameObject.SetActive(false);
            BG.gameObject.SetActive(false);
        }
        Areas.gameObject.SetActive(true);
    }

    public void InitCrystalPanel()
    {
        AreaPrefab = ResourcesLoader.LoadPrefab("Prefabs/UI/Meeting/CrystalArea");

        CrystalAreaList = new List<CrystalArea>();
        foreach (Area area in GridContainer.Instance.Areas.AreaLists)
        {
            CrystalArea crystalArea = Instantiate(AreaPrefab, Areas).GetComponent<CrystalArea>();
            area.CA = crystalArea;
            crystalArea.Init(this, area);
            CrystalAreaList.Add(crystalArea);
            crystalArea.gameObject.SetActive(false);
            //每个区域中心在UI上显示三个槽位，供放置水晶
            //每个绑定一个按钮，记录水晶类型
            //结束时结算Buff
            //area.centerPosition
        }
        Manager = MonthMeeting.Instance;
        txt_WhiteChys = BG.Find("Img_White").GetComponentInChildren<Text>();
        txt_OrangeChys = BG.Find("Img_Orange").GetComponentInChildren<Text>();
        txt_GrayChys = BG.Find("Img_Gray").GetComponentInChildren<Text>();
        txt_BlueChys = BG.Find("Img_Blue").GetComponentInChildren<Text>();
        txt_BlackChys = BG.Find("Img_Black").GetComponentInChildren<Text>();
        btn_Finish = transform.Find("Btn_Finish").GetComponent<Button>();
        putCrystalDict = new Dictionary<CrystalType, Transform>();
        putCrystalDict.Add(CrystalType.Black, PutButton.Find("Btn_Black"));
        putCrystalDict.Add(CrystalType.Blue, PutButton.Find("Btn_Blue"));
        putCrystalDict.Add(CrystalType.Gray, PutButton.Find("Btn_Gray"));
        putCrystalDict.Add(CrystalType.Orange, PutButton.Find("Btn_Orange"));
        putCrystalDict.Add(CrystalType.White, PutButton.Find("Btn_White"));
        putCrystalDict.Add(CrystalType.None, PutButton.Find("Btn_None"));
    }

    protected override void OnClear()
    {
        PutButton.gameObject.SetActive(false);
    }

    protected override void UpdateSpecific()
    {
        txt_WhiteChys.text = "× " + Manager.CrystalDict[CrystalType.White];
        txt_OrangeChys.text = "× " + Manager.CrystalDict[CrystalType.Orange];
        txt_GrayChys.text = "× " + Manager.CrystalDict[CrystalType.Gray];
        txt_BlueChys.text = "× " + Manager.CrystalDict[CrystalType.Blue];
        txt_BlackChys.text = "× " + Manager.CrystalDict[CrystalType.Black];

        if (Manager.CrystalDict[CrystalType.Black] > 0) 
        {
            btn_Finish.interactable = false;
        }
        else
        {
            btn_Finish.interactable = true;
        }
    }

    

    protected override void OnButton(string btnName)
    {
        switch (btnName)
        {
            case "Btn_White":
                PutCrystal(CrystalType.White);
                break;  
            case "Btn_Orange":
                PutCrystal(CrystalType.Orange);
                break;  
            case "Btn_Gray":
                PutCrystal(CrystalType.Gray);
                break; 
            case "Btn_Blue":
                PutCrystal(CrystalType.Blue);
                break; 
            case "Btn_Black":
                PutCrystal(CrystalType.Black);
                break; 
            case "Btn_None":
                RemoveCrystal();
                break;
            case "Btn_Finish":
                EndMeeting();
                break;
            default:
                break;
        }
    }

    private void LateUpdate()
    {
        if (PutButton.gameObject.activeSelf)
        {
            PutButton.transform.position = tempSeat.position - Vector3.up * 100;
        }
    }

    //当前想要放置水晶的位置
    public CrystalArea tempArea;
    public Transform tempSeat;
    public void SelectSeat(CrystalArea area, Transform seat)
    {
        PutButton.gameObject.SetActive(true);
        tempSeat = seat;
        tempArea = area;

        putCrystalDict[CrystalType.None].gameObject.SetActive(true);
        foreach (KeyValuePair<CrystalType, int> item in Manager.CrystalDict) 
        {
            putCrystalDict[item.Key].gameObject.SetActive(false);
            if (item.Value > 0) 
            {
                putCrystalDict[item.Key].gameObject.SetActive(true);
            }
        }
        //显示放水晶的面板
        //当前有多少水晶就显示几个水晶按钮，此外还有一个×，表示移除

    }

    public void PutCrystal(CrystalType type)
    {
        if (!tempSeat)
        {
            Debug.LogError("没有选择位置");
        }

        if (tempArea.TryPutCrystal(tempSeat, type))
        {
            tempSeat = null;
            tempArea = null;
            PutButton.gameObject.SetActive(false);
        }
    }

    public void RemoveCrystal()
    {
        tempArea.RemoveCrystal(tempSeat);
        tempSeat = null;
        tempArea = null;
        PutButton.gameObject.SetActive(false);
    }

    public void EndMeeting()
    {
        tempSeat = null;
        tempArea = null;

        PutButton.gameObject.SetActive(false);

        foreach (CrystalArea item in CrystalAreaList)
        {
            item.Settle();
        }
        //月会结束时如果之前没有手动打开水晶面板时再关闭
        if (toggle_ShowPanel.isOn == false)
            Areas.gameObject.SetActive(false);
        foreach (CompanyItem item in GameControl.Instance.Items)
        {
            if (item.item.Type == CompanyItemType.MonthMeeting)
                item.button.interactable = false;
        }
        Manager.EndPutting();
    }

    //显示水晶面板
    public void ToggleBuildingInfos(bool show)
    {
        if (show == false)
        {
            SetWndState(false);
            Areas.gameObject.SetActive(false);
            foreach (DivisionControl div in GameControl.Instance.CurrentDivisions)
            {
                div.Text_Status.gameObject.SetActive(false);
                div.StatusShowed = false;
            }
        }
        else
        {
            SetWndState();
            foreach (DivisionControl div in GameControl.Instance.CurrentDivisions)
            {
                div.Text_Status.gameObject.SetActive(true);
                div.StatusShowed = true;
            }
        }
    }
}
