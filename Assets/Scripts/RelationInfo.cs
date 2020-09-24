using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RelationInfo : MonoBehaviour
{
    public Text Text_Name, Text_Friend, Text_Master, Text_Love, Text_RPoint;
}

public class Relation
{
    public int FriendValue = 0; //-2仇人 -1陌路 0普通 1朋友 2挚友 此处是指目标相对自身的关系(如目标是我的师傅)(此处可能造成目标方向问题)
    public int MasterValue = 0; //0无 1徒弟 2师傅
    public int LoveValue = 0;   //0无 1倾慕 2追求 3情侣 4伴侣
    public int RPoint;
    public RelationInfo Info;
    public Employee Target, Self;

    public Relation(Employee target, Employee self)
    {
        Target = target;
        Self = self;
        RPoint = Random.Range(30, 40);
        Info = MonoBehaviour.Instantiate(Target.InfoDetail.GC.RelationInfoPrefab, Self.InfoDetail.RelationContent);
        UpdateUI();
    }

    public void UpdateUI()
    {
        Info.Text_Name.text = Target.Name;
        Info.Text_RPoint.text = RPoint.ToString();
        if (FriendValue == -2)
            Info.Text_Friend.text = "仇人";
        else if (FriendValue == -1)
            Info.Text_Friend.text = "普通";
        else if (FriendValue == 0)
            Info.Text_Friend.text = "普通";
        else if (FriendValue == 1)
            Info.Text_Friend.text = "朋友";
        else if (FriendValue == 2)
            Info.Text_Friend.text = "挚友";

        if (MasterValue == -1)
            Info.Text_Master.text = "";
        else if (MasterValue == 0)
            Info.Text_Master.text = "徒弟";
        else if (MasterValue == 1)
            Info.Text_Master.text = "师傅";

        if (LoveValue == 0)
            Info.Text_Love.text = "";
        else if(LoveValue == 1)
            Info.Text_Love.text = "倾慕";
        else if (LoveValue == 2)
            Info.Text_Love.text = "追求";
        else if (LoveValue == 3)
            Info.Text_Love.text = "情侣";
        else if (LoveValue == 4)
            Info.Text_Love.text = "伴侣";
    }

    //检查关系(已删除)
    public void RelationCheck()
    {
        //int type = Random.Range(1, 4);
        //float Posb = Random.Range(0.0f, 1.0f);
        //Relation r = Target.FindRelation(Self);
        //if(type == 1 && Self.Lover != Target)
        //{
        //    if (Self.Lover != null)
        //    {
        //        if (RPoint >= 80 && Posb < 0.3f)
        //            RemoveLover();
        //    }
        //    else if (Target.Lover != null)
        //    {
        //        if (r.RPoint >= 80 && Posb < 0.3f)
        //            r.RemoveLover();
        //    }
        //    else if (RPoint >= 80)
        //    {
        //        if (r.RPoint < 80 && Posb < 0.3f)
        //            AddLover();
        //        else if (r.RPoint >= 80 && Posb < 0.6f)
        //            AddLover();  
        //    }
        //}
        //else if(type == 2)
        //{
        //    if (Self.Master != null)
        //    {
        //        if (Posb < 0.3f)
        //            RemoveMaster();
        //    }
        //    else if (Target.Students.Count >= 5)
        //    {
        //        if (Posb < 0.3f)
        //            Target.Students[Random.Range(0, 5)].FindRelation(Target).RemoveMaster();
        //    }
        //    else
        //    {
        //        if (Posb < 0.3f)
        //            AddMaster();
        //    }
            
        //}
        //else if(type == 3)
        //{
        //    int value = 0;
        //    if(FriendValue == 1)
        //    {
        //        if (RPoint < 30 && r.RPoint >= 30 && Posb < 0.3f)
        //            value = 0;
        //        else if (RPoint < 30 && r.RPoint < 30 && Posb < 0.6f)
        //            value = 0;
        //        else
        //            value = 1;
        //    }
        //    else if(FriendValue == -1)
        //    {
        //        if (RPoint > 60 && r.RPoint <= 60 && Posb < 0.3f)
        //            value = 0;
        //        else if (RPoint > 60 && r.RPoint > 60 && Posb < 0.6f)
        //            value = 0;
        //        else
        //            value = -1;
        //    }
        //    else if(FriendValue == 0)
        //    {
        //        if(RPoint > 60)
        //        {
        //            if (r.RPoint > 60 && Posb < 0.6f)
        //                value = 1;
        //            else if (r.RPoint <= 60 && Posb < 0.3f)
        //                value = 1;
        //        }
        //        else if(RPoint < 30)
        //        {
        //            if (r.RPoint < 30 && Posb < 0.6f)
        //                value = -1;
        //            else if (r.RPoint >= 30 && Posb < 0.3f)
        //                value = -1;
        //        }
        //    }

        //    FriendValue = value;
        //    UpdateUI();

        //    r.FriendValue = value;
        //    r.UpdateUI();
        //}
    }

    //除友情关系以外的几种关系增减函数
    public void AddLover()
    {
        Relation rt = Target.FindRelation(Self);

        LoveValue = 1;
        UpdateUI();
        Self.Lover = Target;

        rt.LoveValue = 1;
        rt.UpdateUI();
        Target.Lover = Self;
    }
    public void RemoveLover()
    {
        Relation rt = Self.Lover.FindRelation(Self);
        rt.LoveValue = 0;
        rt.UpdateUI();
        Self.Lover.Lover = null;

        LoveValue = 0;
        UpdateUI();
        Self.Lover = null;
    }
    public void AddMaster()
    {
        Target.FindRelation(Self).MasterValue = 1;
        Target.FindRelation(Self).UpdateUI();
        Target.Students.Add(Self);

        MasterValue = 2;
        UpdateUI();
        Self.Master = Target;
    }
    public void RemoveMaster()
    {
        Self.Master.Students.Remove(Self);
        Self.Master.FindRelation(Self).MasterValue = 0;
        Self.Master.FindRelation(Self).UpdateUI();

        MasterValue = 0;
        UpdateUI();
        Self.Master = null;
    }

}
