using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// <para>A Singleton of ScriptableObject for storing data.</para>
/// Author: Rezky Ashari
/// </summary>
public class ScriptableData<T> : ScriptableObject where T : ScriptableObject {

    static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
                _instance = Create();
            return _instance;
        }
    }

    static T Create()
    {
        T[] dataList = Resources.LoadAll<T>("Data");
        T data = dataList.Length > 0? dataList[0] : null;
        if (data == null)
        {
            //Debug.Log("Settings not found in Data folder, Create a new one.");
            data = CreateInstance<T>();
#if UNITY_EDITOR
            CreateFolderIfNotExists("Assets", "Resources");
            CreateFolderIfNotExists("Assets/Resources", "Data");
            string inspectorTitle = System.Text.RegularExpressions.Regex.Replace(ObjectNames.GetInspectorTitle(data), "[()]", "");
            string dataName = System.Text.RegularExpressions.Regex.Replace(inspectorTitle.Replace(" (Script)", ""), @"\s+", "");
            AssetDatabase.CreateAsset(data, string.Format("Assets/Resources/Data/{0}.asset", dataName));
            AssetDatabase.SaveAssets();
#endif
        }
        return data;
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
