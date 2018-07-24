using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;


[CustomEditor(typeof(AnimationHandler))]
public class AnimationHandlerEditor : Editor {

    static string stateName;

    GUIContent onClickPlayLabel;
    GUIContent clickIDLabel;
    GUIContent onStartLabel;
    GUIContent onCompleteLabel;
    SerializedProperty onStartProperty;
    SerializedProperty onCompleteProperty;

    string[] stateNames;

    private void OnEnable()
    {
        onClickPlayLabel = new GUIContent("Play On Click", "Animation state to play when clicked.");
        clickIDLabel = new GUIContent("Click ID", "ID to send to the scene's click event.");
        onStartLabel = new GUIContent("On Start", "Methods to execute when 'OnStartAnimation' is called from AnimationEvent");
        onCompleteLabel = new GUIContent("On Complete", "Methods to execute when 'OnCompleteAnimation' is called from AnimationEvent");
        onStartProperty = serializedObject.FindProperty("onStart");
        onCompleteProperty = serializedObject.FindProperty("onComplete");
    }

    public override void OnInspectorGUI()
    {
        AnimationHandler animationHandler = (AnimationHandler)target;

        animationHandler.onClickPlay = EditorGUILayout.TextField(onClickPlayLabel, animationHandler.onClickPlay);
        animationHandler.clickID = EditorGUILayout.TextField(clickIDLabel, animationHandler.clickID);

        if (Application.isPlaying)
        {
            EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.LabelField("State Name", GUILayout.Width(80));
            if (stateNames == null)
            {
                stateNames = GetAnimationStates(animationHandler.Animator);
            }
            int selected = System.Array.IndexOf(stateNames, stateName);
            selected = EditorGUILayout.Popup("Play Animation", selected > 0? selected : 0, stateNames);
            stateName = stateNames[selected];  //EditorGUILayout.TextField("Play Animation", stateName);
            if (GUILayout.Button("Play", GUILayout.Width(60)))
            {
                animationHandler.Play(stateName);
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.HelpBox("You can test playing an animation in Play mode.", MessageType.Info);
        }

        EditorGUILayout.PropertyField(onStartProperty, onStartLabel);
        EditorGUILayout.PropertyField(onCompleteProperty, onCompleteLabel);

        serializedObject.ApplyModifiedProperties();
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
}
