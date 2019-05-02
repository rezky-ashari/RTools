using UnityEditor;
using UnityEngine;

namespace RTools
{
    /// <summary>
    /// <para>
    /// Centralized settings for game.
    /// Menu created: RTools > Game Setup
    /// </para>
    /// Author: Rezky Ashari
    /// </summary>
    public class GameSetupEditor : EditorWindow
    {

        string oldVersion, versionMessage, message;
        int oldVersionCode;
        MessageType versionMessageType;
        bool versionUpdated = false;

        [MenuItem("RTools/Game Setup", priority = 1)]
        static void ShowWindow()
        {
            GetWindow(typeof(GameSetupEditor), false, "Game Setup");
        }

        private void OnGUI()
        {
            GameSetup setup = GameSetup.Instance;

            #region Set default values when app ID is not set
            if (string.IsNullOrEmpty(setup.appID) && !string.IsNullOrEmpty(PlayerSettings.applicationIdentifier))
            {
                setup.appID = PlayerSettings.applicationIdentifier;
                setup.appName = PlayerSettings.productName;
                setup.appVersion = PlayerSettings.bundleVersion;
                setup.appVersionCode = PlayerSettings.Android.bundleVersionCode;
                setup.companyName = PlayerSettings.companyName;

                Texture2D[] icons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Unknown);
                if (icons.Length > 1)
                {
                    setup.icon = icons[0];
                }
            }
            #endregion

            EditorGUILayout.Space();

            setup.appID = EditorGUILayout.TextField("App ID", setup.appID);
            setup.appName = EditorGUILayout.TextField("Title", setup.appName);

            Horizontal(() =>
            {
                Vertical(() =>
                {
                    setup.appVersion = EditorGUILayout.TextField("Version", setup.appVersion);
                    setup.appVersionCode = EditorGUILayout.IntField("Version Code", setup.appVersionCode);
                });
                if (GUILayout.Button("+", GUILayout.Height(30)))
                {
                    Undo.RecordObject(setup, "Update version");
                    UpdateVersion(setup);
                }
            });
            if (!string.IsNullOrEmpty(versionMessage))
            {
                EditorGUILayout.HelpBox(versionMessage, versionMessageType);
                if (oldVersion == setup.appVersion) versionMessage = "";
            }

            setup.companyName = EditorGUILayout.TextField("Company Name", setup.companyName);

            DisplayLabel("Icons");
            Horizontal(() =>
            {
                setup.icon = TextureField("Default", setup.icon); //(Texture2D)EditorGUILayout.ObjectField("Icon", setup.icon, typeof(Texture2D), false);
                setup.roundIcon = TextureField("Round", setup.roundIcon);
                setup.adaptiveIconForeground = TextureField("Adapt. FG", setup.adaptiveIconForeground);
                setup.adaptiveIconBackground = TextureField("Adapt. BG", setup.adaptiveIconBackground);
            });

            if (!string.IsNullOrEmpty(message) && !versionUpdated) EditorGUILayout.HelpBox(message, MessageType.Info);

            if (GUILayout.Button("Apply Changes"))
            {
                versionMessage = "";
                versionUpdated = false;

                PlayerSettings.productName = setup.appName;
                PlayerSettings.applicationIdentifier = setup.appID;
                PlayerSettings.companyName = setup.companyName;
                PlayerSettings.bundleVersion = setup.appVersion;
                PlayerSettings.Android.bundleVersionCode = setup.appVersionCode;
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { setup.icon });
                SetIcons(UnityEditor.Android.AndroidPlatformIconKind.Adaptive, setup.adaptiveIconBackground, setup.adaptiveIconForeground);
                SetIcons(UnityEditor.Android.AndroidPlatformIconKind.Round, setup.roundIcon);

                message = System.DateTime.Now + ". Your changes has been applied.";
            }

            if (GUI.changed) EditorUtility.SetDirty(setup);
        }

        private void UpdateVersion(GameSetup setup)
        {
            oldVersion = setup.appVersion;
            string[] bundleVer = oldVersion.Split('.');
            if (bundleVer.Length == 0)
            {
                versionMessage = "Bundle version is not in valid format. Example of valid format: '1.0.2' or '3.0'";
                versionMessageType = MessageType.Error;
            }
            else
            {
                bool incrementValue = true;
                for (int i = bundleVer.Length - 1; i >= 0; i--)
                {
                    int versionNumber = int.Parse(bundleVer[i]);
                    versionNumber++;

                    if (i > 0 && versionNumber > 9)
                        versionNumber = 0;
                    else
                        incrementValue = false;

                    bundleVer[i] = versionNumber.ToString();
                    if (!incrementValue) break;
                }

                setup.appVersion = string.Join(".", bundleVer);

                oldVersionCode = setup.appVersionCode;
                int buildNumber = oldVersionCode + 1;
                setup.appVersionCode = buildNumber;

                versionMessage = string.Format("Version updated from {0} to {1}. Build Version Code updated from {2} to {3}. Click 'Apply Changes' to use this version.", oldVersion, setup.appVersion, oldVersionCode, setup.appVersionCode);
                versionMessageType = MessageType.Info;

                versionUpdated = true;
            }
        }

        void Horizontal(System.Action inside)
        {
            EditorGUILayout.BeginHorizontal();
            inside();
            EditorGUILayout.EndHorizontal();
        }

        void Vertical(System.Action inside)
        {
            EditorGUILayout.BeginVertical();
            inside();
            EditorGUILayout.EndVertical();
        }

        void DisplayLabel(string label)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        }

        private Texture2D TextureField(string name, Texture2D texture)
        {
            GUILayout.BeginVertical();
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperCenter,
                fixedWidth = 70
            };
            GUILayout.Label(name, style);
            var result = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(70), GUILayout.Height(70));
            GUILayout.EndVertical();
            return result;
        }

        /// <summary>
        /// Set Icons for Android platform.
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="textures"></param>
        void SetIcons(PlatformIconKind kind, params Texture2D[] textures)
        {
            var platform = BuildTargetGroup.Android;
            var icons = PlayerSettings.GetPlatformIcons(platform, kind);
            for (var i = 0; i < icons.Length; i++)
            {
                icons[i].SetTextures(i == 0 ? textures : null);
            }
            PlayerSettings.SetPlatformIcons(platform, kind, icons);
        }
    }
}
