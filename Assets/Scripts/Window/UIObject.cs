using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 需要特殊处理的UI物体基类
/// </summary>
public class UIObject : MonoBehaviour
{
    bool m_AnchorOnPosition;               //固定在3d世界里
    bool m_AnchorOnTrans;                  //绑定3d世界的某物体上

    Vector3 m_AnchorPos = Vector3.zero;            //固定在3d世界里的位置
    Transform m_AnchorTrans = null;                //绑定的物体
    Vector3 m_TransOffset_UI = Vector3.zero;       //绑定的物体在世界中位置偏移
    Vector3 m_TransOffset_World = Vector3.zero;    //绑定的物体在UI中位置偏移

    private void Update()
    {
        if (m_AnchorOnPosition)
        {
            transform.position = Function.World2ScreenPoint(m_AnchorPos);
        }
        if (m_AnchorOnTrans)
        {
            if (!m_AnchorTrans)
            {
                Destroy(gameObject);
                return;
            }
            transform.position = Vector3.Lerp(transform.position, Function.World2ScreenPoint(m_AnchorTrans.position + m_TransOffset_World) + m_TransOffset_UI, 0.8f);
        }

        UpdateSpecific();
    }

    protected virtual void UpdateSpecific() { }
    
    public void Anchor(Transform trans, Vector3 UIOffset = default, Vector3 worldOffset = default)
    {
        m_AnchorOnPosition = false;
        m_AnchorOnTrans = true;
        m_AnchorTrans = trans;
        m_TransOffset_UI = UIOffset;
        m_TransOffset_World = worldOffset;
        transform.position = Function.World2ScreenPoint(m_AnchorTrans.position + m_TransOffset_World) + m_TransOffset_UI;
    }

    public void Anchor(Vector3 worldPos)
    {
        m_AnchorOnTrans = false;
        m_AnchorOnPosition = true;
        m_AnchorPos = worldPos;
        transform.position = Function.World2ScreenPoint(m_AnchorPos);
    }
}
