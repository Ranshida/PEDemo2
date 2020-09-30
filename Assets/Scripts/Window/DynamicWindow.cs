using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 动态弹窗、消息提示面板
/// </summary>
public class DynamicWindow : MonoBehaviour
{
    public static DynamicWindow Instance { get; private set; }
    private GameObject dialoguePrefab;

    private Transform tips;
    private Transform infos;

    private void Awake()
    {
        Instance = this;
        tips = transform.Find("Tips");
        tips = transform.Find("Infos");
        dialoguePrefab = ResourcesLoader.LoadPrefab("Prefabs/UI/Dialogue");
    }

    //设置对话
    public void SetDialogue(Transform followedTrans, string conversition, float timer = 3, Vector3 UIOffset = default, Vector3 worldOffset = default)
    {
        GameObject sign = Instantiate(dialoguePrefab, transform); //设置了锚点，就不需要初始化位置了
        UIDialogue pooledDialogue = sign.GetComponentInChildren<UIDialogue>();
        pooledDialogue.Init(conversition, timer);
        pooledDialogue.Anchor(followedTrans, UIOffset, worldOffset);
    }
}
