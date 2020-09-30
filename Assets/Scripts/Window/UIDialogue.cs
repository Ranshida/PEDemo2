using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 战斗场景对话UI
/// 更新世间2020.4.2
/// </summary>
public class UIDialogue : UIObject
{
    float m_Timer;
    float m_LifeTime;
    Text m_Text;

    public void Init(string conversition, float lifeTime)
    {
        m_Text = GetComponentInChildren<Text>();
        m_Text.text = conversition;
        m_LifeTime = lifeTime;
        m_Timer = Time.time;
    }

    protected override void UpdateSpecific()
    {
        if (m_Timer + m_LifeTime < Time.time)
        {
            Destroy(gameObject);
        }
    }
}

