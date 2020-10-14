using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CEOControl : MonoBehaviour
{
    public GameControl GC;
    public Text Text_Name, Text_Name2, Text_FailContent, Text_SuccessContent, Text_OptionContent, Text_Result, Text_Requirement;
    public GameObject SelectPanel, ResultPanel, WarningPanel, SuccessText, FailText, ResultCheckButton, ConvinceButtons, EmpSelectWarning, SpyButton;
    public Employee Target, CEO;

    private void Start()
    {
        if (GC == null)
            GC = GameControl.Instance;
    }

    public void SetPanelContent(Employee T)
    {
        Target = T;
        if (GC.CEOSkillNum == 6)
        {
            Text_Name.text = "给" + Target.Name + "送礼物";
            Text_SuccessContent.text = "好感度+10";
            Text_FailContent.text = "好感度-5，员工独裁文化倾向+10";
            Text_Requirement.text = "50金钱,体力10点";
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        else if (GC.CEOSkillNum == 7)
        {
            Text_Name.text = "与" + Target.Name + "闲聊";
            Text_SuccessContent.text = "好感度+5";
            Text_FailContent.text = "好感度-5，员工独裁文化倾向+5";
            Text_Requirement.text = "体力20点";
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        else if (GC.CEOSkillNum == 8)
        {
            Text_Name.text = "威胁" + Target.Name;
            Text_SuccessContent.text = "好感度-20，对方会在下次会议时无条件同意CEO的意见";
            Text_FailContent.text = "好感度-20，员工独裁文化倾向+20";
            Text_Requirement.text = "无";
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        else if (GC.CEOSkillNum == 9)
        {
            Text_Name.text = "与" + Target.Name + "结成朋友";
            Text_SuccessContent.text = "两人结为朋友";
            Text_FailContent.text = "好感度-10";
            Text_Requirement.text = "与对方不是朋友、挚友、情侣，需要好感度>50";
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        else if (GC.CEOSkillNum == 10)
        {
            Text_Name.text = "与" + Target.Name + "结成挚友";
            Text_SuccessContent.text = "两人结为挚友";
            Text_FailContent.text = "好感度-10";
            Text_Requirement.text = "与对方是朋友，需要好感度>80";
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        else if (GC.CEOSkillNum == 11)
        {
            Text_Name.text = "与" + Target.Name + "结成陌路";
            Text_SuccessContent.text = "单方面视对方为陌路";
            Text_FailContent.text = "无";
            Text_Requirement.text = "需要好感度<0";
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        else if (GC.CEOSkillNum == 12)
        {
            Text_Name.text = "与" + Target.Name + "结成仇人";
            Text_SuccessContent.text = "双方结为仇人";
            Text_FailContent.text = "对方信念-5";
            Text_Requirement.text = "需要好感度<-40";
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        else if (GC.CEOSkillNum == 13)
        {
            Text_Name.text = "与" + Target.Name + "创建派系";
            Text_SuccessContent.text = "双方创建一个派系";
            Text_FailContent.text = "对方信念-5，好感度 - 10";
            Text_Requirement.text = "双方都不属于某一派系";
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        else if (GC.CEOSkillNum == 14)
        {
            Text_Name.text = "加入" + Target.Name + "的派系";
            Text_SuccessContent.text = "加入对方所在的派系";
            Text_FailContent.text = "对方信念-5，好感度 - 10";
            Text_Requirement.text = "自己不属于某一派系，对方属于某一派系";
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        else if (GC.CEOSkillNum == 15)
        {
            Text_Name.text = "加入" + Target.Name + "的派系";
            Text_SuccessContent.text = "员工被说服，可以从说服的可执行列表中选择一项命令员工进行";
            Text_FailContent.text = "好感度-5员工独裁文化倾向 + 10";
            Text_Requirement.text = "体力20";
            Text_OptionContent.text = "执行，成功率:" + CalcPosb() + "%";
        }
        Text_Name2.text = Text_Name.text;
        SelectPanel.SetActive(true);
        ResultPanel.SetActive(false);
    }

    public void ConfirmSkill()
    {
        if (GC.CEOSkillNum == 6)
        {
            if (GC.Money >= 50 && GC.Stamina >= 10)
            {
                GC.Money -= 50;
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
            ActiveSkill();
        }
        else if (GC.CEOSkillNum == 9)
        {
            Relation r = CEO.FindRelation(Target);
            if (r.FriendValue < 1 && r.LoveValue < 3 && r.RPoint > 50)
                ActiveSkill();
            else
                WarningPanel.SetActive(true);
        }
        else if (GC.CEOSkillNum == 10)
        {
            Relation r = CEO.FindRelation(Target);
            if (r.FriendValue == 1 && r.RPoint > 80)
                ActiveSkill();
            else
                WarningPanel.SetActive(true);
        }
        else if (GC.CEOSkillNum == 11)
        {
            Relation r = CEO.FindRelation(Target);
            if (r.RPoint < 0)
                ActiveSkill();
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
    }

    void ActiveSkill()
    {
        int num = CalcExtra() + Random.Range(2, 13);
        SelectPanel.SetActive(false);
        if (num > 8)
        {
            Text_Result.text = Text_SuccessContent.text;
            FailText.SetActive(false);
            SuccessText.SetActive(true);
            if (GC.CEOSkillNum == 6)
            {
                CEO.ChangeRelation(Target, 10);
                Target.ChangeRelation(CEO, 10);
            }
            else if (GC.CEOSkillNum == 7)
            {
                CEO.ChangeRelation(Target, 10);
                Target.ChangeRelation(CEO, 10);
            }
            else if (GC.CEOSkillNum == 8)
            {
                CEO.ChangeRelation(Target, -20);
                Target.ChangeRelation(CEO, -20);
                Target.ObeyTime += 32;
            }
            else if (GC.CEOSkillNum == 9)
            {
                CEO.FindRelation(Target).FriendValue = 1;
                Target.FindRelation(CEO).FriendValue = 1;
            }
            else if (GC.CEOSkillNum == 10)
            {
                CEO.FindRelation(Target).FriendValue = 2;
                Target.FindRelation(CEO).FriendValue = 2;
            }
            else if (GC.CEOSkillNum == 11)
            {
                CEO.FindRelation(Target).FriendValue = -1;
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
        }
        else
        {
            Text_Result.text = Text_FailContent.text;
            FailText.SetActive(true);
            SuccessText.SetActive(false);
            if (GC.CEOSkillNum == 6)
            {
                CEO.ChangeRelation(Target, -5);
                Target.ChangeRelation(CEO, -5);
                Target.ChangeCharacter(0, -10);
            }
            else if (GC.CEOSkillNum == 7)
            {
                CEO.ChangeRelation(Target, -5);
                Target.ChangeRelation(CEO, -5);
                Target.ChangeCharacter(0, -5);
            }
            else if (GC.CEOSkillNum == 8)
            {
                CEO.ChangeRelation(Target, -20);
                Target.ChangeRelation(CEO, -20);
                Target.ChangeCharacter(0, -20);
            }
            else if (GC.CEOSkillNum == 9)
            {
                CEO.ChangeRelation(Target, -10);
                Target.ChangeRelation(CEO, -10);
            }
            else if (GC.CEOSkillNum == 10)
            {
                CEO.ChangeRelation(Target, -10);
                Target.ChangeRelation(CEO, -10);
            }
            else if (GC.CEOSkillNum == 12)
            {
                Target.ChangeCharacter(4, -5);
            }
            else if (GC.CEOSkillNum == 13)
            {
                CEO.ChangeRelation(Target, -10);
                Target.ChangeRelation(CEO, -10);
                Target.ChangeCharacter(4, -5);
            }
            else if (GC.CEOSkillNum == 14)
            {
                CEO.ChangeRelation(Target, -10);
                Target.ChangeRelation(CEO, -10);
                Target.ChangeCharacter(4, -5);
            }
            else if (GC.CEOSkillNum == 15)
            {
                CEO.ChangeRelation(Target, -5);
                Target.ChangeRelation(CEO, -5);
                Target.ChangeCharacter(0, -10);
            }
        }
        ResultPanel.SetActive(true);
    }

    public void SetConvinceNum(int num)
    {
        //17获取支持 18调查 19当间谍 20解除内鬼
        if (num == 18 || num == 19)
        {
            GC.CEOSkillNum = num;
            GC.CurrentEmpInfo = Target.InfoDetail;
            if (num == 18)
                EmpSelectWarning.SetActive(true);
            else if (num == 19 && Target.InfoDetail.Entity.SpyTarget == null)
            {
                GC.foeControl.TargetSelectPanel.SetActive(true);
                GC.TotalEmpContent.parent.parent.gameObject.SetActive(false);
            }
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

    int CalcPosb()
    {
        int value = (int)((float)(CalcExtra() + 4) / 11.0f * 100);
        if (value < 0)
            value = 0;
        else if (value > 100)
            value = 100;
        return value;
    }
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

            if (CEO.CurrentClique != null && Target.CurrentClique != null && CEO.CurrentClique == Target.CurrentClique)
                value += 2;

            value += (int)(CEO.Charm * 0.2f);
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

            if (CEO.CurrentClique != null && Target.CurrentClique != null && CEO.CurrentClique == Target.CurrentClique)
                value += 2;

            value += (int)(CEO.Gossip * 0.2f);
        }
        //威胁
        else if (GC.CEOSkillNum == 8)
        {
            if (Target.CurrentOffice != null && (Target.CurrentOffice.building.Type == BuildingType.CEO办公室 || Target.CurrentOffice.building.Type == BuildingType.高管办公室))
                value += 1;
            if (CEO.CurrentClique != null && Target.CurrentClique != null && CEO.CurrentClique == Target.CurrentClique)
                value += 1;

            value -= (int)(Target.Manage * 0.2f);
            value += (int)(CEO.Convince * 0.1f);
        }
        //朋友
        else if (GC.CEOSkillNum == 9)
        {
            value += (int)(CEO.Charm * 0.2f);
            value += (int)(CEO.Convince * 0.2f);
            if ((CEO.Character[0] >= 50 && Target.Character[0] >= 50) || (CEO.Character[0] <= -50 && Target.Character[0] <= -50))
                value += 1;
            else if ((CEO.Character[0] >= 50 && Target.Character[0] <= -50) || (CEO.Character[0] <= -50 && Target.Character[0] >= 50))
                value -= 2;

            if ((CEO.Character[1] >= 50 && Target.Character[1] >= 50) || (CEO.Character[1] <= -50 && Target.Character[1] <= -50))
                value += 1;
            else if ((CEO.Character[1] >= 50 && Target.Character[1] <= -50) || (CEO.Character[1] <= -50 && Target.Character[1] >= 50))
                value -= 2;
        }
        //挚友
        else if (GC.CEOSkillNum == 10)
        {
            value += (int)(CEO.Charm * 0.1f);
            value += (int)(CEO.Convince * 0.1f);
            if ((CEO.Character[0] >= 50 && Target.Character[0] >= 50) || (CEO.Character[0] <= -50 && Target.Character[0] <= -50))
                value += 1;
            else if ((CEO.Character[0] >= 50 && Target.Character[0] <= -50) || (CEO.Character[0] <= -50 && Target.Character[0] >= 50))
                value -= 2;

            if ((CEO.Character[1] >= 50 && Target.Character[1] >= 50) || (CEO.Character[1] <= -50 && Target.Character[1] <= -50))
                value += 1;
            else if ((CEO.Character[1] >= 50 && Target.Character[1] <= -50) || (CEO.Character[1] <= -50 && Target.Character[1] >= 50))
                value -= 2;
        }
        //陌路
        else if (GC.CEOSkillNum == 11)
        {
            if ((CEO.Character[0] >= 50 && Target.Character[0] >= 50) || (CEO.Character[0] <= -50 && Target.Character[0] <= -50))
                value -= 1;
            else if ((CEO.Character[0] >= 50 && Target.Character[0] <= -50) || (CEO.Character[0] <= -50 && Target.Character[0] >= 50))
                value += 2;

            if ((CEO.Character[1] >= 50 && Target.Character[1] >= 50) || (CEO.Character[1] <= -50 && Target.Character[1] <= -50))
                value -= 1;
            else if ((CEO.Character[1] >= 50 && Target.Character[1] <= -50) || (CEO.Character[1] <= -50 && Target.Character[1] >= 50))
                value += 2;
        }
        //仇人
        else if (GC.CEOSkillNum == 12)
        {
            if ((CEO.Character[0] >= 50 && Target.Character[0] >= 50) || (CEO.Character[0] <= -50 && Target.Character[0] <= -50))
                value -= 1;
            else if ((CEO.Character[0] >= 50 && Target.Character[0] <= -50) || (CEO.Character[0] <= -50 && Target.Character[0] >= 50))
                value += 2;

            if ((CEO.Character[1] >= 50 && Target.Character[1] >= 50) || (CEO.Character[1] <= -50 && Target.Character[1] <= -50))
                value -= 1;
            else if ((CEO.Character[1] >= 50 && Target.Character[1] <= -50) || (CEO.Character[1] <= -50 && Target.Character[1] >= 50))
                value += 2;
        }
        //创建派系
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

            if ((CEO.Character[0] >= 50 && Target.Character[0] >= 50) || (CEO.Character[0] <= -50 && Target.Character[0] <= -50))
                value += 1;
            else if ((CEO.Character[0] >= 50 && Target.Character[0] <= -50) || (CEO.Character[0] <= -50 && Target.Character[0] >= 50))
                value -= 2;

            if ((CEO.Character[1] >= 50 && Target.Character[1] >= 50) || (CEO.Character[1] <= -50 && Target.Character[1] <= -50))
                value += 1;
            else if ((CEO.Character[1] >= 50 && Target.Character[1] <= -50) || (CEO.Character[1] <= -50 && Target.Character[1] >= 50))
                value -= 2;

            value += (int)(CEO.Charm * 0.2f);
            value += (int)(CEO.Convince * 0.2f);
        }
        //加入派系
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

            if ((CEO.Character[0] >= 50 && Target.Character[0] >= 50) || (CEO.Character[0] <= -50 && Target.Character[0] <= -50))
                value += 1;
            else if ((CEO.Character[0] >= 50 && Target.Character[0] <= -50) || (CEO.Character[0] <= -50 && Target.Character[0] >= 50))
                value -= 2;

            if ((CEO.Character[1] >= 50 && Target.Character[1] >= 50) || (CEO.Character[1] <= -50 && Target.Character[1] <= -50))
                value += 1;
            else if ((CEO.Character[1] >= 50 && Target.Character[1] <= -50) || (CEO.Character[1] <= -50 && Target.Character[1] >= 50))
                value -= 2;

            value += (int)(CEO.Charm * 0.2f);
            value += (int)(CEO.Convince * 0.2f);
        }
        //说服
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
        return value;
    }
}
