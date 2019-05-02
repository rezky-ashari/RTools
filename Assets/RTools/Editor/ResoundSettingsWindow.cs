using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RTools
{
    /// <summary>
    /// <para>
    /// Settings window for Resound.
    /// Menu created: RTools > Resound Settings.
    /// InitializeOnLoad: SetDirectiveSymbol (adds RESOUND_CONFIG to directive symbol) 
    /// </para>
    /// Author: Rezky Ashari
    /// </summary>
    public class ResoundSettingsWindow : EditorWindow
    {
        float lastMasterVolume = 0;
        float lastBGMVolume = 0;
        float lastSFXVolume = 0;
        bool initializedValues = false;

        [MenuItem("RTools/Resound Settings &r", priority = 1)]
        static void ShowConfig()
        {
            GetWindow(typeof(ResoundSettingsWindow), false, "Resound");
        }

        private void OnGUI()
        {
            ResoundSettings settings = ResoundSettings.Instance;
            InitValues(settings);

            settings.masterVolume = EditorGUILayout.Slider("Master Volume", settings.masterVolume, 0, 1);
            if (settings.BGMVolume > settings.masterVolume) settings.BGMVolume = settings.masterVolume;
            if (settings.SFXVolume > settings.masterVolume) settings.SFXVolume = settings.masterVolume;

            settings.BGMVolume = EditorGUILayout.Slider("BGM Volume", settings.BGMVolume, 0, 1);
            if (settings.BGMVolume > settings.masterVolume) settings.masterVolume = settings.BGMVolume;

            settings.SFXVolume = EditorGUILayout.Slider("SFX Volume", settings.SFXVolume, 0, 1);
            if (settings.SFXVolume > settings.masterVolume) settings.masterVolume = settings.SFXVolume;

            EditorGUI.BeginChangeCheck();
            settings.muteByDefault = EditorGUILayout.Toggle(new GUIContent("Mute By Default", "Whether to mute Resound from the start."), settings.muteByDefault);
            if (EditorGUI.EndChangeCheck() && Application.isPlaying) Resound.SetMute(settings.muteByDefault);

            if (GUILayout.Button("Open Sounds Folder"))
            {
                EditorUtility.RevealInFinder("Assets/RTools/Resources/Sounds/readme.txt");
            }

            UpdateValues(settings);

            if (GUI.changed)
                EditorUtility.SetDirty(settings);
        }

        void InitValues(ResoundSettings settings)
        {
            if (!initializedValues)
            {
                lastMasterVolume = settings.masterVolume;
                lastBGMVolume = settings.BGMVolume;
                lastSFXVolume = settings.SFXVolume;
                initializedValues = true;
            }
        }

        void UpdateValues(ResoundSettings settings)
        {
            if (Application.isPlaying)
            {
                if (lastMasterVolume != settings.masterVolume)
                {
                    Resound.SetVolume(settings.masterVolume);
                    lastMasterVolume = settings.masterVolume;
                }
                if (lastBGMVolume != settings.BGMVolume)
                {
                    Resound.SetMusicVolume(settings.BGMVolume);
                    lastBGMVolume = settings.BGMVolume;
                }
                if (lastSFXVolume != settings.SFXVolume)
                {
                    Resound.SetSFXVolume(settings.SFXVolume);
                    lastSFXVolume = settings.SFXVolume;
                }
            }
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
}
