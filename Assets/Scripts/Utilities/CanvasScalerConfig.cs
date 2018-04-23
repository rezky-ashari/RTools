using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CanvasScalerConfig : ScriptableObject
{
    public enum MatchMode { Portrait, Landscape, Custom }

    const string parentFolder = "Assets/Resources";
    const string dataFolder = "Data";
    const string fileName = "GlobalCanvasScaler";

    static string ConfigPath
    {
        get
        {
            return Path.Combine(dataFolder, fileName);
        }
    }

    public RenderMode renderMode = RenderMode.ScreenSpaceOverlay;
    public float referenceWidth = 1024;
    public float referenceHeight = 720;
    public MatchMode matchMode;

    [Range(0, 1)]
    public float match = 1;

    public Vector2 ReferenceResolution
    {
        get
        {
            return new Vector2(referenceWidth, referenceHeight);
        }
    }

    /// <summary>
    /// Active configuration file for Global Canvas Scaler.
    /// </summary>
    public static CanvasScalerConfig Instance
    {
        get
        {
            if (_instance == null) _instance = Create();
            return _instance;
        }
    }
    static CanvasScalerConfig _instance;

    static CanvasScalerConfig Create()
    {
        CanvasScalerConfig settings = Resources.Load<CanvasScalerConfig>(ConfigPath);
        if (settings == null)
        {
            //Debug.Log("Settings not found in " + filePath + ", Create a new one.");
            settings = CreateInstance<CanvasScalerConfig>();
#if UNITY_EDITOR
            CreateFolderIfNotExists("Assets", "Resources");
            CreateFolderIfNotExists(parentFolder, dataFolder);

            AssetDatabase.CreateAsset(settings, string.Format("{0}/{1}.asset", parentFolder, ConfigPath));
            AssetDatabase.SaveAssets();
#endif
        }
        return settings;
    }

#if UNITY_EDITOR
    static void CreateFolderIfNotExists(string parentFolder, string subFolder)
    {
        if (!AssetDatabase.IsValidFolder(Path.Combine(parentFolder, subFolder)))
        {
            AssetDatabase.CreateFolder(parentFolder, subFolder);
        }
    }
#endif
}
