using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SceneBoxTool : EditorWindow
{
    private Transform m_BoxRoot;
    private Buildinger m_Buildinger;

    [MenuItem("Tool/SceneBoxTool")]
    private static void Open()
    {
        EditorWindow.GetWindow<SceneBoxTool>();
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
    {
        m_BoxRoot = null;
    }

    private void OnGUI()
    {
        OnGUI_GetObject<Transform>("BoxRoot:",ref m_BoxRoot);

        if (GUILayout.Button("重置Box"))
        {
            ResetData();
        }

        if (GUILayout.Button("导出"))
        {
            Output(ResetData());
        }

        OnGUI_GetObject<Buildinger>("Building:", ref m_Buildinger);

        if (GUILayout.Button("重置Buildinger"))
        {
            ResetBuildRecord();
        }
    }

    private void OnGUI_GetObject<T>(string name, ref T target)where T:Object
    {
        Object tmpObj = EditorGUILayout.ObjectField(name, target, typeof(T), true);
        if (tmpObj != target)
        {
            if (tmpObj == null)
            {
                target = null;
            }
            else
            {
                target = tmpObj as T;
            }
        }
    }

    private List<Rect> ResetData()
    {
        Canvas canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        RectTransform canvasRectTransform = canvas.transform.rectTransform();

        List<Rect> rects = new List<Rect>();
        for (int index = 0; index < m_BoxRoot.childCount; index++)
        {
            Transform childTransform = m_BoxRoot.GetChild(index);
            RectTransform childRectTransform = childTransform.rectTransform();
            BoxCollider box = childRectTransform.GetComponent<BoxCollider>();
            if (box != null)
            {
                box.center = Vector3.zero;
                box.size = childRectTransform.rect.size;

                Rect rect = childRectTransform.rect;
                
                rect.x = childTransform.position.x;
                rect.y = childTransform.position.y;

                rects.Add(rect);
            }
        }

        return rects;
    }

    private void Output(List<Rect> rects)
    {
        GameConfig.OutputSceneBoxData(rects);
    }

    private void ResetBuildRecord()
    {
        BoxCollider[] boxColliders = m_Buildinger.transform.GetComponentsInChildren<BoxCollider>();
        int count = boxColliders.Length;
        m_Buildinger.BuildingList = new List<Transform>(count);
        for (int index = 0; index < count; index++)
        {
            m_Buildinger.BuildingList.Add(boxColliders[index].rectTransform());
        }

        PrefabUtility.ApplyPrefabInstance(m_Buildinger.gameObject, InteractionMode.AutomatedAction);
    }
}
