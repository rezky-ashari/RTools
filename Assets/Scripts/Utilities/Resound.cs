using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// <para>Simple sound manager.</para>
/// Author: Rezky Ashari
/// </summary>
public class Resound {

    /// <summary>
    /// Resource folder name that contains all audio files.
    /// </summary>
    public const string soundResourcePath = "Sounds";

    /// <summary>
    /// Called whenever mute state changed.
    /// </summary>
    public static event Action<bool> OnMuteStateChanged;

    /// <summary>
    /// Limit for sfx's audio sources.
    /// </summary>
    const int sfxLimit = 20;

    /// <summary>
    /// Default volume for all sounds.
    /// </summary>
    static float masterVolume = 1f;

    static float defaultBGMVolume = 1f;
    static float defaultSFXVolume = 1f;

    static ResoundHost Host
    {
        get {
            return ResoundHost.GetInstance();
        }
    }   

    /// <summary>
    /// Whether audio is in mute state.
    /// </summary>
    public static bool IsMute { get { return Host.mute; } }

    /// <summary>
    /// Play a sound effect.
    /// </summary>
    /// <param name="filename">Sound filename as defined in <code>Playlist</code>.</param>
    /// <param name="loop"></param>
    public static void PlaySFX(string filename, bool? loop = false, float? volume = null)
    {
        Host.PlaySFX(filename, volume.HasValue? (float)volume : defaultSFXVolume, (bool)loop);
    }

    /// <summary>
    /// Stop the playing sound effects.
    /// </summary>
    /// <param name="filename">SFX filename as defined in <code>Playlist</code>.</param>
    public static void StopSFX(string filename = "all")
    {
        Host.StopSFX(filename);
    }

    /// <summary>
    /// Set the volume for all SFXs.
    /// </summary>
    /// <param name="volume"></param>
    public static void SetSFXVolume(float volume)
    {
        defaultSFXVolume = Mathf.Clamp01(volume);
        Host.SetSFXVolume(volume);
    }

    /// <summary>
    /// Play a BGM (or replace the one that currently playing).
    /// </summary>
    /// <param name="filename">BGM filename as defined in <code>Playlist</code>.</param>
    public static void PlayMusic(string filename)
    {
        Host.PlayMusic(filename);
    }

    /// <summary>
    /// Stop the current BGM.
    /// </summary>
    public static void StopMusic()
    {
        Host.StopMusic();
    }

    /// <summary>
    /// Set volume for all BGMs.
    /// </summary>
    /// <param name="volume"></param>
    public static void SetMusicVolume(float volume)
    {
        defaultBGMVolume = Mathf.Clamp01(volume);
        Host.SetMusicVolume(volume);
    }

    /// <summary>
    /// Set mute state.
    /// </summary>
    /// <param name="mute"></param>
    public static void SetMute(bool mute)
    {
        AudioListener.pause = mute;
        AudioListener.volume = (mute) ? 0 : masterVolume;
        Host.mute = mute;

        if (OnMuteStateChanged != null) OnMuteStateChanged(mute);
    }

    /// <summary>
    /// Toggle mute state (mute to unmute, and vice versa).
    /// </summary>
    public static void ToggleMute()
    {
        SetMute(!IsMute);
    }

    /// <summary>
    /// Set volume for all sounds.
    /// </summary>
    /// <param name="volume"></param>
    public static void SetVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        AudioListener.volume = masterVolume;
    }

    #region Editor Menu
    private const string menuName = "Rezky Tools/Mute Resound";
    private static bool isToggled;

#if UNITY_EDITOR && !RESOUND_CONFIG
    [MenuItem(menuName)]
    private static void MuteInEditor()
    {
        isToggled = !isToggled;
        Menu.SetChecked(menuName, isToggled);
        EditorPrefs.SetBool(menuName, isToggled);
        if (Application.isPlaying) SetMute(isToggled);
    }

    [MenuItem(menuName, true)]
    private static bool MuteInEditorValidate()
    {
        isToggled = EditorPrefs.GetBool(menuName, false);
        Menu.SetChecked(menuName, isToggled);
        return true;
    }
#endif

    private static bool IsMuteInEditor()
    {
#if UNITY_EDITOR
        return EditorPrefs.GetBool(menuName, false);
#else
        return false;
#endif
    }
    #endregion

    #region Host
    class ResoundHost : MonoBehaviour
    {
        static ResoundHost _instance;

        public bool mute = false;
        bool lastMuteState = false;

        public string playingBGM;

        int updateRate = 120;
        int passedFrame = 0;

        AudioSource musicSource;
        List<AudioSource> sfxSources = new List<AudioSource>(); 

        public static ResoundHost GetInstance()
        {
            if (_instance == null)
            {
                GameObject container = new GameObject("ResoundHost");
                DontDestroyOnLoad(container);
                _instance = container.AddComponent<ResoundHost>();
#if RESOUND_CONFIG
                ResoundSettings settings = ResoundSettings.Instance;
                masterVolume = settings.masterVolume;
                defaultBGMVolume = settings.BGMVolume;
                defaultSFXVolume = settings.SFXVolume;
                SetMute(settings.muteByDefault);
#else
                SetMute(IsMuteInEditor());
#endif

            }
            return _instance;
        }

        private void Start()
        {
            //AudioListener.volume = defaultVolume;
        }

        private void Update()
        {
            if (passedFrame < updateRate)
            {
                passedFrame++;
            }
            else
            {
                passedFrame = 0;
                if (sfxSources.Count > 0)
                {
                    AudioSource sfx;
                    for (int i = sfxSources.Count - 1; i >= 0; i--)
                    {
                        sfx = sfxSources[i];
                        if (!sfx.isPlaying)
                        {                            
                            sfxSources.Remove(sfx);
                            Destroy(sfx);
                        }                        
                    }
                }
            }

            if (lastMuteState != mute)
            {
                SetMute(mute);
                lastMuteState = mute;
            }
        }

        AudioSource AddAudioSource()
        {
            AudioSource audioSrc = gameObject.AddComponent<AudioSource>();
            audioSrc.playOnAwake = false;
            return audioSrc;
        }

        public void PlaySFX(string filename, float volume, bool loop = false)
        {
            if (sfxSources.Count < sfxLimit)
            {
                AudioSource sfxSource = AddAudioSource();
                sfxSource.clip = Resources.Load<AudioClip>(soundResourcePath + "/" + filename);
                if (sfxSource.clip != null)
                {
                    sfxSource.loop = loop;
                    sfxSource.volume = volume;
                    sfxSource.Play();
                    sfxSources.Add(sfxSource);
                }
                else
                {
                    Debug.LogWarning("Can't play SFX because audio file '" + filename + "' is not in 'Sounds' Folder");
                }
            }
        }

        public void StopSFX(string filename = "all")
        {
            bool stopAll = filename == "all";
            for (int i = 0; i < sfxSources.Count; i++)
            {
                if (stopAll || sfxSources[i].clip.name == filename)
                {
                    sfxSources[i].Stop();
                }
            }
        }

        public void PlayMusic(string filename)
        {
            if (musicSource == null)
            {
                musicSource = AddAudioSource();
                musicSource.playOnAwake = true;
            }

            if (playingBGM == filename && musicSource.isPlaying) return;

            musicSource.clip = Resources.Load<AudioClip>(soundResourcePath + "/" + filename);
            musicSource.loop = true;
            musicSource.volume = defaultBGMVolume;
            musicSource.Play();
            playingBGM = filename;
        }

        public void StopMusic(bool fadeOut = false)
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }

        public void SetMusicVolume(float volume)
        {
            musicSource.volume = volume;
        }

        internal void SetSFXVolume(float volume)
        {
            for (int i = 0; i < sfxSources.Count; i++)
            {
                sfxSources[i].volume = volume;
            }
        }
    }
#endregion
}