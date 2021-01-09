﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestControl : MonoBehaviour
{
    public static QuestControl Instance;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 确定，确定之后接触暂停继续
    /// </summary>
    /// <param name="txt">文案</param>
    public void Init(string txt)
    {

    }

    /// <summary>
    /// 确定，确定之后执行方法
    /// </summary>
    /// <param name="txt">文案</param>
    /// <param name="onAccept">确定后执行</param>
    public void Init(string txt, Action onAccept)
    {

    }

    /// <summary>
    /// 接受或拒绝
    /// </summary>
    /// <param name="txt">文案</param>
    /// <param name="onAccept">接受后执行</param>
    /// <param name="onRefuse">拒绝后执行</param>
    public void Init(string txt, Action onAccept, Action onRefuse)
    {

    }

    /// <summary>
    /// 引导任务
    /// </summary>
    /// <param name="step">第几个任务</param>
    public void Finish(int step)
    {

    }
}
