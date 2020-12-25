using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameRoot : MonoBehaviour
{
    public static string Key_Files = "FilesKey";
    public bool GameActive;
    public GameObject fileGo;

    public Transform fileContent;

    private void Awake()
    {
        GameActive = false;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        
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
            Debug.LogError("清除存档");
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

    public void FilesPanel()
    {
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
                //go.GetComponent<Button>().onClick.AddListener(() => { LoadGame(i); });
                go.transform.Find("Txt_Key").GetComponent<Text>().text = "空";
                go.transform.Find("Txt_Time").GetComponent<Text>().text = "";
            }
            else
            {
                Debug.LogError(i);
                Debug.LogError(files.KeysDict[i].FileKey);
                //已有存档
                go.GetComponent<Button>().onClick.AddListener(() => { LoadGame(i); });
                go.transform.Find("Txt_Key").GetComponent<Text>().text = files.KeysDict[i].FileKey;
                go.transform.Find("Txt_Time").GetComponent<Text>().text = files.KeysDict[i].Date;
            }
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
        BuildingManage.Instance.InitBuilding(BuildingType.CEO办公室, new Int2(0, 8));
    }


    //读取存档
    public void LoadGame(int fileIndex)
    {
        Debug.LogError("读取存档：" + fileIndex);

        //Action action = () =>
        //{
        //    GameActive = true;
        //    BuildingManage.Instance.Init();
        //    OnLoadGame(fileIndex);
        //};

        //AsyncLoadScene("SampleScene", action);
    }

    private void OnLoadGame(int fileIndex)
    {
        BuildingManage.Instance.InitBuilding(BuildingType.CEO办公室, new Int2(0, 8));
    }


    public void Save()
    {
        if (!GameActive)
        {
            return;
        }

        Debug.LogError("Save");

        //首次存档
        if (!ES3.KeyExists(Key_Files))
        {
            Debug.LogError("First");
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

                FilesData test1 = new FilesData();
                test1.Init(key);
                test.KeysDict.Add(i, test1);
                Debug.LogError(i + "  " + test.KeysDict[i].FileName);
                ES3.Save(Key_Files, test);

                FilesKey newtest = ES3.Load(Key_Files) as FilesKey;
                Debug.LogError(newtest.KeysDict[i].FileName);
                break;
            }
        }

       



        //FilesKey filesKey = ES3.Load(Key_Files) as FilesKey;
        //int index = -1;
        //for (int i = 0; i < filesKey.allFiles.Length; i++)
        //{  //找到下一个可用的存档
        //    if (filesKey.allFiles[i] == false)
        //    {
        //        index = i;

        //        break;
        //    }
        //}

        //if (index != -1)
        //{
        //    filesKey.allFiles[index] = true;
        //    string key = index.ToString();
        //    Debug.LogError("Confirm: " + index + " " + key);

        //    //这个存档自身的数据
        //    FilesData fileData = new FilesData(key);
        //    filesKey.KeysDict.Add(index, fileData);
        //    Debug.LogError(filesKey.KeysDict[index].FileKey);
        //    ES3.Save(Key_Files, filesKey);

        //    FilesKey ttt = ES3.Load(Key_Files) as FilesKey;
        //    Debug.LogError(ttt.KeysDict[index].FileKey);

        //    //游戏的数据
        //    //GameFiles files = new GameFiles();
        //    //Files_Building building = BuildingManage.Instance.Save();
        //    //ES3.Save(key, files);
        //}
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
