using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CEOControl : MonoBehaviour
{
    public GameControl GC;
    public Button SkillButton;
    //Text_Condition为旧的Text_Fail，用来显示失败效果
    public Text Text_Name, Text_Name2, Text_ConditionContent, Text_SuccessContent, Text_OptionContent, Text_Result, Text_Requirement, Text_Extra;
    public GameObject SelectPanel, ResultPanel, WarningPanel, SuccessText, FailText, ResultCheckButton, CRChangePanel, VacationPanel, TrainingPanel;
    public Employee Target, Target2 ,CEO;

    //改变信仰的相关参数
    public int CultureValue = 1, ReligionValue = 1, SkillValue = 1, CEOSkillNum = 0;

    int SuccessLimit = 10;
    bool ASkillUsed = false, BSkillUsed = false, CSkillUsed = false, DSkillUsed = false;//引导任务专用判定

    private void Start()
    {
        if (GC == null)
            GC = GameControl.Instance;
    }
    //更新面板
    public void SetPanelContent(Employee T, Employee T2 = null)
    {
        Target = T;
        Target2 = T2;
        //送礼
        if (CEOSkillNum == 6)
        {
            SuccessLimit = 10;
            Text_Name.text = "给" + Target.Name + "送礼物";
            Text_SuccessContent.text = "好感度+5";
            Text_Requirement.text = "消耗体力:10";
            Text_Extra.text = "陌路-2 \n仇敌 - 4\n" + AddText(1) + AddText(6);
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        //闲聊
        else if (CEOSkillNum == 7)
        {
            SuccessLimit = 10;
            Text_Name.text = "与" + Target.Name + "闲聊";
            Text_SuccessContent.text = "好感度+10";
            Text_Requirement.text = "消耗体力:20";
            Text_Extra.text = "朋友 +1 \n挚友 +3 \n陌路 -2 \n仇敌 - 4 \n" + AddText(1) + AddText(6);
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        //删除8 改为 说服-批评责备
        else if (CEOSkillNum == 8)
        {
            SuccessLimit = 6;
            Text_Name.text = "批评责备" + Target.Name;
            Text_SuccessContent.text = "使对方获得1层苦涩(浅蓝)情绪";
            Text_Requirement.text = "消耗体力:20";
            Text_Extra.text = AddText(2) + AddText(6);
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        //删除9 改为 说服-点名表扬
        else if (CEOSkillNum == 9)
        {
            SuccessLimit = 6;
            Text_Name.text = "点名表扬" + Target.Name;
            Text_SuccessContent.text = "使对方获得1层愉悦(浅黄)情绪";
            Text_Requirement.text = "消耗体力:20";
            Text_Extra.text = AddText(2) + AddText(6);
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        //删除10 改为 说服-激怒
        else if (CEOSkillNum == 10)
        {
            SuccessLimit = 6;
            Text_Name.text = "激怒" + Target.Name;
            Text_SuccessContent.text = "使对方获得1层反感(浅红)情绪";
            Text_Requirement.text = "消耗体力:20";
            Text_Extra.text = AddText(2) + AddText(6);
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        //删除11 改为 魅力-拉升关系
        else if (CEOSkillNum == 11)
        {
            SuccessLimit = 10;
            Text_Name.text = "拉升" + Target.Name + "和" + Target2.Name + "的关系";
            Text_SuccessContent.text = "使指定的两名员工之间的关系上升20点";
            Text_Requirement.text = "消耗体力:35";
            Text_Extra.text = AddText(4) + AddText(6);
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        //改变信仰
        else if (CEOSkillNum == 16)
        {
            SuccessLimit = 14;
            Text_Name.text = "劝说" + Target.Name + "改变信仰";
            Text_SuccessContent.text = "指定一个信仰方向和一个文化方向，使对方的文化和信仰分别在对应的方向上产生巨大偏向";
            Text_Requirement.text = "消耗体力:50";
            Text_Extra.text = AddText(1) + AddText(3) + AddText(6) + "选择的方向与CEO自身相同 +1 \n选择的方向与CEO自身不同 -1\n";
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        //保持忠诚
        else if (CEOSkillNum == 17)
        {
            SuccessLimit = 10;
            Text_Name.text = "劝说" + Target.Name + "保持忠诚";
            Text_SuccessContent.text = "员工添加“忠诚”状态";
            Text_Requirement.text = "消耗体力:30";
            Text_Extra.text = AddText(2) + AddText(6);
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        //删除18  改为安抚，删除原安抚
        else if (CEOSkillNum == 18)
        {
            SuccessLimit = 10;
            Text_Name.text = "安抚" + Target.Name;
            Text_SuccessContent.text = "恢复对方心力10%";
            Text_Requirement.text = "消耗体力:20";
            Text_Extra.text = AddText(5) + AddText(6);
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        //间谍
        else if (CEOSkillNum == 19)
        {
            SuccessLimit = 10;
            Text_Name.text = "说服" + Target.Name + "成为间谍";
            Text_SuccessContent.text = "成功后对方会成为商业间谍潜入他人公司";
            Text_Requirement.text = "消耗体力:80";
            Text_Extra.text = AddText(2) + "对方“忠诚”状态 +2\n" + AddText(6); 
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        //加入核心团队说服
        else if (CEOSkillNum == 20)
        {
            SuccessLimit = 8;
            Text_Name.text = "说服" + Target.Name + "加入核心团队";
            Text_SuccessContent.text = "对方加入核心团队";
            Text_Requirement.text = "无额外消耗";
            Text_Extra.text = AddText(5);
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        //离开核心团队说服
        else if (CEOSkillNum == 21)
        {
            SuccessLimit = 14;
            Text_Name.text = "说服" + Target.Name + "离开核心团队";
            Text_SuccessContent.text = "对方离开核心团队";
            Text_Requirement.text = "无额外消耗";
            Text_Extra.text = AddText(7);
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        Text_ConditionContent.text = "一枚20面骰子，点数>=" + SuccessLimit;
        Text_Name2.text = Text_Name.text;
        SelectPanel.GetComponent<WindowBaseControl>().SetWndState(true);
        WarningPanel.SetActive(false);
        ResultPanel.GetComponent<WindowBaseControl>().SetWndState(false);
    }

    //确认发动技能时的条件检测
    public void ConfirmSkill()
    {
        if (CEOSkillNum == 6)
        {
            if (GC.Stamina >= 10)
            {
                GC.Stamina -= 10;
                ActiveSkill();
            }
            else
                WarningPanel.SetActive(true);
        }
        else if (CEOSkillNum == 7)
        {
            if (GC.Stamina >= 20)
            {
                GC.Stamina -= 20;
                ActiveSkill();
            }
            else
                WarningPanel.SetActive(true);
        }
        else if (CEOSkillNum == 8)
        {
            if (GC.Stamina >= 20)
            {
                GC.Stamina -= 20;
                ActiveSkill();
            }
            else
                WarningPanel.SetActive(true);
        }
        else if (CEOSkillNum == 9)
        {
            if (GC.Stamina >= 20)
            {
                GC.Stamina -= 20;
                ActiveSkill();
            }
            else
                WarningPanel.SetActive(true);
        }
        else if (CEOSkillNum == 10)
        {
            if (GC.Stamina >= 20)
            {
                GC.Stamina -= 20;
                ActiveSkill();
            }
            else
                WarningPanel.SetActive(true);
        }
        else if (CEOSkillNum == 11)
        {
            if (GC.Stamina >= 35)
            {
                GC.Stamina -= 35;
                ActiveSkill();
            }
            else
                WarningPanel.SetActive(true);
        }
        else if (CEOSkillNum == 16)
        {
            if (GC.Stamina >= 50)
            {
                CRChangePanel.SetActive(true);
            }
            else
                WarningPanel.SetActive(true);
        }
        else if (CEOSkillNum == 17)
        {
            if (GC.Stamina >= 30)
            {
                GC.Stamina -= 30;
                ActiveSkill();
            }
            else
                WarningPanel.SetActive(true);
        }
        else if (CEOSkillNum == 18)
        {
            if (GC.Stamina >= 20)
            {
                GC.Stamina -= 20;
                ActiveSkill();
            }
            else
                WarningPanel.SetActive(true);
        }
        else if (CEOSkillNum == 19)
        {
            if (GC.Stamina >= 80)
            {
                GC.Stamina -= 80;
                ActiveSkill();
            }
            else
                WarningPanel.SetActive(true);
        }
        else if (CEOSkillNum == 20 || CEOSkillNum == 21)
        {
            ActiveSkill();
        }
    }
    //信仰改变技能确认
    public void ConfirmCRChange()
    {
        ActiveSkill();
        GC.Stamina -= 50;
    }

    void ActiveSkill()
    {
        int num = CalcExtra() + Random.Range(1, 21);
        SelectPanel.GetComponent<WindowBaseControl>().SetWndState(false);
        if (num >= SuccessLimit)
        {
            Text_Result.text = Text_SuccessContent.text;
            FailText.SetActive(false);
            SuccessText.SetActive(true);
            if (CEOSkillNum == 6)
            {
                CEO.ChangeRelation(Target, 5);
                Target.ChangeRelation(CEO, 5);
            }
            else if (CEOSkillNum == 7)
            {
                CEO.ChangeRelation(Target, 10);
                Target.ChangeRelation(CEO, 10);
            }
            else if (CEOSkillNum == 8)
            {
                Target.AddEmotion(EColor.LBlue);
                CSkillUsed = true;
                if (CSkillUsed == true && DSkillUsed == true)
                    GC.QC.Finish(12);
            }
            else if (CEOSkillNum == 9)
            {
                Target.AddEmotion(EColor.LYellow);
                DSkillUsed = true;
                if (CSkillUsed == true && DSkillUsed == true)
                    GC.QC.Finish(12);
            }
            else if (CEOSkillNum == 10)
            {
                Target.AddEmotion(EColor.LRed);
                //引导任务判定
                ASkillUsed = true;
                if (ASkillUsed == true && BSkillUsed == true)
                    GC.QC.Finish(6);
            }
            else if (CEOSkillNum == 11)
            {
                Target.ChangeRelation(Target2, 20);
                Target2.ChangeRelation(Target, 20);
                GC.Text_EmpSelectTip.text = "选择第一个员工";
                GC.CurrentEmpInfo2 = null;
            }
            else if (CEOSkillNum == 16)
            {
                if (CultureValue == 1)
                    Target.ChangeCharacter(0, -50);
                else
                    Target.ChangeCharacter(0, 50);

                if (ReligionValue == 1)
                    Target.ChangeCharacter(1, -50);
                else
                    Target.ChangeCharacter(1, 50);
                //引导任务判定
                BSkillUsed = true;
                if (ASkillUsed == true && BSkillUsed == true)
                    GC.QC.Finish(6);
            }
            else if (CEOSkillNum == 17)
            {
                //Target.InfoDetail.AddPerk(new Perk41(Target), true);
            }
            else if (CEOSkillNum == 18)
            {
                Target.Mentality += (int)(Target.MentalityLimit * 0.1f);
            }
            else if (CEOSkillNum == 19)
            {
                GC.CurrentEmpInfo = Target.InfoDetail;
                //GC.foeControl.ShowSpyPanel();
            }
            else if (CEOSkillNum == 20)
            {
                GC.BSC.CurrentBSInfo.EmpJoin(Target);
                GC.TotalEmpPanel.SetWndState(false);
                GC.ResetSelectMode();
            }
            else if (CEOSkillNum == 21)
            {
                GC.BSC.CurrentBSInfo.EmpLeft();
            }
            CEO.InfoDetail.AddHistory(Text_Name.text + "成功," + Text_Result.text);
            if (Target != null)
                Target.InfoDetail.AddHistory("CEO" + Text_Name.text + "成功," + Text_Result.text);
        }
        else
        {
            Text_Result.text = "技能使用失败";
            FailText.SetActive(true);
            SuccessText.SetActive(false);           
            CEO.InfoDetail.AddHistory(Text_Name.text + "失败," + Text_Result.text);
            if (Target != null)
                Target.InfoDetail.AddHistory("CEO" + Text_Name.text + "失败," + Text_Result.text);
            if(CEOSkillNum == 10)
            {
                ASkillUsed = true;
                if (ASkillUsed == true && BSkillUsed == true)
                    GC.QC.Finish(6);
            }
            else if (CEOSkillNum == 16)
            {
                //引导任务判定
                BSkillUsed = true;
                if (ASkillUsed == true && BSkillUsed == true)
                    GC.QC.Finish(6);
                //GC.QC.Finish(6);
            }
            else if (CEOSkillNum == 11)
            {
                GC.Text_EmpSelectTip.text = "选择第一个员工";
                GC.CurrentEmpInfo2 = null;
            }
            else if (CEOSkillNum == 20 || CEOSkillNum == 21)
            {
                GC.BSC.CurrentBSInfo = null;
                Target.CoreMemberTime += 96;
                GC.TotalEmpPanel.SetWndState(false);
                GC.ResetSelectMode();
            }
        }
        ResultPanel.GetComponent<WindowBaseControl>().SetWndState(true);
    }

    //改变信仰选项对应函数
    public void CultureChangeA(bool value)
    {
        if (value == true)
            CultureValue = 1;
    }
    public void CultureChangeB(bool value)
    {
        if (value == true)
            CultureValue = 2;
    }
    public void ReligionChangeA(bool value)
    {
        if (value == true)
            ReligionValue = 1;
    }
    public void ReligionChangeB(bool value)
    {
        if (value == true)
            ReligionValue = 2;
    }
    //培养技能选择
    public void SetTargetSkill(int type)
    {

    }

    //返回成功率
    int CalcPosb()
    {
        int value = (int)((float)(20 + CalcExtra() - SuccessLimit) / 20.0f * 100);
        if (value < 0)
            value = 0;
        else if (value > 100)
            value = 100;
        return value;
    }
    //计算额外效果
    int CalcExtra()
    {
        int value = 0;
        Relation r = CEO.FindRelation(Target);
        //送礼
        if (CEOSkillNum == 6)
        {
            if (r.FriendValue == -1)
                value -= 2;
            else if (r.FriendValue == -2)
                value -= 4;
            value += CalcExtra(1) + CalcExtra(6);
        }
        //闲聊
        else if (CEOSkillNum == 7)
        {
            if (r.FriendValue == 1)
                value += 1;
            else if (r.FriendValue == 2)
                value += 3;
            else if (r.FriendValue == -1)
                value -= 2;
            else if (r.FriendValue == -2)
                value -= 4;
            value += CalcExtra(1) + CalcExtra(6);
        }
        //8威胁 改为 说服-批评责备
        else if (CEOSkillNum == 8)
        {
            value += CalcExtra(2) + CalcExtra(6);
        }
        //9朋友 改为 说服-点名表扬
        else if (CEOSkillNum == 9)
        {
            value += CalcExtra(2) + CalcExtra(6);
        }
        //10挚友 改为 说服-激怒
        else if (CEOSkillNum == 10)
        {
            value += CalcExtra(2) + CalcExtra(6);
        }
        //11陌路 改为 魅力-拉升关系
        else if (CEOSkillNum == 11)
        {
            value += CalcExtra(4) + CalcExtra(6);
        }
        //改变信仰
        else if (CEOSkillNum == 16)
        {
            if(CultureValue == 1)
            {
                if (CEO.CharacterTendency[0] == -1)
                    value += 1;
                else
                    value -= 1;
            }
            else if (CultureValue == 2)
            {
                if (CEO.CharacterTendency[0] == 1)
                    value += 1;
                else
                    value -= 1;
            }
            if (ReligionValue == 1)
            {
                if (CEO.CharacterTendency[1] == -1)
                    value += 1;
                else
                    value -= 1;
            }
            else if (ReligionValue == 2)
            {
                if (CEO.CharacterTendency[1] == 1)
                    value += 1;
                else
                    value -= 1;
            }
            value += CalcExtra(1) + CalcExtra(3) + CalcExtra(6);
        }
        //保持忠诚
        else if (CEOSkillNum == 17)
        {
            value += CalcExtra(2) + CalcExtra(6);
        }
        //18提升信念(删除) 改为安抚
        else if (CEOSkillNum == 18)
        {
            value += CalcExtra(5) + CalcExtra(6);
        }
        //19间谍
        else if (CEOSkillNum == 19)
        {
            foreach(PerkInfo info in Target.InfoDetail.PerksInfo)
            {
                if(info.CurrentPerk.Num == 41)
                {
                    value += 2;
                    break;
                }
            }
            value += CalcExtra(2) + CalcExtra(6);
        }
        //20核心成员加入说服
        else if (CEOSkillNum == 20)
        {
            value += CalcExtra(5);
        }
        //21核心成员离开说服
        else if (CEOSkillNum == 21)
        {
            value += CalcExtra(7);
        }
        return value;
    }

    string AddText(int type)
    {
        string content = "";
        if (type == 1)
            content += "存在“出众”特质时 +1 \n存在“领袖气质”特质时 +2 \n存在“传奇”特质时 +3\n";
        else if (type == 2)
            content += "存在“善辩”特质时 +1 \n存在“说客”特质时 +2 \n存在“雄辩家”特质时 +3\n";
        else if (type == 3)
            content += "对方身上具有: \n1个愤怒（红色）情绪 +4 \n1个沮丧（紫色）情绪 +4 \n1个反感（浅红）情绪 +2 \n1个委屈（浅紫）情绪 +2\n";
        else if (type == 4)
            content += "双方身上具有: \n1个好奇（黄色）情绪 +2 \n1个沉思（绿色）情绪 +2 \n1个愉悦（浅黄）情绪 +1 \n1个敬畏（浅绿）情绪 +1 \n" +
                       "1个愤怒（红色）情绪 -2 \n1个沮丧（紫色）情绪 -2 \n1个反感（浅红）情绪 -1 \n1个委屈（浅紫）情绪 -1\n";
        else if (type == 5)
            content += "对方身上具有: \n1个好奇（黄色）情绪 +2 \n1个沉思（绿色）情绪 +2 \n1个愉悦（浅黄）情绪 +1 \n1个敬畏（浅绿）情绪 +1 \n" +
                       "1个愤怒（红色）情绪 -2 \n1个沮丧（紫色）情绪 -2 \n1个反感（浅红）情绪 -1 \n1个委屈（浅紫）情绪 -1\n";
        else if (type == 6)
            content += "士气>80时 +2 \n士气 > 60时 +1 \n士气 < 40时 -1 \n士气 < 20时 -2\n";
        else if (type == 7)
            content += "对方身上具有: \n1个好奇（黄色）情绪 -2 \n1个沉思（绿色）情绪 -2 \n1个愉悦（浅黄）情绪 -1 \n1个敬畏（浅绿）情绪 -1 \n" +
                       "1个愤怒（红色）情绪 +2 \n1个沮丧（紫色）情绪 +2 \n1个反感（浅红）情绪 +1 \n1个委屈（浅紫）情绪+1\n";

        return content;
    }
    int CalcExtra(int type)
    {
        int value = 0;
        if (type == 1)
        {
            foreach (PerkInfo perk in CEO.InfoDetail.PerksInfo)
            {
                if (perk.CurrentPerk.Num == 78)
                    value += 1;
                else if (perk.CurrentPerk.Num == 79)
                    value += 2;
                else if (perk.CurrentPerk.Num == 80)
                    value += 3;
            }
        }
        else if (type == 2)
        {
            foreach (PerkInfo perk in CEO.InfoDetail.PerksInfo)
            {
                if (perk.CurrentPerk.Num == 75)
                    value += 1;
                else if (perk.CurrentPerk.Num == 76)
                    value += 2;
                else if (perk.CurrentPerk.Num == 77)
                    value += 3;
            }
        }
        else if (type == 3)
        {
            foreach (Emotion e in Target.CurrentEmotions)
            {
                if (e.color == EColor.Red || e.color == EColor.Purple)
                    value += (e.Level * 4);
                else if (e.color == EColor.LRed || e.color == EColor.LPurple)
                    value += (e.Level * 2);
            }
        }
        else if (type == 4)
        {
            foreach (Emotion e in Target.CurrentEmotions)
            {
                if (e.color == EColor.Yellow || e.color == EColor.Green)
                    value += (e.Level * 2);
                else if (e.color == EColor.LYellow || e.color == EColor.LGreen)
                    value += e.Level;
                else if (e.color == EColor.LRed || e.color == EColor.LPurple)
                    value -= e.Level;
                else if (e.color == EColor.Red || e.color == EColor.Purple)
                    value -= (e.Level * 2);
            }
            foreach (Emotion e in Target2.CurrentEmotions)
            {
                if (e.color == EColor.Yellow || e.color == EColor.Green)
                    value += (e.Level * 2);
                else if (e.color == EColor.LYellow || e.color == EColor.LGreen)
                    value += e.Level;
                else if (e.color == EColor.LRed || e.color == EColor.LPurple)
                    value -= e.Level;
                else if (e.color == EColor.Red || e.color == EColor.Purple)
                    value -= (e.Level * 2);
            }
        }
        else if (type == 5)
        {
            foreach (Emotion e in Target.CurrentEmotions)
            {
                if (e.color == EColor.Yellow || e.color == EColor.Green)
                    value += (e.Level * 2);
                else if (e.color == EColor.LYellow || e.color == EColor.LGreen)
                    value += e.Level;
                else if (e.color == EColor.LRed || e.color == EColor.LPurple)
                    value -= e.Level;
                else if (e.color == EColor.Red || e.color == EColor.Purple)
                    value -= (e.Level * 2);
            }
        }
        else if (type == 6)
        {
            if (GC.Morale > 80)
                value += 2;
            else if (GC.Morale > 60)
                value += 1;
            else if (GC.Morale >= 40)
                value += 0;
            else if (GC.Morale >= 20)
                value -= 1;
            else if (GC.Morale < 20)
                value -= 2;
        }
        else if (type == 7)
        {
            foreach (Emotion e in Target.CurrentEmotions)
            {
                if (e.color == EColor.Yellow || e.color == EColor.Green)
                    value -= (e.Level * 2);
                else if (e.color == EColor.LYellow || e.color == EColor.LGreen)
                    value -= e.Level;
                else if (e.color == EColor.LRed || e.color == EColor.LPurple)
                    value += e.Level;
                else if (e.color == EColor.Red || e.color == EColor.Purple)
                    value += (e.Level * 2);
            }
        }
        return value;
    }

    public void SetCEOSkill(int num)
    {
        if (num == 1)
        {
            if (CEO.StaminaLimit >= 45)
            {
                CEOSkillNum = 1;
                GC.ShowDepSelectPanel(GC.CurrentDeps);
                GC.SelectMode = 6;
            }
            else
            {
                GC.CreateMessage("CEO体力上限不足");
            }
        }
        else if (num == 2)
        {
            if (CEO.Stamina >= 30)
            {
                CEOSkillNum = 2;
                GC.ShowDepSelectPanel(GC.CurrentDeps);
                GC.SelectMode = 6;
            }
        }
        else if (num == 3)
        {
            if (CEO.StaminaLimit >= 45)
            {
                GC.SelectMode = 6;
                CEOSkillNum = num;
                GC.TotalEmpPanel.SetWndState(true);
                GC.Text_EmpSelectTip.gameObject.SetActive(true);
                GC.Text_EmpSelectTip.text = "选择一个员工";
            }
            else
            {
                GC.CreateMessage("CEO体力上限不足");
            }
        }
        else if (num == 4)
        {
            if (CEO.Stamina >= 20)
            {
                GC.SelectMode = 6;
                CEOSkillNum = 4;
                GC.TotalEmpPanel.SetWndState(true);
            }
        }
        else if (num == 21)
        {
            CEOSkillNum = num;
            SetPanelContent(GC.BSC.CurrentBSInfo.emp);
        }
        else
        {
            CEOSkillNum = num;
            GC.SelectMode = 6;
            if (CEOSkillNum == 11)
                GC.SelectMode = 10;
            GC.TotalEmpPanel.SetWndState(true);
            GC.Text_EmpSelectTip.gameObject.SetActive(true);
            GC.Text_EmpSelectTip.text = "选择一个员工";
            if (num == 5 || num == 19)
            {
                foreach (Employee e in GC.CurrentEmployees)
                {
                    if (e.InfoDetail.Entity.OutCompany == true)
                        e.InfoB.gameObject.SetActive(false);
                }
            }
            else if (num == 20)
            {
                foreach (Employee e in GC.CurrentEmployees)
                {
                    if (e.isCEO == true || e.CoreMemberTime > 0 || GC.BSC.CoreMembers.Contains(e))
                        e.InfoB.gameObject.SetActive(false);
                }
            }
        }
        this.gameObject.GetComponent<WindowBaseControl>().SetWndState(false);
    }

    public void TrainEmp(int type)
    {
        if (CEO.StaminaLimit >= 45)
        {
            new EmpBuff(GC.CurrentEmpInfo.emp, type);
            new EmpBuff(CEO, 16, -45);
            GC.CurrentEmpInfo.emp.InfoDetail.AddHistory("接受了CEO的培养");
            CEO.InfoDetail.AddHistory("培养了" + GC.CurrentEmpInfo.emp.Name);
            GC.QC.Init("技能释放成功\n\n" + GC.CurrentEmpInfo.emp.Name + "所选技能每回合无条件获得3点经验，持续4个月");
        }
        else
        {
            GC.CreateMessage("CEO体力上限不足");
        }
    }

    public void SetEmpVacationTime(int type)
    {
        GC.CurrentEmpInfo.emp.VacationTime = type;
        GC.CurrentEmpInfo.emp.InfoDetail.Entity.SetBusy();
        GC.CurrentEmpInfo.emp.InfoDetail.AddHistory("被安排放假" + type + "工时");
        CEO.InfoDetail.AddHistory("安排" + GC.CurrentEmpInfo.emp.Name + "放假" + type + "工时");
        if (GC.CurrentEmpInfo.emp.isCEO == true)
        {
            GC.ResetSelectMode();
            SkillButton.interactable = false;
            GC.CEOVacation = true;
            foreach (DepControl dep in GC.CurrentDeps)
            {
                dep.AddPerk(new Perk115(null));
            }
        }
    }
}
