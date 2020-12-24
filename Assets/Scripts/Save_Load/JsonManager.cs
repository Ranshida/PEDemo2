using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonManager : MonoBehaviour
{
    public static JsonManager Instance;

    private static string saveKey = "Save";

    private void Awake()
    {
        Instance = this;
    }

    public void Save(SaveScene scene)
    {
        ES3.Save<SaveScene>("1", scene);
    }

    public SaveScene Load()
    {
        SaveScene scene = ES3.Load<SaveScene>("1");
        return scene;
    }

    //public void Save(SaveScene scene)
    //{
    //    PersistenceData totalRecord = JsonUtility.FromJson<PersistenceData>(saveKey);
    //    if (totalRecord == null)
    //    {
    //        totalRecord = new PersistenceData();
    //    }

    //    totalRecord.FloatA = scene.FloatA;
    //    totalRecord.FloatB = scene.FloatB;
    //    totalRecord.FloatC = scene.FloatC;
    //    totalRecord.IntegeA = scene.IntegeA;
    //    totalRecord.IntegeB = scene.IntegeB;
    //    totalRecord.IntegeC = scene.IntegeC;
    //    totalRecord.List = scene.List;
    //    totalRecord.Dict = scene.Dict;


    //    string totalJson = JsonUtility.ToJson(totalRecord);
    //    PlayerPrefs.SetString(saveKey, totalJson);             //上传数据
       
    //}

    //public PersistenceData Load()
    //{
    //    PersistenceData totalRecord = JsonUtility.FromJson<PersistenceData>(PlayerPrefs.GetString(saveKey));
    //    if (totalRecord == null)
    //    {
    //        totalRecord = new PersistenceData();
    //    }
    //    return totalRecord;
    //}
}
