using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// <para>Global canvas scaler. Add this script to every canvas that need to be set based on the desired game resolution.</para>
/// Author: Rezky Ashari
/// </summary>
[ExecuteInEditMode]
public class GlobalCanvasScaler : MonoBehaviour {

    [HideInInspector]
    public RenderMode renderMode;
    //[Range(0, 4000), HideInInspector]
    //public float referenceWidth = 1024;
    //[Range(0, 4000), HideInInspector]
    //public float referenceHeight = 720;
    //[Range(0, 1), HideInInspector]
    //public float match = 1;

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
    Canvas CanvasComponent { 
        get
        {
            if (_canvas == null) _canvas = GetComponent<Canvas>();
            return _canvas;
        }
    }

    private void Reset()
    {
        Update();
    }

    // Use this for initialization
    void Start () {
        //scalerList.AddIfNotExists(this);
        Reset();
	}
	
	// Update is called once per frame
	void Update () {
        if (!Application.isPlaying)
        {
            CanvasScalerConfig config = CanvasScalerConfig.Instance;
            
            Scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            Scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            Scaler.referenceResolution = config.ReferenceResolution;
            Scaler.matchWidthOrHeight = config.match;

            CanvasComponent.renderMode = renderMode = config.renderMode;
        }
        
        if (renderMode == RenderMode.ScreenSpaceCamera && CanvasComponent.worldCamera == null)
        {
            CanvasComponent.worldCamera = Camera.main;
        }

    }
}
