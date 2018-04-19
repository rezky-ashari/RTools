using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Duplicater : EditorWindow {

    GameObject baseGameObject;
    GameObject prefabRoot;
    Transform container;
    int numberToCreate = 1;
    string basename;

    [MenuItem("Rezky Tools/Duplicater &D")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
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

        if (baseGameObject != null && container != null && numberToCreate > 0 && GUILayout.Button("Create"))
        {
            bool isUI = baseGameObject.GetComponent<CanvasRenderer>() != null;
            for (int i = 0; i < numberToCreate; i++)
            {
                GameObject instantiatedObject = null;
                if (prefabRoot != null)
                {
                    instantiatedObject = (GameObject)PrefabUtility.InstantiatePrefab(prefabRoot);
                    Debug.Log("Instantiate Prefab: " + instantiatedObject);
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
            prefabRoot = PrefabUtility.FindPrefabRoot(baseGameObject);
            container = baseGameObject.transform.parent;
            basename = baseGameObject.name;
            Debug.Log("Prefab root: " + prefabRoot);
        }
        
    }
}
