using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmotionInfo : MonoBehaviour
{
    bool ShowPanel = false;
    float Timer = 0.0f;

    public Text Text_Name;
    public Emotion E;
    public Image a;

    InfoPanel info;

    private void Start()
    {
        info = GameControl.Instance.infoPanel;
        //Color tempColor = Color.white;
        //ColorUtility.TryParseHtmlString("#FF0000", out tempColor);
        //Text_Name.text = "愤怒/惊诧";
        //Text_Name.color = Color.white;
        //a.color = tempColor;
    }
    private void Update()
    {
        if (ShowPanel == true)
        {
            if (Timer < 0.25f)
                Timer += Time.deltaTime;
            else
            {
                if (E != null)
                {
                    info.Text_Name.text = Text_Name.text;
                    if (E.Level > 1)
                        info.Text_Name.text += "(" + E.Level + "层)";
                    if (E.color == EColor.Red)
                    {
                        info.Text_Description.text = "事件判定修正-2";
                    }
                    else if (E.color == EColor.LRed)
                    {
                        info.Text_Description.text = "事件判定修正-1";
                    }
                    else if (E.color == EColor.Yellow)
                    {
                        info.Text_Description.text = "事件判定修正+2";
                    }
                    else if (E.color == EColor.LYellow)
                    {
                        info.Text_Description.text = "事件判定修正+1";
                    }
                    else if (E.color == EColor.Blue)
                    {
                        info.Text_Description.text = "事件判定无修正";
                    }
                    else if (E.color == EColor.LBlue)
                    {
                        info.Text_Description.text = "事件判定无修正";
                    }
                    else if (E.color == EColor.Green)
                    {
                        info.Text_Description.text = "事件判定修正+2";
                    }
                    else if (E.color == EColor.LGreen)
                    {
                        info.Text_Description.text = "事件判定修正+1";
                    }
                    else if (E.color == EColor.Orange)
                    {
                        info.Text_Description.text = "事件判定无修正";
                    }
                    else if (E.color == EColor.LOrange)
                    {
                        info.Text_Description.text = "事件判定无修正";
                    }
                    else if (E.color == EColor.Purple)
                    {
                        info.Text_Description.text = "事件判定修正-2";
                    }
                    else if (E.color == EColor.LPurple)
                    {
                        info.Text_Description.text = "事件判定修正-1";
                    }
                    info.Text_ExtraInfo.text = "";
                }
                if (info.Visible == false)
                    info.ShowPanel();
                info.transform.position = Input.mousePosition;
            }
        }
    }

    public void SetColor(Emotion emotion)
    {
        info = GameControl.Instance.infoPanel;
        Color tempColor = Color.white;
        if (emotion.color == EColor.Red)
        {
            ColorUtility.TryParseHtmlString("#FF0000", out tempColor);
            Text_Name.text = "愤怒/惊诧";
            Text_Name.color = Color.white;
        }
        else if (emotion.color == EColor.LRed)
        {
            ColorUtility.TryParseHtmlString("#FF6666", out tempColor);
            Text_Name.text = "反感/侮辱";
        }
        else if (emotion.color == EColor.Yellow)
        {
            ColorUtility.TryParseHtmlString("#FFFF00", out tempColor);
            Text_Name.text = "好奇/兴致";
        }
        else if (emotion.color == EColor.LYellow)
        {
            ColorUtility.TryParseHtmlString("#FFFFCC", out tempColor);
            Text_Name.text = "愉悦/安乐";
        }
        else if (emotion.color == EColor.Blue)
        {
            ColorUtility.TryParseHtmlString("#0000FF", out tempColor);
            Text_Name.text = "悲伤/恐惧";
            Text_Name.color = Color.white;
        }
        else if (emotion.color == EColor.LBlue)
        {
            ColorUtility.TryParseHtmlString("#99CCFF", out tempColor);
            Text_Name.text = "辛酸/苦涩";
        }
        else if (emotion.color == EColor.Green)
        {
            ColorUtility.TryParseHtmlString("#00CC00", out tempColor);
            Text_Name.text = "沉思";
        }
        else if (emotion.color == EColor.LGreen)
        {
            ColorUtility.TryParseHtmlString("#99FF99", out tempColor);
            Text_Name.text = "敬畏";
        }
        else if (emotion.color == EColor.Orange)
        {
            ColorUtility.TryParseHtmlString("#FF8000", out tempColor);
            Text_Name.text = "狂想";
        }
        else if (emotion.color == EColor.LOrange)
        {
            ColorUtility.TryParseHtmlString("#FFCC99", out tempColor);
            Text_Name.text = "骄傲";
        }
        else if (emotion.color == EColor.Purple)
        {
            ColorUtility.TryParseHtmlString("#6600CC", out tempColor);
            Text_Name.text = "沮丧";
            Text_Name.color = Color.white;
        }
        else if (emotion.color == EColor.LPurple)
        {
            ColorUtility.TryParseHtmlString("#CC99FF", out tempColor);
            Text_Name.text = "委屈";
        }
        a.color = tempColor;
        E = emotion;
    }

    public void PointerEnter()
    {
        ShowPanel = true;
    }

    public void PointerExit()
    {
        ShowPanel = false;
        info.ClosePanel();
        Timer = 0;
    }
}
