using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpriteLoader))]
public class SpriteLoaderEditor : Editor {

    SerializedProperty resourceDirectories;
    SerializedProperty fileNames;

    GUIContent positionTooltip;

    bool isAddingResourceFolder = false;
    string newFolderName;
    string message = "";

    private void OnEnable()
    {
        resourceDirectories = serializedObject.FindProperty("resourceDirectories");
        fileNames = serializedObject.FindProperty("fileNames");
        positionTooltip = new GUIContent("Position", "[Read Only] Position in string format. Useful for Item Registry.");
    }

    void EndAddingFolder()
    {
        isAddingResourceFolder = false;
        GUIUtility.keyboardControl = 0;
        message = "";
    }

    public override void OnInspectorGUI()
    {
        SpriteLoader spriteLoader = (SpriteLoader)target;

        if (!string.IsNullOrEmpty(message))
        {
            EditorGUILayout.HelpBox(message, MessageType.Info);
        }

        if (isAddingResourceFolder)
        {
            EditorGUILayout.LabelField("Add New Resource Folder", EditorStyles.boldLabel);
            GUI.SetNextControlName("NewFolderName");
            newFolderName = EditorGUILayout.TextField("Folder Name", newFolderName);
            EditorGUILayout.LabelField("Path: Resources/" + newFolderName);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel"))
            {
                EndAddingFolder();
            }
            if (GUILayout.Button("Add"))
            {
                if (spriteLoader.resourceDirectories.IndexOf(newFolderName) >= 0)
                {
                    message = "Already added this folder.";
                }
                else
                {
                    spriteLoader.resourceDirectories.Add(newFolderName);
                    EndAddingFolder();
                }                
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            serializedObject.Update();

            if (spriteLoader.fileNames.Count > 0)
            {
                spriteLoader.index = EditorGUILayout.IntField("Index", spriteLoader.index);
                EditorGUILayout.TextField("Item Name", spriteLoader.itemName);
                EditorGUILayout.TextField(positionTooltip, spriteLoader.position);

                EditorGUILayout.PropertyField(fileNames, true);
            }

            EditorGUI.BeginChangeCheck();

            //spriteLoader.resourceDirectory = EditorGUILayout.DelayedTextField("Resource Directory", spriteLoader.resourceDirectory);
            EditorGUILayout.PropertyField(resourceDirectories, true);

            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Directory"))
            {
                //spriteLoader.resourceDirectories.Add("");
                isAddingResourceFolder = true;
                EditorGUI.FocusTextInControl("NewFolderName");
            }
            if (GUILayout.Button("Update List"))
            {
                spriteLoader.UpdateList();
            }
            EditorGUILayout.EndHorizontal();

            if (GUI.changed) EditorUtility.SetDirty(target);
        }        
    }


}
