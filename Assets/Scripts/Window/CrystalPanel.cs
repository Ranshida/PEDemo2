using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 水晶放置面板
/// </summary>
public class CrystalPanel : WindowRoot
{
    private int BlackCount = 0;//黑色水晶计数 用于检测垃圾桶有没有放够

    public MonthMeeting Manager;
    public List<Areas> UnlockedAreaList;
    public Transform BG;
    public Transform Areas;
    public Transform PutButton;
    public Transform TrashContent;
    public Button TrashButtonPrefab;

    public Toggle toggle_ShowPanel;
    private Button btn_Finish;
    private Text txt_WhiteChys;
    private Text txt_OrangeChys;
    private Text txt_GrayChys;
    private Text txt_BlueChys;
    private Text txt_BlackChys;

    private Dictionary<CrystalType, Transform> putCrystalDict;
    private Dictionary<Transform, CrystalType> TrashCrystalDict = new Dictionary<Transform, CrystalType>();

    private GameObject AreaPrefab;     //区域的水晶放置物体
    private List<CrystalArea> CrystalAreaList;
    private List<GameObject> TrashButtons = new List<GameObject>();

    protected override void OnActive()
    {
        base.OnActive();

        //月会结束后的显示
        if (Manager.MeetingStart == true)
        {
            TrashContent.parent.parent.parent.gameObject.SetActive(true);
            btn_Finish.gameObject.SetActive(true);
            BG.gameObject.SetActive(true);
            foreach(CrystalArea area in CrystalAreaList)
            {
                area.ResetAllCrystal();
            }
            BlackCount = Manager.CrystalDict[CrystalType.Black];
            SetTrashButtons();
            AskPause();
        }
        //普通的视图
        else
        {
            TrashContent.parent.parent.parent.gameObject.SetActive(false);
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

        if (BlackCount > 0) 
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

    private void SetTrashButtons()
    {
        int count = 0;
        foreach (var c in Manager.CrystalDict)
        {
            if (c.Key != CrystalType.Black && c.Key != CrystalType.None)
            {
                count += c.Value;
            }
        }
        if (count <= BlackCount)
        {
            EndMeeting();
            QuestControl.Instance.Init("黑色水晶过多，月会直接结束");
        }
        if (Manager.CrystalDict[CrystalType.Black] > TrashButtons.Count)
        {
            for (int i = 0; i < Manager.CrystalDict[CrystalType.Black]; i++)
            {
                if (TrashButtons.Count < i + 1)
                {
                    Button b = Instantiate(TrashButtonPrefab, TrashContent);
                    TrashCrystalDict.Add(b.transform, CrystalType.None);
                    b.onClick.AddListener(delegate () { SelectTrashSeat(b.transform); });
                    TrashButtons.Add(b.gameObject);
                }
                TrashButtons[i].SetActive(true);
                TrashButtons[i].transform.Find("Crystal").gameObject.SetActive(false);
                TrashCrystalDict[TrashButtons[i].transform] = CrystalType.None;
            }
        }
        else
        {
            for (int i = 0; i < TrashButtons.Count; i++)
            {
                if (i < Manager.CrystalDict[CrystalType.Black])
                {
                    TrashButtons[i].SetActive(true);
                    TrashButtons[i].transform.Find("Crystal").gameObject.SetActive(false);
                    TrashCrystalDict[TrashButtons[i].transform] = CrystalType.None;
                }
                else
                    TrashButtons[i].SetActive(false);
            }
        }
    }

    //垃圾桶选择
    public void SelectTrashSeat(Transform seat)
    {
        PutButton.gameObject.SetActive(true);
        tempSeat = seat;
        tempArea = null;
        //只显示黑色水晶和X
        putCrystalDict[CrystalType.None].gameObject.SetActive(true);
        foreach (KeyValuePair<CrystalType, int> item in Manager.CrystalDict)
        {
            putCrystalDict[item.Key].gameObject.SetActive(false);
            //不显示黑色水晶
            if (item.Value > 0 && item.Key != CrystalType.Black)
            {
                putCrystalDict[item.Key].gameObject.SetActive(true);
            }
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
            //不显示黑色水晶
            if (item.Value > 0 && item.Key != CrystalType.Black) 
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

        //普通放置
        if (tempArea != null && tempArea.TryPutCrystal(tempSeat, type))
        {
            tempSeat = null;
            tempArea = null;
            PutButton.gameObject.SetActive(false);
        }
        //垃圾桶放置
        else if (tempSeat != null)
        {
            if (TrashCrystalDict[tempSeat] != CrystalType.None)
            {
                Manager.RecycleCrystal(type);
                TrashCrystalDict[tempSeat] = CrystalType.None;
                BlackCount += 1;//以前有的话只是替换，要先把计数加回去
            }

            Image img = tempSeat.Find("Crystal").GetComponent<Image>();
            img.gameObject.SetActive(true);
            img.color = MonthMeeting.GetCrystalColor(type);
            Manager.PutCrystal(type);
            TrashCrystalDict[tempSeat] = type;
            BlackCount -= 1;
            tempSeat = null;
            PutButton.gameObject.SetActive(false);
        }
    }

    public void RemoveCrystal()
    {
        //普通移除
        if (tempArea != null)
        {
            tempArea.RemoveCrystal(tempSeat);
            tempSeat = null;
            tempArea = null;
            PutButton.gameObject.SetActive(false);
        }
        //垃圾桶移除
        else
        {
            if (TrashCrystalDict[tempSeat] == CrystalType.None)
                return;
            Image img = tempSeat.Find("Crystal").GetComponent<Image>();
            img.gameObject.SetActive(false);
            Manager.RecycleCrystal(TrashCrystalDict[tempSeat]);
            TrashCrystalDict[tempSeat] = CrystalType.None;
            BlackCount += 1;
            tempSeat = null;
            PutButton.gameObject.SetActive(false);
        }
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
