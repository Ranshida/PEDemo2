using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DiceType
{
    Buff, Money, AssistBuff,Attack, Defense, Debuff, 
}

public class BSDiceControl : MonoBehaviour
{
    //类型编号见DiceType枚举
    public int CurrentType = 0; //当前骰子朝上面的类型
    public bool Selected = false;//是否已经被选中
    public bool CanSelect = true;//是否可选

    public BrainStormControl BSC;

    public int[] PosbDiceType = new int[6];  //存储骰子6个面的类型
    public GameObject[] DiceImages = new GameObject[6];//6个种类面的图片


    //根据形参设定各个面的类型
    public void SetSides(int[] sides)
    {
        for(int i = 0; i < 6; i ++)
        {
            PosbDiceType[i] = sides[i];
        }
    }

    public void SelectDice()
    {
        if ((BSC.SelectedDices.Count > 2 || CanSelect == false) && Selected == false)
            return;
        
        if(Selected == false)
        {
            Selected = true;
            transform.parent = BSC.SelectedDiceContent;
            BSC.SelectedDices.Add(this);
        }
        else
        {
            Selected = false;
            transform.parent = BSC.DiceContent;
            BSC.SelectedDices.Remove(this);
        }
        BSC.CheckSkillType();
    }

    public void SetOutline(bool show)
    {
        if (show == true)
        {
            GetComponent<Outline>().enabled = true;
            CanSelect = true;
        }
        else
        {
            GetComponent<Outline>().enabled = false;
            CanSelect = false;
        }
    }

    public void RandomSide()
    {
        int side = Random.Range(0, 6);

        //随机到空白面则直接删除
        if(PosbDiceType[side] == -1)
        {
            BSC.EmptyDiceNum += 1;
            BSC.CurrentDices.Remove(this);
            Destroy(this.gameObject);           
            return;
        }

        //显示对面的图片并赋值
        for (int i = 0; i < 6; i++)
        {
            DiceImages[i].SetActive(false);
        }
        if (PosbDiceType[side] >= 0)
            DiceImages[PosbDiceType[side]].SetActive(true);
        CurrentType = PosbDiceType[side];
    }

}
