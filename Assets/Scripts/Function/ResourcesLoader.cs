using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

/// <summary>
/// 资源加载脚本
/// 更新时间2020.9.5
/// </summary>
public static class ResourcesLoader
{
    //缓存池，用于优化
    private static Dictionary<string, GameObject> m_GoDIc = new Dictionary<string, GameObject>();
    //加载预制体
    public static GameObject LoadPrefab(string path, bool cache = false)
    {
        GameObject prefab = null;
        if (!m_GoDIc.TryGetValue(path, out prefab))
        {
            prefab = Resources.Load<GameObject>(path);
            if (cache)
            {
                m_GoDIc.Add(path, prefab);
            }
        }
        return prefab;
    }

    //Sprite缓存池
    private static Dictionary<string, Sprite> m_SpriteDic = new Dictionary<string, Sprite>();
    //加载Sprite资源
    public static Sprite LoadSprite(string path, bool cache = false)
    {
        Sprite sp = null;
        if (!m_SpriteDic.TryGetValue(path, out sp))
        {
            sp = Resources.Load<Sprite>(path);
            if (cache)
            {
                m_SpriteDic.Add(path, sp);
            }
        }
        return sp;
    }

    //Texture缓存池
    private static Dictionary<string, Texture> m_TextureDic = new Dictionary<string, Texture>();
    //加载Texture资源
    public static Texture LoadTexture(string path, bool cache = false)
    {
        Texture tx = null;
        if (!m_TextureDic.TryGetValue(path, out tx))
        {
            tx = Resources.Load<Texture>(path);
            if (cache)
            {
                m_TextureDic.Add(path, tx);
            }
        }
        return tx;
    }
    
    //Material缓存池
    private static Dictionary<string, Material> m_MaterialDic = new Dictionary<string, Material>();
    //加载Texture资源
    public static Material LoadMaterial(string path, bool cache = false)
    {
        Material ma = null;
        if (!m_MaterialDic.TryGetValue(path, out ma))
        {
            ma = Resources.Load<Material>(path);
            if (cache)
            {
                m_MaterialDic.Add(path, ma);
            }
        }
        return ma;
    }

    //AudioClip缓存池
    private static Dictionary<string, AudioClip> m_AudioDic = new Dictionary<string, AudioClip>();
    //加载音乐音效资源
    public static AudioClip LoadAudio(string path, bool cache = false)
    {
        AudioClip au = null;
        if (!m_AudioDic.TryGetValue(path, out au))
        {
            au = Resources.Load<AudioClip>(path);
            if (cache)
            {
                m_AudioDic.Add(path, au);
            }
        }
        return au;
    }

    //泛型加载
    public static T Load<T>(string path) where T : Object
    {
        T obj = Resources.Load<T>(path);
        if (obj == null)
        {
            Debug.LogError("未找到对应资源，路径：" + path);
            return null;
        }
        else
        {
            return obj;
        }
    }

    //泛型加载文件夹内全部
    public static T[] LoadAll<T>(string path) where T : Object
    {
        T[] sprites = Resources.LoadAll<T>(path);
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError("未找到对应资源，路径：" + path);
            return null;
        }
        else
        {
            return sprites;
        }
    }

    //加载Xml配置文件
    public static XmlDocument LoadXml(string path)
    {
        //加载xml文件
        TextAsset xml = Resources.Load<TextAsset>(path);
        if (!xml)
        {
            Debug.LogError("配置文件不存在" + path);
            return null;
        }
        Debug.Log("武器数据加载成功");
        //读取xml文件内容
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xml.text);
        return doc;
    }
}
