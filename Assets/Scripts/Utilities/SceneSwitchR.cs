﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// <para>Handy class for changing scene asynchronously and showing Ads.</para>
/// Author: Rezky Ashari
/// </summary>
public class SceneSwitchR {

    /// <summary>
    /// Transition duration in seconds.
    /// </summary>
    const float transitionDuration = 0.5f;

    /// <summary>
    /// Whether to show interstitial ad after load.
    /// </summary>
    static bool showAdAfterLoad = false;

    public static bool IsOnTransition { get; private set; }

    /// <summary>
    /// CanvasGroup of a transition overlay to dispose.
    /// </summary>
    static CanvasGroup transitionOverlay;

    /// <summary>
    /// Called before changing scene. Automatically will be set to null after called once.
    /// </summary>
    public static Action OnBeforeChangingScene;

    /// <summary>
    /// Called after changing scene. Automatically will be set to null after called once.
    /// </summary>
    public static Action OnAfterChangingScene;

    /// <summary>
    /// Called when scene changed.
    /// </summary>
    public static event Action OnSceneChanged;

    /// <summary>
    /// Called before destroying the transition overlay.
    /// </summary>
    public static event Action OnDestroyingOverlay;

    /// <summary>
    /// Create a dark fader from the resource prefab.
    /// Dark fader must be a game object that contains a CanvasGroup element.
    /// </summary>
    /// <param name="initialAlpha"></param>
    /// <returns></returns>
    static CanvasGroup CreateTransitionOverlay(float initialAlpha = 0)
    {
        GameObject overlayPrefab = Resources.Load<GameObject>("Prefabs/TransitionCanvas");
        if (overlayPrefab == null)
        {
            overlayPrefab = Resources.Load<GameObject>("Prefabs/DarkFader");
        }
        CanvasGroup cg = UnityEngine.Object.Instantiate(overlayPrefab).GetComponent<CanvasGroup>();
        cg.alpha = initialAlpha;
        return cg;
    }

    /// <summary>
    /// Name of the current active scene.
    /// </summary>
    public static string CurrentSceneName
    {
        get {
            if (currentSceneName == "") currentSceneName = SceneManager.GetActiveScene().name;
            return currentSceneName;
        }
    }
    static string currentSceneName = "";

    /// <summary>
    /// Switch scene to a new scene.
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="showAd"></param>
	public static void To(string sceneName, bool showAd = false)
    {
        if (!IsOnTransition)
        {
            ExecuteOnce(ref OnBeforeChangingScene);
            IsOnTransition = true;
            showAdAfterLoad = showAd;
            RezTween.To(CreateTransitionOverlay(), transitionDuration / 2, "alpha:1").OnComplete = () =>
            RezTween.StartCoroutine(LoadScene(sceneName));
        }        
    }

    /// <summary>
    /// Load scene async.
    /// </summary>
    /// <param name="sceneName">Target scene name</param>
    /// <returns></returns>
    static IEnumerator LoadScene(string sceneName)
    {
        yield return SceneManager.LoadSceneAsync(sceneName);

        currentSceneName = sceneName;
        //Debug.Log("Current scene name: " + currentSceneName);

        ExecuteOnce(ref OnAfterChangingScene);
        if (OnSceneChanged != null) OnSceneChanged();

        transitionOverlay = CreateTransitionOverlay(1);
        
        if (showAdAfterLoad)
        {
            RezTween.DelayedCall(0.1f, () =>
            {
                DestroyLoadingImage();
#if IKAAN_PLUGIN
                RezTween.DelayedCall(IkaanAPI.Ads != null? IkaanAPI.Ads.AdTimeOut : 3f, DestroyOverlay);
                IkaanAPI.Ads.ShowInterstitial(DestroyOverlay, DestroyOverlay);
#else 
                FadeOutOverlay();
#endif
            });
            showAdAfterLoad = false;
        }
        else
        {
            FadeOutOverlay();
        }
#if IKAAN_PLUGIN
        IkaanAPI.LogScreen(sceneName);
#endif
    }

    private static void DestroyLoadingImage()
    {
        if (transitionOverlay != null)
        {
            Transform secondChild = transitionOverlay.transform.GetChild(1);
            if (secondChild != null) secondChild.gameObject.SetActive(false);
        }
    }

    static void DestroyOverlay()
    {
        if (transitionOverlay != null)
        {
            IsOnTransition = false;
            ExecuteOnce(ref OnDestroyingOverlay);

            UnityEngine.Object.Destroy(transitionOverlay.gameObject);
            transitionOverlay = null;
        }
    }

    static void FadeOutOverlay()
    {
        if (transitionOverlay == null) return;
        RezTween.To(transitionOverlay, transitionDuration / 2, "alpha:0").OnComplete = () =>
        DestroyOverlay();
    }

    /// <summary>
    /// Execute a listener then sets it's value to null.
    /// </summary>
    /// <param name="action"></param>
    static void ExecuteOnce(ref Action action)
    {
        if (action != null) action();
        action = null;
    }

    /// <summary>
    /// Execute a method after scene transition.
    /// </summary>
    /// <param name="action"></param>
    public static void ExecuteAfterTransition(Action action)
    {
        if (IsOnTransition)
        {
            OnDestroyingOverlay += action;
        }
        else
        {
            action();
        }
    }
}
