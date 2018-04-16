using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class SceneCreator : MonoBehaviour {

    public string sceneName = "";
    public string description = "";

    string _message = "";
    public string Message { get { return _message; } }

    bool needToAttach = false;
    public bool ScriptReady { get { return needToAttach; } }

    // Use this for initialization
    void Start () {
        sceneName = SceneManager.GetActiveScene().name;
    }
	
	// Update is called once per frame
	void Update () {
        
    }

    public void AttachScript()
    {
        if (gameObject.GetComponent(Type.GetType(sceneName)) == null)
        {
            gameObject.AddComponent<MobileInputHandler>();
            gameObject.AddComponent(Type.GetType(sceneName));
        }
        else
        {
            needToAttach = false;
            DestroyImmediate(this);
        }       
    }

    public void CreateScript()
    {
        if (sceneName.Length == 0)
        {
            _message = "Scene name can not be empty";
            return;
        }
        if (sceneName.Contains(" "))
        {
            _message = "Scene name can not contains white-spaces";
            return;
        }
        if (Type.GetType(sceneName) != null)
        {
            AttachScript();
            DestroyImmediate(this);
            return;
        }

#if UNITY_EDITOR
        //Loading the template text file which has some code already in it.
        //Note that the text file is stored in the path PROJECT_NAME/Assets/CharacterTemplate.txt
        TextAsset templateTextFile = AssetDatabase.LoadAssetAtPath("Assets/Scripts/Scene/Template.txt", typeof(TextAsset)) as TextAsset;

        string contents = "";
        //If the text file is available, lets get the text in it
        //And start replacing the place holder data in it with the 
        //options we created in the editor window
        if (templateTextFile != null)
        {
            contents = templateTextFile.text;
            contents = contents.Replace("SCENE_NAME", sceneName);
            if (description.Length > 0 && !description.EndsWith("."))
            {
                description += ".";
            }
            string fullDescription = description.Length > 0 ? string.Format("/// <summary>\n/// {0}\n/// </summary>", description) : description;
            contents = contents.Replace("SCENE_DESCRIPTION", fullDescription);
        }
        else
        {
            Debug.LogError("Can't find the Scene Template file! Is it at the path YOUR_PROJECT/Assets/Scripts/Scene/Template.txt?");
        }

        //Let's create a new Script named "SCENE_NAME.cs"
        using (StreamWriter sw = new StreamWriter(string.Format(Application.dataPath + "/Scripts/Scene/{0}.cs", sceneName)))
        {
            sw.Write(contents);
        }
        //Refresh the Asset Database
        AssetDatabase.Refresh();

        //Now we need to attach the newly created script
        //We can use EditorApplication.isCompiling, but it doesn't seem to kick in
        //after a few frames after creating the script. So, I've created a roundabout way
        //to do so. Please see the Update function
        needToAttach = true;
#endif
    }

}
