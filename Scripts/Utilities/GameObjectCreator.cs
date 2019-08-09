#if UNITY_EDITOR
using System.Reflection;
using UnityEditor; 
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RTools
{
    /// <summary>
    /// <para>
    /// Handy class for creating preset gameObjects.
    /// Menu created:
    /// 1. GameObject > UI > Game Canvas,
    /// 2. GameObject > UI > Game Button,
    /// 3. GameObject > UI > Popup Dialog.
    /// </para>
    /// Author: Rezky Ashari
    /// </summary>
    public class GameObjectCreator
    {
        #region Rename Utility
        private static double renameTime;

        private static void EngageRenameMode()
        {
            if (EditorApplication.timeSinceStartup >= renameTime)
            {
                EditorApplication.update -= EngageRenameMode;
                EditorWindow.focusedWindow.SendEvent(new Event() { keyCode = KeyCode.F2, type = EventType.KeyDown });
            }
        }

        /// <summary>
        /// Execute the rename mode.
        /// </summary>
        /// <param name="target">GameObject to rename.</param>
        private static void Rename(GameObject target)
        {
            Selection.activeGameObject = target;
            renameTime = EditorApplication.timeSinceStartup + 0.2d;
            EditorApplication.update += EngageRenameMode;
        }
        #endregion

        #region Game Canvas
#if UNITY_EDITOR
        [MenuItem("GameObject/UI/Game Canvas")]
        static void AddGameCanvas()
        {
            GameObject gameObject = CreateGameCanvas("GameCanvas");
            Selection.activeGameObject = gameObject;
        }
#endif
        /// <summary>
        /// Create a UI canvas.
        /// </summary>
        /// <param name="name">Canvas name</param>
        /// <returns></returns>
        public static GameObject CreateGameCanvas(string name = "GameCanvas")
        {
            GameObject gameObject = new GameObject(name);
            gameObject.AddComponent<Canvas>();
            gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<GraphicRaycaster>();
            gameObject.AddComponent<GlobalCanvasScaler>();

            if (GameObject.Find("EventSystem") == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }

            return gameObject;
        }
        #endregion

        #region Game Button
#if UNITY_EDITOR
        [MenuItem("GameObject/UI/Game Button", false, 11)]
        static void AddGameButton()
        {
            GameObject buttonObject = CreateGameButton("GameButton");

            if (Selection.activeGameObject != null)
            {
                buttonObject.transform.SetParent(Selection.activeGameObject.transform, false);
            }
            else
            {
                Canvas parentCanvas = Object.FindObjectOfType<Canvas>();
                if (parentCanvas != null)
                {
                    buttonObject.transform.SetParent(parentCanvas.transform, false);
                }
            }
            
            Rename(buttonObject);
        }
#endif
        /// <summary>
        /// Create an image button with GameButton component attached.
        /// </summary>
        /// <param name="name">Button name</param>
        /// <param name="parent">Parent to attach to</param>
        /// <returns></returns>
        public static GameObject CreateGameButton(string name, GameObject parent = null)
        {
            GameObject buttonObject = new GameObject(name);
            buttonObject.AddComponent<RectTransform>();
            buttonObject.AddComponent<Image>();
            buttonObject.AddComponent<GameButton>();

            if (parent != null) buttonObject.SetParent(parent, false);

            return buttonObject;
        }
        #endregion

        #region Popup Dialog
#if UNITY_EDITOR
        [MenuItem("GameObject/UI/Popup Dialog")]
        static void AddPopupDialog()
        {
            Canvas canvas = null;

            if (Selection.activeGameObject != null)
            {
                canvas = Selection.activeGameObject.GetComponentInParent<Canvas>();
            }

            if (canvas == null)
            {
                canvas = Object.FindObjectOfType<Canvas>();
            }

            if (canvas == null)
            {
                canvas = CreateGameCanvas().GetComponent<Canvas>();
            }

            GameObject popup = CreatePopupDialog("Popup Dialog");
        }
#endif
        public static GameObject CreatePopupDialog(string name)
        {
            GameObject popup = new GOCreatorChainable(name, true)
                .AddComponent<Image>()
                .AddComponent<PopupDialog>()
                .gameObject;

            return popup;
        }
        #endregion
    }

    /// <summary>
    /// <para>Chainable Game Object Creator. Useful for quick game object creation.</para>
    /// Author: Rezky Ashari
    /// </summary>
    public class GOCreatorChainable
    {
        public GameObject gameObject;

        /// <summary>
        /// Create a new chainable game object creator.
        /// </summary>
        /// <param name="name"></param>
        public GOCreatorChainable(string name, bool useRectTransform = false)
        {
            gameObject = new GameObject(name);
            if (useRectTransform) gameObject.AddComponent<RectTransform>();
        }

        public GOCreatorChainable AddComponent<T>()
        {
            gameObject.AddComponent(typeof(T));
            return this;
        }

        public GOCreatorChainable AddComponent<T>(out T component) where T : Component
        {
            component = (T)gameObject.AddComponent(typeof(T));
            return this;
        }

        public GOCreatorChainable SetParent(GameObject parent, bool worldPositionStays = true)
        {
            return SetParent(parent.transform, worldPositionStays);
        }

        public GOCreatorChainable SetParent(Transform parent, bool worldPositionStays = true)
        {
            gameObject.transform.SetParent(parent, worldPositionStays);
            return this;
        }

        public GOCreatorChainable SetRectTransformPivot(Vector2 min, Vector2 max)
        {
            RectTransform rectTransform = gameObject.transform as RectTransform;
            rectTransform.anchorMin = min;
            rectTransform.anchorMax = max;
            return this;
        }

        public static GOCreatorChainable Create(string name, bool useRectTransform = false)
        {
            return new GOCreatorChainable(name, useRectTransform);
        }
    }
}
