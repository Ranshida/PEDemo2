using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystal : MonoBehaviour
{
    public CrystalType type;

    public void SelectCrystal()
    {
        if (GameControl.Instance.CurrentItem != null)
        {
            GameControl.Instance.CurrentItem.TargetCrystal = this;
            GameControl.Instance.CurrentItem.UseItem();
        }
    }
}
