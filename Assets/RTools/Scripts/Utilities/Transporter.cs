using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RTools
{
    /// <summary>
    /// <para>Save and manage game object's positions in lazy way.</para>
    /// Author: Rezky Ashari
    /// </summary>
    [ExecuteInEditMode]
    public class Transporter : MonoBehaviour
    {
        public int currentPosition = 0;

        public float moveDuration = 0.5f;
        public bool lockPositions = false;
        public List<Vector3> positions = new List<Vector3>();

        int lastPositionIndex = -1;
        bool isMovingByTween = false;
        string warningMessage = "";

        RezTween currentOperation;
        List<string> moveQueue = new List<string>();

        /// <summary>
        /// Action to execute when move progress complete. Automatically set to null after called once.
        /// </summary>
        public Action onCompleteMove;

        string controller = "";
        public string Controller
        {
            get { return controller; }
        }

        public bool HasController
        {
            get { return controller != ""; }
        }

        /// <summary>
        /// Move progress from 0 to 1.
        /// </summary>
        public float MoveProgress
        {
            get
            {
                if (currentOperation != null)
                {
                    return currentOperation.Progress;
                }
                return 0;
            }
        }

        /// <summary>
        /// Current warning message.
        /// </summary>
        public string WarningMessage { get { return warningMessage; } }

        /// <summary>
        /// Whether the current position is different from the saved position.
        /// </summary>
        public bool PositionNotSync
        {
            get { return transform.localPosition != positions[currentPosition]; }
        }

        // Use this for initialization
        void Start()
        {
            if (positions.Count == 0) AddCurrentPosition();
        }

        // Update is called once per frame
        public void Update()
        {
            if (positions.Count == 0) AddCurrentPosition();
            if (currentPosition < 0 || currentPosition >= positions.Count || isMovingByTween) return;

            if (currentPosition != lastPositionIndex)
            {
                ApplyCurrentPosition();
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (lockPositions)
                {
                    transform.localPosition = positions[currentPosition];
                }
                else
                {
                    positions[currentPosition] = transform.localPosition;
                }
            }

#endif
        }

        private void ApplyCurrentPosition()
        {
            if (currentPosition < 0 || currentPosition >= positions.Count)
            {
                Debug.LogError("Transporter can't move " + gameObject.name + " to position " + currentPosition + " because it is not defined!");
            }
            transform.localPosition = positions[currentPosition];
            lastPositionIndex = currentPosition;
        }

        public void RemoveCurrentPosition()
        {
            if (currentPosition > 0)
            {
                positions.RemoveAt(currentPosition);
                currentPosition--;
                Update();
                ResetWarningMessage();
            }
            else
            {
                warningMessage = "Can not remove the default position.";
            }
        }

        void ResetWarningMessage()
        {
            warningMessage = "";
        }

        public void AddCurrentPosition()
        {
            positions.Add(transform.localPosition);
            currentPosition = positions.Count - 1;
            ResetWarningMessage();
        }

        /// <summary>
        /// Move the game object to a specified position index.
        /// </summary>
        /// <param name="position">Target position index</param>
        /// <param name="duration">Move duration (in seconds). Value 0 will use the default move duration.</param>
        /// <param name="useTween">Whether to move by tween</param>
        public Transporter MoveTo(int position, bool useTween = true, float duration = 0)
        {

            if (useTween)
            {
                //isMovingByTween = true;
                //RezTween currentOperation = RezTween.MoveTo(gameObject, duration > 0? duration : moveDuration, positions[position]);
                //currentOperation.OnComplete = FinishedMoving;
                //moveQueue.Add(position + "|" + duration);
                //ToNextPosition();
                //return this;
            }
            else
            {
                //moveQueue.Add(position + "|-1");
                //ApplyCurrentPosition();
                //return null;
                duration = -1;
            }

            moveQueue.Add(position + "|" + duration);
            ToNextPosition();
            return this;
        }

        void ToNextPosition()
        {
            if (!isMovingByTween)
            {
                if (moveQueue.Count > 0)
                {
                    string[] queueData = moveQueue[0].Split('|');
                    moveQueue.RemoveAt(0);

                    int position = int.Parse(queueData[0]);
                    float duration = float.Parse(queueData[1]);
                    currentPosition = Mathf.Clamp(position, 0, positions.Count - 1);
                    if (duration == -1)
                    {
                        ApplyCurrentPosition();
                        ToNextPosition();
                    }
                    else if (lastPositionIndex != currentPosition)
                    {
                        currentOperation = RezTween.MoveTo(gameObject, duration > 0 ? duration : moveDuration, positions[currentPosition]);
                        currentOperation.OnComplete = FinishedMoving;
                        isMovingByTween = true;
                    }
                }
                else if (onCompleteMove != null)
                {
                    onCompleteMove();
                    onCompleteMove = null;
                }
            }
        }

        void FinishedMoving()
        {
            lastPositionIndex = currentPosition;
            isMovingByTween = false;
            ToNextPosition();
        }

        public void Pause()
        {
            if (currentOperation != null)
            {
                currentOperation.Pause();
            }
        }

        public void Resume()
        {
            if (currentOperation != null)
            {
                currentOperation.Resume();
            }
        }

        /// <summary>
        /// Set the move duration in seconds.
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public Transporter SetMoveDuration(float duration)
        {
            moveDuration = duration;
            return this;
        }

        public void PreventInspectorChanges(string controller)
        {
            this.controller = controller;
        }

        public void AllowInspectorChanges()
        {
            controller = "";
        }

        /// <summary>
        /// Move through all positions.
        /// </summary>
        public void MoveThrougAllPositions(Action onComplete = null)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                MoveTo(i, i != 0);
            }
            onCompleteMove = onComplete;
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(Transporter))]
    [CanEditMultipleObjects]
    public class TransporterEditor : Editor
    {

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

            EditorGUI.BeginDisabledGroup(transporter.HasController);
            EditorGUILayout.PropertyField(currentPosition);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(moveDuration);
            EditorGUILayout.PropertyField(lockPositions);

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

            if (!Application.isPlaying && transporter.enabled) transporter.Update();
        }
    }
#endif
}
