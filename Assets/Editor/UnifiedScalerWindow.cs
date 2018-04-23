using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UnifiedScalerWindow : EditorWindow {

    Vector2 scrollPosition;
    GameObject currentGameObject;
    Transform[] selectedTransforms;
    bool showSelectedTransforms;
    bool setToNativeSize;
    float currentScale;

    [MenuItem("Rezky Tools/Unified Scaler &s")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        GetWindow(typeof(UnifiedScalerWindow), true, "Unified Scaler");
    }

    private void OnEnable()
    {
        OnSelectionChange();
    }

    private void OnGUI()
    {
        if (currentGameObject == null)
        {
            GUILayout.Label("Please select a game object on scene", EditorStyles.boldLabel);
        }
        else
        {
            string unitType = "Game Object";
            if (selectedTransforms.Length > 1) unitType += "s";

            GUILayout.Label("Scale the selected " + unitType, EditorStyles.boldLabel);
            if (selectedTransforms.Length > 1)
            {
                showSelectedTransforms = EditorGUILayout.Foldout(showSelectedTransforms, selectedTransforms.Length + " selected " + unitType);
                if (showSelectedTransforms)
                {
                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                    Transform transform;
                    for (int i = 0; i < selectedTransforms.Length; i++)
                    {
                        transform = selectedTransforms[i];
                        EditorGUILayout.ObjectField(transform.name, selectedTransforms[i], typeof(Transform), true);
                    }
                    EditorGUILayout.EndScrollView();
                }
            }
            else
            {
                EditorGUILayout.ObjectField("Selected Game Object", currentGameObject, typeof(GameObject), true);
            }

            EditorGUI.BeginChangeCheck();
            currentScale = EditorGUILayout.FloatField("Scale", currentScale);

            Image image;
            bool isContainingImage = true;
            for (int i = 0; i < selectedTransforms.Length; i++)
            {
                selectedTransforms[i].localScale = new Vector3(currentScale, currentScale, currentScale);
                image = selectedTransforms[i].GetComponent<Image>();
                if (image == null) isContainingImage = false;
                else if (setToNativeSize) image.SetNativeSize();
            }
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("25%")) currentScale = 0.25f;
            if (GUILayout.Button("50%")) currentScale = 0.5f;
            if (GUILayout.Button("100%")) currentScale = 1f;
            if (GUILayout.Button("+10%")) currentScale += 0.1f;
            if (GUILayout.Button("-10%")) currentScale -= 0.1f;
            GUILayout.EndHorizontal();

            if (isContainingImage)
            {
                setToNativeSize = GUILayout.Button(new GUIContent("Set to Native Size", "Set the selected image to it's native size"));
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(selectedTransforms, "Scaled Objects");
            }
        }
    }

    private void OnSelectionChange()
    {
        currentGameObject = Selection.activeGameObject;
        selectedTransforms = Selection.transforms;
        if (currentGameObject != null)
        {
            currentScale = currentGameObject.transform.localScale.x;
            Repaint();
        }        
    }
}
