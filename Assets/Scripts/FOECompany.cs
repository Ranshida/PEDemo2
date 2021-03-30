using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FOECompany : MonoBehaviour
{
    FOEControl FC;

    public int ActionPoint = 4;//执行力
    public int Node;//节点数
    public int Line, Triangle, Square;//线段、三角形和正方形
    public int Shield;//护盾
    public int Market;//控制市场数

    //攻击对手的节点和图形
    public void Attack(int type, FOECompany target)
    {
        if (target.Shield > 0)
        {
            target.Shield -= 1;
            return;
        }
        else
        {
            //节点
            if (type == 1)
            {
                if (target.Node > 0)
                    target.Node -= 1;
            }
            //线段
            else if (type == 2)
            {
                if (target.Line > 0)
                {
                    target.Line -= 1;
                    target.Node += 1;
                }
            }
            //三角形
            else if (type == 3)
            {
                if (target.Triangle > 0)
                {
                    target.Triangle -= 1;
                    target.Node += 2;
                }
            }
            //正方形
            else if (type == 4)
            {
                if (target.Square > 0)
                {
                    target.Square -= 1;
                    target.Node += 3;
                }
            }

        }
    }

    //获取市场
    public void ConquerMarket(int type, FOECompany target)
    {
        int Limit, Value = 0;
        if (target != null)
            Limit = target.Market;
        else
            Limit = FC.NeutralMarket;
        //线段
        if (type == 1)
        {
            Line -= 1;
            ActionPoint -= 1;
            Value = 2;
        }
        //三角形
        else if (type == 2)
        {
            Triangle -= 1;
            ActionPoint -= 2;
            Value = 5;
        }
        else if (type == 3)
        {
            Square -= 1;
            ActionPoint -= 3;
            Value = 8;
        }

        if (Value > Limit)
            Value = Limit;

        if (target != null)
            target.Market -= Value;
        else
            FC.NeutralMarket -= Value;

        Market += Value;
    }
}
