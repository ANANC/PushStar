using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameConfig  {

    public static string s_OutputPath = "Assets/Config/SceneBoxConfig.txt";

    private class BoxData
    {
        public List<Rect> m_SceneRects;
    }

    public static void OutputSceneBoxData(List<Rect> rects)
    {
        BoxData data = new BoxData();
        data.m_SceneRects = rects;

        string jsonContent = JsonUtility.ToJson(data,true);
        if (!File.Exists(s_OutputPath))
        {
            File.Create(s_OutputPath);
        }
        File.WriteAllText(s_OutputPath, jsonContent);
        Debug.Log("输出完毕");
    }

    public static List<Rect> LoadSceneBoxData()
    {
        string config = File.ReadAllText(s_OutputPath);
        BoxData data = new BoxData();
        data = JsonUtility.FromJson<BoxData>(config);
        return data.m_SceneRects;
    }

}
