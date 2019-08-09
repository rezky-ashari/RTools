using UnityEngine;
using UnityEngine.UI;

namespace RTools
{
    /// <summary>
    /// <para>Global canvas scaler. Add this script to every canvas that need to be set based on the desired game resolution.</para>
    /// Author: Rezky Ashari
    /// </summary>
    [ExecuteInEditMode, RequireComponent(typeof(Canvas)), RequireComponent(typeof(CanvasScaler))]
    public class GlobalCanvasScaler : MonoBehaviour
    {

        CanvasScaler canvasScaler;
        CanvasScaler Scaler
        {
            get
            {
                if (canvasScaler == null) canvasScaler = GetComponent<CanvasScaler>();
                return canvasScaler;
            }
        }

        Canvas _canvas;
        Canvas CanvasComponent
        {
            get
            {
                if (_canvas == null) _canvas = GetComponent<Canvas>();
                return _canvas;
            }
        }

        static Camera _mainCamera;
        static Camera MainCamera
        {
            get
            {
                if (_mainCamera == null)
                {
                    _mainCamera = Camera.main;
                }
                return _mainCamera;
            }
        }

        private void Reset()
        {
            Update();
        }

        // Use this for initialization
        void Start()
        {
            //scalerList.AddIfNotExists(this);
            if (Application.isPlaying)
            {
                ApplyGlobalSettings();
            }
            else
            {
                Reset();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!Application.isPlaying)
            {
                ApplyGlobalSettings();
            }
        }

        void ApplyGlobalSettings()
        {
            CanvasScalerConfig config = CanvasScalerConfig.Instance;

            Scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            Scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            Scaler.referenceResolution = config.ReferenceResolution;
            Scaler.matchWidthOrHeight = config.match;

            CanvasComponent.renderMode = config.renderMode;

            if (config.renderMode == RenderMode.ScreenSpaceCamera && CanvasComponent.worldCamera == null)
            {
                CanvasComponent.worldCamera = MainCamera;
            }
        }
    }
}
