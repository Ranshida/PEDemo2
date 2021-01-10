using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 协程的服务
/// </summary>
public class CoroutineSvc : MonoBehaviour
{
    public static CoroutineSvc Instance = null;

    private void Awake()
    {
        Instance = this;
    }

    public void TryStopCoroutine(Coroutine cor)
    {
        if (cor == null)
            return;

        StopCoroutine(cor);
    }

    /// <summary>
    /// 延时调用方法
    /// </summary>
    /// <param name="timer"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public Coroutine WaitForAction(float timer, Action action)
    {
        //没有等待时间，立即执行
        if (timer == 0)
        {
            action();
            return null;
        }

        return StartCoroutine(WaitAction(timer, action));
    }


    private IEnumerator WaitAction(float timer, Action action)
    {
        yield return new WaitForSeconds(timer);
        action();
    }
}
