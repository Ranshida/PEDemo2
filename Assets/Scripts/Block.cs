using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public int Type; //1浏览器 2杀毒 3物流 4搜索引擎

    public int[] User = new int[5];

    public Block(int type)
    {
        Type = type;
        int a = 1000000;
        if(type == 1)
        {
            User[0] = (int)(a * 0.3f);
            User[1] = (int)(a * 0.3f);
            User[2] = (int)(a * 0.15f);
            User[3] = (int)(a * 0.05f);
            User[4] = (int)(a * 0.2f);
        }
        else if(type == 2)
        {
            User[0] = (int)(a * 0.3f);
            User[1] = (int)(a * 0.1f);
            User[2] = (int)(a * 0.2f);
            User[3] = (int)(a * 0.1f);
            User[4] = (int)(a * 0.3f);
        }
        else if (type == 3)
        {
            User[0] = (int)(a * 0.1f);
            User[1] = (int)(a * 0.1f);
            User[2] = (int)(a * 0.3f);
            User[3] = (int)(a * 0.1f);
            User[4] = (int)(a * 0.4f);
        }
        else if (type == 4)
        {
            User[0] = (int)(a * 0.3f);
            User[1] = (int)(a * 0.3f);
            User[2] = (int)(a * 0.15f);
            User[3] = (int)(a * 0.05f);
            User[4] = (int)(a * 0.2f);
        }
    }
}
