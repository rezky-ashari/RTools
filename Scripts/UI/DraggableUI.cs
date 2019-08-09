using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace RTools
{
    /// <summary>
    /// <para>Make UI gameobject to be draggable.</para>
    /// Author: Rezky Ashari
    /// </summary>
    public class DraggableUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {

        /// <summary>
        /// Whether to disable drag for all dragable UIs.
        /// </summary>
        public static bool disableDrag = false;

        /// <summary>
        /// Current dragged gameobject.
        /// </summary>
        public static GameObject Current
        {
            get
            {
                return _current;
            }
        }
        static GameObject _current;

        public bool lockX = false;
        public bool lockY = false;

        public bool useBounds = false;
        public Rect dragBounds;
        public float boundLeft;
        public float boundRight;
        public float boundTop;
        public float boundBottom;

        public bool lockSiblingIndex = false;

        public bool backOnDrop = false;
        public Vector3 defaultPosition;

        public UnityEvent onDrag;
        public UnityEvent onDrop;

        /// <summary>
        /// Event listener when drag operation start.
        /// </summary>
        public event Action OnStartDrag;
        /// <summary>
        /// Event listener when drag operation end.
        /// </summary>
        public event Action OnEndDrag;
        /// <summary>
        /// Called when this draggable UI returned to it's default position.
        /// </summary>
        public event Action OnReturnedToDefaultPosition;

        Action onComplete;

        Canvas _parentCanvas;
        Vector3 offset;

        float planeDistance = 10f;
        float defaultZ = 0;
        int defaultSiblingIndex = 0;

        public float TopBound
        {
            get { return dragBounds.yMin; }
        }

        public float BottomBound
        {
            get { return dragBounds.yMax; }
        }

        public float RightBound
        {
            get { return dragBounds.xMin; }
        }

        public float LeftBound
        {
            get { return dragBounds.xMax; }
        }

        Canvas ParentCanvas
        {
            get
            {
                if (_parentCanvas == null)
                {
                    _parentCanvas = GetComponentInParent<Canvas>();
                }
                return _parentCanvas;
            }
        }

        bool IsOverlayMode
        {
            get
            {
                return ParentCanvas.renderMode == RenderMode.ScreenSpaceOverlay;
            }
        }

        private void Reset()
        {
            defaultPosition = transform.localPosition;
            defaultZ = transform.localPosition.z;
        }

        /// <summary>
        /// Attach a box collider to use with DropArea.
        /// </summary>
        public void AttachBoxCollider()
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            Rect transformRect = (transform as RectTransform).rect;
            collider.size = new Vector2(transformRect.width, transformRect.height);
        }

        // Use this for initialization
        void Start()
        {

            planeDistance = ParentCanvas.planeDistance;
        }

        // Update is called once per frame
        void Update()
        {
            if (useBounds)
            {
                if (!lockY)
                {
                    if (transform.localPosition.y > boundTop)
                    {
                        transform.localPosition = transform.localPosition.SetY(boundTop);
                    }
                    else if (transform.localPosition.y < boundBottom)
                    {
                        transform.localPosition = transform.localPosition.SetY(boundBottom);
                    }
                }
                if (!lockX)
                {
                    if (transform.localPosition.x > boundRight)
                    {
                        transform.localPosition = transform.localPosition.SetX(boundRight);
                    }
                    else if (transform.localPosition.x < boundLeft)
                    {
                        transform.localPosition = transform.localPosition.SetX(boundLeft);
                    }
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (disableDrag) return;

            if (IsOverlayMode)
            {
                offset = transform.position - Input.mousePosition;
            }
            else
            {
                offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }

            defaultSiblingIndex = transform.GetSiblingIndex();
            if (!lockSiblingIndex) transform.SetAsLastSibling();

            _current = gameObject;

            onDrag.Invoke();
            OnStartDrag?.Invoke();
        }

        public void UpdateDefaultPosition()
        {
            defaultPosition = transform.localPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (disableDrag) return;

            Vector3 target;
            if (IsOverlayMode)
            {
                target = Input.mousePosition + offset;
            }
            else
            {
                Vector3 screenPoint = Input.mousePosition;
                screenPoint.z = planeDistance;
                target = Camera.main.ScreenToWorldPoint(screenPoint) + offset;
            }
            if (lockX) target.x = transform.position.x;
            if (lockY) target.y = transform.position.y;
            transform.position = target.SetZ(defaultZ);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (disableDrag) return;

            onDrop.Invoke();
            OnEndDrag?.Invoke();
            if (backOnDrop) BackToDefaultPosition();
            _current = null;
        }

        public void BackToDefaultPosition(Action onComplete)
        {
            this.onComplete = onComplete;
            BackToDefaultPosition();
        }

        public void BackToDefaultPosition()
        {
            if (gameObject.activeInHierarchy)
            {
                RezTween tween = RezTween.MoveTo(gameObject, 0.5f, defaultPosition);
                tween.OnComplete = () =>
                {
                    onComplete?.Invoke();
                    OnReturnedToDefaultPosition?.Invoke();
                    BackToDefaultSiblingIndex();
                };
            }
            else
            {
                transform.localPosition = defaultPosition;
                onComplete?.Invoke();
                OnReturnedToDefaultPosition?.Invoke();
                BackToDefaultSiblingIndex();
            }
        }

        public void BackToDefaultSiblingIndex()
        {
            transform.SetSiblingIndex(defaultSiblingIndex);
        }

        private void OnDisable()
        {
            if (_current == gameObject) _current = null;
        }

        private void OnDestroy()
        {
            OnDisable();
        }

        /// <summary>
        /// Disable drag on this UI.
        /// </summary>
        public void LockMovements()
        {
            lockX = lockY = true;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Custom Inspector for DraggableUI
    /// </summary>
    [CustomEditor(typeof(DraggableUI))]
    [CanEditMultipleObjects]
    public class DraggableUIEditor : Editor
    {

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
            EditorGUILayout.PropertyField(lockY, new GUIContent("Lock Y", "Prevent drag on Y axes"));

            EditorGUILayout.PropertyField(useBounds, new GUIContent("Use Bounds", "Restrict drag within bounds"));
            if (draggableUI.useBounds)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(boundLeft, new GUIContent("Left"));
                EditorGUILayout.PropertyField(boundRight, new GUIContent("Right"));
                EditorGUILayout.PropertyField(boundTop, new GUIContent("Top"));
                EditorGUILayout.PropertyField(boundBottom, new GUIContent("Bottom"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(backOnDrop, new GUIContent("Back On Drop", "Tween back this UI to default position on drop"));
            if (draggableUI.backOnDrop)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(defaultPosition, new GUIContent("Default Position", "Position to tween back"));
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
#endif
}