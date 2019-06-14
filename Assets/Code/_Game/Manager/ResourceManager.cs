using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : BaseManager
{
    public enum Type
    {
        CharacterObject,
        UI,
        Build,
    }

    private readonly Dictionary<Type, string> m_DirectoryName =new Dictionary<Type, string>
    {
        {Type.CharacterObject, "CharacterObject"},
        {Type.UI, "UI"},
        {Type.Build,"Build" }
    };

    public GameObject Instance(Type type,string resourceName)
    {
        Object uiObject = Load(type, resourceName);
        if(uiObject == null)
        {
            return null;
        }
        return GameObject.Instantiate(uiObject) as GameObject;
    }

    public Object Load(Type type, string uiName)
    {
        return Load<Object>(m_DirectoryName[type]+"/" + uiName);
    }


    public T  Load<T>(string path) where T : Object
    {
       return  Resources.Load<T>(path);
    }

    public GameObject LoadBuild(string buildName)
    {
        GameObject go = Instance(Type.Build, buildName);
        go.transform.SetParent(GameDefine.buildRoot);
        go.transform.localScale = Vector3.one;
        go.transform.localPosition =Vector3.zero;

        return go;
    }
}
