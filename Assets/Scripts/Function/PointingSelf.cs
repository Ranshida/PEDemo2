using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointingSelf : MonoBehaviour
{
    public Action StartPointing;
    public Action OnPointing;
    public Action EndPointing;

    bool m_Pointing = false;

    private void Update()
    {
        if (!m_Pointing)
        {
            if (UISvc.PointingSelf(transform))
            {
                m_Pointing = true;
                StartPointing?.Invoke();
            }
        }
        else
        {
            if (UISvc.PointingSelf(transform))
            {
                OnPointing?.Invoke();
            }
            else
            {
                m_Pointing = false;
                EndPointing?.Invoke();
            }
        }
    }

    public void ClearAll() 
    {
        StartPointing = null;
        OnPointing = null;
        EndPointing = null;
    }
}
