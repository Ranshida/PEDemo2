using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StaticRoot : MonoBehaviour
{
    public static bool IsGameRootScene = false;
    private void Start()
    {
        if (!IsGameRootScene)
        {
            SceneManager.LoadScene("Root");
        }
    }
}
