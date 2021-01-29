using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 自动进入GameRoot场景
/// 更新时间2019.11.21
/// </summary>
public class StaticAutoRoot:MonoBehaviour
{
    public static bool IsGameRootScene = false;
    private void Start()
    {
        if (!IsGameRootScene)
        {
            IsGameRootScene = true;
            SceneManager.LoadScene("GameRoot");
        }
    }
}
