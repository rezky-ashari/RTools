using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RTools
{
    /// <summary>
    /// <para>
    /// Setup window for GlobalCanvasScaler.
    /// Menu created:
    /// 1. RTools > Global Canvas Setup,
    /// 2. GameObject > UI > Game Canvas.
    /// </para>
    /// Author: Rezky Ashari
    /// </summary>
    public class GlobalCanvasScalerSetup : EditorWindow
    {

        [MenuItem("RTools/Global Canvas Setup", priority = 1)]
        public static void ShowWindow()
        {
            GetWindow(typeof(GlobalCanvasScalerSetup), false, "Canvas Setup");
        }

        [MenuItem("GameObject/UI/Game Canvas")]
        public static void AddGameCanvas()
        {
            GameObject gameObject = new GameObject("GameCanvas");
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

            Selection.activeGameObject = gameObject;
        }

        public void OnGUI()
        {
            GUILayout.Label("Global Canvas Scaler Configuration", EditorStyles.boldLabel);
            CanvasScalerConfig config = CanvasScalerConfig.Instance;

            config.renderMode = (RenderMode)EditorGUILayout.EnumPopup("Render Mode", config.renderMode);

            config.referenceWidth = EditorGUILayout.FloatField("Width", config.referenceWidth);
            config.referenceHeight = EditorGUILayout.FloatField("Height", config.referenceHeight);

            config.matchMode = (CanvasScalerConfig.MatchMode)EditorGUILayout.EnumPopup("Match Orientation", config.matchMode);
            if (config.matchMode == CanvasScalerConfig.MatchMode.Custom)
            {
                config.match = EditorGUILayout.Slider("Match", config.match, 0, 1);
            }
            else
            {
                config.match = config.matchMode == CanvasScalerConfig.MatchMode.Portrait ? 0 : 1;
            }

            EditorGUILayout.HelpBox("Add 'GlobalCanvasScaler' component to a Canvas object to apply these settings.", MessageType.None);

            if (GUI.changed) EditorUtility.SetDirty(config);
        }
    }
}
