using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameRoot : MonoBehaviour
{
    public static string Key_Files = "FilesKey";      //总Key
    public bool GameActive { get; private set; }

    public GameObject fileGo;
    public bool QuickEnter;
    public Transform fileContent;

    private void Awake()
    {
        GameActive = false;
        StaticAutoRoot.IsGameRootScene = true;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            string[] keys = ES3.GetKeys();
            foreach (string key in keys)
            {
                ES3.DeleteKey(key);
            }
            Debug.Log("清除存档");
        }

        if (GameActive)
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                Save();
            }
        }

        if (prgCB != null)
        {
            prgCB();
        }
    }

    private void Start()
    {
        if (QuickEnter)
        {
            NewGame();
        }
    }


    //开始新游戏
    public void NewGame()
    {
        Action action = () =>
        {
            GameActive = true;
            BuildingManage.Instance.Init();
            OnNewGame();
        };

        AsyncLoadScene("SampleScene", action);
    }
    private void OnNewGame()
    {
        //基础办公室、员工
        BuildingManage.Instance.OnNewGame();
    }

    //读取存档
    public void LoadGame(int fileIndex)
    {
        //从未存过档
        if (!ES3.KeyExists(Key_Files))
            return;

        FilesKey filesKey = ES3.Load(Key_Files) as FilesKey;
        string key = filesKey.KeysDict[fileIndex].FileKey;

        GameFiles gameFiles = ES3.Load(key) as GameFiles;
        Debug.Log("读取存档：" + fileIndex);

        Action action = () =>
        {
            GameActive = true;
            BuildingManage.Instance.Init();   //需要初始化的组件必不可少
            OnLoadGame(gameFiles);
        };
        AsyncLoadScene("SampleScene", action);
    }
    private void OnLoadGame(GameFiles files)
    {
        BuildingManage.Instance.OnLoadGame(files);
        GameControl.Instance.Money = files.main.Money;
    }

    //打开存档面板
    public void FilesPanel()
    {
        foreach (Transform child in Function.ReturnChildList(fileContent))
        {
            Destroy(child.gameObject);
        }

        //首次存档
        if (!ES3.KeyExists(Key_Files))
        {
            FilesKey newFilesKey = new FilesKey();
            ES3.Save(Key_Files, newFilesKey);
        }
        FilesKey files = ES3.Load(Key_Files) as FilesKey;
        for (int i = 0; i < files.allFiles.Length; i++)
        {
            GameObject go = Instantiate(fileGo, fileContent);

            if (files.allFiles[i] == false)
            {  //没有存档
                go.transform.Find("Txt_Key").GetComponent<Text>().text = "空";
                go.transform.Find("Txt_Time").GetComponent<Text>().text = "";
            }
            else
            {  //已有存档
                int index = i;
                go.GetComponent<Button>().onClick.AddListener(() => { LoadGame(index); });
                go.transform.Find("Txt_Key").GetComponent<Text>().text = files.KeysDict[index].FileKey;
                go.transform.Find("Txt_Time").GetComponent<Text>().text = files.KeysDict[index].Date;
            }
        }
    }

    public void Save()
    {
        if (!GameActive)
        {
            return;
        }

        Debug.Log("Save");

        //首次存档
        if (!ES3.KeyExists(Key_Files))
        {
            Debug.Log("First");
            FilesKey newTest = new FilesKey();
            ES3.Save(Key_Files, newTest);
        }
        FilesKey test = ES3.Load(Key_Files) as FilesKey;

        for (int i = 0; i < test.allFiles.Length; i++)
        {
            if (test.allFiles[i] == false)
            {
                string key = DateTime.Now.ToShortDateString() + "-" + i.ToString();
                test.allFiles[i] = true;

                FilesData fileData = new FilesData();
                fileData.Init(key);
                test.KeysDict.Add(i, fileData);
                Debug.Log(i + "  " + test.KeysDict[i].FileName);
                ES3.Save(Key_Files, test);

                //游戏的数据
                GameFiles files = new GameFiles();

                File_Building building = BuildingManage.Instance.Save();
                files.building = building;

                files.main = new Files_Main();
                files.main.Money = GameControl.Instance.Money;
                ES3.Save(key, files);
                break;
            }
       
        }
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
