using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Rezky's debugger utility.
/// </summary>
public class DebugR {

    static Text _debugDisplay;
    static Text DebugDisplay
    {
        get
        {
            if (_debugDisplay == null)
            {
                InitializeDebugDisplay();
            }
            return _debugDisplay;
        }
    }

    private static void InitializeDebugDisplay()
    {
        GameObject canvasObject = new GameObject("DebugR Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();

        GameObject.DontDestroyOnLoad(canvasObject);

        GameObject panel = new GameObject("Panel");
        panel.AddComponent<CanvasGroup>().interactable = false;
        panel.AddComponent<Image>().color = new Color(0, 0, 0, 0.2f);
        panel.transform.ToRectTransform().anchorMin = new Vector2(0, 1);
        panel.transform.ToRectTransform().anchorMax = new Vector2(1, 1);
        panel.transform.ToRectTransform().pivot = new Vector2(0.5f, 1);
        
        panel.SetParent(canvasObject, false);

        Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        GameObject text = new GameObject("Text");
        _debugDisplay = text.AddComponent<Text>();
        _debugDisplay.fontSize = 20;
        _debugDisplay.font = ArialFont;
        _debugDisplay.material = ArialFont.material;
        text.transform.ToRectTransform().pivot = new Vector2(0, 1);
        text.transform.ToRectTransform().anchorMin = new Vector2(0, 1);
        text.transform.ToRectTransform().anchorMax = new Vector2(1, 1);
        text.transform.ToRectTransform().anchoredPosition = new Vector2(100, 0);
        text.SetParent(panel, false);
    }

    /// <summary>
    /// Log to screen.
    /// </summary>
    /// <param name="text"></param>
    public static void Log(string text)
    {
        DebugDisplay.text = text;
    }
}
