using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/* Author: Rezky - Ikaan Studio */ 

[CustomEditor(typeof(Transporter))]
[CanEditMultipleObjects]
public class TransporterEditor : Editor {

    SerializedProperty currentPosition;
    SerializedProperty positions;
    SerializedProperty moveDuration;
    SerializedProperty lockPositions;

    private void OnEnable()
    {
        currentPosition = serializedObject.FindProperty("currentPosition");
        positions = serializedObject.FindProperty("positions");
        moveDuration = serializedObject.FindProperty("moveDuration");
        lockPositions = serializedObject.FindProperty("lockPositions");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();

        Transporter transporter = (Transporter)target;

        if (transporter.HasController)
        {
            EditorGUILayout.HelpBox("Some values driven by " + transporter.Controller + ".", MessageType.Info);
        }

        //if (transporter.PositionNotSync)
        //{
        //    EditorGUILayout.HelpBox("You can't change locked positions", MessageType.Warning);
        //}

        //DrawDefaultInspector();

        EditorGUI.BeginDisabledGroup(transporter.HasController);
        EditorGUILayout.PropertyField(currentPosition);
        //transporter.currentPosition = EditorGUILayout.IntField("Current Position", transporter.currentPosition);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.PropertyField(moveDuration);
        //transporter.moveDuration = EditorGUILayout.FloatField(new GUIContent("Move Duration", "Duration (in seconds) to move between positions"), transporter.moveDuration);
        EditorGUILayout.PropertyField(lockPositions);
        //transporter.lockPositions = EditorGUILayout.Toggle(new GUIContent("Lock Positions", "Prevent the saved positions to change"), transporter.lockPositions);        

        EditorGUI.BeginDisabledGroup(transporter.HasController);        
        EditorGUILayout.PropertyField(positions, true);        
        EditorGUI.EndDisabledGroup();

        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();

        if (targets.Length == 1)
        {
            if (!transporter.HasController)
            {
                GUILayout.BeginHorizontal();
                if (transporter.PositionNotSync)
                {
                    if (GUILayout.Button("Back to position"))
                    {
                        transporter.transform.localPosition = transporter.positions[transporter.currentPosition];
                    }
                }
                if (GUILayout.Button("Remove Current"))
                {
                    transporter.RemoveCurrentPosition();
                }
                if (GUILayout.Button("Add New Position"))
                {
                    transporter.AddCurrentPosition();
                }
                GUILayout.EndHorizontal();
            }            

            if (transporter.WarningMessage != "")
            {
                EditorGUILayout.HelpBox(transporter.WarningMessage, MessageType.Warning);
            }
        }       

        transporter.Update();
    }
}
