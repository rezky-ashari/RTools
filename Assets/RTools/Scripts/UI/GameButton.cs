using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

namespace RTools
{
    /// <summary>
    /// <para>What you need for a game button.</para>
    /// Author: Rezky Ashari
    /// </summary>
    [ExecuteInEditMode]
    public class GameButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Serializable]
        public class OnClickHandler : UnityEvent<string> { };

        [Tooltip("Button ID that will be sent to Scene's click handler")]
        public string ID;

        [Tooltip("Whether this button is receiving click")]
        public bool interactable = true;

        [Tooltip("Prevent interactable state to change the image's alpha.")]
        public bool lockAlpha = false;

        [Tooltip("Whether to ignore click on transparent area of this button. You have to check the 'Read/Write Enabled' in image's Import Settings.")]
        public bool ignoreTransparentArea = false;

        [Tooltip("Ignore click when vertically dragged.")]
        public bool ignoreYDrag = false;

        [Tooltip("Ignore click when horizontally dragged.")]
        public bool ignoreXDrag = false;

        [Tooltip("Scale target to give a 'pressed' effect. Set to None if you want to disable scaling on pressed.")]
        public GameObject scaleTarget;

        [Tooltip("For group of buttons that required only one button to be selected. Attach 'SelectableGroup' component to SceneManager to get started.")]
        public SelectableGroup selectableGroup;

        [SerializeField]
        public OnClickHandler onClick;

        /// <summary>
        /// Called when this button is being held down (pressed).
        /// </summary>
        public Action onPress;

        /// <summary>
        /// Called when this button is released after pressed.
        /// </summary>
        public Action onRelease;

        bool ScaleOnDown
        {
            get
            {
                return scaleTarget != null;
            }
        }

        Vector3 defaultScale = Vector3.one;
        float currentAlpha = 1;
        float lastAlpha = 0;

        bool receiveRaycast = true;

        const float dragThreshold = 25;
        bool listenToMouseUp = false;
        float startY;
        float startX;

        /// <summary>
        /// Whether this button receives raycast.
        /// </summary>
        public bool RaycastTarget
        {
            get
            {
                return receiveRaycast;
            }
            set
            {
                receiveRaycast = value;
                ProcessImages(image =>
                {
                    image.raycastTarget = receiveRaycast;
                });
            }
        }

        void Reset()
        {
            ID = gameObject.name;
            scaleTarget = gameObject;
        }

        // Use this for initialization
        void Start()
        {
            if (Application.isPlaying)
            {
                if (ignoreTransparentArea)
                {
                    GetComponent<Image>().alphaHitTestMinimumThreshold = 0.01f;
                }
            }
            else
            {
                UpdateAlpha();
            }
        }

        private void UpdateAlpha()
        {
            if (!lockAlpha)
            {
                currentAlpha = interactable ? 1f : 0.5f;
                if (currentAlpha != lastAlpha)
                {
                    ProcessImages(image =>
                    {
                        Color currentColor = image.color;
                        image.color = new Color(currentColor.r, currentColor.g, currentColor.b, currentAlpha);
                    });
                    lastAlpha = currentAlpha;
                }
            }
        }

        /// <summary>
        /// Do operation for all button images.
        /// </summary>
        /// <param name="handler"></param>
        void ProcessImages(Action<Image> handler)
        {
            Image[] images = GetComponentsInChildren<Image>();
            for (int i = 0; i < images.Length; i++)
            {
                handler(images[i]);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Application.isPlaying)
            {
                if (listenToMouseUp && Input.GetMouseButtonUp(0))
                {
                    Vector3 mousePosition = Input.mousePosition;
                    if (ignoreYDrag)
                    {
                        float distanceY = Math.Abs(startY - mousePosition.y);
                        if (distanceY <= dragThreshold) ExecuteClick();
                    }
                    if (ignoreXDrag)
                    {
                        float distanceX = Math.Abs(startX - mousePosition.x);
                        if (distanceX <= dragThreshold) ExecuteClick();
                    }

                    listenToMouseUp = false;
                }
            }
            UpdateAlpha();
        }

        void UpdateInteractableState()
        {
            if (interactable)
            {
                if (currentAlpha != 1)
                {
                    UpdateAlpha();
                }
            }
            else
            {
                if (currentAlpha != 0)
                {
                    UpdateAlpha();
                }
            }
        }

        /// <summary>
        /// Send click event of this button.
        /// </summary>
        public void SendClickToScene()
        {
            sceneListener?.Invoke(ID);
            onClick.Invoke(ID);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!interactable) return;

            if (ScaleOnDown && scaleTarget != null)
            {
                defaultScale = scaleTarget.transform.localScale;
                ScaleTo(defaultScale * 0.95f);
            }

            onPress?.Invoke();
        }

        void ScaleTo(Vector3 scale)
        {
            scaleTarget.transform.localScale = scale;
        }

        void ExecuteClick()
        {
            if (selectableGroup != null) selectableGroup.SetSelected(this);
            SendClickToScene();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!interactable) return;
            if (ScaleOnDown && scaleTarget != null) ScaleTo(defaultScale);

            listenToMouseUp = ignoreYDrag || ignoreXDrag;
            if (listenToMouseUp)
            {
                startY = Input.mousePosition.y;
                startX = Input.mousePosition.x;
            }
            else
            {
                ExecuteClick();
            }

            onRelease?.Invoke();
        }

        #region Do not edit. For Scene Handler.
        static Action<string> sceneListener;
        public static void SetSceneListener(Action<string> listener)
        {
            sceneListener = listener;
        }
        #endregion
    }
}