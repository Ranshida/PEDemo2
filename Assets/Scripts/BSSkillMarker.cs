using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BSSkillMarker : MonoBehaviour
{
    public GameObject[] images = new GameObject[6];
    public Text Text_Count;

    public void SetInfo(int type, int num)
    {
        for(int i = 0; i < 6; i++)
        {
            if (i == type)
                images[i].SetActive(true);
            else
                images[i].SetActive(false);
        }
        Text_Count.text = num.ToString();
    }
}
