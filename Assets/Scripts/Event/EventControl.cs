using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventControl : MonoBehaviour
{
    public GameObject ManageVotePanel;
    public GameControl GC;
    public Transform ManageVoteContent;
    public VoteCell VoteCellPrefab;
    public Text Text_MeetingName, Text_ManageVoteResult;


    List<VoteCell> VCells = new List<VoteCell>();
    private void Start()
    {
        if (GC == null)
            GC = GameControl.Instance;
    }

    //换高管投票检测
    public bool ManagerVoteCheck(Employee emp, bool Dismissal = false, bool Fire = false)
    {
        //CEO不检测
        if (emp.isCEO == true)
            return true;

        int VoteNum = 1;
        int AgreeNum = 1;
        ManageVotePanel.SetActive(true);
        for(int i = 0; i < VCells.Count; i++)
        {
            Destroy(VCells[i].gameObject);
        }
        VCells.Clear();

        if (Dismissal == true)
            Text_MeetingName.text = "解除" + emp.Name + "高管职务会议投票结果";
        else
            Text_MeetingName.text = "任命" + emp.Name + "高管职务会议投票结果";
        if (Fire == true)
            Text_MeetingName.text = "开除员工" + emp.Name + "会议投票结果";

        for (int i = 0; i < GC.CurrentOffices.Count; i++)
        {
            if ((GC.CurrentOffices[i].building.Type == BuildingType.CEO办公室 || GC.CurrentOffices[i].building.Type == BuildingType.高管办公室) && GC.CurrentOffices[i].CurrentManager != null)
            {
                Employee E = GC.CurrentOffices[i].CurrentManager, CEO = GC.CurrentEmployees[0];
                //CEO和自己不投票
                if (E.isCEO == true || E == emp)
                    continue;
                VoteCell V = Instantiate(VoteCellPrefab, ManageVoteContent);
                int VoteValue = 0;
                string Description = "和目标关系", Description2 = "和CEO关系";
                V.Text_Name.text = E.Name;
                VCells.Add(V);
                //和目标的关系
                Relation r = E.FindRelation(emp);
                //友情关系检测
                if (r.FriendValue == -2)
                {
                    if (Dismissal == false)
                    {
                        VoteValue -= 4;
                        Description += "\n仇敌-4";
                    }
                    else
                    {
                        VoteValue += 4;
                        Description += "\n仇敌+4";
                    }
                }
                else if (r.FriendValue == -1)
                {
                    if (Dismissal == false)
                    {
                        VoteValue -= 2;
                        Description += "\n陌路-2";
                    }
                    else
                    {
                        VoteValue += 2;
                        Description += "\n陌路+2";
                    }
                }
                else if (r.FriendValue == 1)
                {
                    if (Dismissal == false)
                    {
                        VoteValue += 1;
                        Description += "\n朋友+1";
                    }
                    else
                    {
                        VoteValue -= 1;
                        Description += "\n朋友-1";
                    }
                }
                else if (r.FriendValue == 2)
                {
                    if (Dismissal == false)
                    {
                        VoteValue += 3;
                        Description += "\n挚友+3";
                    }
                    else
                    {
                        VoteValue -= 3;
                        Description += "\n挚友-3";
                    }
                }
                //恋爱关系检测
                if (r.LoveValue == 1)
                {
                    if (Dismissal == false)
                    {
                        VoteValue += 1;
                        Description += "\n倾慕+1";
                    }
                    else
                    {
                        VoteValue -= 1;
                        Description += "\n倾慕-1";
                    }
                }
                else if (r.LoveValue == 2)
                {
                    if (Dismissal == false)
                    {
                        VoteValue -= 1;
                        Description += "\n追求者-1";
                    }
                    else
                    {
                        VoteValue += 1;
                        Description += "\n追求者+1";
                    }
                }
                else if (r.LoveValue == 3)
                {
                    if (Dismissal == false)
                    {
                        VoteValue += 2;
                        Description += "\n情侣+2";
                    }
                    else
                    {
                        VoteValue -= 2;
                        Description += "\n情侣-2";
                    }
                }
                else if (r.LoveValue == 4)
                {
                    if (Dismissal == false)
                    {
                        VoteValue += 3;
                        Description += "\n情侣+3";
                    }
                    else
                    {
                        VoteValue -= 3;
                        Description += "\n情侣-3";
                    }
                }
                //师徒关系检测 (可能方向有问题)
                if(r.MasterValue == 1)
                {
                    if (Dismissal == false)
                    {
                        VoteValue += 1;
                        Description += "\n门徒+1";
                    }
                    else
                    {
                        VoteValue -= 1;
                        Description += "\n门徒-1";
                    }
                }
                else if (r.MasterValue == 2)
                {
                    if (Dismissal == false)
                    {
                        VoteValue += 2;
                        Description += "\n师承+2";
                    }
                    else
                    {
                        VoteValue -= 2;
                        Description += "\n师承-2";
                    }
                }
                //派系检测
                if (E.CurrentClique != null && E.CurrentClique.Members.Contains(emp) == true)
                {
                    if (Dismissal == false)
                    {
                        VoteValue += 3;
                        Description += "\n同派系+3";
                    }
                    else
                    {
                        VoteValue -= 3;
                        Description += "\n同派系-3";
                    }
                }
                //文化检测
                if((E.Character[0] >= 50 && emp.Character[0] >= 50) ||(E.Character[0] <= -50 && emp.Character[0] <= -50))
                {
                    if (Dismissal == false)
                    {
                        VoteValue += 1;
                        Description += "\n文化一致+1";
                    }
                    else
                    {
                        VoteValue -= 1;
                        Description += "\n文化一致-1";
                    }
                }
                else if ((E.Character[0] >= 50 && emp.Character[0] <= -50) || (E.Character[0] <= -50 && emp.Character[0] >= 50))
                {
                    if (Dismissal == false)
                    {
                        VoteValue -= 2;
                        Description += "\n文化相反-2";
                    }
                    else
                    {
                        VoteValue += 2;
                        Description += "\n文化相反+2";
                    }
                }
                //信仰检测
                if ((E.Character[1] >= 50 && emp.Character[1] >= 50) || (E.Character[1] <= -50 && emp.Character[1] <= -50))
                {
                    if (Dismissal == false)
                    {
                        VoteValue += 1;
                        Description += "\n信仰一致+1";
                    }
                    else
                    {
                        VoteValue -= 1;
                        Description += "\n信仰一致-1";
                    }
                }
                else if ((E.Character[1] >= 50 && emp.Character[1] <= -50) || (E.Character[1] <= -50 && emp.Character[1] >= 50))
                {
                    if (Dismissal == false)
                    {
                        VoteValue -= 2;
                        Description += "\n信仰相反-2";
                    }
                    else
                    {
                        VoteValue += 2;
                        Description += "\n信仰相反+2";
                    }
                }

                //和CEO的关系
                r = E.FindRelation(CEO);
                //友情关系检测
                if (r.FriendValue == -2)
                {
                    VoteValue -= 4;
                    Description2 += "\n仇敌-4";
                }
                else if (r.FriendValue == -1)
                {
                    VoteValue -= 2;
                    Description2 += "\n陌路-2";
                }
                else if (r.FriendValue == 1)
                {
                    VoteValue += 1;
                    Description2 += "\n朋友+1";
                }
                else if (r.FriendValue == 2)
                {
                    VoteValue += 3;
                    Description2 += "\n挚友+3";
                }
                //恋爱关系检测
                if (r.LoveValue == 1)
                {
                    VoteValue += 1;
                    Description2 += "\n倾慕+1";
                }
                else if (r.LoveValue == 2)
                {
                    VoteValue -= 1;
                    Description2 += "\n追求者-1";
                }
                else if (r.LoveValue == 3)
                {
                    VoteValue += 2;
                    Description2 += "\n情侣+2";
                }
                else if (r.LoveValue == 4)
                {
                    VoteValue += 3;
                    Description2 += "\n情侣+3";
                }
                //师徒关系检测 (可能方向有问题)
                if (r.MasterValue == 1)
                {
                    VoteValue += 1;
                    Description2 += "\n门徒+1";
                }
                else if (r.MasterValue == 2)
                {
                    VoteValue += 2;
                    Description2 += "\n师承+2";
                }
                //派系检测
                if (E.CurrentClique != null && E.CurrentClique.Members.Contains(CEO) == true)
                {
                    VoteValue += 3;
                    Description2 += "\n同派系+3";
                }
                //文化检测
                if ((E.Character[0] >= 50 && CEO.Character[0] >= 50) || (E.Character[0] <= -50 && CEO.Character[0] <= -50))
                {
                    VoteValue += 1;
                    Description2 += "\n文化一致+1";
                }
                else if ((E.Character[0] >= 50 && CEO.Character[0] <= -50) || (E.Character[0] <= -50 && CEO.Character[0] >= 50))
                {
                    VoteValue -= 2;
                    Description2 += "\n文化相反-2";
                }
                //信仰检测
                if ((E.Character[1] >= 50 && CEO.Character[1] >= 50) || (E.Character[1] <= -50 && CEO.Character[1] <= -50))
                {
                    VoteValue += 1;
                    Description2 += "\n信仰一致+1";
                }
                else if ((E.Character[1] >= 50 && CEO.Character[1] <= -50) || (E.Character[1] <= -50 && CEO.Character[1] >= 50))
                {
                    VoteValue -= 2;
                    Description2 += "\n信仰相反-2";
                }

                //计算技能影响（分就职和离职两种）
                int ExtraValue = ExtraValue = (int)(emp.Manage * 0.1f); ;
                if (Dismissal == true)
                {                   
                    VoteValue += ExtraValue;
                    Description += "\n管理技能+" + ExtraValue;
                }
                else
                {
                    VoteValue -= ExtraValue;
                    Description += "\n管理技能-" + ExtraValue;
                }

                //确定结果
                VoteNum += 1;
                if (VoteValue >= 0)
                {
                    V.image.color = Color.green;
                    V.Text_Value.text = "+" + VoteValue;
                    AgreeNum += 1;
                }
                else
                {
                    V.image.color = Color.red;
                    V.Text_Value.text = "-" + Mathf.Abs(VoteValue);
                }
                V.Text_Vote.text = Description;
                V.Text_Vote2.text = Description2;
            }
        }

        if ((AgreeNum * 2) >= VoteNum)
        {
            Text_ManageVoteResult.text = "通过";
            return true;
        }
        else
        {
            Text_ManageVoteResult.text = "未通过";
            return false;
        }
    }
}
