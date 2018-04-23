using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(DraggableUI))]
[CanEditMultipleObjects]
public class DraggableUIEditor : Editor {

    SerializedProperty lockX;
    SerializedProperty lockY;
    SerializedProperty useBounds;
    SerializedProperty boundLeft;
    SerializedProperty boundRight;
    SerializedProperty boundTop;
    SerializedProperty boundBottom;
    SerializedProperty backOnDrop;
    SerializedProperty defaultPosition;

    private void OnEnable()
    {
        lockX = serializedObject.FindProperty("lockX");
        lockY = serializedObject.FindProperty("lockY");
        useBounds = serializedObject.FindProperty("useBounds");
        boundLeft = serializedObject.FindProperty("boundLeft");
        boundRight = serializedObject.FindProperty("boundRight");
        boundTop = serializedObject.FindProperty("boundTop");
        boundBottom = serializedObject.FindProperty("boundBottom");
        backOnDrop = serializedObject.FindProperty("backOnDrop");
        defaultPosition = serializedObject.FindProperty("defaultPosition");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();

        DraggableUI draggableUI = (DraggableUI)target;
        bool editingMultipleObjects = targets.Length != 1;

        EditorGUILayout.PropertyField(lockX, new GUIContent("Lock X", "Prevent drag on X axes"));
        //draggableUI.lockX = EditorGUILayout.Toggle(new GUIContent("Lock X", "Prevent drag on X axes"), draggableUI.lockX);
        EditorGUILayout.PropertyField(lockY, new GUIContent("Lock Y", "Prevent drag on Y axes"));
        //draggableUI.lockY = EditorGUILayout.Toggle(new GUIContent("Lock Y", "Prevent drag on Y axes"), draggableUI.lockY);

        EditorGUILayout.PropertyField(useBounds, new GUIContent("Use Bounds", "Restrict drag within bounds"));
        //draggableUI.useBounds = EditorGUILayout.Toggle(new GUIContent("Use Bounds", "Restrict drag within bounds"), draggableUI.useBounds);
        if (draggableUI.useBounds)
        {
            //EditorGUILayout.PropertyField(dragBounds);
            //draggableUI.dragBounds = EditorGUILayout.RectField("Drag Bounds", draggableUI.dragBounds);

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(boundLeft, new GUIContent("Left"));
            EditorGUILayout.PropertyField(boundRight, new GUIContent("Right"));
            EditorGUILayout.PropertyField(boundTop, new GUIContent("Top"));
            EditorGUILayout.PropertyField(boundBottom, new GUIContent("Bottom"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.PropertyField(backOnDrop, new GUIContent("Back On Drop", "Tween back this UI to default position on drop"));
        //draggableUI.backOnDrop = EditorGUILayout.Toggle(new GUIContent("Back On Drop", "Tween back this UI to default position on drop"), draggableUI.backOnDrop);
        if (draggableUI.backOnDrop)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(defaultPosition, new GUIContent("Default Position", "Position to tween back"));
            //draggableUI.defaultPosition = EditorGUILayout.Vector3Field(new GUIContent("Default Position", "Position to tween back"), draggableUI.defaultPosition);
            EditorGUI.indentLevel--;

            if (!editingMultipleObjects && draggableUI.defaultPosition != draggableUI.transform.localPosition)
            {
                if (GUILayout.Button("Update Default Position"))
                {
                    draggableUI.UpdateDefaultPosition();
                    EditorSceneManager.MarkAllScenesDirty();
                }
            }
        }

        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();

        if (!editingMultipleObjects)
        {
            if (draggableUI.GetComponent<Collider2D>() == null)
            {
                if (GUILayout.Button(new GUIContent("Attach Collider", "Attach a box collider to use with DropArea")))
                {
                    draggableUI.AttachBoxCollider();
                    EditorSceneManager.MarkAllScenesDirty();
                }
            }

            if (draggableUI.lockX && draggableUI.lockY)
            {
                EditorGUILayout.HelpBox("How can we move if X and Y is locked?", MessageType.Warning);
            }
        }        
    }
}
