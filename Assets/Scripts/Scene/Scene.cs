using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

/// <summary>
/// <para>Base class for game scenes.</para>
/// Author: Rezky Ashari
/// </summary>
public class Scene : MonoBehaviour {

    /// <summary>
    /// Listen to scene's onClick event.
    /// </summary>
    public event Action<string, string> OnClickListener;

    static Scene Current
    {
        get
        {
            if (_currentScene == null)
            {
                bool isError = true;
                GameObject sceneManager = GameObject.Find("SceneManager");
                if (sceneManager != null)
                {
                    _currentScene = sceneManager.GetComponent<Scene>();
                    isError = _currentScene == null;
                }
                if (isError)
                Debug.LogError("Can't send click event. There's no 'SceneManager' object in hierarchy list or maybe it doesn't contain a 'Scene' script.");
            }
            return _currentScene;
        }
    }
    static Scene _currentScene;

    /// <summary>
    /// This scene's name.
    /// </summary>
    public string SceneName
    {
        get
        {
            return SceneManager.GetActiveScene().name;
        }
    }

    protected virtual void Awake()
    {
        GameButton.SetSceneListener(Click);
    }

#if UNITY_EDITOR
    [MenuItem("GameObject/Scene Manager", false, 11)]
    static void AddSceneManager()
    {
        if (GameObject.Find("SceneManager") != null)
        {
            Debug.LogWarning("You only need one scene manager per scene.");
        }
        else
        {
            GameObject sceneManager = new GameObject("SceneManager");
            //sceneManager.AddComponent<MobileInputHandler>();
            sceneManager.AddComponent<SceneCreator>();
            Selection.activeGameObject = sceneManager;
            EditorSceneManager.MarkAllScenesDirty();
        }
    }
#endif

    public static void SendEvent(string name)
    {
        if (Current != null) Current.OnEvent(name);
    }

    /// <summary>
    /// Send a custom event to the current scene.
    /// </summary>
    /// <param name="name">Event name</param>
    public virtual void OnEvent(string name)
    {

    }

    /// <summary>
    /// Send a click event to the current scene.
    /// </summary>
    /// <param name="id">Click id</param>
    /// <param name="senderName">Click value</param>
    public static void SendClickEvent(string id, string senderName = "")
    {
        //Debug.Log("Send click event to " + CurrentScene);
        if (Current != null) Current.Click((senderName != "") ? senderName + ":" + id : id);
    }   

    public void Click(string id)
    {
        //string senderInfo = "Receive click from " + id;
        //Debug.Log(senderInfo);

        string value = "";

        if (id.Contains(":"))
        {
            string[] data = id.Split(':');
            id = data[0];
            value = data[1];
        }

        HandleClick(id, value);

        if (OnClickListener != null) OnClickListener(id, value);
    }

    public virtual void HandleClick(string id, string value)
    {

    }

    protected virtual void OnDestroy()
    {
        OnClickListener = null;
        _currentScene = null;
        Resources.UnloadUnusedAssets();
    }

    /// <summary>
    /// Hide gameObjects by set their active state to false.
    /// </summary>
    /// <param name="gameObjects"></param>
    protected void Hide(params GameObject[] gameObjects)
    {
        for (int i = 0; i < gameObjects.Length; i++)
        {
            gameObjects[i].SetActive(false);
        }
    }

    /// <summary>
    /// Show gameObjects by set their active state to true.
    /// </summary>
    /// <param name="gameObjects"></param>
    protected void Show(params GameObject[] gameObjects)
    {
        for (int i = 0; i < gameObjects.Length; i++)
        {
            gameObjects[i].SetActive(true);
        }
    }

    /// <summary>
    /// Add click listener to the scene.
    /// </summary>
    /// <param name="listener"></param>
    public static void AddClickListener(Action<string, string> listener)
    {
        if (Current != null) Current.OnClickListener += listener;
    }

    /// <summary>
    /// Remove click listener from scene.
    /// </summary>
    /// <param name="listener"></param>
    public static void RemoveClickListener(Action<string, string> listener)
    {
        if (Current != null) Current.OnClickListener -= listener;
    }

    /// <summary>
    /// Get scene list in build settings.
    /// </summary>
    /// <returns></returns>
    public static string[] GetList()
    {
#if UNITY_EDITOR
        return (from scene in EditorBuildSettings.scenes where scene.enabled select scene.path).ToArray();
#else
        Debug.Log("Get scene list only available in Editor Mode");
        return new string[0];
#endif
    }
}
