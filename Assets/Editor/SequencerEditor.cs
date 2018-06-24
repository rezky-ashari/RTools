using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(Sequencer))]
public class SequencerEditor : Editor {
    
    static GUIContent gcStartRecord;
    static GUIContent gcStopRecord;
    static GUILayoutOption smallButtonLayout;
    static GUIStyle labelStyle;

    public override void OnInspectorGUI()
    {
        Sequencer seq = (Sequencer)target;
        
        EditorGUILayout.Space();

        if (labelStyle == null)
        {
            labelStyle = "Foldout";
            labelStyle.wordWrap = true;
        }

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
            string label = actionName;
            if (!action.foldout)
            {
                label = actionName + " : " + action.ModeInfo;
                for (int k = 0; k < action.linkedActions.Count; k++)
                {
                    label += ", " + action.linkedActions[k].ModeInfo;
                }
            }
            EditorGUILayout.BeginHorizontal();
            
            action.active = EditorGUILayout.Toggle("", action.active, GUILayout.Width(12));
            action.foldout = GUILayout.Toggle(action.foldout, label, labelStyle, GUILayout.ExpandWidth(true));
            
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

        if (GUI.changed && !Application.isPlaying)
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
                if (Application.isPlaying)
                {
                    EditorGUILayout.TextField("Animation", action.target);
                }
                else
                {
                    if (action.animator != null)
                    {
                        if (action.animator != action.lastAnimator || action.Options == null)
                        {
                            action.Options = GetAnimationStates(action.animator);
                            action.lastAnimator = action.animator;
                        }
                        action.selected = EditorGUILayout.Popup("Animation", action.selected, action.Options);
                        //if (action.optionList == null) action.optionList = new SequencerAction.Options();
                        action.target = action.SelectedValue; // SetOptionsPopup("Animation", action.optionList);
                    }
                    else
                    {
                        action.lastAnimator = null;
                        action.selected = 0;
                        action.Options = null;
                    }
                }                
                break;
            case SequencerActionMode.InvokeFunction:
                SetGameObject(action);
                if (Application.isPlaying) {
                    EditorGUILayout.ObjectField("Script", action.script, typeof(MonoBehaviour), true);
                    EditorGUILayout.TextField("Function", action.target);
                }
                else
                {
                    if (action.gameObject != null)
                    {
                        //SetScript(action);
                        if (action.lastGameObject != action.gameObject || action.Options == null)
                        {
                            action.Options = GetScriptsAttached(action.gameObject);
                            action.lastGameObject = action.gameObject;
                        }
                        action.selected = EditorGUILayout.Popup("Script", action.selected, action.Options);
                        action.script = (MonoBehaviour)action.gameObject.GetComponent(action.SelectedValue);
                        string[] methods = GetMethods(action.script);
                        if (methods.Length > 0)
                        {
                            action.selectedMethod = (int)Mathf.Clamp(EditorGUILayout.Popup("Function", action.selectedMethod, methods), 0, methods.Length);
                            string[] methodData = methods[action.selectedMethod].Split('(');
                            action.target = methodData[0];
                            action.paramType = methodData[1].Replace(")", "").ToLower();
                            switch (action.paramType)
                            {
                                case "string":
                                    action.paramString = EditorGUILayout.TextField("Parameter", action.paramString);
                                    break;
                                case "int":
                                    action.paramInt = EditorGUILayout.IntField("Parameter", action.paramInt);
                                    break;
                                case "float":
                                    action.paramFloat = EditorGUILayout.FloatField("Parameter", action.paramFloat);
                                    break;
                                default:
                                    //Debug.Log("Type not handled: " + action.paramType);
                                    break;
                            }
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("There is no invokeable function in this sript.", MessageType.Error);
                        }
                    }
                    else
                    {
                        action.lastGameObject = null;
                        action.selected = 0;
                    }
                }                
                break;
        }
    }



    string[] GetScriptsAttached(GameObject gameObject)
    {
        Component[] behaviours = gameObject.GetComponents(typeof(MonoBehaviour));
        string[] behaviourNames = new string[behaviours.Length];
        for (int i = 0; i < behaviours.Length; i++)
        {
            behaviourNames[i] = behaviours[i].GetType().Name;
        }
        return behaviourNames;
    }

    string[] GetMethods(MonoBehaviour mono)
    {
        var typeWithMethods = mono.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        List<string> methodList = new List<string>();
        ParameterInfo[] parameterInfo;
        foreach (var function in typeWithMethods)
        {
            parameterInfo = function.GetParameters();
            if (function.GetParameters().Length <= 1)
            {
                string paramType = parameterInfo.Length == 1 ? parameterInfo[0].ParameterType.Name.ToLower() : "";
                if (paramType.StartsWith("int")) paramType = "int";
                else if (paramType == "single") paramType = "float";
                methodList.Add(function.Name + "(" + paramType + ")");
            }            
        }
        return methodList.ToArray();
    }

    string[] GetAnimationStates(Animator animator)
    {
        var runtimeController = animator.runtimeAnimatorController;
        if (runtimeController == null)
        {
            Debug.Log("RuntimeAnimatorController must not be null.");
            return new string[] { };
        }

        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(runtimeController));
        if (controller == null)
        {
            Debug.LogErrorFormat("AnimatorController must not be null.");
            return new string[] { };
        }

        ChildAnimatorState[] states = controller.layers[0].stateMachine.states;
        string[] stateNames = new string[states.Length];
        for (int i = 0; i < states.Length; i++)
        {
            stateNames[i] = states[i].state.name;
        }
        return stateNames;
    }

    void SetScript(SequencerAction action)
    {
        action.script = (MonoBehaviour)EditorGUILayout.ObjectField("Script", action.script, typeof(MonoBehaviour), true);
    }

    void SetDuration(SequencerAction action, string label = "Duration")
    {
        action.duration = EditorGUILayout.FloatField(label, action.duration);
    }

    void SetPosition(SequencerAction action)
    {
        if (gcStartRecord == null)
        {
            gcStartRecord = new GUIContent("R", "Start Recording");
            gcStopRecord = new GUIContent("S", "Stop Recording");
            smallButtonLayout = GUILayout.Width(20);
        }

        if (action.gameObject != null)
        {
            EditorGUILayout.BeginHorizontal();
            if (action.isRecording)
            {
                action.position = EditorGUILayout.Vector3Field("Position", action.gameObject.transform.localPosition);
                if (GUILayout.Button(gcStopRecord, smallButtonLayout))
                {
                    action.isRecording = false;
                    action.gameObject.transform.localPosition = action.positionBeforeRecording;
                }
            }
            else
            {
                action.position = EditorGUILayout.Vector3Field("Position", action.position);
                if (GUILayout.Button(gcStartRecord, smallButtonLayout))
                {
                    action.positionBeforeRecording = action.gameObject.transform.localPosition;
                    action.isRecording = true;
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            action.position = EditorGUILayout.Vector3Field("Position", action.position);
        }
    }

    void SetName(SequencerAction action, string label = "Name")
    {
        action.target = EditorGUILayout.TextField(label, action.target);
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
