﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RTools
{
    /// <summary>
    /// <para>
    /// Create a TODO list inside the Unity Editor.
    /// Menu created: RTools > To Do List (alt+d).
    /// </para>
    /// Author: Rezky Ashari
    /// </summary>
    public class ToDoList : ScriptableObject
    {
        public List<Item> list = new List<Item>();

        [System.Serializable]
        public class Item
        {
            public bool isDone;
            public string task;
        }
    }

    [CustomEditor(typeof(ToDoList))]
    public class ToDoListEditor : Editor
    {
        GUIStyle doneStyle;
        GUIStyle normalStyle;

        [MenuItem("RTools/To Do List &d")]
        static void ShowToDoList()
        {
            Selection.activeObject = ShowToDoList(true);
        }

        public static ToDoList ShowToDoList(bool createIfNotFound = false)
        {
            const string filePath = "Assets/ToDoList.asset";
            ToDoList toDoList = AssetDatabase.LoadAssetAtPath<ToDoList>(filePath);
            if (toDoList == null && createIfNotFound)
            {
                toDoList = CreateInstance<ToDoList>();
                AssetDatabase.CreateAsset(toDoList, filePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            return toDoList;
        }

        public override void OnInspectorGUI()
        {
            ToDoList todo = (ToDoList)target;

            InitializeGUIStyle();

            if (todo.list.Count > 0)
            {
                ToDoList.Item item;
                for (int i = 0; i < todo.list.Count; i++)
                {
                    item = todo.list[i];
                    EditorGUILayout.BeginHorizontal();
                    item.isDone = GUILayout.Toggle(item.isDone, "", GUILayout.ExpandWidth(false));
                    item.task = EditorGUILayout.TextArea(item.task, item.isDone ? doneStyle : normalStyle);
                    if (GUILayout.Button("x", GUILayout.Width(20)))
                    {
                        Undo.RecordObject(todo, "Remove Task");
                        todo.list.RemoveAt(i);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No task to show. \r\nYou may want to click the 'Add' button below.", MessageType.Info);
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add", GUILayout.MaxWidth(100)))
            {
                Undo.RecordObject(todo, "Undo Add");
                todo.list.Add(new ToDoList.Item());
            }
            GUILayout.EndHorizontal();

            if (GUI.changed) EditorUtility.SetDirty(todo);
        }

        private void InitializeGUIStyle()
        {
            if (doneStyle == null)
            {
                normalStyle = GUI.skin.textArea;
                normalStyle.wordWrap = true;
                doneStyle = new GUIStyle(normalStyle);
                doneStyle.normal.textColor = Color.grey;
                doneStyle.focused.textColor = Color.grey;
                doneStyle.active.textColor = Color.grey;
                doneStyle.wordWrap = true;
            }
        }
    }
}
