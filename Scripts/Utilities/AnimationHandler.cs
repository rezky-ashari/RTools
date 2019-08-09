using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace RTools
{
    [RequireComponent(typeof(Animator))]
    public class AnimationHandler : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>
        /// Animation state to play when clicked.
        /// </summary>
        public string onClickPlay;
        /// <summary>
        /// ID to send to the scene's click event.
        /// </summary>
        public string clickID;
        public UnityEvent onStart;
        public UnityEvent onComplete;

        public string[] animationStates;
        public int selectedState = 0;

        public string SelectedStateName
        {
            get
            {
                return animationStates[selectedState];
            }
        }

        Animator _animator;
        public Animator Animator
        {
            get
            {
                if (_animator == null) _animator = GetComponent<Animator>();
                return _animator;
            }
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void PlaySound(string name)
        {
            if (name.Contains("|"))
            {
                string[] names = name.Split('|');
                Resound.PlaySFX(names[UnityEngine.Random.Range(0, names.Length)].Trim());
            }
            else if (name.Contains(","))
            {
                string[] names = name.Split(',');
                for (int i = 0; i < names.Length; i++)
                {
                    Resound.PlaySFX(names[i].Trim());
                }
            }
            else
            {
                Resound.PlaySFX(name);
            }

        }

        void PlaySoundLoop(string name)
        {
            Resound.PlaySFX(name, true);
        }

        void StopAllSound()
        {
            Resound.StopSFX();
        }

        void StopSoundLoop(string name)
        {
            Resound.StopSFX(name);
        }

        void OnStartAnimation()
        {
            onStart.Invoke();
        }

        void OnCompleteAnimation()
        {
            onComplete.Invoke();
        }

        /// <summary>
        /// Play an animation state.
        /// </summary>
        /// <param name="stateName"></param>
        public void Play(string stateName)
        {
            Animator.Play(stateName);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!string.IsNullOrEmpty(onClickPlay) && onClickPlay != "(none)") Play(onClickPlay);
            if (!string.IsNullOrEmpty(clickID)) Scene.SendClickEvent(clickID);
        }

        public void SendSceneEvent(string eventName)
        {
            Scene.SendEvent(eventName);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// <para>Custom Inspector for AnimationHandler</para>
    /// Author: Rezky Ashari
    /// </summary>
    [CustomEditor(typeof(AnimationHandler))]
    public class AnimationHandlerEditor : Editor
    {

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

            if (animationHandler.animationStates == null)
            {
                animationHandler.animationStates = GetAnimationStates(animationHandler.Animator);
            }

            animationHandler.selectedState = EditorGUILayout.Popup("Play On Click", animationHandler.selectedState, animationHandler.animationStates);
            animationHandler.onClickPlay = animationHandler.SelectedStateName;
            animationHandler.clickID = EditorGUILayout.TextField(clickIDLabel, animationHandler.clickID);

            if (Application.isPlaying)
            {
                EditorGUILayout.BeginHorizontal();
                if (stateNames == null)
                {
                    int statesLength = animationHandler.animationStates.Length - 1;
                    stateNames = new string[statesLength];
                    Array.Copy(animationHandler.animationStates, 1, stateNames, 0, statesLength);
                }
                int selected = Array.IndexOf(stateNames, stateName);
                selected = EditorGUILayout.Popup("Play Animation", selected > 0 ? selected : 0, stateNames);
                stateName = stateNames[selected];
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
            string[] stateNames = new string[states.Length + 1];
            stateNames[0] = "(none)";
            for (int i = 0; i < states.Length; i++)
            {
                stateNames[i + 1] = states[i].state.name;
            }
            return stateNames;
        }
    }
#endif
}
