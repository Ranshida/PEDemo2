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
    public GameObject SelectPanel, ResultPanel, WarningPanel, SuccessText, FailText, ResultCheckButton, ConvinceButtons, 
        EmpSelectWarning, SpyButton, CRChangePanel;
    public Employee Target, Target2 ,CEO;

    //改变信仰的相关参数
    public int CultureValue = 1, ReligionValue = 1, SkillValue = 1;

    int SuccessLimit = 10;
    bool ASkillUsed = false, BSkillUsed = false;//引导任务专用判定

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
        if (GC.CEOSkillNum == 6)
        {
            SuccessLimit = 10;
            Text_Name.text = "给" + Target.Name + "送礼物";
            Text_SuccessContent.text = "好感度+5";
            Text_Requirement.text = "消耗体力:10";
            Text_Extra.text = "陌路-2 \n仇敌 - 4\n" + AddText(1) + AddText(6);
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        //闲聊
        else if (GC.CEOSkillNum == 7)
        {
            SuccessLimit = 10;
            Text_Name.text = "与" + Target.Name + "闲聊";
            Text_SuccessContent.text = "好感度+10";
            Text_Requirement.text = "消耗体力:20";
            Text_Extra.text = "朋友 +1 \n挚友 +3 \n陌路 -2 \n仇敌 - 4 \n" + AddText(1) + AddText(6);
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        //删除8 改为 说服-批评责备
        else if (GC.CEOSkillNum == 8)
        {
            SuccessLimit = 6;
            Text_Name.text = "批评责备" + Target.Name;
            Text_SuccessContent.text = "使对方获得1层苦涩(浅蓝)情绪";
            Text_Requirement.text = "消耗体力:20";
            Text_Extra.text = AddText(2) + AddText(6);
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        //删除9 改为 说服-点名表扬
        else if (GC.CEOSkillNum == 9)
        {
            SuccessLimit = 6;
            Text_Name.text = "点名表扬" + Target.Name;
            Text_SuccessContent.text = "使对方获得1层愉悦(浅黄)情绪";
            Text_Requirement.text = "消耗体力:20";
            Text_Extra.text = AddText(2) + AddText(6);
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        //删除10 改为 说服-激怒
        else if (GC.CEOSkillNum == 10)
        {
            SuccessLimit = 6;
            Text_Name.text = "激怒" + Target.Name;
            Text_SuccessContent.text = "使对方获得1层反感(浅红)情绪";
            Text_Requirement.text = "消耗体力:20";
            Text_Extra.text = AddText(2) + AddText(6);
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        //删除11 改为 魅力-拉升关系
        else if (GC.CEOSkillNum == 11)
        {
            SuccessLimit = 10;
            Text_Name.text = "拉升" + Target.Name + "和" + Target2.Name + "的关系";
            Text_SuccessContent.text = "使指定的两名员工之间的关系上升20点";
            Text_Requirement.text = "消耗体力:35";
            Text_Extra.text = AddText(4) + AddText(6);
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        //删除12
        else if (GC.CEOSkillNum == 12)
        {
            Text_Name.text = "与" + Target.Name + "结成仇人";
            Text_SuccessContent.text = "双方结为仇人";
            Text_ConditionContent.text = "对方信念-5";
            Text_Requirement.text = "需要好感度<-40";
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        //删除13 
        else if (GC.CEOSkillNum == 13)
        {
            Text_Name.text = "与" + Target.Name + "创建派系";
            Text_SuccessContent.text = "双方创建一个派系";
            Text_ConditionContent.text = "对方信念-5，好感度 - 10";
            Text_Requirement.text = "双方都不属于某一派系";
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        //删除14
        else if (GC.CEOSkillNum == 14)
        {
            Text_Name.text = "加入" + Target.Name + "的派系";
            Text_SuccessContent.text = "加入对方所在的派系";
            Text_ConditionContent.text = "对方信念-5，好感度 - 10";
            Text_Requirement.text = "自己不属于某一派系，对方属于某一派系";
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        //删除15
        else if (GC.CEOSkillNum == 15)
        {
            Text_Name.text = "加入" + Target.Name + "的派系";
            Text_SuccessContent.text = "员工被说服，可以从说服的可执行列表中选择一项命令员工进行";
            Text_ConditionContent.text = "好感度-5员工独裁文化倾向 + 10";
            Text_Requirement.text = "体力20";
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        //改变信仰
        else if (GC.CEOSkillNum == 16)
        {
            SuccessLimit = 14;
            Text_Name.text = "劝说" + Target.Name + "改变信仰";
            Text_SuccessContent.text = "指定一个信仰方向和一个文化方向，使对方的文化和信仰分别在对应的方向上产生巨大偏向";
            Text_Requirement.text = "消耗体力:50";
            Text_Extra.text = AddText(1) + AddText(3) + AddText(6) + "选择的方向与CEO自身相同 +1 \n选择的方向与CEO自身不同 -1\n";
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        //保持忠诚
        else if (GC.CEOSkillNum == 17)
        {
            SuccessLimit = 10;
            Text_Name.text = "劝说" + Target.Name + "保持忠诚";
            Text_SuccessContent.text = "员工添加“忠诚”状态";
            Text_Requirement.text = "消耗体力:30";
            Text_Extra.text = AddText(2) + AddText(6);
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        //删除18  改为安抚，删除原安抚
        else if (GC.CEOSkillNum == 18)
        {
            SuccessLimit = 10;
            Text_Name.text = "安抚" + Target.Name;
            Text_SuccessContent.text = "恢复对方心力10%";
            Text_Requirement.text = "消耗体力:20";
            Text_Extra.text = AddText(5) + AddText(6);
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        //间谍
        else if (GC.CEOSkillNum == 19)
        {
            SuccessLimit = 10;
            Text_Name.text = "说服" + Target.Name + "成为间谍";
            Text_SuccessContent.text = "成功后对方会成为商业间谍潜入他人公司";
            Text_Requirement.text = "消耗体力:80";
            Text_Extra.text = AddText(2) + "对方“忠诚”状态 +2\n" + AddText(6); 
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        Text_ConditionContent.text = "一枚20面骰子，点数>=" + SuccessLimit;
        Text_Name2.text = Text_Name.text;
        SelectPanel.SetActive(true);
        WarningPanel.SetActive(false);
        ResultPanel.SetActive(false);
    }
    //（旧）技能CD设置
    public void CEOSkillConfirm()
    {
        if (GC.CEOSkillNum == 1)
        {
            GC.Stamina -= 20;
            GC.CEOSkillCD[0] = 16;
        }
        else if (GC.CEOSkillNum == 2)
        {
            GC.Stamina -= 30;
            GC.CEOSkillCD[1] = 24;
        }
        else if (GC.CEOSkillNum == 3)
        {
            //GC.Stamina -= 50;
            //GC.CEOSkillCD[2] = 32;
        }
        else if (GC.CEOSkillNum == 4)
        {
            GC.Stamina -= 20;
            GC.CEOSkillCD[3] = 8;
        }
        else if (GC.CEOSkillNum == 5)
        {
            //GC.Stamina -= 50;
            //GC.CEOSkillCD[4] = 32;
        }
        //GC.CEOSkillButton[GC.CEOSkillNum - 1].interactable = false;
        //GC.Text_CEOSkillCD[GC.CEOSkillNum - 1].gameObject.SetActive(true);
        //GC.Text_CEOSkillCD[GC.CEOSkillNum - 1].text = "CD:" + GC.CEOSkillCD[GC.CEOSkillNum - 1] + "时";
        //GC.CEOSkillNum = 0;
        //GC.SelectMode = 0;
    }

    //确认发动技能时的条件检测
    public void ConfirmSkill()
    {
        if (GC.CEOSkillNum == 6)
        {
            if (GC.Stamina >= 10)
            {
                GC.Stamina -= 10;
                ActiveSkill();
            }
            else
                WarningPanel.SetActive(true);
        }
        else if (GC.CEOSkillNum == 7)
        {
            if (GC.Stamina >= 20)
            {
                GC.Stamina -= 20;
                ActiveSkill();
            }
            else
                WarningPanel.SetActive(true);
        }
        else if (GC.CEOSkillNum == 8)
        {
            if (GC.Stamina >= 20)
            {
                GC.Stamina -= 20;
                ActiveSkill();
            }
            else
                WarningPanel.SetActive(true);
        }
        else if (GC.CEOSkillNum == 9)
        {
            if (GC.Stamina >= 20)
            {
                GC.Stamina -= 20;
                ActiveSkill();
            }
            else
                WarningPanel.SetActive(true);
        }
        else if (GC.CEOSkillNum == 10)
        {
            if (GC.Stamina >= 20)
            {
                GC.Stamina -= 20;
                ActiveSkill();
            }
            else
                WarningPanel.SetActive(true);
        }
        else if (GC.CEOSkillNum == 11)
        {
            if (GC.Stamina >= 35)
            {
                GC.Stamina -= 35;
                ActiveSkill();
            }
            else
                WarningPanel.SetActive(true);
        }
        else if (GC.CEOSkillNum == 12)
        {
            Relation r = CEO.FindRelation(Target);
            if (r.RPoint < -40)
                ActiveSkill();
            else
                WarningPanel.SetActive(true);
        }
        else if (GC.CEOSkillNum == 13)
        {
            if (CEO.CurrentClique == null && Target.CurrentClique == null)
                ActiveSkill();
            else
                WarningPanel.SetActive(true);
        }
        else if (GC.CEOSkillNum == 14)
        {
            if (CEO.CurrentClique == null && Target.CurrentClique != null)
                ActiveSkill();
            else
                WarningPanel.SetActive(true);
        }
        else if (GC.CEOSkillNum == 15)
        {
            if (GC.Stamina >= 20)
            {
                GC.Stamina -= 20;
                ActiveSkill();
            }
            else
                WarningPanel.SetActive(true);
        }
        else if (GC.CEOSkillNum == 16)
        {
            if (GC.Stamina >= 50)
            {
                CRChangePanel.SetActive(true);
            }
            else
                WarningPanel.SetActive(true);
        }
        else if (GC.CEOSkillNum == 17)
        {
            if (GC.Stamina >= 30)
            {
                GC.Stamina -= 30;
                ActiveSkill();
            }
            else
                WarningPanel.SetActive(true);
        }
        else if (GC.CEOSkillNum == 18)
        {
            if (GC.Stamina >= 20)
            {
                GC.Stamina -= 20;
                ActiveSkill();
            }
            else
                WarningPanel.SetActive(true);
        }
        else if (GC.CEOSkillNum == 19)
        {
            if (GC.Stamina >= 80)
            {
                GC.Stamina -= 80;
                ActiveSkill();
            }
            else
                WarningPanel.SetActive(true);
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
        SelectPanel.SetActive(false);
        if (num >= SuccessLimit)
        {
            Text_Result.text = Text_SuccessContent.text;
            FailText.SetActive(false);
            SuccessText.SetActive(true);
            if (GC.CEOSkillNum == 6)
            {
                CEO.ChangeRelation(Target, 5);
                Target.ChangeRelation(CEO, 5);
            }
            else if (GC.CEOSkillNum == 7)
            {
                CEO.ChangeRelation(Target, 10);
                Target.ChangeRelation(CEO, 10);
            }
            else if (GC.CEOSkillNum == 8)
            {
                Target.AddEmotion(EColor.LBlue);
            }
            else if (GC.CEOSkillNum == 9)
            {
                Target.AddEmotion(EColor.LYellow);
            }
            else if (GC.CEOSkillNum == 10)
            {
                Target.AddEmotion(EColor.LRed);
                //引导任务判定
                ASkillUsed = true;
                if (ASkillUsed == true && BSkillUsed == true)
                    GC.QC.Finish(3);
                    //GC.QC.Finish(6);
            }
            else if (GC.CEOSkillNum == 11)
            {
                Target.ChangeRelation(Target2, 20);
                Target2.ChangeRelation(Target, 20);
                GC.Text_EmpSelectTip.text = "选择第一个员工";
                GC.CurrentEmpInfo2 = null;
            }
            else if (GC.CEOSkillNum == 12)
            {
                CEO.FindRelation(Target).FriendValue = -2;
                Target.FindRelation(CEO).FriendValue = -2;
            }
            else if (GC.CEOSkillNum == 13)
            {
                CEO.CurrentClique = new Clique();
                Target.CurrentClique = CEO.CurrentClique;
                CEO.CurrentClique.Members.Add(CEO);
                CEO.CurrentClique.Members.Add(Target);
            }
            else if (GC.CEOSkillNum == 14)
            {
                CEO.CurrentClique = Target.CurrentClique;
                CEO.CurrentClique.Members.Add(CEO);
            }
            else if (GC.CEOSkillNum == 15)
            {
                ResultCheckButton.SetActive(false);
                ConvinceButtons.SetActive(true);
                for (int i = 0; i < Target.InfoDetail.PerksInfo.Count; i++)
                {
                    if (Target.InfoDetail.PerksInfo[i].CurrentPerk.Num == 29)
                    {
                        SpyButton.SetActive(true);
                        break;
                    }
                }
            }
            else if (GC.CEOSkillNum == 16)
            {
                if (CultureValue == 1)
                    Target.ChangeCharacter(0, -50);
                else
                    Target.ChangeCharacter(0, 50);

                if (ReligionValue == 1)
                    Target.ChangeCharacter(1, -50);
                else if (CEO.Character[1] < 0)
                    Target.ChangeCharacter(1, 50);
                //引导任务判定
                BSkillUsed = true;
                if (ASkillUsed == true && BSkillUsed == true)
                    GC.QC.Finish(3);
                    //GC.QC.Finish(6);
            }
            else if (GC.CEOSkillNum == 17)
            {
                Target.InfoDetail.AddPerk(new Perk41(Target), true);
            }
            else if (GC.CEOSkillNum == 18)
            {
                Target.Mentality += (int)(Target.MentalityLimit * 0.1f);
            }
            else if (GC.CEOSkillNum == 19)
            {
                GC.CurrentEmpInfo = Target.InfoDetail;
                GC.foeControl.ShowSpyPanel();
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
            #region 旧失败效果
            //if (GC.CEOSkillNum == 6)
            //{
            //    CEO.ChangeRelation(Target, -5);
            //    Target.ChangeRelation(CEO, -5);
            //    Target.ChangeCharacter(0, -10);
            //}
            //else if (GC.CEOSkillNum == 7)
            //{
            //    CEO.ChangeRelation(Target, -5);
            //    Target.ChangeRelation(CEO, -5);
            //    Target.ChangeCharacter(0, -5);
            //}
            //else if (GC.CEOSkillNum == 8)
            //{
            //    CEO.ChangeRelation(Target, -20);
            //    Target.ChangeRelation(CEO, -20);
            //    Target.ChangeCharacter(0, -20);
            //}
            //else if (GC.CEOSkillNum == 9)
            //{
            //    CEO.ChangeRelation(Target, -10);
            //    Target.ChangeRelation(CEO, -10);
            //}
            //else if (GC.CEOSkillNum == 10)
            //{
            //    CEO.ChangeRelation(Target, -10);
            //    Target.ChangeRelation(CEO, -10);
            //}
            //else if (GC.CEOSkillNum == 12)
            //{
            //    Target.ChangeCharacter(4, -5);
            //}
            //else if (GC.CEOSkillNum == 13)
            //{
            //    CEO.ChangeRelation(Target, -10);
            //    Target.ChangeRelation(CEO, -10);
            //    Target.ChangeCharacter(4, -5);
            //}
            //else if (GC.CEOSkillNum == 14)
            //{
            //    CEO.ChangeRelation(Target, -10);
            //    Target.ChangeRelation(CEO, -10);
            //    Target.ChangeCharacter(4, -5);
            //}
            //else if (GC.CEOSkillNum == 15)
            //{
            //    CEO.ChangeRelation(Target, -5);
            //    Target.ChangeRelation(CEO, -5);
            //    Target.ChangeCharacter(0, -10);
            //}
            #endregion
            CEO.InfoDetail.AddHistory(Text_Name.text + "失败," + Text_Result.text);
            if (Target != null)
                Target.InfoDetail.AddHistory("CEO" + Text_Name.text + "失败," + Text_Result.text);
            if(GC.CEOSkillNum == 10)
            {
                ASkillUsed = true;
                if (ASkillUsed == true && BSkillUsed == true)
                    GC.QC.Finish(3);
            }
            else if (GC.CEOSkillNum == 16)
            {
                //引导任务判定
                BSkillUsed = true;
                if (ASkillUsed == true && BSkillUsed == true)
                    GC.QC.Finish(3);
                //GC.QC.Finish(6);
            }
            else if (GC.CEOSkillNum == 11)
            {
                GC.Text_EmpSelectTip.text = "选择第一个员工";
                GC.CurrentEmpInfo2 = null;
            }
        }
        ResultPanel.SetActive(true);
    }

    //旧的说服功能（已删）
    public void SetConvinceNum(int num)
    {
        //17获取支持 18调查 19当间谍 20解除内鬼
        if (num == 18 || num == 19)
        {
            //GC.CEOSkillNum = num;
            //GC.CurrentEmpInfo = Target.InfoDetail;
            //if (num == 18)
            //    EmpSelectWarning.SetActive(true);
            //else if (num == 19 && Target.InfoDetail.Entity.SpyTarget == null)
            //{
            //    GC.foeControl.TargetSelectPanel.SetActive(true);
            //    GC.TotalEmpContent.parent.parent.gameObject.SetActive(false);
            //}
        }
        else if (num == 17)
        {
            Target.ObeyTime += 32;
            GC.CreateMessage("从" + Target.Name + "处获取了支持");
        }
        else if (num == 20)
        {
            PerkInfo P = null;
            for (int i = 0; i < Target.InfoDetail.PerksInfo.Count; i++)
            {
                if (Target.InfoDetail.PerksInfo[i].CurrentPerk.Num == 29)
                {
                    P = Target.InfoDetail.PerksInfo[i];
                    break;
                }
            }
            if(P != null)
            {
                Target.InfoDetail.PerksInfo.Remove(P);
                Destroy(P.gameObject);
                GC.CreateMessage("解除了" + Target.Name + "的内鬼状态");
                Target.BaseMotivation[2] -= 50;
                Target.isSpy = false;
            }

        }
        ResultPanel.SetActive(false);
        ResultCheckButton.SetActive(true);
        SpyButton.SetActive(false);
        ConvinceButtons.SetActive(false);
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
        if (GC.CEOSkillNum == 6)
        {
            if (r.FriendValue == -1)
                value -= 2;
            else if (r.FriendValue == -2)
                value -= 4;
            value += CalcExtra(1) + CalcExtra(6);
        }
        //闲聊
        else if (GC.CEOSkillNum == 7)
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
        else if (GC.CEOSkillNum == 8)
        {
            value += CalcExtra(2) + CalcExtra(6);
        }
        //9朋友 改为 说服-点名表扬
        else if (GC.CEOSkillNum == 9)
        {
            value += CalcExtra(2) + CalcExtra(6);
        }
        //10挚友 改为 说服-激怒
        else if (GC.CEOSkillNum == 10)
        {
            value += CalcExtra(2) + CalcExtra(6);
        }
        //11陌路 改为 魅力-拉升关系
        else if (GC.CEOSkillNum == 11)
        {
            value += CalcExtra(4) + CalcExtra(6);
        }
        //12仇人(删除)
        else if (GC.CEOSkillNum == 12)
        {
            if ((CEO.Character[0] >= 20 && Target.Character[0] >= 20) || (CEO.Character[0] <= -20 && Target.Character[0] <= -20))
                value -= 1;
            else if ((CEO.Character[0] >= 20 && Target.Character[0] <= -20) || (CEO.Character[0] <= -20 && Target.Character[0] >= 20))
                value += 2;

            if ((CEO.Character[1] >= 20 && Target.Character[1] >= 20) || (CEO.Character[1] <= -20 && Target.Character[1] <= -20))
                value -= 1;
            else if ((CEO.Character[1] >= 20 && Target.Character[1] <= -20) || (CEO.Character[1] <= -20 && Target.Character[1] >= 20))
                value += 2;
        }
        //创建派系(删除)
        else if (GC.CEOSkillNum == 13)
        {
            if (r.FriendValue == 1)
                value += 1;
            else if (r.FriendValue == 2)
                value += 3;
            else if (r.FriendValue == -1)
                value -= 2;
            else if (r.FriendValue == -2)
                value -= 4;

            if ((CEO.Character[0] >= 20 && Target.Character[0] >= 20) || (CEO.Character[0] <= -20 && Target.Character[0] <= -20))
                value += 1;
            else if ((CEO.Character[0] >= 20 && Target.Character[0] <= -20) || (CEO.Character[0] <= -20 && Target.Character[0] >= 20))
                value -= 2;

            if ((CEO.Character[1] >= 20 && Target.Character[1] >= 20) || (CEO.Character[1] <= -20 && Target.Character[1] <= -20))
                value += 1;
            else if ((CEO.Character[1] >= 20 && Target.Character[1] <= -20) || (CEO.Character[1] <= -20 && Target.Character[1] >= 20))
                value -= 2;

            value += (int)(CEO.Charm * 0.2f);
            value += (int)(CEO.Convince * 0.2f);
        }
        //加入派系(删除)
        else if (GC.CEOSkillNum == 14)
        {
            if (r.FriendValue == 1)
                value += 1;
            else if (r.FriendValue == 2)
                value += 3;
            else if (r.FriendValue == -1)
                value -= 2;
            else if (r.FriendValue == -2)
                value -= 4;

            if ((CEO.Character[0] >= 20 && Target.Character[0] >= 20) || (CEO.Character[0] <= -20 && Target.Character[0] <= -20))
                value += 1;
            else if ((CEO.Character[0] >= 20 && Target.Character[0] <= -20) || (CEO.Character[0] <= -20 && Target.Character[0] >= 20))
                value -= 2;

            if ((CEO.Character[1] >= 20 && Target.Character[1] >= 20) || (CEO.Character[1] <= -20 && Target.Character[1] <= -20))
                value += 1;
            else if ((CEO.Character[1] >= 20 && Target.Character[1] <= -20) || (CEO.Character[1] <= -20 && Target.Character[1] >= 20))
                value -= 2;

            value += (int)(CEO.Charm * 0.2f);
            value += (int)(CEO.Convince * 0.2f);
        }
        //说服(删除)
        else if (GC.CEOSkillNum == 15)
        {
            if (r.FriendValue == 1)
                value += 1;
            else if (r.FriendValue == 2)
                value += 3;
            else if (r.FriendValue == -1)
                value -= 2;
            else if (r.FriendValue == -2)
                value -= 4;

            if (CEO.CurrentClique != null && Target.CurrentClique != null && CEO.CurrentClique == Target.CurrentClique)
                value += 3;

            value += (int)(CEO.Convince * 0.2f);
        }
        //改变信仰
        else if (GC.CEOSkillNum == 16)
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
        else if (GC.CEOSkillNum == 17)
        {
            value += CalcExtra(2) + CalcExtra(6);
        }
        //18提升信念(删除) 改为安抚
        else if (GC.CEOSkillNum == 18)
        {
            value += CalcExtra(5) + CalcExtra(6);
        }
        //19间谍
        else if (GC.CEOSkillNum == 19)
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
        return value;
    }
}
