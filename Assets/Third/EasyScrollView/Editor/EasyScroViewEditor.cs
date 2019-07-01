using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CoreFramework
{
    [CustomEditor(typeof(EasyScrollView))]
    public class EasyScroViewEditor : Editor
    {
        private EasyScrollView m_EasyScrollView = null;
        private int m_Movement = 0;
        private SerializedObject m_ScrollRect = null;
        private SerializedProperty m_ScrollRectHorizonta = null;
        private SerializedProperty m_ScrollRectVertical = null;

        private SerializedProperty m_ESVData = null;
        private SerializedProperty m_CellWidth = null;
        private SerializedProperty m_CellHeight = null;
        private SerializedProperty m_GropHorizontalValue = null;
        private SerializedProperty m_GropVerticalValue = null;

        private SerializedProperty m_MoveTweenSpeed = null;
        private SerializedProperty m_PaddingLeft = null;
        private SerializedProperty m_PaddingTop = null;


        public void OnEnable()
        {
            m_EasyScrollView = (EasyScrollView) target;
            GameObject root = m_EasyScrollView.gameObject;
            Object scrollRectObject = EditorUtility.InstanceIDToObject(root.GetComponent<ScrollRect>().GetInstanceID());
            m_ScrollRect = new SerializedObject(scrollRectObject);
            m_ScrollRectHorizonta = m_ScrollRect.FindProperty("m_Horizontal");
            m_ScrollRectVertical = m_ScrollRect.FindProperty("m_Vertical");
            m_Movement = m_ScrollRectHorizonta.boolValue ? 1 : 2;

            m_ESVData = serializedObject.FindProperty("m_ESVData");
            m_CellWidth = m_ESVData.FindPropertyRelative("m_CellWidth");
            m_CellHeight = m_ESVData.FindPropertyRelative("m_CellHeight");
            m_GropHorizontalValue = m_ESVData.FindPropertyRelative("m_GropHorizontalValue");
            m_GropVerticalValue = m_ESVData.FindPropertyRelative("m_GropVerticalValue");
            m_MoveTweenSpeed = serializedObject.FindProperty("m_MoveTweenSpeed");
            m_PaddingLeft = serializedObject.FindProperty("m_PaddingLeft");
            m_PaddingTop = serializedObject.FindProperty("m_PaddingTop");

        }

        public override void OnInspectorGUI()
        {
            m_ScrollRect.Update();
            serializedObject.Update();

            EditorGUILayout.LabelField("格子大小");
            EditorGUILayout.PropertyField(m_CellWidth, new GUIContent("  宽度："));
            m_CellWidth.intValue = m_CellWidth.intValue < 0 ? 0 : m_CellWidth.intValue;
            EditorGUILayout.PropertyField(m_CellHeight, new GUIContent("  高度："));
            m_CellHeight.intValue = m_CellHeight.intValue < 0 ? 0 : m_CellHeight.intValue;

            EditorGUILayout.LabelField("偏移大小");
            EditorGUILayout.PropertyField(m_PaddingLeft, new GUIContent("  宽度："));
            m_PaddingLeft.intValue = m_PaddingLeft.intValue < 0 ? 0 : m_PaddingLeft.intValue;
            EditorGUILayout.PropertyField(m_PaddingTop, new GUIContent("  高度："));
            m_PaddingTop.intValue = m_PaddingTop.intValue < 0 ? 0 : m_PaddingTop.intValue;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("排序", GUILayout.Width(50));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("  方向：", GUILayout.Width(116));
            EditorGUILayout.LabelField("水平", GUILayout.Width(26));
            if (EditorGUILayout.Toggle(m_Movement == 1, GUILayout.Width(40)) && m_Movement != 1)
            {
                m_Movement = 1;
            }

            EditorGUILayout.LabelField("垂直", GUILayout.Width(26));
            if (EditorGUILayout.Toggle(m_Movement == 2, GUILayout.Width(40)) && m_Movement != 2)
            {
                m_Movement = 2;
            }

            EditorGUILayout.EndHorizontal();
            m_ScrollRectHorizonta.boolValue = m_Movement == 1;
            m_ScrollRectVertical.boolValue = m_Movement == 2;

            if (m_ScrollRectHorizonta.boolValue)
            {
                EditorGUILayout.PropertyField(m_GropVerticalValue, new GUIContent("  行数："));
                m_GropVerticalValue.intValue = m_GropVerticalValue.intValue < 0 ? 0 : m_GropVerticalValue.intValue;
            }
            else
            {
                EditorGUILayout.PropertyField(m_GropHorizontalValue, new GUIContent("  列数："));
                m_GropHorizontalValue.intValue = m_GropHorizontalValue.intValue < 0 ? 0 : m_GropHorizontalValue.intValue;
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Tween");
            EditorGUILayout.PropertyField(m_MoveTweenSpeed, new GUIContent("  Speed："));
            m_MoveTweenSpeed.intValue = m_MoveTweenSpeed.intValue < 0 ? 0 : m_MoveTweenSpeed.intValue;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("编辑时试用：", GUILayout.Width(116));
            if (GUILayout.Button("排版"))
            {
                m_EasyScrollView.ResetPos();
            }

            EditorGUILayout.EndHorizontal();


            m_ScrollRect.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();

        }
    }
}