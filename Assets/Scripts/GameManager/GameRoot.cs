using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRoot : MonoBehaviour
{
    /// <summary>
    /// 开始新游戏
    /// </summary>
    public void StartNewGame()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
