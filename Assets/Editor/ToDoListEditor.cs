using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ToDoList))]
public class ToDoListEditor : Editor {

    GUIStyle doneStyle;
    GUIStyle normalStyle;

    [MenuItem("Rezky Tools/To Do List &d")]
    static void ShowToDoList()
    {
        const string filePath = "Assets/ToDoList.asset";
        ToDoList toDoList = AssetDatabase.LoadAssetAtPath<ToDoList>(filePath);
        if (toDoList == null)
        {
            toDoList = CreateInstance<ToDoList>();
            AssetDatabase.CreateAsset(toDoList, filePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        Selection.activeObject = toDoList;
    }

    public override void OnInspectorGUI()
    {
        ToDoList todo = (ToDoList)target;

        InitializeGUIStyle();

        if (todo.list != null && todo.list.Count > 0)
        {
            ToDoList.Item item;
            for (int i = 0; i < todo.list.Count; i++)
            {
                item = todo.list[i];
                EditorGUILayout.BeginHorizontal();
                item.isDone = GUILayout.Toggle(item.isDone, "", GUILayout.ExpandWidth(false));
                item.task = EditorGUILayout.TextArea(item.task, item.isDone? doneStyle : normalStyle);
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
