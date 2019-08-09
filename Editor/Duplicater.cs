using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RTools
{
    /// <summary>
    /// <para>
    /// Utility window for duplicating a gameobject to many gameobjects.
    /// Menu created: RTools > Duplicater (shift+alt+d).
    /// </para>
    /// Author: Rezky Ashari
    /// </summary>
    public class Duplicater : EditorWindow
    {

        GameObject baseGameObject;
        GameObject prefabRoot;
        Transform container;
        int numberToCreate = 1;
        string basename;

        [MenuItem("RTools/Duplicater #&d")]
        public static void ShowWindow()
        {
            GetWindow<Duplicater>(true, "Duplicater");
        }

        private void OnEnable()
        {
            OnSelectionChange();
        }

        private void OnGUI()
        {
            baseGameObject = (GameObject)EditorGUILayout.ObjectField("Base Game Object", baseGameObject, typeof(GameObject), true);
            container = (Transform)EditorGUILayout.ObjectField("Container", container, typeof(Transform), true);
            basename = EditorGUILayout.TextField("Prefix Name", basename);
            numberToCreate = EditorGUILayout.IntField("Amount", numberToCreate);

            if (baseGameObject == null || numberToCreate == 0)
            {
                EditorGUILayout.HelpBox("Please select a gameObject to duplicate and specify the amount.", MessageType.Info);
            }
            else if (GUILayout.Button("Create"))
            {
                bool isUI = baseGameObject.GetComponent<CanvasRenderer>() != null;
                for (int i = 0; i < numberToCreate; i++)
                {
                    GameObject instantiatedObject = null;
                    if (prefabRoot != null)
                    {
                        instantiatedObject = (GameObject)PrefabUtility.InstantiatePrefab(prefabRoot);
                    }
                    if (instantiatedObject == null) instantiatedObject = Instantiate(baseGameObject);
                    instantiatedObject.transform.SetParent(container, !isUI);
                    int num = i + 1;
                    instantiatedObject.name = basename.Length == 0 ? baseGameObject.name + num : basename + num;
                }
            }
        }

        private void OnSelectionChange()
        {
            baseGameObject = Selection.activeGameObject;
            if (baseGameObject != null)
            {
                prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(baseGameObject);
                container = baseGameObject.transform.parent;
                basename = baseGameObject.name;
            }

        }
    }
}
