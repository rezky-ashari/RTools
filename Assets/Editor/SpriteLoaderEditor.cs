using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpriteLoader))]
public class SpriteLoaderEditor : Editor {

    SerializedProperty resourceDirectories;
    SerializedProperty fileNames;

    private void OnEnable()
    {
        resourceDirectories = serializedObject.FindProperty("resourceDirectories");
        fileNames = serializedObject.FindProperty("fileNames");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SpriteLoader spriteLoader = (SpriteLoader)target;

        if (spriteLoader.fileNames.Count > 0)
        {
            spriteLoader.index = EditorGUILayout.IntField("Index", spriteLoader.index);
            EditorGUILayout.TextField("Item Name", spriteLoader.itemName);
            EditorGUILayout.TextField("Position", spriteLoader.position);

            EditorGUILayout.PropertyField(fileNames, true);
        }

        EditorGUI.BeginChangeCheck();

        //spriteLoader.resourceDirectory = EditorGUILayout.DelayedTextField("Resource Directory", spriteLoader.resourceDirectory);
        EditorGUILayout.PropertyField(resourceDirectories, true);

        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Directory"))
        {
            spriteLoader.resourceDirectories.Add("");
        }
        if (GUILayout.Button("Update List"))
        {
            spriteLoader.UpdateList();
        }
        EditorGUILayout.EndHorizontal();


        
        if (GUI.changed) EditorUtility.SetDirty(target);
    }

}
