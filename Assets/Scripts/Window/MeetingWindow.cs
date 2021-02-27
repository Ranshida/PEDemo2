using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeetingWindow : WindowRoot
{
    private MonthMeeting Manager;

    private GameObject dialoguePrefab_Right;
    private GameObject dialoguePrefab_Left;
    int dialogueIndex = 1;      //单数显示右侧的，复数显示左侧的

    public Transform StartPanel;      //月会开始前的界面
    public Transform ResultPanel;     //结果界面
    public Transform DialogueContent;         //对话框容器
    public Transform ItemContent;         //对话框容器
    public Transform CrystalResult;         //水晶框容器
    public Text txt_Result;

    private GameObject crystalPrefab;
    private GameObject ItemButtonPrefab;
    private List<GameObject> dialogueList;
    private List<GameObject> crystalList;

    protected override void OnActive()
    {
        Manager = MonthMeeting.Instance;
        dialoguePrefab_Right = ResourcesLoader.LoadPrefab("Prefabs/UI/Meeting/MeetingDialogue1");
        dialoguePrefab_Left = ResourcesLoader.LoadPrefab("Prefabs/UI/Meeting/MeetingDialogue2");
        ItemButtonPrefab = ResourcesLoader.LoadPrefab("Prefabs/UI/Meeting/Btn_Item");
        crystalPrefab = ResourcesLoader.LoadPrefab("Prefabs/UI/Meeting/Img_Crystal");
        dialogueIndex = 1;
        StartPanel.gameObject.SetActive(true);
        ResultPanel.gameObject.SetActive(false);
        dialogueList = new List<GameObject>();
        crystalList = new List<GameObject>();
        AskPause();
    }

    protected override void OnButton(string btnName)
    {
        switch (btnName)
        {
            case "Btn_Start":
                StartMeeting();
                break;
            case "Btn_Finish":
                EndMeeting();
                break;
            case "Btn_Hide":
                HideResult();
                break;
            case "Btn_Show":
                ShowResult();
                break;
            default:
                break;
        }
    }

    private void HideResult()
    {
        ResultPanel.gameObject.SetActive(false);
        transform.Find("Btn_Show").gameObject.SetActive(true);
    }

    private void ShowResult()
    {
        ResultPanel.gameObject.SetActive(true);
        transform.Find("Btn_Show").gameObject.SetActive(false);
    }

    public void StartMeeting()
    {
        StartPanel.gameObject.SetActive(false);
        ResultPanel.gameObject.SetActive(true);
        Manager.OnStartMeeting();
    }

    /// <summary>
    /// 添加高管对话
    /// </summary>
    /// <param name="dialogue">对话内容</param>
    /// <param name="detail">细节描述</param>
    /// <param name="result">结果  1：成功  2：大成功  3：失败  4：大失败</param>
    /// <param name="type">水晶类型（颜色）</param>
    public void AddDiaLogue(string dialogue,string detail,int result,CrystalType type)
    {
        GameObject newGo;
        if (Function.IsOdd(dialogueIndex))
            newGo = Instantiate(dialoguePrefab_Right, DialogueContent);
        else
            newGo = Instantiate(dialoguePrefab_Left, DialogueContent);
        GameObject crystal1 = newGo.transform.Find("Crystal_1").gameObject;
        GameObject crystal2 = newGo.transform.Find("Crystal_2").gameObject;
        
        newGo.transform.Find("Txt_Detail").GetComponent<Text>().text = detail;
        newGo.transform.Find("Img_Dialogue").Find("Txt_Dialogue").GetComponent<Text>().text = dialogue;
        if (result == 1) 
        {  //成功
            newGo.GetComponent<Image>().color = new Color(0.7f, 0.85f, 0.7f);             //成功or失败决定背景颜色
            newGo.transform.Find("Txt_Result").GetComponent<Text>().text = "成功";
            crystal2.SetActive(false);
            crystal1.GetComponent<Image>().color = MonthMeeting.GetCrystalColor(type);    //水晶颜色
        }
        if (result == 2) 
        {  //大成功
            newGo.GetComponent<Image>().color = new Color(0.7f, 0.85f, 0.7f);             //成功or失败决定背景颜色
            newGo.transform.Find("Txt_Result").GetComponent<Text>().text = "大成功";
            crystal1.GetComponent<Image>().color = MonthMeeting.GetCrystalColor(type);    //水晶颜色
            crystal2.GetComponent<Image>().color = MonthMeeting.GetCrystalColor(type);    //水晶颜色
        }
        if (result == 3) 
        {  //失败
            newGo.GetComponent<Image>().color = new Color(0.95f, 0.7f, 0.65f);             //成功or失败决定背景颜色
            newGo.transform.Find("Txt_Result").GetComponent<Text>().text = "失败";
            crystal2.SetActive(false);
            crystal1.SetActive(false);
        }
        if (result == 4) 
        {  //大失败
            newGo.GetComponent<Image>().color = new Color(0.95f, 0.7f, 0.65f);             //成功or失败决定背景颜色

            newGo.transform.Find("Txt_Result").GetComponent<Text>().text = "大失败";
            crystal2.SetActive(false);
            crystal1.GetComponent<Image>().color = MonthMeeting.GetCrystalColor(type);    //水晶颜色
        }
        dialogueList.Add(newGo);
        dialogueIndex++;
    }

    //显示统计结果
    public void ShowResult(Dictionary<CrystalType, int> CrystalDict)
    {
        foreach (KeyValuePair<CrystalType, int> item in CrystalDict)
        {
            if (item.Key != CrystalType.None && item.Value > 0) 
            {
                for (int i = 0; i < item.Value; i++)
                {
                    GameObject go = Instantiate(crystalPrefab, CrystalResult);
                    crystalList.Add(go);
                    go.GetComponent<Image>().color = MonthMeeting.GetCrystalColor(item.Key);
                }
            }
        }

        //txt_Result.text = "结果统计： ";
        //foreach (KeyValuePair<CrystalType,int> item in CrystalDict)
        //{
        //    txt_Result.text += MonthMeeting.GetCrystalChineseName(item.Key) + " × " + item.Value + "   ";
        //}

        //物品按钮
        List<Button> itemButtons = new List<Button>();
        foreach (CompanyItem item in GameControl.Instance.Items)
        {
            if (item.Type == CompanyItemType.MonthMeeting)
            {
                itemButtons.Add(Instantiate(ItemButtonPrefab, ItemContent).GetComponent<Button>());
            }
        }

        //Dictionary<CrystalType, List<Crystal>> resultDict = new Dictionary<CrystalType, List<Crystal>>();
        //foreach (Crystal crystal in allCrystal)
        //{
        //    if (!resultDict.ContainsKey(crystal.Type))
        //    {
        //        resultDict.Add(crystal.Type, new List<Crystal>());
        //    }
        //    resultDict[crystal.Type].Add(crystal);
        //}

        //foreach (KeyValuePair<CrystalType, List<Crystal>> item in resultDict)
        //{
        //    txt_Result.text += Crystal.GetChineseName(item.Key) + " × " + item.Value.Count + "\n";
        //}
    }

    public void EndMeeting()
    {
        Manager.EndMeeting();
        foreach (var item in crystalList)
        {
            Destroy(item);
        }
        crystalList.Clear();
        SetWndState(false);
    }

    protected override void OnClear()
    {
        foreach (GameObject gameObject in dialogueList)
        {
            Destroy(gameObject);
        }
        base.OnClear();
    }
}
