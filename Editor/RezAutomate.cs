using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace RTools
{
    /// <summary>
    /// Automate the process of creating and updating 'Playlist' and 'Scenes' data.
    /// Deprecated. Should create UI for this.
    /// Menu created:
    /// 1. RTools > Resolve > Playlist,
    /// 2. RTools > Resolve > GameScenes.
    /// </summary>
    public class RezAutomate
    {
        static List<string> savedScenes;

        static bool needUpdatePlaylist = false;

        static string PlaylistPath
        {
            get { return Application.dataPath + "/Scripts/Utilities/Playlist.cs"; }
        }

        [MenuItem("RTools/Resolve/Playlist")]
        static void ResolvePlaylist()
        {
            CreatePlaylist();
            EditorUtility.DisplayDialog("Resolve Playlist", "Succesfully recreate the Playlist", "OK");
        }

        [MenuItem("RTools/Resolve/GameScenes")]
        static void ResolveGameScenes()
        {
            CreateSceneList();
            EditorUtility.DisplayDialog("Resolve GameScenes", "Succesfully recreate the GameScenes", "OK");
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string str in importedAssets)
            {
                //Debug.Log("Reimported Asset: " + str);
                CheckPlaylistChanges(str);
            }
            foreach (string str in deletedAssets)
            {
                //Debug.Log("Deleted Asset: " + str);
                CheckPlaylistChanges(str);
            }

            for (int i = 0; i < movedAssets.Length; i++)
            {
                //Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
            }

            if (needUpdatePlaylist)
            {
                CreatePlaylist();
                AssetDatabase.Refresh();
            }
        }

        static void CheckPlaylistChanges(string str)
        {
            if (str.Contains(Resound.soundResourcePath)) needUpdatePlaylist = true;
        }

        private static void CreatePlaylist()
        {
            AudioClip[] soundFiles = Resources.LoadAll<AudioClip>(Resound.soundResourcePath);

            string contents = "///<summary>\n///<para>List of sounds that can be played using Resound.</para>\n///Author: Rezky Ashari\n///</summary>\npublic struct Playlist\n{";
            for (int i = 0; i < soundFiles.Length; i++)
            {
                string filename = Path.GetFileNameWithoutExtension(soundFiles[i].name);
                contents += string.Format("\n\t public const string {0} = \"{1}\";", filename.Replace(" ", "_"), filename);
            }
            using (StreamWriter sw = new StreamWriter(PlaylistPath))
            {
                sw.Write(contents + "\n}");
            }
        }

        static void CreateSceneList()
        {
            string[] sceneList = Scene.GetList();
            if (savedScenes != null && sceneList.Length == savedScenes.Count)
            {
                bool changed = false;
                for (int i = 0; i < sceneList.Length; i++)
                {
                    if (savedScenes.IndexOf(sceneList[i]) == -1)
                    {
                        changed = true;
                        break;
                    }
                }
                if (!changed)
                {
                    Trace("There's no change in build settings. No update for scenelist.");
                    return;
                }
            }

            if (savedScenes == null) Trace("Creating scene list...");
            else Trace("Updating the scene list...");

            savedScenes = new List<string>();
            string contents = "///<summary>\n///<para>List of scene names in Build Settings.</para>\n///Author: Rezky Ashari\n///</summary>\npublic struct GameScenes\n{";
            for (int i = 0; i < sceneList.Length; i++)
            {
                savedScenes.Add(sceneList[i]);
                string filename = Path.GetFileNameWithoutExtension(sceneList[i]);
                if (filename.Length == 0) continue;
                contents += string.Format("\n\t public const string {0} = \"{1}\";", filename.Replace(" ", "").Replace("-", "_"), filename);
            }
            using (StreamWriter sw = new StreamWriter(Application.dataPath + "/Scripts/Scene/GameScenes.cs"))
            {
                sw.Write(contents + "\n}");
            }

            Trace("Done");
        }

        static void Trace(string text)
        {
            // DEBUG only
            //Debug.Log(text);
        }
    }
}
