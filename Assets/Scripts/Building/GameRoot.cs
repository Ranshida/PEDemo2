using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRoot : MonoBehaviour
{
    public static string Key_Files = "FilesKey";
    public bool GameActive;

    private void Awake()
    {
        GameActive = false;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        Action action = () =>
        {
            GameActive = true;
            Init();
            NewGame();
        };
        AsyncLoadScene("SampleScene", action);
    }

    private void Update()
    {
        if (prgCB != null)
        {
            prgCB();
        }
    }

    public void Init()
    {
        BuildingManage.Instance.Init();
    }

    public void NewGame()
    {
        BuildingManage.Instance.InitBuilding(BuildingType.CEO办公室, new Int2(0, 8));
    }

    public void LoadGame()
    {

    }


    public void Save()
    {
        if (!GameActive)
        {
            return;
        }

        //首次存档
        if (!ES3.KeyExists(Key_Files))
        {
            FilesKey newFilesKey = new FilesKey();
            ES3.Save(Key_Files, newFilesKey);
        }

        FilesKey filesKey = ES3.Load(Key_Files) as FilesKey;
        for (int i = 0; i < filesKey.allFiles.Length; i++)
        {  //存档可用
            if (filesKey.allFiles[i] == false)
            {
                //这个存档的Key
                string key = DateTime.Now.ToShortDateString() + "-" + i.ToString();
                FilesData fileData = new FilesData(key);
                filesKey.KeysDict[i] = fileData;

                if (filesKey.KeysDict.ContainsKey(key))
                {
                    Debug.LogError("已包含这个");
                }
            }
        }


        //这个存档自身的数据


        //游戏的数据
        GameFiles files = new GameFiles();
        Files_Building building = BuildingManage.Instance.Save();

        files.building = building;
        foreach (ES3.k item in collection)
        {

        }
        ES3.Save()
    }


    private Action prgCB = null;
    public void AsyncLoadScene(string sceneName, Action loaded)
    {
        AsyncOperation sceneAsync = SceneManager.LoadSceneAsync(sceneName);
        prgCB = () => {
            float val = sceneAsync.progress; Debug.LogWarning(val);
            //SetProgress(val);    可以的话，显示加载进度
            if (val == 1)
            {
                if (loaded != null)
                {
                    loaded();
                    Debug.LogWarning("Loaded");
                }
                prgCB = null;
                sceneAsync = null;
                //FinishLoading();    //看看有没有需要UI表示加载完成的
            }
        };
    }
}
