using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏入口脚本
/// 不会销毁，用于提供永久方法
/// 更新时间2020.9.8
/// </summary>
public class GameRoot : MonoBehaviour
{
    public static GameRoot Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StaticRoot.IsGameRootScene = true;
        DontDestroyOnLoad(this.gameObject);
        SceneManager.LoadScene("MainMenu");
    }
}
