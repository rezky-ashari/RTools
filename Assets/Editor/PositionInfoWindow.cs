using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PositionInfoWindow : EditorWindow {

    GameObject gameObject;
    bool isUI;

    [MenuItem("Rezky Tools/Position Info &p")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        GetWindow(typeof(PositionInfoWindow), false, "Position Info");
    }

    private void OnEnable()
    {
        gameObject = Selection.activeGameObject;
    }

    private void OnGUI()
    {
        gameObject = (GameObject)EditorGUILayout.ObjectField("Game Object", gameObject, typeof(GameObject), true);
        if (gameObject != null)
        {
            EditorGUILayout.Vector3Field("Local Position", gameObject.transform.localPosition);
            EditorGUILayout.Vector3Field("Position", gameObject.transform.position);
            if (gameObject.GetComponent<CanvasRenderer>() != null)
            {
                EditorGUILayout.Vector3Field("Position", (gameObject.transform as RectTransform).anchoredPosition);
            }
        }
        Repaint();
    }
}
