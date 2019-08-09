using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RTools
{
    /// <summary>
    /// <para>
    /// Utility for creating a scene manager object and attach the relative scripts.
    /// </para>
    /// Author: Rezky Ashari
    /// </summary>
    public class SceneCreator : MonoBehaviour
    {
        const string TEMPLATE_PATH = "Assets/RTools/Scripts/Scene/Template.txt";

        public string sceneName = "";
        public string description = "";

        string _message = "";
        public string Message { get { return _message; } }

        bool needToAttach = false;
        public bool ScriptReady { get { return needToAttach; } }

        private void Reset()
        {
            sceneName = SceneManager.GetActiveScene().name;
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
            TextAsset templateTextFile = AssetDatabase.LoadAssetAtPath(TEMPLATE_PATH, typeof(TextAsset)) as TextAsset;

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
                Debug.LogError("Can't find the Scene Template file! Is it at this path: " + TEMPLATE_PATH);
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

#if UNITY_EDITOR
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
#endif
}
