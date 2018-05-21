using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ResoundConfig))]
public class ResoundConfigEditor : Editor
{
    float lastMasterVolume = 0;
    float lastBGMVolume = 0;
    float lastSFXVolume = 0;
    bool initializedValues = false;

    public override void OnInspectorGUI()
    {
        ResoundConfig config = (ResoundConfig)target;
        InitValues(config);

        config.masterVolume = EditorGUILayout.Slider("Master Volume", config.masterVolume, 0, 1);
        if (config.BGMVolume > config.masterVolume) config.BGMVolume = config.masterVolume;
        if (config.SFXVolume > config.masterVolume) config.SFXVolume = config.masterVolume;

        config.BGMVolume = EditorGUILayout.Slider("BGM Volume", config.BGMVolume, 0, 1);
        if (config.BGMVolume > config.masterVolume) config.masterVolume = config.BGMVolume;

        config.SFXVolume = EditorGUILayout.Slider("SFX Volume", config.SFXVolume, 0, 1);
        if (config.SFXVolume > config.masterVolume) config.masterVolume = config.SFXVolume;

        EditorGUI.BeginChangeCheck();
        config.muteByDefault = EditorGUILayout.Toggle("Mute By Default", config.muteByDefault);
        if (EditorGUI.EndChangeCheck() && Application.isPlaying) Resound.SetMute(config.muteByDefault);

        UpdateValues(config);
    }

    void InitValues(ResoundConfig config)
    {
        if (!initializedValues)
        {
            lastMasterVolume = config.masterVolume;
            lastBGMVolume = config.BGMVolume;
            lastSFXVolume = config.SFXVolume;
            initializedValues = true;
        }
    }

    void UpdateValues(ResoundConfig config)
    {
        if (Application.isPlaying)
        {
            if (lastMasterVolume != config.masterVolume)
            {
                Resound.SetVolume(config.masterVolume);
                lastMasterVolume = config.masterVolume;
            }
            if (lastBGMVolume != config.BGMVolume)
            {
                Resound.SetMusicVolume(config.BGMVolume);
                lastBGMVolume = config.BGMVolume;
            }
            if (lastSFXVolume != config.SFXVolume)
            {
                Resound.SetSFXVolume(config.SFXVolume);
                lastSFXVolume = config.SFXVolume;
            }
        }
    }

    [MenuItem("Rezky Tools/Resound Config")]
    static void ShowConfig()
    {
        Selection.activeObject = ResoundConfig.Instance;
    }

    [InitializeOnLoadMethod]
    static void SetDirectiveSymbol()
    {
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string dirSymbol = "RESOUND_CONFIG";
        string existingSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        if (!existingSymbols.Contains(dirSymbol))
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, existingSymbols.Length > 0 ? existingSymbols + ";" + dirSymbol : dirSymbol);
        }
    }
}
#endif

/// <summary>
/// <para>Configuration for Resound library.</para>
/// Author: Rezky Ashari
/// </summary>
public class ResoundConfig : ScriptableData<ResoundConfig> {

    public float masterVolume = 1f;
    public float BGMVolume = 0.8f;
    public float SFXVolume = 1f;
    public bool muteByDefault = false;
}
