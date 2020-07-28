using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Research : MonoBehaviour
{
    static public int BaseValue = 200;
    public int Type, Progress, CurrentProgress, ExtraProgress;
    public float SuccessRate, ExtraRate;

    public DepControl DC;
    public GameControl GC;
    public GameObject SuccessText, FailText;
    public Text Text_Name, Text_Effect, Text_Progress, Text_SuccessRate, Text_Extra;
    public Button SelectButton, ExtraButton;

    public void SetResearch(int type)
    {
        Type = type;
        CurrentProgress = 0;
        SelectButton.gameObject.SetActive(true);
        if(type == 1)
        {
            Text_Name.text = "博客内容推荐";
            Text_Effect.text = "提高功能性500";
            Progress = 10 * BaseValue;
            ExtraProgress = 6 * BaseValue;
            SuccessRate = 0.55f;
            ExtraRate = 0.1f;
        }
        else if(Type == 2)
        {
            Text_Name.text = "搜索排位算法";
            Text_Effect.text = "提高流畅性300 功能性300";
            Progress = 10 * BaseValue;
            ExtraProgress = 6 * BaseValue;
            SuccessRate = 0.55f;
            ExtraRate = 0.1f;
        }
        else if (Type == 3)
        {
            Text_Name.text = "电子商务管理";
            Text_Effect.text = "每千名用户利润+5";
            Progress = 14 * BaseValue;
            ExtraProgress = 4 * BaseValue;
            SuccessRate = 0.35f;
            ExtraRate = 0.1f;
        }
        else if (Type == 4)
        {
            Text_Name.text = "智能手机";
            Text_Effect.text = "提高所有产品分数500";
            Progress = 20 * BaseValue;
            ExtraProgress = 10 * BaseValue;
            SuccessRate = 0.2f;
            ExtraRate = 0.1f;
        }
        else if (Type == 5)
        {
            Text_Name.text = "3G通信";
            Text_Effect.text = "功能性+300 流畅性+500";
            Progress = 20 * BaseValue;
            ExtraProgress = 10 * BaseValue;
            SuccessRate = 0.2f;
            ExtraRate = 0.1f;
        }
        else if (Type == 6)
        {
            Text_Name.text = "ugc社区";
            Text_Effect.text = "功能性+200";
            Progress = 8 * BaseValue;
            ExtraProgress = 4 * BaseValue;
            SuccessRate = 0.55f;
            ExtraRate = 0.1f;
        }
        else if (Type == 7)
        {
            Text_Name.text = "新能源汽车";
            Text_Effect.text = "每千名用户利润+10";
            Progress = 22 * BaseValue;
            ExtraProgress = 6 * BaseValue;
            SuccessRate = 0.20f;
            ExtraRate = 0.1f;
        }
        else if (Type == 8)
        {
            Text_Name.text = "VR技术";
            Text_Effect.text = "美学+400, 功能性+300";
            Progress = 18 * BaseValue;
            ExtraProgress = 6 * BaseValue;
            SuccessRate = 0.20f;
            ExtraRate = 0.1f;
        }
        else if (Type == 9)
        {
            Text_Name.text = "共享经济";
            Text_Effect.text = "每千名用户利润+5";
            Progress = 12 * BaseValue;
            ExtraProgress = 4 * BaseValue;
            SuccessRate = 0.35f;
            ExtraRate = 0.1f;
        }
        else if (Type == 10)
        {
            Text_Name.text = "3D打印";
            Text_Effect.text = "每千名用户利润+10";
            Progress = 18 * BaseValue;
            ExtraProgress = 4 * BaseValue;
            SuccessRate = 0.2f;
            ExtraRate = 0.1f;
        }
        else if (Type == 11)
        {
            Text_Name.text = "自动驾驶";
            Text_Effect.text = "安全性+400";
            Progress = 12 * BaseValue;
            ExtraProgress = 6 * BaseValue;
            SuccessRate = 0.35f;
            ExtraRate = 0.1f;
        }
        else if (Type == 12)
        {
            Text_Name.text = "人工智能";
            Text_Effect.text = "功能性、流畅性、安全性分别增加1000";
            Progress = 25 * BaseValue;
            ExtraProgress = 12 * BaseValue;
            SuccessRate = 0.05f;
            ExtraRate = 0.1f;
        }
        else if (Type == 13)
        {
            Text_Name.text = "量子编程";
            Text_Effect.text = "流畅性+2000";
            Progress = 19 * BaseValue;
            ExtraProgress = 10 * BaseValue;
            SuccessRate = 0.05f;
            ExtraRate = 0.1f;
        }
        else if (Type == 14)
        {
            Text_Name.text = "脑机接口";
            Text_Effect.text = "美学、功能性分别增加1000，安全性-500";
            Progress = 31 * BaseValue;
            ExtraProgress = 16 * BaseValue;
            SuccessRate = 0.05f;
            ExtraRate = 0.1f;
        }
        DC.Text_Task.text = "当前任务:" + Text_Name.text;
        UpdateUI();
    }

    public void UpdateUI()
    {
        Text_Progress.text = "进度: " + CurrentProgress + "/" + Progress;
        Text_SuccessRate.text = "成功率:" + (SuccessRate * 100) + "%";
    }

    public void ResearchFinish()
    {
        float Posb = Random.Range(0.0f, 1.0f);
        if (Posb < SuccessRate + GC.ResearchSuccessRateExtra)
        {
            for (int i = 0; i < GC.PrC.CurrentProduct.Count; i++)
            {
                Product Cp = GC.PrC.CurrentProduct[i];
                if (Type == 1)
                {
                    Cp.Score[1] += 500;
                }
                else if (Type == 2)
                {
                    Cp.Score[1] += 300;
                    Cp.Score[2] += 300;
                }
                else if (Type == 3)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        Cp.Profit[j] += 5;
                    }
                }
                else if (Type == 4)
                {
                    Cp.Score[0] += 500;
                    Cp.Score[1] += 500;
                    Cp.Score[2] += 500;
                    Cp.Score[3] += 500;
                }
                else if (Type == 5)
                {
                    Cp.Score[1] += 300;
                    Cp.Score[2] += 500;
                }
                else if (Type == 6)
                {
                    Cp.Score[1] += 200;
                }
                else if (Type == 7)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        Cp.Profit[j] += 10;
                    }
                }
                else if (Type == 8)
                {
                    Cp.Score[0] += 400;
                    Cp.Score[1] += 300;
                }
                else if (Type == 9)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        Cp.Profit[j] += 5;
                    }
                }
                else if (Type == 10)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        Cp.Profit[j] += 10;
                    }
                }
                else if (Type == 11)
                {
                    Cp.Score[3] += 400;
                }
                else if (Type == 12)
                {
                    Cp.Score[1] += 1000;
                    Cp.Score[2] += 1000;
                    Cp.Score[3] += 1000;
                }
                else if (Type == 13)
                {
                    Cp.Score[2] += 2000;
                }
                else if (Type == 14)
                {
                    Cp.Score[0] += 1000;
                    Cp.Score[1] += 1000;
                    Cp.Score[3] -= 500;
                }

                Cp.CalcUser();
                Cp.UpdateUI();
            }
            SuccessText.SetActive(true);
        }
        else
            FailText.SetActive(true);
        DC.SurveyButton.interactable = true;
    }

    public void SelectResearch()
    {
        DC.CurrentResearch = this;
        ExtraButton.gameObject.SetActive(true);
        for(int i = 0; i < 3; i++)
        {
            if(DC.Researches[i] != this)
            {
                DC.Researches[i].ResetUI(); ;
            }
            DC.Researches[i].SelectButton.gameObject.SetActive(false);
        }
    }

    public void AddExtra()
    {
        Text_Extra.text = "额外投入" + ExtraProgress + "研发点数提高成功率" + (ExtraRate * 100) + "%";
    }

    public void ResetUI()
    {
        Text_Name.text = "无";
        Text_Effect.text = "————";
        Text_Progress.text = "进度: ----";
        Text_SuccessRate.text = "成功率: ----";
        SuccessText.SetActive(false);
        FailText.SetActive(false);
    }
}
