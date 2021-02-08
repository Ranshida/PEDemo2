using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeetingWindow : WindowRoot
{
    private MonthMeeting Manager;

    private GameObject dialoguePrefab;

    public Transform Content;
    public Transform StartPanel;
    public Transform ResultPanel;
    public Text txt_Result;
    private List<GameObject> dialogueList;

    protected override void OnActive()
    {
        Manager = MonthMeeting.Instance;
        dialoguePrefab = ResourcesLoader.LoadPrefab("Prefabs/UI/Building/MeetingDialogue");
        StartPanel.gameObject.SetActive(true);
        ResultPanel.gameObject.SetActive(false);
        dialogueList = new List<GameObject>();
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
            default:
                break;
        }
    }



    public void StartMeeting()
    {
        StartPanel.gameObject.SetActive(false);
        ResultPanel.gameObject.SetActive(true);
        Manager.OnStartMeeting();
    }
    //添加高管对话
    public void AddDiaLogue(string txt)
    {
        GameObject newGo = Instantiate(dialoguePrefab, Content);
        newGo.GetComponentInChildren<Text>().text = txt;
        dialogueList.Add(newGo);
    }

    //显示统计结果
    public void ShowResult(Dictionary<CrystalType, int> CrystalDict)
    {
        foreach (KeyValuePair<CrystalType,int> item in CrystalDict)
        {
            txt_Result.text += MonthMeeting.GetCrystalChineseName(item.Key) + " × " + item.Value + "\n";
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

        //foreach (KeyValuePair<CrystalType,List<Crystal>> item in resultDict)
        //{
        //    txt_Result.text+= Crystal.GetChineseName(item.Key) + " × " +item.Value.Count + "\n";
        //}
    }

    public void EndMeeting()
    {
        Manager.EndMeeting();
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
