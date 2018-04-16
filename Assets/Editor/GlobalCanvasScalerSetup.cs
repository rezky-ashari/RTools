using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CanvasScalerConfig))]
public class GlobalCanvasScalerSetup : Editor {

    [MenuItem("Rezky Tools/Global Canvas Setup")]
    public static void ShowWindow()
    {
        //GetWindow(typeof(GlobalCanvasScalerSetup));
        Selection.activeObject = CanvasScalerConfig.Instance;
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Label("Global Canvas Scaler Configuration", EditorStyles.boldLabel);
        CanvasScalerConfig config = CanvasScalerConfig.Instance;

        config.renderMode = (RenderMode)EditorGUILayout.EnumPopup("Render Mode", config.renderMode);

        config.referenceWidth = EditorGUILayout.FloatField("Width", config.referenceWidth);
        config.referenceHeight = EditorGUILayout.FloatField("Height", config.referenceHeight);

        //EditorGUILayout.EnumMaskField("Match Mode", matchMode);

        //groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        config.matchMode = (CanvasScalerConfig.MatchMode)EditorGUILayout.EnumPopup("Match Orientation", config.matchMode);
        if (config.matchMode == CanvasScalerConfig.MatchMode.Custom)
        {
            config.match = EditorGUILayout.Slider("Match", config.match, 0, 1);
        }
        else
        {
            config.match = config.matchMode == CanvasScalerConfig.MatchMode.Portrait ? 1 : 0;
        }
        

        //EditorGUILayout.EndToggleGroup();

        if (GUI.changed) EditorUtility.SetDirty(config);
    }    
}
