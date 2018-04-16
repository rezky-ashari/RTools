using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneCreator))]
public class SceneCreatorEditor : Editor
{

    public override void OnInspectorGUI()
    {
        SceneCreator creator = (SceneCreator)target;

        if (creator.ScriptReady)
        {
            EditorGUILayout.HelpBox("Attaching script...", MessageType.Info);
            if (!EditorApplication.isCompiling) creator.AttachScript();
        }
        else
        {
            GUILayoutOption labelWidthOption = GUILayout.Width(80);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Scene Name", labelWidthOption);
            creator.sceneName = EditorGUILayout.TextField(creator.sceneName);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Description", labelWidthOption);
            creator.description = EditorGUILayout.TextField(creator.description);
            GUILayout.EndHorizontal();

            if (!creator.ScriptReady)
            {
                if (GUILayout.Button("Create Script") || (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return))
                {
                    creator.CreateScript();
                }
            }            

            if (creator.Message != "")
            {
                EditorGUILayout.HelpBox(creator.Message, MessageType.Error);
            }            
        }
    }
}
