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
public class Resound : MonoBehaviour {

    /// <summary>
    /// Resource folder name that contains all audio files.
    /// </summary>
    public const string soundResourcePath = "Sounds";

    const int sfxLimit = 20;

    static ResoundHost Host
    {
        get {
            return ResoundHost.GetInstance();
        }
    }

    static float defaultVolume = 1f;
    static float defaultMusicVolume = 0.5f;
    static float defaultSfxVolume = 0.6f;

    /// <summary>
    /// Whether audio is in mute state.
    /// </summary>
    public static bool IsMute { get { return Host.mute; } }

    /// <summary>
    /// Play a sound effect.
    /// </summary>
    /// <param name="filename">Sound filename as defined in <code>Playlist</code>.</param>
    /// <param name="loop"></param>
    public static float PlaySFX(string filename, bool loop = false)
    {
        return Host.PlaySFX(filename, loop).clip.length;
    }

    public static float FadeInSFX(string filename, bool loop = false)
    {
        return Host.FadeInSFX(filename, loop).clip.length;
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
    /// Play a BGM (or replace the one that currently playing).
    /// </summary>
    /// <param name="filename">BGM filename as defined in <code>Playlist</code>.</param>
    public static void PlayMusic(string filename)
    {
        Host.PlayMusic(filename);
    }

    /// <summary>
    /// Stops the currently playing BGM.
    /// </summary>
    /// <param name="delay">Delay before stop (in seconds)</param>
    public static void StopMusic(float delay = 0)
    {
        if (delay > 0)
        {
            RezTween.DelayedCall(delay, () => Host.StopMusic());
        }
        else
        {
            Host.StopMusic();
        }        
    }

    public static void FadeOutMusic()
    {
        Host.FadeOutMusic();
    }

    /// <summary>
    /// Set mute state.
    /// </summary>
    /// <param name="mute"></param>
    public static void SetMute(bool mute)
    {
        AudioListener.pause = mute;
        AudioListener.volume = (mute) ? 0 : defaultVolume;
        Host.mute = mute;
    }

    /// <summary>
    /// Toggle mute state (mute to unmute, and vice versa).
    /// </summary>
    public static void ToggleMute()
    {
        SetMute(!IsMute);
    }

    #region Editor Menu
#if UNITY_EDITOR
    private const string menuName = "Rezky Tools/Mute Resound in Editor";
    private static bool isToggled;  

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

    [MenuItem("Rezky Tools/Open Sound Folder")]
    private static void OpenSoundFolder()
    {
        EditorUtility.RevealInFinder("Assets/Resources/Sounds/readme.txt");
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
        public string playingBGM;

        bool lastMuteState = false;

        int updateRate = 120;
        int passedFrame = 0;

        AudioSource musicSource;
        List<AudioSource> sfxSources = new List<AudioSource>();
        List<SoundFader> soundFaderList = new List<SoundFader>();

        public static ResoundHost GetInstance()
        {
            if (_instance == null)
            {
                GameObject container = new GameObject("ResoundHost");
                DontDestroyOnLoad(container);
                _instance = container.AddComponent<ResoundHost>();
                SetMute(IsMuteInEditor());
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

        private void FixedUpdate()
        {
            SoundFader soundFader; 
            for (int i = soundFaderList.Count - 1; i >= 0; i--)
            {
                soundFader = soundFaderList[i];
                soundFader.Update(Time.fixedDeltaTime);
                if (soundFader.IsComplete)
                {
                    soundFaderList.RemoveAt(i);
                }
            }
        }

        AudioSource AddAudioSource()
        {
            AudioSource audioSrc = gameObject.AddComponent<AudioSource>();
            audioSrc.playOnAwake = false;
            return audioSrc;
        }

        public AudioSource PlaySFX(string filename, bool loop)
        {
            if (sfxSources.Count < sfxLimit)
            {
                string filePath = soundResourcePath + "/" + filename;
                AudioClip clip = Resources.Load<AudioClip>(filePath);
                if (clip != null)
                {
                    AudioSource sfxSource = AddAudioSource();
                    sfxSource.clip = clip;
                    sfxSource.loop = loop;
                    sfxSource.volume = defaultSfxVolume;
                    sfxSource.Play();
                    sfxSources.Add(sfxSource);
                    return sfxSource;
                }
                else
                {
                    Debug.Log("The audio file you're trying to play is not found! " + filePath);
                }
            }
            return null;
        }

        public AudioSource FadeInSFX(string filename, bool loop)
        {
            AudioSource sfxSource = PlaySFX(filename, loop);
            AddSoundFader(new SoundFader(sfxSource, true, defaultSfxVolume));
            return sfxSource;
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

        public AudioSource PlayMusic(string filename)
        {
            if (musicSource == null)
            {
                musicSource = AddAudioSource();
                musicSource.playOnAwake = true;
            }
            else if (musicSource.volume == 0 && filename == playingBGM)
            {
                musicSource.Stop();
                playingBGM = "";
            }

            if (playingBGM != filename || !musicSource.isPlaying)
            {
                musicSource.clip = Resources.Load<AudioClip>(soundResourcePath + "/" + filename);
                musicSource.loop = true;
                musicSource.volume = defaultMusicVolume;
                musicSource.Play();

                playingBGM = filename;
            }

            return musicSource;
        }

        public void StopMusic(bool fadeOut = false)
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }

        void AddSoundFader(SoundFader soundFader)
        {
            soundFaderList.Add(soundFader);
        }
        
        public void FadeOutMusic()
        {
            AddSoundFader(new SoundFader(musicSource, false, defaultMusicVolume));
        }
    }
    #endregion

    class SoundFader
    {
        private float maxVolume = 1f;
        private AudioSource source;
        private bool isFadeIn = true;
        private float updateSpeed = 1f;
        
        public bool IsComplete { get; private set; }

        public SoundFader(AudioSource source, bool isFadeIn, float maxVolume = 1f)
        {
            this.source = source;
            this.isFadeIn = isFadeIn;
            this.maxVolume = maxVolume;            
            source.volume = isFadeIn ? 0 : maxVolume;
        }

        public void Update(float deltaTime)
        {
            if (source == null)
            {
                IsComplete = true;
                return;
            }

            if (isFadeIn)
            {
                if (source.volume < maxVolume)
                {
                    source.volume += updateSpeed * deltaTime;
                }
                else
                {
                    IsComplete = true;
                }
            }
            else
            {
                if (source.volume > 0)
                {
                    source.volume -= updateSpeed * deltaTime;
                }
                else
                {
                    IsComplete = true;
                }
            }
        }
    }
}

