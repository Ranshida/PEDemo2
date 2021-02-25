using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTree : MonoBehaviour
{
    //此技能系统为旧版头脑风暴相关
    //目前相关玩法均已删除
    //此脚本仅留作参考


    #region 旧技能树

    //public GameObject SubSkillPanelA, SubSkillPanelB;
    //public Text Text_TreeName;
    //public EmpInfo info;

    //public Text[] Text_Names = new Text[5], Text_DCost = new Text[5], Text_SCost = new Text[5], Text_Requires = new Text[5], Text_Describes = new Text[5];

    //public int SkillLevel = 0, SkillType = 0;

    //bool ExtraSkillASet = false, ExtraSkillBSet = false;//两个侧边技能

    //public void InitSkill()
    //{
    //    //InitSkill被调用两次，刷新招聘一次，实际招聘一次，目前技能不会转移
    //    SkillType = info.emp.StarType;
    //    SetDefaultInfo(SkillType);
    //    SkillCheck();
    //}
    //public void SkillCheck()
    //{
    //    //1观察
    //    if (SkillType == 1)
    //    {
    //        if (SkillLevel == 0 && info.emp.Observation >= 5)
    //        {
    //            info.AddSkill(new Skill40());
    //            SkillLevel = 1;
    //            SetNodeInfo(new Skill59(), 1);
    //            SetNodeInfo(new Skill55(), 3);
    //        }
    //        if (SkillLevel == 1 && info.emp.Observation >= 10)
    //        {
    //            info.ReplaceSkill(new Skill40(), new Skill59());
    //            SkillLevel = 2;
    //            SetNodeInfo(new Skill35(), 2);
    //            SetNodeInfo(new Skill49(), 4);
    //        }
    //        if (SkillLevel == 2 && info.emp.Observation >= 15)
    //        {
    //            info.AddSkill(new Skill35());
    //            SkillLevel = 3;
    //        }
    //        if (SkillLevel > 0 && ExtraSkillASet == false && info.emp.Gossip >= 7)
    //        {
    //            ExtraSkillASet = true;
    //            info.AddSkill(new Skill55());
    //        }
    //        if (SkillLevel > 1 && ExtraSkillBSet == false && info.emp.Convince >= 10)
    //        {
    //            ExtraSkillBSet = true;
    //            info.AddSkill(new Skill49());
    //        }
    //    }
    //    //2程序
    //    else if (SkillType == 2)
    //    {
    //        if (SkillLevel == 0 && info.emp.Skill1 >= 5)
    //        {
    //            info.AddSkill(new Skill32());
    //            SkillLevel = 1;
    //            SetNodeInfo(new Skill60(), 1);
    //            SetNodeInfo(new Skill53(), 3);
    //        }
    //        if (SkillLevel == 1 && info.emp.Skill1 >= 10)
    //        {
    //            info.ReplaceSkill(new Skill32(), new Skill60());
    //            SkillLevel = 2;
    //            SetNodeInfo(new Skill56(), 2);
    //            SetNodeInfo(new Skill58(), 4);
    //        }
    //        if (SkillLevel == 2 && info.emp.Skill1 >= 15)
    //        {
    //            info.AddSkill(new Skill56());
    //            SkillLevel = 3;
    //        }
    //        if (SkillLevel > 0 && ExtraSkillASet == false && info.emp.HR >= 10)
    //        {
    //            ExtraSkillASet = true;
    //            info.AddSkill(new Skill53());
    //        }
    //        if (SkillLevel > 1 && ExtraSkillBSet == false && info.emp.Forecast >= 10)
    //        {
    //            ExtraSkillBSet = true;
    //            info.AddSkill(new Skill58());
    //        }
    //    }
    //    //3产品
    //    else if (SkillType == 3)
    //    {
    //        if (SkillLevel == 0 && info.emp.Skill3 >= 5)
    //        {
    //            info.AddSkill(new Skill32());
    //            SkillLevel = 1;
    //            SetNodeInfo(new Skill60(), 1);
    //            SetNodeInfo(new Skill55(), 3);
    //        }
    //        if (SkillLevel == 1 && info.emp.Skill3 >= 10)
    //        {
    //            info.ReplaceSkill(new Skill32(), new Skill60());
    //            SkillLevel = 2;
    //            SetNodeInfo(new Skill45(), 2);
    //            SetNodeInfo(new Skill50(), 4);
    //        }
    //        if (SkillLevel == 2 && info.emp.Skill3 >= 15)
    //        {
    //            info.AddSkill(new Skill45());
    //            SkillLevel = 3;
    //        }
    //        if (SkillLevel > 0 && ExtraSkillASet == false && info.emp.Gossip >= 7)
    //        {
    //            ExtraSkillASet = true;
    //            info.AddSkill(new Skill55());
    //        }
    //        if (SkillLevel > 1 && ExtraSkillBSet == false && info.emp.Decision >= 10)
    //        {
    //            ExtraSkillBSet = true;
    //            info.AddSkill(new Skill50());
    //        }
    //    }
    //    //4市场
    //    else if (SkillType == 4)
    //    {
    //        if (SkillLevel == 0 && info.emp.Skill2 >= 5)
    //        {
    //            info.AddSkill(new Skill32());
    //            SkillLevel = 1;
    //            SetNodeInfo(new Skill60(), 1);
    //            SetNodeInfo(new Skill54(), 3);
    //        }
    //        if (SkillLevel == 1 && info.emp.Skill2 >= 10)
    //        {
    //            info.ReplaceSkill(new Skill32(), new Skill60());
    //            SkillLevel = 2;
    //            SetNodeInfo(new Skill48(), 2);
    //            SetNodeInfo(new Skill49(), 4);
    //        }
    //        if (SkillLevel == 2 && info.emp.Skill2 >= 15)
    //        {
    //            info.AddSkill(new Skill48());
    //            SkillLevel = 3;
    //        }
    //        if (SkillLevel > 0 && ExtraSkillASet == false && info.emp.Charm >= 7)
    //        {
    //            ExtraSkillASet = true;
    //            info.AddSkill(new Skill54());
    //        }
    //        if (SkillLevel > 1 && ExtraSkillBSet == false && info.emp.Convince >= 10)
    //        {
    //            ExtraSkillBSet = true;
    //            info.AddSkill(new Skill49());
    //        }
    //    }
    //    //5财务
    //    else if (SkillType == 5)
    //    {
    //        if (SkillLevel == 0 && info.emp.Finance >= 5)
    //        {
    //            info.AddSkill(new Skill34());
    //            SkillLevel = 1;
    //            SetNodeInfo(new Skill64(), 1);
    //            SetNodeInfo(new Skill54(), 3);
    //        }
    //        if (SkillLevel == 1 && info.emp.Finance >= 10)
    //        {
    //            info.ReplaceSkill(new Skill34(), new Skill64());
    //            SkillLevel = 2;
    //            SetNodeInfo(new Skill46(), 2);
    //            SetNodeInfo(new Skill49(), 4);
    //        }
    //        if (SkillLevel == 2 && info.emp.Finance >= 15)
    //        {
    //            info.AddSkill(new Skill46());
    //            SkillLevel = 3;
    //        }
    //        if (SkillLevel > 0 && ExtraSkillASet == false && info.emp.Charm >= 7)
    //        {
    //            ExtraSkillASet = true;
    //            info.AddSkill(new Skill54());
    //        }
    //        if (SkillLevel > 1 && ExtraSkillBSet == false && info.emp.Convince >= 10)
    //        {
    //            ExtraSkillBSet = true;
    //            info.AddSkill(new Skill49());
    //        }
    //    }
    //    //6人力
    //    else if (SkillType == 6)
    //    {
    //        if (SkillLevel == 0 && info.emp.HR >= 5)
    //        {
    //            info.AddSkill(new Skill41());
    //            SkillLevel = 1;
    //            SetNodeInfo(new Skill63(), 1);
    //            SetNodeInfo(new Skill54(), 3);
    //        }
    //        if (SkillLevel == 1 && info.emp.HR >= 10)
    //        {
    //            info.ReplaceSkill(new Skill41(), new Skill63());
    //            SkillLevel = 2;
    //            SetNodeInfo(new Skill38(), 2);
    //            SetNodeInfo(new Skill50(), 4);
    //        }
    //        if (SkillLevel == 2 && info.emp.HR >= 15)
    //        {
    //            info.AddSkill(new Skill38());
    //            SkillLevel = 3;
    //        }
    //        if (SkillLevel > 0 && ExtraSkillASet == false && info.emp.Charm >= 7)
    //        {
    //            ExtraSkillASet = true;
    //            info.AddSkill(new Skill54());
    //        }
    //        if (SkillLevel > 1 && ExtraSkillBSet == false && info.emp.Decision >= 10)
    //        {
    //            ExtraSkillBSet = true;
    //            info.AddSkill(new Skill50());
    //        }
    //    }
    //    //7强壮
    //    else if (SkillType == 7)
    //    {
    //        if (SkillLevel == 0 && info.emp.Strength >= 5)
    //        {
    //            info.AddSkill(new Skill42());
    //            SkillLevel = 1;
    //            SetNodeInfo(new Skill62(), 1);
    //            SetNodeInfo(new Skill55(), 3);
    //        }
    //        if (SkillLevel == 1 && info.emp.Strength >= 10)
    //        {
    //            info.ReplaceSkill(new Skill42(), new Skill62());
    //            SkillLevel = 2;
    //            SetNodeInfo(new Skill43(), 2);
    //            SetNodeInfo(new Skill49(), 4);
    //        }
    //        if (SkillLevel == 2 && info.emp.Strength >= 15)
    //        {
    //            info.AddSkill(new Skill43());
    //            SkillLevel = 3;
    //        }
    //        if (SkillLevel > 0 && ExtraSkillASet == false && info.emp.Gossip >= 7)
    //        {
    //            ExtraSkillASet = true;
    //            info.AddSkill(new Skill55());
    //        }
    //        if (SkillLevel > 1 && ExtraSkillBSet == false && info.emp.Convince >= 10)
    //        {
    //            ExtraSkillBSet = true;
    //            info.AddSkill(new Skill49());
    //        }
    //    }
    //    //8坚韧
    //    else if (SkillType == 8)
    //    {
    //        if (SkillLevel == 0 && info.emp.Tenacity >= 5)
    //        {
    //            info.AddSkill(new Skill44());
    //            SkillLevel = 1;
    //            SetNodeInfo(new Skill61(), 1);
    //            SetNodeInfo(new Skill54(), 3);
    //        }
    //        if (SkillLevel == 1 && info.emp.Tenacity >= 10)
    //        {
    //            info.ReplaceSkill(new Skill44(), new Skill61());
    //            SkillLevel = 2;
    //            SetNodeInfo(new Skill47(), 2);
    //            SetNodeInfo(new Skill50(), 4);
    //        }
    //        if (SkillLevel == 2 && info.emp.Tenacity >= 15)
    //        {
    //            info.AddSkill(new Skill47());
    //            SkillLevel = 3;
    //        }
    //        if (SkillLevel > 0 && ExtraSkillASet == false && info.emp.Charm >= 7)
    //        {
    //            ExtraSkillASet = true;
    //            info.AddSkill(new Skill54());
    //        }
    //        if (SkillLevel > 1 && ExtraSkillBSet == false && info.emp.Decision >= 10)
    //        {
    //            ExtraSkillBSet = true;
    //            info.AddSkill(new Skill50());
    //        }
    //    }
    //    //9行业
    //    else if (SkillType == 9)
    //    {
    //        if (SkillLevel == 0 && info.emp.Forecast >= 5)
    //        {
    //            info.AddSkill(new Skill33());
    //            SkillLevel = 1;
    //            SetNodeInfo(new Skill65(), 1);
    //            SetNodeInfo(new Skill55(), 3);
    //        }
    //        if (SkillLevel == 1 && info.emp.Forecast >= 10)
    //        {
    //            info.ReplaceSkill(new Skill33(), new Skill65());
    //            SkillLevel = 2;
    //            SetNodeInfo(new Skill37(), 2);
    //            SetNodeInfo(new Skill50(), 4);
    //        }
    //        if (SkillLevel == 2 && info.emp.Forecast >= 15)
    //        {
    //            info.AddSkill(new Skill37());
    //            SkillLevel = 3;
    //        }
    //        if (SkillLevel > 0 && ExtraSkillASet == false && info.emp.Gossip >= 7)
    //        {
    //            ExtraSkillASet = true;
    //            info.AddSkill(new Skill55());
    //        }
    //        if (SkillLevel > 1 && ExtraSkillBSet == false && info.emp.Decision >= 10)
    //        {
    //            ExtraSkillBSet = true;
    //            info.AddSkill(new Skill50());
    //        }
    //    }
    //    //10谋略
    //    else if (SkillType == 10)
    //    {
    //        if (SkillLevel == 0 && info.emp.Strategy >= 5)
    //        {
    //            info.AddSkill(new Skill36());
    //            SkillLevel = 1;
    //            SetNodeInfo(new Skill66(), 1);
    //            SetNodeInfo(new Skill55(), 3);
    //        }
    //        if (SkillLevel == 1 && info.emp.Strategy >= 10)
    //        {
    //            info.ReplaceSkill(new Skill36(), new Skill66());
    //            SkillLevel = 2;
    //            SetNodeInfo(new Skill37(), 2);
    //            SetNodeInfo(new Skill49(), 4);
    //        }
    //        if (SkillLevel == 2 && info.emp.Strategy >= 15)
    //        {
    //            info.AddSkill(new Skill37());
    //            SkillLevel = 3;
    //        }
    //        if (SkillLevel > 0 && ExtraSkillASet == false && info.emp.Gossip >= 7)
    //        {
    //            ExtraSkillASet = true;
    //            info.AddSkill(new Skill55());
    //        }
    //        if (SkillLevel > 1 && ExtraSkillBSet == false && info.emp.Convince >= 10)
    //        {
    //            ExtraSkillBSet = true;
    //            info.AddSkill(new Skill49());
    //        }
    //    }
    //    //11-13管理
    //    else if (SkillType == 11)
    //    {
    //        if (SkillLevel == 0 && info.emp.Manage >= 10 && info.emp.HR >= 5)
    //        {
    //            info.AddSkill(new Skill53());
    //            SkillLevel = 1;
    //            SetNodeInfo(new Skill67(), 1);
    //        }
    //        if (SkillLevel == 1 && info.emp.Manage >= 10 && info.emp.HR >= 10)
    //        {
    //            info.ReplaceSkill(new Skill53(), new Skill67());
    //            SkillLevel = 2;
    //            SetNodeInfo(new Skill58(), 2);
    //        }
    //        if (SkillLevel == 2 && info.emp.Manage >= 15 && info.emp.Forecast >= 10)
    //        {
    //            info.AddSkill(new Skill58());
    //            SkillLevel = 3;
    //        }
    //    }
    //    else if (SkillType == 12)
    //    {
    //        if (SkillLevel == 0 && info.emp.Manage >= 10 && info.emp.Decision >= 5)
    //        {
    //            info.AddSkill(new Skill57());
    //            SkillLevel = 1;
    //            SetNodeInfo(new Skill68(), 1);
    //        }
    //        if (SkillLevel == 1 && info.emp.Manage >= 10 && info.emp.Decision >= 10)
    //        {
    //            info.ReplaceSkill(new Skill57(), new Skill68());
    //            SkillLevel = 2;
    //            SetNodeInfo(new Skill51(), 2);
    //        }
    //        if (SkillLevel == 2 && info.emp.Manage >= 15 && info.emp.Finance >= 10)
    //        {
    //            info.AddSkill(new Skill51());
    //            SkillLevel = 3;
    //        }
    //    }
    //    else if (SkillType == 13)
    //    {
    //        if (SkillLevel == 0 && info.emp.Manage >= 10 && info.emp.Decision >= 5)
    //        {
    //            info.AddSkill(new Skill57());
    //            SkillLevel = 1;
    //            SetNodeInfo(new Skill68(), 1);
    //        }
    //        if (SkillLevel == 1 && info.emp.Manage >= 10 && info.emp.Decision >= 10)
    //        {
    //            info.ReplaceSkill(new Skill57(), new Skill68());
    //            SkillLevel = 2;
    //            SetNodeInfo(new Skill52(), 2);
    //        }
    //        if (SkillLevel == 2 && info.emp.Manage >= 15 && info.emp.Strategy >= 10)
    //        {
    //            info.AddSkill(new Skill52());
    //            SkillLevel = 3;
    //        }
    //    }
    //}

    //public void ChangeSkillTree(int type = 0)
    //{
    //    info.ClearSkillPreset();
    //    foreach(SkillInfo skill in info.SkillsInfo)
    //    {
    //        Destroy(skill.gameObject);
    //    }
    //    info.SkillsInfo.Clear();
    //    if (type == 0)
    //        type = Random.Range(1, 14);
    //    SetDefaultInfo(type);
    //    SkillCheck();
    //}

    //void SetDefaultInfo(int num)
    //{
    //    //招聘技能树需要反复刷新所以先设置为都可见
    //    SubSkillPanelA.SetActive(true);
    //    SubSkillPanelB.SetActive(true);
    //    //重置所有节点内容
    //    for (int i = 0; i < 5; i++)
    //    {
    //        Text_Names[i].text = "";
    //        Text_DCost[i].text = "";
    //        Text_SCost[i].text = "";
    //        Text_Describes[i].text = "???";
    //    }

    //    if (num == 1)
    //    {
    //        Text_Requires[0].text = "观察>=5";
    //        Text_Requires[1].text = "观察>=10";
    //        Text_Requires[2].text = "观察>=15";
    //        Text_Requires[3].text = "八卦>=7";
    //        Text_Requires[4].text = "说服>=10";
    //        SetNodeInfo(new Skill40(), 0);
    //        Text_TreeName.text = "观察";
    //    }
    //    else if (num == 2)
    //    {
    //        Text_Requires[0].text = "技术>=5";
    //        Text_Requires[1].text = "技术>=10";
    //        Text_Requires[2].text = "技术>=15";
    //        Text_Requires[3].text = "人力>=10";
    //        Text_Requires[4].text = "行业>=10";
    //        SetNodeInfo(new Skill32(), 0);
    //        Text_TreeName.text = "程序";
    //    }
    //    else if (num == 3)
    //    {
    //        Text_Requires[0].text = "产品>=5";
    //        Text_Requires[1].text = "产品>=10";
    //        Text_Requires[2].text = "产品>=15";
    //        Text_Requires[3].text = "八卦>=7";
    //        Text_Requires[4].text = "决策>=10";
    //        SetNodeInfo(new Skill32(), 0);
    //        Text_TreeName.text = "产品";
    //    }
    //    else if (num == 4)
    //    {
    //        Text_Requires[0].text = "市场>=5";
    //        Text_Requires[1].text = "市场>=10";
    //        Text_Requires[2].text = "市场>=15";
    //        Text_Requires[3].text = "魅力>=7";
    //        Text_Requires[4].text = "说服>=10";
    //        SetNodeInfo(new Skill32(), 0);
    //        Text_TreeName.text = "市场";
    //    }
    //    else if (num == 5)
    //    {
    //        Text_Requires[0].text = "财务>=5";
    //        Text_Requires[1].text = "财务>=10";
    //        Text_Requires[2].text = "财务>=15";
    //        Text_Requires[3].text = "魅力>=7";
    //        Text_Requires[4].text = "说服>=10";
    //        SetNodeInfo(new Skill34(), 0);
    //        Text_TreeName.text = "财务";
    //    }
    //    else if (num == 6)
    //    {
    //        Text_Requires[0].text = "人力>=5";
    //        Text_Requires[1].text = "人力>=10";
    //        Text_Requires[2].text = "人力>=15";
    //        Text_Requires[3].text = "魅力>=7";
    //        Text_Requires[4].text = "决策>=10";
    //        SetNodeInfo(new Skill41(), 0);
    //        Text_TreeName.text = "人力";
    //    }
    //    else if (num == 7)
    //    {
    //        Text_Requires[0].text = "强壮>=5";
    //        Text_Requires[1].text = "强壮>=10";
    //        Text_Requires[2].text = "强壮>=15";
    //        Text_Requires[3].text = "八卦>=7";
    //        Text_Requires[4].text = "说服>=10";
    //        SetNodeInfo(new Skill42(), 0);
    //        Text_TreeName.text = "强壮";
    //    }
    //    else if (num == 8)
    //    {
    //        Text_Requires[0].text = "坚韧>=5";
    //        Text_Requires[1].text = "坚韧>=10";
    //        Text_Requires[2].text = "坚韧>=15";
    //        Text_Requires[3].text = "魅力>=7";
    //        Text_Requires[4].text = "决策>=10";
    //        SetNodeInfo(new Skill44(), 0);
    //        Text_TreeName.text = "坚韧";
    //    }
    //    else if (num == 9)
    //    {
    //        Text_Requires[0].text = "行业>=5";
    //        Text_Requires[1].text = "行业>=10";
    //        Text_Requires[2].text = "行业>=15";
    //        Text_Requires[3].text = "八卦>=7";
    //        Text_Requires[4].text = "决策>=10";
    //        SetNodeInfo(new Skill33(), 0);
    //        Text_TreeName.text = "行业";
    //    }
    //    else if (num == 10)
    //    {
    //        Text_Requires[0].text = "谋略>=5";
    //        Text_Requires[1].text = "谋略>=10";
    //        Text_Requires[2].text = "谋略>=15";
    //        Text_Requires[3].text = "八卦>=7";
    //        Text_Requires[4].text = "说服>=10";
    //        SetNodeInfo(new Skill36(), 0);
    //        Text_TreeName.text = "谋略";
    //    }
    //    else if (num == 11)
    //    {
    //        Text_Requires[0].text = "管理>=10人力>=5";
    //        Text_Requires[1].text = "管理>=10人力>=10";
    //        Text_Requires[2].text = "管理>=15行业>=10";
    //        SubSkillPanelA.SetActive(false);
    //        SubSkillPanelB.SetActive(false);
    //        SetNodeInfo(new Skill53(), 0); 
    //        Text_TreeName.text = "管理1";
    //    }
    //    else if (num == 12)
    //    {
    //        Text_Requires[0].text = "管理>=10决策>=5";
    //        Text_Requires[1].text = "管理>=10决策>=10";
    //        Text_Requires[2].text = "管理>=15财务>=10";
    //        SubSkillPanelA.SetActive(false);
    //        SubSkillPanelB.SetActive(false);
    //        SetNodeInfo(new Skill57(), 0);
    //        Text_TreeName.text = "管理2";
    //    }
    //    else if (num == 13)
    //    {
    //        Text_Requires[0].text = "管理>=10决策>=5";
    //        Text_Requires[1].text = "管理>=10决策>=10";
    //        Text_Requires[2].text = "管理>=15谋略>=10";
    //        SubSkillPanelA.SetActive(false);
    //        SubSkillPanelB.SetActive(false);
    //        SetNodeInfo(new Skill57(), 0);
    //        Text_TreeName.text = "管理3";
    //    }
    //}
    //void SetNodeInfo(Skill s, int num)
    //{
    //    Text_Names[num].text = s.Name;
    //    Text_DCost[num].text = "点数:" + s.DiceCost;
    //    Text_SCost[num].text = "体力:" + s.StaminaCost;
    //    Text_Describes[num].text = s.Description;
    //}
    #endregion
}
