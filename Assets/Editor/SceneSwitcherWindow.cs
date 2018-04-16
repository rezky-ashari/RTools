using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Quick switch scene in Editor.
/// Author: Rezky Ashari
/// </summary>
public class SceneSwitcherWindow : EditorWindow {

    static string lastActiveScenePath = "";

    private static GUIStyle toggleButtonStyleNormal = null;
    private static GUIStyle toggleButtonStyleToggled = null;

    static bool showAllScenes = false;
    static bool showScenesInBuildSettings = true;

    Vector2 scenesScrollPosition;

    [MenuItem("Rezky Tools/Scene Switcher &X")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        GetWindow<SceneSwitcherWindow>("SceneSwitcher");
    }

    private void OnGUI()
    {
        if (toggleButtonStyleNormal == null)
        {
            toggleButtonStyleNormal = "Button";
            toggleButtonStyleToggled = new GUIStyle(toggleButtonStyleNormal);
            toggleButtonStyleToggled.normal.background = toggleButtonStyleToggled.active.background;
        }

        if (lastActiveScenePath != "")
        {
            if (GUILayout.Button("Back to " + Path.GetFileNameWithoutExtension(lastActiveScenePath))) SwitchScene(lastActiveScenePath);
        }

        GUILayout.Label("Quick Switch Scene", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Scenes in Build Settings", showScenesInBuildSettings ? toggleButtonStyleToggled : toggleButtonStyleNormal))
        {
            showScenesInBuildSettings = true;
            showAllScenes = false;
        }
        if (GUILayout.Button("All Scenes", showAllScenes ? toggleButtonStyleToggled : toggleButtonStyleNormal))
        {
            showScenesInBuildSettings = false;
            showAllScenes = true;
        }
        GUILayout.EndHorizontal();

        scenesScrollPosition = EditorGUILayout.BeginScrollView(scenesScrollPosition);
        if (showScenesInBuildSettings)
        {
            DisplayScenesInBuildSettings();
        }
        if (showAllScenes)
        {
            DisplayAllScenes();
        }
        EditorGUILayout.EndScrollView();
    }

    private void DisplayAllScenes()
    {
        var guids = AssetDatabase.FindAssets("t:Scene");
        var paths = Array.ConvertAll(guids, AssetDatabase.GUIDToAssetPath);
        
        if (paths.Length == 0)
        {
            EditorGUILayout.HelpBox("There is no scene asset saved in your project.", MessageType.Warning);
        }
        else
        {
            string path;
            for (int i = 0; i < paths.Length; i++)
            {
                path = paths[i];
                if (GUILayout.Button(Path.GetFileNameWithoutExtension(path)))
                {
                    SwitchScene(path);
                }
            }
        }        
    }

    void DisplayScenesInBuildSettings()
    {
        EditorBuildSettingsScene[] sceneList = EditorBuildSettings.scenes;
        if (sceneList.Length == 0)
        {
            EditorGUILayout.HelpBox("There is no scene in Build Settings.", MessageType.Warning);
        }
        else
        {
            EditorBuildSettingsScene scene;
            for (int i = 0; i < sceneList.Length; i++)
            {
                scene = sceneList[i];
                if (GUILayout.Button(Path.GetFileNameWithoutExtension(scene.path)))
                {
                    SwitchScene(scene.path);
                }
            }
        }        
    }

    void SwitchScene(string scenePath)
    {
        UnityEngine.SceneManagement.Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (currentScene.path != scenePath)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                lastActiveScenePath = currentScene.path;
                EditorSceneManager.OpenScene(scenePath);
            }            
        }        
        Close();
    }
}
