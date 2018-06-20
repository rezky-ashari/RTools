using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(Sequencer))]
public class SequencerEditor : Editor {
    

    public override void OnInspectorGUI()
    {
        Sequencer seq = (Sequencer)target;
        
        EditorGUILayout.Space();

        seq.playOnAwake = EditorGUILayout.Toggle("Play On Awake", seq.playOnAwake);
        if (Application.isPlaying)
        {
            EditorGUILayout.IntSlider("Playing Action", seq.currentAct, 0, seq.actions.Count);
        }
        else
        {
            seq.currentAct = EditorGUILayout.IntField("Starting Action", seq.currentAct);
            if (seq.currentAct < 0) seq.currentAct = 0;
            if (seq.currentAct > seq.actions.Count) seq.currentAct = seq.actions.Count;
        }

        for (int i = 0; i < seq.actions.Count; i++)
        {
            EditorGUILayout.Space();

            SequencerActionParent action = seq.actions[i];
            string actionName = "Action #" + (i);

            EditorGUILayout.BeginHorizontal();
            
            action.active = EditorGUILayout.Toggle("", action.active, GUILayout.Width(12));
            action.foldout = GUILayout.Toggle(action.foldout, !action.foldout? actionName + ": " + action.ModeInfo : actionName, "Foldout", GUILayout.ExpandWidth(true));
            
            EditorGUILayout.EndHorizontal();

            if (action.foldout)
            {
                EditorGUI.BeginDisabledGroup(!action.active);
                EditorGUI.indentLevel++;

                DrawControl(action);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add Linked Action"))
                {
                    action.linkedActions.Add(new SequencerAction());
                }
                if (GUILayout.Button("Delete"))
                {
                    seq.actions.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                SequencerAction linkedAct;
                for (int j = 0; j < action.linkedActions.Count; j++)
                {
                    EditorGUILayout.Space();
                    linkedAct = action.linkedActions[j];
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Linked #" + j + ": " + linkedAct.ModeInfo);
                    if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(17)))
                    {
                        action.linkedActions.RemoveAt(j);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                    DrawControl(linkedAct);
                }

                EditorGUI.indentLevel--;
                EditorGUI.EndDisabledGroup();
            }            
        }

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Add Action"))
        {
            seq.actions.Add(new SequencerActionParent());
        }
        EditorGUILayout.EndHorizontal();

        if (GUI.changed)
        {
            EditorSceneManager.MarkAllScenesDirty();
        }
    }

    void DrawControl(SequencerAction action)
    {
        action.mode = (SequencerActionMode)EditorGUILayout.EnumPopup("Mode", action.mode);
        switch (action.mode)
        {
            case SequencerActionMode.Wait:
                SetDuration(action);
                break;
            case SequencerActionMode.MoveTo:
                SetGameObject(action);
                SetPosition(action);
                SetDuration(action, "Move Duration");
                break;
            case SequencerActionMode.PlayAnimation:
                SetAnimator(action);
                SetName(action, "Animation Name");
                break;
        }
    }

    void SetDuration(SequencerAction action, string label = "Duration")
    {
        action.duration = EditorGUILayout.FloatField(label, action.duration);
    }

    void SetPosition(SequencerAction action)
    {
        action.position = EditorGUILayout.Vector3Field("Position", action.position);
    }

    void SetName(SequencerAction action, string label = "Name")
    {
        action.name = EditorGUILayout.TextField(label, action.name);
    }

    void SetGameObject(SequencerAction action)
    {
        action.gameObject = (GameObject)EditorGUILayout.ObjectField("Game Object", action.gameObject, typeof(GameObject), true);
    }

    void SetAnimator(SequencerAction action)
    {
        action.animator = (Animator)EditorGUILayout.ObjectField("Animator", action.animator, typeof(Animator), true);
    }
}
