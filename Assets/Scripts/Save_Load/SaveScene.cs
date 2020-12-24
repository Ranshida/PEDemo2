using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveScene : MonoBehaviour
{
    public int IntegeA;
    public int IntegeB;
    public int IntegeC;
    public float FloatA;
    public float FloatB;
    public float FloatC;
    public List<int> List;
    public Test2 test;
    public Dictionary<int, int> Dict;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Dict = new Dictionary<int, int>();
            Dict.Add(1, 123);
            Dict.Add(2, 233);
            Dict.Add(3, 777);
            Debug.LogError("AddDict");
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            JsonManager.Instance.Save(this);
            Debug.LogError("Save");
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            JsonManager.Instance.Load();
            Debug.LogError("Load");
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            foreach (KeyValuePair<int,int> pair in Dict)
            {
                Debug.LogError(pair.Key + " " + pair.Value);
            }
        }
    }
}

public class Test2
{
    public int IntegeA;
    public float FloatA;
}