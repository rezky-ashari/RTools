using System;
using System.Collections;
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
    static GUIStyle labelStyleHighligted;
    static GUIContent gcUpButton;
    static GUIContent gcDownButton;
    static GUIContent gcLinkedPaste;
    static GUIContent gcLinkedCopy;
    static GUIContent gcLinkedDelete;

    static SequencerAction clipboard;

    Vector2 scrollViewPosition;

    readonly Color normalColor = new Color(0f, 0f, 0f, 0.25f);
    readonly Color breakColor = new Color(0.5f, 0f, 0f, 0.2f);

    SerializedProperty onBreak;

    int focusIndex = -1;
    int newActionPosition = -1;
    int highlightedActionIndex = 0;

    [MenuItem("GameObject/Sequencer", false, 10)]
    static void CreateSequencerObject()
    {
        GameObject gameObject = new GameObject("Sequence");
        gameObject.AddComponent<Sequencer>();
        if (Selection.activeGameObject != null)
        {
            gameObject.transform.SetParent(Selection.activeTransform);
        }
        Selection.activeGameObject = gameObject;
    }

    private void InitializeStyles()
    {
        if (labelStyle == null)
        {
            labelStyle = "Foldout";
            labelStyle.wordWrap = true;
            labelStyleHighligted = new GUIStyle(labelStyle);
            labelStyleHighligted.normal.textColor = Color.yellow;

            smallButtonLayout = GUILayout.Width(20);

            gcUpButton = new GUIContent("▲", "Move this action up");
            gcDownButton = new GUIContent("▼", "Move this action down");
            gcLinkedCopy = new GUIContent("C", "Copy this linked action");
            gcLinkedPaste = new GUIContent("P", "Paste to this linked action");
            gcLinkedDelete = new GUIContent("X", "Delete this linked action");
        }

        //if (onBreak == null)
        //{
        //    onBreak = serializedObject.FindProperty("onBreak");
        //}
    }

    public override void OnInspectorGUI()
    {
        Sequencer seq = (Sequencer)target;
        
        EditorGUILayout.Space();

        InitializeStyles();

        if (Application.isPlaying)
        {
            seq.isPlaying = EditorGUILayout.Toggle("Is Playing", seq.isPlaying);
            EditorGUILayout.IntSlider("Playing Action", seq.currentAct, 0, seq.actions.Count);
        }
        else
        {
            seq.currentAct = EditorGUILayout.IntField("Starting Action", seq.currentAct);
            if (seq.currentAct < 0) seq.currentAct = 0;
            if (seq.currentAct > seq.actions.Count) seq.currentAct = seq.actions.Count;
        }

        seq.playOnAwake = EditorGUILayout.ToggleLeft("Play On Awake", seq.playOnAwake);
        seq.debugLog = EditorGUILayout.ToggleLeft("Debug Log", seq.debugLog);

        //EditorGUILayout.PropertyField(onBreak);

        scrollViewPosition = EditorGUILayout.BeginScrollView(scrollViewPosition);
        
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

            if (seq.IsOnProgress && highlightedActionIndex < seq.currentAct - 1)
            {
                highlightedActionIndex++;
            }

            action.active = EditorGUILayout.Toggle("", action.active, GUILayout.Width(12));
            action.foldout = GUILayout.Toggle(action.foldout, label, Application.isPlaying && i == highlightedActionIndex? labelStyleHighligted : labelStyle, GUILayout.ExpandWidth(true));

            if (i < seq.actions.Count - 1 && GUILayout.Button(gcDownButton, smallButtonLayout))
            {
                SequencerActionParent temp = seq.actions[i + 1];
                seq.actions[i + 1] = action;
                seq.actions[i] = temp;
                focusIndex = i + 1;
                break;
            }
            if (i > 0 && GUILayout.Button(gcUpButton, smallButtonLayout))
            {
                SequencerActionParent temp = seq.actions[i - 1];
                seq.actions[i - 1] = action;
                seq.actions[i] = temp;
                focusIndex = i - 1;
                break;
            }

            EditorGUILayout.EndHorizontal();

            bool isBreakMode = action.mode == SequencerActionMode.Break;
            if (!isBreakMode && !action.foldout)
            {
                for (int linkedIdx = 0; linkedIdx < action.linkedActions.Count; linkedIdx++)
                {
                    if (action.linkedActions[linkedIdx].mode == SequencerActionMode.Break)
                    {
                        isBreakMode = true;
                        break;
                    }
                }
            }

            Vector3 mousePos = Event.current.mousePosition;
            Rect lastRect = GUILayoutUtility.GetLastRect();
            Color targetColor = isBreakMode ? breakColor : normalColor;
            action.color = Color.Lerp(action.color, targetColor, 0.15f);
            EditorGUI.DrawRect(lastRect, action.isRecording? Color.red : action.color);

            // Scroll to the newly created action, while wait for the end of frame (lastRect position != Vector2.Zero)
            if (focusIndex == i && lastRect.position.x != 0)
            {
                scrollViewPosition = lastRect.position;
                focusIndex = -1;
                action.color = Color.green;
            }
            

            //if (i == 0 && lastRect.Contains(mousePos))
            //{
            //    Debug.Log("Mouse is on me. Last Rect: " + lastRect + ", Mouse Position: " + mousePos);
            //}

            if (action.foldout)
            {
                EditorGUI.BeginDisabledGroup(!action.active);
                EditorGUI.indentLevel++;

                // Parent Control
                DrawControl(action);

                EditorGUI.EndDisabledGroup();
                
                // Parent Buttons
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (clipboard != null)
                {
                    if (GUILayout.Button("Paste"))
                    {
                        Undo.RecordObject(seq, "Paste");
                        action.Paste(clipboard);
                        clipboard = null;
                    }
                }
                else
                {
                    if (GUILayout.Button("Copy"))
                    {
                        clipboard = action.Copy();
                    }
                }

                if (GUILayout.Button("Add Linked Action"))
                {
                    action.linkedActions.Add(new SequencerAction());
                }
                if (GUILayout.Button("Delete"))
                {
                    Undo.RecordObject(seq, "Delete Sequence Action");
                    seq.actions.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                // Linked Actions
                SequencerAction linkedAct;
                for (int j = 0; j < action.linkedActions.Count; j++)
                {
                    EditorGUILayout.Space();
                    linkedAct = action.linkedActions[j];
                    EditorGUILayout.BeginHorizontal();
                    label = "Linked #" + j;
                    if (!linkedAct.foldout)
                    {
                        label += ": " + linkedAct.ModeInfo;
                    }
                    linkedAct.foldout = GUILayout.Toggle(linkedAct.foldout, label, labelStyle, GUILayout.ExpandWidth(true));
                    if (clipboard != null)
                    {
                        if (GUILayout.Button(gcLinkedPaste, smallButtonLayout))
                        {
                            Undo.RecordObject(seq, "Paste");
                            linkedAct.Paste(clipboard);
                            clipboard = null;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(gcLinkedCopy, smallButtonLayout))
                        {
                            clipboard = linkedAct.Copy();
                        }
                    }
                    if (GUILayout.Button(gcLinkedDelete, GUILayout.Width(20), GUILayout.Height(17)))
                    {
                        Undo.RecordObject(seq, "Delete Sequence Action");
                        action.linkedActions.RemoveAt(j);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.BeginDisabledGroup(!action.active);

                    Color linkedColor = linkedAct.mode == SequencerActionMode.Break? breakColor : normalColor;
                    linkedColor.a -= 0.1f;
                    EditorGUI.DrawRect(GUILayoutUtility.GetLastRect(), linkedColor);

                    if (linkedAct.foldout)
                    {
                        DrawControl(linkedAct);
                        if (linkedAct.mode == SequencerActionMode.Wait)
                        {
                            EditorGUILayout.HelpBox("Wait mode in linked action will have no effect.", MessageType.Warning);
                        }
                    }
                }

                EditorGUI.indentLevel--;
                EditorGUI.EndDisabledGroup();
            }            
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        // Bottom toolbar
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("Action #", GUILayout.Width(50));
        newActionPosition = EditorGUILayout.IntField(newActionPosition >= 0 && newActionPosition < seq.actions.Count? newActionPosition : seq.actions.Count, GUILayout.Width(30));
        if (GUILayout.Button("Insert"))
        {
            InsertNewTo(seq, newActionPosition);
            if (newActionPosition == seq.actions.Count - 1) newActionPosition++;
        }
        if (GUILayout.Button("To First"))
        {
            InsertNewTo(seq, 0);
        }
        if (GUILayout.Button("To Last"))
        {
            InsertNewTo(seq, seq.actions.Count);
        }
        EditorGUILayout.EndHorizontal();

        if (Application.isPlaying)
        {
            
        }
        else if (GUI.changed)
        {
            EditorSceneManager.MarkAllScenesDirty();
        }
    }

    void InsertNewTo(Sequencer seq, int index)
    {
        SequencerActionParent actionParent = new SequencerActionParent();
        seq.actions.Insert(index, actionParent);
        focusIndex = index;
    }

    void DrawControl(SequencerAction action)
    {
        action.mode = (SequencerActionMode)EditorGUILayout.EnumPopup("Mode", action.mode);
        switch (action.mode)
        {
            case SequencerActionMode.Break:
                EditorGUILayout.HelpBox("Temporarily stop the sequence here.\nCall 'Play' from script to resume the sequence.", MessageType.Info);
                break;
            case SequencerActionMode.Wait:
                SetDuration(action);
                break;
            case SequencerActionMode.MoveTo:
                SetGameObject(action);
                SetPosition(action);
                SetDuration(action, "Move Duration");
                if (action.duration > 0) SetEase(action);
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
                        if (action.animator != action.lastAnimator || action.Options == null || action.Options.Length == 0)
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
                    EditorGUILayout.TextField("Function", action.target + "(" + action.GetParameterValue() + ")");
                }
                else
                {
                    if (action.gameObject != null)
                    {
                        //SetScript(action);
                        if (action.lastGameObject != action.gameObject || action.Options == null || action.Options.Length == 0 || action.script == null)
                        {
                            action.Options = GetScriptsAttached(action.gameObject);
                            action.lastGameObject = action.gameObject;
                        }
                        action.selected = EditorGUILayout.Popup("Script", action.selected, action.Options);
                        action.script = action.gameObject.GetComponent(action.SelectedValue);
                        if (action.script != null)
                        {
                            string[] methods = GetMethods(action.script);
                            if (methods.Length > 0)
                            {
                                string targetName = string.Format("{0}({1})", action.target, action.paramType);
                                int findSelectedIdx = Array.FindIndex(methods, m => m == targetName);
                                action.selectedMethod = Mathf.Clamp(EditorGUILayout.Popup("Function", findSelectedIdx /*action.selectedMethod*/, methods), 0, methods.Length);
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
                                    case "boolean":
                                        SetBoolParameter(action);
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
                    }
                    else
                    {
                        action.lastGameObject = null;
                        action.selected = 0;
                    }
                }                
                break;
            case SequencerActionMode.SetActive:
                SetGameObject(action);
                SetBoolParameter(action, "Active");
                break;
            case SequencerActionMode.SetAlpha:
                SetGameObject(action);
                action.paramFloat = EditorGUILayout.Slider("Alpha", action.paramFloat, 0, 1);
                SetDuration(action);
                break;
            case SequencerActionMode.SetScale:
                SetGameObject(action);
                action.position = EditorGUILayout.Vector3Field("Scale", action.position);
                SetDuration(action);
                break;
        }
    }

    private static void SetBoolParameter(SequencerAction action, string label = "Parameter")
    {
        EditorGUILayout.BeginHorizontal();
        action.paramBool = EditorGUILayout.Toggle(label, action.paramBool, GUILayout.ExpandWidth(false));
        EditorGUILayout.LabelField(action.paramBool ? "true" : "false");
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
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

    string[] GetMethods(Component mono)
    {
        var typeWithMethods = mono.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);

        List<string> methodList = new List<string>();
        ParameterInfo[] parameterInfo;
        foreach (var function in typeWithMethods)
        {
            parameterInfo = function.GetParameters();
            if (function.ReturnParameter.ParameterType.Name == "Void" && function.GetParameters().Length <= 1)
            {
                string paramType = parameterInfo.Length == 1 ? parameterInfo[0].ParameterType.Name.ToLower() : "";

                if (paramType.StartsWith("int")) paramType = "int";
                else if (paramType == "single") paramType = "float";

                switch (paramType)
                {
                    case "":
                    case "int":
                    case "float":
                    case "string":
                    case "boolean":
                        methodList.Add(function.Name + "(" + paramType + ")");
                        break;
                }                
            }            
        }
        return methodList.ToArray();
    }

    static string[] GetAnimationStates(Animator animator)
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
                    Selection.activeGameObject = action.gameObject;
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

    readonly string[] easeList =
    {
        "Linear", "Back In", "Back Out", "Spring"
    };

    void SetEase(SequencerAction action)
    {
        action.paramInt = EditorGUILayout.Popup("Ease", action.paramInt, easeList);
        switch (easeList[action.paramInt])
        {
            case "Back In":
                action.paramString = RezTweenEase.BACK_IN;
                break;
            case "Back Out":
                action.paramString = RezTweenEase.BACK_OUT;
                break;
            case "Spring":
                action.paramString = RezTweenEase.SPRING;
                break;
            default:
                action.paramString = RezTweenEase.LINEAR;
                break;
        }
    }
}
